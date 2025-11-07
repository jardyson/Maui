using DeviceHelper.Connectors.TCPIP;
using Global.Interface;
using GlobalDeclar;
using Modbus.Device;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static log4net.Appender.FileAppender;

namespace DeviceHelper.Connectors.Melsec
{
    /// <summary>
    /// 三菱FX5U PLC二进制协议通信类
    /// 基于MC协议二进制格式实现
    /// </summary>
    public class HSLConnector : BaseModbus<Socket, int, int>, IDisposable
    {
        private bool _isConnected;
        private const int Timeout = 5000; // 通信超时时间(ms)
        /// <summary>
        /// 是否已连接
        /// </summary>
        public override bool IsConnected => this.modbusClient == null ?false: this.modbusClient.Connected;
        public override bool IsActive { get; set; } = true;

        public override DeviceVisiterProtocalEnum VisiterProtocalType { get; set; } = DeviceVisiterProtocalEnum.ModbusTcp;
        public override DevType devType { get => base.devType; set => base.devType = value; }
        public bool IsListening { get; set; }
        public EventHandler onConnectedHandle { get; set; }
        public EventHandler onDisconnectHandle { get; set; }
        public Func<object, string> onMsgHandle { get; set; }
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        public EventHandler onSendHandle { get; set; }
        IPEndPoint endPoint;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ipAddress">PLC的IP地址</param>
        /// <param name="port">通信端口，默认5006</param>
        public HSLConnector(string code, string name, string ip, int port)
        {
            this.code = code;
            this.Name = name;
            this.IP = IPAddress.Parse(ip);
            this.Port = port;
            endPoint = new IPEndPoint(this.IP, this.Port);
        }
        /// <summary>
        /// 连接到PLC
        /// </summary>
        /// <returns>连接是否成功</returns>
        public override bool Connect()
        {
            try
            {
                this.modbusClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.modbusClient.Connect(endPoint);
            }
            catch (Exception exc)
            {
                throw exc;
            }

            return modbusClient.Connected;
        }
        /// <summary>
        /// 从PLC断开连接
        /// </summary>
        public override bool Disconnect()
        {
            try
            {
                if (this.modbusClient!=null && this.modbusClient.Connected)
                    this.modbusClient?.Disconnect(true);
            }
            finally
            {
                //modbusClient = null;
            }

            return true;
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            Disconnect();
            GC.SuppressFinalize(this);
        }

        private void ModbusClient_SendDataChanged(object sender)
        {
            onSendHandle?.Invoke(sender, null);
        }

        private void ModbusClient_ReceiveDataChanged(object sender)
        {
            onMsgHandle?.Invoke($"{this.IP}:{this.Port} received{sender.ToString()}");
        }

        private void ModbusClient_ConnectedChanged(object sender)
        {
            onConnectedHandle?.Invoke($"{this.IP}:{this.Port}{(this.IsConnected ? "已连接" : "已断开")}", null);
        }

        public override int[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                {
                    modbusClient.Send(BuildCommand(startAddress));
                    var receiveBuffer = new byte[1024];
                    int count = modbusClient.Receive(receiveBuffer);
                    var a = BitConverter.ToString(receiveBuffer).Replace("-", "").Substring(0,40);
                    // 解析返回数据（跳过前11字节头部）
                    for (int i = 11; i < count; i += 2)
                    {
                        // 注意：Modbus是大端序，可能需要反转字节
                        byte[] temp = new byte[] { receiveBuffer[i], receiveBuffer[i + 1] };
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(temp);  // 反转字节顺序以适应小端系统
                        }
                        int value = BitConverter.ToInt16(temp, 0);
                        return new int[] { value };
                    }
                    return new int[] { 0 };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        byte[] BuildCommand(int address)
        {
            //         | 字节范围   | 内容 | 说明 |
            //| -----  | ---------- | ----------------------------------------------------                  |
            //| 0–1   | `50 00`    | 子头（固定：表示使用 3E 帧 TCP 通信）                                 |
            //| 2      | `00`       | 网络号（一般为 0）                                                    |
            //| 3      | `FF`       | PLC 号（通常设为 255 表示无指定）                                     |
            //| 4      | `FF`       | I / O 编号低字节（默认）                                              |
            //| 5      | `03`       | I / O 编号高字节（固定）                                              |
            //| 6–7   | `00 0C`    | 请求数据长度（后续的数据长度，从字节 9 开始算起），0x000C = 12 字节   |
            //| 8–9   | `00 0A`    | CPU 监视定时器（以 250ms 为单位，0x000A = 10 × 250ms = 2.5s 超时）   |
            //| 10–11 | `01 04`    | 命令码：`0x0401`，表示“批量读取”                                    |
            //| 12–13 | `00 00`    | 子命令：`0x0000`，表示读取“字单位”                                  |
            //| 14–16 | `ED 07 00` | 起始地址：`0x07ED` = 十进制 * *2029 * *（小端格式）                   |
            //| 17     | `A8`       | 设备代码：`A8` 表示** D 寄存器（字地址）**                            |
            //| 18–19 | `01 00`    | 读取点数：`0x0001` = 1 个字                                           |
            //| 20     | 无         | 此数据包至此为止                                                      |
            var bits = BitConverter.GetBytes(address);
            var sendBuffer = new byte[]
                {
                    0x50, 0x00, 
                    0x00, 0xFF,
                    0xFF, 0x03,
                    0x00, 0x0C,
                    0x00, 0x10,
                    0x00, 0x01,
                    0x04, 0x00,
                    0x00, 
                    0xA0, 0x1A, 0x00, 0x00, //字址，两个字节的长度
                    0x01, 00
                };

            // 设置数据长度字段（从第9字节起，单位字节，不含前面8字节）
            ushort dataLength = (ushort)(sendBuffer.Length - 9);
            sendBuffer[7] = (byte)(dataLength >> 8 & 0xFF);
            sendBuffer[6] = (byte)(dataLength & 0xFF);

            return sendBuffer;
        }

        /// <summary>
        /// 三菱MC协议
        /// </summary>
        public class Mitsubishi_TCP
        {
            IPAddress IP;
            IPEndPoint Host;
            public Socket client;
            string str_ip;
            string str_port;
            bool _isconnected = false;

            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="ip"></param>
            /// <param name="port"></param>
            public void sClient(string ip, string port)
            {
                str_ip = ip;
                str_port = port;
            }
            /// <summary>
            /// 连接
            /// </summary>
            /// <returns></returns>
            public bool Connect()
            {
                try
                {
                    IP = IPAddress.Parse(str_ip);
                    Host = new IPEndPoint(IP, Convert.ToInt32(str_port));
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect(Host);
                    _isconnected = true;
                    return true;
                }
                catch
                {
                    _isconnected = false;
                    return false;
                }
            }
            /// <summary>
            /// 判断是否连接
            /// </summary>
            public bool IsConnected
            {
                get
                {
                    return _isconnected;
                }
            }
            /// <summary>
            /// 接收数据
            /// </summary>
            /// <param name="s"></param>
            /// <returns></returns>
            public string Recieve()
            {
                lock (client)
                {
                    try
                    {
                        if (client != null && client.Connected)
                        {
                            _isconnected = true;
                            if (client.Available > 0)
                            {
                                byte[] data = new byte[client.Available];
                                client.Receive(data);
                                return byteToHexStr(data);
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            _isconnected = false;
                            return "";
                        }
                    }
                    catch
                    {
                        _isconnected = false;
                        return "";
                    }
                }
            }

            /// <summary>
            /// 发送数据
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public bool Send(string str)
            {
                lock (client)
                {
                    try
                    {
                        if (client != null && client.Connected)
                        {
                            _isconnected = true;
                            byte[] data = Hex_StrToByte(str);
                            client.Send(data);
                            ReSleep(20);
                            return true;
                        }
                        else
                        {
                            _isconnected = false;
                            return false;
                        }
                    }
                    catch
                    {
                        //MessageBox.Show(e.Message);
                        _isconnected = false;
                        return false;
                    }
                }
            }

            public bool StopDispose()
            {
                try
                {
                    Dispose();
                    return true;
                }
                catch
                {
                    _isconnected = false;
                    return false;
                }
            }

            public void Dispose()
            {
                if (client != null)
                {
                    try
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Disconnect(true);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(ex.Message);
                    }
                    client.Close();
                    _isconnected = false;
                }
            }

            public string IP_2(string ip)
            {
                string[] str = ip.Split('.');
                if (str.Length == 4)
                {
                    return DecToHex_Str_2(Convert.ToInt16(str[3]));
                }
                else
                {
                    return "";
                }
            }

            private string DealBarcode(string str1)
            {
                byte[] Redata = Hex_StrToByte(str1);
                string str = bytetoStr(Redata);
                return str.Trim();
            }

            /// <summary>
            /// 读取条形码
            /// </summary>
            /// <returns></returns>
            public string ReadBarcode()
            {
                string barcode = string.Empty;
                lock (client)
                {
                    try
                    {
                        string str = "500000FFFF03000C00100001040000361000A80A00";
                        if (Send(str))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 8 && str1.Contains("D00000FFFF030016"))
                            {
                                string str2 = str1.Substring(str1.Length - 40, 40);
                                //return Convert.ToInt32(str2, 16);
                                barcode = DealBarcode(str2);
                                return barcode;
                            }
                            else
                            {
                                return "";
                            }
                        }
                        else
                        {
                            Recieve();
                            return "";
                        }
                    }
                    catch
                    {
                        return "";
                    }
                }
                //return barcode;
            }

            private Int16[] StrToDec(string str, int count)
            {
                Int16[] data = new Int16[count];
                for (int i = 0; i < count; i++)
                {
                    int j = i * 4;
                    string dstr = str.Substring(j + 2, 2) + str.Substring(j, 2);
                    data[i] = Convert.ToInt16(dstr, 16);
                }
                return data;
            }

            public Int16 ReadPLC_D(int address)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040000";///500000FFFF03000C00100001040000 3610 00A8 0A00
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A8";
                        string Count = DecToHex_Str_LH4(1);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4(4)))
                            {
                                string str2 = str1.Substring(str1.Length - 4, 4);
                                short[] sh = StrToDec(str2, 1);
                                return sh[0];
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            Recieve();
                            return -1;
                        }
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }

            public Int16[] ReadPLC_D(int address, Int16 count)
            {
                lock (client)
                {
                    try
                    {
                        if (count == 0)
                            return null;

                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040000";///500000FFFF03000C00100001040000 3610 00A8 0A00
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A8";
                        string Count = DecToHex_Str_LH4(count);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4((Int16)(2 + count * 2))))
                            {
                                string str2 = str1.Substring(str1.Length - 4 * count, 4 * count);
                                return StrToDec(str2, count);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Recieve();
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            private Int16[] StrToDecM(string str, int count)
            {
                Int16[] data = new Int16[count];
                for (int i = 0; i < count; i++)
                {
                    string dstr = str.Substring(i, 1);
                    data[i] = Convert.ToInt16(dstr, 16);
                }
                return data;
            }

            public Int16 ReadPLC_M(int address)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "90";
                        string Count = DecToHex_Str_LH4(1);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4(3)))
                            {
                                string str2 = str1.Substring(str1.Length - 2, 1);
                                return Convert.ToInt16(str2);
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            Recieve();
                            return -1;
                        }
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }

            public Int16[] ReadPLC_M(int address, Int16 count)
            {
                lock (client)
                {
                    try
                    {
                        if (count == 0)
                            return null;

                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "90";
                        string Count = DecToHex_Str_LH4(count);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4((Int16)(2 + Math.Ceiling(count / 2.0)))))
                            {
                                string str2 = str1.Substring(str1.Length - count - 1, count);
                                return StrToDecM(str2, count);
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Recieve();
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            public Int16[] ReadPLC_B(int address, Int16 count)
            {
                lock (client)
                {
                    try
                    {
                        if (count == 0)
                            return null;

                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A0";
                        string Count = DecToHex_Str_LH4(count);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {

                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4((Int16)(2 + Math.Ceiling(count / 2.0)))))
                            {
                                if ((count % 2.0) == 0)
                                {
                                    string str2 = str1.Substring(str1.Length - count, count);
                                    return StrToDecM(str2, count);
                                }
                                else
                                {
                                    string str2 = str1.Substring(str1.Length - count - 1, count);
                                    return StrToDecM(str2, count);
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            Recieve();
                            return null;
                        }

                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public Int16 ReadPLC_B(int address)//16
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部

                        string Head = "500000FFFF03000C00100001040100";
                        string Address = DecToHex_Str_LH6(address);//100000
                        string Command = "A0";
                        string Count = DecToHex_Str_LH4(1);//0100
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {

                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4(3)))
                            {
                                string str2 = str1.Substring(str1.Length - 2, 1);
                                Int16 re = Convert.ToInt16(str2);
                                return re;
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            Recieve();
                            return -1;
                        }
                    }
                    catch
                    {
                        return -1;
                    }
                }
            }
            public void ReSleep(int cc)
            {
                for (int i = 0; i < cc; i++)
                {
                    if (client.Available > 0)
                    {
                        Thread.Sleep(10);
                        break;
                    }
                    Thread.Sleep(10);
                }
            }

            private string DECToLHstr(Int16[] data)
            {
                string str = "";
                for (int i = 0; i < data.Length; i++)
                {
                    str += DecToHex_Str_LH4(data[i]);
                }
                return str;
            }

            public bool WritePLC_D(Int32 address, Int16 data)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";
                        string Head2 = DecToHex_Str_LH4(14);
                        string Head3 = "010001140000";//"100001140000";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A8";
                        string Count = DecToHex_Str_LH4(1);
                        string dataHex = DecToHex_Str_LH4(data);
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            public bool WritePLC_D(Int32 address, Int16[] data)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";
                        string Head2 = DecToHex_Str_LH4((Int16)(12 + 2 * data.Length));
                        string Head3 = "100001140000";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A8";
                        string Count = DecToHex_Str_LH4((Int16)data.Length);
                        string dataHex = DECToLHstr(data);
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            public Int32 ReadPLC_DD(int address)
            {
                lock (client)
                {
                    try
                    {

                        //QnA兼容3E的二进制通信代码头部
                        string Head = "500000FFFF03000C00100001040000";///500000FFFF03000C00100001040000 3610 00A8 0A00
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A8";
                        string Count = DecToHex_Str_LH4(2);
                        string sendstr = Head + Address + Command + Count;

                        if (Send(sendstr))
                        {

                            string str1 = Recieve();
                            if (str1.Length > 14 && str1.Contains("D00000FFFF0300" + DecToHex_Str_LH4((Int16)(2 + 2 * 2))))
                            {
                                string str2 = str1.Substring(str1.Length - 4 * 2, 4 * 2);
                                return StrToDecDD(str2, 1);
                            }
                            else
                            {
                                return -1;
                            }
                        }
                        else
                        {
                            Recieve();
                            return -1;
                        }

                    }
                    catch
                    {
                        return -1;
                    }
                }

            }
            private Int32 StrToDecDD(string str, int count)
            {
                Int32 data = new Int32();
                for (int i = 0; i < count; i++)
                {
                    int j = i * 8;
                    string dstr = str.Substring(j + 2, 2) + str.Substring(j, 2);
                    string dstr2 = str.Substring(j + 6, 2) + str.Substring(j + 4, 2);
                    data = Convert.ToInt32(dstr2 + dstr, 16);
                }
                return data;
            }

            private string DECToLHstrM(Int16[] data)
            {
                string str = "";
                for (int i = 0; i < data.Length; i++)
                {
                    if (data[i] == 0)
                    {
                        str += "0";
                    }
                    else
                    {
                        str += "1";
                    }
                }
                if (data.Length % 2 == 1)
                {
                    str += "0";
                }
                return str;
            }

            public bool WritePLC_M(Int32 address, Int16 data)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";
                        string Head2 = DecToHex_Str_LH4(13);
                        string Head3 = "100001140100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "90";
                        string Count = DecToHex_Str_LH4(1);
                        string dataHex = "";// DECToLHstrM(data);
                        if (data == 1)
                            dataHex = "10";
                        else
                            dataHex = "00";
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            public bool WritePLC_M(Int32 address, Int16[] data)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";
                        string Head2 = DecToHex_Str_LH4((Int16)(12 + (Int16)Math.Ceiling(data.Length / 2.0)));
                        string Head3 = "100001140100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "90";
                        string Count = DecToHex_Str_LH4((Int16)data.Length);
                        string dataHex = DECToLHstrM(data);
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {
                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            public bool WritePLC_B(Int32 address, Int16[] data)
            {
                lock (client)
                {
                    try
                    {
                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";
                        string Head2 = DecToHex_Str_LH4((Int16)(12 + (Int16)Math.Ceiling(data.Length / 2.0)));
                        string Head3 = "100001140100";
                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A0";
                        string Count = DecToHex_Str_LH4((Int16)data.Length);
                        string dataHex = DECToLHstrM(data);
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {

                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;

                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }

                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            public bool WritePLC_B(Int32 address, Int16 data)//272 0
            {
                lock (client)
                {
                    try
                    {

                        //QnA兼容3E的二进制通信代码头部"500000FFFF03000E00100001140000241000A801000200";
                        string Head1 = "500000FFFF0300";

                        string Head2 = DecToHex_Str_LH4(13);
                        string Head3 = "100001140100";

                        string Address = DecToHex_Str_LH6(address);
                        string Command = "A0";
                        string Count = DecToHex_Str_LH4(1);
                        string dataHex = "";// DECToLHstrM(data);
                        if (data == 1)
                            dataHex = "10";
                        else
                            dataHex = "00";
                        string sendstr = Head1 + Head2 + Head3 + Address + Command + Count + dataHex;
                        if (Send(sendstr))
                        {

                            string str1 = Recieve();
                            if (str1.Length > 11 && str1.Contains("D00000FFFF030002000000"))
                            {
                                return true;

                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            Recieve();
                            return false;
                        }

                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            /// <summary>
            /// 字符串转化为十六进制字符串
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public string StrToHex_Str(string str)
            {
                byte[] data = Encoding.ASCII.GetBytes(str);
                string value = string.Empty;
                for (int i = 0; i < data.Length; i++)
                {
                    value += data[i].ToString("X2");
                }
                return value;
            }

            /// <summary>
            /// 十六进制字符串转化为字节数组
            /// </summary>
            /// <param name="Hex_Str"></param>
            /// <returns></returns>
            public byte[] Hex_StrToByte(string Hex_Str)
            {
                Hex_Str = Hex_Str.Replace(" ", "");
                if (Hex_Str.Length % 2 != 0)
                    Hex_Str += " ";
                byte[] returnBytes = new byte[Hex_Str.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                    returnBytes[i] = Convert.ToByte(Hex_Str.Substring(i * 2, 2), 16);
                return returnBytes;
            }

            /// <summary> 
            /// 字节数组转16进制字符串 
            /// </summary> 
            /// <param name="bytes"></param> 
            /// <returns></returns> 
            public string byteToHexStr(byte[] bytes)
            {
                string returnStr = "";
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        returnStr += bytes[i].ToString("X2");
                    }
                }
                return returnStr;
            }

            //十六进制相加和为十六进制
            public string Hex_StrAddToHex_Str(string Hex_Str)
            {
                byte[] data = Hex_StrToByte(Hex_Str);
                Int32 sum = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    sum += Convert.ToInt32(data[i]);
                }
                return Convert.ToString(sum, 16);
            }

            public string BCC(string Hex_Str)
            {
                string str = Hex_StrAddToHex_Str(Hex_Str);
                string value = string.Empty;
                value = str.PadLeft(4, '0').Substring(2, 2) + str.PadLeft(4, '0').Substring(0, 2);
                return value;
            }

            /// <summary>
            /// 字节数组转换为字符串
            /// </summary>
            /// <param name="data"></param>
            /// <returns></returns>
            public string bytetoStr(byte[] data)
            {
                string str = Encoding.ASCII.GetString(data);
                return str;
            }

            //十进制转为十六进制2位表示
            public string DecToHex_Str_2(int data)
            {
                string str = Convert.ToString(data, 16).ToUpper();
                if (str.Length == 1)
                    str = "0" + str;
                return str.Substring(0, 2);
            }

            //十进制转为十六进制六位表示
            public string DecToHex_Str_6(Int32 data)
            {
                string str = Convert.ToString(data, 16).ToUpper();
                switch (str.Length)
                {
                    case 1:
                        str = "00000" + str;
                        break;
                    case 2:
                        str = "0000" + str;
                        break;
                    case 3:
                        str = "000" + str;
                        break;
                    case 4:
                        str = "00" + str;
                        break;
                    default:
                        break;
                }

                return str;
            }

            public string DecToHex_Str_LH6(Int32 data)
            {
                string str = DecToHex_Str_6(data);

                return str.Substring(4, 2) + str.Substring(2, 2) + str.Substring(0, 2);
            }

            //十进制转为十六进制六位表示
            public string DecToHex_Str_8(Int32 data)
            {
                string str = Convert.ToString(data, 16).ToUpper();
                switch (str.Length)
                {
                    case 1:
                        str = "0000000" + str;
                        break;
                    case 2:
                        str = "000000" + str;
                        break;
                    case 3:
                        str = "00000" + str;
                        break;
                    case 4:
                        str = "0000" + str;
                        break;
                    case 5:
                        str = "000" + str;
                        break;
                    case 6:
                        str = "00" + str;
                        break;
                    case 7:
                        str = "0" + str;
                        break;
                    default:
                        break;
                }
                string str1 = str.Substring(4, 4) + str.Substring(0, 4);
                return str1;
            }

            //十进制转为十六进制4位表示
            public string DecToHex_Str_4(Int16 data)
            {
                string str = Convert.ToString(data, 16).ToUpper();
                switch (str.Length)
                {
                    case 1:
                        str = "000" + str;
                        break;
                    case 2:
                        str = "00" + str;
                        break;
                    case 3:
                        str = "0" + str;
                        break;

                    default:
                        break;
                }
                string str1 = str.Substring(0, 4);
                return str1;
            }

            public string DecToHex_Str_LH4(Int16 data)
            {
                string str = DecToHex_Str_4(data);
                return str.Substring(2, 2) + str.Substring(0, 2);
            }

            //十进制转化为八进制
            public string DecToOct_Str(Int32 data)
            {
                return Convert.ToString(data, 8);
            }

            //十进制转化为二进制
            public string DecToBin_Str(Int32 data)
            {
                return Convert.ToString(data, 2);
            }
            //十六进制转化为十进制
            public Int32 Hex_StrToDec(string Hex_Str)
            {
                return Convert.ToInt32(Hex_Str, 16);
            }

            //八进制转化为十进制
            public Int32 Oct_StrToDec(string Oct_Str)
            {
                return Convert.ToInt32(Oct_Str, 16);
            }

            //二进制转化为十进制
            public Int32 Bin_StrToDec(string Bin_Str)
            {
                return Convert.ToInt32(Bin_Str, 16);
            }

            /// <summary>
            /// FCS中的异或校验并返回十六进制字符串
            /// </summary>
            /// <param name="str"></param>
            /// <returns></returns>
            public string FCS(string str)
            {
                int i = str.Length;
                string result = string.Empty;
                int A = 0;
                foreach (char c in str)
                {
                    A = A ^ Convert.ToUInt16(c);
                }
                result = DecToHex_Str_2(A);
                return result;
            }
        }
    }
}