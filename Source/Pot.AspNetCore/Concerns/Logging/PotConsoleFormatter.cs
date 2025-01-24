using AllOverIt.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace Pot.AspNetCore.Concerns.Logging;

// https://learn.microsoft.com/en-us/dotnet/core/extensions/console-log-formatter

// Based on SimpleConsoleFormatter: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Logging.Console/src/SimpleConsoleFormatter.cs

internal sealed class PotConsoleFormatter : ConsoleFormatter, IDisposable
{
    internal const string FormatterName = nameof(PotConsoleFormatter);

    private const string LogLevelPadding = ": ";
    private const string TimestampPadding = " ";
    private const string CorrelationIdPadding = " ";
    private static readonly string _messagePadding = new(' ', GetLogLevelString(LogLevel.Information).Length + LogLevelPadding.Length);
    private static readonly string _newLineWithMessagePadding = Environment.NewLine + _messagePadding;

    private IDisposable? _optionsReloadToken;
    private PotConsoleFormatterOptions _formatterOptions;

    public PotConsoleFormatter(IOptionsMonitor<PotConsoleFormatterOptions> options)
        : base(FormatterName)
    {
        // Uses a change token to synchronize updates, based on the options pattern, and the IOptionsMonitor interface
        (_optionsReloadToken, _formatterOptions) = (options.OnChange(ReloadLoggerOptions), options.CurrentValue);
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        if (logEntry.State is BufferedLogRecord bufferedRecord)
        {
            var message = bufferedRecord.FormattedMessage ?? string.Empty;

            WriteInternal(null, textWriter, message, bufferedRecord.LogLevel, null, bufferedRecord.EventId.Id,
                bufferedRecord.Exception, logEntry.Category, bufferedRecord.Timestamp);
        }
        else
        {
            var message = logEntry.Formatter(logEntry.State, logEntry.Exception);

            if (logEntry.Exception is null && message is null)
            {
                return;
            }

            string? correlationId = null;
            var stateItems = (IReadOnlyList<KeyValuePair<string, object?>>?)logEntry.State;

            if (stateItems is not null)
            {
                var enumerator = stateItems.GetEnumerator();

                while (correlationId is null && enumerator.MoveNext())
                {
                    if (enumerator.Current.Key == "correlationId")
                    {
                        correlationId = (string?)enumerator.Current.Value;
                    }
                }
            }

            // We extract most of the work into a non-generic method to save code size. If this was left in the generic
            // method, we'd get generic specialization for all TState parameters, but that's unnecessary.
            WriteInternal(scopeProvider, textWriter, message, logEntry.LogLevel, correlationId, logEntry.EventId.Id,
                logEntry.Exception?.ToString(), logEntry.Category, GetCurrentDateTime());
        }
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
        _optionsReloadToken = null;
    }

    private void ReloadLoggerOptions(PotConsoleFormatterOptions options)
    {
        _formatterOptions = options;
    }

    private void WriteInternal(IExternalScopeProvider? scopeProvider, TextWriter textWriter, string message, LogLevel logLevel,
        string? correlationId, int eventId, string? exceptionStr, string category, DateTimeOffset timestampOffset)
    {
        var logLevelString = GetLogLevelString(logLevel);

        if (logLevelString is not null)
        {
            textWriter.Write(logLevelString);
            textWriter.Write(LogLevelPadding);
        }

        string? timestamp = null;

        if (_formatterOptions.TimestampFormat is not null)
        {
            timestamp = _formatterOptions.UseUtcTimestamp
                ? $"{timestampOffset.ToString(_formatterOptions.TimestampFormat)} (utc)"
                : timestampOffset.ToString(_formatterOptions.TimestampFormat);
        }

        if (timestamp is not null)
        {
            textWriter.Write(timestamp);
            textWriter.Write(TimestampPadding);
        }

        if (correlationId is not null)
        {
            textWriter.Write('[');
            textWriter.Write(correlationId);
            textWriter.Write(']');
            textWriter.Write(CorrelationIdPadding);
        }

        // Example:
        // info: ConsoleApp.Program[10]
        //       Request received

        // category and event id
        textWriter.Write(category);
        textWriter.Write('[');

        Span<char> span = stackalloc char[10];

        if (eventId.TryFormat(span, out int charsWritten))
        {
            textWriter.Write(span[..charsWritten]);
        }
        else
        {
            textWriter.Write(eventId.ToString());
        }

        textWriter.Write(']');

        var singleLine = _formatterOptions.SingleLine;

        if (!singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }

        // scope information
        WriteScopeInformation(textWriter, scopeProvider, singleLine);
        WriteMessage(textWriter, message, singleLine);

        // Example:
        // System.InvalidOperationException
        //    at Namespace.Class.Function() in File:line X
        if (exceptionStr is not null)
        {
            // exception message
            WriteMessage(textWriter, exceptionStr, singleLine);
        }

        if (singleLine)
        {
            textWriter.Write(Environment.NewLine);
        }
    }

    private static void WriteMessage(TextWriter textWriter, string message, bool singleLine)
    {
        if (message.IsNotNullOrEmpty())
        {
            if (singleLine)
            {
                textWriter.Write(' ');
                WriteReplacing(textWriter, Environment.NewLine, " ", message);
            }
            else
            {
                textWriter.Write(_messagePadding);
                WriteReplacing(textWriter, Environment.NewLine, _newLineWithMessagePadding, message);
                textWriter.Write(Environment.NewLine);
            }
        }

        static void WriteReplacing(TextWriter writer, string oldValue, string newValue, string message)
        {
            var newMessage = message.Replace(oldValue, newValue);
            writer.Write(newMessage);
        }
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        return _formatterOptions.TimestampFormat is not null
            ? _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now
            : DateTimeOffset.MinValue;
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private void WriteScopeInformation(TextWriter textWriter, IExternalScopeProvider? scopeProvider, bool singleLine)
    {
        if (_formatterOptions.IncludeScopes && scopeProvider is not null)
        {
            var paddingNeeded = !singleLine;

            scopeProvider.ForEachScope((scope, state) =>
            {
                if (paddingNeeded)
                {
                    paddingNeeded = false;
                    state.Write(_messagePadding);
                    state.Write("=> ");
                }
                else
                {
                    state.Write(" => ");
                }

                state.Write(scope);
            }, textWriter);

            if (!paddingNeeded && !singleLine)
            {
                textWriter.Write(Environment.NewLine);
            }
        }
    }
}
