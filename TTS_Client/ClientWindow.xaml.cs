using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		public ClientWindow() {
			InitializeComponent();
			BuyTicketListView.ItemsSource = allBuyTicket;
			TicketListView.ItemsSource = allTicketInfo;
			QueryStartTime = DateTime.Now;
			QueryEndTime = DateTime.Now.AddHours(12);
			textBlock_Copy12.Text = QueryStartTime.ToString() + " - " + QueryEndTime.ToString();
			ticketQueryInfo = new TicketQueryInfo();

			ListenerThread = new Thread(new ThreadStart(ListenerthreadMethod));
			ListenerThread.IsBackground = true; //主线程结束后，该线程自动结束
			ListenerThread.Start(); //启动线程
		}

		public struct StationInfo {
			public int StationNumber { get; set; } //车站编号
			public string StationName { get; set; } //车站名称
			public int Line { get; set; } //线路编号
			public string LineName { get; set; } //线路名称
		} //单个站点的相关信息

		public struct TicketInfo {
			public int TicketNumber { get; set; } //订单序号
			public int TicketPrice { get; set; } //车票价格
			public int TicketLine { get; set; } //所属路线序号
			public string LineName { get; set; } //所属线路名称
			public int TrainID { get; set; } //车次
			public string BuyTime { get; set; } //购买时间

			public int EnterStationNumber { get; set; } //出发站点序号
			public string EnterStationName { get; set; } //出发站点名称
			public string EnterStationTime { get; set; }
			public string EnterStationTimeIn { get; set; }
			public string EnterStationTimeOut { get; set; }

			public int LeaveStationNumber; //到达站点序号
			public string LeaveStationName { get; set; } //到达站点名称
			public string LeaveStationTime { get; set; }
			public string LeaveStationTimeIn { get; set; }
			public string LeaveStationTimeOut { get; set; }
		} //单个车票的相关信息

		public struct BuyTicket {
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

			public int TicketPrice { get; set; }
			public int TicketLine { get; set; }
			public string LineName { get; set; } //所属线路名称
			public int TrainID { get; set; } //车次
			public int BuyNumber { get; set; } //车票购买数量
		} //单个车票购买记录的相关信息

		public struct TicketQueryInfo {
			public int EnterStationNumber { get; set; }
			public string EnterStationName { get; set; }
			public int LeaveStationNumber { get; set; }
			public string LeaveStationName { get; set; }
			public DateTime StartTime { get; set; }
			public DateTime EndTime { get; set; }
			public bool SameLine { get; set; } //是否在一条线路上
			public int Line { get; set; } //若在一条线路上，则为线路编号
			public string LineName { get; set; } //若在一条线路上，则为线路名称
		}

		public class StateObject {
			public TcpClient tcpClient = null;
			public NetworkStream netstream = null;
			public byte[] buffer;
		}

		TicketQueryInfo ticketQueryInfo;

		public class AllStationInfo : ObservableCollection<StationInfo> { } //定义集合
		public class AllTicketInfo : ObservableCollection<TicketInfo> { } //定义集合
		public class AllBuyTicket : ObservableCollection<BuyTicket> { } //定义集合
		AllStationInfo allStationInfo = new AllStationInfo { };
		AllTicketInfo allTicketInfo = new AllTicketInfo { };
		AllBuyTicket allBuyTicket = new AllBuyTicket { };

		public DateTime QueryStartTime { get; set; }
		public DateTime QueryEndTime { get; set; }

		private Thread ListenerThread; //接收信息的侦听线程类变量
		public delegate void ReadDataF(TcpClient tcpClient); //代表无返回值 Tcpclient参数方法

		string UserID; //用户ID
		int MyPort; //本程序侦听准备使用的端口号
		string LoginPort; //登录端口
		IPAddress myIPAddress = null; //本程序侦听使用的IP地址
		TcpListener tcpListener = null; //接收信息的侦听类对象,检查是否有信息
		string IPAndPort; //记录本地IP和端口号
		string dbpw; //数据库访问密钥


		private void button2_Click(object sender, RoutedEventArgs e) {
			if (ticketQueryInfo.EnterStationNumber == 0 || ticketQueryInfo.LeaveStationNumber == 0
				|| ticketQueryInfo.StartTime.Year <= 1 || ticketQueryInfo.EndTime.Year <= 1) {
				MessageBox.Show("查询条件不完整，请重新输入！");
				return;
			} //查询未完成
			bool NeedChangeLine = false; //是否需要换乘
			if (NeedChangeLine) {
				LineSelect lineSelect = new LineSelect(ticketQueryInfo);
				lineSelect.ShowDialog();
			} //需要换乘，进入换乘方案选择窗口
			else {
				BuyTicketWindow buyTicketWindow = new BuyTicketWindow(ticketQueryInfo);
				buyTicketWindow.ShowDialog();
			} //不需要换乘，进入购票窗口
		}

		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			LocationSelect locationSelect = new LocationSelect("请选择到达地点", allStationInfo);
			locationSelect.ShowDialog();
			if (locationSelect.StationName != null) {
				textBlock_Copy7.Text = locationSelect.StationName + " (" + locationSelect.StationNumber.ToString() + ")";
				ticketQueryInfo.LeaveStationNumber = locationSelect.StationNumber;
				ticketQueryInfo.LeaveStationName = locationSelect.StationName;
			}
		} //选择到达地点

		private void button_Click(object sender, RoutedEventArgs e) {
			LocationSelect locationSelect = new LocationSelect("请选择出发地点", allStationInfo);
			locationSelect.ShowDialog();
			if (locationSelect.StationName != null) {
				textBlock_Copy2.Text = locationSelect.StationName + " (" + locationSelect.StationNumber.ToString() + ")";
				ticketQueryInfo.EnterStationNumber = locationSelect.StationNumber;
				ticketQueryInfo.EnterStationName = locationSelect.StationName;
			}
		} //选择出发地点

		private void button3_Click(object sender, RoutedEventArgs e) {
			TimeSelect timeSelect = new TimeSelect(QueryStartTime, QueryEndTime);
			timeSelect.ShowDialog();
			if (timeSelect.StartTime.Year != 1) {
				QueryStartTime = timeSelect.QueryStartTime;
				ticketQueryInfo.StartTime = timeSelect.QueryStartTime;
			}
			if (timeSelect.EndTime.Year != 1) {
				QueryEndTime = timeSelect.QueryEndTime;
				ticketQueryInfo.EndTime = timeSelect.QueryEndTime;
			}
			textBlock_Copy12.Text = QueryStartTime.ToString() + " - " + QueryEndTime.ToString();
		} //选择出发时间

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {

		}

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

		public void readRevMsg(TcpClient tcpClient) {
			/*
			byte[] bytes = ReadFromTcpClient(tcpClient); //获取数据
			FriendIPAndPort friendIPAndPort = new FriendIPAndPort();
			IMClassLibrary.ChatDataPackage chatData = new IMClassLibrary.ChatDataPackage(bytes);
			string message = string.Empty;
			switch (chatData.MessageType) {
				case 4: //单人聊天数据包
					IMClassLibrary.SingleChatDataPackage chatData1 = new IMClassLibrary.SingleChatDataPackage(bytes);
					if (chatData1.Message == "添加您为好友") {
						TcpClient tcpClient1;
						StateObject stateObject;
						tcpClient1 = new TcpClient(); //每次发送建立一个TcpClient类对象
						stateObject = new StateObject(); ////每次发送建立一个StateObject类对象
						stateObject.tcpClient = tcpClient1;
						//stateObject.buffer = SendMsg;
						stateObject.friendIPAndPort = chatData1.Receiver; //所选好友IP和端口号
						IMClassLibrary.SingleChatDataPackage addFriendData = new IMClassLibrary.SingleChatDataPackage(UserID, IPAndPort, "已收到添加请求");
						stateObject.buffer = addFriendData.DataPackageToBytes(); //buffer为发送的数据包的字节数组
						tcpClient1.BeginConnect(chatData1.Receiver.Split(':')[0], int.Parse(chatData1.Receiver.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject); //异步连接
					}
					friendIPAndPort.friendIP = chatData1.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData1.Receiver.Split(':')[1];
					friendIPAndPort.friendID = chatData1.Sender;
					message = chatData1.Receiver + "（用户ID:" + chatData1.Sender + "）（" + chatData1.sendTime.ToString() + "）说:" + chatData1.Message;
					Msg msg = new Msg();
					msg.MsgID = (allMsg.Count + 1).ToString();
					msg.MsgTime = chatData1.sendTime.ToString();
					msg.UserIP = friendIPAndPort.friendIP;
					msg.UserPort = friendIPAndPort.friendPort;
					msg.UserName = chatData1.Sender;
					msg.ChatMsg = chatData1.Message;
					msg.IsGroup = "个人聊天";
					msg.Type = chatData.MessageType;
					//allMsg.Add(msg);
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetMsg(SetMsgViewSource), msg);
					break;
				case 5: //多人聊天数据包
					IMClassLibrary.MultiChatDataPackage chatData2 = new IMClassLibrary.MultiChatDataPackage(bytes);
					friendIPAndPort.friendIP = chatData2.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData2.Receiver.Split(':')[1];
					friendIPAndPort.friendID = chatData2.Sender;
					message = chatData2.Receiver + "（用户ID:" + chatData2.SenderID + ",来自群聊" + chatData2.Sender.ToString() + "）（" + chatData2.sendTime.ToString() + "）说:" + chatData2.Message;
					Msg msg2 = new Msg();
					msg2.MsgID = (allMsg.Count + 1).ToString();
					msg2.MsgTime = chatData2.sendTime.ToString();
					msg2.UserIP = friendIPAndPort.friendIP;
					msg2.OriginPort = friendIPAndPort.friendPort;
					msg2.UserPort = chatData2.Sender.ToString();
					msg2.UserName = chatData2.SenderID;
					msg2.ChatMsg = chatData2.Message;
					msg2.IsGroup = "群组聊天";
					msg2.Type = chatData.MessageType;
					int j;
					for (j = 0; j < allMsg.Count; j++) {
						if (allMsg[j].UserName == msg2.UserName) {
							break;
						}
					}
					if (j == allMsg.Count) {
						this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetMsg(SetMsgViewSource), msg2);
					}
					//allMsg.Add(msg2);
					break;
				case 7: //文件传输数据包
					IMClassLibrary.FileDataPackage chatData3 = new IMClassLibrary.FileDataPackage(bytes);
					FileList.Add(chatData3); //加入List中待下载
					friendIPAndPort.friendIP = chatData3.Receiver.Split(':')[0];
					friendIPAndPort.friendPort = chatData3.Receiver.Split(':')[1];
					friendIPAndPort.friendID = chatData3.Sender;
					message = chatData3.Receiver + "（用户ID:" + chatData3.Sender + "）（" + chatData3.sendTime.ToString() + "）给你发了一个文件，请接收";
					Msg msg3 = new Msg();
					msg3.MsgID = (allMsg.Count + 1).ToString();
					msg3.MsgTime = chatData3.sendTime.ToString();
					msg3.UserIP = friendIPAndPort.friendIP;
					msg3.UserPort = friendIPAndPort.friendPort;
					msg3.UserName = chatData3.Sender;
					msg3.ChatMsg = "发送了一个文件";
					msg3.IsGroup = "文件消息";
					msg3.Type = chatData.MessageType;
					this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetMsg(SetMsgViewSource), msg3);
					//allMsg.Add(msg3);
					break;
				default:
					MessageBox.Show("聊天数据包读取失败");
					return;
			}
			int i;
			for (i = 0; i < myFriendIPAndPorts.Count; i++) {
				if (friendIPAndPort.friendPort == myFriendIPAndPorts[i].friendPort && friendIPAndPort.friendIP == myFriendIPAndPorts[i].friendIP ||
					friendIPAndPort.friendPort == myFriendIPAndPorts[i].friendID && friendIPAndPort.friendIP == myFriendIPAndPorts[i].friendIP) {
					break;
				}
			}
			if (i == myFriendIPAndPorts.Count) {
				friendIPAndPort = GetContact(friendIPAndPort);
				//myFriendIPAndPorts.Add(friendIPAndPort);
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new SetList(SetListViewSource), friendIPAndPort);
			} //未找到该ip与端口号，需要增加
			if (message != string.Empty) {
				//FriendListBox.Items.Add(message);
				this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new OneArgDelegate(SetFriendListBox), message); //接受信息在FriendListBox显示
			}
			*/
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



	}
}
