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
	/// BuyTicketWindow.xaml 的交互逻辑
	/// </summary>
	public partial class BuyTicketWindow : Window {
		public BuyTicketWindow() {
			InitializeComponent();
		}

		public BuyTicketWindow(ClientWindow.TicketQueryInfo ticketQueryInfo) {
			InitializeComponent();
			this.ticketQueryInfo = ticketQueryInfo;
			listView.ItemsSource = trainAndStationInfos;
			trainAndStationInfos = SubmitTicketQuery(ticketQueryInfo);
		}

		public ClientWindow.TicketQueryInfo ticketQueryInfo;
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

	}
}
