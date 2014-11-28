'Empty JTriniDATWebService instance for COPY/PASTE purposes.

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices


<Assembly: SuppressIldasmAttribute()> 

Public Class JHotmailSignup
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    '

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
        http_events.event_onpost = AddressOf OnPost
        http_events.event_onstream = AddressOf OnNetworkStream
        GetIOHandler().Configure(http_events)

    End Sub

    Public Sub OnNetworkStream(ByVal buffer As String)

        Dim end_of_stream As Boolean

        end_of_stream = IsNothing(buffer)

        If end_of_stream Then

        End If

    End Sub
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> inbox called. >> Object Type=" & obj.ObjectTypeName)

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")
            logline("Omega Flush call.")
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
       

    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")

    End Sub

End Class