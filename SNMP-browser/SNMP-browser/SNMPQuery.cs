using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNMP_browser
{
    class SNMPQuery
    {
        public string oid { get; set; }

        public SNMPQuery(string oid)
        {
            this.oid = oid;
        }
    }
}
