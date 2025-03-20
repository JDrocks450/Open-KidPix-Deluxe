using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KidPix.API.Util
{
    public static class StreamExtensions
    {
        public static void CopyToExact(this Stream Stream, Stream Destination, int Length) => CopyToExact(Stream, Destination, (uint)Length);
        
        public static void CopyToExact(this Stream Stream, Stream Destination, uint Length)
        {
            byte[] buffer = new byte[Length];
            int read = 0, totalRead = 0;
            do
            {
                read = Stream.Read(buffer, 0, buffer.Length);
                if (read == 0) break;
                totalRead += read;
                Destination.Write(buffer, 0, read);
            }
            while (totalRead < Length);
        }
    }
}
