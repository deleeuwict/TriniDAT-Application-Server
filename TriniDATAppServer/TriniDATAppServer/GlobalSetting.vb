Option Compare Text
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Management
Imports System.Reflection
Imports System.Security.Principal

Public Class GlobalSetting

    Private Shared builder_xml_path_root As String
    Private Shared general_config_root As String
    Private Shared static_data_root As String
    Private Shared core_config_path As String
    Private Shared exec_path As String
    Private Shared xml_applications_root As String
    Private Shared session_root As String
    Private Shared logfile_root As String
    Private Shared remote_app_upload_url As String
    Private Shared user_paykey As String
    Private Shared app_package_serverindex_uri As String
    Private Shared trinidat_setup_url As String
    Public Const DEFAULT_HTTPSERVER_PORT As Integer = 80
    Public Const MULTI_THREADED_WAIT As Boolean = True
    Public Const SESSION_COOKIE_NAME As String = "X-SessionId"
    Public Const EXECPREFIX_ACTION_OBJECTFORWARDER As String = "tdatobjfwd"
    Public Shared Function Load() As Boolean
        'all missing required directories will automatically be created on application start.
        Dim root_string As String
        Dim idstring As String

        Dim mp As WindowsPrincipal
        mp = New WindowsPrincipal(WindowsIdentity.GetCurrent())

        If Not mp.IsInRole(WindowsBuiltInRole.Administrator) And Not GlobalObject.userIsGJ Then
            MessageBox.Show("This application requires administrator priveledges to run.", "Restart server as administrator.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            End
        End If


        Try
            idstring = "1"

            root_string = GlobalSetting.ExeFilePath

            'remove Visual Basic .NET paths.
            GlobalSetting.exec_path = Replace(root_string, "\Release", "")
            GlobalSetting.exec_path = Replace(GlobalSetting.exec_path, "\Debug", "")
            GlobalSetting.exec_path = Replace(GlobalSetting.exec_path, "\bin", "")

            'CONSTANTS
            '======
            'general_config_root
            GlobalSetting.general_config_root = GlobalSetting.getRoot() & "trinidat_config\"
            GlobalSetting.session_root = GlobalSetting.getRoot() & "trinidad_sessions\"
            GlobalSetting.logfile_root = GlobalSetting.getRoot() & "trinidat_logs\"
            GlobalSetting.builder_xml_path_root = GlobalSetting.getConfigRoot() & "builder\"
            GlobalSetting.xml_applications_root = GlobalSetting.getConfigRoot() & "apps\"
            GlobalSetting.static_data_root = GlobalSetting.getConfigRoot() & "static\"
            GlobalSetting.core_config_path = GlobalSetting.getConfigRoot() & "core\"

            'Create paths

            If Not Directory.Exists(GlobalSetting.static_data_root) Then
                Directory.CreateDirectory(GlobalSetting.static_data_root)
                Try
                    Directory.CreateDirectory(GlobalSetting.static_data_root)
                    Call TriniDATServerEvent.OnCreateStaticPaths(GlobalSetting.getStaticDataRoot())
                Catch ex As Exception
                    GlobalObject.Msg(GlobalSetting.getStaticDataRoot() & ": " & ex.Message)
                End Try
            End If

            If Not Directory.Exists(GlobalSetting.session_root) Then
                Directory.CreateDirectory(GlobalSetting.session_root)
            End If

            If Not Directory.Exists(GlobalSetting.logfile_root) Then
                Directory.CreateDirectory(GlobalSetting.logfile_root)
            End If

            If Not Directory.Exists(GlobalSetting.core_config_path) Then
                Directory.CreateDirectory(GlobalSetting.core_config_path)
            End If

            If Not Directory.Exists(GlobalSetting.getSimonPath()) Then
                Directory.CreateDirectory(GlobalSetting.getSimonPath())
            End If

            If Not Directory.Exists(GlobalSetting.builder_xml_path_root) Then
                Directory.CreateDirectory(GlobalSetting.builder_xml_path_root)
            End If


            If Not Directory.Exists(GlobalSetting.xml_applications_root) Then
                Directory.CreateDirectory(GlobalSetting.xml_applications_root)
            End If

            If Not Directory.Exists(GlobalSetting.getSpeechPath()) Then
                Directory.CreateDirectory((GlobalSetting.getSpeechPath()))
            End If

            Return True

        Catch ex As Exception
            MsgBox("Sorry, cannot acces configuration files. Please ensure you are running the server with administrator priveledges and have set sufficient file permissions for '" & GlobalSetting.ExeFilePath & " and all subdirectories." & "'." & vbNewLine & "Error: " & ex.Message)
            Return False
        End Try
    End Function
   
    Public Shared ReadOnly Property havePayKey As Boolean
        Get
            Return Not IsNothing(GlobalSetting.PayKey)
        End Get
    End Property
    Public Shared Property PayKey As String
        Get
            Return GlobalSetting.user_paykey
        End Get
        Set(ByVal value As String)
            GlobalSetting.user_paykey = value
        End Set
    End Property
    Public Shared Function getTempDir() As String
        Dim default_output_folder As String

        default_output_folder = Environment.GetEnvironmentVariable("Temp")

        If Mid(default_output_folder, Len(default_output_folder) - 1, 1) <> "\" Then
            default_output_folder &= "\"
        End If

        If Not Directory.Exists(default_output_folder) Then
            Try
                Directory.CreateDirectory(default_output_folder)
            Catch ex As Exception

            End Try
        End If

        Return default_output_folder
    End Function
    Public Shared Function getWindowsSerial() As String

        Dim MOS As ManagementObjectSearcher = New ManagementObjectSearcher("Select * From Win32_OperatingSystem")
        Dim MOC As System.Management.ManagementObjectCollection = MOS.Get
        Dim retval As String

        Try
            retval = MOC(0)("SerialNumber")
        Catch ex As Exception
            retval = Nothing
        End Try

        Return retval

    End Function
    Public Shared ReadOnly Property ExeFilePath() As String
        Get
            Dim retval As String
            Dim fi As FileInfo
            fi = New FileInfo(Assembly.GetExecutingAssembly().Location)

            retval = fi.DirectoryName

            If Right(retval, 1) <> "\" Then
                retval &= "\"
            End If

            Return retval
        End Get
    End Property
    Public Shared ReadOnly Property ExecMD5() As String
        Get
            Return GlobalObject.getMD5CheckSum(New FileInfo(Assembly.GetExecutingAssembly().Location), True)
        End Get
    End Property

    Public Shared Function getCoreConfigPath() As String
        Return GlobalSetting.core_config_path
    End Function
    Public Shared Function getUsersXML() As XDocument

        If File.Exists(GlobalSetting.getUsersXMLPath) Then
            Return XDocument.Parse(File.ReadAllText(GlobalSetting.getUsersXMLPath))
        Else

            'generate new file
            Dim user_xml_template As String

            user_xml_template = "<serverusers><serveruser name='$COMPUTER' password='$COMPUTER' maxthread='$MAXTHREAD'><permissiontable><appcachereload>true</appcachereload><appdebugging>true</appdebugging></permissiontable></serveruser></serverusers>"
            user_xml_template = Replace(user_xml_template, "'", Chr(34))

            'set defaults.
            user_xml_template = Replace(user_xml_template, "$COMPUTER", My.Computer.Name)
            user_xml_template = Replace(user_xml_template, "$MAXTHREAD", GlobalObject.OfficialLicense.getT.ToString)

            Try

                File.WriteAllText(GlobalSetting.getUsersXMLPath(), user_xml_template)
                Return GlobalSetting.getUsersXML()

            Catch ex As Exception
                GlobalObject.MsgColored("Error generating system files: Cannot write '" & GlobalSetting.getUsersXMLPath & "'.", Color.Red)
                Return Nothing
            End Try
        End If
    End Function
    Public Shared Property Latest_SetupURL() As String
        Get
            Return GlobalSetting.trinidat_setup_url
        End Get
        Set(ByVal value As String)
            GlobalSetting.trinidat_setup_url = value
        End Set
    End Property
    Public Shared Property AppStore_UploadURL() As String
        Get
            Return GlobalSetting.remote_app_upload_url
        End Get
        Set(ByVal value As String)
            GlobalSetting.remote_app_upload_url = value
        End Set
    End Property
    Public Shared Property CoreApp_ServerIndexURL() As String
        Get
            Return GlobalSetting.app_package_serverindex_uri
        End Get
        Set(ByVal value As String)
            GlobalSetting.app_package_serverindex_uri = value
        End Set
    End Property
    Public Shared Function getUsersXMLPath() As String
        Return GlobalSetting.core_config_path & "users.xml"
    End Function
    Public Shared Function getRoot() As String
        Return GlobalSetting.exec_path
    End Function
    Public Shared Function getSessionRoot() As String
        Return GlobalSetting.session_root
    End Function
    Public Shared Function getLogfilePath() As String
        Return GlobalSetting.logfile_root
    End Function
    Public Shared Function getConfigRoot() As String
        Return GlobalSetting.general_config_root
    End Function
    Public Shared Function getStaticDataRoot() As String
        Return GlobalSetting.static_data_root
    End Function
    Public Shared Function getMacroPath() As String
        Return GlobalSetting.getConfigRoot() & "eventmacrofiles\"
    End Function
    Public Shared Function getServerStartupMacro() As String
        Return GlobalSetting.getMacroPath() & "serverstart.bat"
    End Function
    Public Shared Function getServerStopMacro() As String
        Return GlobalSetting.getMacroPath() & "serverstop.bat"
    End Function
    Public Shared Function haveServerStartMacro() As String
        Return File.Exists(GlobalSetting.getServerStartupMacro)
    End Function
    Public Shared Function haveServerStopMacro() As String
        Return File.Exists(GlobalSetting.getServerStopMacro)
    End Function

    Public Shared Function getAppPrototypeDeclarationPath() As String
        Return GlobalSetting.builder_xml_path_root & "allprototypes.xml"
    End Function
    Public Shared Function getBuilderXMLPath() As String
        Return GlobalSetting.builder_xml_path_root
    End Function
    Public Shared Function getAppsRoot() As String
        Return GlobalSetting.xml_applications_root
    End Function
    Public Shared Function getSimonPath() As String
        Return GlobalSetting.getConfigRoot & "simon\"
    End Function
    '&  & "simon.xml"
    Public Shared Function getSpeechPath() As String
        Return GlobalSetting.getConfigRoot() & "speech\"
    End Function
    Public Shared Function getSimonXMCommandFile() As String
        Return GlobalSetting.getSimonPath() & "commands.xml"
    End Function
    Public Shared ReadOnly Property haveSimonCommandFile As Boolean
        Get
            Return File.Exists(GlobalSetting.getSimonXMCommandFile())
        End Get
    End Property
    Public Shared Function getSimonXMLTranslationTableFile() As String
        Return GlobalSetting.getSimonPath() & "translation.xml"
    End Function
    Public Shared Function getApplicationsIndexXMLFilePath() As String
        Return GlobalSetting.getAppsRoot() & "index.xml"
    End Function

    Public Shared Function getApplicationsIndexXML() As XDocument

        Dim path As String

        path = GlobalSetting.getApplicationsIndexXMLFilePath

        If File.Exists(path) Then
            Return XDocument.Parse(File.ReadAllText(path))
        Else
            'create new app index.
            Dim index_template As String

            index_template = "<apps>"
            ' index_template = "<app/>"
            index_template &= "</apps>"
            index_template = Replace(index_template, "'", Chr(34))

            Try

                File.WriteAllText(path, index_template)

            Catch ex As Exception
                GlobalObject.MsgColored("Fatal error: can't generate application index file '" & path & "'. Check file permissions and restart server to fix.", Color.Red)
            End Try

            Return GlobalSetting.getApplicationsIndexXML()
        End If
    End Function

    Public Shared Function getHTTPServerConfig() As BosswaveTCPServerConfig

        Dim retval As BosswaveTCPServerConfig
        Dim xconfig As XDocument
        Dim configfile As String
        Dim config_xml As String

        retval = BosswaveTCPServerConfig.getDefault

        configfile = GlobalSetting.getCoreConfigPath() & "httpconfig.xml"

        Try
            retval.config_file_exists = File.Exists(configfile)

            If retval.config_file_exists Then

                config_xml = File.ReadAllText(configfile)
                xconfig = XDocument.Parse(config_xml)

                Dim q = From hsn In xconfig.Descendants("httpserver").Descendants()

                For Each http_config_node As XElement In q

                    If http_config_node.Name = "listenon" Then
                        retval.server_ip = IPAddress.Parse(http_config_node.Value)
                        retval.default_address_set = False
                    ElseIf http_config_node.Name = "defaultmaxthread" Then
                        Try
                            If Not IsNumeric(http_config_node.Value) Then
                                Throw New Exception("<DefaultMaxThread> error. Port number is expected. Found: " & http_config_node.Value)
                            End If

                            retval.DefaultMaxThread = CLng(http_config_node.Value)

                            If retval.DefaultMaxThread > GlobalObject.OfficialLicense.getT Then
                                Throw New Exception("<DefaultMaxThread> value exceeds current server license. Reset to " & GlobalObject.OfficialLicense.GetT.ToString & " .")
                                retval.DefaultMaxThread = GlobalObject.OfficialLicense.GetT
                            End If

                        Catch ex As Exception
                            GlobalObject.MsgColored(ex.Message, Color.Red)
                        End Try

                    ElseIf http_config_node.Name = "userpaykey" Then
                        GlobalSetting.PayKey = http_config_node.Value
                    ElseIf http_config_node.Name = "listenat" Then
                        Try
                            retval.server_port = CInt(http_config_node.Value)
                        Catch ex As Exception
                            GlobalObject.Msg("<listenon> error. Port number is expected. Found: " & http_config_node.Value)
                        End Try
                    ElseIf http_config_node.Name = "requesttimeoutsec" Then
                        Try
                            retval.socket_timeout = CInt(http_config_node.Value)
                        Catch ex As Exception
                            GlobalObject.Msg("httpconfig.xml:<requesttimeoutsec> error. Decimal number expected. Found: " & http_config_node.Value)
                        End Try
                    ElseIf http_config_node.Name = "receivebuffersize" Then
                        Try
                            retval.receive_buffer_size = CLng(http_config_node.Value)

                            If retval.receive_buffer_size < 1024 Then
                                GlobalObject.Msg("httpconfig.xml:<receivebuffersize> ignored: minimal 1024 bytes required but set to " & http_config_node.Value.ToString & " bytes in config file.")
                                retval.receive_buffer_size = 1024
                            End If
                        Catch ex As Exception
                            GlobalObject.Msg("httpconfig.xml:<receivebuffersize> error. Decimal number expected. Found: " & http_config_node.Value)
                        End Try
                    End If
                Next
            Else
                GlobalObject.Msg("Config file not found.")
                retval.server_ip = IPAddress.Parse("127.0.0.1")
                retval.default_address_set = True
            End If

            retval.succes = True

        Catch ex As Exception
            GlobalObject.Msg("httpconfig.xml might be corrupt. Error parsing: " & ex.Message)
            retval.succes = False
        End Try


        Return retval
    End Function
    Public Shared ReadOnly Property getHTTPServerConfigPath() As String
        Get
            Return GlobalSetting.getCoreConfigPath() & "httpconfig.xml"
        End Get
    End Property
End Class


