using System;
using System.Runtime.CompilerServices;

namespace Wildlife.PitayaCSharp.Logger
{
    public delegate void LogFunction(PitayaLogLevel level, string message);

    /// <summary>
    /// Performs corrected leveled logging.
    /// </summary>
    public interface IPitayaLogger
    {
        PitayaLogLevel LogLevel { get; set;}

        /// <summary>
        /// Logs on the <c>Debug</c> level.
        /// Information logged on this level should be useful for debugging. Avoid enabling it on production builds.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogDebug(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);

        /// <summary>
        /// Logs on the <c>Information</c> level.
        /// Information logged on this level should be used for checking system flow and business logic. Use it sparsely.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogInfo(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);

        /// <summary>
        /// Logs on the <c>Warn</c> level.
        /// Information logged on this level should contain events that could potentially become errors.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogWarning(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);
        
        /// <summary>
        /// Logs on the <c>Error</c> level.
        /// This level should contain all error conditions.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogError(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);
        
        /// <summary>
        /// Logs on the <c>Fatal</c> level.
        /// Events on this level should mean the end/crash of the program.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogFatal(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);

        /// <summary>
        /// Logs on the <c>Warn</c> level.
        /// Information logged on this level should contain events that could potentially become errors.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogWarning(Exception exception, string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);
        
        /// <summary>
        /// Logs on the <c>Error</c> level.
        /// This level should contain all error conditions.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogError(Exception exception, string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);
        
        /// <summary>
        /// Logs on the <c>Fatal</c> level.
        /// Events on this level should mean the end/crash of the program.
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="message">Message</param>
        /// <param name="caller">Name of the function from where the method is called. Does not need to be provided.</param>
        /// <param name="line">Number of the line from where the method is called. Does not need to be provided.</param>
        /// <param name="file">Name of the file from where the method is called. Does not need to be provided.</param>
        /// <param name="args">Formatting parameters</param>
        void LogFatal(Exception exception, string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args);
    }
}