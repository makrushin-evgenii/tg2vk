using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Model.Entities;
using TelegramAggregator.Model.Repositories;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using Document = VkNet.Model.Attachments.Document;

namespace TelegramAggregator.Controls.MessagesControl
{
    public class SendMessagesHandler : IUpdateHandler
    {
        private readonly AggregatorBotConfiguration _aggregatorBotConfiguration;
        private readonly IBotUserRepository _botUserRepository;

        public SendMessagesHandler(IBotUserRepository botUserRepository,
            AggregatorBotConfiguration aggregatorBotConfiguration)
        {
            _botUserRepository = botUserRepository;
            _aggregatorBotConfiguration = aggregatorBotConfiguration;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return update.Type == UpdateType.Message && 
                   _botUserRepository.GetByTelegramId(update.Message.Chat.Id).VkAccount != null;; 
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var message = update.Message;
            var botUser = _botUserRepository.GetByTelegramId(message.Chat.Id);
            if (botUser.VkAccount == null)
            {
                throw new ArgumentNullException();
            }

            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            try
            {
                var messagesSendParams = await GetMessagesSendParams(bot, botUser, message, vkApi);
                var sendedMsgId = await vkApi.Messages.SendAsync(messagesSendParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                await bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Ошибка доставки сообщения::{e.Message}",
                    replyToMessageId: message.MessageId);
            }

            return UpdateHandlingResult.Handled;
        }

        private static async Task<MessagesSendParams> GetMessagesSendParams(IBot bot, BotUser botUser, Message message, VkApi vkApi)
        {
            var messagesSendParams = new MessagesSendParams
            {
                PeerId = botUser.VkAccount.CurrentPeer
            };
            
            switch (message.Type)
            {
                case MessageType.Text:
                    messagesSendParams.Message = message.Text;
                    break;
                case MessageType.Document:
                    await GetDocumentMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Photo:
                    // TODO: !!!
                    await GetPhotoMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Sticker:
                    // TODO: !!!
                    await GetStickerMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Audio:
                    // TODO: !!!
                    await GetAudioMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Voice:
                    await GetVoiceMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Video:
                    // TODO: !!!
                    await GetVideoMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.VideoNote:
                    await GetVideoNoteMsgSendParams(bot, message, messagesSendParams, vkApi);
                    break;
                case MessageType.Location:
                    var location = message.Location;
                    messagesSendParams.Message = $"Latitude: {location.Latitude}\r\nLongitude: {location.Longitude}";
                    break;
                case MessageType.Contact:
                    var contact = message.Contact;
                    messagesSendParams.Message = $"{contact.FirstName} {contact.LastName}\r\n{contact.PhoneNumber}";
                    break;
                case MessageType.Service:
                    //TODO: тут можно обрабатывать удаленные и отредактированные сообщения
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (message.IsForwarded || message.ForwardFromChat != null)
            {
                MarkAsForwarded(message, messagesSendParams);
            }

            return messagesSendParams;
        }

        private static void MarkAsForwarded(Message message, MessagesSendParams messagesSendParams)
        {
            var msgOwnerLink = message.ForwardFromChat != null
                ? $"t.me/{message.ForwardFromChat.Username}"
                : $"t.me/{message.ForwardFrom.Username}";

            messagesSendParams.Message = $"Пересланное сообщение от {msgOwnerLink}\r\n" + messagesSendParams.Message;
        }

        private static async Task GetPhotoMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var photoSize = message.Photo
                .OrderBy(size => size.FileSize)
                .LastOrDefault();
            
            var photoFile = await bot.Client.GetFileAsync(photoSize?.FileId);
            messagesSendParams.Message = message.Caption;
            messagesSendParams.Attachments = new[] {UploadPhotoToVk(bot, vkApi, photoFile)};
        }
        
        private static Photo UploadPhotoToVk(IBot bot, VkApi vkApi, File photoFile)
        {
            var wc = new WebClient();
            wc.DownloadFile(
                $"https://api.telegram.org/file/bot{bot.Options.ApiToken}/{photoFile.FilePath}",
                "devnull/" + photoFile.FilePath);

            var uploadServer = vkApi.Photo.GetMessagesUploadServer(1);

            var responce = wc.UploadFile(uploadServer.UploadUrl, "devnull/" + photoFile.FilePath);

            var responseImg = Encoding.ASCII.GetString(responce);

            return vkApi.Photo.SaveMessagesPhoto(responseImg).First();
        }
        
        private static async Task GetStickerMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var stickerFile = await bot.Client.GetFileAsync(message.Sticker.Thumb.FileId);

            try
            {
                messagesSendParams.Message = message.Caption;
                messagesSendParams.Attachments = new[] { UploadPhotoToVk(bot, vkApi, stickerFile) };
            }
            catch (Exception e)
            {
                messagesSendParams.Message = message.Caption + message.Sticker.Emoji;
                Console.WriteLine(e);
            }
        }

        private static async Task GetAudioMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var audioFile = await bot.Client.GetFileAsync(message.Audio.FileId);
            messagesSendParams.Attachments = new[] { await UploadDocumentToVk(bot, vkApi, audioFile, audioFile.FilePath) };
            messagesSendParams.Message = message.Caption;
        }
        
        private static async Task GetVoiceMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var voiceFile = await bot.Client.GetFileAsync(message.Voice.FileId);
            messagesSendParams.Attachments = new[] { await UploadDocumentToVk(bot, vkApi, voiceFile, voiceFile.FilePath) };
            messagesSendParams.Message = message.Caption;
        }
        
        private static async Task GetVideoMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var videoFile = await bot.Client.GetFileAsync(message.Video.FileId);
            messagesSendParams.Attachments = new[] { await UploadDocumentToVk(bot, vkApi, videoFile, videoFile.FilePath) };
            messagesSendParams.Message = message.Caption;
        }
        
        private static async Task GetVideoNoteMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var videoNoteFile = await bot.Client.GetFileAsync(message.VideoNote.FileId);
            messagesSendParams.Attachments = new[] { await UploadDocumentToVk(bot, vkApi, videoNoteFile, videoNoteFile.FilePath) };
            messagesSendParams.Message = message.Caption;
        }
        
        private static async Task GetDocumentMsgSendParams(IBot bot, Message message, MessagesSendParams messagesSendParams, VkApi vkApi)
        {
            var document = message.Document;
            var documentFile = await bot.Client.GetFileAsync(message.Document.FileId);
            messagesSendParams.Attachments = new[] { await UploadDocumentToVk(bot, vkApi, documentFile, document.FileName) };
            messagesSendParams.Message = message.Caption;
        }

        private static async Task<Document> UploadDocumentToVk(IBot bot, VkApi vkApi, File documentFile, string title = "doc")
        {
            var wc = new WebClient();
            wc.DownloadFile(
                $"https://api.telegram.org/file/bot{bot.Options.ApiToken}/{documentFile.FilePath}",
                "devnull/" + documentFile.FilePath);

            var uploadServer = vkApi.Docs.GetUploadServer();
            var responce = wc.UploadFile(uploadServer.UploadUrl, "devnull/" + documentFile.FilePath);
            var responseJson = Encoding.ASCII.GetString(responce);
            
            return vkApi.Docs.Save(responseJson, title).FirstOrDefault();
        }
    }
}