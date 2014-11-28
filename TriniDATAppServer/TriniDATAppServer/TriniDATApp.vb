Option Explicit On
Option Compare Text

Imports Microsoft.VisualBasic.ApplicationServices
Imports System
Imports System.EventArgs
Imports System.Windows
Imports System.IO
Imports System.Reflection
Imports TriniDATSockets
Imports System.Threading


Namespace TriniDAT
    Public Class BosswaveTabbed2 'fixes InitializeComponent error in frmServerMain.
        Inherits BosswaveTabbed
    End Class

    Partial Public Class TriniDAT_Core
        Inherits Application

        Private MainForm As frmServerMain
        Private objMutex As System.Threading.Mutex

        Public Sub New()
            MyBase.New()

            objMutex = New System.Threading.Mutex(False, "TriniDAT")
            If objMutex.WaitOne(0, False) = False Then
                objMutex.Close()
                objMutex = Nothing
                MessageBox.Show("Another instance is already running!")
                End
            End If

            System.Windows.Forms.Application.EnableVisualStyles()


            AddHandler AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve, AddressOf Me.OnReflectionOnlyResolve
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf Me.OnLoadAsmOnDemand
            AddHandler AppDomain.CurrentDomain.ProcessExit, AddressOf onTermination
            AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf onError



        End Sub

        Private Shared Sub onError(ByVal sender As Object, ByVal e As System.UnhandledExceptionEventArgs)

            If GlobalObject.haveServerForm Then
                GlobalObject.MsgColored("Unhandled exception notice: " & e.ExceptionObject.Message, Color.Red)
            End If

            Dim targetPath As String
            Dim targetFilename As String
            Dim error_log As String

            targetPath = "C:\temp\"

            If Not Directory.Exists(targetPath) Then
                Directory.CreateDirectory(targetPath)
            End If

            targetFilename = "trinidat_error.txt"
            error_log = targetPath & targetFilename

            If File.Exists(error_log) Then
                File.AppendAllText(error_log, Now.ToString & " Error: " & e.ExceptionObject.Message.ToString)
            Else
                File.WriteAllText(error_log, Now.ToString & "Error: " & e.ExceptionObject.Message.ToString)
            End If


        End Sub

        Public Shared Function onTermination(ByVal sender As Object, ByVal args As System.ResolveEventArgs) As System.Reflection.Assembly
            GlobalSpeech.Enabled = False

            'close log
            TrafficMonitor.closeStream()


        End Function

        Public Shared Function OnLoadAsmOnDemand(ByVal sender As Object, ByVal args As System.ResolveEventArgs) As System.Reflection.Assembly

            If InStr(args.Name, "resource") Then Return Nothing

            If Not GlobalObject.haveApplicationCache Then GoTo NOT_FOUND

            'scan all apps depedencies

            For Each trinidad_app As BosswaveApplication In GlobalObject.ApplicationCache

                If trinidad_app.haveMappingPoints Then

                    For Each trinidad_app_mp In trinidad_app.ApplicationMappingPoints
                        If trinidad_app_mp.haveMappingPointInstance() Then

                            If trinidad_app_mp.MappingPoint.HaveDependencyList() Then

                                For Each dep_path_or_fullname As String In trinidad_app_mp.MappingPoint.getDependencyPaths()
                                    'path: assembly string
                                    '        full path

                                    If InStr(dep_path_or_fullname, "Culture") = 0 Then
                                        Dim fi As FileInfo
                                        Dim dll_name As String

                                        Try
                                            fi = New FileInfo(dep_path_or_fullname)

                                            If Not IsNothing(fi) Then
                                                dll_name = Replace(fi.Name, ".dll", "")

                                                If Left(args.Name, Len(dll_name)) = dll_name Then
                                                    Return Assembly.LoadFile(dep_path_or_fullname)
                                                End If

                                            End If
                                        Catch ex As Exception
                                            GlobalObject.Msg("Error while loading assembly file " & dep_path_or_fullname & ": " & ex.Message)
                                        End Try
                                    ElseIf args.Name = dep_path_or_fullname Then
                                        Try
                                            Return Assembly.Load(dep_path_or_fullname)

                                        Catch ex As Exception
                                            GlobalObject.Msg("Error while loading assembly by name " & dep_path_or_fullname & ": " & ex.Message)
                                        End Try
                                    End If
                                Next
                            End If
                        End If
                    Next
                End If
            Next

NOT_FOUND:
            Try
                If InStr(args.Name, "Microsoft.mshtml") = 0 Then
                    GlobalObject.Msg("Warning, unable to load '" & args.Name & ":  file does not exist or incorrect library version. Please update the local .NET Framework GAC with the affected library file(s).")
                End If
                'appdomain.CurrentDomain.
                Return Nothing

            Catch ex As Exception


                Return Nothing
            End Try


        End Function

        Public Shared Function getReflectionOnlyAssemblyByFilename(ByVal filename As String) As Assembly
            Dim loaded_asm As Assembly
            Dim all_asms As List(Of Assembly)
            Dim current_assembly_filename As FileInfo
            Dim search_file_title As FileInfo

            search_file_title = New FileInfo(filename)

            all_asms = New List(Of Assembly)(AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            all_asms.AddRange(AppDomain.CurrentDomain.GetAssemblies())

            For Each loaded_asm In all_asms

                Try

                    If loaded_asm.Location <> "" Then
                        current_assembly_filename = New FileInfo(loaded_asm.Location)
                    Else
                        current_assembly_filename = New FileInfo(loaded_asm.ManifestModule.ScopeName)
                    End If


                    If current_assembly_filename.Name = search_file_title.Name Then
                        If Not loaded_asm.ReflectionOnly Then
                            'load as reflection only
                            Return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(loaded_asm.Location))
                        Else
                            Return loaded_asm
                        End If
                    End If


                Catch ex As Exception

                End Try
            Next
            Return Nothing
        End Function

      
        Public Function getLTBSD(ByVal val1 As String, ByVal val2 As String) As Integer
            'Get License Type By Incorrect Server Date.
            'val1 = server date in format ("dd-mm-yyyyy hh:ss")
            'val2 = server's returned gid code.
            'return value: 0 to 4 license type
            '                    negative number if invalid.
            '
            'todo: also goof with year

            Dim retval As TriniDATServerTypes.TRINIDAT_SERVER_LICENSE
            Dim dparts() As String 'date parts
            Dim gparts() As String 'gid parts
            Dim MAANDjaardag As Integer
            Dim g As Integer 'gid index

            dparts = Split(val1, "-") ' ("dd-mm-yyyyy hh:ss")
            If dparts.Length <> 3 Then Return 0 - Second(Now) - 4

            gparts = Split(val2, "-") 'AAAAAA-BBBBBB-DDDDDD-FFFFFF-GGGGGG (len: 35)
            If Len(val2) <> 34 Or gparts.Length <> 5 Then Return 0 - Second(Now) - 2

            'verify the pseudo month value.
            If Not IsNumeric(dparts(1)) Then Return 0 - Second(Now) - 3

            MAANDjaardag = CInt(dparts(1))

            'get gidpart by invalid month's range

            If MAANDjaardag <= 3 Then
                g = 0 'gid part to look at
            ElseIf MAANDjaardag <= 6 Then
                g = 1 'gid part to look at
            ElseIf MAANDjaardag <= 9 Then
                g = 2 'gid part to look at
            ElseIf MAANDjaardag <= 11 Then
                g = 3 'gid part to look at
            Else
                g = 4 'gid part to look at
            End If

            'last char in gid section is licenseid 0..4
            Dim lc As String
            Dim invalid_serial As Boolean 'note: this is a fake var name and functions exactly the opposite.
            lc = Right(gparts(g), 1)

            invalid_serial = True

            If Not IsNumeric(lc) Then
                invalid_serial = False
            Else
                If CLng(lc) < 0 Or CLng(lc) > 4 Then
                    invalid_serial = False
                End If
            End If

            If Not invalid_serial Then
                Return 0 - Second(Now) - 28
            End If

            retval = CInt(lc)

            Return retval
        End Function
        Public Function validateInstallation()

            Dim fi_executable As FileInfo
            Dim fi_types As FileInfo
            Dim fi_l As FileInfo
            Dim md5_exec As String '<TriniDAT executable file>
            Dim md5_types As String 'TriniDATServerTypes.dll
            Dim md5_l As String 'TriniDATServerLicense.dll
            Dim fatal_err_msg As String
            Dim w As String
            Dim current_license_name As String
            Dim license_class_obj As Object


            fi_executable = New FileInfo(Assembly.GetExecutingAssembly().Location)
            fatal_err_msg = "Fatal error while validating installation files. Quiting"

            md5_exec = GlobalObject.getMD5CheckSum(fi_executable, True)
            If IsNothing(md5_exec) Then
                GlobalObject.MsgColored(fatal_err_msg & ". Error code 1.", Color.Red)
                Return False
            End If

            fi_types = New FileInfo(GlobalSetting.ExeFilePath & "TriniDATServerTypes.dll")
            md5_types = GlobalObject.getMD5CheckSum(fi_types, True)
            If IsNothing(md5_types) Then
                GlobalObject.MsgColored(fatal_err_msg & ". Error code 2.", Color.Red)
            End If

            'note: needs manual copy to debug/release paths.
            fi_l = New FileInfo(GlobalSetting.ExeFilePath & "TriniDATServerLicense.dll")
            md5_l = GlobalObject.getMD5CheckSum(fi_l, True)
            If IsNothing(md5_l) Then
                GlobalObject.MsgColored(fatal_err_msg & ". Error code 1.", Color.Red)
                Return False
            End If

            'Calc MD5 checksum for current executable
            'Calc MD5 checksum for TriniDAT Server Types.
            'Request: md5exec, md5types, windows serialnumber, <remote server ip address>.
            'Response:
            'If update required, the server will reply with UPDATE. a ZIP file with a structure relative to executable base path.
            'If OK. the server will send GID string: A-B-C. The X-Server-Date month has a incorrect value. According the server's incorrect month the GID part is discerned and the last char is the license type while the rest are random numbers. GID: AA-BB-CC-DD-FF.  
            'If invalid serial number, the server will repy with INVALID.
            w = GlobalObject.getServerCertificateStr
      
            license_class_obj = GlobalObject.getNewLicenseObject(0)

            If IsNothing(license_class_obj) Then
                Return False
            End If

            current_license_name = license_class_obj.getLicenseName()


            Return doOnlineValidation(license_class_obj.getVerificationURL(w, md5_types, md5_exec, md5_l), md5_types, md5_exec, md5_l)

        End Function

        Public Function doOnlineValidation(ByVal val As String, ByVal types_hash As String, ByVal exec_hash As String, ByVal l_hash As String) As Boolean
            'val = validation URL

            '1. GET URL

            Dim wc As Net.WebClient
            Dim xml_str As String
            Dim xupdater As XElement
            Dim gid As String
            Dim dh_name As String
            Dim date_str As String
            Dim update_install_url As String

            Dim new_exec_md5 As String
            Dim new_types_md5 As String
            Dim new_l_md5 As String

            '<reply><header><gid>5DCMMJ-DMLONT-OMKNMG-OJZIM0-0GRGGJ</gid><installer>http://www.deleeuwict.nl/latest/TriniDATSetup.exe</installer><ip>77.166.216.208</ip></header><checksum><exec>4041DE0E191E4362E4A63585480F99B1</exec><types>859A64989A01F85B0C4556399D87759F</types><l>7DF1B4FF235D28DF06C1B846C78A8EF3</l></checksum></reply>

            dh_name = "X-Object-Sync"

            Try
                wc = New Net.WebClient
                xml_str = wc.DownloadString(val)
                date_str = wc.ResponseHeaders(dh_name)
                xupdater = XElement.Parse(xml_str)

                Dim xheader = From node In xupdater.Descendants("header")
                Dim xgid = xheader.Descendants("gid")

                Dim xsetup = xheader.Descendants("setupurl")


                If xsetup.Count = 1 Then
                    GlobalSetting.Latest_SetupURL = xsetup(0).Value
                Else
                    GlobalSetting.Latest_SetupURL = "(none)"
                End If

                If Not xgid.Count = 1 Then
                    Throw New Exception("Server returned invalid response (gid).")
                Else
                    gid = xgid(0).Value
                End If

                Dim xchecksum = From node In xupdater.Descendants("checksum")

                If Not xchecksum.Count = 1 Then
                    Throw New Exception("Server returned invalid response (checksum).")
                End If

                Dim xhash_exec = xchecksum.Descendants("exec")

                If Not xhash_exec.Count = 1 Then
                    Throw New Exception("Server returned invalid response (executable).")
                Else
                    'server verison.
                    new_exec_md5 = xhash_exec(0).Value

                    Dim old_exec_md5 As String
                    Dim xinstaller = xheader.Descendants("execlocation")

                    update_install_url = Nothing

                    If Not xinstaller.Count = 1 Then
                        Throw New Exception("Server returned invalid response (types).")
                    Else
                        update_install_url = xinstaller(0).Value
                    End If

                    'my version.
                    old_exec_md5 = GlobalSetting.ExecMD5

                    'debug
                    'If (1 = 1) Or (Not GlobalObject.userIsGJ And (old_exec_md5 <> new_exec_md5)) Then
                    If Not GlobalObject.userIsGJ And (old_exec_md5 <> new_exec_md5) Then


                        If Not IsNothing(new_exec_md5) Then

                            If MessageBox.Show("TriniDAT Data Application server needs to update. click OK to continue.", "Update Installation", MessageBoxButton.OKCancel, MessageBoxImage.Question) = DialogResult.OK Then
                                new_exec_md5 = new_exec_md5
                                'GlobalObject.OpenURL(update_install_url)

                                Dim update_exepath As FileInfo
                                Dim update_endpoint As String
                                'debug
                                'update_endpoint = New FileInfo(GlobalSetting.ExeFilePath).DirectoryName & "\mydest.exe"
                                update_endpoint = Assembly.GetExecutingAssembly().Location

                                update_exepath = GlobalObject.generateUpdateDownloader(update_install_url, New FileInfo(GlobalSetting.ExeFilePath).DirectoryName & "\trinidatupdate.exe", 5000, update_endpoint)

                                If Not IsNothing(update_exepath) Then
                                    'ExecuteFileWExitCode
                                    Try
                                        GlobalObject.ExecuteFile(update_exepath.FullName, "", False, ProcessWindowStyle.Minimized)
                                        End

                                    Catch ex As Exception
                                        MessageBox.Show("Failed to execute update utility.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                                        MsgBox(ex.Message)
                                    End Try
                                Else
                                    MessageBox.Show("Update failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)
                                End If

                                End
                            End If
                        End If

                    End If
                End If

                Dim xhash_types = xchecksum.Descendants("types")

                If Not xhash_types.Count = 1 Then
                    Throw New Exception("Server returned invalid response (types).")
                Else
                    new_types_md5 = xhash_types(0).Value
                End If

                Dim xhash_l = xchecksum.Descendants("l")

                If Not xhash_l.Count = 1 Then
                    Throw New Exception("Server returned invalid response (li).")
                Else
                    new_l_md5 = xhash_l(0).Value
                End If


                'store other metadata 
                '=============
                'Appstore URLS.
                Dim xappstore = From node In xupdater.Descendants("appstore")

                If Not xappstore.Count = 1 Then
                    Throw New Exception("Server returned invalid response (appstore).")
                End If

                GlobalSetting.AppStore_UploadURL = xappstore.Descendants("upload")(0).Value


                'core app url: serverindex.
                Dim xserverindex_app = From node In xheader.Descendants("indexapp")

                If Not xserverindex_app.Count = 1 Then
                    Throw New Exception("Server returned invalid response (indexapp).")
                End If

                GlobalSetting.CoreApp_ServerIndexURL = xheader.Descendants("indexapp")(0).Value

                '3. GID verification.
                Dim lid As Integer

                lid = getLTBSD(date_str, gid)
                If lid < 0 Or lid > 4 Then
                    'hacked
                    Return False
                End If

                'set license.
                GlobalObject.OfficialLicense = GlobalObject.getNewLicenseObject(CType(lid, TriniDATServerTypes.TRINIDAT_SERVER_LICENSE))

                If Not GlobalObject.foundLicense Then
                    'hacker
                    Return False
                End If

                'match configured thread count against actual server license type.
                If GlobalObject.OfficialLicense.Verify Then
                    Return True
                Else
                    MsgBox("Version mismatch. Please renew your TriniDAT installation with the currently available online setup. ")
                    MsgBox("Will quit now.")
                    End
                    Return False
                End If

            Catch ex As Exception
                Return False
            End Try

            Return False

        End Function

        Public Function OnReflectionOnlyResolve(ByVal sender As Object, ByVal args As System.ResolveEventArgs) As System.Reflection.Assembly

            Dim loaded_asm As Assembly
            Dim all_asms As List(Of Assembly)

            all_asms = New List(Of Assembly)(AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies())
            all_asms.AddRange(AppDomain.CurrentDomain.GetAssemblies())

            For Each loaded_asm In all_asms

                If loaded_asm.FullName = args.Name Then
                    If Not loaded_asm.ReflectionOnly Then
                        'load as reflection only
                        Return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(loaded_asm.Location))
                    Else
                        Return loaded_asm
                    End If

                End If

            Next

            Throw New Exception("Error: Unable to find assembly ' " & args.Name & "'.")

        End Function
        Public Shared Sub Main()

            Dim c As TriniDAT_Core

            c = New TriniDAT_Core

            c.Run()

        End Sub

        Private Sub TriniDAT_Core_LoadCompleted(ByVal sender As Object, ByVal e As System.Windows.Navigation.NavigationEventArgs) Handles Me.LoadCompleted
            
        End Sub


        Private Sub onStarted() Handles Me.Startup

            Dim splash_showtime_sec As Integer
            Dim splash_start_ms As Long
            Dim splash As frmSplash



            splash_start_ms = GlobalObject.GetTickCount()
            splash_showtime_sec = 1
            splash = New frmSplash
            splash.Show()



            If Not GlobalSetting.Load() Then
                MsgBox("Fatal error")
                End
            End If

            Do Until CInt((GlobalObject.GetTickCount() - splash_start_ms) / 1000) > splash_showtime_sec
                System.Threading.Thread.Sleep(100)
            Loop

            splash.Close()
            splash.Dispose()

            Call OnSplashFormClose()

        End Sub

        Private Sub OnSplashFormClose()

            'INIT
            If Not Me.validateInstallation Then
                'illegal versions would have quit by now.
                MsgBox("Updating failure. TriniDAT Server may not work correct. Restart the application to try again.")

                'make sure the default version is the free edition.
                If Not GlobalObject.foundLicense Then
                    GlobalObject.OfficialLicense = GlobalObject.getNewLicenseObject(TriniDATServerTypes.TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE)
                End If
            End If

            If Not GlobalMappingPointASMCollection.Initialize() Then
                MsgBox("Out of memory.")
                End
            End If

            Me.MainForm = New frmServerMain
            GlobalObject.serverForm = Me.MainForm
            GlobalObject.serverForm.ShowDialog()

QUIT:
            Try

                If GlobalObject.haveActionPasserthread Then
                    Try
                        GlobalObject.ActionPasserThread.Abort()
                    Finally
                        If GlobalObject.haveHiddenExecutionForm Then
                            Try
                                GlobalObject.HiddenExecutionForm.Dispose()
                            Finally
                                GlobalObject.HiddenExecutionForm = Nothing
                            End Try
                        End If
                    End Try
                End If

                'Stop server
                If GlobalObject.haveServerThread Then
                    If GlobalObject.server.Running() Then
                        GlobalObject.server.stopServer()
                    End If
                End If

                'invalidate server form
                GlobalObject.serverForm = Nothing

                If Not IsNothing(MainForm) Then

                    'delete console
                    If MainForm.haveWebbrowserForm() Then
                        If MainForm.WakeUpWBThread() Then
                            Call MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.KillFormThreaded)
                        End If
                    End If
                End If

                If GlobalObject.haveSimon Then
                    'disconnect open debug sockets
                    Call GlobalObject.simon.EraseAllFrames()
                End If

            Catch ex As Exception
                Debug.Print("Application exit msg: " & ex.Message)
            End Try

        End Sub

        Private Sub OnAppActivated() Handles MyBase.Activated
            Dim x As Integer
            Dim f

            If Not GlobalObject.haveServerForm Then Exit Sub

            f = System.Windows.Forms.Form.ActiveForm


            If MainForm.haveWebbrowser() Then
                '        If MainForm.threaded_webbrowser_thread.ThreadState = ThreadState.Running Then


                If Not IsNothing(f) Then
                    If f.name = "frmServerMain" Then
                        '  MainForm.Text = Now.ToString & " activatehandler  activeform: server   web has focus? " & MainForm.wb_focused.ToString

                        If MainForm.haveWebbrowserForm() Then
                            If MainForm.tabCsl.SelectedIndex = MainForm.getRootWebbrowserTabIndex() Then
                                Call MainForm.Invoke(MainForm.doDisableFormTabWinProcThreaded)
                                MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.enableStayOnTopThreaded)
                                Call MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.ShowNowThreaded)
                                MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.disableStayOnTopThreaded)
                                Call MainForm.Invoke(MainForm.doEnableFormTabWinProcThreaded)
                                '  MainForm.wb_focused = True

                            End If
                        End If
                    End If
                End If
            End If

        End Sub
        Private Sub OnLeave() Handles MyBase.Deactivated
            Dim x As Integer
            Dim f
            Dim active_form As Integer

            f = System.Windows.Forms.Form.ActiveForm

            active_form = 0

            If Not IsNothing(f) Then

                If f.name = "frmThreadedConsoleWB" Then
                    '  MainForm.Text = Now.ToString & " deactivatehandler activeform: webb   web has focus? " & MainForm.wb_focused.ToString
                    active_form = 1
                ElseIf f.name = "frmServerMain" Then
                    '  MainForm.Text = Now.ToString & " deactivatehandler  activeform: server   web has focus? " & MainForm.wb_focused.ToString
                    active_form = 0
                End If
            End If

            If GlobalObject.haveServerForm Then

                If GlobalObject.haveServerthread Then
                    'DESIGN FOR PRODUCTION MODE
                    '=======================

                    'make text appear white 
                    If GlobalObject.server.ServerMode = TriniDATServerTypes.TRINIDAT_SERVERMODE.MODE_LIVE Then
                        MainForm.txtServerActivity.ForeColor = Color.White
                    End If

                End If



                If MainForm.haveWebbrowser() Then
                    'if no form yet then its being created 
                    If Not MainForm.haveWebbrowserForm() Then
                        MainForm.BringToFront()
                    Else
                        If MainForm.threaded_webbrowser_thread.ThreadState = ThreadState.Running Then

                            If active_form = 0 And Not MainForm.wb_focused Then
                                MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.disableStayOnTopThreaded)
                                MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.HideNowThreaded)
                                MainForm.wb_focused = False
                            ElseIf active_form = 1 And MainForm.wb_focused Then
                                '    Call MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.HideNowThreaded)
                                MainForm.threaded_webbrowser_form.Invoke(MainForm.threaded_webbrowser_form.disableStayOnTopThreaded)
                                MainForm.wb_focused = False
                            End If
                        End If
                    End If
                End If
            End If
        End Sub

    End Class

End Namespace
