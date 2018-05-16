using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAggregator.Controls.MessagesControl.Common;
using TelegramAggregator.Model.Entities;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

namespace TelegramAggregator.Controls.MessagesControl.Services.NotificationsService
{
    public class NotificationsService : INotificationsService
    {
        private const int LongPoolWait = 20;
        private const int LongPoolMode = 2;
        private const int LongPoolVersion = 2;

        private readonly AggregatorBot _bot;
        private readonly Dictionary<long, bool> _listeningTaskIsActive;

        public NotificationsService(AggregatorBot bot)
        {
            _bot = bot;
            _listeningTaskIsActive = new Dictionary<long, bool>();
        }

        public async Task EnableNotifications(BotUser botUser)
        {
            var userTelegramId = botUser.TelegramUserId;
            if (_listeningTaskIsActive.ContainsKey(userTelegramId) && _listeningTaskIsActive[userTelegramId])
            {
                throw new ArgumentException(
                    $"Получение уведомлений для пользователя tgid{userTelegramId} уже включено");
            }

            if (botUser.VkAccount == null)
            {
                throw new ArgumentNullException(nameof(botUser.VkAccount), "Необходима авторизация Вконтакте");
            }

            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            await Task.Factory.StartNew(async () =>
            {
                var client = new HttpClient();
                var longPollServer = vkApi.Messages.GetLongPollServer();
                var ts = longPollServer.Ts;

                _listeningTaskIsActive[userTelegramId] = true;
                while (_listeningTaskIsActive[userTelegramId])
                {
                    var updateResponse = await client
                        .GetAsync(
                            $"https://{longPollServer.Server}?act=a_check&key={longPollServer.Key}&ts={ts}&wait={LongPoolWait}&mode={LongPoolMode}&version={LongPoolVersion}");
                    var jsoned = await updateResponse.Content.ReadAsStringAsync();
                    var updates = JsonConvert.DeserializeObject<JObject>(jsoned);

                    var longPollHistory = await vkApi.Messages.GetLongPollHistoryAsync(
                        new MessagesGetLongPollHistoryParams
                        {
                            Ts = ts
                        });

                    foreach (var message in longPollHistory.Messages)
                    {
                        DeliverMessageToUser(botUser, vkApi, message);
                    }

                    ts = updates["ts"].ToObject<ulong>();
                }
            });
        }

        public void DisableNotifications(BotUser botUser)
        {
            if (!_listeningTaskIsActive.ContainsKey(botUser.TelegramUserId))
            {
                return;
            }

            _listeningTaskIsActive[botUser.TelegramUserId] = false;
        }

        private async Task DeliverMessageToUser(BotUser botUser, VkApi vkApi, Message message)
        {
            var heading = GetHeading(message, vkApi);

            var replyMarkup = GetReplyMarkup(message);

            await _bot.Client.SendTextMessageAsync(
                botUser.TelegramUserId,
                $"`{heading}`\r\n{message.Body}",
                ParseMode.Markdown,
                replyMarkup: replyMarkup
            );


            await DeliverAttachmentsToUser(botUser, vkApi, message.Attachments);

            foreach (var forwardedMessage in message.ForwardedMessages)
            {
                await DeliverForwardedMessageToUser(botUser, forwardedMessage, vkApi);
            }
        }

        private static string GetHeading(Message message, VkApi vkApi)
        {
            var heading = "";

            var outgoingMessage = (message.Out.HasValue && message.Out.Value);
            var groupChatMessage = message.ChatId.HasValue;

            var user = GetNameById(vkApi, message.UserId.Value);
            
            if (outgoingMessage && !groupChatMessage)
            {
                heading = $"Вы -> {user}";
            }

            if (!outgoingMessage && !groupChatMessage)
            {
                heading = $"{user} -> Вам";
            }

            if (outgoingMessage && groupChatMessage)
            {
                heading = $"Вы в беседе {message.Title}";
            }

            if (!outgoingMessage && groupChatMessage)
            {
                heading = $"{user} в беседе {message.Title}";
            }

            return heading;
        }

        private static User GetUserById(VkApi vkApi, long id)
        {
            var screenName = $"id{id}";

            var peers = vkApi.Users.Get(new[] {screenName}, ProfileFields.All, null,
                true);

            if (!peers.Any())
            {
                throw new ArgumentException($"Пользователей Вконтакте с именем {screenName} не найдено");
            }

            return peers.First();
        }
        
        private static string GetNameById(VkApi vkApi, long id)
        {
            if (id < 0)
            {
                var group = vkApi.Groups.GetById(null, (-id).ToString(), GroupsFields.Description).FirstOrDefault();
                return $"{group.Name}";
            }

            var user = GetUserById(vkApi, id);
            return $"{user.FirstName} {user.LastName}";
        }

        private async Task DeliverForwardedMessageToUser(BotUser botUser, Message message, VkApi vkApi)
        {
            await _bot.Client.SendTextMessageAsync(
                botUser.TelegramUserId,
                $"`Пересланное сообщение от {GetNameById(vkApi, message.UserId.Value)}`\r\n{message.Body}",
                ParseMode.Markdown
            );

            await DeliverAttachmentsToUser(botUser, vkApi, message.Attachments);

            foreach (var forwardedMessage in message.ForwardedMessages)
            {
                await DeliverForwardedMessageToUser(botUser, forwardedMessage, vkApi);
            }
        }

        private async Task DeliverAttachmentsToUser(BotUser botUser, VkApi vkApi, IEnumerable<Attachment> attachments)
        {
            var userTelegramId = botUser.TelegramUserId;
            
            foreach (var attachment in attachments)
            {
                //
                //
                //  НАИБОЛЕЕ ВАЖНЫЕ ТИПЫ ВЛОЖЕНИЙ
                //    стоит уделять больше внимания именно их обработке
                //    рассположены в порядке убывания "важности" на мой вкус
                //
                //

                if (attachment.Type == typeof(Photo))
                {
                    // OK
                    await DeliverPhotoToUser(userTelegramId, (Photo) attachment.Instance);
                }
                
                if (attachment.Type == typeof(Post))
                {
                    // OK
                    await DeliverPostToUser(botUser, (Post) attachment.Instance, vkApi);
                }

                if (attachment.Type == typeof(Audio))
                {
                    // OK
                    // TODO: Обходить ограничение на получение N аудиозаписей в сути
                    await DeliverAudioToUser(userTelegramId, (Audio) attachment.Instance);
                }
                
                if (attachment.Type == typeof(Video))
                {
                    // TODO: Не могу получить ссылку на плеер и видео в целом. 
                    // разобраться как достать простую ссылку на него
                    await DeliverVideoToUser(userTelegramId, (Video) attachment.Instance);
                }

                if (attachment.Type == typeof(Document))
                {
                    // OK
                    await DeliverDocumentToUser(userTelegramId, (Document) attachment.Instance);
                }
                
                if (attachment.Type == typeof(Sticker))
                {
                    // OK
                    await DeliverStickerToUser(userTelegramId, (Sticker) attachment.Instance);
                }

                //
                //
                //  МЕНЕЕ ВАЖНЫЕ ТИПЫ ВЛОЖЕНИЙ
                //    используются значительно реже. Поэтому с их обработкой
                //    не смысла возиться, пока есть проблеммы с "важными" вложениями
                //
                //

                if (attachment.Type == typeof(Graffiti))
                {
                    // OK
                    await DeliverGraffitiToUser(userTelegramId, (Graffiti) attachment.Instance);
                }

                if (attachment.Type == typeof(Link))
                {
                    // TODO: это что вообще и где найти?
                    await DeliverLinkToUser(userTelegramId, (Link) attachment.Instance);
                }

                if (attachment.Type == typeof(Note))
                {
                    // TODO: это что вообще?
                    await DeliverNoteToUser(userTelegramId, (Note) attachment.Instance, vkApi);
                }

                if (attachment.Type == typeof(Poll))
                {
                    await DeliverPollToUser(userTelegramId, (Poll) attachment.Instance, vkApi);
                }

                if (attachment.Type == typeof(Album))
                {
                    // TODO: отправлять обложку с подписью и ссылкой на весь альбом
                    await DeliverAlbumToUser(userTelegramId, (Album) attachment.Instance, vkApi);
                }

                if (attachment.Type == typeof(Gift))
                {
                    await DeliverGiftToUser(userTelegramId, (Gift) attachment.Instance);
                }

                if (attachment.Type == typeof(WallReply))
                {
                    await DeliverWallReplyToUser(userTelegramId, (WallReply) attachment.Instance, vkApi);
                }

                if (attachment.Type == typeof(Page))
                {
                    // TODO: это что вообще?
                    await DeliverPageToUser(userTelegramId, (Page) attachment.Instance);
                }

                if (attachment.Type == typeof(MarketAlbum))
                {
                    await DeliverMarketAlbumToUser(userTelegramId, (MarketAlbum) attachment.Instance);
                }

                if (attachment.Type == typeof(Market))
                {
                    await DeliverMarketToUser(userTelegramId, (Market) attachment.Instance);
                }
            }
        }

        private async Task DeliverMarketToUser(long userTelegramId, Market market)
        {
            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: Информация о продукте`\r\n{market.Title} {market.Description}");
        }

        private async Task DeliverMarketAlbumToUser(long userTelegramId, MarketAlbum marketAlbum)
        {
            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: Подборка товаров`\r\n{marketAlbum.Title}");
        }

        private async Task DeliverPageToUser(long userTelegramId, Page page)
        {
            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: Информация о вики-странице сообщества {page.GroupId}`\r\n{page.ViewUrl}",
                ParseMode.Markdown);
        }

        private async Task DeliverWallReplyToUser(long userTelegramId, WallReply wallReply, VkApi vkApi)
        {
            // TODO: чот не узнает парня
            var ownerName = wallReply.OwnerId.HasValue
                ? GetNameById(vkApi, wallReply.OwnerId.Value)
                : "Undefined";
            
            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: Комментарий {ownerName} к записи на стене`\r\n{wallReply.Text}",
                ParseMode.Markdown,
                replyMarkup:new InlineKeyboardMarkup(new []{InlineKeyboardButton.WithCallbackData($"{wallReply.Likes.Count} ❤️", "wrpl-like"), InlineKeyboardButton.WithCallbackData("Ответить", "wrpl-reply")})
            );
        }

        private async Task DeliverGiftToUser(long userTelegramId, Gift gift)
        {
            var giftThumbs = new[]
            {
                gift.Thumb256,
                gift.Thumb96,
                gift.Thumb48
            };
            var bestThumb = giftThumbs.FirstOrDefault(uri => uri != null);
            await _bot.Client.SendPhotoAsync(userTelegramId, new InputOnlineFile(bestThumb.ToString()));

        }

        private async Task DeliverStickerToUser(long userTelegramId, Sticker sticker)
        {
            var stickerSrcUris = new[]
            {
                sticker.Photo352,
                sticker.Photo256,
                sticker.Photo128,
                sticker.Photo64
            };
            var bestSrcUri = stickerSrcUris.FirstOrDefault(uri => uri != null);
            await _bot.Client.SendPhotoAsync(userTelegramId, new InputOnlineFile(bestSrcUri));

        }

        private async Task DeliverPostToUser(BotUser botUser, Post post, VkApi vkApi)
        {
            // CopyHistory еще надо обрабатывать... т.е. репосты репостов тип
            await _bot.Client.SendTextMessageAsync(
                botUser.TelegramUserId,
                $"`Вложение: Запись со стены. {GetNameById(vkApi, post.FromId.Value)}`\r\n {post.Text}", 
                ParseMode.Markdown,
                replyMarkup:new InlineKeyboardMarkup(new []
                {
                    InlineKeyboardButton.WithCallbackData($"{post.Likes?.Count} ❤", "wpst-like"), 
                    InlineKeyboardButton.WithCallbackData($"{post.Comments?.Count} 💬", "wpst-comment"),
                    InlineKeyboardButton.WithCallbackData($"{post.Reposts?.Count} 🔊", "wpst-share"),
                })
                );
            
            await DeliverAttachmentsToUser(botUser, vkApi, post.Attachments);

            foreach (var copyPost in post.CopyHistory)
            {
                await DeliverPostToUser(botUser, copyPost, vkApi);
            }
        }

        private async Task DeliverAlbumToUser(long userTelegramId, Album album, VkApi vkApi)
        {
            var thumb = album.Thumb;
            var thumbUris = new[]
            {
                thumb.BigPhotoSrc,
                thumb.PhotoSrc,
                thumb.Photo2560,
                thumb.Photo1280,
                thumb.Photo807,
                thumb.Photo604,
                thumb.Photo130,
                thumb.Photo75
            };
            var bestSrcUri = thumbUris.FirstOrDefault(uri => uri != null);

            await _bot.Client.SendPhotoAsync(
                userTelegramId,
                new InputOnlineFile(bestSrcUri.ToString()),
                $"Вложение: Альбом с фотографиями пользователя {GetNameById(vkApi, album.OwnerId.Value)}\r\n{album.Title} ({album.Size} фото)\r\n{album.Description}"
            );
        }

        private async Task DeliverPollToUser(long userTelegramId, Poll poll, VkApi vkApi)
        {
            var pollAnswerStats = new StringBuilder();
            foreach (var pollAnswer in poll.Answers)
            {
                pollAnswerStats.Append(
                    $"{pollAnswer.Text}\r\nПроголосовал {pollAnswer.Votes} человек. ({pollAnswer.Rate}%)\r\n");
            }

            var pollType = poll.Anonymous.HasValue && poll.Anonymous.Value
                ? "Анонимное голосование"
                : "Открытое голосование";

            var ownerName = poll.OwnerId.HasValue
                ? GetNameById(vkApi, poll.OwnerId.Value)
                : "Undefined";

            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: {pollType} от {ownerName}`\r\n{poll.Question}\r\n{pollAnswerStats}\r\nВсего проголосовало {poll.Votes} человек", ParseMode.Markdown);
        }

        private async Task DeliverNoteToUser(long userTelegramId, Note note, VkApi vkApi)
        {
            await _bot.Client.SendTextMessageAsync(userTelegramId,
                $"`Вложение: Заметка пользователя {GetNameById(vkApi, note.OwnerId.Value)}`\r\n{note.Title}\r\n{note.Text}", ParseMode.Markdown);
        }

        private async Task DeliverLinkToUser(long userTelegramId, Link link)
        {
            await _bot.Client.SendTextMessageAsync(userTelegramId, $"`Вложение: Ссылка на Web-страницу`\r\n{link.Uri}",
                ParseMode.Markdown);
        }

        private async Task DeliverGraffitiToUser(long userTelegramId, Graffiti graffiti)
        {
            await _bot.Client.SendPhotoAsync(userTelegramId, new InputOnlineFile(graffiti.Photo586));
        }

        private async Task DeliverDocumentToUser(long userTelegramId, Document document)
        {
            try
            {
                await _bot.Client.SendDocumentAsync(userTelegramId, new InputOnlineFile(document.Uri));
            }
            catch (Exception e)
            {
                await _bot.Client.SendTextMessageAsync(userTelegramId, $"Вложение: Документ {document.Title} {document.Uri}");
            }
        }

        private async Task DeliverAudioToUser(long userTelegramId, Audio audio)
        {
            await _bot.Client.SendAudioAsync(userTelegramId, new InputOnlineFile(audio.Uri.ToString()));
        }

        private async Task DeliverVideoToUser(long userTelegramId, Video video)
        {
            try
            {
                await _bot.Client.SendVideoAsync(userTelegramId, new InputOnlineFile(video.Player.ToString()),
                    caption: $"{video.Title}\r\n{video.Description}");
            }
            catch (Exception e)
            {
                await _bot.Client.SendTextMessageAsync(
                    userTelegramId,
                    $"`Ошибка доставки вложения: Видеозапись`\r\nException msg::{e.Message}",
                    ParseMode.Markdown
                );
            }
        }

        private async Task DeliverPhotoToUser(long userTelegramId, Photo photo)
        {
            // Нельзя полагаться на то, что всегда будет доступна PhotoSrc или какой-то конкретный размер.
            // Поэтому я выбираю из всех ДОСТУПНЫХ размеров максимальный, а если таких вообще нет - сообщаем 
            // о странной фотографии (хотя такого точно не может быть =) )
            var photoSrcUris = new[]
            {
                photo.BigPhotoSrc,
                photo.PhotoSrc,
                photo.Photo2560,
                photo.Photo1280,
                photo.Photo807,
                photo.Photo604,
                photo.Photo130,
                photo.Photo75
            };
            var bestSrcUri = photoSrcUris.FirstOrDefault(uri => uri != null);
            await _bot.Client.SendPhotoAsync(userTelegramId, new InputOnlineFile(bestSrcUri.ToString()));
        }

        private static IReplyMarkup GetReplyMarkup(Message message)
        {
            var msgId = message.Id;
            IReplyMarkup replyMarkup = message.Out.HasValue && message.Out.Value
                ? new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("ответить", $"{Constants.MessageReply}{msgId}"),
                    InlineKeyboardButton.WithCallbackData("переслать", $"{Constants.MessageForward}{msgId}"),
                    InlineKeyboardButton.WithCallbackData("редактировать", $"{Constants.MessageEdit}{msgId}"),
                    InlineKeyboardButton.WithCallbackData("удалить", $"{Constants.MessageDelite}{msgId}")
                })
                : new InlineKeyboardMarkup(new[]
                {
                    InlineKeyboardButton.WithCallbackData("ответить", $"{Constants.MessageReply}{msgId}"),
                    InlineKeyboardButton.WithCallbackData("переслать", $"{Constants.MessageForward}{msgId}")
                });
            return replyMarkup;
        }
    }
}