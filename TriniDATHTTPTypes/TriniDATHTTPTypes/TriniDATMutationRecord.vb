Imports System.Collections.Specialized

Public Class TriniDATMutationRecord
    Inherits List(Of String)

    Private debug_logger As TriniDATTypeLogger

    Public Sub New(Optional ByVal _debug_logger As TriniDATTypeLogger = Nothing)
        MyBase.New()

        If IsNothing(_debug_logger) Then
            _debug_logger = AddressOf NullEvent
        End If

        Me.debug_logger = _debug_logger

    End Sub
    Public Sub setDebuggerLog(ByVal _logfunc As TriniDATTypeLogger)
        Me.debug_logger = _logfunc
    End Sub
    Public Sub Record(ByVal context As String, ByVal value As String)
        Msg("BlingMutationDictionary: context: " & context & "  value: " & value & ")")
        MyBase.Add(context & " " & value)

    End Sub

    Private Sub NullEvent(ByVal txt As String)

    End Sub

    Private Sub Msg(ByVal txt As String)
        Call Me.debug_logger(txt)
    End Sub

    Public ReadOnly Property MutationList(Optional ByVal clean_list As Boolean = False) As String
        Get

            Dim retval As String
            Dim x As Integer

            retval = ""
            retval &= "count=" & Me.Count.ToString & vbCrLf

            For x = 0 To Me.Count - 1
                retval &= Me.Item(x) & vbCrLf
            Next

            If clean_list Then Me.Clear()


            Return retval

        End Get
    End Property

End Class
