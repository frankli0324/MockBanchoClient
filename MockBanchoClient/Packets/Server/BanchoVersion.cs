using System;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Packet (75)]
    public class BanchoVersion : IPacket {
        public int version { get; private set; }
        public void ReadFrom (Serialization.BanchoPacketReader reader) {
            this.version = reader.ReadInt32 ();
        }
        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }
}