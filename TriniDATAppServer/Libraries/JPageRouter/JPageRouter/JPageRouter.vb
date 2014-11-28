Option Explicit On
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Reflection
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATServerTypes
Imports System.Web
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JPageRouter
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    Private current_server_url As String
    Public Sub New()
        MyBase.New()
    End Sub

    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox


        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        GetIOHandler().Configure(http_events)


        'get current server url
        Me.askServerURL()

    End Sub

    Public ReadOnly Property currentServerURL()
        Get
            Return Me.current_server_url
        End Get
    End Property
    Public Sub setServerURL(ByVal val As String)
        Me.current_server_url = val
    End Sub

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "Setting_ServerURL" Then
            Me.setServerURL(obj.Content)
            Return False
        End If

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Return False
        End If

        Return False
    End Function


    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        Dim decide_for_url As String

        decide_for_url = HttpUtility.UrlDecode(HTTP_URI_Parameters("routeurl"))

        'return the url  of the service that handles this website
        If InStr(decide_for_url, "live.com") Then
            Me.GetIOHandler().writeRaw(True, Me.currentServerURL() & "/signupprovider/email/hotmail", True)
            Exit Sub
        End If

        Me.GetIOHandler().writeRaw(True, Me.currentServerURL() & "/console", True)

    End Sub
   

End Class

