using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class FriendBecameOnline : LongPollUpdate
    {
        static FriendBecameOnline()
        {
            RegisterType(8, typeof (FriendBecameOnline));
        }
        
        public long UserId { get; private set; }
        public ulong Extra { get; private set; }
        public ulong Ts { get; private set; }

        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static FriendBecameOnline FromJson(VkResponseArray response)
        {
            var friendBecameOnline = new FriendBecameOnline();
            friendBecameOnline.InitializeStandartFields(response);
            friendBecameOnline.UserId = -((long) response[1]);
            friendBecameOnline.Extra = (ulong) response[2];
            friendBecameOnline.Ts = (ulong) response[3];
            return friendBecameOnline;
        }
    }
}