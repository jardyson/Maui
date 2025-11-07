using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeviceHelper.Connectors
{
    public class TcpIpAccess:BaseModbus<TcpClient, int, int>
    {
        private TcpClient client;
        private TcpListener server;
        private Thread serverThread;
        private bool isRunning;

        public override bool IsConnected => client == null ? false : client.Connected;

        public override bool IsActive { get; set; } = true;

        public TcpIpAccess(string code, string name,string ip, int port)
        {
            this.code = code;
            this.Name = name;
            this.IP = IPAddress.Parse(ip);
            this.Port = port;

            NetworkStream stream = null;
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        if (client!=null && client.Connected)
                        {
                            // 获取网络流
                            stream = client.GetStream();
                            // 接收数据
                            byte[] buffer = new byte[1024];
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            var a = Convert.ToString(buffer[3], 2).PadLeft(8, '0');
                            // 处理接收到的数据
                            //string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            onMsgHandle(a);
                        }
                        Thread.Sleep(200);
                    }
                    catch (Exception exc)
                    {
                        onMsgHandle?.Invoke(exc.Message);
                    }
                }
            });
        }

        // 客户端连接方法
        public override bool Connect()
        {
            try
            {
                this.client = new TcpClient();
                this.client.Connect(IP, Port);
                onConnectedHandle?.Invoke($"连接成功{IP}:{Port}", null);
            }
            catch (Exception ex)
            {
                onConnectedHandle?.Invoke($"连接失败{IP}:{Port}\r\n{ex.Message}",null);
            }

            return true;
        }
        public override bool Disconnect()
        {
            try
            {
                this.client?.EndConnect(null);
            }
            catch(Exception exc) {
                onDisconnectHandle?.Invoke(exc.Message, null);
                return false;
            }

            return true;
        }
        public void Dispose()
        {
            client?.Dispose();
        }

        // 客户端发送消息方法
        public void SendMessage(byte[] data)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    //byte[] data = Encoding.UTF8.GetBytes(message);
                    NetworkStream stream = client.GetStream();
                    stream.Write(data, 0, data.Length);
                    var strs = string.Empty;
                    foreach (var item in data)
                        strs += item.ToString();
                    onSendHandle?.Invoke($" has send: {strs}", null);
                }
                catch (Exception ex)
                {
                    onSendHandle?.Invoke($"发送消息出错: {data} {ex.Message}", null);
                }
            }
        }

        // 客户端接收消息方法
        public string ReceiveMessage()
        {
            if (client != null && client.Connected)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    onMsgHandle?.Invoke($"接收到消息: {message}");
                    return message;
                }
                catch (Exception ex)
                {
                    onMsgHandle?.Invoke($"接收消息出错: {ex.Message}");
                }
            }
            return null;
        }

        // 客户端关闭连接方法
        public void CloseClient()
        {
            if (client != null)
            {
                client.Close();
                onMsgHandle?.Invoke($"客户端连接已关闭: {this.IP}:{this.Port}");
            }
        }

        // 服务器启动方法
        public void StartServer(int port)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, port);
                server.Start();
                isRunning = true;
                serverThread = new Thread(HandleServerConnections);
                serverThread.Start();
                Console.WriteLine("服务器已启动，等待连接...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动服务器出错: {ex.Message}");
            }
        }

        // 服务器处理连接方法
        private void HandleServerConnections()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("有客户端连接");
                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        Console.WriteLine($"处理连接出错: {ex.Message}");
                    }
                }
            }
        }

        // 服务器处理客户端方法
        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"收到客户端消息: {message}");

                // 回显消息给客户端
                byte[] response = Encoding.UTF8.GetBytes("消息已收到");
                stream.Write(response, 0, response.Length);

                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理客户端出错: {ex.Message}");
            }
        }

        // 服务器停止方法
        public void StopServer()
        {
            isRunning = false;
            if (server != null)
            {
                server.Stop();
                Console.WriteLine("服务器已停止");
            }
            if (serverThread != null && serverThread.IsAlive)
            {
                serverThread.Join();
            }
        }
    }
}
