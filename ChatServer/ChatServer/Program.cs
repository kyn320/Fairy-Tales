using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;


namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server myServer = new Server(10000); // 포트 10000번을 열어서 서버를 구동시킨다. 포트값은 원하는대로 바꿔도 된다.

            while (true)
            {
                Thread.Sleep(1);
            }
        }
    }
}
