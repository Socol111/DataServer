using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace project.ViewModel
{
    class FilesWork
    {
        string path;
        public FilesWork(string s)
        {
            path = s;
        }
        public void ReadListInstrument(List<Instrumensts> p)
        {
            string s1,s2;
            using (StreamReader sr = new StreamReader(path))
            {
                sr.ReadLine();//шапка
                while (!sr.EndOfStream)
                {
                    s1 = sr.ReadLine();
                    s2 = sr.ReadLine();
                    p.Add(new Instrumensts(s1,s2));
                    sr.ReadLine();
                    sr.ReadLine();
                }
            };
        }
    }
}
