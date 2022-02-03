using Newtonsoft.Json;
using System;
using System.IO;

namespace IPFIXCollector
{
    class Program
    {
        static void Main(string[] args)
        {

            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json"))
            {
                Console.WriteLine("Settings.json not exists! Creating new one...");

                CollectorSettings collectorSettings = new CollectorSettings();
                collectorSettings.UDPPort = 4739;
                collectorSettings.SaveByDate = true;
                collectorSettings.SaveHeaderData = false;
                collectorSettings.SaveTemplateData = false;
                collectorSettings.SaveByTemplateID = true;
                collectorSettings.AddDateTimeToOutput = false;
                collectorSettings.Debug = false;
                collectorSettings.SaveDebugToFile = false;
                collectorSettings.PathToSaveFolder = "./output/";
                collectorSettings.SaveFileName = "output.json";
                collectorSettings.IndentedOutput = false;
                collectorSettings.SaveDataToFile = true;
                collectorSettings.SaveTemplates = false;
                collectorSettings.LoadTemplates = true;
                collectorSettings.SwapStringBytes = false;
                collectorSettings.SwapHexBytes = true;

                collectorSettings.Mediator.UseMediator = false;
                collectorSettings.Mediator.SendTo_IpAddress = "";
                collectorSettings.Mediator.SendTo_UDPPort = 4739;
                collectorSettings.Mediator.TinyToIPFix_ObservationDomainID = 0;
                collectorSettings.Mediator.From_UDPPort = 8739;

                MappingItem mappingItem = new MappingItem();
                mappingItem.FieldID = 8;
                mappingItem.EnterpriseNumber = 0;
                mappingItem.Name = "sourceIPv4Address";
                mappingItem.OutputMode = 2;
                collectorSettings.MappingList.Add(mappingItem);

                string output = JsonConvert.SerializeObject(collectorSettings, Formatting.Indented);
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json", output);
            }

            IPFixCollector ipfixCollector = new IPFixCollector();
            ipfixCollector.Listen();
        }

    }
}