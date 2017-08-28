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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Cobra Data Server v1.0";

            //Подписки
     
            QUIKSHARPconnector.Event_Print += new Action<string, object>(add);
            QUIKSHARPconnector.Event_CMD += new Action<int, int, int, string>(cmd);
            Pipe.Event_Print += new Action<string, object>(add);
            Pipe.Event_CMD += new Action<int, int, int, string>(cmd);

            // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 800);
            timer.Tick += timer_Tick;
            timer.Start();

            Task.Run(() =>
            {
                ViewModelMain.task1_release();
            });
        }

        void add(string msg, object c)
        {
            box.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                //if (box.l > 5000) box.Clear();
                //box.AppendText(s + Environment.NewLine);

                TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                range.Text = msg+ "\r";
                range.ApplyPropertyValue(TextElement.ForegroundProperty, c);
                box.ScrollToEnd();//  Autoscroll
            }));
        }

        private void timer_Tick(object sender, EventArgs e)
        {
                l1.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    l1.Content = data.ct_global.ToString();
                }));


            string msg = "";

            foreach (var i in ViewModelMain._instr)
            {
                msg = i.name + "  pipesend=" + i.ct.ToString()+"\r";
            }

            boxstat.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                //if (box.l > 5000) box.Clear();
                //box.AppendText(s + Environment.NewLine);
                boxstat.Document.Blocks.Clear();

                TextRange range = new TextRange(boxstat.Document.ContentEnd, boxstat.Document.ContentEnd);
                range.Text = msg;
                range.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Green);
            }));

        }


        void cmd(int cod, int p1, int p2, string s)
        {

        }

        Window setting;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (setting != null) return;
            setting = new WindowSettings
            {
                Topmost = true,
                WindowStyle = WindowStyle.ToolWindow,
                Name = "winsettings"
            };

            setting.Closing += setting_Closing;
            setting.Show();
        }
        private void setting_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            setting = null;
        }

    }
   

}
