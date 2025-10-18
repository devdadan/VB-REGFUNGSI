Imports System.IO
Imports System.Net
Imports System.Text
Imports Microsoft.VisualBasic.CompilerServices
Imports Newtonsoft.Json
Imports System.Net.NetworkInformation
Imports System.Windows.Forms

Public Class Form1
    Public snik As String
    Public spass As String
    Public nama As String
    Public result As String()
    Private url1, url2, url3 As String
    Public Sub New()
        result = New String(9) {}
        InitializeComponent()

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            Button1.Text = "Loading .."
            Dim flag As Boolean = Operators.CompareString(Me.txtnik.Text, "", False) = 0
            If flag Then
                Interaction.MsgBox("NIK harus diisi!!", MsgBoxStyle.OkOnly, Nothing)
            Else
                Dim flag2 As Boolean = Operators.CompareString(Me.txtpass.Text, "", False) = 0
                If flag2 Then
                    Interaction.MsgBox("Password harus diisi!!", MsgBoxStyle.OkOnly, Nothing)
                Else
                    Me.snik = Me.txtnik.Text
                    Me.spass = Me.txtpass.Text

                    Me.login()
                End If
            End If
        Catch ex As Exception
            MsgBox(ex.Message)
        Finally
            Dim flag4 As Boolean = Operators.CompareString(Me.result(0), "0", False) <> 0
            If flag4 Then
                MyBase.Close()
            End If
        End Try
        Button1.Text = "LOGIN"
    End Sub

    Private Sub login()
        Try

            ServiceLoginESS(Me.snik, Me.spass, Me.url1)

        Catch ex As Exception

        End Try

    End Sub
    Public Function PostLoginData(nik As String, name As String, plogin As String) As String
        Try
            Dim url As String = "http://192.168.190.64:8080/api/save-login"
            Dim request As HttpWebRequest = CType(WebRequest.Create(url), HttpWebRequest)
            request.Method = "POST"
            request.ContentType = "application/json"

            Dim postData As String = $"{{""nik"":""{nik}"",""name"":""{name}"",""plogin"":""{plogin}""}}"
            Dim byteArray As Byte() = Encoding.UTF8.GetBytes(postData)
            Using dataStream As Stream = request.GetRequestStream()
                dataStream.Write(byteArray, 0, byteArray.Length)
            End Using
            Using response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                Using reader As New StreamReader(response.GetResponseStream())
                    Dim responseText As String = reader.ReadToEnd()
                    Return responseText
                End Using
            End Using
        Catch ex As Exception
            Fungsi.Tulislog(ex.Message)
        End Try

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Fungsi.GetServer("root", "20250501")
        url1 = "http://172.20.20.41:8070/LoginESSREST.svc/v4/LoginESSSD7"
        url2 = "http://172.20.20.41:8070/LoginESSREST.svc/LoginESS"
        url3 = "http://172.20.20.41:8070/LoginESSREST.svc/LoginESS"
    End Sub

    Private Sub txtpass_TextChanged(sender As Object, e As EventArgs) Handles txtpass.TextChanged

    End Sub

    Private Function ServiceLoginESS(nik As String, pass As String, url As String) As String
        Dim user As user = New user()
        Dim flag As Boolean = False
        Dim text As String = ""
        Dim array As String() = url.ToString().Split(New Char() {"/"c})
        Dim text2 As String = array(2)
        Dim array2 As String() = text2.ToString().Split(New Char() {":"c})
        text = array2(0)
        Try
            flag = My.Computer.Network.Ping(text, 5000)
        Catch ex As Exception

        End Try
        Dim flag2 As Boolean = Not flag
        If flag2 Then
            MsgBox("Koneksi ke server terputus, silahkan coba kembali!")
            Me.result(0) = "99"
            Me.result(1) = "Error, Koneksi ke server terputus"
            Me.result(2) = nik
        Else
            user.nik = nik
            user.pass = pass
            Dim parameter As String = JsonConvert.SerializeObject(user)

            Dim text3 As String = SendHTTP(url, parameter, "", Conversions.ToString(60))
            Dim ess As ESS = JsonConvert.DeserializeObject(Of ESS)(text3)
            Dim data As Pegawai = ess.Data
            Dim flag4 As Boolean = ess.MESSAGE.ToLower().Contains("sukses")
            If flag4 Then
                result(0) = "1"
                Me.result(1) = ess.MESSAGE.ToString()
                Me.result(2) = data.NIK.ToString()
                Me.result(3) = data.NAMA.ToString()
                Me.result(4) = data.KODECABANG.ToString()
                Me.result(5) = data.CABANG.ToString()
                Me.result(6) = data.KODEBAGIAN.ToString()
                Me.result(7) = data.BAGIAN.ToString()
                Me.result(8) = data.JABATAN.ToString()
            Else
                Me.result(0) = "99"
                Me.result(1) = ess.MESSAGE.ToString()
                Me.result(2) = nik
                Me.result(3) = ""
                Me.result(4) = ""
                Me.result(5) = ""
                Me.result(6) = ""
                Me.result(7) = ""
                Me.result(8) = ""
            End If
        End If

    End Function
    Public Shared Function SendHTTP(URL As String, Parameter As String, userAgent As String, TimeOutWeb As String) As String
        Dim text As String = ""
        Dim s As String = If(("{""user"":" + Parameter), "")

        Try
            Dim httpWebRequest As HttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
            httpWebRequest.ContentType = "application/json"
            httpWebRequest.Timeout = CInt(Math.Round(Conversions.ToDouble(TimeOutWeb) * 1000.0))
            httpWebRequest.Method = "POST"
            Dim asciiencoding As ASCIIEncoding = New ASCIIEncoding()
            Dim encoding As Encoding = asciiencoding
            Dim bytes As Byte() = encoding.GetBytes(s)
            httpWebRequest.ContentLength = CLng(bytes.Length)
            Dim flag2 As Boolean = Operators.CompareString(userAgent, "", False) <> 0
            If flag2 Then
                httpWebRequest.UserAgent = userAgent
            End If
            httpWebRequest.KeepAlive = True
            Dim requestStream As Stream = httpWebRequest.GetRequestStream()
            requestStream.Write(bytes, 0, bytes.Length)
            requestStream.Close()
            Dim httpWebResponse As HttpWebResponse = CType(httpWebRequest.GetResponse(), HttpWebResponse)
            Dim streamReader As StreamReader = New StreamReader(httpWebResponse.GetResponseStream())
            text = streamReader.ReadToEnd()
        Catch ex As WebException
            Fungsi.Tulislog("SendHTTP ex : Error " + ex.Message + ex.StackTrace)
            text = ""
        Catch ex2 As Exception
            Fungsi.Tulislog("SendHTTP exx : Error " + ex2.Message + ex2.StackTrace)
            text = ""
        Finally
        End Try
        Return text.Trim()
    End Function

    Private Sub txtpass_KeyDown(sender As Object, e As KeyEventArgs) Handles txtpass.KeyDown
        If e.KeyCode = Keys.Enter Then
            Button1.PerformClick()
        End If
    End Sub
End Class