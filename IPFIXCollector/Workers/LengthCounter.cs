using IPFIXCollector.DataTemplates;
using System;
using System.Collections.Generic;
using System.Text;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.Workers
{
    public static class LengthCounter
    {
        public static int CountLenghtOfFieldsTemplate(DataTemplate dataTemplate)
        {
            int lensize = 0;

            foreach (IPFix_Template_Data data in dataTemplate.template_list)
            {
                if (data.EnterpriseNumber > 0)
                {
                    lensize += 4;
                }

                lensize += 4;
            }

            return lensize;
        }

        public static int CountLenghtOfFieldsData(DataTemplate dataTemplate)
        {
            int lensize = 0;

            foreach (IPFix_Template_Data data in dataTemplate.template_list)
            {
                lensize += data.FieldDataLength;
            }

            return lensize;
        }
    }
}
