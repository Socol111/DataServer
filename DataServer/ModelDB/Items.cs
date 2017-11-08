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
        //public Order()
        //{
        //    //va10 = 0;
        //    //va11 = 0;
        //    //va12 = 0;
        //    //va13 = 0;
        //    //va14 = 0;
        //    //va15 = 0;
        //    //va16 = 0;
        //    //va17 = 0;
        //    //va18 = 0;
        //    //va19 = 0;
        //    //va20 = 0;
        //}

        [Key]
        public int OrderId { get; set; }

        [ForeignKey("Ticker")]
        public int TickerId { get; set; }

        public Ticker Ticker { get; set; }


        public DateTime time { get; set; }

        public string NAMETEST { get; set; }
        public double bid1  { get  ; set; }
        public double vb1 { get; set; }
        public double bid2 { get; set; }
        public double vb2 { get; set; }
        public double bid3 { get; set; }
        public double vb3 { get; set; }
        public double bid4 { get; set; }
        public double vb4 { get; set; }
        public double bid5 { get; set; }
        public double vb5 { get; set; }
        public double bid6 { get; set; }
        public double vb6 { get; set; }
        public double bid7 { get; set; }
        public double vb7 { get; set; }
        public double bid8 { get; set; }
        public double vb8 { get; set; }
        public double bid9 { get; set; }
        public double vb9 { get; set; }
        public double bid10 { get; set; }
        public double vb10 { get; set; }
        public double bid11{ get; set; }
        public double vb11 { get; set; }
        public double bid12 { get; set; }
        public double vb12 { get; set; }
        public double bid13 { get; set; }
        public double vb13 { get; set; }
        public double bid14 { get; set; }
        public double vb14 { get; set; }
        public double bid15 { get; set; }
        public double vb15 { get; set; }
        public double bid16 { get; set; }
        public double vb16 { get; set; }
        public double bid17 { get; set; }
        public double vb17 { get; set; }
        public double bid18 { get; set; }
        public double vb18 { get; set; }
        public double bid19 { get; set; }
        public double vb19 { get; set; }
        public double bid20 { get; set; }
        public double vb20 { get; set; }

        public double bid21 { get; set; }
        public double vb21 { get; set; }
        public double bid22 { get; set; }
        public double vb22 { get; set; }
        public double bid23 { get; set; }
        public double vb23 { get; set; }
        public double bid24 { get; set; }
        public double vb24 { get; set; }
        public double bid25 { get; set; }
        public double vb25 { get; set; }
        public double bid26 { get; set; }
        public double vb26 { get; set; }
        public double bid27 { get; set; }
        public double vb27 { get; set; }
        public double bid28 { get; set; }
        public double vb28 { get; set; }
        public double bid29 { get; set; }
        public double vb29 { get; set; }
        public double bid30 { get; set; }
        public double vb30 { get; set; }
        public double bid31 { get; set; }
        public double vb31 { get; set; }
        public double bid32 { get; set; }
        public double vb32 { get; set; }
        public double bid33 { get; set; }
        public double vb33 { get; set; }
        public double bid34 { get; set; }
        public double vb34 { get; set; }
        public double bid35 { get; set; }
        public double vb35 { get; set; }
        public double bid36 { get; set; }
        public double vb36 { get; set; }
        public double bid37 { get; set; }
        public double vb37 { get; set; }
        public double bid38 { get; set; }
        public double vb38 { get; set; }
        public double bid39 { get; set; }
        public double vb39 { get; set; }
        public double bid40 { get; set; }
        public double vb40 { get; set; }
        public double bid41 { get; set; }
        public double vb41 { get; set; }
        public double bid42 { get; set; }
        public double vb42 { get; set; }
        public double bid43 { get; set; }
        public double vb43 { get; set; }
        public double bid44 { get; set; }
        public double vb44 { get; set; }
        public double bid45 { get; set; }
        public double vb45 { get; set; }
        public double bid46 { get; set; }
        public double vb46 { get; set; }
        public double bid47 { get; set; }
        public double vb47 { get; set; }
        public double bid48 { get; set; }
        public double vb48 { get; set; }
        public double bid49 { get; set; }
        public double vb49 { get; set; }
        public double bid50 { get; set; }
        public double vb50 { get; set; }

        public double ask1 { get; set; }
        public double va1 { get; set; }
        public double ask2 { get; set; }
        public double va2 { get; set; }
        public double ask3 { get; set; }
        public double va3 { get; set; }
        public double ask4 { get; set; }
        public double va4 { get; set; }
        public double ask5 { get; set; }
        public double va5 { get; set; }
        public double ask6 { get; set; }
        public double va6 { get; set; }
        public double ask7 { get; set; }
        public double va7 { get; set; }
        public double ask8 { get; set; }
        public double va8 { get; set; }
        public double ask9 { get; set; }
        public double va9 { get; set; }
        public double ask10 { get; set; }
        public double va10 { get; set; }
        public double ask11 { get; set; }
        public double va11 { get; set; }
        public double ask12 { get; set; }
        public double va12 { get; set; }
        public double ask13 { get; set; }
        public double va13 { get; set; }
        public double ask14 { get; set; }
        public double va14 { get; set; }
        public double ask15 { get; set; }
        public double va15 { get; set; }
        public double ask16 { get; set; }
        public double va16 { get; set; }
        public double ask17 { get; set; }
        public double va17 { get; set; }
        public double ask18 { get; set; }
        public double va18 { get; set; }
        public double ask19 { get; set; }
        public double va19 { get; set; }
        public double ask20 { get; set; }
        public double va20 { get; set; }
        public double ask21 { get; set; }
        public double va21 { get; set; }
        public double ask22 { get; set; }
        public double va22 { get; set; }
        public double ask23 { get; set; }
        public double va23 { get; set; }
        public double ask24 { get; set; }
        public double va24 { get; set; }
        public double ask25 { get; set; }
        public double va25 { get; set; }
        public double ask26 { get; set; }
        public double va26 { get; set; }
        public double ask27 { get; set; }
        public double va27 { get; set; }
        public double ask28 { get; set; }
        public double va28 { get; set; }
        public double ask29 { get; set; }
        public double va29 { get; set; }
        public double ask30 { get; set; }
        public double va30 { get; set; }
        public double ask31 { get; set; }
        public double va31 { get; set; }
        public double ask32 { get; set; }
        public double va32 { get; set; }
        public double ask33 { get; set; }
        public double va33 { get; set; }
        public double ask34 { get; set; }
        public double va34 { get; set; }
        public double ask35 { get; set; }
        public double va35 { get; set; }
        public double ask36 { get; set; }
        public double va36 { get; set; }
        public double ask37 { get; set; }
        public double va37 { get; set; }
        public double ask38 { get; set; }
        public double va38 { get; set; }
        public double ask39 { get; set; }
        public double va39 { get; set; }
        public double ask40 { get; set; }
        public double va40 { get; set; }
        public double ask41 { get; set; }
        public double va41 { get; set; }
        public double ask42 { get; set; }
        public double va42 { get; set; }
        public double ask43 { get; set; }
        public double va43 { get; set; }
        public double ask44 { get; set; }
        public double va44 { get; set; }
        public double ask45 { get; set; }
        public double va45 { get; set; }
        public double ask46 { get; set; }
        public double va46 { get; set; }
        public double ask47 { get; set; }
        public double va47 { get; set; }
        public double ask48 { get; set; }
        public double va48 { get; set; }
        public double ask49 { get; set; }
        public double va49 { get; set; }
        public double ask50 { get; set; }
        public double va50 { get; set; }

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
        /// <summary>
        /// период сессии 1-норм  0-открытие 2-закрытие
        /// </summary>
        public int periodsession { get; set; }
        /// <summary>
        /// тип сделки
        /// </summary>
        public bool buy { get; set; }
        public double price { get; set; }

        public long qty { get; set; }

        public double openinter { get; set; }

    }
}
