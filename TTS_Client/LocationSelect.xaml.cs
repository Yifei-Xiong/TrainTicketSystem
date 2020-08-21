using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// LocationSelect.xaml 的交互逻辑
    /// </summary>
    public partial class LocationSelect : Window
    {
        public LocationSelect() {
            InitializeComponent();
        }

		public int StationNumber { get; set; }

		public string StationName { get; set; }

		public ClientWindow.AllStationInfo allStation;
		public ClientWindow.AllStationInfo searchStation;

		public LocationSelect(string title, ClientWindow.AllStationInfo source) {
			InitializeComponent();
			this.Title = title;
			this.allStation = new ClientWindow.AllStationInfo();
			for (int i = 0; i < source.Count; i++) {
				allStation.Add(source[i]);
			} //Copy
			listView.ItemsSource = this.allStation;
			listView.Items.SortDescriptions.Add(new SortDescription("StationNumber", ListSortDirection.Ascending));
		}

		private void Button2_Click(object sender, RoutedEventArgs e) {
			searchStation = new ClientWindow.AllStationInfo();
			for (int i = 0; i < allStation.Count; i++) {
				searchStation.Add(allStation[i]);
			} //Copy

			if (precision.IsChecked == true) {
				if (textBox.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].StationNumber.ToString() != textBox.Text) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //StationNumber
				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].StationName != textBox_Copy1.Text) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //StationName
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].LineName != textBox_Copy.Text) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //LineName
			} //精确搜索

			else {
				if (textBox.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].StationNumber.ToString().IndexOf(textBox.Text) == -1) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //StationNumber
				if (textBox_Copy1.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].StationName.IndexOf(textBox_Copy1.Text) == -1) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //StationName
				if (textBox_Copy.Text != string.Empty) {
					for (int i = 0; i < searchStation.Count; i++) {
						if (searchStation[i].LineName.IndexOf(textBox_Copy.Text) == -1) {
							searchStation.Remove(searchStation[i]);
							i--;
						}
					}
				} //LineName
			} //模糊搜索

			listView.ItemsSource = searchStation;
		} //筛选

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {
			textBox.Clear();
			textBox_Copy1.Clear();
			textBox_Copy.Clear();
			listView.ItemsSource = allStation;
		} //清空

		private void button_Click(object sender, RoutedEventArgs e) {
			if (listView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择地点！");
				return;
			}
			ClientWindow.StationInfo info = new ClientWindow.StationInfo();
			info = (ClientWindow.StationInfo)listView.SelectedItem;
			if (info.StationName == "加载中...") {
				return;
			}
			StationNumber = info.StationNumber;
			StationName = info.StationName;
			Close();
		}
	}
}
