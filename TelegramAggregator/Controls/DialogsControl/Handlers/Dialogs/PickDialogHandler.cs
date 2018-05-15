using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.DialogsControl.Common;
using TelegramAggregator.Model.Repositories;

namespace TelegramAggregator.Controls.DialogsControl.Handlers.Dialogs
{
    public class PickDialogHandler : IUpdateHandler
    {
        private readonly IBotUserRepository _botUserRepository;

        public PickDialogHandler(IBotUserRepository botUserRepository)
        {
            _botUserRepository = botUserRepository;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return
                update.Type == UpdateType.CallbackQuery &&
                update.IsCallbackCommand(Constants.PickDialog);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            if (!long.TryParse(update.TrimCallbackCommand(Constants.PickDialog), out var dialogId))
            {
                return UpdateHandlingResult.Handled;
            }

            var botUser = _botUserRepository.GetByTelegramId(update.CallbackQuery.Message.Chat.Id);
            botUser.VkAccount.CurrentPeer = dialogId;

            await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, $"Выбран диалог: {dialogId}");

            return UpdateHandlingResult.Handled;
        }
    }
}