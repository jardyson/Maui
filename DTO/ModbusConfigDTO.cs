namespace SLDTO
{
    public class ModbusConfigDTO<T>
    {
        public Int64 id { get; set; }
        /// <summary>
        /// 编码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 配置字符串
        /// </summary>
        public T ConfigStr { get; set; }
        /// <summary>
        /// 方案ID
        /// </summary>
        public Int64 sulotionid { get; set; }
        public bool Isdefault { get; set; } = false;
    }
}
