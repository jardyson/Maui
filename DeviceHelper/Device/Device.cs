using Global.Interface;

namespace DeviceHelper.Device
{
    public class Device : IDevice
    {
        public string Name { get; set; }
        public List<IDeviceVisiterBase> DeviceVisiters { get; set; }
    }
}