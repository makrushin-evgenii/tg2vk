using System;
using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    /// <summary>
    /// Событие нового сообщения
    /// </summary>
    [Serializable]
    public class NewMessage : LongPollUpdate
    {
        static NewMessage()
        {
            RegisterType(4, typeof (NewMessage));
        }

        /// <summary>
        /// Идентификатор сообщения.
        /// </summary>
        public ulong MsgId { get; set; }

        /// <summary>
        /// Флаги сообщения
        /// </summary>
        public ulong Flags { get; set; }
        
        /// <summary>
        /// идентификатор назначения.
        /// </summary>
        /// <remarks>
        /// Для пользователя: id пользователя.
        /// Для групповой беседы: 2000000000 + id беседы.
        /// Для сообщества: -id сообщества.
        /// </remarks>
        public ulong PeerId { get; set; }

        /// <summary>
        /// Время отправки сообщения в Unixtime.
        /// </summary>
        public ulong Ts { get; set; }

        /// <summary>
        /// Текст сообщения.
        /// </summary>
        public string Text { get; set; }


        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static NewMessage FromJson(VkResponseArray response)
        {
            var newMessage = new NewMessage();
            newMessage.InitializeStandartFields(response);
            newMessage.MsgId = (ulong) response[1];
            newMessage.Flags = (ulong)response[2];
            newMessage.PeerId = (ulong) response[3];
            newMessage.Ts = (ulong) response[4];
            newMessage.Text = (string) response[5];

            return newMessage;
        }
    }
}