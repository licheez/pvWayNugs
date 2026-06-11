using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using pvNugsLoggerNc10Abstractions;

namespace pvNugsCacheNc10Memory.ut;

/// <summary>
/// No-op <see cref="ILoggerService"/> for use in unit tests.
/// All operations are silently discarded.
/// </summary>
internal sealed class NullLoggerService : ILoggerService
{
    // ── ILogger ────────────────────────────────────────────────────────────
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter) { }
    public bool IsEnabled(LogLevel logLevel) => false;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    // ── ILoggerService – context ───────────────────────────────────────────
    public void SetUser(string? userId, string? companyId = null) { }
    public void SetTopic(string? topic) { }

    // ── ILoggerService – sync overloads ────────────────────────────────────
    public void Log(string message, SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(IEnumerable<string> messages, SeverityEnu severity,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(Exception e, SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(IMethodResult result,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(IEnumerable<string> messages, string? topic, SeverityEnu severity,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(string message, string? topic, SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(IMethodResult result, string? topic,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    public void Log(Exception e, string? topic, SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) { }

    // ── ILoggerService – async overloads ───────────────────────────────────
    public Task LogAsync(string message, SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(IEnumerable<string> messages, SeverityEnu severity,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(Exception e, SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(IMethodResult result,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(string message, string? topic, SeverityEnu severity = SeverityEnu.Debug,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(IEnumerable<string> messages, string? topic, SeverityEnu severity,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(Exception e, string? topic, SeverityEnu severity = SeverityEnu.Fatal,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    public Task LogAsync(IMethodResult result, string? topic,
        [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = -1) => Task.CompletedTask;

    // ── IDisposable / IAsyncDisposable ─────────────────────────────────────
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

