using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CobraDataServer;
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
using System.Runtime.InteropServices;
using Serilog;

namespace CobraDataServer
{
 
    partial class ViewModelMain : ViewModelBase
    {
        public static event Action<string> winadd;
        public static event Action<string> winerr;


        public ViewModelMain()
        {
    
            if (data.onestart) return;
            data.onestart = true;
            ini_command();
            
            CreateTimer1(500);

           // var p = new Pipe("SiZ7");
           
        }


        private static bool threads_on = false;
        static bool loc = false;
        public static void timer()
        {
            if (loc) return;
            loc= true;
            if (threadprocess.exit) return;


            //mydb.item.CREATEtest();//////////////////////////////
            //return;

            if (!threads_on)
            {
                threads_on = true;
                threadprocess.create();
            }

            if (data.fatal)
            {
                mes.errLOG("МЕНЕДЖЕР - Фатал. остановка quik.Stop()");
                try
                {
                    // создаем новый поток для стоп (для исключения вылета)
                    Thread myThread = new Thread(new ThreadStart(QUIKSTOP));
                    myThread.Start();
                    Thread.Sleep(5000);
                }
                catch (Exception ex)
                {
                    mes.errLOG("МЕНЕДЖЕР - Фатал. остановка quik.Stop() err="+ex.Message);

                }

  
                data.fatal_need_rst_task = true;
                mes.errLOG("МЕНЕДЖЕР -Остановка всех процессов-");
                threadprocess.stop_all();

                loctask = false;
                
                data.fatal = false;
                data.fatal_need_rst_task = false;
                mes.errLOG("МЕНЕДЖЕР - ЗАПУСК всех процессов...");
                threadprocess.create();
            }

            loc = false;
        }


        static void QUIKSTOP()
        {
            data.quik.Stop();
        }

        static bool loctask = false;
        //static byte rst_not_connect = 0;
        static bool PIPE_ok = false;
        public static void CREATE_PIPE()
        {
           
            if (data.PIPEENABLE)
            {
                data.PIPEENABLE = false;
                foreach (var i in data._instr)
                {
                   data.listpipe.Add(new Pipe(i.name));
                    Thread.Sleep(100);
                }
                mes.add("== Все PIPE подключены успешно  ==");
                PIPE_ok = true;
                data.PIPEENABLE = true;
            }
        }
    /// <summary>
    /// 
    /// </summary>
    public static void task1_release(CancellationToken cts)
    {
          
            if (loctask) { err("отмена запуска задачи - уже выполняется"); return; }
            loctask = true;

            if (data.quik == null) data.quik = new QUIKSHARPconnector();

            while (true)
            {
                add("Начало выполнения task1");
                if (data.fatal_need_rst_task || cts.IsCancellationRequested) break;
                Thread.Sleep(500);


                    if (!data.quik.Connect(data._instr))
                    {
                        err("Connect НЕУДАЧЕН пауза ...");
                        Thread.Sleep(2500); continue;
                    }

                if (!PIPE_ok) CREATE_PIPE();

                 Thread.Sleep(2000);
                  if (data.fatal) { mes.errLOG("фатал. выход из задачи task11"); break; }

                    mes.addLOG("Запуск главного цикла");
                    data.quik.work();//main cycle
                    mes.addLOG(" Главный цикл остановлен");

  
                add("QuikSharp stop");

                // создаем новый поток для стоп (для исключения вылета)
                Thread myThread = new Thread(new ThreadStart(QUIKSTOP));
                myThread.Start();
                Thread.Sleep(5000);
            }

            loctask = false;
            mes.errLOG("Завершение главного потока task1");
            data.fatal_need_rst_task = false;//task закончена
            //  quik = null;          
            Thread.Sleep(3000);
        }


        public static void task2_release(CancellationToken cts)
        {
            try
            {
                while (data.quik == null) Thread.Sleep(500);
                add("Начало выполнения task2");
                while (true)
                {
                    if (cts.IsCancellationRequested) break;
                    data.quik.getAll();

                }
                mes.errLOG("Завершение главного потока task2");
            }
            catch (Exception ex)
            {
                mes.errLOG("task2ERROR "+ex);
                data.fatal = true;
            }
        }




        static void add(string s)
        {
            winadd?.Invoke(s);
        }

        static void err(string s)
        {
            winerr?.Invoke(s);
        }
    }//class


    /// <summary>
    /// СООБЩЕНИЯ
    /// </summary>
    static class mes
    {
        public static event Action<string, object> Event_Print;

        public static void addLOG(string s)
        {
            Event_Print?.Invoke(s, System.Windows.Media.Brushes.Green);
            Log.Debug(s);
        }
        public static void errLOG(string s)
        {
            Event_Print?.Invoke(s, System.Windows.Media.Brushes.Red);
            Log.Debug(s);
        }
        public static void LOG(string s)
        {
            Log.Debug(s);
        }
        public static void add(string s)
        {
            Event_Print?.Invoke(s, System.Windows.Media.Brushes.Green);
        }

        public static void err(string s)
        {
            Event_Print?.Invoke(s, System.Windows.Media.Brushes.Red);
        }

    }



}//namespace
