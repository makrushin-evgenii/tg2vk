using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    /// <summary>
    /// Пользователь $user_id набирает текст в диалоге. 
    /// </summary>
    /// <remarks>
    /// Событие приходит раз в ~5 секунд при наборе текста. $flags = 1. 
    /// </remarks>
    public class UserTypingInPrivateDialog : LongPollUpdate
    {
        static UserTypingInPrivateDialog()
        {
            RegisterType(61, typeof (UserTypingInPrivateDialog));
        }
        
        public ulong UserId { get; private set; }
        public ulong Flags { get; private set; }

        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static UserTypingInPrivateDialog FromJson(VkResponseArray response)
        {
            var userTyping = new UserTypingInPrivateDialog();
            userTyping.InitializeStandartFields(response);
            userTyping.UserId = (ulong) response[1];
            userTyping.Flags = (ulong) response[2];
            return userTyping;
        }
    }
}