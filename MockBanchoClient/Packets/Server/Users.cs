using System;
using System.Collections.Generic;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Packet (11)]
    public class UsersDetail : IPacket {
        public int user_id;
        public UserStatus status = new UserStatus ();
        public long score_ranked, score_total;
        public float acc;
        public int count_plays;
        public int rank;
        public short pp_count;
        public void ReadFrom (BanchoPacketReader reader) {
            user_id = reader.ReadInt32 ();
            status.ReadFrom (reader);
            score_ranked = reader.ReadInt64 ();
            acc = reader.ReadSingle ();
            count_plays = reader.ReadInt32 ();
            score_total = reader.ReadInt64 ();
            rank = reader.ReadInt32 ();
            pp_count = reader.ReadInt16 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Packet (83)]
    public class UserPresenceDetail : IPacket {
        [System.Flags]
        public enum PlayerRank : byte {
            None = 0b00000000, //Not logged in
            Default = 0b00000001, //Standard, logged in user
            Bat = 0b00000010, //Beatmap Approval Team
            Supporter = 0b00000100,
            Friend = 0b00001000, //unused
            SuperMod = 0b00010000, //peppy, blue name in chat
            Tournament = 0b00100000, //allowed to host tournaments
        }
        public enum GameMode : byte {
            Standard,
            Taiko,
            CatchTheBeat,
            Mania
        }

        public int user_id;
        public bool is_osu_client;
        public string username;
        public int timezone;
        public byte country;
        public PlayerRank role;
        public GameMode mode;
        public float longitude, latitude;
        public int rank;
        public void ReadFrom (BanchoPacketReader reader) {
            user_id = reader.ReadInt32 ();
            username = reader.ReadString ();
            timezone = reader.ReadByte () - 24;
            country = reader.ReadByte ();
            byte flag = reader.ReadByte ();
            role = (PlayerRank) (flag & 00011111);
            mode = (GameMode) ((flag & 11100000) >> 5);
            (longitude, latitude) = (reader.ReadSingle (), reader.ReadSingle ());
            rank = reader.ReadInt32 ();

            if (user_id < 0) user_id = -user_id;
            else is_osu_client = user_id != 0;
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Packet (95)]
    public class UserPresenceSingle : IPacket {
        public int user_id;
        public void ReadFrom (BanchoPacketReader reader) {
            this.user_id = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Packet (96)]
    public class UserPresenceBundle : IPacket {
        public List<int> onlineUsers;
        public void ReadFrom (BanchoPacketReader reader) {
            this.onlineUsers = reader.ReadInt32List ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }

    [Packet (12)]
    public class UserQuit : IPacket {
        public enum UserQuitType {
            FullDisconnect,
            StillInClient,
            StillOnIrc
        }
        public int user_id;
        public UserQuitType quit_type;
        public void ReadFrom (BanchoPacketReader reader) {
            user_id = reader.ReadInt32 ();
            quit_type = (UserQuitType) reader.ReadByte ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new NotImplementedException ();
        }
    }
}