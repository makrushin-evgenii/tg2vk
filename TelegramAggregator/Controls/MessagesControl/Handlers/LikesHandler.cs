using System;
using System.Threading.Tasks;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAggregator.Model.Repositories;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;

namespace TelegramAggregator.Controls.MessagesControl.Handlers
{
    public class LikesHandler : IUpdateHandler
    {
        private readonly IBotUserRepository _botUserRepository;

        public LikesHandler(IBotUserRepository botUserRepository)
        {
            _botUserRepository = botUserRepository;
        }

        public bool CanHandleUpdate(IBot bot, Update update)
        {
            return update.Type == UpdateType.CallbackQuery &&
                update.CallbackQuery.Data.StartsWith("like" , StringComparison.Ordinal);
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

            // like / type / id / owner
            var args = update.CallbackQuery.Data.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            var type = args[1];
            var id = long.Parse(args[2]);
            var owner = long.Parse(args[3]);

            var isLiked = await vkApi.Likes.IsLikedAsync(LikeObjectType.Post, id, ownerId: owner);

            if (isLiked)
            {
                await vkApi.Likes.DeleteAsync(LikeObjectType.Post, id, owner);
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Лайк убран");
            }
            else
            {
                await vkApi.Likes.AddAsync(new LikesAddParams()
                {
                    // Тут надо выбирать тип в зависимости от аргументов. А то тупо получается
                    Type = LikeObjectType.Post,
                    ItemId = id,
                    OwnerId = owner
                });                
                await bot.Client.AnswerCallbackQueryAsync(update.CallbackQuery.Id, "Лайк добавлен");
            }
            
            return UpdateHandlingResult.Handled;
        }
    }
}