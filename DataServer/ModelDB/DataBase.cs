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
                var entities = new List<Order>(count);
          
                return entities;
            }
        }

        public class EntityGenerator_Trade
        {
            public static List<Trade> GeneratOrder(int count)
            {
                var entities = new List<Trade>(count);

                return entities;
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
                  
                

                    int i = 1;
                    foreach (var tik in data._instr)
                    {
                        db.Tickers.Add(new Ticker {Id = i, Name = tik.name});
                        i++;
                    }

                    //Обновляем сведения об изменениях. Работает быстро
                    db.ChangeTracker.DetectChanges();

                    ////// entities - коллекция сущностей EntityFramework
                   List<Order> entities = EntityGenerator_Order.GeneratOrder(100);


                    mes.add("state = " + db.Database.Connection.State);
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
                              bid1 = j,
                              volbid1 = j
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
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid1", "volbid1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid2", "bid2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid2", "volbid2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid3", "bid3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid3", "volbid3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid4", "bid4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid4", "volbid4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid5", "bid5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid5", "volbid5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid6", "bid6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid6", "volbid6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid7", "bid7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid7", "volbid7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid8", "bid8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid8", "volbid8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid9", "bid9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid9", "volbid9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid10", "bid10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid10", "volbid10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid11", "bid11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid11", "volbid11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid12", "bid12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid12", "volbid12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid13", "bid13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid13", "volbid13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid14", "bid14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid14", "volbid14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid15", "bid15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid15", "volbid15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid16", "bid16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid16", "volbid16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid17", "bid17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid17", "volbid17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid18", "bid18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid18", "volbid18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid19", "bid19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid19", "volbid19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("bid20", "bid20"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volbid20", "volbid20"));

                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask1", "ask1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask1", "volask1"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask2", "ask2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask2", "volask2"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask3", "ask3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask3", "volask3"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask4", "ask4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask4", "volask4"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask5", "ask5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask5", "volask5"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask6", "ask6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask6", "volask6"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask7", "ask7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask7", "volask7"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask8", "ask8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask8", "volask8"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask9", "ask9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask9", "volask9"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask10", "ask10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask10", "volask10"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask11", "ask11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask11", "volask11"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask12", "ask12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask12", "volask12"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask13", "ask13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask13", "volask13"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask14", "ask14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask14", "volask14"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask15", "ask15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask15", "volask15"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask16", "ask16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask16", "volask16"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask17", "ask17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask17", "volask17"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask18", "ask18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask18", "volask18"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask19", "ask19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask19", "volask19"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("ask20", "ask20"));
                    bcp.ColumnMappings.Add(new SqlBulkCopyColumnMapping("volask20", "volask20"));



                    bcp.WriteToServer(reader);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error map  " + ex.Message);
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

                    bcp.WriteToServer(reader);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("error map  " + ex.Message);
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
