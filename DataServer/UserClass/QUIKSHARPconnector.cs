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

        List<Instrumensts> _instr;
        

        static ConcurrentQueue<OrderBook> FIFOorderbook;
        static ConcurrentQueue<AllTrade> FIFOtrade;

  

        public static int getSIZEorderbook
        {
            get { return FIFOorderbook.Count; }
        }

        public static int getSIZEtrade
        {
            get { return FIFOtrade.Count; }
        }

        public List<Instrumensts> GET_Instr { get => _instr; set => _instr = value; }

        public QUIKSHARPconnector()
        {
            FIFOtrade = new ConcurrentQueue<AllTrade>();
            FIFOorderbook = new ConcurrentQueue<OrderBook>();
        }
        public void work()
        {   
            mes.add("Подписки...");
            if (_quik == null) mes.err("err quik");
            var listFN = new List<string>();

            foreach (var i in GET_Instr)
            {
                if (!data.TickerIsOk(i.name)) continue;
                string fn= Sub(i.name, i.Class.Replace("@", ""));
                listFN.Add(fn);
            }

            int ind = 0;
            foreach (var i in listFN)
            {
                _instr[ind++].namefull = i;
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
                if (data.fatal) break;

                getData();

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

        OrderBook order;
        AllTrade trade;

        // An action to consume the ConcurrentQueue.
        Action act_getdata = () =>
        {
           
        };

        void getData()
        {
            // ПАРАЛЕЛЬНОЕ ВЫПОЛНЕННИЕ
            //Parallel.Invoke(act_getdata, act_getdata);

            if (FIFOorderbook.Count == 0 && FIFOtrade.Count == 0) Thread.Sleep(90);

            if (FIFOorderbook.Count != 0) FIFOorderbook.TryDequeue(out order);
            if (FIFOtrade.Count != 0) FIFOtrade.TryDequeue(out trade);
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
            _instr = p;

          
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

                    foreach (var i in _instr)
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
            // if (_quik == null) return;
            // if (!_quik.Service.QuikService.IsStarted) return;

            try
            {


                lock (lok)
                {
                    FIFOorderbook.Enqueue(quote);
                    if (FIFOorderbook.Count == 50000) mes.err("Переполнение буфера данных");

                    short ct = -1;
                    foreach (var i in _instr)
                    {
                        ct++;
                        if (quote.sec_code == i.name)/*&& quote.class_code == tool.ClassCode*/
                        {
                            _instr[ct].orders++;

                            toolOrderBook = quote;
                            bid = Convert.ToDecimal(toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price);
                            offer = Convert.ToDecimal(toolOrderBook.offer[0].price);
                            double volume = toolOrderBook.offer[0].quantity;

                            data.servertime = toolOrderBook.server_time;
                            data.ct_global++;

                            if (data.pipe_enable)
                            {
                                foreach (var p in data.listpipe)
                                {
                                    if (p.Name == i.name)
                                    {
                                        if (i.ask == offer && i.bid == bid) { break; }
                                        else
                                        {
                                            i.ct++;

                                            //if (DateTime.Now.Hour> data.hour_start_pipe)
                                            p.send("tick;" + bid.ToString() + ";" + offer.ToString() + ";", p.Name);
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
            catch (Exception ex) { mes.err("err orders event "+ex.Message); }
        }

        //StringBuilder s1, s2;
        QuikDateTime tt;
        string tektime;
        object loktrade = new object();
        void ALLTRADE(AllTrade t)
        {
            // if (_quik == null) return;
            // if (!_quik.Service.QuikService.IsStarted) return;
          try
          {

            lock (loktrade)
            {
                //FIFOorderbook.Enqueue(quote);
                //if (FIFOorderbook.Count == 50000) err("Переполнение буфера данных");
                short ct = -1;
                foreach (var i in _instr)
                {
                    ct++;
                   if( !data.first_Not_data ) FIFOtrade.Enqueue(t);

                    if (FIFOtrade.Count == 50000) mes.err("Переполнение буфера данных сделок");

                    if (t.SecCode == i.name)/*&& quote.class_code == tool.ClassCode*/
                    {
                        _instr[ct].interes = t.OpenInterest;
                        break;
                    }

                }

              
                

                var cla = t.ClassCode;
                var period = t.Period;
                var code = t.SecCode;
                var size = t.Qty;

                //s1.Insert(0, cla);



                tt = t.Datetime;
                tektime = String.Format("{0}.{1}.{2}  {3}:{4}:{5}",
                   tt.day, tt.month, tt.year, tt.hour, tt.min, tt.sec);

            }

          }
          catch (Exception ex) { mes.err("err trades event " + ex.Message); }
        }



    


    }
}
