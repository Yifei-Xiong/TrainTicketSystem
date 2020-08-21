using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		}

		private MySqlConnection connection;
		private string dbpw; //数据库密钥
		private int nowEnterPort;
		TcpListener myListener = null;

		public struct UserInfo {
			public string UserID { get; set; } //用户登录ID
			public string Password { get; set; } //用户密码
			public int AccontType { get; set; } //用户身份，1为普通用户，2为管理员
		} //用户信息

		public class AllUser : ObservableCollection<UserInfo> { }

		public void InitSQLDocker() {
			connection = new MySqlConnection("server=222.16.54.158;user=root;password=" + dbpw + ";database=tts_serverdb;");
			try {
				connection.Open();
			}
			catch {
				MessageBox.Show("与远程数据库的通讯失败！");
			}
		}

		private void button_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		} //关于

		private void button2_Click(object sender, RoutedEventArgs e) {
			this.dbpw = passwordBox.Password;
			passwordBox.Clear();
		} //连接数据库

		private void button_StartServer_Click(object sender, RoutedEventArgs e) {
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
					var IP = newClient.Client.RemoteEndPoint.ToString();
					byte[] receiveBytes = ReadFromTcpClient(newClient);
					int type = 0;
					using (MemoryStream ms = new MemoryStream(receiveBytes)) {
						IFormatter formatter = new BinaryFormatter();
						var DataPackage = formatter.Deserialize(ms) as TTS_Core.DataPackage;
						if (DataPackage == null) {
							MessageBox.Show("接收数据非数据包");
							continue;
						}
						//type = DataPackage.MessageType;
					}
					if (type == 0) {
						MessageBox.Show("数据包非法");
						continue;
					}
					switch (type) {
						case 1: {}
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

		AllUser SQLDocker_user
		{
			get {
				if (connection == null || connection.State != System.Data.ConnectionState.Open) {
					InitSQLDocker();
				} //若不曾连接到数据库，则进行连接
				MySqlCommand sql = new MySqlCommand("SELECT UserID, Password, AccontType FROM alluser", connection);
				MySqlDataReader reader = sql.ExecuteReader();
				AllUser result = new AllUser();
				while (reader.Read()) {
					UserInfo userinfo = new UserInfo();
					userinfo.UserID = reader[0].ToString();
					userinfo.Password = reader[1].ToString();
					userinfo.AccontType = int.Parse(reader[2].ToString());
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
					MySqlCommand sql = new MySqlCommand("INSERT INTO alluser(UserID, Password, AccontType) "
						+ "VALUES(\"" + userinfo.UserID + "\", \"" + userinfo.Password + "\", " + userinfo.AccontType + ")", connection);
					sql.ExecuteNonQuery();
					// VALUES (\"UserID\", \"Password\", AccontType) <->  VALUES ("UserID", "Password", AccontType)
				}
			}
		} //用户信息抽象成Docker，实现内容获取和内容覆盖

		private void UserInfoUpdate (UserInfo newInfo) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand query = new MySqlCommand("UPDATE alluser SET Password=\"" + newInfo.Password + "\", AccontType=" + newInfo.AccontType 
				+ "WHERE UserID=\"" + newInfo.UserID + "\"", connection );
			query.ExecuteNonQuery();
			// UPDATE alluser SET Password="Password", AccontType=AccontType WHERE UserID="UserID"
		} //实现单条内容修改，也即某一用户的用户密码、权限修改

		private void UserInfoDelete (string UserID) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand query = new MySqlCommand("DELETE FROM alluser WHERE UserID=\"" + UserID + "\"", connection);
			query.ExecuteNonQuery();
			// DELETE FROM alluser WHERE UserID="UserID"
		} //实现单条删除

		private void UserInfoAdd(UserInfo userinfo) {
			if (connection == null || connection.State != System.Data.ConnectionState.Open) {
				InitSQLDocker();
			} //若不曾连接到数据库，则进行连接
			MySqlCommand sql = new MySqlCommand("INSERT INTO alluser(UserID, Password, AccontType) "
						+ "VALUES(\"" + userinfo.UserID + "\", \"" + userinfo.Password + "\", " + userinfo.AccontType + ")", connection);
			sql.ExecuteNonQuery();
			// DELETE FROM alluser WHERE UserID="UserID"
		} //实现单条增加
	}
}
