using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {

    [Packet (0)]
    public class UserStatus : IPacket {
        public enum PlayerAction {
            Idle,
            Afk,
            Playing,
            Editing,
            Modding,
            Multiplayer,
            Watching,
            Unknown,
            Testing,
            Submitting,
            Paused,
            Lobby,
            Multiplaying,
            OsuDirect
        }
        public enum GameMode : byte {
            Standard,
            Taiko,
            CatchTheBeat,
            Mania
        }
        [System.Flags]
        public enum Mods : uint {
            None = 0,
            NoFail = 1 << 0,
            Easy = 1 << 1,
            TouchDevice = 1 << 2, //previously NoVideo
            Hidden = 1 << 3,
            HardRock = 1 << 4,
            SuddenDeath = 1 << 5,
            DoubleTime = 1 << 6,
            Relax = 1 << 7,
            HalfTime = 1 << 8,
            Nightcore = 1 << 9,
            Flashlight = 1 << 10,
            Autoplay = 1 << 11,
            SpunOut = 1 << 12,
            Relax2 = 1 << 13, //AutoPilot
            Perfect = 1 << 14,
            Key4 = 1 << 15,
            Key5 = 1 << 16,
            Key6 = 1 << 17,
            Key7 = 1 << 18,
            Key8 = 1 << 19,
            FadeIn = 1 << 20,
            Random = 1 << 21,
            Cinema = 1 << 22,
            Target = 1 << 23,
            Key9 = 1 << 24,
            KeyCoop = 1 << 25,
            Key1 = 1 << 26,
            Key3 = 1 << 27,
            Key2 = 1 << 28,
            ScoreV2 = 1 << 29,
        }

        public PlayerAction action;
        public string action_description;
        public string beatmap_checksum;
        public Mods mods;
        public GameMode mode;
        public int beatmap_id;
        public void ReadFrom (BanchoPacketReader reader) {
            action = (PlayerAction) reader.ReadByte ();
            action_description = reader.ReadString ();
            beatmap_checksum = reader.ReadString ();
            mods = (Mods) reader.ReadUInt32 ();
            mode = (GameMode) reader.ReadByte ();
            beatmap_id = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }
}