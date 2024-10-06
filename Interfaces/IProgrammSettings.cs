using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anti_Bot.Interfaces
{
    public interface IProgrammSettings<TProgrammData>
    {


        public TProgrammData ReturnAllProgrammSettings();
        public void SaveAllProgrammSettings(TProgrammData programmSettings);

        public void LoadProgrammSettingsFromFile();
        public void SaveProgrammSettingsToFile();


    }
}
