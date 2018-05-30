using System;
using System.Collections;
using System.Collections.Generic;
using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    /// <summary>
    ///     Ответ на запрос к longpoll серверу
    ///     См. описание https://vk.com/dev/using_longpoll
    /// </summary>
    public class Updates : IEnumerable<Update>
    {
        private VkResponseArray _updatesRaw;

        /// <summary>
        ///     Отметка времени
        /// </summary>
        public ulong Ts { get; set; }

        public static Updates FromJson(VkResponse response)
        {                     
            var updates = new Updates();

            if (response == null)
            {
                return updates;
            }
            
            if (!response.ContainsKey("ts") || !response.ContainsKey("updates"))
            {
                return updates;
            }

            updates.Ts = (ulong) response["ts"];
            updates._updatesRaw = response["updates"];

            return updates;
        }
        
        public IEnumerator<Update> GetEnumerator()
        {
            foreach (VkResponseArray update in _updatesRaw)
            {
                Update cur;
                try
                {
                    cur = Update.FromJson(update);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }

                yield return cur;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}