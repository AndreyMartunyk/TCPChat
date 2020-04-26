using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerCommands
{

    public static class ServerCommands
    {
        public const string SEPARATOR = "===>>>";

        public static string CommandToText(Commands command)
        {

            return command.ToString() + SEPARATOR;
        }
        public static Commands TextToCommand(string data)
        {
            Commands result = Commands.None;

            int separatorIndex = data.IndexOf(SEPARATOR);
            //Console.WriteLine("separatorIndex = {0}", separatorIndex);

            if (separatorIndex >= 0)
            {
                string command = data.Substring(0, separatorIndex);
                Enum.TryParse<Commands>(command, out result);
            }

            return result;
        }


        public static Commands TextToCommand(string data, out string restText)
        {
            Commands result = Commands.None;
            restText = "";

            int separatorIndex = data.IndexOf(SEPARATOR);
            //Console.WriteLine("separatorIndex = {0}", separatorIndex);

            if (separatorIndex >= 0)
            {
                string command = data.Substring(0, separatorIndex);
                Enum.TryParse<Commands>(command, out result);

                restText = data.Substring(separatorIndex + SEPARATOR.Length).Trim();
                
            }

            return result;
        }





    }


}
