using DeviceHelper.Config;
using Global.Interface;
using GlobalDeclar;
using GlobalDeclar.Config;
using Modbus.Device;
using RJCP.IO.Ports;
using System.IO.Ports;
using SerialDataReceivedEventArgs = RJCP.IO.Ports.SerialDataReceivedEventArgs;

namespace DeviceHelper
{
    public class SerialPortUnit : BaseModbus<ModbusIpMaster, ushort, string>, IDeviceVisiterBase
    {
        public override bool IsConnected => serialPort.IsOpen;

        public SerialPortStream serialPort { get; set; }
        public new DeviceVisiterProtocalEnum VisiterProtocalType { get; set; } = DeviceVisiterProtocalEnum.SerialPort;
        public string PortName => serialPort.PortName;
        public int BaudRate => serialPort.BaudRate;

        public override bool IsActive { get; set; } = true;

        public SerialPortUnit(SerialConfig config)
        {
            code = config.Code;
            Name = config.name;
            Port = config.port;

            serialPort = new SerialPortStream();
            serialPort.PortName = config.port;
            serialPort.BaudRate = config.BaudRate;
            serialPort.StopBits = config.StopBits;
            serialPort.DataBits = config.DataBits;
            //serialPort.Parity = config.Parity;
            serialPort.ReadTimeout = config.ReadTimeout;
            serialPort.WriteTimeout = config.WriteTimeout;
            serialPort.RtsEnable = config.RtsEnable;
            serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            onDeviceReceivedHandle?.Invoke(new deviceReceviedParam()
            {
                RecevicedMsg = serialPort.ReadExisting().Replace("\r",""),
                visiter = this
            });
        }

        public override bool Connect()
        {
            try
            {
                serialPort?.Close();
                serialPort.Open();
            }
            catch (Exception exc)
            {
                throw new Exception($"打开{serialPort.PortName}失败 [{exc.Message}] \r\n {exc.StackTrace}");
            }

            return true;
        }
        public override bool Disconnect()
        {
            return base.Disconnect();
        }
        public void Dispose()
        {
            try
            {
                serialPort?.Close();
            }
            catch (Exception exc)
            {
                XLog.Error(exc.Message + "\r\n" + exc.StackTrace);
            }
        }
    }
}