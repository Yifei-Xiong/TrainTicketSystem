using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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
using MySql.Data.MySqlClient;

namespace TTS_Server {
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window {

		public MainWindow() {
			InitializeComponent();
			dbpw = string.Empty;
		}

		private MySqlConnection connection;
		private string dbpw; //数据库密钥
		private int nowEnterPort;
		TcpListener myListener = null;

		public struct UserInfo {
			public string UserID { get; set; } //用户登录ID
			public string Password { get; set; } //用户密码
			public int AccountType { get; set; } //用户身份，1为普通用户，2为管理员
			public string Phone { get; set; } //用户手机号码
			public string UserName { get; set; } //用户昵称
			public double Balance { get; set; } //用户余额
		} //用户信息

		public class StateObject {
			public TcpClient tcpClient = null;
			public NetworkStream netstream = null;
			public byte[] buffer;
		} //类定义

		public class AllUser : ObservableCollection<UserInfo> { }

		public void InitSQLDocker() {
			string debug = "server=" + dbipTextBox.Text + ";Port=3306;user=root;password=" + dbpw + ";database=tts_serverdb;";
			connection = new MySqlConnection("server=" + dbipTextBox.Text + ";Port=3306;user=root;password=" + dbpw + ";database=tts_serverdb;");
			try {
				connection.Open();
			}
			catch {
				MessageBox.Show("与远程数据库的通讯失败！");
			}
			finally {

			}
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		} //关于

		private void button2_Click(object sender, RoutedEventArgs e) {
			this.dbpw = passwordBox.Password;
			InitSQLDocker();
			passwordBox.Clear();
			if (connection != null && connection.State == System.Data.ConnectionState.Open) {
				MessageBox.Show("远程数据库连接成功！");
			}
		} //连接数据库

		private void button_StartServer_Click(object sender, RoutedEventArgs e) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				MessageBox.Show("请先连接远程数据库！");
				return;
			}
			if ((string)button_StartServer.Content == "关闭服务器") {
				this.Close();
			}

			bool canTurnPortToInt = int.TryParse(portText.Text, out nowEnterPort);
			if (canTurnPortToInt == false || nowEnterPort > 65535 || nowEnterPort < 1024) {
				MessageBox.Show("端口号输入错误");
				return;
			}
			button_StartServer.Content = "关闭服务器";
			//IsPortCanUse = true;
			var threadAccept = new Thread(AcceptClientConnect);
			threadAccept.IsBackground = true;
			threadAccept.Start();
		} //开启服务器

		/*
		public AllTrainAndStationInfo trainAndStationInfos;

		public struct TrainAndStationInfo {
			public int TrainNumber { get; set; } //列车编号
			public int StationNumber { get; set; } //车站编号
			public DateTime ArriveTime { get; set; } //到站时间
			public DateTime LeaveTime { get; set; } //出站时间
			public int RemainSeats { get; set; } //剩余座位
		}

		public class AllTrainAndStationInfo : ObservableCollection<TrainAndStationInfo> { } //定义集合

		private AllTrainAndStationInfo SubmitTicketQuery(ClientWindow.TicketQueryInfo ticketQueryInfo) {
			//发送查询数据至服务端
			//接收服务端回传的查询结果
			AllTrainAndStationInfo trainAndStationInfos = new AllTrainAndStationInfo();
			TrainAndStationInfo trainAndStationInfo = new TrainAndStationInfo();
			trainAndStationInfo.TrainNumber = 103;
			trainAndStationInfo.StationNumber = 1151;
			trainAndStationInfo.ArriveTime = DateTime.Now.AddHours(1);
			trainAndStationInfo.LeaveTime = DateTime.Now.AddHours(1.3);
			trainAndStationInfo.RemainSeats = 19;
			trainAndStationInfos.Add(trainAndStationInfo);

			trainAndStationInfo.TrainNumber = 104;
			trainAndStationInfo.StationNumber = 1151;
			trainAndStationInfo.ArriveTime = DateTime.Now.AddHours(1);
			trainAndStationInfo.LeaveTime = DateTime.Now.AddHours(1.3);
			trainAndStationInfo.RemainSeats = 19;
			trainAndStationInfos.Add(trainAndStationInfo);

			return trainAndStationInfos;

		}
		*/

		private void AcceptClientConnect() {
			//IPAddress ip = (IPAddress)Dns.GetHostAddresses(Dns.GetHostName()).GetValue(0);//服务器端ip
			IPAddress ip = IPAddress.Parse("127.0.0.1");
			try {
				myListener = new TcpListener(ip, nowEnterPort);//创建TcpListener实例
				myListener.Start();//start
			}
			catch {
				MessageBox.Show("TcpListener创建失败，请更改端口号或检查计算机网络！");
				Close();
				return;
			}
			var newClient = new TcpClient();
			while (true) {
				try {
					newClient = myListener.AcceptTcpClient();//等待客户端连接
				}
				catch {
					if (newClient == null)
						return;
				}

				try {
					var IP = newClient.Client.RemoteEndPoint.ToString(); //发送方的IP和端口号
					byte[] receiveBytes = ReadFromTcpClient(newClient);
					string IPandPort = ip.ToString() + ":" + nowEnterPort.ToString();
					TTS_Core.MESSAGETYPE type;
					using (MemoryStream ms = new MemoryStream(receiveBytes)) {
						IFormatter formatter = new BinaryFormatter();
						var dataPackage = formatter.Deserialize(ms) as TTS_Core.DataPackage;
						if (dataPackage == null) {
							MessageBox.Show("接收数据非数据包");
							continue;
						}
						type = dataPackage.MessageType;
					}
					if (type == TTS_Core.MESSAGETYPE.K_DATA_PACKAGE) {
						MessageBox.Show("数据包非法");
						continue;
					}
					switch (type) {
						case TTS_Core.MESSAGETYPE.K_LOGIN_DATA_PACKAGE: {
								string SendMessage = "消息异常";
								var package = new TTS_Core.LoginDataPackage(receiveBytes);
								UserInfo serverData = UserInfoSearch(package.Sender);
								if (serverData.Password == package.Password) {
									SendMessage = "用户登录成功！";
									package.Sender = serverData.Phone + "\n"
										+ serverData.UserName + "\n" + serverData.Balance.ToString() + "\n";
									if (serverData.AccountType == 2) {
										SendMessage = "管理员登录成功！";
									}
								}
								else {
									SendMessage = "账号或密码错误！";
								}
								TcpClient tcpClient;
								StateObject stateObject;
								tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
								stateObject = new StateObject(); //每次发送建立一个StateObject类对象
								stateObject.tcpClient = tcpClient;
								var data = new TTS_Core.DataPackage(SendMessage, IPandPort, package.Sender);
								stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
								tcpClient.BeginConnect(package.IPandPort.Split(':')[0], int.Parse(package.IPandPort.Split(':')[1]),
									new AsyncCallback(SentCallBackF), stateObject);
							} //login
							break;
						case TTS_Core.MESSAGETYPE.K_REGISTER_DATA_PACKAGE: {
								var package = new TTS_Core.RegisterDataPackage(receiveBytes);
								UserInfo serverData = UserInfoSearch(package.Sender);
								string SendMessage;
								if (serverData.UserID == null) {
									UserInfo user = new UserInfo();
									user.UserID = package.UserID;
									user.Password = package.Password;
									user.AccountType = 1;
									user.Phone = "未指定";
									user.UserName = package.UserID; 
									user.Balance = 0;
									UserInfoAdd(user);
									SendMessage = "注册成功！";
								}
								else {
									SendMessage = "注册失败，该账号已存在！";
								}
								TcpClient tcpClient;
								StateObject stateObject;
								tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
								stateObject = new StateObject(); //每次发送建立一个StateObject类对象
								stateObject.tcpClient = tcpClient;
								var data = new TTS_Core.DataPackage(SendMessage, IPandPort, package.Sender);
								stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
								tcpClient.BeginConnect(package.IPandPort.Split(':')[0], int.Parse(package.IPandPort.Split(':')[1]),
									new AsyncCallback(SentCallBackF), stateObject);
							} //register
							break;
						case TTS_Core.MESSAGETYPE.K_TICKETQUERY_DATA_PACKAGE: {
								var package = new TTS_Core.TicketQueryDataPackage(receiveBytes);
							} //车票查询
							break;
						case TTS_Core.MESSAGETYPE.K_QUERY_DATA_PACKAGE: {
								var package = new TTS_Core.QueryDataPackage(receiveBytes);
								switch (package.QueryType) {
									case TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY: {
											buy_ticket_query(package.IPandPort, package.ExtraMsg.Split('\n')[0], package.ExtraMsg.Split('\n')[1]);
										} break;
								}
							} //特定查询
							break;
						case TTS_Core.MESSAGETYPE.K_USER_INFO_CHANGE: {
								var package = new TTS_Core.InfoChangeDataPackage(receiveBytes);
								if (package.ChangeType == 1) {
									UserInfo serverData = UserInfoSearch(package.Sender);
									serverData.UserName = package.ChangeValue;
									UserInfoUpdate(serverData);
								} //更改昵称
								else if (package.ChangeType == 2) {
									UserInfo serverData = UserInfoSearch(package.Sender);
									serverData.Phone = package.ChangeValue;
									UserInfoUpdate(serverData);
								} //更改手机号
								else if (package.ChangeType == 3) {
									UserInfo serverData = UserInfoSearch(package.Sender);
									string query = serverData.Phone + "\n"
										+ serverData.UserName + "\n" + serverData.Balance.ToString();
									TcpClient tcpClient;
									StateObject stateObject;
									tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
									stateObject = new StateObject(); //每次发送建立一个StateObject类对象
									stateObject.tcpClient = tcpClient;
									var data = new TTS_Core.DataPackage("用户信息刷新成功！", IPandPort, query);
									stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
									tcpClient.BeginConnect(package.IPandPort.Split(':')[0], int.Parse(package.IPandPort.Split(':')[1]),
										new AsyncCallback(SentCallBackF), stateObject);
								} //更新用户信息
								else if (package.ChangeType == 4) {
									UserInfo serverData = UserInfoSearch(package.Sender);
									serverData.Balance = serverData.Balance + double.Parse(package.ChangeValue);
									UserInfoUpdate(serverData);
									string query = serverData.Phone + "\n"
										+ serverData.UserName + "\n" + serverData.Balance.ToString();
									TcpClient tcpClient;
									StateObject stateObject;
									tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
									stateObject = new StateObject(); //每次发送建立一个StateObject类对象
									stateObject.tcpClient = tcpClient;
									var data = new TTS_Core.DataPackage("充值成功！", IPandPort, query);
									stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
									tcpClient.BeginConnect(package.IPandPort.Split(':')[0], int.Parse(package.IPandPort.Split(':')[1]),
										new AsyncCallback(SentCallBackF), stateObject);
								} //充值
							} // 用户信息更新
							break;
					}
				}
				catch {
					break;
				}
			}
		}

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

		AllUser SQLDocker_user
		{
			get {
				if (connection == null || connection.State != System.Data.ConnectionState.Open) {
					InitSQLDocker();
				} //若不曾连接到数据库，则进行连接
				MySqlCommand sql = new MySqlCommand("SELECT UserID, Password, AccountType, phone, username, balance FROM alluser", connection);
				MySqlDataReader reader = sql.ExecuteReader();
				AllUser result = new AllUser();
				while (reader.Read()) {
					UserInfo userinfo = new UserInfo();
					userinfo.UserID = reader[0].ToString();
					userinfo.Password = reader[1].ToString();
					userinfo.AccountType = int.Parse(reader[2].ToString());
					userinfo.Phone = reader[3].ToString();
					userinfo.UserName = reader[4].ToString();
					userinfo.Balance = double.Parse(reader[5].ToString());
					result.Add(userinfo);
				}
				reader.Close();
				return result;
			}
			set {
				if (connection == null || connection.State != System.Data.ConnectionState.Open) {
					InitSQLDocker();
				} //若不曾连接到数据库，则进行连接
				var query = new MySqlCommand("DELETE FROM alluser", connection);
				query.ExecuteNonQuery();
				foreach (UserInfo userinfo in value) {
					MySqlCommand sql = new MySqlCommand("INSERT INTO alluser(UserID, Password, AccountType, phone, username, balance) "
						+ "VALUES(\"" + userinfo.UserID + "\", \"" + userinfo.Password + "\", " + userinfo.AccountType.ToString() 
						+ ", \"" + userinfo.Phone + "\", \"" + userinfo.UserName + "\", " + userinfo.Balance.ToString() + ")", connection);
					sql.ExecuteNonQuery();
					// VALUES (\"UserID\", \"Password\", AccountType, \"phone\", \"username\", balance) <->  VALUES ("UserID", "Password", AccountType)
				}
			}
		} //用户信息抽象成Docker，实现内容获取和内容覆盖

		private void UserInfoUpdate (UserInfo newInfo) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand query = new MySqlCommand("UPDATE alluser SET Password=\"" + newInfo.Password + "\", AccountType=" + newInfo.AccountType .ToString()
				+ ", phone=\"" + newInfo.Phone + "\", username = \"" + newInfo.UserName + "\", balance=" + newInfo.Balance.ToString()
				+ " WHERE UserID=\"" + newInfo.UserID + "\"", connection );
			try {
				query.ExecuteNonQuery();
			}
			catch { }
			// UPDATE alluser SET Password="Password", AccountType=AccountType ,
			// phone="phone", username="username", balance=balance WHERE UserID="UserID"
		} //实现单条用户信息内容修改，也即某一用户的用户密码、权限修改

		private void UserInfoDelete (string UserID) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand query = new MySqlCommand("DELETE FROM alluser WHERE UserID=\"" + UserID + "\"", connection);
			try {
				query.ExecuteNonQuery();
			}
            catch { }
			// DELETE FROM alluser WHERE UserID="UserID"
		} //实现单条用户信息删除

		private void UserInfoAdd(UserInfo userinfo) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand sql = new MySqlCommand("INSERT INTO alluser(UserID, Password, AccountType, phone, username, balance) "
						+ "VALUES(\"" + userinfo.UserID + "\", \"" + userinfo.Password + "\", " + userinfo.AccountType.ToString()
						+ ", \"" + userinfo.Phone + "\", \"" + userinfo.UserName + "\", " + userinfo.Balance.ToString() + ")", connection);
			try {
				sql.ExecuteNonQuery();
			}
			catch { }
		} //实现单条用户信息增加

		private UserInfo UserInfoSearch(string userID) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand sql = new MySqlCommand("SELECT UserID, Password, AccountType, phone, username, balance FROM alluser WHERE UserID=\"" + userID + "\"", connection);
			UserInfo info = new UserInfo();
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					info.UserID = reader[0].ToString();
					info.Password = reader[1].ToString();
					info.AccountType = int.Parse(reader[2].ToString());
					info.Phone = reader[3].ToString();
					info.UserName = reader[4].ToString();
					info.Balance = double.Parse(reader[5].ToString());
				}
				reader.Close();
			}
			catch { }
			return info;
		} //实现单条用户信息查询

		private void buy_ticket_query(string ip, string EnterID, string LeaveID) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand sql = new MySqlCommand("SELECT A.lineid FROM stationline A, stationline B WHERE A.stationid=" +
				EnterID + " AND A.lineid=B.lineid AND A.stationid<>B.stationid AND B.stationid=" + LeaveID, connection);
			// SELECT A.lineid FROM stationline A, stationline B WHERE A.stationid=出发站ID AND A.lineid=B.lineid AND A.stationid<>B.stationid AND B.stationid=到达站ID
			List<string> OutList = new List<string>();
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					OutList.Add(reader[0].ToString());
				}
				reader.Close();
			}
			catch { }
			if (OutList.Count == 1) {
				string ExtraMsg = "0" + "\n" + "1" + "\n" + OutList[0];
				TcpClient tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
				StateObject stateObject = new StateObject(); //每次发送建立一个StateObject类对象
				stateObject.tcpClient = tcpClient;
				var data = new TTS_Core.QueryDataPackage("Server", ip, "", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
				stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
				tcpClient.BeginConnect(ip.Split(':')[0], int.Parse(ip.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject);
			}
			else if (OutList.Count >= 1) {
				string ExtraMsg = "0" + "\n" + OutList.Count.ToString() + "\r";
				ExtraMsg = ExtraMsg + EnterID + "\n" + StationNameQuery(EnterID) + "\n" + LeaveID + "\n" +
					StationNameQuery(LeaveID) + "\r";
				for (int i = 0; i < OutList.Count; i++) {
					ExtraMsg = ExtraMsg + OutList[i] + "\n";
				} // 0 \n 2 \r startID \n startName \n endID \n endName \r 2 \n 7 \n \r line2name \n line2order \n
				  // line2time \n line2price \r line7name \n line7order \n line7time \n line7price \n
				ExtraMsg = ExtraMsg + "\r";
				for (int i = 0; i < OutList.Count; i++) {
					ExtraMsg = ExtraMsg + LineNameQuery(OutList[i]) + "\n" + StationOrderQuery(OutList[i], EnterID, LeaveID)
						+ "\n" + TimeQuery(OutList[i], EnterID, LeaveID).ToString() + "\n" 
						+ TicketPriceQuery(OutList[i], EnterID, LeaveID).ToString() + "\r";
				}
				TcpClient tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
				StateObject stateObject = new StateObject(); //每次发送建立一个StateObject类对象
				stateObject.tcpClient = tcpClient;
				var data = new TTS_Core.QueryDataPackage("Server", ip, "", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
				stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
				tcpClient.BeginConnect(ip.Split(':')[0], int.Parse(ip.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject);
			}
			else {
				//需要换乘至少一次
				List<string> OutList1_1 = new List<string>();
				List<string> OutList1_2 = new List<string>();
				List<string> OutList1_3 = new List<string>();
				MySqlCommand sql1 = new MySqlCommand("SELECT A.lineid, B.stationid, C.lineid FROM stationline A, stationline B, " +
					"stationline C, stationline D WHERE A.stationid="+EnterID+" AND A.lineid=B.lineid AND A.stationid<>B.stationid " +
					"AND B.stationid=C.stationid AND B.lineid<>C.lineid AND C.lineid=D.lineid AND C.stationid<>D.stationid AND" +
					" D.stationid="+ LeaveID, connection);
				try {
					MySqlDataReader reader = sql1.ExecuteReader();
					while (reader.Read()) {
						OutList1_1.Add(reader[0].ToString());
						OutList1_2.Add(reader[0].ToString());
						OutList1_3.Add(reader[0].ToString());
					}
					reader.Close();
				}
				catch { }
				if (OutList1_1.Count >= 2) {
					//若一次换乘的方案多于一种，则不考虑两次换乘方案
					string ExtraMsg = "1" + "\n" + OutList1_1.Count.ToString() + "\r";
					ExtraMsg = ExtraMsg + EnterID + "\n" + StationNameQuery(EnterID) + "\n" + LeaveID + "\n" + StationNameQuery(LeaveID) + "\r";
					for (int i = 0; i < OutList1_1.Count; i++) {
						ExtraMsg = ExtraMsg + OutList1_1[i] + "\n" + LineNameQuery(OutList1_1[i]) + "\n" +
							OutList1_2[i] + "\n" + StationNameQuery(OutList1_2[i]) + "\n" +
							OutList1_3[i] + "\n" + LineNameQuery(OutList1_3[i]) + "\n" +
							(int.Parse(StationOrderQuery(OutList1_1[i], EnterID, OutList1_2[i])) + int.Parse(StationOrderQuery(OutList1_3[i], OutList1_2[i], LeaveID))).ToString() + "\n" +
							(TimeQuery(OutList1_1[i], EnterID, OutList1_2[i]).AddTicks(TimeQuery(OutList1_3[i], OutList1_2[i], LeaveID).Ticks)).ToString() + "\n" +
							(TicketPriceQuery(OutList1_1[i], EnterID, OutList1_2[i]) + TicketPriceQuery(OutList1_3[i], OutList1_2[i], LeaveID)).ToString() + "\r";
					} // lineid linename stationid stationname lineid linename order time price
					TcpClient tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
					StateObject stateObject = new StateObject(); //每次发送建立一个StateObject类对象
					stateObject.tcpClient = tcpClient;
					var data = new TTS_Core.QueryDataPackage("Server", ip, "", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
					stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
					tcpClient.BeginConnect(ip.Split(':')[0], int.Parse(ip.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject);
				} else if (OutList1_1.Count == 1) {
					//一次换乘方案仅有一种，继续考虑两次换乘方案
					string ExtraMsg = "1" + "\n" + OutList1_1.Count.ToString() + "\r";
					ExtraMsg = ExtraMsg + EnterID + "\n" + StationNameQuery(EnterID) + "\n" + LeaveID + "\n" + StationNameQuery(LeaveID) + "\r";
					for (int i = 0; i < OutList1_1.Count; i++) {
						ExtraMsg = ExtraMsg + OutList1_1[i] + "\n" + LineNameQuery(OutList1_1[i]) + "\n" +
							OutList1_2[i] + "\n" + StationNameQuery(OutList1_2[i]) + "\n" +
							OutList1_3[i] + "\n" + LineNameQuery(OutList1_3[i]) + "\n" +
							(int.Parse(StationOrderQuery(OutList1_1[i], EnterID, OutList1_2[i])) + int.Parse(StationOrderQuery(OutList1_3[i], OutList1_2[i], LeaveID))).ToString() + "\n" +
							(TimeQuery(OutList1_1[i], EnterID, OutList1_2[i]).AddTicks(TimeQuery(OutList1_3[i], OutList1_2[i], LeaveID).Ticks)).ToString() + "\n" +
							(TicketPriceQuery(OutList1_1[i], EnterID, OutList1_2[i])+ TicketPriceQuery(OutList1_3[i], OutList1_2[i], LeaveID)).ToString();
					} // lineid linename stationid stationname lineid linename order time price

					ExtraMsg = ExtraMsg + "\\";
					List<string> OutList2_1 = new List<string>();
					List<string> OutList2_2 = new List<string>();
					List<string> OutList2_3 = new List<string>();
					List<string> OutList2_4 = new List<string>();
					List<string> OutList2_5 = new List<string>();
					MySqlCommand sql2 = new MySqlCommand("SELECT A.lineid, B.stationid, C.lineid FROM stationline A, stationline B, " +
						"stationline C, stationline D WHERE A.stationid=" + EnterID + " AND A.lineid=B.lineid AND A.stationid<>B.stationid " +
						"AND B.stationid=C.stationid AND B.lineid<>C.lineid AND C.lineid=D.lineid AND C.stationid<>D.stationid AND" +
						" D.stationid=" + LeaveID, connection);
					try {
						MySqlDataReader reader = sql2.ExecuteReader();
						while (reader.Read()) {
							OutList2_1.Add(reader[0].ToString());
							OutList2_2.Add(reader[0].ToString());
							OutList2_3.Add(reader[0].ToString());
							OutList2_4.Add(reader[0].ToString());
							OutList2_5.Add(reader[0].ToString());
						}
						reader.Close();
					}
					catch { }
					ExtraMsg = ExtraMsg + "2" + "\n" + OutList2_1.Count.ToString() + "\r";
					ExtraMsg = ExtraMsg + EnterID + "\n" + StationNameQuery(EnterID) + "\n" + LeaveID + "\n" + StationNameQuery(LeaveID) + "\r";
					for (int i = 0; i < OutList2_1.Count; i++) {
						ExtraMsg = ExtraMsg + OutList2_1[i] + "\n" + LineNameQuery(OutList2_1[i]) + "\n" +
							OutList2_2[i] + "\n" + StationNameQuery(OutList2_2[i]) + "\n" +
							OutList2_3[i] + "\n" + LineNameQuery(OutList2_3[i]) + "\n" +
							OutList2_4[i] + "\n" + StationNameQuery(OutList2_4[i]) + "\n" +
							OutList2_5[i] + "\n" + LineNameQuery(OutList2_5[i]) + "\n" +
							(int.Parse(StationOrderQuery(OutList2_1[i], EnterID, OutList2_2[i])) +
							int.Parse(StationOrderQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i])) +
							int.Parse(StationOrderQuery(OutList2_5[i], OutList2_4[i], LeaveID))).ToString() + "\n" +
							(TimeQuery(OutList2_1[i], EnterID, OutList2_2[i])
							.AddTicks(TimeQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i]).Ticks +
							TimeQuery(OutList2_5[i], OutList2_4[i], LeaveID).Ticks)).ToString() + "\n" +
							(TicketPriceQuery(OutList2_1[i], EnterID, OutList2_2[i]) +
							TicketPriceQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i]) +
							TicketPriceQuery(OutList2_5[i], OutList2_4[i], LeaveID)).ToString();
					}

					TcpClient tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
					StateObject stateObject = new StateObject(); //每次发送建立一个StateObject类对象
					stateObject.tcpClient = tcpClient;
					var data = new TTS_Core.QueryDataPackage("Server", ip, "", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
					stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
					tcpClient.BeginConnect(ip.Split(':')[0], int.Parse(ip.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject);
				} else {
					//需要换乘至少两次
					List<string> OutList2_1 = new List<string>();
					List<string> OutList2_2 = new List<string>();
					List<string> OutList2_3 = new List<string>();
					List<string> OutList2_4 = new List<string>();
					List<string> OutList2_5 = new List<string>();
					MySqlCommand sql2 = new MySqlCommand("SELECT A.lineid, B.stationid, C.lineid FROM stationline A, stationline B, " +
						"stationline C, stationline D WHERE A.stationid=" + EnterID + " AND A.lineid=B.lineid AND A.stationid<>B.stationid " +
						"AND B.stationid=C.stationid AND B.lineid<>C.lineid AND C.lineid=D.lineid AND C.stationid<>D.stationid AND" +
						" D.stationid=" + LeaveID, connection);
					try {
						MySqlDataReader reader = sql2.ExecuteReader();
						while (reader.Read()) {
							OutList2_1.Add(reader[0].ToString());
							OutList2_2.Add(reader[0].ToString());
							OutList2_3.Add(reader[0].ToString());
							OutList2_4.Add(reader[0].ToString());
							OutList2_5.Add(reader[0].ToString());
						}
						reader.Close();
					}
					catch { }
					string ExtraMsg = "2" + "\n" + OutList2_1.Count.ToString() + "\r";
					ExtraMsg = ExtraMsg + EnterID + "\n" + StationNameQuery(EnterID) + "\n" + LeaveID + "\n" + StationNameQuery(LeaveID) + "\r";
					for (int i = 0; i < OutList2_1.Count; i++) {
						ExtraMsg = ExtraMsg + OutList2_1[i] + "\n" + LineNameQuery(OutList2_1[i]) + "\n" +
							OutList2_2[i] + "\n" + StationNameQuery(OutList2_2[i]) + "\n" +
							OutList2_3[i] + "\n" + LineNameQuery(OutList2_3[i]) + "\n" +
							OutList2_4[i] + "\n" + StationNameQuery(OutList2_4[i]) + "\n" +
							OutList2_5[i] + "\n" + LineNameQuery(OutList2_5[i]) + "\n" +
							(int.Parse(StationOrderQuery(OutList2_1[i], EnterID, OutList2_2[i])) +
							int.Parse(StationOrderQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i])) +
							int.Parse(StationOrderQuery(OutList2_5[i], OutList2_4[i], LeaveID))).ToString() + "\n" +
							(TimeQuery(OutList2_1[i], EnterID, OutList2_2[i])
							.AddTicks(TimeQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i]).Ticks +
							TimeQuery(OutList2_5[i], OutList2_4[i], LeaveID).Ticks)).ToString() + "\n" +
							(TicketPriceQuery(OutList2_1[i], EnterID, OutList2_2[i])+
							TicketPriceQuery(OutList2_3[i], OutList2_2[i], OutList2_4[i])+
							TicketPriceQuery(OutList2_5[i], OutList2_4[i], LeaveID)).ToString();
					} // lineid linename stationid stationname lineid linename stationid stationname lineid linename order time price
					TcpClient tcpClient = new TcpClient(); //每次发送建立一个TcpClient类对象
					StateObject stateObject = new StateObject(); //每次发送建立一个StateObject类对象
					stateObject.tcpClient = tcpClient;
					var data = new TTS_Core.QueryDataPackage("Server", ip, "", TTS_Core.QUERYTYPE.K_BUYTICKET_QUERY, ExtraMsg);
					stateObject.buffer = data.DataPackageToBytes(); //buffer为发送的数据包的字节数组
					tcpClient.BeginConnect(ip.Split(':')[0], int.Parse(ip.Split(':')[1]), new AsyncCallback(SentCallBackF), stateObject);
				} 
			} 

		} //实现线路的查询

		private DateTime TimeQuery(string lineID, string EnterID, string LeaveID) {
			MySqlCommand sql = new MySqlCommand("SELECT B.arrivetime-A.leavetime FROM trainstation A, trainstation B, train C WHERE " +
				"A.trainid=B.trainid AND B.arrivetime>A.leavetime AND A.stationid="+EnterID+" AND B.stationid="+LeaveID+
				" AND B.trainid=C.trainid AND C.lineid=" + lineID, connection);
			string time = null;
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					time = reader[0].ToString();
				}
				reader.Close();
			}
			catch { }
			return DateTime.Parse(time) ;
		}
		private string StationOrderQuery(string lineID, string EnterID, string LeaveID) {
			MySqlCommand sql = new MySqlCommand("SELECT ABS(B.stationorder-A.stationorder) FROM stationline A, stationline B WHERE " +
				"A.lineid=B.lineid AND B.lineid=" + lineID + " AND A.stationid=" + EnterID + " AND B.stationid=" + LeaveID, connection);
			string order = null;
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					order = reader[0].ToString();
				}
				reader.Close();
			}
			catch { }
			return order;
		}
		private string StationNameQuery(string stationID) {
			MySqlCommand sql = new MySqlCommand("SELECT stationname from station where stationid="+stationID, connection);
			string name = null;
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					name = reader[0].ToString();
				}
				reader.Close();
			}
			catch { }
			return name;
		}

		private string LineNameQuery(string LineID) {
			MySqlCommand sql = new MySqlCommand("SELECT linename from line where lineid=" + LineID, connection);
			string name = null;
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					name = reader[0].ToString();
				}
				reader.Close();
			}
			catch { }
			return name;
		}

		private double TicketPriceQuery(string lineID, string EnterID, string LeaveID) {
			MySqlCommand sql = new MySqlCommand("SELECT ticketprice FROM ticketprice WHERE enterstationid=" + EnterID 
				+ " AND leavestationid=" + "LeaveID", connection);
			string price = null;
			double ret = 0;
			try {
				MySqlDataReader reader = sql.ExecuteReader();
				while (reader.Read()) {
					price = reader[0].ToString();
				}
				reader.Close();
				ret = double.Parse(price);
			}
			catch { }
			return ret;
		}
	}
}
