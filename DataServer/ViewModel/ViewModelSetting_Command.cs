using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using CobraDataServer;
using project.Helpers;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Threading;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;


namespace CobraDataServer
{
    partial class ViewModelSetting : ViewModelBase
    {

        public RelayCommand key_CreateDB { get; set; }
        //public RelayCommand key_SAVE { get; set; }
        //public RelayCommand key_ADDBEST { get; set; }

        void ini_command()
        {
            key_CreateDB = new RelayCommand(_key_CreateDB);
        }

        private void _key_CreateDB(object obj)
        {
            mydb.item.CREATEtest();
        }

    }//class

}//namespace
