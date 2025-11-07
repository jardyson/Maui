using GlobalDeclar;

namespace Global.Interface
{
    /// <summary>
    /// 设备访问接口基类
    /// </summary>
    public interface IDeviceVisiterBase: IDisposable
    {
        /// <summary>
        /// 编码
        /// </summary>
        string code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 读取状态次数
        /// </summary>
        int readCount { get; set; }
        bool IsCheckhealth { get; set; }
        int IsErrNotify { get; }
        /// <summary>
        /// 设备
        /// </summary>
        DeviceVisiterProtocalEnum VisiterProtocalType { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        DevType devType { get; set; }
        EventHandler onConnectedHandle { get; set; }
        EventHandler onDisconnectHandle { get; set; }
        Func<object, string> onMsgHandle { get; set; }
        Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        EventHandler onSendHandle { get; set; }
        bool Connect();
        bool Disconnect();
        bool IsConnected { get; }
        bool IsActive { get; set; }
    }
}
