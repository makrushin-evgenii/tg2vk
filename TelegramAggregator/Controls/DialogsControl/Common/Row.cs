using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAggregator.Model.Extensions;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace TelegramAggregator.Controls.DialogsControl.Common
{
    public static class Row
    {
        public static IEnumerable<IEnumerable<InlineKeyboardButton>> Dialogs(VkApi api)
        {
            const int vkChatsStartId = 2000000000;
            const int dialogsPerPage = 8;

            var dialogs = api.Messages.GetDialogs(new MessagesDialogsGetParams
            {
                Count = dialogsPerPage
            });

            foreach (var message in dialogs.Messages)
            {
                yield return Dialog(message);
            }

            IEnumerable<InlineKeyboardButton> Dialog(Message message)
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
                    var peer = api.GetUserById(message.UserId.Value);

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