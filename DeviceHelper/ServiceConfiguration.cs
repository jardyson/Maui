using Brilliantech.Framwork.Utils.ConfigUtil;
using System.IO.Ports;

namespace DeviceHelper.Config
{
    public class ServiceConfiguration
    {
        private ConfigUtil config;

        public ServiceConfiguration()
        {
            config = new ConfigUtil("ServiceConfig", "Config/ServiceConfig.ini");
            this.InitConfigValue();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configPath">配置文件文件名，文件必须放在Config目录下</param>
        public ServiceConfiguration(string configFileName)
        {
            if (string.IsNullOrEmpty(configFileName))
            {
                configFileName = "ServiceConfig.ini";
            }
            config = new ConfigUtil("ServiceConfig", string.Format("Config/{0}", configFileName));
            this.InitConfigValue();
        }

        /// <summary>
        /// 初始化配置值
        /// </summary>
        private void InitConfigValue()
        {
            try
            {
                string[] Keys = config.GetAllNodeKey();
                WorkfaceNr = config.Get("WorkfaceNr");
                PortName = config.Get("PortName");
                ModbusTCP = config.Get("ModbusTCP");
                ModbusTCP1 = config.Get("ModbusTCP1");
                ModbusTCP2 = config.Get("ModbusTCP2");
                ModbusTCP3 = config.Get("ModbusTCP3");
                ModbusTCP4 = config.Get("ModbusTCP4");
                ModbusTCP5 = config.Get("ModbusTCP5");
                ModbusTCP6 = config.Get("ModbusTCP6");
                ModbusTCP7 = config.Get("ModbusTCP7");
                ModbusTCP8 = config.Get("ModbusTCP8");
                ModbusTCP9 = config.Get("ModbusTCP9");
                ModbusTCP10 = config.Get("ModbusTCP10");
                DataLog = config.Get("DataLog");
                if (Keys.Contains("offsetIndex")) OffsetIndex = int.Parse(config.Get("offsetIndex"));
                if (Keys.Contains("PortName2")) PortName2 = config.Get("PortName2");
                if (Keys.Contains("PortName3")) PortName3 = config.Get("PortName3");
                if (Keys.Contains("BaudRate")) BaudRate = int.Parse(config.Get("BaudRate"));
                if (Keys.Contains("StopBits")) StopBits = (StopBits)Enum.Parse(typeof(StopBits), config.Get("StopBits"));
                if (Keys.Contains("DataBits")) DataBits = int.Parse(config.Get("DataBits"));
                if (Keys.Contains("Parity")) Parity = (Parity)Enum.Parse(typeof(Parity), config.Get("Parity"));
                if (Keys.Contains("ReadTimeout")) ReadTimeout = int.Parse(config.Get("ReadTimeout"));
                if (Keys.Contains("RtsEnable")) RtsEnable = bool.Parse(config.Get("RtsEnable"));
                if (Keys.Contains("TcpServerPort")) TcpServerPort = int.Parse(config.Get("TcpServerPort"));
                if (Keys.Contains("BeforeSupNo")) BeforeSupNo = int.Parse(config.Get("BeforeSupNo"));
                if (Keys.Contains("AfterSupNo")) AfterSupNo = int.Parse(config.Get("AfterSupNo"));
                if (Keys.Contains("IsDown")) IsDown = bool.Parse(config.Get("IsDown"));
                if (Keys.Contains("GrpQty")) GrpQty = int.Parse(config.Get("GrpQty"));
                if (Keys.Contains("IsServer")) IsServer = bool.Parse(config.Get("IsServer"));
                if (Keys.Contains("WorkDirection")) WorkDirection = config.Get("WorkDirection");
                if (Keys.Contains("IsDebug")) IsDebug = bool.Parse(config.Get("IsDebug"));
                if (Keys.Contains("Server")) ServerIP = config.Get("Server");
                if (Keys.Contains("SiteUpTime")) SiteUpTime = int.Parse(config.Get("SiteUpTime"));
                if (Keys.Contains("TouchStopTime"))
                {
                    var a = 3;
                    int.TryParse(config.Get("TouchStopTime"), out a);
                    TouchStopTime = a;
                }
                if (Keys.Contains("ZJAlertTime"))
                {
                    var a = 8;
                    int.TryParse(config.Get("ZJAlertTime"), out a);
                    ZJAlertTime = a;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #region 方法
        /// <summary>
        /// 获取某个Key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            return config.Get(key);
        }
        /// <summary>
        /// 设置某个Key值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, string value)
        {
            config.Set(key, value);
        }
        /// <summary>
        /// 保存
        /// </summary>
        public void Save()
        {
            config.Save();
        }
        #endregion

        #region 属性
        public int OffsetIndex { get; set; } = 0;
        /// <summary>
        /// 工作面编号
        /// </summary>
        public string WorkfaceNr
        {
            get;
            set;
        }
        /// <summary>
        /// ModbusTCP服务器
        /// </summary>
        public string ModbusTCP { get; set; }
        public string ModbusTCP1 { get; set; }
        public string ModbusTCP2 { get; set; }
        public string ModbusTCP3 { get; set; }
        public string ModbusTCP4 { get; set; }
        public string ModbusTCP5 { get; set; }
        public string ModbusTCP6 { get; set; }
        public string ModbusTCP7 { get; set; }
        public string ModbusTCP8 { get; set; }
        public string ModbusTCP9 { get; set; }
        public string ModbusTCP10 { get; set; }
        /// <summary>日志数据库 IP:端口:表名 </summary>
        public string DataLog { get; set; }
        /// <summary>
        /// 串口名
        /// </summary>
        public string PortName { get; set; }
        /// <summary>
        /// 串口名
        /// </summary>
        public string PortName3 { get; set; }
        /// <summary>
        /// 支架操作台串口名
        /// </summary>
        public string PortName2 { get; set; }
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 停止位
        /// </summary>
        public StopBits StopBits { get; set; }

        public int DataBits { get; set; }

        /// <summary>
        /// 奇偶校验
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// 读取超时
        /// </summary>
        public int ReadTimeout { get; set; }

        /// <summary>
        /// 准备好接收数据
        /// </summary>
        public bool RtsEnable { get; set; }


        /// <summary>
        /// TCP 服务端口
        /// </summary>
        public int TcpServerPort { get; set; }

        /// <summary>
        /// 前推流
        /// </summary>
        public int BeforeSupNo { get; set; }

        /// <summary>
        /// 后推流
        /// </summary>
        public int AfterSupNo { get; set; }

        /// <summary>
        /// 操作台位置
        /// </summary>
        public bool IsDown { get; set; }
        /// <summary>
        /// 陈祖数量
        /// </summary>
        public int GrpQty { get; set; }
        /// <summary>
        /// 服务器
        /// </summary>
        public bool IsServer { get; set; }
        /// <summary>
        /// 参数程序
        /// </summary>
        public string ServerIP { get; set; }
        /// <summary>
        /// 工作面方向
        /// </summary>
        public string WorkDirection { get; set; }

        /// <summary>
        /// 调试模式
        /// </summary>
        public bool IsDebug { get; set; }
        /// <summary>
        /// 串口模式
        /// </summary>
        public bool ComHasPort3 { get; set; }
        /// <summary>
        /// 写入功能码
        /// </summary>
        public int FunctionReadCode { get; set; }
        /// <summary>
        /// 读功能码
        /// </summary>
        public int FunctionWriteCode { get; set; }
        /// <summary>
        /// 触摸屏动作结束时长(默认3秒)
        /// </summary>
        public int TouchStopTime { get; set; } = 3;
        /// <summary>
        /// 支架预警时长
        /// </summary>
        public int ZJAlertTime { get; set; }
        /// <summary>
        /// 上传定位时长
        /// </summary>
        public int SiteUpTime { get; set; }
        #endregion
    }
}
