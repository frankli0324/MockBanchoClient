using System;
using System.IO;
using MockBanchoClient;
using MockBanchoClient.Packets;
using Xunit;

namespace MockBanchoClientTest {
    public class Connection {
        [Fact]
        public async void TestConnection () {
            var client = new Client ();
            var creds = Newtonsoft.Json.Linq.JArray.Parse (
                await File.ReadAllTextAsync ("../../../secret/bancho_cred.json")
            ).ToObject<string[]> ();
            var (username, password) = (creds[0], creds[1]);
            Assert.True (await client.Login (username, password));
            foreach (var i in client.Poll ()) {
                // Console.Write ($"[{i.GetType ().ToString ().Split ('.').Last ()}] ");
                // Console.WriteLine (Newtonsoft.Json.JsonConvert.SerializeObject (
                //     i, Newtonsoft.Json.Formatting.Indented
                // ));
                if (i is LoginReply && (i as LoginReply).reply == -8) {
                    Console.WriteLine (client.verification_url);
                }
            }
        }
    }
}