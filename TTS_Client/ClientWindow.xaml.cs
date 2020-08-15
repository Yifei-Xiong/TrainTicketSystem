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
	}
}
