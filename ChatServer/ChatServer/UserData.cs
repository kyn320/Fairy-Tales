using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class UserData
    {
        public const int BufferSize = 32768;
        public Socket workSocket = null;
        public byte[] buffer = new byte[BufferSize];
        public int recvlen = 0;
    }
}
