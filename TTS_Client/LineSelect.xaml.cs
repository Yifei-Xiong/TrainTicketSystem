using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TTS_Client
{
    /// <summary>
    /// LineSelect.xaml 的交互逻辑
    /// </summary>
    public partial class LineSelect : Window
    {
        public LineSelect() {
            InitializeComponent();
			debug_program();
        }

		public LineSelect(ClientWindow.TicketQueryInfo ticketQueryInfo, string ExtraMsg, 
			ClientWindow.AllBuyTicket allBuyTicket, int type, string UserID, IPAddress myIPAddress, TcpListener tcpListener
				, int MyPort, int ServerPort) {
			InitializeComponent();
			this.ticketQueryInfo = ticketQueryInfo; 
			this.ExtraMsg = ExtraMsg;
			this.allBuyTicket = allBuyTicket;
			this.type = type;
			this.UserID = UserID;
			this.myIPAddress = myIPAddress;
			this.tcpListener = tcpListener;
			this.MyPort = MyPort;
			this.ServerPort = ServerPort;
			allinfo = new AllInfo();
			listview.ItemsSource = allinfo;
			InitListView();
		}

		public ClientWindow.TicketQueryInfo ticketQueryInfo;
		public ClientWindow.AllBuyTicket allBuyTicket;
		public string ExtraMsg { get; set; }
		public int type { get; set; } //线路选择的类型
		public string UserID { get; set; }
		public IPAddress myIPAddress { get; set; }
		public TcpListener tcpListener { get; set; }
		public int MyPort { get; set; }
		public int ServerPort { get; set; }

		public struct Info {
			public int Number { get; set; }
			public string Detail { get; set; }
			public string _detail { get; set; }
			public int ChangeTimes { get; set; }
			public int TotalOrder { get; set; }
			public string TotalTime { get; set; }
			public DateTime _totalTime { get; set; }
			public double TotalPrice { get; set; }
			public string Note { get; set; }
        }

		public class AllInfo : ObservableCollection<Info> { } //定义集合
		public AllInfo allinfo;

		private void button_Click(object sender, RoutedEventArgs e) {
			if (listview.SelectedItems.Count == 0) {
				MessageBox.Show("请选择方案！");
				return;
			}
			Info info = new Info();
			info = (Info)listview.SelectedItem;
			string[] cache = info._detail.Split('\n');
			if (cache.Length==3) {
				CallBuyTicketWindow(cache[0], cache[1], cache[2]);
			} 
			else if (cache.Length == 5) {
				CallBuyTicketWindow(cache[0], cache[1], cache[2]);
				CallBuyTicketWindow(cache[2], cache[3], cache[4]);
			}
			else if (cache.Length == 7) {
				CallBuyTicketWindow(cache[0], cache[1], cache[2]);
				CallBuyTicketWindow(cache[2], cache[3], cache[4]);
				CallBuyTicketWindow(cache[4], cache[5], cache[6]);
			}
			Close();
		} //确认线路

		private void CallBuyTicketWindow(string Enter, string Line, string Leave) {
			ClientWindow.TicketQueryInfo subinfo1 = new ClientWindow.TicketQueryInfo();
			subinfo1.EnterStationNumber = int.Parse(Enter);
			subinfo1.Line = int.Parse(Line);
			subinfo1.LeaveStationNumber = int.Parse(Leave);
			subinfo1.StartTime = this.ticketQueryInfo.StartTime;
			subinfo1.EndTime = this.ticketQueryInfo.EndTime;
			string info1Msg = Enter + "\n" + Line + "\n" + Leave + "\n" + ticketQueryInfo.StartTime.ToString() + "\n" + ticketQueryInfo.EndTime.ToString();
			TcpClient tcpClient = null;
			NetworkStream networkStream = null;
			try {
				tcpClient = new TcpClient();
				tcpClient.Connect(myIPAddress, ServerPort); //建立与服务器的连接
				networkStream = tcpClient.GetStream();
				if (networkStream.CanWrite) {
					TTS_Core.QueryDataPackage data = new TTS_Core.QueryDataPackage(UserID, myIPAddress + ":" +
						MyPort.ToString(), "server", TTS_Core.QUERYTYPE.K_TICKETINFO_QUERY, info1Msg);
					byte[] sendBytes = data.DataPackageToBytes(); //注册数据包转化为字节数组
					networkStream.Write(sendBytes, 0, sendBytes.Length);
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
					networkStream.Close();
				}
				tcpClient.Close();
			}

			BuyTicketWindow buy = new BuyTicketWindow(subinfo1, info1Msg);
			buy.ShowDialog();
			allBuyTicket.Add(buy.selectTicket);
		} //调起买票窗体

		private void InitListView () {
			if (type==1) {
				int count = int.Parse(ExtraMsg.Split('\r')[0].Split('\n')[1]);
				//无需换乘，多种路线
				int id = 1;
				for (int i=0;i<count;i++) {
					string cache = ExtraMsg.Split('\r')[i + 3];
					Info info = new Info();
					info.Number = id;
					info.Detail = ExtraMsg.Split('\r')[1].Split('\n')[1] + " -> " +
						cache.Split('\n')[0] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[3];
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + "\n"
						+ ExtraMsg.Split('\r')[2].Split('\n')[i] + "\n"
						+ ExtraMsg.Split('\r')[1].Split('\n')[2];
					//station A id, line X id, station B id
					info.ChangeTimes = 0;
					info.TotalOrder = int.Parse(cache.Split('\n')[1]);
					info._totalTime = DateTime.Parse(cache.Split('\n')[2]);
					info.TotalTime = (info._totalTime.Minute + info._totalTime.Hour*60).ToString()+"min";
					info.TotalPrice = double.Parse(cache.Split('\n')[3]);
					info.Note = "";
					id = id + 1;
					allinfo.Add(info);
				}
			}
			else if (type==2) {
				//一次换乘，多种路线
				int count = int.Parse(ExtraMsg.Split('\r')[0].Split('\n')[1]);
				int id = 1;
				for (int i = 0; i < count; i++) {
					string cache = ExtraMsg.Split('\r')[i + 2];
					Info info = new Info();
					info.Number = id;
					info.Detail = ExtraMsg.Split('\r')[1].Split('\n')[1] + " -> " +
						cache.Split('\n')[1] + " -> "+ cache.Split('\n')[3] + " -> " +
						cache.Split('\n')[5] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[3]; 
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + "\n" +
						cache.Split('\n')[0] + "\n" + cache.Split('\n')[2] + "\n" +
						cache.Split('\n')[4] + "\n" + ExtraMsg.Split('\r')[1].Split('\n')[2];
					//station A id, line X id, station B id, line Y id, station C id
					info.ChangeTimes = 1;
					info.TotalOrder = int.Parse(cache.Split('\n')[6]);
					info._totalTime = DateTime.Parse(cache.Split('\n')[7]);
					info.TotalTime = (info._totalTime.Minute + info._totalTime.Hour * 60).ToString() + "min";
					info.TotalPrice = double.Parse(cache.Split('\n')[8]);
					info.Note = "";
					id = id + 1;
					allinfo.Add(info);
				}
			}
			else if (type==3) {
				//一次换乘，两次换乘

				string[] sep = ExtraMsg.Split('\\');

				ExtraMsg = sep[0];
				int count = int.Parse(ExtraMsg.Split('\r')[0].Split('\n')[1]);
				int id = 1;
				for (int i = 0; i < count; i++) {
					string cache = ExtraMsg.Split('\r')[i + 2];
					Info info = new Info();
					info.Number = id;
					info.Detail = ExtraMsg.Split('\r')[1].Split('\n')[1] + " -> " +
						cache.Split('\n')[1] + " -> " + cache.Split('\n')[3] + " -> " +
						cache.Split('\n')[5] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[3];
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + "\n" +
						cache.Split('\n')[0] + "\n" + cache.Split('\n')[2] + "\n" +
						cache.Split('\n')[4] + "\n" + ExtraMsg.Split('\r')[1].Split('\n')[2];
					//station A id, line X id, station B id, line Y id, station C id
					info.ChangeTimes = 1;
					info.TotalOrder = int.Parse(cache.Split('\n')[6]);
					info._totalTime = DateTime.Parse(cache.Split('\n')[7]);
					info.TotalTime = (info._totalTime.Minute + info._totalTime.Hour * 60).ToString() + "min";
					info.TotalPrice = double.Parse(cache.Split('\n')[8]);
					info.Note = "";
					id = id + 1;
					allinfo.Add(info);
				}

				ExtraMsg = sep[1];
				count = int.Parse(ExtraMsg.Split('\r')[0].Split('\n')[1]);
				for (int i = 0; i < count; i++) {
					string cache = ExtraMsg.Split('\r')[i + 2];
					Info info = new Info();
					info.Number = id;
					info.Detail = ExtraMsg.Split('\r')[1].Split('\n')[1] + " -> " +
						cache.Split('\n')[1] + " -> " + cache.Split('\n')[3] + " -> " +
						cache.Split('\n')[5] + " -> " + cache.Split('\n')[7] + " -> " +
						cache.Split('\n')[9] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[3];
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + "\n" +
						cache.Split('\n')[0] + "\n" + cache.Split('\n')[2] + "\n" +
						cache.Split('\n')[4] + "\n" + cache.Split('\n')[6] + "\n" +
						cache.Split('\n')[8] + "\n" + ExtraMsg.Split('\r')[1].Split('\n')[2];
					//station A id, line X id, station B id, line Y id, station C id, line Z id, station D id
					info.ChangeTimes = 2;
					info.TotalOrder = int.Parse(cache.Split('\n')[10]);
					info._totalTime = DateTime.Parse(cache.Split('\n')[11]);
					info.TotalTime = (info._totalTime.Minute + info._totalTime.Hour * 60).ToString() + "min";
					info.TotalPrice = double.Parse(cache.Split('\n')[12]);
					info.Note = "";
					id = id + 1;
					allinfo.Add(info);
				}

			}
			else if (type==4) {
				//两次换乘
				int count = int.Parse(ExtraMsg.Split('\r')[0].Split('\n')[1]);
				int id = 1;
				for (int i = 0; i < count; i++) {
					string cache = ExtraMsg.Split('\r')[i + 2];
					Info info = new Info();
					info.Number = id;
					info.Detail = ExtraMsg.Split('\r')[1].Split('\n')[1] + " -> " +
						cache.Split('\n')[1] + " -> " + cache.Split('\n')[3] + " -> " +
						cache.Split('\n')[5] + " -> " + cache.Split('\n')[7] + " -> " +
						cache.Split('\n')[9] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[3];
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + "\n" +
						cache.Split('\n')[0] + "\n" + cache.Split('\n')[2] + "\n" +
						cache.Split('\n')[4] + "\n" + cache.Split('\n')[6] + "\n" +
						cache.Split('\n')[8] + "\n" + ExtraMsg.Split('\r')[1].Split('\n')[2];
					//station A id, line X id, station B id, line Y id, station C id, line Z id, station D id
					info.ChangeTimes = 2;
					info.TotalOrder = int.Parse(cache.Split('\n')[10]);
					info._totalTime = DateTime.Parse(cache.Split('\n')[11]);
					info.TotalTime = (info._totalTime.Minute + info._totalTime.Hour * 60).ToString() + "min";
					info.TotalPrice = double.Parse(cache.Split('\n')[12]);
					info.Note = "";
					id = id + 1;
					allinfo.Add(info);
				}
			}
			if (allinfo.Count==0) {
				MessageBox.Show("查询失败，可能是路线过于复杂");
            }

			int minchangtimes = allinfo[0].ChangeTimes;
			double minpay = allinfo[0].TotalPrice;
			double mintime = allinfo[0]._totalTime.Ticks;
			int minorder = allinfo[0].TotalOrder;
			for (int i = 0; i < allinfo.Count; i++) {
				if (allinfo[i].ChangeTimes < minchangtimes) {
					minchangtimes = allinfo[i].ChangeTimes;
				}
				if (allinfo[i].TotalPrice < minpay) {
					minpay = allinfo[i].TotalPrice;
				}
				if (allinfo[i]._totalTime.Ticks < mintime) {
					mintime = allinfo[i]._totalTime.Ticks;
				}
				if (allinfo[i].TotalOrder < minorder) {
					minorder = allinfo[i].TotalOrder;
				}
			}
			for (int i = 0; i < allinfo.Count; i++) {
				if (allinfo[i].ChangeTimes == minchangtimes) {
					Info info = allinfo[i];
					info.Note = info.Note + " 换乘次数最少 ";
					allinfo[i] = info;
				}
				if (allinfo[i].TotalPrice == minpay) {
					Info info = allinfo[i];
					info.Note = allinfo[i].Note + " 车票总价最少 ";
					allinfo[i] = info;
				}
				if (allinfo[i]._totalTime.Ticks == mintime) {
					Info info = allinfo[i];
					info.Note = allinfo[i].Note + " 乘坐耗时最少 ";
					allinfo[i] = info;
				}
				if (allinfo[i].TotalOrder == minorder) {
					Info info = allinfo[i];
					info.Note = allinfo[i].Note + " 经过站数最少 ";
					allinfo[i] = info;
				}
			}
		}

		private void debug_program () {
			allinfo = new AllInfo();
			listview.ItemsSource = allinfo;
			this.type = 3;
			this.ExtraMsg = "1 \n 2 \r 333 \n 五山 \n 777 \n 大学城南 \r 3 \n 三号线 \n 703 \n 汉溪长隆 \n" +
				"7 \n 七号线 \n 14 \n 00:45:00 \n 6.6 \r 3 \n 三号线 \n 1003 \n 这是哪儿 \n" +
				"10 \n 十号线 \n 18 \n 01:06:00 \n 6.9 \\ 2 \n 2 \r 333 \n 五山 \n 777 \n 大学城南 \r 3 \n 三号线 \n 888 \n 客村 \n" +
				"8 \n 八号线 \n 401 \n 万胜围 \n 4 \n 四号线 \n 12 \n 00:42:00 \n 6.5 \r 3 \n 三号线 \n 555 \n 珠江新城 \n" +
				"5 \n 五号线 \n 501 \n 车陂南 \n 4 \n 四号线 \n 12 \n 00:44:00 \n 6.52 ";
			InitListView();
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
	}
}
