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
    public class AuthCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class AuthCommand : CommandBase<AuthCommandArgs>
    {
        private readonly IBotUserRepository _botUserRepository;
        private readonly INotificationsService _notificationsService;

        public AuthCommand(IBotUserRepository botUserRepository, INotificationsService notificationsService)
            : base(Constants.Command)
        {
            _botUserRepository = botUserRepository;
            _notificationsService = notificationsService;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update, AuthCommandArgs args)
        {
            if (args.ArgsInput.Split(" ", StringSplitOptions.RemoveEmptyEntries).Length != 1)
            {
                await bot.Client.SendTextMessageAsync(update.Message.Chat.Id,
                    $"`Формат команды: {Constants.CommandFormat}`", ParseMode.Markdown);
                return UpdateHandlingResult.Handled;
            }

            var acessToken = args.ArgsInput;

            var botUser = _botUserRepository.GetByTelegramId(update.Message.Chat.Id);
            if (botUser == null)
            {
                botUser = new BotUser
                {
                    TelegramChatId = update.Message.Chat.Id,
                    TelegramUserId = update.Message.Chat.Id
                };
            }

            IReplyMarkup replyMarkup = new ReplyKeyboardRemove();

            try
            {
                var userInfo = botUser.AuthorizeVk(acessToken);
                await _notificationsService.EnableNotifications(botUser);
                _botUserRepository.Add(botUser);
                await bot.Client.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"`Вы авторизованы как: {userInfo.FirstName} {userInfo.LastName}`",
                    ParseMode.Markdown,
                    replyMarkup: replyMarkup);
            }
            catch (Exception e)
            {
                await bot.Client.SendTextMessageAsync(update.Message.Chat.Id, e.Message);
            }

            return UpdateHandlingResult.Handled;
        }
    }
}