Imports System.Collections.Specialized

Public Delegate Sub TriniDATSpecializedHTTPHandler(ByVal processed_parameter_list As TriniDATGenericParameterCollection, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary)

Public Class TriniDATGenericParameterCollection
    Inherits List(Of Object)

    Public Shadows Sub Add(ByVal parameter_specification As TriniDAT_ServerFunctionParameterSpec)
        MyBase.Add(parameter_specification)
    End Sub
    Public Shadows Sub Add(ByVal v As TriniDAT_ServerFunctionParameter)
        MyBase.Add(v)
    End Sub
    Public ReadOnly Property haveParameters As Boolean
        Get
            Return (Me.Count > 0)
        End Get
    End Property
    Public Function getById(ByVal id As String) As TriniDAT_ServerFunctionParameter

        If Not Me.haveParameters Then Return Nothing

        For Each par As Object In Me

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
    Public Function ReplaceSpecWithInstance(ByVal old_spec As TriniDAT_ServerFunctionParameterSpec, ByVal actual_parameter As TriniDAT_ServerFunctionParameter) As Boolean

        Dim x As Integer
        Dim current_spec_obj As Object
        Dim current_spec As TriniDAT_ServerFunctionParameterSpec


        For x = 0 To Me.Count - 1

            current_spec_obj = Me.Item(x)

            If TypeOf current_spec_obj Is TriniDAT_ServerFunctionParameterSpec Then
                current_spec = CType(current_spec_obj, TriniDAT_ServerFunctionParameterSpec)

                If Me.Item(x).ParameterName = old_spec.ParameterName Then
                    'remove specification
                    Me.RemoveAt(x)

                    'add paramter
                    Me.Add(actual_parameter)
                    Return True
                End If
            End If
        Next

        Return False

    End Function

End Class


Public Class TriniDATParameterSpecCollection
    Inherits List(Of TriniDAT_ServerFunctionParameterSpec)

    Public Shared Function createFrom(ByVal src As TriniDATGenericParameterCollection) As TriniDATParameterSpecCollection
        Dim retval As TriniDATParameterSpecCollection

        retval = New TriniDATParameterSpecCollection

        For Each webservice_parameter_specification As Object In src

            If TypeOf webservice_parameter_specification Is TriniDAT_ServerFunctionParameterSpec Then
                retval.Add(webservice_parameter_specification)
            End If

        Next

        Return retval
    End Function



End Class
