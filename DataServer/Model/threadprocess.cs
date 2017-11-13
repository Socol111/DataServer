using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CobraDataServer
{
    static class threadprocess
    {
        public static Thread mt, pipeTHREAD, dbTHREAD;
      //  public static bool pipe_enable = true;
        public static bool exit = false;

        public static CancellationTokenSource cts1;
        public static CancellationToken cancellationToken;

        static bool thread_start = false;

        public static void create()
        {
            if (thread_start) { mes.err("Создание задачи - отмена уже создана "); return; }

            thread_start = true;
            data.fatal_need_rst_task = false;
            data.fatal = false;

            cts1 = new CancellationTokenSource();
            cancellationToken = cts1.Token;//для task1

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            threadprocess.mt = new Thread(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    ViewModelMain.task1_release(cancellationToken);//main cycle
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
            threadprocess.mt.Name = "QUIKSHARP THREAD";

            mes.add("Запуск главного потока task1"); threadprocess.mt.Start();



            PipeWork _pip = new PipeWork();

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            threadprocess.pipeTHREAD = new Thread(() =>
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

            if (data.PIPEENABLE)
            {
                threadprocess.pipeTHREAD.Name = "PIPE THREAD";

                mes.add("Запуск PIPE потока");
                threadprocess.pipeTHREAD.Start();
            }





            Db_work _db = new Db_work();

            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            threadprocess.dbTHREAD = new Thread(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    _db.work();
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
            threadprocess.dbTHREAD.Name = "DATABASE THREAD";

            mes.add("Запуск DATABASE потока");
            threadprocess.dbTHREAD.Start();

        }




        public static void stop_all()
        {
            mes.errLOG("stop ALL");
            threadprocess.exit = true;
            Thread.Sleep(100);

            threadprocess.mt.Abort(5000);
            while (threadprocess.mt.ThreadState == System.Threading.ThreadState.Running)
            {
                Thread.Sleep(100);
                mes.errLOG("задача прерывается....");
            }

            threadprocess.pipeTHREAD.Abort(5000);
            while (threadprocess.pipeTHREAD.ThreadState == System.Threading.ThreadState.Running)
            {
                Thread.Sleep(100);
                mes.errLOG("задача PIPE прерывается....");
            }


            threadprocess.dbTHREAD.Abort(5000);
            while (threadprocess.dbTHREAD.ThreadState == System.Threading.ThreadState.Running)
            {
                Thread.Sleep(100);
                mes.errLOG("задача PIPE прерывается....");
            }

            thread_start = false;
            threadprocess.exit = false;
        }

    }
}
