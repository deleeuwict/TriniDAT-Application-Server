Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.Timers
Imports System.Threading
Imports TriniDATServerTypes


Public Class JOmega 'last class in every chain, sends around notifications.
    Inherits JTriniDATWebService

    Private mappingpointBytesReceived As Long
    Private mappingpointBytesReceivedTotal As Long
    Private Declare Function GetTickCount Lib "Kernel32" () As Long

    Public Const socket_timer_kill_ms As Integer = 20

    Private current_url As String
    Private console_logging_enabled As Boolean

    Public Sub New()
        MyBase.New()

        Call Init()
    End Sub
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function


    Private Sub Init()
        mappingpointBytesReceived = 0
        mappingpointBytesReceivedTotal = 0
    End Sub
    Public Overrides Function DoConfigure() As Boolean
        'Me.CurrentBootState = "config"

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox
        mb_events.event_err = AddressOf deliveryerr

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnHTTPRequest
        http_events.event_onpost = AddressOf OnHTTPRequest

        'configure protocol first.
        Return getIOHandler().Configure(http_events)

    End Function
   
    Public Sub OnHTTPRequest(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Me.current_url = HTTP_URI_Path

        If Not IsNothing(HTTP_URI_Parameters) Then
            If HTTP_URI_Parameters("output") = "console" Then
                Me.LogConsole = True
            End If
        End If

        Me.FinalizeConnection()

    End Sub
    Public Property LogConsole As Boolean
        Get
            Return Me.console_logging_enabled
        End Get
        Set(ByVal value As Boolean)
            Me.console_logging_enabled = True
        End Set
    End Property
    'called directly by openprocesschain
    Public Sub MappingPointStop(ByVal mp_config As MappingPointBootstrapData)

        'Me.CurrentBootState = "stop"
        Me.Msg("Broadcast end...")
        Dim killmsg As New JSONObject
        killmsg.ObjectType = "JOmega"
        killmsg.Directive = "MAPPING_POINT_STOP"
        killmsg.Attachment = mp_config
        Call Me.LocalBroadcast(killmsg)


        Dim startTime As Long
        Dim timeElapsed As Long

        startTime = GetTickCount()
        timeElapsed = 0

        'timeElapsed = mapping_poin
        Do Until Not Me.GetIOHandler().getConnection().isConnected() Or (timeElapsed > JOmega.socket_timer_kill_ms)
            timeElapsed = GetTickCount() - startTime
            Thread.Sleep(100)
        Loop

        If Me.GetIOHandler().getConnection().isConnected() Then
            Msg("time out: " & Me.getProcessDescriptor().getParent().getURI())
            GetIOHandler().getConnection().forceDisconnect()
        Else
            Msg("Closing:" & Me.getProcessDescriptor().getParent().getURI())
        End If

    End Sub

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then
            'Me.CurrentBootState = "start"
            Return False
        End If


        If obj.ObjectTypeName = "JOMEGA" And obj.Directive = "DESTROY" Then
            'kill the mapping point by ending the connection
            Call FinalizeConnection()
            Return False
        End If

        Return False

    End Function

    Public Sub FinalizeConnection()
        'flush/disconnect the master socket.
        Msg("KillConnection: Signaling server thread to close link.")

        Dim output As String

        output = GetIOHandler().flush()

        If Me.LogConsole Then

            If IsNothing(output) Then
                'hypothetically
                output = ""
            End If

            'write header
            output = "'" & Me.getProcessDescriptor().getParent().info.App.applicationname & "' mapping point '" & Me.current_url & "' output:" & vbNewLine & vbNewLine & output

            GlobalObject.MsgColored(output, Color.GreenYellow)
        End If

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)
        'logline(">> error sending object r. Type=" & obj.ObjectTypeName)
    End Sub

End Class
