using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VkNet;
using VkNet.Model;
using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    public class LongPoll
    {
        public const int Wait = 20;
        public const int Mode = 2;
        public const int Version = 2;

        private readonly VkApi _vkApi;

        public LongPoll(VkApi vkApi, LongPollServerResponse longPollServerResponse)
        {
            _vkApi = vkApi;
            ResetLongPollServer(longPollServerResponse);
        }

        public string Server { get; private set; }
        public string Key { get; private set; }
        public ulong? Pts { get; private set; }
        public ulong Ts { get; private set; }

        public async Task<VkResponse> GetUpdatesResponce()
        {
            try
            {
                var client = new HttpClient();
                Console.WriteLine($"Запрос LongPoll, c парметрами" +
                                  $"Uri https://{Server}?act=a_check&key={Key}&ts={Ts}&wait={Wait}&mode={Mode}&version={Version}");
                var updateResponse = await client
                    .GetAsync(
                        $"https://{Server}?act=a_check&key={Key}&ts={Ts}&wait={Wait}&mode={Mode}&version={Version}");
                var jsoned = await updateResponse.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<JObject>(jsoned);
                Console.WriteLine($"Получен ответ от LongPoll\r\n{response}");

                if (response.ContainsKey("failed"))
                {
                    Console.WriteLine($"Обработка ошибки LongPoll...");
                    HandleFailure(response);
                    return null;
                }

                var newTs = response["ts"].ToObject<ulong>();
                Ts = newTs;

                return new VkResponse(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private void HandleFailure(JObject response)
        {
            if (!response.ContainsKey("failed"))
            {
                return;
            }

            var failureCode = response["failed"].ToObject<int>();

            if (failureCode == 1)
            {
                Console.WriteLine("failed:1 — история событий устарела или была частично утеряна, " +
                                  "приложение может получать события далее, " +
                                  "используя новое значение ts из ответа.");
                var newTs = response["ts"].ToObject<ulong>();
                Ts = newTs;
            }
            else if (failureCode == 2)
            {
                Console.WriteLine("failed:2 — истекло время действия ключа, " +
                                  "нужно заново получить key методом messages.getLongPollServer");
                var longPollServer = _vkApi.Messages.GetLongPollServer();
                ResetLongPollServer(longPollServer);
            }
            else if (failureCode == 3)
            {
                Console.WriteLine("failed:3 — информация о пользователе утрачена, " +
                                  "нужно запросить новые key и ts методом messages.getLongPollServer.");
                var longPollServer = _vkApi.Messages.GetLongPollServer();
                ResetLongPollServer(longPollServer);
            }
            else if (failureCode == 4)
            {
                Console.WriteLine("failed: 4 — передан недопустимый номер версии в параметре version.");
                throw new NotImplementedException();
            }
        }

        private void ResetLongPollServer(LongPollServerResponse longPollServerResponse)
        {
            Server = longPollServerResponse.Server;
            Key = longPollServerResponse.Key;
            Pts = longPollServerResponse.Pts;
            Ts = longPollServerResponse.Ts;
        }
    }
}