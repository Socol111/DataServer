using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Data;
using QuikSharp.DataStructures;
using System.Diagnostics;

namespace CobraDataServer
{
    public class MSSQL
    {

        public class EntityGenerator_Order
        {
            public static List<Order> GeneratOrder(int count)
            {
                return new List<Order>(count);
            }
        }

        public class EntityGenerator_Trade
        {
            public static List<Trade> GeneratOrder(int count)
            {
                return new List<Trade>(count);
            }
        }

        public void ADDtikers(MyContext db)
        {
            int i = 1;
            foreach (var tik in data._instr)
            {
                db.Tickers.Add(new Ticker { Id = i, Name = tik.tickerName });
                i++;
            }

        }

        public void CREATEtest()
        {
            if (data._instr.Count == 0)
            {
                mes.err("нет тикеров для создания базы данных");
                return;
            }

            mes.add("--------------------------");
            mes.add("СОЗДАНИЕ БАЗЫ ДАННЫХ ....");

            try
            {
                using (var db = new MyContext())
                {                 
                    //Отключаем автоматическое слежение за изменениями
                    db.Configuration.AutoDetectChangesEnabled = false;
                    //db.Configuration.ValidateOnSaveEnabled = false;

                    db.Database.Delete();

                    ADDtikers(db);

                    //Обновляем сведения об изменениях. Работает быстро
                    db.ChangeTracker.DetectChanges();

                    ////// entities - коллекция сущностей EntityFramework
                   List<Order> entities = EntityGenerator_Order.GeneratOrder(100);

                    mes.add("conn = " + db.Database.Connection.ToString());
                    mes.add("AutoDetectChanges = " + db.Configuration.AutoDetectChangesEnabled.ToString());

                    for (int j = 0; j < 50; j++)
                    {
                        entities.Add
                        (
                          new Order()
                          {
                              TickerId = 1,
                              time = new DateTime(2016, 7, 3),
                              ask1 = j,
                              va1 = j,
                              bid1 = j,
                              vb1 = j
                          }
                        );

                    }
                    mes.add("Таблицы созданы");

                    var sw = Stopwatch.StartNew();
                  
                    db.Orders.AddRange(entities);
                    int recordsAffected = db.SaveChanges();
                    sw.Stop();

                    db.Orders.RemoveRange(entities);
                    recordsAffected = db.SaveChanges();
                   
                    mes.add("тестовая запись выполнена " + recordsAffected.ToString()+ "  время = "+ sw.ElapsedMilliseconds.ToString()+" (мс)");
                    mes.add("База успешно создана.");
                }
            }
  
            catch (Exception ex)
            {
                MessageBox.Show("error database create " + ex.Message);
                mes.err("Ошибка БД");
            }
        }


        public void WRITE_TO_DB_ORDER(List<Order> ent)
        {

            using (IDataReader reader = ent.GetDataReader())
            using (SqlConnection connection = new SqlConnection(mydb.GetStringConnection()))
            using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
            {
                connection.Open();

                bcp.DestinationTableName = "[Orders]";
                //bcp.BatchSize = 0;//(число строк, загружаемых за раз на сервер, по умолчанию — равно 0
                try
                {
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("OrderId", "OrderId"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TickerId", "TickerId"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("time", "time"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NAMETEST", "NAMETEST"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid1", "bid1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb1", "vb1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid2", "bid2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb2", "vb2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid3", "bid3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb3", "vb3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid4", "bid4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb4", "vb4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid5", "bid5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb5", "vb5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid6", "bid6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb6", "vb6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid7", "bid7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb7", "vb7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid8", "bid8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb8", "vb8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid9", "bid9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb9", "vb9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid10", "bid10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb10", "vb10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid11", "bid11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb11", "vb11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid12", "bid12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb12", "vb12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid13", "bid13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb13", "vb13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid14", "bid14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb14", "vb14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid15", "bid15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb15", "vb15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid16", "bid16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb16", "vb16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid17", "bid17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb17", "vb17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid18", "bid18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb18", "vb18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid19", "bid19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb19", "vb19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid20", "bid20"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb20", "vb20"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid21", "bid21"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb21", "vb21"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid22", "bid22"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb22", "vb22"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid23", "bid23"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb23", "vb23"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid24", "bid24"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb24", "vb24"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid25", "bid25"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb25", "vb25"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid26", "bid26"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb26", "vb26"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid27", "bid27"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb27", "vb27"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid28", "bid28"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb28", "vb28"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid29", "bid29"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb29", "vb29"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid30", "bid30"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb30", "vb30"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid31", "bid31"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb31", "vb31"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid32", "bid32"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb32", "vb32"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid33", "bid33"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb33", "vb33"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid34", "bid34"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb34", "vb34"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid35", "bid35"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb35", "vb35"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid36", "bid36"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb36", "vb36"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid37", "bid37"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb37", "vb37"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid38", "bid38"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb38", "vb38"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid39", "bid39"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb39", "vb39"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid40", "bid40"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb40", "vb40"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid41", "bid41"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb41", "vb41"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid42", "bid42"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb42", "vb42"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid43", "bid43"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb43", "vb43"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid44", "bid44"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb44", "vb44"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid45", "bid45"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb45", "vb45"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid46", "bid46"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb46", "vb46"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid47", "bid47"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb47", "vb47"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid48", "bid48"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb48", "vb48"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid49", "bid49"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb49", "vb49"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid50", "bid50"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("vb50", "vb50"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask1", "ask1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va1", "va1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask2", "ask2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va2", "va2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask3", "ask3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va3", "va3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask4", "ask4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va4", "va4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask5", "ask5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va5", "va5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask6", "ask6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va6", "va6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask7", "ask7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va7", "va7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask8", "ask8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va8", "va8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask9", "ask9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va9", "va9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask10", "ask10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va10", "va10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask11", "ask11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va11", "va11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask12", "ask12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va12", "va12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask13", "ask13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va13", "va13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask14", "ask14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va14", "va14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask15", "ask15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va15", "va15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask16", "ask16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va16", "va16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask17", "ask17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va17", "va17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask18", "ask18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va18", "va18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask19", "ask19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va19", "va19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask20", "ask20"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va20", "va20"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask21", "ask21"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va21", "va21"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask22", "ask22"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va22", "va22"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask23", "ask23"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va23", "va23"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask24", "ask24"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va24", "va24"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask25", "ask25"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va25", "va25"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask26", "ask26"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va26", "va26"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask27", "ask27"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va27", "va27"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask28", "ask28"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va28", "va28"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask29", "ask29"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va29", "va29"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask30", "ask30"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va30", "va30"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask31", "ask31"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va31", "va31"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask32", "ask32"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va32", "va32"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask33", "ask33"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va33", "va33"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask34", "ask34"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va34", "va34"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask35", "ask35"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va35", "va35"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask36", "ask36"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va36", "va36"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask37", "ask37"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va37", "va37"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask38", "ask38"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va38", "va38"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask39", "ask39"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va39", "va39"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask40", "ask40"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va40", "va40"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask41", "ask41"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va41", "va41"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask42", "ask42"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va42", "va42"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask43", "ask43"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va43", "va43"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask44", "ask44"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va44", "va44"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask45", "ask45"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va45", "va45"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask46", "ask46"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va46", "va46"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask47", "ask47"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va47", "va47"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask48", "ask48"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va48", "va48"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask49", "ask49"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va49", "va49"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask50", "ask50"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("va50", "va50"));

                    bcp.WriteToServer(reader);
                }
                catch (Exception ex)
                {
                    mes.errLOG("Ошибка записи в таблицу Orders " + ex.Message);
                }
            }


        }

        public void WRITE_TO_DB_TRADE(List<Trade> ent)
        {

            using (IDataReader reader = ent.GetDataReader())
            using (SqlConnection connection = new SqlConnection(mydb.GetStringConnection()))
            using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
            {
                connection.Open();

                bcp.DestinationTableName = "[Trades]";
                //bcp.BatchSize = 0;//(число строк, загружаемых за раз на сервер, по умолчанию — равно 0

                try
                {
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TradeId", "TradeId"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("TickerId", "TickerId"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("time", "time"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("NAMETEST", "NAMETEST"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("price", "price"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("qty", "qty"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("openinter", "openinter"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("periodsession", "periodsession"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("buy", "buy"));

                    bcp.WriteToServer(reader);
                }
                catch (Exception ex)
                {
                    mes.errLOG("Ошибка записи в таблицу Trades "+ex.Message);
                }
            }


        }

        public class MyContext : DbContext
        {
            public MyContext(): base("name=MyContext")            {

                Database.Connection.ConnectionString = mydb.GetStringConnection();
            }
          
            public DbSet<Ticker> Tickers { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<Trade> Trades { get; set; }
        }



    }
}
