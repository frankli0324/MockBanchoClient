using System;
using System.IO;
using System.Linq;
using MockBanchoClient.Packets;
using MockBanchoClient.Serialization;
using Xunit;

namespace MockBanchoClientTest
{
    public class PacketParse {
        [Fact]
        public async void TestParsePacketFromFile () {
            byte[] content = await File.ReadAllBytesAsync ("../../../test.raw");
            using (var stream = new MemoryStream (content))
            using (var serializer = new BanchoPacketReader (stream)) {
                while (true) {
                    try {
                        var packet = serializer.ReadPacket ();
                        LogPacket (packet);
                    } catch (NotImplementedException) {
                        Console.WriteLine ("unknown packet");
                    } catch (EndOfStreamException) {
                        break;
                    }
                }
            }
        }
        void LogPacket (IPacket packet) {
            Console.Write ($"[{packet.GetType ().ToString ().Split ('.').Last ()}] ");
            switch (packet.GetType ().ToString ().Split ('.').Last ()) {
            case "UserPresenceBundle":
                var p1 = packet as UserPresenceBundle;
                Console.WriteLine (p1.onlineUsers.Count + " users found");
                break;
            case "ChatChannelJoinSuccess":
                Console.WriteLine (
                    (packet as ChatChannelJoinSuccess).channel +
                    " channel joined");
                break;
            case "ChatChannelDetail":
                Console.WriteLine (
                    (packet as ChatChannelDetail).name +
                    " channel available"
                );
                break;
            case "ChatChannelListingComplete":
                Console.WriteLine ("finished listing channels");
                break;
            case "UserPresenceSingle":
                Console.WriteLine(
                    (packet as UserPresenceSingle).user_id + 
                    " joined the game"
                );
                break;
            case "UserQuit":
                Console.WriteLine (
                    (packet as UserQuit).user_id +
                    " quitted the game"
                );
                break;
            }
        }
    }
}