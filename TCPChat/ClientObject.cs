using System;
using System.Net.Sockets;
using System.Text;

namespace TCPChat
{
    public class ClientObject
    {
        public string Id { get; private set; }
        public NetworkStream Stream { get; private set; }
        public string UserName { get; private set; }
        public TcpClient Client { get; private set; }

        public ClientObject(TcpClient tcpClient, string name)
        {
            Id = Guid.NewGuid().ToString();
            Client = tcpClient;
            Stream = tcpClient.GetStream();
            UserName = name;
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder message = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                message.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return message.ToString();
        }

        // закрытие подключения
        public void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (Client != null)
                Client.Close();
        }
    }
}
