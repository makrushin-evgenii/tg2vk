using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class ReadAllOutcomingMessages : LongPollUpdate
    {
        static ReadAllOutcomingMessages()
        {
            RegisterType(7, typeof (ReadAllOutcomingMessages));
        }

        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static ReadAllOutcomingMessages FromJson(VkResponseArray response)
        {
            var readAllOutcomingMessages = new ReadAllOutcomingMessages();
            readAllOutcomingMessages.InitializeStandartFields(response);
            return readAllOutcomingMessages;
        }
    }
}