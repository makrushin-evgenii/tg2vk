using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.DialogsControl.Common;
using TelegramAggregator.Model.Repositories;
using VkNet;

namespace TelegramAggregator.Controls.DialogsControl.Handlers.Commands
{
    public class DialogsCommandArgs : ICommandArgs
    {
        public string RawInput { get; set; }

        public string ArgsInput { get; set; }
    }

    public class DialogsCommand : CommandBase<DialogsCommandArgs>
    {
        private readonly IBotUserRepository _botUserRepository;
        private const int DefaultStart = 0;
        private const int DefaultCount = 5;
        
        public DialogsCommand(IBotUserRepository botUserRepository)
            : base(Constants.Command)
        {
            _botUserRepository = botUserRepository;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update,
            DialogsCommandArgs args)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.Message.Chat.Id);

            if (botUser.VkAccount == null)
            {
                await bot.Client.SendTextMessageAsync(update.Message.Chat.Id,"`Необходима авторизация`", ParseMode.Markdown);
                return UpdateHandlingResult.Handled;
            }
            
            var api = new VkApi();
            await api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            var start = HandleArgs(args);
            var dialogsMarkup = Markup.Dialogs(api, start, DefaultCount);

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Выберете диалог:",
                replyMarkup: dialogsMarkup);

            return UpdateHandlingResult.Handled;
        }

        private static int HandleArgs(DialogsCommandArgs args)
        {
            var splitArgs = args.ArgsInput.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (splitArgs.Length != 1)
            {
                return DefaultStart;
            }

            return int.TryParse(splitArgs[0], out var start) 
                ? start 
                : DefaultStart;
        }
    }
}