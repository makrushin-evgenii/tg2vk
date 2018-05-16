using System;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework;
using TelegramAggregator.DialogsControl.Handlers.Commands;

namespace TelegramAggregator.Controls.AuthControl
{
    public static class AuthControlExtensions
    {
        public static TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot>
            AddAuthHandlers<TBot>(
                this TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot> botBuilder)
            where TBot : BotBase<TBot>
        {
            return botBuilder
                .AddUpdateHandler<AuthCommand>()
                .AddUpdateHandler<LogoutCommand>();
        }

        public static IServiceCollection AddAuthControlServices(this IServiceCollection services)
        {
            throw new NotImplementedException();
        }
    }
}