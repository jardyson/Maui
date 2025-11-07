using DeviceHelper.Connectors.TCPIP;
using Global.Interface;
using GlobalDeclar;

namespace DeviceHelper.Connectors.RFID
{
    public class RFIDUnit : TcpIpClient, IDeviceVisiterTcpIPRFID
    {
        int fd = -1;
        private Thread serverThread;
        private bool isRunning;
        public string ver = string.Empty;
        public int id = -1;

        public override DeviceVisiterProtocalEnum VisiterProtocalType { get; set; } = DeviceVisiterProtocalEnum.RFIDTcpIp;

        public string ReadRFIDResult = string.Empty;
        public bool IsWroking = false;
        public RFIDUnit(string code, string name, string ip, int port, bool ischeckhealth) : base(code, name, ip, port, ischeckhealth)
        {
        }

        public override void DecodeReviced(byte[] buffer, int n)
        {
            var msg = BytesToHex(buffer, n);
            onDeviceReceivedHandle?.Invoke(new deviceReceviedParam
            {
                RecevicedMsg = msg,
                visiter = this
            });
        }

        public async Task<string> ReadRFID(string HexCmd)
        {
            string rtn = string.Empty;
            try
            {
                rtn = await base.SendAsyncByHex(HexCmd);
                IsWroking = true;
                //延期1秒后关闭工作状态
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    IsWroking = false;
                });
            }
            catch (Exception exc)
            {
                throw exc;
            }

            return rtn;
        }
    }
}