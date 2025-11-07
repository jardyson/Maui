using GlobalDeclar;
using System;

namespace Global.Interface
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="int">针对不同的连接组件NMODBUS4,EASYMODBUS使用的类型不尽相同</typeparam>
    public interface IDeviceVisiterModbusFuncs: IDeviceVisiterBase
        //where int : struct
    {
        /// <summary>
        /// 读取单个线圈，DO,01
        /// </summary>
        bool[] ReadCoils(int startAddress, int numberOfPoints);
        /// <summary>
        /// 读取输入线圈/离散量线圈 ，DI,02
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取数量</param>
        /// <returns></returns>
        bool[] ReadInputs(int startAddress, int numberOfPoints);
        /// <summary>
        /// 读取保持寄存器 ，AO,03
        /// </summary>
        /// <param name="startAddress">开始地址</param>
        /// <param name="numberOfPoints">读取数量,不超过125</param>
        /// <returns></returns>
        int[] ReadHoldingRegisters(int startAddress, int numberOfPoints);
        /// <summary>
        /// 读取输入寄存器，AI,04
        /// </summary>
        int[] ReadInputRegisters(int startAddress, int numberOfPoints);
        /// <summary>
        /// 写单个线圈DO,05
        /// </summary>
        bool WriteSingleCoil(int startAddress, bool data);
        /// <summary>
        /// 写单个输入线圈/离散量线圈 AO,06
        /// </summary>
        bool WriteSingleRegister(int startAddress, int data);
        /// <summary>
        /// 写一组线圈,15
        /// </summary>
        bool WriteMultipleCoils(int startAddress, bool[] data);
        /// <summary>
        /// 写一组保持寄存器,16
        /// </summary>
        bool WriteMultipleRegisters(int startAddress, int[] data);
        bool Write(ModbusWriteEnum writeEnum, int startAddress, int[] data);

        int[] Read(ModbusReadEnum ReadFunction, int startAddress, int length);
    }
}
