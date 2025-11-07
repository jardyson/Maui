namespace SLDTO
{
    public class ScanHistoryDTO
    {
        public Int64 id { get; set; }
        /// <summary>
        /// 产品编号
        /// </summary>
        public string? sparepartno { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string? sparepartname { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string? supplierno { get; set; }
        /// <summary>
        /// 供应商
        /// </summary>
        public string? supplier { get; set; }
        /// <summary>
        /// 打码内容
        /// </summary>
        public string printtext { get; set; }
        /// <summary>
        /// 对比结果
        /// </summary>
        public string? matchresult { get; set; }
        public string? BatchNo { get; set; }
        /// <summary>
        /// 对比时间
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
