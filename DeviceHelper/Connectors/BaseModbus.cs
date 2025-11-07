using Global.Interface;
using GlobalDeclar;
using System.Net;

namespace DeviceHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="valuetype">Modbus读写涉及到的参数是什么类型，TCP不用管</typeparam>
    public abstract class BaseModbus<T, valuetype, prottype> : VisiterBase, IDeviceVisiterBase
        where valuetype : struct
    {
        public T modbusClient;

        public object rwLock = new object();

        public int SlaveID { get; set; }
        public IPAddress IP { get; set; }
        public prottype Port { get; set; }
        public string code { get; set; }
        public string Name { get; set; }

        public virtual DeviceVisiterProtocalEnum VisiterProtocalType { get; set; }
        public virtual DevType devType { get; set; }
        public EventHandler onConnectedHandle { get; set; }
        public EventHandler onDisconnectHandle { get; set; }
        public Func<object, string> onMsgHandle { get; set; }
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        public EventHandler onSendHandle { get; set; }

        public virtual bool IsConnected { get; }
        public int readCount { get; set; } = 10;
        public bool IsCheckhealth { get; set; }
        public abstract bool IsActive { get; set ; }

        public virtual bool Connect()
        {
            return false;
        }

        public virtual bool Disconnect()
        {
            throw new NotImplementedException();
        }

        public virtual bool[] ReadCoils(int startAddress, int numberOfPoints)
        {
            throw new NotImplementedException();
        }

        public virtual valuetype[] ReadHoldingRegisters(int startAddress, int numberOfPoints)
        {
            throw new NotImplementedException();
        }

        public virtual valuetype[] ReadInputRegisters(int startAddress, int numberOfPoints)
        {
            throw new NotImplementedException();
        }

        public virtual bool[] ReadInputs(int startAddress, int numberOfPoints)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteMultipleCoils(int startAddress, bool[] data)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteMultipleRegisters(int startAddress, valuetype[] data)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteSingleCoil(int startAddress, bool data)
        {
            throw new NotImplementedException();
        }

        public virtual bool WriteSingleRegister(int startAddress, int data)
        {
            throw new NotImplementedException();
        }

        public virtual valuetype[] Read(ModbusReadEnum ReadFunction, int startAddress, int numberOfPoints, int length)
        {
            valuetype[] rtn = new valuetype[length];
            lock (rwLock)
                switch (ReadFunction)
                {
                    case ModbusReadEnum.ReadCoils:
                        bool[] items = new bool[length];

                        items = ReadCoils( startAddress, (UInt16)(items.Length));
                        if (items == null)
                            return null;
                        for (global::System.Int32 i = 0; i < items.Length; i++)
                            rtn[i] = (valuetype)Convert.ChangeType(items[i] ? 1 : 0, typeof(valuetype));

                        return rtn;
                    case ModbusReadEnum.ReadInputs:
                        bool[] items1 = ReadInputs(startAddress, (UInt16)length);
                        if (items1 == null)
                            return null;
                        for (global::System.Int32 i = 0; i < items1.Length; i++)
                            rtn[i] = (valuetype)Convert.ChangeType(items1[i] ? 1 : 0, typeof(valuetype));
                        break;
                    case ModbusReadEnum.ReadHoldingRegisters:
                        rtn = ReadHoldingRegisters(startAddress, (UInt16)length);
                        break;
                    case ModbusReadEnum.ReadInputRegisters:
                        rtn = ReadInputRegisters(startAddress, (UInt16)length);
                        break;
                    default:
                        break;
                }

            return rtn;
        }

        public virtual valuetype[] Read(ModbusReadEnum ReadFunction, valuetype startAddress, int length)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}