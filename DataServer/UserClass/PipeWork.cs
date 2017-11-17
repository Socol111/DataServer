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
            try
            {
                while (data.Not_data) Thread.Sleep(100);
                while (true)
                {
                    data.crashpipe = false;
                    if (threadprocess.exit) break;
                    if (data.pipeque.Count != 0 && data.PIPEENABLE)
                    {
                        data.pipeque.TryDequeue(out _pip);

                        int i = -1;
                        foreach (var p in data.listpipe)
                        {
                            i++;
                            if (threadprocess.exit) break;
                            if (p.Name == _pip.namepipe && DateTime.Now.Hour >= data.hour_start_pipe)
                            {
                                //mes.add("tick;" + _pip.biditem + ";" + _pip.askitem + ";"+ p.Name);
                                data._instr[i].ct++;
                                p.send("tick;" + _pip.biditem + ";" + _pip.askitem + ";", p.Name);
                            }
                        }


                    }
                    else Thread.Sleep(200);
                }

                mes.err("Завершение потока PIPE");
            }
            catch (Exception ex)
            {
                mes.errLOG("PIPEerror "+ex);
                data.fatal = true;
            }
        }


    }
}
