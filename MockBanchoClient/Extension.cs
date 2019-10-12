using System;
using System.Security.Cryptography;
using System.Text;

namespace MockBanchoClient.Extension {
    public static class StringExtensions {

        /// <summary>
        /// Convert a hexadecimal string to a byte array
        /// </summary>
        public static byte[] ToByteArray (this string hexString) {
            byte[] retval = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
                retval[i / 2] = Convert.ToByte (hexString.Substring (i, 2), 16);
            return retval;
        }

        private static MD5 md5_hasher = MD5.Create ();
        public static string Md5 (this string str) {
            string res = "";
            foreach (var b in md5_hasher.ComputeHash (
                    Encoding.UTF8.GetBytes (str)
                )) res += b.ToString ("x2");
            return res;
        }
    }
}