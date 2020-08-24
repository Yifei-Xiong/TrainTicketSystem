using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Shapes;

namespace TTS_Client {
	/// <summary>
	/// ClientWindow.xaml 的交互逻辑
	/// </summary>
	public partial class ClientWindow : Window {

        //委托
        private delegate void ReadDataF(TcpClient tcpClient); //代表无返回值 Tcpclient参数方法

        //结构体定义
        public struct StationInfo
        {
            public int StationNumber { get; set; } //车站编号
            public string StationName { get; set; } //车站名称
            public int Line { get; set; } //线路编号
            public string LineName { get; set; } //线路名称
        } //单个站点的相关信息

        public struct TicketInfo
        {
            public int TicketNumber { get; set; } //订单序号
            public double TicketPrice { get; set; } //车票价格
            public int TicketLine { get; set; } //所属路线序号
            public string LineName { get; set; } //所属线路名称
            public int TrainID { get; set; } //车次
            public string BuyTime { get; set; } //购买时间

            public int EnterStationNumber { get; set; } //出发站点序号
            public string EnterStationName { get; set; } //出发站点名称
            public string EnterStationTime { get; set; }
            public string EnterStationTimeIn { get; set; }
            public string EnterStationTimeOut { get; set; }

            public int LeaveStationNumber { get; set; } //到达站点序号
            public string LeaveStationName { get; set; } //到达站点名称
            public string LeaveStationTime { get; set; }
            public string LeaveStationTimeIn { get; set; }
            public string LeaveStationTimeOut { get; set; }

			public string UserID { get; set; }
			public int _state { get; set; } //1为已支付订单，2为申请取消的订单，3为失效的订单
			public string State { get; set; }
        } //单个车票的相关信息

        public struct BuyTicket
        {
            public int EnterStationNumber { get; set; }
            public string EnterStationName { get; set; }
            public string EnterStationTime { get; set; }
            public string EnterStationTimeIn { get; set; }
            public string EnterStationTimeOut { get; set; }

            public int LeaveStationNumber { get; set; }
            public string LeaveStationName { get; set; }
            public string LeaveStationTime { get; set; }
            public string LeaveStationTimeIn { get; set; }
            public string LeaveStationTimeOut { get; set; }

            public double TicketPrice { get; set; }
            public int TicketLine { get; set; }
            public string LineName { get; set; } //所属线路名称
            public int TrainID { get; set; } //车次
            public int BuyNumber { get; set; } //车票购买数量
			public int TicketRemain { get; set; } //车票剩余数量
			public string TimeTake { get; set; } //花费时间
		} //单个车票购买记录的相关信息

        public struct TicketQueryInfo
        {
            public int EnterStationNumber { get; set; }
            public string EnterStationName { get; set; }
            public int LeaveStationNumber { get; set; }
            public string LeaveStationName { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public int Line { get; set; } //若在一条线路上，则为线路编号
            public string LineName { get; set; } //若在一条线路上，则为线路名称
        }


        //集合定义
        public class AllStationInfo : ObservableCollection<StationInfo> { } //定义集合
        public class AllTicketInfo : ObservableCollection<TicketInfo> { } //定义集合
        public class AllBuyTicket : ObservableCollection<BuyTicket> { } //定义集合
		public AllStationInfo allStationInfo;
		public AllTicketInfo allTicketInfo;
		public AllTicketInfo searchTicketInfo;
		public AllBuyTicket allBuyTicket;


        //类定义
        public class StateObject
        {
            public TcpClient tcpClient = null;
            public NetworkStream netstream = null;
            public byte[] buffer;
        }


        //属性变量定义
        string UserID; //用户ID
        int MyPort; //本程序侦听准备使用的端口号
        int LoginPort; //登录端口，也即是服务器的端口
        IPAddress myIPAddress = null; //本程序侦听使用的IP地址
        TcpListener tcpListener = null; //接收信息的侦听类对象,检查是否有信息
        string IPAndPort; //记录本地IP和端口号
		double RemainMoney; //剩余金额
		string UserName { get; set; }
		string Phone { get; set; }

        TicketQueryInfo ticketQueryInfo;
        private Thread ListenerThread; //接收信息的侦听线程类变量
        public DateTime QueryStartTime { get; set; }
        public DateTime QueryEndTime { get; set; }


        //构造函数重载
        public ClientWindow(string ID, TcpListener tcpListener, int MyPort, string LoginPort, bool IsAdmin, string info)
        {
            InitializeComponent();
			allStationInfo = new AllStationInfo { };
			allTicketInfo = new AllTicketInfo { };
			allBuyTicket = new AllBuyTicket { };

			BuyTicketListView.ItemsSource = allBuyTicket;
            TicketListView.ItemsSource = allTicketInfo;
			TicketListView.Items.SortDescriptions.Add(new SortDescription("TicketNumber", ListSortDirection.Descending));
			ticketQueryInfo = new TicketQueryInfo();
            ticketQueryInfo.StartTime = DateTime.Now;
            ticketQueryInfo.EndTime = DateTime.Now.AddDays(10);
            textBlock_Copy12.Text = ticketQueryInfo.StartTime.ToString() + " - " + ticketQueryInfo.EndTime.ToString();
			textBlock_Copy13.Text = ID;

			myIPAddress = IPAddress.Parse("127.0.0.1");
            IPAndPort = myIPAddress.ToString() + ":" + MyPort.ToString();
            //ListenerThread = new Thread(new ThreadStart(ListenerthreadMethod));
            //ListenerThread.IsBackground = true; //主线程结束后，该线程自动结束
            //ListenerThread.Start(); //启动线程
            UserID = ID; //设置用户名
            this.MyPort = MyPort; //本机Listener监听的端口
            this.tcpListener = tcpListener;
            this.LoginPort = int.Parse(LoginPort); //服务器的端口

			string[] infostr = info.Split('\n');
			this.Phone = infostr[0];
			this.UserName = infostr[1];
			this.RemainMoney = double.Parse(infostr[2]);
			textBlock_Copy18.Text = infostr[0];
			textBlock_Copy22.Text = infostr[1];
			textBlock_Copy14.Text = infostr[2];
			textBlock_Copy24.Text = MyPort.ToString();
			textBlock_Copy26.Text = LoginPort;

			ProgramItem_Data();

			if (IsAdmin == true) {
				textBlock_Copy16.Text = "管理员";
				button7.IsEnabled = false;
				TicketItem.IsEnabled = false;
				tabControl.SelectedItem = UserItem;
				this.Title = this.Title + " (管理员)";
			} //是管理员
			else {
				button12.Visibility = System.Windows.Visibility.Hidden;
				button13.Visibility = System.Windows.Visibility.Hidden;
				SystemItem.Visibility = System.Windows.Visibility.Hidden;
			} //是普通用户

			Refresh_Data(0); //获取用户信息


		}//构造函数，将登录页面的某些数据传过来

        public ClientWindow() {
			InitializeComponent();
			BuyTicketListView.ItemsSource = allBuyTicket;
			TicketListView.ItemsSource = allTicketInfo;
			ticketQueryInfo = new TicketQueryInfo();
			ticketQueryInfo.StartTime = DateTime.Now;
			ticketQueryInfo.EndTime = DateTime.Now.AddHours(12);
			textBlock_Copy12.Text = ticketQueryInfo.StartTime.ToString() + " - " + ticketQueryInfo.EndTime.ToString();

			ListenerThread = new Thread(new ThreadStart(ListenerthreadMethod));
			ListenerThread.IsBackground = true; //主线程结束后，该线程自动结束
			ListenerThread.Start(); //启动线程
		}

        
        //点击查询/购买按钮
		private void button2_Click(object sender, RoutedEventArgs e) {
			if (ticketQueryInfo.EnterStationNumber == 0 || ticketQueryInfo.LeaveStationNumber == 0
				|| ticketQueryInfo.StartTime.Year <= 1 || ticketQueryInfo.EndTime.Year <= 1) {
				MessageBox.Show("查询条件不完整，请重新输入！");
				return;
			} //查询未完成


			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			string Msg = null;
			string ExtraMsg = ticketQueryInfo.EnterStationNumber.ToString() + "\n" + ticketQueryInfo.LeaveStationNumber.ToString();
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.QueryDataPackage info = new TTS_Core.QueryDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.QueryDataPackage(bytes);
				Msg = package.ExtraMsg;

			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}

			if (Msg == null) {
				return;
            }
			if (Msg.Split('\n')[0]=="0") {
				if (Msg.Split('\n')[1] == "1") {
					//无需换乘，一种路线
					TicketQueryInfo subinfo1 = new ClientWindow.TicketQueryInfo();
					subinfo1.EnterStationNumber = this.ticketQueryInfo.EnterStationNumber;
					subinfo1.Line = int.Parse(Msg.Split('\n')[2]);
					subinfo1.LeaveStationNumber = this.ticketQueryInfo.LeaveStationNumber;
					subinfo1.StartTime = this.ticketQueryInfo.StartTime;
					subinfo1.EndTime = this.ticketQueryInfo.EndTime;
					string info1Msg = subinfo1.EnterStationNumber.ToString() + "\n" + subinfo1.Line.ToString()
						+ "\n" + subinfo1.LeaveStationNumber.ToString() + "\n" + ticketQueryInfo.StartTime.ToString() + "\n" + ticketQueryInfo.EndTime.ToString();
					TcpClient tcpClient1 = null;
					NetworkStream networkStream1 = null;
					try {
						tcpClient1 = new TcpClient();
						tcpClient1.Connect(myIPAddress, LoginPort); //建立与服务器的连接
						networkStream1 = tcpClient1.GetStream();
						if (networkStream1.CanWrite) {
							TTS_Core.QueryDataPackage data = new TTS_Core.QueryDataPackage(UserID, myIPAddress + ":" +
								MyPort.ToString(), "server", TTS_Core.QUERYTYPE.K_TICKETINFO_QUERY, info1Msg);
							byte[] sendBytes = data.DataPackageToBytes(); //注册数据包转化为字节数组
							networkStream1.Write(sendBytes, 0, sendBytes.Length);
						}
						var newClient = tcpListener.AcceptTcpClient();
						var bytes = ReadFromTcpClient(newClient); //获取数据
						var package = new TTS_Core.QueryDataPackage(bytes);
						subinfo1.EnterStationName = package.ExtraMsg.Split('\r')[0].Split('\n')[0];
						subinfo1.LineName = package.ExtraMsg.Split('\r')[0].Split('\n')[1];
						subinfo1.LeaveStationName = package.ExtraMsg.Split('\r')[0].Split('\n')[2];
						info1Msg = package.ExtraMsg;
					}
					catch {
						MessageBox.Show("指定时段内无可选车次！");
						return;
					}
					finally {
						if (networkStream != null) {
							networkStream1.Close();
						}
						tcpClient1.Close();
					}

					BuyTicketWindow buy = new BuyTicketWindow(subinfo1, info1Msg);
					buy.ShowDialog();
					if (buy.selectTicket.TrainID != 0) {
						allBuyTicket.Add(buy.selectTicket);
					}
				}
				else {
					//无需换乘，多种路线
					LineSelect lineSelect = new LineSelect(ticketQueryInfo, Msg, allBuyTicket, 1, UserID, myIPAddress, tcpListener, MyPort, LoginPort);
					lineSelect.ShowDialog();


				}
			}
			else if (Msg.Split('\n')[0] == "1") {
				if (Msg.IndexOf("\\") == -1) {
					//一次换乘，多种路线
					LineSelect lineSelect = new LineSelect(ticketQueryInfo, Msg, allBuyTicket, 2, UserID, myIPAddress, tcpListener, MyPort, LoginPort);
					lineSelect.ShowDialog();
				}
				else {
					//一次换乘，两次换乘
					LineSelect lineSelect = new LineSelect(ticketQueryInfo, Msg, allBuyTicket, 3, UserID, myIPAddress, tcpListener, MyPort, LoginPort);
					lineSelect.ShowDialog();
				}
            }
			else if (Msg.Split('\n')[0] == "2") {
				//两次换乘
				LineSelect lineSelect = new LineSelect(ticketQueryInfo, Msg, allBuyTicket, 4, UserID, myIPAddress, tcpListener, MyPort, LoginPort);
				lineSelect.ShowDialog();
			}

		}


        //点击关于按钮
		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}


        //点击选择到达地点按钮
        private void button1_Click(object sender, RoutedEventArgs e) {
            //向服务器发送异步请求
            TcpClient tcpClient;
            StateObject stateObject;
            TTS_Core.QueryDataPackage queryData;
            tcpClient = new TcpClient();
            stateObject = new StateObject();
            stateObject.tcpClient = tcpClient;
            queryData = new TTS_Core.QueryDataPackage(UserID, IPAndPort, "Server", TTS_Core.QUERYTYPE.K_ARRIVAL_STATION, "");  //到达站点查询
            stateObject.buffer = queryData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
            tcpClient.BeginConnect(myIPAddress, LoginPort, new AsyncCallback(SentCallBackF), stateObject); //异步连接

			//弹出地点信息窗口
			allStationInfo.Clear();  //清空信息
			var newClient = tcpListener.AcceptTcpClient();
			var bytes = ReadFromTcpClient(newClient); //获取数据
			var package = new TTS_Core.QueryDataPackage(bytes);
			string[] ExtraMsg = package.Sender.Split('\r');
			int Count = ExtraMsg.Length;
			StationInfo stationInfo = new StationInfo();
			for (int i = 0; i < Count-1; i++) {
				stationInfo.StationNumber = int.Parse(ExtraMsg[i].Split('\n')[0]);
				stationInfo.StationName = ExtraMsg[i].Split('\n')[1];
				stationInfo.LineName = ExtraMsg[i].Split('\n')[2];
				allStationInfo.Add(stationInfo);
			}
			LocationSelect locationSelect = new LocationSelect("请选择出发地点", allStationInfo);
			locationSelect.ShowDialog();
			if (locationSelect.StationName == ticketQueryInfo.EnterStationName) {
				MessageBox.Show("到达地点不能和到达地点相同！");
				return;
			}
			if (locationSelect.StationName != null) {
				textBlock_Copy7.Text = locationSelect.StationName + " (" + locationSelect.StationNumber.ToString() + ")";
				ticketQueryInfo.LeaveStationNumber = locationSelect.StationNumber;
				ticketQueryInfo.LeaveStationName = locationSelect.StationName;
			}
		} //选择到达地点


        //出发站点以及到达站点选择的按钮点击事件实际上是相同的
		private void button_Click(object sender, RoutedEventArgs e) {
            //向服务器发送异步请求
            TcpClient tcpClient;
            StateObject stateObject;
            TTS_Core.QueryDataPackage queryData;
            tcpClient = new TcpClient();
            stateObject = new StateObject();
            stateObject.tcpClient = tcpClient;
            queryData = new TTS_Core.QueryDataPackage(UserID, IPAndPort, "Server",TTS_Core.QUERYTYPE.K_DEPARTURE_STATION, "");  //出发站点查询
            stateObject.buffer = queryData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
            tcpClient.BeginConnect(myIPAddress, LoginPort, new AsyncCallback(SentCallBackF), stateObject); //异步连接

            //弹出地点信息窗口
            allStationInfo.Clear();  //清空信息
			var newClient = tcpListener.AcceptTcpClient();
			var bytes = ReadFromTcpClient(newClient); //获取数据
			var package = new TTS_Core.QueryDataPackage(bytes);
			string[] ExtraMsg = package.Sender.Split('\r');
			int Count = ExtraMsg.Length;
			StationInfo stationInfo = new StationInfo();
			for (int i=0; i<Count-1;i++) {
				string debuga = ExtraMsg[i];
				string[] debugc = ExtraMsg[i].Split('\n');
				string debugb = ExtraMsg[i].Split('\n')[0];
				stationInfo.StationNumber = int.Parse(ExtraMsg[i].Split('\n')[0]);
				stationInfo.StationName = ExtraMsg[i].Split('\n')[1];
				stationInfo.LineName = ExtraMsg[i].Split('\n')[2];
				allStationInfo.Add(stationInfo);
			}
			LocationSelect locationSelect = new LocationSelect("请选择出发地点", allStationInfo);
			locationSelect.ShowDialog();
			if (locationSelect.StationName == ticketQueryInfo.LeaveStationName) {
				MessageBox.Show("出发地点不能和到达地点相同！");
				return;
			}
			if (locationSelect.StationName != null) {
				textBlock_Copy2.Text = locationSelect.StationName + " (" + locationSelect.StationNumber.ToString() + ")";
				ticketQueryInfo.EnterStationNumber = locationSelect.StationNumber;
				ticketQueryInfo.EnterStationName = locationSelect.StationName;
			}
		} //选择出发地点


        //点击选择出发时间按钮
        private void button3_Click(object sender, RoutedEventArgs e) {
			TimeSelect timeSelect = new TimeSelect(ticketQueryInfo.StartTime, ticketQueryInfo.EndTime);
			timeSelect.ShowDialog();
			if (timeSelect.StartTime.Year != 1) {
				ticketQueryInfo.StartTime = timeSelect.QueryStartTime;
			}
			if (timeSelect.EndTime.Year != 1) {
				ticketQueryInfo.EndTime = timeSelect.QueryEndTime;
			}
			textBlock_Copy12.Text = ticketQueryInfo.StartTime.ToString() + " - " + ticketQueryInfo.EndTime.ToString();
		} //选择出发时间


		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox_tic1.Clear();
			textBox_tic2.Clear();
			textBox_tic3.Clear();
			textBox_tic4.Clear();
			textBox_tic5.Clear();
			textBox_tic6.Clear();
			textBox_tic7.Clear();
			textBox_tic8.Clear();
			textBox_tic9.Clear();
			TicketListView.ItemsSource = allTicketInfo;
		} //清空

		private void button2_Copy_Click_1(object sender, RoutedEventArgs e) {
			searchTicketInfo = new AllTicketInfo();
			for (int i = 0; i < allTicketInfo.Count; i++) {
				searchTicketInfo.Add(allTicketInfo[i]);
			} //Copy

			if (precision.IsChecked == true) {
				if (textBox_tic1.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TicketNumber.ToString() != textBox_tic1.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TicketNumber
				if (textBox_tic2.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TrainID.ToString() != textBox_tic2.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TrainID
				if (textBox_tic3.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].EnterStationName.ToString() != textBox_tic3.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //EnterStationName
				if (textBox_tic4.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].EnterStationTime.ToString() != textBox_tic4.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //EnterStationTime
				if (textBox_tic5.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LeaveStationName.ToString() != textBox_tic5.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LeaveStationName
				if (textBox_tic6.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LeaveStationTimeIn.ToString() != textBox_tic6.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LeaveStationTimeIn
				if (textBox_tic7.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TicketPrice.ToString() != textBox_tic7.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TicketPrice
				if (textBox_tic8.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LineName.ToString() != textBox_tic8.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LineName
				if (textBox_tic9.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].BuyTime.ToString() != textBox_tic9.Text) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //BuyTime
			} //精确搜索

			else {
				if (textBox_tic1.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TicketNumber.ToString().IndexOf(textBox_tic1.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TicketNumber
				if (textBox_tic2.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TrainID.ToString().IndexOf(textBox_tic2.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TrainID
				if (textBox_tic3.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].EnterStationName.ToString().IndexOf(textBox_tic3.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //EnterStationName
				if (textBox_tic4.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].EnterStationTime.ToString().IndexOf(textBox_tic4.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //EnterStationTime
				if (textBox_tic5.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LeaveStationName.ToString().IndexOf(textBox_tic5.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LeaveStationName
				if (textBox_tic6.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LeaveStationTimeIn.ToString().IndexOf(textBox_tic6.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LeaveStationTimeIn
				if (textBox_tic7.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].TicketPrice.ToString().IndexOf(textBox_tic7.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //TicketPrice
				if (textBox_tic8.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].LineName.ToString().IndexOf(textBox_tic8.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //LineName
				if (textBox_tic9.Text != string.Empty) {
					for (int i = 0; i < searchTicketInfo.Count; i++) {
						if (searchTicketInfo[i].BuyTime.ToString().IndexOf(textBox_tic9.Text) == -1) {
							searchTicketInfo.Remove(searchTicketInfo[i]);
							i--;
						}
					}
				} //BuyTime
			} //模糊搜索

			TicketListView.ItemsSource = searchTicketInfo;
		} //筛选

		private void button8_Click(object sender, RoutedEventArgs e) {
			this.LoadDefaultTestData();
		}

		private void button8_Copy_Click(object sender, RoutedEventArgs e) {
			this.ClearData();
		}

		private void LoadDefaultTestData() {

			StationInfo stationInfo = new StationInfo();

			stationInfo.Line = 3;
			stationInfo.LineName = "三号线";
			stationInfo.StationName = "五山";
			stationInfo.StationNumber = 302;
			allStationInfo.Add(stationInfo);

			stationInfo.Line = 3;
			stationInfo.LineName = "三号线";
			stationInfo.StationName = "天河客运站";
			stationInfo.StationNumber = 301;
			allStationInfo.Add(stationInfo);

			stationInfo.Line = 1;
			stationInfo.LineName = "一号线";
			stationInfo.StationName = "体育西路站";
			stationInfo.StationNumber = 106;
			allStationInfo.Add(stationInfo);

		} //载入测试数据(仅用于调试)

		private void ClearData() {

		} //清空已有数据(仅用于调试)



        //解拆服务器发回的数据包
		public void readRevMsg(TcpClient tcpClient) {
			byte[] bytes = ReadFromTcpClient(tcpClient); //获取数据
			TTS_Core.QueryDataPackage queryData = new TTS_Core.QueryDataPackage(bytes);
			string message = string.Empty;
            //数据包分类操作
			switch (queryData.MessageType) {
				case TTS_Core.MESSAGETYPE.K_QUERY_DATA_PACKAGE: //查询数据包类
                    //查询数据包分类操作
                    switch (queryData.QueryType)
                    {
                        case TTS_Core.QUERYTYPE.K_DEPARTURE_STATION:  //出发站点查询
                        case TTS_Core.QUERYTYPE.K_ARRIVAL_STATION:  //到达站点查询
                            //填充站点信息查询list view

                            break;
                    }
					break;
				default:
					MessageBox.Show("聊天数据包读取失败");
					return;
			}
		} //被异步调用的方法


		private void ListenerthreadMethod() {
			TcpClient tcpClient = null; //服务器和客户机连接的 TcpClient类对象
			ReadDataF readDataF = new ReadDataF(readRevMsg); //方法 readRevMsg
			while (true) {
				try {
					tcpClient = tcpListener.AcceptTcpClient(); //阻塞等待客户端的连接
					readDataF.BeginInvoke(tcpClient, null, null); //异步调用方法readRevMsg
				}
				catch {

				} //即使发生错误，如果tcpClient不为null，下次循环将引用其他对象
			}
		} //侦听线程执行的方法


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
				MessageBox.Show("读数据失败");
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


        //回调函数
		private void SentCallBackF(IAsyncResult ar) {
			StateObject stateObject = (StateObject)ar.AsyncState;
			TcpClient tcpClient = stateObject.tcpClient; //得到下载使用的类对象
			NetworkStream netStream = null; //下载使用的流对象
			try {
				tcpClient.EndConnect(ar); //结束和下载服务器的连接，如下载错误将产生异常
				netStream = tcpClient.GetStream();
				if (netStream.CanWrite) {
					netStream.Write(stateObject.buffer, 0, stateObject.buffer.Length); //传入要发送的内容
				}
				else {
					MessageBox.Show("暂时无法与服务端通讯");
				}
			}
			catch {
				MessageBox.Show("暂时无法与服务端通讯");
			}
			finally {
				if (netStream != null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
		} //不在主线程执行


        //选项卡切换函数
        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			if (e.Source is TabControl) {
				//切换到订单查询选项
				if (OrderItem.IsSelected) {
					OrderItem_Selected();
				}
				//切换到车票查询/购买选项
				else if (TicketItem.IsSelected) {
					//
				}
				//切换到用户信息选项卡
				else if (UserItem.IsSelected) {
					UserItem_Selected();
				}
			}
        }

        //切换到订单查询选项事件
        public void OrderItem_Selected()
        {
            //向服务器发送查询请求
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			string ExtraMsg = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.QueryDataPackage data = new TTS_Core.QueryDataPackage(UserID, IPAndPort, "Server", TTS_Core.QUERYTYPE.K_USER_ORDER, "");
					byte[] sendBytes = data.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.QueryDataPackage(bytes);
				ExtraMsg = package.ExtraMsg;
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
			if (ExtraMsg==null) {
				MessageBox.Show("查询订单失败!");
			} else {
				string[] split = ExtraMsg.Split('\r');
				allTicketInfo.Clear();
				TicketInfo ticket = new TicketInfo();
				for (int i=0; i<split.Length-1; i++) {
					string[] substr = split[i].Split('\n');
					ticket.TicketNumber = int.Parse(substr[0]);
					ticket.EnterStationNumber = int.Parse(substr[1]);
					ticket.EnterStationName = substr[2];
					ticket.LeaveStationNumber = int.Parse(substr[3]);
					ticket.LeaveStationName = substr[4];
					ticket.EnterStationTime = substr[5];
					ticket.LeaveStationTime = substr[6];
					ticket.LineName = substr[7];
					ticket.TicketLine = int.Parse(substr[8]);
					ticket.TrainID = int.Parse(substr[9]);
					ticket.UserID = substr[10];
					ticket.BuyTime = substr[11];
					ticket.TicketPrice = double.Parse(substr[12]);
					ticket._state = int.Parse(substr[13]);
					if (ticket._state == 1) {
						ticket.State = "已支付";
					}
					else if (ticket._state == 2) {
						ticket.State = "已申请取消";
					}
					else if (ticket._state == 3) {
						ticket.State = "已取消";
					}
					allTicketInfo.Add(ticket);
				}

			}

		}

        //切换到用户信息选项卡事件
        void UserItem_Selected()
        {
            //
        }


        //程序信息选项卡信息载入
        void ProgramItem_Data()
        {

        }

		private void button5_Click(object sender, RoutedEventArgs e) {
			if (BuyTicketListView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择需要删除的车票");
				return;
			}
			BuyTicket[] buyTicket = new BuyTicket[BuyTicketListView.SelectedItems.Count];
			for (int i = 0; i < buyTicket.Length; i++) {
				buyTicket[i] = (BuyTicket)BuyTicketListView.SelectedItems[i];
			}
			for (int i = 0; i < buyTicket.Length; i++) {
				allBuyTicket.Remove(buyTicket[i]);
			}
		} //删除所选车票

		private void button4_Click(object sender, RoutedEventArgs e) {
			if (allBuyTicket.Count == 0) {
				MessageBox.Show("您还未添加车票");
			}
			if (BuyTicketListView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择需要购买的车票");
				return;
			}
			BuyTicket[] buyTickets = new BuyTicket[BuyTicketListView.SelectedItems.Count];
			for (int i = 0; i < buyTickets.Length; i++) {
				buyTickets[i] = (BuyTicket)BuyTicketListView.SelectedItems[i];
			}
			SendBuyTicketToServer(buyTickets);
		} //购买选中的车票

		private void button6_Click(object sender, RoutedEventArgs e) {
			if (allBuyTicket.Count == 0) {
				MessageBox.Show("您还未添加车票");
				return;
			}
			BuyTicket[] buyTickets = new BuyTicket[allBuyTicket.Count];
			for (int i = 0; i < buyTickets.Length; i++) {
				buyTickets[i] = allBuyTicket[i];
			}
			SendBuyTicketToServer(buyTickets);
		} //添加购买的所有车票

		private void SendBuyTicketToServer (BuyTicket[] buyTickets) {
			double totalCost = 0;
			for (int i = 0; i < buyTickets.Length; i++) {
				totalCost += buyTickets[i].TicketPrice;
			}
			if (totalCost > RemainMoney) {
				MessageBox.Show("余额不足，总共需要" + totalCost.ToString() + "元，当前用于余额为" + 
					RemainMoney.ToString() + "元，还需" + (totalCost - RemainMoney).ToString() + "元。");
				return;
			}

			string ExtraMsg = "";
			for (int i = 0; i < buyTickets.Length; i++) {
				//TrainID, EnterID, LeaveID, UserID, BuyNumber
				ExtraMsg = ExtraMsg + buyTickets[i].TrainID + "\n" + buyTickets[i].EnterStationName + "\n" +
					buyTickets[i].LeaveStationName.ToString() + "\n" + UserID + "\n" + buyTickets[i].BuyNumber.ToString() + "\r";
			}

			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.QueryDataPackage data = new TTS_Core.QueryDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", TTS_Core.QUERYTYPE.K_SUBMIT_BUY, ExtraMsg);
					byte[] sendBytes = data.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.QueryDataPackage(bytes);
				MessageBox.Show(package.ExtraMsg); //可知购买成功或失败
				if (package.ExtraMsg == "购买成功！") {
					for (int i = 0; i < buyTickets.Length; i++) {
						allBuyTicket.Remove(buyTickets[i]);
					}
				}
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		}

		private void Button_Click_1(object sender, RoutedEventArgs e) {
			ChangeUserInfo changeUserInfo = new ChangeUserInfo("更改手机号码", "请输入新的手机号码：");
			changeUserInfo.ShowDialog();
			if (changeUserInfo.value == string.Empty) {
				return;
			}
			if (System.Text.RegularExpressions.Regex.IsMatch(changeUserInfo.value, @"^1[3456789]\d{9}$") == false) {
				MessageBox.Show("手机号无法通过正则表达式验证！");
				return;
			}

			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.InfoChangeDataPackage info = new TTS_Core.InfoChangeDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", 2, changeUserInfo.value);
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				button11_Click(sender, e);
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		} //更改手机号码

		private void Button_Click_2(object sender, RoutedEventArgs e) {
			ChangeUserInfo changeUserInfo = new ChangeUserInfo("更改用户昵称", "请输入新的用户昵称：");
			changeUserInfo.ShowDialog();
			if (changeUserInfo.value == string.Empty) {
				return;
			}
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.InfoChangeDataPackage info = new TTS_Core.InfoChangeDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", 1, changeUserInfo.value);
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				button11_Click(sender, e);
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		} //更改用户昵称

		private void button11_Click(object sender, RoutedEventArgs e) {
			Refresh_Data(1); //为1则包含弹窗提示
		} //刷新用户信息

		private void Refresh_Data(int show) {
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.InfoChangeDataPackage info = new TTS_Core.InfoChangeDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", 3, "");
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.DataPackage(bytes);
				string message = package.Sender;
				if (show==1) {
					MessageBox.Show(message);
				}
				string[] infostr = package.Receiver.Split('\n');
				this.Phone = infostr[0];
				this.UserName = infostr[1];
				this.RemainMoney = double.Parse(infostr[2]);
				textBlock_Copy18.Text = infostr[0];
				textBlock_Copy22.Text = infostr[1];
				textBlock_Copy14.Text = infostr[2];
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		} //刷新用户信息

		private void button9_Click(object sender, RoutedEventArgs e) {
			double addvalue;
			double currentvalue = RemainMoney;
			bool canTurnPortToInt = double.TryParse(textBox.Text, out addvalue);
			if (canTurnPortToInt == false || addvalue > 100000 || addvalue <= 0) {
				MessageBox.Show("请输入正确的充值金额");
				return;
			}

			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.InfoChangeDataPackage info = new TTS_Core.InfoChangeDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", 4, addvalue.ToString());
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.DataPackage(bytes);
				string message = package.Sender;
				MessageBox.Show(message);
				string[] infostr = package.Receiver.Split('\n');
				this.Phone = infostr[0];
				this.UserName = infostr[1];
				this.RemainMoney = double.Parse(infostr[2]);
				textBlock_Copy18.Text = infostr[0];
				textBlock_Copy22.Text = infostr[1];
				textBlock_Copy14.Text = infostr[2];
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		} //充值

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
			ManagerWindow_user activity = new ManagerWindow_user(UserID, myIPAddress, LoginPort, tcpListener, MyPort);
			activity.ShowDialog();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
			var activity = new ManagerWindow_line(UserID, myIPAddress, LoginPort, tcpListener, MyPort);
			activity.ShowDialog();
        }

		private void button7_Click(object sender, RoutedEventArgs e) {
			//申请取消所选订单
			if (TicketListView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择需要申请取消的订单");
				return;
			}
			TicketInfo[] infos = new TicketInfo[TicketListView.SelectedItems.Count];
			string ExtraMsg = "2" + "\r";
			for (int i = 0; i < infos.Length; i++) {
				infos[i] = (TicketInfo)TicketListView.SelectedItems[i];
				if (infos[i]._state!=1) {
					MessageBox.Show("你已经申请过取消该订单了！");
					return;
				}
				ExtraMsg = ExtraMsg + infos[i].TicketNumber.ToString() + "\n";
			}
			TicketStateChange(ExtraMsg);
		} //申请取消所选订单

		private void button13_Click(object sender, RoutedEventArgs e) {
			//将该订单置于失效状态
			if (TicketListView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择需要置于失效的订单");
				return;
			}
			TicketInfo[] infos = new TicketInfo[TicketListView.SelectedItems.Count];
			string ExtraMsg = "3" + "\r";
			for (int i = 0; i < infos.Length; i++) {
				infos[i] = (TicketInfo)TicketListView.SelectedItems[i];
				if (infos[i]._state == 3) {
					MessageBox.Show("只有未失效的订单才可以置于失效！");
					return;
				}
				ExtraMsg = ExtraMsg + infos[i].TicketNumber.ToString() + "\n";
			}
			TicketStateChange(ExtraMsg);
		} //将该订单置于失效状态

		private void button12_Click(object sender, RoutedEventArgs e) {
			//将该订单置于生效状态
			if (TicketListView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择需要置于已支付状态的订单");
				return;
			}
			TicketInfo[] infos = new TicketInfo[TicketListView.SelectedItems.Count];
			string ExtraMsg = "1" + "\r";
			for (int i = 0; i < infos.Length; i++) {
				infos[i] = (TicketInfo)TicketListView.SelectedItems[i];
				if (infos[i]._state == 1) {
					MessageBox.Show("只有未生效的订单才可以置于已支付状态！");
					return;
				}
				ExtraMsg = ExtraMsg + infos[i].TicketNumber.ToString() + "\n";
			}
			TicketStateChange(ExtraMsg);
		} //将该订单置于生效状态

		private void TicketStateChange(string ExtraMsg) {
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, LoginPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.QueryDataPackage info = new TTS_Core.QueryDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", TTS_Core.QUERYTYPE.K_TICKET_STATE, ExtraMsg );
					byte[] sendBytes = info.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
				}
				var newClient = tcpListener.AcceptTcpClient();
				var bytes = ReadFromTcpClient(newClient); //获取数据
				var package = new TTS_Core.QueryDataPackage(bytes);
				MessageBox.Show(package.ExtraMsg);
				OrderItem_Selected();
			}
			catch {
				MessageBox.Show("无法连接到服务器!");
				return;
			}
			finally {
				if (networkStream != null) {
					networkStream.Close();
				}
				tcpClient.Close();
			}
		} //订单状态改变

		private void button10_Click(object sender, RoutedEventArgs e) {
			LoginWindow loginWindow = new LoginWindow(UserID, "127.0.0.1:" + LoginPort);
			loginWindow.Show();
			this.Close();
		} //退出登录
	}
}
