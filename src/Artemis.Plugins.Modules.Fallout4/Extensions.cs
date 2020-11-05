using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Artemis.Plugins.Modules.Fallout4
{
    internal static class Extensions
    {
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char c;

            while ((c = reader.ReadChar()) != char.MinValue && reader.BaseStream.Position < reader.BaseStream.Length)
                builder.Append(c);

            return builder.ToString();
        }

        public static byte[] FullRead(this NetworkStream s, uint expectedSize)
        {
            byte[] data = new byte[expectedSize];

            int size = (int)expectedSize;
            int total = 0;

            //we might need to receive multiple packets to get all the data
            while (total < size)
            {
                int recv = s.Read(data, total, size - total);
                if (recv == 0)
                {
                    break;
                    //maybe should handle this differently?
                }
                total += recv;
            }

            return data;
        }
    }
}
