using System;

namespace SampleLibrary
{
    public class TestClass
    {
        private static readonly Logger _logger = Logger.GetLogger<TestClass>();

        public void TestLogging()
        {
            _logger.Trace("Test Trace message");
            _logger.Debug("Test Debug message");
            _logger.Info("Test Info message");
            _logger.Warn("Test Warn message");
            _logger.Error("Test Error message");
            _logger.Fatal("Test Fatal message");
        }
    }
}