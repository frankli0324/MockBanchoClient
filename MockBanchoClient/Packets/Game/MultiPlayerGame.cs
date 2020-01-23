using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Recv (81)]
    public class MatchPlayerSkipped : IPacket {
        public int player;
        public void ReadFrom (BanchoPacketReader reader) {
            player = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Send (87)][Recv (88)]
    public class MultiInvite : IPacket {
        public Message message;
        public int user_id;
        public MultiInvite (int user_id) => this.user_id = user_id;
        public void ReadFrom (BanchoPacketReader reader) {
            message = reader.ReadMessage ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.Write (user_id);
        }
    }

    [Send (90)][Recv (91)]
    public class MultiChangePassword : IPacket {
        public string password;
        public MultiChangePassword (string password) => this.password = password;
        public void ReadFrom (BanchoPacketReader reader) {
            password = reader.ReadString ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.Write (password);
        }
    }
}