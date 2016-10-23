using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Server
    {
        public static Random Rand = new Random(); // 랜덤 발생기를 생성한다.

        public static List<User> UserList = new List<User>(); // 접속한 모든 유저들의 정보를 담을 리스트
        public static ManualResetEvent allDone = new ManualResetEvent(false); // 스레드끼리 충돌을 방지하기 위한 객체

        public Server(int port) // port값으로 서버를 구동시킨다.
        {
            UserList.Clear(); // 모든 유저를 초기화한다.
            StartListening(port); // 클라이언트의 소켓 접속을 대기한다.
        }

        public static void DeleteUser(User temp)
        {
            UserList.Remove(temp);
        }

        public static void StartListening(int port)
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, port);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // TCP 프로토콜 리스너를 생성한다.
            listener.NoDelay = true;
            listener.LingerState = new LingerOption(true, 0);
            listener.SendBufferSize = 81920;
            listener.ReceiveBufferSize = 81920;

            try
            {
                listener.Bind(localEndPoint); // 설정된 값으로 소켓 바인딩을 처리한다.
                listener.Listen(100); // 동시에 접속받을 수 있는 큐의 크기를 정한다. 동시 접속자를 의미하는게 아니라 동시에 받을 수 있는 소켓 커넥션을 의미한다.
                Console.WriteLine("Waiting for a connection...");

                while (true)
                {
                    allDone.Reset();
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        static void AcceptCallback(IAsyncResult ar) // 클라이언트가 접속에 성공한 경우 호출되는 콜백 함수
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            handler.NoDelay = true;
            handler.LingerState = new LingerOption(true, 0);
            handler.SendBufferSize = 81920;
            handler.ReceiveBufferSize = 81920;

            UserList.Add(new User(handler));
        }
    }
}
