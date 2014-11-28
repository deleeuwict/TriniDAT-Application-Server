Public Class frmSplash

    Public Sub loadApp()

        GoTo SKIPDB1

        'INIT DB
      '  Call SQL.openDB()

SKIPDB1:
        Dim frm As New frmServerMain

        frm.ShowDialog()
        frm.Dispose()


        GoTo SKIPDB2
      '  Call SQL.closeDB()

        Me.Dispose()

SKIPDB2:
    End Sub

    Private Sub splashForm_Shown(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Shown

    End Sub

    Private Sub RectangleShape1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
End Class