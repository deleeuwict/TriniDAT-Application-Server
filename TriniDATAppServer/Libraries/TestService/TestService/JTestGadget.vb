Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes
Public Class JTestGadget
    Inherits JTriniDATWebService

    Public Shared runtimeCount As Integer = 0

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
        http_events.event_onpost = AddressOf OnPost
        http_events.event_onstream = AddressOf OnStream
        GetIOHandler().Configure(http_events)

        Return True

    End Function
    Public Sub OnStream(ByVal buffer As String)

        Dim end_of_stream As Boolean

        end_of_stream = IsNothing(buffer)

        If end_of_stream Then

        End If

    End Sub
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> inbox called. >> Object Type=" & obj.ObjectTypeName)

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Me.GetIOHandler().addOutput("Omega notification received.")
            Me.GetIOHandler().Flush(True, True)

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
        runtimeCount = runtimeCount + 1

        Me.GetIOHandler().addOutput("Hello from gadget x. " & runtimeCount.ToString)


    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")
        runtimeCount = runtimeCount + 1



    End Sub

End Class
