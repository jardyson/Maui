using GlobalDeclar;
using System.Net;

namespace Global.Interface
{
    public interface IDeviceVisiterTcpIPServer : IDeviceVisiterBase
    {
        DeviceVisiterProtocalEnum VisiterProtocalType { get; set; }
        Task StartServerAsync();
        void StopServer();
        bool IsListening { get; set; }
        Task<string> SendAsync(string message);
        string Send(string message);
        void DecodeReviced(byte[] buffer, int n);
    }
}