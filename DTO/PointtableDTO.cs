using GlobalDeclar;

namespace SLDTO
{
    public class PointtableDTO: BaseNotifyChanged
    {
        public Int64 id { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string? Code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 写或读
        /// </summary>
        public OperatorEnum? type { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public int addressfrom { get; set; }
        /// <summary>
        /// 地址2
        /// </summary>
        public int? addressto { get; set; }
        /// <summary>
        /// 预期值
        /// </summary>
        public float? prepervalue { get; set; }
        /// <summary>
        /// 读取类型
        /// </summary>
        public ModbusReadEnum? readfunction { get; set; }
        /// <summary>
        /// 写入类型
        /// </summary>
        public ModbusWriteEnum? writefunction { get; set; }
        /// <summary>
        /// 读写长度:1单字，2双字，4四字，8八字，依此类推
        /// </summary>
        public ushort operationLen { get; set; }
        public string? devicecode { get; set; }
        /// <summary>
        /// TCP指令
        /// </summary>
        public string? TcpString { get; set; }
        public Int64 solutionid { get; set; }
        public string? Remark { get; set; }
    }
}
