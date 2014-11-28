Imports Microsoft.VisualBasic.Interaction

Public Class frmAppLicense

    Private Sub cmdAppEngine_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdInstall.Click
        Me.DialogResult = Windows.Forms.DialogResult.Yes
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdStop.Click
        Me.DialogResult = Windows.Forms.DialogResult.Abort
    End Sub

    Private Sub chkAccept_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkAccept.CheckStateChanged
        Me.cmdInstall.Enabled = chkAccept.Checked

        Call toggleButton()
    End Sub

    Private Sub toggleButton()
        If Not Me.cmdInstall.Enabled Then
            Me.cmdInstall.BackColor = Me.cmdStop.BackColor
        Else
            Me.cmdInstall.BackColor = Color.Gold
        End If
    End Sub
    Private Sub frmAppLicense_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.txtLicense.Left = 0
        Me.txtLicense.Top = Me.Label1.Height + 6
        Me.txtLicense.Width = Me.Width
        Me.txtLicense.Height = Me.Height - Me.txtLicense.Top

        Call toggleButton()

        Beep()
    End Sub
End Class