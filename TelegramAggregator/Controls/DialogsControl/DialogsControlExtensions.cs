using System;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Framework;
using TelegramAggregator.Controls.DialogsControl.Handlers.Commands;
using TelegramAggregator.Controls.DialogsControl.Handlers.Dialogs;

namespace TelegramAggregator.Controls.DialogsControl
{
    public static class DialogsControlExtensions
    {
        public static TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot>
            AddDialogsHandlers<TBot>(
                this TelegramBotFrameworkIServiceCollectionExtensions.ITelegramBotFrameworkBuilder<TBot> botBuilder)
            where TBot : BotBase<TBot>
        {
            return botBuilder
                .AddUpdateHandler<DialogsCommand>()
                .AddUpdateHandler<PickDialogHandler>();
        }

        public static IServiceCollection AddDialogsControlServices(this IServiceCollection services)
        {
            throw new NotImplementedException();
        }
    }
}