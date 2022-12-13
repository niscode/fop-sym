using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// human tracker に接続し、L2のデータを受け取る。
// (シーン中のL2Generatorにアタッチが必要。)
// 他のスクリプトが、受け取られたデータを得るには、 getData を呼ぶ。

public class L2Bridge : MonoBehaviour
{
    public string hostname = "hil-mouse02"; // ip: 10.186.38.42
    public int portN = 7003;

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    //private static L2DataDict l2datadict;
    private bool running = true;

    public class L2Data
    {
        public long id;         // Unique ID
                                // public int type; 
        public DateTime time;   // 最後に検出されていた時刻  server_time
        public DateTime recieve_time;   // 最後に受信した時刻 ASPFBridgeが動いている時刻
        public Vector3 pos;     // 検出されたX・Y座標（Unity上ではX,Z軸成分に格納）を格納　高さ（Y軸成分）に0.6fを格納。
    }

    // UI
    public 

    // Start is called before the first frame update
    void Start()
    {
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // サーバに接続しに行くメソッド
    private void ConnectToTcpServer()
    {
        try
        {
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    // サーバーからのデータを改行まで読み込む。
    private String ReadLine(NetworkStream stream)
    {
        String line = "";
        return (line);

        //int d;
        //while (running)
        //{
            //if (stream.DataAvailable)
            //{
                //d = stream.ReadByte();
                //if (d == 10)
                //{ // \n = 10?
                //    return (line);
                //}
                //else
                //{
                //    if (d != 13)
                //    {
                //        line += Convert.ToChar(d);
                //    }
                //}
            //}
            //else
            //{
            //    // ミリ秒数 スレッドが中断される
            //    System.Threading.Thread.Sleep(50);
            //}
        //}

        //return (line);
    }

    // human tracker から得られた文字列を、データに変換する。
    // ==================================================
    // 1670382494,663,1,HT,erica,     (5items & 1json/line)
    // {    "htType": "Human",
    //      "isSpeaking": "false",
    //      "sourceType": "HT3D",
    //      "timestamp": "1.670382494663E+09",
    //      "head": "-1.53982138634",
    //      "velocity": "1.17023420334",
    //      "optionFields": "{}",
    //      "uniqueID": "2",
    //      "oTheta": "-1.53982138634",
    //      "y": "-2110.47143555",
    //      "x": "4767.90869141",
    //      "z": "1284.06176758",
    //      "type": "0",
    //      "id": "12081404",
    //      "k2body": "",
    //      "mTheta": "-1.53982138634"
    // }
    // ==================================================
    // unix_time_sec, unix_time_msec, ...
    private List<L2Data> ParseL2String(string line)
    {
        List<L2Data> ret = new List<L2Data>();
        string[] items = line.Split(',');

        long unix_time_msec = long.Parse(items[0]) * 1000 + long.Parse(items[1]);               // unix time (msec)
        DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(unix_time_msec).LocalDateTime;  // unix_time -> DateTime
        DateTime now = DateTime.Now;
        int n = int.Parse(items[2]);  // 検出された人数
        


        //if (items.Length > 9)
        //{
        //    long unix_time_msec = long.Parse(items[0]) * 1000 + long.Parse(items[1]); // unix time (msec)
        //    DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(unix_time_msec).LocalDateTime; // unix_time -> DateTime
        //    DateTime now = DateTime.Now;
        //    int n = int.Parse(items[2]);
        //    int head = 3;
        //    for (int i = 0; i < n; i++)
        //    {
        //        L2Data l2data = new L2Data();
        //        l2data.time = time;
        //        l2data.receive_time = now;
        //        l2data.id = long.Parse(items[head]); // human tracker でつけられた ID
        //        l2data.type = int.Parse(items[head + 1]); // type
        //        l2data.p.x = float.Parse(items[head + 2]); // x 座標
        //        l2data.p.y = float.Parse(items[head + 3]); // y 座標
        //                                                   // 未解明なデータが、 4 5                6                  7                      8
        //                                                   //                   -1 8.14641676737030 -0.551659472481993 -0.036899607406078500  -1
        //        ret.Add(l2data);
        //        head += 9;
        //    }
        //}

        return (ret);
    }

    // サーバからデータを受け取りに行くメソッド
    private void ListenForData()
    {
        try
        {
            // HumanTracker のホスト名と, ポート番号
            socketConnection = new System.Net.Sockets.TcpClient(hostname, portN);
            while (running)
            {
                // Get a stream object for reading 				
                using (NetworkStream stream = socketConnection.GetStream())
                {
                    SendStartMessage();
                    Debug.Log("Sucess connecting to server");
                    String line;
                    while ((line = ReadLine(stream)) != null && running)
                    {
                        // ParseL2Stringメソッドを使って
                        List<L2Data> l2datalist = ParseL2String(line);

                        //lock (l2datadict)
                        //{
                        //    l2datadict.Clear();
                        //    foreach (L2Data l2data in l2datalist)
                        //    {
                        //        l2datadict.Add(l2data.id, l2data); // 新規にデータを追加
                        //    }
                        //}
                    }
                }
                Debug.Log("Server connection end");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }


    // サーバにデータを送りに行くメソッド・・・接続時に GET HT 3.0\n を送る
    private void SendStartMessage()
    {
        if (socketConnection == null)
        {
            return;
        }

        try
        {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "GET HT 3.0\n";
                // Convert string message to byte array.
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent the starting message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

}
