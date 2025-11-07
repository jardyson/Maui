using RJCP.IO.Ports;

namespace DeviceDelar
{
    public class SerialConfig : ISerialConfig
    {
        public SerialConfig()
        {
        }
        public string name { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// 串口名
        /// </summary>
        public string port { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }
        /// <summary>
        /// 奇偶校验
        /// </summary>
        public Parity Parity { get; set; }

        public int DataBits { get; set; }

        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }

        public bool RtsEnable { get; set; }
    }
}
