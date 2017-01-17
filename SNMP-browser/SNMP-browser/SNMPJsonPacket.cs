using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNMP_browser
{
    class SNMPJsonPacket
    {
        public string oid { get; set; }

        public string value { get; set; }

        public string type { get; set; }
        
        public SNMPJsonPacket(string oid, string value, string type)
        {
            this.oid = oid;
            this.value = value;
            this.type = type;
        }
    }
}
