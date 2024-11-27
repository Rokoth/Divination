using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Divination.Common
{
    public class ErrorNotifyLoggerConfiguration
    {
        public int EventId { get; set; }

        public ErrorNotifyOptions Options { get; set; }

        public List<LogLevel> LogLevels { get; set; } = new List<LogLevel>()
        {
            LogLevel.Error,
            LogLevel.Critical
        };
    }
}
