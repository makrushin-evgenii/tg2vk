using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAggregator.Model.Extensions;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace TelegramAggregator.Controls.DialogsControl.Common
{
    public static class Row
    {
        public static IEnumerable<IEnumerable<InlineKeyboardButton>> Dialogs(VkApi api, int start, int count)
        {
            const int vkChatsStartId = 2000000000;

            var dialogs = api.Messages.GetDialogs(new MessagesDialogsGetParams
            {
                Offset = start,
                Count = (uint) count
            });

            var usrScreenames = dialogs.Messages
                .Where(message => message.UserId.HasValue)
                .Select(message => $"id{message.UserId.Value}");
            
            var users = api.Users.Get(usrScreenames, ProfileFields.All, null, true);

            foreach (var message in dialogs.Messages)
            {
                yield return Dialog(message, users);
            }


            IEnumerable<InlineKeyboardButton> Dialog(Message message, IEnumerable<User> usersData)
            {
                if (message.ChatId.HasValue)
                {
                    yield return InlineKeyboardButton.WithCallbackData(
                        $"{message.Title}: {message.Body}",
                        $"{Constants.PickDialog}{message.ChatId + vkChatsStartId}");
                }
                else if (message.UserId.HasValue)
                {
                    // TODO: переделать этот метод.
                    // Получать информацию сразу о всех пользователях,
                    // чтобы сократить обращения к vk-api
                    var peer = usersData.FirstOrDefault(usr => usr.Id == message.UserId);

                    yield return InlineKeyboardButton.WithCallbackData(
                        $"{peer.FirstName} {peer.LastName}: {message.Body}",
                        $"{Constants.PickDialog}{message.UserId}");
                }
            }
        }
       
        public static IEnumerable<InlineKeyboardButton> Controls(int page = 0)
        {
            return new[]
            {
                page > 0
                    ? InlineKeyboardButton.WithCallbackData(
                        "< сюда",
                        $"{Constants.GotoPage}{page - 1}")
                    : " ",
                InlineKeyboardButton.WithCallbackData(
                    "туда >",
                    $"{Constants.GotoPage}{page + 1}")
            };
        }
    }
}