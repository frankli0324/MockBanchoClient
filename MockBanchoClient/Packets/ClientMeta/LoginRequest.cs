using System;
using System.IO;
using System.Text;
using MockBanchoClient.Serialization;

namespace MockBanchoClient.Packets {
    [Send (256)]
    public class LoginRequest : IPacket {
        string login_info;
        public LoginRequest (string username, string password, string version, string timezone, string client_hash) {
            login_info = "" +
                username + '\n' +
                password.Md5 () + '\n' +
                $"{version}|{timezone}|0|{client_hash}|0|" + '\n';
        }
        public void ReadFrom (BanchoPacketReader reader) {
            throw new System.NotImplementedException ();
        }

        public void WriteTo (BanchoPacketWriter writer) {
            writer.BaseStream.Seek (-7, SeekOrigin.Current);
            writer.BaseStream.Write (Encoding.UTF8.GetBytes (login_info));
        }
    }
}