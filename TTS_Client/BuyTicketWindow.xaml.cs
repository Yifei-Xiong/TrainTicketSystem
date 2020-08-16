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


	}
}
