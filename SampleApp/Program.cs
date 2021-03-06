﻿using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SampleApp
{
    internal class Program
    {
        #region NLog

        private static readonly NLog.LogLevel[] _nLogLogLevelArray = new NLog.LogLevel[] {
                    NLog.LogLevel.Trace,
                    NLog.LogLevel.Debug,
                    NLog.LogLevel.Info,
                    NLog.LogLevel.Warn,
                    NLog.LogLevel.Error,
                    NLog.LogLevel.Fatal };

        private static Action<DateTime, int, string, Exception> NLogLogHandlerProvider(string loggerName)
        {
            var nLogLogger = NLog.LogManager.GetLogger(loggerName);

            return new Action<DateTime, int, string, Exception>(delegate (DateTime timeStamp, int logLevel, string message, Exception exception)
            {
                NLog.LogEventInfo logEvent = new NLog.LogEventInfo(_nLogLogLevelArray[logLevel], loggerName, message);
                logEvent.TimeStamp = timeStamp;
                nLogLogger.Log(logEvent);
            });
        }

        #endregion NLog

        #region Serilog

        private static readonly Serilog.Parsing.MessageTemplateParser _seriLogMessageTemplateParser = new Serilog.Parsing.MessageTemplateParser();
        private static LoggerConfiguration _loggerConfig;

        private static Serilog.Events.LogEventLevel[] _seriLogLogLevelArray = new Serilog.Events.LogEventLevel[] {
                    Serilog.Events.LogEventLevel.Verbose,
                    Serilog.Events.LogEventLevel.Debug,
                    Serilog.Events.LogEventLevel.Information,
                    Serilog.Events.LogEventLevel.Warning,
                    Serilog.Events.LogEventLevel.Error,
                    Serilog.Events.LogEventLevel.Fatal };

        private static Action<DateTime, int, string, Exception> SerilogLogHandlerProvider(string loggerName)
        {
            var seriLogLogger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Console().WriteTo.File("serilog\\serilog.log").CreateLogger();

            return new Action<DateTime, int, string, Exception>(delegate (DateTime timeStamp, int logLevel, string message, Exception exception)
            {
                DateTimeOffset timeStampOffset = new DateTimeOffset(timeStamp);

                List<Serilog.Parsing.MessageTemplateToken> tokens = new List<Serilog.Parsing.MessageTemplateToken>();
                Serilog.Parsing.MessageTemplateParser parser = new Serilog.Parsing.MessageTemplateParser();

                Serilog.Events.MessageTemplate messageTemplate = _seriLogMessageTemplateParser.Parse(message);

                List<Serilog.Events.LogEventProperty> properties = new List<Serilog.Events.LogEventProperty>();

                Serilog.Events.LogEvent logEvent = new Serilog.Events.LogEvent(timeStampOffset, _seriLogLogLevelArray[logLevel], exception, messageTemplate, properties);

                seriLogLogger.Write(logEvent);
            });
        }

        #endregion Serilog

        #region Log4Net

        private static log4net.Core.Level[] _log4NetLogLevelArray = new log4net.Core.Level[] {
                    log4net.Core.Level.Trace,
                    log4net.Core.Level.Debug,
                    log4net.Core.Level.Info,
                    log4net.Core.Level.Warn,
                    log4net.Core.Level.Error,
                    log4net.Core.Level.Fatal };

        private static log4net.Repository.ILoggerRepository _log4netLogRepo;

        private static Action<DateTime, int, string, Exception> Log4NetLogHandlerProvider(string loggerName)
        {
            if (_log4netLogRepo == null)
            {
                _log4netLogRepo = log4net.LogManager.GetRepository(Assembly.GetEntryAssembly());
                log4net.Config.XmlConfigurator.ConfigureAndWatch(_log4netLogRepo, new FileInfo("log4net.config"));
            }

            var log4netLogger = _log4netLogRepo.GetLogger(loggerName);

            return new Action<DateTime, int, string, Exception>(delegate (DateTime timeStamp, int logLevel, string message, Exception exception)
            {
                log4net.Core.LoggingEventData logEventData = new log4net.Core.LoggingEventData();

                logEventData.Level = _log4NetLogLevelArray[logLevel];
                logEventData.TimeStampUtc = timeStamp.ToUniversalTime();

                logEventData.Message = message;

                if (exception != null) logEventData.ExceptionString = exception.ToString();

                log4net.Core.LoggingEvent logEvent = new log4net.Core.LoggingEvent(logEventData);
                log4netLogger.Log(logEvent);
            });
        }

        #endregion Log4Net

        private static void Main(string[] args)
        {
            Console.WriteLine("0) None");
            Console.WriteLine("1) NLog");
            Console.WriteLine("2) Log4Net");
            Console.WriteLine("3) Serilog");
            Console.Write("Select a logging framework (Enter 0-3): ");

            int selectedLogFramework;
            if (int.TryParse(Console.ReadLine(), out selectedLogFramework))
            {
                switch (selectedLogFramework)
                {
                    case 1:
                        Console.WriteLine("NLog logging framework selected");
                        SampleLibrary.Logger.LogHandlerProvider = new Func<string, Action<DateTime, int, string, Exception>>(NLogLogHandlerProvider);
                        break;

                    case 2:
                        Console.WriteLine("log4net logging framework selected");
                        SampleLibrary.Logger.LogHandlerProvider = new Func<string, Action<DateTime, int, string, Exception>>(Log4NetLogHandlerProvider);
                        break;

                    case 3:
                        Console.WriteLine("serilog logging framework selected");
                        SampleLibrary.Logger.LogHandlerProvider = new Func<string, Action<DateTime, int, string, Exception>>(SerilogLogHandlerProvider);
                        break;

                    default:
                        Console.WriteLine("No logging framework selected");
                        break;
                }

                SampleLibrary.TestClass testClass = new SampleLibrary.TestClass();
                testClass.TestLogging();

                Console.WriteLine("Press any key...");
                Console.ReadKey();
            }
        }
    }
}