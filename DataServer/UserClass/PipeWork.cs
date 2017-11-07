using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CobraDataServer
{ 
    class PipeWork
    {
        private static PipeItem _pip;
        public void Transmit()
        {

            while (true)
            {
                if (threadprocess.exit) break;
                if (data.pipeque.Count != 0 && data.PIPEENABLE)
                {
                    data.pipeque.TryDequeue(out _pip);

                    int i = -1;
                    foreach (var p in data.listpipe)
                    {
                        i++;
                        if (threadprocess.exit) break;
                        if (p.Name == _pip.namepipe)
                        {        
                                //mes.add(_pip.namepipe+"   "+ data._instr[i].name);
                                data._instr[i].ct++;
                                //if (DateTime.Now.Hour> data.hour_start_pipe)
                                p.send("tick;" + _pip.biditem + ";" + _pip.askitem + ";", p.Name);
     
                        }
                    }


                }
                else Thread.Sleep(50);
            }

        }


    }
}
