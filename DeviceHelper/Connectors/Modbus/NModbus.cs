using Global.Interface;
using GlobalDeclar;
using Modbus.Device;
using System.Net;
using System.Net.Sockets;

namespace DeviceHelper
{
    //    byte[] message = new byte[8]; // At least 6 bytes + 2 bytes CRC
    //    message[0] = slaveAddress;    // Slave address (1 byte)
    //message[1] = 0x03;             // Function code for read holding registers (1 byte)
    //message[2] = (byte) (startAddress >> 8); // Starting address high byte (1 byte)
    //message[3] = (byte) (startAddress & 0xFF); // Starting address low byte (1 byte)
    //message[4] = (byte) (numRegisters >> 8); // Number of registers high byte (1 byte)
    //message[5] = (byte) (numRegisters & 0xFF); // Number of registers low byte (1 byte)
    // Calculate and add CRC (2 bytes) here using Modbus CRC calculation function

    /// <summary>
    /// Modbus TCP/IP访问器
    /// </summary>
    /// <typeparam name="T">读写类型</typeparam>
    public class NModbus : BaseModbus<ModbusIpMaster, int,int>, IDeviceVisiterModbusFuncs, IDeviceNetwork<int>, IDisposable
        //where T : struct,IConvertible
    {
        public override bool IsConnected => (tcpClient?.Connected ?? false);
        public override bool IsActive { get; set; } = true;

        public new DeviceVisiterProtocalEnum VisiterProtocalType { get; set; } = DeviceVisiterProtocalEnum.ModbusTcp;

        TcpClient tcpClient;

        public NModbus(string code, string name, string ip, int port, int slaveid, DeviceVisiterProtocalEnum visiterProtocalEnum)
        {

            if (string.IsNullOrEmpty(ip) || port == 0)
                return;
            this.code = code;
            this.Name = name;
            this.IP = IPAddress.Parse(ip);
            this.Port = port;
            this.SlaveID = slaveid;


            VisiterProtocalType = visiterProtocalEnum;
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
            onConnectedHandle?.Invoke($"{this.IP}:{this.Port}{(this.tcpClient.Connected ? "已连接" : "已断开")}", null);
        }

        public override bool Connect()
        {
            try
            {
                if (tcpClient != null)
                {
                    tcpClient.Dispose();
                    tcpClient = null;
                }

                tcpClient = new TcpClient();
                tcpClient.Connect(this.IP, this.Port);
                if (modbusClient != null)
                    modbusClient.Dispose();

                modbusClient = ModbusIpMaster.CreateIp(tcpClient);
                modbusClient.Transport.WaitToRetryMilliseconds = 20;
                modbusClient.Transport.Retries = 5;
                modbusClient.Transport.WriteTimeout = 50;
                modbusClient.Transport.ReadTimeout = 50;
                modbusClient.Transport.SlaveBusyUsesRetryCount = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return tcpClient.Connected;
        }

        public override bool Disconnect()
        {
            try
            {
                if (tcpClient == null) return false;

                tcpClient.Close();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 读取单个线圈，DO,01
        /// </summary>
        public override bool[] ReadCoils(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                    return modbusClient?.ReadCoils((byte)this.SlaveID, Convert.ToUInt16(startAddress), (ushort)numberOfPoints);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 读取输入线圈/离散量线圈，DI,02
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns></returns>
        public override bool[] ReadInputs(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                    return modbusClient?.ReadInputs((byte)this.SlaveID, Convert.ToUInt16(startAddress), (ushort)numberOfPoints);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 读取保持寄存器，AO,03
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取数量,不超过125</param>
        /// <returns></returns>
        public override int[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                {
                    var a = modbusClient?.ReadHoldingRegisters((byte)this.SlaveID, Convert.ToUInt16(startAddress), (ushort)numberOfPoints);

                    var b = new int[a.Length];
                    a.CopyTo(b, 0);

                    return b;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 读取输入寄存器，AI,04
        /// </summary>
        public override int[] ReadInputRegisters(int startAddress, int numberOfPoints)
        {
            try
            {
                lock (rwLock)
                {
                    var a = modbusClient?.ReadInputRegisters((byte)this.SlaveID, Convert.ToUInt16(startAddress), Convert.ToUInt16(numberOfPoints));

                    return Convert.ChangeType(a, typeof(int[])) as int[];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// 写单个线圈 DO,05
        /// </summary>
        public override bool WriteSingleCoil(int startAddress, bool data)
        {
            try
            {
                lock (rwLock)
                    modbusClient?.WriteSingleCoil((byte)this.SlaveID, Convert.ToUInt16(startAddress), data);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
        /// <summary>
        /// 写单个输入线圈/离散量线圈 AO,06
        /// </summary>
        public override bool WriteSingleRegister(int startAddress, int data)
        {
            try
            {
                lock (rwLock)
                    modbusClient?.WriteSingleRegister((byte)this.SlaveID, Convert.ToUInt16(startAddress), ushort.Parse(data.ToString()));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
        /// <summary>
        /// 写一组线圈,15
        /// </summary>
        public override bool WriteMultipleCoils(int startAddress, bool[] data)
        {
            try
            {
                lock (rwLock)
                    modbusClient?.WriteMultipleCoils((byte)this.SlaveID, Convert.ToUInt16(startAddress), data);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
        /// <summary>
        /// 写一组保持寄存器,16
        /// </summary>
        public override bool WriteMultipleRegisters(int startAddress, int[] data)
        {
            try
            {
                ushort[] ushorts = new ushort[data.Length];
                for (global::System.Int32 i = 0; i < data.Length; i++)
                    ushorts[i] = ushort.Parse(data[i].ToString());

                lock (rwLock)
                    modbusClient?.WriteMultipleRegisters((byte)this.SlaveID, Convert.ToUInt16(startAddress), ushorts);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
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
        public bool Write(ModbusWriteEnum writeEnum, int startAddress, int[] value)
        {
            bool items = false;
            lock (rwLock)
                switch (writeEnum)
                {
                    case ModbusWriteEnum.WriteCoil:
                        items = WriteSingleCoil(startAddress, value[0] == 1 ? true : false);
                        break;
                    case ModbusWriteEnum.WriteRegister:
                        items = WriteSingleRegister(startAddress,(int)Convert.ChangeType(value[0], typeof(int)));
                        break;
                    case ModbusWriteEnum.WriteMutiCoils:
                        bool[] v = new bool[value.Length];
                        for (global::System.Int32 i = 0; i < value.Length; i++)
                            v[i] = value[i] == 1 ? true : false;
                        items = WriteMultipleCoils(startAddress, v);
                        break;
                    case ModbusWriteEnum.WriteMutiRegisters:
                        int[] v1 = new int[value.Length];
                        for (global::System.Int32 i = 0; i < value.Length; i++)
                            v1[i] = (int)Convert.ChangeType(value[i], typeof(int));
                        items = WriteMultipleRegisters(startAddress, v1);
                        break;
                    default:
                        break;
                }

            return items;
        }

        public void Dispose()
        {
            this.Disconnect();
        }
    }
}