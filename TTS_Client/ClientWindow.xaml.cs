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

		TicketQueryInfo ticketQueryInfo;

		public class AllStationInfo : ObservableCollection<StationInfo> { } //定义集合
		public class AllTicketInfo : ObservableCollection<TicketInfo> { } //定义集合
		public class AllBuyTicket : ObservableCollection<BuyTicket> { } //定义集合
		AllStationInfo allStationInfo = new AllStationInfo { };
		AllTicketInfo allTicketInfo = new AllTicketInfo { };
		AllBuyTicket allBuyTicket = new AllBuyTicket { };

		public DateTime QueryStartTime { get; set; }
		public DateTime QueryEndTime { get; set; }

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
	}
}
