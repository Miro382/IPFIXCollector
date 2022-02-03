using IPFIXCollector.Workers;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPFIXCollector.TinyIPFix
{
    public class TinyIPFixStructure
    {
        public struct TinyIPFix_Header
        {
            public UInt16 Flags_Length;
            public Byte Sequence_Number;
        };


        public struct TinyIPFix_Set
        {
            public Byte SetID;
            public Byte Length;
        };


        public void Convert_IPFIXStructure(ref TinyIPFix_Header data)
        {
            ByteWorker byteWorker = new ByteWorker();
            data.Flags_Length = byteWorker.ToLittleEndian16(data.Flags_Length);
        }

    }
}
