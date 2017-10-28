using QuikSharp.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static CobraDataServer.MSSQL;

namespace CobraDataServer
{
    class Db_work
    {
        private Order ord;
        private Trade trd;
        private int j;
        public void work()
        {
            try
            {
                var entities = EntityGenerator.GeneratOrder(100);
                while (true)
                {
                    if (threadprocess.exit) break;
                    if (mydb.enable)
                    {

                        if (mydb.FIFOorderbook.Count == 0) Thread.Sleep(1000);


                        if (mydb.FIFOorderbook.Count > mydb.sizepacket)
                        {


                            entities.Clear();
                            for (j = 0; j < mydb.sizepacket; j++)
                            {
                                mydb.FIFOorderbook.TryDequeue(out ord);
                                entities.Add(ord);
                            }

                            mydb.item.WRITE_TO_DB(entities);
                        }

                        //using (var db = new MyContext())
                        //{
                        //    entities.Clear();

                        //    //for (int j = 0; j < 100; j++)
                        //    //{
                        //    //    if (mydb.FIFOorderbook.Count == 0) break;
                        //    //    mydb.FIFOorderbook.TryDequeue(out order);
                        //    //    entities.Add(order);
                        //    //}

                        //    db.Orders.AddRange(entities);
                        //    int recordsAffected = db.SaveChanges();




                        //}
                    }
                    else Thread.Sleep(100);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("err write to db "+ex.Message);
            }
        }

    }
}
