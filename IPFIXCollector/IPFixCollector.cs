using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using static IPFIXCollector.IPFix.IPFixStructure;
using IPFIXCollector.IPFix;
using IPFIXCollector.DataTemplates;
using IPFIXCollector.Workers;
using static IPFIXCollector.TinyIPFix.TinyIPFixStructure;
using IPFIXCollector.TinyIPFix;
using IPFIXCollector.Mediator;

namespace IPFIXCollector
{
    public class IPFixCollector
    {
        private CollectorSettings collectorSettings = new CollectorSettings();
        private Dictionary<string, MappingItem> mappingdict = new Dictionary<string, MappingItem>();


        private Dictionary<UInt16, DataTemplate> templateList = new Dictionary<UInt16, DataTemplate>();

        private IPFixWorker ipfixWorker = new IPFixWorker();
        private TinyIPFixWorker tinyIPFixWorker = new TinyIPFixWorker();


        private MessageLogger logger;
        private DataWorker dataWorker;

        private MediatorSender mediatorSender;

        private TinyIPFixData tinyIPFixData = new TinyIPFixData();

        private DataTemplateSerializer dataTemplateSerializer = new DataTemplateSerializer();

        private ByteWorker byteWorker = new ByteWorker();

        public IPFixCollector()
        {
            string readcontent = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "/Settings.json");
            collectorSettings = JsonConvert.DeserializeObject<CollectorSettings>(readcontent);

            foreach(MappingItem mappingItem in collectorSettings.MappingList)
            {
                mappingdict.Add(mappingItem.EnterpriseNumber + "." + mappingItem.FieldID, mappingItem);
            }

            logger = new MessageLogger(collectorSettings);
            dataWorker = new DataWorker(collectorSettings);
            mediatorSender = new MediatorSender(collectorSettings.Mediator);

            try
            {
                if (collectorSettings.LoadTemplates)
                {
                    List<DataTemplate> dataTemplates = dataTemplateSerializer.LoadTemplates();

                    foreach (DataTemplate dataTemplate in dataTemplates)
                    {
                        if (dataTemplate != null)
                        {
                            templateList.Add((UInt16)dataTemplate.TemplateID, dataTemplate);
                        }
                    }
                }
            }catch(Exception ex)
            {
                logger.PrintError("Load Templates from files failed: "+ex.ToString());
            }
        }


        public void Listen()
        {
            byte[] data = new byte[1024];
            Console.WriteLine("Starting IPFIX Collector... Debug: "+collectorSettings.Debug);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, collectorSettings.UDPPort);
            UdpClient newsock = new UdpClient(ipep);

            Console.WriteLine("Listening on port: " + collectorSettings.UDPPort);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);


            while (true)
            {
                try
                {
                    Array.Clear(data, 0, data.Length);

                    data = newsock.Receive(ref sender);

                    logger.PrintDebug("Message received from: " + sender.ToString());


                    IPFix_Set_Detect setd = ipfixWorker.GetIPFIXSetDetect(ref data, 0);

                    if (setd.SetID == 10)
                    {
                        logger.PrintDebug("Process IPFIX Message...");
                        ProcessIPFIXMessage(ref data);

                        if (collectorSettings.Mediator.UseMediator)
                        {
                            mediatorSender.ResendIPFIX(ref data);
                        }
                    }
                    else
                    {
                        logger.PrintDebug("Process TinyIPFIX Message...");
                        tinyIPFixData.ClearData();
                        ProcessTinyIPFIXMessage(ref data);

                        if (collectorSettings.Mediator.UseMediator)
                        {

                            if (templateList.ContainsKey(tinyIPFixData.TemplateID))
                            {
                                DataTemplate dataTemplate;
                                templateList.TryGetValue(tinyIPFixData.TemplateID, out dataTemplate);

                                mediatorSender.SendTinyIPFIXToIPFIX(tinyIPFixData, dataTemplate);
                            }
                        }
                    }

                }
                catch(Exception ex)
                {
                    logger.PrintError(ex.ToString());
                }
            }
        }


        void ProcessIPFIXMessage(ref byte[] data)
        {

            //get ipfix header
            IPFix_Header header = ipfixWorker.GetIPFIXHeader(ref data);

            logger.PrintDebug("Version_number: " + header.Version_Number + "   Length: " + header.Length + "  ExportTime: " + header.Export_Time + "  Sequence Number: " + header.Sequence_Number + "  Observation Domain ID: " + header.Observation_Domain_ID);

            IPFix_Set_Detect setd = ipfixWorker.GetIPFIXSetDetect(ref data, 16);


            if(setd.SetID < 256)
            {
                UInt16 templateid = 0;
                int len = GetTemplateData(ref data, out templateid);

                logger.PrintDebug("Len: " + len + "  length: " + header.Length);

                if((len + 16) < header.Length)
                {
                    GetData(ref data, templateid, header, true);
                }
            }
            else
            {
                GetData(ref data, setd.SetID, header, false);
            }


            logger.PrintDebug("---------------");


            /*
            Array.Copy(data, 24 + (template_list.Count * 4) + (entp * 4), tempdata, 0, 4);
            IPFix_Data_Header ipdataheader = ByteArrayToStructure<IPFix_Data_Header>(tempdata);
            ipfixstructure.Convert_IPFIXStructure(ref ipdataheader);
            PrintDebug("SetID: " + ipdataheader.SetID + "  Length: " + ipdataheader.Length);
            */

        }


        private int GetTemplateData(ref byte[] data, out UInt16 otemplateID)
        {
            //get ipfix template header
            IPFix_Template_Header templateheader = ipfixWorker.GetIPFIXTemplateHeader(ref data);

            otemplateID = templateheader.TemplateID;

            DataTemplate dataTemplate;
            if (templateList.ContainsKey(templateheader.TemplateID))
            {
                templateList.TryGetValue(templateheader.TemplateID, out dataTemplate);
                dataTemplate.FieldCount = templateheader.FieldCount;
            }
            else
            {
                dataTemplate = new DataTemplate();
                dataTemplate.FieldCount = templateheader.FieldCount;
                dataTemplate.TemplateID = templateheader.TemplateID;
                templateList.Add(templateheader.TemplateID, dataTemplate);
            }

            logger.PrintDebug("SetID: " + templateheader.SetID + "  Length: " + templateheader.Length + "  TemplateID: " + templateheader.TemplateID + "  FieldCount: " + templateheader.FieldCount);


            int entp = 0;
            dataTemplate.template_list.Clear();

            for (int i = 0; i < templateheader.FieldCount; i++)
            {

                IPFix_Template_Data ipdata = ipfixWorker.GetIPFIXTemplateData(ref data, i, ref entp);
                dataTemplate.template_list.Add(ipdata);

                logger.PrintDebug("Field Name: " + ipdata.FieldName + "  Field Length: " + ipdata.FieldDataLength + "  Enterprise number: " + ipdata.EnterpriseNumber);
            }

            dataTemplate.EnterpriseFieldCount = entp;

            if(collectorSettings.SaveTemplates)
            {
                dataTemplateSerializer.SaveTemplate(dataTemplate);
            }


            return templateheader.Length;
        }



        private void GetData(ref byte[] data, UInt16 templateID, IPFix_Header header, bool containsTemplate)
        {
            DataTemplate dataTemplate;

            templateList.TryGetValue(templateID, out dataTemplate);

            if(dataTemplate == null)
            {
                throw new Exception("Error - Template '"+ templateID + "' not exists...");
            }

            JObject jsondata = new JObject();

            UInt16 offset = 0;

            for (int i = 0; i < dataTemplate.template_list.Count; i++)
            {
                byte[] ipdata = ipfixWorker.GetIPFIXData(ref data, dataTemplate.template_list.Count, dataTemplate.EnterpriseFieldCount, offset, dataTemplate.template_list[i].FieldDataLength,  containsTemplate);

                offset += dataTemplate.template_list[i].FieldDataLength;

                JProperty jdata;

                string rname;
                object rvalue;

                GetNameOfField(dataTemplate.template_list[i].EnterpriseNumber, dataTemplate.template_list[i].FieldName, ipdata, out rname, out rvalue);

                jdata = new JProperty(rname, rvalue);
                jsondata.Add(jdata);
            }


            if (collectorSettings.SaveHeaderData)
            {
                JObject headerdata = new JObject();
                headerdata.Add("ExportTime", new JValue(header.Export_Time));
                headerdata.Add("SequenceNumber", new JValue(header.Sequence_Number));
                headerdata.Add("ObservationDomainID", new JValue(header.Observation_Domain_ID));
                jsondata.Add(new JProperty("Header", headerdata));
            }

            if (collectorSettings.SaveTemplateData)
            {
                JObject templatedata = new JObject();
                templatedata.Add("TemplateID", new JValue(dataTemplate.TemplateID));
                jsondata.Add(new JProperty("Template", templatedata));
            }

            if(collectorSettings.AddDateTimeToOutput)
            {
                JProperty jdata = new JProperty("DateTime", string.Format("{0:yyyy-MM-ddTHH:mm:ss.FFFZ}", DateTime.UtcNow));
                jsondata.Add(jdata);
            }

            string json_out = jsondata.ToString(dataWorker.GetFormatting(), null);
            dataWorker.SaveData(json_out, templateID);
        }




        void ProcessTinyIPFIXMessage(ref byte[] data)
        {
            TinyIPFix_Header tinyIPFix_Header = tinyIPFixWorker.GetTinyIPFIXHeader(ref data);

            int messagetype = 0;


            if (tinyIPFix_Header.Flags_Length >= 3072)
            {
                messagetype = 2;
            }
            else if (tinyIPFix_Header.Flags_Length >= 1024)
            {
                messagetype = 1;
            }


            logger.PrintDebug("Flags and Length: " + tinyIPFix_Header.Flags_Length + "   Sequence Number: " + tinyIPFix_Header.Sequence_Number + "  Message Type: " + messagetype);


            TinyIPFix_Set tinyIPFix_Set = tinyIPFixWorker.GetTinyIPFIXSet(ref data);


            if (messagetype == 2)
            {
                logger.PrintDebug("TemplateID: " + tinyIPFix_Set.SetID + "   Length: " + tinyIPFix_Set.Length);
            }
            else
            {
                logger.PrintDebug("TemplateID: " + tinyIPFix_Set.SetID + "   FieldCount: " + tinyIPFix_Set.Length);
            }

            if(messagetype == 1)
            {
                GetTinyTemplateData(ref data, tinyIPFix_Set.SetID, tinyIPFix_Set.Length);
            }
            else
            {
                GetDataFromTiny(ref data, tinyIPFix_Set.SetID, tinyIPFix_Header);
            }

            tinyIPFixData.TemplateID = tinyIPFix_Set.SetID;
            tinyIPFixData.Sequence_Number = tinyIPFix_Header.Sequence_Number;
            tinyIPFixData.MessageType = messagetype;

            logger.PrintDebug("---------------");

        }


        private void GetTinyTemplateData(ref byte[] data, byte templateID, byte fieldcount)
        {

            DataTemplate dataTemplate;
            if (templateList.ContainsKey(templateID))
            {
                templateList.TryGetValue(templateID, out dataTemplate);
                dataTemplate.FieldCount = fieldcount;
            }
            else
            {
                dataTemplate = new DataTemplate();
                dataTemplate.FieldCount = fieldcount;
                dataTemplate.TemplateID = templateID;
                templateList.Add(templateID, dataTemplate);
            }


            int entp = 0;
            dataTemplate.template_list.Clear();

            for (int i = 0; i < fieldcount; i++)
            {

                IPFix_Template_Data ipdata = tinyIPFixWorker.GetIPFIXTemplateDataFromTinyIPFix(ref data, i, ref entp);
                dataTemplate.template_list.Add(ipdata);

                logger.PrintDebug("Field Name: " + ipdata.FieldName + "  Field Length: " + ipdata.FieldDataLength + "  Enterprise number: " + ipdata.EnterpriseNumber);
            }

            dataTemplate.EnterpriseFieldCount = entp;

            if (collectorSettings.SaveTemplates)
            {
                dataTemplateSerializer.SaveTemplate(dataTemplate);
            }
        }



        private void GetDataFromTiny(ref byte[] data, byte templateID, TinyIPFix_Header header)
        {
            DataTemplate dataTemplate;

            templateList.TryGetValue(templateID, out dataTemplate);

            if (dataTemplate == null)
            {
                throw new Exception("Error - Template '" + templateID + "' not exists...");
            }

            JObject jsondata = new JObject();

            UInt16 offset = 0;

            for (int i = 0; i < dataTemplate.template_list.Count; i++)
            {
                byte[] ipdata = tinyIPFixWorker.GetIPFIXDataFromTinyIPFix(ref data, offset, dataTemplate.template_list[i].FieldDataLength);
                offset += dataTemplate.template_list[i].FieldDataLength;


                tinyIPFixData.Data.Add(new DataContent(ipdata));
                JProperty jdata;

                string rname;
                object rvalue;

                GetNameOfField(dataTemplate.template_list[i].EnterpriseNumber, dataTemplate.template_list[i].FieldName, ipdata, out rname, out rvalue);

                jdata = new JProperty(rname, rvalue);
                jsondata.Add(jdata);
            }


            if (collectorSettings.SaveHeaderData)
            {
                JObject headerdata = new JObject();
                headerdata.Add("SequenceNumber", new JValue(header.Sequence_Number));
                jsondata.Add(new JProperty("Header", headerdata));
            }

            if (collectorSettings.SaveTemplateData)
            {
                JObject templatedata = new JObject();
                templatedata.Add("TemplateID", new JValue(dataTemplate.TemplateID));
                jsondata.Add(new JProperty("Template", templatedata));
            }

            if (collectorSettings.AddDateTimeToOutput)
            {
                JProperty jdata = new JProperty("DateTime", string.Format("{0:yyyy-MM-ddTHH:mm:ss.FFFZ}", DateTime.UtcNow));
                jsondata.Add(jdata);
            }

            string json_out = jsondata.ToString(dataWorker.GetFormatting(), null);
            dataWorker.SaveData(json_out, templateID);
        }


        public void GetNameOfField(uint enterprisenumber, int fieldid, byte[] input_value, out string name, out object output_value)
        {
            MappingItem mappingItem;
            if (mappingdict.ContainsKey(enterprisenumber + "." + fieldid))
            {
                mappingdict.TryGetValue(enterprisenumber + "." + fieldid, out mappingItem);

                output_value = dataWorker.GetDataFromBytes(input_value, mappingItem.OutputMode, logger, collectorSettings.SwapStringBytes);
                name = mappingItem.Name;
            }
            else
            {
                if (enterprisenumber > 0)
                {
                    name = enterprisenumber + "." + fieldid;
                    output_value = dataWorker.GetDataFromBytes(input_value, 0, logger, collectorSettings.SwapStringBytes);
                }
                else
                {
                    name = "" + fieldid;
                    output_value = dataWorker.GetDataFromBytes(input_value, 0, logger, collectorSettings.SwapStringBytes);
                }
            }
        }


    }
}
