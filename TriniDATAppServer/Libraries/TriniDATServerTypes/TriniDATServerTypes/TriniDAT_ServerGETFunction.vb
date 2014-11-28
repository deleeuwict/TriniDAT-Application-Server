Option Compare Text
Imports System.Collections.Specialized

Public Class TriniDAT_ServerGETFunction
    'FIRST OBJECT GENERATION:  TriniDAT_ServerFunctionParameterSpec  SECOND OBJECT GENERATION: TriniDAT_ServerFunctionParameter 
    Private _function_parameters As TriniDATGenericParameterCollection
    Private _function_name As String
    Private _server_geturl As String
    Private _user_func_delegate As TriniDATSpecializedHTTPHandler
    Private _inbox_notifications_enabled As Boolean

    Public Sub New(ByVal target As TriniDATSpecializedHTTPHandler)
        _user_func_delegate = target
        Me._function_parameters = New TriniDATGenericParameterCollection
    End Sub

    Public ReadOnly Property Parameters As TriniDATGenericParameterCollection
        Get
            Return Me._function_parameters
        End Get
    End Property
    Public ReadOnly Property haveMissingParameters As Boolean
        Get
            If Not Me.haveParameters Then Return False

            For Each get_par In Me.Parameters
                If TypeOf get_par Is TriniDAT_ServerFunctionParameter Then
                    If get_par.Found = False And get_par.required = True Then
                        Return True
                    End If
                End If
            Next

            Return False
        End Get
    End Property
    Public ReadOnly Property getMissingParameters As TriniDATGenericParameterCollection
        Get
            If Not Me.haveParameters Then Return Nothing

            Dim retval As TriniDATGenericParameterCollection

            retval = New TriniDATGenericParameterCollection

            For Each get_par In Me.Parameters
                If TypeOf get_par Is TriniDAT_ServerFunctionParameter Then
                    If get_par.Found = False And get_par.required = True Then
                        retval.Add(get_par)
                    End If
                End If
            Next

            Return retval
        End Get
    End Property
    Public Function dumpForm(ByVal formid As String, Optional ByVal form_method As String = "GET") As String

        Dim output As String
        Dim target As String

        If Not Me.haveParameters Then Return ""

        target = Me.FunctionURL

        output = "<form action=""" & target & """ name=""" & formid & """ id=""" & formid & """ method=""" & form_method & """>" & vbCrLf
        output &= "     <table>" & vbCrLf
        For Each get_parameter In Me.Parameters
            If get_parameter.haveBosswaveParameter Then
                output &= "         <tr>" & vbCrLf
                output &= "             <td>" & get_parameter.id & "" & get_parameter.id & " (" & get_parameter.ParameterPrototypeID & ")</td>" & vbCrLf
                output &= "             <td>:</td>" & vbCrLf
                output &= "             <td>" & get_parameter.BosswaveParameter.generateHTMLField(get_parameter.ParameterValue) & "</td>" & vbCrLf
                output &= "         </tr>" & vbCrLf
            End If
        Next

        output &= "     </table>" & vbCrLf
        output &= "<input type=""submit"" value=""Send""/>"
        output &= "</form>" & vbCrLf

        Return output
    End Function


    Public Property ReceiveCallNotification As Boolean
        Get
            Return Me._inbox_notifications_enabled
        End Get
        Set(ByVal value As Boolean)
            Me._inbox_notifications_enabled = value
        End Set
    End Property

    Public Shadows Sub Add(ByVal v As Object)
        'not allowed.
        Me.Add(v)
        Throw New Exception("Depreciated. Use AddGET or AddPOST to add functions.")
    End Sub

    Public Function TriggerFunction(ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary) As Boolean
        Try

            Call _user_func_delegate(Me.Parameters, AllParameters, Headers)

            Return True

        Catch ex As Exception

        End Try

        Return False
    End Function


    'Public Property ServerMethod As TriniDAT_HTTPServerMethod
    '    Get
    '        Return Me._http_method
    '    End Get
    '    Set(ByVal value As TriniDAT_HTTPServerMethod)
    '        Me._http_method = value
    '    End Set
    'End Property
    Public Property FunctionURL As String
        Get
            Return Me._server_geturl
        End Get
        Set(ByVal value As String)
            Me._server_geturl = value
        End Set
    End Property

    'Public Property FunctionName As String
    '    Get
    '        Return Me._function_name
    '    End Get
    '    Set(ByVal value As String)
    '        Me._function_name = value
    '    End Set
    'End Property

    Public ReadOnly Property haveParameters As Boolean
        Get
            If Not IsNothing(Me.Parameters) Then
                Return Me.Parameters.haveParameters
            Else
                Return False
            End If

        End Get
    End Property
    Public Function haveParameterByName(ByVal id As String) As Boolean

        If Not Me.haveParameters Then Return False

        For Each par As Object In Me.Parameters

            If TypeOf par Is TriniDAT_ServerFunctionParameter Then

                If CType(par, TriniDAT_ServerFunctionParameter).ID = id Then
                    Return True
                End If

            End If

        Next

        Return False

    End Function
    Public Function getParameterByName(ByVal id As String) As TriniDAT_ServerFunctionParameter

        If Not Me.haveParameters Then Return Nothing

        For Each par As Object In Me.Parameters

            If TypeOf par Is TriniDAT_ServerFunctionParameter Then

                Dim get_parameter As TriniDAT_ServerFunctionParameter

                get_parameter = CType(par, TriniDAT_ServerFunctionParameter)

                If get_parameter.ID = id Then
                    Return get_parameter
                End If

            End If

        Next

        Return Nothing

    End Function
End Class