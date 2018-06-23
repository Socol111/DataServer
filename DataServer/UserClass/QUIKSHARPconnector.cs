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
        OrderBook ob;
        AllTrade tb;
        //List<Candle> toolCandles;
        //List<Order> listOrders;
        //List<Trade> listTrades;
        //List<DepoLimitEx> listDepoLimits;
        //List<PortfolioInfoEx> listPortfolio;
        //List<MoneyLimit> listMoneyLimits;
        //List<MoneyLimitEx> listMoneyLimitsEx;
        //   Order order;

       

        static ConcurrentQueue<OrderBook> FIFOorderbook = new ConcurrentQueue<OrderBook>();
        static ConcurrentQueue<AllTrade> FIFOtrade = new ConcurrentQueue<AllTrade>();

        public static ConcurrentQueue<OrderBook> FIFOorderbookall = new ConcurrentQueue<OrderBook>();
        public static ConcurrentQueue<AllTrade> FIFOtradeall = new ConcurrentQueue<AllTrade>();


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
           
        }
        public void work()
        {
            data.Not_connect = true;
            bool nofind = false;
            if (data._instr.Count==0) mes.err("НЕ ЗАДАНЫ ТИЕКРЫ!!!");
            mes.add("Подписки...");
            if (_quik == null) mes.err("err quik");
            var listFN = new List<string>();

            foreach (var i in data._instr)
            {
                if (data.eliminate.Contains(i.tickerCOD)) {mes.add("Игнор подписки " + i.tickerCOD); continue;}

                string fn= Sub(i.tickerCOD, i.Class.Replace("@", ""));
                if (fn != "") listFN.Add(fn);
                else
                {
                    mes.err("Не найден " + i.tickerCOD);
                    nofind = true;
                }
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

            if (!nofind) mes.add("== Подписка на события всех сделок выполнена ==");
            else return;

            data.Not_connect = false;
            data.Not_data = false;
            int notpipe = 0;
            //****************************************************************************************
            //                          MAIN
            //****************************************************************************************
            while (true)//main cycle
            {
                Thread.Sleep(1000);

                if (data.need_rst)
                {
                    Rst();
                    mes.errLOG("Перезапуск сервисов QuikSharp");
                    data.need_rst = false;
                }

                if (data.Not_data)
                {
                    mes.errLOG("НЕТ ДАННЫХ.");
                    if (FIFOorderbookall.Count == 0 && FIFOtradeall.Count == 0)//дописываем данные в базу
                    {
                        if (data.fatal)
                        {
                            mes.errLOG(" fatal. Главный цикл остановлен");
                            return;
                        }
                        mes.errLOG("НЕТ ДАННЫХ,  reconnect...");
                        return;
                    }
                
                }

                if (data.PIPEENABLE)
                {
                    if (data.crashpipe) notpipe++;
                    else notpipe = 0;

                    if (data.crashpipe && notpipe > data.PIPEtimeout)
                    {
                        notpipe = 0;
                        mes.errLOG("поток PIPE не отвечает. последний удачный transmit = " + data.crashpipeINFO);
                        threadprocess.PIPE_Thread_restart();
                    }
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
            data._instr = p;
   
            try
            {

                if (_quik == null)
                {
                    int port = data.portLUA;//34130
                    mes.add("инициализация QuikSharp... порт "+port.ToString());
                    if (data.fatal_need_rst_task) { mes.add("прерывание"); return false; }

                    _quik = new Quik(port, new InMemoryStorage());

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
                Thread.Sleep(3000); return false;
            }

            mes.add("Connect выполнен");
            return true;
        }


        public void Stop()
        {
            data.Not_connect = true;
            try
            {
                if (_quik != null)
                {
                    mes.add("запуск отмены подписок ... ");
                     _quik.Events.OnAllTrade -= ALLTRADE;

                    foreach (var i in data._instr)
                    {
                        DeSub(i.tickerCOD, i.Class.Replace("@", ""));
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

                //mes.add("Определяем код класса инструмента " + secCode + "@" +classCode);
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
                    //mes.add(secCode + "@" + classCode+" найден");
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
                        ob = new OrderBook();

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

            data.Not_data = true;//необходимо переподключение

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
                    foreach (var sym in mydb.listtickers)
                    {
                        if (sym == quote.sec_code)
                        {
                            data.ct_global++;
                            FIFOorderbookall.Enqueue(quote);
                            if (FIFOorderbookall.Count == 500000) mes.errLOG("Переполнение буфера OrderBook");
                            break;
                        }
                    }
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
                foreach (var sym in mydb.listtickers)
                {
                    if (sym == t.SecCode)
                    {
                        FIFOtradeall.Enqueue(t);
                        if (FIFOtradeall.Count == 300000) mes.errLOG("Переполнение буфера данных сделок");
                        break;
                    }
                }

            }
        }

        private Order orderwr;
        private Trade tradewr;
        private double bid, ask;
        private int szb, sza;
        void Analiz_Quote(OrderBook quote)
        {
            short ct = -1;
            foreach (var i in data._instr)
            {
                ob = quote;
                ct++;
                if (ob.sec_code == i.tickerCOD)/*&& quote.class_code == tool.ClassCode*/
                {
                    data._instr[ct].orders++;
                        
                    try
                    {
                            szb = ob.bid.Count();
                            sza = ob.offer.Count();
                            orderwr = new Order()
                            {
                                TickerId = ct,
                                time = GetNormalTime(ob.LuaTimeStamp), 
                                NAMETEST = ob.sec_code,
                            };
                            data.servertime = orderwr.time.ToLongTimeString();

                            if (szb >= 2)
                            {
                                orderwr.bid1 = ob.bid[szb - 1].price;
                                orderwr.vb1 = ob.bid[szb - 1].quantity;
                            }
                            if (szb >= 3)
                            {
                                orderwr.bid2 = ob.bid[szb - 2].price;
                                orderwr.vb2 = ob.bid[szb - 2].quantity;
                            }
                            if (szb >= 4)
                            {
                                orderwr.bid3 = ob.bid[szb - 3].price;
                                orderwr.vb3 = ob.bid[szb - 3].quantity;
                            }
                            if (szb >= 5)
                            {
                                orderwr.bid4 = ob.bid[szb - 4].price;
                                orderwr.vb4 = ob.bid[szb - 4].quantity;
                            }
                            if (szb >= 6)
                            {
                                orderwr.bid5 = ob.bid[szb - 5].price;
                                orderwr.vb5 = ob.bid[szb - 5].quantity;
                            }
                            if (szb >= 7)
                            {
                                orderwr.bid6 = ob.bid[szb - 6].price;
                                orderwr.vb6 = ob.bid[szb - 6].quantity;
                            }
                            if (szb >= 8)
                            {
                                orderwr.bid7 = ob.bid[szb - 7].price;
                                orderwr.vb7 = ob.bid[szb - 7].quantity;
                            }
                            if (szb >= 9)
                            {
                                orderwr.bid8 = ob.bid[szb - 8].price;
                                orderwr.vb8 = ob.bid[szb - 8].quantity;
                            }
                            if (szb >= 10)
                            {
                                orderwr.bid9 = ob.bid[szb - 9].price;
                                orderwr.vb9 = ob.bid[szb - 9].quantity;
                            }
                            if (szb >= 11)
                            {
                                orderwr.bid10 = ob.bid[szb - 10].price;
                                orderwr.vb10 = ob.bid[szb - 10].quantity;
                            }
                            if (szb >= 12)
                            {
                                orderwr.bid11 = ob.bid[szb - 11].price;
                                orderwr.vb11 = ob.bid[szb - 11].quantity;
                            }
                            if (szb >= 13)
                            {
                                orderwr.bid12 = ob.bid[szb - 12].price;
                                orderwr.vb12 = ob.bid[szb - 12].quantity;
                            }
                            if (szb >= 14)
                            {
                                orderwr.bid13 = ob.bid[szb - 13].price;
                                orderwr.vb13 = ob.bid[szb - 13].quantity;
                            }
                            if (szb >= 15)
                            {
                                orderwr.bid14 = ob.bid[szb - 14].price;
                                orderwr.vb14 = ob.bid[szb - 14].quantity;
                            }
                            if (szb >= 16)
                            {
                                orderwr.bid15 = ob.bid[szb - 15].price;
                                orderwr.vb15 = ob.bid[szb - 15].quantity;
                            }
                            if (szb >= 17)
                            {
                                orderwr.bid16 = ob.bid[szb - 16].price;
                                orderwr.vb16 = ob.bid[szb - 16].quantity;
                            }
                            if (szb >= 18)
                            {
                                orderwr.bid17 = ob.bid[szb - 17].price;
                                orderwr.vb17 = ob.bid[szb - 17].quantity;
                            }
                            if (szb >= 19)
                            {
                                orderwr.bid18 = ob.bid[szb - 18].price;
                                orderwr.vb18 = ob.bid[szb - 18].quantity;
                            }

                        try
                        {
                            if (szb >= 20)
                            {
                                orderwr.bid19 = ob.bid[szb - 19].price;
                                orderwr.vb19 = ob.bid[szb - 19].quantity;
                            }
                            if (szb >= 21)
                            {
                                orderwr.bid20 = ob.bid[szb - 20].price;
                                orderwr.vb20 = ob.bid[szb - 20].quantity;
                            }

                            if (szb >= 22)
                            {
                                orderwr.bid21 = ob.bid[szb - 21].price;
                                orderwr.vb21 = ob.bid[szb - 21].quantity;
                            }
                            if (szb >= 23)
                            {
                                orderwr.bid22 = ob.bid[szb - 22].price;
                                orderwr.vb22 = ob.bid[szb - 22].quantity;
                            }
                            if (szb >= 24)
                            {
                                orderwr.bid23 = ob.bid[szb - 23].price;
                                orderwr.vb23 = ob.bid[szb - 23].quantity;
                            }
                            if (szb >= 25)
                            {
                                orderwr.bid24 = ob.bid[szb - 24].price;
                                orderwr.vb24 = ob.bid[szb - 24].quantity;
                            }
                            if (szb >= 26)
                            {
                                orderwr.bid25 = ob.bid[szb - 25].price;
                                orderwr.vb25 = ob.bid[szb - 25].quantity;
                            }
                            if (szb >= 27)
                            {
                                orderwr.bid26 = ob.bid[szb - 26].price;
                                orderwr.vb26 = ob.bid[szb - 26].quantity;
                            }
                            if (szb >= 28)
                            {
                                orderwr.bid27 = ob.bid[szb - 27].price;
                                orderwr.vb27 = ob.bid[szb - 27].quantity;
                            }
                            if (szb >= 29)
                            {
                                orderwr.bid28 = ob.bid[szb - 28].price;
                                orderwr.vb28 = ob.bid[szb - 28].quantity;
                            }
                            if (szb >= 30)
                            {
                                orderwr.bid29 = ob.bid[szb - 29].price;
                                orderwr.vb29 = ob.bid[szb - 29].quantity;
                            }
                            if (szb >= 31)
                            {
                                orderwr.bid30 = ob.bid[szb - 30].price;
                                orderwr.vb30 = ob.bid[szb - 30].quantity;
                            }

                            if (szb >= 32)
                            {
                                orderwr.bid31 = ob.bid[szb - 31].price;
                                orderwr.vb31 = ob.bid[szb - 31].quantity;
                            }
                            if (szb >= 33)
                            {
                                orderwr.bid32 = ob.bid[szb - 32].price;
                                orderwr.vb32 = ob.bid[szb - 32].quantity;
                            }
                            if (szb >= 34)
                            {
                                orderwr.bid33 = ob.bid[szb - 33].price;
                                orderwr.vb33 = ob.bid[szb - 33].quantity;
                            }
                            if (szb >= 35)
                            {
                                orderwr.bid34 = ob.bid[szb - 34].price;
                                orderwr.vb34 = ob.bid[szb - 34].quantity;
                            }
                            if (szb >= 36)
                            {
                                orderwr.bid35 = ob.bid[szb - 35].price;
                                orderwr.vb35 = ob.bid[szb - 35].quantity;
                            }
                            if (szb >= 37)
                            {
                                orderwr.bid36 = ob.bid[szb - 36].price;
                                orderwr.vb36 = ob.bid[szb - 36].quantity;
                            }
                            if (szb >= 38)
                            {
                                orderwr.bid37 = ob.bid[szb - 37].price;
                                orderwr.vb37 = ob.bid[szb - 37].quantity;
                            }
                            if (szb >= 39)
                            {
                                orderwr.bid38 = ob.bid[szb - 38].price;
                                orderwr.vb38 = ob.bid[szb - 38].quantity;
                            }
                            if (szb >= 40)
                            {
                                orderwr.bid39 = ob.bid[szb - 39].price;
                                orderwr.vb39 = ob.bid[szb - 39].quantity;
                            }

                            if (szb >= 41)
                            {
                                orderwr.bid40 = ob.bid[szb - 40].price;
                                orderwr.vb40 = ob.bid[szb - 40].quantity;
                            }
                            if (szb >= 42)
                            {
                                orderwr.bid41 = ob.bid[szb - 41].price;
                                orderwr.vb41 = ob.bid[szb - 41].quantity;
                            }
                            if (szb >= 43)
                            {
                                orderwr.bid42 = ob.bid[szb - 42].price;
                                orderwr.vb42 = ob.bid[szb - 42].quantity;
                            }
                            if (szb >= 44)
                            {
                                orderwr.bid43 = ob.bid[szb - 43].price;
                                orderwr.vb43 = ob.bid[szb - 43].quantity;
                            }
                            if (szb >= 45)
                            {
                                orderwr.bid44 = ob.bid[szb - 44].price;
                                orderwr.vb44 = ob.bid[szb - 44].quantity;
                            }
                            if (szb >= 46)
                            {
                                orderwr.bid45 = ob.bid[szb - 45].price;
                                orderwr.vb45 = ob.bid[szb - 45].quantity;
                            }
                            if (szb >= 47)
                            {
                                orderwr.bid46 = ob.bid[szb - 46].price;
                                orderwr.vb46 = ob.bid[szb - 46].quantity;
                            }
                            if (szb >= 48)
                            {
                                orderwr.bid47 = ob.bid[szb - 47].price;
                                orderwr.vb47 = ob.bid[szb - 47].quantity;
                            }
                            if (szb >= 49)
                            {
                                orderwr.bid48 = ob.bid[szb - 48].price;
                                orderwr.vb48 = ob.bid[szb - 48].quantity;
                            }
                            if (szb >= 50)
                            {
                                orderwr.bid49 = ob.bid[szb - 49].price;
                                orderwr.vb49 = ob.bid[szb - 49].quantity;
                            }
                            if (szb >= 51)
                            {
                                orderwr.bid50 = ob.bid[szb - 50].price;
                                orderwr.vb50 = ob.bid[szb - 50].quantity;
                            }

                        }
                        catch (Exception ex)
                        {
                            mes.err("Ошибка BID стакана  " + ob.sec_code + " " + ex);
                        }

                        /////ask
                        if (sza >= 1)
                            {
                                orderwr.ask1 = ob.offer[0].price;
                                orderwr.va1 = ob.offer[0].quantity;
                            }
                            if (sza >= 2)
                            {
                                orderwr.ask2 = ob.offer[1].price;
                                orderwr.va2 = ob.offer[1].quantity;
                            }
                            if (sza >= 3)
                            {
                                orderwr.ask3 = ob.offer[2].price;
                                orderwr.va3 = ob.offer[2].quantity;
                            }
                            if (sza >= 4)
                            {
                                orderwr.ask4 = ob.offer[3].price;
                                orderwr.va4 = ob.offer[3].quantity;
                            }
                            if (sza >= 5)
                            {
                                orderwr.ask5 = ob.offer[4].price;
                                orderwr.va5 = ob.offer[4].quantity;
                            }
                            if (sza >= 6)
                            {
                                orderwr.ask6 = ob.offer[5].price;
                                orderwr.va6 = ob.offer[5].quantity;
                            }
                            if (sza >= 7)
                            {
                                orderwr.ask7 = ob.offer[6].price;
                                orderwr.va7 = ob.offer[6].quantity;
                            }
                            if (sza >= 8)
                            {
                                orderwr.ask8 = ob.offer[7].price;
                                orderwr.va8 = ob.offer[7].quantity;
                            }
                            if (sza >= 9)
                            {
                                orderwr.ask9 = ob.offer[8].price;
                                orderwr.va9 = ob.offer[8].quantity;
                            }

                            if (sza >= 10)
                            {
                                orderwr.ask10 = ob.offer[9].price;
                                orderwr.va10 = ob.offer[9].quantity;
                            }
                            if (sza >= 11)
                            {
                                orderwr.ask11 = ob.offer[10].price;
                                orderwr.va11 = ob.offer[10].quantity;
                            }
                            if (sza >= 12)
                            {
                                orderwr.ask12 = ob.offer[11].price;
                                orderwr.va12 = ob.offer[11].quantity;
                            }
                            if (sza >= 13)
                            {
                                orderwr.ask13 = ob.offer[12].price;
                                orderwr.va13 = ob.offer[12].quantity;
                            }
                            if (sza >= 14)
                            {
                                orderwr.ask14 = ob.offer[13].price;
                                orderwr.va14 = ob.offer[13].quantity;
                            }
                            if (sza >= 15)
                            {
                                orderwr.ask15 = ob.offer[14].price;
                                orderwr.va15 = ob.offer[14].quantity;
                            }
                            if (sza >= 16)
                            {
                                orderwr.ask16 = ob.offer[15].price;
                                orderwr.va16 = ob.offer[15].quantity;
                            }
                            if (sza >= 17)
                            {
                                orderwr.ask17 = ob.offer[16].price;
                                orderwr.va17 = ob.offer[16].quantity;
                            }
                            if (sza >= 18)
                            {
                                orderwr.ask18 = ob.offer[17].price;
                                orderwr.va18 = ob.offer[17].quantity;
                            }

                        try
                        {

                            if (sza >= 19)
                            {
                                orderwr.ask19 = ob.offer[18].price;
                                orderwr.va19 = ob.offer[18].quantity;
                            }
                            if (sza >= 20)
                            {
                                orderwr.ask20 = ob.offer[19].price;
                                orderwr.va20 = ob.offer[19].quantity;
                            }

                            if (sza >= 21)
                            {
                                orderwr.ask21 = ob.offer[20].price;
                                orderwr.va21 = ob.offer[20].quantity;
                            }
                            if (sza >= 22)
                            {
                                orderwr.ask22 = ob.offer[21].price;
                                orderwr.va22 = ob.offer[21].quantity;
                            }
                            if (sza >= 23)
                            {
                                orderwr.ask23 = ob.offer[22].price;
                                orderwr.va23 = ob.offer[22].quantity;
                            }
                            if (sza >= 24)
                            {
                                orderwr.ask24 = ob.offer[23].price;
                                orderwr.va24 = ob.offer[23].quantity;
                            }
                            if (sza >= 25)
                            {
                                orderwr.ask25 = ob.offer[24].price;
                                orderwr.va25 = ob.offer[24].quantity;
                            }
                            if (sza >= 26)
                            {
                                orderwr.ask26 = ob.offer[25].price;
                                orderwr.va26 = ob.offer[25].quantity;
                            }
                            if (sza >= 27)
                            {
                                orderwr.ask27 = ob.offer[26].price;
                                orderwr.va27 = ob.offer[26].quantity;
                            }
                            if (sza >= 28)
                            {
                                orderwr.ask28 = ob.offer[27].price;
                                orderwr.va28 = ob.offer[27].quantity;
                            }
                            if (sza >= 29)
                            {
                                orderwr.ask29 = ob.offer[28].price;
                                orderwr.va29 = ob.offer[28].quantity;
                            }
                            if (sza >= 30)
                            {
                                orderwr.ask30 = ob.offer[29].price;
                                orderwr.va30 = ob.offer[29].quantity;
                            }

                            if (sza >= 31)
                            {
                                orderwr.ask31 = ob.offer[30].price;
                                orderwr.va31 = ob.offer[30].quantity;
                            }
                            if (sza >= 32)
                            {
                                orderwr.ask32 = ob.offer[31].price;
                                orderwr.va32 = ob.offer[31].quantity;
                            }
                            if (sza >= 33)
                            {
                                orderwr.ask33 = ob.offer[32].price;
                                orderwr.va33 = ob.offer[32].quantity;
                            }
                            if (sza >= 34)
                            {
                                orderwr.ask34 = ob.offer[33].price;
                                orderwr.va34 = ob.offer[33].quantity;
                            }
                            if (sza >= 35)
                            {
                                orderwr.ask35 = ob.offer[34].price;
                                orderwr.va35 = ob.offer[34].quantity;
                            }
                            if (sza >= 36)
                            {
                                orderwr.ask36 = ob.offer[35].price;
                                orderwr.va36 = ob.offer[35].quantity;
                            }
                            if (sza >= 37)
                            {
                                orderwr.ask37 = ob.offer[36].price;
                                orderwr.va37 = ob.offer[36].quantity;
                            }
                            if (sza >= 38)
                            {
                                orderwr.ask38 = ob.offer[37].price;
                                orderwr.va38 = ob.offer[37].quantity;
                            }
                            if (sza >= 39)
                            {
                                orderwr.ask39 = ob.offer[38].price;
                                orderwr.va39 = ob.offer[38].quantity;
                            }
                            if (sza >= 40)
                            {
                                orderwr.ask40 = ob.offer[39].price;
                                orderwr.va40 = ob.offer[39].quantity;
                            }

                            if (sza >= 41)
                            {
                                orderwr.ask41 = ob.offer[40].price;
                                orderwr.va41 = ob.offer[40].quantity;
                            }
                            if (sza >= 42)
                            {
                                orderwr.ask42 = ob.offer[41].price;
                                orderwr.va42 = ob.offer[41].quantity;
                            }
                            if (sza >= 43)
                            {
                                orderwr.ask43 = ob.offer[42].price;
                                orderwr.va43 = ob.offer[42].quantity;
                            }
                            if (sza >= 44)
                            {
                                orderwr.ask44 = ob.offer[43].price;
                                orderwr.va44 = ob.offer[43].quantity;
                            }
                            if (sza >= 45)
                            {
                                orderwr.ask45 = ob.offer[44].price;
                                orderwr.va45 = ob.offer[44].quantity;
                            }
                            if (sza >= 46)
                            {
                                orderwr.ask46 = ob.offer[45].price;
                                orderwr.va46 = ob.offer[45].quantity;
                            }
                            if (sza >= 47)
                            {
                                orderwr.ask47 = ob.offer[46].price;
                                orderwr.va47 = ob.offer[46].quantity;
                            }
                            if (sza >= 48)
                            {
                                orderwr.ask48 = ob.offer[47].price;
                                orderwr.va48 = ob.offer[47].quantity;
                            }
                            if (sza >= 49)
                            {
                                orderwr.ask49 = ob.offer[48].price;
                                orderwr.va49 = ob.offer[48].quantity;
                            }
                            if (sza >= 50)
                            {
                                orderwr.ask50 = ob.offer[49].price;
                                orderwr.va50 = ob.offer[49].quantity;
                            }

                        }
                        catch (Exception ex)
                        {
                            mes.LOG("Ошибка OFFER стакана  " + ob.sec_code + " " + ex);
                        }


                        if (data._instr[ct].lastorder.vb1 == orderwr.vb1 &&
                            data._instr[ct].lastorder.vb2 == orderwr.vb2 &&
                            data._instr[ct].lastorder.vb3 == orderwr.vb3 &&
                            data._instr[ct].lastorder.vb4 == orderwr.vb4 &&
                            data._instr[ct].lastorder.vb5 == orderwr.vb5 &&
                            data._instr[ct].lastorder.vb6 == orderwr.vb6 &&
                            data._instr[ct].lastorder.vb7 == orderwr.vb7 &&
                            data._instr[ct].lastorder.vb8 == orderwr.vb8 &&
                            data._instr[ct].lastorder.vb9 == orderwr.vb9 &&
                            data._instr[ct].lastorder.vb10 == orderwr.vb10 &&
                            data._instr[ct].lastorder.vb11 == orderwr.vb11 &&
                            data._instr[ct].lastorder.vb12 == orderwr.vb12 &&
                            data._instr[ct].lastorder.vb13 == orderwr.vb13 &&
                            data._instr[ct].lastorder.vb14 == orderwr.vb14 &&
                            data._instr[ct].lastorder.vb15 == orderwr.vb15 &&
                            data._instr[ct].lastorder.vb16 == orderwr.vb16 &&
                            data._instr[ct].lastorder.vb17 == orderwr.vb17 &&
                            data._instr[ct].lastorder.vb18 == orderwr.vb18 &&
                            data._instr[ct].lastorder.vb19 == orderwr.vb19 &&
                            data._instr[ct].lastorder.vb20 == orderwr.vb20 &&
                            data._instr[ct].lastorder.vb21 == orderwr.vb21 &&
                            data._instr[ct].lastorder.vb22 == orderwr.vb22 &&
                            data._instr[ct].lastorder.vb23 == orderwr.vb23 &&
                            data._instr[ct].lastorder.vb24 == orderwr.vb24 &&
                            data._instr[ct].lastorder.vb25 == orderwr.vb25 &&
                            data._instr[ct].lastorder.vb26 == orderwr.vb26 &&
                            data._instr[ct].lastorder.vb27 == orderwr.vb27 &&
                            data._instr[ct].lastorder.vb28 == orderwr.vb28 &&
                            data._instr[ct].lastorder.vb29 == orderwr.vb29 &&
                            data._instr[ct].lastorder.vb30 == orderwr.vb30 &&
                            data._instr[ct].lastorder.vb31 == orderwr.vb31 &&
                            data._instr[ct].lastorder.vb32 == orderwr.vb32 &&
                            data._instr[ct].lastorder.vb33 == orderwr.vb33 &&
                            data._instr[ct].lastorder.vb34 == orderwr.vb34 &&
                            data._instr[ct].lastorder.vb35 == orderwr.vb35 &&
                            data._instr[ct].lastorder.vb36 == orderwr.vb36 &&
                            data._instr[ct].lastorder.vb37 == orderwr.vb37 &&
                            data._instr[ct].lastorder.vb38 == orderwr.vb38 &&
                            data._instr[ct].lastorder.vb39 == orderwr.vb39 &&
                            data._instr[ct].lastorder.vb40 == orderwr.vb40 &&
                            data._instr[ct].lastorder.vb41 == orderwr.vb41 &&
                            data._instr[ct].lastorder.vb42 == orderwr.vb42 &&
                            data._instr[ct].lastorder.vb43 == orderwr.vb43 &&
                            data._instr[ct].lastorder.vb44 == orderwr.vb44 &&
                            data._instr[ct].lastorder.vb45 == orderwr.vb45 &&
                            data._instr[ct].lastorder.vb46 == orderwr.vb46 &&
                            data._instr[ct].lastorder.vb47 == orderwr.vb47 &&
                            data._instr[ct].lastorder.vb48 == orderwr.vb48 &&
                            data._instr[ct].lastorder.vb49 == orderwr.vb49 &&
                            data._instr[ct].lastorder.vb50 == orderwr.vb50 &&


                            data._instr[ct].lastorder.va1 == orderwr.va1 &&
                            data._instr[ct].lastorder.va2 == orderwr.va2 &&
                            data._instr[ct].lastorder.va3 == orderwr.va3 &&
                            data._instr[ct].lastorder.va4 == orderwr.va4 &&
                            data._instr[ct].lastorder.va5 == orderwr.va5 &&
                            data._instr[ct].lastorder.va6 == orderwr.va6 &&
                            data._instr[ct].lastorder.va7 == orderwr.va7 &&
                            data._instr[ct].lastorder.va8 == orderwr.va8 &&
                            data._instr[ct].lastorder.va9 == orderwr.va9 &&
                            data._instr[ct].lastorder.va10 == orderwr.va10 &&
                            data._instr[ct].lastorder.va11 == orderwr.va11 &&
                            data._instr[ct].lastorder.va12 == orderwr.va12 &&
                            data._instr[ct].lastorder.va13 == orderwr.va13 &&
                            data._instr[ct].lastorder.va14 == orderwr.va14 &&
                            data._instr[ct].lastorder.va15 == orderwr.va15 &&
                            data._instr[ct].lastorder.va16 == orderwr.va16 &&
                            data._instr[ct].lastorder.va17 == orderwr.va17 &&
                            data._instr[ct].lastorder.va18 == orderwr.va18 &&
                            data._instr[ct].lastorder.va19 == orderwr.va19 &&
                            data._instr[ct].lastorder.va20 == orderwr.va20 &&
                            data._instr[ct].lastorder.va21 == orderwr.va21 &&
                            data._instr[ct].lastorder.va22 == orderwr.va22 &&
                            data._instr[ct].lastorder.va23 == orderwr.va23 &&
                            data._instr[ct].lastorder.va24 == orderwr.va24 &&
                            data._instr[ct].lastorder.va25 == orderwr.va25 &&
                            data._instr[ct].lastorder.va26 == orderwr.va26 &&
                            data._instr[ct].lastorder.va27 == orderwr.va27 &&
                            data._instr[ct].lastorder.va28 == orderwr.va28 &&
                            data._instr[ct].lastorder.va29 == orderwr.va29 &&
                            data._instr[ct].lastorder.va30 == orderwr.va30 &&
                            data._instr[ct].lastorder.va31 == orderwr.va31 &&
                            data._instr[ct].lastorder.va32 == orderwr.va32 &&
                            data._instr[ct].lastorder.va33 == orderwr.va33 &&
                            data._instr[ct].lastorder.va34 == orderwr.va34 &&
                            data._instr[ct].lastorder.va35 == orderwr.va35 &&
                            data._instr[ct].lastorder.va36 == orderwr.va36 &&
                            data._instr[ct].lastorder.va37 == orderwr.va37 &&
                            data._instr[ct].lastorder.va38 == orderwr.va38 &&
                            data._instr[ct].lastorder.va39 == orderwr.va39 &&
                            data._instr[ct].lastorder.va40 == orderwr.va40 &&
                            data._instr[ct].lastorder.va41 == orderwr.va41 &&
                            data._instr[ct].lastorder.va42 == orderwr.va42 &&
                            data._instr[ct].lastorder.va43 == orderwr.va43 &&
                            data._instr[ct].lastorder.va44 == orderwr.va44 &&
                            data._instr[ct].lastorder.va45 == orderwr.va45 &&
                            data._instr[ct].lastorder.va46 == orderwr.va46 &&
                            data._instr[ct].lastorder.va47 == orderwr.va47 &&
                            data._instr[ct].lastorder.va48 == orderwr.va48 &&
                            data._instr[ct].lastorder.va49 == orderwr.va49 &&
                            data._instr[ct].lastorder.va50 == orderwr.va50 &&

                                data._instr[ct].lastorder.bid1 == orderwr.bid1 &&
                                data._instr[ct].lastorder.bid5 == orderwr.bid5 &&
                                data._instr[ct].lastorder.bid10 == orderwr.bid10 &&
                                data._instr[ct].lastorder.bid20 == orderwr.bid20 &&
                                data._instr[ct].lastorder.ask1 == orderwr.ask1 &&
                                data._instr[ct].lastorder.ask5 == orderwr.ask5 &&
                                data._instr[ct].lastorder.ask10 == orderwr.ask10 &&
                                data._instr[ct].lastorder.ask20 == orderwr.ask20

                            )
                            {
                                //равно
                            }
                            else
                            {
                                if (mydb.enable  && mydb.listtickers.Contains(ob.sec_code))
                                {
                                    mydb.FIFOorderbook.Enqueue(orderwr);
                                        if (mydb.FIFOorderbook.Count == 500000)
                                        {
                                            mes.errLOG("Переполнение буфера данных Orders");
                                        }
                                }
                            }
                     
                            data._instr[ct].lastorder = new Order
                            {
                                vb1 = orderwr.vb1,
                                vb2 = orderwr.vb2,
                                vb3 = orderwr.vb3,
                                vb4 = orderwr.vb4,
                                vb5 = orderwr.vb5,
                                vb6 = orderwr.vb6,
                                vb7 = orderwr.vb7,
                                vb8 = orderwr.vb8,
                                vb9 = orderwr.vb9,
                                vb10 = orderwr.vb10,
                                vb11 = orderwr.vb11,
                                vb12 = orderwr.vb12,
                                vb13 = orderwr.vb13,
                                vb14 = orderwr.vb14,
                                vb15 = orderwr.vb15,
                                vb16 = orderwr.vb16,
                                vb17 = orderwr.vb17,
                                vb18 = orderwr.vb18,
                                vb19 = orderwr.vb19,
                                vb20 = orderwr.vb20,
                                vb21 = orderwr.vb21,
                                vb22 = orderwr.vb22,
                                vb23 = orderwr.vb23,
                                vb24 = orderwr.vb24,
                                vb25 = orderwr.vb25,
                                vb26 = orderwr.vb26,
                                vb27 = orderwr.vb27,
                                vb28 = orderwr.vb28,
                                vb29 = orderwr.vb29,
                                vb30 = orderwr.vb30,
                                vb31 = orderwr.vb31,
                                vb32 = orderwr.vb32,
                                vb33 = orderwr.vb33,
                                vb34 = orderwr.vb34,
                                vb35 = orderwr.vb35,
                                vb36 = orderwr.vb36,
                                vb37 = orderwr.vb37,
                                vb38 = orderwr.vb38,
                                vb39 = orderwr.vb39,
                                vb40 = orderwr.vb40,
                                vb41 = orderwr.vb41,
                                vb42 = orderwr.vb42,
                                vb43 = orderwr.vb43,
                                vb44 = orderwr.vb44,
                                vb45 = orderwr.vb45,
                                vb46 = orderwr.vb46,
                                vb47 = orderwr.vb47,
                                vb48 = orderwr.vb48,
                                vb49 = orderwr.vb49,
                                vb50 = orderwr.vb50,

                                va1 = orderwr.va1,
                                va2 = orderwr.va2,
                                va3 = orderwr.va3,
                                va4 = orderwr.va4,
                                va5 = orderwr.va5,
                                va6 = orderwr.va6,
                                va7 = orderwr.va7,
                                va8 = orderwr.va8,
                                va9 = orderwr.va9,
                                va10 = orderwr.va10,
                                va11 = orderwr.va11,
                                va12 = orderwr.va12,
                                va13 = orderwr.va13,
                                va14 = orderwr.va14,
                                va15 = orderwr.va15,
                                va16 = orderwr.va16,
                                va17 = orderwr.va17,
                                va18 = orderwr.va18,
                                va19 = orderwr.va19,
                                va20 = orderwr.va20,
                                va21 = orderwr.va21,
                                va22 = orderwr.va22,
                                va23 = orderwr.va23,
                                va24 = orderwr.va24,
                                va25 = orderwr.va25,
                                va26 = orderwr.va26,
                                va27 = orderwr.va27,
                                va28 = orderwr.va28,
                                va29 = orderwr.va29,
                                va30 = orderwr.va30,
                                va31 = orderwr.va31,
                                va32 = orderwr.va32,
                                va33 = orderwr.va33,
                                va34 = orderwr.va34,
                                va35 = orderwr.va35,
                                va36 = orderwr.va36,
                                va37 = orderwr.va37,
                                va38 = orderwr.va38,
                                va39 = orderwr.va39,
                                va40 = orderwr.va40,
                                va41 = orderwr.va41,
                                va42 = orderwr.va42,
                                va43 = orderwr.va43,
                                va44 = orderwr.va44,
                                va45 = orderwr.va45,
                                va46 = orderwr.va46,
                                va47 = orderwr.va47,
                                va48 = orderwr.va48,
                                va49 = orderwr.va49,
                                va50 = orderwr.va50,


                                bid1 = orderwr.bid1,
                                bid5 = orderwr.bid5,
                                bid10 = orderwr.bid10,
                                bid20 = orderwr.bid20,

                                ask1 = orderwr.ask1,
                                ask5 = orderwr.ask5,
                                ask10 = orderwr.ask10,
                                ask20 = orderwr.ask20,
                            };

                        //------- to PIPE
                        if (data.PIPEENABLE && szb >= 3 )
                        {
                            if (ob.bid[szb - 1] != null)
                            {
                                bid = ob.bid[szb - 1].price;
                                ask = ob.offer[0].price;

                                if (ask == data._instr[ct].lastPIPEask &&
                                    bid == data._instr[ct].lastPIPEbid)
                                {
                                }
                                else
                                {
                                    data.pipeque.Enqueue(new PipeItem()
                                    {
                                        tickerCOD = ob.sec_code,
                                        biditem = bid,
                                        askitem = ask
                                    });

                                    data._instr[ct].lastPIPEbid = bid;
                                    data._instr[ct].lastPIPEask = ask;
                                }
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        mes.LOG("Ошибка в потоке Orders  "+ob.sec_code+" "+ ex);
                    }
                    break;
                }
            }

        }

        bool napr = false;
        //private DateTime dtconv;
        void Analiz_Trade(AllTrade _trade)
        {
            try
            { 
                short ct = -1;
                tb = _trade;
                foreach (var i in data._instr)
                {
                    ct++;              
                    if (tb.SecCode == i.tickerCOD)/*&& quote.class_code == tool.ClassCode*/
                    {
                        data._instr[ct].interes = tb.OpenInterest;
                        //dtconv = new DateTime
                        //(
                        //    tb.Datetime.year,
                        //    tb.Datetime.month,
                        //    tb.Datetime.day,
                        //    tb.Datetime.hour,
                        //    tb.Datetime.min,
                        //    tb.Datetime.sec,
                        //    tb.Datetime.mcs / 1000
                        //);
                       
                        if ((byte)tb.Flags == 1) napr = false;
                                                             else napr = true;
                        tradewr = new Trade()
                        {                      
                            TickerId = ct,
                            time = GetNormalTime(tb.LuaTimeStamp),
                            NAMETEST = tb.SecCode,
                            price = tb.Price,
                            qty = tb.Qty,
                            openinter = tb.OpenInterest,
                            periodsession = tb.Period,
                            buy = napr
                        };
                   
                        if (mydb.enable)
                        {
                            mydb.FIFOtrade.Enqueue(tradewr);
                            if (mydb.FIFOtrade.Count == 50000)
                            {
                                mes.errLOG("Переполнение буфера данных Trades");
                            }
                        }
                        break;
                    }

                }
            }
            catch (Exception ex) { mes.err("err trades event " + ex.Message); }
        }


        //OrderBook order;
        //AllTrade trade;
        OrderBook orderALL;
        AllTrade tradeALL;


        
        // An action to consume the ConcurrentQueue.
        Action act_getdata = () =>
        {
    
        };

        private DateTime normtime;
        DateTime GetNormalTime(long time)
        {
            normtime = new DateTime(1970, 1, 1, data.correct_time, 0, 0).AddMilliseconds(time);
            return normtime;
        }


        bool correct = false;
        /// <summary>
        /// Чтение промежуточного буффера чтобы QuikSharp Callback не затягивался
        /// </summary>
       public void getAll()
        {
            if (FIFOorderbookall.Count == 0 && FIFOtradeall.Count == 0// && FIFOorderbook.Count == 0
                 /* && FIFOtrade.Count == 0*/) Thread.Sleep(200);


            if (FIFOorderbookall.Count != 0)
            {
                FIFOorderbookall.TryDequeue(out orderALL);
                if (orderALL != null)
                {
                    try
                    {
                        if (orderALL.bid.Count() != 0 && orderALL.offer.Count() != 0) correct = true;
                    }
                    catch
                    {
                        correct = false;
                    }

                    if (correct) Analiz_Quote(orderALL);
                }
            }
            if (FIFOtradeall.Count != 0)
            {
                FIFOtradeall.TryDequeue(out tradeALL);
                if (tradeALL != null)
                {
                   Analiz_Trade(tradeALL);
                }
            }

            data.crashdb = false;
        }


    }
}
