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
            if (data._instr.Count==0) mes.err("НЕ ЗАДАНЫ ТИЕКРЫ!!!");
            mes.add("Подписки...");
            if (_quik == null) mes.err("err quik");
            var listFN = new List<string>();

            foreach (var i in data._instr)
            {
                if (data.eliminate.Contains(i.name)) {mes.add("Игнор подписки " + i.name); continue;}
                string fn= Sub(i.name, i.Class.Replace("@", ""));
                if (fn!="") listFN.Add(fn); else mes.err("Не найден " + i.name);
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

        private Order orderwr;
        private Trade tradewr;
        private bool ravno;
        private double bid, ask;
        void Analiz_Quote(OrderBook quote)
        {
            try { 

            short ct = -1;
            foreach (var i in data._instr)
            {
                ob = quote;
                ct++;
                if (ob.sec_code == i.name)/*&& quote.class_code == tool.ClassCode*/
                {
                    data._instr[ct].orders++;
                    data.ct_global++;
                        
                    try
                    {
                        var epoch = new DateTime(1970, 1, 1, 3, 0, 0).AddMilliseconds(ob.LuaTimeStamp);
                        data.servertime = epoch.ToLongTimeString();// epoch.Hour+":"+epoch.Minute+":"+epoch.Second;

                        if (ob.bid != null && ob.offer != null)
                        {
                             ravno = false;
                            //--------------- to DB
                            if (mydb.enable && ob.bid.Count()>15 && ob.offer.Count() > 15)
                            {
                                orderwr = new Order()
                                {
                                    TickerId = ct,
                                    time = epoch, 
                                    NAMETEST = ob.sec_code,
                                    
                                    bid1 = ob.bid[ob.bid.Count() - 1].price,
                                    volbid1 = ob.bid[ob.bid.Count() - 1].quantity,
                                    bid2 = ob.bid[ob.bid.Count() - 2].price,
                                    volbid2 = ob.bid[ob.bid.Count() - 2].quantity,
                                    bid3 = ob.bid[ob.bid.Count() - 3].price,
                                    volbid3 = ob.bid[ob.bid.Count() - 3].quantity,
                                    bid4 = ob.bid[ob.bid.Count() - 4].price,
                                    volbid4 = ob.bid[ob.bid.Count() - 4].quantity,
                                    bid5 = ob.bid[ob.bid.Count() - 5].price,
                                    volbid5 = ob.bid[ob.bid.Count() - 5].quantity,
                                    bid6 = ob.bid[ob.bid.Count() - 6].price,
                                    volbid6 = ob.bid[ob.bid.Count() - 6].quantity,
                                    bid7 = ob.bid[ob.bid.Count() - 7].price,
                                    volbid7 = ob.bid[ob.bid.Count() - 7].quantity,
                                    bid8 = ob.bid[ob.bid.Count() - 8].price,
                                    volbid8 = ob.bid[ob.bid.Count() - 8].quantity,
                                    bid9 = ob.bid[ob.bid.Count() - 9].price,
                                    volbid9 = ob.bid[ob.bid.Count() - 9].quantity,
                                    bid10 = ob.bid[ob.bid.Count() - 10].price,
                                    volbid10 = ob.bid[ob.bid.Count() - 10].quantity,
                                    bid11 = ob.bid[ob.bid.Count() - 11].price,
                                    volbid11 = ob.bid[ob.bid.Count() - 11].quantity,
                                    bid12 = ob.bid[ob.bid.Count() - 12].price,
                                    volbid12 = ob.bid[ob.bid.Count() - 12].quantity,
                                    bid13 = ob.bid[ob.bid.Count() - 13].price,
                                    volbid13 = ob.bid[ob.bid.Count() - 13].quantity,
                                    bid14 = ob.bid[ob.bid.Count() - 14].price,
                                    volbid14 = ob.bid[ob.bid.Count() - 14].quantity,
                                    bid15 = ob.bid[ob.bid.Count() - 15].price,
                                    volbid15 = ob.bid[ob.bid.Count() - 15].quantity,
                                    bid16 = ob.bid[ob.bid.Count() - 16].price,
                                    volbid16 = ob.bid[ob.bid.Count() - 16].quantity,
                                    bid17 = ob.bid[ob.bid.Count() - 17].price,
                                    volbid17 = ob.bid[ob.bid.Count() - 17].quantity,
                                    bid18 = ob.bid[ob.bid.Count() - 18].price,
                                    volbid18 = ob.bid[ob.bid.Count() - 18].quantity,
                                    bid19 = ob.bid[ob.bid.Count() - 19].price,
                                    volbid19 = ob.bid[ob.bid.Count() - 19].quantity,
                                    bid20 = ob.bid[ob.bid.Count() - 20].price,
                                    volbid20 = ob.bid[ob.bid.Count() - 20].quantity,

                                    ask1 = ob.offer[0].price,
                                    volask1 = ob.offer[0].quantity,
                                    ask2 = ob.offer[1].price,
                                    volask2 = ob.offer[1].quantity,
                                    ask3 = ob.offer[2].price,
                                    volask3 = ob.offer[2].quantity,
                                    ask4 = ob.offer[3].price,
                                    volask4 = ob.offer[3].quantity,
                                    ask5 = ob.offer[4].price,
                                    volask5 = ob.offer[4].quantity,
                                    ask6 = ob.offer[5].price,
                                    volask6 = ob.offer[5].quantity,
                                    ask7 = ob.offer[6].price,
                                    volask7 = ob.offer[6].quantity,
                                    ask8 = ob.offer[7].price,
                                    volask8 = ob.offer[7].quantity,
                                    ask9 = ob.offer[8].price,
                                    volask9 = ob.offer[8].quantity,
                                    ask10 = ob.offer[9].price,
                                    volask10 = ob.offer[9].quantity,
                                    ask11 = ob.offer[10].price,
                                    volask11 = ob.offer[10].quantity,
                                    ask12 = ob.offer[11].price,
                                    volask12 = ob.offer[11].quantity,
                                    ask13 = ob.offer[12].price,
                                    volask13 = ob.offer[12].quantity,
                                    ask14 = ob.offer[13].price,
                                    volask14 = ob.offer[13].quantity,
                                    ask15 = ob.offer[14].price,
                                    volask15 = ob.offer[14].quantity,
                                    ask16 = ob.offer[15].price,
                                    volask16 = ob.offer[15].quantity,
                                    ask17 = ob.offer[16].price,
                                    volask17 = ob.offer[16].quantity,
                                    ask18 = ob.offer[17].price,
                                    volask18 = ob.offer[17].quantity,
                                    ask19 = ob.offer[18].price,
                                    volask19 = ob.offer[18].quantity,
                                    ask20 = ob.offer[19].price,
                                    volask20 = ob.offer[19].quantity,


                                };


                                if (data._instr[ct].lastorder.volbid1 == orderwr.volbid1 &&
                                data._instr[ct].lastorder.volbid2 == orderwr.volbid2 &&
                                data._instr[ct].lastorder.volbid3 == orderwr.volbid3 &&
                                data._instr[ct].lastorder.volbid4 == orderwr.volbid4 &&
                                data._instr[ct].lastorder.volbid5 == orderwr.volbid5 &&
                                data._instr[ct].lastorder.volbid6 == orderwr.volbid6 &&
                                data._instr[ct].lastorder.volbid7 == orderwr.volbid7 &&
                                data._instr[ct].lastorder.volbid8 == orderwr.volbid8 &&
                                data._instr[ct].lastorder.volbid9 == orderwr.volbid9 &&
                                data._instr[ct].lastorder.volbid10 == orderwr.volbid10 &&
                                data._instr[ct].lastorder.volbid11 == orderwr.volbid11 &&
                                data._instr[ct].lastorder.volbid12 == orderwr.volbid12 &&
                                data._instr[ct].lastorder.volbid13 == orderwr.volbid13 &&
                                data._instr[ct].lastorder.volbid14 == orderwr.volbid14 &&
                                data._instr[ct].lastorder.volbid15 == orderwr.volbid15 &&
                                data._instr[ct].lastorder.volbid16 == orderwr.volbid16 &&
                                data._instr[ct].lastorder.volbid17 == orderwr.volbid17 &&
                                data._instr[ct].lastorder.volbid18 == orderwr.volbid18 &&
                                data._instr[ct].lastorder.volbid19 == orderwr.volbid19 &&
                                data._instr[ct].lastorder.volbid20 == orderwr.volbid20 &&

                                data._instr[ct].lastorder.volask1 == orderwr.volask1 &&
                                data._instr[ct].lastorder.volask2 == orderwr.volask2 &&
                                data._instr[ct].lastorder.volask3 == orderwr.volask3 &&
                                data._instr[ct].lastorder.volask4 == orderwr.volask4 &&
                                data._instr[ct].lastorder.volask5 == orderwr.volask5 &&
                                data._instr[ct].lastorder.volask6 == orderwr.volask6 &&
                                data._instr[ct].lastorder.volask7 == orderwr.volask7 &&
                                data._instr[ct].lastorder.volask8 == orderwr.volask8 &&
                                data._instr[ct].lastorder.volask9 == orderwr.volask9 &&
                                data._instr[ct].lastorder.volask10 == orderwr.volask10 &&
                                data._instr[ct].lastorder.volask11 == orderwr.volask11 &&
                                data._instr[ct].lastorder.volask12 == orderwr.volask12 &&
                                data._instr[ct].lastorder.volask13 == orderwr.volask13 &&
                                data._instr[ct].lastorder.volask14 == orderwr.volask14 &&
                                data._instr[ct].lastorder.volask15 == orderwr.volask15 &&
                                data._instr[ct].lastorder.volask16 == orderwr.volask16 &&
                                data._instr[ct].lastorder.volask17 == orderwr.volask17 &&
                                data._instr[ct].lastorder.volask18 == orderwr.volask18 &&
                                data._instr[ct].lastorder.volask19 == orderwr.volask19 &&
                                data._instr[ct].lastorder.volask20 == orderwr.volask20 &&

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
                                     ravno = true;//равно
                                }
                                else
                                {
                                    if (mydb.listtickers.Contains(ob.sec_code))
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

                                volbid1 = orderwr.volbid1,
                                volbid2 = orderwr.volbid2,
                                volbid3 = orderwr.volbid3,
                                volbid4 = orderwr.volbid4,
                                volbid5 = orderwr.volbid5,
                                volbid6 = orderwr.volbid6,
                                volbid7 = orderwr.volbid7,
                                volbid8 = orderwr.volbid8,
                                volbid9 = orderwr.volbid9,
                                volbid10 = orderwr.volbid10,
                                volbid11 = orderwr.volbid11,
                                volbid12 = orderwr.volbid12,
                                volbid13 = orderwr.volbid13,
                                volbid14 = orderwr.volbid14,
                                volbid15 = orderwr.volbid15,
                                volbid16 = orderwr.volbid16,
                                volbid17 = orderwr.volbid17,
                                volbid18 = orderwr.volbid18,
                                volbid19 = orderwr.volbid19,
                                volbid20 = orderwr.volbid20,

                                volask1 = orderwr.volask1,
                                volask2 = orderwr.volask2,
                                volask3 = orderwr.volask3,
                                volask4 = orderwr.volask4,
                                volask5 = orderwr.volask5,
                                volask6 = orderwr.volask6,
                                volask7 = orderwr.volask7,
                                volask8 = orderwr.volask8,
                                volask9 = orderwr.volask9,
                                volask10 = orderwr.volask10,
                                volask11 = orderwr.volask11,
                                volask12 = orderwr.volask12,
                                volask13 = orderwr.volask13,
                                volask14 = orderwr.volask14,
                                volask15 = orderwr.volask15,
                                volask16 = orderwr.volask16,
                                volask17 = orderwr.volask17,
                                volask18 = orderwr.volask18,
                                volask19 = orderwr.volask19,
                                volask20 = orderwr.volask20,

                                    bid1 = orderwr.bid1,
                                    bid5 = orderwr.bid5,
                                    bid10 = orderwr.bid10,
                                    bid20 = orderwr.bid20,

                                    ask1 = orderwr.ask1,
                                    ask5 = orderwr.ask5,
                                    ask10 = orderwr.ask10,
                                    ask20 = orderwr.ask20,
                                };

                            }

                            
                                //------- to PIPE
                                if (data.PIPEENABLE)
                                {
                                    if (!mydb.enable)
                                    {
                                        bid = ob.bid[ob.bid.Count() - 1].price;
                                        ask = ob.offer[0].price;
                                        if (ask == data._instr[ct].lastorder.ask1 &&
                                            bid == data._instr[ct].lastorder.bid1

                                        )
                                        {
                                            ravno = true;
                                        }
                                    }

                                    if (!ravno)
                                    {
                                        data.pipeque.Enqueue(new PipeItem()
                                        {
                                            namepipe = ob.sec_code,
                                            biditem = bid,
                                            askitem = ask

                                        });
                                        if (!mydb.enable)
                                        {
                                            data._instr[ct].lastorder.bid1 = bid;
                                            data._instr[ct].lastorder.ask1 = ask;
                                        }
                                    }

                                }
                            }
                    }
                    catch (Exception ex) { mes.err("err orders bidoffer size="+ ob.bid.Count().ToString()+"/"+ ob.offer.Count().ToString()+" err=" + ex.Message); }

                    break;
                }
            }

            }
            catch (Exception ex) { mes.err("err orders event " + ex.Message); }
        }

        private DateTime dtconv;
        void Analiz_Trade(AllTrade _trade)
        {
            try
            { 
                short ct = -1;
                tb = _trade;
                foreach (var i in data._instr)
                {
                    ct++;              
                    if (tb.SecCode == i.name)/*&& quote.class_code == tool.ClassCode*/
                    {
                        data._instr[ct].interes = tb.OpenInterest;
                        try
                        {

                            dtconv = new DateTime
                            (
                                tb.Datetime.year,
                                tb.Datetime.month,
                                tb.Datetime.day,
                                tb.Datetime.hour,
                                tb.Datetime.min,
                                tb.Datetime.sec,
                                tb.Datetime.mcs / 1000
                            );

                        tradewr = new Trade()
                        {                      
                            TickerId = ct,
                            time = dtconv,
                            NAMETEST = tb.SecCode,
                            price = tb.Price,
                            qty = tb.Qty,
                            openinter = tb.OpenInterest,
                        };
                    }
                    catch (Exception ex) { mes.err("err tradesPARSE "+ dtconv.ToLongTimeString()+"."+ dtconv.Millisecond + "  "+ ex.Message); }



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
