using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Wildlife.PitayaCSharp.Client;

namespace Wildlife.PitayaCSharp.Compression
{
    public class Gzip
    {
        public static byte[] DeflateData(byte[] data)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Compress))
                {
                    zipStream.Write(data, 0, data.Length);
                }
                return compressedStream.ToArray();
            }
        }

        public static byte[] InflateData(byte[] data)
        {
            using (var compressedStream = new MemoryStream(data))
            using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            using (var decompressedStream = new MemoryStream())
            {
                if (compressedStream.ReadByte() != 0x78 || compressedStream.ReadByte() != 0x9C)
                {
                    throw new Exception("Incorrect zlib header");
                }

                zipStream.CopyTo(decompressedStream);
                return decompressedStream.ToArray();
            }
        }

        public static bool IsCompressed(byte[] data)
        {
            return data.Length > 2 &&
            (
                // zlib
                (data[0] == 0x78 &&
                (data[1] == 0x9C ||
                data[1] == 0x01 ||
                data[1] == 0xDA ||
                data[1] == 0x5E)) ||
                // gzip
                (data[0] == 0x1F && data[1] == 0x8B));
        }
    }
}
