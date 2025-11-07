using System.IO.Ports;

namespace DeviceHelper.Controller
{
    public class DeviceActionQueue
    {
        /// <summary>
        /// TCPIP设备
        /// </summary>
        //public IModbus modbus { get; set; }
        /// <summary>
        /// 原指令
        /// </summary>
        public int commandEnum { get; set; }
        /// <summary>
        /// 命令类型
        /// </summary>
        public CommandType CommandType { get; set; }
        /// <summary>
        /// 配置节点数据，包含写入类型
        /// </summary>
        //public DevItem devItem { get; set; }
        /// <summary>
        /// 启动指令,作为文字提示返回
        /// </summary>
        public string actionname { get; set; }
        /// <summary>
        /// 触摸屏模式下停止指令(触摸屏专用)
        /// </summary>
        public string stopname { get; set; } = string.Empty;
        /// <summary>
        /// 延时时长
        /// </summary>
        public int DelayTime { get; set; } = 2000;
        #region 支架部分
        /// <summary>
        /// 支架指令
        /// </summary>
        public byte[] zjCmd { get; set; }
        public byte[] zjStopCmd { get; set; }
        /// <summary>
        /// Com设备
        /// </summary>
        public SerialPort SerialPort { get; set; } 
        /// <summary>
        /// 命令来源
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        #endregion
    }
}
