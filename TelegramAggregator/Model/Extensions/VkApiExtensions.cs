using System;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;

namespace TelegramAggregator.Model.Extensions
{
    public static class VkApiExtensions
    {
        public static User GetUserById(this VkApi vkApi, long id)
        {
            vkApi.Authorize(new ApiAuthParams
            {
                AccessToken = "b23f06ed107fdbe12a18ffcca53bd77f4b6f086135291545621af878afcd8fd5ab910b7c6e6c9a415963f"
            });

            var screenName = $"id{id}";

            var peers = vkApi.Users.Get(new[] {screenName}, ProfileFields.FirstName | ProfileFields.LastName, null,
                true);

            if (!peers.Any())
            {
                throw new ArgumentException($"Пользователей Вконтакте с именем {screenName} не найдено");
            }

            return peers.First();
        }
    }
}