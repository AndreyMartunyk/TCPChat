using System;
using System.Threading;

namespace TCPChat
{
    class Program
    {
        static ServerObject server; // сервер

        static void Main(string[] args)
        {
            Console.WindowWidth = 80;

            try
            {
                server = new ServerObject();
                server.Listen();
            }
            catch (Exception e)
            {
                server.Disconnect();
                Console.WriteLine(e.Message);
            }
        }
    }
}
