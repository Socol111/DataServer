using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using project.Model;
using project.Helpers;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows.Data;
using System.Threading.Tasks;

namespace project.ViewModel
{

    partial class ViewModelMain : ViewModelBase
    {
        public static QUIKSHARPconnector q = new QUIKSHARPconnector();
       
        //**********************************************************
        // INIT
        //**********************************************************
        public ViewModelMain()
        {

            ini_command();
            CreateTimer1(500);
        }

      

        public static async Task task1_release()
        {
            int num = -5;
            try
            {
                int result = 1;
                //if (num < 1)
                //    throw new Exception("Число не должно быть меньше 1");

                result = await Task.Run(() =>
                {
                    //for (int i = 1; i <= num; i++)
                    //{
                    //    result *= i;
                    //}

                    q.start();

                    return result;
                });
               // Console.WriteLine("Факториал числа {0} равен {1}", num, result);
            }
            catch (Exception ex)
            {
                await Log(ex);
            }
            finally
            {
               // await Task.Run(() => MessageBox.Show("ok " )) ; //Console.WriteLine("await в блоке finally"));
            }


        }

        //static async Task<int> Factorial(int x)
        //{
        //    //int result = 1;
        //    //if (x < 1)
        //    //    throw new Exception("Число не должно быть меньше 1");

        //    return await Task.Run(() =>
        //    {
        //        q.start();
        //        return 5;
        //    });
        //}

        static async Task Log(Exception ex)
        {
            await Task.Run(() =>
            {
                MessageBox.Show("Ошибка " + ex.ToString());/// Console.WriteLine(ex);
            });
        }

        //public static void task1_release()
        //    {
        //        // Use this line to throw an exception that is not handled.
        //        // Task task1 = Task.Factory.StartNew(() => { throw new IndexOutOfRangeException(); } );

        //        Task task1 = Task.Factory.StartNew(() => { q.start(); });


        //        try
        //        {
        //            task1.Wait();
        //        }
        //        catch (Exception ex)
        //        {

        //            MessageBox.Show("ОШИБКА " + ex.ToString());
        //        }
        //        //catch (AggregateException ex)
        //        //{
        //        //    ex.Handle((x) =>
        //        //    {
        //        //        string s = "";
        //        //        if (x is UnauthorizedAccessException) // This we know how to handle.
        //        //        {
        //        //            s += x.ToString();
        //        //            MessageBox.Show("ОШИБКА " + s);
        //        //            // return true;
        //        //        }
        //        //        return false; // Let anything else stop the application.
        //        //    });
        //        //}

        //        Console.WriteLine("task1 has completed.");

        //    }



        //    public Task<string> AsyncTaskSTART(string url)
        //    {

        //        return Task.Run(() =>
        //        {
        //            //----------------


        //            q.start();
        //            return "";

        //            //----------------
        //        });
        //    }

        //    async void RUN(string x)
        //    {
        //        string ss = await AsyncTaskSTART(x);
        //    }


    }//class
}//namespace
