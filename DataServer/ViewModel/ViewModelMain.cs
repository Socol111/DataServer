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
        public static event Action stopprogramm;
        public static event Action<string> winadd;
        public static event Action<string> winerr;


        public ViewModelMain()
        {
         
          // return;
            
            if (data.onestart) return;
            data.onestart = true;
            ini_command();

            bool existed;
            // получаем GIUD приложения
            string guid = Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly()).ToString();

            Mutex mutexObj = new Mutex(true, guid, out existed);

            if (!existed)
            {
                stopprogramm?.Invoke();////ДУБЛИКАТ ПРОГРАММЫ
                return;
            }
            
            CreateTimer1(500);
            data.getTickers();

           // var p = new Pipe("SiZ7");
           
        }


        private static bool threads_on = false;
        static bool loc = false;
        public static void timer()
        {
            if (loc) return;
            loc= true;

            //mydb.item.CREATEtest();//////////////////////////////
            //return;
            if (!threads_on)
            {
                threads_on = true;
                create();  
            }

            if (data.fatal)
            {
                err("Фатал. остановка QuikSharp");
                try
                {
                    data.quik.Stop();
                    
                }
                catch
                { }

  
                data.fatal_need_rst_task = true;
                err("Фатал. прерывание задачи");

                mt.Abort(5000);
                while (mt.ThreadState == System.Threading.ThreadState.Running)
                { Thread.Sleep(500); err("задача прерывается...."); }

                loctask = false;
                thread_start = false;
                err("Фатал. задача успешно прервана");

                data.fatal = false;
   

                add("Создание задачи из таймера");
                create();
            }

            loc = false;
        }

        public static CancellationTokenSource cts1;
        public static CancellationToken cancellationToken;
        public static Thread mt, pipeTHREAD;
        static bool thread_start = false;

      
        public static void create()
        {
            if (thread_start) { err("Создание задачи - отмена уже создана "); return; }
            thread_start = true;

            data.fatal_need_rst_task = false;
            data.fatal = false;

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

            add("Запуск задачи");
            mt.Start();



            PipeWork _pip = new PipeWork();

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            pipeTHREAD = new Thread(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    _pip.Transmit();
                    tcs.SetResult("ok");

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

            if (data.pipe_enable)
            {
                pipeTHREAD.Name = "PIPE THREAD";

                add("Запуск PIPE потока");
                pipeTHREAD.Start();
            }

        }




        static bool loctask = false;
        static byte rst_not_connect = 0;
        static bool PIPE_ok = false;
        public static void CREATE_PIPE()
        {
           
            if (data.pipe_enable)
            {
  
                foreach (var i in data._instr)
                {
                   data.listpipe.Add(new Pipe(i.name));
                }
                mes.add("== Все PIPE подключены успешно  ==");
                PIPE_ok = true;
        
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
                add("Начало выполнения задачи");
                if (data.fatal_need_rst_task || cts.IsCancellationRequested) break;
                    Thread.Sleep(500);


                    if (!data.quik.Connect(data._instr))
                    {
                        err("Connect НЕУДАЧЕН пауза ...");
                        Thread.Sleep(2500); continue;
                    }

                if (!PIPE_ok) CREATE_PIPE();

                if (data.Not_connect)
                {
                    rst_not_connect++;
                    if (rst_not_connect > 5)
                    {
                        err("фатал. нет подключения"); break;
                    }

                }
                else rst_not_connect = 0;

                 Thread.Sleep(2000);
                  if (data.fatal) { err("фатал. выход из задачи"); break; }

                    add("Запуск главного цикла");
                    data.quik.work();//main cycle
                    add(" главный цикл остановлен");

  
                add("QuikSharp stop");
                data.quik.Stop();
               
            }

            loctask = false;
            err("Фатальный останов пауза 3сек...");
             data.fatal_need_rst_task = false;//task закончена
              //  quik = null;
          
                Thread.Sleep(3000);
                err("завершение задачи");
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
