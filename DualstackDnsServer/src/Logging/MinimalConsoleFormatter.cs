using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace DualstackDnsServer.Logging;

/// <summary>
/// Minimal console formatter: prints level and message only, without category/event id.
/// Example: "info: [DnsQueryHandler] DNS response: 0 answers"
/// </summary>
public sealed class MinimalConsoleFormatter : ConsoleFormatter
{
    public const string FormatterName = "minimal";

    public MinimalConsoleFormatter() : base(FormatterName)
    {
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        if (textWriter == null)
            throw new ArgumentNullException(nameof(textWriter));

        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);
        if (string.IsNullOrEmpty(message) && logEntry.Exception == null)
            return;

        var builder = new StringBuilder();
        builder.Append(GetLevelString(logEntry.LogLevel));
        builder.Append(' ');
        builder.Append(message);
        if (logEntry.Exception != null)
        {
            builder.Append(" | ");
            builder.Append(logEntry.Exception.GetType().Name);
            builder.Append(':');
            builder.Append(' ');
            builder.Append(logEntry.Exception.Message);
        }
        builder.Append(Environment.NewLine);
        textWriter.Write(builder.ToString());
    }

    private static string GetLevelString(LogLevel level) => level switch
    {
        LogLevel.Trace => "trce:",
        LogLevel.Debug => "dbug:",
        LogLevel.Information => "info:",
        LogLevel.Warning => "warn:",
        LogLevel.Error => "fail:",
        LogLevel.Critical => "crit:",
        _ => "info:"
    };
}
