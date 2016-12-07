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

    partial class MainWindow : Form
    {
        SnmpClient snmp;
        DataGridView grid = new DataGridView();



        public MainWindow(SnmpClient client)
        {
            this.snmp = client;
            InitializeComponent();
            dataGridView();

        }

        private void dataGridView()
        {
            var binding = new BindingSource();
            tabPage1.Controls.Add(grid);
            tabPage1.Refresh();
            grid.Visible = true;
            grid.Size = new System.Drawing.Size(450, 272);
            grid.ColumnCount = 4;
            grid.Columns[0].Name = "Name/OID";
            grid.Columns[1].Name = "Value";
            grid.Columns[2].Name = "Type";
            grid.Columns[3].Name = "IP:Port";
            grid.DataSource = binding.DataSource;
            //addRows();    
        }

        private void addRows(string oid)
        {
                snmp.GetRequest(oid);
                grid.Rows.Add(snmp.getOidNumber(), snmp.getValue(), snmp.getType(), snmp.getIpPort());
        }
        private void addRowsNext(string oid)
        {
            snmp.GetNextRequest(oid);
            grid.Rows.Add(snmp.getOidNumber(), snmp.getValue(), snmp.getType(), snmp.getIpPort());
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            snmp.GetTree();
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
            foreach (var i in snmp.lista)
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

        private void button1_Click_1(object sender, EventArgs e)
        {
            string oid = snmp.translate(null, treeView1.SelectedNode.Text);
            if(this.comboBox1.Text == "GetRequest")
            {
                snmp.GetRequest(oid+".0");
                addRows(oid + ".0");
            }
            else if (this.comboBox1.Text == "GetNextRequest")
            {
                snmp.GetNextRequest(oid + ".0");
                addRowsNext(oid + ".0");
            }
            else if (this.comboBox1.Text == "GetTable")
            {
                foreach (var i in snmp.lista)
                    {
                        if (treeView1.SelectedNode.Text == i.name && i.name.Contains("Table"))
                        {
                            tabPage4.Controls.Clear();
                            DataGridView table = new DataGridView();
                            table.Columns.Clear();
                            table.Rows.Clear();
                            snmp.tableColumns.Clear();
                            snmp.results.Clear();
                            tabPage4.Refresh();
                            tabPage4.Controls.Add(table);
                            table.Size = new System.Drawing.Size(450, 272);
                            snmp.GetTable(i.Oid);
                            table.ColumnCount = snmp.tableColumns.Count;
                            for (int j = 1; j <= table.ColumnCount; j++)
                            {
                                table.Columns[j-1].Name = i.Oid+".1."+j;
                            }
                            
                            foreach ( String key in snmp.results.Keys)
                            {

                                var index = table.Rows.Add();
                                for (uint j = 0; j < table.ColumnCount; j++)
                                {
                                        int mm = (int)j;
                                        table.Rows[index].Cells[mm].Value = snmp.results[key][j+1].ToString();
                                }
                            }
                            tabPage4.Refresh();
                            table.Visible = true;
                        }
                    }
            }
            Console.WriteLine("koniec");

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            grid.Rows.Clear();
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
                        foreach (var i in snmp.lista)
                        {
                            if (i.name == NodeName)
                                addRows(i.Oid);
                        }
                 }
            }
           
        }

    }
}
