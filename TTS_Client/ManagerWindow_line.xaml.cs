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
    public partial class ManagerWindow_line : Window
    {
        private string user;
        private IPAddress ip_address;
        private int port;

        TcpListener tcp_listener;
        int listen_port;

        public class LineClass
        {
            public int lineid { get; set; }
            public string linename { get; set; }
        }

        public ManagerWindow_line()
        {
            InitializeComponent();
        }

        public ManagerWindow_line(string user, IPAddress ip_address, int port, TcpListener tcp_listener, int listen_port)
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
                    int lineid = -1;
                    if (Text_lineid.Text != "" && !int.TryParse(Text_lineid.Text, out lineid))
                    {
                        MessageBox.Show("lineid填写错误");
                        return;
                    }

                    var package = new TTS_Core.LineOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_QUERY,
                        lineid, Text_linename.Text);
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
                            LineClass {
                                lineid = int.Parse(package_rec.dataSet.Tables[0].Rows[i][0].ToString().Trim()),
                                linename = package_rec.dataSet.Tables[0].Rows[i][1].ToString().Trim(),
                            }) ;
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

            int selected_lineid;
            LineClass itemInfo = DataView.SelectedItem as LineClass;
            if (itemInfo != null && itemInfo is LineClass)
            {
                selected_lineid = itemInfo.lineid;
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
                    var package = new TTS_Core.LineOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_DELETE,
                        selected_lineid, "");

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

            LineClass itemInfo = DataView.SelectedItem as LineClass;
            if (itemInfo == null || !(itemInfo is LineClass))
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
                    var package = new TTS_Core.LineOperationPackage(user, ip_address + ":" + listen_port.ToString(), "server",
                        TTS_Core.Enum_OP.K_MODIFY,
                        itemInfo.lineid, Text_linename_Copy.Text);

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
                        itemInfo.linename = Text_linename_Copy.Text;
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

            LineClass itemInfo = DataView.SelectedItem as LineClass;
            if (itemInfo == null || !(itemInfo is LineClass))
            {
                MessageBox.Show("获取选中项出现问题");
                return;
            }

            Text_linename_Copy.Text = itemInfo.linename;
        }
    }
}
