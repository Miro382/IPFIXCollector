using IPFIXCollector.Workers;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPFIXCollector.IPFix
{
    public class IPFixStructure
    {

        public struct IPFix_Header
        {
            public UInt16 Version_Number;
            public UInt16 Length;
            public UInt32 Export_Time;
            public UInt32 Sequence_Number;
            public UInt32 Observation_Domain_ID;
        };


        public struct IPFix_Template_Header
        {
            public UInt16 SetID;
            public UInt16 Length;
            public UInt16 TemplateID;
            public UInt16 FieldCount;
        };

        public struct IPFix_Template_Data
        {
            public UInt16 FieldName;
            public UInt16 FieldDataLength;
            public UInt32 EnterpriseNumber;
        };

        public struct IPFix_Template_Data_NoEnterprise
        {
            public UInt16 FieldName;
            public UInt16 FieldDataLength;
        };

        public struct IPFix_Data_Header
        {
            public UInt16 SetID;
            public UInt16 Length;
        };

        public struct IPFix_Data
        {
            public UInt32 Data;
        };


        public struct IPFix_Set_Detect
        {
            public UInt16 SetID;
        };


        public void Convert_IPFIXStructure(ref IPFix_Header data_Header)
        {
            ByteWorker byteWorker = new ByteWorker();
            data_Header.Version_Number = byteWorker.ToLittleEndian16(data_Header.Version_Number);
            data_Header.Length = byteWorker.ToLittleEndian16(data_Header.Length);
            data_Header.Export_Time = byteWorker.ToLittleEndian32(data_Header.Export_Time);
            data_Header.Sequence_Number = byteWorker.ToLittleEndian32(data_Header.Sequence_Number);
            data_Header.Observation_Domain_ID = byteWorker.ToLittleEndian32(data_Header.Observation_Domain_ID);
        }

        public void Convert_IPFIXStructure(ref IPFix_Template_Header data_Header)
        {
            ByteWorker byteWorker = new ByteWorker();
            data_Header.SetID = byteWorker.ToLittleEndian16(data_Header.SetID);
            data_Header.Length = byteWorker.ToLittleEndian16(data_Header.Length);
            data_Header.TemplateID = byteWorker.ToLittleEndian16(data_Header.TemplateID);
            data_Header.FieldCount = byteWorker.ToLittleEndian16(data_Header.FieldCount);
        }

        public void Convert_IPFIXStructure(ref IPFix_Template_Data data)
        {
            ByteWorker byteWorker = new ByteWorker();
            data.FieldName = byteWorker.ToLittleEndian16(data.FieldName);
            data.FieldDataLength = byteWorker.ToLittleEndian16(data.FieldDataLength);
            data.EnterpriseNumber = byteWorker.ToLittleEndian32(data.EnterpriseNumber);
        }

        public void Convert_IPFIXStructure(ref IPFix_Data_Header data)
        {
            ByteWorker byteWorker = new ByteWorker();
            data.SetID = byteWorker.ToLittleEndian16(data.SetID);
            data.Length = byteWorker.ToLittleEndian16(data.Length);
        }

        public void Convert_IPFIXStructure(ref IPFix_Data data)
        {
            ByteWorker byteWorker = new ByteWorker();
            data.Data = byteWorker.ToLittleEndian32(data.Data);
        }

        public void Convert_IPFIXStructure(ref IPFix_Set_Detect data)
        {
            ByteWorker byteWorker = new ByteWorker();
            data.SetID = byteWorker.ToLittleEndian16(data.SetID);
        }
    }
}
