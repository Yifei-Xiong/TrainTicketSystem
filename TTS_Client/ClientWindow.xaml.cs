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
		}

		public struct StationInfo {
			int StationNumber;
			string StationName;
			string LineID;
			string LineName;
		} //单个站点的相关信息

		public struct TicketInfo {
			int TicketNumber; //订单序号
			int TicketPrice; //车票价格
			int TicketLine; //所属路线序号
			string LineName; //所属线路名称
			int TrainID; //车次
			string BuyTime; //购买时间

			int EnterStationNumber; //出发站点序号
			string EnterStationName; //出发站点名称
			string EnterStationTime;
			string EnterStationTimeIn;
			string EnterStationTimeOut;

			int LeaveStationNumber; //到达站点序号
			string LeaveStationName; //到达站点名称
			string LeaveStationTime;
			string LeaveStationTimeIn;
			string LeaveStationTimeOut;
		} //单个车票的相关信息

		public struct BuyTicket {
			int EnterStationNumber;
			string EnterStationName;
			string EnterStationTime;
			string EnterStationTimeIn;
			string EnterStationTimeOut;

			int LeaveStationNumber;
			string LeaveStationName;
			string LeaveStationTime;
			string LeaveStationTimeIn;
			string LeaveStationTimeOut;

			int TicketPrice;
			int TicketLine;
			string LineName; //所属线路名称
			int TrainID; //车次
			int BuyNumber; //车票购买数量
		} //单个车票购买记录的相关信息

		public class AllStationInfo : ObservableCollection<StationInfo> { } //定义集合
		public class AllTicketInfo : ObservableCollection<TicketInfo> { } //定义集合
		public class AllBuyTicket : ObservableCollection<BuyTicket> { } //定义集合
		AllStationInfo allStationInfo = new AllStationInfo { };
		AllTicketInfo allTicketInfo = new AllTicketInfo { };
		AllBuyTicket allBuyTicket = new AllBuyTicket { };

		private void button2_Click(object sender, RoutedEventArgs e) {
			//
			MessageBox.Show("查询成功！");

			//
			MessageBox.Show("查询成功！该方案需要换乘，请选择换乘方案！");
		}

		private void button_about_Click(object sender, RoutedEventArgs e) {
			About about = new About();
			about.ShowDialog();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			LocationSelect locationSelect = new LocationSelect("请选择到达地点");
			locationSelect.ShowDialog();
			textBlock_Copy7.Text = locationSelect.StationName;
		} //选择到达地点

		private void button_Click(object sender, RoutedEventArgs e) {
			LocationSelect locationSelect = new LocationSelect("请选择出发地点");
			locationSelect.ShowDialog();
			textBlock_Copy2.Text = locationSelect.StationName;
		} //选择出发地点

		private void button3_Click(object sender, RoutedEventArgs e) {

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

		} //载入测试数据(仅用于调试)

		private void ClearData() {

		} //清空已有数据(仅用于调试)
	}
}
