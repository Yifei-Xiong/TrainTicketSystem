using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
			ClientWindow.AllBuyTicket allBuyTicket, int type) {
			InitializeComponent();
			this.ticketQueryInfo = ticketQueryInfo;
			this.ExtraMsg = ExtraMsg;
			this.allBuyTicket = allBuyTicket;
			this.type = type;
			allinfo = new AllInfo();
			listview.ItemsSource = allinfo;
			InitListView();
		}

		public ClientWindow.TicketQueryInfo ticketQueryInfo;
		public ClientWindow.AllBuyTicket allBuyTicket;
		public string ExtraMsg { get; set; }
		public int type { get; set; } //线路选择的类型

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

		} //确认线路

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
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + " -> " +
						cache.Split('\n')[0] + " -> " + cache.Split('\n')[2] + " -> " +
						cache.Split('\n')[4] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[2];
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
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + " -> " +
						cache.Split('\n')[0] + " -> " + cache.Split('\n')[2] + " -> " +
						cache.Split('\n')[4] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[2];
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
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + " -> " +
						cache.Split('\n')[0] + " -> " + cache.Split('\n')[2] + " -> " +
						cache.Split('\n')[4] + " -> " + cache.Split('\n')[6] + " -> " +
						cache.Split('\n')[8] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[2];
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
					info._detail = ExtraMsg.Split('\r')[1].Split('\n')[0] + " -> " +
						cache.Split('\n')[0] + " -> " + cache.Split('\n')[2] + " -> " +
						cache.Split('\n')[4] + " -> " + cache.Split('\n')[6] + " -> " +
						cache.Split('\n')[8] + " -> " + ExtraMsg.Split('\r')[1].Split('\n')[2];
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
				Close();
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
	}
}
