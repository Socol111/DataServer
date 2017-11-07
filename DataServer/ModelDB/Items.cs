using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobraDataServer
{
    public class Ticker
    {
        public int Id { get; set; }
        public string Name { get; set; }

        //public ICollection<Order> Orders { get; set; }
        //public ICollection<Trade> Trades { get; set; }
    }

    [Table("Orders")]
    public class Order
    {
       [Key]
        public int OrderId { get; set; }

        [ForeignKey("Ticker")]
        public int TickerId { get; set; }

        public Ticker Ticker { get; set; }


        public DateTime time { get; set; }

        public string NAMETEST { get; set; }
        public double bid1  { get  ; set; }
        public double volbid1 { get; set; }
        public double bid2 { get; set; }
        public double volbid2 { get; set; }
        public double bid3 { get; set; }
        public double volbid3 { get; set; }
        public double bid4 { get; set; }
        public double volbid4 { get; set; }
        public double bid5 { get; set; }
        public double volbid5 { get; set; }
        public double bid6 { get; set; }
        public double volbid6 { get; set; }
        public double bid7 { get; set; }
        public double volbid7 { get; set; }
        public double bid8 { get; set; }
        public double volbid8 { get; set; }
        public double bid9 { get; set; }
        public double volbid9 { get; set; }
        public double bid10 { get; set; }
        public double volbid10 { get; set; }
        public double bid11{ get; set; }
        public double volbid11 { get; set; }
        public double bid12 { get; set; }
        public double volbid12 { get; set; }
        public double bid13 { get; set; }
        public double volbid13 { get; set; }
        public double bid14 { get; set; }
        public double volbid14 { get; set; }
        public double bid15 { get; set; }
        public double volbid15 { get; set; }
        public double bid16 { get; set; }
        public double volbid16 { get; set; }
        public double bid17 { get; set; }
        public double volbid17 { get; set; }
        public double bid18 { get; set; }
        public double volbid18 { get; set; }
        public double bid19 { get; set; }
        public double volbid19 { get; set; }
        public double bid20 { get; set; }
        public double volbid20 { get; set; }



        public double ask1 { get; set; }
        public double volask1 { get; set; }
        public double ask2 { get; set; }
        public double volask2 { get; set; }
        public double ask3 { get; set; }
        public double volask3 { get; set; }
        public double ask4 { get; set; }
        public double volask4 { get; set; }
        public double ask5 { get; set; }
        public double volask5 { get; set; }
        public double ask6 { get; set; }
        public double volask6 { get; set; }
        public double ask7 { get; set; }
        public double volask7 { get; set; }
        public double ask8 { get; set; }
        public double volask8 { get; set; }
        public double ask9 { get; set; }
        public double volask9 { get; set; }
        public double ask10 { get; set; }
        public double volask10 { get; set; }
        public double ask11 { get; set; }
        public double volask11 { get; set; }
        public double ask12 { get; set; }
        public double volask12 { get; set; }
        public double ask13 { get; set; }
        public double volask13 { get; set; }
        public double ask14 { get; set; }
        public double volask14 { get; set; }
        public double ask15 { get; set; }
        public double volask15 { get; set; }
        public double ask16 { get; set; }
        public double volask16 { get; set; }
        public double ask17 { get; set; }
        public double volask17 { get; set; }
        public double ask18 { get; set; }
        public double volask18 { get; set; }
        public double ask19 { get; set; }
        public double volask19 { get; set; }
        public double ask20 { get; set; }
        public double volask20 { get; set; }


    }

    [Table("Trades")]
    public class Trade
    {
        [Key]
        public int TradeId { get; set; }

        [ForeignKey("Ticker")]
        public int TickerId { get; set; }

        public Ticker Ticker { get; set; }

        public DateTime time { get; set; }

        public string NAMETEST { get; set; }

        public double price { get; set; }

        public long qty { get; set; }

        public double openinter { get; set; }

    }
}
