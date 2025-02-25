using System;
using System.Collections.Generic;

namespace Wildlife.PitayaCSharp.Protobuf
{
    public class Encoder
    {
        //Encode the UInt32.
        public static byte[] EncodeUInt32(string n)
        {
            return EncodeUInt32(Convert.ToUInt32(n));
        }

        /// <summary>
        /// Encode the UInt32.
        /// </summary>
        /// <returns>
        /// byte[]
        /// </returns>
        /// <param name='n'>
        /// int
        /// </param>
        public static byte[] EncodeUInt32(uint n)
        {
            List<byte> byteList = new List<byte>();
            do
            {
                uint b = n & 0x7F;
                n >>= 7;
                if (n != 0)
                {
                    b += 128;
                }

                byteList.Add(Convert.ToByte(b));
            } while (n != 0);

            return byteList.ToArray();
        }

        //Encode SInt32
        public static byte[] EncodeSInt32(string n)
        {
            return EncodeSInt32(Convert.ToInt32(n));
        }

        /// <summary>
        /// Encodes the SInt32.
        /// </summary>
        /// <returns>
        /// byte []
        /// </returns>
        /// <param name='n'>
        /// int
        /// </param>
        public static byte[] EncodeSInt32(int n)
        {
            UInt32 num = (uint) (n < 0 ? (Math.Abs(n) * 2 - 1) : n * 2);
            return EncodeUInt32(num);
        }

        /// <summary>
        /// Encodes the float.
        /// </summary>
        /// <returns>
        /// byte []
        /// </returns>
        /// <param name='n'>
        /// float.
        /// </param>
        public static byte[] EncodeFloat(float n)
        {
            byte[] bytes = BitConverter.GetBytes(n);
            if (!BitConverter.IsLittleEndian)
            {
                Util.Util.Reverse(bytes);
            }

            return bytes;
        }

        //Get the byte length of message.
        public static int ByteLength(string msg)
        {
            return System.Text.Encoding.UTF8.GetBytes(msg).Length;
        }
    }
}
