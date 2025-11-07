using Global.Interface;
using GlobalDeclar;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceHelper
{
    public class Mitsubishi_TCP : BaseModbus<Socket, int, int>, IDeviceVisiterModbusFuncs, IDisposable
    {
        IPEndPoint endPoint;
        public override DeviceVisiterProtocalEnum VisiterProtocalType => DeviceVisiterProtocalEnum.ModbusTcp;
        public override DevType devType { get; set; } = DevType.Melsecs;
        public EventHandler onConnectedHandle { get; set; }
        public EventHandler onDisconnectHandle { get; set; }
        public Func<object, string> onMsgHandle { get; set; }
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        public EventHandler onSendHandle { get; set; }
        public override bool IsActive { get; set; } = true;

        public Mitsubishi_TCP(string code, string name, string ip, int port)
        {
            this.code = code;
            this.Name = name;
            this.IP = IPAddress.Parse(ip);
            this.Port = port;
            endPoint = new IPEndPoint(this.IP, this.Port);
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public override bool Connect()
        {
            try
            {
                modbusClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                modbusClient.Connect(endPoint);
                return true;
            }
            catch (Exception exc)
            {
                throw exc;
            }
        }
        /// <summary>
        /// 判断是否连接
        /// </summary>
        public override bool IsConnected => modbusClient?.Connected ?? false;
        
        public override int[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                {
                    return new int[] { int.Parse(ReadPLC_D(startAddress).ToString()) };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override bool WriteMultipleRegisters(int startAddress, int[] data)
        {
            lock (rwLock)
            {
                var s = new short[data.Length];
                for (global::System.Int32 i = 0; i < data.Length; i++)
                    s[i] = Convert.ToInt16(data[i]);

                return WritePLC_D(startAddress, s);
            }
        }

        /// <summary>
        /// 读数据
        /// </summary>
        /// <param name="functionreadcode"></param>
        /// <param name="modbus"></param>
        /// <param name="startaddr"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public override int[] Read(ModbusReadEnum ReadFunction, int startAddress, int length)
        {
            int[] rtn = new int[length];
            lock (rwLock)
                switch (ReadFunction)
                {
                    case ModbusReadEnum.ReadCoils:
                        bool[] items = new bool[length];

                        items = ReadCoils(startAddress, (UInt16)(items.Length));
                        if (items == null)
                            return null;
                        for (global::System.Int32 i = 0; i < items.Length; i++)
                            rtn[i] = (int)Convert.ChangeType(items[i] ? 1 : 0, typeof(int));

                        return rtn;
                    case ModbusReadEnum.ReadInputs:
                        bool[] items1 = ReadInputs(startAddress, (UInt16)length);
                        if (items1 == null)
                            return null;
                        for (global::System.Int32 i = 0; i < items1.Length; i++)
                            rtn[i] = (int)Convert.ChangeType(items1[i] ? 1 : 0, typeof(int));
                        break;
                    case ModbusReadEnum.ReadHoldingRegisters:
                        rtn = ReadHoldingRegisters(startAddress, (UInt16)length);
                        break;
                    case ModbusReadEnum.ReadInputRegisters:
                        rtn = ReadInputRegisters(startAddress, (UInt16)length);
                        break;
                    default:
                        break;
                }

            return rtn;
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

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string Recieve()
        {
            lock (rwLock)
            {
                if (modbusClient != null && modbusClient.Connected)
                {
                    if (modbusClient.Available > 0)
                    {
                        byte[] data = new byte[modbusClient.Available];
                        modbusClient.Receive(data);
                        return byteToHexStr(data);
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public bool Send(string str)
        {
            lock (rwLock)
            {
                try
                {
                    if (modbusClient != null && modbusClient.Connected)
                    {
                        byte[] data = Hex_StrToByte(str);
                        modbusClient.Send(data);
                        ReSleep(20);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
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
                return false;
            }
        }

        public void Dispose()
        {
            if (modbusClient != null)
            {
                try
                {
                    modbusClient.Shutdown(SocketShutdown.Both);
                    modbusClient.Disconnect(true);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(ex.Message);
                }
                modbusClient.Close();
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
                if (modbusClient.Available > 0)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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
            lock (modbusClient)
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

        public bool Write(ModbusWriteEnum writeEnum, int startAddress, int[] data)
        {
            return true;
        }
    }
}
