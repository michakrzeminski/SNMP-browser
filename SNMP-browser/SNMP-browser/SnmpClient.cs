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
        // SNMP community name
        OctetString community = new OctetString("community");
        AgentParameters param;
        Pdu pdu;
        SnmpV1Packet result;
        UdpTarget target;

        public SnmpClient()
        {
           // this.conn();
            // Define agent parameters class
            param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;

            IpAddress agent = new IpAddress("127.0.0.1");
            target = new UdpTarget((IPAddress)agent, 161, 2000, 2);

            pdu = new Pdu(PduType.Get);
            // Add variables you wish to query
            pdu.VbList.Add("1.3.6.1.2.1.1.1.0");
            pdu.VbList.Add("1.3.6.1.2.1.1.2.0");
            pdu.VbList.Add("1.3.6.1.2.1.1.3.0"); //sysUpTime
            pdu.VbList.Add("1.3.6.1.2.1.1.4.0");
            pdu.VbList.Add("1.3.6.1.2.1.1.5.0");
            pdu.VbList.Add("1.3.6.1.2.1.1.6.0");
            pdu.VbList.Add("1.3.6.1.2.1.1.7.0");
            // Make SNMP request
            result = (SnmpV1Packet)target.Request(pdu, param);
        }

        public void conn(String name)
        {
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
                    switch(name)
                    {
                        case "sysDescr":
                            {
                                Console.WriteLine("sysDescr:");
                                Console.WriteLine(result.Pdu.VbList[0].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
                                break;
                            }
                        case "sysObjectID":
                            {

                                Console.WriteLine("sysObjectID:");
                                Console.WriteLine(result.Pdu.VbList[1].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[1].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[1].Value.ToString());
                                break;
                            }
                        case "sysUpTime":
                            {
                                Console.WriteLine("sysupTime:");
                                Console.WriteLine(result.Pdu.VbList[2].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[2].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[2].Value.ToString());
                                break;
                            }
                        case "sysContact":
                            {
                                Console.WriteLine("sysContact:");
                                Console.WriteLine(result.Pdu.VbList[3].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[3].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[3].Value.ToString());
                                break;
                            }

                        case "sysName":
                            {
                                Console.WriteLine("sysName:");
                                Console.WriteLine(result.Pdu.VbList[4].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[4].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[4].Value.ToString());
                                break;
                            }
                        case "sysLocation":
                            {
                                Console.WriteLine("sysLocation:");
                                Console.WriteLine(result.Pdu.VbList[5].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[5].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[5].Value.ToString());
                                break;
                            }
                        case "sysServices":
                            {
                                Console.WriteLine("sysServices:");
                                Console.WriteLine(result.Pdu.VbList[6].Oid.ToString());
                                Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[6].Value.Type));
                                Console.WriteLine(result.Pdu.VbList[6].Value.ToString());
                                break;
                            }
                    default:
                        Console.WriteLine("inny");
                            break;
                    }
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
