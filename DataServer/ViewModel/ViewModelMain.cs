using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using project.Model;
using project.Helpers;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace project.ViewModel
{
    partial class ViewModelMain : ViewModelBase
    {
        public static QUIKSHARPconnector quik;
        public static List<Instumensts> _instr;
        private static Mutex m_instance;
        public static event Action stopprogramm;


        public ViewModelMain()
        {
            ini_command();
           
            bool tryCreateNewApp;
            m_instance = new Mutex(true, "CobraDataSerevMutex", out tryCreateNewApp);
            if (!tryCreateNewApp)
            {
                if (stopprogramm != null) stopprogramm();
                return;
            }

            CreateTimer1(500);
            _instr = new List<Instumensts>();
            FilesWork f = new FilesWork("d:/z/zAmerikaFinam/MQL4/Files/CobraConnector/ticker.ini");
            f.ReadListInstrument(_instr);

            create();
        }

        static bool loc = false;
        public static void timer()
        {
            if (loc) return;
            loc= true;
            
            if (data.fatal)
            {
 
                try
                {
                    quik.Stop();
                    
                }
                catch
                { }

  
                data.fatal_need_rst_task = true;

                mt.Abort(5000);
                while (mt.ThreadState == System.Threading.ThreadState.Running)
                { Thread.Sleep(500); }

                //MessageBox.Show("task остановлена");

                data.fatal = false;
                data.block_new_pipe = false;

                create();
            }

            loc = false;
        }

        public static CancellationTokenSource cts1;
        public static CancellationToken cancellationToken;
        public static Thread mt;
        static bool thread_start = false;
        public static void create()
        {
            if (thread_start) return;
            thread_start = true;
               data.fatal = false;
                cts1 = new CancellationTokenSource();
                cancellationToken = cts1.Token;//для task1

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            mt = new Thread(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    ViewModelMain.task1_release(cancellationToken);
                    tcs.SetResult("ok");
                    thread_start = false;
                }
                catch (OperationCanceledException e)
                {
                    tcs.SetException(e);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
               // return tcs.Task;
            });
            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            mt.Name = "QUIKSHARP THREAD";
            mt.Start();
        }


        /// <summary>
        /// 
        /// </summary>
        public static void task1_release(CancellationToken cts)
        {

                while (true)
                {
                    if (data.fatal_need_rst_task || cts.IsCancellationRequested) break;
                    Thread.Sleep(500);
                    if (quik == null) quik = new QUIKSHARPconnector();

                    if (quik != null)
                    {
                        if (!quik.Connect(_instr)) { Thread.Sleep(2500); continue; }
                    }
                
                    Thread.Sleep(5000);
                    if (data.fatal) break;
                    quik.work();//main cycle


                    data.block_new_pipe = true;
                    quik.Stop();
                }
                data.fatal_need_rst_task = false;//task закончена
                quik = null;

                Thread.Sleep(3000);
            }



    }//class

}//namespace
