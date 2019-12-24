using System;
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
        public object ReadBanchoObject () {
            switch ((BanchoType) ReadByte ()) {
            case BanchoType.Null:
                return null;
            case BanchoType.Bool:
                return ReadBoolean ();
            case BanchoType.Byte:
                return ReadByte ();
            case BanchoType.UShort:
                return ReadUInt16 ();
            case BanchoType.UInt:
                return ReadUInt32 ();
            case BanchoType.ULong:
                return ReadUInt64 ();
            case BanchoType.SByte:
                return ReadSByte ();
            case BanchoType.Short:
                return ReadInt16 ();
            case BanchoType.Int:
                return ReadInt32 ();
            case BanchoType.Long:
                return ReadInt64 ();
            case BanchoType.Char:
                return ReadChar ();
            case BanchoType.String:
                return base.ReadString ();
            case BanchoType.Float:
                return ReadSingle ();
            case BanchoType.Double:
                return ReadDouble ();
            case BanchoType.Decimal:
                return ReadDecimal ();
            case BanchoType.DateTime:
                return new DateTime (ReadInt64 (), DateTimeKind.Utc);
            case BanchoType.ByteArray:
                return ReadBytes (ReadInt32 ());
            case BanchoType.CharArray:
                return ReadChars (ReadInt32 ());
            case BanchoType.Unknown:
            case BanchoType.Serializable:
            default:
                throw new NotImplementedException ();
            }
        }
        public List<T> ReadList<T> () {
            var l = new List<T> ();
            int length = ReadInt32 ();
            for (int i = 0; i < length; i++)
                l.Add ((T) ReadBanchoObject ());
            return l;
        }
        public override string ReadString () {
            if (ReadByte () != 0)
                return base.ReadString ();
            else return null;
        }
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
            short type = ReadInt16 ();
            ReadByte (); // skipped byte
            int body_length = ReadInt32 ();
            var start = this.BaseStream.Position;
            var p = PacketFactory.CreatePacket (
                packet_type: type, this
            );
            for (; BaseStream.Position - start < body_length; ReadByte ());
            return p;
        }
    }
    public class BanchoPacketWriter : BinaryWriter {
        public BanchoPacketWriter (Stream output) : base (output) { }
        public void Write (IPacket packet) {
            packet.WriteTo (this);
        }
    }

}