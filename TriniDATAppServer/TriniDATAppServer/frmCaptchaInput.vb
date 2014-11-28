Imports System.IO

Public Class frmCaptchaInput

    Private image_path As String
    Private TicketMaster As Object
    Private ticket_master_mapping_point As mappingPointRoot

    Private Sub frmCaptchaInput_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Public Sub New(ByVal image_url As String)
        Me.InitializeComponent()

        'need mapping point to obtain the DLL filepath for Ticket Master class.
        ' ticket_master_mapping_point = BosswaveApplicationHost.getbyClassName("JTicketMaster")

        If IsNothing(ticket_master_mapping_point) Then
            MsgBox("unable to load JTicketMaster class")
            Me.Close()
        End If

        '   TicketMaster = JServiceLauncher.createJService(JServiceLauncher.getExternalType("JTicketMaster", ticket_master_mapping_point.getDependencyPaths()))

        If IsNothing(TicketMaster) Then
            MsgBox("unable to load JTicketMaster class")
            Me.Close()
        End If


        Me.setCaptchaImage(image_url)

    End Sub
    Public Function getTicketFromImageFilename() As String

        Dim last_part As String
        Dim ext_start As Integer
        Dim retval As String

        last_part = Mid(Me.image_path, InStrRev(Me.image_path, "\") + 1)
        ext_start = InStr(last_part, ".")

        If ext_start > 0 Then
            retval = Mid(last_part, 1, ext_start - 1)
        Else
            retval = last_part
        End If

        Return retval
    End Function
    Public Sub setTicket(ByVal ticketid As String)

    End Sub
    Public Sub setCaptchaImage(ByVal _image_path As String)
        Me.image_path = _image_path
    End Sub
    Public Sub loadCaptchaImage()
        Me.PictureBox1.Load(Me.image_path)
        Me.Text = "Ticketid: " & getTicketFromImageFilename()
        Call loadTicketInfo()
    End Sub
    Public Sub loadTicketInfo()
        Dim ticketpath As String
       
        ticketpath = ticketMaster.ticketstorage_absolute_path
        ticketpath &= getTicketFromImageFilename() & ".info"

        If File.Exists(ticketpath) Then

            Dim all_info As String
            Dim infos() As String
            Dim x As Integer

            all_info = File.ReadAllText(ticketpath)

            infos = all_info.Split("=")

            For x = 0 To infos.Length Step 2
                If infos(x) = "url" Then
                    Me.lblURL.Text = infos(x + 1)
                End If
            Next
        Else
            Me.lblURL.Text = "No ticket info found!"
        End If

    End Sub
    Private Sub cmdReload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdReload.Click

    End Sub

    Private Sub frmCaptchaInput_Shown(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Shown
        Me.loadCaptchaImage()
        '    Me.BringToFront()
    End Sub

    Private Sub cmdSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSend.Click

        Me.WriteResult("captcha=" & txtCaptchaText.Text)
        Me.Close()

    End Sub

    Private Sub TextBox1_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtCaptchaText.KeyPress
        If e.KeyChar = Chr(13) Then
            Call cmdSend_Click(Nothing, Nothing)
        End If
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.WriteResult("captcha=cancel")
        Me.Close()
    End Sub

    Public Sub WriteResult(ByVal retval As String)
        Dim ticketid As String

        ticketid = getTicketFromImageFilename()
        'delete old ticket
        TicketMaster.deleteTicketGlobal(ticketid)
        'write new version
        TicketMaster.writeTicketGlobal(ticketid, retval & vbCrLf)
    End Sub
End Class