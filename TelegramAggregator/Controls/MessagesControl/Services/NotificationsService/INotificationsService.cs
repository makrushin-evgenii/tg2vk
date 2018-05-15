using System.Threading.Tasks;
using TelegramAggregator.Model.Entities;

namespace TelegramAggregator.Controls.MessagesControl.Services.NotificationsService
{
    public interface INotificationsService
    {
        Task EnableNotifications(BotUser botUser);

        void DisableNotifications(BotUser botUser);
    }
}