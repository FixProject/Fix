using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OwinHelpers
{
    public static class FileInfoExtension
    {
        public static ArraySegment<byte> ToOwinBody(this FileInfo fileInfo)
        {
            return new ArraySegment<byte>(new byte[1].Concat(Encoding.ASCII.GetBytes(fileInfo.FullName)).ToArray());
        }
    }
}
