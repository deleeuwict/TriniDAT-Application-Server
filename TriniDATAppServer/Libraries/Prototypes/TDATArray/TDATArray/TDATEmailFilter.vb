Option Explicit On
Option Compare Text
Imports TriniDATDictionaries
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 
Public Class TDATEmailFilter

    Private my_internal_list As String
    Private options As TriniDATWordDictionary

    Public Sub New(ByVal initializator_list As Object)
        'base_prototype = inherited type
        'initializator_list=passed in app.action xml

        my_internal_list = initializator_list
        options = New TriniDATWordDictionary("", New List(Of String)(my_internal_list.Split(",")))

    End Sub

    Public Function OPERATOR_HASJSON(ByVal user_text As String) As Boolean
        Return False

    End Function
    '   
    Public Function OPERATOR_HASSTRING(ByVal user_text As String) As Boolean

        Return options.HasIn(user_text)

    End Function

    Public ReadOnly Property isValidValue(ByVal val As String) As Boolean
        Get

            Return True


        End Get
    End Property



End Class
