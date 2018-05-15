using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using TelegramAggregator.Controls.CalendarControl.Common;
using TelegramAggregator.Controls.CalendarControl.Services;

namespace TelegramAggregator.Controls.CalendarControl.Handlers.Commands
{
    public class CalendarCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class CalendarCommand : CommandBase<CalendarCommandArgs>
    {
        private readonly LocalizationService _locale;

        public CalendarCommand(LocalizationService locale)
            : base(Constants.Command)
        {
            _locale = locale;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update,
            CalendarCommandArgs args)
        {
            var calendarMarkup = Markup.Calendar(DateTime.Today, _locale.DateCulture);

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Pick dialog:",
                replyMarkup: calendarMarkup);

            return UpdateHandlingResult.Handled;
        }
    }
}