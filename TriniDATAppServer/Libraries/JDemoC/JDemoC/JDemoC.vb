Imports TriniDATServerTypes
Imports System.Collections.Specialized

Public Class JDemoC
    Inherits JTriniDATWebService

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
        http_events.event_onpost = AddressOf OnPost
        GetIOHandler().Configure(http_events)

        Return True
    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Return False
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Me.GetIOHandler().addOutput("GET C<BR>")

    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Me.GetIOHandler().addOutput("POST C<BR>")
    End Sub


End Class
