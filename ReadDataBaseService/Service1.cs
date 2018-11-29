using Services;
using Services.DataBase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Timers;
using CommunicationServers.Sockets;
using ProtocolFamily.tianyuan;
namespace ReadDataBaseService
{
    public partial class Service1 : ServiceBase
    {
        SqlHelper sql = new SqlHelper();
        //SocketServerEx socketServerEx = new SocketServerEx();
        IniHelper ini = new IniHelper(System.AppDomain.CurrentDomain.BaseDirectory + @"\Set.ini");
        Timer timer = new Timer();
        COMMHelper com = new COMMHelper();
        public Service1()
        {
             InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string port = ini.ReadIni("Config", "Port");
            string baudRate = ini.ReadIni("Config", "BaudRate");
            //this.socketServerEx.Listen("6000");
            //this.socketServerEx.NewMessage2Event += SocketServerEx_NewMessage2Event;
            SimpleLogHelper.Instance.WriteLog(LogType.Info,"打开串口：" + com.OpenPort(port, baudRate));
            com.DataReceiveEvent += Com_DataReceiveEvent;
            string Interval = ini.ReadIni("Config", "Interval");
            timer.Interval = int.Parse(Interval);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Com_DataReceiveEvent(object sender, COMMEventArgs e)
        {
            if(e.BackDataAsHex.Length == 16)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "有数据请求：" + e.BackDataAsHex);
                //发送modbus
                var a = ini.ReadIni("Config", "Name").Split(',');
                string b = "";
                foreach (var item in a)
                {
                    b += ini.ReadIni("Config", item);
                }
                string length = (a.Length * 4).ToString("X2");
                b = "0103" + length + b;
                b += CRC.ToModbusCRC16(b);
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "返回的数据：" + b);
                CharacterConversion ch = new CharacterConversion();
                byte[] c = ch.HexConvertToByte(b);
                com.Send(c);
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                this.ReadDataBase();
                if(com.ComState == false)
                {
                    com.ClosePort();
                    string port = ini.ReadIni("Config", "Port");
                    string baudRate = ini.ReadIni("Config", "BaudRate");
                    SimpleLogHelper.Instance.WriteLog(LogType.Info, "打开串口：" + com.OpenPort(port, baudRate));
                }
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, ex);
            }
            
        }

        //private void SocketServerEx_NewMessage2Event(System.Net.IPEndPoint remoteIpEndPoint, string Message)
        //{
        //    SimpleLogHelper.Instance.WriteLog(LogType.Info, remoteIpEndPoint);
        //    SimpleLogHelper.Instance.WriteLog(LogType.Info, Message);
        //    //发送modbus
        //    var a = ini.ReadIni("Config", "Name").Split(',');
        //    string b = "";
        //    foreach(var item in a)
        //    {
        //        b += ini.ReadIni("Config", item);
        //    }
        //    string length = (a.Length * 4).ToString("X2");
        //    b = "0103" + length + b;
        //    b += CRC.ToModbusCRC16(b);
        //    SimpleLogHelper.Instance.WriteLog(LogType.Info, b);
        //    CharacterConversion ch = new CharacterConversion();
        //    byte[] c = ch.HexConvertToByte(b);
        //    socketServerEx.Send(remoteIpEndPoint, c);
        //}

        private void ReadDataBase()
        {
            try
            {
                sql.Open();
                SimpleLogHelper.Instance.WriteLog(LogType.Info,"与数据库连接：" + sql.TestConn);
                if(sql.TestConn)
                {
                    var a = ini.ReadIni("Config", "Name").Split(',');
                    foreach(var item in a)
                    {
                        string b = string.Format("SELECT SUM(Net) AS 'Net' from Trade WHERE Product='{0}' and Taretime>= '2018-11-01 00:00:00'", item);
                        List<TradeModel> tradeModels = sql.GetDataTable<TradeModel>(b);
                        if(tradeModels != null)
                        {
                            if(tradeModels[0].Net!=null)
                            {
                                b = MathHelper.SingleToHex(tradeModels[0].Net);
                            }
                            else
                            {
                                b = "00000000";
                            }
                        }
                        else
                        {
                            b = "00000000";
                        }
                        SimpleLogHelper.Instance.WriteLog(LogType.Info, item + ":" + b);
                        ini.WriteIni("Config",item, b);
                    }
                    sql.Close();
                }
            }
            catch(Exception ex)
            {
                SimpleLogHelper.Instance.WriteLog(LogType.Info, "从数据库读取数据失败");
                SimpleLogHelper.Instance.WriteLog(LogType.Error, ex);
                sql.Close();
            }
        }

        protected override void OnStop()
        {
            try
            {
                sql.Close();
                com.ClosePort();
                //socketServerEx.Disconnect();
            }
            catch
            {

            }
        }
    }
}
