using System.Globalization;

namespace TelegramAggregator.Controls.CalendarControl.Services
{
    public class LocalizationService
    {
        private readonly AggregatorBotConfiguration _configuration;

        public DateTimeFormatInfo DateCulture;

        public LocalizationService(AggregatorBotConfiguration configuration)
        {
            _configuration = configuration;

            DateCulture = configuration.BotLocale == null
                ? new CultureInfo("en-US", false).DateTimeFormat
                : new CultureInfo(configuration.BotLocale, false).DateTimeFormat;
        }
    }
}