<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmPostDetails
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPostDetails))
        Me.txtPostData = New System.Windows.Forms.TextBox()
        Me.line = New System.Windows.Forms.GroupBox()
        Me.cmdParse = New System.Windows.Forms.Button()
        Me.postcontrols = New System.Windows.Forms.GroupBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.lblURL = New System.Windows.Forms.Label()
        Me.txtURL = New System.Windows.Forms.TextBox()
        Me.cmdPluginDisqus = New System.Windows.Forms.Button()
        Me.cmdDummy = New System.Windows.Forms.Button()
        Me.txtUrlEncoded = New System.Windows.Forms.TextBox()
        Me.txtNotUrlEncoded = New System.Windows.Forms.TextBox()
        Me.txtTemporaryURLDecoded = New System.Windows.Forms.TextBox()
        Me.lblHdrValue = New System.Windows.Forms.Label()
        Me.lblHdrVarName = New System.Windows.Forms.Label()
        Me.txtDataLabelTemplate = New System.Windows.Forms.Label()
        Me.postcontrols.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtPostData
        '
        Me.txtPostData.HideSelection = False
        Me.txtPostData.Location = New System.Drawing.Point(12, 1)
        Me.txtPostData.Multiline = True
        Me.txtPostData.Name = "txtPostData"
        Me.txtPostData.Size = New System.Drawing.Size(399, 324)
        Me.txtPostData.TabIndex = 0
        Me.txtPostData.Text = resources.GetString("txtPostData.Text")
        '
        'line
        '
        Me.line.Location = New System.Drawing.Point(417, 1)
        Me.line.Name = "line"
        Me.line.Size = New System.Drawing.Size(2, 325)
        Me.line.TabIndex = 2
        Me.line.TabStop = False
        '
        'cmdParse
        '
        Me.cmdParse.Location = New System.Drawing.Point(12, 331)
        Me.cmdParse.Name = "cmdParse"
        Me.cmdParse.Size = New System.Drawing.Size(399, 38)
        Me.cmdParse.TabIndex = 4
        Me.cmdParse.Text = "Parse"
        Me.cmdParse.UseVisualStyleBackColor = True
        '
        'postcontrols
        '
        Me.postcontrols.Controls.Add(Me.txtDataLabelTemplate)
        Me.postcontrols.Controls.Add(Me.lblHdrValue)
        Me.postcontrols.Controls.Add(Me.lblHdrVarName)
        Me.postcontrols.Controls.Add(Me.Label1)
        Me.postcontrols.Controls.Add(Me.lblURL)
        Me.postcontrols.Controls.Add(Me.txtURL)
        Me.postcontrols.Location = New System.Drawing.Point(425, 1)
        Me.postcontrols.Name = "postcontrols"
        Me.postcontrols.Size = New System.Drawing.Size(421, 324)
        Me.postcontrols.TabIndex = 5
        Me.postcontrols.TabStop = False
        Me.postcontrols.Text = "POST FIELDS"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.Label1.Location = New System.Drawing.Point(328, 52)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(38, 9)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "HTTP/1.1"
        '
        'lblURL
        '
        Me.lblURL.AutoSize = True
        Me.lblURL.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblURL.ForeColor = System.Drawing.SystemColors.ActiveCaption
        Me.lblURL.Location = New System.Drawing.Point(12, 41)
        Me.lblURL.Name = "lblURL"
        Me.lblURL.Size = New System.Drawing.Size(78, 12)
        Me.lblURL.TabIndex = 5
        Me.lblURL.Text = "Location Path:"
        '
        'txtURL
        '
        Me.txtURL.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtURL.HideSelection = False
        Me.txtURL.Location = New System.Drawing.Point(83, 38)
        Me.txtURL.Name = "txtURL"
        Me.txtURL.Size = New System.Drawing.Size(237, 18)
        Me.txtURL.TabIndex = 4
        '
        'cmdPluginDisqus
        '
        Me.cmdPluginDisqus.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdPluginDisqus.Location = New System.Drawing.Point(425, 331)
        Me.cmdPluginDisqus.Name = "cmdPluginDisqus"
        Me.cmdPluginDisqus.Size = New System.Drawing.Size(118, 20)
        Me.cmdPluginDisqus.TabIndex = 8
        Me.cmdPluginDisqus.Text = "Run Plugin.Disqus"
        Me.cmdPluginDisqus.UseVisualStyleBackColor = True
        '
        'cmdDummy
        '
        Me.cmdDummy.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdDummy.Location = New System.Drawing.Point(570, 331)
        Me.cmdDummy.Name = "cmdDummy"
        Me.cmdDummy.Size = New System.Drawing.Size(192, 20)
        Me.cmdDummy.TabIndex = 9
        Me.cmdDummy.Text = "Compile Social I.D. Chrome Extension"
        Me.cmdDummy.UseVisualStyleBackColor = True
        '
        'txtUrlEncoded
        '
        Me.txtUrlEncoded.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer))
        Me.txtUrlEncoded.Location = New System.Drawing.Point(242, 12)
        Me.txtUrlEncoded.Name = "txtUrlEncoded"
        Me.txtUrlEncoded.Size = New System.Drawing.Size(66, 20)
        Me.txtUrlEncoded.TabIndex = 10
        Me.txtUrlEncoded.Visible = False
        '
        'txtNotUrlEncoded
        '
        Me.txtNotUrlEncoded.BackColor = System.Drawing.Color.FromArgb(CType(CType(128, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(128, Byte), Integer))
        Me.txtNotUrlEncoded.Location = New System.Drawing.Point(242, 38)
        Me.txtNotUrlEncoded.Name = "txtNotUrlEncoded"
        Me.txtNotUrlEncoded.Size = New System.Drawing.Size(66, 20)
        Me.txtNotUrlEncoded.TabIndex = 11
        Me.txtNotUrlEncoded.Visible = False
        '
        'txtTemporaryURLDecoded
        '
        Me.txtTemporaryURLDecoded.BackColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(224, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.txtTemporaryURLDecoded.Location = New System.Drawing.Point(242, 64)
        Me.txtTemporaryURLDecoded.Name = "txtTemporaryURLDecoded"
        Me.txtTemporaryURLDecoded.Size = New System.Drawing.Size(66, 20)
        Me.txtTemporaryURLDecoded.TabIndex = 12
        Me.txtTemporaryURLDecoded.Visible = False
        '
        'lblHdrValue
        '
        Me.lblHdrValue.AutoSize = True
        Me.lblHdrValue.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHdrValue.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblHdrValue.Location = New System.Drawing.Point(81, 19)
        Me.lblHdrValue.Name = "lblHdrValue"
        Me.lblHdrValue.Size = New System.Drawing.Size(37, 12)
        Me.lblHdrValue.TabIndex = 14
        Me.lblHdrValue.Text = "VALUE"
        '
        'lblHdrVarName
        '
        Me.lblHdrVarName.AutoSize = True
        Me.lblHdrVarName.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblHdrVarName.ForeColor = System.Drawing.SystemColors.ControlText
        Me.lblHdrVarName.Location = New System.Drawing.Point(12, 19)
        Me.lblHdrVarName.Name = "lblHdrVarName"
        Me.lblHdrVarName.Size = New System.Drawing.Size(34, 12)
        Me.lblHdrVarName.TabIndex = 13
        Me.lblHdrVarName.Text = "NAME"
        '
        'txtDataLabelTemplate
        '
        Me.txtDataLabelTemplate.AutoSize = True
        Me.txtDataLabelTemplate.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtDataLabelTemplate.ForeColor = System.Drawing.Color.DarkMagenta
        Me.txtDataLabelTemplate.Location = New System.Drawing.Point(12, 71)
        Me.txtDataLabelTemplate.Name = "txtDataLabelTemplate"
        Me.txtDataLabelTemplate.Size = New System.Drawing.Size(102, 12)
        Me.txtDataLabelTemplate.TabIndex = 16
        Me.txtDataLabelTemplate.Text = "data label textcolor"
        Me.txtDataLabelTemplate.Visible = False
        '
        'frmPostDetails
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(858, 381)
        Me.Controls.Add(Me.txtTemporaryURLDecoded)
        Me.Controls.Add(Me.txtNotUrlEncoded)
        Me.Controls.Add(Me.txtUrlEncoded)
        Me.Controls.Add(Me.cmdDummy)
        Me.Controls.Add(Me.cmdPluginDisqus)
        Me.Controls.Add(Me.postcontrols)
        Me.Controls.Add(Me.cmdParse)
        Me.Controls.Add(Me.line)
        Me.Controls.Add(Me.txtPostData)
        Me.MaximizeBox = False
        Me.Name = "frmPostDetails"
        Me.Text = "Post Extracter"
        Me.postcontrols.ResumeLayout(False)
        Me.postcontrols.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtPostData As System.Windows.Forms.TextBox
    Friend WithEvents line As System.Windows.Forms.GroupBox
    Friend WithEvents cmdParse As System.Windows.Forms.Button
    Friend WithEvents postcontrols As System.Windows.Forms.GroupBox
    Friend WithEvents lblURL As System.Windows.Forms.Label
    Friend WithEvents txtURL As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdPluginDisqus As System.Windows.Forms.Button
    Friend WithEvents cmdDummy As System.Windows.Forms.Button
    Friend WithEvents txtUrlEncoded As System.Windows.Forms.TextBox
    Friend WithEvents txtNotUrlEncoded As System.Windows.Forms.TextBox
    Friend WithEvents txtTemporaryURLDecoded As System.Windows.Forms.TextBox
    Friend WithEvents lblHdrValue As System.Windows.Forms.Label
    Friend WithEvents lblHdrVarName As System.Windows.Forms.Label
    Friend WithEvents txtDataLabelTemplate As System.Windows.Forms.Label

End Class
