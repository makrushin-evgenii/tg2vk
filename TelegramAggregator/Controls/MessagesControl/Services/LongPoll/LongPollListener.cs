using System;
using System.Threading.Tasks;
using VkNet;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class LongPollListener
    {
        public delegate void EditMessageDelegate(VkApi owner, EditMessage editMessage);

        public delegate void FriendBecameOfflineDelegate(VkApi owner, FriendBecameOffline friendBecameOffline);

        public delegate void FriendBecameOnlineDelegate(VkApi owner, FriendBecameOnline friendBecameOnline);

        public delegate void NewMessageDelegate(VkApi owner, NewMessage newMessage);

        public delegate void ReadAllIncomingMessagesDelegate(VkApi owner, ReadAllIncomingMessages readAllIncomingMessages);

        public delegate void ReadAllOutcomingMessagesDelegate(VkApi owner, ReadAllOutcomingMessages readAllOutcomingMessages);

        public delegate void UserTypingInPrivateDialogDelegate(VkApi owner, UserTypingInPrivateDialog userTypingInPrivateDialog);

        private readonly EditMessageDelegate _editMessage;
        private readonly FriendBecameOfflineDelegate _friendBecameOffline;
        private readonly FriendBecameOnlineDelegate _friendBecameOnline;
        private readonly NewMessageDelegate _newMessage;
        private readonly ReadAllIncomingMessagesDelegate _readAllIncomingMessages;
        private readonly ReadAllOutcomingMessagesDelegate _readAllOutcomingMessages;
        private readonly UserTypingInPrivateDialogDelegate _userTypingInPrivateDialog;


        private readonly VkApi _vkApi;
        private Task _listeningTask;
        private bool _taskIsActive;

        public LongPollListener(VkApi vkApi, 
            EditMessageDelegate editMessage = null, 
            FriendBecameOfflineDelegate friendBecameOffline = null, 
            FriendBecameOnlineDelegate friendBecameOnline = null, 
            NewMessageDelegate newMessage = null, 
            ReadAllIncomingMessagesDelegate readAllIncomingMessages = null, 
            ReadAllOutcomingMessagesDelegate readAllOutcomingMessages = null, 
            UserTypingInPrivateDialogDelegate userTypingInPrivateDialog = null
        )
        {
            _vkApi = vkApi;
            _newMessage = newMessage;
            _editMessage = editMessage;
            _friendBecameOffline = friendBecameOffline;
            _friendBecameOnline = friendBecameOnline;
            _readAllIncomingMessages = readAllIncomingMessages;
            _readAllOutcomingMessages = readAllOutcomingMessages;
            _userTypingInPrivateDialog = userTypingInPrivateDialog;
        }

        public void Start()
        {
            _taskIsActive = true;
            _listeningTask = new Task(ListenToLongPoll);
            _listeningTask.Start();

            Console.WriteLine("Прослушивание включено");
        }

        public void Stop()
        {
            if (_taskIsActive == false)
            {
                return;
            }

            _taskIsActive = false;
            _listeningTask.Wait();

            Console.WriteLine("Прослушивание остановлено");
        }

        private async void ListenToLongPoll()
        {
            var longPoll = new LongPoll(_vkApi, _vkApi.Messages.GetLongPollServer());

            while (_taskIsActive)
            {
                var updates = Updates.FromJson(await longPoll.GetUpdatesResponce());

                if (updates == null)
                {
                    continue;
                }
                
                foreach (var update in updates)
                {
                    if (update == null)
                        continue;
                    
                    switch (update.Instance)
                    {
                        case EditMessage editMessage:
                            _editMessage?.Invoke(_vkApi, editMessage);
                            break;
                        case FriendBecameOffline friendBecameOffline:
                            _friendBecameOffline?.Invoke(_vkApi, friendBecameOffline);
                            break;
                        case FriendBecameOnline friendBecameOnline:
                            _friendBecameOnline?.Invoke(_vkApi, friendBecameOnline);
                            break;
                        case NewMessage newMessage:
                            _newMessage?.Invoke(_vkApi, newMessage);
                            break;
                        case ReadAllIncomingMessages readAllIncomingMessages:
                            _readAllIncomingMessages?.Invoke(_vkApi, readAllIncomingMessages);
                            break;
                        case ReadAllOutcomingMessages readAllOutcomingMessages:
                            _readAllOutcomingMessages?.Invoke(_vkApi, readAllOutcomingMessages);
                            break;
                        case UserTypingInPrivateDialog userTypingInPrivateDialog:
                            _userTypingInPrivateDialog?.Invoke(_vkApi, userTypingInPrivateDialog);
                            break;
                    }
                }
            }
        }
    }
}