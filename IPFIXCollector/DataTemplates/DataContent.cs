using System;
using System.Collections.Generic;
using System.Text;

namespace IPFIXCollector.DataTemplates
{
    public class DataContent
    {
        public byte[] Data;

        public DataContent()
        {

        }

        public DataContent(byte[] data)
        {
            Data = data;
        }
    }
}
