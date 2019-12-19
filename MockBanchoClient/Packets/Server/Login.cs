using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Packet (92)]
    public class LockClient : IPacket {
        public int length; // in seconds
        public void ReadFrom (BanchoPacketReader reader) {
            length = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (5)]
    public class LoginReply : IPacket {
        public int reply;
        public bool logged_in { get => reply > 0; }
        public void ReadFrom (BanchoPacketReader reader) {
            reply = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (71)]
    public class UserPermission : IPacket {
        public int permission;
        public void ReadFrom (BanchoPacketReader reader) {
            permission = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (76)]
    public class MainMenuNews : IPacket {
        public string news;
        public void ReadFrom (BanchoPacketReader reader) {
            news = reader.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }
}