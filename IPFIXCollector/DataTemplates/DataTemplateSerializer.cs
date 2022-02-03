using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IPFIXCollector.DataTemplates
{
    public class DataTemplateSerializer
    {
        public void SaveTemplate(DataTemplate dataTemplate)
        {
            string output = JsonConvert.SerializeObject(dataTemplate);

            if(!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory+"/Templates/"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Templates/");
            }

            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "/Templates/template_"+dataTemplate.TemplateID+".template", output);
        }


        public List<DataTemplate> LoadTemplates()
        {
            List<DataTemplate> datalist = new List<DataTemplate>();

            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/Templates/"))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/Templates/");
                return datalist;
            }

            foreach(string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "/Templates/"))
            {
                string input = File.ReadAllText(file);
                DataTemplate data = JsonConvert.DeserializeObject<DataTemplate>(input);
                datalist.Add(data);
            }

            return datalist;
        }
    }
}
