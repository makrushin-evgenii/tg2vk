using System;
using System.Collections.Generic;
using System.Linq;
using VkNet.Utils;

namespace TelegramAggregator.Controls.MessagesControl.Services.LongPoll
{
    /// <summary>Событие LongPoll</summary>
    [Serializable]
    public abstract class LongPollUpdate
    {
        /// <summary>Коллекция событий</summary>
        /// <remarks> код события -> тип </remarks>
        private static readonly IDictionary<int, Type> Types = new Dictionary<int, Type>();

        /// <summary>Код события</summary>
        public ulong EventCode { get; set; }

        public IEnumerable<string> Fields { get; set; }

        /// <summary>Преобразовать вложение в строку.</summary>
        public override string ToString()
        {
            return $"{EventCode}:" + string.Join("; ", Fields);;
        }

        /// <summary>Зарегистрировать тип.</summary>
        /// <param name="code">Код события.</param>
        /// <param name="type">Соответствие.</param>
        protected static void RegisterType(int code, Type type)
        {
            LongPollUpdate.Types.Add(code, type);
        }

        /// <summary>Соответствие типу.</summary>
        /// <param name="code">Код события.</param>
        /// <returns>Соответствующий тип</returns>
        private static Type MatchType(int code)
        {
            return LongPollUpdate.Types[code];
        }

        protected void InitializeStandartFields(VkResponseArray response)
        {
            EventCode = (ulong) response[0];;
            Fields = response
                .Skip(1)
                .Select(field => field.ToString());
        }
    }
}