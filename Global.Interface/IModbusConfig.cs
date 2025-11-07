using GlobalDeclar;
using System.IO.Ports;

namespace Global.Interface.Config
{
    public interface IModbusConfig : IConfig
    {
        string Ip { get; set; }
        int Port { get; set; }
        int SlaveID { get; set; } 
        int Baudrate { get; set; }
        //Parity Parity { get; set; } 
        //StopBits StopBits { get; set; }
    }
}
