using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IPFIXCollector.Workers
{
    public class MessageLogger
    {
        CollectorSettings collectorSettings;
        public MessageLogger(CollectorSettings s_collectorSettings)
        {
            collectorSettings = s_collectorSettings;
        }

        public void PrintError(string text)
        {
            Console.WriteLine(text);

            if (collectorSettings.SaveDebugToFile)
            {
                SaveDebugToFile(text);
            }
        }

        public void PrintDebug(string text)
        {
            if (collectorSettings.Debug)
            {
                Console.WriteLine(text);
            }

            if(collectorSettings.SaveDebugToFile)
            {
                SaveDebugToFile(text);
            }
        }

        private void SaveDebugToFile(string text)
        {
            File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory+"/debug.log", text + Environment.NewLine);
        }
    }
}
