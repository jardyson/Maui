using Global.Interface;

namespace DeviceControlService.Interface
{
    public interface IUpperMachine : IDevice
    {
        void Connect();
        void Disconnect();

        string IP { get; set; }
        int Port { get; set; }
        int SlaveID { get; set; }
    }
}
