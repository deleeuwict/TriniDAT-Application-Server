Imports TriniDATServerTypes
Imports System.Collections.Specialized

Public Class JPicUploader
    Inherits JTriniDATWebService

    Private picture_save_path As String
    Private myshop_getfunction As TriniDAT_ServerGETFunction
    Private myshop_postfunction As TriniDAT_ServerPOSTFunction
    Private my_webservice_url As String

    Public Overrides Function DoConfigure() As Boolean

        'configure object engine.
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        'configure packet engine.
        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        '  http_events.event_onpost = AddressOf OnMyShopPost

   

        Return (getIOHandler().Configure(http_events))
    End Function

    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerTypes.TriniDATServerFunctionTable) As Boolean

        'set up post file handlers.

        Dim myshop_required_parameter As TriniDAT_ServerFunctionParameterSpec

        myshop_postfunction = New TriniDAT_ServerPOSTFunction(AddressOf OnMyShopPost)
        myshop_postfunction.FunctionURL = Me.makeRelative("/shop13")

        myshop_required_parameter = New TriniDAT_ServerFunctionParameterSpec
        myshop_required_parameter.ParameterType = "String"
        myshop_required_parameter.ParameterName = "lastname"
        'receive live function call notification.
        myshop_postfunction.ReceiveCallNotification = True


        'add parameter to function
        myshop_postfunction.Parameters.Add(myshop_required_parameter)

        myshop_required_parameter = New TriniDAT_ServerFunctionParameterSpec
        myshop_required_parameter.ParameterType = "boolean"
        myshop_required_parameter.ParameterName = "posted"
        myshop_required_parameter.Required = False

        'receive live function call notification.
        myshop_postfunction.ReceiveCallNotification = True

        'add parameter to function
        myshop_postfunction.Parameters.Add(myshop_required_parameter)

        'add to webservice function table.
        http_function_table.AddPOST(myshop_postfunction, True)

        Return True

    End Function
    Public Sub OnMyShopPage(ByVal processed_parameter_list As TriniDATGenericParameterCollection, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary)
        Me.getIOHandler().addOutput("glorious moment #33 because my shop handle is called.<BR>")
        Me.getIOHandler().addoutput("Session Create Date: " & Me.getProcessDescriptor().getParent().info.direct_session.CreateDate.ToString & "<br>")
        Me.getIOHandler().addoutput("Session Create Date var: " & Me.getIOHandler().sessiondat("date") & "<br>")
        'SessionDat

        '        Me.getIOHandler().addOutput("shop handle=" & processed_parameter_list )

        For Each par As TriniDAT_ServerFunctionParameter In processed_parameter_list
            Me.getIOHandler().addOutput("<BR>" & par.ID & "=" & par.ParameterValue & " type=" & par.ParameterPrototypeID)
        Next

    End Sub

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean


        'catch message
        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then

            'picture_save_path
            Me.picture_save_path = CType(obj.Attachment, MappingPointBootstrapData).static_path
            Me.my_webservice_url = CType(obj.Attachment, MappingPointBootstrapData).mapping_point_url
        End If

        Return True
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        Dim my_html_form As String

        my_html_form = Me.myshop_postfunction.dumpForm("my form")

        Me.getIOHandler().addoutput("Test form<br>")

        If Len(Me.getIOHandler().sessiondat("date")) < 1 Then
            Me.getIOHandler().addoutput("set date<br>")

            Me.getIOHandler().sessiondat("date") = Now.ToString

        End If

        Me.getIOHandler().addoutput("Session Create Date: " & Me.getProcessDescriptor().getParent().info.direct_session.CreateDate.ToString & "<br>")
        Me.getIOHandler().addoutput("Session Create Date var: " & Me.getIOHandler().sessiondat("date") & "<br>")

        'CreateDate
        Me.getIOHandler().addoutput(my_html_form)

        '  Me.getIOHandler().Session.uservars("date") = Now.ToString


    End Sub
    Public Sub OnMyShopPost(ByVal processed_parameter_list As TriniDATGenericParameterCollection, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary)

        Dim my_html_form As String

        '  my_html_form = Me.myshop_postfunction.dumpForm("my form")

        Me.getIOHandler().addoutput("posted form<br>")


        If Len(Me.getIOHandler().sessiondat("date")) < 1 Then
            Me.getIOHandler().addoutput("set date<br>")

            Me.getIOHandler().sessiondat("date") = Now.ToString

        End If

        Me.getIOHandler().addoutput("Session Create Date: " & Me.getProcessDescriptor().getParent().info.direct_session.CreateDate.ToString & "<br>")
        Me.getIOHandler().addoutput("Session Create Date var: " & Me.getIOHandler().sessiondat("date") & "<br>")

        Me.getIOHandler().addoutput("AllParameters:<br>")

        For Each key_name In AllParameters.Keys

            Me.getIOHandler().addoutput(key_name & " =  " & AllParameters(key_name) & "<BR>" & vbCrLf)

        Next


        Me.getIOHandler().addoutput("<BR>processed_parameter_list:<br>")

        For Each par In processed_parameter_list

            Me.getIOHandler().addoutput(par.id & " =  " & par.ParameterValue & "<BR>" & vbCrLf)

        Next


    End Sub
    Public Sub PostFileHandler(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

    End Sub


End Class
