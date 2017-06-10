using System.Windows;
using IPTVman.ViewModel;
using System.Windows.Data;
using System.Windows.Threading;
using project.Model;
using project.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace project.ViewModel
{
    public delegate void Delegate_Print(string s);
    public delegate void Delegate_Command(int cod, int p1, int p2,string s);


    public partial class MainWindow : Window
    {
        QUIKSHARPconnector q;


        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Cobra Data Server v1.0";

            //Подписки
            QUIKSHARPconnector.Event_Print += new Delegate_Print(add);
            QUIKSHARPconnector.Event_CMD += new Delegate_Command(cmd);


            q = new QUIKSHARPconnector();

            RUN("");


            // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += timer_Tick;
            timer.Start();

         
        }



        public Task<string> AsyncTaskSTART(string url)
        {

            return Task.Run(() =>
            {
                //----------------


                q.start();
                return "";

                //----------------
            });
        }

        async void RUN(string x)
        {
            string ss = await AsyncTaskSTART(x);
        }



        void add(string s)
        {

            tb.Dispatcher.Invoke(DispatcherPriority.Background, new
            Action(() =>
            {
                if (tb.LineCount > 500) tb.Clear();
                tb.AppendText(s + Environment.NewLine);
                tb.ScrollToEnd();
            }));
        }


        private void timer_Tick(object sender, EventArgs e)
        {

         

        }


        void cmd(int cod, int p1, int p2, string s)
        {


        }









    }
}
