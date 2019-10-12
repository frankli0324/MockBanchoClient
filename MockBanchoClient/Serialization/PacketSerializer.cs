using System;
using System.IO;
using MockBanchoClient.Packets;
namespace MockBanchoClient.Serialization {
    public static class PacketSerializer {
        /// <summary>
        /// Serializes a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static byte[] Serialize (IPacket packet) =>
            throw new NotImplementedException ();
        /// <summary>
        /// Unserializes a SINGLE Packet
        /// </summary>
        /// <param name="content"></param>
        /// <returns>the packet unserialized</returns>
        /// <remark>
        /// if multiple packets exists in the content, 
        /// this will only return the first packet unserialized
        /// </remark>
        public static IPacket Unserialize (byte[] content) =>
            new BanchoPacketReader (
                new MemoryStream (content)
            ).ReadPacket ();
    }
}