using System;
using System.Collections.Generic;
using System.Text;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.DataTemplates
{
    public interface IDataTemplate
    {
        public int GetTemplateID();
        public int GetFieldCount();

        public int GetEnterpriseFieldCount();

        public List<IPFix_Template_Data> GetTemplateList();
    }
}
