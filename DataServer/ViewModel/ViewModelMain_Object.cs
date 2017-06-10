using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace project.ViewModel
{
    partial class ViewModelMain : ViewModelBase
    {
       

        public object memory
        {
            get
            {
                return "(C) 2017 Memory Usage: " + string.Format("{0:0.00} MB", GC.GetTotalMemory(true) / 1024.0 / 1024.0);
            }
         
        }


    }//class

}//namespace
