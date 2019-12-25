using System.Collections.Generic;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Recv (72)]
    public class FriendList : IPacket {
        public List<int> friends = new List<int> ();
        public void ReadFrom (BanchoPacketReader reader) {
            friends = reader.ReadInt32List ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }
}