namespace TelegramAggregator.Model.Entities
{
    public class BotUser
    {
        public int Id { get; set; }
        public long TelegramUserId { get; set; }
        public long TelegramChatId { get; set; }
        public VkAccount VkAccount { get; set; }
    }
}