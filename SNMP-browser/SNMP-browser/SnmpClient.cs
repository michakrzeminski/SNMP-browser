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

        private TcpListener listener;
        private int proxyPort;
       // private static BinaryWriter writer;
        private AsynchronousSocketListener proxy;

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public SnmpClient(MainWindow windowHandler)
        {
            this.windowHandler = windowHandler;
            OidNumber = "";
            value = "";
            type = "";
            ipPort = "";
            param = new AgentParameters(community);
            param.Version = SnmpVersion.Ver1;

            IpAddress agent = new IpAddress(address);
            target = new UdpTarget((IPAddress)agent, 161, 2000, 2);

            translation = new Dictionary<string, string>();
            this.readTranslationFile();

            varBindListPerTrap = new Dictionary<int, List<VarBind>>();
            Thread trap_thread = new Thread(trapReceiver);
            trap_thread.Start();
        }

        public SnmpClient()
        {
            OidNumber = "";
            value = "";
            type = "";
            ipPort = "";
            param = new AgentParameters(community);
            param.Version = SnmpVersion.Ver1;

            IpAddress agent = new IpAddress(address);
            target = new UdpTarget((IPAddress)agent, 161, 2000, 2);

            Thread proxy = new Thread(initproxy);
            proxy.Start();
        }


        public void initproxy()
        {
            this.proxy = new AsynchronousSocketListener();
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

            OidNumber = result.Pdu.VbList[0].Oid.ToString();
            type = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);
            value = result.Pdu.VbList[0].Value.ToString();
            ipPort = address + ":161";

            return result;
        }
        public SnmpV1Packet GetNextRequest(string OID)
        {
            this.param.Version = SnmpVersion.Ver1;
            this.pdu = new Pdu(PduType.GetNext);
            this.pdu.VbList.Add(OID);
            result = (SnmpV1Packet)target.Request(pdu, param);

            OidNumber = result.Pdu.VbList[0].Oid.ToString();
            type = SnmpConstants.GetTypeName(result.Pdu.VbList[0].Value.Type);
            value = result.Pdu.VbList[0].Value.ToString();
            ipPort = address + ":161";

            return result;
        }
        public void GetTable(string OID)
        {
            this.param.Version = SnmpVersion.Ver2;
            Oid startOid = new Oid(OID);
            startOid.Add(1);
            Console.WriteLine(startOid);
            Pdu bulkPdu = Pdu.GetBulkPdu();
            bulkPdu.VbList.Add(startOid);
            bulkPdu.NonRepeaters = 0;
            bulkPdu.MaxRepetitions = 100;
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
                if (res.Version != SnmpVersion.Ver2)
                {
                    Console.WriteLine("Received wrong SNMP version response packet.");
                    target.Close();
                    return;
                }
                if (res.Pdu.ErrorStatus != 0)
                {
                    Console.WriteLine("SNMP agent returned error {0} for request Vb index {1}",
                                      res.Pdu.ErrorStatus, res.Pdu.ErrorIndex);
                    target.Close();
                    return;
                }

                foreach (Vb v in res.Pdu.VbList)
                {
                    curOid = (Oid)v.Oid.Clone();
                    if (startOid.IsRootOf(v.Oid))
                    {
                        uint[] childOids = Oid.GetChildIdentifiers(startOid, v.Oid);
                        uint[] instance = new uint[childOids.Length - 1];
                        Array.Copy(childOids, 1, instance, 0, childOids.Length - 1);
                        String strInst = InstanceToString(instance);
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
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (startOid.IsRootOf(curOid))
                {
                    bulkPdu.VbList.Clear();
                    bulkPdu.VbList.Add(curOid);
                    bulkPdu.NonRepeaters = 0;
                    bulkPdu.MaxRepetitions = 100;
                }
            }
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
                        foreach (Vb v in result.Pdu.VbList)
                        {
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
                                lastOid = v.Oid;                              
                            }
                            else
                            {
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
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 162);
            EndPoint ep = (EndPoint)ipep;
            socket.Bind(ep);

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
            bool run = true;
            int inlen = -1;
            while (run)
            {
                byte[] indata = new byte[16 * 1024];
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
                    int ver = SnmpPacket.GetProtocolVersion(indata, inlen);
                    if (ver == (int)SnmpVersion.Ver1)
                    {
                        SnmpV1TrapPacket pkt = new SnmpV1TrapPacket();
                        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        List<VarBind> varBindContentList = new List<VarBind>();
                        pkt.decode(indata, inlen);

                        foreach (Vb v in pkt.Pdu.VbList)
                        {
                            varBindContentList.Add(new VarBind(v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));

                        }
                        string ruleName = "NULL";
                        varBindListPerTrap.Add(trapCounter, varBindContentList);
                        windowHandler.addTrap(getGenericType(pkt.Pdu.Generic), pkt.Pdu.AgentAddress.ToString(),date, ruleName);
                        trapCounter++;
                    }
                    else
                    {
                        SnmpV2Packet pkt = new SnmpV2Packet();
                        string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        List<VarBind> varBindContentList = new List<VarBind>();
                        pkt.decode(indata, inlen);

                        if ((SnmpSharpNet.PduType)pkt.Pdu.Type != PduType.V2Trap)
                        {
                        }
                        else
                        {
                            foreach (Vb v in pkt.Pdu.VbList)
                            {
                                varBindContentList.Add(new VarBind(v.Oid.ToString(), SnmpConstants.GetTypeName(v.Value.Type), v.Value.ToString()));
                            }
                            string ruleName = "NULL";
                            varBindListPerTrap.Add(trapCounter, varBindContentList);
                            windowHandler.addTrap(pkt.Pdu.TrapObjectID.ToString(), "?", date, ruleName);
                            trapCounter++;
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

        private void Listen()
        {
            listener.Start();

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Połączono!");
                Thread clientThread = new Thread(new ParameterizedThreadStart(ListenThread));
                clientThread.Start(client);
            }
        }

        private void ListenThread(Object client)
        {
            TcpClient clienttmp = (TcpClient)client;
            //BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            StreamReader read = new StreamReader(clienttmp.GetStream());
            //BinaryWriter writer = new BinaryWriter(clienttmp.GetStream());
            StreamWriter write = new StreamWriter(clienttmp.GetStream());
            
            while (true)
            {
                String received_data = read.ReadLine();
                Console.WriteLine(received_data);
                SNMPJsonPacket packet = sendRequest(received_data);
                if (packet.value != null)
                {
                    try
                    {
                        write.WriteLine(packet.value.ToString());
                        Console.WriteLine("wyslano " + packet.value);
                    }catch(IOException e)
                    {
                        Console.WriteLine(e.Data);
                    }
                }
                 
                
               
            }

            // reader.Close();
        }

        private SNMPJsonPacket sendRequest(string oid)
        {
            SnmpV1Packet packet = this.GetRequest((string)oid);
            SNMPJsonPacket packetJson = new SNMPJsonPacket(packet.Pdu.VbList[0].Oid.ToString(), packet.Pdu.VbList[0].Value.ToString(), packet.Pdu.VbList[0].Type.ToString());
            return packetJson;
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void test()
        {
            SNMPQuery querry = new SNMPQuery("testoid");
            string data = JMessage.Serialize(JMessage.FromValue(querry));
            Console.WriteLine(data);
            Console.WriteLine("=========================");

            JMessage received_object = JMessage.Deserialize(data);
            if (received_object.Type == typeof(SNMPQuery))
            {
                SNMPQuery received_query = received_object.Value.ToObject<SNMPQuery>();

                Console.WriteLine(received_query.oid);

            }
            else
            {
                Console.WriteLine("Wrong received message format");
            }


        }

        public static string getRequestProxy(string content)
        {
            string result = "";
            content = content.Remove(content.Length - 1);
            SnmpV1Packet packet = Instance.GetRequest(content);
            return packet.Pdu.VbList[0].Value.ToString();
        }

        public static SnmpClient Instance
        {
            get { return instance; }
        }
        private static SnmpClient instance = new SnmpClient();
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