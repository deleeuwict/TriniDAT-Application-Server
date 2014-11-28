<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmAppLicense
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
        Me.txtLicense = New System.Windows.Forms.TextBox()
        Me.chkAccept = New System.Windows.Forms.CheckBox()
        Me.cmdInstall = New System.Windows.Forms.Button()
        Me.cmdStop = New System.Windows.Forms.Button()
        Me.ShapeContainer1 = New Microsoft.VisualBasic.PowerPacks.ShapeContainer()
        Me.RectangleShape1 = New Microsoft.VisualBasic.PowerPacks.RectangleShape()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'txtLicense
        '
        Me.txtLicense.BackColor = System.Drawing.Color.White
        Me.txtLicense.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtLicense.Font = New System.Drawing.Font("Verdana", 11.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtLicense.Location = New System.Drawing.Point(-22, 22)
        Me.txtLicense.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.txtLicense.Multiline = True
        Me.txtLicense.Name = "txtLicense"
        Me.txtLicense.ReadOnly = True
        Me.txtLicense.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtLicense.Size = New System.Drawing.Size(618, 344)
        Me.txtLicense.TabIndex = 0
        '
        'chkAccept
        '
        Me.chkAccept.AutoSize = True
        Me.chkAccept.BackColor = System.Drawing.Color.White
        Me.chkAccept.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.chkAccept.ForeColor = System.Drawing.Color.Red
        Me.chkAccept.Location = New System.Drawing.Point(147, 348)
        Me.chkAccept.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.chkAccept.Name = "chkAccept"
        Me.chkAccept.Size = New System.Drawing.Size(250, 16)
        Me.chkAccept.TabIndex = 1
        Me.chkAccept.Text = "i accept the terms && conditions"
        Me.chkAccept.UseVisualStyleBackColor = False
        '
        'cmdInstall
        '
        Me.cmdInstall.BackColor = System.Drawing.Color.Gold
        Me.cmdInstall.Enabled = False
        Me.cmdInstall.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdInstall.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdInstall.ForeColor = System.Drawing.Color.WhiteSmoke
        Me.cmdInstall.Location = New System.Drawing.Point(395, 345)
        Me.cmdInstall.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdInstall.Name = "cmdInstall"
        Me.cmdInstall.Size = New System.Drawing.Size(119, 20)
        Me.cmdInstall.TabIndex = 13
        Me.cmdInstall.Text = "Install"
        Me.cmdInstall.UseVisualStyleBackColor = False
        '
        'cmdStop
        '
        Me.cmdStop.BackColor = System.Drawing.SystemColors.ControlDarkDark
        Me.cmdStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdStop.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdStop.ForeColor = System.Drawing.Color.WhiteSmoke
        Me.cmdStop.Location = New System.Drawing.Point(522, 345)
        Me.cmdStop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdStop.Name = "cmdStop"
        Me.cmdStop.Size = New System.Drawing.Size(53, 20)
        Me.cmdStop.TabIndex = 14
        Me.cmdStop.Text = "Stop"
        Me.cmdStop.UseVisualStyleBackColor = False
        '
        'ShapeContainer1
        '
        Me.ShapeContainer1.Location = New System.Drawing.Point(0, 0)
        Me.ShapeContainer1.Margin = New System.Windows.Forms.Padding(0)
        Me.ShapeContainer1.Name = "ShapeContainer1"
        Me.ShapeContainer1.Shapes.AddRange(New Microsoft.VisualBasic.PowerPacks.Shape() {Me.RectangleShape1})
        Me.ShapeContainer1.Size = New System.Drawing.Size(600, 366)
        Me.ShapeContainer1.TabIndex = 15
        Me.ShapeContainer1.TabStop = False
        '
        'RectangleShape1
        '
        Me.RectangleShape1.BackColor = System.Drawing.Color.Red
        Me.RectangleShape1.BackStyle = Microsoft.VisualBasic.PowerPacks.BackStyle.Opaque
        Me.RectangleShape1.BorderColor = System.Drawing.Color.LightPink
        Me.RectangleShape1.Location = New System.Drawing.Point(1, 1)
        Me.RectangleShape1.Name = "RectangleShape1"
        Me.RectangleShape1.Size = New System.Drawing.Size(830, 21)
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Red
        Me.Label1.ForeColor = System.Drawing.SystemColors.Window
        Me.Label1.Location = New System.Drawing.Point(7, 6)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(134, 12)
        Me.Label1.TabIndex = 16
        Me.Label1.Text = ":: License Agreement"
        '
        'frmAppLicense
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.LightSlateGray
        Me.ClientSize = New System.Drawing.Size(600, 366)
        Me.ControlBox = False
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmdStop)
        Me.Controls.Add(Me.cmdInstall)
        Me.Controls.Add(Me.chkAccept)
        Me.Controls.Add(Me.txtLicense)
        Me.Controls.Add(Me.ShapeContainer1)
        Me.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.MaximizeBox = False
        Me.Name = "frmAppLicense"
        Me.ShowIcon = False
        Me.ShowInTaskbar = False
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "License"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtLicense As System.Windows.Forms.TextBox
    Friend WithEvents chkAccept As System.Windows.Forms.CheckBox
    Friend WithEvents cmdInstall As System.Windows.Forms.Button
    Friend WithEvents cmdStop As System.Windows.Forms.Button
    Friend WithEvents ShapeContainer1 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    Friend WithEvents RectangleShape1 As Microsoft.VisualBasic.PowerPacks.RectangleShape
    Friend WithEvents Label1 As System.Windows.Forms.Label
End Class
