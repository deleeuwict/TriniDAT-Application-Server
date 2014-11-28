Option Explicit On
Option Compare Text
Imports System.Media.SoundPlayer
Imports System.Threading
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Web
Imports Newtonsoft.Json
Imports System.IO
Imports TriniDATServerTypes
Imports SimonTypes
Imports System.Xml


Public Class frmServerMain
    Const SIMON_COMMAND_CLEAR = "CLR"
 
    Delegate Sub setWBFocusStatus(ByVal focused As Boolean)
    Delegate Sub serverLogNewLineDelegate()
    Delegate Sub serverLogDelegate(ByVal txt As String)
    Delegate Sub serverLogColoredDelegate(ByVal txt As String, ByVal clr As Color)
    Delegate Sub ChangeWindowTitleDelegate(ByVal val As String)
    Delegate Sub ChangeWindowColorDelegate(ByVal val As Color)
    Delegate Sub ChangeLogBackgroundColorDelegate(ByVal val As Color)
    Delegate Sub onHTTPDebugPacketReceivedDeletegate(ByVal packetid As String, ByVal packetIn As Boolean, ByVal packettype As String, ByVal fullPacket As String)
    Delegate Sub restoreOrgWindowtitleDelegate()
    Delegate Sub OnFirstRunDelegate()

    Public Delegate Sub OnAddTrafficItem(ByVal obj As XElement)
    Public Delegate Sub OnAddTrafficLVItem(ByVal item As ListViewItem)
    Public Delegate Sub OnServerStartedThrd(ByVal current_config As BosswaveTCPServerConfig)
    Public Delegate Sub OnServerStoppedThrd(ByVal current_config As BosswaveTCPServerConfig)
    Public Delegate Sub OnSpeechModeChangedDelegate(ByVal enabled As Boolean)
    Public Delegate Sub OnAppStoreFormLoadedDelegate()
    Public Delegate Sub OnEnableFormTabWinProcDelegate()
    Public Delegate Sub OnDisableFormTabWinProcDelegate()
    Public Delegate Sub SimonsProgressBarSetterDelegate(ByVal x As Integer, ByVal max_val As Integer)
    Public Delegate Sub SimonsProgressVisiblitySetterDelegate(ByVal x As Boolean)

    Public OnFirstRunThreaded As New OnFirstRunDelegate(AddressOf OnFirstRun)
    Public OnAppStoreFormLoadedThread As New OnAppStoreFormLoadedDelegate(AddressOf onAppStoreBrowserWindowLoadCompleted)
    Public serverLogNewLinethrd As New serverLogNewLineDelegate(AddressOf serverLogNewLine)
    Public serverLogThrd As New serverLogDelegate(AddressOf serverLog)
    Public serverColoredNonSpacedLogThrd As New serverLogColoredDelegate(AddressOf serverColoredNonSpaced)
    Public serverColoredLogThrd As New serverLogColoredDelegate(AddressOf serverColoredLog)
    Public setWBFocusStatusThrd As New setWBFocusStatus(AddressOf OnTabWebBrowserFocusChange)
    Public serverLogLogObject As New OnAddTrafficItem(AddressOf OnLogObjectWritten)
    Public serverLogLVItemLog As New OnAddTrafficLVItem(AddressOf AddTraffic)

    Public changeLogBackgroundColorThreaded As New ChangeLogBackgroundColorDelegate(AddressOf ChangeLogBackColor)
    Public changeWinTitleThreaded As New ChangeWindowTitleDelegate(AddressOf ChangeWindowTitle)
    Public changeWinColorThreaded As New ChangeWindowColorDelegate(AddressOf ChangeFormColor)
    Public changeRestoreTitleThreaded As New restoreOrgWindowtitleDelegate(AddressOf Me.restoreOrgWindowtitle)

    Public doEnableFormTabWinProcThreaded As New OnEnableFormTabWinProcDelegate(AddressOf Me.enableFormAndTab)
    Public doDisableFormTabWinProcThreaded As New OnDisableFormTabWinProcDelegate(AddressOf Me.DisableFormAndTab)

    Public onSpeechModeChangedThreaded As New OnSpeechModeChangedDelegate(AddressOf OnSpeechModeChanged)
    Public setSimonProgressBarThreaded As New SimonsProgressBarSetterDelegate(AddressOf setSimonsProgress)
    Public setSimonProgressBarVisibleThreaded As New SimonsProgressVisiblitySetterDelegate(AddressOf setSimonsProgressVisible)

    Public Const maxLogBufferSize = 1024 * 2
    Public Const maxListviewItems = 75

    'JAVASCRIPT LIBRARY VARS
    Public Const js_setnetout_url_identifier As String = "onNETServerOut"
    Public Const js_setbotout_url_identifier As String = "onBotOut"

    'MAPPING
    Private mapping_points As Collection
    Public threaded_webbrowser_thread As Thread
    Public threaded_webbrowser_form As frmThreadedConsoleWB
    Public threaded_query_editor As Thread
    Public threaded_query_editor_form As frmQueryEditor

    Public wb_focused As Boolean
    Private sizeStates As New sizeStates

    Private drag_resize As Boolean
    Private dragging_tags As Boolean
    Private drag_startpos As Point
    Private drag_resizepos As Point

    Public OnServerStartedThreadedCaller As New OnServerStartedThrd(AddressOf OnServerStartGuiThreaded)
    Public OnServerStoppedThreadedCaller As New OnServerStoppedThrd(AddressOf OnServerStopGuiThreaded)

    Private Const TAB_INDEX_LOG As Integer = 0
    Private Const TAB_INDEX_APPS As Integer = 1
    Private Const TAB_INDEX_OBJECTTRAFFIC As Integer = 2
    Private Const MAX_TRAFFIC_LVITEMS As Integer = 600

    Private frmIEScript As IEScripter

    Public Sub OnSpeechModeChangedDirect(ByVal enabled As Boolean)
        Call OnSpeechModeChanged(enabled)
    End Sub
    Public Sub setSimonsProgressVisible(ByVal x As Boolean)
        'called by Simon.RELOAD
        Me.simonsProgressbar.Visible = x
    End Sub
    Private Sub setSimonsProgress(ByVal x As Integer, ByVal max_val As Integer)
        Me.simonsProgressbar.Maximum = max_val
        Me.simonsProgressbar.Value = x

    End Sub
    Private Sub onAppStoreBrowserWindowLoadCompleted()
        'called using delegate.
        Me.Enabled = True
        Me.tabCsl.Enabled = True
    End Sub
    Private Sub OnSpeechModeChanged(ByVal enabled As Boolean)

        If Not enabled Then
            cmdSpeechEnableToggle.BackColor = Color.Black
            cmdSpeechEnableToggle.Text = "Speech: Off"
            cmdSpeechEnableToggle.FlatStyle = FlatStyle.Popup
            GlobalObject.MsgColored("Speech off.", Color.Black)
        Else
            cmdSpeechEnableToggle.BackColor = Color.Orange
            cmdSpeechEnableToggle.Text = "Speech: On"
            cmdSpeechEnableToggle.FlatStyle = FlatStyle.Flat
            GlobalObject.MsgColored("Speech on.", Color.Green)

            Try
                'defend with cowbell technology
                Dim Sound As New System.Media.SoundPlayer()

                Sound.SoundLocation = GlobalSetting.getSpeechPath() & "talk.wav"
                Sound.Load()
                Sound.Play()
            Catch ex As Exception

            End Try


        End If

        Me.Refresh()
        Me.Focus()

    End Sub
    Private Sub cmdCampaignCreate_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim frm As New frmCodeEditor
        frm.ShowDialog()
        frm.Dispose()
        Me.Dispose() 'debug mode

    End Sub

    Private Sub OnTabWebBrowserFocusChange(ByVal still_focused As Boolean)
        Me.wb_focused = still_focused

    End Sub
    Private Sub DisableFormAndTab()
        'stop important window procs.
        Me.Enabled = False
        Me.tabCsl.Enabled = False
    End Sub

    Private Sub enableFormAndTab()
        'restart important window procs.
        Me.Enabled = True
        Me.tabCsl.Enabled = True
    End Sub

    Public Function WakeUpWBThread() As Boolean
        'KillFormthreaded

        Try
            If Me.haveWebbrowser() Then
                If Me.threaded_webbrowser_thread.ThreadState = ThreadState.Suspended Or Me.threaded_webbrowser_thread.ThreadState = ThreadState.SuspendRequested Then
                    Me.threaded_webbrowser_thread.Resume()
                    If GlobalSetting.PayKey <> Me.threaded_webbrowser_form.Tag Then
                        'paykey has changed. reload browser.
                        Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.reloadAppStoreThreaded)
                    End If
                End If
            End If
            Return True
        Catch ex As Exception
            serverLog("WakeUpWBThread Error: " & ex.Message)
        End Try
        Return False

    End Function
    Public ReadOnly Property haveWebbrowser() As Boolean
        Get
            Dim retval As Boolean
            retval = Not IsNothing(Me.threaded_webbrowser_thread)
            If retval Then
                retval = (Me.threaded_webbrowser_thread.ThreadState <> ThreadState.Stopped)
            End If
            Return retval
        End Get
    End Property
    Public ReadOnly Property haveWebbrowserForm() As Boolean
        Get
            Return Not (IsNothing(Me.threaded_webbrowser_form))
        End Get
    End Property
    Public ReadOnly Property haveQEditor() As Boolean
        Get
            Dim retval As Boolean
            retval = Not IsNothing(Me.threaded_query_editor)
            If retval Then
                retval = (Me.threaded_query_editor.ThreadState <> ThreadState.Stopped)
            End If
            Return retval
        End Get
    End Property
    Public ReadOnly Property haveQEditorForm() As Boolean
        Get
            Return Not (IsNothing(Me.threaded_query_editor_form))
        End Get
    End Property

    Public Sub onServerModeChange()

        If Not GlobalObject.haveServerthread Then
            Exit Sub
        End If

        If optSMDev.Checked Then
            GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV
            GlobalObject.Msg("Server now in 'dev' mode.")
            optSMDev.Font = New Font(optSMDev.Font, FontStyle.Bold)
            optSMProd.Font = New Font(optSMProd.Font, FontStyle.Regular)
            If GlobalObject.haveSimon Then
                GlobalObject.simon.setConsoleContext(SimonConsoleContext.SERVER_DEV, Nothing)
            End If

            Me.cmdAppInstallation.Enabled = True
            Me.cmdNetwork.Enabled = True

        ElseIf optSMProd.Checked Then
            GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_LIVE
            GlobalObject.Msg("Server now in 'live' mode.")
            optSMProd.Font = New Font(optSMProd.Font, FontStyle.Bold)
            optSMDev.Font = New Font(optSMDev.Font, FontStyle.Regular)
            If GlobalObject.haveSimon Then
                GlobalObject.simon.setConsoleContext(SimonConsoleContext.SERVER_LIVE, Nothing)
            End If
            Me.cmdNetwork.Enabled = False
            Me.cmdAppInstallation.Enabled = False
        End If

    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        'SET UP SIMON
        '==========

        If Not GlobalSetting.haveSimonCommandFile() Then
            Me.serverColoredLog("Simon commands unavailable. Missing commands.xml", Color.Red)
            GlobalSpeech.Enabled = False
        Else
            'set up speech
            GlobalSpeech.Enabled = True

            If GlobalObject.userIsGJ Then
                GlobalSpeech.Enabled = False
            End If

            GlobalObject.simon = New SimonGlobalCommandHandler(Me)
            GlobalSpeech.SpeakWelcome()

        End If

        'GUI INIT
        '=====
        'Me.SimonsLine.X1 = Me.lstSimonsConsole.Left
        'Me.SimonsLine.X2 = Me.lstSimonsConsole.Left
        'Me.SimonsLine.BorderWidth = Me.lstSimonsConsole.Width

        Me.tabCsl.SelectedIndex = TAB_INDEX_LOG
        Me.txtTrafficObjectContents.Visible = False
        Me.lblFrom.Visible = False
        Me.lblTo.Visible = False
        Me.lvTrafficEntryContent.Visible = False
        Me.txtServerActivity.Text = ""
        Me.simonsProgressbar.Location = New Point(Me.lstSimonsConsole.Location.X, Me.lstSimonsConsole.Location.Y)
        Me.simonsProgressbar.Height = Me.lstSimonsConsole.Height
        Me.simonsProgressbar.Width = Me.lstSimonsConsole.Width
        Me.simonsProgressbar.Visible = True

        Call Me.createObjectTrafficListview(ObjectTrafficListview_Mode.MODE_ALL)

        Call frmServerMain_Resize(Nothing, Nothing)

        Call TriniDATServerEvent.OnSystemStartup()

        'set developer mode
        Me.optSMDev.Checked = True
        Call optSMDev_Click(Nothing, Nothing)

        Call startServerThread()

        'SUB-PROJECT  - IE Scripter
        Me.frmIEScript = New IEScripter
        Me.frmIEScript.Show()

        'todo: create tcp listener for remote simon commands.

    End Sub



    Public Sub cmdServerStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdServerStart.Click

        'Clean up and start.
        txtServerActivity.Text = ""
        Me.lvObjectTraffic.Clear()
        Me.lvTrafficEntryContent.Clear()
        Me.lvTrafficEntryContent.Visible = False
        Call startServerThread()


    End Sub

    Public Sub cmdServerStop_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdServerStop.Click
        GlobalObject.server.stopServer()

        If GlobalObject.haveServerthread() Then
            If GlobalObject.server_thread.ThreadState = ThreadState.Running Or GlobalObject.server_thread.ThreadState = ThreadState.Background Then
                Try
                    GlobalObject.server_thread.Abort()
                Catch ex As Exception

                End Try
            End If
            serverLog("Clear Server thread.")
            GlobalObject.server_thread = Nothing
        End If

        serverLog("Server has stopped.")
        cmdServerStart.Enabled = True
        cmdServerStop.Enabled = False

    End Sub
    Public Sub serverLog(ByVal txt As String)
        If txtServerActivity.TextLength = 3 Then
            If Mid(txtServerActivity.Text, 1, 1) = "." Then
                txtServerActivity.Text = ""
            End If
        End If
        If txtServerActivity.Lines.Length > 5000 Then
            Me.txtServerActivity.Text = "auto-clear."

        End If
        txtServerActivity.AppendText(txt & vbNewLine & vbNewLine)
        txtServerActivity.ScrollToCaret()

        Debug.Print("serverLog delegate: " + txt)

    End Sub
    Public Sub serverLogNewLine()
        txtServerActivity.AppendText(vbNewLine)
        txtServerActivity.ScrollToCaret()
    End Sub
    Public Sub serverColoredNonSpaced(ByVal txt As String, ByVal new_clr As Color)
        'log without double line markup.
        txtServerActivity.SelectionColor = new_clr
        txtServerActivity.AppendText(txt & vbNewLine)
        txtServerActivity.ScrollToCaret()

    End Sub
    Public Sub serverColoredLog(ByVal txt As String, ByVal new_clr As Color)
        txtServerActivity.SelectionColor = new_clr
        Call serverLog(txt)
    End Sub
    Sub startServerThread()

        Dim boot_mode As TRINIDAT_SERVERMODE



        If Me.optSMDev.Checked Then
            boot_mode = TRINIDAT_SERVERMODE.MODE_DEV
        ElseIf Me.optSMProd.Checked Then
            boot_mode = TRINIDAT_SERVERMODE.MODE_LIVE
        End If

        GlobalObject.startServerThread(boot_mode)

        cmdServerStart.Enabled = False
        cmdServerStop.Enabled = True

    End Sub


    Public Function Convert2dArrayToAssociativeJScriptArray(ByVal arr() As Object) As String
        Dim retval As String
        retval = JsonConvert.SerializeObject(arr)
        'convert 2d Array to associative Object
        retval = Replace(retval, Chr(34) & "," & Chr(34), Chr(34) & " : " & Chr(34))
        retval = Replace(retval, "[[", "{")
        retval = Replace(retval, "]]", "}")
        retval = Replace(retval, "],[", " , ")

        Return retval
    End Function


    Private Sub cmdOpenServerPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdOpenServerPage.Click
        Dim oPro As New Process
        Dim httpcfg As BosswaveTCPServerConfig

        httpcfg = GlobalSetting.getHTTPServerConfig()


        With oPro
            .StartInfo.UseShellExecute = True
            .StartInfo.Arguments = ""
            .StartInfo.FileName = "http://" & httpcfg.server_ip.ToString & ":" & httpcfg.server_port.ToString
            .Start()
        End With
    End Sub

    Private Sub cmdCLS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCLS.Click
        txtServerActivity.Clear()

    End Sub
    Private Sub frmServerMain_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Application.Exit()

    End Sub

    Private Sub tabCsl_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tabCsl.SelectedIndexChanged


        If Threading.Thread.CurrentThread.ThreadState <> ThreadState.Running Then
            Exit Sub
        End If


        If tabCsl.SelectedIndex = Me.getRootWebbrowserTabIndex Then

            Dim coord As Rectangle

            coord = tabCsl.SelectedTab.RectangleToScreen(tabCsl.SelectedTab.ClientRectangle)

            If Not Me.haveWebbrowser() Then
                'generate html index
                'todo: Call generateAppStorePage()

                'create Webbrowser

                'freeze until load completion.
                Me.DisableFormAndTab()
                Me.threaded_webbrowser_thread = New Thread(AddressOf createThreadedWB)
                threaded_webbrowser_thread.SetApartmentState(System.Threading.ApartmentState.STA)
                threaded_webbrowser_thread.Start(coord)

            ElseIf Me.haveWebbrowserForm() Then
                'resume thread
                Call Me.WakeUpWBThread()

                'resize it
                Call resizeThreadedBrowserTab(coord)

                Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.enableStayOnTopThreaded)
                'show it
                Call Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.ShowNowThreaded)
                Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.disableStayOnTopThreaded)
                Me.wb_focused = True
                'bring to front
                '                Call Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.sty)
            End If

        Else

            If tabCsl.SelectedIndex = 0 Then
                Me.lstSimonsConsole.Focus()
            End If

            'SUSPEND INVISBLE THREADS
            If haveWebbrowser() And Me.haveWebbrowserForm() Then

                If Me.threaded_webbrowser_thread.ThreadState = ThreadState.Running Then

                    'hide thread form.
                    Call Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.HideNowThreaded)
                    Me.wb_focused = False

                    'suspend wb GUI
                    Me.threaded_webbrowser_thread.Suspend()
                    Thread.Sleep(10)
                End If
            End If
        End If
    End Sub

    Public Sub restoreOrgWindowtitle()
        Call OnServerStartGuiThreaded(GlobalObject.CurrentServerConfiguration)
    End Sub
    Public Sub OnServerStartGuiThreaded(ByVal current_config As BosswaveTCPServerConfig)
        Me.lblWindowTitle.Text = ":: TriniDAT Data Application Server | (c) " & Year(Now).ToString & " GertJan de Leeuw | De Leeuw ICT | Server: " & current_config.server_ip.ToString & " Port: " & current_config.server_port.ToString
        Me.simonsProgressbar.Visible = False
    End Sub

    Public Sub OnServerStopGuiThreaded(ByVal current_config As BosswaveTCPServerConfig)
        Me.lblWindowTitle.Text = ":: TriniDAT Data Application Server | (c) " & Year(Now).ToString & " GertJan de Leeuw | De Leeuw ICT | Not running | " & current_config.server_ip.ToString & "."
    End Sub

    Private Sub resizeThreadedBrowserTab(ByVal tabbed_screen_coord As Rectangle)
        Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.setNewPositionByRectangle_threaded, {tabbed_screen_coord})
        Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.resizeNowThreaded)
    End Sub

    Public Sub createThreadedWB(ByVal coord As Rectangle)
        Me.threaded_webbrowser_form = New frmThreadedConsoleWB(coord, AddressOf Me.OnTabWebBrowserFocusChange)
        Me.threaded_webbrowser_form.Tag = GlobalSetting.PayKey

        Try
            System.Windows.Forms.Application.Run(Me.threaded_webbrowser_form)
            Me.wb_focused = True
        Catch ex As Exception
            GlobalObject.MsgColored("Error loading visual component(s): " & ex.Message, Color.DarkOrange)
            Me.enableFormAndTab()
            Me.tabCsl.SelectedIndex = 0
        End Try

    End Sub

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()


        Me.threaded_webbrowser_thread = Nothing
        Me.threaded_webbrowser_form = Nothing
        Me.threaded_query_editor_form = Nothing
        Me.threaded_webbrowser_thread = Nothing


        'save important initial control positions for relative resizing.
        sizeStates.MainTabCtrlTop = tabCsl.Top
        sizeStates.MainTabCtrlSubstractFormHeight = 10
        sizeStates.CloseWindowCtrlPosWidthDiff = Me.Width - Me.lblCloseWindow.Left
        sizeStates.MaximizeCtrlPosWidthDiff = Me.Width - Me.lblMaximize.Left
        sizeStates.MinimizeCtrlPosWidthDiff = Me.Width - Me.lblMinimizeWindow.Left
        sizeStates.bootHeight = Me.Height
        sizeStates.bootWidth = Me.Width
        sizeStates.buttonDiff = Me.cmdServerStop.Left - (Me.cmdServerStart.Left + Me.cmdServerStart.Width)
        sizeStates.serverLogInitialTop = Me.txtServerActivity.Top
        sizeStates.initialized = True


        'rsize 
        AddHandler lblWindowTitle.MouseDown, AddressOf WindowBar_MouseDown
        AddHandler lblWindowTitle.MouseMove, AddressOf WindowBar_MouseMove
        AddHandler lblWindowTitle.MouseUp, AddressOf WindowBar_MouseUp
        AddHandler lblWindowTitle.MouseLeave, AddressOf WindowBar_MouseLeave
        AddHandler MouseMove, AddressOf WindowBar_MouseMove

        Me.optSMProd.Enabled = (Not GlobalObject.OfficialLicense.currentLicense = TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE)


    End Sub

    Private Sub frmServerMain_LocationChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.LocationChanged

        Call tabCsl_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Public ReadOnly Property getRootWebbrowserTabIndex() As Integer
        Get
            Return 1
        End Get
    End Property

    Private Sub frmServerMain_Activated(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        Call tabCsl_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub frmServerMain_MouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseClick
        Call tabCsl_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub frmServerMain_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize

        If Me.sizeStates.initialized Then

            grpControl.Width = Me.Width - (grpControl.Location.X * 2)
            Me.tabCsl.Width = Me.Width - (tabCsl.Location.X * 2)
            Me.tabCsl.Height = Me.Height - sizeStates.MainTabCtrlTop - sizeStates.MainTabCtrlSubstractFormHeight

            Me.WindowBar.Width = Me.Width + 50

            Me.lblMinimizeWindow.Left = Me.Width - sizeStates.MinimizeCtrlPosWidthDiff
            Me.lblMaximize.Left = Me.Width - sizeStates.MaximizeCtrlPosWidthDiff
            Me.lblCloseWindow.Left = Me.Width - sizeStates.CloseWindowCtrlPosWidthDiff
            Me.lblSizeGrip.Top = Me.Height - Me.lblSizeGrip.Height
            Me.lblSizeGrip.Left = Me.Width - Me.lblSizeGrip.Width

            Me.cmdCLS.Left = Me.cmdAppInstallation.Left + Me.cmdAppInstallation.Width + sizeStates.buttonDiff

            Me.lblWindowTitle.Width = Me.lblMinimizeWindow.Left - Me.lblWindowTitle.Left - 4

         

        End If

        If Me.tabCsl.SelectedIndex = 0 Then
            Me.lstSimonsConsole.Focus()
        End If
    End Sub

    Private Sub tabServerLog_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tabServerLog.Resize
        Me.txtServerActivity.Width = tabServerLog.Width - (tabServerLog.Location.X * 2)
        Me.txtServerActivity.Height = tabServerLog.Height - Me.sizeStates.serverLogInitialTop
        Me.txtServerActivity.ScrollToCaret()

    End Sub

    Private Sub tabRootBrowser_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tabRootBrowser.Resize
        ' Me.txtServerActivity.Width = tabServerLog.Width - (tabServerLog.Location.X * 2)
        '  Me.txtServerActivity.Height = tabServerLog.Height - (tabServerLog.Location.Y * 2)
        Call tabCsl_SelectedIndexChanged(Nothing, Nothing)
    End Sub

    Private Sub lstSimonsConsole_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstSimonsConsole.KeyDown
        If Me.lstSimonsConsole.Text.Length = 0 Then
            Me.SimonsLine.BorderColor = Color.MidnightBlue
        ElseIf Me.SimonsLine.BorderColor <> Color.PaleVioletRed Then
            Me.SimonsLine.BorderColor = Color.PaleVioletRed
        End If

        Select Case e.KeyCode
            Case Keys.Up, Keys.Down
                'ignore selectedindexchange event.
                lstSimonsConsole.Tag = 1
            Case Else
                lstSimonsConsole.Tag = 0
        End Select

    End Sub

    Private Function haveSimonPastCommand(ByVal val As String) As Boolean

        For Each pastcmd As String In Me.lstSimonsConsole.Items
            If val = pastcmd Then
                Return True
            End If

        Next

        Return False
    End Function
    Private Sub lstSimonsConsole_MouseClick(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstSimonsConsole.MouseClick

    End Sub

    Private Sub lstSimonsConsole_Mousewheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles lstSimonsConsole.MouseWheel

        ' txtServerActivity.Focus()
        '   Call txtServerActivity_Mousewheel(sender, e)

    End Sub
    Public Sub ChangeFormColor(ByVal val As Color)
        Me.BackColor = val
        Me.Invalidate()
        Me.Refresh()

    End Sub
    Public Sub ChangeLogBackColor(ByVal val As Color)
        Me.txtServerActivity.BackColor = val
        Me.Invalidate()
        Me.Refresh()

    End Sub
    Public Sub ChangeWindowTitle(ByVal val As String)
        Me.lblWindowTitle.Text = val

    End Sub
    Private Sub txtServerActivity_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtServerActivity.KeyDown
        e.Handled = True

        If e.KeyCode = Keys.Up Or e.KeyCode = Keys.Down Then
            Me.lstSimonsConsole.Focus()
            GlobalObject.DropDownConditionList(Me.lstSimonsConsole)
            Exit Sub
        ElseIf e.KeyCode = Keys.PageUp Then
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
        ElseIf e.KeyCode = Keys.PageDown Then
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
        ElseIf e.KeyCode = Keys.F1 And tabCsl.SelectedIndex <> 0 Then
            tabCsl.SelectedIndex = 0
        ElseIf e.KeyCode = Keys.F2 And tabCsl.SelectedIndex <> 1 Then
            tabCsl.SelectedIndex = 1
        ElseIf e.KeyCode = Keys.F3 And tabCsl.SelectedIndex <> 2 Then
            tabCsl.SelectedIndex = 2
        Else
            Call lstSimonsConsole_KeyDown(sender, e)
        End If
    End Sub

    Private Sub txtServerActivity_MouseHover(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtServerActivity.MouseHover

    End Sub

    Private Sub txtServerActivity_Mousewheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles txtServerActivity.MouseWheel

        If e.Delta + 120 Then
            '   SendKeys.Send("{PGUP}")
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 0, 0)
        ElseIf e.Delta - 120 Then
            '  SendKeys.Send("{PGDN}")
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
            GlobalObject.SendMessage(txtServerActivity.Handle, GlobalObject.EM_SCROLL, 1, 0)
        End If

    End Sub


    Private Sub ConditionalProgrammingToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub createQueryEditorForm()
        Me.threaded_query_editor_form = New frmQueryEditor(Me.Location, AddressOf Me.serverLog, AddressOf Me.QueryEditorFormClosed)
        System.Windows.Forms.Application.Run(Me.threaded_query_editor_form)

    End Sub
    Public Sub QueryEditorFormClosed(ByVal lastpos As Point)
        'called by thread
        Me.threaded_query_editor_form = Nothing

    End Sub
    Private Sub createQueryEditorThread()
        Me.threaded_query_editor = New Thread(AddressOf createQueryEditorForm)
        threaded_query_editor.SetApartmentState(System.Threading.ApartmentState.STA)
        threaded_query_editor.Start()
    End Sub

    Private Sub lblMaximize_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblMaximize.Click

        If Not Me.WindowState = FormWindowState.Maximized Then
            Me.WindowState = FormWindowState.Maximized
        ElseIf Me.sizeStates.initialized Then
            Me.WindowState = FormWindowState.Normal
            Me.Height = Me.sizeStates.bootHeight
            Me.Width = Me.sizeStates.bootWidth

        End If

        If Me.tabCsl.SelectedIndex = Me.getRootWebbrowserTabIndex Then
            If Me.haveWebbrowserForm Then
                'reload appstore window.
                Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.reloadAppStoreThreaded)
            End If
        ElseIf Me.tabCsl.SelectedIndex = 0 Then
            Me.lstSimonsConsole.Focus()
        End If
    End Sub

    Private Sub lblMinimizeWindow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblMinimizeWindow.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub lblCloseWindow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblCloseWindow.Click
        Call Quit()
    End Sub
    Public Sub Quit()
        GlobalSpeech.Enabled = False

        serverLog("Closing down server...")
        Call Me.cmdServerStop_Click(Nothing, Nothing)

        Application.Exit()
    End Sub

    Private Sub WindowBar_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles WindowBar.MouseDown

        If e.Button = MouseButtons.Left Then
            Me.dragging_tags = True
            Me.drag_startpos = PointToClient(Cursor.Position)
        Else
            Me.dragging_tags = False
        End If

    End Sub

    Private Sub WindowBar_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles WindowBar.MouseUp
        Me.dragging_tags = False
    End Sub

    Private Sub WindowBar_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles WindowBar.MouseMove
        If Me.dragging_tags = True And e.Button = MouseButtons.Left Then
            Me.Location = New Point(Cursor.Position.X - Me.drag_startpos.X, Cursor.Position.Y - Me.drag_startpos.Y)
        End If

    End Sub

    Private Sub WindowBar_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WindowBar.MouseLeave
        If Me.dragging_tags = True Then
            Me.Location = New Point(Cursor.Position.X - Me.drag_startpos.X, Cursor.Position.Y - Me.drag_startpos.Y)
        End If
    End Sub

    Private Sub WindowBar_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles WindowBar.MouseDoubleClick
        Call Me.lblMaximize_Click(sender, e)

    End Sub

    Private Sub lblSizeGrip_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lblSizeGrip.MouseMove
        Cursor.Current = Cursors.SizeNWSE
        lblSizeGrip.BackColor = Color.Black

        If Me.drag_resize = True Then
            If Me.drag_resizepos.Y > Cursor.Position.Y Then
                If Me.Height > 250 And Me.Width > 560 Then
                    'shrink
                    Me.Height -= 20
                    Me.Width -= 20

                End If
            Else
                Me.Height += 15 ' Me.drag_resizepos.Y - Cursor.Position.Y
                Me.Width += 15 ' Me.drag_resizepos.X - Cursor.Position.X
            End If
        End If
    End Sub

    Private Sub lblSizeGrip_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lblSizeGrip.MouseDown

        If e.Button = MouseButtons.Left Then
            Me.drag_resize = True
            Me.drag_resizepos = Cursor.Position

            Me.grpControl.Visible = False
            Me.tabCsl.Visible = False
            Me.Opacity = 90
        Else
            Me.drag_resize = False

            Me.grpControl.Visible = True
            Me.tabCsl.Visible = True
            Me.BackColor = Color.SlateGray
        End If

    End Sub

    Private Sub lblSizeGrip_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lblSizeGrip.MouseUp
        Me.drag_resize = False
        Me.BackColor = Color.SlateGray
        Me.grpControl.Visible = True
        Me.tabCsl.Visible = True
        lblSizeGrip.BackColor = Color.LightGray

        If Me.tabCsl.SelectedIndex = 0 Then
            Me.lstSimonsConsole.Focus()
        ElseIf tabCsl.SelectedIndex = Me.getRootWebbrowserTabIndex Then
            If Me.haveWebbrowserForm Then
                'reload appstore window.
                Me.threaded_webbrowser_form.Invoke(Me.threaded_webbrowser_form.reloadAppStoreThreaded)
            End If
        End If
    End Sub

    Private Sub lblSizeGrip_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblSizeGrip.MouseLeave
        lblSizeGrip.BackColor = Color.Yellow
    End Sub

    Private Sub WindowBar_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WindowBar.DoubleClick
        Call lblMaximize_Click(Nothing, Nothing)
    End Sub

    Private Sub lblWindowTitle_DoubleClick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblWindowTitle.DoubleClick
        Call lblMaximize_Click(Nothing, Nothing)
    End Sub

    Private Sub lblWindowTitle_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblWindowTitle.TextChanged
        Me.Text = Replace(lblWindowTitle.Text, "trinidat", "TriniDAT")

    End Sub

    Private Sub createObjectTrafficListview(ByVal view_mode As ObjectTrafficListview_Mode)

        Dim objecttraffic_listview_col1 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col2 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col3 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col4 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col5 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col6 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col7 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col8 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col9 As System.Windows.Forms.ColumnHeader
        Dim objecttraffic_listview_col10 As System.Windows.Forms.ColumnHeader


        lvObjectTraffic.Columns.Clear()

        objecttraffic_listview_col1 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col2 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col3 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col4 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col5 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col6 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col7 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col8 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col9 = New System.Windows.Forms.ColumnHeader
        objecttraffic_listview_col10 = New System.Windows.Forms.ColumnHeader

        'objecttraffic_listview_col1.Text = "Date"
        'objecttraffic_listview_col2.Text = "App"
        'objecttraffic_listview_col3.Text = "Mapping Point"
        'objecttraffic_listview_col4.Text = "Directive"
        'objecttraffic_listview_col5.Text = "Sent By"
        'objecttraffic_listview_col6.Text = "Direction"
        'objecttraffic_listview_col7.Text = "Object Type"
        'objecttraffic_listview_col8.Text = "Msg Protocol"
        'objecttraffic_listview_col9.Text = "Msg State"

        objecttraffic_listview_col1.Text = "Date"
        objecttraffic_listview_col2.Text = "Client"
        objecttraffic_listview_col3.Text = "Direction"
        objecttraffic_listview_col4.Text = "Directive"
        objecttraffic_listview_col5.Text = "Mapping Point"
        objecttraffic_listview_col6.Text = "App"
        objecttraffic_listview_col7.Text = "Object Type"
        objecttraffic_listview_col8.Text = "Msg Protocol"
        objecttraffic_listview_col9.Text = "Msg State"
        objecttraffic_listview_col10.Text = "Remote IP"

        'objecttraffic_listview_col1.Width = 85
        'objecttraffic_listview_col2.Width = 140
        'objecttraffic_listview_col3.Width = 180
        'objecttraffic_listview_col4.Width = 180
        'objecttraffic_listview_col5.Width = 140
        'objecttraffic_listview_col6.Width = 75
        'objecttraffic_listview_col7.Width = 150
        'objecttraffic_listview_col8.Width = 100
        'objecttraffic_listview_col9.Width = 150

        objecttraffic_listview_col1.Width = 85
        objecttraffic_listview_col2.Width = 120
        objecttraffic_listview_col3.Width = 75
        objecttraffic_listview_col4.Width = 180
        objecttraffic_listview_col5.Width = 180
        objecttraffic_listview_col6.Width = 140
        objecttraffic_listview_col7.Width = 150
        objecttraffic_listview_col8.Width = 100
        objecttraffic_listview_col9.Width = 150
        objecttraffic_listview_col10.Width = 140

        lvObjectTraffic.Columns.Add(objecttraffic_listview_col1)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col2)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col3)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col4)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col5)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col6)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col7)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col8)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col9)
        lvObjectTraffic.Columns.Add(objecttraffic_listview_col10)

    End Sub
    Public Sub AddTraffic(ByVal item As ListViewItem)

        If Not IsNothing(Me.lvObjectTraffic) Then

            If Me.lvObjectTraffic.Items.Count >= MAX_TRAFFIC_LVITEMS Then
                Me.lvObjectTraffic.Items.Clear()
            End If

            Me.lvObjectTraffic.Items.Add(item)


            If chkTrafficScroll.Checked Then
                item.EnsureVisible()
            End If
        End If

    End Sub

    Public Sub OnLogObjectWritten(ByVal obj As XElement)
        Dim lvItem As ListViewItem
        Dim reverse_direction As String
        Dim delivery_state As String
        ' xml_template = "<packet date=""$DATE"" time=""$TIME"" class=""$CLASS"" headerobject=""$HEADEROBJECT"" content_type=""$CONTENT_TYPE"" direction=""$DIRECTION"" from=""$FROM"" to=""$TO"">$JSON</packet>"

        reverse_direction = ""

        If obj.@direction.ToString = "Out" Then
            reverse_direction = "In"
        End If

        delivery_state = HttpUtility.UrlDecode(obj.@deliverystate)

        lvItem = New ListViewItem
        lvItem.Tag = obj
        'lvItem.Text = obj.@time.ToString
        'lvItem.SubItems.Add(obj.@srcappname.ToString)
        'lvItem.SubItems.Add(obj.@srcmpurl.ToString)
        'lvItem.SubItems.Add(obj.@directive.ToString)
        'lvItem.SubItems.Add(obj.@from.ToString)
        'lvItem.SubItems.Add(obj.@direction)
        'lvItem.SubItems.Add(obj.@headerobject.ToString)
        'lvItem.SubItems.Add("JSON")
        'lvItem.SubItems.Add(delivery_state)
        lvItem.Text = obj.@time.ToString
        lvItem.SubItems.Add(obj.@from.ToString)
        lvItem.SubItems.Add(obj.@direction)
        lvItem.SubItems.Add(HttpUtility.UrlDecode(obj.@directive.ToString))
        lvItem.SubItems.Add(obj.@srcmpurl.ToString)
        lvItem.SubItems.Add(obj.@srcappname.ToString)
        lvItem.SubItems.Add(obj.@headerobject.ToString)
        lvItem.SubItems.Add("JSON")
        lvItem.SubItems.Add(delivery_state)
        lvItem.SubItems.Add(obj.@remoteip.ToString)

        If Len(obj.Value) > 1 Then
            lvItem.ForeColor = Color.Purple
        Else
            'no object attachment is marked as suspicious traffic.
            lvItem.ForeColor = Color.DarkOrange
        End If

        Me.AddTraffic(lvItem)

        'generate opposite direction
        If reverse_direction <> "" And obj.@deliverystate = "Delivered" Then
            lvItem = New ListViewItem
            obj.@direction = reverse_direction
            lvItem.Tag = obj
            lvItem.Text = obj.@time.ToString
            lvItem.SubItems.Add(obj.@to.ToString)
            lvItem.SubItems.Add(obj.@direction)
            lvItem.SubItems.Add(HttpUtility.UrlDecode(obj.@directive.ToString))
            lvItem.SubItems.Add(obj.@srcmpurl.ToString)
            lvItem.SubItems.Add(obj.@srcappname.ToString)
            lvItem.SubItems.Add(obj.@headerobject.ToString)
            lvItem.SubItems.Add("JSON")
            lvItem.SubItems.Add("Received")
            lvItem.SubItems.Add(obj.@remoteip.ToString)

            If Len(obj.Value) > 1 Then
                lvItem.ForeColor = Color.Green
            Else
                'no object attachment is marked as suspicious traffic.
                lvItem.ForeColor = Color.DarkOrange
            End If

            Me.AddTraffic(lvItem)
        Else
            'mark as error item
            Dim lvItem_font As Font

            lvItem.ForeColor = Color.Red
            lvItem_font = New Font(lvItem.Font.FontFamily, lvItem.Font.Size, FontStyle.Bold)
            lvItem.Font = lvItem_font
        End If


    End Sub
    Private Sub optSMProd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optSMProd.Click

        If GlobalObject.OfficialLicense.CurrentLicense = TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE Then
            MsgBox("This feature is disabled in the free edition. Upgrade your current version to enable live mode. ", MsgBoxStyle.Exclamation, "Feature")
            Exit Sub
        End If

        Me.txtServerActivity.Text = ""
        Call Me.onServerModeChange()
        Me.cmdClearTrafficListview_Click(sender, Nothing)
        Call Me.createObjectTrafficListview(ObjectTrafficListview_Mode.MODE_ALL)

    End Sub

    Private Sub optSMDev_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles optSMDev.Click
        Me.txtServerActivity.Text = ""

        Call Me.onServerModeChange()

    End Sub

    Private Sub lvObjectTraffic_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvObjectTraffic.Click

        Dim xtraffic_item As XElement
        Dim lvitem As ListViewItem
        Dim reverse_direction As String
        Dim lvTrafficDetailItem As ListViewItem

        Me.lblFrom.ForeColor = Color.YellowGreen
        Me.lblTo.ForeColor = Color.YellowGreen

        Me.lblFrom.Text = ""
        Me.lblTo.Text = ""
        Me.txtTrafficObjectContents.Text = ""
        Me.lvTrafficEntryContent.Items.Clear()

        If Me.lvObjectTraffic.SelectedItems.Count < 1 Then Exit Sub
        lvitem = lvObjectTraffic.SelectedItems(0)

        If IsNothing(lvitem) Then Exit Sub

        xtraffic_item = Nothing

        Try
            If lvitem.Tag.GetType Is GetType(System.Xml.Linq.XElement) Then
                xtraffic_item = CType(lvitem.Tag, XElement)
                GoTo PARSE_AS_XELEMENT
            ElseIf TypeOf lvitem.Tag Is String Then
                'HTTP packet
                lvTrafficDetailItem = New ListViewItem
                lvTrafficDetailItem.Text = "Request"
                lvTrafficDetailItem.ImageIndex = 1
                lvTrafficDetailItem.Tag = lvitem.Tag
                Me.lvTrafficEntryContent.Items.Add(lvTrafficDetailItem)
                Me.lblFrom.Text = lvitem.SubItems(1).Text
                Me.lblTo.Text = "Server"
                GoTo SET_VISIBILITY
            Else
                Dim unknown_type As String
                unknown_type = lvitem.Tag.GetType.ToString

                unknown_type = unknown_type
            End If
        Catch ex As Exception
            Dim unknown_type As String
            unknown_type = lvitem.Tag.GetType.ToString

            unknown_type = unknown_type
        End Try



PARSE_AS_XELEMENT:

        If IsNothing(xtraffic_item) Then Exit Sub

        reverse_direction = ""

        If xtraffic_item.@direction.ToString = "Out" Then
            reverse_direction = "In"
        End If


        If Not IsNothing(xtraffic_item.@stringvalue) Then

            lvTrafficDetailItem = New ListViewItem
            lvTrafficDetailItem.Text = "directive"
            lvTrafficDetailItem.ImageIndex = 1
            lvTrafficDetailItem.Tag = HttpUtility.UrlDecode(xtraffic_item.@stringvalue.ToString)
            Me.lvTrafficEntryContent.Items.Add(lvTrafficDetailItem)
        End If

        If Len(xtraffic_item.Value) > 1 Then
            lvTrafficDetailItem = New ListViewItem
            lvTrafficDetailItem.Text = "attachment"
            lvTrafficDetailItem.ImageIndex = 0
            lvTrafficDetailItem.Tag = HttpUtility.UrlDecode(xtraffic_item.Value)
            Me.lvTrafficEntryContent.Items.Add(lvTrafficDetailItem)

        End If


        Me.lblFrom.Text = xtraffic_item.@from
        Me.lblTo.Text = xtraffic_item.@to

SET_VISIBILITY:
        Me.txtTrafficObjectContents.Visible = False
        Me.lblFromCaption.Visible = True
        Me.lblContainerHeader.Visible = True
        Me.lblFrom.Visible = True
        Me.lblToCaption.Visible = True
        Me.lblTo.Visible = True
        Me.lvTrafficEntryContent.Visible = True

    End Sub


    Private Sub lvObjectTraffic_KeyDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lvObjectTraffic.KeyDown
        Call lvObjectTraffic_Click(Nothing, Nothing)

    End Sub

    Private Sub cmdClearTrafficListview_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClearTrafficListview.Click
        Me.lvObjectTraffic.Items.Clear()

    End Sub

    Private Sub tabTrafficOverview_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tabTrafficOverview.Resize
        Me.lvObjectTraffic.Width = tabTrafficOverview.Width - 10
        Me.cmdClearTrafficListview.Left = Me.lvObjectTraffic.Width - Me.cmdClearTrafficListview.Width + 2
        Me.chkTrafficScroll.Left = Me.lvObjectTraffic.Width - Me.chkTrafficScroll.Width + 2

        'trigger the resizing of all footer controls by changing lvTrafficEntryContent.
        lvTrafficEntryContent.Top = tabTrafficOverview.Height - lvTrafficEntryContent.Height - 5

        'adjust Me.lvObjectTraffic.Width  to footer max.
        Me.lvObjectTraffic.Height = lblFromCaption.Top - Me.lvObjectTraffic.Top - 10
        Me.cmdClearTrafficListview.Top = Me.lvObjectTraffic.Height + Me.lvObjectTraffic.Top + 5


    End Sub

    Private Sub lvTrafficEntryContent_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvTrafficEntryContent.Click
        If Me.lvTrafficEntryContent.SelectedItems.Count < 1 Then Exit Sub

        If IsNothing(Me.lvTrafficEntryContent.SelectedItems(0)) Then Exit Sub

        txtTrafficObjectContents.Text = Me.lvTrafficEntryContent.SelectedItems(0).Tag
        Me.txtTrafficObjectContents.Visible = True
        Me.lvTrafficEntryContent.Visible = True
        Me.lblContainerHeader.Visible = True

    End Sub

    Private Sub cmdAppEngine_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdAppInstallation.Click

        Dim AppInstaller As AppStoreInstaller
        Dim app_zip_file As String

        'hide appstore browser to prevent design flaws.
        If tabCsl.SelectedIndex = Me.getRootWebbrowserTabIndex Then
            Me.tabCsl.SelectedIndex = 0
        End If

        If Not GlobalObject.haveSimon Then
            GlobalObject.MsgColored("Unable to access '" & GlobalSetting.getSimonXMCommandFile() & "'", Color.Red)
            MsgBox("Simon commands unavailable. Please check file permissions on '" & GlobalSetting.getSimonXMCommandFile() & "'")
            Exit Sub
        End If

        Me.browseApp.Title = "Select A TriniDAT Data Application Package"
        If Me.browseApp.ShowDialog() = Windows.Forms.DialogResult.Cancel Then
            Exit Sub
        End If

        ' app_zip_file = "C:\Users\gertjan\AppData\Local\Temp\trinidat_publisher.zip"
        app_zip_file = Me.browseApp.FileName

        AppInstaller = New AppStoreInstaller(app_zip_file)
        'check for license

        If AppInstaller.Install(True) Then
            Dim license_frm As frmAppLicense
            license_frm = New frmAppLicense
            license_frm.Text = "License Agreement"
            license_frm.txtLicense.Text = File.ReadAllText(AppInstaller.TempLicenseFile)
            license_frm.ShowDialog()

            If license_frm.DialogResult = Windows.Forms.DialogResult.Abort Then
                MsgBox("Installation was aborted by the user.")
                Exit Sub
            End If
        End If

        GlobalSpeech.Text = "Installing new application..."
        GlobalSpeech.SpeakThreaded()

        If AppInstaller.Install() Then

            'reload the application cache.
            Call GlobalObject.simon.Execute("reload")
            Call GlobalObject.simon.Execute("appmeta " & AppInstaller.NewId.ToString)

            GlobalObject.MsgColored("Successfully installed " & app_zip_file & ".", Color.Pink)
            GlobalSpeech.Text = "application successfully installed."
            GlobalSpeech.SpeakThreaded()

        Else
            MessageBox.Show("Installation failed.", "App Installer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
        End If

        Exit Sub



        'Dim code_compiler As SimpleCodeCompiler

        'code_compiler = New SimpleCodeCompiler
        'code_compiler.Test()
    End Sub

    Private Sub mnuAttachmentSpeakText_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAttachmentSpeakText.Click
        If Me.lvTrafficEntryContent.SelectedItems.Count < 1 Then Exit Sub

        If IsNothing(Me.lvTrafficEntryContent.SelectedItems(0)) Then Exit Sub

        Dim lvitem As ListViewItem

        lvitem = Me.lvTrafficEntryContent.SelectedItems(0)

        If lvitem.Text = "Directive" Then

            GlobalSpeech.Text = CType(lvitem.Tag, String)
            GlobalSpeech.Text = Replace(GlobalSpeech.Text, "_", " ")
            GlobalSpeech.Text = "directive: " & GlobalSpeech.Text
            GlobalSpeech.SpeakThreaded()

        End If

    End Sub

    Private Sub ctxmnuAttachment_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles ctxmnuAttachment.Opening
        If Me.lvTrafficEntryContent.SelectedItems.Count < 1 Then Exit Sub

        If IsNothing(Me.lvTrafficEntryContent.SelectedItems(0)) Then Exit Sub

        Dim lvitem As ListViewItem

        lvitem = Me.lvTrafficEntryContent.SelectedItems(0)

        If lvitem.Text <> "Directive" Then
            e.Cancel = True
        End If
    End Sub

    Private Sub cmdSpeechEnableToggle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSpeechEnableToggle.Click

        If Not GlobalSetting.haveSimonCommandFile() Then
            GlobalSpeech.Enabled = False
            GlobalObject.MsgColored("Cannot active speech feature. Internal file(s) are missing.", Color.Red)
            Exit Sub
        End If

        If GlobalSpeech.Enabled Then
            GlobalSpeech.Enabled = False
        Else
            GlobalSpeech.Enabled = True
        End If

    End Sub


    Private Sub lstSimonsConsole_KeyUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles lstSimonsConsole.KeyUp
        If Me.lstSimonsConsole.Text.Length = 0 And (e.KeyCode = Keys.Back Or e.KeyCode = Keys.Delete) Then
            GlobalObject.CloseDropDownConditionList(Me.lstSimonsConsole)
            Me.SimonsLine.BorderColor = Color.MidnightBlue
            Me.txtServerActivity.Focus()
        End If
    End Sub

    Private Sub lstSimonsConsole_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSimonsConsole.Enter
        Me.lstSimonsConsole.BackColor = Color.LightGray
        Me.lstSimonsConsole.FlatStyle = FlatStyle.Flat


    End Sub

    Private Sub lstSimonsConsole_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSimonsConsole.Leave

        If Me.dragging_tags = True Then Exit Sub

        Me.lstSimonsConsole.BackColor = Color.LightYellow  'Color.LightBlue
        Me.lstSimonsConsole.FlatStyle = FlatStyle.Flat

    End Sub

    Private Sub txtServerActivity_Enter(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtServerActivity.Enter
        If GlobalObject.haveServerthread Then
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_LIVE Then
                Me.txtServerActivity.ForeColor = Color.Black
            End If
        End If
    End Sub

    Private Sub txtServerActivity_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtServerActivity.Leave
        If GlobalObject.haveServerthread Then
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_LIVE Then
                Me.txtServerActivity.ForeColor = Color.WhiteSmoke
            End If
        End If

    End Sub

    Private Sub lstSimonsConsole_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles lstSimonsConsole.KeyPress
        If Not GlobalObject.haveSimon Then Exit Sub

        If e.KeyChar = Chr(13) And Me.lstSimonsConsole.Text <> "" Then
            Dim command_valid As Boolean
            Dim simon_cmd As String

            simon_cmd = Me.lstSimonsConsole.Text.Trim
            e.Handled = True

            If simon_cmd = "empty" Then
                Me.lstSimonsConsole.Items.Clear()
                Exit Sub
            End If

            command_valid = GlobalObject.simon.Execute(simon_cmd)
            Me.lstSimonsConsole.Text = ""
            'trigger empty line line coloring sub.
            Me.SimonsLine.BorderColor = Color.MidnightBlue

            If command_valid Then
                If Not Me.haveSimonPastCommand(simon_cmd) Then
                    'add valid commands to past buffer
                    Me.lstSimonsConsole.Items.Add(simon_cmd)
                End If
                GlobalObject.CloseDropDownConditionList(Me.lstSimonsConsole)
            End If
        End If
    End Sub

    Private Sub txtServerActivity_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles txtServerActivity.MouseMove
        Me.Cursor = Cursors.Arrow
        If Not Me.txtServerActivity.Focused Then
            Me.txtServerActivity.Focus()
        End If


    End Sub

    Private Sub frmServerMain_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        If Not Me.txtServerActivity.Focused Then
            txtServerActivity.Focus()
            Call txtServerActivity_Mousewheel(sender, e)
        End If
    End Sub

    Private Sub txtServerActivity_SelectionChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtServerActivity.SelectionChanged

        mnuServerLog_Copy.Enabled = (txtServerActivity.SelectedText.Length > 0)

        If mnuServerLog_Copy.Enabled Then
            mnuServerLog_GotoURL.Enabled = False

            If InStr(Trim(txtServerActivity.SelectedText), " ") = 0 Then

                Try
                    Dim text_uri As Uri
                    Dim potential_url As String
                    potential_url = txtServerActivity.SelectedText

                    If Mid(potential_url, 1, 3) = "www" Then
                        potential_url = "http://" & potential_url
                    End If

                    text_uri = New Uri(potential_url)
                    If text_uri.IsAbsoluteUri Then
                        mnuServerLog_GotoURL.Enabled = True
                        mnuServerLog_GotoURL.Tag = potential_url
                    End If

                Catch ex As Exception

                End Try

            End If
        End If

    End Sub

    Private Sub mnuServerLog_Copy_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuServerLog_Copy.Click

        If txtServerActivity.SelectedText.Length > 0 Then

            Clipboard.SetText(txtServerActivity.SelectedText)
        End If

    End Sub

    Private Sub mnuServerLog_GotoURL_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuServerLog_GotoURL.Click

        If mnuServerLog_GotoURL.Tag.ToString <> "" Then
            GlobalObject.OpenURL(mnuServerLog_GotoURL.Tag.ToString)
        End If


    End Sub


    Private Sub lstSimonsConsole_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstSimonsConsole.SelectedIndexChanged
        If Me.lstSimonsConsole.Text.Length = 0 Then
            Me.SimonsLine.BorderColor = Color.MidnightBlue
        ElseIf Me.SimonsLine.BorderColor <> Color.brown Then
            Me.SimonsLine.BorderColor = Color.brown
        End If

        If lstSimonsConsole.Tag = 1 Then
            'do no process on arrow keys
            lstSimonsConsole.Tag = 0
            Exit Sub
        End If

        If IsNothing(lstSimonsConsole.SelectedItem) Then Exit Sub

        Dim simon_cmd As String

        simon_cmd = lstSimonsConsole.SelectedItem.ToString

        If lstSimonsConsole.SelectedItem.ToString <> "" Then
            GlobalObject.simon.Execute(simon_cmd)
            lstSimonsConsole.SelectedIndex = -1
        End If
    End Sub

    Private Sub lstSimonsConsole_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles lstSimonsConsole.MouseMove
        If Not Me.lstSimonsConsole.Focused And Me.lstSimonsConsole.CanFocus Then
            Me.lstSimonsConsole.Focus()
        End If


    End Sub

    Private Sub txtServerActivity_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtServerActivity.KeyPress
        e.Handled = True
        lstSimonsConsole.Focus()
        SendKeys.Send("{END}")

        lstSimonsConsole.SelectionStart = Len(lstSimonsConsole.Text)
        lstSimonsConsole.SelectionLength = 1

        If e.KeyChar > ChrW(31) And e.KeyChar < ChrW(127) Then

            If lstSimonsConsole.SelectionLength > 0 Then
                lstSimonsConsole.Text = e.KeyChar
            Else
                lstSimonsConsole.Text &= e.KeyChar
            End If
        ElseIf e.KeyChar = Chr(72) Or e.KeyChar = Chr(80) Then
            Me.lstSimonsConsole.Show()


        End If

        lstSimonsConsole.SelectionLength = 0

        Call lstSimonsConsole_KeyPress(sender, e)
    End Sub

    Private Sub frmServerMain_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        '  Me.Visible = False
        Me.lstSimonsConsole.Focus()
    End Sub


    Private Sub SimonsLine_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles SimonsLine.MouseMove
        Call lstSimonsConsole_MouseMove(sender, e)
    End Sub

    Private Sub mnuServerLog_Clear_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuServerLog_Clear.Click

        If GlobalObject.haveSimon Then
            GlobalObject.simon.Execute(SIMON_COMMAND_CLEAR)
        End If

    End Sub

    Private Sub txtServerActivity_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkClickedEventArgs) Handles txtServerActivity.LinkClicked

        mnuServerLog_GotoURL.Enabled = True
        mnuServerLog_GotoURL.Tag = e.LinkText
        Call Me.mnuServerLog_GotoURL_Click(sender, Nothing)

    End Sub

    Private Sub lvTrafficEntryContent_Move(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvTrafficEntryContent.Move
        txtTrafficObjectContents.Left = lvTrafficEntryContent.Left + 1 + lvTrafficEntryContent.Width
        txtTrafficObjectContents.Top = lvTrafficEntryContent.Top
        lblContainerHeader.Top = lvTrafficEntryContent.Top - 15
        lblTo.Top = lblContainerHeader.Top - lblContainerHeader.Height - 5
        lblFrom.Top = lblTo.Top - lblTo.Height - 10
        lblFromCaption.Top = lblFrom.Top
        lblToCaption.Top = lblTo.Top

    End Sub

    Private Sub lvTrafficEntryContent_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles lvTrafficEntryContent.Resize

    End Sub

    Private Sub cmdDumpFailed_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdDumpInvalid.Click

        'GlobalSetting.
        'deliverystate <> "Delivered"

        If Not TrafficMonitor.haveLogFile Then
            MsgBox("No log file found.")
            Exit Sub
        End If

        Dim xtraffic As XDocument
        Dim sTempFileName As String
        Dim fsTemp As System.IO.FileStream

        sTempFileName = System.IO.Path.GetTempFileName()
        fsTemp = New System.IO.FileStream(sTempFileName, IO.FileMode.Create)


        Try
            xtraffic = XDocument.Parse("<packets>" & File.ReadAllText(TrafficMonitor.FilePath) & "</packets>")

        Catch ex As Exception
            MsgBox("Unable to open file '" & TrafficMonitor.FilePath & "'.")

            fsTemp.Close()
            Exit Sub
        End Try

        Dim xquery = From xlog_fail_item In xtraffic.Descendants("packets") Where xlog_fail_item.@deliverystate <> "Delivered"
        Dim buffer As String
        Dim line_in_bytes() As Byte

        'header
        buffer = "<packets>"
        line_in_bytes = Encoding.UTF8.GetBytes(buffer.ToString)
        fsTemp.Write(line_in_bytes, 0, line_in_bytes.Length)

        For Each failed_packet As XElement In xquery

            line_in_bytes = Encoding.UTF8.GetBytes(failed_packet.ToString)

            fsTemp.Write(line_in_bytes, 0, line_in_bytes.length)
        Next


        'footer
        buffer = "</packets>"
        line_in_bytes = Encoding.UTF8.GetBytes(buffer.ToString)
        fsTemp.Write(line_in_bytes, 0, line_in_bytes.Length)

        fsTemp.Close()

        Dim oPro As New Process

        With oPro
            .StartInfo.UseShellExecute = True
            .StartInfo.Arguments = sTempFileName
            .StartInfo.FileName = "notepad.exe"
            .Start()
        End With


    End Sub

    Private Sub cmdClearTrafficListview_Move(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdClearTrafficListview.Move
        cmdDumpInvalid.Top = Me.cmdClearTrafficListview.Top
        cmdDumpInvalid.Left = Me.cmdClearTrafficListview.Left - Me.cmdClearTrafficListview.Width - 20
    End Sub

    Private Sub cmdNetwork_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdNetwork.Click


        If MessageBox.Show("This will restart the server, would you like to continue?", "Network Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) <> DialogResult.Yes Then
            Exit Sub
        End If

        Dim pi As PickIPAddress

        If GlobalObject.haveServerThread Then
            Call cmdServerStop_Click(Nothing, Nothing)
        End If

        pi = New PickIPAddress
        pi.ShowDialog()

        Call cmdServerStart_Click(Nothing, Nothing)

    End Sub
    Private Sub OnFirstRun()
        'context; .Invoke'd  by ServerEvents8

        GlobalObject.MsgColored("Info: First run detected.", Color.Gold)

        If Not GlobalObject.haveSimon Then
            GlobalObject.MsgColored("Error: simon interfact not active.", Color.Gold)
            Exit Sub
        End If

        MessageBox.Show("Welcome!" & vbNewLine & vbNewLine & "TriniDAT Server has detected your first run of this software. After you have clicked 'OK',  additional components will be downloaded to finish the server's installation.", "TriniDAT", MessageBoxButtons.OK, MessageBoxIcon.Information)
    

        Dim download_cmd As String
        Dim temp_file As String
        Dim install_ok As Boolean

        temp_file = GlobalSetting.getTempDir() & "indexpage.zip"
        install_ok = False

        GlobalObject.MsgColored("Downloading core application files, please wait...", Color.LightPink)

        download_cmd = "DOWNLOADFILE " & GlobalSetting.CoreApp_ServerIndexURL & " " & temp_file

        If GlobalObject.simon.Execute(download_cmd) = True Then
            GlobalObject.MsgColored("Download complete. Starting installation of 'TriniDAT index app'...", Color.Gold)

            Dim appInstaller As AppStoreInstaller
            appInstaller = New AppStoreInstaller(temp_file, True)
            appInstaller.NewId = 1
            install_ok = appInstaller.Install(False)

            If install_ok Then
                GlobalObject.MsgColored("Installation complete. Triggering cache reload event.", Color.DarkGreen)
                install_ok = GlobalObject.simon.Execute("RELOAD")
            End If

        End If

        If install_ok Then
            GlobalObject.MsgColored("First run: Installation complete!", Color.Gold)

            If GlobalObject.ApplicationCache.haveApplication(1) Then
                GlobalSpeech.Text = "All files were installed successfully."
                GlobalSpeech.SpeakThreaded()

                If MessageBox.Show("Installation was successful! Would you like to see the server page now?", "TriniDAT", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
                    GlobalObject.MsgColored("Executing Simon command: 'WWW'...", Color.DarkGreen)
                    GlobalObject.simon.Execute("WWW")
                    'stimulate GUI thread for cold boot error.
                    Thread.Sleep(500)
                    Me.tabCsl.Visible = False
                    Me.tabCsl.Visible = True
                End If
            End If
        Else
            GlobalObject.MsgColored("First run: Sorry, some components failed to install. Try again later.", Color.Red)

            GlobalSpeech.Text = "We are sorry. Some components failed to install."
            GlobalSpeech.SpeakThreaded()
        End If

    End Sub
    Private Sub lblLinkForum_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblLinkForum.Click
        '  MsgBox(Len(Me.txtUserPaymentHandle.Text))
        GlobalObject.OpenURL("http://www.deleeuwict.nl/support")

    End Sub

    Private Sub lblPaymentRegister_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblPaymentRegister.Click
        GlobalObject.OpenURL("http://www.deleeuwict.nl/forum")

    End Sub

    Private Sub lblPaymentRegister_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblPaymentRegister.MouseHover
        Cursor.Current = Cursors.Hand

    End Sub

    Private Sub lblPaymentRegister_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles lblPaymentRegister.MouseLeave
        Cursor.Current = Cursors.Arrow
    End Sub

    Private Sub txtUserPaymentHandle_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUserPaymentHandle.Leave
       


    End Sub

    Private Sub txtUserPaymentHandle_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUserPaymentHandle.TextChanged
        cmdUpdatePaykey.Enabled = True
    End Sub

    Private Sub cmdUpdatePaykey_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdUpdatePaykey.Click
        Dim wc As WebClient
        Dim retval As String

        Call txtUserPaymentHandle_Leave(sender, Nothing)
        Me.cmdUpdatePaykey.Enabled = False
        Me.cmdNetwork.Focus()

        Try

            wc = New WebClient
            retval = wc.DownloadString("http://www.deleeuwict.nl/trinidat/api/vkey.php?v=" & txtUserPaymentHandle.Text)

            If retval = "SUCCESS" Then
                GlobalSpeech.Text = "Congratulations, payment data is saved."
                GlobalSpeech.SpeakEliteThreaded()
                MessageBox.Show("Payment key OK.", "Payment data Saved", MessageBoxButtons.OK, MessageBoxIcon.Information)

                'save user's payment info.
                Me.txtUserPaymentHandle.Text = Trim(Me.txtUserPaymentHandle.Text)

                Me.cmdUpdatePaykey.Enabled = (Me.txtUserPaymentHandle.Text.Length > 0)

                Dim latest_config As BosswaveTCPServerConfig
                latest_config = GlobalSetting.getHTTPServerConfig()

                GlobalSetting.PayKey = Trim(Me.txtUserPaymentHandle.Text)

                If Not latest_config.Write() Then
                    MessageBox.Show("Error writing server config.", "Server Configuration", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                End If

                lblPaymentRegister.Visible = Not GlobalSetting.havePayKey
                lblNoIdInfo.Visible = lblPaymentRegister.Visible


            ElseIf retval = "FAIL" Then
                GlobalSpeech.Text = "this payment key is not valid. please enter correctly."
                GlobalSpeech.SpeakEliteThreaded()
                MessageBox.Show("The payment key you entered could not be validated.", "Invalid Payment data", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)

            Else
                GlobalSpeech.Text = "error connecting to server."
                GlobalSpeech.SpeakEliteThreaded()
                Throw New Exception("Unknown server response")
            End If

        Catch ex As Exception
            MessageBox.Show("Unable to connect to payment server: " & ex.Message, "Payment Server Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            GlobalObject.MsgColored(ex.Message, Color.Red)
        End Try

    End Sub
End Class

Public Class sizeStates
    Public MainTabCtrlTop As Integer
    Public MainTabCtrlSubstractFormHeight As Integer
    Public CloseWindowCtrlPosWidthDiff As Integer
    Public MinimizeCtrlPosWidthDiff As Integer
    Public MaximizeCtrlPosWidthDiff As Integer
    Public bootHeight As Integer
    Public bootWidth As Integer
    Public buttonDiff As Integer
    Public serverLogInitialTop As Integer
    Public initialized As Boolean
    Public Sub New()
        initialized = False
    End Sub
End Class

Public Enum ObjectTrafficListview_Mode
    MODE_ALL = 1
End Enum