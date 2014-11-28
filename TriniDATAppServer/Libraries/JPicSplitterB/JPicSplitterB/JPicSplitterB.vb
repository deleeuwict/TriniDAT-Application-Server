Imports TriniDATServerTypes
Imports System.Collections.Specialized
    Public Class JPicSplitterB
    Inherits JTriniDATWebService

    Private my_configuration_info As MappingPointBootstrapData

    Public Overrides Function DoConfigure() As Boolean

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet

        getIOHandler().Configure(http_events)

        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        'catch message
        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then

            'store mapping point info
            Me.my_configuration_info = CType(obj.Attachment, MappingPointBootstrapData)

        End If

        'reserved for future use
        Return False
    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

 
        If HTTP_URI_Parameters("middle") = "1" Then

            Dim my_picture_path As String

            'resolve picture path.
            my_picture_path = Me.my_configuration_info.static_path & "JPicSplitter\middle.jpg"

            'write file to browser stream.
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().setOutputMime("image/jpg")
            Me.getIOHandler().writeFile(True, my_picture_path, True)
        Else
            'feed html to mapping point stream.
            Me.getIOHandler().addOutput("<img src=" & Chr(34) & Me.my_configuration_info.mapping_point_url & "?middle=1" & Chr(34) & ">")
        End If

    End Sub

End Class
