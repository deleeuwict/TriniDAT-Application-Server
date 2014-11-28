Imports System.IO

Public Class HiddenActionExecutor

    Private is_executing As Boolean

    Public Delegate Sub ExecFileDelegate(ByVal execution_info() As String)
    Public Delegate Function isFreeDelegate() As Boolean

    Public ExecFileThreaded As New ExecFileDelegate(AddressOf Executefile)
    Public isFreeThreaded As New isFreeDelegate(AddressOf isFree)

    Private Sub HiddenActionExecutor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.is_executing = False

    End Sub

    Public Function isFree() As Boolean
        Return Not Me.is_executing
    End Function

    Public Sub Executefile(ByVal execution_info() As String)
        Dim execution_path As String
        Dim execution_param As String

        'passed in format: path=param
        execution_path = execution_info(0)
        If execution_info.Length = 1 Then
            execution_param = execution_info(1)
        Else
            execution_param = ""
        End If

        Me.is_executing = True

        If GlobalObject.haveServerThread Then
            Try
                Dim window_style As ProcessWindowStyle

                If GlobalObject.server.ServerMode = TriniDATServerTypes.TRINIDAT_SERVERMODE.MODE_DEV Then
                    window_style = ProcessWindowStyle.Normal
                Else
                    window_style = ProcessWindowStyle.Minimized
                End If

                GlobalObject.ExecuteFile(execution_path, execution_param, True, window_style)
            Finally
                Me.is_executing = False
            End Try
        End If
        'delete pseudo file.
        Call OnExecutedFile(execution_path)


    End Sub
    Private Sub OnExecutedFile(ByVal execution_path As String)

        Dim delete_file As Boolean

        delete_file = InStr(execution_path, GlobalSetting.EXECPREFIX_ACTION_OBJECTFORWARDER) > 0

        If delete_file Then
            Try
                File.Delete(execution_path)
            Catch ex As Exception

            End Try
        End If
    End Sub

    Private Sub HiddenActionExecutor_Shown(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Shown
        Me.Hide()
    End Sub
End Class