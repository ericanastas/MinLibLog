# MinLibLog

MinLibLog is a minimal logging abstraction for .NET Library developers that avoids adding a dependency to any specific logging framework. It is based off [Damian Hickey's LibLog](https://github.com/damianh/LibLog) project, but does not require consuming applications to reference any types outside the .NET Framework/.NET Core `System` namespace.

# Library Project Usage

In library projects loggers are created to log library events to.

1.  Copy [Logger.cs](https://github.com/aireq/MinLibLog/blob/master/SampleLibrary/Logger.cs) into the library project
2.  Change the namespace from `SampleLibrary` to the namespace of the library project.
3.  Create loggers in the library by calling `Logger.GetLogger<T>()` or `Logger.GetLogger(string name)`.
4.  Log messages to various methods on these `Logger` instances.

**Example:**

```C#
public class MyClass
{
    //Create a Logger for MyClass
    private static readonly Logger _logger = Logger.GetLoggerFor<MyClass>();

    public MyClass()
    {
        try
        {
            //Log a simple text message
            _logger.Info("Simple info level log message");

            //Log a composite message and arguments
            _logger.Debug("Logging with arguments: UserName = {0}, Machine = {1}", System.Environment.UserName, System.Environment.MachineName);

            throw new Exception("Exception to log");
        }
        catch (Exception exp)
        {
            //Log Error message with exception
            _logger.Error("{0} exception thrown", exp, exp.GetType().Name);
        }
    }
}
```


# Application Project Usage

Applications configure the destination of log messages by setting `Logger.LogHandlerProvider` for each library using MinLibLog.

```C#
var logFunc = new Func<string, Action<DateTime, int, string, Exception>>(LogHandlerProvider);

LibraryA.Logger.LogHandlerProvider = logFunc;
LibraryB.Logger.LogHandlerProvider = logFunc;
LibraryC.Logger.LogHandlerProvider = logFunc;

```

`LogHandlerProvider` accepts a logger name, and returns an `Action<DateTime, int, string, Exception>` delegate to handle log messages from the library. These these delegates take five arguments.

1. **DateTime:** Timestamp of the log event
2. **int:** An integer from 0-5 that identifies the level or severity of the log message. (See below)
3. **string:** A message describing the logged event.
4. **Exception:** An exception to log. This is optional and may be null.

**Log Levels**

- **Trace [0]:** Very detailed logs, which may include high-volume information such as protocol payloads. This log level is typically only enabled during development
- **Debug [1]:** Debugging information, less detailed than trace, typically not enabled in production environment.
- **Info [2]:** Information messages, which are normally enabled in production environment
- **Warn [3]:** Warning messages, typically for non-critical issues, which can be recovered or which are temporary failures
- **Error [4]:** Error messages - most of the time these are Exceptions
- **Fatal [5]:** Very serious errors!


# Log Framework Examples

The following show examples of configuring MinLibLog for various popular logging frameworks.

## NLog

MinLibLog configuration example for [NLog](https://nlog-project.org/)

```C#
private static readonly NLog.LogLevel[] _nLogLogLevelArray = new NLog.LogLevel[] {
            NLog.LogLevel.Trace,
            NLog.LogLevel.Debug,
            NLog.LogLevel.Info,
            NLog.LogLevel.Warn,
            NLog.LogLevel.Error,
            NLog.LogLevel.Fatal };

private static Action<DateTime, int, string, Exception> LogHandlerProvider(string loggerName)
{
    var nLogLogger = NLog.LogManager.GetLogger(loggerName);

    return new Action<DateTime, int, string, Exception>(delegate (DateTime timeStamp, int logLevel, string message, Exception exception)
    {
        NLog.LogEventInfo logEvent = new NLog.LogEventInfo(_nLogLogLevelArray[logLevel], loggerName, System.Globalization.CultureInfo.InvariantCulture, message);
        logEvent.TimeStamp = timeStamp;
        nLogLogger.Log(logEvent);
    });
}

```

## Log4Net


MinLibLog configuration example for [log4net](http://logging.apache.org/log4net/)

```C#
private static log4net.Core.Level[] _log4NetLogLevelArray = new log4net.Core.Level[] {
            log4net.Core.Level.Trace,
            log4net.Core.Level.Debug,
            log4net.Core.Level.Info,
            log4net.Core.Level.Warn,
            log4net.Core.Level.Error,
            log4net.Core.Level.Fatal };

private static log4net.Repository.ILoggerRepository _log4netLogRepo;

private static Action<DateTime, int, string, Exception> LogHandlerProvider(string loggerName)
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
```

## Serilog


MinLibLog configuration example for [Serilog](https://serilog.net/)

```C#

private static readonly Serilog.Parsing.MessageTemplateParser _seriLogMessageTemplateParser = new Serilog.Parsing.MessageTemplateParser();
private static LoggerConfiguration _loggerConfig;

private static Serilog.Events.LogEventLevel[] _seriLogLogLevelArray = new Serilog.Events.LogEventLevel[] {
            Serilog.Events.LogEventLevel.Verbose,
            Serilog.Events.LogEventLevel.Debug,
            Serilog.Events.LogEventLevel.Information,
            Serilog.Events.LogEventLevel.Warning,
            Serilog.Events.LogEventLevel.Error,
            Serilog.Events.LogEventLevel.Fatal };

private static Action<DateTime, int, string, Exception> LogHandlerProvider(string loggerName)
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

```
