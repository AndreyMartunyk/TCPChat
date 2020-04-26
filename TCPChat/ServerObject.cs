using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using ChatServerCommands;
namespace TCPChat
{
    public class ServerObject
    {
        static TcpListener server; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        int _port;

        public ServerObject(int port = 8888)
        {
            _port = port;
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
            BroadcastMessage(clientObject, "вошел в чат!");

        }
        protected internal void RemoveConnection(string id)
        {
            ClientObject client = GetClient(id); 
            if (client != null)
            {
                clients.Remove(client);
                BroadcastMessage(client, "вышел из чата чата!");
            }
        }
        private ClientObject GetClient (string id)
        {
            ClientObject res = null;

            foreach (var client in clients)
            {
                if (client.Id == id)
                {
                    res = client;
                    break;
                }
            }

            return res;
        }

        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                var port = _port;
                var localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("Сервер запущен");
                var tempClient = new TcpClient();

                while (true)
                {
                    if(server.Pending() == true)
                    {
                        tempClient = server.AcceptTcpClient();
                        var data = ReadMessage(tempClient);
                        if (ServerCommands.TextToCommand(data.ToString(), out string restText) == Commands.ConnectToServer)
                        {
                            if (isUniqueName(restText))
                            {
                                ClientObject client = new ClientObject(tempClient, restText);
                                AddConnection(client);
                                SendMessage(Commands.AcceptConnection, "", client.Stream);
                                Console.WriteLine("Enter connection from {0}", tempClient.Client.RemoteEndPoint);
                            }
                            else
                            {
                                SendMessage(Commands.DiscardConnection, "Выберите другое имя", tempClient.GetStream());
                                tempClient.Close();
                            }
                        }
                    }
                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].Client.Available > 0)
                        {

                            var data = ReadMessage(clients[i].Client);
                            string clientsMessage;
                            Commands clientCommand = ServerCommands.TextToCommand(data.ToString(), out clientsMessage);

                            string response = "";
                            switch (clientCommand)
                            {
                                case Commands.None:
                                    clientCommand = Commands.BadRequest;
                                    response = "не знаю такую команду";
                                    break;
                                case Commands.NewChatMessage:
                                    clientCommand = Commands.Message_OK;
                                    BroadcastMessage(clients[i], clientsMessage);
                                    break;
                                case Commands.ConnectToServer:
                                    clientCommand = Commands.BadRequest;
                                    response = "Вы уже вошли в чат";
                                    break;
                                case Commands.Quit:
                                    clients[i].Close();
                                    RemoveConnection(clients[i].Id);
                                    --i;
                                    continue;
                                default:
                                    clientCommand = Commands.BadRequest;
                                    response = "Не известная или не уместная команда!!";
                                    break;
                            }
                            SendMessage(clientCommand, response, clients[i].Stream);
                        }
                    }

                    System.Threading.Thread.Sleep(200);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();

                Disconnect();
            }
        }

        // трансляция сообщения подключенным клиентам
        private void BroadcastMessage(ClientObject client, string message)
        {
            message = String.Format("{0}: {1}", client.UserName, message);

            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id != client.Id) // если id клиента не равно id отправляющего
                {
                    SendMessage(Commands.NewChatMessage, message, clients[i].Stream, false);    //передача данных без экранирования на сервере
                }
            }
            Console.WriteLine("Server broadcasted: {0}", message);
        }

        /// <summary>
        /// Пересылка сообщения клиенту
        /// </summary>
        /// <param name="command">команда которую хотим переслать клиенту</param>
        /// <param name="message">сообщение прилагаемое к команде</param>
        /// <param name="stream">поток в который будем отправлять</param>
        /// <param name="consoled">Нужно ли экранировать сообщение в консоль сервера. По умолчанию - нужно</param>
        private void SendMessage(Commands command, string message, NetworkStream stream, bool consoled = true)
        {
            message = ServerCommands.CommandToText(command) + message;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
            if (consoled)
            {
                Console.WriteLine("Server sent: {0}", message.Trim());
            }
        }

        private StringBuilder ReadMessage(TcpClient client)
        {
            var bytes = new byte[256];

            NetworkStream stream = client.GetStream();
            var bytesReadCount = 0;
            var data = new StringBuilder();
            do
            {
                bytesReadCount = stream.Read(bytes, 0, bytes.Length);
                data.Append(Encoding.Unicode.GetString(bytes, 0, bytesReadCount));
            } while (client.Available > 0);

            Console.WriteLine("Server read: {0}", data);
            return data;
        }

        // отключение всех клиентов
        public void Disconnect()
        {
            server.Stop(); 

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); 
            }
            Console.ReadKey();
            Environment.Exit(0); 
        }

        private bool isUniqueName(string name)
        {
            bool success = true;
            foreach (var client in clients)
            {
                if (name == client.UserName)
                {
                    success = false;
                    break;
                }
            }

            return success;
        }

        
    }
}
