﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace VNetDev.LoggerService.Database
{
    /// <summary>
    /// Database log entry
    /// </summary>
    public class DatabaseLoggerLogEntry
    {
        private string? _exceptionMessage;
        private string? _scope;

        /// <summary>
        /// Log entry ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Log UTC time
        /// </summary>
        public DateTime LogUtcTime { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Log source name
        /// </summary>
        [MaxLength(256), Column(TypeName = "varchar(256)")]
        public string Source { get; set; } = default!;

        /// <summary>
        /// Log message
        /// </summary>
        [MaxLength(4000)]
        public string Message { get; set; } = default!;

        /// <summary>
        /// Log level
        /// </summary>
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Log Event ID
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Log scope
        /// </summary>
        [MaxLength(512)]
        public string? Scope
        {
            get => _scope;
            set => _scope = string.IsNullOrWhiteSpace(value)
                ? null
                : value;
        }

        /// <summary>
        /// Log exception
        /// </summary>
        public string? ExceptionMessage
        {
            get => _exceptionMessage;
            set => _exceptionMessage = string.IsNullOrWhiteSpace(value)
                ? null
                : value?.Substring(0, 20480);
        }

        /// <summary>
        /// Log string representation.
        /// </summary>
        /// <returns>Log string</returns>
        public override string ToString() =>
            $"[{Id}] {LogLevel}: {nameof(LogUtcTime)}: '{LogUtcTime}', {nameof(Source)}: '{Source}', " +
            (Scope == null ? "" : $"{nameof(Scope)}: '{Scope}', ") +
            $"{nameof(Message)}: '{Message}', {nameof(EventId)}: '{EventId}', " +
            $"{nameof(ExceptionMessage)}: '{ExceptionMessage ?? "<null>"}'";
    }
}