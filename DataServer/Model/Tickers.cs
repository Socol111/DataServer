using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobraDataServer
{

    public class PipeItem
    {
        //public string tickerNAME;
        public string tickerCOD;
        public double biditem;
        public double askitem;
    }


    public class Instrumensts
    {
        public Instrumensts(string name, string cod, string cl)
        {
            tickerName = name;
            tickerCOD = cod;
            Class = cl;
            lastorder = new Order();
        }

        public Order lastorder { get; set; }
        public double interes { get; set; } = 0;

        public int ct { get; set; } = 0;

        public int orders { get; set; } = 0;

        public int trades { get; set; } = 0;

        public string tickerCOD { get; set; }

        public string tickerName { get; set; }

        public string namefull { get; set; }

        public string Class { get; set; }

        public double lastPIPEask { get; set; }

        public double lastPIPEbid { get; set; }

        public double size_lot { get; set; }
    }


}
