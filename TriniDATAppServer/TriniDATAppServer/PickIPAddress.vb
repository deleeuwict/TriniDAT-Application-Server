Imports System.Net.NetworkInformation
Imports System.Net.NetworkInformation.IPAddressInformation
Imports System.Net
Imports System.Net.Sockets
Public Class PickIPAddress

    Private adapters() As NetworkInterface
    Private Sub lblTechSupport_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblTechSupport.Click
        GlobalObject.OpenURL("http://www.deleeuwict.nl/forum/")

    End Sub

    Private Sub lblTechSupport_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblTechSupport.MouseLeave
        lblTechSupport.Font = New Font(lblTechSupport.Font.FontFamily, lblTechSupport.Font.Size, FontStyle.Regular)
    End Sub

    Private Sub lblTechSupport_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lblTechSupport.MouseMove
        lblTechSupport.Font = New Font(lblTechSupport.Font.FontFamily, lblTechSupport.Font.Size, FontStyle.Underline)
    End Sub

    Private Sub PickIPAddress_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
  
        Me.adapters = NetworkInterface.GetAllNetworkInterfaces()
        Me.lstServerInterfaceIP.Items.Clear()

        Call getPList()

    End Sub

    Private Sub getPList()

        Me.lstServerInterfaceIP.Items.Add("127.0.0.1")

        For Each IP As IPAddress In Dns.GetHostEntry(Dns.GetHostName()).AddressList
            If IP.AddressFamily = AddressFamily.InterNetwork Then
                Me.lstServerInterfaceIP.Items.Add(IP.ToString)
            End If
        Next IP



    End Sub

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click

        If Me.lstServerInterfaceIP.SelectedIndex = -1 Then
            MsgBox("Please select IP address.")
            Me.lstServerInterfaceIP.Focus()
            Exit Sub
        End If

        If Me.txtServerPort.Value < 1 Then
            MsgBox("Need port number.")
            Me.txtServerPort.Focus()
            Exit Sub
        End If

        Dim ip_address As String

        ip_address = Me.lstServerInterfaceIP.SelectedItem.ToString

        If Not GlobalObject.TestTCPServerConnection(ip_address, Me.txtServerPort.Value) Then
            MessageBox.Show("Could not listen on port with the specified configuration. Try another IP address or port number.")
        Else
            GlobalObject.CurrentServerConfiguration.server_ip = IPAddress.Parse(Me.lstServerInterfaceIP.SelectedItem.ToString)
            GlobalObject.CurrentServerConfiguration.server_port = Me.txtServerPort.Value
            GlobalObject.CurrentServerConfiguration.Write()
            Me.Close()
        End If

    End Sub
End Class