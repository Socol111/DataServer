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
        public RelayCommand key_piperst { get; set; }
        //public RelayCommand key_SAVE { get; set; }
        //public RelayCommand key_ADDBEST { get; set; }

        void ini_command()
        {
            key_CreateDB = new RelayCommand(_key_CreateDB);
            key_piperst = new RelayCommand(_key_piperst);
        }

        private void _key_CreateDB(object obj)
        {
            if (mydb.enable)
            {
                MessageBox.Show("Сначала остановите запись в БАЗУ");
                return;
            }
            mydb.item.CREATEtest();
        }

        private void _key_piperst(object sender)
        {
            if (!data.PIPEENABLE) return;
            mes.errLOG("РУЧНОЙ Рестарт PIPE");

            new Task( () =>
                {
                    threadprocess.PIPE_Thread_restart();
                    threadprocess.PIPE_all_reconnect();
                }
            ).Start();
        }
    }//class

}//namespace
