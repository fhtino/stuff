using HCALib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace HttpsCertAlert
{
    public partial class MainForm : Form
    {

        private Data _data;
        private bool _iconAnimationFlag = false;
        private System.Timers.Timer _checkTimer;



        public MainForm()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.Icon = Properties.Resources.world_blue;
            notifyIcon.Icon = Properties.Resources.lock_blue;
            notifyIcon.Visible = true;

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
                await Engine.Exec(_data);
                _data.Save();
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

            dataGridView1.DataSource = _data.CheckList
                .Select(
                    x => new
                    {
                        URL = x.Url,
                        Status = x.Status,
                        LastCheck = x.LastCheckDT,
                        ExpirationDelta = (x.ValidToDT != DateTime.MinValue) ?
                                          (int?)DateTime.UtcNow.Subtract(x.ValidToDT).TotalDays :
                                          null,
                        x.LastError
                    })
                .OrderBy(x => x.ExpirationDelta)
                .ToList();

            if (position > 0)
                dataGridView1.FirstDisplayedScrollingRowIndex = position;
        }
        

        private void updateGUITimer_Tick(object sender, EventArgs e)
        {
            UpdateGrid();

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


    }

}
