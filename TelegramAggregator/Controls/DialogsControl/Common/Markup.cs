using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using VkNet;

namespace TelegramAggregator.Controls.DialogsControl.Common
{
    public static class Markup
    {
        public static InlineKeyboardMarkup Dialogs(VkApi api)
        {
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();
            keyboardRows.AddRange(Row.Dialogs(api));
            keyboardRows.Add(Row.Controls());

            return new InlineKeyboardMarkup(keyboardRows);
        }
    }
}