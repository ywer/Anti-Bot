using Anti_Bot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Anti_Bot.Internals;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Anti_Bot.Internals
{
    internal class ProgrammSettings : IProgrammSettings<ProgrammSettings.ProgrammData>
    {
        #region Data
        public class ProgrammData
        {
            public string LogLevelFile { get;set; }
            public string LoglevelConsole { get; set; }
            public bool DidSetup { get; set; }
        }
        private ProgrammData PSettings = new ProgrammData();
        SerLogging Log = new SerLogging();
        #endregion
        #region Data Save/Load

        public ProgrammSettings() 
        {
            string WorkPath = Directory.GetCurrentDirectory();
            string ConfigDir = WorkPath + @"\Config";
            string ServerConfigDir = ConfigDir + @"\ServerConfig\";
            try
            {

                if (!Directory.Exists(ConfigDir))
                {
                    Directory.CreateDirectory(ConfigDir);

                }

                if (!Directory.Exists(ServerConfigDir))
                {
                    Directory.CreateDirectory(ServerConfigDir);

                }


            }
            catch (Exception ex)
            {
                SerLogging logger = new SerLogging();
                logger.Log("Setting Error: " + ex,  SerLogging.MessageLevel.Error);
                return;
            }


            return;

        }

        public void LoadProgrammSettingsFromFile()
        {
            string WorkPath = Directory.GetCurrentDirectory();
            string ConfigDir = WorkPath + @"\Config\";
            string FilePath = ConfigDir + "\\Settings.json";

            if (File.Exists(FilePath))
            {
                var jsonString = File.ReadAllText(FilePath);
                var FileName = Path.GetFileNameWithoutExtension(FilePath);
                if (!string.IsNullOrEmpty(jsonString))
                {

                        var Data = JsonConvert.DeserializeObject<ProgrammData>(jsonString);
                        if (Data != null)
                        {

                            PSettings = Data;

                        }
                    
                }
            }

            return;
        }

        public void SaveProgrammSettingsToFile()
        {
            if(PSettings == null)
            {
                return;
            }
            string WorkPath = Directory.GetCurrentDirectory();
            string ConfigDir = WorkPath + @"\Config\";
            string FilePath = ConfigDir + "Settings.json";





            string json = JsonConvert.SerializeObject(PSettings, Formatting.Indented);
            File.WriteAllText(FilePath, json);




            return;



        }
        #endregion

        #region ChangeData
        public ProgrammData ReturnAllProgrammSettings()
        {
            NullCheck();

            return PSettings;
        }

        private void NullCheck()
        {
            if(PSettings == null)
            {
                LoadProgrammSettingsFromFile();
                
                return;
            }
            return;
        }


        public void SaveAllProgrammSettings(ProgrammData Settings)
        {
            NullCheck();
            PSettings = Settings;
            return;

        }

        public void ChangeFileLoglevel(string LogLevel)
        {
            NullCheck();
           string Level = Log.ParseLogLevelToString(LogLevel);
            PSettings.LogLevelFile = Level;
            return;

        }

        public string ReturnFileLogLevel() 
        {
            NullCheck();
            SerLogging Log = new SerLogging();
            string Level = PSettings.LogLevelFile;
            string LogLevel = Log.ParseLogLevelToString(Level);
            return LogLevel;
        }


        public void ChangeConsoleLogLevel(string LogLevel)
        {
            NullCheck();
            string Level = Log.ParseLogLevelToString(LogLevel);
            PSettings.LoglevelConsole = Level;
            return;

        }

        public string ReturnConsoleLogLevel()
        {
            NullCheck();
            SerLogging Log = new SerLogging();
            string Level = PSettings.LoglevelConsole;
            string LogLevel = Log.ParseLogLevelToString(Level);
            return LogLevel;
        }


        #endregion
    }
}
