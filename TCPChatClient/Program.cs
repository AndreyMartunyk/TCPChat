using System;
using System.Net.Sockets;
using System.Text;
using ChatServerCommands;

namespace TCPChat
{
    class Program
    {
        static string userName;
        private static string host;
        private static int port;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.WindowWidth = 65;


            Console.Write("Введите ip: ");
            host = "127.0.0.1";         // just for test
            //host = Console.ReadLine();
            Console.Write("Введите port: ");
            port = 8888;                // just for test
            //while (!int.TryParse(Console.ReadLine(), out port))
            //{
            //    Console.WriteLine("Неверно введен порт! Введите порт: ");
            //}
            try
            {
                bool connected = false;
                //цикл подключения клиента

               

                do
                {
                    client = new TcpClient();

                    client.Connect(host, port); //подключение клиента
                    stream = client.GetStream(); // получаем поток

                    Console.Write("Введите свое имя: ");
                    userName = Console.ReadLine();

                    SendMessageToServer(Commands.ConnectToServer, userName);

                    string responseMessage = GetResponse();
                    Commands responseCommand = ServerCommands.TextToCommand(responseMessage);
                    if (responseCommand == Commands.AcceptConnection)
                    {
                        connected = true;
                        Console.WriteLine("Добро пожаловать, {0}!", userName);
                    }
                    else
                    {
                        Console.WriteLine(responseMessage);
                        Disconnect();
                        Console.WriteLine("Try again: ");
                    }

                } while (!connected);

                Console.WriteLine("Введите сообщение: ");

                //цикл переписок
                while (true)
                {
                   
                    string message;

                   
                    //если пользователь начинает вводить сообщение
                    if (Console.KeyAvailable)
                    {
                        message = Console.ReadLine();
                        Commands command = CheckTextToCommand(message);
                        SendMessageToServer(command, message);
                        if (command == Commands.Quit)
                        {
                            Disconnect();
                        }
                    }


                    //если есть сообщение от сервера
                    if (stream.DataAvailable)
                    {
                        string response = GetResponse();
                        Commands command = ServerCommands.TextToCommand(response, out string restText);
                        switch (command)
                        {
                            case Commands.NewChatMessage:
                                ShowMessage(restText.Trim());
                                break;
                            case Commands.Message_OK:
                                //тут может быть логика успешной оправки сообщения
                                break;
                            default:
                                Console.WriteLine("UNKNOW COMMAND!!!");
                                break;
                        }

                    }

                    System.Threading.Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        static void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        static string GetResponse()
        {
            StringBuilder responseMessage = new StringBuilder();
            try
            {
                var buffer = new byte[1024];
                int bytesRead = 0;

                do
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    responseMessage.Append(Encoding.Unicode.GetString(buffer, 0, bytesRead));
                } while (stream.DataAvailable);

            }
            catch (Exception e)
            {
                //если вываливается с ошибкой, то отключаем
                Console.WriteLine(e);
                Disconnect();
            }

            return responseMessage.ToString();
        }

        static void SendMessageToServer(Commands command, string message = "")
        {
            message = ServerCommands.CommandToText(command) + message;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        static void Disconnect()
        {
            Console.WriteLine("Подключение прервано!"); //соединение было прервано
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            //Environment.Exit(0); //завершение процесса
        }

        //если команд станет больше нужно будет вывести команды и их приведение к Commands в библиотеку ChatServerCommands
        static Commands CheckTextToCommand(string text)
        {
            Commands res = Commands.NewChatMessage;
            string quitCommand = ".quit";

            if (text.Length >= quitCommand.Length && text.Substring(0, quitCommand.Length) == quitCommand)
            {
                res = Commands.Quit;
            }

            return res;
        }

  
    }
}
