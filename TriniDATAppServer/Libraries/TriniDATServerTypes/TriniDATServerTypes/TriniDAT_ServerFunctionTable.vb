Option Explicit On
Option Compare Text
Imports System.Collections.Specialized

Public Class TriniDATServerFunctionTable
    Inherits List(Of Object)

    Public Function getAllByURL(ByVal url As String) As TriniDATServerFunctionTable

        Dim retval As TriniDATServerFunctionTable

        retval = New TriniDATServerFunctionTable

        For Each func_desc As Object In Me

            If func_desc.FunctionURL = url Then
                retval.Add(func_desc)
            End If

        Next

        If retval.haveFunctionList Then
            Return retval
        Else
            Return Nothing
        End If
    End Function

    Public Function getByURL(ByVal url As String) As TriniDAT_ServerGETFunction

        If Me.hasByURL(url) Then

            For Each func_desc As Object In Me

                If func_desc.FunctionURL = url Then
                    Return func_desc
                End If

            Next
        Else
            Return Nothing
        End If
    End Function

    Public Shadows Sub Add(ByVal val As Object)

        If TypeOf val Is TriniDAT_ServerGETFunction Then
            Me.AddGET(val, val.ReceiveCallNotification)
        ElseIf TypeOf val Is TriniDAT_ServerPOSTFunction Then
            Me.AddPOST(val, val.ReceiveCallNotification)
        Else
            Throw New Exception("Invalid object '" & val.GetType.ToString & "'.")
        End If

    End Sub
    Public Sub AddGET(ByVal function_declaration As TriniDAT_ServerGETFunction, ByVal receive_notification As Boolean)
        function_declaration.ReceiveCallNotification = receive_notification
        MyBase.Add(function_declaration)
    End Sub
    Public Sub AddPOST(ByVal function_declaration As TriniDAT_ServerPOSTFunction, ByVal receive_notification As Boolean)
        function_declaration.ReceiveCallNotification = receive_notification
        MyBase.Add(function_declaration)
    End Sub
    Public Function AllGETFunctions() As TriniDATServerFunctionTable
        Dim retval As Object

        retval = New TriniDATServerFunctionTable

        For Each func In Me

            If TypeOf func Is TriniDAT_ServerGETFunction Then
                retval.add(func)
            End If

        Next

        Return retval
    End Function
    Public Function AllPOSTFunctions() As TriniDATServerFunctionTable
        Dim retval As Object

        retval = New TriniDATServerFunctionTable

        For Each func In Me

            If TypeOf func Is TriniDAT_ServerPOSTFunction Then
                retval.add(func)
            End If

        Next

        Return retval
    End Function
    Public ReadOnly Property haveGETFunctions As Boolean
        Get
        
            For Each func In Me

                If TypeOf func Is TriniDAT_ServerGETFunction Then
                    Return True
                End If

            Next

            Return False
        End Get
    End Property

    Public ReadOnly Property havePOSTFunctions As Boolean
        Get

            For Each func In Me

                If TypeOf func Is TriniDAT_ServerPOSTFunction Then
                    Return True
                End If

            Next

            Return False
        End Get
    End Property

    Public ReadOnly Property haveFunctionList As Boolean
        Get
            Return Me.Count > 0
        End Get
    End Property
    Public Function hasByURL(ByVal url As String) As Boolean

        For Each func_desc As Object In Me

            If func_desc.URL = url Then
                Return True
            End If

        Next

        Return False
    End Function
End Class


Public Enum TriniDAT_HTTPServerMethod
    SERVER_METHOD_GET = 1
    SERVER_METHOD_POST = 2
End Enum

'abc

