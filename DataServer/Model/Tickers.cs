using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobraDataServer
{
    public class Instrumensts
    {
        public Instrumensts(string s, string cl)
        {
            name = s;
            Class = cl;
            bid = 0;
            ask = 0;
        }


        public double interes { get; set; } = 0;

        public int ct { get; set; } = 0;

        public int orders { get; set; } = 0;

        public int trades { get; set; } = 0;

        public string name { get; set; }

        public string namefull { get; set; }

        public string Class { get; set; }

        public decimal ask { get; set; }

        public decimal bid { get; set; }
    }


}
