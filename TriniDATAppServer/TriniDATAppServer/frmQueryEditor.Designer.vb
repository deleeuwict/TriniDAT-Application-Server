<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmQueryEditor
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
        Me.components = New System.ComponentModel.Container()
        Me.lstProgram = New System.Windows.Forms.ComboBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.mnuMutations = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuInsert = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDelete = New System.Windows.Forms.ToolStripMenuItem()
        Me.cmdSave = New System.Windows.Forms.Button()
        Me.Button2 = New System.Windows.Forms.Button()
        Me.ShapeContainer1 = New Microsoft.VisualBasic.PowerPacks.ShapeContainer()
        Me.windowBar = New Microsoft.VisualBasic.PowerPacks.RectangleShape()
        Me.lblWindowTitle = New System.Windows.Forms.Label()
        Me.lblCloseWindow = New System.Windows.Forms.Label()
        Me.lblMinimizeWindow = New System.Windows.Forms.Label()
        Me.tabsAppEditor = New BosswaveTabbed()
        Me.tabMPConfig1 = New System.Windows.Forms.TabPage()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.lvFunction = New System.Windows.Forms.ListView()
        Me.tabsAction = New BosswaveTabbed()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.TabPage5 = New System.Windows.Forms.TabPage()
        Me.lvPutStatMethod = New System.Windows.Forms.ListView()
        Me.ColumnHeader14 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader15 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader16 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPage6 = New System.Windows.Forms.TabPage()
        Me.lstGetMethod = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.listMappingPoint = New System.Windows.Forms.ComboBox()
        Me.lblNoData = New System.Windows.Forms.Label()
        Me.lvJClass = New System.Windows.Forms.ListView()
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.lvCompilerConfigurationEditor = New System.Windows.Forms.ListView()
        Me.ColumnHeader12 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader13 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.cmdBuild = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.RichTextBox1 = New System.Windows.Forms.RichTextBox()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.TextBox1 = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.mnuMutations.SuspendLayout()
        Me.tabsAppEditor.SuspendLayout()
        Me.tabMPConfig1.SuspendLayout()
        Me.tabsAction.SuspendLayout()
        Me.TabPage5.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        Me.SuspendLayout()
        '
        'lstProgram
        '
        Me.lstProgram.BackColor = System.Drawing.Color.Beige
        Me.lstProgram.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstProgram.FormattingEnabled = True
        Me.lstProgram.Location = New System.Drawing.Point(114, 28)
        Me.lstProgram.Name = "lstProgram"
        Me.lstProgram.Size = New System.Drawing.Size(356, 20)
        Me.lstProgram.TabIndex = 14
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.ForeColor = System.Drawing.Color.Black
        Me.Label5.Location = New System.Drawing.Point(8, 32)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(102, 12)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Server Profile:"
        '
        'mnuMutations
        '
        Me.mnuMutations.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuInsert, Me.mnuDelete})
        Me.mnuMutations.Name = "mnuInsertCondition"
        Me.mnuMutations.Size = New System.Drawing.Size(129, 48)
        '
        'mnuInsert
        '
        Me.mnuInsert.Name = "mnuInsert"
        Me.mnuInsert.Size = New System.Drawing.Size(128, 22)
        Me.mnuInsert.Text = "Insert Field"
        '
        'mnuDelete
        '
        Me.mnuDelete.Name = "mnuDelete"
        Me.mnuDelete.Size = New System.Drawing.Size(128, 22)
        Me.mnuDelete.Text = "Delete"
        '
        'cmdSave
        '
        Me.cmdSave.BackColor = System.Drawing.SystemColors.ControlDark
        Me.cmdSave.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdSave.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdSave.Location = New System.Drawing.Point(477, 27)
        Me.cmdSave.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdSave.Name = "cmdSave"
        Me.cmdSave.Size = New System.Drawing.Size(83, 21)
        Me.cmdSave.TabIndex = 16
        Me.cmdSave.Text = "Save"
        Me.cmdSave.UseVisualStyleBackColor = False
        '
        'Button2
        '
        Me.Button2.BackColor = System.Drawing.SystemColors.ControlDark
        Me.Button2.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.Button2.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.Button2.Location = New System.Drawing.Point(568, 27)
        Me.Button2.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Button2.Name = "Button2"
        Me.Button2.Size = New System.Drawing.Size(83, 21)
        Me.Button2.TabIndex = 17
        Me.Button2.Text = "Edit"
        Me.Button2.UseVisualStyleBackColor = False
        '
        'ShapeContainer1
        '
        Me.ShapeContainer1.Location = New System.Drawing.Point(0, 0)
        Me.ShapeContainer1.Margin = New System.Windows.Forms.Padding(0)
        Me.ShapeContainer1.Name = "ShapeContainer1"
        Me.ShapeContainer1.Shapes.AddRange(New Microsoft.VisualBasic.PowerPacks.Shape() {Me.windowBar})
        Me.ShapeContainer1.Size = New System.Drawing.Size(660, 658)
        Me.ShapeContainer1.TabIndex = 18
        Me.ShapeContainer1.TabStop = False
        '
        'windowBar
        '
        Me.windowBar.BackColor = System.Drawing.SystemColors.ControlDark
        Me.windowBar.BackStyle = Microsoft.VisualBasic.PowerPacks.BackStyle.Opaque
        Me.windowBar.BorderColor = System.Drawing.SystemColors.Control
        Me.windowBar.Location = New System.Drawing.Point(0, -9)
        Me.windowBar.Name = "windowBar"
        Me.windowBar.Size = New System.Drawing.Size(660, 28)
        '
        'lblWindowTitle
        '
        Me.lblWindowTitle.AutoSize = True
        Me.lblWindowTitle.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblWindowTitle.ForeColor = System.Drawing.Color.Gold
        Me.lblWindowTitle.Location = New System.Drawing.Point(8, 3)
        Me.lblWindowTitle.Name = "lblWindowTitle"
        Me.lblWindowTitle.Size = New System.Drawing.Size(213, 12)
        Me.lblWindowTitle.TabIndex = 19
        Me.lblWindowTitle.Text = ":: Bosswave | Automation Module"
        '
        'lblCloseWindow
        '
        Me.lblCloseWindow.AutoSize = True
        Me.lblCloseWindow.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblCloseWindow.ForeColor = System.Drawing.Color.Gold
        Me.lblCloseWindow.Location = New System.Drawing.Point(638, 2)
        Me.lblCloseWindow.Name = "lblCloseWindow"
        Me.lblCloseWindow.Size = New System.Drawing.Size(13, 12)
        Me.lblCloseWindow.TabIndex = 20
        Me.lblCloseWindow.Text = "X"
        '
        'lblMinimizeWindow
        '
        Me.lblMinimizeWindow.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblMinimizeWindow.Font = New System.Drawing.Font("BankGothic Md BT", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMinimizeWindow.ForeColor = System.Drawing.Color.Gold
        Me.lblMinimizeWindow.Location = New System.Drawing.Point(623, 2)
        Me.lblMinimizeWindow.Name = "lblMinimizeWindow"
        Me.lblMinimizeWindow.Size = New System.Drawing.Size(12, 10)
        Me.lblMinimizeWindow.TabIndex = 21
        Me.lblMinimizeWindow.Text = "-"
        '
        'tabsAppEditor
        '
        Me.tabsAppEditor.Alignment = System.Windows.Forms.TabAlignment.Bottom
        Me.tabsAppEditor.Controls.Add(Me.tabMPConfig1)
        Me.tabsAppEditor.Controls.Add(Me.TabPage2)
        Me.tabsAppEditor.Location = New System.Drawing.Point(1, 55)
        Me.tabsAppEditor.Name = "tabsAppEditor"
        Me.tabsAppEditor.SelectedIndex = 0
        Me.tabsAppEditor.Size = New System.Drawing.Size(654, 597)
        Me.tabsAppEditor.TabIndex = 15
        '
        'tabMPConfig1
        '
        Me.tabMPConfig1.BackColor = System.Drawing.Color.SlateGray
        Me.tabMPConfig1.Controls.Add(Me.Label2)
        Me.tabMPConfig1.Controls.Add(Me.lvFunction)
        Me.tabMPConfig1.Controls.Add(Me.tabsAction)
        Me.tabMPConfig1.Controls.Add(Me.lstGetMethod)
        Me.tabMPConfig1.Controls.Add(Me.Label6)
        Me.tabMPConfig1.Controls.Add(Me.listMappingPoint)
        Me.tabMPConfig1.Controls.Add(Me.lblNoData)
        Me.tabMPConfig1.Controls.Add(Me.lvJClass)
        Me.tabMPConfig1.Location = New System.Drawing.Point(4, 4)
        Me.tabMPConfig1.Name = "tabMPConfig1"
        Me.tabMPConfig1.Padding = New System.Windows.Forms.Padding(3)
        Me.tabMPConfig1.Size = New System.Drawing.Size(646, 568)
        Me.tabMPConfig1.TabIndex = 0
        Me.tabMPConfig1.Text = "App Design"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(9, 136)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(60, 12)
        Me.Label2.TabIndex = 28
        Me.Label2.Text = "Function"
        '
        'lvFunction
        '
        Me.lvFunction.Location = New System.Drawing.Point(7, 157)
        Me.lvFunction.Name = "lvFunction"
        Me.lvFunction.Size = New System.Drawing.Size(612, 94)
        Me.lvFunction.TabIndex = 27
        Me.lvFunction.UseCompatibleStateImageBehavior = False
        '
        'tabsAction
        '
        Me.tabsAction.Alignment = System.Windows.Forms.TabAlignment.Bottom
        Me.tabsAction.Controls.Add(Me.TabPage1)
        Me.tabsAction.Controls.Add(Me.TabPage5)
        Me.tabsAction.Controls.Add(Me.TabPage6)
        Me.tabsAction.Location = New System.Drawing.Point(7, 264)
        Me.tabsAction.Name = "tabsAction"
        Me.tabsAction.SelectedIndex = 0
        Me.tabsAction.Size = New System.Drawing.Size(632, 289)
        Me.tabsAction.TabIndex = 26
        '
        'TabPage1
        '
        Me.TabPage1.BackColor = System.Drawing.Color.SlateGray
        Me.TabPage1.Location = New System.Drawing.Point(4, 4)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(624, 260)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "Forwarder"
        '
        'TabPage5
        '
        Me.TabPage5.Controls.Add(Me.lvPutStatMethod)
        Me.TabPage5.Location = New System.Drawing.Point(4, 4)
        Me.TabPage5.Name = "TabPage5"
        Me.TabPage5.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage5.Size = New System.Drawing.Size(624, 260)
        Me.TabPage5.TabIndex = 1
        Me.TabPage5.Text = "Drop Statistical Item"
        Me.TabPage5.UseVisualStyleBackColor = True
        '
        'lvPutStatMethod
        '
        Me.lvPutStatMethod.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lvPutStatMethod.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader14, Me.ColumnHeader15, Me.ColumnHeader16})
        Me.lvPutStatMethod.ContextMenuStrip = Me.mnuMutations
        Me.lvPutStatMethod.FullRowSelect = True
        Me.lvPutStatMethod.GridLines = True
        Me.lvPutStatMethod.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.lvPutStatMethod.LabelEdit = True
        Me.lvPutStatMethod.Location = New System.Drawing.Point(5, 11)
        Me.lvPutStatMethod.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.lvPutStatMethod.Name = "lvPutStatMethod"
        Me.lvPutStatMethod.Size = New System.Drawing.Size(599, 195)
        Me.lvPutStatMethod.TabIndex = 15
        Me.lvPutStatMethod.UseCompatibleStateImageBehavior = False
        Me.lvPutStatMethod.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader14
        '
        Me.ColumnHeader14.Text = "Object"
        Me.ColumnHeader14.Width = 200
        '
        'ColumnHeader15
        '
        Me.ColumnHeader15.Text = "Condition"
        Me.ColumnHeader15.Width = 200
        '
        'ColumnHeader16
        '
        Me.ColumnHeader16.Text = "Value"
        Me.ColumnHeader16.Width = 200
        '
        'TabPage6
        '
        Me.TabPage6.Location = New System.Drawing.Point(4, 4)
        Me.TabPage6.Name = "TabPage6"
        Me.TabPage6.Size = New System.Drawing.Size(624, 260)
        Me.TabPage6.TabIndex = 2
        Me.TabPage6.Text = "Switch DP"
        Me.TabPage6.UseVisualStyleBackColor = True
        '
        'lstGetMethod
        '
        Me.lstGetMethod.BackColor = System.Drawing.Color.Beige
        Me.lstGetMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.lstGetMethod.FormattingEnabled = True
        Me.lstGetMethod.Items.AddRange(New Object() {"POST", "GET"})
        Me.lstGetMethod.Location = New System.Drawing.Point(573, 8)
        Me.lstGetMethod.Name = "lstGetMethod"
        Me.lstGetMethod.Size = New System.Drawing.Size(70, 20)
        Me.lstGetMethod.TabIndex = 25
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(495, 12)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(76, 12)
        Me.Label6.TabIndex = 24
        Me.Label6.Text = "Invoked by:"
        '
        'listMappingPoint
        '
        Me.listMappingPoint.BackColor = System.Drawing.Color.Beige
        Me.listMappingPoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.listMappingPoint.FormattingEnabled = True
        Me.listMappingPoint.Location = New System.Drawing.Point(9, 8)
        Me.listMappingPoint.Name = "listMappingPoint"
        Me.listMappingPoint.Size = New System.Drawing.Size(360, 20)
        Me.listMappingPoint.TabIndex = 23
        '
        'lblNoData
        '
        Me.lblNoData.AutoSize = True
        Me.lblNoData.BackColor = System.Drawing.Color.Beige
        Me.lblNoData.Font = New System.Drawing.Font("BankGothic Md BT", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNoData.ForeColor = System.Drawing.Color.Maroon
        Me.lblNoData.Location = New System.Drawing.Point(16, 82)
        Me.lblNoData.Name = "lblNoData"
        Me.lblNoData.Size = New System.Drawing.Size(578, 17)
        Me.lblNoData.TabIndex = 22
        Me.lblNoData.Text = "No statistics have been generated by this mapping point."
        Me.lblNoData.Visible = False
        '
        'lvJClass
        '
        Me.lvJClass.BackColor = System.Drawing.Color.Beige
        Me.lvJClass.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5})
        Me.lvJClass.FullRowSelect = True
        Me.lvJClass.GridLines = True
        Me.lvJClass.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.lvJClass.Location = New System.Drawing.Point(8, 31)
        Me.lvJClass.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.lvJClass.Name = "lvJClass"
        Me.lvJClass.Size = New System.Drawing.Size(631, 90)
        Me.lvJClass.TabIndex = 16
        Me.lvJClass.UseCompatibleStateImageBehavior = False
        Me.lvJClass.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Index"
        Me.ColumnHeader3.Width = 50
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Name"
        Me.ColumnHeader4.Width = 200
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Functional Description"
        Me.ColumnHeader5.Width = 300
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.lvCompilerConfigurationEditor)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Controls.Add(Me.cmdBuild)
        Me.TabPage2.Controls.Add(Me.Label4)
        Me.TabPage2.Controls.Add(Me.RichTextBox1)
        Me.TabPage2.Controls.Add(Me.Button1)
        Me.TabPage2.Controls.Add(Me.TextBox1)
        Me.TabPage2.Controls.Add(Me.Label3)
        Me.TabPage2.Location = New System.Drawing.Point(4, 4)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(646, 568)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Builder"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'lvCompilerConfigurationEditor
        '
        Me.lvCompilerConfigurationEditor.BackColor = System.Drawing.Color.Beige
        Me.lvCompilerConfigurationEditor.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader12, Me.ColumnHeader13})
        Me.lvCompilerConfigurationEditor.FullRowSelect = True
        Me.lvCompilerConfigurationEditor.GridLines = True
        Me.lvCompilerConfigurationEditor.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.lvCompilerConfigurationEditor.LabelEdit = True
        Me.lvCompilerConfigurationEditor.Location = New System.Drawing.Point(12, 90)
        Me.lvCompilerConfigurationEditor.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.lvCompilerConfigurationEditor.Name = "lvCompilerConfigurationEditor"
        Me.lvCompilerConfigurationEditor.Size = New System.Drawing.Size(615, 97)
        Me.lvCompilerConfigurationEditor.TabIndex = 14
        Me.lvCompilerConfigurationEditor.UseCompatibleStateImageBehavior = False
        Me.lvCompilerConfigurationEditor.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader12
        '
        Me.ColumnHeader12.Text = "Setting Name"
        Me.ColumnHeader12.Width = 200
        '
        'ColumnHeader13
        '
        Me.ColumnHeader13.Text = "Value"
        Me.ColumnHeader13.Width = 200
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.Black
        Me.Label1.Location = New System.Drawing.Point(14, 69)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(53, 12)
        Me.Label1.TabIndex = 6
        Me.Label1.Text = "Config"
        '
        'cmdBuild
        '
        Me.cmdBuild.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdBuild.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdBuild.Location = New System.Drawing.Point(106, 46)
        Me.cmdBuild.Name = "cmdBuild"
        Me.cmdBuild.Size = New System.Drawing.Size(408, 22)
        Me.cmdBuild.TabIndex = 5
        Me.cmdBuild.Text = "Build Solution"
        Me.cmdBuild.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label4.ForeColor = System.Drawing.Color.Black
        Me.Label4.Location = New System.Drawing.Point(14, 250)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(143, 12)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Compilation Result"
        '
        'RichTextBox1
        '
        Me.RichTextBox1.BackColor = System.Drawing.Color.Beige
        Me.RichTextBox1.Location = New System.Drawing.Point(12, 265)
        Me.RichTextBox1.Name = "RichTextBox1"
        Me.RichTextBox1.Size = New System.Drawing.Size(618, 294)
        Me.RichTextBox1.TabIndex = 3
        Me.RichTextBox1.Text = ""
        '
        'Button1
        '
        Me.Button1.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.Button1.Font = New System.Drawing.Font("BankGothic Md BT", 5.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Button1.Location = New System.Drawing.Point(525, 14)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(33, 23)
        Me.Button1.TabIndex = 2
        Me.Button1.Text = "..."
        Me.Button1.UseVisualStyleBackColor = True
        '
        'TextBox1
        '
        Me.TextBox1.Location = New System.Drawing.Point(106, 19)
        Me.TextBox1.Name = "TextBox1"
        Me.TextBox1.Size = New System.Drawing.Size(408, 19)
        Me.TextBox1.TabIndex = 1
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.ForeColor = System.Drawing.Color.Gold
        Me.Label3.Location = New System.Drawing.Point(10, 24)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(87, 12)
        Me.Label3.TabIndex = 0
        Me.Label3.Text = "Output Path:"
        '
        'frmQueryEditor
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.SlateGray
        Me.ClientSize = New System.Drawing.Size(660, 658)
        Me.ControlBox = False
        Me.Controls.Add(Me.lblMinimizeWindow)
        Me.Controls.Add(Me.lblWindowTitle)
        Me.Controls.Add(Me.Button2)
        Me.Controls.Add(Me.cmdSave)
        Me.Controls.Add(Me.tabsAppEditor)
        Me.Controls.Add(Me.lstProgram)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.lblCloseWindow)
        Me.Controls.Add(Me.ShapeContainer1)
        Me.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.MaximizeBox = False
        Me.Name = "frmQueryEditor"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Mapping Point | Automation Profile"
        Me.TopMost = True
        Me.mnuMutations.ResumeLayout(False)
        Me.tabsAppEditor.ResumeLayout(False)
        Me.tabMPConfig1.ResumeLayout(False)
        Me.tabMPConfig1.PerformLayout()
        Me.tabsAction.ResumeLayout(False)
        Me.TabPage5.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lstProgram As System.Windows.Forms.ComboBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents tabsAppEditor As BosswaveTabbed
    Friend WithEvents tabMPConfig1 As System.Windows.Forms.TabPage
    Friend WithEvents lvJClass As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents lblNoData As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents TextBox1 As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents RichTextBox1 As System.Windows.Forms.RichTextBox
    Friend WithEvents cmdBuild As System.Windows.Forms.Button
    Friend WithEvents listMappingPoint As System.Windows.Forms.ComboBox
    Friend WithEvents cmdSave As System.Windows.Forms.Button
    Friend WithEvents Button2 As System.Windows.Forms.Button
    Friend WithEvents lstGetMethod As System.Windows.Forms.ComboBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lvCompilerConfigurationEditor As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader12 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader13 As System.Windows.Forms.ColumnHeader
    Friend WithEvents mnuMutations As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuInsert As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuDelete As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ShapeContainer1 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    Friend WithEvents windowBar As Microsoft.VisualBasic.PowerPacks.RectangleShape
    Friend WithEvents lblWindowTitle As System.Windows.Forms.Label
    Friend WithEvents lblCloseWindow As System.Windows.Forms.Label
    Friend WithEvents lblMinimizeWindow As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents lvFunction As System.Windows.Forms.ListView
    Friend WithEvents tabsAction As BosswaveTabbed
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage5 As System.Windows.Forms.TabPage
    Friend WithEvents lvPutStatMethod As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader14 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader15 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader16 As System.Windows.Forms.ColumnHeader
    Friend WithEvents TabPage6 As System.Windows.Forms.TabPage


End Class
