Public Class IEScripter

    Private Sub cmdExit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdExit.Click
        Application.Exit()

    End Sub

    Private Sub txtConsole_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtConsole.KeyPress
        Me.Text = Asc(e.KeyChar).ToString

        Dim last_return_pos As Long
        Dim cmd As String

        If Asc(e.KeyChar) = 13 Then
            last_return_pos = InStrRev(Me.txtConsole.Text, Chr(13))
            If last_return_pos < 1 Then last_return_pos = -1

            cmd = Mid(Me.txtConsole.Text, last_return_pos + 2)
            'todo: pass as simon handler.
            Me.Text = cmd
        End If

    End Sub
End Class