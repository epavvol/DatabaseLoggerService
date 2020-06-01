using System;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database logger configuration
    /// </summary>
    public class DatabaseLoggerOptions
    {
        private byte _syncInterval = 5;
        private Type? _dbContextType;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public DatabaseLoggerOptions()
        {
        }

        /// <summary>
        /// Constructor with Database context type specification
        /// </summary>
        /// <param name="dbContextType">Database context object type</param>
        public DatabaseLoggerOptions(Type dbContextType)
        {
            DbContextType = dbContextType;
        }

        /// <summary>
        /// Database context object type
        /// </summary>
        public Type? DbContextType
        {
            get => _dbContextType;
            set => _dbContextType = typeof(IDatabaseLoggerDbContext).IsAssignableFrom(value)
                ? value
                : throw new ArgumentException("Context must implement IDatabaseLoggerDbContext interface.",
                    nameof(DbContextType));
        }

        /// <summary>
        /// Include scopes in logging
        /// </summary>
        public bool IncludeScopes { get; set; }

        /// <summary>
        /// Log synchronization interval
        /// </summary>
        public byte SyncInterval
        {
            get => _syncInterval;
            set => _syncInterval = value > 1
                ? value
                : throw new ArgumentException("Value must be greater than 1.", nameof(SyncInterval));
        }
    }
}