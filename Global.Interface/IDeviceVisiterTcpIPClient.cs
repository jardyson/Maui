using GlobalDeclar;
using System.Net;

namespace Global.Interface
{
    public interface IDeviceVisiterTcpIPClient : IDeviceVisiterBase, IDeviceNetwork<int>
    {
        DeviceVisiterProtocalEnum VisiterProtocalType { get; set; }
        Task SendAsyncByAscii(string message);
        bool IsListening { get; set; }
        void DecodeReviced(byte[] buffer, int n);
    }
}
