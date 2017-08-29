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
        public static QUIKSHARPconnector quik;
        public static List<Instumensts> _instr;

        public ViewModelMain()
        {
            ini_command();
            CreateTimer1(500);
            _instr = new List<Instumensts>();
            FilesWork f = new FilesWork("d:/z/zAmerikaFinam/MQL4/Files/CobraConnector/ticker.ini");
            f.ReadListInstrument(_instr);
       
            quik = new QUIKSHARPconnector();
        }
        public async static void task1_release()
        {
            while (true)
            {
                Thread.Sleep(500);
                quik.Connect(_instr);
                Thread.Sleep(5000);
                quik.work();
            }
        }

    }//class
}//namespace
