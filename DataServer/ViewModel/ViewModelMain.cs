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

        //**********************************************************
        // INIT
        //**********************************************************
        public ViewModelMain()
        {


            ini_command();
            CreateTimer1(500);

        }

 

    }//class
}//namespace
