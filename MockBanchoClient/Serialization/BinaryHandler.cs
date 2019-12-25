using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            ushort type = ReadUInt16 ();
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
            ushort type = (packet.GetType ().GetCustomAttribute (
                typeof (SendAttribute)
            ) as SendAttribute).packet_code;
            BaseStream.Seek (7, SeekOrigin.Current);
            long length = BaseStream.Position;
            packet.WriteTo (this);
            if (packet is LoginRequest) return;
            length = BaseStream.Position - length;
            BaseStream.Seek (-length - 7, SeekOrigin.Current);
            Write (type);
            Write ((byte) 0);
            Write ((uint) length);
            BaseStream.Seek (length, SeekOrigin.Current);
        }
        public void Write (DateTime time) {
            this.Write (time.ToUniversalTime ().Ticks);
        }
        public void Write<T> (List<T> packet_list) where T : IPacket {
            if (packet_list == null) {
                this.Write (-1);
                return;
            }
            int count = packet_list.Count;
            this.Write (count);
            for (int i = 0; i < count; i++) {
                packet_list[i].WriteTo (this);
            }
        }

    }

}