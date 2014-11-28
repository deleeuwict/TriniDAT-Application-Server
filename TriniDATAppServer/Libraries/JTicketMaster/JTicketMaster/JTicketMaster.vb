Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATHTTPTypes
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JTicketMaster
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    '
    Public runtimeCount As Integer = 0
    Public Shared ticketstorage_absolute_path As String = "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\BlingBlingServor\BlingBlingServor\bosswave_config\modules\tickets\"
    Public Const OWN_MAPPING_POINT As String = "/tickets/"

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
        http_events.event_onget = AddressOf OnGet
        GetIOHandler().Configure(http_events)

    End Sub

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> user inbox called.. Type=" & obj.ObjectTypeName)


        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")
            logline("Omega Flush call.")
        End If

        If obj.ObjectTypeName = "JOMEGAREPLY" And obj.Directive = "SUCCES" Then
            Msg("Omega succesfully disabled.")
        End If

        If obj.ObjectTypeName = "JWriteTicket" And obj.Directive <> "" And Not IsNothing(obj.Attachment) Then
            Me.writeTicket(obj.Content, obj.Attachment)
        End If


        Return False
    End Function
    Public Sub delivered(ByVal obj As JSONObject, destination_url as string)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub



    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnGet")
        runtimeCount = runtimeCount + 1


        If Left(HTTP_URI_Path, Len(JTicketMaster.OWN_MAPPING_POINT)) <> JTicketMaster.OWN_MAPPING_POINT Then
            'ignore get module when embedded in other mapping point
            Exit Sub
        End If


        Dim TicketId As String
        Dim ticketpath As String

        TicketId = Replace(HTTP_URI_Path, "/tickets/", "")

        ticketpath = JTicketMaster.ticketstorage_absolute_path & TicketId

        'read file

        If System.IO.File.Exists(ticketpath) Then
            Me.GetIOHandler().setHTTPResponse(200)
            Call Me.GetIOHandler().writeFile(True, ticketpath, True)
            Try
                System.IO.File.Delete(ticketpath)
            Catch ex As Exception

            End Try
        Else
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().writeRaw(True, ticketpath & " not found.", True)
        End If

    End Sub

    Public Function readTicket(ByVal TicketId As String, ByVal read_and_delete As Boolean) As String


        Dim ticketpath As String
        Dim retval As String
        ticketpath = JTicketMaster.ticketstorage_absolute_path & TicketId

        'read file
        Try
            If System.IO.File.Exists(ticketpath) Then
                Me.GetIOHandler().setHTTPResponse(200)
                retval = System.IO.File.ReadAllText(ticketpath)

                If read_and_delete Then
                    System.IO.File.Delete(ticketpath)
                End If

                Return retval

            Else
                Err.Raise(0, 0, ticketpath & " not found.")
            End If

        Catch ex As Exception
            Msg("error reading ticket: " & ex.Message)

        End Try

        Return Nothing
    End Function

    Private Function writeTicket(ByVal ticketid As String, ByVal contents As String) As Boolean

        Return JTicketMaster.writeTicketGlobal(ticketid, contents)


    End Function
    Public Shared Function deleteTicketGlobal(ByVal ticketid As String, Optional ByVal msgproc As TriniDATTypeLogger = Nothing) As Boolean
        'designed for non jservices

        Dim ticketfile As String
        Dim retval As Boolean

        Try
            ticketfile = JTicketMaster.ticketstorage_absolute_path & ticketid

            If Not IsNothing(msgproc) Then
                msgproc("Deleting ticketfile " & ticketfile & " ...")
            End If

            If File.Exists(ticketfile) Then
                File.Delete(ticketfile)
                retval = True
            Else
                Err.Raise(200, 0, "Error: Ticket file doesn't exist: " & ticketfile)
            End If

            Return retval

        Catch ex As Exception
            If Not IsNothing(msgproc) Then
                msgproc(ex.Message)
            End If
            Return Nothing
        End Try

    End Function
    Public Shared Function writeTicketGlobal(ByVal ticketid As String, ByVal contents As String, Optional ByVal msgproc As TriniDATTypeLogger = Nothing) As Boolean
        'designed for non jservices

        Dim ticketfile As String
        Dim retval As Boolean

        Try
            ticketfile = JTicketMaster.ticketstorage_absolute_path & ticketid

            If Not IsNothing(msgproc) Then
                msgproc("Writing ticketfile " & ticketfile & " ...")
            End If

            If Not File.Exists(ticketfile) Then
                File.WriteAllText(ticketfile, contents & vbCrLf)
                retval = True
            Else
                Err.Raise(200, 0, "Error: Ticket file already exists: " & ticketfile)
            End If

            Return retval

        Catch ex As Exception
            If Not IsNothing(msgproc) Then
                msgproc(ex.Message)
            End If
            Return Nothing
        End Try

    End Function
End Class

