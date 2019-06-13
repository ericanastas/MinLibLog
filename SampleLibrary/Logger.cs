using System;
using System.Collections.Generic;

namespace SampleLibrary
{
    /// <summary>
    /// MinLibLog Logging abstraction class
    /// </summary>
    /// <remarks>See https://github.com/aireq/MinLibLog for more information</remarks>
    public class Logger
    {
        private static IDictionary<string, Logger> _loggers = new Dictionary<string, Logger>();
        private static Func<string, Action<DateTime, int, string, System.Exception>> _logHandlerProvider;

        private Logger(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            RefreshLogHandler();
        }

        /// <summary>
        /// Event fires whenever an event is logged from any logger
        /// </summary>
        internal static event EventHandler<LogEventArgs> EventLogged;

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
        public static Func<string, Action<DateTime, int, string, System.Exception>> LogHandlerProvider
        {
            get
            {
                return _logHandlerProvider;
            }

            set
            {
                if (_logHandlerProvider != value)
                {
                    _logHandlerProvider = value;

                    //Refreshes LogHandler for all Loggers
                    foreach (var logger in _loggers.Values) logger.RefreshLogHandler();
                }
            }
        }

        private Action<DateTime, int, string, System.Exception> LogHandler { get; set; }

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
            if (_loggers.ContainsKey(name))
            {
                var logger = new Logger(name);
                _loggers.Add(logger.Name, logger);
            }
            return _loggers[name];
        }

        /// <summary>
        /// Creates a new Logger for the type T.
        /// </summary>
        /// <remarks>Logger name will equal the full name of the type</remarks>
        /// <typeparam name="T"></typeparam>
        /// <returns>A Logger named the full name of the type T</returns>
        internal static Logger GetLoggerFor<T>()
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

        ///<summary>
        /// Logs an event
        ///</summary>
        /// <param name="level">The log level of the event</param>
        /// <param name="message">A message to log as a composite format string</param>
        /// <param name="exception">An exception to associate with the log event</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        internal void Log(LogLevel level, string message, System.Exception exception, params object[] args)
        {
            if (LogHandler != null | EventLogged != null)
            {
                var timeStamp = DateTime.Now;

                //Format
                string formattedMessage = string.Format(message, args);

                //Fires EventLogged Event
                var eventLoggedHandler = EventLogged;
                if (eventLoggedHandler != null)
                {
                    LogEventArgs logEventArgs = new LogEventArgs(this.Name, timeStamp, level, formattedMessage, exception);
                    eventLoggedHandler.Invoke(null, logEventArgs);
                }

                if (LogHandler != null)
                {
                    LogHandler(timeStamp, (int)level, formattedMessage, exception);
                }
            }
        }

        /// <summary>
        /// Logs an event
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

        /// <summary>
        /// Refresh the LogHandler from the current LogHandlerProvider
        /// </summary>
        private void RefreshLogHandler()
        {
            if (LogHandlerProvider != null) LogHandler = LogHandlerProvider(Name);
            else LogHandler = null;
        }

        /// <summary>
        /// EventArgs used the Logger.EventLogged event
        /// </summary>
        internal class LogEventArgs : EventArgs
        {
            public LogEventArgs(string loggerName, DateTime timeStamp, LogLevel logLevel, string message, System.Exception exception)
            {
                this.LoggerName = loggerName ?? throw new ArgumentNullException(nameof(loggerName));
                this.TimeStamp = timeStamp;
                this.LogLevel = logLevel;
                this.Message = message;
                this.Exception = exception;
            }

            public System.Exception Exception { get; }
            public string LoggerName { get; }
            public LogLevel LogLevel { get; }

            public string Message { get; }
            public DateTime TimeStamp { get; }
        }
    }
}