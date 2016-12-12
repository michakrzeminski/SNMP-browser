using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SNMP_browser
{

    public partial class MainWindow : Form
    {
        SnmpClient snmpClient;

        public MainWindow()
        {
            this.snmpClient = new SnmpClient(this);
            InitializeComponent();
            dataGridView();
            trapGridView();
            monitorGridView();
        }

        private void dataGridView()
        {
            var binding = new BindingSource();
            tabPage1.Controls.Add(grid);
            tabPage1.Refresh();
            grid.Visible = true;
            grid.Size = new System.Drawing.Size(tabPage1.Width, tabPage1.Height);
            grid.ColumnCount = 4;
            grid.Columns[0].Name = "Name/OID";
            grid.Columns[1].Name = "Value";
            grid.Columns[2].Name = "Type";
            grid.Columns[3].Name = "IP:Port";
            grid.DataSource = binding.DataSource;  
        }

        private void trapGridView()
        {
            var binding = new BindingSource();
            splitContainer1.Panel1.Controls.Add(grid2);
            splitContainer1.Refresh();
            grid2.Visible = true;
            grid2.Size = new System.Drawing.Size(splitContainer1.Panel1.Width, splitContainer1.Panel1.Height);
            grid2.ColumnCount = 4;
            grid2.Columns[0].Name = "Description";
            grid2.Columns[1].Name = "Source";
            grid2.Columns[2].Name = "Time";
            grid2.Columns[3].Name = "Severity";
            grid2.DataSource = binding.DataSource;
            grid2.SelectionChanged += grid2_SelectionChanged;
        }

        private void monitorGridView()
        {
            var binding = new BindingSource();
            tabPage3.Controls.Add(grid3);
            tabPage3.Refresh();
            grid3.Visible = true;
            grid3.Size = new System.Drawing.Size(tabPage3.Width, tabPage3.Height);
            grid3.ColumnCount = 4;
            grid3.Columns[0].Name = "Name/OID";
            grid3.Columns[1].Name = "Value";
            grid3.Columns[2].Name = "Type";
            grid3.Columns[3].Name = "IP:Port";
            grid3.DataSource = binding.DataSource;
        }

        delegate void addTrapCallback(string description, string source, string time, string severity);

        public void addTrap(string description, string source, string time, string severity)
        {
            if (this.grid2.InvokeRequired)
            {
                addTrapCallback d = new addTrapCallback(addTrap);
                this.Invoke(d, new object[] { description, source, time, severity });
            }
            else
            {
                grid2.Rows.Add(description, source, time, severity);
            }  
        }

        delegate void addMonitorRowCallback(string OID, string value, string type, string ipPort);

        public void addMonitorRow(string OID, string value, string type, string ipPort)
        {           
            if (this.grid3.InvokeRequired)
            {
                addMonitorRowCallback d = new addMonitorRowCallback(addMonitorRow);
                this.Invoke(d, new object[] { OID, value, type,ipPort });
            }
            else
            {
                grid3.Rows.Add(OID, value, type, ipPort);
            }
        }

        private void addRows(string oid)
        {
                snmpClient.GetRequest(oid);
                grid.Rows.Add(snmpClient.getOidNumber(), snmpClient.getValue(), snmpClient.getType(), snmpClient.getIpPort());
        }
        private void addRowsNext(string oid)
        {
            snmpClient.GetNextRequest(oid);
            grid.Rows.Add(snmpClient.getOidNumber(), snmpClient.getValue(), snmpClient.getType(), snmpClient.getIpPort());
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            snmpClient.GetTree();
            treeView1.Nodes.Add("MIB Tree");
            treeView1.Nodes[0].Nodes.Add("iso.org.dod.internet.mgmt.mib-2");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("system");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("interfaces");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("at");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("ip");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("icmp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("tcp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("udp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("egp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("snmp");
            treeView1.Nodes[0].Nodes[0].Nodes.Add("host");
            foreach (var i in snmpClient.lista)
            {
                if (i.Oid.Contains("1.3.6.1.2.1.1."))
                    treeView1.Nodes[0].Nodes[0].Nodes[0].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.2."))
                    treeView1.Nodes[0].Nodes[0].Nodes[1].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.3."))
                    treeView1.Nodes[0].Nodes[0].Nodes[2].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.4."))
                    treeView1.Nodes[0].Nodes[0].Nodes[3].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.5."))
                    treeView1.Nodes[0].Nodes[0].Nodes[4].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.6."))
                    treeView1.Nodes[0].Nodes[0].Nodes[5].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.7."))
                    treeView1.Nodes[0].Nodes[0].Nodes[6].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.8."))
                    treeView1.Nodes[0].Nodes[0].Nodes[7].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.10."))
                    treeView1.Nodes[0].Nodes[0].Nodes[8].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.11."))
                    treeView1.Nodes[0].Nodes[0].Nodes[8].Nodes.Add(i.name);
                if (i.Oid.Contains("1.3.6.1.2.1.25."))
                    treeView1.Nodes[0].Nodes[0].Nodes[9].Nodes.Add(i.name);

            }          
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string oid = textBox1.Text;
            if(this.comboBox1.Text == "GetRequest")
            {
                snmpClient.GetRequest(oid);
                if(snmpClient.getValue()=="Null")
                    MessageBox.Show("No such name error (127.0.0.1)");
                else
                addRows(oid);
            }
            else if (this.comboBox1.Text == "GetNextRequest")
            {
                snmpClient.GetNextRequest(oid );
                addRowsNext(oid );
                textBox1.Text = snmpClient.getOidNumber();
            }
            else if (this.comboBox1.Text == "GetTable")
            {
                foreach (var i in snmpClient.lista)
                    {
                        if (treeView1.SelectedNode.Text == i.name && i.name.Contains("Table"))
                        {
                            tabPage4.Controls.Clear();
                            DataGridView table = new DataGridView();
                            table.Columns.Clear();
                            table.Rows.Clear();
                            snmpClient.tableColumns.Clear();
                            snmpClient.results.Clear();
                            tabPage4.Refresh();
                            tabPage4.Controls.Add(table);
                            table.Size = new System.Drawing.Size(450, 272);
                            snmpClient.GetTable(i.Oid);
                            table.ColumnCount = snmpClient.tableColumns.Count;
                            for (int j = 1; j <= table.ColumnCount; j++)
                            {
                                table.Columns[j-1].Name = i.Oid+".1."+j;
                            }
                            
                            foreach ( String key in snmpClient.results.Keys)
                            {

                                var index = table.Rows.Add();
                                for (uint j = 0; j < table.ColumnCount; j++)
                                {
                                        int mm = (int)j;
                                        table.Rows[index].Cells[mm].Value = snmpClient.results[key][j+1].ToString();
                                }
                            }
                            tabPage4.Refresh();
                            table.Visible = true;
                        }
                    }
            }
        }

        private void grid2_SelectionChanged(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            if (grid2.CurrentCell.RowIndex != (grid2.RowCount - 1))
            {
                List<VarBind> varBindListTmp = snmpClient.varBindListPerTrap[grid2.CurrentCell.RowIndex];
                richTextBox1.AppendText(String.Format("Variable Bindings:{0}", Environment.NewLine));
                foreach (VarBind v in varBindListTmp)
                {
                    richTextBox1.AppendText(String.Format("{0}  {1}  {2}{3}", v.OID, v.type, v.value, Environment.NewLine));

                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (tabPage1 == TabControl.SelectedTab)
            {
                grid.Rows.Clear();
            }else if(tabPage2 == TabControl.SelectedTab)
            {
                grid2.Rows.Clear();
                snmpClient.varBindListPerTrap.Clear();
                snmpClient.resetTrapCounter();
            }else if(tabPage3 == TabControl.SelectedTab)
            {
                grid3.Rows.Clear();
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (treeView1.SelectedNode.Nodes.Count == 0)
            {
                if (treeView1.SelectedNode.Text.Contains("Table"))
                {
                }
                 else
                 {
                        string NodeName = treeView1.SelectedNode.Text;
                        foreach (var i in snmpClient.lista)
                        {
                            if (i.name == NodeName)
                                addRows(i.Oid);
                        }
                 }
            }
           
        }

        

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            
            string oid = snmpClient.translate(null, treeView1.SelectedNode.Text);

            if (treeView1.SelectedNode.Nodes.Count == 0)
            {
                snmpClient.GetRequest(oid + ".0");
                textBox1.Text = snmpClient.getOidNumber();
            }
            else
                textBox1.Text = oid ;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void monitorButton_Click(object sender, EventArgs e)
        {
            string oid = textBox1.Text;
            TabControl.SelectedTab = tabPage3;
            snmpClient.monitor = true;
            Thread monitorThread = new Thread(new ParameterizedThreadStart(snmpClient.monitorObject));
            monitorThread.Start(oid);         
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            snmpClient.monitor = false;
        }
    }
}
