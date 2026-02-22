// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Pastel;

namespace Endmin;

public static class Logger
{
    private static LogSeverity error { get; } = new("Error", ConsoleColor.Red, "#960000");
    private static LogSeverity warning { get; } = new("Warning", ConsoleColor.Yellow, "#a39800");
    private static LogSeverity debug { get; } = new("Debug");
    private static LogSeverity verbose { get; } = new("Verbose", ConsoleColor.Gray, "#004c75");

    public static LogCategory Runtime { get; } = new("Runtime");

    public static LogCategory Network { get; } = new("Network");

    public static LogCategory Io { get; } = new("IO");

    public record LogCategory(string Name);

    public record LogEvent(string Message, LogSeverity Severity, LogCategory Category, bool DisplayTimestamp, Guid Uuid);

    public record LogSeverity(string Name, ConsoleColor? ConsoleColor = null, string DebugOverlayColor = "#4f4f4f");

    private static void log(string message, LogSeverity severity, LogCategory category, bool displayTimestamp)
    {
        var logString = "";

        if (displayTimestamp)
        {
            var formattedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            logString += $"({formattedTime}) ";
        }

        logString += $"[{severity.Name}/{category.Name}]: {message}";

        if (severity.ConsoleColor != null)
        {
            logString = logString.Pastel(severity.ConsoleColor.Value);
        }

        Console.WriteLine(logString);
    }

    public static void Debug(string message) => log(message, debug, Runtime, true);
    public static void Verbose(string message) => log(message, verbose, Runtime, true);
    public static void Warning(string message) => log(message, warning, Runtime, true);
    public static void Error(string message) => log(message, error, Runtime, true);

    public static void Debug(string message, LogCategory category) => log(message, debug, category, true);
    public static void Verbose(string message, LogCategory category) => log(message, verbose, category, true);
    public static void Warning(string message, LogCategory category) => log(message, warning, category, true);
    public static void Error(string message, LogCategory category) => log(message, error, category, true);

    public static void Exception(Exception exception, LogCategory category)
    {
        log(exception.ToString(), error, category, true);
        if (exception.InnerException != null)
        {
            Exception(exception.InnerException, category);
        }
    }
}
