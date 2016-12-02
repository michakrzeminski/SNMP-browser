using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SnmpSharpNet;

namespace SNMP_browser
{
    class SnmpClient
    {
        public SnmpClient()
        {
            this.conn();
        }
        public void conn()
        {
            // SNMP community name
            OctetString community = new OctetString("community");

            // Define agent parameters class
            AgentParameters param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;

            IpAddress agent = new IpAddress("127.0.0.1");
            UdpTarget target = new UdpTarget((IPAddress)agent, 161, 2000, 2);

            Pdu pdu = new Pdu(PduType.Get);
            // Add variables you wish to query
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            // Make SNMP request
            SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);
            if (result != null)
            {
                // ErrorStatus other then 0 is an error returned by 
                // the Agent - see SnmpConstants for error definitions
                if (result.Pdu.ErrorStatus != 0)
                {
                    // agent reported an error with the request
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}", result.Pdu.ErrorStatus, result.Pdu.ErrorIndex);
                }
                else
                {
                    // Reply variables are returned in the same order as they were added
                    //  to the VbList
                    Console.WriteLine("sysupTime:");
                    Console.WriteLine(result.Pdu.VbList[0].Oid.ToString());
                    Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type));
                    Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
                }
            }
            else
            {
                Console.WriteLine("No response received from SNMP agent.");
            }
            target.Close();
        }
    }
}
