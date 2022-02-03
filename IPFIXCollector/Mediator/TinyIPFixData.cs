using IPFIXCollector.DataTemplates;
using System;
using System.Collections.Generic;
using System.Text;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.Mediator
{
    public class TinyIPFixData
    {
        public UInt32 Sequence_Number;
        public UInt16 TemplateID;
        public int MessageType;
        public List<DataContent> Data = new List<DataContent>();

        public void ClearData()
        {
            Data.Clear();
        }
    }
}
