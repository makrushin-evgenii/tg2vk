using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAggregator.Controls.AuthControl.Common;
using TelegramAggregator.Controls.MessagesControl.Services.NotificationsService;
using TelegramAggregator.Model.Entities;
using TelegramAggregator.Model.Extensions;
using TelegramAggregator.Model.Repositories;

namespace TelegramAggregator.DialogsControl.Handlers.Commands
{
    public class LogoutCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class LogoutCommand : CommandBase<AuthCommandArgs>
    {
        private readonly IBotUserRepository _botUserRepository;
        private readonly INotificationsService _notificationsService;

        public LogoutCommand(IBotUserRepository botUserRepository, INotificationsService notificationsService)
            : base("logout")
        {
            _botUserRepository = botUserRepository;
            _notificationsService = notificationsService;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update, AuthCommandArgs args)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.Message.Chat.Id);
            if (botUser == null)
            {
                return UpdateHandlingResult.Handled;
            }

            _notificationsService.DisableNotifications(botUser);
            botUser.VkAccount = null;

            return UpdateHandlingResult.Handled;
        }
    }
}