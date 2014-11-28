
Option Compare Text
Option Explicit On

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes

'Template JTriniDATWebService instance for COPY/PASTE purposes.
Public Class JServiceDummy
    Inherits JTriniDATWebService

    Private my_configuration_info As MappingPointBootstrapData

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
        http_events.event_onstream = AddressOf OnStream
        Return getIOHandler().Configure(http_events)

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean

        Dim get_function As TriniDAT_ServerGETFunction
        Dim get_parameter_def As TriniDAT_ServerFunctionParameterSpec

        get_function = New TriniDAT_ServerGETFunction(AddressOf ShowHelloWorld)
        get_function.FunctionURL = Me.makeRelative("/helloworld")

        get_parameter_def = New TriniDAT_ServerFunctionParameterSpec()
        get_parameter_def.ParameterName = "myrequiredparameter"
        get_parameter_def.ParameterType = "String"
        get_parameter_def.Required = True

        'add parameter
        get_function.Parameters.Add(get_parameter_def)

        'add to http GET-POST router table.
        http_function_table.Add(get_function)

        Return True
    End Function

    Public Sub ShowHelloWorld(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        Me.getIOHandler().addOutput("Value of myrequiredparameter = " & parameter_list.getById("myrequiredparameter").ParameterValue)

    End Sub
    Public Sub OnStream(ByVal buffer As String)

        Dim end_of_stream As Boolean

        end_of_stream = IsNothing(buffer)

        If end_of_stream Then

        End If

    End Sub
    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        'catch message
        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then

            'store mapping point info
            Me.my_configuration_info = CType(obj.Attachment, MappingPointBootstrapData)

        End If

        Return False
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
       

    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")

    End Sub

End Class
