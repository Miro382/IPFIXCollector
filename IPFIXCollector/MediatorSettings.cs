using System;
using System.Collections.Generic;
using System.Text;

namespace IPFIXCollector
{
    public class MediatorSettings
    {
        public bool UseMediator;
        public string SendTo_IpAddress;
        public int SendTo_UDPPort;
        public UInt32 TinyToIPFix_ObservationDomainID;
        public int From_UDPPort;
    }
}
