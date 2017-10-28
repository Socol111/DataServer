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
            if (Ignore.Text == "") return;
            data.eliminate.Add(Ignore.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            data.eliminate.RemoveAt(Lst1.SelectedIndex);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

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
                //Update.GuiElement("PathTiker");
                data.getTickers();
            }
           
        }

        private void selectpath_Click2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                data.pathTIKERS2 = ofd.FileName;
                PathTikerz2.Text = data.pathTIKERS2;
                //Update.GuiElement("PathTiker");
                data.getTickers();
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
            mydb.listtickers.RemoveAt(ListBDact.SelectedIndex);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            string n = ListBD.SelectedItem as string;
            
            mydb.listtickers.Add("dadas");
           
        }
    }
}
