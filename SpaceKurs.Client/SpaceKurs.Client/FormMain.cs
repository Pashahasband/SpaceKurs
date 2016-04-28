﻿namespace SpaceKurs.Client
{
    using System;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Threading;
    using System.Windows.Forms;

    using Microsoft.AspNet.SignalR.Client;

    public partial class FormMain : Form
    {
        static private Socket Client;
        private IPAddress ip = null;
        private int port = 0;
        private Thread th;
        WebClient dwprewie = new WebClient();
        //Video video;
        /// <summary>
        /// This name is simply added to sent messages to identify the user; this 
        /// sample does not include authentication.
        /// </summary>
        /// 
        private String UserName { get; set; }
        private IHubProxy HubProxy { get; set; }
        //const string ServerURI = "http://192.168.0.198:8080/signalr";
        private HubConnection Connection { get; set; }
        /// <summary>
        /// Creates and connects the hub connection and hub proxy. This method
        /// is called asynchronously from SignInButton_Click.
        /// </summary>
        private async void ConnectAsync(string IP, string PORT)
        {
            Connection = new HubConnection("http://" + IP + ":" + PORT+ "/signalr");
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
           /* HubProxy.On<string, string>("AddMessage", (name, message) =>
                this.Invoke((Action)(() =>
                    RichTextBoxConsole.AppendText(String.Format("{0}: {1}" + Environment.NewLine, name, message))
                ))
            );*/

            HubProxy.On<Guid>(
                "OnNewImageReceived",
                (id) => this.Invoke(
                    (Action)(() =>
                    {
                        //TODO Вот тут должна быть обработка оповещения о новой картиночке
                        //1. Надо сделать метод, который будет скачивать превьюшечку и оповещать об этом.
                        //HttpClient dwprewie = new HttpClient();
                        //WebClient dwprewie = new WebClient();
                        dwprewie.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(FileDownloadComplete);
                        // pictureBox1 = dwprewie.Get("http://" + IP + ":" + PORT + "/api/images");
                        Uri imageuri = new Uri("http://" + IP + ":" + PORT + "/api/images");
                        dwprewie.DownloadFileAsync(imageuri, "MyDownloadOmage.png");
                        //Открываем файл картинки...
                        System.IO.FileStream fs = new System.IO.FileStream("MyDownloadOmage.png", System.IO.FileMode.Open);
                        System.Drawing.Image img = System.Drawing.Image.FromStream(fs);
                        fs.Close();
                        //Помещаем исходное изображение в PictureBox1
                        pictureBox1.Image = img;
                        
                        label1.Text = "Появилось новое изображение, хотите ли вы скачать его себе на ПК?";
                        buttonyes.Visible = true;
                        buttonno.Visible = true;

                        //2. Повесить куда-нибудь загрузку всей картинки, когда будет получен апрув

                        //3. 
                    })));
            try
            {
                await Connection.Start();
            }
            catch (HttpRequestException)
            {
                //StatusText.Text = "Unable to connect to server: Start server before connecting clients.";
                //No connection: Don't enable Send button or show chat UI
                return;
            }

            //Activate UI
            /*SignInPanel.Visible = false;
            ChatPanel.Visible = true;
            ButtonSend.Enabled = true;
            TextBoxMessage.Focus();
            RichTextBoxConsole.AppendText("Connected to server at " + ServerURI + Environment.NewLine);*/
            label1.Text = "Connected to server at " + IP + ":" + PORT + Environment.NewLine;

        }

        /// <summary>
        /// If the server is stopped, the connection will time out after 30 seconds (default), and the 
        /// Closed event will fire.
        /// </summary>
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
           /* this.Invoke((Action)(() => ChatPanel.Visible = false));
            this.Invoke((Action)(() => ButtonSend.Enabled = false));
            this.Invoke((Action)(() => StatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => SignInPanel.Visible = true));*/
        }
        public FormMain()
        {


            InitializeComponent();
            try
            {
                var sr = new StreamReader(@"Client_info/data_info.txt");
                string buffer = sr.ReadToEnd();
                sr.Close();
                string[] connect_info = buffer.Split(':');
                
                buttonyes.Visible = false;
                buttonno.Visible = false;
                ip = IPAddress.Parse(connect_info[0]);
                port = int.Parse(connect_info[1]);
                label1.ForeColor = Color.Green;
                label1.Text = "Настройки: \n IP сервера: " + connect_info[0] + "\n Порт сервера:" + connect_info[1];
                label1.Text = "Connecting to server...";
                ConnectAsync(connect_info[0], connect_info[1].Replace("\r\n", string.Empty));


               // OpenVideo();


                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
                this.Hide();

                //очистить картинку
                pictureBox1.Image = null;
                //pictureBox1.Load(@"Client_info/lena.jpg");

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
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
            Application.Exit();
        }

        private void обАвтореToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("By Pashahasband");
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Connection != null)
            {
                Connection.Stop();
                Connection.Dispose();
            }
        }
        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }
        //private void OpenVideo()
        //{
        //    var openFileDialog = new OpenFileDialog();
        //    openFileDialog.InitialDirectory = Application.StartupPath;
        //    if (openFileDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        int height = pictureBox1.Height;
        //        int width = pictureBox1.Width;
        //        video = new Video(openFileDialog.FileName);
        //        video.Owner = pictureBox1;
        //        pictureBox1.Width = width;
        //        pictureBox1.Height = height;
        //        video.Play();
        //        video.Pause();
        //    }
        //}

        /* private void FileDownloadComplete(object sender, AsyncCompletedEventArgs e)
         {
             MessageBox.Show("Download comleted");
         }*/
        private void FileDownloadComplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download comleted");
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = false;
            }
        }
    }
}
