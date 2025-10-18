Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Net.NetworkInformation
Imports System.Windows.Forms
Imports System.Net
Imports Microsoft.VisualBasic.CompilerServices
Imports Microsoft.VisualBasic.FileIO
Imports System.Net.Sockets
Imports Newtonsoft.Json.Linq
Imports Newtonsoft.Json

Public Class Fungsi
    Dim Sql As String
    Dim cmd As MySqlCommand
    Public passtok, passtok2, passtok3 As String
    Public Shared con As MySqlConnection
    Dim mdr As MySqlDataReader
    Public Sub ambilpass()
        Try
            koneklokal()
            Sql = "SELECT period, period2, period3 FROM versi"
            cmd = New MySqlCommand(Sql, con)
            mdr = cmd.ExecuteReader()
            If mdr.Read() Then
                Dim period As String = mdr.GetString(0)
                Dim period2 As String = mdr.GetString(1)
                Dim period3 As String = mdr.GetString(2)

                passtok = GetServer("root", period)
                passtok2 = GetServer("root", period2)
                passtok3 = GetServer("root", period3)
            End If
            mdr.Close()
            con.Close()
        Catch ex As Exception
            MsgBox(ex.Message + ex.StackTrace)
        End Try
    End Sub
    Public Shared Function GetPass() As String
        Dim sector As String = String.Empty
        Try
            koneklokal()
            Dim Sql As String = "SELECT concat(period,'|',period2,'|',period3) as pass FROM siedp.versi"
            Using cmd As New MySqlCommand(Sql, con)
                Using mdr As MySqlDataReader = cmd.ExecuteReader()
                    If mdr.Read() Then
                        sector = mdr.GetString(0)
                    End If
                End Using
            End Using




        Catch ex As Exception
            'MsgBox("Gagal mendapatkan sector!" & "Akan ambil dari sector.txt")
        Finally
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
        End Try
        Return sector
    End Function
    Public Shared Function GetPass2() As String
        Dim sector As String = String.Empty
        Try
            Dim apiUrl As String = "http://192.168.190.64:8080/api/sector"

            Using client As New WebClient()
                sector = client.DownloadString(apiUrl)
            End Using

        Catch ex As Exception
        End Try

        Return sector
    End Function
    Public Shared Function CekKoneksi(ipAddress As String) As Boolean
        Try
            Dim pingSender As New Ping()
            Dim reply As PingReply = pingSender.Send(ipAddress)
            If reply.Status = IPStatus.Success Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Shared Function ShowFrmPassword() As String
        Dim result As String = String.Empty
        Try
            Using frmPassword As New Form1()
                frmPassword.ShowDialog()
                result = frmPassword.snik
                Tulislog(frmPassword.snik)
            End Using
        Catch ex As Exception

        End Try
        Return result
    End Function


    Public Shared Function GetServer(user As String, period As String) As String
        Dim Pass = GetVersi(user, period)
        Return Pass
    End Function
    Public Shared Function GetVersi(User As String, periode As String) As String
        Dim Period As String = periode
        Return Versi(Period.Substring(0, 4) & "-" & Period.Substring(4, 2) & "-" & Period.Substring(6, 2), User.ToLower())
    End Function
    Public Shared Function Versi(Periode As String, User As String) As String
        'MsgBox(User & vbCrLf & DefaultVer(User))
        Dim text = Encrypt(User & " : " & DefaultVer(User), Date.Parse(Periode).ToString("yyyy-MM-dd"), User)
        text = text.Replace("'", "")
        Return text.Substring(10) & text.Substring(0, 10)
    End Function
    Public Shared Function DefaultVer(User As String) As String
        Dim result = ""
        If Equals(User.ToUpper().Trim(), "root".ToUpper().Trim()) Then
            result = Decrypt("AYBOzt4YQ4zHbTY2bRiajA==", User, "12345")
        End If
        If Equals(User.ToUpper().Trim(), "kasir".ToUpper().Trim()) Then
            result = Decrypt("kRUXVE+bgdwh3Ptfbiw9yg==", User, "12345")
        End If
        If Equals(User.ToUpper().Trim(), "app".ToUpper().Trim()) Then
            result = Decrypt("hEUSKSjtKQ8dgPIwNIU2Dg==", User, "12345")
        End If
        If Equals(User.ToUpper().Trim(), "edp".ToUpper().Trim()) Then
            result = Decrypt("21TRmBPTF5Vs2b3mM6FnrA==", User, "12345")
        End If
        If Equals(User.ToUpper().Trim(), "dbe".ToUpper().Trim()) Then
            result = Decrypt("hGBKT3Q+fgNe9sE4lU2Osw==", User, "12345")
        End If
        Return result
    End Function

    Public Shared Function Encrypt(plainText As String, passPhrase As String, saltValue As String) As String
        Dim strHashName = "SHA1"
        Dim iterations = 2
        Dim s = "@1B2c3D4e5F6g7H8"
        Dim num = 256
        Dim bytes = Encoding.ASCII.GetBytes(s)
        Dim bytes2 = Encoding.ASCII.GetBytes(saltValue)
        Dim bytes3 = Encoding.UTF8.GetBytes(plainText)
        Dim passwordDeriveBytes As PasswordDeriveBytes = New PasswordDeriveBytes(passPhrase, bytes2, strHashName, iterations)
        Dim bytes4 As Byte() = passwordDeriveBytes.GetBytes(Math.Round(num / 8.0))
        Dim transform As ICryptoTransform = New RijndaelManaged With {
.Mode = CipherMode.CBC
}.CreateEncryptor(bytes4, bytes)
        Dim memoryStream As MemoryStream = New MemoryStream()
        Dim cryptoStream As CryptoStream = New CryptoStream(memoryStream, transform, CryptoStreamMode.Write)
        cryptoStream.Write(bytes3, 0, bytes3.Length)
        cryptoStream.FlushFinalBlock()
        Dim inArray As Byte() = memoryStream.ToArray()
        memoryStream.Close()
        cryptoStream.Close()
        Return Convert.ToBase64String(inArray)
    End Function
    Public Shared Function Decrypt(cipherText As String, passPhrase As String, saltValue As String) As String
        Dim strHashName = "SHA1"
        Dim iterations = 2
        Dim s = "@1B2c3D4e5F6g7H8"
        Dim num = 256
        Dim bytes = Encoding.ASCII.GetBytes(s)
        Dim bytes2 = Encoding.ASCII.GetBytes(saltValue)
        Dim array = Convert.FromBase64String(cipherText)

        Dim passwordDeriveBytes As PasswordDeriveBytes = New PasswordDeriveBytes(passPhrase, bytes2, strHashName, iterations)
        Dim bytes3 As Byte() = passwordDeriveBytes.GetBytes(Math.Round(num / 8.0))
        Dim transform As ICryptoTransform = New RijndaelManaged With {
.Mode = CipherMode.CBC
}.CreateDecryptor(bytes3, bytes)
        Dim memoryStream As MemoryStream = New MemoryStream(array)
        Dim cryptoStream As CryptoStream = New CryptoStream(memoryStream, transform, CryptoStreamMode.Read)
        Dim array2 = New Byte(array.Length + 1 - 1) {}
        Dim count As Integer = cryptoStream.Read(array2, 0, array2.Length)
        memoryStream.Close()
        cryptoStream.Close()
        Return Encoding.UTF8.GetString(array2, 0, count)
    End Function
    Public Shared Sub koneklokal()
        Try
            Dim Mysql As String = "server=192.168.190.100;uid=root;pwd=15032012;database=siedp;Pooling=False;Connection Timeout=120;"
            con = New MySqlConnection(Mysql)
            If con.State = ConnectionState.Closed Then
                con.Open()
            End If
        Catch ex As Exception
            Tulislog("GAGAL KONEK: " & ex.Message)
        End Try
    End Sub
    Public Shared Sub koneklokal2(db As String)
        Try
            Dim Mysql As String = "server=192.168.190.100;uid=root;pwd=15032012;database=" & db & ";Pooling=False;Connection Timeout=120;"
            con = New MySqlConnection(Mysql)
            If con.State = ConnectionState.Closed Then
                con.Open()
            End If
        Catch ex As Exception
            Tulislog("GAGAL KONEK: " & ex.Message)
        End Try
    End Sub
    Public Shared Sub Tulislog(slog As String)
        Try
            Dim thn As String = Format(Now, "yyyy-MM-dd")
            Dim th As String = Format(Now, "yyyyMM")
            Dim thnn As String = Format(Now, "yyyy-MM-dd HH:mm:ss")
            Dim tulis As StreamWriter
            If Not File.Exists(Application.StartupPath & "\TRACELOG_" + th + ".txt") Then
                tulis = File.CreateText(Application.StartupPath & "\TRACELOG_" + th + ".txt")
                tulis.WriteLine("##### LOG PROGRAM #####")
                tulis.WriteLine(thnn & " : " & slog)
                tulis.Flush()
                tulis.Close()
            Else
                tulis = File.AppendText(Application.StartupPath & "\TRACELOG_" + th + ".txt")
                tulis.WriteLine(thnn & " : " & slog)
                tulis.Flush()
                tulis.Close()
            End If
        Catch ex As Exception

        End Try
    End Sub
    Private Shared failedAttempts As Integer = 0
    Private Shared lockoutTime As DateTime = DateTime.MinValue
    Dim resultess As String()
    Public Function HitEss() As String()
        Try

            Dim Text As String
            Text = "http://lb1-pik.indomaret.co.id:8070/LoginESSREST.svc/v4/LoginESSSD7"
            Dim flag4 As Boolean = False
            Dim array As String() = Text.ToString().Split(New Char() {"/"c})
            Dim text2 As String = array(2)
            Dim array2 As String() = text2.ToString().Split(New Char() {":"c})
            Dim hostNameOrAddress As String = array2(0)
            Try
                flag4 = My.Computer.Network.Ping(hostNameOrAddress, 5000)
            Catch ex As Exception

            End Try

            Dim frmEssSD As Form1 = New Form1()
            frmEssSD.ShowDialog()
            resultess = frmEssSD.result

        Catch ex2 As Exception
            Tulislog(ex2.Message + ex2.StackTrace)
            Me.resultess(0) = "99"
            Me.resultess(1) = ex2.Message + ex2.StackTrace
        End Try
        Return Me.resultess
    End Function

    Public Shared Function ApiRequest(Param As String) As String
        Try
            Dim webRequest As WebRequest = WebRequest.Create("http://192.168.190.64:8080/api/SupportRND")
            webRequest.Method = "POST"

            ' ubah jadi JSON request
            Dim bytes As Byte() = Encoding.UTF8.GetBytes(Param)
            webRequest.ContentType = "application/json"
            webRequest.ContentLength = CLng(bytes.Length)

            Using stream As Stream = webRequest.GetRequestStream()
                stream.Write(bytes, 0, bytes.Length)
            End Using

            Dim response As WebResponse = webRequest.GetResponse()
            Using stream As Stream = response.GetResponseStream()
                Using streamReader As New StreamReader(stream)
                    Dim result As String = streamReader.ReadToEnd()
                    response.Close()
                    Return result
                End Using
            End Using

        Catch ex As Exception
            Tulislog("Gagal dapat response API : " & Strings.Left(ex.ToString(), 180))
            Return "ERROR"
        End Try
    End Function
    Public Shared Function Getversiprg(appname As String, useip As Boolean, ip As String) As String
        Try
            Dim tmp As String = ""
            Dim payloadCreate As String
            If Not useip Then
                payloadCreate =
                   "{" &
                   """opt"":""get""," &
                   """sql"":"" SELECT VERSION FROM DB_SCRAP.VERSION_INFO WHERE APPNAME='" + appname + "' """ &
                   "}"
            Else
                payloadCreate =
                "{" &
                   """opt"":""get""," &
                   """sql"":"" SELECT VERSION FROM DB_SCRAP.VERSION_INFO WHERE APPNAME='" + appname + "' AND IP='" + ip + "'""" &
                   "}"
            End If

            Dim response As String = ApiRequest(payloadCreate)
            Dim jsoncheck As JObject = JObject.Parse(response)

            If jsoncheck("success") IsNot Nothing AndAlso jsoncheck("success").Value(Of Boolean)() = True Then
                Dim array As JArray = jsoncheck("data")
                If array.Count > 0 Then
                    tmp = array(0)("VERSION").ToString()
                End If
            End If
            Return tmp
        Catch ex As Exception
            MsgBox("Gagal get versi " + ex.Message)
            Tulislog("GetVersi : " + ex.Message)

        End Try
    End Function
    Public Shared Function Setdata(sql As String) As String
        Try
            Dim result As String = "GAGAL"
            Dim payloadCreate As String
            payloadCreate =
                "{" &
                   """opt"":""set""," &
                   """sql"":"" " + sql.Replace("""", "\""") + " """ &
                   "}"

            Dim responseSet As String = Fungsi.ApiRequest(payloadCreate)
            Dim jsonSet As JObject = JObject.Parse(responseSet)

            If jsonSet("success") IsNot Nothing AndAlso jsonSet("success").Value(Of Boolean)() = True Then
                result = "Success"
            Else
                result = "GAGAL"
            End If
            Return result
        Catch ex As Exception
            MsgBox("Gagal SetData" + ex.Message)
            Tulislog("Gagal SetData" + ex.Message)
        End Try
    End Function
    Public Shared Function Getdata(sql As String) As DataTable
        Try
            Dim result As New DataTable
            Dim payloadCreate As String
            payloadCreate =
                "{" &
                   """opt"":""set""," &
                   """sql"":"" " + sql.Replace("""", "\""") + " """ &
                   "}"

            Dim responseSet As String = Fungsi.ApiRequest(payloadCreate)
            Dim jsonSet As JObject = JObject.Parse(responseSet)

            If jsonSet("success") IsNot Nothing AndAlso jsonSet("success").Value(Of Boolean)() = True Then
                Dim dataArray As JArray = jsonSet("data")
                result = JsonConvert.DeserializeObject(Of DataTable)(dataArray.ToString())
            Else

            End If
            Return result
        Catch ex As Exception
            MsgBox("Gagal Getdata" + ex.Message)
            Tulislog("Gagal Getdata" + ex.Message)
            Return Nothing
        End Try
    End Function
    Public Shared Function getStation() As String
        Dim result As String = ""
        Try
            result = Environment.GetEnvironmentVariable("station")
        Catch ex As Exception
            Tulislog("Gagal baca station : " + ex.ToString())
        End Try
        Return result
    End Function
    Public Shared Function getNamaToko() As String
        Dim result As String = ""
        If File.Exists("D:\BCKMYSQL\UGD\UGD_TOKO.CSV") Then
            Dim str As String = "D:\BCKMYSQL\UGD\"
            Dim textFieldParser As TextFieldParser = New TextFieldParser(str + "UGD_TOKO.CSV")
            textFieldParser.TextFieldType = FieldType.Delimited
            textFieldParser.SetDelimiters(New String() {"|"})
            While Not textFieldParser.EndOfData
                result = textFieldParser.ReadFields()(6)
            End While
            textFieldParser.Close()
        ElseIf File.Exists("E:\CAD\UGD\UGD_TOKO.CSV") Then
            Dim str2 As String = "E:\CAD\UGD\"
            Dim textFieldParser2 As TextFieldParser = New TextFieldParser(str2 + "UGD_TOKO.CSV")
            textFieldParser2.TextFieldType = FieldType.Delimited
            textFieldParser2.SetDelimiters(New String() {"|"})
            While Not textFieldParser2.EndOfData
                result = textFieldParser2.ReadFields()(6)
            End While
            textFieldParser2.Close()
        ElseIf Not Directory.Exists("D:\I-KIOSK") Then
            Tulislog("D:\BCKMYSQL\UGD\UGD_TOKO.CSV tidak ada, harap transfer prodmast")
        End If
        Return result
    End Function
    Public Shared Function getIPAddress() As String
        Dim text As String = ""

        Try
            Dim addressList As IPAddress() = Dns.GetHostEntry(Dns.GetHostName()).AddressList

            For Each ip As IPAddress In addressList
                If ip.AddressFamily = AddressFamily.InterNetwork Then
                    If ip.ToString().StartsWith("10") Then
                        Return ip.ToString()
                    End If

                    ' Kalau belum ada IP disimpan, simpan yg pertama ketemu
                    If String.IsNullOrEmpty(text) Then
                        text = ip.ToString()
                    End If
                End If
            Next
        Catch ex As Exception
            text = "Tidak dapat mendeteksi IP (" & ex.Message & ")"
        End Try

        Return text
    End Function
    Public Shared Function getKdtk() As String
        Dim result As String = ""
        If File.Exists("D:\BCKMYSQL\UGD\UGD_TOKO.CSV") Then
            Dim str As String = "D:\BCKMYSQL\UGD\"
            Dim textFieldParser As TextFieldParser = New TextFieldParser(str + "UGD_TOKO.CSV")
            textFieldParser.TextFieldType = FieldType.Delimited
            textFieldParser.SetDelimiters(New String() {"|"})
            While Not textFieldParser.EndOfData
                result = textFieldParser.ReadFields()(5)
            End While
            textFieldParser.Close()
        ElseIf File.Exists("E:\CAD\UGD\UGD_TOKO.CSV") Then
            Dim str2 As String = "E:\CAD\UGD\"
            Dim textFieldParser2 As TextFieldParser = New TextFieldParser(str2 + "UGD_TOKO.CSV")
            textFieldParser2.TextFieldType = FieldType.Delimited
            textFieldParser2.SetDelimiters(New String() {"|"})
            While Not textFieldParser2.EndOfData
                result = textFieldParser2.ReadFields()(5)
            End While
            textFieldParser2.Close()
        ElseIf Not Directory.Exists("D:\I-KIOSK") Then
            Tulislog("Tidak ditemukan data getKdtk")
        End If
        Return result
    End Function
    Public Shared Function getCabang() As String
        Dim result As String = ""
        If File.Exists("D:\BCKMYSQL\UGD\UGD_TOKO.CSV") Then
            Dim str As String = "D:\BCKMYSQL\UGD\"
            Dim textFieldParser As TextFieldParser = New TextFieldParser(str + "UGD_TOKO.CSV")
            textFieldParser.TextFieldType = FieldType.Delimited
            textFieldParser.SetDelimiters(New String() {"|"})
            While Not textFieldParser.EndOfData
                result = textFieldParser.ReadFields()(37)
            End While
            textFieldParser.Close()
        ElseIf File.Exists("E:\CAD\UGD\UGD_TOKO.CSV") Then
            Dim str2 As String = "E:\CAD\UGD\"
            Dim textFieldParser2 As TextFieldParser = New TextFieldParser(str2 + "UGD_TOKO.CSV")
            textFieldParser2.TextFieldType = FieldType.Delimited
            textFieldParser2.SetDelimiters(New String() {"|"})
            While Not textFieldParser2.EndOfData
                result = textFieldParser2.ReadFields()(37)
            End While
            textFieldParser2.Close()
        ElseIf Not Directory.Exists("D:\I-KIOSK") Then
            Tulislog("Tidak ditemukan data getCabang")
        End If
        Return result
    End Function
End Class
