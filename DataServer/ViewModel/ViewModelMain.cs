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
using System.Runtime.InteropServices;

namespace project.ViewModel
{
    partial class ViewModelMain : ViewModelBase
    {
        public static QUIKSHARPconnector quik;
        public static List<Instrumensts> _instr;
        public static event Action stopprogramm;
        public static event Action<string> winadd;
        public static event Action<string> winerr;

        public ViewModelMain()
        {
            ini_command();

            bool existed;
            // получаем GIUD приложения
            string guid = Marshal.GetTypeLibGuidForAssembly(System.Reflection.Assembly.GetExecutingAssembly()).ToString();

            Mutex mutexObj = new Mutex(true, guid, out existed);

            if (!existed)
            {
                if (stopprogramm != null) stopprogramm();////ДУБЛИКАТ ПРОГРАММЫ
                return;
            }
            
            CreateTimer1(500);
            _instr = new List<Instrumensts>();
            FilesWork f = new FilesWork("d:/z/zAmerikaFinam/MQL4/Files/CobraConnector/ticker.ini");
            f.ReadListInstrument(_instr);

            add("Создание задачи");
            create();
        }

        static bool loc = false;
        public static void timer()
        {
            if (loc) return;
            loc= true;
            
            if (data.fatal)
            {
                err("Фатал. остановка QuikSharp");
                try
                {
                    quik.Stop();
                    
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
                data.block_new_pipe = false;

                add("Создание задачи из таймера");
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

            err("Запуск задачи");
            mt.Start();
        }

        static bool loctask = false;
        static byte rst_not_connect = 0;
        /// <summary>
        /// 
        /// </summary>
        public static void task1_release(CancellationToken cts)
        {
            if (loctask) { err("отмена запуска задачи - уже выполняется"); return; }
            loctask = true;

            if (quik == null) quik = new QUIKSHARPconnector();

            while (true)
            {
                add("Начало выполнения задачи");
                if (data.fatal_need_rst_task || cts.IsCancellationRequested) break;
                    Thread.Sleep(500);


                    if (!quik.Connect(_instr))
                    {
                        err("Connect НЕУДАЧЕН пауза ...");
                        Thread.Sleep(2500); continue;
                    }
            

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
                    quik.work();//main cycle
                    add(" главный цикл остановлен");

                 data.block_new_pipe = true;
                add("QuikSharp stop");
               quik.Stop();
               
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
            if (winadd != null) winadd(s);
        }

        static void err(string s)
        {
            if (winerr != null) winerr(s);
        }
    }//class

}//namespace
