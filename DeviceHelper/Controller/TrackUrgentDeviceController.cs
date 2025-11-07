//using CloudSmartMineConnectorCommon;
//using CloudSmartMineGlobalModel.DeviceMessage;
//using CloudSmartMineGlobalModel.Enums;
//using DeviceHelperCommon.Config;
//using DeviceHelperCommon.Modbus;
//using DeviceHelpLibrary.Log;
//using Newtonsoft.Json;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;

////list
//namespace DeviceHelperCommon.Controller
//{
//    public partial class TrackUrgentDeviceController : DeviceController, IMsgProcessor
//    {
//        private Dictionary<string, ModBusConfig> dirConfigModBus = new Dictionary<string, ModBusConfig>();
//        private DefectCodeService defectCodeService = new DefectCodeService();
//        List<EquipmentTransferMessage> messages = new List<EquipmentTransferMessage>();

//        private Department workface;
//        private static TDtable TD;
//        #region 初始化
//        // 消息事件
//        private Queue messageDataPubQ = new Queue();
//        private Queue messageDataPubQueue;
//        private Thread messageDataPubThread;
//        private ManualResetEvent messageDataPubEvent = new ManualResetEvent(false);

//        private Queue messageDataPersistQ = new Queue();
//        private Queue messageDataPersistQueue;
//        private Thread messageDataPersistThread;
//        private ManualResetEvent messageDataPersistEvent = new ManualResetEvent(false);

//        private Queue messageForwardDeepPersistQueue;
//        private Thread messageForwardDeepPersistThread;
//        private ManualResetEvent messageForwardDeepPersistEvent = new ManualResetEvent(false);

//        private ModbusTcpServer Mserver;
//        private ModBusConfig MCG;
//        private ModbusRW modbusTcp;
//        SupporterDeviceMessage msg;
//        Action<string> callBack;
//        #endregion

//        // service
//        EquipmentRealTimeDataService service = new EquipmentRealTimeDataService(new SupporterRealTimeDataProcessor());

//        public TrackUrgentDeviceController(ServiceConfiguration ServiceConfigObject
//            , Action<string> connected = null
//            , Action<string> disconnected = null)
//        {

//            this.ServiceConfigObject = ServiceConfigObject;
//            this.ControllerName = "跟踪急停";
//            this.Init(connected, disconnected);
//        }

//        private void Init(Action<string> connected, Action<string> disconnected)
//        {
//            this.workface = new DepartmentService().findByNr(ServiceConfigObject.WorkfaceNr);
//            if (workface == null)
//                throw new KeyNotFoundException("部门数据为空");

//            this.InitQueue();
//            this.InitSerialPort();
//            this.InitDeviceServiceServer(connected, disconnected);

//            //数据库保存
//            if (ServiceConfigObject.DataLog != null)
//            {
//                var dlog = ServiceConfigObject.DataLog.Split(':');
//                TD = new TDtable(dlog[0], short.Parse(dlog[1]), dlog[2], dlog[3], dlog[4]);
//            }
//            else
//            {
//                SendClientMsg("DataLog Is Null,whitout save", true);
//            }
//        }
//        public new void InitDeviceServiceServer(Action<string> connected, Action<string> disconnected)
//        {
//            base.InitDeviceServiceServer(connected, disconnected);
//            deviceServiceServer.RegisterMsgProcessor(typeof(EquipmentTransferMessage), this);
//        }
//        public Dictionary<string, Tuple<int, int, DateTime>> locationParamDict = new Dictionary<string, Tuple<int, int, DateTime>>();
//        private void InitQueue()
//        {
//            messageDataPersistThread = new Thread(this.MessagePresistThreadMethod);
//            messageDataPersistQueue = Queue.Synchronized(messageDataPersistQ);

//            messageDataPubThread = new Thread(this.MessagePubThreadMethod);
//            messageDataPubQueue = Queue.Synchronized(messageDataPubQ);
//        }

//        void InitModbus()
//        {
//            DevExtends.Load("TrackUrgentConfig");
//            if (ServiceConfigObject.ModbusTCP != null)
//            {
//                dirConfigModBus.Add("SJ", new ModBusConfig(ServiceConfigObject.ModbusTCP, (modbus, devitem) =>
//                {
//                    if (modbus.GetModBus() == null)
//                        return;
//                    var modbusTcp = modbus.modBus;
//                    try
//                    {
//                        List<EquipmentTransferMessage> messages = new List<EquipmentTransferMessage>();
//                        var col = Read(devitem.FunctionReadCode, in modbusTcp, devitem.codeStart, devitem.codeLen);
//                        if (col == null)
//                        {
//                            XLog.Error($"三机监控失败{devitem.Name} {devitem.codeStart}-{devitem.codeStart + devitem.codeLen}");
//                            devitem.SetAllValue(new bool[] { }, devitem.codeStart);
//                            this.StartReceivedMessage(new EquipmentTransferMessage()
//                            {
//                                cmdAction = CmdAction.TrackUrgentMJ,
//                                equipmentTypeCode = devitem.Name,
//                                value = null
//                            });
//                        }
//                        else
//                        {
//                            WriteMsgOnly($"三机监控数据{devitem.Name} {devitem.codeStart} - {devitem.codeStart + devitem.codeLen}{JsonConvert.SerializeObject(col)}");
//                            devitem.SetAllValue(col, devitem.codeStart);
//                            devitem.SaveTDNew(TD);
//                            this.StartReceivedMessage(new EquipmentTransferMessage()
//                            {
//                                equipmentTypeCode = devitem.Name,
//                                value = JsonConvert.SerializeObject(devitem)
//                            });
//                        }

//                        foreach (var m in messages) { this.StartReceivedMessage(m); }
//                    }
//                    catch (Exception ex)
//                    {
//                        WriteMsgOnly($"三机监控失败{modbus.ModbusTCP}");
//                        XLog.Error($"{ex.Message}\r\n{ex.StackTrace}");
//                    }

//                }, DevExtends.GetDevItem("SJStatus")));

//                dirConfigModBus["SJ"].MonitorModbus();
//            }
//            if (ServiceConfigObject.ModbusTCP1 != null)
//            {
//                dirConfigModBus.Add("PD", new ModBusConfig(ServiceConfigObject.ModbusTCP1
//                    , (modbus, devitem) =>
//                {
//                    if (modbus.GetModBus() == null)
//                        return;

//                    var modbusTcp = modbus.modBus;
//                    try
//                    {
//                        List<EquipmentTransferMessage> messages = new List<EquipmentTransferMessage>();
//                        var col = Read(devitem.FunctionReadCode, in modbusTcp, devitem.codeStart, devitem.codeLen);
//                        if (col == null)
//                        {
//                            XLog.Error($"皮带监控失败{devitem.Name} {devitem.codeStart}-{devitem.codeStart + devitem.codeLen}");
//                            devitem.SetAllValue(new bool[] { }, devitem.codeStart);
//                            this.StartReceivedMessage(new EquipmentTransferMessage()
//                            {
//                                cmdAction = CmdAction.TrackUrgentMJ,
//                                equipmentTypeCode = devitem.Name,
//                                value = "皮带监控数据读取失败"
//                            });

//                            callBack?.BeginInvoke("皮带监控数据读取失败", null, null);
//                        }
//                        else
//                        {
//                            WriteMsgOnly($"皮带监控数据{devitem.Name} {devitem.codeStart} - {devitem.codeStart + devitem.codeLen}{JsonConvert.SerializeObject(col)}");
//                            devitem.SetAllValue(col, devitem.codeStart);
//                            devitem.SaveTDNew(TD);
//                            this.StartReceivedMessage(new EquipmentTransferMessage()
//                            {
//                                cmdAction = CmdAction.TrackUrgentPD,
//                                equipmentTypeCode = devitem.Name,
//                                value = JsonConvert.SerializeObject(devitem)
//                            });
//                        }

//                        foreach (var m in messages) { this.StartReceivedMessage(m); }
//                    }
//                    catch (Exception ex)
//                    {
//                        WriteMsgOnly($"皮带监控失败{modbus.ModbusTCP}");
//                        XLog.Error($"{ex.Message}\r\n{ex.StackTrace}");
//                    }

//                }
//                , DevExtends.GetDevItem("pdstatus")));

//                dirConfigModBus["PD"].MonitorModbus();
//            }
//        }
//        /// <summary>
//        /// ?????????????
//        /// </summary>
//        /// <param name="receivedMessage"></param>
//        private void StartReceivedMessage(EquipmentTransferMessage receivedMessage)
//        {
//            try
//            {
//                messageDataPubQueue.Enqueue(receivedMessage);
//                messageDataPubEvent.Set();

//                messageDataPersistQueue.Enqueue(receivedMessage);
//                messageDataPersistEvent.Set();
//            }
//            catch (Exception ex)
//            {
//                XLog.Error($"StartReceivedMessage err:{ex.Message}");
//            }
//        }

//        /// <summary>
//        /// ?????????????
//        /// </summary>
//        /// <param name="receivedMessage"></param>
//        private void StartReceivedForwardDeep(SupporterForwardDeepMetaData receivedMessage)
//        {
//            messageForwardDeepPersistQueue.Enqueue(receivedMessage);
//            messageForwardDeepPersistEvent.Set();
//        }

//        /// <summary>
//        /// ??????????
//        /// </summary>
//        private void MessagePubThreadMethod()
//        {
//            while (true)
//            {
//                while (messageDataPubQueue.Count > 0)
//                    ReceivedMessagePub((EquipmentTransferMessage)messageDataPubQueue.Dequeue());

//                messageDataPubEvent.WaitOne();
//                messageDataPubEvent.Reset();
//            }
//        }

//        /// <summary>
//        /// ??????????
//        /// </summary>
//        private void MessagePresistThreadMethod()
//        {
//            while (true)
//            {
//                while (messageDataPersistQueue.Count > 0)
//                    ReceivedMessagePersist((EquipmentTransferMessage)messageDataPersistQueue.Dequeue());

//                messageDataPersistEvent.WaitOne();
//                messageDataPersistEvent.Reset();
//            }
//        }

//        /// <summary>
//        /// ???????????????
//        /// </summary>
//        /// <param name="message"></param>
//        private void ReceivedMessagePersist(EquipmentTransferMessage message)
//        {
//            try
//            {
//                ReceivedMessagePub<EquipmentTransferMessage>(message);
//            }
//            catch (Exception ex)
//            {
//                XLog.Error(ex.Message);
//            }
//        }
//        /// <summary>
//        /// 启动
//        /// </summary>
//        public override void Start()
//        {
//            this.messageDataPubThread.Start();
//            this.messageDataPersistThread.Start();
//            this.deviceServiceServer.Start();

//            this.InitModbus();
//        }
//        /// <summary>
//        /// 反序列化
//        /// </summary>
//        /// <param name="msg"></param>
//        /// <returns></returns>
//        public object FromMessage(string msg)
//        {
//            // 反序列化
//            return JsonConvert.DeserializeObject<EquipmentTransferMessage>(msg, new JsonSerializerSettings()
//            {
//                TypeNameHandling = TypeNameHandling.Auto
//            });
//        }
//        Stopwatch controlWatch = new Stopwatch();
//        SupporterBaseCommand supporter;
//        /// <summary>
//        /// 处理接收到地消息
//        /// </summary>
//        /// <param name="msg"></param>
//        public void Process(object msg)
//        {
//            var cmd = msg as EquipmentTransferMessage;

//            Console.WriteLine($"{DateTime.Now.ToString("O")}\t TCP收到←:" + JsonConvert.SerializeObject(cmd));
//        }
//        /// <summary>
//        /// ??
//        /// </summary>
//        public override void Stop()
//        {
//            this.messageDataPubQueue.Clear();
//            this.messageDataPersistQueue.Clear();

//            if (this.messageDataPubThread != null)
//            {
//                this.messageDataPubEvent.Close();
//                this.messageDataPubThread.Abort();
//            }

//            if (this.messageDataPersistThread != null)
//            {
//                this.messageDataPersistEvent.Close();
//                this.messageDataPersistThread.Abort();
//            }
//            if (this.messageForwardDeepPersistEvent != null)
//            {
//                this.messageForwardDeepPersistEvent.Close();
//                this.messageForwardDeepPersistThread.Abort();
//            }
//        }
//    }
//}