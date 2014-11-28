Option Explicit On
Option Compare Text
Imports System.Web
Imports System.Text
Imports System.Collections.Specialized
Imports System.IO
Imports System.Security.Cryptography
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class JCaptchaDrop
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    '
    Public runtimeCount As Integer = 0
    Private current_ticketid As String

    Public Const CAPTCHA_DROP_PATH As String = "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\BlingBlingServor\BlingBlingServor\bosswave_config\modules\captcha\"
    Public all_base64 As String
    Public mode As String
    Private from_url As String


    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub

    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox
        mb_events.event_delivered = AddressOf delivered
        mb_events.event_err = AddressOf deliveryerr

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onpost = AddressOf OnPost
        http_events.event_onstream = AddressOf OnNetworkStream
        GetIOHandler().Configure(http_events)
    End Sub

    Public Sub OnNetworkStream(ByVal buffer As String)

        Dim end_of_stream As Boolean

        end_of_stream = IsNothing(buffer)

        If end_of_stream Then
            Call writePicture()

            'write reload instruction for browser
            writeTicket(Me.getTicket(), "reloadticket=10")

            'write ticketinfo
            'Me.from_url
            writeTicket(Me.getTicket() & ".info", "url=" & Me.from_url)

            Exit Sub
        Else
            Me.all_base64 &= buffer
        End If

    End Sub
    Public Function getTicket() As String
        Return Me.current_ticketid
    End Function
    Public Shared Function StrToByteArray(ByVal str As String) As Byte()
        Dim encoding As New System.Text.UTF8Encoding()
        Return encoding.GetBytes(str)
    End Function 'StrToByteArray
    Public Sub writePicture()

        'decode the base64 string and write to binary file
        Dim picture_file_path As String
        Dim base64_input_byte() As Byte
        Dim fs As FileStream
        Dim fb64 As FromBase64Transform
        Dim tempfile As String
        Dim x As Integer

        picture_file_path = JCaptchaDrop.CAPTCHA_DROP_PATH & Me.getTicket() & ".jpg"

        tempfile = "C:\temp\base64.dat"
        base64_input_byte = StrToByteArray(Me.all_base64)

        For x = 0 To base64_input_byte.Length
            If base64_input_byte(x) = 0 Then
                Exit For
            End If
        Next

        If x < 2 Then Exit Sub

        ReDim Preserve base64_input_byte(x - 1)


        Try
            Dim base64_decoded_byte(4) As Byte
            Dim byteswritten As Integer

            fb64 = New FromBase64Transform()
    
            'output file 
            fs = New FileStream(picture_file_path, FileMode.Create, FileAccess.Write)


            x = 0
            While (base64_input_byte.Length - 4) > x
                byteswritten = fb64.TransformBlock(base64_input_byte, x, 4, base64_decoded_byte, 0)
                x += 4

                fs.Write(base64_decoded_byte, 0, byteswritten)
            End While


            base64_decoded_byte = fb64.TransformFinalBlock(base64_input_byte, x, base64_input_byte.Length - x)
            fs.Write(base64_decoded_byte, 0, base64_decoded_byte.Length)


        Catch ex As Exception
            Me.GetIOHandler().writeRaw(True, "Error decoding Base64", True)
            Exit Sub
        End Try

        Try

            '  fs.Write(picture_bytes, 0, picture_bytes.Length)
        Catch ex As Exception
            Me.GetIOHandler().writeRaw(True, "Error writing binary file", True)
            fs.Close()
            Exit Sub
        End Try

        '   input_file.Close()
        fs.Close()

        Me.GetIOHandler().writeRaw(True, "ACCEPTED", True)
    End Sub
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> inbox called. >> Object Type=" & obj.ObjectTypeName)

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")
        End If

        Return False
    End Function



    Public Sub delivered(ByVal obj As JSONObject, destination_url as string)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub


    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")
        runtimeCount = runtimeCount + 1

        Dim ticketid As String

        'EXTRACT URL PARAMETERS
        ticketid = Mid(HTTP_URI_Path, InStrRev(HTTP_URI_Path, "/") + 1)

        If ticketid = "" Then
            Me.GetIOHandler().setHTTPResponse(404)
            Me.GetIOHandler().writeRaw(True, "Error: Unique TicketId required.", True)
            Exit Sub
        End If

        Me.current_ticketid = ticketid

        Me.from_url = HttpUtility.UrlDecode(HTTP_URI_Headers("X-Captcha-URL"))

        Dim dropurl As String

        dropurl = Me.getProcessDescriptor().getParent().getURI() & "put"

        If Left(HTTP_URI_Path, Len(dropurl)) = dropurl Then
            If IsNumeric(HTTP_URI_Parameters("alldata_length")) Then
                If CLng(HTTP_URI_Parameters("alldata_length")) > 0 Then
                    Me.all_base64 = HTTP_URI_Parameters("alldata")
                    Me.setMode("put")
                End If
            End If
        End If

    End Sub
    Public Function writeTicket(ByVal ticketid As String, ByVal contents As String) As Boolean
        Dim ticket_write_request As JSONObject

        Try

            ticket_write_request = New JSONObject

            ticket_write_request.ObjectType = "JWriteTicket"
            ticket_write_request.Directive = ticketid
            ticket_write_request.Attachment = contents
            Me.getMailProvider().Send(ticket_write_request, Nothing, "JTicketMaster")
            Return True

        Catch ex As Exception
            Msg("writeTicket: Error requesting ticketwrite: " & ex.Message)
        End Try

        Return False
    End Function
    Public Sub setMode(ByVal val As String)
        Me.mode = val
    End Sub

    Public ReadOnly Property getMode() As String
        Get
            Return Me.mode
        End Get
    End Property
End Class

