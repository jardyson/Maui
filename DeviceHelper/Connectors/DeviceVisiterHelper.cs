using DeviceHelper.Config;
using DeviceHelper.Connectors.RFID;
using DeviceHelper.Connectors.TCPIP;
using Global.Interface;
using GlobalDeclar;
using GlobalDeclar.Config;
using static DeviceHelper.Connectors.TCPIP.ListenClient;

namespace DeviceHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">设备写入和读取的类型int,unit,ushort等</typeparam>
    public class DeviceVisiterHelper
    {
        public static DeviceVisiterHelper _;
        public static DeviceVisiterHelper Instance => _ ?? (_ = new DeviceVisiterHelper());
        public static List<IDeviceVisiterBase> Build(DeviceVisiterProtocalEnum visiterEnum, IConfig config)
        {
            List<IDeviceVisiterBase> vists = new List<IDeviceVisiterBase>();
            var mconfig = (AppConfig)config;
            switch (visiterEnum)
            {
                case DeviceVisiterProtocalEnum.ModbusRTU:
                case DeviceVisiterProtocalEnum.ModbusTcp:
                    foreach (var item1 in mconfig.ModbusNetworks)
                        if (item1.DevType == DevType.StandardDevice)
                        {
                            if (item1.ReadWriteValueType == DeviceReadWriteValueType.IntType)
                                vists.Add(new NModbus(item1.code, item1.Name, item1.ip, item1.port, item1.slaveid, visiterEnum));
                            else if (item1.ReadWriteValueType == DeviceReadWriteValueType.UshortType)
                                vists.Add(new NModbus(item1.code, item1.Name, item1.ip, item1.port, item1.slaveid, visiterEnum));
                        }
                        else if (item1.DevType == DevType.Melsecs)
                            vists.Add(new Mitsubishi_TCP(item1.code, item1.Name, item1.ip, item1.port));
                    break;
                case DeviceVisiterProtocalEnum.SerialPort:
                    mconfig.ScanGuns.ForEach(x =>
                    {
                        vists.Add(new SerialPortUnit(x));
                    });
                    break;
                case DeviceVisiterProtocalEnum.RFIDTcpIp:
                    foreach (var item1 in mconfig.RFIDS)
                        vists.Add(new RFIDUnit(item1.code, item1.name, item1.ip, item1.port, item1.ischeckhealth==1));
                    break;
                case DeviceVisiterProtocalEnum.TcpIPServer:
                    var a = mconfig.TcpIps.Where(x => x.IsServer == 1).FirstOrDefault();
                    vists.Add(new TcpIpServer(a.code, a.Name, a.ip, a.port));
                    break;
                case DeviceVisiterProtocalEnum.TcpIPClient:
                    mconfig.TcpIps.ForEach(a => { 
                        vists.Add(new TcpIpClient(a.code, a.Name, a.ip, a.port,a.IsCheckhealth==1));
                    });//.Where(x => x.IsServer == 0).FirstOrDefault();
                    break;
                default:
                    return null;
            }

            return vists;
        }
    }
}
