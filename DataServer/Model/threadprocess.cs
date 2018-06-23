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
        public static Thread mt,write, pipeTHREAD, dbTHREAD;
      //  public static bool pipe_enable = true;
        public static bool exit = false;

        public static CancellationTokenSource cts1;
        public static CancellationToken cancellationToken;

        static bool thread_start = false;

        public static void create()
        {
            if (thread_start) { mes.errLOG("Создание задачи - отмена уже создана "); return; }

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

            mes.addLOG("Запуск главного потока task1"); threadprocess.mt.Start();


            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            threadprocess.write = new Thread(() =>
            {
                var tcs = new TaskCompletionSource<string>();
                try
                {
                    ViewModelMain.task2_release(cancellationToken);
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
            threadprocess.write.Name = "writeDB THREAD";

            mes.addLOG("Запуск потока task2 "); threadprocess.write.Start();

            //start pipe
            create_PIPE();

            //start db
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

            mes.addLOG("Запуск DATABASE потока");
            threadprocess.dbTHREAD.Start();

        }


        public static void create_PIPE()
        {
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

                mes.addLOG("Запуск PIPE потока");
                threadprocess.pipeTHREAD.Start();
            }

        }

        public static  void PIPE_Thread_restart()
        {
            try
            {
                data.crashpipe = false;
                data.PIPEENABLE = false;
                mes.errLOG("PIPE прерывание потока");
                pipeTHREAD.Abort(5000);
                while (threadprocess.pipeTHREAD.ThreadState == System.Threading.ThreadState.Running)
                {
                    Thread.Sleep(100);
                    mes.errLOG("задача PIPE прерывается....");
                }

                mes.errLOG("PIPE поток остановлен");

                data.PIPEENABLE = true;
                create_PIPE();
                data.crashpipe = false;
            }
            catch (Exception ex)
            {
                mes.errLOG("Ошибка остановки PIPE потока "+ex.Message);
            }
        }

        public static void PIPE_all_reconnect()
        {
            try
            {
                if (data.listpipe != null && data.listpipe.Count!=0)
                {
                    mes.errLOG("Остановка всех PIPE...");
                    data.PIPEENABLE = false;
                    foreach (var i in data.listpipe)
                    {
                        mes.err("Остановка pipe-"+i.Name);
                        i.stopPIPE();
                    }

                    mes.addLOG("PIPE рекконект");
                    data.listpipe.Clear();
                    foreach (var i in data._instr)
                    {
                        mes.add("PIPE соединяемcя с "+i.tickerName);
                        data.listpipe.Add(new Pipe(i.tickerName, i.tickerCOD));
                    }
                    mes.add("== Все PIPE пересозданы ==");
                    data.PIPEENABLE = true;
                    data.crashpipe = false;
                }
            }
            catch (Exception ex)
            {
                mes.errLOG("Ошибка PIPE рекконект " + ex.Message);
            }
        }

        public static void stop_all()
        {
            mes.errLOG("stop ALL");
            threadprocess.exit = true;
            thread_start = false;
            Thread.Sleep(100);
            
            threadprocess.mt.Abort(5000);
            while (threadprocess.mt.ThreadState == System.Threading.ThreadState.Running)
            {
                Thread.Sleep(100);
                mes.errLOG("task1 прерывается....");
            }

            threadprocess.write.Abort(5000);
            while (threadprocess.write.ThreadState == System.Threading.ThreadState.Running)
            {
                Thread.Sleep(100);
                mes.errLOG("task2 прерывается....");
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
                mes.errLOG("задача DB прерывается....");
            }

            thread_start = false;
            threadprocess.exit = false;
        }

    }
}
