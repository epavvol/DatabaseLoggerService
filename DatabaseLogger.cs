using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// ILogger implementation for Entity Framework Core usage
    /// </summary>
    public class DatabaseLogger : ILogger, IDisposable
    {
        private readonly string _name;
        private readonly DatabaseLoggerCollector _logCollector;
        private bool _disposed;
        private Func<string, LogLevel, bool> _filter;
        private readonly IExternalScopeProvider? _scopeProvider;
        private bool _includeScopes;
        private static readonly IDisposable DisposableObject = new Disposable();

        /// <summary>
        /// Log sources filter
        /// </summary>
        public Func<string, LogLevel, bool> Filter
        {
            get => _filter;
            set => _filter = value == null ? throw new ArgumentNullException(nameof(value)) : _filter = value;
        }

        /// <summary>
        /// Constructor, initialization of logger instance.
        /// </summary>
        /// <param name="name">Source name</param>
        /// <param name="logCollector">Log collector object</param>
        /// <param name="options">Logger configuration</param>
        /// <param name="scopeProvider">Logger scope</param>
        /// <param name="filter">Log sources filter function</param>
        public DatabaseLogger(string name, DatabaseLoggerCollector logCollector, DatabaseLoggerOptions options,
            IExternalScopeProvider scopeProvider, Func<string, LogLevel, bool>? filter)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _logCollector = logCollector;
            _scopeProvider = scopeProvider;
            _filter = filter ?? ((_, __) => true);
            LoadOptions(options);
        }

        /// <summary>
        /// Loads/reloads logger configuration
        /// </summary>
        /// <param name="options">Logger configuration</param>
        public void LoadOptions(DatabaseLoggerOptions options)
        {
            _includeScopes = options.IncludeScopes;
        }

        /// <summary>
        /// Adds log entry
        /// </summary>
        /// <param name="logLevel">Log level</param>
        /// <param name="eventId">Event ID</param>
        /// <param name="state">State</param>
        /// <param name="exception">Exception object</param>
        /// <param name="formatter">Function to format log entry</param>
        /// <typeparam name="TState">State type</typeparam>
        /// <exception cref="ArgumentException">Thrown if formatter provided is null</exception>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            CheckDisposed();
            if (!IsEnabled(logLevel))
                return;
            if (formatter == null)
                throw new ArgumentException(nameof(formatter));
            var message = formatter(state, exception);
            _logCollector.AddEntry(new DatabaseLoggerLogEntry
            {
                Message = message,
                Source = _name,
                EventId = eventId.Id,
                LogLevel = logLevel,
                Scope = GetScopeInformation(),
                ExceptionMessage = exception?.ToString()
            });
        }

        /// <summary>
        /// Checks if log level is enabled for log source 
        /// </summary>
        /// <param name="logLevel">Logging level</param>
        /// <returns>bool if logging enabled, otherwise false.</returns>
        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None && Filter(_name, logLevel);

        /// <summary>
        /// Starts new logging scope
        /// </summary>
        /// <param name="state">State</param>
        /// <typeparam name="TState">State type</typeparam>
        /// <returns>Disposable object to close logging scope.</returns>
        public IDisposable BeginScope<TState>(TState state) =>
            _scopeProvider?.Push(state) ?? DisposableObject;

        private string? GetScopeInformation()
        {
            if (!_includeScopes || _scopeProvider == null)
                return null;

            var scopes = new List<string>();
            _scopeProvider
                .ForEachScope((scope, builder) =>
                        builder.Add(scope.ToString()),
                    scopes);

            return string.Join('.', scopes);
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose() => _disposed = true;

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(DatabaseLogger));
        }

        private class Disposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}