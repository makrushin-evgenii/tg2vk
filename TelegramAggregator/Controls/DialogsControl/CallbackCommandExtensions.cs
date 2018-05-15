using System;
using Telegram.Bot.Types;

namespace TelegramAggregator.Controls.DialogsControl
{
    public static class CallbackCommandExtensions
    {
        public static bool IsCallbackCommand(this Update update, string command)
        {
            return update.CallbackQuery.Data.StartsWith(command, StringComparison.Ordinal);
        }

        public static string TrimCallbackCommand(this Update update, string pattern)
        {
            return update.CallbackQuery.Data.Replace(pattern, string.Empty);
        }
    }
}