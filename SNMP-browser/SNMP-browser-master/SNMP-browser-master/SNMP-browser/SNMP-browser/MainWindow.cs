using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNMP_browser
{

    public partial class MainWindow : Form
    {
        SnmpClient snmp = new SnmpClient();
        public MainWindow()
        {
            InitializeComponent();      
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            treeView1.Nodes.Add("MIB Tree");
            treeView1.Nodes[0].Nodes.Add("iso.org.dod.internet.mgmt.mib-2");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("system");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysDescr");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysObjectID");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysUpTime");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysContact");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysName");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysLocation");
            treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add("sysServices");

            treeView1.Nodes[0].Nodes[0].Nodes.Add("interfaces");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("at");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("ip");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("icmp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("tcp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("udp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("egp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("snmp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("host");
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
                switch (treeView1.SelectedNode.Text)
                {
                    case "sysDescr":
                        snmp.conn("sysDescr");
                        break;
                    case "sysObjectID":
                        snmp.conn("sysObjectID");
                        break;
                    case "sysUpTime":
                        snmp.conn("sysUpTime");
                        break;
                    case "sysContact":
                        snmp.conn("sysContact");
                        break;
                    case "sysName":
                        snmp.conn("sysName");
                        break;
                    case "sysLocation":
                        snmp.conn("sysLocation");
                        break;
                    case "sysServices":
                        snmp.conn("sysServices");
                        break;
                    default:
                        snmp.conn("inny");
                        break;

                }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            
            

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
