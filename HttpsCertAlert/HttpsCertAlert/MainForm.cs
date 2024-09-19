using HCALib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace HttpsCertAlert
{
    public partial class MainForm : Form
    {

        private Data _data;
        private bool _iconAnimationFlag = false;
        private System.Timers.Timer _checkTimer;
        private string _sortingColumn = null;
        private bool _sortingOrderAsc = true;



        public MainForm()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.world_blue;
            notifyIcon.Icon = Properties.Resources.lock_blue;
            notifyIcon.Visible = true;

            lblVersion.Text = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _data = Data.LoadFromFile("data.xml");

            _checkTimer = new System.Timers.Timer(1000);
            _checkTimer.AutoReset = false;
            _checkTimer.Elapsed += _checkTimer_Elapsed;
            _checkTimer.Start();

            UpdateGrid();
        }


        private async void _checkTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                toolStripStatusInfo.Text = "Processing...";
                    await Engine.Exec(_data);
                _data.Save();
                toolStripStatusInfo.Text = "";
            }
            finally
            {
                var timer = (sender as System.Timers.Timer);
                timer.Interval = 1 * 60 * 1000;
                timer.Start();   // reactivate the timer
            }
        }


        private void UpdateGrid()
        {
            int position = dataGridView1.FirstDisplayedScrollingRowIndex;

            var data = _data.CheckList
                .Select(
                    x => new
                    {
                        Group = x.Group,
                        URL = x.Url,
                        Status = x.Status,

                        ExpirationDays = (x.ValidToDT != DateTime.MinValue) ?
                                          (int?)DateTime.UtcNow.Subtract(x.ValidToDT).TotalDays :
                                          null,
                        ValidTo = x.ValidToDT,
                        LastCheck = x.LastCheckDT,
                        x.LastError
                    });

            if (_sortingColumn == null)
            {
                _sortingColumn = "ValidTo";
                _sortingOrderAsc = true;
            }

            var dataOrdered = data.OrderBy(x => x.GetType().GetProperty(_sortingColumn).GetValue(x)).ToList();
            if (!_sortingOrderAsc)
            {
                dataOrdered.Reverse();
            }

            dataGridView1.DataSource = dataOrdered.ToList();

            dataGridView1.Columns[0].Width = 100;
            dataGridView1.Columns[1].Width = 200;
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Width = 100;
            dataGridView1.Columns[4].Width = 100;
            dataGridView1.Columns[5].Width = 100;
            dataGridView1.Columns[6].Width = 200;

            if (position > 0)
                dataGridView1.FirstDisplayedScrollingRowIndex = position;
        }


        private void updateGUITimer_Tick(object sender, EventArgs e)
        {
            if (cbAutoRefresh.Checked)
            {
                UpdateGrid();
            }

            _iconAnimationFlag = !_iconAnimationFlag;

            if (_iconAnimationFlag)
            {
                var ms = _data.CheckList.Max(x => x.Status);

                switch (ms)
                {
                    case Status.UNKNOWN: notifyIcon.Icon = Properties.Resources.lock_blue; break;
                    case Status.OK: notifyIcon.Icon = Properties.Resources.lock_green; break;
                    case Status.EXPIRING: notifyIcon.Icon = Properties.Resources.lock_blue; break;
                    case Status.EXPIRED: notifyIcon.Icon = Properties.Resources.lock_red; break;
                    case Status.ERROR: notifyIcon.Icon = Properties.Resources.lock_red; break;
                }
            }
            else
            {
                notifyIcon.Icon = Properties.Resources.lock_green;
            }

        }


        private void notifyIcon_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }


        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                // this.Hide();  // ??? required ???
            }
        }


        private void dataGridView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string newColumnName = dataGridView1.Columns[e.ColumnIndex].DataPropertyName;

            if (newColumnName == _sortingColumn)
            {
                _sortingOrderAsc = !_sortingOrderAsc;
            }

            _sortingColumn = newColumnName;
        }


        private void linkToWebSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/fhtino/stuff");
        }

    }

}
