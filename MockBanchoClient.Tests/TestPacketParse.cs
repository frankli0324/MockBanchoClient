using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MockBanchoClient.Packets;
using MockBanchoClient.Serialization;
using Xunit;

namespace MockBanchoClientTest {
    public class PacketParse {
        private static Dictionary<Type, string> packets = new Dictionary<Type, string> () {
            {
            // typeof (LockClient),
            // "\x5C\x00\x00\x04\x00\x00\x00\x00\x00\x00\x00"
            // }, {
            typeof (UserPresenceBundle),
            "\x60\x00\x00\x06\x00\x00\x00\x01\x00\x12\x34\x56\x00"
            },
        };

        [Fact]
        public void TestParsePacket () {
            foreach (var i in packets) {
                using (var stream = new MemoryStream (
                    Encoding.UTF8.GetBytes (i.Value)
                ))
                using (var serializer = new BanchoPacketReader (stream)) {
                    IPacket test = serializer.ReadPacket ();
                    Assert.True (test.GetType () == i.Key);
                }
            }
        }
    }
}