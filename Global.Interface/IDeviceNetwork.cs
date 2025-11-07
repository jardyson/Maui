using System.Net;

namespace Global.Interface
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">端口类型,分INT和STRING</typeparam>
    public interface IDeviceNetwork<T>
    {
        IPAddress IP { get; set; }
        T Port { get; }
    }
}
