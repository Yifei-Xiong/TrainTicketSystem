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

namespace TTS_Client
{
    /// <summary>
    /// TimeSelect.xaml 的交互逻辑
    /// </summary>
    public partial class TimeSelect : Window
    {
        public TimeSelect() {
            InitializeComponent();
        }

		public TimeSelect(DateTime StartTime, DateTime EndTime) {
			QueryStartTime = StartTime;
			QueryEndTime = EndTime;
			InitializeComponent();
			DatePicker1.DisplayDate = StartTime;
			DatePicker2.DisplayDate = EndTime;
			textBox.Text = StartTime.Hour.ToString();
			textBox1.Text = StartTime.Minute.ToString();
			textBox_Copy.Text = EndTime.Hour.ToString();
			textBox1_Copy.Text = EndTime.Minute.ToString();
		}

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public DateTime QueryStartTime { get; set; }
		public DateTime QueryEndTime { get; set; }

		private void button_Click(object sender, RoutedEventArgs e) {
			StartTime = DateTime.Parse(DatePicker1.Text + " " + textBox.Text + ":" + textBox1.Text + ":00");
			EndTime = DateTime.Parse(DatePicker2.Text + " " + textBox_Copy.Text + ":" + textBox1_Copy.Text + ":00");
			if (StartTime >= EndTime) {
				MessageBox.Show("截止时间不能早于出发时间！");
			}
			else if (StartTime.AddDays(30) < EndTime) {
				MessageBox.Show("查询时间跨度不能大于30天！");
			}
			else {
				QueryStartTime = StartTime;
				QueryEndTime = EndTime;
				Close();
			}
		}
	}
}
