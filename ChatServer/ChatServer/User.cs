using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

enum MOVE
{
    STOP,
    DOWN,
    UP,
    LEFT,
    RIGHT,
}

namespace ChatServer
{
    class User
    {
        string nick;
        UserData data = new UserData(); // 소켓, 버퍼, 데이터 길이 등을 저장할 클래스 변수를 생성한다.
        float x = 0, y = 0;
        MOVE myMove = MOVE.STOP, dir = MOVE.STOP;
        int avatarType = 0;

        public User(Socket socket) // User 클래스의 생성자
        {
            data.workSocket = socket; // UserData의 workSocket를 서버에 연결된 소켓으로 설정한다.

            // 비동기 소켓 리시브를 실행한다. 클라이언트에서 데이터가 도착하면 ReadCallback이 자동으로 호출된다.
            data.workSocket.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
            WriteLine("CONNECT"); // 유저가 접속했을때 곧바로 클라이언트로 보내지는 패킷
        }

        void ReadCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = data.workSocket;
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    data.recvlen += bytesRead;

                    while (true)
                    {
                        short length;
                        Util.GetShort(data.buffer, 0, out length);

                        if (length > 0 && data.recvlen >= length)
                        {
                            ParsePacket(length);
                            data.recvlen -= length;

                            if (data.recvlen > 0)
                            {
                                Buffer.BlockCopy(data.buffer, length, data.buffer, 0, data.recvlen);
                            }
                            else
                            {
                                handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                                break;
                            }
                        }
                        else
                        {
                            handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                            break;
                        }
                    }
                }
                else
                {
                    handler.BeginReceive(data.buffer, data.recvlen, UserData.BufferSize, 0, new AsyncCallback(ReadCallback), data);
                }
            }

            catch (Exception)
            {
                Server.DeleteUser(this);
                Console.WriteLine("Socket Closed.");
            }
        }

        void Login() // 서버에 저장된 유저 리스트에 따라 모든 클라이언트가 동일한 유저 리스트를 갖도록 정보를 순차적으로 보낸다.
        {
            for (int i = 0; i < Server.UserList.Count; i++) // 서버에 접속된 모든 유저 검색
            {
                if (Server.UserList[i] != this) // 내가 아닌 다른 유저들인 경우에만
                {
                    // 나 자신에게 먼저 접속해 있던 모든 유저들의 정보를 보낸다.
                    //WriteLine(string.Format("USER:{0}:{1}:{2}:{3}:{4}:{5}", Server.UserList[i].nick, Server.UserList[i].x, Server.UserList[i].y, (int)Server.UserList[i].myMove, (int)Server.UserList[i].dir, Server.UserList[i].avatarType));
                    WriteLine(string.Format("USER:{0}:{1}",Server.UserList[i].x,Server.UserList[i].y));
                    // 기존에 접속해 있던 모든 유저들에게 내 정보를 보낸다.
                    Server.UserList[i].WriteLine(string.Format("USER:{0}:{1}", 0, 0));
                    //Server.UserList[i].WriteLine(string.Format("USER:{0}:{1}:{2}:{3}:{4}:{5}", nick, 0, 0, (int)MOVE.STOP, (int)MOVE.DOWN, avatarType));
                }
                else
                {
                    // 나 자신이 접속했음을 알린다.
                    Console.WriteLine("[Login] "+nick+" is login");
                    WriteLine(string.Format("ADDUSER"));
                }
            }
        }

        void Chat(string text)
        {
            int index = Server.UserList.IndexOf(this); // 나의 인덱스 번호 얻어오기

            for (int i = 0; i < Server.UserList.Count; i++) // 서버에 접속된 모든 유저 검색
            {
                Server.UserList[i].WriteLine(string.Format("CHAT:{0}:{1}", index, text)); // index 번호의 유저가 채팅을 보냈음을 알린다.
            }
            Console.WriteLine("[Chat] " + nick + " : "+text);
        }

        void Disconnect()
        {
            int index = Server.UserList.IndexOf(this); // 나의 인덱스 번호 얻어오기

            for (int i = 0; i < Server.UserList.Count; i++) // 서버에 접속된 모든 유저 검색
            {
                if (Server.UserList[i] != this) // 내가 아닌 다른 유저들인 경우에만
                {
                    Server.UserList[i].WriteLine(string.Format("REMOVE:{0}", index)); // index 번호가 접속을 끊었음을 알린다.
                }
            }
            Console.WriteLine("[Logout] " + nick + " is logout");
        }

        void Move()
        {
            int index = Server.UserList.IndexOf(this); // 나의 인덱스 번호 얻어오기

            for (int i = 0; i < Server.UserList.Count; i++) // 서버에 접속된 모든 유저 검색
            {
                if (Server.UserList[i] != this) // 내가 아닌 다른 유저들인 경우에만
                {
                    Server.UserList[i].WriteLine(string.Format("MOVE:{0}:{1}:{2}", index, x, y)); // 내 인덱스 번호와 현재 위치 이동할 방향을 보낸다.
                }
            }
            //if (myMove > MOVE.STOP) dir = myMove; // STOP이 아닌 경우 마지막 바라보던 방향을 저장해둔다.
            Console.WriteLine("[Move] " + index + " | x : "+x+" | y :"+y);
        }

        private void ParsePacket(int length)
        {
            string msg = Encoding.UTF8.GetString(data.buffer, 2, length - 2);
            string[] text = msg.Split(':');

            if (text[0].Equals("CONNECT")) // 유저가 CONNECT를 받았을때 서버로 보내지는 패킷
            {
                //nick = text[1]; // 유저의 닉네임
                //avatarType = int.Parse(text[2]); // 유저의 아바타 타입 0~4까지 5가지
                Console.WriteLine("connected");
                Login();
            }
            else if (text[0].Equals("DISCONNECT")) // 유저가 게임을 종료했을때 서버로 보내지는 패킷
            {
                if (nick.Length > 0)
                {
                    Console.WriteLine(nick + " is disconnected");
                    Disconnect();
                }
                data.workSocket.Shutdown(SocketShutdown.Both);
                data.workSocket.Close();
            }
            else if (text[0].Equals("MOVE")) // 유저가 보낸 유저 이동 패킷
            {
                x = float.Parse(text[1]); // 현재 X좌표
                y = float.Parse(text[2]); // 현재 Y좌표
                //myMove = (MOVE)int.Parse(text[3]); // 이동할 방향값
                Move(); // 다른 유저에게 이동 패킷 전송
            }
            else if (text[0].Equals("CHAT")) // 유저가 보낸 채팅 패킷 처리
            {
                Chat(text[1]);
            }
        }

        byte[] ShortToByte(int val)
        {
            byte[] temp = new byte[2];
            temp[1] = (byte)((val & 0x0000ff00) >> 8);
            temp[0] = (byte)((val & 0x000000ff));
            return temp;
        }

        void WriteLine(string text) // 문자열을 클라이언트로 보내준다. 문자열 최대 크기 4096 바이트 (크기를 늘려도 무관하다)
        {
            try
            {
                if (data.workSocket != null && data.workSocket.Connected) // 소켓이 연결된 상태이면
                {
                    byte[] buff = new byte[4096]; // 바이트 배열 버퍼 생성
                    Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2); // 문자열 크기 2바이트를 버퍼에 먼저 넣는다.
                    Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length); // 실제 문자열을 바이트로 변환해서 버퍼에 추가한다.
                    data.workSocket.Send(buff, Encoding.UTF8.GetBytes(text).Length + 2, 0); // 문자열 크기 + 길이 2바이트를 소켓으로 송신한다.
                }
            }

            catch (System.Exception ex) // 패킷 전송때 소켓 에러 발생시
            {
                if (nick.Length > 0) Disconnect();

                data.workSocket.Shutdown(SocketShutdown.Both); // 소켓을 셧다운
                data.workSocket.Close(); // 소켓을 닫는다.

                Server.DeleteUser(this); // 유저 리스트에서 삭제한다.
                Console.WriteLine("WriteLine Error : " + ex.Message);
            }
        }
    }
}
