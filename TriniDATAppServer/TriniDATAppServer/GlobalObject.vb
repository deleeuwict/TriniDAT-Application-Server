Imports System.Threading
Imports System.IO
Imports System.Security.Cryptography
Imports System.Reflection
Imports TriniDATDictionaries
Imports TriniDATServerTypes

Public Class GlobalObject

    Public Shared simon As SimonGlobalCommandHandler
    Public Shared serverForm As frmServerMain
    Public Shared server As TriniDATServer
    Public Shared server_thread As Thread
    Private Shared default_httpserver_config As BosswaveTCPServerConfig
    Private Shared apps As BosswaveAppCache
    'Public Declare Function GetTickCount Lib "Kernel32" () As Long
    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As Integer, ByVal wMsg As Integer, ByVal wParam As Integer, ByRef lParam As Integer) As Integer
    Public Shared id_counter As Long
    Private Const CB_SHOWDROPDOWN = &H14F
    Public Const EM_SCROLL = &HB5
    Public Shared CONSOLE_DEFAULT_COLOR As Color = Color.DarkViolet
    Private Shared NEXT_DEBUG_OBJECTID As Integer = -1
    Private Shared license_mod As Assembly
    Private Shared current_license_obj As Object
    Private Shared upload_dialog As frmUploadPackage
    Private Shared pub_dialog As frmSellPackageWizard
    Private Shared exec_form As HiddenActionExecutor
    Public Shared exec_form_thread As Thread
    Private Shared hidden_process_execution_list As TriniDATWordDictionary
    Private Shared current_action_index As Long
    Private Shared action_passing_thrd As Thread
    Private Shared is_first_run As Boolean
    Private Shared first_hidden_execaction As Boolean
    Public Shared Property FirstHiddenAction As Boolean
        Get
            Return GlobalObject.first_hidden_execaction
        End Get
        Set(ByVal value As Boolean)
            GlobalObject.first_hidden_execaction = value
        End Set
    End Property
    Public Shared Property FirstRun As Boolean
        Get
            Return GlobalObject.is_first_run
        End Get
        Set(ByVal value As Boolean)
            GlobalObject.is_first_run = value
        End Set
    End Property
    Public Shared ReadOnly Property haveActionPasserthread As Boolean
        Get
            If Not IsNothing(GlobalObject.action_passing_thrd) Then
                Return (GlobalObject.action_passing_thrd.ThreadState = ThreadState.Running Or GlobalObject.action_passing_thrd.ThreadState = ThreadState.Background)
            Else
                Return False
            End If
        End Get
    End Property
    Public Shared Property ActionPasserThread As Thread
        Get
            Return GlobalObject.action_passing_thrd
        End Get
        Set(ByVal value As Thread)
            GlobalObject.action_passing_thrd = value
        End Set
    End Property
    Public Shared Function userIsGJ() As Boolean
    	Console.WriteLine(GlobalSetting.getWindowsSerial())
        Return GlobalSetting.getWindowsSerial() = "55677-OEM-0011903-00117"
    End Function
    Public Shared Function GetTickCount() As Long
        Return System.Environment.TickCount
    End Function
    Public Shared ReadOnly Property haveHiddenExecutionForm As Boolean
        Get
            Return Not IsNothing(GlobalObject.exec_form)
        End Get
    End Property
    Public Shared ReadOnly Property haveHiddenActionExecutableList() As Boolean
        Get
            Return Not IsNothing(GlobalObject.hidden_process_execution_list)
        End Get
    End Property
    Public Shared ReadOnly Property HiddenActionExecutableList() As TriniDATWordDictionary
        Get
            Return GlobalObject.hidden_process_execution_list
        End Get
    End Property
    Public Shared Function resetActionProcessesExecList() As Boolean
        Try
            GlobalObject.hidden_process_execution_list = New TriniDATWordDictionary("", Nothing)
            GlobalObject.current_action_index = 0
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    Public Shared Property HiddenExecutionForm() As HiddenActionExecutor
        Get
            Return GlobalObject.exec_form
        End Get
        Set(ByVal value As HiddenActionExecutor)
            GlobalObject.exec_form = value

        End Set
    End Property
    Public Shared Sub AddAction(ByVal exec_file As String, ByVal params As String)
        GlobalObject.hidden_process_execution_list.Add(exec_file & "=" & params)
    End Sub
    Public Shared ReadOnly Property CurrentHiddenAction As String
        Get
            If GlobalObject.haveHiddenActionExecutableList Then
                If Not GlobalObject.HiddenActionExecutableList.haveContents Then
                    'completed.
                    Return Nothing
                End If

                Return GlobalObject.HiddenActionExecutableList.getWordList().Item(GlobalObject.current_action_index)
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public Shared Function ExecuteNextAction() As Boolean

        If Not GlobalObject.haveHiddenActionExecutableList Then
            Return False
        End If

    
        If GlobalObject.FirstHiddenAction Then
            GlobalObject.FirstHiddenAction = False
            GlobalObject.ActionPasserThread = New Thread(AddressOf GlobalObject.ExecActionThread)
            GlobalObject.ActionPasserThread.Start()
            Thread.CurrentThread.Sleep(1000)
        End If

        Return True
    End Function
    Private Shared Sub ExecActionThread()
        Dim thread_is_waiting As Boolean
        Dim current_exec As String
        Dim Params() As String
        Dim process_list_looped As Boolean

EXEC_AGAIN:
        If Not GlobalObject.haveServerThread Then
            GoTo KILL_THREAD
        End If
        thread_is_waiting = False

        current_exec = GlobalObject.CurrentHiddenAction 'depends on  GlobalObject.current_action_index

        If IsNothing(current_exec) Then
            GoTo KILL_THREAD
            Exit Sub
        Else
            Params = Split(current_exec, "=")
        End If

        Do Until CType(GlobalObject.HiddenExecutionForm.Invoke(GlobalObject.HiddenExecutionForm.isFreeThreaded), Boolean) = True

            If thread_is_waiting Then
                Thread.CurrentThread.Sleep(300)
            End If

            If Not GlobalObject.haveHiddenExecutionForm Then
                Exit Sub
            End If

            thread_is_waiting = True
        Loop

        If GlobalObject.haveServerThread Then
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                GlobalObject.MsgColored("Executing pending action(s)..", Color.Green)
            End If
        End If

        current_exec = GlobalObject.CurrentHiddenAction
        GlobalObject.HiddenExecutionForm.Invoke(GlobalObject.HiddenExecutionForm.ExecFileThreaded, {params})

        GlobalObject.current_action_index = GlobalObject.current_action_index + 1

        process_list_looped = Not GlobalObject.HiddenActionExecutableList.haveContents

        If process_list_looped = False Then
            process_list_looped = (GlobalObject.current_action_index = GlobalObject.HiddenActionExecutableList.getWordList().Count - 1)
        End If

        If Not process_list_looped Then
            Thread.CurrentThread.Sleep(400)
            GoTo EXEC_AGAIN
        End If

KILL_THREAD:
        'reset.
        GlobalObject.FirstHiddenAction = True
        GlobalObject.resetActionProcessesExecList()
        Threading.Thread.CurrentThread.Abort()
    End Sub
    Public Shared Sub RunHiddenActionForm()

        Try
            System.Windows.Forms.Application.Run(GlobalObject.HiddenExecutionForm)

        Catch ex As Exception
            GlobalObject.MsgColored("Error loading hidden action executor: " & ex.Message, Color.DarkOrange)

        End Try

    End Sub
    Public Shared ReadOnly Property haveAppPublisherForm As Boolean
        Get
            Return Not IsNothing(GlobalObject.pub_dialog)
        End Get
    End Property
    Public Shared Property CurrentAppPublisherForm As frmSellPackageWizard
        Get
            Return GlobalObject.pub_dialog
        End Get
        Set(ByVal value As frmSellPackageWizard)
            GlobalObject.pub_dialog = value
        End Set
    End Property

    Public Shared ReadOnly Property haveUploadProgressForm As Boolean
        Get
            Return Not IsNothing(GlobalObject.CurrentUploadProgressForm)
        End Get
    End Property
    Public Shared Property CurrentUploadProgressForm As frmUploadPackage
        Get
            Return GlobalObject.upload_dialog
        End Get
        Set(ByVal value As frmUploadPackage)
            GlobalObject.upload_dialog = value
        End Set
    End Property
    Public Shared ReadOnly Property foundLicense As Boolean
        Get
            If Assembly.GetCallingAssembly().Location = Assembly.GetExecutingAssembly().Location Then
                Return Not IsNothing(GlobalObject.current_license_obj)
            Else
                'hacker.
                End
                Return Nothing
            End If
        End Get
    End Property
    Public Shared Property OfficialLicense As Object
        Get
            Return GlobalObject.current_license_obj
        End Get
        Set(ByVal value As Object)
            'do not accept set calls from external modules.
            If Assembly.GetCallingAssembly().Location = Assembly.GetExecutingAssembly().Location Then
                GlobalObject.current_license_obj = value
            Else
                'hacker.
                End
            End If
        End Set
    End Property
    Public Shared Function getNewLicenseObject(ByVal licensing_model As TriniDATServerTypes.TRINIDAT_SERVER_LICENSE) As Object
        If Not GlobalObject.licenseModuleLoaded Then
            GlobalObject.license_mod = GlobalObject.loadLicenseModule
        End If

        If CType(licensing_model, Integer) < 0 Or CType(licensing_model, Integer) > 4 Then
            'something debuggin mah' shit
            'format C:\
            Return Nothing
        End If

        Return GlobalObject.licenseNewClassInstance(licensing_model)

    End Function
    Private Shared Function loadLicenseModule() As Assembly

        Dim licenseFilePathStr As String
        Dim licenseFilePath As FileInfo
        Dim licenseDLLFilename As String = "TriniDATServerLicense.dll"
        Dim licenseModule As Assembly

        licenseFilePathStr = GlobalSetting.ExeFilePath


        If GlobalObject.userIsGJ Then
            licenseFilePathStr = "C:\Documents and Settings\DaG\Mijn documenten\Visual Studio 2010\Projects\TriniDATAppServer\TriniDATServerLicense\TriniDATServerLicense\bin\Release\"
        End If

        licenseFilePathStr &= licenseDLLFilename
        licenseFilePath = New FileInfo(licenseFilePathStr)

        Try

            If Not licenseFilePath.Exists() Then
                Throw New Exception("License file missing.")
            End If

            'load DLL
            licenseModule = Assembly.LoadFile(licenseFilePath.FullName)

            Return licenseModule

        Catch ex As Exception
            Debug.Print("License err: " & ex.Message)
            Return Nothing
        End Try

    End Function
    Public Shared ReadOnly Property licenseNewClassInstance(ByVal current_license As TriniDATServerTypes.TRINIDAT_SERVER_LICENSE) As Object
        Get
            If Not GlobalObject.licenseModuleLoaded Then
                Throw New Exception("DIE")
                Return Nothing
            End If

            Dim retvaltype As Type
            Dim retval As Object

            retvaltype = GlobalObject.licenseModule.GetType("TriniDATServerLicense.TriniDATServerLicense")

            If IsNothing(retvaltype) Then
                Return Nothing
            End If

            'create instance
            retval = Activator.CreateInstance(retvaltype, {current_license})

            Return retval
        End Get
    End Property
    Public Shared ReadOnly Property licenseModuleLoaded() As Boolean
        Get

            Return Not IsNothing(GlobalObject.license_mod)
        End Get
    End Property

    Public Shared ReadOnly Property licenseModule() As Assembly
        Get
            Return GlobalObject.license_mod
        End Get
    End Property
    Public Shared Function nextDebuggingObjectId() As Long
        GlobalObject.NEXT_DEBUG_OBJECTID -= 1
        Return GlobalObject.NEXT_DEBUG_OBJECTID
    End Function
    Public Shared Sub DropDownConditionList(ByVal cb As ComboBox)

        'Drop the list
        Call GlobalObject.SendMessage(cb.Handle, CB_SHOWDROPDOWN, True, 0)
    End Sub

    Public Shared Function getMD5CheckSum(ByVal val As FileInfo, ByVal remove_dashes As Boolean) As String

        If Not val.Exists Then Return Nothing

        Dim fs As FileStream
        Dim file_checksum() As Byte
        Dim md5_gen As MD5
        Dim retval As String

        md5_gen = MD5.Create

        Try

            fs = New FileStream(val.FullName, FileMode.Open, FileAccess.Read)

            file_checksum = md5_gen.ComputeHash(fs)

            If IsNothing(file_checksum) Then
                Throw New Exception("Invalid hash generated.")
            End If


        Catch ex As Exception
            GlobalObject.MsgColored("Checksum error: " & ex.Message, Color.Red)
            GlobalObject.MsgColored("Error generating MD5 checksum for '" & val.FullName & "'. Update failure.", Color.Red)
            Return Nothing
        End Try


        If Not remove_dashes Then
            Return BitConverter.ToString(file_checksum)
        Else
            retval = BitConverter.ToString(file_checksum)
            retval = Replace(retval, "-", "")
            Return retval
        End If

    End Function

    Public Shared Sub CloseDropDownConditionList(ByVal cb As ComboBox)

        'Close List
        Call GlobalObject.SendMessage(cb.Handle, CB_SHOWDROPDOWN, False, 0)
    End Sub

    Public Shared Function getCopyrightString() As String
        Return "(C) " & Year(Now.ToString) & " GertJan de Leeuw. De Leeuw ICT."
    End Function
    Public Shared Function OpenURL(ByVal val As String) As Boolean


        Dim othread As Thread
        othread = New Thread(AddressOf OpenURLThreaded)
        othread.Start(val)

        Return True
    End Function
    Private Shared Sub OpenURLThreaded(ByVal val As String)

        Dim oPro As New Process

        Try

            With oPro
                .StartInfo.UseShellExecute = True
                .StartInfo.Arguments = ""
                .StartInfo.FileName = "iexplore.exe"
                .StartInfo.Arguments = val
                .Start()
            End With

        Catch ex As Exception
            GlobalObject.MsgColored("Error opening  " & val & " : " & ex.Message, Color.DarkRed)
        End Try

    End Sub
    Public Shared ReadOnly Property haveSimon As Boolean
        Get
            Return Not IsNothing(GlobalObject.simon)
        End Get
    End Property

    Public Shared ReadOnly Property NextId() As Long
        Get
            GlobalObject.id_counter = GlobalObject.id_counter + 1
            Return GlobalObject.id_counter
        End Get
    End Property

    Public Shared ReadOnly Property haveApplicationCache As Boolean
        Get
            Return Not IsNothing(GlobalObject.ApplicationCache)
        End Get
    End Property
    Public Shared Property ApplicationCache() As BosswaveAppCache
        Get
            Return GlobalObject.apps
        End Get
        Set(ByVal value As BosswaveAppCache)
            GlobalObject.apps = value
        End Set
    End Property
    Public Shared Sub Msg(ByVal txt As String)
        If GlobalObject.haveServerThread Then
            If GlobalObject.haveServerForm Then
                serverForm.Invoke(GlobalObject.serverForm.serverLogThrd, New Object() {txt})
            End If
        End If
    End Sub
    Public Shared Sub MsgNewLine()
        If Not IsNothing(serverForm) Then
            serverForm.Invoke(GlobalObject.serverForm.serverLogNewLinethrd)
        End If
    End Sub
    Public Shared Function getVersionShort() As String
        Return Replace(GlobalObject.getVersionNo(), ",", ".")
    End Function
    Public Shared Function getVersionString() As String
        Return "version " & Replace(GlobalObject.getVersionNo(), ",", ".")
    End Function
    Public Shared ReadOnly Property getServerCertificateStr() As String
        Get
            Dim w As String

            w = GlobalSetting.getWindowsSerial
            w = Replace(w, "OEM", "DOM")
            w = Replace(w, "-", "")
            Return w
        End Get
    End Property
    Public Shared Function getVersionNo() As Double
        Return 1.02
    End Function
    Public Shared Function generateUpdateDownloader(ByVal update_url As String, ByVal downloadFilePath As String, ByVal waitTimeMS As Long, ByVal filecopyTarget As String) As FileInfo
        'generate EXE file to download file in update_url. 
        Dim retval As FileInfo

        Dim src_code As String
        Dim output_exe_path As String

        'write HTTP fetcher source-code.
        src_code = "Imports System" & vbNewLine
        src_code &= "Imports System.Net" & vbNewLine
        src_code &= "Imports System.IO" & vbNewLine
        src_code &= "Imports System.Threading" & vbNewLine
        src_code &= "Imports System.Diagnostics" & vbNewLine & vbNewLine

        src_code &= "Module Module1" & vbNewLine

        src_code &= "Function Main() as Integer" & vbNewLine

        src_code &= "Dim wc As WebClient" & vbNewLine
        src_code &= "Dim URL as String" & vbNewLine
        src_code &= "Dim targetfile as String" & vbNewLine
        src_code &= "Dim fdest as String" & vbNewLine

        src_code &= "" & vbNewLine
        src_code &= "fdest = '" & filecopyTarget & "'" & vbNewLine
        src_code &= "wc = New WebClient" & vbNewLine
        src_code &= "URL = '$TARGETURL' " & vbNewLine
        src_code &= "targetfile ='$TARGETFILE'" & vbNewLine
        src_code &= vbNewLine
        src_code &= "   if file.exists(targetfile) then " & vbNewLine
        src_code &= "       Try" & vbNewLine
        src_code &= "           File.Delete(targetfile)" & vbNewLine
        src_code &= "       Catch ex As Exception" & vbNewLine
        src_code &= "           Console.WriteLine(ex.message)" & vbNewLine
        src_code &= "       End Try" & vbNewLine
        src_code &= "   end if" & vbNewLine
        src_code &= vbNewLine
        src_code &= "Try" & vbNewLine

        src_code &= "" & vbNewLine
        src_code &= "   Console.WriteLine( 'Downloading TriniDAT Data Application Server update...' )" & vbNewLine
        src_code &= "   wc.DownloadFile(URL,targetfile)" & vbNewLine
        src_code &= "Console.WriteLine( 'Download complete.' )" & vbNewLine

        src_code &= "" & vbNewLine

        src_code &= "Catch ex As Exception" & vbNewLine
        src_code &= "   Console.WriteLine(ex.message)" & vbNewLine
        src_code &= "   return -1" & vbNewLine
        src_code &= "End Try" & vbNewLine

        src_code &= "if file.exists(targetfile) then " & vbNewLine
        src_code &= "   Console.WriteLine( 'Preparing copy...' )" & vbNewLine
        src_code &= "   Thread.Sleep(" & waitTimeMS & ")" & vbNewLine
        src_code &= "       Try" & vbNewLine
        src_code &= "        Console.WriteLine('copying update...')" & vbNewLine
        src_code &= "            if file.exists(fdest) then " & vbNewLine
        src_code &= "                 Try" & vbNewLine
        src_code &= "                   File.Delete(fdest)" & vbNewLine
        src_code &= "                Catch ex As Exception" & vbNewLine
        src_code &= "                   Console.WriteLine(ex.message)" & vbNewLine
        src_code &= "                  End Try" & vbNewLine
        src_code &= "           end if" & vbNewLine
        src_code &= vbNewLine
        src_code &= "         System.IO.File.copy(targetfile,fdest)" & vbNewLine
        src_code &= "           Console.WriteLine('Trigger: ' & fdest)" & vbNewLine
        src_code &= "         Process.Start(fdest)" & vbNewLine
        src_code &= "       Catch ex As Exception" & vbNewLine
        src_code &= "           Console.WriteLine('file copy error: ' & ex.message)" & vbNewLine
        src_code &= "           return -3" & vbNewLine
        src_code &= "       End Try" & vbNewLine
        src_code &= "return 2" & vbNewLine
        src_code &= "else" & vbNewLine
        src_code &= "return -2" & vbNewLine
        src_code &= "end if" & vbNewLine
        src_code &= "End Function" & vbNewLine
        src_code &= "End Module" & vbNewLine

        src_code = Replace(src_code, "$TARGETURL", update_url)
        src_code = Replace(src_code, "$TARGETFILE", downloadFilePath)
        src_code = Replace(src_code, "'", Chr(34))

        'example:  CompileCode(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, "<srccode>", output_exe_file)

        output_exe_path = GlobalSetting.getTempDir() & "tdatupdate" & TriniDATUserSession.generateNewSessionId() & ".exe"

        If SimpleCodeCompiler.VBSourceCodeCompiler(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, src_code, output_exe_path) Then
            'Call GlobalObject.ExecuteFile(output_exe_path)
            retval = New FileInfo(output_exe_path)
            If retval.Exists() Then
                Return retval
            End If
        End If

        Return Nothing
    End Function

    Public Shared Sub MsgColored(ByVal txt As String, ByVal clr As Color)
        If Not IsNothing(serverForm) Then

            If GlobalObject.haveServerThread Then
                If GlobalObject.server.ServerMode = TriniDATServerTypes.TRINIDAT_SERVERMODE.MODE_LIVE Then
                    'disable coloring in production mode.
                    serverForm.Invoke(GlobalObject.serverForm.serverLogThrd, New Object() {txt})
                Else
                    serverForm.Invoke(GlobalObject.serverForm.serverColoredLogThrd, New Object() {txt, clr})
                End If
            Else
                serverForm.Invoke(GlobalObject.serverForm.serverColoredLogThrd, New Object() {txt, clr})
            End If


        End If
    End Sub
    Public Shared Sub MsgColoredNonSpaced(ByVal txt As String, ByVal clr As Color)
        If Not IsNothing(serverForm) Then

            If GlobalObject.haveServerThread Then
                If GlobalObject.server.ServerMode = TriniDATServerTypes.TRINIDAT_SERVERMODE.MODE_LIVE Then
                    'disable coloring in production mode.
                    serverForm.Invoke(GlobalObject.serverForm.serverLogThrd, New Object() {txt})
                Else
                    serverForm.Invoke(GlobalObject.serverForm.serverColoredNonSpacedLogThrd, New Object() {txt, clr})
                End If
            Else
                serverForm.Invoke(GlobalObject.serverForm.serverColoredNonSpacedLogThrd, New Object() {txt, clr})
            End If


        End If
    End Sub
    Public Shared ReadOnly Property haveServerThread() As Boolean
        Get
            Return Not IsNothing(GlobalObject.server_thread)
        End Get
    End Property
    Public Shared ReadOnly Property haveServerForm() As Boolean
        Get
            Return Not IsNothing(GlobalObject.serverForm)
        End Get
    End Property
    Public Shared Function startServerThread(ByVal mode As TriniDATServerTypes.TRINIDAT_SERVERMODE) As Boolean
        Try

            'start server thread.
            GlobalObject.server = New TriniDATServer(mode)

            GlobalObject.server_thread = New Thread(AddressOf GlobalObject.server.Listener)
            GlobalObject.server_thread.Start()

            Return True
        Catch ex As Exception

        End Try
        Return False
    End Function

    Public Shared ReadOnly Property serverState As BosswaveServerState
        Get
            Try

                If Not haveServerThread Then
                    Return BosswaveServerState.OFFLINE
                End If

                If GlobalObject.server.Running Then
                    Return BosswaveServerState.ONLINE
                End If

                Return BosswaveServerState.OFFLINE

            Catch ex As Exception

            End Try
        End Get
    End Property

    Public Shared Function TestTCPServerConnection(ByVal interface_ip As String, ByVal port As Integer) As Boolean

        Dim serversocket As Net.Sockets.TcpListener

        Try
            serversocket = New Net.Sockets.TcpListener(Net.IPAddress.Parse(interface_ip), port)
            serversocket.Start()

        Catch ex As Exception
            Return False
        End Try

        serversocket.Stop()

        Return True
    End Function
    Public Shared Function ExecuteFileWExitCode(ByVal val As String, Optional ByVal arg As String = "") As Integer

        Try
            '
            Dim oPro As New Process
            With oPro

                .StartInfo.UseShellExecute = True
                .StartInfo.Arguments = arg
                .StartInfo.FileName = val
                .Start()
                .WaitForExit()
            End With

            Return oPro.ExitCode
        Catch ex As Exception
            GlobalObject.MsgColored("Error executing '" & val & "': " & ex.Message, Color.Red)
            Return Nothing
        End Try
    End Function
    Public Shared Function ExecuteFile(ByVal val As String, Optional ByVal arg As String = "", Optional ByVal wait As Boolean = False, Optional ByVal window_style As System.Diagnostics.ProcessWindowStyle = ProcessWindowStyle.Normal) As Boolean

        Try
            '
            Dim oPro As New Process
            With oPro

                .StartInfo.UseShellExecute = True
                .StartInfo.Arguments = arg
                .StartInfo.FileName = val
                .StartInfo.WindowStyle = window_style
                .Start()

                If wait Then
                    .WaitForExit()
                End If

            End With

            Return True
        Catch ex As Exception
            GlobalObject.MsgColored("Error executing '" & val & "': " & ex.Message, Color.Red)
            Return False
        End Try
    End Function

    Public Shared Property CurrentServerConfiguration As BosswaveTCPServerConfig
        Get
            Return GlobalObject.default_httpserver_config
        End Get
        Set(ByVal value As BosswaveTCPServerConfig)
            GlobalObject.default_httpserver_config = value

        End Set
    End Property

    Public Shared ReadOnly Property getMappingPointDescriptorBySessionId(ByVal appid As String, ByVal mapping_point_url As String, ByVal sessionid As String) As mapping_point_descriptor
        Get
            'called by threads that want to access their info
            Dim retval As mapping_point_descriptor
            Dim related_session_obj As TriniDATUserSession

            If Not GlobalObject.haveServerThread Then Return Nothing
            If GlobalObject.serverState <> BosswaveServerState.ONLINE Then Return Nothing

            'ask the active server for the application thru sessionid

            related_session_obj = GlobalObject.server.Users.getSessionById(sessionid)

            If IsNothing(related_session_obj) Then
                GlobalObject.Msg("Unable to find threadinfo by session " & sessionid)
            End If

            Dim app As BosswaveApplication

            app = related_session_obj.Application(appid)

            If Not IsNothing(app) Then
                Return app.ApplicationMappingPoints.getDescriptorByURL(mapping_point_url)
            End If

            Return Nothing
        End Get
    End Property

End Class

Public Enum BosswaveServerState
    OFFLINE = 1
    ONLINE = 2
End Enum
