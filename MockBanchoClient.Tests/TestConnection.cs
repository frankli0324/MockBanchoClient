using System.IO;
using MockBanchoClient;
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
        }
    }
}