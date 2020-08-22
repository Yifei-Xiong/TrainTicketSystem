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
	/// ChangeUserInfo.xaml 的交互逻辑
	/// </summary>
	public partial class ChangeUserInfo : Window {
		public ChangeUserInfo() {
			InitializeComponent();
		}

		public ChangeUserInfo(string title, string message) {
			InitializeComponent();
			this.Title = title;
			textBlock.Text = message;
			this.value = string.Empty;
		}

		public string value { get; set; }

		private void button_Click(object sender, RoutedEventArgs e) {
			this.value = textBox.Text;
			this.Close();
		}

		private void button1_Click(object sender, RoutedEventArgs e) {
			this.value = string.Empty;
			this.Close();
		}
	}
}
