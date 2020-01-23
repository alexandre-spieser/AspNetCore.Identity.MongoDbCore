using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCore.Identity.MongoDbCore.IntegrationTests
{
    public class MockLoggerFactory : ILoggerFactory
    {
        public StringBuilder LogStore;

        public List<ILogger> Loggers = new List<ILogger>();

        public MockLoggerFactory()
        {
            LogStore = new StringBuilder();
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            var logger = new MockLogger(categoryName, LogStore);

            Loggers.Add(logger);

            return logger;
        }

        public ILogger<T> CreateLogger<T>() where T : class
        {
            //var logger = MockHelpers.MockILogger<T>(LogStore).Object;
            var logger = new MockLogger<T>(LogStore);

            Loggers.Add(logger);

            return logger;
        }

        public void Dispose()
        {
        }
    }

    public class MockLogger : MockLogger<object>
    {
        public String Name { get; protected set; }
        public MockLogger(string name, StringBuilder store) : base(store)
        {
            Name = name;
        }
    }

    public class MockLogger<T> : ILogger<T>
    {
        public StringBuilder LogMessages;

        public MockLogger(StringBuilder store)
        {
            LogMessages = store;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null)
            {
                LogMessages.Append(formatter(state, exception));
            }
            else
            {
                LogMessages.Append(state.ToString());
            }
        }
    }

}
