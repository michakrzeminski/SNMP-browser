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
        SnmpV2Packet result2;
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

            this.GetRequest("1.3.6.1.2.1.1.3.0");
            this.GetNextRequest("1.3.6.1.2.1.1.2.0");
            this.GetTable("1.3.6.1.2.1.2.2");
        }
        public void conn(String name)
        {
            if (result != null)
            {
                if (result.Pdu.ErrorStatus != 0)
                {
                    Console.WriteLine("Error in SNMP reply. Error {0} index {1}", result.Pdu.ErrorStatus, result.Pdu.ErrorIndex);
                }
                else
                {
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
        public SnmpV1Packet GetRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            this.pdu = new Pdu(PduType.Get);
            this.pdu.VbList.Add(OID);
            result = (SnmpV1Packet)target.Request(pdu, param);

            //TODO display in gridView
            Console.WriteLine(result.Pdu.VbList[0].Oid.ToString());
            Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type));
            Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
            return result;
        }
        public SnmpV1Packet GetNextRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            this.pdu = new Pdu(PduType.GetNext);
            this.pdu.VbList.Add(OID);
            result = (SnmpV1Packet)target.Request(pdu, param);

            //TODO display in gridView
            Console.WriteLine(result.Pdu.VbList[0].Oid.ToString());
            Console.WriteLine(SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type));
            Console.WriteLine(result.Pdu.VbList[0].Value.ToString());
            return result;
        }
        public void GetTable(string OID)
        {
            //TODO pobiera dane dla calej tabeli, trzeba to wyswietlic
            this.param.Version = SnmpVersion.Ver2;
            Dictionary<String, Dictionary<uint, AsnType>> result = new Dictionary<String, Dictionary<uint, AsnType>>();
            // Not every row has a value for every column so keep track of all columns available in the table
            List<uint> tableColumns = new List<uint>();
            Oid startOid = new Oid(OID);
            startOid.Add(1);
            Pdu bulkPdu = Pdu.GetBulkPdu();
            bulkPdu.VbList.Add(startOid);
            bulkPdu.NonRepeaters = 0;
            // Tune MaxRepetitions to the number best suited to retrive the data
            bulkPdu.MaxRepetitions = 100;
            // Current OID will keep track of the last retrieved OID and be used as 
            //  indication that we have reached end of table
            Oid curOid = (Oid)startOid.Clone();
            while (startOid.IsRootOf(curOid))
            {
                SnmpPacket res = null;
                try
                {
                    res = target.Request(bulkPdu, param);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Request failed: {0}", ex.Message);
                    target.Close();
                    return;
                }
                // For GetBulk request response has to be version 2
                if (res.Version != SnmpVersion.Ver2)
                {
                    Console.WriteLine("Received wrong SNMP version response packet.");
                    target.Close();
                    return;
                }
                // Check if there is an agent error returned in the reply
                if (res.Pdu.ErrorStatus != 0)
                {
                    Console.WriteLine("SNMP agent returned error {0} for request Vb index {1}",
                                      res.Pdu.ErrorStatus, res.Pdu.ErrorIndex);
                    target.Close();
                    return;
                }
                // Go through the VbList and check all replies
                foreach (Vb v in res.Pdu.VbList)
                {
                    curOid = (Oid)v.Oid.Clone();
                    // VbList could contain items that are past the end of the requested table.
                    // Make sure we are dealing with an OID that is part of the table
                    if (startOid.IsRootOf(v.Oid))
                    {
                        // Get child Id's from the OID (past the table.entry sequence)
                        uint[] childOids = Oid.GetChildIdentifiers(startOid, v.Oid);
                        // Get the value instance and converted it to a dotted decimal
                        //  string to use as key in result dictionary
                        uint[] instance = new uint[childOids.Length - 1];
                        Array.Copy(childOids, 1, instance, 0, childOids.Length - 1);
                        String strInst = InstanceToString(instance);
                        // Column id is the first value past <table oid>.entry in the response OID
                        uint column = childOids[0];
                        if (!tableColumns.Contains(column))
                            tableColumns.Add(column);
                        if (result.ContainsKey(strInst))
                        {
                            result[strInst][column] = (AsnType)v.Value.Clone();
                        }
                        else
                        {
                            result[strInst] = new Dictionary<uint, AsnType>();
                            result[strInst][column] = (AsnType)v.Value.Clone();
                        }
                    }
                    else
                    {
                        // We've reached the end of the table. No point continuing the loop
                        break;
                    }
                }
                // If last received OID is within the table, build next request
                if (startOid.IsRootOf(curOid))
                {
                    bulkPdu.VbList.Clear();
                    bulkPdu.VbList.Add(curOid);
                    bulkPdu.NonRepeaters = 0;
                    bulkPdu.MaxRepetitions = 100;
                }
            }
            //TODO display in gridView
            Console.WriteLine("debug");
        }
        public void GetSubTree(string OID)
        {
            //TODO get all data for TreeView
            // + dislpay
        }
        public static string InstanceToString(uint[] instance)
        {
            StringBuilder str = new StringBuilder();
            foreach (uint v in instance)
            {
                if (str.Length == 0)
                    str.Append(v);
                else
                    str.AppendFormat(".{0}", v);
            }
            return str.ToString();
        }
    }
}
