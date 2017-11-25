using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.ObjectModel;

namespace CobraDataServer
{
    [Serializable]
    public class ser_data
    {
        public string str1 { get; set; }
        public string str2 { get; set; }
        public string str3 { get; set; }
        public string str4 { get; set; }
        public string str5 { get; set; }
        public string str6 { get; set; }
        public string str7 { get; set; }
     
        public bool b1 { get; set; }
        public bool b2 { get; set; }

        public List<string> os1 { get; set; }
        public ObservableCollection<string> os2 { get; set; }

        public int in1 { get; set; }
        public int in2 { get; set; }
        public int in3 { get; set; }

        public int in4 { get; set; }
        public int in5 { get; set; }
        public int in6 { get; set; }
        public int in7 { get; set; }
        public int in8 { get; set; }
        public int in9 { get; set; }


        public ser_data()
        {
        }

        public void Prepare_to_save()
        {
            str1 = data.pipe_prefix1;
            str2 = data.pipe_prefix2;
            str3 = data.pathTIKERS1;
            str4 = data.pathTIKERS2;
            str5 = mydb.Connectparam;
            str6 = mydb.Path;
            str7 = mydb.Namebd;

            os2 = data.eliminate;
            os1 = mydb.listtickers;
            b1 = data.PIPEENABLE;
            b2 = mydb.enable;
            in1 = mydb.sizepacket;
            in2 = mydb.sizepackettrade;
            in3 = data.correct_time;

            in4 = data.h1;
            in5 = data.h2;
            in6 = data.m1;
            in7 = data.m2;
            in8 = data.hour_start;
            in9 = data.hour_stop;
        }

        public void Update_new_data()
        {
            data.pipe_prefix1= str1;
            data.pipe_prefix2 = str2;
            data.pathTIKERS1 = str3;
            data.pathTIKERS2 = str4;
            mydb.Connectparam = str5;
            mydb.Path = str6;
            mydb.Namebd = str7;

            data.eliminate= os2;
            mydb.listtickers = os1;

            data.PIPEENABLE = b1;
            mydb.enable = b2;
            mydb.sizepacket = in1;
            mydb.sizepackettrade = in2;
            data.correct_time = (byte)in3;

            data.h1 = (byte)in4;
            data.h2 = (byte)in5;
            data.m1 = (byte)in6;
            data.m2 = (byte)in7;
            data.hour_start = (byte)in8;
            data.hour_stop = (byte)in9;
        }
    }
    public static class SETTING 
    {   
        static string path = AppDomain.CurrentDomain.BaseDirectory + "//settings.xml";
        public static void SaveInXmlFormat()
        {
            ser_data dt = new ser_data();
            dt.Prepare_to_save();
            XmlSerializer formatter = new XmlSerializer(typeof(ser_data));

            using (Stream fStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                formatter.Serialize(fStream, dt);
            }

        }

        public static void ReadFromXML()
        {
            ser_data dt = new ser_data();
            XmlSerializer formatter = new XmlSerializer(typeof(ser_data));

            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                if (fs == null)  return;
                if (fs.Length == 0) return;
                // десериализация
                using (fs)
                {
                    dt = (ser_data)formatter.Deserialize(fs);
                }
                dt.Update_new_data();
            }
            catch (Exception Ситуация)
            {
                // Отчет обо всех возможных ошибках:
                MessageBox.Show(Ситуация.Message, "Ошибка в файле настроек ",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        public static bool SaveToBin()
        {
           
            // Open a file and serialize the object into it in binary format.
            // EmployeeInfo.osl is the file that we are creating. 
            // Note:- you can give any extension you want for your file
            // If you use custom extensions, then the user will now 
            //   that the file is associated with your program.
            //pathq + "//data.bin"

            try
            {
                Stream stream = File.Open(path, FileMode.Create);
                BinaryFormatter bformatter = new BinaryFormatter();
                //bformatter.Serialize(stream, IPTVman.Model.data);
                stream.Close();
                return true;
            }
            catch (Exception Ситуация)
            {
                // Отчет обо всех возможных ошибках:
                MessageBox.Show(Ситуация.Message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            // return;

            //System.Xml.Serialization.XmlSerializer writer =
            //    new System.Xml.Serialization.XmlSerializer(typeof(SerializationDATA));
            //var path = AppDomain.CurrentDomain.BaseDirectory + "SerializationOverview.xml";
            //System.IO.FileStream file = System.IO.File.Create(path);
            //writer.Serialize(file, overview);
            //file.Close();
        }


       // static SerializationDATA loadCLASS;
        public static bool ReadFromBin()
        {
            try
            {
                //loadCLASS = new SerializationDATA();

                //Open the file written above and read values from it.
                Stream stream = File.Open(path, FileMode.Open);
                BinaryFormatter  bformatter = new BinaryFormatter();


                //loadCLASS = (SerializationDATA)bformatter.Deserialize(stream);
                stream.Close();

          
                return true;
        
            }
            catch (Exception Ситуация)
            {
                // Отчет обо всех возможных ошибках:
                MessageBox.Show(Ситуация.Message, "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
        }
    }


}
