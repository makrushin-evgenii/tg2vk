namespace TelegramAggregator.Model.Entities
{
    public class VkAccount
    {
        public long Id { get; set; }
        public string AcessToken { get; set; }
        public long CurrentPeer { get; set; }
    }
}