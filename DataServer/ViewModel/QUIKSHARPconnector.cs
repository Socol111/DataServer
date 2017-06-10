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
        bool isServerConnected;
        static Quik _quik;

        Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];


        bool isSubscribedToolOrderBook = false;
        string classCode = "";
        string clientCode;
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



        public static event Delegate_Print Event_Print;
        public static event Delegate_Command Event_CMD;


        public bool start()
        {

   
            Connect();

            Sub("RIM7", "SPBFUT");
            Sub("SiM7", "SPBFUT");



            try
            {
                _quik.Events.OnAllTrade += ALLTRADE;
            }
            catch { add("ОШИБКА Подписки на события всех сделок"); }
            finally { add("Подписываемся на события всех сделок. В"); }
            return true;

        }

        private void Connect()
        {
  
            try
            {
                add("Соединяемся со скриптом QuikSharp..." );
              // _quik = new Quik(Quik.DefaultPort, new InMemoryStorage()); 
           

            }
            catch
            {
                add("Ошибка инициализации QuikSharp...");
            }
            if (_quik != null)
            {
                add("Экземпляр QuikSharp создан.");
                try
                {
                    add("Получаем статус соединения ..." );
                    isServerConnected = _quik.Service.IsConnected().Result;
                    if (isServerConnected)
                    {
                        add("Соединение с сервером установлено." );
                        
                    }
                    else
                    {
                        add("Соединение с сервером НЕ установлено." );

                        while (true)
                        {
                            add("Повтор соединения");
                            Thread.Sleep(3000);

                            isServerConnected = _quik.Service.IsConnected().Result;
                            if (isServerConnected)
                            {
                                add("Соединение с сервером установлено.");
                                break;
                            }
                            else
                            {
                                add("Соединение с сервером НЕ установлено!");

                            }
                        }
                   }
                }
                catch
                {
                    add("Неудачная попытка получить статус соединения с сервером." );
                }
            }
        }



       


        void Sub(string secCode, string classCode)
        {
            
            try
            {

                string rez= "";

                add("Определяем код класса инструмента " + secCode + ", " + "...");
                try
                {
                    rez = _quik.Class.GetSecurityClass("SPBFUT,TQBR,TQBS,TQNL,TQLV,TQNE,TQOB", secCode).Result;
                }
                catch
                {
                    add("Ошибка определения класса инструмента. Убедитесь, что тикер указан правильно");
                }


                if (rez == classCode)
                {
                    add(secCode + "@" + classCode+" найден");

                }
                else add("не найден инструмент " + secCode+"@"+classCode  + "   ответ "+rez);




                //    add("Определяем код клиента..." );
                //    clientCode = _quik.Class.GetClientCode().Result;

                //add("код клиента найден "+ clientCode);


              //  add("Создаем экземпляр инструмента " + secCode + "@" + classCode + "   ..." );
                    tool = new Tool(_quik, secCode, classCode);
                    if (tool != null && tool.Name != null && tool.Name != "")
                    {
                        add("Инструмент " + tool.Name + " создан." );

                        //textBoxAccountID.Text = tool.AccountID;
                        //textBoxFirmID.Text = tool.FirmID;
                        //textBoxShortName.Text = tool.Name;
                        //textBoxLot.Text = Convert.ToString(tool.Lot);
                        //textBoxStep.Text = Convert.ToString(tool.Step);
                        //textBoxGuaranteeProviding.Text = Convert.ToString(tool.GuaranteeProviding);
                        //textBoxLastPrice.Text = Convert.ToString(tool.LastPrice);
                        //textBoxQty.Text = Convert.ToString(GetPositionT2(_quik, tool, clientCode));
                        add("Подписываемся на стакан..." );

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
                            add("Подписка на стакан " + tool.Name + " не удалась.");
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
                add("Ошибка получения данных по инструменту." );
            }
        }





        Queue<OrderBook> FIFOorderbook;
        Queue<AllTrade> FIFOtrade;


        static int ct = 0;
        void OnQuoteDo(OrderBook quote)
        {
            ct++;
            if (FIFOorderbook == null)
                FIFOorderbook = new Queue<OrderBook>();

            FIFOorderbook.Enqueue(quote);
            //if (FIFOorderbook.Count > 60000) WRITE();


            return;

            //if (quote.sec_code == tool.SecurityCode && quote.class_code == tool.ClassCode)
            //{
            //    toolOrderBook = quote;
            //    bid = Convert.ToDecimal(toolOrderBook.bid[toolOrderBook.bid.Count() - 1].price);
            //    offer = Convert.ToDecimal(toolOrderBook.offer[0].price);


            //}
        }

        string tektime = "";


        void ALLTRADE(AllTrade t)
        {
            if (FIFOtrade == null)
                FIFOtrade = new Queue<AllTrade>();

            FIFOtrade.Enqueue(t);

          //  if (FIFOtrade.Count > 60000) WRITE();

            QuikDateTime tt = t.Datetime;
            tektime = String.Format("{0}.{1}.{2}  {3}:{4}:{5}", tt.day, tt.month, tt.year, tt.hour, tt.min, tt.sec);


        }



        public Task<string> AsyncTaskSTART(string url)
        {

            return Task.Run(() =>
            {
                //----------------


                ///q.start();
                return "";

                //----------------
            });
        }

        async void RUN(string x)
        {
            string ss = await AsyncTaskSTART(x);
        }

















        void add(string s)
        {
            if (Event_Print != null)  Event_Print(s);
        }





    }
}
