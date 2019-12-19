using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Packet (64)]
    public class ChatChannelJoinSuccess : IPacket {
        public string channel;
        public void ReadFrom (BanchoPacketReader reader) {
            channel = reader.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (65)]
    // channel available
    public class ChatChannelDetail : IPacket {
        public string name, topic;
        public short online_users;
        public void ReadFrom (BanchoPacketReader reader) {
            (name, topic) = (reader.ReadString (), reader.ReadString ());
            online_users = reader.ReadInt16 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (89)]
    public class ChatChannelListingComplete : IPacket {
        public void ReadFrom (BanchoPacketReader reader) { }
        public void WriteTo (BanchoPacketWriter writer) { }
    }
}