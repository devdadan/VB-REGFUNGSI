Imports MySql.Data.MySqlClient
Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Net.NetworkInformation
Imports System.Windows.Forms

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
            MsgBox(ex.Message & vbCrLf & ex.StackTrace)
        Finally
            If con IsNot Nothing AndAlso con.State = ConnectionState.Open Then
                con.Close()
            End If
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
    Public Shared Function GetServer(user As String, period As String) As String
        Dim Pass = GetVersi(user, period)
        Return Pass
    End Function
    Public Shared Function GetVersi(User As String, periode As String) As String
        Dim Period As String = periode
        Return Versi(Period.Substring(0, 4) & "-" & Period.Substring(4, 2) & "-" & Period.Substring(6, 2), User.ToLower())
    End Function
    Public Shared Function Versi(Periode As String, User As String) As String

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


End Class
