using GlobalDeclar;

namespace DeviceDelar
{
    public class AppConfig : BaseNotifyChanged, IConfig
    {
        public string code { get; set; }
        public string name { get; set; }
        public string printname { get; set; }
        public string PrintName
        {
            get { return printname; }
            set { printname = value;
                OnPropertyChanged();
            }
        }
        public int plcno { get; set; }
        public List<GlobalDeclar.Network> ModbusNetworks { get; set; }
        public List<GlobalDeclar.Network> TcpIps { get; set; }
        public List<GlobalDeclar.Network> Melsecs { get; set; }
        public List<SerialConfig> ScanGuns { get; set; }
        public List<RFIDConfig> RFIDS { get; set; }

        public int scanout { get; set; } = 10;
    }
}