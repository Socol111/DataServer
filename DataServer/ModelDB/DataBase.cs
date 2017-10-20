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

namespace CobraDataServer
{
    public class MSSQL
    {
      
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
                 

                    db.Categories.Add(new Category { CategoryId = "id1", Name = "Foods" });
                    db.Categories.Add(new Category { CategoryId = "id2", Name = "Foods" });


                    //Обновляем сведения об изменениях. Работает быстро
                    db.ChangeTracker.DetectChanges(); 
                    int recordsAffected = db.SaveChanges();

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
    
            public DbSet<Category> Categories { get; set; }
            public DbSet<Product> Products { get; set; }
        }



    }
}
