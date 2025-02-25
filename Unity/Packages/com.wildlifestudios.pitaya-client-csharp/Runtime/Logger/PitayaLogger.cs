using System;
using System.Text;
using System.Runtime.CompilerServices;
using Wildlife.PitayaCSharp.Logger.Providers;

namespace Wildlife.PitayaCSharp.Logger
{
    public class PitayaLogger : IPitayaLogger
    {
        public PitayaLogLevel LogLevel { get; set; }
        LogFunction _logFunction;
        IFormatter _formatter;

        internal PitayaLogger(PitayaLogLevel logLevel, LogFunction logFunction)
        {
            LogLevel = logLevel;
            _logFunction = logFunction ?? throw new ArgumentNullException(nameof(logFunction));
            _formatter = new DefaultLoggerFormatter();
        }

        public void LogDebug(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Debug, message, caller, line, file, args);
        }

        public void LogInfo(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Info, message, caller, line, file, args);
        }

        public void LogWarning(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Warn, message, caller, line, file, args);
        }

        public void LogError(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Error, message, caller, line, file, args);
        }

        public void LogFatal(string message, [CallerMemberName] string caller = null, [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Fatal, message, caller, line, file, args);
        }

        public void LogWarning(Exception exception, string message, [CallerMemberName] string caller = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Warn, exception, message, caller, line, file, args);
        }

        public void LogError(Exception exception, string message, [CallerMemberName] string caller = null,
            [CallerLineNumber] int line = 0,
            [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Error, exception, message, caller, line, file, args);
        }

        public void LogFatal(Exception exception, string message, [CallerMemberName] string caller = null,
            [CallerLineNumber] int line = 0, [CallerFilePath] string file = null, params object[] args)
        {
            Log(PitayaLogLevel.Fatal, exception, message, caller, line, file, args);
        }

        private void Log(PitayaLogLevel logLevel, string message, string caller = null, int line = 0,
            string file = null, params object[] args)
        {
            Log(logLevel, null, message, caller, line, file, args);
        }

        private void Log(PitayaLogLevel logLevel, Exception exception, string message, string caller, int line, string file,
            params object[] args)
        {
            if (_logFunction == null)
            {
                return;
            }

            if (logLevel < 0 || logLevel < LogLevel)
            {
                return;
            }

            string formattedMessage = (args == null) ? string.Format(message, args) : message;

            LogMessage logMessage = BuildLogMessage(logLevel, exception, formattedMessage, caller, line, file);

            _logFunction(logLevel, _formatter.Format(logMessage));
        }

        private LogMessage BuildLogMessage(PitayaLogLevel logLevel, Exception exception, string message, string caller, int line,
            string file)
        {
            return new LogMessage
            {
                ErrorMessage = exception?.Message,
                StackTrace = exception?.StackTrace,
                Level = logLevel,
                Message = message,
                ClientTimestamp = DateTime.Now,
                FunctionName = caller,
                FileName = file,
                LineNumber = line,
            };
        }
    }
}