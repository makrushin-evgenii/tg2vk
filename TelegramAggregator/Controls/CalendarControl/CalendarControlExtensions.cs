using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework;
using TelegramAggregator.Controls.CalendarControl.Handlers.Calendar;
using TelegramAggregator.Controls.CalendarControl.Handlers.Commands;
using TelegramAggregator.Controls.CalendarControl.Handlers.MonthYear;
using TelegramAggregator.Controls.CalendarControl.Services;

namespace TelegramAggregator.Controls.CalendarControl
{
    public static class CalendarControlExtensions
    {
        public static TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot>
            AddCalendarHandlers<TBot>(
                this TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot> botBuilder)
            where TBot : BotBase<TBot>
        {
            return botBuilder
                .AddUpdateHandler<CalendarCommand>()
                .AddUpdateHandler<ChangeToHandler>()
                .AddUpdateHandler<PickDateHandler>()
                .AddUpdateHandler<YearMonthPickerHandler>()
                .AddUpdateHandler<MonthPickerHandler>()
                .AddUpdateHandler<YearPickerHandler>();
        }


        public static IServiceCollection AddCalendarControlServices(this IServiceCollection services)
        {
            return services
                .AddTransient<LocalizationService>();
        }
    }
}