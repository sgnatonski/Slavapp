using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlavApp.Microservice.Requests;
using System.Globalization;

namespace SlavApp.Microservice.Serialization
{
    internal class ContextMessageSurrogate
    {
        public string RawData { get; set; }
        public string User { get; set; }
        public long CreatedAt { get; set; }

        public static implicit operator ContextMessageSurrogate(ContextMessage msg)
        {
            return
                msg != null
                ? new ContextMessageSurrogate
                {
                    RawData = msg.RawData,
                    User = msg.User, 
                    CreatedAt = msg.CreatedAt.UtcTicks
                }
                : null;
        }

        public static implicit operator ContextMessage(ContextMessageSurrogate msg)
        {
            return new ContextMessage
            {
                RawData = msg.RawData, 
                User = msg.User, 
                CreatedAt = new DateTimeOffset(msg.CreatedAt, TimeZoneInfo.Local.BaseUtcOffset)
            };
        }
    }
}
