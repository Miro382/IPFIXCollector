using IPFIXCollector.IPFix;
using IPFIXCollector.Workers;
using System;
using System.Collections.Generic;
using System.Text;
using static IPFIXCollector.IPFix.IPFixStructure;
using static IPFIXCollector.TinyIPFix.TinyIPFixStructure;

namespace IPFIXCollector.TinyIPFix
{
    public class TinyIPFixWorker
    {
        TinyIPFixStructure tinyIPFixStructure = new TinyIPFixStructure();
        IPFixStructure ipfixstructure = new IPFixStructure();

        public TinyIPFix_Header GetTinyIPFIXHeader(ref byte[] data)
        {
            byte[] tempdata = new byte[10];
            Array.Copy(data, 0, tempdata, 0, 3);
            TinyIPFix_Header header = ByteWorker.ByteArrayToStructure<TinyIPFix_Header>(tempdata);
            tinyIPFixStructure.Convert_IPFIXStructure(ref header);

            return header;
        }

        public TinyIPFix_Set GetTinyIPFIXSet(ref byte[] data)
        {
            byte[] tempdata = new byte[10];
            Array.Copy(data, 3, tempdata, 0, 2);
            TinyIPFix_Set structuredata = ByteWorker.ByteArrayToStructure<TinyIPFix_Set>(tempdata);

            return structuredata;
        }


        public IPFix_Template_Data GetIPFIXTemplateDataFromTinyIPFix(ref byte[] data, int i, ref int entp)
        {
            byte[] tempdata = new byte[1024];
            Array.Copy(data, 5 + (i * 4) + (entp * 4), tempdata, 0, 4);
            IPFix_Template_Data ipdata = ByteWorker.ByteArrayToStructure<IPFix_Template_Data>(tempdata);
            ipdata.EnterpriseNumber = 0;
            ipfixstructure.Convert_IPFIXStructure(ref ipdata);

            if (ipdata.FieldName > 32768)
            {
                Array.Copy(data, 5 + (i * 4) + (entp * 4), tempdata, 0, 8);
                ipdata = ByteWorker.ByteArrayToStructure<IPFix_Template_Data>(tempdata);
                ipfixstructure.Convert_IPFIXStructure(ref ipdata);
                ipdata.FieldName -= 32768;
                entp++;
            }

            return ipdata;
        }


        public byte[] GetIPFIXDataFromTinyIPFix(ref byte[] data, UInt16 offset, UInt16 length)
        {
            byte[] tempdata = new byte[length];

            Array.Copy(data, 5 + offset, tempdata, 0, length);

            return tempdata;
        }
    }
}
