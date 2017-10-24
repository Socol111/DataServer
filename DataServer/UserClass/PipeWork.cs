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
                if (data.exit) break;
                if (data.pipeque.Count != 0)
                {
                    data.pipeque.TryDequeue(out _pip);

                    int i = -1;
                    foreach (var p in data.listpipe)
                    {
                        i++;
                        if (data.exit) break;
                        if (p.Name == _pip.namepipe)
                        {
                            if (_pip.askitem == data._instr[i].ask && _pip.biditem == data._instr[i].bid)
                            {
                                break;
                            }
                            else
                            {
                                //mes.add(_pip.namepipe+"   "+ data._instr[i].name);
                                data._instr[i].ct++;
                                //if (DateTime.Now.Hour> data.hour_start_pipe)
                                p.send("tick;" + _pip.biditem + ";" + _pip.askitem + ";", p.Name);
                            }

                            data._instr[i].ask = _pip.biditem;
                            data._instr[i].bid = _pip.askitem;
                           
                        }
                    }


                }
                else Thread.Sleep(50);
            }

        }


    }
}
