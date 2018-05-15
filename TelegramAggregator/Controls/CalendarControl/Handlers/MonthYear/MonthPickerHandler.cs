using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.CalendarControl.Common;
using TelegramAggregator.Controls.CalendarControl.Services;

namespace TelegramAggregator.Controls.CalendarControl.Handlers.MonthYear
{
    public class MonthPickerHandler : IUpdateHandler
    {
        private readonly LocalizationService _locale;

        public MonthPickerHandler(LocalizationService locale)
        {
            _locale = locale;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return
                update.Type == UpdateType.CallbackQuery &&
                update.IsCallbackCommand(Constants.PickMonth);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            if (!DateTime.TryParseExact(
                update.TrimCallbackCommand(Constants.PickMonth),
                Constants.DateFormat,
                null,
                DateTimeStyles.None,
                out var date))
            {
                return UpdateHandlingResult.Handled;
            }

            var monthPickerMarkup = Markup.PickMonth(date, _locale.DateCulture);

            await bot.Client.EditMessageReplyMarkupAsync(
                update.CallbackQuery.Message.Chat.Id,
                update.CallbackQuery.Message.MessageId,
                monthPickerMarkup);

            return UpdateHandlingResult.Handled;
        }
    }
}