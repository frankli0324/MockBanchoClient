using System.Collections.Generic;
using System.IO;
using MockBanchoClient.Packets;
namespace MockBanchoClient.Serialization {
    /// <summary>
    /// Extended the BinaryReader, added some
    /// types that are used in Bancho network
    /// </summary>
    public class BanchoPacketReader : BinaryReader {
        public BanchoPacketReader (Stream input) : base (input) { }
        public List<int> ReadInt32List () {
            short length = ReadInt16 ();
            List<int> ret = new List<int> ();
            for (int i = 0; i < length; i++)
                ret.Add (ReadInt32 ());
            return ret;
        }
        private byte[] ReadPacketRawBody () {
            ReadByte ();
            int length = ReadInt32 ();
            var body = ReadBytes (length);
            return body;
        }
        public IPacket ReadPacket () {
            return ReadPackets (1) [0];
        }
        public IPacket[] ReadPackets (int cnt) {
            List<IPacket> ret = new List<IPacket> ();
            for (int i = 0; i < cnt; i++) {
                short type = ReadInt16 ();
                try {
                    ReadByte (); // skipped byte
                    int body_length = ReadInt32 ();
                    ret.Add (PacketFactory.CreatePacket (
                        packet_type: type, this
                    ));
                    // how to check if the Child class
                    // is reading EXACTLY body_length bytes?
                } catch (KeyNotFoundException) {
                    // log.warning: 
                }
            }
            return ret.ToArray ();
        }
    }
}