using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace TTS_Core
{
    //数据包类型的枚举
    public enum MESSAGETYPE
    {
        K_DATA_PACKAGE,  //数据包类
        K_LOGIN_DATA_PACKAGE,  //登入数据包类
        K_REGISTER_DATA_PACKAGE,  //注册数据包类
        K_QUERY_DATA_PACKAGE,  //查询数据包类
        K_TICKETQUERY_DATA_PACKAGE  //客户端向服务端发送的查询条件类
    }

    //查询类别的枚举
    public enum QUERYTYPE
    {
        K_DEPARTURE_STATION,  // 出发站点查询
        K_ARRIVAL_STATION,  //到达站点查询
        K_USER_ORDER,  //用户订单查询
        K_LINE,  //线路查询
        K_TRAIN,  //列车查询
        K_STATION,  //车站查询
        K_STATION_LINE,  //车站-线路查询
        K_TRAIN_STATION,  //列车-车站查询
        K_TRAIN_PASSENGERS, //列车乘客查询
        K_TICKET_PRICE  //车票价格查询
    }


    //数据包类，Type=0
    [Serializable]
	public class DataPackage {
		public DataPackage() {
			sendTime = DateTime.Now;
		}
		public byte[] DataPackageToBytes() {
			using (MemoryStream ms = new MemoryStream()) {
				IFormatter formatter = new BinaryFormatter();
				formatter.Serialize(ms, this);
				return ms.GetBuffer();
			}
		} //数据包转化为字节数组
		public DataPackage(string sender, string receiver) {
			this.Sender = sender;
			this.Receiver = receiver;
			sendTime = DateTime.Now;
		} //构造函数 接受发送者与接收者字符串
        public DataPackage(byte[] Bytes)
        {
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                DataPackage dataPackage = formatter.Deserialize(ms) as DataPackage;
                if (dataPackage != null)
                {
                    this.MessageType = dataPackage.MessageType;
                }
            }
        } //构造函数 字节数组转化为数据包

        public DateTime sendTime { get; set; } //消息的发送时间
		public string Sender { get; set; }
		public string Receiver { get; set; }
		public MESSAGETYPE MessageType = MESSAGETYPE.K_DATA_PACKAGE; //数据包类Type为0
	}

	//登入数据包类，Type=1
	[Serializable]
	public class LoginDataPackage : DataPackage {
		public LoginDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				LoginDataPackage loginDataPackage = formatter.Deserialize(ms) as LoginDataPackage;
				if (loginDataPackage != null) {
					this.Password = loginDataPackage.Password;
					this.UserID = loginDataPackage.UserID;
					this.Sender = loginDataPackage.Sender;
					this.Receiver = loginDataPackage.Receiver;
					this.sendTime = loginDataPackage.sendTime;
					this.MessageType = loginDataPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public LoginDataPackage(string sender, string receiver, string userID, string password) : base(sender,receiver) {
			MessageType = MESSAGETYPE.K_LOGIN_DATA_PACKAGE;
			this.UserID = userID;
			this.Password = password;
		} //构造函数 接受发送者,接收者字符串,登录用户名与密码
		public string UserID { get; set; } //登录用户名
		public string Password { get; set; } //登录密码
	}

	//注册数据包类，Type=2
	[Serializable]
	public class RegisterDataPackage : DataPackage {
		public RegisterDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				RegisterDataPackage registerPackage = formatter.Deserialize(ms) as RegisterDataPackage;
				if (registerPackage != null) {
					this.Password = registerPackage.Password;
					this.UserID = registerPackage.UserID;
					this.Sender = registerPackage.Sender;
					this.Receiver = registerPackage.Receiver;
					this.sendTime = registerPackage.sendTime;
					this.MessageType = registerPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public RegisterDataPackage(string sender, string receiver, string userID, string password) : base(sender, receiver) {
			MessageType = MESSAGETYPE.K_REGISTER_DATA_PACKAGE;
			this.UserID = userID;
			this.Password = password;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码
		public string UserID { get; set; } //注册用户名
		public string Password { get; set; } //注册密码
	}

	//查询数据包类，Type=11
	[Serializable]
	public class QueryDataPackage : DataPackage {
		public QueryDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				QueryDataPackage queryPackage = formatter.Deserialize(ms) as QueryDataPackage;
				if (queryPackage != null) {
					this.Sender = queryPackage.Sender;
					this.Receiver = queryPackage.Receiver;
					this.sendTime = queryPackage.sendTime;
					this.MessageType = queryPackage.MessageType;
					this.QueryType = queryPackage.QueryType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public QueryDataPackage(string sender, string receiver, QUERYTYPE QueryType) : base(sender, receiver) {
			MessageType = MESSAGETYPE.K_QUERY_DATA_PACKAGE;
			this.QueryType = QueryType;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码
		public QUERYTYPE QueryType { get; set; } //查询类别
		// 1为出发站点查询，2为到达站点查询，3为用户订单查询
		// 11为线路查询，12为列车查询，13为车站查询，14为车站-线路查询，15为列车-车站查询
		// 16为列车乘客查询，17为车票价格查询
	}

	//客户端向服务端发送的查询条件类，Type=12
	[Serializable]
	public class TicketQueryDataPackage : DataPackage {
		public TicketQueryDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				TicketQueryDataPackage queryPackage = formatter.Deserialize(ms) as TicketQueryDataPackage;
				if (queryPackage != null) {
					this.Sender = queryPackage.Sender;
					this.Receiver = queryPackage.Receiver;
					this.sendTime = queryPackage.sendTime;
					this.MessageType = queryPackage.MessageType;
					this.EnterStationNumber = queryPackage.EnterStationNumber;
					this.LeaveStationNumber = queryPackage.LeaveStationNumber;
					this.StartTime = queryPackage.StartTime;
					this.EndTime = queryPackage.EndTime;
				}
			}
		} //构造函数 字节数组转化为数据包
		public TicketQueryDataPackage(string sender, string receiver, int EnterStationNumber,
			int LeaveStationNumber, DateTime StartTime, DateTime EndTime) : base(sender, receiver) {
			MessageType = MESSAGETYPE.K_TICKETQUERY_DATA_PACKAGE;
			this.EnterStationNumber = EnterStationNumber;
			this.LeaveStationNumber = LeaveStationNumber;
			this.StartTime = StartTime;
			this.EndTime = EndTime;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码
		public int EnterStationNumber { get; set; }
		public int LeaveStationNumber { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}
