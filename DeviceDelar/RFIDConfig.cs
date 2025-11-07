using GlobalDeclar;

namespace DeviceDelar
{
    public class RFIDConfig : IConfig
    {
        public string code { get; set; }
        public string name { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public int ischeckhealth { get; set; }
    }
}