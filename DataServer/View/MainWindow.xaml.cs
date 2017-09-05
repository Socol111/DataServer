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
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace project.ViewModel
{
    public partial class MainWindow : Window
    {
        public Task task1=null;
        public Window header;
        private object threadLock = new object();
        int ct_no_connect=0;

        public MainWindow()
        {
            header = this;
            ViewModelMain.stopprogramm += new Action(endprog);
            this.Title = "Cobra Data Server v1.0";
            InitializeComponent();

           


            //Подписки
            QUIKSHARPconnector.Event_Print += new Action<string, object>(add);
            //QUIKSHARPconnector.Event_CMD += new Action<int, int, int, string>(cmd);
            Pipe.Event_Print += new Action<string, object>(add);
            //Pipe.Event_CMD += new Action<int, int, int, string>(cmd);

            // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            timer.Tick += timer_Tick;
            timer.Start();

            
 
        }

        private void endprog()
        {
            var result = System.Windows.Forms.MessageBox.Show(
             "Уже запущен экземпляр сервера",
             "Сообщение",
             MessageBoxButtons.OK,
             MessageBoxIcon.Information,
             MessageBoxDefaultButton.Button1
             );
            this.Close();
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

        string mess = "";
        int l1_mem = 0;
        bool loc = false;
        int ct_fatal;
        private void timer_Tick(object sender, EventArgs e)
        {
            if (loc) return;
            loc = true;

            tmr.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
            {
                tmr.Content = DateTime.Now.ToString();
            }));

            if (data.Not_connect && !data.fatal)
            {
                
                ct_fatal++;
                if (ct_fatal > 50)
                {
                    add("-- запуск fatal restart ---", System.Windows.Media.Brushes.Red);
                    data.fatal = true;
                    ct_fatal = 0;
                }
            }






            if (l1_mem != data.ct_global)
            {
                ct_no_connect = 0;

                l1.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1.Content = data.ct_global.ToString();
                }));

                l1err.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1err.Content = "";
                }));

                l1_mem = data.ct_global;
            }
            else//счетчик стоит
            {

                if (data.Not_data) goto exit;
                if (data.Not_connect) goto exit;

                ct_no_connect++;

                l1err.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1err.Content = ct_no_connect.ToString();
                }));


                if (ct_no_connect == 2)
                {
                    if (!data.Not_data)
                    {
                        tmr.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                        {
                            tmr_last.Content = DateTime.Now.ToString();
                        }));
                    }

                    l1.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                    {
                        l1.Foreground = System.Windows.Media.Brushes.Red;
                    }));
                }

                //if (ct_no_connect == 12) { data.need_rst = true; }
                if (ct_no_connect > 29)
                {
                    
                    data.Not_data = true; ct_no_connect = 0; goto exit;
                    
                }
            }

            if (ct_no_connect==0)
            {
                l1.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1.Foreground = System.Windows.Media.Brushes.White;
                }));

            }


            

           

           

            mess = "";
           // mess = "Всего pipesend="+data.ct_global.ToString()+"\r";

            foreach (var i in ViewModelMain._instr)
            {
                mess += i.name + "  pipesend=" + i.ct.ToString() + "\r";
            }

            boxstat.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
            {
                //if (box.l > 5000) box.Clear();
                //box.AppendText(s + Environment.NewLine);
                boxstat.Document.Blocks.Clear();

                TextRange range = new TextRange(boxstat.Document.ContentEnd, boxstat.Document.ContentEnd);
                range.Text = mess;
                range.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Green);
            }));

            exit:
            loc = false;
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (ViewModelMain.mt!=null)
                ViewModelMain.mt.Abort();
        }
    }
   

}
