using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.MessagesControl.Common;
using TelegramAggregator.Model.Repositories;

namespace TelegramAggregator.Controls.MessagesControl.Handlers.Messages
{
    public class EditMessagesHandler : IUpdateHandler
    {
        private readonly IBotUserRepository _botUserRepository;

        public EditMessagesHandler(IBotUserRepository botUserRepository)
        {
            _botUserRepository = botUserRepository;
        }
        
        public bool CanHandleUpdate(IBot bot, Update update)
        {            
            return
                update.Type == UpdateType.CallbackQuery &&
                update.CallbackQuery.Data.StartsWith(Constants.MessageEdit , StringComparison.Ordinal);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.CallbackQuery.Message.Chat.Id);
            if (botUser.VkAccount == null)
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Необходима авторизация");
            }
            
            await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Пока не доступно");
            return UpdateHandlingResult.Handled;
        }
    }
}