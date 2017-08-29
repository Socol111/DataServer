using System;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace project.Model
{
    public static class data
    {
        public static int ct_global=0;
        public static string path_settings= "";
        public static string pipe_prefix = "IN_Cobraconnector_";
        public static bool pipe_enable = true;
        public static System.Collections.ObjectModel.ObservableCollection<string> eliminate = 
             new ObservableCollection<string> { "AA" , "BA" ,"AAPL" ,"EBAY", "USDRUB" };
    }


    
}
