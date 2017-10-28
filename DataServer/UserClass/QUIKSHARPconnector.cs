using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuikSharp;
using QuikSharp.DataStructures;
using QuikSharp.DataStructures.Transaction;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using CobraDataServer;
using Serilog;
using System.Collections.Concurrent;
using System.Windows;

namespace CobraDataServer
{
    public class QUIKSHARPconnector
    {
        Quik _quik;
        Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

       
        bool isSubscribedToolOrderBook = false;
        //string classCode = "";
        ///string clientCode;
 
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

       

        static ConcurrentQueue<OrderBook> FIFOorderbook;
        static ConcurrentQueue<AllTrade> FIFOtrade;

        static ConcurrentQueue<OrderBook> FIFOorderbookall;
        static ConcurrentQueue<AllTrade> FIFOtradeall;
  

        public static int getSIZEorderbook
        {
            get { return FIFOorderbookall.Count; }
        }

        public static int getSIZEtrade
        {
            get { return FIFOtradeall.Count; }
        }

      

        public QUIKSHARPconnector()
        {
            FIFOtrade = new ConcurrentQueue<AllTrade>();
            FIFOorderbook = new ConcurrentQueue<OrderBook>();
            FIFOtradeall = new ConcurrentQueue<AllTrade>();
            FIFOorderbookall = new ConcurrentQueue<OrderBook>();

        }
        public void work()
        {   
            mes.add("Подписки...");
            if (_quik == null) mes.err("err quik");
            var listFN = new List<string>();

            foreach (var i in data._instr)
            {
                if (!data.TickerIsOk(i.name)) continue;
                string fn= Sub(i.name, i.Class.Replace("@", ""));
                listFN.Add(fn);
            }

            int ind = 0;
            foreach (var i in listFN)
            {
                data._instr[ind++].namefull = i;
            }

            try
            {
                _quik.Events.OnAllTrade += ALLTRADE;
            }
            catch { mes.err("ОШИБКА Подписки на события всех сделок"); }

            mes.add("== Подписка на события всех сделок выполнена ==");

            data.Not_connect = false;
            data.Not_data = false;

            //****************************************************************************************
            //                          MAIN
            //****************************************************************************************
            while (true)//main cycle
            {            
                getAll();
               
                if (data.fatal) break;

                if (data.need_rst)
                {
                    Rst();
                    data.need_rst = false;
                }

                if (data.Not_data)
                {
                    mes.err(" НЕТ ДАННЫХ,  reconnect...");
                    return;
                }


            }

        }


        // <summary>
        // connect 
        // </summary>
        // <param name = "p" ></ param >
        // < returns ></ returns >
        public bool Connect(List<Instrumensts> p)
        {

            mes.add("Start Connect...");
            // FIFOorderbook = new Queue<OrderBook>();
            data._instr = p;

          
            try
            {

                if (_quik == null)
                {
                    mes.add("инициализация QuikSharp...");
                    if (data.fatal_need_rst_task) { mes.add("прерывание"); return false; }

                    _quik = new Quik(34130, new InMemoryStorage());

                    mes.add("connecting... result =" + _quik.Service.IsConnected().Result);

                    if (data.fatal_need_rst_task) { mes.add("прерывание"); return false; }
                    bool r = _quik.Debug.IsQuik().Wait(6000);
                    if (!r) { mes.add("скрипт не отвечает   рестарт   выход из Connect"); Thread.Sleep(30000); return false; }

                    mes.add("скрипт тест  ping " + _quik.Debug.Ping().Result);


                }

                else
                {
                    mes.add("!!!!перезапуск!!!!  QuikSharp ");
                    _quik.Service.QuikService.Stop();
                    Thread.Sleep(5000);
                    _quik.Service.QuikService.Start();
                }


                //if (connect_ok) add("инициализация QuikSharp соединение c Lua скриптом OK. ");
                //else
                //{
                //    err("ошибка нет соединения  Повтор...");
                //   Thread.Sleep(10000);  return false;
                //}
            }
            catch (Exception ex)
            {
                mes.err("Ошибка инициализации QuikSharp... " + ex.Message);
                mes.add("Повтор соединения");
                Thread.Sleep(3000); return false;
            }


            mes.add("Connect выполнен");
            return true;
        }


        public void Stop()
        {
            try
            {
                if (_quik != null)
                {
                    mes.add("запуск отмены подписок ... ");
                     _quik.Events.OnAllTrade -= ALLTRADE;

                    foreach (var i in data._instr)
                    {
                        DeSub(i.name, i.Class.Replace("@", ""));
                    }

                    mes.add("отмена подписок выполнена ");
                }
                else mes.add("отмена подписок Quik = null");

                if (data.Not_data)
                {
                    try
                    {
                        mes.add("остановка сервисов...");
                        if (_quik != null)
                        {
                            Trace.WriteLine("-- остановка сервисов. start");
                            _quik.Service.QuikService.Stop();

                            _quik.StopService();
                            Trace.WriteLine("-- остановка сервисов. end");
                            mes.add("остановка сервисов выполнена");
                        }
                    }
                    catch (Exception ex) { mes.add("остановка сервисов ошибка " + ex.Message); }
                }
                else mes.add("отмена подписок servis not stop");

                mes.add("*");
                mes.add("Рестарт ...");

                _quik = null;

                Thread.Sleep(5000);
            }

            catch (Exception ex) { mes.err(ex.Message); }
        }
        public void Rst()
        {
            if (_quik != null)
            {
                mes.add("Перезапуск сервисов start");
                _quik.Service.QuikService.Stop();
                _quik.Service.QuikService.Start();
                mes.add("Перезапуск сервисов end");
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


        string  Sub(string secCode, string classCode)
        {
            string fullname = "";
            try
            {
                string rez = "";

                //add("Определяем код класса инструмента " + secCode + "@" +classCode);
                try
                {
                    rez = _quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB", secCode).Result;
                }
                catch
                {
                    mes.err("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно");
                }

                if (rez == classCode)
                {
                    //add(secCode + "@" + classCode+" найден");
                }
                else { mes.err("не найден инструмент " + secCode + "@" + classCode); return ""; }

                //    add("Определяем код клиента..." );
                //    clientCode = _quik.Class.GetClientCode().Result;

                //add("код клиента найден "+ clientCode);


                // add("Создаем экземпляр инструмента " + secCode + "@" + classCode + "   ..." );
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

                        //add("Подписываемся на колбэк 'OnQuote'..." );
                        _quik.Events.OnQuote += OnQuoteDo;
                        mes.add("Подписка на стакан " + tool.Name + " прошла успешно.");
                        fullname = tool.Name;
                        _quik.Events.OnClose += Events_OnClose;
                        _quik.Events.OnCleanUp += Events_OnCleanUp;
                        _quik.Events.OnConnected += Events_OnConnected;
                        _quik.Events.OnConnectedToQuik += Events_OnConnectedToQuik;
                        _quik.Events.OnDisconnected += Events_OnDisconnected;
                        _quik.Events.OnDisconnectedFromQuik += Events_OnDisconnectedFromQuik;
                        _quik.Events.OnStop += Events_OnStop;

                        //timerRenewForm.Enabled = true;
                        //listBoxCommands.SelectedIndex = 0;
                        //listBoxCommands.Enabled = true;
                        //buttonCommandRun.Enabled = true;
                    }
                    else
                    {
                        mes.err("Подписка на стакан " + tool.Name + " не удалась.");
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
                mes.err("Ошибка получения данных по инструменту.");
            }
            return fullname;
        }

        private void Events_OnStop(int signal)
        {
            if (data.Not_connect) return;
            data.Not_connect = true;
            mes.errLOG("скрипт QUIKSHARP остановлен принудительно");
        }

        private void Events_OnDisconnectedFromQuik()
        {
            if (data.Not_connect) return;
            data.Not_connect = true;
            mes.errLOG("скрипт QUIKSHARP - НЕ ЗАПУЩЕН");
        }


        private void Events_OnConnectedToQuik(int port)
        {
            if (!data.Not_connect) return;
            data.Not_connect = false;
            fatalQUIK = false;
            data.first_Not_data = false;
            mes.addLOG("скрипт QUIKSHARP ЗАПУСТИЛСЯ");
        }




        bool fatalQUIKserver = true;
        private void Events_OnConnected()
        {
            if (!fatalQUIKserver) return;
            fatalQUIKserver = false;
            fatalQUIK = false;
            mes.addLOG("QUIK подключен к Серверу брокера");
        }

        private void Events_OnDisconnected()
        {
            if (fatalQUIKserver) return;
            fatalQUIKserver = true;
            mes.errLOG("QUIK отключен от Сервера брокера");
        }
        private void Events_OnCleanUp()
        {
            mes.addLOG("смена сессии QUIK");  
        }

        bool fatalQUIK = false;
        private void Events_OnClose()
        {
            if (fatalQUIK) return;
            fatalQUIK = true;
            mes.errLOG("QUIK ЗАКРЫЛСЯ");
        }

        object lok = new object();
        void OnQuoteDo(OrderBook quote)
        {
             //if (_quik == null) return;
             if (!_quik.Service.QuikService.IsStarted) return;
             if (quote == null) return;

                lock (lok)
                {
                    FIFOorderbookall.Enqueue(quote);
                    if (FIFOorderbookall.Count == 50000) mes.err("Переполнение буфера данных");
                }//lock

        }

        object loktrade = new object();
        void ALLTRADE(AllTrade t)
        {
            // if (_quik == null) return;
            if (!_quik.Service.QuikService.IsStarted) return;
            if (t == null) return;

            lock (loktrade)
            {
                FIFOtradeall.Enqueue(t);
                if (FIFOtradeall.Count == 50000) mes.err("Переполнение буфера данных сделок");
            }
        }

        void Analiz_Quote(OrderBook quote)
        {
            try { 

            short ct = -1;
            foreach (var i in data._instr)
            {
                ct++;
                if (quote.sec_code == i.name)/*&& quote.class_code == tool.ClassCode*/
                {
                    data._instr[ct].orders++;
                    data.ct_global++;
                        
                    try
                    {
                        toolOrderBook = quote;
                            data.servertime = toolOrderBook.server_time;

                        if (toolOrderBook.bid != null && toolOrderBook.offer != null)
                        {
       
                            //--------------- to DB
                            if (mydb.enable)
                            {
                                var ord = new Order()
                                {
                                    Time = DateTime.Now,
                                    Name = toolOrderBook.sec_code+"_"+ toolOrderBook.server_time,

                                    bid1 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid1 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid2 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid2 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid3 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid3 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid4 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid4 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid5 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid5 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid6 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid6 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid7 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid7 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid8 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid8 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid9 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid9 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid10 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid10 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid11 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid11 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid12 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid12 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid13 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid13 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid14 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid14 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid15 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid15 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid16 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid16 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid17 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid17 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid18 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid18 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid19 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid19 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,
                                    bid20 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                    volbid20 = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].quantity,

                                    ask1 = toolOrderBook.offer[0].price,
                                    volask1 = toolOrderBook.offer[0].quantity,
                                    ask2 = toolOrderBook.offer[0].price,
                                    volask2 = toolOrderBook.offer[0].quantity,
                                    ask3 = toolOrderBook.offer[0].price,
                                    volask3 = toolOrderBook.offer[0].quantity,
                                    ask4 = toolOrderBook.offer[0].price,
                                    volask4 = toolOrderBook.offer[0].quantity,
                                    ask5 = toolOrderBook.offer[0].price,
                                    volask5 = toolOrderBook.offer[0].quantity,
                                    ask6 = toolOrderBook.offer[0].price,
                                    volask6 = toolOrderBook.offer[0].quantity,
                                    ask7 = toolOrderBook.offer[0].price,
                                    volask7 = toolOrderBook.offer[0].quantity,
                                    ask8 = toolOrderBook.offer[0].price,
                                    volask8 = toolOrderBook.offer[0].quantity,
                                    ask9 = toolOrderBook.offer[0].price,
                                    volask9 = toolOrderBook.offer[0].quantity,
                                    ask10 = toolOrderBook.offer[0].price,
                                    volask10 = toolOrderBook.offer[0].quantity,
                                    ask11 = toolOrderBook.offer[0].price,
                                    volask11 = toolOrderBook.offer[0].quantity,
                                    ask12 = toolOrderBook.offer[0].price,
                                    volask12 = toolOrderBook.offer[0].quantity,
                                    ask13 = toolOrderBook.offer[0].price,
                                    volask13 = toolOrderBook.offer[0].quantity,
                                    ask14 = toolOrderBook.offer[0].price,
                                    volask14 = toolOrderBook.offer[0].quantity,
                                    ask15 = toolOrderBook.offer[0].price,
                                    volask15 = toolOrderBook.offer[0].quantity,
                                    ask16 = toolOrderBook.offer[0].price,
                                    volask16 = toolOrderBook.offer[0].quantity,
                                    ask17 = toolOrderBook.offer[0].price,
                                    volask17 = toolOrderBook.offer[0].quantity,
                                    ask18 = toolOrderBook.offer[0].price,
                                    volask18 = toolOrderBook.offer[0].quantity,
                                    ask19 = toolOrderBook.offer[0].price,
                                    volask19 = toolOrderBook.offer[0].quantity,
                                    ask20 = toolOrderBook.offer[0].price,
                                    volask20 = toolOrderBook.offer[0].quantity,


                                };

                                mydb.FIFOorderbook.Enqueue(ord);
                                if (mydb.FIFOorderbook.Count == 50000)
                                    mes.err("Переполнение буфера данных DB");

                            }

                                //------- to PIPE
                                if (threadprocess.pipe_enable)
                                {
                                    data.pipeque.Enqueue(new PipeItem()
                                    {
                                      namepipe = quote.sec_code,
                                      biditem = toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price,
                                      askitem = toolOrderBook.offer[0].price

                                    });

                                }
                            }
                    }
                    catch (Exception ex) { mes.err("err orders bidoffer " + ex.Message); }

                    break;
                }
            }

            }
            catch (Exception ex) { mes.err("err orders event " + ex.Message); }
        }

        QuikDateTime tt;
        string tektime;
        void Analiz_Trade(AllTrade t)
        {
            try
            { 
                short ct = -1;
                foreach (var i in data._instr)
                {
                    ct++;              
                    if (t.SecCode == i.name)/*&& quote.class_code == tool.ClassCode*/
                    {
                        data._instr[ct].interes = t.OpenInterest;
                        break;
                    }

                }
            }
            catch (Exception ex) { mes.err("err trades event " + ex.Message); }
        }


        OrderBook order;
        AllTrade trade;
        OrderBook orderALL;
        AllTrade tradeALL;


        // An action to consume the ConcurrentQueue.
        Action act_getdata = () =>
        {

        };


        /// <summary>
        /// Чтение промежуточного буффера чтобы QuikSharp не ожидал
        /// </summary>
        void getAll()
        {
            if (FIFOorderbookall.Count == 0 && FIFOtradeall.Count == 0
                && FIFOorderbook.Count == 0 && FIFOtrade.Count == 0) Thread.Sleep(90);


            if (FIFOorderbookall.Count != 0)
            {
                FIFOorderbookall.TryDequeue(out orderALL);
                Analiz_Quote(orderALL);
            }
            if (FIFOtradeall.Count != 0)
            {
                FIFOtradeall.TryDequeue(out tradeALL);
                Analiz_Trade(tradeALL);
            }

           
        }


    }
}
