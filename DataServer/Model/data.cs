using System;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;

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
        public static bool pipe_enable = true;

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

        public static bool exit = false;
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
        public static MSSQL item = new MSSQL();
        public static string Connectparam = @"data source=DAV;"+//initial catalog=TradesAndOrders;" +
                //@"AttachDbFileName=D:\DB\TradesAndOrders2.mdf;" +
                @"integrated security = True; MultipleActiveResultSets = True; App = EntityFramework";

        public static bool enable;
        public static string Path = @"D:\DB\TradesAndOrders.mdf";
        public static string Namebd= @"TradesAndOrders";

        public static string GetStringConnection()
        {
            string rez = Connectparam;
            if (Path != "") rez += @"AttachDbFileName=" + Path + ";";
            if (Namebd != "") rez += @"initial catalog="+ Namebd + ";";
            return rez;
        }
    }

}
