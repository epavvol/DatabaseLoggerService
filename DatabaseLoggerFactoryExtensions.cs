using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database logger factory extension methods
    /// </summary>
    public static class DatabaseLoggerFactoryExtensions
    {
        /// <summary>
        /// Adds a console logger named 'Database' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <typeparam name="TDbContext">Database context object type</typeparam>
        /// <returns><c>ILoggingBuilder</c> itself</returns>
        public static ILoggingBuilder AddDatabase<TDbContext>(
            this ILoggingBuilder builder)
            where TDbContext : IDatabaseLoggerDbContext
        {
            AddServices(builder);
            builder.Services.Configure<DatabaseLoggerOptions>(o =>
                o.DbContextType = typeof(TDbContext));
            return builder;
        }

        /// <summary>
        /// Adds a console logger named 'Database' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="settingsAction">Database logger configuration</param>
        /// <typeparam name="TDbContext">Database context object type</typeparam>
        /// <returns><c>ILoggingBuilder</c> itself</returns>
        public static ILoggingBuilder AddDatabase<TDbContext>(
            this ILoggingBuilder builder,
            Action<DatabaseLoggerOptions> settingsAction
        )
            where TDbContext : IDatabaseLoggerDbContext
        {
            AddServices(builder);
            builder.Services.Configure<DatabaseLoggerOptions>(o =>
            {
                settingsAction(o);
                o.DbContextType = typeof(TDbContext);
            });

            return builder;
        }

        private static void AddServices(ILoggingBuilder builder)
        {
            builder.AddConfiguration();
            builder.Services.TryAddSingleton<DatabaseLoggerCollector>();
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DatabaseLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<DatabaseLoggerOptions, DatabaseLoggerProvider>(
                builder.Services);
            builder.Services.AddHostedService<DatabaseLoggerService>();
        }
    }
}