using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Packet (95)]
    public class UserPresenceSingle : IPacket {
        public int onlineUser;
        public UserPresenceSingle (BanchoPacketReader reader) => ReadFromReader (reader);
        public void ReadFromReader (BanchoPacketReader reader) {
            this.onlineUser = reader.ReadInt32 ();
        }
        public byte[] GetBytes () {
            throw new NotImplementedException ();
        }
        public void WriteToStream (Stream output) {
            throw new NotImplementedException ();
        }
    }

    [Packet (96)]
    public class UserPresenceBundle : IPacket {
        public List<int> onlineUsers;
        public UserPresenceBundle (BanchoPacketReader reader) =>
            ReadFromReader (reader);
        public void ReadFromReader (BanchoPacketReader reader) {
            this.onlineUsers = reader.ReadInt32List ();
        }
        public byte[] GetBytes () {
            throw new NotImplementedException ();
        }
        public void WriteToStream (Stream output) {
            throw new NotImplementedException ();
        }
    }
}