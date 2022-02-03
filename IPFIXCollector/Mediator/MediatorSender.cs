using IPFIXCollector.DataTemplates;
using IPFIXCollector.Workers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.Mediator
{
    public class MediatorSender
    {

        private MediatorSettings mediatorSettings;
        private ByteWorker byteWorker = new ByteWorker();

        private Dictionary<int, MediatorData> mediatorDataDict = new Dictionary<int, MediatorData>();

        public MediatorSender(MediatorSettings s_mediatorSettings)
        {
            mediatorSettings = s_mediatorSettings;
        }

        public void ResendIPFIX(ref byte[] data)
        {
            SendUDP(mediatorSettings.From_UDPPort, mediatorSettings.SendTo_IpAddress, mediatorSettings.SendTo_UDPPort, data);
        }

        public void SendTinyIPFIXToIPFIX(TinyIPFixData tinyIPFixData, DataTemplate dataTemplate)
        {
            TinyIPFixConvertor tinyIPFixConvertor = new TinyIPFixConvertor(tinyIPFixData, mediatorSettings);

            MediatorData mediatorData;

            if(mediatorDataDict.ContainsKey(dataTemplate.TemplateID))
            {
                mediatorDataDict.TryGetValue(dataTemplate.TemplateID, out mediatorData);
                if((long)(tinyIPFixData.Sequence_Number + (mediatorData.SeqOverByte * 256)) - mediatorData.SequenceNumber < 0)
                {
                    if (mediatorData.SequenceNumber >= 4294967295)
                    {
                        mediatorData.SeqOverByte = 0;
                        mediatorData.SequenceNumber = 0;
                    }
                    else
                    {
                        mediatorData.SeqOverByte++;
                    }
                }

                mediatorData.SequenceNumber += (UInt16)((tinyIPFixData.Sequence_Number + (mediatorData.SeqOverByte*256)) - mediatorData.SequenceNumber);

            }
            else
            {
                mediatorData = new MediatorData();
                mediatorData.SequenceNumber = tinyIPFixData.Sequence_Number;
                mediatorData.TemplateID = tinyIPFixData.TemplateID;
                mediatorData.SeqOverByte = 0;
                mediatorDataDict.Add(tinyIPFixData.TemplateID, mediatorData);
            }

            if (tinyIPFixData.MessageType == 1)
            {
                UInt16 templateLength;
                IPFix_Template_Header template_Header = tinyIPFixConvertor.BuildTemplateHeader(dataTemplate, out templateLength);

                IPFix_Header header = tinyIPFixConvertor.BuildHeader(templateLength, (UInt32)mediatorData.SequenceNumber);

                List<object> templatedata = tinyIPFixConvertor.BuildTemplateData(dataTemplate);

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(byteWorker.StructureToByteArray(header));
                    ms.Write(byteWorker.StructureToByteArray(template_Header));

                    foreach(object obj in templatedata)
                    {
                        ms.Write(byteWorker.StructureToByteArray(obj));
                    }

                    SendUDP(mediatorSettings.From_UDPPort, mediatorSettings.SendTo_IpAddress, mediatorSettings.SendTo_UDPPort, ms.ToArray());
                }

            }
            else
            {
                UInt16 dataLength;
                IPFix_Data_Header data_Header = tinyIPFixConvertor.BuildDataHeader(dataTemplate, out dataLength);

                IPFix_Header header = tinyIPFixConvertor.BuildHeader(dataLength, (UInt32)mediatorData.SequenceNumber);

                List<byte[]> datalist = tinyIPFixConvertor.BuildDataList();

                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(byteWorker.StructureToByteArray(header));
                    ms.Write(byteWorker.StructureToByteArray(data_Header));

                    foreach (byte[] obj in datalist)
                    {
                        ms.Write(obj);
                    }

                    SendUDP(mediatorSettings.From_UDPPort, mediatorSettings.SendTo_IpAddress, mediatorSettings.SendTo_UDPPort, ms.ToArray());
                }
            }
        }

        private void SendUDP(int srcPort, string dstIp, int dstPort, byte[] data)
        {
            using (UdpClient c = new UdpClient(srcPort))
            {
                c.Send(data, data.Length, dstIp, dstPort);
            }
        }

    }
}
