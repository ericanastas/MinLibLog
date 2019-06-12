using System;

namespace SampleLibrary
{
    /// <summary>
    /// MinLibLog Logging abstraction class
    /// </summary>
    /// <remarks>See https://github.com/aireq/MinLibLog for more information</remarks>
    public class Logger
    {
        private Action<DateTime, int, string, System.Exception, object[]> _logHandler;

        private Logger(string name, Action<DateTime, int, string, System.Exception, object[]> logHandler)
        {
            _logHandler = logHandler;
            Name = name ?? string.Empty;
        }

        /// <summary>
        /// The serverity level of a log event
        /// </summary>
        internal enum LogLevel
        {
            /// <summary>
            /// Very serious errors!
            /// </summary>
            Fatal = 5,

            /// <summary>
            ///  Error messages - most of the time these are Exceptions
            /// </summary>
            Error = 4,

            /// <summary>
            /// Warning messages, typically for non-critical issues, which can be recovered or which are temporary failures
            /// </summary>
            Warn = 3,

            /// <summary>
            /// Information messages, which are normally enabled in production environment
            /// </summary>
            Info = 2,

            /// <summary>
            /// Debugging information, less detailed than trace, typically not enabled in production environment.
            /// </summary>
            Debug = 1,

            /// <summary>
            /// Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
            /// </summary>
            Trace = 0
        }

        /// <summary>
        /// Func delegate that accepts a logger name, and returns an Action delegate that log messages should be sent to.
        /// </summary>
        /// <remarks></remarks>
        public static Func<string, Action<DateTime, int, string, System.Exception, object[]>> LogHandlerProvider { get; set; }
        /// <summary>
        /// The name of the logger
        /// </summary>
        private string Name { get; }

        /// <summary>
        /// Creates a new Logger with the provided name
        /// </summary>
        /// <param name="name">The logger name</param>
        /// <returns>A Logger instance with the provided name</returns>
        internal static Logger GetLogger(string name)
        {
            if (LogHandlerProvider != null)
            {
                return new Logger(name, LogHandlerProvider(name));
            }
            else
            {
                return new Logger(name, null);
            }
        }

        /// <summary>
        /// Creates a new Logger for the type T.
        /// </summary>
        /// <remarks>Logger name will equal the full name of the type</remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>A Logger named the full name of the type T</returns>
        internal static Logger GetLogger<T>()
        {
            var type = typeof(T);
            return GetLogger(type.FullName);
        }

        /// <summary>
        /// Logs a debug level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Debug(string message, params object[] args)
        {
            Log(LogLevel.Debug, message, args);
        }

        /// <summary>
        /// Logs a debug level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Debug(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Debug, message, exception, args);
        }

        /// <summary>
        /// Logs a error level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Error(string message, params object[] args)
        {
            Log(LogLevel.Error, message, args);
        }

        /// <summary>
        /// Logs a error level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Error(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Error, message, exception, args);
        }

        /// <summary>
        /// Logs a fatal level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Fatal(string message, params object[] args)
        {
            Log(LogLevel.Fatal, message, args);
        }

        /// <summary>
        /// Logs a fatal level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Fatal(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Fatal, message, exception, args);
        }

        /// <summary>
        /// Logs a info level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Info(string message, params object[] args)
        {
            Log(LogLevel.Info, message, args);
        }

        /// <summary>
        /// Logs a info level event
        /// <summary>
        /// Logs a info level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Info(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Info, message, exception, args);
        }

        /// </summary>
        /// <param name="level">The log level of the event</param>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Log(LogLevel level, string message, System.Exception exception, params object[] args)
        {
            if (_logHandler != null)
            {
                _logHandler(DateTime.Now, (int)level, message, exception, args);
            }
        }

        /// </summary>
        /// <param name="level">The log level of the event</param>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">A log message to associate with</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Log(LogLevel level, string message, params object[] args)
        {
            this.Log(level, message, null, args);
        }

        /// <summary>
        /// <summary>
        /// Logs a trace level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Trace(string message, params object[] args)
        {
            Log(LogLevel.Trace, message, args);
        }

        /// <summary>
        /// Logs a trace level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Trace(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Trace, message, exception, args);
        }

        /// <summary>
        /// Logs a warn level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Warn(string message, params object[] args)
        {
            Log(LogLevel.Warn, message, args);
        }

        /// <summary>
        /// Logs a warn level event
        /// </summary>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Warn(string message, Exception exception, params object[] args)
        {
            Log(LogLevel.Warn, message, exception, args);
        }
    }
}