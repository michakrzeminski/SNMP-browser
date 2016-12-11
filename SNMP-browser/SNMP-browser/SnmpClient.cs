using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SnmpSharpNet;
using System.Threading;



namespace SNMP_browser
{
    public class SnmpClient
    {
        // SNMP community name
        OctetString community = new OctetString("public");
        AgentParameters param;

        Pdu pdu;
        SnmpV1Packet result;
        SnmpV2Packet result2;
        UdpTarget target;
        public List<Dane> lista = new List<Dane>();
        public List<uint> tableColumns = new List<uint>();
        public Dictionary<String, Dictionary<uint, AsnType>> results = new Dictionary<String, Dictionary<uint, AsnType>>();
        public string address = "127.0.0.1";
        public string OidNumber;
        public string value;
        public string type;
        public string ipPort;
        private Dictionary<string, string> translation;
        private MainWindow windowHandler;
        public Dictionary<int, List<VarBind>> varBindListPerTrap;
        public static int trapCounter = 0;
        public bool monitor = false;

        public SnmpClient(MainWindow windowHandler)
        {
            this.windowHandler = windowHandler;
            OidNumber = "";
            value = "";
            type = "";
            ipPort = "";
            // Define agent parameters class
            param = new AgentParameters(community);
            // Set SNMP version to 1 (or 2)
            param.Version = SnmpVersion.Ver1;

            IpAddress agent = new IpAddress(address);
            target = new UdpTarget((IPAddress)agent, 161, 2000, 2);

            translation = new Dictionary<string, string>();
            this.readTranslationFile();

            varBindListPerTrap = new Dictionary<int, List<VarBind>>();
            Thread trap_thread = new Thread(trapReceiver);
            trap_thread.Start();
        }

        public void resetTrapCounter()
        {
            trapCounter = 0;
        }

        public void Add(string _oidNumber, string _value, string _type, string _ipPort)
        {
            OidNumber = _oidNumber;
            value = _value;
            type = _type;
            ipPort = _ipPort;
        }


        public string getOidNumber()
        {
            return OidNumber;
        }

        public string getValue()
        {
            return value;
        }

        public string getType()
        {
            return type;
        }

        public string getIpPort()
        {
            return ipPort;
        }
        
        public void readTranslationFile()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            //remove \bin\Debug from path
            path = path.Remove(path.IndexOf("bin"), 10);
            path += "translation.txt";
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] temp = line.Split(null);
                        translation.Add(temp[0], temp[1]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        public SnmpV1Packet GetRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            this.pdu = new Pdu(PduType.Get);
            this.pdu.VbList.Add(OID);
            result = (SnmpV1Packet)target.Request(pdu, param);

            //TODO display in gridView

           OidNumber = result.Pdu.VbList[0].Oid.ToString();
           type = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);
           value = result.Pdu.VbList[0].Value.ToString();
           ipPort = address + ":161";

           // Console.WriteLine(OID);
            return result;
        }
        public SnmpV1Packet GetNextRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            this.pdu = new Pdu(PduType.GetNext);
            this.pdu.VbList.Add(OID);
            result = (SnmpV1Packet)target.Request(pdu, param);

            //TODO display in gridView
            OidNumber = result.Pdu.VbList[0].Oid.ToString();
            type = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);
            value = result.Pdu.VbList[0].Value.ToString();
            ipPort = address + ":161";
            return result;
        }
        public void GetTable(string OID)
        {
            //TODO pobiera dane dla calej tabeli, trzeba to wyswietlic
            this.param.Version = SnmpVersion.Ver2;
            
            // Not every row has a value for every column so keep track of all columns available in the table
            
            Oid startOid = new Oid(OID);
            startOid.Add(1);
            Console.WriteLine(startOid);
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
                        if (results.ContainsKey(strInst))
                        {
                            results[strInst][column] = (AsnType)v.Value.Clone();
                        }
                        else
                        {
                            results[strInst] = new Dictionary<uint, AsnType>();
                            
                            results[strInst][column] = (AsnType)v.Value.Clone();
                            //Console.WriteLine(result[strInst][column]);
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
        public void GetTree()
        {
            int counter = 0;
            param.Version = SnmpVersion.Ver1;
            Oid rootOid = new Oid("1.3.6.1.2.1");
            Oid lastOid = (Oid)rootOid.Clone();
            Pdu pdu = new Pdu(PduType.GetNext);
            while (lastOid != null)
            {
                if (pdu.RequestId != 0)
                {
                    pdu.RequestId += 1;
                }
                pdu.VbList.Clear();
                pdu.VbList.Add(lastOid);
                SnmpV1Packet result = (SnmpV1Packet)target.Request(pdu, param);

                if (result != null)
                {
                    if (result.Pdu.ErrorStatus != 0)
                    {
                        Console.WriteLine("Error in SNMP reply. Error {0} index {1}",
                            result.Pdu.ErrorStatus, result.Pdu.ErrorIndex);
                        lastOid = null;
                        break;
                    }
                    else
                    {
                        // Walk through returned variable bindings
                        foreach (Vb v in result.Pdu.VbList)
                        {
                            // Check that retrieved Oid is "child" of the root OID
                            if (rootOid.IsRootOf(v.Oid))
                            {
                                OidNumber = v.Oid.ToString();
                                string temp = OidNumber.Substring(OidNumber.Length - 2);
                                if(temp == ".0")
                                {
                                    counter = 0;
                                    string name = translate(OidNumber.Remove(OidNumber.Length - 2), null);
                                    if (name != null)
                                    {
                                        lista.Add(new Dane(v.Oid.ToString(), name));
                                    }
                                }
                                else
                                {
                                    if (translate(lastOid.ToString().Remove(lastOid.ToString().Length - 2), null) != null && counter == 0)
                                    {
                                        int length = lastOid.ToString().Length - 2;
                                        string oid = OidNumber.Substring(0, length);
                                        string name = translate(oid, null);
                                        if (name != null)
                                        {
                                            lista.Add(new Dane(oid, name));
                                            counter++;
                                        }
                                    }
                                }

                                //lista.Add(new Dane{ OidNumbers = v.Oid.ToString() });
                                lastOid = v.Oid;
                                
                                
                                
                            }
                            else
                            {
                                // we have reached the end of the requested
                                // MIB tree. Set lastOid to null and exit loop
                                lastOid = null;
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No response received from SNMP agent.");
                }
            }
            foreach (var i in lista)
            {
                if (OidNumber.Contains("1.3.6.1.2.1.55"))
                    break;
            }
            Console.WriteLine("debug");
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
        public string translate(string OID, string name)
        {
            if(OID != null)
            {
                KeyValuePair<string,string> temp = translation.FirstOrDefault(t => t.Value == OID);
                if(temp.Key != null)
                    return temp.Key;
            }
            if(name != null)
            {
                KeyValuePair<string, string> temp = translation.FirstOrDefault(t => t.Key == name);
                if (temp.Value != null)
                    return temp.Value;
            }
            return null;
        }

        public  void trapReceiver()
        {
            // Construct a socket and bind it to the trap manager port 162 
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
            EndPoint ep = (EndPoint)ipep;
            socket.Bind(ep);
            // Disable timeout processing. Just block until packet is received 
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
            bool run = true;
            int inlen = -1;
            while (run)
            {
                byte[] indata = new byte[16 * 1024];
                // 16KB receive buffer int inlen = 0;
                IpAddress addressIP = new IpAddress(address);
                IPEndPoint peer = new IPEndPoint((IPAddress)addressIP, 0);
                EndPoint inep = (EndPoint)peer;
                try
                {
                    inlen = socket.ReceiveFrom(indata, ref inep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception {0}", ex.Message);
                    inlen = -1;
                }
                if (inlen > 0)
                {
                    // Check protocol version int 
                    int ver = SnmpPacket.GetProtocolVersion(indata, inlen);
                    if (ver == (int)SnmpVersion.Ver1)
                    {
                        // Parse SNMP Version 1 TRAP packet 
                        SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        List<VarBind> varBindContentList = new List<VarBind>();
                        pkt.decode(indata, inlen);
                        //Console.WriteLine("** SNMP Version 1 TRAP received from {0}:", inep.ToString());
                        //Console.WriteLine("*** Trap generic: {0}", pkt.Pdu.Generic);
                        //Console.WriteLine("*** Trap specific: {0}", pkt.Pdu.Specific);
                        //Console.WriteLine("*** Agent address: {0}", pkt.Pdu.AgentAddress.ToString());
                        //Console.WriteLine("*** Timestamp: {0}", pkt.Pdu.TimeStamp.ToString());
                        //Console.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
                        //Console.WriteLine("*** VarBind content:");
                        foreach (Vb v in pkt.Pdu.VbList)
                        {
                           // Console.WriteLine("**** {0} {1}: {2}", v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
                            varBindContentList.Add(new VarBind(v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));

                        }
                       // Console.WriteLine("** End of SNMP Version 1 TRAP data.");
                        string ruleName = "NULL";
                        varBindListPerTrap.Add(trapCounter, varBindContentList);
                        windowHandler.addTrap(getGenericType(pkt.Pdu.Generic), pkt.Pdu.AgentAddress.ToString(),date, ruleName);
                        trapCounter++;
                    }
                    else
                    {
                        // Parse SNMP Version 2 TRAP packet 
                        SnmpV2Packet pkt = new SnmpV2Packet();
                        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        List<VarBind> varBindContentList = new List<VarBind>();
                        pkt.decode(indata, inlen);
                       // Console.WriteLine("** SNMP Version 2 TRAP received from {0}:", inep.ToString());
                        if ((SnmpSharpNet.PduType)pkt.Pdu.Type != PduType.V2Trap)
                        {
                           // Console.WriteLine("*** NOT an SNMPv2 trap ****");
                        }
                        else
                        {
                            //Console.WriteLine("*** Community: {0}", pkt.Community.ToString());
                            //Console.WriteLine("*** VarBind count: {0}", pkt.Pdu.VbList.Count);
                            //Console.WriteLine("*** VarBind content:");
                            foreach (Vb v in pkt.Pdu.VbList)
                            {
                                //Console.WriteLine("**** {0} {1}: {2}",v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString());
                                varBindContentList.Add(new VarBind(v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }
                            string ruleName = "NULL";
                            varBindListPerTrap.Add(trapCounter, varBindContentList);
                            windowHandler.addTrap(pkt.Pdu.TrapObjectID.ToString(), "?", date, ruleName);
                            trapCounter++;
                            //Console.WriteLine("** End of SNMP Version 2 TRAP data.");
                        }
                    }
                }
                else
                {
                    if (inlen == 0)
                        Console.WriteLine("Zero length packet received.");
                }
            }
        
    }
        private string getGenericType(int generic)
        {
            if (generic == 0)
            {
                return "coldStart";
            }else if (generic == 1)
            {
                return "warmStart";
            }else if(generic==2){
                return "linkDown";
            } else if (generic == 3)
            {
                return "linkUp";
            } else if (generic == 5)
            {
                return "egpNeighborLoss";
            } else if (generic == 4)
            {
                return "authenticationFailure";
            }else
            {
                return "Other";
            }
        }
        
        public void monitorObject(object OID)
        {
            while (monitor)
            {
                SnmpV1Packet packet = this.GetRequest((string)OID);
                this.windowHandler.addMonitorRow(packet.Pdu.VbList[0].Oid.ToString(), packet.Pdu.VbList[0].Value.ToString(), packet.Pdu.VbList[0].Type.ToString(), this.ipPort);
                Thread.Sleep(5000);
            }
        }


        public SnmpV1Packet GetMonitorRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            Pdu pduM = new Pdu(PduType.Get);
            pduM.VbList.Add(OID);
            SnmpV1Packet resultM = (SnmpV1Packet)target.Request(pduM, param);
            return resultM;
        }
    }
    
}

public class VarBind
{
    public string OID;
    public string type;
    public string value;

    public VarBind(string OID, string type, string value)
    {
        this.OID = OID;
        this.type = type;
        this.value = value;
    }
}

public class Dane
{
    public string Oid;
    public string name;
    public Dane(string Oid, string name)
    {
        this.Oid = Oid;
        this.name = name;
    }
}
