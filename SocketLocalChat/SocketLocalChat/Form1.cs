using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace SocketLocalChat
{
    public partial class Form1 : Form
    {

       
        

        private void IP_Enter(object sender, EventArgs e)
        {
            if (IP.Text == (String)IP.Tag)
            {
                IP.Text = "";
            }
        }

        private void IP_Leave(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(IP.Text))
            {
                IP.Text = (String)IP.Tag;
            }
        }


        public Form1()
        {
            InitializeComponent();
            //Создаем поток для приема сообщений
            new Thread(new ThreadStart(Receiver)).Start();
            new Thread(new ThreadStart(FileReceiver)).Start();
        }

        //Метод потока
        protected void Receiver()
        {
            //Создаем Listener на порт "по умолчанию"
            TcpListener Listen = new TcpListener(7000);
            //Начинаем прослушку
            Listen.Start();
            //и заведем заранее сокет
            Socket ReceiveSocket;
            while (true)
            {
                try
                {
                    //Пришло сообщение
                    ReceiveSocket = Listen.AcceptSocket();
                    Byte[] Receive = new Byte[256];
                    //Читать сообщение будем в поток
                    using (MemoryStream MessageR = new MemoryStream())
                    {
                        //Количество считанных байт
                        Int32 ReceivedBytes;
                        do
                        {//Собственно читаем
                            ReceivedBytes = ReceiveSocket.Receive(Receive, Receive.Length, 0);
                            //и записываем в поток
                            MessageR.Write(Receive, 0, ReceivedBytes);
                            //Читаем до тех пор, пока в очереди не останется данных
                        } while (ReceiveSocket.Available > 0);
                        //Добавляем изменения в ChatBox
                        ChatBox.BeginInvoke(AcceptDelegate, new object[] { "Received " + Encoding.Default.GetString(MessageR.ToArray()), ChatBox });
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                
            }
        }

        //Метод потока
        protected void FileReceiver()
        {
            //Создаем Listener на порт "по умолчанию"
            TcpListener Listen = new TcpListener(6999);
            //Начинаем прослушку
            Listen.Start();
            //и заведем заранее сокет
            Socket ReceiveSocket;
            while (true)
            {
                try
                {
                    //Пришло сообщение
                    ReceiveSocket = Listen.AcceptSocket();
                    Byte[] Receive = new Byte[256];
                    //Читать сообщение будем в поток
                    using (MemoryStream MessageR = new MemoryStream())
                    {
                        
                        //Количество считанных байт
                        Int32 ReceivedBytes;
                        Int32 Firest256Bytes = 0;
                        String FilePath = "";
                        do
                        {//Собственно читаем
                            ReceivedBytes = ReceiveSocket.Receive(Receive, Receive.Length, 0);
                            //Разбираем первые 256 байт
                            if (Firest256Bytes < 256) 
                            {
                                Firest256Bytes += ReceivedBytes;
                                Byte[] ToStr = Receive;
                                //Учтем, что может возникнуть ситуация, когда они не могу передаться "сразу" все
                                if (Firest256Bytes > 256)
                                {
                                    Int32 Start = Firest256Bytes - ReceivedBytes;
                                    Int32 CountToGet = 256 - Start;
                                    Firest256Bytes = 256;
                                    //В случае если было принято >256 байт (двумя сообщениями к примеру)
                                    //Остаток (до 256) записываем в "путь файла"
                                    ToStr = Receive.Take(CountToGet).ToArray();
                                    //А остальную часть - в будующий файл
                                    Receive = Receive.Skip(CountToGet).ToArray();
                                    MessageR.Write(Receive, 0, ReceivedBytes);
                                }
                                //Накапливаем имя файла
                                FilePath += Encoding.Default.GetString(ToStr);
                            } else

                            //и записываем в поток
                            MessageR.Write(Receive, 0, ReceivedBytes);
                            //Читаем до тех пор, пока в очереди не останется данных
                        } while (ReceivedBytes == Receive.Length);
                        //Убираем лишние байты
                        String resFilePath = FilePath.Substring(0, FilePath.IndexOf('\0'));
                        using (var File = new FileStream(resFilePath, FileMode.Create))
                        {//Записываем в файл
                            File.Write(MessageR.ToArray(), 0, MessageR.ToArray().Length);
                        }//Уведомим пользователя
                        ChatBox.BeginInvoke(AcceptDelegate, new object[] { "Received: " + resFilePath, ChatBox });
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        /// <summary>
        /// Отправляет сообщение в потоке на IP, заданный в контроле IP
        /// </summary>
        /// <param name="Message">Передаваемое сообщение</param>
        void ThreadSend(object Message)
        {
              try
            {
                  //Проверяем входной объект на соответствие строке
                String MessageText = "";
                if (Message is String)
                {
                    MessageText = Message as String;
                }
                else 
                    throw new Exception("На вход необходимо подавать строку");
               
                  Byte[] SendBytes = Encoding.Default.GetBytes(MessageText);
                //Создаем сокет, коннектимся
                IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(IP.Text), 7000);
                Socket Connector = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Connector.Connect(EndPoint);
                Connector.Send(SendBytes);
                Connector.Close();
                //Изменяем поле сообщений (уведомляем, что отправили сообщение)
               
                 ChatBox.BeginInvoke(AcceptDelegate, new object[] { "Send " + MessageText, ChatBox });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
                
           
              
                
              
            
        }
      
        //Делегат доступа к контролам формы
        delegate void SendMsg(String Text, RichTextBox Rtb);
        
        SendMsg AcceptDelegate = (String Text, RichTextBox Rtb) =>
            {
                Rtb.Text += Text + "\n";     
            };

        //Обработчик кнопки
        private void Send_Click(object sender, EventArgs e)
        {
            
            new Thread(new ParameterizedThreadStart(ThreadSend)).Start(Message.Text);          
        }

        private void button1_Click(object sender, EventArgs e)
        {//Отправляем файл
            //Добавим на форму OpenFileDialog и вызовем его
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Коннектимся
                IPEndPoint EndPoint = new IPEndPoint(IPAddress.Parse(IP.Text), 6999);
                Socket Connector = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Connector.Connect(EndPoint);
                //Получаем имя из полного пути к файлу
                StringBuilder FileName = new StringBuilder(openFileDialog1.FileName);
                //Выделяем имя файла
                int index = FileName.Length - 1;
                while (FileName[index] != '\\' && FileName[index] != '/')
                {
                    index--;
                }
                //Получаем имя файла
                String resFileName = "";
                for (int i = index + 1; i < FileName.Length; i++)
                    resFileName += FileName[i];
                //Записываем в лист
                List<Byte> First256Bytes = Encoding.Default.GetBytes(resFileName).ToList();
                Int32 Diff = 256 - First256Bytes.Count;
                //Остаток заполняем нулями
                for (int i = 0 ; i < Diff; i ++ )
                    First256Bytes.Add(0);
               //Начинаем отправку данных
                Byte[] ReadedBytes = new Byte[256];
                using (var FileStream = new FileStream(openFileDialog1.FileName,FileMode.Open))
                {
                    using (var Reader = new BinaryReader(FileStream))
                    {
                           Int32 CurrentReadedBytesCount;
                        //Вначале отправим название файла
                        Connector.Send(First256Bytes.ToArray());
                        do{
                            //Затем по частям - файл
                            CurrentReadedBytesCount = Reader.Read(ReadedBytes, 0, ReadedBytes.Length);
                            Connector.Send(ReadedBytes,CurrentReadedBytesCount,SocketFlags.None);
                        }
                        while (CurrentReadedBytesCount == ReadedBytes.Length);
                    }
                }
                //Завершаем передачу данных
                Connector.Close();
            }
        }
    }
}
