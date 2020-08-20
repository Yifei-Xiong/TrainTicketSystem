using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
namespace TTS_server_test
{
    /// <summary>
    /// 作为调试用的简易服务端
    /// </summary>
    class Program
    {
        static int MyPort = 1499;  //本机端口号
        IPAddress MyIPAdress = IPAddress.Parse("127.0.0.1");  //本机ip地址
        TcpListener tcpListener = null;  //监听
        string IPAndPort;  //本机ip与端口号
        private Thread Listenerthread;

        private delegate void ReadDataF(TcpClient tcpClient);
        private delegate void LoginF(string infor);
        private delegate void RegisterF(string infor);
        private delegate void OfflineF(string infor);
        private delegate void OnlineF(string infor);

        //查找可用端口
        public bool FindPort()
        {
            MyPort++;
            for (int i = 0; i < 51; i++)
            {
                //端口尝试
                try
                {
                    Console.WriteLine("正在尝试连接...第{0}次", i + 1);
                    tcpListener = new TcpListener(MyIPAdress, MyPort);
                    tcpListener.Start();
                    IPAndPort = MyIPAdress.ToString() + ":" + MyPort.ToString();
                    Console.WriteLine("连接成功！服务器的本地ip与端口号为  " + IPAndPort);
                    break;
                }
                catch
                {
                    MyPort++;
                }
                if (i == 50)
                {
                    Console.WriteLine("连接失败！请检查您的网络连接！");
                    return false;
                }
            }
            return true;
        }

        //线程方法
        public void ListenerthreadMethod()
        {
            TcpClient tcpClient = null;
            ReadDataF readDataF = new ReadDataF(readRevMsg);
            while (true)
            {
                try
                {
                    //同步阻塞
                    tcpClient = tcpListener.AcceptTcpClient();
                    //异步调用
                    readDataF.BeginInvoke(tcpClient, null, null);
                }
                catch { }
            }
        }

        //接收到信息后的操作
        public void readRevMsg(TcpClient tcpClient)
        {
            byte[] bytes = ReadFromTcpClient(tcpClient); //获取数据
            TTS_Core.DataPackage dataPackage = new TTS_Core.DataPackage(bytes);  //第一次解包
            string message = string.Empty;
            //数据包分类操作，第二次解包
            switch (dataPackage.MessageType)
            {
                case TTS_Core.MESSAGETYPE.K_QUERY_DATA_PACKAGE: //查询数据包类
                    TTS_Core.QueryDataPackage queryData = new TTS_Core.QueryDataPackage(bytes);
                    Console.WriteLine("Get the DataPackage of {0}!", queryData.QueryType.ToString());
                    break;
                case TTS_Core.MESSAGETYPE.K_LOGIN_DATA_PACKAGE:
                    TTS_Core.LoginDataPackage loginData = new TTS_Core.LoginDataPackage(bytes);
                    Console.WriteLine("Get the DataPackage of {0}!", loginData.MessageType.ToString());
                    break;
                case TTS_Core.MESSAGETYPE.K_REGISTER_DATA_PACKAGE:
                    TTS_Core.RegisterDataPackage registerData = new TTS_Core.RegisterDataPackage(bytes);
                    Console.WriteLine("Get the DataPackage of {0}!", registerData.MessageType.ToString());
                    break;
                default:
                    Console.WriteLine("Get the DataPackage of {0}!", dataPackage.MessageType.ToString());
                    return;
            }
        }

        //从TcpClient对象中读出未知长度的字节数组
        public byte[] ReadFromTcpClient(TcpClient tcpClient)
        {
            List<byte> data = new List<byte>();
            NetworkStream networkStream = null;
            //新建可储存缓冲区长度的字节数组
            byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
            int n = 0;
            try
            {
                networkStream = tcpClient.GetStream();
                if (networkStream.CanRead)
                {
                    do
                    {
                        n = networkStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
                        //若还未读到末尾
                        if (n == (int)tcpClient.ReceiveBufferSize) data.AddRange(bytes);
                        //读到结尾
                        else if (n != 0)
                        {
                            byte[] bytes1 = new byte[n];
                            for (int i = 0; i < n; i++)
                            {
                                bytes1[i] = bytes[i];
                            }
                            data.AddRange(bytes1);
                        }
                    } while (networkStream.DataAvailable);
                }
                bytes = data.ToArray();
            }
            catch
            {
                Console.WriteLine("读取数据失败！");
                bytes = null;
            }
            finally
            {
                if (networkStream != null) networkStream.Close();
                tcpClient.Close();
            }
            return bytes;
        }

        //主函数
        static void Main(string[] args)
        {
            Program p = new Program();
            if (!p.FindPort()) return;
            p.Listenerthread = new Thread(new ThreadStart(p.ListenerthreadMethod));
            p.Listenerthread.IsBackground = true;
            p.Listenerthread.Start();
            while (true) { }
        }
    }
}
