using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Log collector object
    /// </summary>
    public class DatabaseLoggerCollector : IEnumerable<string>
    {
        private readonly ConcurrentQueue<DatabaseLoggerLogEntry> _logQueue
            = new ConcurrentQueue<DatabaseLoggerLogEntry>();

        internal void AddEntry(DatabaseLoggerLogEntry entry) => _logQueue.Enqueue(entry);
        internal bool TryGetEntry(out DatabaseLoggerLogEntry? entry) => _logQueue.TryDequeue(out entry);
        internal bool IsEmpty => _logQueue.IsEmpty;

        #region IEnumerable interface implementation

        /// <summary>
        /// Enumerates log entries in collector
        /// </summary>
        /// <returns>Enumerator of strings</returns>
        public virtual IEnumerator<string> GetEnumerator() =>
            _logQueue
                .Select(x => x.ToString())
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}