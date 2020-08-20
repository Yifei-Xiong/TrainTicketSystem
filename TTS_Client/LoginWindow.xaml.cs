using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace TTS_Client {
	/// <summary>
	/// LoginWindow.xaml 的交互逻辑
	/// </summary>
	public partial class LoginWindow : Window {

        //委托
        private delegate void ReadDataF(TcpClient tcpClient);

        //类定义
        public class StateObject
        {
            public TcpClient tcpClient = null;
            public NetworkStream netstream = null;
            public byte[] buffer;
        }

        //变量定义
        private Thread Listenerthread;  //线程
        TcpListener tcpListener = null;
        IPAddress myIPAddress = null;
        static int MyPort = 37529;


        //构造函数重载
        public LoginWindow() {
			InitializeComponent();
			myIPAddress = IPAddress.Parse("127.0.0.1");
			for (int i = 0; i <= 100; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 100) {
					MessageBox.Show("无法与服务器建立通信 (Error 01)");
					this.Close();
				}
			}
            Listenerthread = new Thread(new ThreadStart(ListenThreadMethod));
            Listenerthread.IsBackground = true;
            Listenerthread.Start();
        }

        /*public LoginWindow(string UserID, string IP) {
			InitializeComponent();
			textBox_id.Text = UserID;
			textBox_ip.Text = IP;
			myIPAddress = IPAddress.Parse("127.0.0.1");
			for (int i = 0; i <= 100; i++) {
				try {
					tcpListener = new TcpListener(myIPAddress, MyPort);
					tcpListener.Start();
					break;
				}
				catch {
					MyPort++; //已被使用,端口号加1
				}
				if (i == 100) {
					MessageBox.Show("无法与服务器建立通信 (Error 02)");
					this.Close();
				}
			}
            Listenerthread = new Thread(new ThreadStart(ListenThreadMethod));
            Listenerthread.IsBackground = true;
            Listenerthread.Start();
        }
        */

        //从TcpClient对象中读出未知长度的字节数组
        public byte[] ReadFromTcpClient(TcpClient tcpClient) {
			List<byte> data = new List<byte>();
			NetworkStream netStream = null;
			byte[] bytes = new byte[tcpClient.ReceiveBufferSize]; //字节数组保存接收到的数据
			int n = 0;
			try {
				netStream = tcpClient.GetStream();
				if (netStream.CanRead) {
					do { //文件大小未知
						n = netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
						if (n == (int)tcpClient.ReceiveBufferSize) {
							data.AddRange(bytes);
						} //如果bytes被读入数据填满
						else if (n != 0) {
							byte[] bytes1 = new byte[n];
							for (int i = 0; i < n; i++) {
								bytes1[i] = bytes[i];
							}
							data.AddRange(bytes1);
						} //读入的字节数不为0
					} while (netStream.DataAvailable); //是否还有数据
				} //判断数据是否可读
				bytes = data.ToArray();
			}
			catch {
				MessageBox.Show("从Tcp对象中读入数据 (Error 03)");
				bytes = null;
			}
			finally {
				if (netStream != null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
			return bytes;
		}

		
		//侦听线程执行的方法
		private void ListenThreadMethod() {
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

		
        //点击注册按钮
		private void button_register_Click(object sender, RoutedEventArgs e) {
            //向服务器发送异步请求
            TcpClient tcpClient;
            StateObject stateObject;
            TTS_Core.RegisterDataPackage registerData;
            tcpClient = new TcpClient();
            stateObject = new StateObject();
            stateObject.tcpClient = tcpClient;
            registerData = new TTS_Core.RegisterDataPackage(myIPAddress + MyPort.ToString(), textBox_ip.Text, textBox_id.Text, sha256(passwordBox.Password));  
            stateObject.buffer = registerData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
            tcpClient.BeginConnect(myIPAddress, int.Parse(textBox_ip.Text.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject); //异步连接
        }


        //点击登录按钮
		private void button_login_Click(object sender, RoutedEventArgs e) {
            //向服务器发送异步请求
            TcpClient tcpClient;
            StateObject stateObject;
            TTS_Core.LoginDataPackage loginData;
            tcpClient = new TcpClient();
            stateObject = new StateObject();
            stateObject.tcpClient = tcpClient;
            loginData = new TTS_Core.LoginDataPackage(myIPAddress + MyPort.ToString(), textBox_ip.Text, textBox_id.Text, sha256(passwordBox.Password));
            stateObject.buffer = loginData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
            tcpClient.BeginConnect(myIPAddress, int.Parse(textBox_ip.Text.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject); //异步连接

            ClientWindow clientWindow = new ClientWindow(textBox_id.Text, tcpListener, MyPort, textBox_ip.Text.Split(':')[1]);
			clientWindow.Show();
			Close();
		}


        //点击关于按钮
		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}


        //加密数据
		public string sha256(string data) {
			byte[] bytes = Encoding.UTF8.GetBytes(data);
			byte[] hash = SHA256Managed.Create().ComputeHash(bytes);
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < hash.Length; i++) {
				builder.Append(hash[i].ToString("X2"));
			}
			return builder.ToString();
		}


        //接收到信息后的操作
        public void readRevMsg(TcpClient tcpClient)
        {
            byte[] bytes = ReadFromTcpClient(tcpClient); //获取数据
            TTS_Core.QueryDataPackage queryData = new TTS_Core.QueryDataPackage(bytes);
            string message = string.Empty;
            //数据包分类操作
            switch (queryData.MessageType)
            {
                
                default:
                    Console.WriteLine("聊天数据包读取失败");
                    return;
            }
        }


        //回调函数
        private void SentCallBackF(IAsyncResult ar)
        {
            StateObject stateObject = (StateObject)ar.AsyncState;
            TcpClient tcpClient = stateObject.tcpClient; //得到下载使用的类对象
            NetworkStream netStream = null; //下载使用的流对象
            try
            {
                tcpClient.EndConnect(ar); //结束和下载服务器的连接，如下载错误将产生异常
                netStream = tcpClient.GetStream();
                if (netStream.CanWrite)
                {
                    netStream.Write(stateObject.buffer, 0, stateObject.buffer.Length); //传入要发送的内容
                }
                else
                {
                    MessageBox.Show("暂时无法与服务端通讯");
                }
            }
            catch
            {
                MessageBox.Show("暂时无法与服务端通讯");
            }
            finally
            {
                if (netStream != null)
                {
                    netStream.Close();
                }
                tcpClient.Close();
            }
        } //不在主线程执行

    }
}
