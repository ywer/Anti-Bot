using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Anti_Bot.Interfaces;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using static Anti_Bot.Internals.SerLogging;

namespace Anti_Bot.Internals
{
    //https://github.com/serilog/serilog-sinks-file
    //https://github.com/serilog/serilog/wiki/Configuration-Basics
    //https://github.com/serilog/serilog/wiki/Getting-Started
    //https://github.com/serilog/serilog/wiki/Configuration-Basics#sinks
    //https://nblumhardt.com/2014/10/dynamically-changing-the-serilog-level/
    internal class SerLogging : ILogging<Logger, LogEventLevel,MessageLevel>
    {
        #region Data
        static Logger? FileLogger;
        static Logger? ConsoleLogger;
        static LoggingLevelSwitch MainFileLogSwitch;
        static LoggingLevelSwitch MainConsoleLogSwitch;
        static LogEventLevel? FileLogLevel;
        static LogEventLevel? ConsoleLogLevel;

        public enum MessageLevel
        {
            All,
            Debug,
            Info,
            Warning,
            Error,
            Empty,
            Mute
        }


        #endregion

        public SerLogging()
        {
            ProgrammSettings Settings = new ProgrammSettings();
            Program Main = new Program();
            string WorkPath = Directory.GetCurrentDirectory();
            string LoggingPath = WorkPath + "\\Logging\\";
            if (!Directory.Exists(LoggingPath))
            {
                Directory.CreateDirectory(LoggingPath);

            }

            if (FileLogger == null)
            {
                string FileLevel = Settings.ReturnFileLogLevel();
                if (string.IsNullOrEmpty(FileLevel))
                {
                    FileLevel = LogEventLevel.Warning.ToString();
                    
                }
                MainFileLogSwitch = new LoggingLevelSwitch();
                LogEventLevel Level = ParseLogLevel(FileLevel);
                MainFileLogSwitch.MinimumLevel= Level;
                string LogFilePath = LoggingPath + "Log.txt";
                FileLogger = new LoggerConfiguration()
                .WriteTo.File(LogFilePath, fileSizeLimitBytes: null, rollingInterval: RollingInterval.Day)
                .MinimumLevel.Warning()
                .MinimumLevel.ControlledBy(MainFileLogSwitch)
                .CreateLogger();
                

            }
            if (ConsoleLogger == null)
            {
                string ConsoleLevel = Settings.ReturnConsoleLogLevel();
                if(string.IsNullOrEmpty(ConsoleLevel))
                {
                    ConsoleLevel = LogEventLevel.Warning.ToString();
                }
                MainConsoleLogSwitch = new LoggingLevelSwitch();
                LogEventLevel LogLevel = ParseLogLevel(ConsoleLevel);
                MainConsoleLogSwitch.MinimumLevel= LogLevel;
                ConsoleLogger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Warning()
                .MinimumLevel.ControlledBy(MainConsoleLogSwitch)
                .CreateLogger();
                
            }

            return;
        }

        

        public void Log(string message, MessageLevel LogLevel)
        {
            if (string.IsNullOrEmpty(message)) 
            {
                return;
            }
            if (ConsoleLogger == null|| FileLogger == null) 
            {
                Console.WriteLine("No Logger!");
                return;
            }

            if (FileLogLevel == null || ConsoleLogLevel == null)
            {
                FileLogLevel = ReturnCurrentFileLogLevel();
                ConsoleLogLevel = ReturnCurrentConsoleLogLevel();
            }

            switch (LogLevel)
            {
                case MessageLevel.All:
                    FileLogger.Verbose(message);
                    ConsoleLogger.Verbose(message);
                    break;
                case MessageLevel.Debug:
                    FileLogger.Debug(message);
                    ConsoleLogger.Debug(message);
                    break;
                case MessageLevel.Info:
                    FileLogger.Information(message);
                    ConsoleLogger.Information(message);
                    break;
                case MessageLevel.Warning:
                    FileLogger.Warning(message);
                    ConsoleLogger.Warning(message);
                    break;
                case MessageLevel.Error:
                    FileLogger.Error(message);
                    ConsoleLogger.Error(message);
                    break;
                case MessageLevel.Mute:
                    break;
                default:
                    FileLogger.Warning(message);
                    ConsoleLogger.Warning(message);
                    break;

            }

            return;
        }

        public void ChangeFileLogLevel(LogEventLevel LogLevel)
        {
            if (FileLogger == null)
            {
                return;
            }
            ProgrammSettings Setting = new ProgrammSettings();
            Setting.ChangeFileLoglevel(LogLevel.ToString());
            FileLogLevel = LogLevel;

            if (MainFileLogSwitch != null)
            {
                MainFileLogSwitch.MinimumLevel = LogLevel;
            }


            return;
        }

        public void ChangeConsoleLogLevel(LogEventLevel LogLevel)
        {
            if (ConsoleLogger == null)
            {
                return;
            }
            ProgrammSettings Setting = new ProgrammSettings();
            Setting.ChangeConsoleLogLevel(LogLevel.ToString());
            ConsoleLogLevel = LogLevel;

            if (MainConsoleLogSwitch != null)
            {
                MainConsoleLogSwitch.MinimumLevel = LogLevel;
            }


            return;
        }

        public Logger ReturnFileLogger()
        {
            return FileLogger;
        }

        public Logger ReturnConsoleLogger()
        {
            return ConsoleLogger;

        }

        public LogEventLevel ReturnCurrentConsoleLogLevel()
        {
            ProgrammSettings settings = new ProgrammSettings();
            string level = settings.ReturnConsoleLogLevel();
            LogEventLevel LogLevel = ParseLogLevel(level);
            ConsoleLogLevel = LogLevel;
            return LogLevel;
        }

        public LogEventLevel ReturnCurrentFileLogLevel()
        {
            ProgrammSettings settings = new ProgrammSettings();
            string level = settings.ReturnFileLogLevel();
            LogEventLevel LogLevel = ParseLogLevel(level);
            FileLogLevel = LogLevel;
            return LogLevel;
        }

        public LogEventLevel ParseLogLevel(string Level)
        {
            LogEventLevel level = LogEventLevel.Verbose;
            if (Enum.TryParse<LogEventLevel>(Level, true, out level))
            {

                return level;
            }
            else
            {
                return LogEventLevel.Verbose;
            }
        }

        public string ParseLogLevelToString(string Level)
        {
            LogEventLevel level = LogEventLevel.Verbose;
            if (Enum.TryParse<LogEventLevel>(Level, true, out level))
            {

                return level.ToString();
            }
            else
            {
                return LogEventLevel.Verbose.ToString();
            }
        }

        public void CloseLoggers()
        {
            Log("Shutting down Logging",  MessageLevel.Info);
            if (FileLogger != null)
            {
                FileLogger.DisposeAsync();
            }
            if (ConsoleLogger != null)
            {
                ConsoleLogger.DisposeAsync();
            }
            return;
        }

    }

}