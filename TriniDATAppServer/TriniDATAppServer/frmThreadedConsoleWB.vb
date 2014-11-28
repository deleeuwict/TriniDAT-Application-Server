Public Class frmThreadedConsoleWB
    Public Delegate Sub ShowNow()
    Public Delegate Sub HideNow()

    ' Public Delegate Sub onTopNow()
    Public Delegate Sub resizeNow()
    Public Delegate Sub KillProc()
    Public Delegate Sub ReloadAppStoreDelegate()

    Public Delegate Sub setPositions(ByVal coord As Rectangle)

    Public reloadAppStoreThreaded As New ReloadAppStoreDelegate(AddressOf ReloadAppStoreThrd)

    Public setNewPositionByRectangle_threaded As New setPositions(AddressOf setThreadedSizeInfo)
    Public resizeNowThreaded As New resizeNow(AddressOf doThreadedResize)
    Public enableStayOnTopThreaded As New resizeNow(AddressOf doSetThreadedOnTop)
    Public disableStayOnTopThreaded As New resizeNow(AddressOf doUnSetThreadedOnTop)
    Public KillFormThreaded As New KillProc(AddressOf KillForm)

    Public ShowNowThreaded As New ShowNow(AddressOf ShowFormThreaded)
    Public HideNowThreaded As New HideNow(AddressOf HideFormThreaded)

    Private thread_coord As Rectangle
    Private onfocus_change_callback As frmServerMain.setWBFocusStatus
    Private first_time_show As Boolean

    Public Sub New(ByVal coord As Rectangle, ByVal _focus_change_callback As frmServerMain.setWBFocusStatus)
        InitializeComponent()
        Me.Visible = False
        Me.onfocus_change_callback = _focus_change_callback

        Me.setThreadedSizeInfo(coord)

    End Sub
    Private Sub KillForm()
        Me.Close()
    End Sub
    Private Sub ShowFormThreaded()
        Call onfocus_change_callback(True)

        If Not first_time_show Then
            'else wait for DocumentCompleted event to show form
            Me.Show()
        End If

    End Sub
    Private Sub HideFormThreaded()
        Me.Hide()
    End Sub
    Private Sub setThreadedSizeInfo(ByVal coord As Rectangle)
        Me.thread_coord = coord
    End Sub
    Private Sub ReloadAppStoreThrd()

        web.Navigate("http://www.deleeuwict.nl/trinidat/appstore/storeindex.php?v=" & HttpUtility.UrlEncode(GlobalObject.getVersionString()) & "&r=" & Second(Now).ToString & "&viewer=" & GlobalSetting.PayKey)

    End Sub
    Private Sub frmThreadedConsoleWB_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Call ShowRoot()

    End Sub

    Private Overloads Sub OnLostFocus(ByVal sender As Object, ByVal e As EventArgs)
        Call Me.OnLostFocus(e)
    End Sub
    Protected Overrides Sub OnLostFocus(ByVal e As EventArgs)
        Call onfocus_change_callback(False)
    End Sub

    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        Call onfocus_change_callback(True)
    End Sub
    Private Overloads Sub OnGotFocus(ByVal sender As Object, ByVal e As EventArgs)
        Call Me.OnGotFocus(e)
    End Sub

    Private Sub doUnSetThreadedOnTop()
        Me.TopMost = False
    End Sub

    Private Sub doSetThreadedOnTop()
        Me.TopMost = True
    End Sub
    Private Sub doThreadedResize()
        Call refreshWindowPos()
    End Sub

    Private Sub ShowRoot()
        ' web.Navigate(GlobalSetting.getStaticDataRoot() & "webconsole\appstore.html")
        web.Navigate("http://www.deleeuwict.nl/trinidat/appstore/storeindex.php?v=" & HttpUtility.UrlEncode(GlobalObject.getVersionString()) & "&r=" & Second(Now).ToString & "&viewer=" & Me.Tag)
        ' web.Navigate("http://192.168.2.1/1")
    End Sub

    Public Sub refreshWindowPos()
        Me.Size = Me.thread_coord.Size
        Me.Location = Me.thread_coord.Location

        Me.web.Height = Me.Height - 7
        Me.web.Width = Me.Width - 5
    End Sub

    Private Sub frmThreadedConsoleWB_Shown(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Shown
        Call refreshWindowPos()
        Call onfocus_change_callback(True)
    End Sub


    Private Sub web_DocumentCompleted(ByVal sender As System.Object, ByVal e As System.Windows.Forms.WebBrowserDocumentCompletedEventArgs) Handles web.DocumentCompleted
        Me.Show()

        're-enable tab selection.
        Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.OnAppStoreFormLoadedThread)

    End Sub

End Class