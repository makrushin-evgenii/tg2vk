using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class ReadAllIncomingMessages : LongPollUpdate
    {
        static ReadAllIncomingMessages()
        {
            RegisterType(6, typeof (ReadAllIncomingMessages));
        }

        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static ReadAllIncomingMessages FromJson(VkResponseArray response)
        {
            var readAllIncomingMessages = new ReadAllIncomingMessages();
            readAllIncomingMessages.InitializeStandartFields(response);
            return readAllIncomingMessages;
        }
    }
}
