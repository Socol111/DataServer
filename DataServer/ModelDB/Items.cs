using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobraDataServer
{
    public class Ticker
    {
        public int TickerId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Trade> Trades { get; set; }
    }

    public class Order
    {
        public DateTime OrderId { get; set; }
        public ushort TickerId { get; set; }

        public decimal bid1 { get; set; }
        public uint volbid1 { get; set; }

        public decimal bid2 { get; set; }
        public uint volbid2 { get; set; }

        public decimal bid3 { get; set; }
        public uint volbid3 { get; set; }

        public decimal ask1 { get; set; }
        public uint volask1 { get; set; }

        public decimal ask2 { get; set; }
        public uint volask2 { get; set; }

        public decimal ask3 { get; set; }
        public uint volask3 { get; set; }

        public virtual Ticker Ticker { get; set; }
    }

    public class Trade
    {
        public DateTime TradeId { get; set; }
        public ushort TickerId { get; set; }

        public virtual Ticker Ticker { get; set; }
    }
}
