using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MockBanchoClient.Helpers;
using MockBanchoClient.Serialization;
using Newtonsoft.Json.Linq;

namespace MockBanchoClient {
    public class Client {
        private string token = "", username = "", password = "";
        Queue<Packets.IPacket> send_queue = new Queue<Packets.IPacket> ();
        object send_queue_lock = new object ();
        private pWebRequest ActiveRequest = null;
        private string version = "b20191223.6";
        private string timezone = "8";
        private string executable_md5 = "2696a60f471503473dc42f041cebbde1";
        private string client_hash {
            get {
                return string.Concat (new string[] {
                    "",
                    executable_md5 + ":",
                    "runningunderwine:",
                    "runningunderwine".Md5 () + ":",
                    "anotherstaticrandomstring".Md5 () + ":",
                    "somestaticrandomstring".Md5 () + ":",
                });
            }
        }
        public string verification_url {
            get {
                return "" +
                    "https://osu.ppy.sh/p/verify?u=" +
                    username.Replace (" ", "%20") +
                    "&reason=bancho&ch=" + client_hash;
            }
        }
        public async Task<bool> Login (string username, string password) {
            this.username = username;
            this.password = password;
            HttpClient client = new HttpClient (new HttpClientHandler () {
                AutomaticDecompression =
                    DecompressionMethods.GZip |
                    DecompressionMethods.Deflate
            });
            client.DefaultRequestHeaders.Clear ();
            client.DefaultRequestHeaders.Add ("User-Agent", "osu!");
            client.DefaultRequestHeaders.Add ("Connection", "Keep-Alive");
            foreach (var i in JArray.Parse (await (await client.GetAsync (
                    "https://osu.ppy.sh/web/check-updates.php?" +
                    "action=check&" + "stream=stable40"
                )).Content.ReadAsStringAsync ()))
                if (i["filename"].ToObject<string> () == "osu!.exe")
                    executable_md5 = i["file_hash"].ToObject<string> ();

            string country = await (await client.GetAsync (
                "https://osu.ppy.sh/web/bancho_connect.php?" +
                string.Join ('&', new List < (string, string) > {
                    ("v", version),
                    ("u", username),
                    ("h", password.Md5 ()),
                    ("fx", "dotnet4|dotnet4"),
                    ("ch", client_hash)
                }.ConvertAll ((x) =>
                    x.Item1 + "=" + Uri.EscapeDataString (x.Item2)
                ))
            )).Content.ReadAsStringAsync ();

            HttpRequestMessage request = new HttpRequestMessage (
                HttpMethod.Get, "http://c4.ppy.sh/"
            );
            request.Headers.Clear ();
            request.Headers.Add ("osu-version", version);
            var response = await client.SendAsync (request);
            if (response.IsSuccessStatusCode == false)
                return false;
            this.token = response.Headers.GetValues ("cho-token").First ();
            send_queue.Enqueue (new Packets.LoginPacket (
                username, password, version, timezone, client_hash
            ));
            return true;
        }
        private pWebRequest GetRequest () {
            MemoryStream stream = new MemoryStream ();
            BanchoPacketWriter writer = new BanchoPacketWriter (stream);
            try {
                lock (send_queue_lock) {
                    while (send_queue.Any ()) {
                        writer.Write (send_queue.Dequeue ());
                    }
                }
                stream.Seek (0, SeekOrigin.Begin);
                pWebRequest request = new pWebRequest (
                    "https://c4.ppy.sh", new object[0]
                );
                request.AddHeader ("osu-token", token);
                request.AddHeader ("osu-version", version);
                request.AddRaw (stream);
                return request;
            } finally {
                writer.Dispose ();
                stream.Dispose ();
            }
        }
        public IEnumerable<Packets.IPacket> Poll () {
            while (true) {
                ActiveRequest = GetRequest ();
                ActiveRequest.BlockingPerform ();
                var res_stream = ActiveRequest.ResponseStream;
                using (var serializer = new BanchoPacketReader (res_stream)) {
                    while (true) {
                        Packets.IPacket packet = null;
                        long start_position = res_stream.Position;
                        try {
                            while (!(ActiveRequest.Completed || ActiveRequest.Aborted) &&
                                res_stream.Position == res_stream.Length) {
                                Thread.Sleep (500);
                            }
                            packet = serializer.ReadPacket ();
                        } catch (Exception) {
                            if (ActiveRequest.Completed || ActiveRequest.Aborted) {
                                Console.WriteLine ("connection completed or aborted");
                                ActiveRequest.Dispose ();
                                Thread.Sleep (10000);
                                ActiveRequest = GetRequest ();
                                ActiveRequest.BlockingPerform ();
                                res_stream = ActiveRequest.ResponseStream;
                            } else {
                                res_stream.Seek (start_position, SeekOrigin.Begin);
                            }
                        }
                        if (packet != null)
                            yield return packet;
                    }
                }
            }
        }
    }
}