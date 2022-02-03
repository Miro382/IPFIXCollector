using System;
using System.Collections.Generic;
using System.Text;

namespace IPFIXCollector
{
    public class CollectorSettings
    {
        public int UDPPort;
        public bool SaveByDate;
        public bool SaveByTemplateID;
        public bool AddDateTimeToOutput;
        public bool SaveHeaderData;
        public bool SaveTemplateData;
        public bool IndentedOutput;
        public bool Debug;
        public bool SaveDebugToFile;
        public string PathToSaveFolder;
        public string SaveFileName;
        public bool LoadTemplates;
        public bool SaveTemplates;
        public bool SwapStringBytes;
        public bool SwapHexBytes;
        public List<MappingItem> MappingList = new List<MappingItem>();
        public bool SaveDataToFile;
        public MediatorSettings Mediator = new MediatorSettings();
    }
}
