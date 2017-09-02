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

namespace project.ViewModel
{
    partial class ViewModelMain : ViewModelBase
    {
        public static QUIKSHARPconnector quik;
        public static List<Instumensts> _instr;

     

        public ViewModelMain()
        {
            ini_command();
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
                    quik.stop();

                }
                catch
                { }

                data.fatal_need_rst_task = true;
  
                while (data.fatal_need_rst_task)
                { Thread.Sleep(500); }


                data.fatal = false;
                data.block_new_pipe = false;

                create();
            }

            loc = false;
        }
           
      

            public static void create()
            {
                quik = new QUIKSHARPconnector();

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            var task1 = Task.Run(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    //lock (threadLock)
                   // {
                        ViewModelMain.task1_release();
                    //}
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
                return tcs.Task;
            });
            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

        }


        /// <summary>
        /// 
        /// </summary>
        public static void task1_release()
        {
            while (true)
            {
                if (data.fatal) break;
                Thread.Sleep(500);
                if (!quik.Connect(_instr)) { Thread.Sleep(2500); break; }
                Thread.Sleep(5000);
                if (data.fatal) break;
                quik.work();//main cycle

                
                data.block_new_pipe = true;
                quik.stop();
            }
        }



    }//class
}//namespace
