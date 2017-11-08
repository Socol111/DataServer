using System.Windows;
using System.Windows.Threading;
using System;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Threading;
using System.Windows.Forms;
using Serilog;
using System.Runtime.InteropServices;

namespace CobraDataServer
{
    public partial class MainWindow : Window
    {
        public Task task1=null;
        public Window header;
        private object threadLock = new object();
        int ct_no_connect=0;
        readonly string NameServer = "Cobra Data Server v1.0";
        public MainWindow()
        {   
        
            bool existed;
            // получаем GIUD приложения
            string guid = Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly()).ToString();

            Mutex mutexObj = new Mutex(true, guid, out existed);

            if (!existed)
            {
                System.Windows.MessageBox.Show("Уже есть запущенная копия " + guid);
                this.Close();
            }

            header = this;
            this.Title = NameServer;
            InitializeComponent();


            SETTING.ReadFromXML();
           
            data.getTickers();
            //Подписки
            ViewModelMain.winadd += ViewModelMain_winadd;
            ViewModelMain.winerr += ViewModelMain_winerr;

            mes.Event_Print += new Action<string, object>(add);
            //QUIKSHARPconnector.Event_CMD += new Action<int, int, int, string>(cmd);
            //Pipe.Event_Print += new Action<string, object>(add);
            //Pipe.Event_CMD += new Action<int, int, int, string>(cmd);

            // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            timer.Tick += Timer_Tick;
            timer.Start();


            // use a timer to periodically update the memory usage
            DispatcherTimer timer2 = new DispatcherTimer();
            timer2.Interval = new TimeSpan(1, 0, 0);
            timer2.Tick += Timer_TickHOUR;
            timer2.Start();


            //Install - Package Serilog
            //Install - Package Serilog.Sinks.Literate
            //Install - Package Serilog.Sinks.RollingFile
            Log.Logger = new LoggerConfiguration()
                           .MinimumLevel.Debug()
                           .WriteTo.LiterateConsole()
                           .WriteTo.RollingFile("logs\\{Date} Cobra Data Server.txt")
                           .CreateLogger();

            Log.Debug("Start Cobra Data Server");

        }

        string getCT()
        {

            int rez= data.ct_global - data.ctglobalATstart;
            return rez.ToString();
        }

       
        private void Timer_TickHOUR(object sender, EventArgs e)
        {
            if (DateTime.Now.Hour == 10 ) data.ctglobalATstart = data.ct_global;
            if (DateTime.Now.Hour == 18 ) Log.Debug("Статистика событий 18ч "+ getCT());
            if (DateTime.Now.Hour == 23) Log.Debug("Статистика событий 23ч " + getCT());
        }

        private void ViewModelMain_winerr(string obj)
        {
            add((string)obj, System.Windows.Media.Brushes.Red);
        }

        private void ViewModelMain_winadd(string obj)
        {
            add((string)obj, System.Windows.Media.Brushes.Green);
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

        byte rst = 0;
        void add(string msg, object c)
        {
            if (msg == "*")
            {
                rst++;
                if (rst > 100)
                {
                    rst = 0;
                    box.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        box.Document.Blocks.Clear();
                    }));
                }
            }
            else
            box.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                //if (box.l > 5000) box.Clear();
                //box.AppendText(s + Environment.NewLine);

                TextRange range = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                range.Text = DateTime.Now.ToString() + "." + DateTime.Now.Millisecond + "   " + msg + "\r";
                range.ApplyPropertyValue(TextElement.ForegroundProperty, c);
                box.ScrollToEnd();//  Autoscroll
            }));
        }

        string mess = "";
        int l1_mem = 0;
        bool loc = false;
        int ct_fatal; byte cttitle;

        private void Timer_Tick(object sender, EventArgs e)
        {
            tmr.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
            {
                tmr.Content = DateTime.Now.ToString();
            }));

            servertime.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
            {
               servertime.Content = data.servertime;
            }));

            if (loc) return;
            loc = true;

            if (data.Not_connect && !data.fatal)
            {
                
                ct_fatal++;
                if (ct_fatal > 50)
                {
                    add("-- Фатальный рестарт ---", System.Windows.Media.Brushes.Red);
                    Log.Debug( "Фатальный рестарт");
                    data.fatal = true;
                    ct_fatal = 0;
                }
            }


            if (l1_mem != data.ct_global)
            {
                data.Not_connect = false;

                if (data.first_Not_data)
                {
                    data.first_Not_data = false;
                    Log.Debug("             Данные появились");
                }

                ct_no_connect = 0;

                l1.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1.Content = data.ct_global.ToString();
                }));

                l1err.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    l1err.Content = "";
                }));

                buf.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    buf.Content = QUIKSHARPconnector.getSIZEorderbook.ToString();
                }));

                buftrades.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    buftrades.Content = QUIKSHARPconnector.getSIZEtrade.ToString();
                }));

                bufpipe.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    bufpipe.Content = data.pipeque.Count.ToString();
                }));

                bufdb.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
                {
                    bufdb.Content = mydb.FIFOorderbook.Count.ToString()+"/"+mydb.FIFOtrade.Count.ToString();
                }));

                cttitle++;
                if (cttitle == 1) this.Title = "/ " + NameServer;
                if (cttitle == 2) this.Title = "- " + NameServer;
                if (cttitle == 3) this.Title = @"\ " + NameServer;
                if (cttitle == 4) { this.Title = @"| " + NameServer; cttitle = 0; }

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


                if (ct_no_connect == 5 && DateTime.Now.Hour>=10)
                {
                    if ( !data.first_Not_data)
                    {
                        data.first_Not_data = true;

                        Log.Debug("Нет потока данных");
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


                if (data.Not_connect) { ct_no_connect = 0; ct_fatal = 0; goto exit; }


                //if (ct_no_connect == 12) { data.need_rst = true; }
                if (ct_no_connect > 29)
                {
                    if (DateTime.Now.Hour >= 10 && DateTime.Now.Hour <= 23)
                    {
                        add("сработал счетчик нет данных", System.Windows.Media.Brushes.Red);
                        data.Not_data = true;
                    }
                    ct_no_connect = 0;
                    goto exit;
                    
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

            foreach (var i in data._instr)
            {
                mess += i.name + "/" + i.namefull
                                + "  pipesend=" + i.ct
                               + "  orders=" + i.orders
                               + "  inter=" + i.interes
                    + "\r";
            }

            boxstat.Dispatcher.Invoke(/*DispatcherPriority.Background,*/ new Action(() =>
            {
                //box.AppendText(s + Environment.NewLine);
                boxstat.Document.Blocks.Clear();

                TextRange range = new TextRange(boxstat.Document.ContentEnd, boxstat.Document.ContentEnd);
                range.Text = mess;
                range.ApplyPropertyValue(TextElement.ForegroundProperty, System.Windows.Media.Brushes.Green);
            }));

            exit:
            loc = false;
        }


        Window setting;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (setting != null) return;
            setting = new WindowSettings
            {
                Topmost = true,
                WindowStyle = WindowStyle.ThreeDBorderWindow,
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
           
            threadprocess.stop_all();

            Log.Debug("=========== Close Cobra Data Server ===========");
            Log.CloseAndFlush();

        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            threadprocess.exit = true;
            Thread.Sleep(900);
            foreach (var p in data.listpipe)
            {
                p.stopPIPE();
            }
            this.Close();
        }
    }



  

}
