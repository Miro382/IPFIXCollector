using System.Collections.Generic;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.DataTemplates
{
    public class DataTemplate : IDataTemplate
    {
        public int TemplateID { get; set; }

        public int FieldCount { get; set; }

        public List<IPFix_Template_Data> template_list = new List<IPFix_Template_Data>();

        public int EnterpriseFieldCount { get; set; }


        public int GetTemplateID()
        {
            return TemplateID;
        }

        public int GetFieldCount()
        {
            return FieldCount;
        }

        public List<IPFix_Template_Data> GetTemplateList()
        {
            return template_list;
        }

        public int GetEnterpriseFieldCount()
        {
            return EnterpriseFieldCount;
        }
    }
}
