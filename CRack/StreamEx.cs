using System.IO;

namespace CRack
{
    static class StreamEx
    {
        public static byte[] ToBytes(this Stream stream)
        {
            var bytes = new byte[stream.Length];
            if (bytes.Length > 0)
                stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
    }
}