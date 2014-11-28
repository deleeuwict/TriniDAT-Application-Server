Option Explicit On
Option Compare Text
Imports TriniDATServerTypes
Imports System.Collections.Specialized
Imports TriniDATHTTPTypes
Imports System.Text

Public Class JExternalSimonWebservice
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
        getIOHandler().Configure(http_events)

        Return True
    End Function
    Public Sub MyCMDHandler(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        Dim cmd_text As String

        cmd_text = parameter_list.getById("cmd").ParameterValue

        'set encoding if you want to parse non-english.
        Me.getIOHandler().setEncoding(New UTF8Encoding)

        'todo: exectute console app w/ redirected output


        Me.getIOHandler().addOutput("Done: " & cmd_text)

    End Sub
 
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal servers_function_table As TriniDATServerFunctionTable) As Boolean

        Dim Simon_command As TriniDAT_ServerGETFunction
        Dim cmd_par As TriniDAT_ServerFunctionParameterSpec

        cmd_par = New TriniDAT_ServerFunctionParameterSpec()
        cmd_par.ParameterName = "cmd"
        cmd_par.ParameterType = "String"
        cmd_par.Required = True

        'add '/text?keywords=..' sub-mapping point.
        Simon_command = New TriniDAT_ServerGETFunction(AddressOf MyCMDHandler)
        Simon_command.FunctionURL = Me.makeRelative("/cmd")
        Simon_command.Parameters.Add(cmd_par)

        'Register our URLS .
        servers_function_table.Add(Simon_command)

        'Completed.
        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        Return False
    End Function

End Class
