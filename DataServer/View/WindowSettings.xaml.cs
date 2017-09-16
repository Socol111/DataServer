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

            hour.Text = data.hour_start_pipe.ToString();

             // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        

        private void timer_Tick(object sender, EventArgs e)
        {
           
        }


        void cmd(int cod, int p1, int p2, string s)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (newel.Text == "") return;
            Model.data.eliminate.Add(newel.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Model.data.eliminate.RemoveAt(lst1.SelectedIndex);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}
