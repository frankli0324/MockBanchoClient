using System;
using System.Text;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    public class LoginPacket : IPacket {
        string login_info;
        public LoginPacket (string username, string password, string version, string timezone, string client_hash) {
            login_info = "" +
                username + '\n' +
                password.Md5 () + '\n' +
                $"{version}|{timezone}|0|{client_hash}|0|" + '\n';
        }
        public void ReadFrom (BanchoPacketReader reader) {
            throw new System.NotImplementedException ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.BaseStream.Write (Encoding.UTF8.GetBytes (login_info));
        }
    }
}