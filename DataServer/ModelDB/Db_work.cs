using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CobraDataServer.MSSQL;

namespace CobraDataServer
{
    class Db_work
    {
        private Order ord;
        private Trade trd;
        private int j;
        private bool push=false;

        public void work()
        {
            MainWindow.nofinddata += MainWindow_nofinddata;
            try
            {
                mes.add("запущен DATABASE поток");
                if (threadprocess.exit) mes.add("запущен DATABASE поток. ИДЕТ ПРОЦЕСС ПРЕРЫВНИЯ");
                while (true)
                {
                    if (threadprocess.exit) break;
                    if (mydb.enable)
                    {
                        if (mydb.FIFOorderbook.Count == 0 && mydb.FIFOtrade.Count == 0)
                        {
                            push = false; Thread.Sleep(300);
                        }

                        if (mydb.FIFOorderbook.Count > mydb.sizepacket || 
                        ((data.Not_connect || push) && mydb.FIFOorderbook.Count>0))
                        {
                            wro();
                        }
                        else
                        {
                            if (mydb.FIFOtrade.Count==0 && mydb.FIFOtrade.Count == 0) Thread.Sleep(100);
                        }

                        ////TRADES
                        if (mydb.FIFOtrade.Count > mydb.sizepackettrade ||
                            ((data.Not_connect || push) && mydb.FIFOtrade.Count >0))
                        {
                            wrt();
                        }
                        else  Thread.Sleep(100);


                    }
                    else Thread.Sleep(500);
                }
                mes.err("Завершение DATABASE потока");
            }
            catch (Exception ex)
            {
                mes.errLOG("Ошибка потока работы с БАЗОЙ "+ex.Message);
            }


        }

        /// <summary>
        /// нет потока данных дописываем остатки в базу
        /// </summary>
        private void MainWindow_nofinddata()
        {
            if (mydb.FIFOorderbook.Count > 0 || mydb.FIFOorderbook.Count > 0)
            {
                push = true;
            }
        }

        List<Order> entitiesORDER = EntityGenerator_Order.GeneratOrder(5000);
        List<Trade> entitiesTRADE = EntityGenerator_Trade.GeneratOrder(1000);

        void wro()
        {
            entitiesORDER.Clear();
            for (j = 0; j < mydb.sizepacket; j++)
            {
                if (mydb.FIFOorderbook.Count != 0)
                {
                    mydb.FIFOorderbook.TryDequeue(out ord);
                    entitiesORDER.Add(ord);
                }
            }

            mydb.item.WRITE_TO_DB_ORDER(entitiesORDER);
        }

        void wrt()
        {
            if (mydb.FIFOtrade.Count != 0)
            {
                entitiesTRADE.Clear();
                for (j = 0; j < mydb.sizepacket; j++)
                {
                    if (mydb.FIFOtrade.Count != 0)
                    {
                        mydb.FIFOtrade.TryDequeue(out trd);
                        entitiesTRADE.Add(trd);
                    }
                }

                mydb.item.WRITE_TO_DB_TRADE(entitiesTRADE);
            }
        }
       
    }
}
