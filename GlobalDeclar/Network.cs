namespace GlobalDeclar
{
    public class Network : IConfig
    {
        public string code { get; set; }
        public string Name { get; set; }
        public string ip { get; set; }
        public int port { get; set; }
        public int slaveid { get; set; }
        /// <summary>
        /// TCP连接是否服务端
        /// </summary>
        public int IsServer { get; set; }
        /// <summary>
        /// 是否检查网络健康状态
        /// </summary>
        public int IsCheckhealth { get; set; } = 0;
        /// <summary>
        /// 是否通知错误
        /// </summary>
        public int IsErrNotify { get; set; } = 0;
        public DeviceReadWriteValueType ReadWriteValueType { get; set; } = 0;
        public DevType DevType { get; set; }
    }
}