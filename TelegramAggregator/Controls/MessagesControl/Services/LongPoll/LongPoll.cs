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

        public async Task<VkResponse> GetUpdates()
        {
            try
            {
                var client = new HttpClient();
                var updateResponse = await client
                    .GetAsync(
                        $"https://{Server}?act=a_check&key={Key}&ts={Ts}&wait={Wait}&mode={Mode}&version={Version}");
                var jsoned = await updateResponse.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<JObject>(jsoned);

                if (response.ContainsKey("failed"))
                {
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

            switch (failureCode)
            {
                case 1:
                    // failed:1 — история событий устарела или была частично утеряна, приложение может получать события далее, используя новое значение ts из ответа.
                    var newTs = response["ts"].ToObject<ulong>();
                    Ts = newTs;
                    break;
                case 2:
                case 3:
                {
                    // "failed:2 — истекло время действия ключа, нужно заново получить key методом messages.getLongPollServer"
                    // "failed:3 — информация о пользователе утрачена, нужно запросить новые key и ts методом messages.getLongPollServer."
                    var longPollServer = _vkApi.Messages.GetLongPollServer();
                    ResetLongPollServer(longPollServer);
                    break;
                }
                case 4:
                    // "failed: 4 — передан недопустимый номер версии в параметре version."
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