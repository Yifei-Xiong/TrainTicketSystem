using System;
using System.Collections.Generic;
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
		}

		public struct StationInfo {
			int StationNumber;
			string StationName;
			string LineID;
			string LineName;
		} //单个站点的相关信息

		public struct TicketInfo {
			int EnterStationNumber;
			string EnterStationName;
			string EnterStationTimeIn;
			string EnterStationTimeOut;

			int LeaveStationNumber;
			string LeaveStationName;
			string LeaveStationTimeIn;
			string LeaveStationTimeOut;

			int TicketPrice;
			int TicketLine;
			string LineID;
			string LineName;
			int TrainID;
		} //单个车票的相关信息

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


	}
}
