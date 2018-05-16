using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Controls.MessagesControl.Common;
using TelegramAggregator.Model.Repositories;
using VkNet;

namespace TelegramAggregator.Controls.MessagesControl.Handlers.Messages
{
    public class DeleteMessagesHandler : IUpdateHandler
    {
        private readonly IBotUserRepository _botUserRepository;

        public DeleteMessagesHandler(IBotUserRepository botUserRepository)
        {
            _botUserRepository = botUserRepository;
        }
        
        public bool CanHandleUpdate(IBot bot, Update update)
        {            
            return
                update.Type == UpdateType.CallbackQuery &&
                update.CallbackQuery.Data.StartsWith(Constants.MessageDelite , StringComparison.Ordinal);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.CallbackQuery.Message.Chat.Id);
            if (botUser.VkAccount == null)
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Необходима авторизация");
            }

            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            if (!ulong.TryParse(update.CallbackQuery.Data.Substring(Constants.MessageForward.Length),
                out var msgToDeleteId))
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Сообщение не найдено");
            }

            try
            {
                vkApi.Messages.Delete(new ulong[] {msgToDeleteId}, false, true);
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Сообщение удалено");
            }
            catch (Exception e)
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Не получилось удалить сообщение");
                Console.WriteLine(e);
            }
            
            return UpdateHandlingResult.Handled;
        }
    }
}