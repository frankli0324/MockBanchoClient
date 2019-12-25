using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    public class Message : IPacket {
        public string sender, message, channel;
        public int sender_id;
        public void ReadFrom (BanchoPacketReader reader) {
            sender = reader.ReadString ();
            message = reader.ReadString ();
            channel = reader.ReadString ();
            sender_id = reader.ReadInt32 ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.Write (sender);
            writer.Write (message);
            writer.Write (channel);
            writer.Write (sender_id);
        }
    }

    [Recv (7)]
    public class ChatMessage : IPacket {
        public Message message;
        public void ReadFrom (BanchoPacketReader reader) {
            message = new Message ();
            message.ReadFrom (reader);
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Send (25)]
    public class PrivateChatMessage : IPacket {
        Message message;
        public PrivateChatMessage (Message m) => message = m;
        public void ReadFrom (BanchoPacketReader reader) {
            throw new System.NotImplementedException ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            message.WriteTo (writer);
        }
    }

    [Send (1)]
    public class PublicChatMessage : IPacket {
        Message message;
        public PublicChatMessage (Message m) => message = m;
        public void ReadFrom (BanchoPacketReader reader) {
            throw new System.NotImplementedException ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            message.WriteTo (writer);
        }
    }

    [Send (87)][Recv (88)]
    public class MultiInvite : IPacket {
        public Message message;
        int user_id;
        public MultiInvite (int user_id) {
            this.user_id = user_id;
        }
        public void ReadFrom (BanchoPacketReader reader) {
            message = new Message ();
            message.ReadFrom (reader);
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.Write (user_id);
        }
    }

}