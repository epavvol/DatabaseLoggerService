using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database logger service runner
    /// </summary>
    public class DatabaseLoggerService : IHostedService, IDisposable
    {
        private readonly ILogger<DatabaseLoggerService> _logger;
        private DatabaseLoggerOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDisposable? _optionsReloadToken;

        private int _executionCount;
        private int _processedCount;
        private Timer? _timer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">ILogger for logging service events</param>
        /// <param name="optionsMonitor">Database logger configuration monitor</param>
        /// <param name="serviceProvider">Dependency injection service provider</param>
        public DatabaseLoggerService(ILogger<DatabaseLoggerService> logger,
            IOptionsMonitor<DatabaseLoggerOptions> optionsMonitor,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = optionsMonitor.CurrentValue;
            _serviceProvider = serviceProvider;
            _optionsReloadToken = optionsMonitor.OnChange(ReloadOptions);
        }

        /// <summary>
        /// Reconfigure database logger service
        /// </summary>
        /// <param name="options">Database logger configuration</param>
        private void ReloadOptions(DatabaseLoggerOptions options)
        {
            _logger.LogWarning($"{nameof(DatabaseLoggerService)} is reloading its configuration, interval changing to {options.SyncInterval} seconds. [{options}]");
            _options = options;
            _timer?.Change(TimeSpan.Zero,
                TimeSpan.FromSeconds(_options.SyncInterval));
        }

        /// <summary>
        /// Starts the service runner
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogWarning($"{nameof(DatabaseLoggerService)} starting up with interval of {_options.SyncInterval} seconds.");
            _timer = new Timer(
                async _ => await ExecuteAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(_options.SyncInterval));
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync()
        {
            _executionCount++;
            using var serviceScope = _serviceProvider.CreateScope();
            var logCollector = serviceScope.ServiceProvider.GetService<DatabaseLoggerCollector>();
            if (logCollector.IsEmpty)
                return;
            using var dbContext = (IDatabaseLoggerDbContext) serviceScope
                .ServiceProvider
                .GetService(_options.DbContextType);
            _processedCount += await new DatabaseLoggerLogHandler(dbContext, logCollector)
                .ProcessAsync();
        }

        /// <summary>
        /// Stops the service runner
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(DatabaseLoggerService)} is stopping! " +
                                   $"In total {_processedCount} logs were processed in {_executionCount} executions.");
            _timer?.Change(Timeout.Infinite, 0);
            await ExecuteAsync();
        }

        /// <summary>
        /// Dispose service object
        /// </summary>
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
            _timer?.Dispose();
        }
    }
}