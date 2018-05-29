using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class FriendBecameOffline : LongPollUpdate
    {
        static FriendBecameOffline()
        {
            RegisterType(9, typeof (FriendBecameOffline));
        }
        
        public long UserId { get; private set; }
        public ulong Extra { get; private set; }
        public ulong Ts { get; private set; }

        /// <summary> Разобрать из json. </summary>
        /// <param name="response">Ответ сервера.</param>
        /// <returns></returns>
        public static FriendBecameOffline FromJson(VkResponseArray response)
        {
            var friendBecameOffline = new FriendBecameOffline();
            friendBecameOffline.InitializeStandartFields(response);
            friendBecameOffline.UserId = -((long) response[1]);
            friendBecameOffline.Extra = (ulong) response[2];
            friendBecameOffline.Ts = (ulong) response[3];
            
            return friendBecameOffline;
        }
    }
}