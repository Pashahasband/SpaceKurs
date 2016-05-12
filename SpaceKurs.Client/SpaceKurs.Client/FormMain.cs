namespace SpaceKurs.Client
{
    using System;
    using System.ComponentModel;
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
        static Guid imageId;
        static string imageExtension;
        static string serverIp;
        static string serverExtension;
        static string serverPort;
        private WebClient webClient = new WebClient();
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
            Connection = new HubConnection("http://" + IP + ":" + PORT + "/signalr");
            //Connection = new HubConnection("http://localhost:8080/signalr");
            Connection.Closed += Connection_Closed;
            HubProxy = Connection.CreateHubProxy("MyHub");
            //Handle incoming event from server: use Invoke to write to console from SignalR's thread
            /* HubProxy.On<string, string>("AddMessage", (name, message) =>
                 this.Invoke((Action)(() =>
                     RichTextBoxConsole.AppendText(String.Format("{0}: {1}" + Environment.NewLine, name, message))
                 ))
             );*/

            HubProxy.On<Guid, string>(
                "OnNewImageReceived",
                (id, extension) => this.Invoke(
                    (Action)(() =>
                    {
                        imageId = id;
                        imageExtension = extension;

                        serverIp = IP;
                        serverPort = PORT;
                        serverExtension = extension;
                        this.webClient.DownloadFileCompleted += this.FileDownloadComplete;
                        var imageUri = new Uri(string.Format("http://{0}:{1}/api/previews/{2}", IP, PORT, id));
                        //this.webClient.DownloadFileAsync(imageUri, string.Format("{0}.jpg", id));  //{1}", id,typeimage);
                        const bool isNotificationNeeded = true;
                        this.webClient.DownloadFileAsync(imageUri, string.Format("{0}{1}", id, extension), isNotificationNeeded);  //{1}", id,typeimage);

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
            label1.Text = string.Format("Connected to server at {0}:{1}{2}", IP, PORT, Environment.NewLine);

        }

        /// <summary>
        /// If the server is stopped, the connection will time out after 30 seconds (default), and the 
        /// Closed event will fire.
        /// </summary>
        private void Connection_Closed()
        {
            //Deactivate chat UI; show login UI. 
            /* this.Invoke((Action)(() => ChatPanel.Visible = false));
             this.Invoke((Action)(() => ButtonSend.Enabled = false));*/
            this.Invoke((Action)(() => label1.Text = "You have been disconnected."));
            /*this.Invoke((Action)(() => SignInPanel.Visible = true));*/
        }
        public FormMain()
        {


            InitializeComponent();
            try
            {
                string buffer;
                using (var sr = new StreamReader(@"Client_info/data_info.txt"))
                {
                    buffer = sr.ReadToEnd();
                }
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

        private void FileDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            //Открываем файл картинки...
            var image = Image.FromFile(string.Format("{0}{1}", imageId, imageExtension));
            int width = image.Width;
            int height = image.Height;
            pictureBox1.Width = width;
            pictureBox1.Height = height;
            //Помещаем исходное изображение в PictureBox1
            pictureBox1.Image = image;

            var isNotificationNeeded = (bool)e.UserState;
            if (isNotificationNeeded)
            {
                this.label1.Text = "Появилось новое изображение, хотите ли вы скачать его себе на ПК?";
                this.buttonyes.Visible = true;
                this.buttonno.Visible = true;
                this.notifyIcon1_Click(sender, e);
            }
            //MessageBox.Show("Download comleted");
        }

        private void FileDownloadCompleteintermediate(object sender, AsyncCompletedEventArgs e)
        {
            //Открываем файл картинки...
            var image = Image.FromFile(string.Format("{0}_intermediate{1}", imageId, serverExtension));
            int width = image.Width;
            int height = image.Height;
            pictureBox1.Width = width;
            pictureBox1.Height = height;
            //Помещаем исходное изображение в PictureBox1
            pictureBox1.Image = image;

                this.label1.Text = "Происходит загрузка изображения, отображается промежуточная степень";
                this.buttonyes.Visible = false;
                this.buttonno.Visible = false;
                this.buttonyes1.Visible = true;
                this.buttonno1.Visible = true;
        }
        private void FileDownloadCompleteimage(object sender, AsyncCompletedEventArgs e)
        {
            //Открываем файл картинки...
            var image = Image.FromFile(string.Format("DownloadedNEWImage{0}", serverExtension));
            int width = image.Width;
            int height = image.Height;
            pictureBox1.Width = width;
            pictureBox1.Height = height;
            //Помещаем исходное изображение в PictureBox1
            pictureBox1.Image = image;

            this.label1.Text = "Изображение загружено на ваш ПК";
            this.buttonyes.Visible = false;
            this.buttonno.Visible = false;
            this.buttonyes1.Visible = false;
            this.buttonno1.Visible = false;
            this.buttonminimum.Visible = true;

        }
        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.WindowState = FormWindowState.Maximized;
                this.ShowInTaskbar = true;
                notifyIcon1.Visible = true;
            }
        }

        private void buttonyes_Click(object sender, EventArgs e)
        {
            const bool isNotificationNeeded = false;

            //this.webClient.DownloadFileCompleted += this.FileDownloadComplete;
            // pictureBox1 = webClient.Get("http://" + IP + ":" + PORT + "/api/images");
            this.webClient.DownloadFileCompleted += this.FileDownloadCompleteintermediate;
            var imageUri = new Uri(string.Format("http://{0}:{1}/api/intermediate/{2}", serverIp, serverPort, imageId));
            this.webClient.DownloadFileAsync(imageUri, string.Format("{0}_intermediate{1}", imageId, serverExtension), isNotificationNeeded);


            //imageUri = new Uri(string.Format("http://{0}:{1}/api/images/{2}", serverIp, serverPort, imageId));
            //this.webClient.DownloadFileAsync(imageUri, string.Format("DownloadedNEWImage{0}", serverExtension), isNotificationNeeded);

          /*  image = Image.FromFile(string.Format("DownloadedNEWImage{0}", serverExtension));
            pictureBox1.Width = image.Width;
            pictureBox1.Height = image.Height;
            pictureBox1.Image = image;*/


            /*pictureBox1.Image = null;
            label1.Text = string.Format("Connected to server at {0}:{1}{2}", serverIp, serverPort, Environment.NewLine);
            buttonyes.Visible = false;
            buttonno.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;*/
            //System.IO.File.Delete("DownloadedImage.jpg");
            //notifyIcon1.Visible = true;
            //this.Hide();

        }

        private void buttonno_Click(object sender, EventArgs e)
        {

            pictureBox1.Image = null;
            label1.Text = string.Format("Connected to server at {0}:{1}{2}", serverIp, serverPort, Environment.NewLine);
            buttonyes.Visible = false;
            buttonno.Visible = false;
            buttonyes1.Visible = false;
            buttonno1.Visible = false;
            buttonminimum.Visible = false;
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            //System.IO.File.Delete("DownloadedImage.jpg");
            // notifyIcon1.Visible = true;
            //this.Hide();


        }

        private void buttonyes1_Click(object sender, EventArgs e)
        {
            const bool isNotificationNeeded = false;
            this.webClient.DownloadFileCompleted += this.FileDownloadCompleteimage;
            var imageUri = new Uri(string.Format("http://{0}:{1}/api/images/{2}", serverIp, serverPort, imageId));
            this.webClient.DownloadFileAsync(imageUri, string.Format("DownloadedNEWImage{0}", serverExtension), isNotificationNeeded);
        }

        private void buttonno1_Click(object sender, EventArgs e)
        {
            buttonno_Click(sender, e);
        }

        private void buttonminimum_Click(object sender, EventArgs e)
        {
            buttonno_Click(sender, e);
        }
    }
}
