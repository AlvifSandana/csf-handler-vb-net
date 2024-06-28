Imports System.ComponentModel

Public Class Form1
    Private counter As Integer = 0
    Private ip_public As String = ""
    Private previous_ip As String = ""
    Private isRunning As Boolean = False
    Private cmd As String = ""
    Private WithEvents bgWorker As New BackgroundWorker()
    Private ctxMenu As ContextMenuStrip

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ctxMenu = New ContextMenuStrip()
        ctxMenu.Items.Add("Show", My.Resources.show_icon, AddressOf ShowForm)
        ctxMenu.Items.Add("Exit", My.Resources.exit_icon, Sub() Application.Exit())

        Timer1.Interval = 1000
        NotifyIcon1.BalloonTipTitle = "IP Public Updater"
        NotifyIcon1.BalloonTipText = "Idle..."
        NotifyIcon1.Text = "IP Public Updater - Stopped"
        NotifyIcon1.ContextMenuStrip = ctxMenu
        NotifyIcon1.ShowBalloonTip(1000)

        bgWorker.WorkerSupportsCancellation = True
        bgWorker.WorkerReportsProgress = False
        AddHandler bgWorker.DoWork, AddressOf BgWorker_DoWork
        AddHandler bgWorker.RunWorkerCompleted, AddressOf BgWorker_RunWorkerCompleted
    End Sub
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ip_public = GetPublicIP()
        If ip_public.Contains("Error") Then
            lbl_IP.Text = "IP Public: -"
            Me.Text = "IP Public Updater - Error getting data, please retry!"
        Else
            lbl_IP.Text = "IP Public: " & ip_public
            If Me.Text.Contains("Error") Then
                Me.Text = "IP Public Updater"
            End If
        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        counter += 1
        If counter = 15 Then
            ip_public = GetPublicIP()
            If ip_public.Contains("Error") Then
                lbl_IP.Text = "Error while getting data, retrying..."
            ElseIf Not previous_ip = ip_public Then
                lbl_IP.Text = "IP Public: " & ip_public
                cmd = $"csf -a {ip_public}"
                bgWorker.RunWorkerAsync()
            End If
            counter = 0
            previous_ip = ip_public
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If Timer1.Enabled Then
            Timer1.Stop()
            isRunning = False
            Button2.Text = "Start"
            Button1.Enabled = True
            Me.Text = "IP Public Updater - Stopped"
            NotifyIcon1.Text = "IP Public Updater - Stopped"
            NotifyIcon1.BalloonTipText = "Stopped, idle..."
            NotifyIcon1.ShowBalloonTip(1000)
        Else
            Timer1.Start()
            isRunning = True
            Button2.Text = "Stop"
            Button1.Enabled = False
            Me.Text = "IP Public Updater - Running"
            NotifyIcon1.Text = "IP Public Updater - Running"
            NotifyIcon1.BalloonTipText = "Running..."
            NotifyIcon1.ShowBalloonTip(1000)
        End If
    End Sub

    Private Sub ShowForm()
        Me.ShowInTaskbar = True
        Me.Visible = True
        NotifyIcon1.Visible = False
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If e.Cancel Then
            Return
        End If

        Me.Visible = False
        Me.ShowInTaskbar = False
        NotifyIcon1.Visible = True
    End Sub

    Private Sub Form1_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Visible = False
            Me.ShowInTaskbar = False
            NotifyIcon1.Visible = True
        End If
    End Sub

    Private Sub PictureBox1_Click(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Me.WindowState = FormWindowState.Minimized
        NotifyIcon1.BalloonTipText = "Minimized to System Tray"
        NotifyIcon1.ShowBalloonTip(1000)
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        ShowForm()
    End Sub


    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If Not bgWorker.IsBusy Then
            cmd = "tail /etc/csf/csf.allow"
            bgWorker.RunWorkerAsync()
        Else
            AppendTextToLog("Background worker is busy." & Environment.NewLine)
        End If
    End Sub


    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        ip_public = GetPublicIP()
        If ip_public.Contains("Error") Then
            lbl_IP.Text = "IP Public: -"
            Me.Text = "IP Public Updater - Error getting data, please retry!"
        Else
            lbl_IP.Text = "IP Public: " & ip_public
            If Me.Text.Contains("Error") Then
                Me.Text = "IP Public Updater"
            End If
            cmd = $"csf -a {ip_public}"
            bgWorker.RunWorkerAsync()
        End If
    End Sub

    Public Sub BgWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        ' Konfigurasi plink
        Dim plinkPath As String = "C:\Users\WIP\source\repos\IPPublicUpdater\IPPublicUpdater\bin\Debug\bin\plink.exe"
        Dim host As String = "213.190.4.38"
        Dim port As String = "65001"
        Dim user As String = "root"
        Dim password As String = "H@r@p@nB@ru2024"
        Dim command As String = cmd
        Dim hostkey As String = "ssh-ed25519 255 SHA256:6bFIMJwdcN68o1tX0VzhMqgwMrmMYu37/bDJVR8f8fk" ' Ganti dengan fingerprint yang sesuai

        ' Buat objek ProcessStartInfo
        Dim psi As New ProcessStartInfo()
        psi.FileName = plinkPath
        psi.Arguments = $"-ssh {user}@{host} -P {port} -pw {password} -hostkey ""{hostkey}"" -batch {command}"
        psi.UseShellExecute = False
        psi.RedirectStandardOutput = True
        psi.RedirectStandardError = True
        psi.CreateNoWindow = True

        ' Jalankan plink dan baca output
        Dim process As New Process()
        process.StartInfo = psi
        AddHandler process.OutputDataReceived, AddressOf ProcessOutputHandler
        AddHandler process.ErrorDataReceived, AddressOf ProcessErrorHandler

        Try
            AppendTextToLog("Connecting to VPS..." & Environment.NewLine, 0)
            process.Start()
            process.BeginOutputReadLine()
            process.BeginErrorReadLine()
            process.WaitForExit()
            AppendTextToLog("Connection task completed." & Environment.NewLine, 1)
        Catch ex As Exception
            AppendTextToLog($"Error: {ex.Message}{Environment.NewLine}", 2)
        End Try
    End Sub

    Public Sub BgWorker_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs)
        If e.Error IsNot Nothing Then
            AppendTextToLog($"Error: {e.Error.Message}{Environment.NewLine}")
        Else
            AppendTextToLog("Worker completed successfully." & Environment.NewLine)
        End If
    End Sub

    Private Sub ProcessOutputHandler(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then
            AppendTextToLog($"{e.Data}{Environment.NewLine}")
        End If
    End Sub

    Private Sub ProcessErrorHandler(sender As Object, e As DataReceivedEventArgs)
        If e.Data IsNot Nothing Then
            AppendTextToLog($"Error: {e.Data}{Environment.NewLine}")
        End If
    End Sub

    Function AppendTextToLog(text As String, Optional is_error As Integer = 0)
        If rtbLog.InvokeRequired Then
            rtbLog.Invoke(New Action(Of String)(AddressOf AppendTextToLog), text)
        Else
            rtbLog.AppendText(text)
            rtbLog.Select(rtbLog.TextLength - text.Length, text.Length)
            If is_error = 2 Then
                rtbLog.SelectionColor = Color.Red
            ElseIf is_error = 1 Then
                rtbLog.SelectionColor = Color.Green
            Else
                rtbLog.SelectionColor = Color.Black
            End If
        End If
        Return True
    End Function
End Class
