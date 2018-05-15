using System.Collections.Generic;
using TelegramAggregator.Model.Entities;

namespace TelegramAggregator.Model.Repositories
{
    public interface IBotUserRepository
    {
        IEnumerable<BotUser> Users { get; }
        BotUser Get(int id);
        BotUser GetByTelegramId(long telegramId);
        void Add(BotUser botUser);
    }
}