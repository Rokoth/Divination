using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;

namespace Divination.Common
{
    public sealed class ErrorNotifyLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private ErrorNotifyLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, ErrorNotifyLogger> _loggers = new ConcurrentDictionary<string, ErrorNotifyLogger>();


        public ErrorNotifyLoggerProvider(
            IOptionsMonitor<ErrorNotifyLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
        {
            var errorNotifyService = new ErrorNotifyService(_currentConfig);
            var logger = _loggers.GetOrAdd(categoryName, name => new ErrorNotifyLogger(name, errorNotifyService, GetCurrentConfig));
            return logger;
        }

        private ErrorNotifyLoggerConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
