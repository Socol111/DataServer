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
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Reflection;
using System.Xml;

namespace CobraDataServer
{
    partial class ViewModelSetting : ViewModelBase
    {

        public RelayCommand key_CreateDB { get; set; }
        public RelayCommand key_piperst { get; set; }
        public RelayCommand key_RAS { get; set; }
        public RelayCommand key_LOT_SAVE { get; set; }
       
        //public RelayCommand key_SAVE { get; set; }
        //public RelayCommand key_ADDBEST { get; set; }

        void ini_command()
        {
            key_CreateDB = new RelayCommand(_key_CreateDB);
            key_piperst = new RelayCommand(_key_piperst);
            key_RAS = new RelayCommand(_key_ras);
            key_LOT_SAVE = new RelayCommand(_key_lot_save);
        }


        List<string> _fd = new List<string>();
        string GET_PATH()
        {
            string executable = Assembly.GetExecutingAssembly().Location;
            return (Path.GetDirectoryName(executable));
        }
        private double Get_LotSize(string cod)
        {
            double rez = 0;
            int index = 0;
            foreach (var i in _fd)
            {
                if (i == cod)
                {
                    try
                    {
                        int var = int.Parse(_fd[index + 1]);
                        rez = (double)var;
                        break;
                    }
                    catch (Exception ex) { MessageBox.Show("ОШИБКА КОНВЕРСИИ " + ex.Message ); }
                }
                index++;
            }
            return rez;
        }

        string Get_URL_GO()
        {
            string path = "";
            string URL = "";
            try
            {
                string executable = Assembly.GetExecutingAssembly().Location;
                path = (Path.GetDirectoryName(executable));
                path += @"\_lots_.ini";
                path = path.Replace("\"", "/");

                _fd.Clear();

                using (StreamReader sr = new StreamReader(path))
                {
                    sr.ReadLine();//шапка
                    URL= sr.ReadLine();//url 
                   
                }
            }
            catch (Exception ex)
            {
                { MessageBox.Show("ОШИБКА " + ex.Message + " path " + path); }
            }
            return URL;
        }


        string GET_PATH_FILE_GO()
        {
            return GET_PATH() + @"\go.xml";
        }

        /// <summary>
        /// Чтение файла ГО с сайта биржи
        /// </summary>
        void LOAD_FILE_GO()
        {
            try
            {
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(Get_URL_GO());
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                    Stream stream = resp.GetResponseStream();

                    string pathsave = GET_PATH_FILE_GO();

                    FileStream file = new FileStream(pathsave, FileMode.Create, FileAccess.Write);
                    StreamWriter write = new StreamWriter(file);
                    int b;
                    for (int i = 0; ; i++)
                    {
                    b = stream.ReadByte();
                    if (b == -1) break;
                    write.Write((char)b);
                    }
                    write.Close();
                    file.Close();
                    }
                    catch (Exception ex)
                    {
                       MessageBox.Show("ОШИБКА ПОЛУЧЕНИЯ ДАННЫХ С САЙТА БИРЖИ " + ex.Message ); 
                    }
        }

        /// <summary>
        /// чтение размера ЛОТА из файла
        /// </summary>
        void load_lot_size()
        {
            string path = "";
            try
            {
                path = GET_PATH() + @"\_lots_.ini";
                path = path.Replace("\"", "/");

                _fd.Clear();

                using (StreamReader sr = new StreamReader(path))
                {
                    sr.ReadLine();//шапка
                    sr.ReadLine();//url 
                    while (!sr.EndOfStream)
                    {
                        _fd.Add(sr.ReadLine());
                    }
                }
            }
            catch (Exception ex)
            {
                { MessageBox.Show("ОШИБКА " + ex.Message +" path "+path); }
            }
        }

        List<string> list_go;
        List<double> list_go_value;

        void PARSE_GO_XML()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(GET_PATH_FILE_GO());
            // получим корневой элемент
            XmlElement doc = xDoc.DocumentElement;
            // обход всех узлов в корневом элементе

            foreach (XmlNode xnode in doc)
            {

                string s = "";
                foreach (var x in xnode.InnerXml)
                {
                    s += x.ToString();
                }

                string[] split_go = s.Split(new Char[] { '"' });

                list_go = new List<string>();
                list_go_value = new List<double>();
                bool flag1 = false;
                bool flag2 = false;
                foreach (string val in split_go)
                {
                    if (flag1)
                    {
                        list_go.Add(val);
                        flag1 = false;
                    }

                    if (flag2)
                    {
                        // data.calcLOTS.Add(val.ToString());
                        string vv = val.Replace(".", ",");
                        list_go_value.Add(double.Parse(vv));
                        flag2 = false;
                    }
                    if (val.Contains("symbol=")) flag1 = true;
                    if (val.Contains("initial_margin=")) flag2 = true;
                }

                //data.calcLOTS.Add("All Symbols = " + list_go.Count());
                foreach (var instr in data._instr)
                {
                    bool find_ok = false;
                    foreach (var j in list_go)
                    {
                        if (instr.tickerCOD.ToUpper() == j.ToUpper()) { find_ok = true; break; }
                    }
                    if (!find_ok) data.calcLOTS.Add("NO FIND Symbols = " +instr.tickerCOD+" / "+instr.tickerName);
                }
            }
         
        }

        double GET_value_from_GO(string COD)
        {
            double val = 0;

            int i = 0;
            foreach (string s in list_go)
            {
                if (s == COD)
                {
                    val = list_go_value[i];
                    break;
                }
                i++;
            }

            return val;
        }


        /// <summary>
        /// Расчет ЛОТА
        /// </summary>
        /// <param name="obj"></param>
        private void _key_ras(object obj)
        {         
            try
            {
                data.calcLOTS.Clear();
                LOAD_FILE_GO();
                PARSE_GO_XML();
                load_lot_size();
     
               
                foreach (var instr in data._instr)
                {
                    foreach (var _t in mydb.listtickers) 
                    {
                        if (instr.tickerCOD.ToUpper() == _t.ToUpper())
                        {
                            double lot = Get_LotSize(instr.tickerCOD);
                            double value = 0;
                            value = GET_value_from_GO(instr.tickerCOD); // instr.lastPIPEbid / lot;
                            if (value > 0)
                            {
                                double v = data.sizeLOT / value;
                                data.calcLOTS.Add(instr.namefull + "; " + instr.tickerCOD +
                                "; " + instr.lastPIPEask + "; " + instr.lastPIPEbid + "; lot=" + lot.ToString() +
                                "; "+ String.Format("ГО={0:f2}   S={1:f2}", value, v) + "; " + Math.Round(v,0));
                            }
                            else
                                data.calcLOTS.Add(instr.namefull + "; " + instr.tickerCOD +
                                "; " + instr.lastPIPEask + "; " + instr.lastPIPEbid +
                                ";    ERROR  ГО !!! ");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                { MessageBox.Show("ОШИБКА " + ex.Message); }
            }
        }

      
        private void _key_lot_save(object obj)
        {
            _key_ras(null);

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            string path = ofd.FileName;
            List<string> mass = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                {
                    while (true)
                    {
                        var str = sr.ReadLine();
                        if (str == null) break;
                        mass.Add(str);
                    }
                }

                int ct = -1;
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                   
                    string cod="";
                    foreach (var s in mass)
                    {
                        bool find_ok=false;
                        if (ct == -1) { sw.WriteLine(s); ct = 0; continue; }//шапка файла
                        ct++;

                        if (ct == 2) cod = s;
                        if (ct == 4)
                        {
                            foreach (var _instr in mydb.listtickers)
                            {

                                if (_instr.ToUpper()==cod.ToUpper())
                                {
                                    double  value = GET_value_from_GO(cod); // instr.lastPIPEbid / lot;
                                    double v = data.sizeLOT / value;
                                    double val = Math.Round(v, 0);
                                    if (val >= 1 && val < 1e6) { find_ok = true; } else val = 0;
      
                                    if (val!=0) sw.WriteLine(val.ToString());
                                    else sw.WriteLine(s);
                                    break;
                                }
                                
                            }
                            if (!find_ok) { sw.WriteLine(s); }

                            find_ok = false;

                        }
                        else sw.WriteLine(s);

                        if (ct == 5) ct = 0;
                    }
                    
                }
                if (ct==0) MessageBox.Show("УСПЕШНО!");
                else MessageBox.Show("ОШИБКА  счетчика");
            }
            catch(Exception ex) { MessageBox.Show("ОШИБКА "+ex.Message); }

        }

        /// <summary>
        /// Создать БД
        /// </summary>
        /// <param name="obj"></param>
        private void _key_CreateDB(object obj)
        {
            if (mydb.enable)
            {
                MessageBox.Show("Сначала остановите запись в БАЗУ");
                return;
            }
            mydb.item.CREATEtest();
        }

        /// <summary>
        /// Рестарт PIPE
        /// </summary>
        /// <param name="sender"></param>
        private void _key_piperst(object sender)
        {
            if (!data.PIPEENABLE) return;
            mes.errLOG("РУЧНОЙ Рестарт PIPE");

            new Task( () =>
                {
                    threadprocess.PIPE_Thread_restart();
                    threadprocess.PIPE_all_reconnect();
                }
            ).Start();
        }
    }//class

}//namespace
