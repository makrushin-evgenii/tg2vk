using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Framework;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramAggregator
{
    public class AggregatorBot : BotBase<AggregatorBot>
    {
        private readonly ILogger<AggregatorBot> _logger;

        public AggregatorBot(IOptions<BotOptions<AggregatorBot>> botOptions, ILogger<AggregatorBot> logger)
            : base(botOptions.Value)
        {
            _logger = logger;
        }

        public override Task HandleFaultedUpdate(Update update, Exception e)
        {
            _logger.LogError("Exception occured in handling update of type `{0}`: {1}", update.Type, e.Message);

            return Task.CompletedTask;
        }

        public override async Task HandleUnknownUpdate(Update update)
        {
            _logger.LogWarning("Unable to handle update of type `{0}`", update.Type);

            string text;
            var replyToMesageId = default(int);

            switch (update.Type)
            {
                case UpdateType.Message when
                    new[] {ChatType.Private, ChatType.Group, ChatType.Supergroup}.Contains(update.Message.Chat.Type):
                    text = $"Unable to handle message update of type `{update.Message.Type}`.";
                    replyToMesageId = update.Message.MessageId;
                    break;
                default:
                    text = null;
                    break;
            }

            if (text != null)
            {
                await Client.SendTextMessageAsync(update.Message.Chat.Id, text, ParseMode.Markdown,
                    replyToMessageId: replyToMesageId);
            }
        }
    }
}