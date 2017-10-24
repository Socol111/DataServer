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

namespace CobraDataServer
{
    public class MSSQL
    {

        public class lstOrders : List<Order>
        {

        }

        public class lstTrades : List<Trade>
        {

        }

        public void CREATEtest()
        {
            mes.err("create database");
         
            try
            {
                using (var db = new MyContext())
                {
                   
                    //Отключаем автоматическое слежение за изменениями
                    db.Configuration.AutoDetectChangesEnabled = false;
                    //db.Configuration.ValidateOnSaveEnabled = false;


                    // Add a food category
                    db.Database.Delete();
                   if (db.Database.Exists()) mes.err("БАЗА УЖЕ СУЩЕСТВУЕТ");
                   else mes.err("БАЗА НЕ СУЩЕСТВУЕТ");

                   // db.Database.Connection.Open();
                    mes.err("state = " + db.Database.Connection.State);
                    mes.err("conn = " + db.Database.Connection.ToString());
                    mes.err("AutoDetectChanges = " + db.Configuration.AutoDetectChangesEnabled.ToString());


                    int i = 1;
                    foreach (var tik in data._instr)
                    {
                        db.Tickers.Add(new Ticker {TickerId = i, Name = tik.name+"@"+tik.namefull});
                        i++;
                    }

                    //Обновляем сведения об изменениях. Работает быстро
                    db.ChangeTracker.DetectChanges(); 
                    int recordsAffected = db.SaveChanges();

                    lstOrders lst = new lstOrders()
                    {

                        new Order()
                        {
                            bid1 = 533,
                            volask1 = 5
                        },

                         new Order()
                        {
                            bid1 = 123,
                            volask2 = 5
                        }
                    };


                    //entities - коллекция сущностей EntityFramework
                    //using (IDataReader reader = lstOrders.GetDataReader())
                    //using (SqlConnection connection = new SqlConnection(mydb.GetStringConnection()))
                    //using (SqlBulkCopy bcp = new SqlBulkCopy(connection))
                    //{
                    //    connection.Open();

                    //    bcp.DestinationTableName = "[Order]";

                    //    bcp.ColumnMappings.Add("Date", "Date");
                    //    bcp.ColumnMappings.Add("Number", "Number");
                    //    bcp.ColumnMappings.Add("Text", "Text");

                    //    bcp.WriteToServer(reader);
                    //}







                    mes.err("create database write = " + recordsAffected.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("error database create " + ex.Message);
            }
            mes.err("end ");
        }  

        public class MyContext : DbContext
        {
            public MyContext(): base("name=MyContext")            {

                Database.Connection.ConnectionString = mydb.GetStringConnection();
            }
    
            public DbSet<Ticker> Tickers { get; set; }
            public DbSet<Order> Orders { get; set; }
        }



    }
}
