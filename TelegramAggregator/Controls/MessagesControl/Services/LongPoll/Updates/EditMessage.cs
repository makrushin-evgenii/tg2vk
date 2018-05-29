using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class EditMessage : LongPollUpdate
    {
        static EditMessage()
        {
            RegisterType(5, typeof (EditMessage));
        }

        public ulong MessageId { get; private set; } 
        public ulong Mask { get; private set; } 
        public ulong PeerId { get; private set; } 
        public ulong Ts { get; private set; } 
        public string NewText { get; private set; } 
        
        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static EditMessage FromJson(VkResponseArray response)
        {
            var editMessage = new EditMessage();
            editMessage.InitializeStandartFields(response);
            editMessage.MessageId = (ulong) response[1];
            editMessage.Mask = (ulong) response[2];
            editMessage.PeerId = (ulong) response[3];
            editMessage.Ts = (ulong) response[4];
            editMessage.NewText = (string) response[5];

            return editMessage;
        }
    }
}