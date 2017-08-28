using System.Windows;
using IPTVman.ViewModel;
using System.Windows.Data;
using System.Windows.Threading;
using project.Model;
using project.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace project.ViewModel
{
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            this.Title = "Settings";
           
            // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += timer_Tick;
            timer.Start();

            Task.Run(() =>
            {
                ViewModelMain.task1_release();
            });
        }

        

        private void timer_Tick(object sender, EventArgs e)
        {

        }


        void cmd(int cod, int p1, int p2, string s)
        {

        }

    }
}
