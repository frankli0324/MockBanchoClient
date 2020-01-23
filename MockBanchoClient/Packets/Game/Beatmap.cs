using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    public enum Ranks {
        XH,
        SH,
        const_2,
        const_3,
        const_4,
        const_5,
        const_6,
        const_7,
        const_8,
        IDLE
    }

    [Recv (69)]
    public class BeatmapInfoReply : IPacket {
        public short id;
        public int beatmapId;
        public int beatmapSetId;
        public int threadId;
        public int ranked;
        public Ranks osuRank = Ranks.IDLE;
        public Ranks taikoRank = Ranks.IDLE;
        public Ranks fruitsRank = Ranks.IDLE;
        public Ranks maniaRank = Ranks.IDLE;
        public string checksum;

        public void ReadFrom (BanchoPacketReader sr) {
            id = sr.ReadInt16 ();
            beatmapId = sr.ReadInt32 ();
            beatmapSetId = sr.ReadInt32 ();
            threadId = sr.ReadInt32 ();
            ranked = sr.ReadByte ();
            osuRank = (Ranks) sr.ReadByte ();
            fruitsRank = (Ranks) sr.ReadByte ();
            taikoRank = (Ranks) sr.ReadByte ();
            maniaRank = (Ranks) sr.ReadByte ();
            checksum = sr.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter sw) {
            sw.Write (id);
            sw.Write (beatmapId);
            sw.Write (beatmapSetId);
            sw.Write (threadId);
            sw.Write ((byte) ranked);
            sw.Write ((byte) osuRank);
            sw.Write ((byte) fruitsRank);
            sw.Write ((byte) taikoRank);
            sw.Write ((byte) maniaRank);
            sw.Write (checksum);
        }
    }
}