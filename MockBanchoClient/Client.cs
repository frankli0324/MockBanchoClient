using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MockBanchoClient.Extension;
using Newtonsoft.Json.Linq;

namespace MockBanchoClient {
    public class Client {
        private string version = "b20191006.1";
        private string timezone = "8";
        private string executable_hash = "6e9fd4b6476e70d0760d88d4cca616ec";
        private string login_remarks = ""; // Marks osu clients running on wine sometimes
        private string metadata_info {
            get {
                return "" +
                    executable_hash + ':' +
                    login_remarks + ':' +
                    "".Md5 ().Md5 () + ':' +
                    "unknown".Md5 ().Md5 () + ':';
            }
        }
        private HttpClient client = new HttpClient (new HttpClientHandler () {
            AutomaticDecompression =
                DecompressionMethods.GZip |
                DecompressionMethods.Deflate
        });
        private async void UpdateMetadata () {
            var response = await client.GetAsync (
                "https://osu.ppy.sh/web/check-updates.php?" +
                "action=check&" +
                "stream=stable40"
            );
            string body = await response.Content.ReadAsStringAsync ();
            foreach (var i in JArray.Parse (body)) {
                if (i["filename"].ToObject<string> () == "osu!.exe")
                    executable_hash = i["file_hash"].ToObject<string> ();
            }
        }
        public Client (bool fetch_latest_metadata = false) {
            client.DefaultRequestHeaders.Clear ();
            client.DefaultRequestHeaders.Add ("User-Agent", "osu!");
            client.DefaultRequestHeaders.Add ("Connection", "Keep-Alive");
            if (fetch_latest_metadata)
                UpdateMetadata ();
        }
        public async Task<bool> Login (string username, string password) {
            string country = await (await client.GetAsync (
                "https://osu.ppy.sh/web/bancho_connect.php?" +
                // "http://localhost?" +
                $"v={version}&u={Uri.EscapeDataString(username)}&h={password.Md5()}" +
                $"&fx={Uri.EscapeDataString("dotnet|dotnet")}" +
                $"&ch={Uri.EscapeDataString(metadata_info)}"
            )).Content.ReadAsStringAsync ();

            string login_info = "" +
                username + '\n' +
                password.Md5 () + '\n' +
                $"{version}|{timezone}|0|{metadata_info}|0|" + '\n';
            HttpRequestMessage request = new HttpRequestMessage (
                HttpMethod.Get,
                "http://c4.ppy.sh/"
            );
            request.Headers.Clear ();
            request.Headers.Add ("osu-version", version);
            var response = await client.SendAsync (request);

            if (response.IsSuccessStatusCode == false)
                return false;
            client.DefaultRequestHeaders.Add ("osu-token", response.Headers.GetValues ("cho-token"));
            return true;
        }
    }
}