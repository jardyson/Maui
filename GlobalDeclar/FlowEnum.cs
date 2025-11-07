namespace GlobalDeclar
{
    public enum FlowEnum
    {
        /// <summary>
        /// 打印状态
        /// </summary>
        print,
        /// <summary>
        /// 保存状态
        /// </summary>
        savedb,
        /// <summary>
        /// 保存结束后反馈PLC状态
        /// </summary>
        writeplc, 
        /// <summary>
        /// 进入扫码匹配,将比对信息写入比对对象
        /// </summary>
        startmatch, 
        /// <summary>
        /// 等待扫码反馈
        /// </summary>
        waitmatch, 
        /// <summary>
        /// 等待PLC信号
        /// </summary>
        init,
        none,
        stats0,
        stats1,
        stats2,
        stats3,
        stats4,
        stats5,
        stats6,
        stats7,
        stats8,
        stats9,
        stats10,
    }
}