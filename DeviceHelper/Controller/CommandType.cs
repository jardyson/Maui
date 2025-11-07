namespace DeviceHelper.Controller
{
    public enum CommandType
    {
        /// <summary>
        /// cmcc命令
        /// </summary>
        CCMC,
        /// <summary>
        /// 触摸屏命令
        /// </summary>
        TOUTCH,
        /// <summary>
        /// 延时
        /// </summary>
        Delay,
        /// <summary>
        /// 指令动作结束
        /// </summary>
        CommandComplate,
        /// <summary>
        /// 设备断开连接
        /// </summary>
        DeviceDisConnect,
    }
}
