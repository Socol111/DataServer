using System.Windows;
using IPTVman.ViewModel;
using System.Windows.Data;
using System.Windows.Threading;
using CobraDataServer;
using project.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using System.IO;

namespace CobraDataServer
{
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();
            this.Title = "Настройки";

             // use a timer to periodically update the memory usage
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += timer_Tick;
            timer.Start();

        }

        private void timer_Tick(object sender, EventArgs e)
        {
           
        }

        void cmd(int cod, int p1, int p2, string s)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var it = Listalltk2.SelectedItem as string;
            if (!data.eliminate.Contains(it)) data.eliminate.Add(it);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var s = Lst1.SelectedIndex;
            if (s < 0) return;
            data.eliminate.RemoveAt(s);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            SETTING.SaveInXmlFormat();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                   data.pathTIKERS1 = dialog.SelectedPath;
        }

        private void selectpath_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                data.pathTIKERS1 = ofd.FileName;
                PathTikerz1.Text = data.pathTIKERS1;
                data.getTickers();
                Listalltk.Items.Refresh();
                Listalltk2.Items.Refresh();
            }
           
        }

        private void selectpath_Click2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                data.pathTIKERS2 = ofd.FileName;
                PathTikerz2.Text = data.pathTIKERS2;
                data.getTickers();
                Listalltk.Items.Refresh();
            }
            
        }

        private void selectpath_x_Copy_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            DialogResult result = folderBrowser.ShowDialog();

            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                //string[] files = Directory.GetFiles(folderBrowser.SelectedPath);
                mydb.Path = folderBrowser.SelectedPath+@"\";
                bdpathx.Text = mydb.Path;
               
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var s = ListBDactual.SelectedIndex;
            if (s<0) return;
            mydb.listtickers.RemoveAt(s);
            ListBDactual.Items.Refresh();

        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string n = Listalltk.SelectedItem as string;
            
            if (!mydb.listtickers.Contains(n))
            mydb.listtickers.Add(n);
            ListBDactual.Items.Refresh();

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            mes.addLOG("ОТКЛЮЧЕНА запись в базу данных");
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            mes.addLOG("Включена запись в базу данных");
        }
    }
}
