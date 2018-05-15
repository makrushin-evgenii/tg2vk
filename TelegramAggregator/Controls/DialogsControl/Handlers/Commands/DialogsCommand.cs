using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
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

        public DialogsCommand(IBotUserRepository botUserRepository)
            : base(Constants.Command)
        {
            _botUserRepository = botUserRepository;
        }

        public override async Task<UpdateHandlingResult> HandleCommand(IBot bot, Update update,
            DialogsCommandArgs args)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.Message.Chat.Id);
            var api = new VkApi();
            await api.AuthorizeAsync(new ApiAuthParams
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            var dialogsMarkup = Markup.Dialogs(api);

            await bot.Client.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Pick dialog:",
                replyMarkup: dialogsMarkup);

            return UpdateHandlingResult.Handled;
        }
    }
}