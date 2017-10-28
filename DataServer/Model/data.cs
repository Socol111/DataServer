using System;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using QuikSharp.DataStructures;

namespace CobraDataServer
{
    public static class data
    {
        public static List<Pipe> listpipe=new List<Pipe>();
       
        public static QUIKSHARPconnector quik;
        public static List<Instrumensts> _instr;
        public static bool onestart = false;
        public static int ct_global=0;
        public static int ctglobalATstart = 0;
        public static string path_settings= "";
        public static string pipe_prefix1 = "IN_Cobraconnector_";
        public static string pipe_prefix2 = "MT_Cobraconnector_";

     
        public static string pathTIKERS1 = "d:/z/zAmerikaFinam/MQL4/Files/CobraConnector/ticker.ini";
        public static string pathTIKERS2 = "";
        public static string servertime="Server Time";
        public static bool first_Not_data = false;
        public static bool Not_data = false;
        public static bool Not_connect = true;
        public static bool fatal = false;
        public static bool fatal_need_rst_task = false;

        public static bool need_rst = false;
        public static byte hour_start_pipe = 10;

       
        public static ConcurrentQueue<PipeItem> pipeque = new ConcurrentQueue<PipeItem>();

        public static System.Collections.ObjectModel.ObservableCollection<string> eliminate = 
             new ObservableCollection<string> { "AA" , "BA" ,"AAPL" ,"EBAY", "USDRUB" };

        /// <summary>
        /// Игнор список
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool TickerIsOk(string name)
        {
            foreach (var el in data.eliminate)
            {
                if (el == name) return false;
            }
            return true;
        }

 
        public static void getTickers()
        {
            if (_instr==null)_instr = new List<Instrumensts>();
            _instr.Clear();
            FilesWork f = new FilesWork(data.pathTIKERS1);
            f.ReadListInstrument(_instr);
            f = new FilesWork(data.pathTIKERS2);
            f.ReadListInstrument(_instr);
        }

    }

    public static class mydb
    {
        public static ConcurrentQueue<Order> FIFOorderbook= new ConcurrentQueue<Order>();
        public static ConcurrentQueue<Trade> FIFOtrade= new ConcurrentQueue<Trade>();

        public static List<string> listtickers = new List<string>();
        public static MSSQL item = new MSSQL();

        public static string Connectparam = @"data source=DAV;" + //initial catalog=TradesAndOrders;" +
                                            //@"AttachDbFileName=rr:\DB\TradesAndOrders2.mdf;" +
                                            @"integrated security = True; MultipleActiveResultSets = True; App = EntityFramework; ";

        public static int sizepacket = 100;
        public static bool enable;
        public static string Path = @"G:\DB\";
        public static string Namebd = @"mydb";

        public static string GetStringConnection()
        {
            string rez ="";
            if (Path != "") rez += @" AttachDbFileName=" + Path + Namebd+ ".mdf;";
            if (Namebd != "") rez += @" initial catalog=" + Namebd + ";";
            return rez + Connectparam;
        }


    }


}
