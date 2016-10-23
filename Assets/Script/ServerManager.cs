using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System;


public class ServerManager : MonoBehaviour
{
    static Socket SocketTCP = null; // Socket 변수 선언
    public string addr = "127.0.0.1"; // 바야바 개인 서버 주소이다. 자신의 PC에서 서버를 실행한 경우 아이피를 127.0.0.1로 바꿔준다.
    //string addr = "127.0.0.1";
    int port = 10000; // 서버의 포트값을 갖는다. 반드시 서버에서 설정된 포트값과 같아야 접속이 된다.
    byte[] buffer = new byte[4096]; // 서버에서 받은 데이터의 버퍼. 서버에서 보내지는 데이터를 이 버퍼에 계속 누적시킨다. 버퍼가 초과할 만큼 데이터가 많이 보내질 가능성이 있으면 버퍼 사이즈를 늘려줘야 한다.
    int recvlen = 0; // 실제로 받은 데이터의 길이

    static ServerManager instance; // 싱글톤 객체
    public static ServerManager Instance { get { return instance; } } // 싱글톤 인스턴스

    public List<PlayerControl> UserList = new List<PlayerControl>();
    

    public GameObject[] UserPrefabs;
    public int AvatarType = 0;

    bool m_paused = false;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        /*
        if (Input.GetMouseButtonUp(0)) // 터치와 마우스 입력은 동일. 같은 위치에서 뗄시 이동.                               
        {

            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Ray2D ray = new Ray2D(wp, Vector2.zero);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);
            if (hit.collider != null)
            {
                print("hit");
                if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject.GetComponent<PlayerControl>().isPlayer == false)
                {
                    int index = UserList.IndexOf(hit.collider.gameObject.GetComponent<PlayerControl>());
                    pannel[2].transform.GetChild(0).GetComponent<UI_VSinfo>().index = index;
                    pannel[2].transform.GetChild(0).GetComponent<UI_VSinfo>().master = true;
                    pannel[2].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = hit.transform.gameObject.GetComponent<Player>().NickName.text + " 에게 대결을 걸까?";
                    pannel[2].SetActive(true);
                }
            }
        }
        */
    }


    public void StartConnect() // Canvas의 Start 버튼이 눌렸을때 호출된다.
    {

            Disconnect(); // 기존에 접속된 상태일 수 있기 때문에 먼저 접속을 초기화한다.

            IPAddress serverIP = IPAddress.Parse(addr); // 서버 아이피 어드레스 설정
            int serverPort = Convert.ToInt32(port); // 포트값을 변환
            SocketTCP = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // TCP 소켓을 생성한다.
            SocketTCP.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 10000); // 송신 제한시간을 10초로 제한한다.
            SocketTCP.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000); // 수신 제한시간을 10초로 제한한다.
            SocketTCP.Connect(new IPEndPoint(serverIP, serverPort)); // 서버로 접속을 시도한다.
            StartCoroutine(PacketProc()); // 서버로부터 들어오는 패킷 데이터 처리를 위한 코루틴을 실행한다.
        
    }

    public void WriteLine(string text) // 서버로 문자열을 보내준다.
    {
        try
        {
            if (SocketTCP != null && SocketTCP.Connected) // 서버에 연결된 상태인지 체크
            {
                byte[] buff = new byte[4096]; // 보낼 데이터의 임시 버퍼

                // 처음 2바이트를 문자열 길이를 나타내는 것으로 한다. 여기에 문자열 헤더를 추가해도 되고, Json으로 변환도 가능하다.
                Buffer.BlockCopy(ShortToByte(Encoding.UTF8.GetBytes(text).Length + 2), 0, buff, 0, 2);

                // 실제 보낼 문자열 데이터를 byte로 변환해서 버퍼에 넣는다.
                Buffer.BlockCopy(Encoding.UTF8.GetBytes(text), 0, buff, 2, Encoding.UTF8.GetBytes(text).Length);

                // 데이터 길이 2바이트 + 실제 문자열 데이터로 구성된 버퍼를 서버로 송신한다.
                SocketTCP.Send(buff, Encoding.UTF8.GetBytes(text).Length + 2, 0);
            }
        }
        catch (System.Exception)
        {

        }
    }

    public void SendChat(InputField input)
    {
        if (!(string.IsNullOrEmpty(input.text.Replace(" ", ""))))
        {
            WriteLine(string.Format("CHAT:{0}", input.text));
            input.text = "";

        }
        input.DeactivateInputField();
    }

    public void Disconnect() // 서버 연결을 끊는다.
    {
        if (SocketTCP != null && SocketTCP.Connected) SocketTCP.Close(); // 서버가 연결된 상태면 소켓을 닫는다.
        StopCoroutine(PacketProc()); // 코루틴을 중단시킨다.
    }

    IEnumerator PacketProc() // 패킷 처리 코루틴 함수
    {
        while (true)
        {
            if (SocketTCP.Available > 0) // 소켓에서 들어온 데이터가 있으면 SocketTCP.Available은 수신받은 데이터 길이를 나타낸다.
            {
                byte[] buff = new byte[4096]; // 데이터를 받기 위한 임시 버퍼
                int nread = SocketTCP.Receive(buff, SocketTCP.Available, 0); // 소켓이 수신한 데이터를 buff로 읽어온다.

                if (nread > 0) // Receive 함수는 실제로 데이터를 받은 길이를 리턴한다. 이 값이 0 이상이면 실제로 뭔가를 받아온 상태이다.
                {
                    Buffer.BlockCopy(buff, 0, buffer, recvlen, nread); // 방금 받아온 데이터를 buffer에 누적시킨다.
                    recvlen += nread; // 실제로 받아온 데이터 길이를 증감시킨다.

                    while (true)
                    {
                        int length = BitConverter.ToInt16(buffer, 0); // 서버에서 보내지는 데이터도 맨앞의 2바이트는 길이를 나타내므로 길이를 얻어온다.

                        if (length > 0 && recvlen >= length) // 서버에서 실제로 받아온 데이터의 길이 recvlen이 문자열 길이 + 2바이트 보다 크면 패킷 1개가 온전히 도착했음을 의미한다.
                        {
                            ParsePacket(length); // 패킷 1개가 도착했기 때문에 파싱해서 처리한다.
                            recvlen -= length; // 패킷 처리가 끝났기 때문에 1개의 길이를 감소시킨다.
                            Buffer.BlockCopy(buffer, length, buffer, 0, recvlen); // 감소된 데이터 길이 만큼 버퍼를 제거해준다.
                        }
                        else
                        {
                            break; // 버퍼에 패킷이 없는 상태면 루프에서 빠져나간다. while 문으로 처리한 이유는 서버에서 1개 이상의 패킷이 동시에 이어서 올수도 있기 때문이다.
                        }
                    }
                }
            }
            yield return null;
        }
    }

    public static byte[] ShortToByte(int val) // int 변수를 2바이트 byte 데이터로 변환시켜주는 함수
    {
        byte[] temp = new byte[2];
        temp[1] = (byte)((val & 0x0000ff00) >> 8);
        temp[0] = (byte)((val & 0x000000ff));
        return temp;
    }

    public void CreatePlayer(Vector3 pos,  bool isPlayer) //MOVE move, MOVE dir, string nick,int type,
    {
        GameObject temp = Instantiate(UserPrefabs[0], pos, Quaternion.identity) as GameObject;
        PlayerControl player = temp.GetComponent<PlayerControl>();

        /*
        player.myMove = move;
        player.SetDirection(dir);
        */
        //player.NickName.text = nick;
        player.isPlayer = isPlayer;
        if (isPlayer)
            CameraControl.instance.SetPlayer(player);
        else {
            temp.GetComponent<Rigidbody2D>().isKinematic = true;
        }

        UserList.Add(player);
    }

    public void ParsePacket(int length) // 서버에서 수신된 패킷을 파싱해서 처리하는 함수
    {
        string msg = Encoding.UTF8.GetString(buffer, 2, length - 2); // 길이를 나타내는 버퍼의 선두 2바이트를 제외하고 뒤에 실제 문자열을 가져온다.
        string[] text = msg.Split(':'); // 이 게임에서는 ':' 문자를 구분자로 사용하기 때문에 이 문자를 기준으로 스플릿 시켜준다.

        if (text[0].Equals("CONNECT")) // 접속이 성공하는 순간 서버에서 보내지는 첫번째 패킷이다.
        {
            Debug.Log("server connected...");
            WriteLine(string.Format("CONNECT")); // NickName.text, AvatarType)); // 서버로 나의 닉네임을 보내준다.
        }
        else if (text[0].Equals("USER")) // 내가 아닌 다른 유저의 플레이어를 생성한다.
        {
            print("asdasd111");
            CreatePlayer(new Vector3(float.Parse(text[1]), float.Parse(text[2])),false);//int.Parse(text[6]), new Vector3(float.Parse(text[2]), float.Parse(text[3]), 0f), text[1], false); // (MOVE)int.Parse(text[4]), (MOVE)int.Parse(text[5]), text[1], false);
        }
        else if (text[0].Equals("ADDUSER")) // 나 자신의 플레이어를 생성한다.
        {
            print("asdasd111");
            CreatePlayer(Vector3.zero, true);//NickName.text, true); //MOVE.STOP, MOVE.DOWN, 
        }
        else if (text[0].Equals("CHAT")) // 채팅 패킷
        {
            int index = int.Parse(text[1]); // 유저 리스트의 인덱스 번호
            //UserList[index].SetChatText(text[2]); // 채팅 문자열
        }
        else if (text[0].Equals("MOVE")) // 유저 이동 패킷
        {
            int index = int.Parse(text[1]); // 유저 리스트의 인덱스 번호
            print(index);
            UserList[index].Moving(new Vector3(float.Parse(text[2]), float.Parse(text[3]), 0f)); // 유저 위치 설정
            //UserList[index].myMove = (MOVE)int.Parse(text[4]); // 이동 코드 설정
        }
        else if (text[0].Equals("REMOVE")) // 유저가 접속을 끊은 경우
        {
            int index = int.Parse(text[1]); // 유저 리스트의 인덱스 번호

            Destroy(UserList[index].gameObject); // 하이어라키에서 유저를 삭제한다.
            UserList.RemoveAt(index); // 유저 리스트에서도 삭제한다.
        }
        else if (text[0].Equals("TimeUpdate"))
        {
            
        }
    }

    void Logout()
    {
        if (SocketTCP != null && SocketTCP.Connected) // 서버에 접속된 상태이면
        {
            WriteLine("DISCONNECT"); // 서버로 DISCONNECT 패킷을 보내 접속을 끊어준다.
            Thread.Sleep(500); // 서버로 패킷이 온전히 도착할때까지 기다려준다.
            SocketTCP.Close(); // 소켓을 닫는다.
        }
        StopCoroutine(PacketProc()); // 코루틴을 중지시킨다.
    }

    void OnApplicationQuit()
    {
        print("asd1231564");
        Logout();
    }

    void OnDestroy() // 앱이 종료되는 순간 호출되는 함수
    {
        print("asd1231564");
        Logout();
    }
}
