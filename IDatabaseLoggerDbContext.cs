using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database context interface required for logger usage
    /// </summary>
    public interface IDatabaseLoggerDbContext: IDisposable
    {
        /// <summary>
        /// Log table DbSet getter
        /// </summary>
        /// <returns>DbSet for log entries table</returns>
        DbSet<DatabaseLoggerLogEntry> GetLoggerDbSet();
        
        /// <summary>
        /// Saves context changes
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of added entries</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        internal void AddEntry(DatabaseLoggerLogEntry entry) => GetLoggerDbSet().Add(entry);

        internal async Task AddRangeAsync(IEnumerable<DatabaseLoggerLogEntry> entries,
            CancellationToken cancellationToken = default) =>
            await GetLoggerDbSet().AddRangeAsync(entries, cancellationToken);
    }
}