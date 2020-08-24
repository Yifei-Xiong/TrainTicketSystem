using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
    /// ManagerWindow_user.xaml 的交互逻辑
    /// </summary>
    public partial class ManagerWindow_trainstation : Window
    {
        private string user;
        private IPAddress ip_address;
        private int port;

        TcpListener tcp_listener;
        int listen_port;

        public class TrainStationClass
        {
            public int trainid { get; set; }
            public int stationid { get; set; }
            public string arrivetime { get; set; }
            public string leavetime { get; set; }
            public int remainseat { get; set; }
        }

        public ManagerWindow_trainstation()
        {
            InitializeComponent();
        }

        public ManagerWindow_trainstation(string user, IPAddress ip_address, int port, TcpListener tcp_listener, int listen_port)
        {
            InitializeComponent();
            this.user = user;
            this.ip_address = ip_address;
            this.port = port;
            this.tcp_listener = tcp_listener;
            this.listen_port = listen_port;
        }

        private void balance_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private bool getStandard(out int a, string text)
        {
            int ret = -1;
            if (text != "" && !int.TryParse(text, out ret))
            {
                a = -1;
                return false;
            }
            a = ret;
            return true;
        }

        private void Button_query_Click(object sender, RoutedEventArgs e)
        {
            DataView.Items.Clear();

            TcpClient tcpClient = null;
            NetworkStream networkStream = null;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ip_address, port); //建立与服务器的连接
                networkStream = tcpClient.GetStream();
                if (networkStream.CanWrite)
                {
                    int trainid = 0;
                    int stationid = 0;
                    int remainseat = 0;
                    if (!getStandard(out trainid, Text_trainid.Text) || !getStandard(out stationid, Text_stationid.Text) || !getStandard(out remainseat, Text_remainseat.Text))
                    {
                        MessageBox.Show("查询信息填写错误");
                        return;
                    }

                    var package = new TTS_Core.TrainStationOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_QUERY,
                        trainid, stationid, Text_arrivetime.Text, Text_leavetime.Text, remainseat);

                    byte[] sendBytes = package.DataPackageToBytes(); //注册数据包转化为字节数组
                    networkStream.Write(sendBytes, 0, sendBytes.Length);

                    var newClient = tcp_listener.AcceptTcpClient();
                    var bytes = ReadFromTcpClient(newClient); //获取数据
                    var package_rec = new TTS_Core.DataSetPackage(bytes);

                    if (package_rec.forbid != 0 && package_rec.forbid != 1)
                    {
                        MessageBox.Show("出大问题");
                    }

                    if (package_rec.forbid == 1)
                    {
                        MessageBox.Show("查询失败，服务器或者网络出现故障");
                    }
                    else
                    {
                        int row = package_rec.row;
                        int col = package_rec.col;
                        
                        for (int i = 0; i < row; ++i)
                        {
                            DataView.Items.Add(new
                            TrainStationClass {
                                trainid = int.Parse(package_rec.dataSet.Tables[0].Rows[i][0].ToString().Trim()),
                                stationid = int.Parse(package_rec.dataSet.Tables[0].Rows[i][1].ToString().Trim()),
                                arrivetime = package_rec.dataSet.Tables[0].Rows[i][2].ToString().Trim(),
                                leavetime = package_rec.dataSet.Tables[0].Rows[i][3].ToString().Trim(),
                                remainseat = int.Parse(package_rec.dataSet.Tables[0].Rows[i][4].ToString().Trim())
                            });
                        }

                        DataView.Items.Refresh();

                        MessageBox.Show("查询成功");
                    }
                }
            }
            catch
            {
                MessageBox.Show("无法连接到服务器或字段填写错误!");
                return;
            }
            finally
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                }
                tcpClient.Close();
            }
        }

        private void Button_delete_Click(object sender, RoutedEventArgs e)
        {
            if (DataView.SelectedItem == null)
                return;

            int selected_trainid;
            int selected_stationid;
            TrainStationClass itemInfo = DataView.SelectedItem as TrainStationClass;
            if (itemInfo != null && itemInfo is TrainStationClass)
            {
                selected_trainid = itemInfo.trainid;
                selected_stationid = itemInfo.stationid;
            } 
            else
            {
                MessageBox.Show("获取选中项出现问题");
                return;
            }
            TcpClient tcpClient = null;
            NetworkStream networkStream = null;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ip_address, port); //建立与服务器的连接
                networkStream = tcpClient.GetStream();
                if (networkStream.CanWrite)
                {
                    var package = new TTS_Core.TrainStationOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_DELETE,
                        selected_trainid, selected_stationid, "", "", 0);

                    byte[] sendBytes = package.DataPackageToBytes();
                    networkStream.Write(sendBytes, 0, sendBytes.Length);

                    var newClient = tcp_listener.AcceptTcpClient();
                    var bytes = ReadFromTcpClient(newClient); //获取数据
                    var package_rec = new TTS_Core.DataSetPackage(bytes);

                    if (package_rec.forbid != 0 && package_rec.forbid != 1)
                    {
                        MessageBox.Show("出大问题");
                    }

                    if (package_rec.forbid == 1)
                    {
                        MessageBox.Show("删除失败，请检查完整性约束或者是服务器故障");
                    }
                    else
                    {
                        DataView.Items.Remove(DataView.SelectedItem);
                        DataView.Items.Refresh();
                    }
                }
            }
            catch
            {
                MessageBox.Show("无法连接到服务器!");
                return;
            }
            finally
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                }
                tcpClient.Close();
            }
        }

        private void Button_modify_Click(object sender, RoutedEventArgs e)
        {
            if (DataView.SelectedItem == null)
                return;

            TrainStationClass itemInfo = DataView.SelectedItem as TrainStationClass;
            if (itemInfo == null || !(itemInfo is TrainStationClass))
            {
                MessageBox.Show("获取选中项出现问题");
                return;
            }

            TcpClient tcpClient = null;
            NetworkStream networkStream = null;
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(ip_address, port); //建立与服务器的连接
                networkStream = tcpClient.GetStream();
                if (networkStream.CanWrite)
                {
                    int remainseat = 0;
                    if (!getStandard(out remainseat, Text_remainseat_Copy.Text))
                    {
                        MessageBox.Show("查询信息填写错误");
                        return;
                    }
                    var package = new TTS_Core.TrainStationOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_MODIFY,
                        itemInfo.trainid, itemInfo.stationid, Text_arrivetime_Copy.Text, Text_leavetime_Copy.Text, remainseat);

                    byte[] sendBytes = package.DataPackageToBytes(); //注册数据包转化为字节数组
                    networkStream.Write(sendBytes, 0, sendBytes.Length);

                    var newClient = tcp_listener.AcceptTcpClient();
                    var bytes = ReadFromTcpClient(newClient); //获取数据
                    var package_rec = new TTS_Core.DataSetPackage(bytes);

                    if (package_rec.forbid != 0 && package_rec.forbid != 1)
                    {
                        MessageBox.Show("出大问题");
                    }

                    if (package_rec.forbid == 1)
                    {
                        MessageBox.Show("修改失败，请检查完整性约束或者是服务器故障");
                    }
                    else
                    {
                        itemInfo.arrivetime = Text_arrivetime_Copy.Text;
                        itemInfo.leavetime = Text_leavetime_Copy.Text;
                        itemInfo.remainseat = remainseat;
                        DataView.Items.Insert(DataView.SelectedIndex, itemInfo);
                        DataView.Items.Remove(DataView.SelectedItem);
                        DataView.Items.Refresh();
                    }
                }
            }
            catch
            {
                MessageBox.Show("无法连接到服务器!");
                return;
            }
            finally
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                }
                tcpClient.Close();
            }
        }

        public byte[] ReadFromTcpClient(TcpClient tcpClient) {
			List<byte> data = new List<byte>();
			NetworkStream netStream = null;
			byte[] bytes = new byte[tcpClient.ReceiveBufferSize]; //字节数组保存接收到的数据
			int n = 0;
			try {
				netStream = tcpClient.GetStream();
				if (netStream.CanRead) {
					do { //文件大小未知
						n = netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
						if (n == (int)tcpClient.ReceiveBufferSize) {
							data.AddRange(bytes);
						} //如果bytes被读入数据填满
						else if (n != 0) {
							byte[] bytes1 = new byte[n];
							for (int i = 0; i < n; i++) {
								bytes1[i] = bytes[i];
							}
							data.AddRange(bytes1);
						} //读入的字节数不为0
					} while (netStream.DataAvailable); //是否还有数据
				} //判断数据是否可读
				bytes = data.ToArray();
			}
			catch {
				MessageBox.Show("读数据失败");
				bytes = null;
			}
			finally {
				if (netStream != null) {
					netStream.Close();
				}
				tcpClient.Close();
			}
			return bytes;
		}

        private void DataView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataView.SelectedItem == null)
                return;

            TrainStationClass itemInfo = DataView.SelectedItem as TrainStationClass;
            if (itemInfo == null || !(itemInfo is TrainStationClass))
            {
                MessageBox.Show("获取选中项出现问题");
                return;
            }

            Text_arrivetime_Copy.Text = itemInfo.arrivetime;
            Text_leavetime_Copy.Text = itemInfo.leavetime;
            Text_remainseat_Copy.Text = itemInfo.remainseat.ToString();
        }
    }
}
