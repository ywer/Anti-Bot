using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_Bot.Interfaces
{
    public interface ILogging<TLogger, TLogLevel,MessageLevel>
        where TLogger : class
        where TLogLevel : Enum
        where MessageLevel : Enum
    {
        void Log(string message, MessageLevel LogLevel);

        void ChangeFileLogLevel(TLogLevel LogLevel);

        void ChangeConsoleLogLevel(TLogLevel LogLevel);

        TLogger ReturnFileLogger();

        TLogger ReturnConsoleLogger();

        TLogLevel ReturnCurrentConsoleLogLevel();

        TLogLevel ReturnCurrentFileLogLevel();

        TLogLevel ParseLogLevel(string Level);

        string ParseLogLevelToString(string Level);

        void CloseLoggers();
    }
}
