using System;
using System.Collections.Generic;
using System.Data;
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
        K_TICKETQUERY_DATA_PACKAGE,  //客户端向服务端发送的查询条件类
		K_USER_INFO_CHANGE, //用户信息修改数据包类
		K_USER_OPERATION_PACKAGE, //管理员查询用户信息包
		K_DATASET_PACKAGE, //表类型包
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
        K_TICKET_PRICE,  //车票价格查询
		K_BUYTICKET_QUERY, // //购买车票查询
		K_TICKETINFO_QUERY, // 拆分后的购买
		K_USER_INFO //用户详情信息
    }

	//数据包类，Type=K_DATA_PACKAGE
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
		public DataPackage(string sender, string IPandPort, string receiver) {
			this.Sender = sender;
			this.Receiver = receiver;
			this.IPandPort = IPandPort;
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
					this.IPandPort = dataPackage.IPandPort;
					this.Sender = dataPackage.Sender;
					this.Receiver = dataPackage.Receiver;
					this.sendTime = dataPackage.sendTime;
				}
            }
        } //构造函数 字节数组转化为数据包

        public DateTime sendTime { get; set; } //消息的发送时间
		public string Sender { get; set; } //发送者的ID
		public string IPandPort { get; set; } //发送者的IP和端口
		public string Receiver { get; set; } //接收者的ID
		public MESSAGETYPE MessageType = MESSAGETYPE.K_DATA_PACKAGE; //数据包类Type为0
	}

	//登入数据包类，Type=K_LOGIN_DATA_PACKAGE
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
					this.IPandPort = loginDataPackage.IPandPort;
					this.MessageType = loginDataPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public LoginDataPackage(string sender, string IPandPort, string receiver, string userID, 
			string password) : base(sender, IPandPort, receiver) {
			MessageType = MESSAGETYPE.K_LOGIN_DATA_PACKAGE;
			this.UserID = userID;
			this.Password = password;
		} //构造函数 接受发送者,接收者字符串,登录用户名与密码
		public string UserID { get; set; } //登录用户名
		public string Password { get; set; } //登录密码
	}

	//注册数据包类，Type=K_REGISTER_DATA_PACKAGE
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
					this.IPandPort = registerPackage.IPandPort;
					this.MessageType = registerPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public RegisterDataPackage(string sender, string IPandPort, string receiver, string userID,
			string password) : base(sender, IPandPort, receiver) {
			MessageType = MESSAGETYPE.K_REGISTER_DATA_PACKAGE;
			this.UserID = userID;
			this.Password = password;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码
		public string UserID { get; set; } //注册用户名
		public string Password { get; set; } //注册密码
	}

	//更改用户信息数据包类，Type=K_USER_INFO_CHANGE
	[Serializable]
	public class InfoChangeDataPackage : DataPackage {
		public InfoChangeDataPackage(byte[] Bytes) {
			using (MemoryStream ms = new MemoryStream(Bytes)) {
				IFormatter formatter = new BinaryFormatter();
				InfoChangeDataPackage infoChangeDataPackage = formatter.Deserialize(ms) as InfoChangeDataPackage;
				if (infoChangeDataPackage != null) {
					this.ChangeType = infoChangeDataPackage.ChangeType;
					this.ChangeValue = infoChangeDataPackage.ChangeValue;
					this.Sender = infoChangeDataPackage.Sender;
					this.Receiver = infoChangeDataPackage.Receiver;
					this.sendTime = infoChangeDataPackage.sendTime;
					this.IPandPort = infoChangeDataPackage.IPandPort;
					this.MessageType = infoChangeDataPackage.MessageType;
				}
			}
		} //构造函数 字节数组转化为数据包
		public InfoChangeDataPackage(string sender, string IPandPort, string receiver, int ChangeType,
			string ChangeValue) : base(sender, IPandPort, receiver) {
			MessageType = MESSAGETYPE.K_USER_INFO_CHANGE;
			this.ChangeType = ChangeType;
			this.ChangeValue = ChangeValue;
		} //构造函数 接受发送者,接收者字符串,登录用户名与密码
		public int ChangeType { get; set; } //更改类型，1为更改用户昵称，2为更改手机号码
		public string ChangeValue { get; set; } //更改字段
	}

	//查询数据包类，Type=K_QUERY_DATA_PACKAGE
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
					this.IPandPort = queryPackage.IPandPort;
					this.QueryType = queryPackage.QueryType;
					this.ExtraMsg = queryPackage.ExtraMsg;
				}
			}
		} //构造函数 字节数组转化为数据包
		public QueryDataPackage(string sender, string IPandPort, string receiver, QUERYTYPE QueryType, string ExtraMsg) : base(sender, IPandPort, receiver) {
			MessageType = MESSAGETYPE.K_QUERY_DATA_PACKAGE;
			this.QueryType = QueryType;
			this.ExtraMsg = ExtraMsg;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码
		public QUERYTYPE QueryType { get; set; } //查询类别
		public string ExtraMsg { get; set; } //额外信息
		// 1为出发站点查询，2为到达站点查询，3为用户订单查询
		// 11为线路查询，12为列车查询，13为车站查询，14为车站-线路查询，15为列车-车站查询
		// 16为列车乘客查询，17为车票价格查询
	}

	//客户端向服务端发送的查询条件类，Type=K_TICKETQUERY_DATA_PACKAGE
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
					this.IPandPort = queryPackage.IPandPort;
				}
			}
		} //构造函数 字节数组转化为数据包
		public TicketQueryDataPackage(string sender, string IPandPort, string receiver, int EnterStationNumber,
			int LeaveStationNumber, DateTime StartTime, DateTime EndTime) : base(sender, IPandPort, receiver) {
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


	//客户端向服务端发送的查询条件类，Type=K_TICKETQUERY_DATA_PACKAGE
	[Serializable]
	public class UserOperationPackage : DataPackage
	{
		public UserOperationPackage(byte[] Bytes)
		{
			using (MemoryStream ms = new MemoryStream(Bytes))
			{
				IFormatter formatter = new BinaryFormatter();
				UserOperationPackage package = formatter.Deserialize(ms) as UserOperationPackage;
				if (package != null)
				{
					this.Sender = package.Sender;
					this.Receiver = package.Receiver;
					this.sendTime = package.sendTime;
					this.MessageType = package.MessageType;
					this.opType = package.opType;
					this.UserID = package.UserID;
					this.Accounttype = package.Accounttype;
					this.Phone = package.Phone;
					this.Username = package.Username;
					this.IPandPort = package.IPandPort;
					this.Balance = package.Balance;
				}
			}
		} //构造函数 字节数组转化为数据包

		public UserOperationPackage(string sender, string IPandPort, string receiver,
			Enum_USER_OP opType,
			string UserID, string Accounttype, string phone, string username, float balance) : base(sender, IPandPort, receiver)
		{
			MessageType = MESSAGETYPE.K_USER_OPERATION_PACKAGE;
			this.opType = opType;
			this.UserID = UserID;
			this.Accounttype = Accounttype;
			this.Phone = phone;
			this.Username = username;
			this.Balance = balance;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码

		public enum Enum_USER_OP
        {
			K_QUERY,
			K_MODIFY,
			K_DELETE
        }

		public Enum_USER_OP opType { get; set; }
		public string UserID { get; set; }
		public string Accounttype { get; set; }
		public string Phone { get; set; }
		public string Username { get; set; }
		public float Balance { get; set; }
	}


	[Serializable]
	public class DataSetPackage : DataPackage
	{
		public DataSetPackage(byte[] Bytes)
		{
			using (MemoryStream ms = new MemoryStream(Bytes))
			{
				IFormatter formatter = new BinaryFormatter();
				DataSetPackage package = formatter.Deserialize(ms) as DataSetPackage;
				if (package != null)
				{
					this.Sender = package.Sender;
					this.Receiver = package.Receiver;
					this.sendTime = package.sendTime;
					this.forbid = package.forbid;
					this.row = package.row;
					this.col = package.col;
					this.dataSet = package.dataSet;
				}
			}
		} //构造函数 字节数组转化为数据包

		public DataSetPackage(string sender, string IPandPort, string receiver,
			int forbid, int row, int col, DataSet dataSet) : base(sender, IPandPort, receiver)
		{
			MessageType = MESSAGETYPE.K_DATASET_PACKAGE;
			this.forbid = forbid;
			this.row = row;
			this.col = col;
			this.dataSet = dataSet;
		} //构造函数 接受发送者,接收者字符串,注册用户名与注册密码

		public int forbid { get; set; }
		public int row { get; set; }
		public int col { get; set; }
		public DataSet dataSet { get; set; }
	}
}
