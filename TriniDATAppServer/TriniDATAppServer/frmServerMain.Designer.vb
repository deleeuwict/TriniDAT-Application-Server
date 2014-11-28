Imports TriniDAT

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmServerMain
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmServerMain))
        Me.grpControl = New System.Windows.Forms.GroupBox()
        Me.optSMDev = New System.Windows.Forms.RadioButton()
        Me.optSMProd = New System.Windows.Forms.RadioButton()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.cmdAppInstallation = New System.Windows.Forms.Button()
        Me.cmdCLS = New System.Windows.Forms.Button()
        Me.cmdOpenServerPage = New System.Windows.Forms.Button()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.cmdServerStop = New System.Windows.Forms.Button()
        Me.cmdServerStart = New System.Windows.Forms.Button()
        Me.WindowBar = New Microsoft.VisualBasic.PowerPacks.RectangleShape()
        Me.ShapeContainer1 = New Microsoft.VisualBasic.PowerPacks.ShapeContainer()
        Me.lblMinimizeWindow = New System.Windows.Forms.Label()
        Me.lblWindowTitle = New System.Windows.Forms.Label()
        Me.lblCloseWindow = New System.Windows.Forms.Label()
        Me.lblMaximize = New System.Windows.Forms.Label()
        Me.TraficLV = New System.Windows.Forms.ImageList(Me.components)
        Me.lblSizeGrip = New System.Windows.Forms.Label()
        Me.ctxmnuAttachment = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuAttachmentSpeakText = New System.Windows.Forms.ToolStripMenuItem()
        Me.ctxServerLog = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuServerLog_Copy = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuServerLog_Clear = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.mnuServerLog_GotoURL = New System.Windows.Forms.ToolStripMenuItem()
        Me.browseApp = New System.Windows.Forms.OpenFileDialog()
        Me.tabCsl = New TriniDAT.BosswaveTabbed2()
        Me.tabServerLog = New System.Windows.Forms.TabPage()
        Me.simonsProgressbar = New System.Windows.Forms.ProgressBar()
        Me.lstSimonsConsole = New System.Windows.Forms.ComboBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.cmdSpeechEnableToggle = New System.Windows.Forms.Button()
        Me.txtServerActivity = New System.Windows.Forms.RichTextBox()
        Me.ShapeContainer3 = New Microsoft.VisualBasic.PowerPacks.ShapeContainer()
        Me.SimonsLine = New Microsoft.VisualBasic.PowerPacks.LineShape()
        Me.tabRootBrowser = New System.Windows.Forms.TabPage()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.tabTrafficOverview = New System.Windows.Forms.TabPage()
        Me.cmdDumpInvalid = New System.Windows.Forms.Button()
        Me.lblContainerHeader = New System.Windows.Forms.Label()
        Me.lvTrafficEntryContent = New System.Windows.Forms.ListView()
        Me.chkTrafficScroll = New System.Windows.Forms.CheckBox()
        Me.cmdClearTrafficListview = New System.Windows.Forms.Button()
        Me.lblTo = New System.Windows.Forms.Label()
        Me.lblFrom = New System.Windows.Forms.Label()
        Me.txtTrafficObjectContents = New System.Windows.Forms.RichTextBox()
        Me.lblToCaption = New System.Windows.Forms.Label()
        Me.lblFromCaption = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lvObjectTraffic = New System.Windows.Forms.ListView()
        Me.ShapeContainer2 = New Microsoft.VisualBasic.PowerPacks.ShapeContainer()
        Me.LineShape1 = New Microsoft.VisualBasic.PowerPacks.LineShape()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.cmdUpdatePaykey = New System.Windows.Forms.Button()
        Me.lblLinkForum = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.lblPaymentRegister = New System.Windows.Forms.Label()
        Me.lblNoIdInfo = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cmdNetwork = New System.Windows.Forms.Button()
        Me.txtUserPaymentHandle = New System.Windows.Forms.TextBox()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.grpControl.SuspendLayout()
        Me.ctxmnuAttachment.SuspendLayout()
        Me.ctxServerLog.SuspendLayout()
        Me.tabCsl.SuspendLayout()
        Me.tabServerLog.SuspendLayout()
        Me.tabRootBrowser.SuspendLayout()
        Me.tabTrafficOverview.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.SuspendLayout()
        '
        'grpControl
        '
        Me.grpControl.Controls.Add(Me.optSMDev)
        Me.grpControl.Controls.Add(Me.optSMProd)
        Me.grpControl.Controls.Add(Me.Label3)
        Me.grpControl.Controls.Add(Me.cmdAppInstallation)
        Me.grpControl.Controls.Add(Me.cmdCLS)
        Me.grpControl.Controls.Add(Me.cmdOpenServerPage)
        Me.grpControl.Controls.Add(Me.Label2)
        Me.grpControl.Controls.Add(Me.cmdServerStop)
        Me.grpControl.Controls.Add(Me.cmdServerStart)
        Me.grpControl.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.grpControl.Location = New System.Drawing.Point(5, 21)
        Me.grpControl.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.grpControl.Name = "grpControl"
        Me.grpControl.Padding = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.grpControl.Size = New System.Drawing.Size(759, 45)
        Me.grpControl.TabIndex = 1
        Me.grpControl.TabStop = False
        '
        'optSMDev
        '
        Me.optSMDev.AutoSize = True
        Me.optSMDev.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optSMDev.Location = New System.Drawing.Point(612, 20)
        Me.optSMDev.Name = "optSMDev"
        Me.optSMDev.Size = New System.Drawing.Size(103, 16)
        Me.optSMDev.TabIndex = 15
        Me.optSMDev.Text = "Development"
        Me.optSMDev.UseVisualStyleBackColor = True
        '
        'optSMProd
        '
        Me.optSMProd.AutoSize = True
        Me.optSMProd.Checked = True
        Me.optSMProd.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.optSMProd.Location = New System.Drawing.Point(558, 20)
        Me.optSMProd.Name = "optSMProd"
        Me.optSMProd.Size = New System.Drawing.Size(48, 16)
        Me.optSMProd.TabIndex = 14
        Me.optSMProd.TabStop = True
        Me.optSMProd.Text = "Live"
        Me.optSMProd.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.Location = New System.Drawing.Point(510, 22)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(42, 12)
        Me.Label3.TabIndex = 13
        Me.Label3.Text = "Mode:"
        '
        'cmdAppInstallation
        '
        Me.cmdAppInstallation.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdAppInstallation.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdAppInstallation.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdAppInstallation.Location = New System.Drawing.Point(252, 18)
        Me.cmdAppInstallation.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdAppInstallation.Name = "cmdAppInstallation"
        Me.cmdAppInstallation.Size = New System.Drawing.Size(124, 20)
        Me.cmdAppInstallation.TabIndex = 12
        Me.cmdAppInstallation.Text = "Install App.."
        Me.cmdAppInstallation.UseVisualStyleBackColor = True
        '
        'cmdCLS
        '
        Me.cmdCLS.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdCLS.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdCLS.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdCLS.Location = New System.Drawing.Point(466, 18)
        Me.cmdCLS.Name = "cmdCLS"
        Me.cmdCLS.Size = New System.Drawing.Size(38, 20)
        Me.cmdCLS.TabIndex = 11
        Me.cmdCLS.Text = "CLR"
        Me.cmdCLS.UseVisualStyleBackColor = True
        '
        'cmdOpenServerPage
        '
        Me.cmdOpenServerPage.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdOpenServerPage.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdOpenServerPage.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdOpenServerPage.Location = New System.Drawing.Point(195, 18)
        Me.cmdOpenServerPage.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdOpenServerPage.Name = "cmdOpenServerPage"
        Me.cmdOpenServerPage.Size = New System.Drawing.Size(57, 20)
        Me.cmdOpenServerPage.TabIndex = 5
        Me.cmdOpenServerPage.Text = "WWW"
        Me.cmdOpenServerPage.UseVisualStyleBackColor = True
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.White
        Me.Label2.Location = New System.Drawing.Point(6, 22)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(68, 12)
        Me.Label2.TabIndex = 4
        Me.Label2.Text = "Controls:"
        '
        'cmdServerStop
        '
        Me.cmdServerStop.Enabled = False
        Me.cmdServerStop.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdServerStop.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdServerStop.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdServerStop.Location = New System.Drawing.Point(138, 18)
        Me.cmdServerStop.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdServerStop.Name = "cmdServerStop"
        Me.cmdServerStop.Size = New System.Drawing.Size(57, 20)
        Me.cmdServerStop.TabIndex = 1
        Me.cmdServerStop.Text = "Stop"
        Me.cmdServerStop.UseVisualStyleBackColor = True
        '
        'cmdServerStart
        '
        Me.cmdServerStart.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdServerStart.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdServerStart.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdServerStart.Location = New System.Drawing.Point(81, 18)
        Me.cmdServerStart.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdServerStart.Name = "cmdServerStart"
        Me.cmdServerStart.Size = New System.Drawing.Size(57, 20)
        Me.cmdServerStart.TabIndex = 0
        Me.cmdServerStart.Text = "Start"
        Me.cmdServerStart.UseVisualStyleBackColor = True
        '
        'WindowBar
        '
        Me.WindowBar.BackColor = System.Drawing.SystemColors.ControlDark
        Me.WindowBar.BackStyle = Microsoft.VisualBasic.PowerPacks.BackStyle.Opaque
        Me.WindowBar.BorderColor = System.Drawing.SystemColors.Control
        Me.WindowBar.Location = New System.Drawing.Point(-5, -10)
        Me.WindowBar.Name = "WindowBar"
        Me.WindowBar.Size = New System.Drawing.Size(777, 28)
        '
        'ShapeContainer1
        '
        Me.ShapeContainer1.Location = New System.Drawing.Point(0, 0)
        Me.ShapeContainer1.Margin = New System.Windows.Forms.Padding(0)
        Me.ShapeContainer1.Name = "ShapeContainer1"
        Me.ShapeContainer1.Shapes.AddRange(New Microsoft.VisualBasic.PowerPacks.Shape() {Me.WindowBar})
        Me.ShapeContainer1.Size = New System.Drawing.Size(770, 525)
        Me.ShapeContainer1.TabIndex = 6
        Me.ShapeContainer1.TabStop = False
        '
        'lblMinimizeWindow
        '
        Me.lblMinimizeWindow.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblMinimizeWindow.Font = New System.Drawing.Font("BankGothic Md BT", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMinimizeWindow.ForeColor = System.Drawing.Color.Gold
        Me.lblMinimizeWindow.Location = New System.Drawing.Point(719, 3)
        Me.lblMinimizeWindow.Name = "lblMinimizeWindow"
        Me.lblMinimizeWindow.Size = New System.Drawing.Size(12, 10)
        Me.lblMinimizeWindow.TabIndex = 23
        Me.lblMinimizeWindow.Text = "-"
        '
        'lblWindowTitle
        '
        Me.lblWindowTitle.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblWindowTitle.ForeColor = System.Drawing.Color.Gold
        Me.lblWindowTitle.Location = New System.Drawing.Point(10, 4)
        Me.lblWindowTitle.Name = "lblWindowTitle"
        Me.lblWindowTitle.Size = New System.Drawing.Size(499, 10)
        Me.lblWindowTitle.TabIndex = 22
        Me.lblWindowTitle.Text = ":: TriniDAT Data Application Server | (c) 2013 GertJan de Leeuw | De Leeuw ICT"
        '
        'lblCloseWindow
        '
        Me.lblCloseWindow.AutoSize = True
        Me.lblCloseWindow.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblCloseWindow.ForeColor = System.Drawing.Color.Gold
        Me.lblCloseWindow.Location = New System.Drawing.Point(753, 2)
        Me.lblCloseWindow.Name = "lblCloseWindow"
        Me.lblCloseWindow.Size = New System.Drawing.Size(13, 12)
        Me.lblCloseWindow.TabIndex = 24
        Me.lblCloseWindow.Text = "X"
        '
        'lblMaximize
        '
        Me.lblMaximize.BackColor = System.Drawing.SystemColors.ControlDark
        Me.lblMaximize.Font = New System.Drawing.Font("BankGothic Md BT", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblMaximize.ForeColor = System.Drawing.Color.Gold
        Me.lblMaximize.Location = New System.Drawing.Point(736, 2)
        Me.lblMaximize.Name = "lblMaximize"
        Me.lblMaximize.Size = New System.Drawing.Size(15, 10)
        Me.lblMaximize.TabIndex = 25
        Me.lblMaximize.Text = "+"
        '
        'TraficLV
        '
        Me.TraficLV.ImageStream = CType(resources.GetObject("TraficLV.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.TraficLV.TransparentColor = System.Drawing.Color.Transparent
        Me.TraficLV.Images.SetKeyName(0, "1365303260_34689.ico")
        Me.TraficLV.Images.SetKeyName(1, "1365303362_34721.ico")
        '
        'lblSizeGrip
        '
        Me.lblSizeGrip.BackColor = System.Drawing.Color.Yellow
        Me.lblSizeGrip.Font = New System.Drawing.Font("BankGothic Md BT", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSizeGrip.ForeColor = System.Drawing.Color.Gold
        Me.lblSizeGrip.Location = New System.Drawing.Point(737, 506)
        Me.lblSizeGrip.Name = "lblSizeGrip"
        Me.lblSizeGrip.Size = New System.Drawing.Size(32, 19)
        Me.lblSizeGrip.TabIndex = 27
        '
        'ctxmnuAttachment
        '
        Me.ctxmnuAttachment.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuAttachmentSpeakText})
        Me.ctxmnuAttachment.Name = "ctxmnuAttachment"
        Me.ctxmnuAttachment.Size = New System.Drawing.Size(104, 26)
        Me.ctxmnuAttachment.Text = "Speak"
        '
        'mnuAttachmentSpeakText
        '
        Me.mnuAttachmentSpeakText.Name = "mnuAttachmentSpeakText"
        Me.mnuAttachmentSpeakText.Size = New System.Drawing.Size(103, 22)
        Me.mnuAttachmentSpeakText.Text = "Speak"
        '
        'ctxServerLog
        '
        Me.ctxServerLog.BackColor = System.Drawing.Color.MidnightBlue
        Me.ctxServerLog.Font = New System.Drawing.Font("BankGothic Md BT", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ctxServerLog.ImageScalingSize = New System.Drawing.Size(32, 32)
        Me.ctxServerLog.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuServerLog_Copy, Me.mnuServerLog_Clear, Me.ToolStripSeparator1, Me.mnuServerLog_GotoURL})
        Me.ctxServerLog.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Table
        Me.ctxServerLog.Name = "ctxServerLog"
        Me.ctxServerLog.Size = New System.Drawing.Size(136, 124)
        '
        'mnuServerLog_Copy
        '
        Me.mnuServerLog_Copy.BackColor = System.Drawing.Color.MidnightBlue
        Me.mnuServerLog_Copy.Enabled = False
        Me.mnuServerLog_Copy.ForeColor = System.Drawing.Color.White
        Me.mnuServerLog_Copy.Image = Global.TriniDAT.My.Resources.Resources.copy
        Me.mnuServerLog_Copy.Name = "mnuServerLog_Copy"
        Me.mnuServerLog_Copy.Size = New System.Drawing.Size(135, 38)
        Me.mnuServerLog_Copy.Text = "Copy Text"
        '
        'mnuServerLog_Clear
        '
        Me.mnuServerLog_Clear.BackColor = System.Drawing.Color.MidnightBlue
        Me.mnuServerLog_Clear.ForeColor = System.Drawing.Color.White
        Me.mnuServerLog_Clear.Image = Global.TriniDAT.My.Resources.Resources._1365907209_BeOS_Documents_folder_1
        Me.mnuServerLog_Clear.Name = "mnuServerLog_Clear"
        Me.mnuServerLog_Clear.Size = New System.Drawing.Size(135, 38)
        Me.mnuServerLog_Clear.Text = "Clear"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.BackColor = System.Drawing.Color.MidnightBlue
        Me.ToolStripSeparator1.ForeColor = System.Drawing.Color.White
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(132, 6)
        '
        'mnuServerLog_GotoURL
        '
        Me.mnuServerLog_GotoURL.BackColor = System.Drawing.Color.MidnightBlue
        Me.mnuServerLog_GotoURL.Enabled = False
        Me.mnuServerLog_GotoURL.ForeColor = System.Drawing.Color.White
        Me.mnuServerLog_GotoURL.Image = Global.TriniDAT.My.Resources.Resources.gourl
        Me.mnuServerLog_GotoURL.Name = "mnuServerLog_GotoURL"
        Me.mnuServerLog_GotoURL.Size = New System.Drawing.Size(135, 38)
        Me.mnuServerLog_GotoURL.Text = "Go URL"
        '
        'browseApp
        '
        Me.browseApp.FileName = "*.zip"
        Me.browseApp.Filter = "ZIP Files|*.zip"
        '
        'tabCsl
        '
        Me.tabCsl.Alignment = System.Windows.Forms.TabAlignment.Bottom
        Me.tabCsl.Controls.Add(Me.tabServerLog)
        Me.tabCsl.Controls.Add(Me.tabRootBrowser)
        Me.tabCsl.Controls.Add(Me.tabTrafficOverview)
        Me.tabCsl.Controls.Add(Me.TabPage1)
        Me.tabCsl.Location = New System.Drawing.Point(4, 72)
        Me.tabCsl.Name = "tabCsl"
        Me.tabCsl.SelectedIndex = 0
        Me.tabCsl.Size = New System.Drawing.Size(760, 439)
        Me.tabCsl.TabIndex = 5
        '
        'tabServerLog
        '
        Me.tabServerLog.Controls.Add(Me.simonsProgressbar)
        Me.tabServerLog.Controls.Add(Me.lstSimonsConsole)
        Me.tabServerLog.Controls.Add(Me.Label9)
        Me.tabServerLog.Controls.Add(Me.cmdSpeechEnableToggle)
        Me.tabServerLog.Controls.Add(Me.txtServerActivity)
        Me.tabServerLog.Controls.Add(Me.ShapeContainer3)
        Me.tabServerLog.Location = New System.Drawing.Point(4, 4)
        Me.tabServerLog.Name = "tabServerLog"
        Me.tabServerLog.Padding = New System.Windows.Forms.Padding(3)
        Me.tabServerLog.Size = New System.Drawing.Size(752, 410)
        Me.tabServerLog.TabIndex = 0
        Me.tabServerLog.Text = "Simon's Console"
        Me.tabServerLog.UseVisualStyleBackColor = True
        '
        'simonsProgressbar
        '
        Me.simonsProgressbar.ForeColor = System.Drawing.Color.Violet
        Me.simonsProgressbar.Location = New System.Drawing.Point(278, 15)
        Me.simonsProgressbar.Name = "simonsProgressbar"
        Me.simonsProgressbar.Size = New System.Drawing.Size(297, 15)
        Me.simonsProgressbar.TabIndex = 33
        Me.simonsProgressbar.Value = 50
        '
        'lstSimonsConsole
        '
        Me.lstSimonsConsole.BackColor = System.Drawing.Color.White
        Me.lstSimonsConsole.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.lstSimonsConsole.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lstSimonsConsole.ForeColor = System.Drawing.Color.MidnightBlue
        Me.lstSimonsConsole.Location = New System.Drawing.Point(96, 10)
        Me.lstSimonsConsole.Name = "lstSimonsConsole"
        Me.lstSimonsConsole.Size = New System.Drawing.Size(480, 20)
        Me.lstSimonsConsole.TabIndex = 31
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(7, 15)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(45, 12)
        Me.Label9.TabIndex = 30
        Me.Label9.Text = "Simon:"
        '
        'cmdSpeechEnableToggle
        '
        Me.cmdSpeechEnableToggle.BackColor = System.Drawing.Color.Orange
        Me.cmdSpeechEnableToggle.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmdSpeechEnableToggle.Font = New System.Drawing.Font("BankGothic Md BT", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdSpeechEnableToggle.Location = New System.Drawing.Point(592, 9)
        Me.cmdSpeechEnableToggle.Name = "cmdSpeechEnableToggle"
        Me.cmdSpeechEnableToggle.Size = New System.Drawing.Size(81, 23)
        Me.cmdSpeechEnableToggle.TabIndex = 29
        Me.cmdSpeechEnableToggle.Text = "Speech On"
        Me.cmdSpeechEnableToggle.UseVisualStyleBackColor = False
        '
        'txtServerActivity
        '
        Me.txtServerActivity.BackColor = System.Drawing.Color.SlateGray
        Me.txtServerActivity.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtServerActivity.ContextMenuStrip = Me.ctxServerLog
        Me.txtServerActivity.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtServerActivity.ForeColor = System.Drawing.Color.White
        Me.txtServerActivity.Location = New System.Drawing.Point(4, 51)
        Me.txtServerActivity.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.txtServerActivity.Name = "txtServerActivity"
        Me.txtServerActivity.ReadOnly = True
        Me.txtServerActivity.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None
        Me.txtServerActivity.Size = New System.Drawing.Size(744, 336)
        Me.txtServerActivity.TabIndex = 4
        Me.txtServerActivity.Text = "server"
        '
        'ShapeContainer3
        '
        Me.ShapeContainer3.Location = New System.Drawing.Point(3, 3)
        Me.ShapeContainer3.Margin = New System.Windows.Forms.Padding(0)
        Me.ShapeContainer3.Name = "ShapeContainer3"
        Me.ShapeContainer3.Shapes.AddRange(New Microsoft.VisualBasic.PowerPacks.Shape() {Me.SimonsLine})
        Me.ShapeContainer3.Size = New System.Drawing.Size(746, 404)
        Me.ShapeContainer3.TabIndex = 32
        Me.ShapeContainer3.TabStop = False
        '
        'SimonsLine
        '
        Me.SimonsLine.BorderColor = System.Drawing.SystemColors.ActiveCaption
        Me.SimonsLine.BorderWidth = 3
        Me.SimonsLine.Name = "SimonsLine"
        Me.SimonsLine.X1 = 92
        Me.SimonsLine.X2 = 571
        Me.SimonsLine.Y1 = 36
        Me.SimonsLine.Y2 = 37
        '
        'tabRootBrowser
        '
        Me.tabRootBrowser.Controls.Add(Me.Label1)
        Me.tabRootBrowser.Location = New System.Drawing.Point(4, 4)
        Me.tabRootBrowser.Name = "tabRootBrowser"
        Me.tabRootBrowser.Padding = New System.Windows.Forms.Padding(3)
        Me.tabRootBrowser.Size = New System.Drawing.Size(752, 410)
        Me.tabRootBrowser.TabIndex = 1
        Me.tabRootBrowser.Text = "Simon's Appstore"
        Me.tabRootBrowser.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("BankGothic Md BT", 48.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.Location = New System.Drawing.Point(151, 136)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(376, 67)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Loading..."
        '
        'tabTrafficOverview
        '
        Me.tabTrafficOverview.Controls.Add(Me.cmdDumpInvalid)
        Me.tabTrafficOverview.Controls.Add(Me.lblContainerHeader)
        Me.tabTrafficOverview.Controls.Add(Me.lvTrafficEntryContent)
        Me.tabTrafficOverview.Controls.Add(Me.chkTrafficScroll)
        Me.tabTrafficOverview.Controls.Add(Me.cmdClearTrafficListview)
        Me.tabTrafficOverview.Controls.Add(Me.lblTo)
        Me.tabTrafficOverview.Controls.Add(Me.lblFrom)
        Me.tabTrafficOverview.Controls.Add(Me.txtTrafficObjectContents)
        Me.tabTrafficOverview.Controls.Add(Me.lblToCaption)
        Me.tabTrafficOverview.Controls.Add(Me.lblFromCaption)
        Me.tabTrafficOverview.Controls.Add(Me.Label4)
        Me.tabTrafficOverview.Controls.Add(Me.lvObjectTraffic)
        Me.tabTrafficOverview.Controls.Add(Me.ShapeContainer2)
        Me.tabTrafficOverview.Location = New System.Drawing.Point(4, 4)
        Me.tabTrafficOverview.Name = "tabTrafficOverview"
        Me.tabTrafficOverview.Size = New System.Drawing.Size(752, 410)
        Me.tabTrafficOverview.TabIndex = 2
        Me.tabTrafficOverview.Text = "Master Stream"
        Me.tabTrafficOverview.UseVisualStyleBackColor = True
        '
        'cmdDumpInvalid
        '
        Me.cmdDumpInvalid.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdDumpInvalid.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdDumpInvalid.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdDumpInvalid.Location = New System.Drawing.Point(577, 209)
        Me.cmdDumpInvalid.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.cmdDumpInvalid.Name = "cmdDumpInvalid"
        Me.cmdDumpInvalid.Size = New System.Drawing.Size(87, 20)
        Me.cmdDumpInvalid.TabIndex = 16
        Me.cmdDumpInvalid.Text = "dump invalid"
        Me.cmdDumpInvalid.UseVisualStyleBackColor = True
        '
        'lblContainerHeader
        '
        Me.lblContainerHeader.AutoSize = True
        Me.lblContainerHeader.Location = New System.Drawing.Point(11, 251)
        Me.lblContainerHeader.Name = "lblContainerHeader"
        Me.lblContainerHeader.Size = New System.Drawing.Size(72, 12)
        Me.lblContainerHeader.TabIndex = 15
        Me.lblContainerHeader.Text = "Container:"
        Me.lblContainerHeader.Visible = False
        '
        'lvTrafficEntryContent
        '
        Me.lvTrafficEntryContent.Activation = System.Windows.Forms.ItemActivation.OneClick
        Me.lvTrafficEntryContent.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lvTrafficEntryContent.ContextMenuStrip = Me.ctxmnuAttachment
        Me.lvTrafficEntryContent.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lvTrafficEntryContent.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lvTrafficEntryContent.HideSelection = False
        Me.lvTrafficEntryContent.LargeImageList = Me.TraficLV
        Me.lvTrafficEntryContent.Location = New System.Drawing.Point(8, 269)
        Me.lvTrafficEntryContent.MultiSelect = False
        Me.lvTrafficEntryContent.Name = "lvTrafficEntryContent"
        Me.lvTrafficEntryContent.Scrollable = False
        Me.lvTrafficEntryContent.Size = New System.Drawing.Size(117, 138)
        Me.lvTrafficEntryContent.TabIndex = 14
        Me.lvTrafficEntryContent.UseCompatibleStateImageBehavior = False
        Me.lvTrafficEntryContent.Visible = False
        '
        'chkTrafficScroll
        '
        Me.chkTrafficScroll.AutoSize = True
        Me.chkTrafficScroll.Checked = True
        Me.chkTrafficScroll.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkTrafficScroll.Location = New System.Drawing.Point(671, 14)
        Me.chkTrafficScroll.Name = "chkTrafficScroll"
        Me.chkTrafficScroll.Size = New System.Drawing.Size(50, 17)
        Me.chkTrafficScroll.TabIndex = 13
        Me.chkTrafficScroll.Text = "scroll"
        Me.chkTrafficScroll.UseVisualStyleBackColor = True
        '
        'cmdClearTrafficListview
        '
        Me.cmdClearTrafficListview.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdClearTrafficListview.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdClearTrafficListview.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdClearTrafficListview.Location = New System.Drawing.Point(671, 208)
        Me.cmdClearTrafficListview.Name = "cmdClearTrafficListview"
        Me.cmdClearTrafficListview.Size = New System.Drawing.Size(72, 20)
        Me.cmdClearTrafficListview.TabIndex = 12
        Me.cmdClearTrafficListview.Text = "CLEAR"
        Me.cmdClearTrafficListview.UseVisualStyleBackColor = True
        '
        'lblTo
        '
        Me.lblTo.AutoSize = True
        Me.lblTo.BackColor = System.Drawing.Color.White
        Me.lblTo.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblTo.ForeColor = System.Drawing.Color.SlateBlue
        Me.lblTo.Location = New System.Drawing.Point(58, 233)
        Me.lblTo.Name = "lblTo"
        Me.lblTo.Size = New System.Drawing.Size(30, 12)
        Me.lblTo.TabIndex = 11
        Me.lblTo.Text = "     "
        '
        'lblFrom
        '
        Me.lblFrom.AutoSize = True
        Me.lblFrom.BackColor = System.Drawing.Color.White
        Me.lblFrom.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblFrom.ForeColor = System.Drawing.Color.SlateBlue
        Me.lblFrom.Location = New System.Drawing.Point(58, 214)
        Me.lblFrom.Name = "lblFrom"
        Me.lblFrom.Size = New System.Drawing.Size(25, 12)
        Me.lblFrom.TabIndex = 10
        Me.lblFrom.Text = "    "
        '
        'txtTrafficObjectContents
        '
        Me.txtTrafficObjectContents.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtTrafficObjectContents.Font = New System.Drawing.Font("BankGothic Md BT", 9.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtTrafficObjectContents.ForeColor = System.Drawing.Color.Crimson
        Me.txtTrafficObjectContents.Location = New System.Drawing.Point(131, 269)
        Me.txtTrafficObjectContents.Name = "txtTrafficObjectContents"
        Me.txtTrafficObjectContents.ReadOnly = True
        Me.txtTrafficObjectContents.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        Me.txtTrafficObjectContents.Size = New System.Drawing.Size(365, 138)
        Me.txtTrafficObjectContents.TabIndex = 9
        Me.txtTrafficObjectContents.Text = ""
        '
        'lblToCaption
        '
        Me.lblToCaption.AutoSize = True
        Me.lblToCaption.Location = New System.Drawing.Point(11, 233)
        Me.lblToCaption.Name = "lblToCaption"
        Me.lblToCaption.Size = New System.Drawing.Size(24, 12)
        Me.lblToCaption.TabIndex = 8
        Me.lblToCaption.Text = "To:"
        Me.lblToCaption.Visible = False
        '
        'lblFromCaption
        '
        Me.lblFromCaption.AutoSize = True
        Me.lblFromCaption.Location = New System.Drawing.Point(10, 214)
        Me.lblFromCaption.Name = "lblFromCaption"
        Me.lblFromCaption.Size = New System.Drawing.Size(39, 12)
        Me.lblFromCaption.TabIndex = 7
        Me.lblFromCaption.Text = "From:"
        Me.lblFromCaption.Visible = False
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(9, 14)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(161, 12)
        Me.Label4.TabIndex = 1
        Me.Label4.Text = "Real-time Server Traffic:"
        '
        'lvObjectTraffic
        '
        Me.lvObjectTraffic.Activation = System.Windows.Forms.ItemActivation.OneClick
        Me.lvObjectTraffic.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.lvObjectTraffic.FullRowSelect = True
        Me.lvObjectTraffic.GridLines = True
        Me.lvObjectTraffic.HideSelection = False
        Me.lvObjectTraffic.HotTracking = True
        Me.lvObjectTraffic.HoverSelection = True
        Me.lvObjectTraffic.Location = New System.Drawing.Point(3, 42)
        Me.lvObjectTraffic.MultiSelect = False
        Me.lvObjectTraffic.Name = "lvObjectTraffic"
        Me.lvObjectTraffic.ShowGroups = False
        Me.lvObjectTraffic.Size = New System.Drawing.Size(740, 163)
        Me.lvObjectTraffic.TabIndex = 0
        Me.lvObjectTraffic.UseCompatibleStateImageBehavior = False
        Me.lvObjectTraffic.View = System.Windows.Forms.View.Details
        '
        'ShapeContainer2
        '
        Me.ShapeContainer2.Location = New System.Drawing.Point(0, 0)
        Me.ShapeContainer2.Margin = New System.Windows.Forms.Padding(0)
        Me.ShapeContainer2.Name = "ShapeContainer2"
        Me.ShapeContainer2.Shapes.AddRange(New Microsoft.VisualBasic.PowerPacks.Shape() {Me.LineShape1})
        Me.ShapeContainer2.Size = New System.Drawing.Size(752, 410)
        Me.ShapeContainer2.TabIndex = 6
        Me.ShapeContainer2.TabStop = False
        '
        'LineShape1
        '
        Me.LineShape1.BorderColor = System.Drawing.Color.RosyBrown
        Me.LineShape1.Name = "LineShape1"
        Me.LineShape1.X1 = 9
        Me.LineShape1.X2 = 433
        Me.LineShape1.Y1 = 32
        Me.LineShape1.Y2 = 33
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.cmdUpdatePaykey)
        Me.TabPage1.Controls.Add(Me.lblLinkForum)
        Me.TabPage1.Controls.Add(Me.Label12)
        Me.TabPage1.Controls.Add(Me.Label10)
        Me.TabPage1.Controls.Add(Me.lblPaymentRegister)
        Me.TabPage1.Controls.Add(Me.lblNoIdInfo)
        Me.TabPage1.Controls.Add(Me.Label5)
        Me.TabPage1.Controls.Add(Me.cmdNetwork)
        Me.TabPage1.Controls.Add(Me.txtUserPaymentHandle)
        Me.TabPage1.Controls.Add(Me.Label8)
        Me.TabPage1.Controls.Add(Me.Label7)
        Me.TabPage1.Location = New System.Drawing.Point(4, 4)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(752, 410)
        Me.TabPage1.TabIndex = 3
        Me.TabPage1.Text = "Configuration"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'cmdUpdatePaykey
        '
        Me.cmdUpdatePaykey.Enabled = False
        Me.cmdUpdatePaykey.FlatStyle = System.Windows.Forms.FlatStyle.System
        Me.cmdUpdatePaykey.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.cmdUpdatePaykey.ForeColor = System.Drawing.SystemColors.ButtonHighlight
        Me.cmdUpdatePaykey.Location = New System.Drawing.Point(507, 44)
        Me.cmdUpdatePaykey.Name = "cmdUpdatePaykey"
        Me.cmdUpdatePaykey.Size = New System.Drawing.Size(70, 20)
        Me.cmdUpdatePaykey.TabIndex = 12
        Me.cmdUpdatePaykey.Text = "Update"
        Me.cmdUpdatePaykey.UseVisualStyleBackColor = True
        '
        'lblLinkForum
        '
        Me.lblLinkForum.AutoSize = True
        Me.lblLinkForum.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblLinkForum.ForeColor = System.Drawing.Color.MidnightBlue
        Me.lblLinkForum.Location = New System.Drawing.Point(98, 192)
        Me.lblLinkForum.Name = "lblLinkForum"
        Me.lblLinkForum.Size = New System.Drawing.Size(22, 12)
        Me.lblLinkForum.TabIndex = 10
        Me.lblLinkForum.Text = "Go"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(11, 192)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(81, 12)
        Me.Label12.TabIndex = 9
        Me.Label12.Text = "User Forum:"
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label10.Location = New System.Drawing.Point(11, 166)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(64, 12)
        Me.Label10.TabIndex = 8
        Me.Label10.Text = "Support"
        '
        'lblPaymentRegister
        '
        Me.lblPaymentRegister.AutoSize = True
        Me.lblPaymentRegister.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPaymentRegister.ForeColor = System.Drawing.Color.MidnightBlue
        Me.lblPaymentRegister.Location = New System.Drawing.Point(583, 86)
        Me.lblPaymentRegister.Name = "lblPaymentRegister"
        Me.lblPaymentRegister.Size = New System.Drawing.Size(87, 12)
        Me.lblPaymentRegister.TabIndex = 7
        Me.lblPaymentRegister.Text = "Get one here"
        '
        'lblNoIdInfo
        '
        Me.lblNoIdInfo.AutoSize = True
        Me.lblNoIdInfo.Location = New System.Drawing.Point(506, 85)
        Me.lblNoIdInfo.Name = "lblNoIdInfo"
        Me.lblNoIdInfo.Size = New System.Drawing.Size(71, 12)
        Me.lblNoIdInfo.TabIndex = 6
        Me.lblNoIdInfo.Text = "No ID yet?"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label5.Location = New System.Drawing.Point(11, 86)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(66, 12)
        Me.Label5.TabIndex = 4
        Me.Label5.Text = "Network"
        '
        'cmdNetwork
        '
        Me.cmdNetwork.Location = New System.Drawing.Point(6, 109)
        Me.cmdNetwork.Name = "cmdNetwork"
        Me.cmdNetwork.Size = New System.Drawing.Size(147, 25)
        Me.cmdNetwork.TabIndex = 3
        Me.cmdNetwork.Text = "Configure.."
        Me.cmdNetwork.UseVisualStyleBackColor = True
        '
        'txtUserPaymentHandle
        '
        Me.txtUserPaymentHandle.BackColor = System.Drawing.Color.LavenderBlush
        Me.txtUserPaymentHandle.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.txtUserPaymentHandle.Font = New System.Drawing.Font("BankGothic Md BT", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.txtUserPaymentHandle.Location = New System.Drawing.Point(100, 44)
        Me.txtUserPaymentHandle.Name = "txtUserPaymentHandle"
        Me.txtUserPaymentHandle.Size = New System.Drawing.Size(401, 17)
        Me.txtUserPaymentHandle.TabIndex = 2
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(9, 46)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(82, 12)
        Me.Label8.TabIndex = 1
        Me.Label8.Text = "Your Pay ID:"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label7.Location = New System.Drawing.Point(11, 16)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(70, 12)
        Me.Label7.TabIndex = 0
        Me.Label7.Text = "Appstore"
        '
        'frmServerMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.BackColor = System.Drawing.Color.SlateGray
        Me.ClientSize = New System.Drawing.Size(770, 525)
        Me.Controls.Add(Me.lblSizeGrip)
        Me.Controls.Add(Me.lblMaximize)
        Me.Controls.Add(Me.lblCloseWindow)
        Me.Controls.Add(Me.lblMinimizeWindow)
        Me.Controls.Add(Me.lblWindowTitle)
        Me.Controls.Add(Me.tabCsl)
        Me.Controls.Add(Me.grpControl)
        Me.Controls.Add(Me.ShapeContainer1)
        Me.Font = New System.Drawing.Font("BankGothic Md BT", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ForeColor = System.Drawing.Color.White
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        Me.Name = "frmServerMain"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "TriniDAT Data Server  | Server Console"
        Me.grpControl.ResumeLayout(False)
        Me.grpControl.PerformLayout()
        Me.ctxmnuAttachment.ResumeLayout(False)
        Me.ctxServerLog.ResumeLayout(False)
        Me.tabCsl.ResumeLayout(False)
        Me.tabServerLog.ResumeLayout(False)
        Me.tabServerLog.PerformLayout()
        Me.tabRootBrowser.ResumeLayout(False)
        Me.tabRootBrowser.PerformLayout()
        Me.tabTrafficOverview.ResumeLayout(False)
        Me.tabTrafficOverview.PerformLayout()
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents grpControl As System.Windows.Forms.GroupBox
    Friend WithEvents cmdServerStart As System.Windows.Forms.Button
    Friend WithEvents cmdServerStop As System.Windows.Forms.Button
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents cmdOpenServerPage As System.Windows.Forms.Button
    Friend WithEvents cmdCLS As System.Windows.Forms.Button
    Friend WithEvents tabServerLog As System.Windows.Forms.TabPage
    Friend WithEvents txtServerActivity As System.Windows.Forms.RichTextBox
    Friend WithEvents tabRootBrowser As System.Windows.Forms.TabPage
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents cmdAppInstallation As System.Windows.Forms.Button
    Friend WithEvents WindowBar As Microsoft.VisualBasic.PowerPacks.RectangleShape
    Friend WithEvents ShapeContainer1 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    Friend WithEvents lblMinimizeWindow As System.Windows.Forms.Label
    Friend WithEvents lblWindowTitle As System.Windows.Forms.Label
    Friend WithEvents lblCloseWindow As System.Windows.Forms.Label
    Friend WithEvents lblMaximize As System.Windows.Forms.Label
    Friend WithEvents tabTrafficOverview As System.Windows.Forms.TabPage
    Friend WithEvents optSMDev As System.Windows.Forms.RadioButton
    Friend WithEvents optSMProd As System.Windows.Forms.RadioButton
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents ShapeContainer2 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    Friend WithEvents LineShape1 As Microsoft.VisualBasic.PowerPacks.LineShape
    Friend WithEvents txtTrafficObjectContents As System.Windows.Forms.RichTextBox
    Friend WithEvents lblToCaption As System.Windows.Forms.Label
    Friend WithEvents lblFromCaption As System.Windows.Forms.Label
    Friend WithEvents lblTo As System.Windows.Forms.Label
    Friend WithEvents lblFrom As System.Windows.Forms.Label
    Friend WithEvents lvObjectTraffic As System.Windows.Forms.ListView
    Friend WithEvents cmdClearTrafficListview As System.Windows.Forms.Button
    Friend WithEvents chkTrafficScroll As System.Windows.Forms.CheckBox
    Friend WithEvents lvTrafficEntryContent As System.Windows.Forms.ListView
    Friend WithEvents TraficLV As System.Windows.Forms.ImageList
    Friend WithEvents lblContainerHeader As System.Windows.Forms.Label
    Friend WithEvents lblSizeGrip As System.Windows.Forms.Label
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtUserPaymentHandle As System.Windows.Forms.TextBox
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents ctxmnuAttachment As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuAttachmentSpeakText As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents cmdSpeechEnableToggle As System.Windows.Forms.Button
    Friend WithEvents lstSimonsConsole As System.Windows.Forms.ComboBox
    Friend WithEvents tabCsl As TriniDAT.BosswaveTabbed2
    Friend WithEvents ShapeContainer3 As Microsoft.VisualBasic.PowerPacks.ShapeContainer
    Friend WithEvents SimonsLine As Microsoft.VisualBasic.PowerPacks.LineShape
    Friend WithEvents ctxServerLog As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuServerLog_GotoURL As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuServerLog_Copy As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuServerLog_Clear As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents cmdDumpInvalid As System.Windows.Forms.Button
    Friend WithEvents lblPaymentRegister As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents cmdNetwork As System.Windows.Forms.Button
    Friend WithEvents lblLinkForum As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents browseApp As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblNoIdInfo As System.Windows.Forms.Label
    Friend WithEvents cmdUpdatePaykey As System.Windows.Forms.Button
    Friend WithEvents simonsProgressbar As System.Windows.Forms.ProgressBar

End Class
