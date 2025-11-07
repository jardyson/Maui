using GlobalDeclar;

namespace Global.Interface
{
    /// <summary>
    /// 访问设备
    /// </summary>
    public interface IDeviceVisiter
    {
        string Name { get; set; }
        VisiterEmun VisiterType { get; set; }
    }
}
