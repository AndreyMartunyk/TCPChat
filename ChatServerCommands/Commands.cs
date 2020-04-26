using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerCommands
{
    public enum Commands
    {
        None = 0,
        //подписываю цифрами что бы в случае удаления какой-либо команды не пришлось менять логику везде из-за смещения индексов
        ConnectToServer = 1,    //запрос на подключение к серверу
        AcceptConnection = 2,   //положительный ответ на поключение к серверу
        DiscardConnection = 3,  //отрицательный ответ на подключение к серверу
        NewChatMessage = 4,     //новое сообщение в чате
        BadRequest = 5,         //сообщение о не правильной/не подходящей команде
        Message_OK = 6,         //сообщение о том что его сообщение отправлено
        Quit = 7,               //сообщение об выходе
    }
}
