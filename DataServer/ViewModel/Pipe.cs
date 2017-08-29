using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Reflection;

namespace project.ViewModel
{
    class Pipe
    {
        NamedPipeClientStream pipCLIENT;
        //NamedPipeServerStream pipeSERVERwrite;
        string name;

        public static event Action<string, object> Event_Print;
        public static event Action<int, int, int, string> Event_CMD;

        public string pipename
        {
            get
            {
                return Model.data.pipe_prefix + name;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }


        public Pipe(string s)
        {
            foreach (var el in Model.data.eliminate)
            {
                if (el == s) return;
            }
            name = s;

            while (true)
            {
                string namechannel = Model.data.pipe_prefix + name;
                try
                {
                    //pipeSERVERwrite = new NamedPipeServerStream(namechannel,
                    //    PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    //pipeSERVERwrite.WaitForConnection();
                    add("подключаем " + namechannel + "...");
                    pipCLIENT = new NamedPipeClientStream(namechannel);
                    pipCLIENT.Connect();
                }
                catch (Exception ex) { err("ПРОБЛЕМА С PIPE " + namechannel + " " + ex.Message); Thread.Sleep(5000); continue; }
                finally { }
                add("PIPE КАНАЛ " + namechannel + " подключен к серверу status=" +
                    pipCLIENT.CanWrite + "/" + pipCLIENT.CanRead);
                break;
            }
        }

        public bool isConnect
        {
            get
            {
                if (pipCLIENT == null) return false;
                return pipCLIENT.IsConnected;
            }
        }

        bool lok_send = false;
        public void send(string msg)
        {
            if (lok_send) return;
            if (!isConnect) return;
    
            lok_send = true;
            WriteMessage(msg);
            lok_send = false;
        }

        public void WriteMessage( string put )
        {
            byte[] msg = Encoding.UTF8.GetBytes(put);

            try
            {
                pipCLIENT.Write(msg, 0, (int)msg.Length);
            }
            catch (Exception ex)
            {
                err("сбой записи в PIPE  " + ex.Message);
            }
            //while (!pipCLIENT.) Thread.Sleep(5);
        }

        public byte[] ReadMessage(PipeStream s)
        {
            MemoryStream ms = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            do
            {
                try
                {
                    ms.Write(buffer, 0, s.Read(buffer, 0, buffer.Length));
                }
                catch { err("сбой чтения из PIPE"); }
            }
            while (!s.IsMessageComplete);
            return ms.ToArray();
        }

        void add(string s)
        {
            if (Event_Print != null) Event_Print(s, System.Windows.Media.Brushes.Green);
        }
        void err(string s)
        {
            if (Event_Print != null) Event_Print(s, System.Windows.Media.Brushes.Red);
        }
    }



}
