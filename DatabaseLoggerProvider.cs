using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database logger provider
    /// </summary>
    [ProviderAlias("Database")]
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool>? _filter;

        private readonly ConcurrentDictionary<string, DatabaseLogger> _loggers
            = new ConcurrentDictionary<string, DatabaseLogger>();

        private readonly DatabaseLoggerCollector _logCollector;
        private DatabaseLoggerOptions _options;
        private readonly IDisposable? _optionsReloadToken;

        /// <summary>
        /// Constructor for manual provider initialization
        /// </summary>
        /// <param name="logCollector">Logging collector object</param>
        /// <param name="options">Logger configuration</param>
        /// <param name="filter">Log sources filter function</param>
        public DatabaseLoggerProvider(DatabaseLoggerCollector logCollector, DatabaseLoggerOptions options,
            Func<string, LogLevel, bool>? filter = null)
        {
            _filter = filter;
            _logCollector = logCollector;
            _options = options;
        }

        /// <summary>
        /// Constructor for automatic provider initialization
        /// </summary>
        /// <param name="logCollector">Log collector object</param>
        /// <param name="optionsMonitor">Configuration monitor object</param>
        public DatabaseLoggerProvider(DatabaseLoggerCollector logCollector,
            IOptionsMonitor<DatabaseLoggerOptions> optionsMonitor)
        {
            _filter = (s, l) => true;
            _logCollector = logCollector;
            _options = optionsMonitor.CurrentValue;
            _optionsReloadToken = optionsMonitor.OnChange(ReloadOptions);
        }

        private void ReloadOptions(DatabaseLoggerOptions options)
        {
            _options = options;
            foreach (var logger in _loggers.Values) logger.LoadOptions(options);
        }

        private Func<string, LogLevel, bool> Filter => _filter ?? ((_, __) => true);

        /// <summary>
        /// Dispose object
        /// </summary>
        public virtual void Dispose()
        {
            foreach (var keyValuePair in _loggers)
                keyValuePair.Value.Dispose();
            _optionsReloadToken?.Dispose();
        }

        /// <summary>
        /// Find or create Logger instance
        /// </summary>
        /// <param name="name">Log Source name</param>
        /// <returns>ILogger instance</returns>
        public virtual ILogger CreateLogger(string name) => _loggers.GetOrAdd(name, CreateLoggerImplementation);

        private DatabaseLogger CreateLoggerImplementation(string name) =>
            new DatabaseLogger(name, _logCollector, _options, new LoggerExternalScopeProvider(), Filter);
    }
}