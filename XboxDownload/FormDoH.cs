﻿
using System;
using System.Diagnostics;
using System.Security.Policy;

namespace XboxDownload
{
    public partial class FormDoH : Form
    {

        public FormDoH()
        {
            InitializeComponent();

            if (Form1.dpixRatio > 1)
            {
                dataGridView1.RowHeadersWidth = (int)(dataGridView1.RowHeadersWidth * Form1.dpixRatio);
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                    col.Width = (int)(col.Width * Form1.dpixRatio);
            }
        }

        private void FormDoH_Load(object sender, EventArgs e)
        {
            List<DataGridViewRow> list = new();
            for (int i = 0; i <= DnsListen.dohs.GetLongLength(0) - 1; i++)
            {
                cbDoh.Items.Add(DnsListen.dohs[i, 0]);
                DataGridViewRow dgvr = new();
                dgvr.CreateCells(dataGridView1);
                dgvr.Resizable = DataGridViewTriState.False;
                dgvr.Tag = DnsListen.dohs[i, 1];
                string name = DnsListen.dohs[i, 0];
                dgvr.Cells[0].Value = !name.Contains("(科学)");
                dgvr.Cells[1].Value = name;
                list.Add(dgvr);
            }
            cbDoh.SelectedIndex = Properties.Settings.Default.DoHServer >= DnsListen.dohs.GetLongLength(0) ? 0 : Properties.Settings.Default.DoHServer;
            if (list.Count >= 1)
            {
                dataGridView1.Rows.AddRange(list.ToArray());
                dataGridView1.ClearSelection();
            }
        }

        private void ButSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DoHServer = cbDoh.SelectedIndex;
            Properties.Settings.Default.Save();
            DnsListen.dohServer = DnsListen.dohs[cbDoh.SelectedIndex, 1];
            this.Close();
        }

        private async void ButTest_Click(object sender, EventArgs e)
        {
            butTest.Enabled = false;
            dataGridView1.ClearSelection();
            string[] hosts = { "www.xbox.com", "www.playstation.com", "www.nintendo.com" };
            await Task.Run(() =>
            {
                Task[] tasks = new Task[dataGridView1.Rows.Count];
                for (int i = 0; i <= tasks.Length - 1; i++)
                {
                    int tmp = i;
                    tasks[tmp] = new Task(() =>
                    {
                        DataGridViewRow dgvr = dataGridView1.Rows[tmp];
                        dgvr.Cells[2].Value = dgvr.Cells[3].Value = dgvr.Cells[4].Value = null;
                        dgvr.Cells[2].Style.ForeColor = dgvr.Cells[3].Style.ForeColor = dgvr.Cells[4].Style.ForeColor = Color.Empty;
                        if (Convert.ToBoolean(dgvr.Cells[0].Value))
                        {
                            string? doh = dgvr.Tag?.ToString();
                            if (!string.IsNullOrEmpty(doh))
                            {
                                _ = ClassDNS.DoH("www.baidu.com", doh, 1000);
                                Stopwatch sw = new();
                                for (int x = 0; x <= hosts.Length - 1; x++)
                                {
                                    string host = hosts[x];
                                    sw.Restart();
                                    string? ip = ClassDNS.DoH(host, doh, 3000);
                                    sw.Stop();
                                    if (!string.IsNullOrEmpty(ip))
                                    {
                                        dgvr.Cells[x + 2].Value = sw.ElapsedMilliseconds;
                                    }
                                    else
                                    {
                                        dgvr.Cells[x + 2].Value = "error";
                                        dgvr.Cells[x + 2].Style.ForeColor = Color.Red;
                                    }
                                }
                            }
                        }
                    });
                }
                Array.ForEach(tasks, x => x.Start());
                Task.WaitAll(tasks);
            });
            butTest.Enabled = true;
        }
    }
}