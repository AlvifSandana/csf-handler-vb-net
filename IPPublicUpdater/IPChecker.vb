Imports System.ComponentModel
Imports System.Net
Imports System.Text

Module IPChecker
    Private cmd As String = ""

    Sub Main()
        Dim publicIP As String = GetPublicIP()
        Console.WriteLine("Public IP: " & publicIP)
    End Sub

    Sub SetCmd(command As String)
        cmd = command
    End Sub

    Function GetPublicIP() As String
        Try
            ' Mengatur versi TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            Using client As New WebClient()
                ' Mengirimkan permintaan GET ke layanan api.ipify.org
                Dim publicIP As String = client.DownloadString("http://api.ipify.org")
                Return publicIP
            End Using
        Catch ex As Exception
            Return "Error: " & ex.Message
        End Try
    End Function

    Function RunBatFile(filePath As String) As String
        Try
            ' Membuat instance baru dari kelas Process
            Dim process As New Process()
            ' Mengatur informasi untuk proses
            Dim startInfo As New ProcessStartInfo With {
                .FileName = filePath,
                .RedirectStandardOutput = True,
                .UseShellExecute = False,
                .CreateNoWindow = True
            }
            process.StartInfo = startInfo
            ' Menjalankan proses dan menangkap output
            process.Start()
            Dim output As String = process.StandardOutput.ReadToEnd()
            process.WaitForExit()

            Dim lines() As String = output.Split(New String() {vbCrLf}, StringSplitOptions.None)
            Dim line2() As String = lines(1).Split(New String() {vbLf}, StringSplitOptions.None)
            Dim lastLine As String = line2(line2.Length - 2)
            Console.WriteLine(lines)
            Return lastLine
        Catch ex As Exception
            Return "Error: " & ex.Message
        End Try
    End Function
End Module
