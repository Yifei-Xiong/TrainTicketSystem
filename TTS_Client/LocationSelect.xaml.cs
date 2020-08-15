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

		public LocationSelect(string title, ClientWindow.AllStationInfo allStationInfo) {
			InitializeComponent();
			this.Title = title;
			listView.ItemsSource = allStationInfo;
			listView.Items.SortDescriptions.Add(new SortDescription("StationNumber", ListSortDirection.Ascending));
		}

		private void Button2_Click(object sender, RoutedEventArgs e) {

		}

		private void Button2_Copy_Click(object sender, RoutedEventArgs e) {

		}

		private void button_Click(object sender, RoutedEventArgs e) {
			if (listView.SelectedItems.Count == 0) {
				MessageBox.Show("未选择地点！");
				return;
			}
			ClientWindow.StationInfo info = new ClientWindow.StationInfo();
			info = (ClientWindow.StationInfo)listView.SelectedItem;
			StationNumber = info.StationNumber;
			StationName = info.StationName;
			Close();
		}
	}
}
