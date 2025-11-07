using GlobalDeclar;

namespace DeviceDelar
{
    public interface ISerialConfig : IConfig
    {
        int BaudRate { get; set; }
        //System.IO.Ports.Parity Parity { get; set; }
        //StopBits StopBits { get; set; }
    }
}
