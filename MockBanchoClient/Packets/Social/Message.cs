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

    static class MessageExtension {
        public static Message ReadMessage (this BanchoPacketReader reader) {
            Message message = new Message ();
            message.ReadFrom (reader);
            return message;
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

    [Recv (7)]
    public class ChatMessage : IPacket {
        public Message message;
        public void ReadFrom (BanchoPacketReader reader) {
            message = reader.ReadMessage ();
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

    [Recv (100)]
    /// <summary>
    /// Target user is not your friend and blocked privated messages from strangers
    /// </summary>
    public class MessageBlocked : IPacket {
        public Message message;
        public void ReadFrom (BanchoPacketReader reader) {
            message = reader.ReadMessage ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }

    [Recv (101)]
    /// <summary>
    /// Target user is silenced
    /// </summary>
    public class MessageSilenced : IPacket {
        public Message message;
        public void ReadFrom (BanchoPacketReader reader) {
            message = reader.ReadMessage ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            throw new System.NotImplementedException ();
        }
    }
}