using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections;
using CobraDataServer;
using project.Helpers;
using System.ComponentModel;

namespace CobraDataServer
{
    partial class ViewModelSetting : ViewModelBase
    {
        static List<string> gettickers()
        {
            var rez = new List<string>();
            foreach (var r in data._instr)
            {
                rez.Add(r.name);
            }
            return rez;
        }

        // private string y = byte.Parse(data.hour_start_pipe.ToString());
        public static object list => data.eliminate;
        public static object listACTUAL => mydb.listtickers;
        public static object listBD => gettickers();

        public string sizepacket { get => mydb.sizepacket.ToString(); set => mydb.sizepacket = int.Parse(value); }
        public string Prefix1 { get => data.pipe_prefix1; set => data.pipe_prefix1 = value; }
        public string Prefix2 { get => data.pipe_prefix2; set => data.pipe_prefix2 = value; }
        public string Pathtik1 { get => data.pathTIKERS1; set => data.pathTIKERS1 = value; }
        public string Pathtik2 { get => data.pathTIKERS2; set => data.pathTIKERS2 = value; }
        public string HourStartPipe { get => data.hour_start_pipe.ToString();  set => byte.Parse(value); }

        public string bdconn { get => mydb.Connectparam; set => mydb.Connectparam = value; }
        public string bdpath { get => mydb.Path; set => mydb.Path = value; }
        public string bdname { get => mydb.Namebd; set => mydb.Namebd = value; }

        public bool enabledatabase { get => mydb.enable; set => mydb.enable = value; }

    }//class


}//namespace
