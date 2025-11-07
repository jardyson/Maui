using Global.Interface;
using GlobalDeclar;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceHelper.Connectors.TCPIP
{
    public class TcpIpClient: VisiterBase, IDeviceVisiterTcpIPClient
    {
        // Common properties
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public string code { get ; set ; }
        public string Name { get ; set ; }
        public virtual DeviceVisiterProtocalEnum VisiterProtocalType { get ; set ; } = DeviceVisiterProtocalEnum.TcpIPClient;

        public bool IsConnected => _client==null? false:_client.Connected;
        public bool IsListening { get; set; } = false;

        public EventHandler onConnectedHandle { get ; set ; }
        public EventHandler onDisconnectHandle { get ; set ; }
        //public EventHandler onReceivedHandle { get ; set ; }
        public Func<object, string> onMsgHandle { get ; set ; }
        public EventHandler onSendHandle { get ; set ; }
        public int readCount { get; set; } = 10;
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get ; set ; }

        protected virtual TcpClient _client { get; set; }
        protected virtual NetworkStream _stream { get; set; }
        public bool IsCheckhealth { get ; set ; }
        public bool IsActive { get; set; } = true;
        public DevType devType { get ; set ; } = DevType.StandardDevice;

        public event Action<TcpClient, string> ServerDataReceived;
        public event Action<TcpClient> ServerClientDisconnected;

        public event Action ClientDisconnected;

        public TcpIpClient(string code,string name,string ip, int port, bool ischeckhealth)
        {
            this.code = code;
            this.Name = name;
            IP = IPAddress.Parse(ip);
            Port = port;
            IsCheckhealth = ischeckhealth;
        }

        // -------- Client methods --------
        public virtual bool Connect()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(IP, Port);
                _stream = _client.GetStream();
                onConnectedHandle?.Invoke($"{this.IP} {this.Port} 连接成功.", null);
                _ = ReceiveLoopAsync();
            }
            catch (Exception exc)
            {
                XLog.Error($"[{this.Name}] 连接错误: {exc.Message} {exc.StackTrace}");
            }
            finally
            {

            }

            return _client.Connected;
        }

        public async Task ReceiveLoopAsync()
        {
            Task.Run(() =>
            {
                var buffer = new byte[1024];
                while (IsConnected)
                {
                    try
                    {
                        int n = _stream.Read(buffer, 0, buffer.Length);
                        if (n == 0) break;
                        DecodeReviced(buffer, n);
                    }
                    catch (Exception exc)
                    {
                        XLog.Error($"[{this.Name}] 接收消息错误:{exc.Message} /r/n {exc.StackTrace}");
                        Thread.Sleep(1000);
                    }

                    Thread.Sleep(20);
                }
            });
        }

        public async Task SendAsyncByAscii(string message)
        {
            if (_stream == null)
                onConnectedHandle?.Invoke("没有连接", null);
            try
            {
                var data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception exc)
            {
                XLog.Error($"[{this.Name}] 发送消息错误:{exc.Message} {exc.StackTrace}");
            }
        }

        public async Task<string> SendAsyncByHex(string hexData)
        {
            try
            {
                if (_stream == null)
                    return $"[{Name}] 设备未连接";
                // 将HEX字符串转换为字节数组
                byte[] bytes = HexStringToByteArray(hexData);
                // 发送数据
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception exc)
            {
                XLog.Error($"[{this.Name}] 发送消息错误:{exc.Message} {exc.StackTrace}");
                return $"[{this.Name}] 发送消息错误:{exc.Message} {exc.StackTrace}";
            }

            return string.Empty;
        }

        // HEX字符串转字节数组
        private static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", ""); // 移除空格
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        // 字节数组转十六进制字符串
        public static string BytesToHex(byte[] bytes, int length)
        {
            StringBuilder hexBuilder = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
            {
                hexBuilder.AppendFormat("{0:X2}", bytes[i]);
            }
            return hexBuilder.ToString();
        }

        public virtual bool Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            onConnectedHandle?.Invoke($"{this.IP} {this.Port} 连接已断开", null);

            return true;
        }

        public void Dispose()
        {
        }

        public virtual void DecodeReviced(byte[] buffer, int n)
        {
            var msg = Encoding.UTF8.GetString(buffer, 0, n);

            onDeviceReceivedHandle?.Invoke(new deviceReceviedParam()
            {
                RecevicedMsg = msg,
                visiter = this
            });
        }
    }
}
