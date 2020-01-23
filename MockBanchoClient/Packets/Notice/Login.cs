using System;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Recv (92)]
    public class LockClient : IPacket {
        public int length; // in seconds
        public void ReadFrom (BanchoPacketReader reader) {
            length = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Recv (5)]
    public class LoginReply : IPacket {
        public int reply;
        public bool logged_in { get => reply > 0; }
        public void ReadFrom (BanchoPacketReader reader) {
            reply = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Recv (75)]
    public class BanchoVersion : IPacket {
        public int version { get; private set; }
        public void ReadFrom (Serialization.BanchoPacketReader reader) {
            this.version = reader.ReadInt32 ();
        }
        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Recv (71)]
    public class UserPermission : IPacket {
        public int permission;
        public void ReadFrom (BanchoPacketReader reader) {
            permission = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Recv (76)]
    public class MainMenuNews : IPacket {
        public string imageUrl, linkUrl;
        public void ReadFrom (BanchoPacketReader reader) {
            var array = reader.ReadString ().Split ('|');
            (imageUrl, linkUrl) = (array[0], (array.Length > 1) ? array[1] : null);
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }
}