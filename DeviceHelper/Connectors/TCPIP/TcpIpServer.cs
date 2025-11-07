using Global.Interface;
using GlobalDeclar;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceHelper.Connectors.TCPIP
{
    public class ListenClient
    {
        public int TimeOut = 10000; // 10 seconds timeout for read operations
        DateTime creatTime = DateTime.Now;

        public IPAddress IP => (client.Client.RemoteEndPoint as IPEndPoint).Address;
        public int Port => (client.Client.RemoteEndPoint as IPEndPoint).Port;
        public TcpClient client;
        NetworkStream _stream;
        public EventHandler onConnectedHandle { get; set; }
        public EventHandler onDisconnectHandle { get; set; }
        byte[] buffer = new byte[1024];
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        IDeviceVisiterBase deviceVisiterBase;
        public object obj = new object();
        public virtual void DecodeReviced(byte[] buffer, int n)
        {
            var msg = Encoding.UTF8.GetString(buffer, 0, n);
            onDeviceReceivedHandle?.Invoke(new deviceReceviedParam()
            {
                RecevicedMsg = msg,
                visiter = deviceVisiterBase
            });
        }

        public async Task<string> SendAsync(string Msg)
        {
            return Task<string>.Run(() =>
             {
                 if (_stream == null)
                     return string.Empty;
                 var data = Encoding.UTF8.GetBytes(Msg);
                 _stream.Write(data, 0, data.Length);
                 onDeviceReceivedHandle?.Invoke(new deviceReceviedParam()
                 {
                     RecevicedMsg = $"[{client.Client.RemoteEndPoint}] 发送数据成功",
                     visiter = deviceVisiterBase
                 });

                 return string.Empty;
             }).GetAwaiter().GetResult();
        }

        public string Send(string Msg)
        {
            if (_stream == null)
                return string.Empty;

            var data = Encoding.UTF8.GetBytes(Msg);
            _stream.Write(data, 0, data.Length);
            onDeviceReceivedHandle?.Invoke(new deviceReceviedParam()
            {
                RecevicedMsg = $"[{client.Client.RemoteEndPoint}] 发送数据成功",
                visiter = deviceVisiterBase
            });

            return string.Empty;
        }

        public void StartListen(IDeviceVisiterBase visiter)
        {
            deviceVisiterBase = visiter;
            _stream = client.GetStream();
            //消息监听
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        int n = await _stream.ReadAsync(buffer, 0, buffer.Length);
                        if (n == 0)
                        {
                            try
                            {
                                var p = client.Client.RemoteEndPoint as IPEndPoint;
                                onDisconnectHandle?.Invoke(this, null);
                                client.Client.Shutdown(SocketShutdown.Both);
                                client.Close();
                                break;
                            }
                            catch (Exception ex)
                            {
                                XLog.Error(ex);
                                break;
                            }
                        }
                        DecodeReviced(buffer, n);
                    }
                    catch (Exception exc)
                    {
                        XLog.Error(exc);
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        public void Disconnect()
        {
            _stream.Close();
            client.Close();

            onConnectedHandle?.Invoke($"{this.IP} {this.Port} 连接已断开", null);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            client?.Dispose();
            onConnectedHandle?.Invoke($"{this.IP} {this.Port} 已摧毁", null);
        }
    }
    public struct ClientInfo
    {
        public IPEndPoint IP;
        public string msg;
    }
    public class TcpIpServer : VisiterBase, IDeviceVisiterTcpIPServer, IDeviceNetwork<int>
    {
        public IPAddress IP { get; set; }
        public int Port { get; }
        public string code { get; set; }
        public string Name { get; set; }
        public DeviceVisiterProtocalEnum VisiterProtocalType { get; set; } = DeviceVisiterProtocalEnum.TcpIPServer;

        public bool IsConnected => clients.Count > 0 ? true : false;
        public bool IsListening { get; set; } = false;

        public EventHandler onConnectedHandle { get; set; }
        public EventHandler onDisconnectHandle { get; set; }
        public Func<object, string> onMsgHandle { get; set; }
        public EventHandler onSendHandle { get; set; }
        public int readCount { get; set; } = 10;
        public Func<deviceReceviedParam, string> onDeviceReceivedHandle { get; set; }
        public bool IsCheckhealth { get; set; }
        public bool IsActive { get; set; } = true;
        public DevType devType { get ; set ; } = DevType.StandardDevice;

        private TcpListener _listener;
        // Events
        public List<ListenClient> clients = new List<ListenClient>();
        public event Action<TcpClient, string> ServerDataReceived;

        public TcpIpServer(string code, string name, string ip, int port)
        {
            this.code = code;
            this.Name = name;
            IP = IPAddress.Parse(ip);
            Port = port;
        }

        // -------- Server methods --------
        public async Task StartServerAsync()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            IsListening = true;
            XLog.Info($"Server listening on *:{Port}");
            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var c = client.Client.RemoteEndPoint as IPEndPoint;
                XLog.Info($"[{c.Address} {c.Port}] 客户端请求连接...");
                Task.Run(() =>
                {
                    HandleServerClientAsync(client);
                });
            }
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

        private async Task HandleServerClientAsync(TcpClient client)
        {
            var a = new ListenClient()
            {
                client = client,
                onDeviceReceivedHandle = this.onDeviceReceivedHandle,
                onDisconnectHandle = this.onDisconnectHandle,
            };
            a.onDisconnectHandle += (sender, e) =>
            {
                var a = sender as ListenClient;
                XLog.Info($"[{a.IP.ToString()}{a.Port}] 客户端断开连接");
                clients.Remove(a);
                onDisconnectHandle?.Invoke(sender, e);
            };
            a.StartListen(this);

            this.clients.Add(a);
        }

        public void StopServer()
        {
            _listener?.Stop();
        }

        public async Task<string> SendAsync(string message)
        {
            if (clients.Count < 1)
                return "没有连接的[TCPIP]客户，无法发送数据";

            clients.ForEach(x => x.SendAsync(message));

            return string.Empty;
        }
        public string Send(string message)
        {
            //lock (obj)
            //{
                if (clients.Count < 1)
                    return "没有连接的[TCPIP]客户，无法发送数据";

                clients.ForEach(x =>
                {
                    try
                    {
                        if (x.client.Connected)
                            x.Send(message);
                    }
                    catch (Exception exc)
                    {
                        XLog.Error(exc);
                    }
                });
            //}

            return string.Empty;
        }

        public bool Disconnect()
        {
            clients.ForEach(x => x.Disconnect());
            onConnectedHandle?.Invoke($"{this.IP} {this.Port} 连接已断开", null);

            return true;
        }

        public void Dispose()
        {
        }

        public bool Connect()
        {
            throw new NotImplementedException();
        }
    }
}
