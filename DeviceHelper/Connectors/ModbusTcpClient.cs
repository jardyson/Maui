using System.Net.Sockets;

namespace DeviceHelper.Connectors
{
    public class ModbusTcpClient
    {
        private readonly string _ip;
        private readonly int _port;
        private TcpClient _tcpClient;

        public ModbusTcpClient(string ip, int port = 502)
        {
            _ip = ip;
            _port = port;
            _tcpClient = new TcpClient();
        }

        public async Task ConnectAsync()
        {
            await _tcpClient.ConnectAsync(_ip, _port);
        }

        public async Task SendModbusCommandAsync(byte[] data)
        {
            if (!_tcpClient.Connected)
                throw new InvalidOperationException("Not connected to the device.");

            var stream = _tcpClient.GetStream();
            await stream.WriteAsync(data, 0, data.Length);

            // 接收响应（可选）
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            var response = new byte[bytesRead];
            Array.Copy(buffer, response, bytesRead);
            Console.WriteLine($"Response: {BitConverter.ToString(response)}");
        }

        public void Disconnect()
        {
            _tcpClient?.Close();
        }
    }
}
