using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.CalendarControl.Common;
using TelegramAggregator.Controls.CalendarControl.Services;

namespace TelegramAggregator.Controls.CalendarControl.Handlers.Calendar
{
    public class ChangeToHandler : IUpdateHandler
    {
        private readonly LocalizationService _locale;

        public ChangeToHandler(LocalizationService locale)
        {
            _locale = locale;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return
                update.Type == UpdateType.CallbackQuery &&
                update.IsCallbackCommand(Constants.ChangeTo);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            if (!DateTime.TryParseExact(
                update.TrimCallbackCommand(Constants.ChangeTo),
                Constants.DateFormat,
                null,
                DateTimeStyles.None,
                out var date))
            {
                return UpdateHandlingResult.Handled;
            }

            var calendarMarkup = Markup.Calendar(date, _locale.DateCulture);

            await bot.Client.EditMessageReplyMarkupAsync(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                calendarMarkup);

            return UpdateHandlingResult.Handled;
        }
    }
}