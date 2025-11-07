namespace Global.Interface
{
    public interface IDeviceVisiterTcpIPRFID : IDeviceVisiterBase
    {
        Task<string> ReadRFID(string HexCmd);
    }
}
