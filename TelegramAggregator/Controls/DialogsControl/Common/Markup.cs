using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using VkNet;

namespace TelegramAggregator.Controls.DialogsControl.Common
{
    public static class Markup
    {
        public static InlineKeyboardMarkup Dialogs(VkApi api, int start, int count)
        {
            var keyboardRows = new List<IEnumerable<InlineKeyboardButton>>();
            keyboardRows.AddRange(Row.Dialogs(api, start, count));
//            keyboardRows.Add(Row.Controls());

            return new InlineKeyboardMarkup(keyboardRows);
        }
    }
}