using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramAggregator.Controls.MessagesControl.Common;
using TelegramAggregator.Model.Repositories;
using VkNet;
using VkNet.Model.RequestParams;

namespace TelegramAggregator.Controls.MessagesControl.Handlers.Messages
{
    public class ForwardMessagesHandler : IUpdateHandler
    {
        private readonly IBotUserRepository _botUserRepository;

        public ForwardMessagesHandler(IBotUserRepository botUserRepository)
        {
            _botUserRepository = botUserRepository;
        }
        
        public bool CanHandleUpdate(IBot bot, Update update)
        {            
            return
                update.Type == UpdateType.CallbackQuery &&
                update.CallbackQuery.Data.StartsWith(Constants.MessageForward, StringComparison.Ordinal);
        }

        public async Task<UpdateHandlingResult> HandleUpdateAsync(IBot bot, Update update)
        {
            var botUser = _botUserRepository.GetByTelegramId(update.CallbackQuery.Message.Chat.Id);
            if (botUser.VkAccount == null)
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Необходима авторизация");
            }
            
            var api = new VkApi();
            await api.AuthorizeAsync(new ApiAuthParams()
            {
                AccessToken = botUser.VkAccount.AcessToken
            });

            if (!long.TryParse(update.CallbackQuery.Data.Substring(Constants.MessageForward.Length),
                out var forwardedMsgId))
            {
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Сообщение не найдено");
            }
            
            await api.Messages.SendAsync(new MessagesSendParams()
            {
                PeerId = botUser.VkAccount.CurrentPeer,
                Message = "fwd",
                ForwardMessages = new long[] { forwardedMsgId }
            });
            
            await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Сообщение переслано в текущий диалог");
            return UpdateHandlingResult.Handled;
        }
    }
}