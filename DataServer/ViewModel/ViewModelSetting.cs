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
using System.Windows.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace CobraDataServer
{
    partial class ViewModelSetting : ViewModelBase
    {
        public ViewModelSetting()
        {
            ini_command();
            //Update.EventUpdate += Update_EventUpdate;
        }
     
       

    }//class

}//namespace
