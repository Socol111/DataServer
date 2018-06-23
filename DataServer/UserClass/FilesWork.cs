using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Windows;

namespace CobraDataServer
{
    class FileUtil
    {
        string GetPath()
        {
            string executable = Assembly.GetExecutingAssembly().Location;
            string path = (Path.GetDirectoryName(executable));

            // AppDomain.CurrentDomain.SetData("DataDirectory", path);
            return path;
        }

    }
    class FilesWork
    {
        string path;
        public FilesWork(string s)
        {
            path = s;
        }
        public void ReadListInstrument(List<Instrumensts> p)
        {
            if (path == "" || path == "*")
            {
              //MessageBox.Show("Не задан путь списка тикеров");
              return;
            }
 
            using (StreamReader sr = new StreamReader(path))
            {
                sr.ReadLine();//шапка
                while (!sr.EndOfStream)
                {
                    string name = sr.ReadLine();
                    string cod = sr.ReadLine();
                    string cl = sr.ReadLine();


                    if (data.eliminate.Contains(cod) 
                        || cod == "" || name == "" || cl == "") { /*mes.add("Игнор тикера " + s1);*/  }
                    else
                    {
                        p.Add(new Instrumensts(name, cod, cl));
                    }
                    sr.ReadLine();
                    sr.ReadLine();
                }
            };
        }
    }
}
