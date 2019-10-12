using System;
namespace MockBanchoClient.Packets {
    [Packet (75)]
    public class BanchoVersion : IPacket {
        public int version { get; private set; }
        public BanchoVersion (Serialization.BanchoPacketReader reader) => ReadFromReader (reader);
        public byte[] GetBytes () {
            throw new NotImplementedException ();
        }
        public void ReadFromReader (Serialization.BanchoPacketReader reader) {
            this.version = reader.ReadInt32 ();
        }
    }
}