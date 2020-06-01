using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Log entries handler 
    /// </summary>
    public class DatabaseLoggerLogHandler
    {
        private IDatabaseLoggerDbContext _context;
        private DatabaseLoggerCollector _logCollector;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logCollector">Log collector object</param>
        public DatabaseLoggerLogHandler(IDatabaseLoggerDbContext context, DatabaseLoggerCollector logCollector)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logCollector = logCollector ?? throw new ArgumentNullException(nameof(logCollector));
        }

        internal async Task<int> ProcessAsync()
        {
            var logEntries = new List<DatabaseLoggerLogEntry>();
            while (_logCollector.TryGetEntry(out var entry))
                logEntries.Add(entry!);
            await _context.GetLoggerDbSet().AddRangeAsync(logEntries);
            var savedCount = await _context.SaveChangesAsync();
            foreach (var logEntry in logEntries.Where(x => x.Id == 0))
                _logCollector.AddEntry(logEntry);
            return savedCount;
        }
    }
}