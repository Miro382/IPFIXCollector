using IPFIXCollector.DataTemplates;
using IPFIXCollector.Workers;
using System;
using System.Collections.Generic;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.Mediator
{
    public class TinyIPFixConvertor
    {
        TinyIPFixData tinyIPFixData;
        MediatorSettings mediatorSettings;
        ByteWorker byteWorker = new ByteWorker();

        public TinyIPFixConvertor(TinyIPFixData s_tinyIPFixData, MediatorSettings s_mediatorSettings)
        {
            tinyIPFixData = s_tinyIPFixData;
            mediatorSettings = s_mediatorSettings;
        }

        public IPFix_Header BuildHeader(UInt16 additionalLength, UInt32 Seqnumber)
        {
            IPFix_Header header = new IPFix_Header();
            header.Version_Number = byteWorker.ToBigEndian16(10);
            header.Export_Time = byteWorker.ToBigEndian32((UInt32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
            header.Observation_Domain_ID = byteWorker.ToBigEndian32(mediatorSettings.TinyToIPFix_ObservationDomainID);
            header.Sequence_Number = byteWorker.ToBigEndian32(Seqnumber);
            header.Length = byteWorker.ToBigEndian16((UInt16)(16 + additionalLength));

            return header;
        }

        public IPFix_Template_Header BuildTemplateHeader(DataTemplate dataTemplate, out UInt16 templateLength)
        {
            templateLength = (UInt16)(LengthCounter.CountLenghtOfFieldsTemplate(dataTemplate) + 8);
            IPFix_Template_Header ipfix_Template_Header = new IPFix_Template_Header();
            ipfix_Template_Header.FieldCount = byteWorker.ToBigEndian16((UInt16)dataTemplate.FieldCount);
            ipfix_Template_Header.TemplateID = byteWorker.ToBigEndian16((UInt16)(dataTemplate.TemplateID + 128));
            ipfix_Template_Header.SetID = byteWorker.ToBigEndian16(2);
            ipfix_Template_Header.Length = byteWorker.ToBigEndian16(templateLength);

            return ipfix_Template_Header;
        }

        public List<object> BuildTemplateData(DataTemplate dataTemplate)
        {
            List<object> templateData = new List<object>();

            for(int i = 0; i < dataTemplate.template_list.Count; i++)
            {
                if(dataTemplate.template_list[i].EnterpriseNumber == 0)
                {
                    IPFix_Template_Data_NoEnterprise data = new IPFix_Template_Data_NoEnterprise();
                    data.FieldDataLength = byteWorker.ToBigEndian16(dataTemplate.template_list[i].FieldDataLength);
                    data.FieldName = byteWorker.ToBigEndian16(dataTemplate.template_list[i].FieldName);
                    templateData.Add(data);
                }
                else
                {
                    IPFix_Template_Data data = new IPFix_Template_Data();
                    data.FieldDataLength = byteWorker.ToBigEndian16(dataTemplate.template_list[i].FieldDataLength);
                    data.FieldName = byteWorker.ToBigEndian16((UInt16)(dataTemplate.template_list[i].FieldName + 32768));
                    data.EnterpriseNumber = byteWorker.ToBigEndian32(dataTemplate.template_list[i].EnterpriseNumber);
                    templateData.Add(data);
                }
            }

            return templateData;
        }

        public IPFix_Data_Header BuildDataHeader(DataTemplate dataTemplate, out UInt16 dataLength)
        {
            dataLength = (UInt16)(LengthCounter.CountLenghtOfFieldsData(dataTemplate) + 4);
            IPFix_Data_Header header = new IPFix_Data_Header();
            header.SetID = byteWorker.ToBigEndian16((UInt16)(tinyIPFixData.TemplateID + 128));
            header.Length = byteWorker.ToBigEndian16(dataLength);

            return header;
        }

        public List<byte[]> BuildDataList()
        {
            List<byte[]> datalist = new List<byte[]>();

            for (int i = 0; i < tinyIPFixData.Data.Count; i++)
            {
                datalist.Add(byteWorker.SwapBytes( tinyIPFixData.Data[i].Data));
            }

            return datalist;
        }
    }
}
