using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Recv (64)]
    public class ChatChannelJoinSuccess : IPacket {
        public string channel;
        public void ReadFrom (BanchoPacketReader reader) {
            channel = reader.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Recv (65)]
    /// <summary>Channel available</summary>
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

    [Recv (67)]
    /// <summary>Client should join the channel</summary>
    public class ChatChannelAutoJoin : IPacket {
        public string channel;
        public void ReadFrom (BanchoPacketReader reader) {
            channel = reader.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Recv (89)]
    public class ChatChannelListingComplete : IPacket {
        public void ReadFrom (BanchoPacketReader reader) { }
        public void WriteTo (BanchoPacketWriter writer) { }
    }
}