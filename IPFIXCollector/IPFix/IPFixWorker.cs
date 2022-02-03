using IPFIXCollector.Workers;
using System;
using static IPFIXCollector.IPFix.IPFixStructure;

namespace IPFIXCollector.IPFix
{
    public class IPFixWorker
    {
        IPFixStructure ipfixstructure = new IPFixStructure();

        public IPFix_Header GetIPFIXHeader(ref byte[] data)
        {
            byte[] tempdata = new byte[100];
            Array.Copy(data, 0, tempdata, 0, 16);
            IPFix_Header header = ByteWorker.ByteArrayToStructure<IPFix_Header>(tempdata);
            ipfixstructure.Convert_IPFIXStructure(ref header);

            return header;
        }

        public IPFix_Template_Header GetIPFIXTemplateHeader(ref byte[] data)
        {
            byte[] tempdata = new byte[1024];
            Array.Copy(data, 16, tempdata, 0, 8);
            IPFix_Template_Header templateheader = ByteWorker.ByteArrayToStructure<IPFix_Template_Header>(tempdata);
            ipfixstructure.Convert_IPFIXStructure(ref templateheader);

            return templateheader;
        }

        public IPFix_Template_Data GetIPFIXTemplateData(ref byte[] data, int i, ref int entp)
        {
            byte[] tempdata = new byte[1024];
            Array.Copy(data, 24 + (i * 4) + (entp * 4), tempdata, 0, 4);
            IPFix_Template_Data ipdata = ByteWorker.ByteArrayToStructure<IPFix_Template_Data>(tempdata);
            ipdata.EnterpriseNumber = 0;
            ipfixstructure.Convert_IPFIXStructure(ref ipdata);

            if (ipdata.FieldName > 32768)
            {
                Array.Copy(data, 24 + (i * 4) + (entp * 4), tempdata, 0, 8);
                ipdata = ByteWorker.ByteArrayToStructure<IPFix_Template_Data>(tempdata);
                ipfixstructure.Convert_IPFIXStructure(ref ipdata);
                ipdata.FieldName -= 32768;
                entp++;
            }

            return ipdata;
        }

        public byte[] GetIPFIXData(ref byte[] data, int count, int entp, UInt16 offset, UInt16 length, bool havetemplate)
        {
            byte[] tempdata = new byte[length];
            if (havetemplate)
            {
                Array.Copy(data, 28 + (count * 4) + (entp * 4) + offset, tempdata, 0, length);
            }
            else
            {
                Array.Copy(data, 20 + offset, tempdata, 0, length);
            }

            return tempdata;
        }


        public IPFix_Set_Detect GetIPFIXSetDetect(ref byte[] data, int pos)
        {
            byte[] tempdata = new byte[10];
            Array.Copy(data, pos, tempdata, 0, 2);
            IPFix_Set_Detect header = ByteWorker.ByteArrayToStructure<IPFix_Set_Detect>(tempdata);
            ipfixstructure.Convert_IPFIXStructure(ref header);

            return header;
        }
    }
}
