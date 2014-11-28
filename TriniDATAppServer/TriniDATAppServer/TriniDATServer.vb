Option Explicit On
Option Compare Text
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Net
Imports System
Imports System.Runtime.CompilerServices
Imports TriniDATSockets
Imports TriniDATServerTypes
Imports TriniDATServerStatTypes

<Assembly: SuppressIldasmAttribute()> 

Public Class TriniDATServer

    Public serverSocket As TcpListener
    Public Shared tcpClientConnected As New AutoResetEvent(False)
    Private serverRunning As Boolean
    Private captcha_listener_thread As Thread
    Private captcha_listener_thread_classinstance As CaptchaImageListener
    Private current_httpserver_config As BosswaveTCPServerConfig
    Private ApplicationUsers As TrinidatUsers
    Private server_mode As TRINIDAT_SERVERMODE

    Public Sub New(ByVal mode As TriniDATServerTypes.TRINIDAT_SERVERMODE)
        Me.captcha_listener_thread = Nothing
        Me.ServerMode = mode

    End Sub

    Public Property ServerMode As TRINIDAT_SERVERMODE
        Get
            Return Me.server_mode
        End Get
        Set(ByVal value As TRINIDAT_SERVERMODE)
            Me.server_mode = value
        End Set
    End Property
    Public Function getRealTimeMappingPointDescriptor(ByVal realtime_mapping_point_session As TriniDATRequestInfo) As mapping_point_descriptor

        Dim abstract_session As TriniDATUserSession
        Dim abstract_user As TriniDATUser

        abstract_user = Me.Users.getBySessionID(realtime_mapping_point_session.direct_session.ID)

        If Not IsNothing(abstract_user) Then
            abstract_session = abstract_user.Sessions.getSessionById(realtime_mapping_point_session.direct_session.ID)
            If Not IsNothing(abstract_session) Then
                Dim mp_app As BosswaveApplication

                mp_app = abstract_session.Application(realtime_mapping_point_session.associated_app.Id)
                If Not IsNothing(mp_app) Then
                    Return mp_app.ApplicationMappingPoints.getDescriptorByURL(realtime_mapping_point_session.mapping_point_desc.URL)
                Else
                    MSG("getRealTimeMappingPointDescriptor:Unable to realtime application by session" & realtime_mapping_point_session.direct_session.ID)
                End If
            Else
                MSG("getRealTimeMappingPointDescriptor: Session " & realtime_mapping_point_session.direct_session.ID & " owned by user " & abstract_user.Username & " out of sync!")
            End If
        Else
            MSG("Unable to find abstract user for " & realtime_mapping_point_session.direct_session.ID)
        End If

        Return Nothing
    End Function
    Public Function Realtime_ModifyThreadCountByUser(ByVal current_session As TriniDATUserSession, ByVal substract As Boolean) As Long

        Dim abstract_user As TriniDATUser

        abstract_user = Me.Users.getBySessionID(current_session.ID)

        If Not IsNothing(abstract_user) Then
            If Not substract Then
                abstract_user.ThreadCount = abstract_user.ThreadCount + 1
            Else
                abstract_user.ThreadCount = abstract_user.ThreadCount - 1
            End If
            Return abstract_user.ThreadCount
        Else
            MSG("Unable to find abstract user for " & current_session.ID)
        End If

        Return -1

    End Function
    Public Function Realtime_ThreadCountByUser(ByVal current_session As TriniDATUserSession) As Long

        Dim abstract_user As TriniDATUser

        abstract_user = Me.Users.getBySessionID(current_session.ID)

        If Not IsNothing(abstract_user) Then
            Return abstract_user.ThreadCount
        Else
            MSG("Unable to find abstract user for " & current_session.ID)
            Return -1
        End If



    End Function
    Public Function UpdateThreadCountRealtime(ByVal current_session As TriniDATUserSession, ByVal substract As Boolean) As Long

        Dim abstract_user As TriniDATUser

        abstract_user = Me.Users.getBySessionID(current_session.ID)

        If Not IsNothing(abstract_user) Then
            If Not substract Then
                abstract_user.ThreadCount = abstract_user.ThreadCount + 1
            Else
                abstract_user.ThreadCount = abstract_user.ThreadCount - 1
            End If
            Return abstract_user.ThreadCount
        Else
            MSG("Unable to find abstract user for " & current_session.ID)
        End If

        Return -1

    End Function


    Private ReadOnly Property serverForm() As frmServerMain
        Get
            Return GlobalObject.serverForm
        End Get
    End Property

    Public Property Users As TriniDATUsers
        Get
            Return Me.ApplicationUsers
        End Get
        Set(ByVal value As TriniDATUsers)
            Me.ApplicationUsers = value
        End Set
    End Property

    Public Property HTTPConfiguration As BosswaveTCPServerConfig
        Get
            Return Me.current_httpserver_config
        End Get
        Set(ByVal value As BosswaveTCPServerConfig)
            Me.current_httpserver_config = value
            GlobalObject.CurrentServerConfiguration = value
        End Set
    End Property
    Public Function Listener() As Boolean

        TriniDATServerEvent.setServer(Me)

        If Not TriniDATServerEvent.onServerInitializeAppCache() Then
            MSG("Error initializing app cache. Aborting.")
            Return False
        End If

        'init messaging sub-system
        JSONMailBoxServer.OnServerStartInit()

        Me.HTTPConfiguration = GlobalSetting.getHTTPServerConfig()



        If Not HTTPConfiguration.succes Then
            MSG("Server configuration failure. Aborting server startup.")
            MSG("Please fix " & GlobalSetting.getCoreConfigPath() & "httpconfig.xml and start the server to continue.")
            Return False
        ElseIf Me.HTTPConfiguration.default_address_set Then
            MSG("configuration notice: default network address is in use. Please edit " & GlobalSetting.getCoreConfigPath() & "httpconfig.xml")
        End If

        If Not GlobalObject.TestTCPServerConnection(Me.HTTPConfiguration.server_ip.ToString, Me.HTTPConfiguration.server_port) Then
            GlobalObject.MsgColored("Invalid network interface or port in use.", Color.Red)
            Return False
        End If

        GlobalObject.MsgColored("HTTP receive buffer: " & (Me.HTTPConfiguration.receive_buffer_size / 1024).ToString & " kb. ", Color.MidnightBlue)

        'start listening
        serverSocket = New TcpListener(Me.HTTPConfiguration.server_ip, Me.HTTPConfiguration.server_port)
        serverSocket.Start()
        Me.serverRunning = True

        'GlobalObject.MsgColored("Load: TriniDAT user list", Color.Red)
        Call TriniDATServerEvent.onServerRegisterUsers()


        GlobalObject.MsgColored("Load: JSON messaging sub-system", Color.Red)
        'notify admin the srver has started.
        Call TriniDATServerEvent.OnServerStarted()

        GlobalObject.MsgColored("TriniDAT Data Server " & GlobalObject.OfficialLicense.getLicenseName() & " " & GlobalObject.getVersionString() & " online.", Color.Gold)
        Call TriniDATServerEvent.doBootMessage(Color.Gold)


        Try

            While True
                Dim socket As TcpClient
                socket = serverSocket.AcceptTcpClient()
                If socket.Connected() Then
                    Call IncomingConnectionHandler(socket)
                    Threading.Thread.Sleep(10)
                Else
                    Exit While
                End If

            End While

        Catch ex As Exception

            ''server is killed by Application OnExit
            Debug.Print("Server thread stopped with message: " & ex.Message)
            Try
                Thread.CurrentThread.Abort()
            Catch thrdex As Exception

            End Try
        End Try
        Return True

    End Function


    Public Sub installCaptchaListener()
        'Note: only call AFTER server has been started.
        MSG("Info: Listening for captcha's...")

        Me.captcha_listener_thread_classinstance = New CaptchaImageListener
        Me.captcha_listener_thread = New Thread(AddressOf Me.captcha_listener_thread_classinstance.StartListening)
        Me.captcha_listener_thread.Start()


    End Sub

    Public ReadOnly Property Running As Boolean
        Get
            Return Me.serverRunning
        End Get
    End Property
    Public Sub stopServer()

        If Not Me.serverRunning Then
            MSG("Stop: Server not running.")
            Exit Sub
        End If


        serverRunning = False
        serverSocket.Stop()

        MSG("Stop: JSON sub-system")
        JSONMailBoxServer.OnServerStopping()

        If Not IsNothing(captcha_listener_thread) Then
            If captcha_listener_thread.ThreadState = ThreadState.Running Or captcha_listener_thread.ThreadState = ThreadState.Background Then

                MSG("Stop: CAPTCHA sub-system..")

                Try
                    captcha_listener_thread.Abort()
                Catch ex As Exception
                    MSG("Stop erorr: captcha thread : " & ex.Message)
                End Try
            End If
        End If

        JSONMailBoxServer.OnBeforeServerStopped()

        'raise events
        Call TriniDATServerEvent.OnServerStopped(Me)

    End Sub

    Public Sub MSG(ByVal TXT As String)
        If Thread.CurrentThread.ThreadState = ThreadState.Running Then
            'Log to main form in a different thread. 
            TXT = "TriniDAT Data Server: " & TXT
            serverForm.Invoke(serverForm.serverColoredLogThrd, New Object() {TXT, Color.brown})
            Debug.Print(TXT)
        End If
    End Sub

    Public Function IncomingConnectionHandler(ByVal incomingsocket As TcpClient) As Boolean
        'Accept connection and fetch all data

        Dim donotuse As TcpClient
        Dim clientip As String
        Dim socket As TriniDATSockets.TriniDATTCPSocket

        If Not serverRunning Then
            Return False
        End If

        Try

            '  MSG("IncomingConnectionHandler: socket #" & incomingsocket.Client.Handle.ToString)
            donotuse = incomingsocket

            'INITIALIZE SOCKET
            socket = New TriniDATSockets.TriniDATTCPSocket(donotuse)
            socket.setReceiveBufferSize(Me.HTTPConfiguration.receive_buffer_size)
            socket.tagIsFirstPacket(True)


        Catch ex As Exception
            MSG("Error while accepting new connection: " & ex.Message & " @ " & ex.TargetSite.ToString)
            Return False
        End Try

        If Not socket.isConnected() Then
            MSG("Socket prematurely disconnected.")
            Return False
        End If

        clientip = socket.RemoteIP


        GlobalObject.MsgColored("Accepted new client. ip: " & clientip, Color.DarkGreen)


        'WILL ENTER DATA RECEIVE LOOP
        Return FetchAndParseData(socket)
    End Function
    Public Function ObjectRequestsHandler(ByVal http_connie As TriniDATClientConnectionManagerHTTP, ByVal org_packet As String) As Boolean
        Dim new_gobject_id As Long
        Dim response As String
        Dim asking_app_id As Long
        Dim asking_app_id_str As String
        Dim asking_mapping_point As String
        Dim asking_app_name As String
        Dim request_parts() As String
        Dim packet_lines() As String


        packet_lines = org_packet.Split(vbCrLf)
        If packet_lines.Length < 2 Then
            GlobalObject.MsgColored("Invalid object request received.", Color.Red)
            Return False
        End If

        request_parts = Split(packet_lines(0))
        asking_app_id = -1


        If request_parts(0) = "TRINIDATOBJECT" Then
            Dim serialized_object As String

            'parse headers
            If http_connie.haveHeader("X-TriniDAT-MappingPoint-URL") Then
                asking_mapping_point = http_connie.Headers("X-TriniDAT-MappingPoint-URL")
            Else
                asking_mapping_point = "(unknown source)"
            End If


            If http_connie.haveHeader("X-TriniDAT-Application-Id") Then

                If http_connie.getConnection().isLocalInterfaceIP Then
                    If IsNumeric(http_connie.Headers("X-TriniDAT-Application-Id")) Then
                        asking_app_id = CLng(http_connie.Headers("X-TriniDAT-Application-Id"))
                        'this ip is from a network adapter running on this server, thus a local app.
                        GlobalObject.Msg("new object request by application '" & GlobalObject.ApplicationCache.AppById(asking_app_id).ApplicationName & "'.")
                    Else
                        GlobalObject.Msg("Invalid application id in object request from local app.")
                    End If
                Else
                    asking_app_id_str = http_connie.Headers("X-TriniDAT-Application-Id")
                    GlobalObject.Msg("Remote object request from " & asking_app_id_str & ".")
                End If
            Else
                GlobalObject.Msg("Debug request is from remote application " & GlobalObject.ApplicationCache.AppById(asking_app_id).ApplicationName & ".")
            End If

            If http_connie.haveHeader("X-TriniDAT-Application-Name") Then
                asking_app_name = http_connie.Headers("X-TriniDAT-Application-Name")
            Else
                asking_app_name = "(unknown app)"
            End If

            If request_parts(1) = New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.REQUEST_DEBUG_OBJECT).MethodString Then
                If Me.ServerMode <> TRINIDAT_SERVERMODE.MODE_DEV Then
                    GlobalObject.MsgColored("dropping debug packet from '" & asking_app_name & "' " & asking_mapping_point & ". Server not in development mode. ", Color.Red)
                    Return True
                End If

                serialized_object = http_connie.getRequestBody(org_packet)

                GlobalSpeech.Text = "Incoming object"
                GlobalSpeech.SpeakEliteThreaded()

                Dim debug_frame As SimonDebugFrame
                debug_frame = New SimonDebugFrame
                debug_frame.appid = asking_app_id
                debug_frame.url = asking_mapping_point
                debug_frame.sessionid = "unknown sessionid"
                debug_frame.JSON = serialized_object
                debug_frame.direct_http_client = http_connie

                'get mapping point info
                If GlobalObject.haveApplicationCache Then
                    If Not GlobalObject.ApplicationCache.haveApplication(asking_app_id) Then
                        GlobalObject.MsgColored("Invalid app id in debug request. dropping socket.", Color.Red)
                        http_connie.setHTTPResponse(404)
                        http_connie.writeRaw(True, "INVALID", True)
                        Return False
                    End If
                End If

                debug_frame.App = GlobalObject.ApplicationCache.AppById(asking_app_id)
                debug_frame.direct_mapping_point = CType(debug_frame.App, BosswaveApplication).ApplicationMappingPoints.getDescriptorByURL(Replace(debug_frame.url, "/" & debug_frame.appid.ToString & "/", "/"))

                Return TriniDATServerEvent.OnIncomingDebugFrame(debug_frame)

            ElseIf request_parts(1) = New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.REQUEST_CREATEOBJECT).MethodString Then

                new_gobject_id = GlobalObject.NextId

                GlobalObject.Msg("Giving object " & new_gobject_id.ToString & " to " & http_connie.RemoteIP & " mapping point: " & asking_mapping_point)

                response = new_gobject_id.ToString.ToString

                http_connie.setHTTPResponse(200)
                http_connie.writeRaw(True, response, True)
                Return True
            End If
        End If

        Return False

    End Function
    Public Function firstRequestHandler(ByRef connie As TriniDATSockets.TriniDATTCPSocket, ByVal packet() As Byte, ByVal packetSize As Long) As Boolean
        'Generally processes the first incoming packet
        'or packets who have accidently lost connection info.
        Dim http_connie As TriniDATClientConnectionManagerHTTP
        Dim x As Integer
        Dim http_packet As String
        Dim URI As String = "" 'for exception handler.
        Dim URI_method As String = ""
        Dim ignoreURLPatterns As Collection
        Dim url_parser As TriniDATURLParser
        Dim create_new_session As Boolean

        Try
            http_packet = Encoding.ASCII.GetString(packet)
            http_connie = New TriniDATClientConnectionManagerHTTP(connie)


            URI_method = TriniDATClientConnectionManagerHTTP.getURLPart("method", http_packet)

            If Me.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                If GlobalObject.haveServerForm() Then
                    'serverLogLVItemLog
                    Dim lvTrafficItem As ListViewItem
                    lvTrafficItem = New ListViewItem
                    lvTrafficItem.Text = Now.ToString
                    lvTrafficItem.Tag = http_packet
                    lvTrafficItem.SubItems.Add(http_connie.RemoteIP)
                    lvTrafficItem.SubItems.Add("IN")
                    lvTrafficItem.SubItems.Add(URI_method)
                    lvTrafficItem.SubItems.Add("/")
                    lvTrafficItem.SubItems.Add("TriniDAT Server")
                    lvTrafficItem.SubItems.Add("Request")
                    lvTrafficItem.SubItems.Add("HTTP")
                    lvTrafficItem.SubItems.Add("Accepted")
                    lvTrafficItem.SubItems.Add(http_connie.RemoteIP)
                    lvTrafficItem.ForeColor = Color.Green
                    Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.serverLogLVItemLog, {lvTrafficItem})
                End If
            End If

            If URI_method = "TRINIDATOBJECT" Then

                If http_connie.parseHeaders(http_packet) > 0 Then
                    Return ObjectRequestsHandler(http_connie, http_packet)
                End If
            End If

            URI = TriniDATClientConnectionManagerHTTP.getURLPart("uri", http_packet)

            If URI_method = "" Or URI = "" Then
                connie.tagIsHTTP(False)
                Err.Raise(100, 0, "Invalid HTTP packet received. Closing connection.")
                If connie.isConnected() Then
                    Call connie.forceDisconnect()
                End If
                Return False
            Else
                'Tag socket HTTP state
                connie.tagIsHTTP(True)
                connie.tagIsGetRequest((URI_method = "GET"))
                connie.tagIsPostRequest((URI_method = "POST"))
                connie.tagOriginalURL(URI)
                connie.tagParameters(TriniDATClientConnectionManagerHTTP.getURLPart("parameter", http_packet))
            End If

            ignoreURLPatterns = New Collection
            ignoreURLPatterns.Add("/favicon.ico")
            ignoreURLPatterns.Add("/robots.txt")

            'Get rid of automated requests
            For Each temp In ignoreURLPatterns

                x = InStr(URI, temp)
                If x > 0 Then
                    '    MSG("Ignore " & URI & " matched by rule: ignore " & temp)
                    connie.tagIsGetRequest(True)
                    http_connie.setHTTPResponse(404)
                    http_connie.writeRaw(True, URI & " Not found", True)
                    Return False
                End If
            Next

            'INIT
            create_new_session = True
            url_parser = New TriniDATURLParser(URI)

            'PARSE HTTP DATA
            http_connie.parseHeaders(http_packet)


            Dim current_user As TriniDATUser
            Dim current_user_session_id_str As String

            current_user = Nothing

            If http_connie.parseHeaders(Text.Encoding.ASCII.GetString(packet)) > 0 Then
                'get session cookie
                If http_connie.HaveSessionCookie() Then
                    current_user_session_id_str = http_connie.SessionCookie
                    current_user = Me.Users.getBySessionID(current_user_session_id_str)
                    If Not IsNothing(current_user) Then
                        http_connie.Session = current_user.Sessions.getSessionById(current_user_session_id_str)
                        create_new_session = False
                    Else
                        MSG("User associated with session '" & current_user_session_id_str & "' not found!")
                    End If
                Else
                    MSG("No session cookie.")
                End If
            Else

                create_new_session = True
                If http_connie.HaveSessionCookie() Then
                    create_new_session = create_new_session
                End If
            End If


            If IsNothing(current_user) Then
                'create on account of guest
                current_user = Me.Users.getDefault()
            End If

            If create_new_session Then
                http_connie.Session = current_user.Sessions.Add(New TriniDATUserSession(TriniDATUserSession.generateNewSessionId()))
                GlobalObject.Msg("Creating session '" & http_connie.Session.ID & "'")
            Else
                If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                    GlobalObject.Msg("Resuming session '" & http_connie.Session.ID & "' with user '" & http_connie.Session.relatedUser.Username & "'.")
                End If

            End If

            If Not url_parser.isApplicationURI() And Not url_parser.isDebugApplicationURI() Then
                Dim index_thread As Thread
                If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                    MSG("Displaying application index to " & http_connie.RemoteIP & ".")
                End If

                index_thread = New Thread(AddressOf Me.outputApplicationIndex)
                index_thread.Start(http_connie)
                Return True
            End If

            'extract: /applicationid/mappingpoint

            Dim url_parse_result As TriniDATRequestInfo

            url_parse_result = url_parser.Parse(http_connie)
            url_parse_result.is_new_session = create_new_session

            'validate application state
            If (url_parse_result.parse_result <> TriniDATRequestInfoType.APP_PLUS_MP) And (Not url_parse_result.parse_result = TriniDATRequestInfoType.APP_PARTIAL_MP) And (Not url_parse_result.parse_result = TriniDATRequestInfoType.APP_IS_INTERFACE) Then
                url_parse_result.http_connection_handler.setHTTPResponse(404)

                If url_parse_result.parse_result = TriniDATRequestInfoType.ERR_INVALID_ID Then
                    url_parse_result.http_connection_handler.writeRaw(True, "Invalid application URL.", True)
                ElseIf url_parse_result.parse_result = TriniDATRequestInfoType.APP_ONLY Then
                    Dim invalid_mp As Boolean
                    invalid_mp = True

                    If url_parse_result.associated_app.DoesInherit And Me.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                        Dim cur_inherited_app As BosswaveApplication
                        invalid_mp = False

                        cur_inherited_app = url_parse_result.associated_app.InheritedApplication

                        While Not IsNothing(cur_inherited_app)

                            If url_parse_result.associated_app.haveMappingPoints Then

                                For Each mp_desc As mapping_point_descriptor In cur_inherited_app.ApplicationMappingPoints

                                    If mp_desc.URL = url_parse_result.unparsed_url_part Then

                                        If mp_desc.MustOverrideFlag Then
                                            'forgot to implement a mapping point url
                                            url_parse_result.http_connection_handler.writeRaw(True, "'" & url_parse_result.associated_app.ApplicationName & "' should override url " & mp_desc.URL & " in interface '" & cur_inherited_app.ApplicationName & "' as defined in " & cur_inherited_app.Filepath & ".", True)
                                        Else
                                            'developer did not choose to override this method, yet it was called.
                                            url_parse_result.http_connection_handler.writeRaw(True, url_parse_result.associated_app.ApplicationName & "': mapping point not implemented: " & mp_desc.URL, True)
                                        End If
                                        Return True
                                    End If
                                Next

                            End If

                            cur_inherited_app = cur_inherited_app.InheritedApplication
                        End While

                        url_parse_result.http_connection_handler.writeRaw(True, "This mapping point is not implemented.", True)
                        Return True
                    End If

                    If invalid_mp And Not url_parse_result.is_debug_request Then
                        url_parse_result.http_connection_handler.writeRaw(True, "This mapping point does not exist in app '" & url_parse_result.associated_app.ApplicationName & "'.", True)
                    End If

                End If

                If Not url_parse_result.is_debug_request Then
                    Return False
                End If

            End If

            'create instance in current session
            If Not url_parse_result.http_connection_handler.Session.haveApplicationById(url_parse_result.associated_app.Id) Then
                'upload application in session
                url_parse_result = url_parse_result.http_connection_handler.Session.createApplication(url_parse_result)
                'update session info
                url_parse_result.http_connection_handler.Session = url_parse_result.direct_session
            End If


            'handle debug directives
            If url_parse_result.is_debug_request And url_parse_result.haveSession Then

                If Me.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                    If url_parse_result.direct_session.relatedUser.Permissions.appdebug Then

                        'redirect hack0r
                        If Not url_parse_result.http_connection_handler.getConnection().isLocalOrIntranetClient Then
                            Dim ip As String
                            ip = url_parse_result.http_connection_handler.getConnection.remoteip

                            url_parse_result.http_connection_handler.setHTTPResponse(200)
                            url_parse_result.http_connection_handler.writeRaw(True, "simon says hack attack.", True)
                            Return True
                        End If

                        If Not url_parse_result.haveUnparsedSection Then

                            If url_parse_result.haveMappingPoint Then
                                'output mp xml
                                url_parse_result.http_connection_handler.setHTTPResponse(200)
                                url_parse_result.http_connection_handler.setOutputMime("application/xhtml+xml")
                                url_parse_result.http_connection_handler.writeRaw(True, "<info>" & url_parse_result.mapping_point_desc.Node.ToString & "</info>", True)
                                Return True
                            Else
                                'output app xml
                                url_parse_result.http_connection_handler.setHTTPResponse(200)
                                url_parse_result.http_connection_handler.setOutputMime("application/xhtml+xml")
                                url_parse_result.http_connection_handler.writeRaw(True, "<info>" & url_parse_result.associated_app.ApplicationNode.ToString & "</info>", True)
                                Return True
                            End If

                        Else
                            'parse debug directives

                            If Not url_parse_result.haveMappingPoint And GlobalObject.haveApplicationCache Then

                                Dim app_level_debug_directive As String
                                app_level_debug_directive = Replace(url_parse_result.unparsed_url_part, "/", "")

                                If app_level_debug_directive = "reload" And url_parse_result.direct_session.relatedUser.Permissions.ReloadAppCache Then
                                    Dim app_count As Long
                                    Me.FlushSessions()
                                    app_count = GlobalObject.ApplicationCache.Reload()
                                    url_parse_result.http_connection_handler.writeRaw(True, "Reload: " & app_count.ToString & " app(s) installed.")

                                    Return True
                                End If

                                Dim permissionerr_msg As ChainResult

                                permissionerr_msg = New ChainResult
                                permissionerr_msg.state_http_code = 401
                                permissionerr_msg.state_message = "Sorry, insufficient permissions."
                                permissionerr_msg.HTTP_OutputState(url_parse_result.http_connection_handler)

                                Return True
                            End If
                        End If

                        Dim err_msg As ChainResult

                        err_msg = New ChainResult
                        err_msg.state_http_code = 400
                        err_msg.state_message = "Malformed debug request."
                        err_msg.HTTP_OutputState(url_parse_result.http_connection_handler)

                        Return False
                    Else
                        'no debug permissions or prouction mode.
                        Dim err_msg As ChainResult

                        err_msg = New ChainResult
                        err_msg.state_http_code = 401
                        err_msg.state_message = "Access denied."
                        err_msg.HTTP_OutputState(url_parse_result.http_connection_handler)

                    End If
                Else
                    Dim err_msg As ChainResult

                    err_msg = New ChainResult
                    err_msg.state_http_code = 401
                    err_msg.state_message = "User '" & url_parse_result.direct_session.relatedUser.Username & "' has no debugging permission."
                    err_msg.state_solution_message = "Set 'appdebugging' to 'true' in " & GlobalSetting.getUsersXMLPath() & "."
                    err_msg.HTTP_OutputState(url_parse_result.http_connection_handler)
                    Return True
                End If
            End If

            'NO DEBUG REQUEST AFTER  THIS

            If url_parse_result.associated_app.IsInterface Then
                Return Me.outputInterfaceDefinition(url_parse_result)
            Else

                If url_parse_result.is_new_app_instance Then
                    If Not TriniDATServerEvent.onApplicationCreated(url_parse_result) Then
                        GlobalObject.Msg("Aborting.")
                        http_connie.setHTTPResponse(404)
                        http_connie.writeRaw(True, "Internal Server error. Session creation failed.", True)
                        Return False
                    End If
                End If

                Dim new_thread_result As MP_THREAD_CREATE_RESULT

                new_thread_result = Me.createMappingRootInstance(url_parse_result, packet, packetSize)

                'MP_THREAD_CREATE_RESULT.ERR_MAXTHREAD_EXCEEDED

                If new_thread_result = MP_THREAD_CREATE_RESULT.ERR Then
                    If url_parse_result.http_connection_handler.getConnection().isConnected() Then
                        'output error message if not done so.
                        Dim err_msg As ChainResult
                        err_msg = New ChainResult
                        err_msg.state_http_code = 500
                        err_msg.state_message = "Initialization of " & url_parse_result.mapping_point_desc.URL & " was aborted."
                        err_msg.state_solution_message = "Resend Request"
                        err_msg.HTTP_OutputState(url_parse_result.http_connection_handler)
                    End If
                    Return False
                ElseIf new_thread_result = MP_THREAD_CREATE_RESULT.ERR_MAXTHREAD_EXCEEDED Then
                    'user has no threads left.
                    Me.showServiceUnAvailable(url_parse_result)
                    Return False
                End If
            End If

            Return True

        Catch ex As Exception
            MSG("firstRequestHandler error: " & ex.Message & "    METHOD=" & URI_method & " URI=" & URI)
        End Try

    End Function

    Public Sub showServiceUnAvailable(ByVal info As TriniDATRequestInfo)

        'Too Many Requests
        info.http_connection_handler.setHTTPResponse(429)
        info.http_connection_handler.writeRaw(True, "Amount of active threads for user '" & info.direct_session.relatedUser.Username & "' reached.", True)

    End Sub

    Public Sub FlushSessions()

        For Each usr In Me.Users
            If usr.haveSession Then
                GlobalObject.MsgColored("Flushing sessions for user " & usr.Username & "...", Color.Red)
                usr.Sessions.Clear()
            End If
        Next

    End Sub

    Public Function outputApplicationIndex(ByVal httpconnie As TriniDATClientConnectionManagerHTTP)
        Call httpconnie.sendRedirect("/1")
        Return True
    End Function

    Private Function outputApplicationIndexOLD(ByVal httpconnie As TriniDATClientConnectionManagerHTTP)
        ''context: thread

        'Dim x As Integer
        'Dim app_index As BosswaveApplicationIndex
        'Dim IndexTitle As String
        'Dim entryHTML_doc_template As String
        'Dim entryHTML_row_template As String
        'Dim entryHTML_table_template As String

        'IndexTitle = "<h1>TriniDAT Data Server<BR></h1>"
        'IndexTitle &= "<h4>=========================</h4>"
        'IndexTitle &= "<h3>(c) 2013 De Leeuw ICT<h3>"
        'IndexTitle &= "<h2>Active on " & Me.HTTPConfiguration.server_ip.ToString & " port " & Me.HTTPConfiguration.server_port.ToString & "</h2>"
        'IndexTitle &= "<BR>Application Index"

        'entryHTML_doc_template = "<html><body>$TITLE<br>$TABLE</body></html>"
        'entryHTML_table_template = "<table><tr>$row</tr></table>"

        'app_index = New BosswaveApplicationIndex


        'For x = 0 To GlobalObject.ApplicationCache.Count - 1
        '    '  Me.lstProgram.Items.Add(Me.app_index.Apps(x).ApplicationName)
        '    entryHTML_row_template = "<td><a href='/" & GlobalObject.ApplicationCache.Item(x).Id.ToString & "/'>" & GlobalObject.ApplicationCache.Item(x).ApplicationName & "</a></td>"
        '    entryHTML_table_template = Replace(entryHTML_table_template, "$ROW", entryHTML_row_template & "$ROW")
        '    httpconnie.addOutput(entryHTML_row_template)
        'Next

        'entryHTML_table_template = Replace(entryHTML_table_template, "$ROW", "")
        'entryHTML_doc_template = Replace(entryHTML_doc_template, "$TABLE", entryHTML_table_template)
        ''set title
        'entryHTML_doc_template = Replace(entryHTML_doc_template, "$TITLE", IndexTitle)

        'httpconnie.writeRaw(True, entryHTML_doc_template, True)
        'Return True
    End Function

    Public Function outputInterfaceDefinition(ByVal info As TriniDATRequestInfo)

        Dim x As Integer
        Dim app_index As BosswaveAppCache
        Dim pageHeader As String
        Dim IndexTitle As String
        Dim entryHTML_doc_template As String
        Dim entryHTML_row_template As String
        Dim entryHTML_table_template As String
        Dim allrows As String
        Dim row_index As Long

        pageHeader = "<title>TriniDAT Server (c) 2013 De Leeuw ICT</title>"
        IndexTitle = "<h2>Webservice '" & info.associated_app.ApplicationName & "' Interface Definitions</h2>"

        entryHTML_doc_template = "<html><head>$HEADER</head><body>$TITLE<br>$TABLE</body></html>"
        entryHTML_table_template = "<h2>Overridable Mapping Points</h2><table>$ROWS</table>"

        allrows = ""
        row_index = 1
        app_index = New BosswaveAppCache

        If info.associated_app.haveMappingPoints Then
            For Each mp_descriptor In info.associated_app.ApplicationMappingPoints
                '  Me.lstProgram.Items.Add(Me.app_index.Apps(x).ApplicationName)
                entryHTML_row_template = "<tr><td>" & row_index.ToString & ".</td><td><a href=" & Chr(34) & info.associated_app.Id.ToString & mp_descriptor.URL & "(definition)" & Chr(34) & ">" & mp_descriptor.URL & "</a></td></tr>"
                entryHTML_row_template &= "<tr><td>&nbsp;</td><td>&nbsp;</td></tr>"
                allrows &= entryHTML_row_template
                row_index += 1
            Next
        End If

        entryHTML_table_template = Replace(entryHTML_table_template, "$ROWS", allrows)
        entryHTML_doc_template = Replace(entryHTML_doc_template, "$TABLE", entryHTML_table_template)
        entryHTML_doc_template = Replace(entryHTML_doc_template, "$HEADER", pageHeader)
        'set title
        entryHTML_doc_template = Replace(entryHTML_doc_template, "$TITLE", IndexTitle)
        'entryHTML_doc_template
        info.http_connection_handler.addOutput(entryHTML_doc_template)
        info.http_connection_handler.Flush(True, True)

        Return True
    End Function
    Public Shared Function getServerTypeCreatorGate() As object_creator
        Return AddressOf JServiceLauncher.getCoreType
    End Function
    Public Function canCreateThread(ByVal info As TriniDATRequestInfo) As Boolean

        If Me.Realtime_ThreadCountByUser(info.direct_session) + 1 > info.direct_session.relatedUser.MaxThreadCount Then
            Return False
        End If

        Return True

    End Function
    Public Function createMappingRootInstance(ByVal info As TriniDATRequestInfo, ByVal packet() As Byte, ByVal packetSize As Long) As MP_THREAD_CREATE_RESULT

        If Not serverRunning Or Not info.direct_socket.isConnected() Or Not info.haveMappingPoint Then
            Return MP_THREAD_CREATE_RESULT.ERR
        End If

        'fill boot info 
        Dim mapping_point_bootinfo As mapping_point_threading_parameters
        mapping_point_bootinfo = New mapping_point_threading_parameters

        mapping_point_bootinfo.bootInfo = info
        mapping_point_bootinfo.bootPacket = packet
        mapping_point_bootinfo.bootPacketsize = packetSize


        If Not canCreateThread(info) Then
            Return MP_THREAD_CREATE_RESULT.ERR_MAXTHREAD_EXCEEDED
        End If

        If mapping_point_bootinfo.bootInfo.mapping_point_desc.haveActiveMappingPointThread() Then

            Dim avg_delay_msec As Long

            avg_delay_msec = mapping_point_descriptor.getWaitThreadInterval(mapping_point_bootinfo.bootInfo.mapping_point_desc)

            'create waiter thread
            If GlobalSetting.MULTI_THREADED_WAIT = False Then

                If mapping_point_bootinfo.bootInfo.mapping_point_desc.haveActiveWaitThread Then
                    'another waiting thread is in progress, we have to wait..

                    MSG(mapping_point_bootinfo.bootInfo.mapping_point_desc.URL.ToString & ": sleep " & avg_delay_msec.ToString & " ms for wait thread...")
                    'sleep until waiter thread is finished

                    If mapping_point_bootinfo.bootInfo.mapping_point_desc.shouldKillActiveThread() Then
                        Dim killResult As Boolean
                        'ensure timestamp is fresh
                        mapping_point_bootinfo.bootInfo.mapping_point_desc.AddRuntimeRecord(GlobalObject.GetTickCount())
                        killResult = mapping_point_bootinfo.bootInfo.mapping_point_desc.KillActiveThread()
                    Else
                        Threading.Thread.Sleep(100)
                    End If

                    'retry
                    Return createMappingRootInstance(info, packet, packetSize)
                End If
            Else

                If mapping_point_bootinfo.bootInfo.mapping_point_desc.shouldKillActiveThread() Then
                    Dim killResult As Boolean
                    'ensure timestamp is fresh
                    mapping_point_bootinfo.bootInfo.mapping_point_desc.AddRuntimeRecord(GlobalObject.GetTickCount())
                    killResult = mapping_point_bootinfo.bootInfo.mapping_point_desc.KillActiveThread()
                    'retry normal
                    Return createMappingRootInstance(info, packet, packetSize)
                End If

                mapping_point_bootinfo.multi_threaded_waitthread = True
                'create waiting thread
                Dim mapping_point_startup_thread As New Thread(AddressOf mapping_point_descriptor.CPUWait)
                mapping_point_startup_thread.Start(mapping_point_bootinfo)

                If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                    MSG("Queuing " & mapping_point_bootinfo.bootInfo.mapping_point_desc.URL & ". avg time: " & mapping_point_bootinfo.bootInfo.mapping_point_desc.getAveragetimeSeconds.ToString & " sec. Wait frequency: " & avg_delay_msec.ToString & " ms. Seen: " & mapping_point_bootinfo.bootInfo.mapping_point_desc.boot_milliseconds.Count.ToString & ".")
                End If

                Return MP_THREAD_CREATE_RESULT.NEW_WAIT_THREAD
            End If

        Else
            'no active mapping point thread
            MSG(mapping_point_bootinfo.bootInfo.mapping_point_desc.URL.ToString & ": Starting mapping point...")
            'fire startup thread directly from current thread.
            mapping_point_bootinfo.multi_threaded_waitthread = False
            mapping_point_descriptor.CPUWait(mapping_point_bootinfo)
            Return MP_THREAD_CREATE_RESULT.NEW_DIRECT_THREAD
        End If

    End Function


    Public Function FetchAndParseData(ByRef socket As TriniDATSockets.TriniDATTCPSocket) As Boolean

        'MSG("FetchAndParseData socket #" & socket.getSocket().Client.Handle.ToString)

        Dim stream As NetworkStream

        If Not socket.canReceive() Then
            MSG("FetchData: Dead socket passed.")
            Return False
        Else
            stream = socket.GetStream()
        End If

        'start receiving data.
        Dim packet(Me.HTTPConfiguration.receive_buffer_size) As Byte
        Dim packetSize As Long

        Try

            'This part is the actual HTTP server.
            stream.ReadTimeout = Me.HTTPConfiguration.socket_timeout
            'any read error is caused by JOmega
            packetSize = stream.Read(packet, 0, Me.HTTPConfiguration.receive_buffer_size)

            If packetSize = 0 Then
                MSG("Closing link")
                Exit Function
            End If

        Catch ex As Exception

            Debug.Print(ex.Message)

            If socket.isConnected() Then
                MSG(" error on connected socket #" & socket.getSocket().Client.Handle.ToString & ": " & ex.Message)
                socket.forceDisconnect()
            End If

            Exit Function
        End Try


        'Connection is not mapped yet, so filter request.

        If packetSize > 4 Then
            'First request: Find mapping point first, then pass to OnMappedDataIncomingd.
            Return Me.firstRequestHandler(socket, packet, packetSize)
        Else
            MSG("Dropping poacket. Should have been handled by mapping root if it even exists...")
        End If


        Return False
    End Function


    Public Sub OnInvalidPacket(ByVal rawPacket As String, ByVal Reason As String)
        msg("Warning: dropped invalid packet: reason=" & Reason & " packet=" & rawPacket & " size=" & rawPacket.Length)
    End Sub

    Public Sub OnDebugURI(ByRef con As TriniDATSockets.TriniDATTCPSocket, ByVal rawPacketBytes As Byte(), ByVal packetLen As Long)
        'TODO: OBSOLETE -> CONVERT ALL TO JCLASSES.


    End Sub

End Class


Public Structure TriniDATProtocols
    Public Const HTTP As Integer = 0
End Structure

Public Enum MP_THREAD_CREATE_RESULT
    ERR_MAXTHREAD_EXCEEDED = -1
    ERR = 0
    NEW_WAIT_THREAD = 1
    NEW_DIRECT_THREAD = 2
End Enum