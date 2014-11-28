<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCaptchaInput
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.txtCaptchaText = New System.Windows.Forms.TextBox()
        Me.cmdReload = New System.Windows.Forms.Button()
        Me.lblURL = New System.Windows.Forms.Label()
        Me.cmdPlus = New System.Windows.Forms.Button()
        Me.cmdMin = New System.Windows.Forms.Button()
        Me.cmdSend = New System.Windows.Forms.Button()
        Me.cmdCancel = New System.Windows.Forms.Button()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'PictureBox1
        '
        Me.PictureBox1.Location = New System.Drawing.Point(12, 12)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(538, 130)
        Me.PictureBox1.TabIndex = 0
        Me.PictureBox1.TabStop = False
        '
        'txtCaptchaText
        '
        Me.txtCaptchaText.Font = New System.Drawing.Font("Miriam Fixed", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(177, Byte))
        Me.txtCaptchaText.Location = New System.Drawing.Point(12, 180)
        Me.txtCaptchaText.Name = "txtCaptchaText"
        Me.txtCaptchaText.Size = New System.Drawing.Size(459, 72)
        Me.txtCaptchaText.TabIndex = 1
        '
        'cmdReload
        '
        Me.cmdReload.Location = New System.Drawing.Point(398, 145)
        Me.cmdReload.Name = "cmdReload"
        Me.cmdReload.Size = New System.Drawing.Size(73, 29)
        Me.cmdReload.TabIndex = 2
        Me.cmdReload.Text = "Recycle"
        Me.cmdReload.UseVisualStyleBackColor = True
        '
        'lblURL
        '
        Me.lblURL.AutoSize = True
        Me.lblURL.Font = New System.Drawing.Font("Microsoft Sans Serif", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblURL.Location = New System.Drawing.Point(12, 153)
        Me.lblURL.Name = "lblURL"
        Me.lblURL.Size = New System.Drawing.Size(51, 18)
        Me.lblURL.TabIndex = 3
        Me.lblURL.Text = "Page:"
        '
        'cmdPlus
        '
        Me.cmdPlus.Location = New System.Drawing.Point(345, 145)
        Me.cmdPlus.Name = "cmdPlus"
        Me.cmdPlus.Size = New System.Drawing.Size(19, 29)
        Me.cmdPlus.TabIndex = 4
        Me.cmdPlus.Text = "+"
        Me.cmdPlus.UseVisualStyleBackColor = True
        '
        'cmdMin
        '
        Me.cmdMin.Location = New System.Drawing.Point(370, 145)
        Me.cmdMin.Name = "cmdMin"
        Me.cmdMin.Size = New System.Drawing.Size(22, 29)
        Me.cmdMin.TabIndex = 5
        Me.cmdMin.Text = "-"
        Me.cmdMin.UseVisualStyleBackColor = True
        '
        'cmdSend
        '
        Me.cmdSend.Location = New System.Drawing.Point(482, 180)
        Me.cmdSend.Name = "cmdSend"
        Me.cmdSend.Size = New System.Drawing.Size(67, 71)
        Me.cmdSend.TabIndex = 6
        Me.cmdSend.Text = "Submit"
        Me.cmdSend.UseVisualStyleBackColor = True
        '
        'cmdCancel
        '
        Me.cmdCancel.Location = New System.Drawing.Point(478, 147)
        Me.cmdCancel.Name = "cmdCancel"
        Me.cmdCancel.Size = New System.Drawing.Size(70, 26)
        Me.cmdCancel.TabIndex = 7
        Me.cmdCancel.Text = "Cancel"
        Me.cmdCancel.UseVisualStyleBackColor = True
        '
        'frmCaptchaInput
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(556, 257)
        Me.Controls.Add(Me.cmdCancel)
        Me.Controls.Add(Me.cmdSend)
        Me.Controls.Add(Me.cmdMin)
        Me.Controls.Add(Me.cmdPlus)
        Me.Controls.Add(Me.lblURL)
        Me.Controls.Add(Me.cmdReload)
        Me.Controls.Add(Me.txtCaptchaText)
        Me.Controls.Add(Me.PictureBox1)
        Me.MaximizeBox = False
        Me.Name = "frmCaptchaInput"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Captcha Please"
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox
    Friend WithEvents txtCaptchaText As System.Windows.Forms.TextBox
    Friend WithEvents cmdReload As System.Windows.Forms.Button
    Friend WithEvents lblURL As System.Windows.Forms.Label
    Friend WithEvents cmdPlus As System.Windows.Forms.Button
    Friend WithEvents cmdMin As System.Windows.Forms.Button
    Friend WithEvents cmdSend As System.Windows.Forms.Button
    Friend WithEvents cmdCancel As System.Windows.Forms.Button
End Class
