using Core.DI;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.loggly;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using static log4net.Appender.ManagedColoredConsoleAppender;

namespace Core.Logging
{
    public class LoggingModule : IModule
    {
        public LogglySettings LogglySettings { get; set; }

        public LoggingModule()
        {
            this.Configure();
        }

        public LoggingModule(LogglySettings logglySettings)
        {
            this.LogglySettings = logglySettings;
            this.Configure();
        }

        public void Register(IServiceCollection serviceCollection)
        {
            var log = LogManager.GetLogger(typeof(LoggingModule));
            serviceCollection.AddSingleton(log);
        }

        private void Configure()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            var managedColoredConsoleAppender = this.GetManagedColoredConsoleAppender();
            var logglyAppender = this.GetLogglyAppender();

            if (logglyAppender == null)
            {
                log4net.Config.BasicConfigurator.Configure(logRepository, managedColoredConsoleAppender);
                return;
            }

            log4net.Config.BasicConfigurator.Configure(logRepository, managedColoredConsoleAppender, logglyAppender);
        }

        private ManagedColoredConsoleAppender GetManagedColoredConsoleAppender()
        {
            var managedColoredConsoleAppender = new ManagedColoredConsoleAppender();
            managedColoredConsoleAppender.AddMapping(this.GetLevelColors(Level.Info, ConsoleColor.Green));
            managedColoredConsoleAppender.AddMapping(this.GetLevelColors(Level.Error, ConsoleColor.DarkRed));
            managedColoredConsoleAppender.AddMapping(this.GetLevelColors(Level.Warn, ConsoleColor.DarkYellow));
            managedColoredConsoleAppender.AddMapping(this.GetLevelColors(Level.Debug, ConsoleColor.Blue));

            PatternLayout patternLayout = new PatternLayout
            {
                ConversionPattern = "[%date] [%thread] [%-5level] - %message%newline"
            };

            patternLayout.ActivateOptions();

            managedColoredConsoleAppender.Layout = patternLayout;
            managedColoredConsoleAppender.ActivateOptions();

            return managedColoredConsoleAppender;
        }

        private LevelColors GetLevelColors(Level level, ConsoleColor foreColor)
        {
            return new LevelColors
            {
                Level = level,
                ForeColor = foreColor
            };
        }

        private LogglyAppender GetLogglyAppender()
        {
            if (this.LogglySettings == null)
            {
                return null;
            }

            var logglyAppender = new LogglyAppender
            {
                RootUrl = "https://logs-01.loggly.com/",
                CustomerToken = this.LogglySettings.LogglyCustomerToken,
                Tag = this.LogglySettings.LogglyTag
            };

            logglyAppender.ActivateOptions();

            return logglyAppender;
        }
    }
}