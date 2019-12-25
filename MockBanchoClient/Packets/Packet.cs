using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [AttributeUsage (AttributeTargets.Class)]
    public class RecvAttribute : Attribute {
        public ushort packet_code { get; private set; }
        public RecvAttribute (ushort code) {
            this.packet_code = code;
        }
    }

    [AttributeUsage (AttributeTargets.Class)]
    public class SendAttribute : Attribute {
        public ushort packet_code { get; private set; }
        public SendAttribute (ushort code) {
            this.packet_code = code;
        }
    }
    public interface IPacket {
        void WriteTo (BanchoPacketWriter writer);
        void ReadFrom (BanchoPacketReader reader);
    }
    public static class PacketFactory {
        private static Dictionary<ushort, Type> loadedPacketTypes =
            new Dictionary<ushort, Type> ();
        private static bool typesLoaded = false;
        private static void LoadPacketTypes () {
            var q = (
                from t in Assembly
                .GetExecutingAssembly ()
                .GetExportedTypes () where t.Namespace == MethodBase
                .GetCurrentMethod ()
                .DeclaringType.Namespace && t
                .GetCustomAttribute (
                    typeof (RecvAttribute)
                ) != null select t
            ).ToList ();
            foreach (var i in q) {
                loadedPacketTypes[(i.GetCustomAttribute (
                    typeof (RecvAttribute)
                ) as RecvAttribute).packet_code] = i;
            }
        }
        public static IPacket CreatePacket (
            ushort packet_type,
            BanchoPacketReader reader
        ) {
            if (!typesLoaded) { LoadPacketTypes (); typesLoaded = true; }
            if (loadedPacketTypes.ContainsKey (packet_type)) {
                IPacket packet = (IPacket) loadedPacketTypes[packet_type].GetConstructor (
                    new Type[] { }
                ).Invoke (new object[] { });
                loadedPacketTypes[packet_type].GetMethod (
                    "ReadFrom",
                    new Type[] { typeof (BanchoPacketReader) }
                ).Invoke (
                    packet,
                    new object[] { reader }
                );
                return packet;
            }
            throw new KeyNotFoundException ("Unknown Packet Type: " + packet_type);
        }
    }
}