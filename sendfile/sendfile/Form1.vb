Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Public Class Form1
    Public fname As String
    Public fullname As String
    Public listener1 As TcpListener
    Public messegeThread As System.Threading.Thread = New System.Threading.Thread(AddressOf messege)
    Private serverThread As System.Threading.Thread
    Public data1 As String
    Public Sub New()
        Me.InitializeComponent()
        messegeThread.IsBackground = True
        messegeThread.Start()
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        'Проверка размера файла, если < 45 мб, то все ок
        Dim OFD As New OpenFileDialog With {.Title = "Выберите файл"}
        If OFD.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim file As New System.IO.FileInfo(OFD.FileName)
            If file.Length > 50000000 Then
                MsgBox("Размер файла слишком велик!")
            Else
                fullname = OFD.FileName 'Путь к отправляемому файлу
                client() 'Запуск процедуры отправки файла
            End If
        End If
    End Sub
    Sub client()
        'Вырезаем из полного пути к файлу только его имя и расширение
        Dim Splitingres As String
        Dim str1() As String
        str1 = Split(fullname, "\")
        Splitingres = str1(UBound(str1))
        fname = Splitingres ' Передаем имя файла в переменную fname
        Dim send As New Socket(AddressFamily.InterNetwork, _
        SocketType.Stream, ProtocolType.Tcp)
        Dim bytes(1024) As Byte
        'ну создаем сокет, байты, получаем ip из текствокса
        Dim ip As System.Net.IPAddress = System.Net.IPAddress.Parse(textBox1.Text)
        Dim remoteEP As New IPEndPoint(ip, 27015) 'Получаем конечную точку ip/port
        send.Connect(remoteEP) 'конектимся
        Dim message() As Byte = Encoding.UTF8.GetBytes(fname) 'Преводим в байты короткое имя файла
        Dim bytesSent As Integer = send.Send(message) ' Посылаем его
        Dim client As TcpClient = New TcpClient(ip.ToString, 27020) 'Создаем ТСР клиент на другом порту
        Dim Stream As NetworkStream = client.GetStream()
        Dim writer As BinaryWriter = New BinaryWriter(Stream)
        Dim fileData() As Byte = New [Byte]() {}
        fileData = File.ReadAllBytes(fullname) 'Ну там выше все понятно,
        'тут читаем все байты из отправляемого файла и помещаем в переменную fileData
        writer.Write(fileData.Length)
        writer.Write(fileData) ' Пишем в соединение BinaryWriter ом все байты.
        writer.Close()
        send.Shutdown(SocketShutdown.Both)
        send.Close()


    End Sub

    Private Sub messege()
        ' Тут мы все слухаем, пока не придет имя файла
        Dim bytes() As Byte = New [Byte](1024) {}
        Dim listener As New Socket(AddressFamily.InterNetwork, _
        SocketType.Stream, ProtocolType.Tcp)
        Dim remoteEP As New IPEndPoint(IPAddress.Any, 27015)
        listener.Bind(remoteEP)
        listener.Listen(10)
        While True
            Dim handler As Socket = listener.Accept()
            data1 = Nothing
            While True
                'Получаем имя файла в data1
                bytes = New Byte(1024) {}
                Dim bytesRec As Integer = handler.Receive(bytes)
                data1 += Encoding.UTF8.GetString(bytes, 0, bytesRec)
                listener1 = New TcpListener(IPAddress.Any, 27020)
                listener1.Start() 'Создаем и запускаем TcpListener для принятия файла
                If label1.InvokeRequired Then 'Выводим сообщение в метку, напрямую нельзя,
                    'Т.к. мы в другом потоке, поэтому обращаемся к ней через Invoke
                    label1.Invoke(New Threading.ThreadStart(AddressOf lab))
                Else
                    lab()
                End If
                Exit While
            End While
            handler.Shutdown(SocketShutdown.Both)
            handler.Close()
        End While
    End Sub
    Sub lab()
        label1.Text = "Бери пака дают!"
    End Sub
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        'По нажатию кнопки получаем имя файла из переменной data1
        'Разрезаем его на имя и расширение
        Dim spl() As String
        spl = Split(data1, ".")
        Dim SFD As New SaveFileDialog With {.Filter = "Файлы (*." & spl(1) & ")|*." & spl(1) & "", .Title = "Куда сохранить файл?"}
        'Ну это сейффайлдиалог
        SFD.FileName = spl(0) 'Имя файла
        If SFD.ShowDialog = Windows.Forms.DialogResult.OK Then
            Dim client As TcpClient = listener1.AcceptTcpClient()
            Dim Stream As NetworkStream = client.GetStream()
            Dim reader As BinaryReader = New BinaryReader(Stream) ' Там все то же только тут BinaryReader для чтения байтов из потока 
            Dim fileLength As Integer = reader.ReadInt32()
            Dim fileData() As Byte = New [Byte]() {}
            fileData = reader.ReadBytes(fileLength)
            File.WriteAllBytes(SFD.FileName, fileData) 'Пишем байт в файл 
            listener1.Stop() ' Останавливаем листнер
            data1 = Nothing
            Label1.Text = ""
        End If
    End Sub
End Class
