using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using project.Model;


namespace project.ViewModel
{
    class QUIKSHARPconnector
    {
        static Quik _quik;
        Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

        bool isSubscribedToolOrderBook = false;
        //string classCode = "";
        ///string clientCode;
        decimal bid;
        decimal offer;
        Tool tool;
         OrderBook toolOrderBook;
        //List<Candle> toolCandles;
        //List<Order> listOrders;
        //List<Trade> listTrades;
        //List<DepoLimitEx> listDepoLimits;
        //List<PortfolioInfoEx> listPortfolio;
        //List<MoneyLimit> listMoneyLimits;
        //List<MoneyLimitEx> listMoneyLimitsEx;
        //   Order order;

        List<Instumensts> _instr;
        List<Pipe> _pipe;

        public static event Action<string, object> Event_Print;
        //public static event Action<int, int, int, string> Event_CMD;
        //bool connect_ok = false;
        public QUIKSHARPconnector()
        {
            
        }
        public void work()
        {
            

            if (!data.block_new_pipe && data.pipe_enable)
            {
                foreach (var i in _instr)
                {
                    _pipe.Add(new Pipe(i.name));
                }
            }

            foreach (var i in _instr)
            {
                Sub(i.name, i.Class.Replace("@",""));
            }

            try
            {
                _quik.Events.OnAllTrade += ALLTRADE;
            }
            catch { err("ОШИБКА Подписки на события всех сделок"); }

            add("Подписка на события всех сделок выполнена");


            data.Not_connect = false;
            data.Not_data = false;

    
            while (true)//main cycle
                {

                    if (data.fatal) break;

                    try
                    {
                        Thread.Sleep(1000);

                        if (data.need_rst)
                        {
                            Rst();
                            data.need_rst = false;
                        }

                        if (data.Not_data)
                        {
                            err(" НЕТ ДАННЫХ,  reconnect...");
                            return;
                        }


                      //add("test connect");

                        bool r = _quik.Debug.IsQuik().Wait(2000);
                        if (!r) 
                        {
                            err(" НЕТ Связи с QuikSharp,  reconnect...");
                            data.Not_connect = true;
                            Stop();
                            Thread.Sleep(5000);
                            add("---------------");
                            return;
                         }
                       
                      //add("test connect end");

                }
                    catch(Exception ex)
                    {
                        err(ex.Message);
                    }
            }
    

        }



        bool connect_ok = false;
        /// <summary>
        /// connect
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Connect(List<Instumensts> p)
        {

            add("Start Connect...");
            FIFOorderbook = new Queue<OrderBook>();
            _instr = p;

            if(!data.block_new_pipe && data.pipe_enable) _pipe = new List<Pipe>();


            try
            {

                if (_quik == null)
                {
                    add("инициализация QuikSharp...");
                    if (data.fatal_need_rst_task) { _quik = null; add("прерывание"); return false; }

                    _quik = new Quik(Quik.DefaultPort, new InMemoryStorage());


                    if (data.fatal_need_rst_task) { _quik = null; add("прерывание"); return false; }
                    bool r = _quik.Debug.IsQuik().Wait(2000);
                    if (!r) { add("скрипт не запущен рестарт"); _quik = null; Thread.Sleep(30000); return false; }

                    add("скрипт ping " + _quik.Debug.Ping().Result);

                    add("connecting...");
                    connect_ok = _quik.Service.IsConnected().Result;
                }

                else
                {
                    add("инициализация QuikSharp перезапуск");
                    _quik.Service.QuikService.Stop();
                    Thread.Sleep(1000);
                    _quik.Service.QuikService.Start();
                }


                if (connect_ok) add("инициализация QuikSharp соединение c Lua скриптом OK. ");
                else
                {
                    err("ошибка нет соединения  Повтор...");
                    _quik = null; Thread.Sleep(10000);  return false;
                }
            }
            catch(Exception ex)
            {
                err("Ошибка инициализации QuikSharp... "+ex.Message );
                add("Повтор соединения");
                _quik = null; Thread.Sleep(3000); return false;
            }


            add("Connect выполнен");
            return true;
        }


        public void Stop()
        {
            if (_quik != null)
            {
                add("запуск отмены подписок ... ");
                _quik.Events.OnAllTrade -= ALLTRADE;

                foreach (var i in _instr)
                {
                    DeSub(i.name, i.Class.Replace("@", ""));
                }

                add("отмена подписок выполнена ");
            }
            else add("отмена подписок Quik = null");

            if (data.Not_data)
            {
                try
                {
                    add("остановка сервисов...");
                    if (_quik != null)
                    {
                        _quik.Service.QuikService.Stop();
                        _quik.StopService();
                        add("остановка сервисов выполнена");
                    }
                }
                catch(Exception ex) { add("остановка сервисов ошибка "+ex.Message); }
            }
            else add("отмена подписок servis not stop");

            add("Рестарт ...");
            _quik = null;
        }
        public void Rst()
        {
            if (_quik != null)
            {
                add("Перезапуск сервисов start");
                _quik.Service.QuikService.Stop();
                _quik.Service.QuikService.Start();
                add("Перезапуск сервисов end");
            }
        }

        void DeSub(string secCode, string classCode)
        {
                if (_quik != null)
                {
                  //  _quik.OrderBook.Subscribe(tool.ClassCode, tool.SecurityCode).Wait();
                   _quik.Events.OnQuote -= OnQuoteDo;

                    // _quik.Candles.Subscribe(tool.ClassCode, tool.SecurityCode, CandleInterval.TICK);
                }
            
        }


        void Sub(string secCode, string classCode)
        {
            try
            {
                string rez= "";

                ///add("Определяем код класса инструмента " + secCode + "@" +classCode);
                try
                {
                    rez = _quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB", secCode).Result;
                }
                catch
                {
                    err("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно");
                }

                if (rez == classCode)
                {
                    //add(secCode + "@" + classCode+" найден");
                }
                else { err("не найден инструмент " + secCode + "@" + classCode); return; }

                //    add("Определяем код клиента..." );
                //    clientCode = _quik.Class.GetClientCode().Result;

                //add("код клиента найден "+ clientCode);


              //  add("Создаем экземпляр инструмента " + secCode + "@" + classCode + "   ..." );
                    tool = new Tool(_quik, secCode, classCode);
                    if (tool != null && tool.Name != null && tool.Name != "")
                    {
                        //add("Инструмент " + tool.Name + " создан." );

                        //textBoxAccountID.Text = tool.AccountID;
                        //textBoxFirmID.Text = tool.FirmID;
                        //textBoxShortName.Text = tool.Name;
                        //textBoxLot.Text = Convert.ToString(tool.Lot);
                        //textBoxStep.Text = Convert.ToString(tool.Step);
                        //textBoxGuaranteeProviding.Text = Convert.ToString(tool.GuaranteeProviding);
                        //textBoxLastPrice.Text = Convert.ToString(tool.LastPrice);
                        //textBoxQty.Text = Convert.ToString(GetPositionT2(_quik, tool, clientCode));
                        //add("Подписываемся на стакан..." );

                        _quik.OrderBook.Subscribe(tool.ClassCode, tool.SecurityCode).Wait();
                       // _quik.OrderBook.Subscribe(tool.ClassCode, "SiM7").Wait();


                        _quik.Candles.Subscribe(tool.ClassCode, tool.SecurityCode, CandleInterval.TICK);
                      //  _quik.Candles.Subscribe(tool.ClassCode, "SiM7", CandleInterval.TICK);





                        isSubscribedToolOrderBook = _quik.OrderBook.IsSubscribed(tool.ClassCode, tool.SecurityCode).Result;
                        if (isSubscribedToolOrderBook)
                        {
                            toolOrderBook = new OrderBook();
                            add("Подписка на стакан "+tool.Name + " прошла успешно." );
                            //add("Подписываемся на колбэк 'OnQuote'..." );
                            _quik.Events.OnQuote += OnQuoteDo;

                            //timerRenewForm.Enabled = true;
                            //listBoxCommands.SelectedIndex = 0;
                            //listBoxCommands.Enabled = true;
                            //buttonCommandRun.Enabled = true;
                        }
                        else
                        {
                            err("Подписка на стакан " + tool.Name + " не удалась.");
                            //textBoxBestBid.Text = "-";
                            //textBoxBestOffer.Text = "-";
                            //timerRenewForm.Enabled = false;
                            //listBoxCommands.Enabled = false;
                            //buttonCommandRun.Enabled = false;
                        }


                    }
             

            }
            catch
            {
                err("Ошибка получения данных по инструменту." );
            }
        }

        Queue<OrderBook> FIFOorderbook;
        Queue<AllTrade> FIFOtrade;

        object lok = new object();
        void OnQuoteDo(OrderBook quote)
        {
            if (_quik == null) return;
            if (!_quik.Service.QuikService.IsStarted) return;

            lock (lok)
            {
                //FIFOorderbook.Enqueue(quote);
                //if (FIFOorderbook.Count > 60000) WRITE();

                foreach (var i in _instr)
                {
                    if (quote.sec_code == i.name)/*&& quote.class_code == tool.ClassCode*/
                    {
                        toolOrderBook = quote;
                        bid = Convert.ToDecimal(toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price);
                        offer = Convert.ToDecimal(toolOrderBook.offer[0].price);

                        if (data.pipe_enable)
                        {
                            foreach (var p in _pipe)
                            {
                                if (p.Name == i.name)
                                {
                                    data.ct_global++;
                                    if (i.ask == offer && i.bid == bid) { break; }
                                    else
                                    { 
                                        i.ct++;
                                        p.send("tick;" + bid.ToString() + ";" + offer.ToString() + ";");
                                    }
                                    i.ask = offer; i.bid = bid;
                                }
                            }
                        }
                        break;
                    }
                }
            }//lock
        }

        string tektime = "";

        void ALLTRADE(AllTrade t)
        {
            if (_quik == null) return;
            if (!_quik.Service.QuikService.IsStarted) return;
            // add("---- all trade ---"); 
            if (FIFOtrade == null)
                FIFOtrade = new Queue<AllTrade>();

           // FIFOtrade.Enqueue(t);

          //  if (FIFOtrade.Count > 60000) WRITE();

            QuikDateTime tt = t.Datetime;
            tektime = String.Format("{0}.{1}.{2}  {3}:{4}:{5}", tt.day, tt.month, tt.year, tt.hour, tt.min, tt.sec);


        }



        //public Task<string> AsyncTaskSTART(string url)
        //{
        //    return Task.Run(() =>
        //    {
        //        //----------------
        //        ///q.start();
        //        return "";

        //        //----------------
        //    });
        //}

        //async void RUN(string x)
        //{
        //   // string ss = await AsyncTaskSTART(x);
        //}

















        void add(string s)
        {
            if (Event_Print != null)  Event_Print(DateTime.Now.ToString() + "." + DateTime.Now.Millisecond + "   "+ s, System.Windows.Media.Brushes.Green);
        }

        void err(string s)
        {
            if (Event_Print != null) Event_Print(DateTime.Now.ToString()+ "."+ DateTime.Now.Millisecond +"   " + s, System.Windows.Media.Brushes.Red);
        }




    }
}
