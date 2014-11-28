Imports System.IO
Imports TriniDATServerTypes
Imports SimonTypes
Imports System.Threading


Public Class TriniDATServerEvent
    'handles administrator-level server events. 

    Public Shared currentServer As TriniDATServer = Nothing


    Public Shared Sub setServer(ByVal _server As TriniDATServer)
        TriniDATServerEvent.currentServer = _server
    End Sub
    Private Shared Function serverAttached() As Boolean
        Return Not IsNothing(currentServer)
    End Function

    Public Shared Function OnIncomingDebugFrame(ByVal debug_frame As SimonDebugFrame) As Boolean

        If GlobalObject.haveSimon Then
            GlobalObject.MsgColored("Incoming debug object.", Color.LightGreen)

            Return GlobalObject.simon.OnIncomingDebugFrame(debug_frame)
        Else
            GlobalObject.MsgColored("Dropping debug packet.", Color.Red)
            Return False
        End If

    End Function
    Public Shared Sub OnSystemStartup()
        'called in server form GUI thread-context.

        GlobalObject.Msg("TriniDAT Data Server " & GlobalObject.getVersionShort() & ", (c) 2013 De Leeuw ICT. this is copyrighted software. See www.deleeuwict.nl for more information.")
        GlobalObject.Msg("=======================")
        Msg("Starting up from " & GlobalSetting.getRoot() & "...")

        If GlobalObject.haveHiddenExecutionForm Then
            Try
                GlobalObject.HiddenExecutionForm.Dispose()
                If GlobalObject.exec_form_thread.ThreadState = ThreadState.Running Or GlobalObject.exec_form_thread.ThreadState = ThreadState.Background Then
                    GlobalObject.exec_form_thread.Abort()
                End If
                GlobalObject.exec_form_thread = Nothing

            Catch e As Exception
            End Try
        End If

        'Initialize action executor form.
        GlobalObject.HiddenExecutionForm = New HiddenActionExecutor()
        GlobalObject.exec_form_thread = New Thread(AddressOf GlobalObject.RunHiddenActionForm)
        'boot up form.
        GlobalObject.exec_form_thread.Start()

        Msg("load: traffic logger")
        TrafficMonitor.garantueeLogFile()

        Msg("logging from: " & TrafficMonitor.FilePath)

        'Load HTTP server config just to see if it exists
        GlobalObject.CurrentServerConfiguration = GlobalSetting.getHTTPServerConfig()

        If Not GlobalSetting.getHTTPServerConfig().config_file_exists Then
            GlobalObject.FirstRun = True
            Dim pick_ip_frm As PickIPAddress
            pick_ip_frm = New PickIPAddress
            pick_ip_frm.ShowDialog()
        Else
            'set up GUI elements.
            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.txtUserPaymentHandle.Text = GlobalSetting.PayKey

                GlobalObject.serverForm.lblPaymentRegister.Visible = Not GlobalSetting.havePayKey
                GlobalObject.serverForm.lblNoIdInfo.Visible = GlobalObject.serverForm.lblPaymentRegister.Visible
            End If
        End If

    End Sub
    Public Shared Function onServerInstallDefaultApplication() As Boolean
        'load default application

        'Dim boot_app As BosswaveApplication

        'boot_app = BosswaveApplicationIndex.getEntryPoint()
        'BosswaveApplicationHost.HostedApplication = boot_app

        'If Not IsNothing(BosswaveApplicationHost.HostedApplication) Then
        '    Msg("successfully configured application '" & BosswaveApplicationHost.HostedApplication.ApplicationName & "'")
        'Else
        '    Msg("Error loading default application. Server is NOT running.")
        'End If

        'Return Not IsNothing(BosswaveApplicationHost.HostedApplication)

    End Function
    Public Shared Function onServerInitializeAppCache() As Boolean
        Dim app_count As Long

        Try

            GlobalObject.MsgColored("load: application cache...", Color.Red)
            GlobalObject.ApplicationCache = New BosswaveAppCache

            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.Invoke(GlobalObject.serverForm.setSimonProgressBarVisibleThreaded, {True})
            End If

            app_count = GlobalObject.ApplicationCache.LoadAll(AppIndexLoadAll_ReturnValue.APPLICATION_COUNT)

            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.Invoke(GlobalObject.serverForm.setSimonProgressBarVisibleThreaded, {False})
            End If

            If app_count = 0 Then
                GlobalObject.MsgColored("no applications to initialize.", Color.Gold)
            End If

            Return True

        Catch ex As Exception
            Return False
        End Try


    End Function

    Public Shared Function onServerRegisterUsers() As Boolean


        Try
            TriniDATServerEvent.currentServer.Users = TriniDATUsers.Create(GlobalSetting.getUsersXML())
        Catch ex As Exception
            GlobalObject.Msg("Error loading users.xml")
            Return False
        End Try

        Return True

    End Function

    Public Shared Sub OnCreateStaticPaths(ByVal static_root As String)

        Dim jstats_path As String

        jstats_path = static_root & "JStats"

        If Not Directory.Exists(jstats_path) Then
            Try
                Directory.CreateDirectory(jstats_path)
            Catch ex As Exception
                GlobalObject.Msg(GlobalSetting.getStaticDataRoot() & ": " & ex.Message)
            End Try

        End If

    End Sub
    Public Shared Function onApplicationCreated(ByVal info As TriniDATRequestInfo) As Boolean
        'PREPARE SESSION
        '============

        'create folder structure

        Dim app_root As String
        Dim config_base_dir As String
        Dim current_config_base_dir As String

        config_base_dir = GlobalSetting.getStaticDataRoot()

        app_root = info.session_path

        If Not Directory.Exists(app_root) Then
            Try
                Directory.CreateDirectory(app_root)

            Catch ex As Exception
                GlobalObject.Msg("onApplicationCreated err: Error creating " & ex.Message)
                Return False
            End Try
        End If

        'create sub-dictories for all modules

        For Each trinidad_app As BosswaveApplication In GlobalObject.ApplicationCache

            If trinidad_app.haveMappingPoints Then

                For Each trinidad_app_mp In trinidad_app.ApplicationMappingPoints
                    If trinidad_app_mp.haveMappingPointInstance() Then

                        For Each module_name In trinidad_app_mp.MappingPoint.getClasses

                            current_config_base_dir = config_base_dir & module_name.getName()
                            If Directory.Exists(current_config_base_dir) Then
                                'create in session folder.
                                Dim session_module_path As String
                                session_module_path = app_root & module_name.getName()

                                If Not Directory.Exists(session_module_path) Then
                                    GlobalObject.Msg("Generating session folder '..\" & info.direct_session.ID & "\" & info.associated_app.Id.ToString & "\" & module_name.getName & "'.")
                                    Directory.CreateDirectory(session_module_path)

                                    '  Call clonedir(current_config_base_dir, module_path)
                                End If
                            End If
                        Next

                    End If
                Next
            End If
        Next

        Return True

    End Function

    Public Shared Function clonedir(ByVal src As String, ByVal todir As String)

        'Dim dirinfo As DirectoryInfo

        'dirinfo = New DirectoryInfo(src)

        'For Each src_file As FileInfo In dirinfo.GetFiles()
        '    Try

        '        FileCopy(src_file.FullName, Replace(src_file.FullName, src, todir))
        '    Catch ex As Exception

        '    End Try


        'Next

        'Return True
    End Function
    Public Shared Function onServerCanRegisterMappings() As Boolean
        'adds built-in mappings when server is ready.



        If Not serverAttached() Then Exit Function

        Return onServerInstallDefaultApplication()

        ''JServiceDummy
        'mp = New mappingPointRoot("admin_init")
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JServiceDummy")
        'mp.classAdd(mp_class)
        'mp.setURI("/admin/")
        'mp.isDebugger = False
        'EntryPointMappingPointDir.getDirectory().Add(mp)


        ''/CS/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JFileserver")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass()
        'mp_class.setName("JWWWConsoleContentRewriter")
        'mp.classAdd(mp_class)
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATChristianMode\menu\CSMainMenu\CSMenu\CSMenu\bin\Release\CSMenu.dll"}))
        'mp.setURI("/cs/")
        'mp.isDebugger = False
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\TestService\TestService\bin\Release\TriniDATserverTypes.dll
        ''/CS/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JTestGadget")
        'mp.classAdd(mp_class)
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\TestService\TestService\bin\Release\TestService.dll"}))
        'mp.setURI("/gadget/")
        'mp.isDebugger = False
        'EntryPointMappingPointDir.getDirectory().Add(mp)


        ''TWITTERFEED
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass()
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JSocialNameVerify") 'insert validators at top for more speed.
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass()
        'mp_class.setName("JSocialAccountEmailChecker")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JTwitterFeedSearch", "Harvestes e-mail addresses from Twitter.")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass()
        'mp_class.setName("JTwitterFeedUsefulParser")
        'mp.classAdd(mp_class)
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JSocialNameVerify\JSocialNameVerify\bin\Release\JSocialNameVerify.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JTwitterFeedSearch\JTwitterFeedSearch\bin\Release\JTwitterFeedSearch.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JTwitterFeedUsefulParser\JTwitterFeedUsefulParser\bin\Release\JTwitterFeedUsefulParser.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JSocialAccountEmailChecker\JSocialAccountEmailChecker\bin\Release\JSocialAccountEmailChecker.dll"})) 'JSocialAccountEmailChecker
        'mp.setURI("/seeds/tweetfeed")
        'mp.isDebugger = False
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ' ''/RANDOM/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JFreebaseQuery")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JRandomSocialId")
        'mp.classAdd(mp_class)
        'mp.setURI("/random/socialid")
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JFreebaseQuery\JFreebaseQuery\bin\Release\JFreebaseQuery.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JRandomSocialId\JRandomSocialId\bin\Release\JRandomSocialId.dll"}))
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''/random/disqus
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JDISQUSThread")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setURI("/random/disqus")
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JDISQUSThread\JDISQUSThread\bin\Release\JDISQUSThread.dll"}))
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ' ''/PROXy / namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass()
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JProxies")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JProxies\JProxies\bin\Release\JProxies.dll"}))
        'mp.setURI("/proxy")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ' ''/FREEBASE / namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JFreebaseQuery")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JFreebaseQuery\JFreebaseQuery\bin\Release\JFreebaseQuery.dll"}))
        'mp.setURI("/freebase/")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ' ''/NETWORK / namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JNetwork")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JNetwork\JNetwork\bin\Release\JNetwork.dll"}))
        'mp.setURI("/network")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ' ''/WINDOWS / namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JComputer")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JComputer\JComputer\bin\Release\JComputer.dll"}))
        'mp.setURI("/computer")
        'EntryPointMappingPointDir.getDirectory().Add(mp)


        ' ''/WPAD / namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JWPADService")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setURI("/wpadservice")
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JWPADService\JWPADService\bin\Release\JWPADService.dll"}))
        'EntryPointMappingPointDir.getDirectory().Add(mp)


        ''/KEYWORDS/ namespace
        ''JGoogleBooksKeywords
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JGoogleBooksKeywords")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.setURI("/keyword/frombook")
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JGoogle\JGoogle\bin\Release\JGoogle.dll"}))
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''/FACEBOOK/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JTicketMaster")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JFacebookMain")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        'mp.Initializor = GetType(FacebookStatic)
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JTicketMaster\JTicketMaster\bin\Release\JTicketMaster.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JFacebookMain\JFacebookMain\bin\Release\JFacebookMain.dll"}))

        '' mp.EncodingPreference = New System.Text.UTF8Encoding
        'mp.setURI("/facebook")

        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''/TICKETS/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JKernelReflection")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JStats")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JTicketMaster")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        '' mp.EncodingPreference = New System.Text.UTF8Encoding
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JTicketMaster\JTicketMaster\bin\Release\JTicketMaster.dll"}))
        'mp.setURI("/tickets/")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''/CAPTCHA/ namespace
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JTicketMaster")
        'mp.classAdd(mp_class)
        'mp_class = New mappingPointClass
        'mp_class.setName("JCaptchaDrop")
        'mp.classAdd(mp_class)
        'mp.isDebugger = False
        '' mp.EncodingPreference = New System.Text.UTF8Encoding
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JTicketMaster\JTicketMaster\bin\Release\JTicketMaster.dll", "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JCaptchaDrop\JCaptchaDrop\bin\Release\JCaptchaDrop.dll"}))
        'mp.setURI("/captcha")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''JHotmailSignup
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JHotmailSignup")
        'mp.classAdd(mp_class)
        '' mp.EncodingPreference = New System.Text.UTF8Encoding
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JHotmailSignup\JHotmailSignup\bin\Release\JHotmailSignup.dll"}))
        'mp.setURI("/signupprovider/email/hotmail")
        'EntryPointMappingPointDir.getDirectory().Add(mp)

        ''JPageRouter
        'mp = New mappingPointRoot()
        'mp_class = New mappingPointClass
        'mp_class.setName("JPageRouter")
        'mp.classAdd(mp_class)
        '' mp.EncodingPreference = New System.Text.UTF8Encoding
        'mp.setURI("/router/")
        'mp.setDependencyPaths(New List(Of String)({"C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATTriniDATServor\Libraries\JPageRouter\JPageRouter\bin\Release\JPageRouter.dll"}))
        'EntryPointMappingPointDir.getDirectory().Add(mp)

    End Function
    Public Shared Function OnMappingPointStarted(ByVal mp As mappingPointRoot, ByVal state_info As ChainResult) As ChainResult
        'called in context of mapping point thread
        mp.boot_time_start_ms = GlobalObject.GetTickCount()
        state_info.state = ChainResultState.FATAL_ERROR

        If Not GlobalObject.haveServerthread Then
            Return state_info
        End If

        Dim threadCount As Long
        threadCount = GlobalObject.server.Realtime_ModifyThreadCountByUser(mp.Session, False)

        GlobalObject.MsgColored("app '" & mp.Info.associated_app.ApplicationName & "' mapping point is online: " & mp.getURI(), Color.DarkGreen)

        If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
            GlobalObject.MsgColored(mp.getURI() & " is thread #" & threadCount & " of " & mp.Info.direct_session.relatedUser.MaxThreadCount.ToString & " by user " & mp.Info.direct_session.relatedUser.Username & ".", Color.DarkGreen)
        End If

        BosswaveAppMappingPoints.updateRuntimeCount(mp.ApplicationId, mp.getURI())

        state_info.state = ChainResultState.OK
        Return state_info

    End Function
    Public Shared Sub OnMappingPointStopped(ByVal mp As mappingPointRoot, ByVal suppress_offline_msg As Boolean)
        'called in context of 
        'mapping point thread 
        'server thread

        If Not GlobalObject.haveServerthread() Then
            GoTo ABORT_MAPPING_POINT_THREAD
        Else
            If Not GlobalObject.serverState = BosswaveServerState.ONLINE Then
                'server stopped
                GoTo ABORT_MAPPING_POINT_THREAD
            End If
        End If

        Dim threadCount As Long
        threadCount = GlobalObject.server.Realtime_ModifyThreadCountByUser(mp.Session, True)

        If Not suppress_offline_msg Then
            GlobalObject.MsgColored("app '" & mp.Info.associated_app.ApplicationName & "': mapping point finished: " & mp.getURI(), Color.DarkOrange)
        End If

        Msg("user " & mp.Info.direct_session.relatedUser.Username & " has #" & threadCount.ToString & " of " & mp.Session.relatedUser.MaxThreadCount.ToString & " running thread(s)")


        'ignore statistical counts if debug mode or thread is being killed.

        If mp.isDebug Then
            GoTo ABORT_MAPPING_POINT_THREAD
        End If

        Dim mp_descriptor As mapping_point_descriptor
        mp_descriptor = GlobalObject.server.getRealTimeMappingPointDescriptor(mp.Info)

        If IsNothing(mp_descriptor) Then
            GoTo ABORT_MAPPING_POINT_THREAD
        ElseIf mp_descriptor.Killed Then
            GoTo ABORT_MAPPING_POINT_THREAD
        End If


        'update statistics
        Dim thread_runtime_record_ms As Long

        Try
            'must write on realtime object.
            thread_runtime_record_ms = GlobalObject.server.getRealTimeMappingPointDescriptor(mp.Info).AddRuntimeRecord(GlobalObject.GetTickCount())
        Catch ex As Exception
            thread_runtime_record_ms = -1

        End Try

        If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
            Msg(mp.getURI() & ": completed in " & thread_runtime_record_ms.ToString & " ms. average: " & mp_descriptor.getAveragetimeMillisecond.ToString & " ms. got " & mp_descriptor.boot_milliseconds.Count.ToString & " lifetime experience(s) for this thread at session " & mp.Session.ID & ".")
        End If


ABORT_MAPPING_POINT_THREAD:
        Try
            'kill socket(s)
            If Not IsNothing(mp.Info) Then
                If Not IsNothing(mp.Info.http_connection_handler) Then
                    If mp.Info.http_connection_handler.getConnection.isConnected() Then
                        mp.Info.http_connection_handler.getConnection.forceDisconnect()
                    End If

                    If Not IsNothing(mp.Info.direct_socket) Then
                        If mp.Info.direct_socket.isConnected() Then
                            mp.Info.direct_socket.forceDisconnect()
                        End If
                    End If
                End If
            End If

            Threading.Thread.CurrentThread.Abort()
        Catch ex As Exception

        End Try

    End Sub
    Public Shared Sub OnServerStarted()
        'Called from TriniDATServer thread.
        Dim current_http_config As BosswaveTCPServerConfig

        current_http_config = GlobalObject.CurrentServerConfiguration

        Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.OnServerStartedThreadedCaller, current_http_config)

        GlobalObject.MsgColored("Server running on " & current_http_config.server_ip.ToString & " port: " & current_http_config.server_port.ToString & "...", Color.Red)

        If GlobalSetting.haveServerStartMacro() Then
            GlobalObject.ExecuteFile(GlobalSetting.getServerStartupMacro(), "", False, ProcessWindowStyle.Maximized)
        End If

        'wipe out previous action execution list.
        Call GlobalObject.resetActionProcessesExecList()
        GlobalObject.FirstHiddenAction = True

        ' Call TriniDATServerEvent.currentServer.installCaptchaListener()

        If Not GlobalSpeech.haveSpeaker Then
            GlobalObject.Msg("Note: Speech features are disabled because speech engine failed to load.")
            If GlobalObject.haveServerForm Then
                Try
                    'disable speech button.
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.onSpeechModeChangedThreaded, False)
                Catch ex As Exception

                End Try

            End If
        Else

            GlobalSpeech.Text = "server has started."
            GlobalSpeech.SpeakThreaded()

        End If

        'ensure simon is in the same context
        If GlobalObject.haveSimon Then
            'will show context message change to user.
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                GlobalObject.simon.setConsoleContext(SimonConsoleContext.SERVER_DEV, Nothing)
            ElseIf GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_LIVE Then
                GlobalObject.simon.setConsoleContext(SimonConsoleContext.SERVER_LIVE, Nothing)
            End If

        End If

        If GlobalObject.haveServerForm Then
            If GlobalObject.FirstRun Then
                'double check if this is first run.
                If GlobalObject.haveApplicationCache Then
                    If Not GlobalObject.ApplicationCache.haveApplication(1) Then
                        'start downloading
                        GlobalObject.serverForm.Invoke(GlobalObject.serverForm.OnFirstRunThreaded)
                    End If
                End If
            End If
        End If

    End Sub

    Public Shared Sub doBootMessage(ByVal current_boot_msg_color As Color)

        If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
            'boot messages to free users only.
            GlobalObject.MsgColored("TriniDAT Data Application Server Software " & GlobalObject.getVersionString() & " (c) - " & Year(Now).ToString & " GertJan de Leeuw | De Leeuw ICT.", current_boot_msg_color)
            GlobalObject.MsgColored("for more information, see http://www.deleeuwict.nl/" & vbNewLine, Color.Gold)

            If GlobalObject.haveSimon Then
                Dim trans As SimonsSession

                trans = GlobalObject.simon.generateSimonsSessionStruct()
                GlobalObject.MsgColored(trans.getTranslated("HELPHINT"), current_boot_msg_color)

            End If
        End If

    End Sub

    Public Shared Sub OnServerStopped(ByVal server As TriniDATServer)
        Msg("Server stopped.")

        If GlobalSetting.haveServerStopMacro() Then
            GlobalObject.ExecuteFile(GlobalSetting.getServerStopMacro(), "", False, ProcessWindowStyle.Maximized)
        End If

        Dim current_http_config As BosswaveTCPServerConfig

        current_http_config = GlobalObject.CurrentServerConfiguration
        Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.OnServerStoppedThreadedCaller, current_http_config)

    End Sub

    Private Shared Sub Msg(ByVal txt As String)
        txt = "Config: " & txt
        Call GlobalObject.MsgColored(txt, Color.DarkGreen)

    End Sub
End Class
