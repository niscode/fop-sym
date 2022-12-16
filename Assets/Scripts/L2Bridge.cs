using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Newtonsoft.Json.Linq;

// ### L2Bridge ###
// 1. // human tracker に接続し、L2のデータを受け取る。
// シーン中のL2Generatorにアタッチが必要。
// 他のスクリプトが、受け取ったデータを得るには、 getData を呼ぶ。
// 
// Shogo Nishimura / niscode    Dec 2022


public class L2Bridge : MonoBehaviour
{
    public string hostname = "hil-mouse02.dil.atr.jp"; // ip: 10.186.38.42
    // public string hostname = "10.186.38.42";
    public int portN = 7003;
    public bool running = true;

    // UniqueID / time1 / time2 / pos.x , pos.z (total: 5 params)
    public class L2Data
    {
        public long id;         // Unique ID
        public DateTime time;   // 最後に検出されていた時刻  server_time
        public DateTime recieve_time;   // 最後に受信した時刻 ASPFBridgeが動いている時刻
        public Vector2 pos2d;     // 検出されたX・Y座標（Unity上ではX,Z軸成分に格納）を格納　高さ（Y軸成分）に0.6fを格納。
    }

    public class L2DataDict : Dictionary<long, L2Data>
    {
        public L2DataDict() : base()
        {
        }
        public L2DataDict(L2DataDict org) : base(org)
        {
        }
    };

    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private static L2DataDict l2datadict;
    

    /*--
      ------------------------------------------------------------------------
    --*/

    // Start is called before the first frame update
    void Start()
    {
        l2datadict = new L2DataDict();  // L2DataDictクラス
        ConnectToTcpServer();           // 起動時にサーバへ接続
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*--
      ------------------------------------------------------------------------
    --*/

    // サーバに接続しに行くメソッド ・・・・ Setup socket connection.
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
            Debug.LogError("OMG 起動時サーバへの接続に失敗 ・・・ On client connect exception: " + e);
        }
    }

    // サーバーからのデータを改行まで読み込む。
    private String ReadLine(NetworkStream stream)
    {
        String line = "";
        int byte_num;
        /* --
        int byteData;
        while (running)
        {
            if (stream.DataAvailable)
            {
                byteData = stream.ReadByte();
                Debug.Log("ReadByte()によるデータの取得：" + byteData);
            }
            else
            {
                System.Threading.Thread.Sleep(50);
            }
        }
        -- */
        while (running)
        {
            if (stream.DataAvailable)  // 読み取り対象のデータがあるかどうかを判定
            {
                byte_num = stream.ReadByte();   // ストリームから 1 バイトを読み取り、ストリーム内の位置を 1 バイト進める
                if (byte_num == 10)
                {
                    return (line);
                }
                else
                {
                    if (byte_num != 13)
                    {
                        line += Convert.ToChar(byte_num);   // 指定した値を Unicode 文字に変換
                    }
                }
            }
            else
            {
                System.Threading.Thread.Sleep(50);
            }
        }
        return (line);
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


    // csvからデータを一行ずつリードするスクリプトを作ろう。
    private List<L2Data> ParseL2String(string line)
    {
        List<L2Data> ret = new List<L2Data>();
        string[] items = line.Split(',');

        long unix_time_msec = long.Parse(items[0]) * 1000 + long.Parse(items[1]);               // unix time (msec)
        DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(unix_time_msec).LocalDateTime;  // unix_time -> DateTime
        DateTime now = DateTime.Now;
        int n = int.Parse(items[2]);  // 検出された人数
        //JObject json = JObject.Parse(items[5]);

        L2Data l2data = new L2Data();
        l2data.time = time;
        l2data.recieve_time = now;
        l2data.id = n;
        l2data.pos2d.x = float.Parse(items[5]);
        l2data.pos2d.y = float.Parse(items[5]);
        ret.Add(l2data);

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
                        string[] items = line.Split(',');   // 長さ: 21

                        // for (int i = 0; i < items.Length; i++)
                        // {
                        //     Debug.Log(i + " 番目の中身: " + items[i]);
                        // }

                        if(items[0] != "HELLO LAY2 003"){
                            // long unix_time;
                            // long.TryParse(items[0], out unix_time);
                            // Debug.Log($"UNIX TIME: {unix_time}");
                            // long msec;
                            // long.TryParse(items[1], out msec);
                            // Debug.Log($"msec: {msec}");
                            // long unix_time_msec = unix_time * 1000 + msec;

                            
                            // long unix_time_msec = long.Parse(items[0]) * 1000 + long.Parse(items[1]);
                            // DateTime time = DateTimeOffset.FromUnixTimeMilliseconds(unix_time_msec).LocalDateTime;
                            // Debug.Log("【UNIX TIME MSEC】-> " + time);

                            // int n = int.Parse(items[2]);  // 検出された人数
                            // Debug.Log("【検出された人数】-> " + n);
                            
                            StringBuilder sb = new StringBuilder();
                            for (int i = 5; i < items.Length; i++)
                            {
                                if (i == items.Length-1) {
                                    sb.Append(items[i]);   // 末尾のカンマはいらない
                                }else{
                                    sb.Append(items[i]).Append(",");
                                }
                            }
                            // Debug.Log("【くっつけてみた】" + sb.ToString());

                            JObject json = JObject.Parse(sb.ToString());
                            // Debug.Log("【JSON】" + json);

                            // int uniqueID = int.Parse(json.Value<string>("uniqueID"));
                            float y = float.Parse(json.Value<string>("y")) / 1000;
                            float x = float.Parse(json.Value<string>("x")) / 1000;
                            long id = long.Parse(json.Value<string>("id"));

                            // Debug.Log("【uniqueID】" + uniqueID);
                            Debug.Log("【Y】" + y);
                            Debug.Log("【X】" + x);
                            Debug.Log("【id】" + id);

                            //foreach (var e in json)
                            //{
                            //    Debug.Log("【JSON書き出し】" + e);
                            //}
                        }
                        
                        //ParseL2Stringメソッドを使って
                        //List<L2Data> l2datalist = ParseL2String(line);
                        //lock (l2datadict)
                        //{
                        //    l2datadict.Clear();
                        //    foreach(L2Data l2data in l2datalist)
                        //    {
                        //        l2datadict.Add(l2data.id, l2data);     // 新規にデータを追加
                        //    }
                        //}    


                        //long unix_time_msec = long.Parse(items[0]) * 1000 + long.Parse(items[1]);               // unix time (msec)
                        //Debug.Log("【UNIX TIME MSEC】-> " + unix_time_msec);

                        //int n = int.Parse(items[2]);  // 検出された人数
                        //Debug.Log("【検出された人数】-> " + n);

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

    public void OnApplicationQuit()
    {
        running = false;
        clientReceiveThread.Join();
    }
}
