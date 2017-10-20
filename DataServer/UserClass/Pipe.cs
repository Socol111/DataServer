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
using CobraDataServer;
using Serilog;
using System.Diagnostics;

namespace CobraDataServer
{
    public class Pipe
    {
        NamedPipeClientStream pipCLIENT1;
        NamedPipeClientStream pipCLIENT2;
        //NamedPipeServerStream pipeSERVERwrite;
        string name;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Pipe(string name)
        {
            this.name = name;
            if (!data.TickerIsOk(name)) return;

            mes.add("подключаем PIPE1 " + name + "...");
            createPIPE1(name);


            if (name.Contains("Z7") && name != "SiZ7" && name != "RIZ7") return;
            mes.add("подключаем PIPE2 " + name + "...");
            createPIPE2(name);
        }

        public void stopPIPE()
        {
            if (pipCLIENT1 != null) pipCLIENT1.Close();
            if (pipCLIENT2 != null) pipCLIENT2.Close();

        }

        void createPIPE1(string name)
        {
            while (true)
            {
                string namechannel = data.pipe_prefix1 + name;
                if (data.pipe_prefix1 == "" || data.pipe_prefix1 == "*") { pipCLIENT1 = null;return; }
                try
                {
                   
                    //pipeSERVERwrite = new NamedPipeServerStream(namechannel,
                    //    PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    //pipeSERVERwrite.WaitForConnection();
                   
                    pipCLIENT1 = new NamedPipeClientStream(namechannel);
                    pipCLIENT1.Connect();
                    
                }
                catch (Exception ex)
                {
                    mes.err("ПРОБЛЕМА С PIPE1 " + namechannel + " " + ex.Message);
                    Log.Debug("ПРОБЛЕМА С PIPE1 " + namechannel + " " + ex.Message);
                    Thread.Sleep(5000); continue;
                }
                mes.add("подключен " + namechannel + "  status=" + 
                pipCLIENT1.CanWrite + "/" + pipCLIENT1.CanRead);
                break;
            }


        }

        void createPIPE2(string name)
        {
            string namePIPE2 = "";
            try
            {
                namePIPE2 = data.pipe_prefix2 + name;
                if (data.pipe_prefix2 == "" || data.pipe_prefix2 == "*") { pipCLIENT2 = null; return; }

                pipCLIENT2 = new NamedPipeClientStream( namePIPE2);
                pipCLIENT2.Connect();

                mes.add("подключен " + namePIPE2 + "  status=" +
                pipCLIENT1.CanWrite + "/" + pipCLIENT1.CanRead);

             

                //byte[] msg = Encoding.UTF8.GetBytes("tick;");
                //pipCLIENT2.Write(msg, 0, (int)msg.Length);
                //msg = Encoding.UTF8.GetBytes("123.5;");
                //pipCLIENT2.Write(msg, 0, (int)msg.Length);
                //msg = Encoding.UTF8.GetBytes("7734.23;");
                //pipCLIENT2.Write(msg, 0, (int)msg.Length);
                //pipCLIENT2.Flush();
                //Trace.WriteLine("WRITE PIPE2 ok");
            }
            catch (Exception ex)
            {
                mes.err("ПРОБЛЕМА С pipe2 " + namePIPE2 + " " + ex.Message);
                Log.Debug("ПРОБЛЕМА С pipe2 " + namePIPE2 + " " + ex.Message);
                while (true)Thread.Sleep(5000); 
            }

          
        }


        public bool isConnectPIPE1
        {
            get
            {
                if (pipCLIENT1 == null ) return false;
                return pipCLIENT1.IsConnected;
            }
        }

        public bool isConnectPIPE2
        {
            get
            {
                if (pipCLIENT2 == null) return false;
                return pipCLIENT2.IsConnected;
            }
        }

        bool lok_send = false;
        public void send(string msg, string name)
        {
            if (lok_send) return;
          
            lok_send = true;
            WriteMessage(msg);
            lok_send = false;
        }

        byte ct_sboi = 0;
        public void WriteMessage( string put )
        {
            byte[] msg = Encoding.UTF8.GetBytes(put);
            string nm = "";
            try
            {
                nm = "PIPE1 " + this.name;
                if (pipCLIENT1 != null) pipCLIENT1.Write(msg, 0, (int)msg.Length);
                nm = "PIPE2 " + this.name;
                if (pipCLIENT2 != null) pipCLIENT2.Write(msg, 0, (int)msg.Length);
            }
            catch (Exception ex)
            {
                ct_sboi++;
                if (ct_sboi > 10)
                {
                    ct_sboi = 0;

                    if (!isConnectPIPE1) { mes.add("реконнект " + nm); createPIPE1(this.name); }
                    if (!isConnectPIPE2) { mes.add("реконнект " + nm); createPIPE2(this.name); }
                }
                else
                mes.err("сбой записи в "+nm+ "err=" + ex.Message);
            }
        }

        public byte[] ReadMessage(PipeStream s)
        {
            MemoryStream mswriter = new MemoryStream();
            byte[] buffer = new byte[0x1000];
            do
            {
                try
                {
                    mswriter.Write(buffer, 0, s.Read(buffer, 0, buffer.Length));
                }
                catch { mes.err("сбой чтения из PIPE"); }
            }
            while (!s.IsMessageComplete);
            return mswriter.ToArray();
        }

       
    }



    class PIPE_Server
    {
        string name = "";
        public PIPE_Server(string n){
            name = n;

            Task.Factory.StartNew(Connect);
        }
 
        private void Connect(){
         
                mes.add("Создаем PIPE server  " + name);
                var stream = new NamedPipeServerStream(name, PipeDirection.InOut, 3, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                stream.WaitForConnection();
                mes.add("Успешно подключен " + name );
                Task.Factory.StartNew(()=>Loop(stream));
            
        }

        string sendmessage = "";

        public void set_send_buffer(string message)
        {
            sendmessage = message;  
        }

        private void Loop(NamedPipeServerStream stream){
            try
            {
                while (true)
                {
                    if (sendmessage != "")
                    {
                        // stream.BeginRead(buf, 0, buf.Length, ar=> ReadCallback(ar, stream, buf), new object());
                        byte[] mess = Encoding.UTF8.GetBytes(sendmessage+"/n/r");
                        mes.add("пишем " + name);
                        stream.BeginWrite(mess, 0, mess.Length, i => { }, new object());
                        Thread.Sleep(500);
                    }
                    Thread.Sleep(100);
                }
            }
            catch { }
        }
 
        private void ReadCallback(IAsyncResult ar, NamedPipeServerStream stream, byte[] buf) {
            
                string s = Encoding.ASCII.GetString(buf).TrimEnd(Encoding.ASCII.GetChars(new byte[] { 0 }));
                if (String.IsNullOrEmpty(s)) { 
                    stream.Close();
                    return;
                }
            // Console.WriteLine(s);byte[] msg = Encoding.UTF8.GetBytes(put);
            var mess = Encoding.ASCII.GetBytes("Received");
            stream.BeginWrite(mess, 0, mess.Length, i => { }, new object());
                buf = new byte[1024];
            stream.BeginRead(buf, 0, buf.Length, arg => ReadCallback(arg, stream, buf), new object());
        }
    }

    //void save(ref byte[] msg)
    //{
    //    try
    //    {
    //        mes.err("попытка write " + name + " "
    //            );
    //        pipeSERVERwrite.Write(msg, 0, (int)msg.Length);
    //        pipeSERVERwrite.WaitForPipeDrain();
    //        modeTransmit = false;
    //        mes.err("удачно отпр " + name);
    //    }
    //    catch { }
    //}

    //class Client
    //{
    //    private static Client instance;
    //    private NamedPipeClientStream stream;
    //    private byte[] message;
    //    private bool stop;

    //    public Client()
    //    {
    //        stream = new NamedPipeClientStream(".", "namedpipe", PipeDirection.InOut, PipeOptions.Asynchronous);
    //        stream.Connect();
    //        Task.Factory.StartNew(Loop);
    //    }

    //    public void Send(string mess)
    //    {
    //        var message = Encoding.ASCII.GetBytes(mess);
    //        stream.Write(message, 0, message.Length);
    //        stream.WaitForPipeDrain();
    //    }

    //    private void Loop()
    //    {

    //        var buf = new byte[1024];
    //        stream.BeginRead(buf, 0, buf.Length, ar => ReadCallback(ar, buf), new object());
    //        stream.BeginRead(buf, 0, buf.Length, ar => {
    //            string s = Encoding.ASCII.GetString(buf).TrimEnd(Encoding.ASCII.GetChars(new byte[] { 0 }));
    //            Console.WriteLine(s);
    //        }, new object());
    //        for (int i = 0; i < 2; i++)
    //        {

    //            Thread.Sleep(1000);
    //        }
    //    }

    //    public void Stop()
    //    {
    //        stream.Close();
    //    }

    //    private void ReadCallback(IAsyncResult ar, byte[] buf)
    //    {
    //        string s = Encoding.ASCII.GetString(buf).TrimEnd(Encoding.ASCII.GetChars(new byte[] { 0 }));
    //        if (String.IsNullOrEmpty(s))
    //        {
    //            Stop();
    //            return;
    //        }
    //        Console.WriteLine(s);
    //        buf = new byte[1024];
    //        stream.BeginRead(buf, 0, buf.Length, arg => ReadCallback(arg, buf), new object());
    //    }
    //}
}
