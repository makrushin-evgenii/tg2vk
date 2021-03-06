﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.CalendarControl.Common;
using TelegramAggregator.Controls.CalendarControl.Services;

namespace TelegramAggregator.Controls.CalendarControl.Handlers.Calendar
{
    public class PickDateHandler : IUpdateHandler
    {
        private readonly LocalizationService _locale;

        public PickDateHandler(LocalizationService locale)
        {
            _locale = locale;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return
                update.Type == UpdateType.CallbackQuery &&
                update.IsCallbackCommand(Constants.PickDate);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            if (!DateTime.TryParseExact(
                update.TrimCallbackCommand(Constants.PickDate),
                Constants.DateFormat,
                null,
                DateTimeStyles.None,
                out var date))
            {
                return UpdateHandlingResult.Handled;
            }

            await bot.Client.SendTextMessageAsync(
                update.CallbackQuery.Message.Chat.Id,
                date.ToString("d", _locale.DateCulture));

            return UpdateHandlingResult.Handled;
        }
    }
}