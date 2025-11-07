namespace GlobalDeclar
{
    public enum ModbusWriteEnum
    {
        /// <summary>
        /// 单线圈
        /// </summary>
        WriteCoil = 5,
        /// <summary>
        /// 寄存器
        /// </summary>
        WriteRegister = 6,
        /// <summary>
        /// 写一组线圈
        /// </summary>
        WriteMutiCoils = 15,
        /// <summary>
        /// 写一组寄存器
        /// </summary>
        WriteMutiRegisters = 16
    }
}