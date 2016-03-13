using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpaceKurs.Client
{
    public partial class FormMain : Form
    {
        static private Socket Client;
        private IPAddress ip = null;
        private int port = 0;
        private Thread th;
        public FormMain()
        {
            InitializeComponent();
            try
            {
                var sr = new StreamReader(@"Client_info/data_info.txt");
                string buffer = sr.ReadToEnd();
                sr.Close();
                string[] connect_info = buffer.Split(':');
                ip = IPAddress.Parse(connect_info[0]);
                port = int.Parse(connect_info[1]);
                label1.ForeColor = Color.Green;
                label1.Text = "Настройки: \n IP сервера: " + connect_info[0] + "\n Порт сервера:" + connect_info[1];
            }
            catch (Exception ex)
            {
                label1.ForeColor = Color.Red;
                label1.Text = "Настройки не найдены";
                FormSettings form = new FormSettings();
                form.Show();
            }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSettings form = new FormSettings();
            form.Show();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (th != null) th.Abort();
            if (Client != null)
            {
                Client.Close();
            }
            Application.Exit();
        }

        private void обАвтореToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("By Pashahasband");
        }
    }
}
