using Anti_Bot.Interfaces;
using Anti_Bot.Internals;
using Serilog;

namespace Anti_Bot
{
    internal class Program
    {
        #region Data
        SerLogging Log = new SerLogging();
        public static double Version = 0.00001;
        #endregion
        static void Main(string[] args)
        {
          ProgrammSettings settings = new ProgrammSettings();
            SerLogging Log = new SerLogging();
            Console.WriteLine("AntiBot, Version: " + Version);
            Log.Log("AntiBot, Version: " + Version,  SerLogging.MessageLevel.Info);
            settings.LoadProgrammSettingsFromFile();
            var Settings = settings.ReturnAllProgrammSettings();
            if(!Settings.DidSetup)
            { 
                DoSetup();
                Program program = new Program();
                program.Shutdown(true);
                return;
            }



        }




        public void Shutdown(bool restart)
        {
            ProgrammSettings settings = new ProgrammSettings();
            Log.Log("Application Quit...",  SerLogging.MessageLevel.Info);
            settings.SaveProgrammSettingsToFile();

            if (restart)
            {
                Log.Log("Restart Application",  SerLogging.MessageLevel.Info);
                //Start process, friendly name is something like MyApp.exe (from current bin directory)
                Thread.Sleep(5000);
                System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.FriendlyName);

                //Close the current process
                Environment.Exit(0);
            }
            else
            {
                Thread.Sleep(5000);
                Log.Log("Quit Application", SerLogging.MessageLevel.Info);
                Environment.Exit(0);
            }
            return;
        }


    }
}
