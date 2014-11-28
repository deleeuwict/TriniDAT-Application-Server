'Declares a URI handler class in header style .
Option Explicit On
Option Compare Text

Imports Newtonsoft.Json
Imports System.Collections.Specialized
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Net
Imports System.Reflection
Imports TriniDATServerTypes
Imports System.Security
Imports System.Security.Policy
Imports System.Security.Permissions
Imports System.Runtime.Remoting
Imports System.IO

Public Class mappingPointRoot

    Public Const SystemReservedClassname As String = "<INTERNAL>"

    Private index As Integer
    Private Classes As List(Of mappingPointClass) 'e.g. frmCodeEditor . Class must implement a set of default JSON event handlers.
    Private description_text As String
    Private URI As String
    Private TriggerURI As String 'the actual URL that made this service instantiate. Likely to be a child URL of the configured mapping url.

    Private processes As List(Of mappingPointInstanceInfo)
    Public Initializor As Type 'a Public Shared class containing functions 'Initialize' and 'NeedInitialization'
    Private prefferedEncoding As Encoding
    Private isdebugged As Boolean 'does not accept HTTP requests when true
    Public isUniqueMappingPoint As Boolean

    Public boot_time_start_ms As Long
    Private dep_load_state As DEPENDENCY_LOADSTATE
    Private dep_err_dll As String

    Private mp_appdomain As AppDomain
    Private bootInfo As TriniDATRequestInfo
    Public bootPacket As Byte()
    Public bootPacketsize As Long

    Public jkernel_class_instantation_dependencies As List(Of String)

    'EX: 
    'http://localhost/admin/jsonstorage/add/JHyperlink <- trigger all handlers by priority
    'http://localhost/admin/jsonstorage/add/JWordpressLink
    'http://localhost/admin/jsonstorage/add/JDISQUSLink
    'http://localhost/admin/jsonstorage/add/JAmazonLink
    '
    Public Sub New(ByVal _info As TriniDATRequestInfo, Optional ByVal _classes_to_instantiate As List(Of mappingPointClass) = Nothing, Optional ByVal _encoding As System.Text.Encoding = Nothing, Optional ByVal _Initializor As Type = Nothing, Optional ByVal _external_asm_dependencies As List(Of String) = Nothing)
        'INIT IMPORTANT CONSTANTS.
        Me.Info = _info
        Me.Description = ""
        Me.isDebug = False

        'default encoding 
        If IsNothing(_encoding) Then
            _encoding = New ASCIIEncoding
        End If

        Me.prefferedEncoding = _encoding
        Me.Initializor = _Initializor

        'INITIALIZE PROCESS LIST HOLDER
        '======================

        processes = New List(Of mappingPointInstanceInfo)


        '======================
    
        If IsNothing(_external_asm_dependencies) Then
            _external_asm_dependencies = New List(Of String)
        End If

        'paths to externally required DLLs in order to create class instances from JKernel.
        Me.DependencyList = _external_asm_dependencies

        '  Me.setAsResidential(_isResidentialChain)

        If Not IsNothing(_classes_to_instantiate) Then
            'CLASS LIST PASSED AS PARAMETER - COPY
            Me.Classes = _classes_to_instantiate
        Else
            Me.Classes = New List(Of mappingPointClass)
        End If

    End Sub
    Private Property DepState As DEPENDENCY_LOADSTATE
        Get
            Return Me.dep_load_state
        End Get
        Set(ByVal value As DEPENDENCY_LOADSTATE)
            Me.dep_load_state = value
        End Set
    End Property
    Private Property DepErrItem As String
        Get
            Return Me.dep_err_dll
        End Get
        Set(ByVal value As String)
            Me.dep_err_dll = value
        End Set
    End Property
    Public Property Info() As TriniDATRequestInfo
        Get
            Return Me.bootInfo
        End Get
        Set(ByVal value As TriniDATRequestInfo)
            Me.bootInfo = value
        End Set
    End Property
    Public Property Description() As String
        Get
            Return Me.description_text
        End Get
        Set(ByVal value As String)
            description_text = value
        End Set
    End Property
    Public Property DependencyList As List(Of String)
        Get
            Return Me.jkernel_class_instantation_dependencies
        End Get
        Set(ByVal value As List(Of String))
            'MappingPointASMCollection
            Me.jkernel_class_instantation_dependencies = value

            'start loading asm instances.
            If Not Me.HaveDependencyList Then Exit Property

            For Each dep_path As String In Me.jkernel_class_instantation_dependencies
                If Not GlobalMappingPointASMCollection.AddByFilepath(dep_path) Then
                    Me.dep_load_state = DEPENDENCY_LOADSTATE.DL_ERR
                    Exit Property
                End If
            Next

        End Set
    End Property



    Public ReadOnly Property getDependencyPaths(Optional ByVal files_only As Boolean = False) As List(Of String)
        Get
            If Not files_only Then
                Return Me.jkernel_class_instantation_dependencies
            Else
                Dim retval As List(Of String)
                retval = New List(Of String)

                For Each dep In Me.jkernel_class_instantation_dependencies

                    If InStr(dep, "=") = 0 Then
                        retval.Add(dep)
                    End If

                Next

                Return retval
            End If

        End Get
    End Property
    Public Property EncodingPreference() As Encoding
        Get
            Return Me.prefferedEncoding
        End Get
        Set(ByVal value As Encoding)
            Me.prefferedEncoding = value
        End Set
    End Property
    Public ReadOnly Property ApplicationId() As String
        Get

            Return Me.Info.associated_app.Id
        End Get
    End Property

    Private Sub Msg(ByVal txt As String)
        Debug.Print("mappingPointRoot @ " & Me.getURI() & "]: " & txt)

    End Sub
    Public Function FeedStreamProcesses(ByRef connie As TriniDATSockets.TriniDATTCPSocket, ByVal packet As Byte(), ByVal packetSize As Long) As Boolean

        Dim proc As mappingPointInstanceInfo
        Dim streamfunc As TriniDATHTTP_EventTable.OnIncomingStreamingData


        For x = 0 To Me.processesCount - 1

            proc = Me.getProcessByIndex(x)

            If Not IsNothing(proc) Then

                Dim jproc As JTriniDATWebService
                jproc = proc.getProcess()

                If Not IsNothing(jproc) Then
                    'check if stream compatible.
                    If jproc.getIOHandler().supportsNetworkStream() Then
                        'pass packet
                        streamfunc = jproc.getIOHandler().getStreamingHandler()

                        If Not IsNothing(packet) Then
                            Call streamfunc(BytetoString(packet, packetSize))
                        Else
                            Call streamfunc(Nothing)
                        End If

                    End If
                End If
            End If

        Next x

        Return True

    End Function

    Public ReadOnly Property getAlphaObject() As Object
        Get
            Dim proc As mappingPointInstanceInfo

            proc = Me.getProcessByClassName("JAlpha", True)
            If Not IsNothing(proc) Then
                Return proc.getProcess()
            Else
                Return Nothing
            End If

        End Get
    End Property

    Public ReadOnly Property getOmegaObject() As Object
        Get
            Dim proc As mappingPointInstanceInfo

            proc = Me.getProcessByClassName("JOmega", True)
            If Not IsNothing(proc) Then
                Return proc.getProcess()
            Else
                Return Nothing
            End If

        End Get
    End Property
    Private Function BytetoString(ByVal packet() As Byte, ByVal packetsize As Long) As String

        Dim retval As String

        retval = Me.prefferedEncoding.GetString(packet)

        Return retval
    End Function

    Public Function doInitializor() As Boolean
        'Initializes the tyype specified in MappingPointroot.Initializor
        Dim classInfo As System.Type
        Dim class_global_method As MethodInfo
        Dim logger As TriniDATHTTPTypes.TriniDATTypeLogger
        Dim call_init As Boolean

        logger = AddressOf Me.Msg
        classInfo = Me.Initializor
        call_init = False

        Try

            If Not IsNothing(classInfo) Then
                Msg("Initializor: initializing type " & Me.Initializor.ToString)
                class_global_method = classInfo.GetMethod("needsInitialization")
                If Not IsNothing(class_global_method) And class_global_method.ReturnType = GetType(Boolean) Then
                    'CALL A STATIC METHOD THAT RETURNS AN BOOL

                    call_init = class_global_method.Invoke(Nothing, Nothing)
                End If

                If call_init Then
                    class_global_method = classInfo.GetMethod("Initialize")
                    class_global_method.Invoke(Nothing, New Object() {logger})
                End If
            End If

            Msg("Initializor OK: initialized type " & Me.Initializor.ToString)
            Return True

        Catch ex As Exception
            Msg("Initializor err: Error initializing type " & Me.Initializor.ToString & ": " & ex.Message)
        End Try

    End Function
    Public ReadOnly Property HaveDependencyList() As Boolean
        Get
            If IsNothing(Me.jkernel_class_instantation_dependencies) Then Return False

            Return Me.getDependencyPaths().Count > 0

        End Get
    End Property
    Public ReadOnly Property haveInitializor As Boolean
        Get
            Return Not IsNothing(Me.Initializor)
        End Get
    End Property

    Private ReadOnly Property isStreamingEnabled() As Boolean
        'false if no jservice process supports streaming.
        Get
            Dim proc As mappingPointInstanceInfo

            For x = 1 To Me.processesCount - 2 'skip JOmega and JAlpha

                proc = Me.getProcessByIndex(x)

                If Not IsNothing(proc) Then

                    Dim jproc As JTriniDATWebService
                    jproc = proc.getProcess()

                    If Not IsNothing(jproc) Then
                        'check if stream compatible.
                        If jproc.getIOHandler().supportsNetworkStream() Then
                            Return True
                        End If
                    End If
                End If

            Next x

            Return False
        End Get

    End Property


    Private Sub invokeJOmega()
        Dim jomega As JOmega
        Dim config As MappingPointBootstrapData

        config = Me.getJAlphaConfig
        jomega = Me.getOmegaObject()

        Call jomega.MappingPointStop(config)
    End Sub
    Public ReadOnly Property HaveRequestInfo() As Boolean
        Get
            Return Not IsNothing(Me.bootInfo)
        End Get
    End Property
    Public ReadOnly Property HaveSession() As Boolean
        Get
            If Not HaveRequestInfo Then Return False
            Return Not IsNothing(Me.Info.direct_session)
        End Get
    End Property
    Public ReadOnly Property Session() As TriniDATUserSession
        Get
            Return Me.Info.direct_session
        End Get
    End Property

    Public Function validateRequestInfo() As Boolean

        If Not HaveRequestInfo Then
            Msg("boot info invalid. Exiting.")
            Return False
        End If

        If Not Me.Info.haveMappingPoint Then
            Msg("boot mp invalid. Exiting.")
            Return False
        End If


        If Not Me.Info.haveConnection Then
            Msg("boot socket info invalid. Exiting.")
            Return False
        End If

        Return True
    End Function

    Public Sub bootHTTPChain()
        'initializer for stateless mapping points.
        'called by HTTP firstRequest
        Dim boot_result As ChainResult
        boot_result = New ChainResult


        boot_result.state = ChainResultState.NEUTRAL
        boot_result = TriniDATServerEvent.OnMappingPointStarted(Me, boot_result)


        If Not validateRequestInfo() Then
            boot_result.state = ChainResultState.FATAL_ERROR
            Msg("boot HaveRequestInfo invalid structure. Exiting.")
        End If


        If Me.dep_load_state = DEPENDENCY_LOADSTATE.DL_ERR Then
            boot_result.state = ChainResultState.FATAL_ERROR
            boot_result.state_message = "Dependency load failure: " & Me.dep_err_dll
        End If

        If boot_result.state = ChainResultState.OK Then
            If haveInitializor Then
                'warning: the initializor is only invoked once in application life-time.
                Call doInitializor()
            End If

            boot_result = Me.OpenChainProcesses()
        End If


        If CType(boot_result.state, Integer) < 0 Then
            GlobalObject.MsgColored("application '" & Me.Info.associated_app.ApplicationName & "': initialization failure while configuring " & Me.getURI() & ".", Color.DarkRed)
            boot_result.state_http_code = 500
            boot_result.state_message = "Unable to create mapping point '" & Me.getURI() & "' for application '" & Me.Info.associated_app.ApplicationName.ToString & "'.<BR><BR>Application initialization failure: " & boot_result.state_message
            If boot_result.state_solution_message <> "none" Then
                boot_result.state_message &= "<BR><BR><b>Fix Suggestion:</b><BR>" & boot_result.state_solution_message
            End If
            boot_result.HTTP_OutputState(Info.http_connection_handler)

            Call TriniDATServerEvent.OnMappingPointStopped(Me, True)
            Exit Sub
        ElseIf boot_result.state = ChainResultState.SOCKET_CLOSED_PACKET Or boot_result.state = ChainResultState.OK Then
            'nothing else left to do
            Call invokeJOmega()
            Call TriniDATServerEvent.OnMappingPointStopped(Me, False)

            Exit Sub
        Else
            Call TriniDATServerEvent.OnMappingPointStopped(Me, False)

            Exit Sub
        End If

        Dim Streaming As Boolean

        Streaming = Me.isStreamingEnabled()

        If Not Streaming Then
            Call invokeJOmega()
            Call TriniDATServerEvent.OnMappingPointStopped(Me, False)
            Exit Sub 'done
        End If



        If (Not Me.Info.direct_socket.disconnectSocket()) And Me.Info.direct_socket.canReceive() = True And Me.Info.direct_socket.Receive_Bytes_Left > 0 Then


            'BELOW IS ONLY USEFUL CODE FOR STREAMING HTTP (BINARY FILE POSTS ETC)
            Dim stream As NetworkStream

            stream = Me.Info.direct_socket.GetStream()
            stream.ReadTimeout = GlobalObject.CurrentServerConfiguration.socket_timeout

            'start receiving data.
            Dim packetSize As Long
            Dim packet(1) As Byte

            Try


                While (Not Me.Info.direct_socket.disconnectSocket()) And Me.Info.direct_socket.isConnected() And Me.Info.direct_socket.canReceive()


                    ReDim packet(Me.Info.direct_socket.Receive_Bytes_Left)

                    packetSize = 0

                    'This part is the actual HTTP server.
                    packetSize = stream.Read(packet, 0, Me.Info.direct_socket.Receive_Bytes_Left)

                    If packetSize > 0 Then
                        ReDim Preserve packet(packetSize)
                        Call Me.FeedStreamProcesses(Me.Info.direct_socket, packet, packetSize)

                        'update byte stats
                        Me.Info.direct_socket.Receive_Bytes_Left = (Me.Info.direct_socket.Receive_Bytes_Left - packetSize) + 1
                    End If

                    If Me.Info.direct_socket.Receive_Bytes_Left = 1 Then
                        'workaround for byte counting problem but works...
                        '  me.info.direct_socket.Receive_Bytes_Left = 1000
                        Exit While
                    ElseIf Me.Info.direct_socket.Receive_Bytes_Left <= 0 Or packetSize = 0 Then
                        Msg("Stream done. Closing link")
                        Exit While
                    End If
                End While
            Catch ex As Exception
                Msg("Read error on stream socket.")
                If Me.Info.direct_socket.isConnected() Then
                    Me.Info.direct_socket.forceDisconnect()
                End If
            End Try
        End If

        If Not Me.Info.direct_socket.disconnectSocket() Then
            'send NULL packet to stream-enabled processes to signal end of connection.
            Me.FeedStreamProcesses(Me.Info.direct_socket, Nothing, Nothing)
        End If

        Call invokeJOmega()
        Call TriniDATServerEvent.OnMappingPointStopped(Me, False)
    End Sub

    Private ReadOnly Property getJAlphaConfig() As MappingPointBootstrapData
        Get
            Dim jalpha_config As MappingPointBootstrapData

            jalpha_config = New MappingPointBootstrapData

            'give some file paths.
            jalpha_config.applicationid = Me.Info.associated_app.Id.ToString
            jalpha_config.application_file = Me.Info.associated_app.Filepath
            jalpha_config.application_folder = Me.Info.App.HomeFolder.FullName
            jalpha_config.static_path = GlobalSetting.getStaticDataRoot()
            jalpha_config.my_session_path = Me.Info.session_path

            jalpha_config.server_mode = GlobalObject.server.ServerMode
            jalpha_config.mapping_point_url = Me.Info.FullMappingPointPath
            Return jalpha_config
        End Get
    End Property
    'wrapper for JTriniDATWebService apps.
    Public Function getApplicationURL() As String
        Return Me.ApplicationURL
    End Function

    Public ReadOnly Property ApplicationURL As String
        Get
            If Not HaveSession Then Return "err"
            Dim retval As String

            retval = Me.Info.relativeApplicationURL
            retval &= Mid(Me.getURI(), 2)
            Return retval

        End Get
    End Property

    Private Function OpenChainProcesses() As ChainResult  '(ByRef connie As TriniDATSockets.TriniDATTCPSocket, ByVal packet As Byte(), ByVal packetSize As Long) As ChainResult

        Dim proc As mappingPointInstanceInfo
        Dim mp_boot_parameters As MappingPointBootstrapData
        Dim process_index As Integer
        Dim JClassInfo As mappingPointClass
        Dim jprocess As JTriniDATWebService
        Dim retval As ChainResult
        Dim jalpha As JAlpha
        retval = New ChainResult

        'assemblies get loaded when DepedencyList property is set (constructor).
        If Me.DepState = DEPENDENCY_LOADSTATE.DL_ERR Then
            retval.state = ChainResultState.FATAL_ERROR
            retval.state_message = "Unable to load '" & Me.dep_err_dll & " "
            If File.Exists(Me.dep_err_dll) Then
                retval.state_message &= ". The file may be locked."
            Else
                retval.state_message &= "'because the file does not exist."
            End If

            Return retval
        End If

        'load mp startup data.
        mp_boot_parameters = Me.getJAlphaConfig

        'parse post data in this thread.
        If Me.Info.direct_socket.IsPostRequest Then
            Me.Info.http_connection_handler.setParameters(Me.Info.http_connection_handler.parsePOSTParameters(System.Text.Encoding.ASCII.GetString(Me.bootPacket)))
        End If

        Me.Info.http_connection_handler.Session = Me.Info.direct_session

        'strategy: give only JOmega the master socket.

        Try

            Msg("OpenChainProcesses: Creating " & Me.getClassCount().ToString & " instance(s) for " & Me.URI & "  current process_count= " & Me.processesCount.ToString & ").")
            Msg("=================================")
            JClassInfo = Nothing

            '1. Create process and HTTP handler
            For x = 0 To Me.getClassCount() - 1

                JClassInfo = Me.getClassByIndex(x)
                retval.state_affected_jclass_name = JClassInfo.getName()

                'Create a new instance.
                Msg("OpenChainProcesses: creating new instance of " & JClassInfo.getName() & " for URI " & Me.URI & "...")

                process_index = -1

                If Not Info.direct_socket.isConnected() Or Info.direct_socket.disconnectSocket() = True Then
                    Msg("OpenChainProcesses: Closing chain. Socket is already closed.")
                    retval.state = ChainResultState.SOCKET_CLOSED_CREATE
                    Return retval
                End If

                'instantiate the JClass and call its constructor.
                'pass packet as byte array
                jprocess = JServiceLauncher.createJService(JClassInfo.getName())

                If IsNothing(jprocess) Then

                    'create remote type.
                    Dim jprocess_type As Type

                    'assembly list is initiailized in the constructor.
                    jprocess_type = GlobalMappingPointASMCollection.getAsmType(JClassInfo.getName())

                    jprocess = JServiceLauncher.createJService(jprocess_type)

                    If Not IsNothing(jprocess) Then
                        jprocess.setServerTypeCreatorGate(AddressOf JServiceLauncher.getCoreType)
                    End If

                End If

                'not local or remote type.
                If IsNothing(jprocess) Then
                    retval.state_message = "Class '" & JClassInfo.getName() & "' not found or one of the initialization routines generated unknown errors."
                    retval.state_solution_message = "1) Make sure all your TriniDAT-specific initialization routines in '" & JClassInfo.getName() & "' return a boolean true. <BR><BR>2) Add the missing DLL that contains the class declaration of '" & JClassInfo.getName() & "' and other dependencies as new mapping point dependency in the application '" & Me.Info.associated_app.ApplicationName & "' definition file. (" & Me.Info.associated_app.Filepath & ")<BR><BR>3. Make sure no application files are being locked.<BR><BR>4. If '" & JClassInfo.getName & "' exists, it might be linking with outdated or unavailable external libraries.<BR><BR>5. Reload the application cache.<BR><BR>6. Check the console log for more information (developer mode only)."
                    Msg(retval.state_message)
                    retval.state = ChainResultState.FATAL_ERROR
                    Return retval
                End If

                If Not IsNothing(jprocess) Then

                    'add to mappingpointInfo collection entry
                    process_index = Me.AddInstance(JClassInfo, jprocess)
                    'report instance id to the created process itself
                    Dim process_info As mappingPointInstanceInfo
                    process_info = Me.getInstanceInfo(process_index)

                    'add protocol handler

                    If jprocess.isJOmega Then
                        'give master socket
                        process_info.setProtocolHandler(Info.http_connection_handler)
                    Else
                        'give slave socket
                        process_info.setProtocolHandler(New TriniDATClientConnectionManagerHTTP(Info.http_connection_handler))
                    End If

                    'attach startup parameters to process from URL
                    If Me.Info.direct_socket.haveParameters() Then
                        process_info.setParameters(Me.Info.direct_socket.Parameters)
                        process_info.getProtocolHandler().setParameters(process_info.getParameters())
                    End If

                    process_info.getProtocolHandler().setparent(process_info)

                    'hand over process info to JService
                    Call process_info.getProcess().setProcessDescriptor(process_info)

                    'set object server info
                    process_info.getProcess().ObjectServer = GlobalObject.CurrentServerConfiguration.server_ip.ToString
                    process_info.getProcess().ObjectServerPort = GlobalObject.CurrentServerConfiguration.server_port
                Else
                    Throw New Exception("Something is wrong. Process creation failed.")
                    ' Exit For

                End If

            Next 'class


            '4. ACTIVATE ALL CONFIGURATION PROCS

            For x = 0 To Me.processesCount - 1


                If Not Me.Info.direct_socket.isConnected() Or Me.Info.direct_socket.disconnectSocket() = True Then
                    Msg("OpenChainProcesses: Aborting. Socket is already closed.")
                    retval.state = ChainResultState.SOCKET_CLOSED_CONFIG
                    Return retval
                End If

                proc = Me.getProcessByIndex(x)

                If Not IsNothing(proc) Then
                    retval.state_affected_jclass_name = proc.getClassInfo().getName()

                    Msg("OpenChainProcesses: Configuring PROCESS # " & x.ToString & " / " & proc.getClassInfo().getName() & " on URI " & Me.URI & "...")

                    'invoke service initialization procs.
                    If Not proc.getProcess().DoConfigure() Then
                        retval.state = ChainResultState.FALSE_CONFIGURE
                        retval.state_message = "<b>Your DoConfigure event should return 'true'.</b>"
                        Return retval
                    End If

                    If Not proc.getProcess().doOnRegisterWebserviceFunctionTableEvent() Then
                        retval.state = ChainResultState.FALSE_FUNCTION_TABLE
                        retval.state_message = "<b>Your OnRegisterWebserviceFunctionTableEvent event should return 'true'.</b>"
                        Return retval
                    End If

                    If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then


                        'verbose output some changes made.
                        If proc.getProcess().haveWebserviceFunctionTable And Left(proc.getProcess().getClassName(), 8) <> "TriniDAT" Then
                            'note does NOT log proc status in internal classes.

                            Dim havePOST_procs As Boolean
                            Dim haveGET_procs As Boolean
                            Dim proc_status_msg As String

                            havePOST_procs = False
                            haveGET_procs = False

                            For Each user_POSTmethod As TriniDAT_ServerPOSTFunction In proc.getProcess().WebserviceFunctionTable.AllpostFunctions()
                                'invoke
                                GlobalObject.MsgColored("'" & Info.associated_app.ApplicationName & "' => '" & Info.recreateRelativeURL & "': User-defined '" & user_POSTmethod.FunctionURL & "' POST proc successfully set @ '" & proc.getClassInfo().getName() & ":" & Info.recreateRelativeURL & "'....", Color.Gold)
                                havePOST_procs = True
                            Next

                            For Each user_GETmethod As TriniDAT_ServerGETFunction In proc.getProcess().WebserviceFunctionTable.AllGETFunctions()
                                'invoke
                                GlobalObject.MsgColored("'" & Info.associated_app.ApplicationName & "' => '" & Info.recreateRelativeURL & "': User-defined '" & user_GETmethod.FunctionURL & "' GET proc successfully set @ '" & proc.getClassInfo().getName() & ":" & Info.recreateRelativeURL & "'....", Color.Gold)
                                haveGET_procs = True
                            Next

                            proc_status_msg = "Notice: '" & Info.associated_app.ApplicationName & "' => '" & Info.recreateRelativeURL

                            If haveGET_procs And Not havePOST_procs Then
                                GlobalObject.MsgColored(proc_status_msg & "': No specialized POST handlers in '" & proc.getClassInfo().getName() & ":" & Info.recreateRelativeURL & "...", Color.Black)
                            ElseIf havePOST_procs And Not haveGET_procs Then
                                GlobalObject.MsgColored(proc_status_msg & "': No specialized GET handlers in '" & proc.getClassInfo().getName() & ":" & Info.recreateRelativeURL & ".", Color.Black)
                            Else
                                GlobalObject.MsgColored(proc_status_msg & "': No specialized URI handlers in '" & proc.getClassInfo().getName() & ":" & Info.recreateRelativeURL & ".", Color.Black)
                            End If

                        End If
                    End If

                Else
                    retval.state_message = "An error occured in the configure stage of " & JClassInfo.getName() & "."
                    retval.state = ChainResultState.FATAL_ERROR
                    Return retval
                End If

            Next x

            '5. JAlpha Broadcast w/ Parameter list
            '==============
            jalpha = Me.getAlphaObject()
            jalpha.MPConfig = mp_boot_parameters
            Call jalpha.MappingPointStart()

      

                '4. OPTIONAL: PASS ENCODED JSON MESSAGE AROUND
                If Me.Info.http_connection_handler.haveHeader("X-MappingPoint-Object") Then

                    Dim json_encoded_simple_jsonobject As String
                    Dim dserialized_object As Object
                    Dim object_sender_classname As String
                    Dim object_recipient_classname As String
                    Dim object_interactive_replyforwarding_classname As String
                    Dim orginating_message_process As Object
                    Dim orginating_message_process_info As mappingPointInstanceInfo
                    Dim orginating_message_process_mailbox As JServicedMailBox
                    Dim interactive_mode As Boolean
                    Dim interactive_console_jservice As Object
                    Dim interactive_console_processid As Long

                    If Me.Info.http_connection_handler.haveHeader("X-MAPPINGPOINT-OBJECT-FROM") Then
                        object_sender_classname = Me.Info.http_connection_handler.Headers("X-MAPPINGPOINT-OBJECT-FROM")
                    Else
                        object_sender_classname = "JInteractiveConsole"
                    End If

                    If Me.Info.http_connection_handler.haveHeader("X-MAPPINGPOINT-OBJECT-ENDPOINT") Then
                        object_interactive_replyforwarding_classname = Me.Info.http_connection_handler.Headers("X-MAPPINGPOINT-OBJECT-ENDPOINT")
                    Else
                        'self route.
                        object_interactive_replyforwarding_classname = "JInteractiveConsole"
                    End If

                    interactive_mode = (object_sender_classname = "JInteractiveConsole")

                    If Not interactive_mode Then

                        If Not Me.haveProcesses(object_sender_classname) Then
                            'Proposed sender JClass does not exist in the mapping point chain.
                            GlobalObject.MsgColored("PassObject error: Sender '" & object_sender_classname & "' does not exist.", Color.Red)
                            'Me.getProcessByClassName(object_sender_classname, True)
                        Else
                            'get the process in who's name we are sending
                            orginating_message_process_info = Me.getProcessByClassName(object_sender_classname, True)

                            If IsNothing(orginating_message_process_info) Then
                                retval.state = ChainResultState.FATAL_ERROR
                                retval.state_message = "Internal error in messaging system. Error code 1."
                                Return retval
                            Else
                                If orginating_message_process_info.haveProcess Then
                                    orginating_message_process = orginating_message_process_info.getProcess()
                                    If IsNothing(orginating_message_process) Then
                                        retval.state = ChainResultState.FATAL_ERROR
                                        retval.state_message = "Internal error in messaging system. Error code 2."
                                        Return retval
                                    End If
                                Else
                                    retval.state = ChainResultState.FATAL_ERROR
                                    retval.state_message = "Target process not running."
                                    Return retval
                                End If
                            End If
                        End If

                        'construct object messaging environment out of thin air.
                        orginating_message_process_mailbox = New JServicedMailBox(orginating_message_process)
                        orginating_message_process_mailbox.Configure(New TriniDATObjectBox_EventTable, Nothing)
                    Else
                        If Not Me.hasClass("JInteractiveConsole") Then
                            Dim err_msg As String
                            err_msg = "Cannot broadcast object, this mapping point does not have interactiveconsolefeatures attribute set."
                            GlobalObject.MsgColored(err_msg, Color.Red)
                            retval.state = ChainResultState.FATAL_ERROR
                            retval.state_message = "Internal error in messaging system. Error code 2."
                            Return retval
                        Else
                            interactive_console_processid = Me.getActiveProcessIdByClass("JInteractiveConsole")

                            If IsNothing(interactive_console_processid) Then
                                retval.state = ChainResultState.FATAL_ERROR
                                retval.state_message = "Internal error in interaction system. Error code 3."
                                Return retval
                            Else
                                interactive_console_jservice = Me.getProcessByIndex(interactive_console_processid)
                                If IsNothing(interactive_console_jservice) Then
                                    retval.state = ChainResultState.FATAL_ERROR
                                    retval.state_message = "Internal error in interaction system. Error code 4."
                                    Return retval
                                End If
                            End If
                        End If
                    End If

                    'construct the object.
                    object_recipient_classname = HttpUtility.UrlDecode(Me.Info.http_connection_handler.Headers("X-MAPPINGPOINT-OBJECT-TO"))
                    json_encoded_simple_jsonobject = Me.Info.http_connection_handler.Headers("X-MappingPoint-Object")
                    json_encoded_simple_jsonobject = HttpUtility.UrlDecode(json_encoded_simple_jsonobject)
                    json_encoded_simple_jsonobject = json_encoded_simple_jsonobject

                    Try
                        Try
                            dserialized_object = JsonConvert.DeserializeObject(json_encoded_simple_jsonobject)
                        Catch ex As Exception
                            Throw New Exception("Fatal error deserializing object: " & ex.Message)
                        End Try

                        If IsNothing(dserialized_object) Then
                            Throw New Exception("Invalid broadcast object.")
                        End If

                        object_sender_classname = object_sender_classname

                        'send it
                        Dim real_json_object_instance As JSONObject

                        real_json_object_instance = New JSONObject

                        Try
                            real_json_object_instance.Directive = dserialized_object("Directive").ToString
                            real_json_object_instance.ObjectType = dserialized_object("ObjectType").ToString
                            real_json_object_instance.Tag = object_interactive_replyforwarding_classname

                        Catch ex As Exception
                            Throw New Exception("Invalid object. Missing header fields.")
                        End Try

                        If Not IsNothing(dserialized_object("ObjectAttachment")) Then
                            real_json_object_instance.Attachment = dserialized_object("ObjectAttachment").ToString
                        Else
                            real_json_object_instance.Attachment = ""
                        End If


                        If Not IsNothing(dserialized_object) Then

                            'proxy with JInteractiveConsole.
                            If interactive_mode Then
                                GlobalObject.MsgColored("Proxying object '" & dserialized_object("ObjectType").ToString & "' => " & object_recipient_classname & "...", Color.Pink)
                                If interactive_console_jservice.haveProcess Then
                                    If interactive_console_jservice.getProcess().ProxySendObject(real_json_object_instance, object_recipient_classname) Then
                                        GlobalObject.MsgColored("Object '" & dserialized_object("ObjectType").ToString & "' successfully processed.", Color.Pink)
                                    Else
                                        GlobalObject.MsgColored("Object '" & dserialized_object("ObjectType").ToString & "' => " & object_recipient_classname & " unable to be sent to one or more recipient(s).", Color.Gold)
                                    End If
                                End If
                            Else
                                'send direct.
                                GlobalObject.MsgColored("Forward object '" & dserialized_object("ObjectType").ToString & "' => " & object_recipient_classname & "...", Color.Pink)
                                Call orginating_message_process_mailbox.Send(real_json_object_instance, object_recipient_classname)
                            End If

                        Else
                            retval.state = ChainResultState.FATAL_ERROR
                            retval.state_message = "Internal error in messaging system. Error code 5."
                            Return retval
                        End If


                    Catch ex As Exception
                        If GlobalObject.haveServerThread Then
                            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                                GlobalObject.MsgColored(ex.Message, Color.Red)
                            End If
                        End If
                    End Try
                End If

            '6. PASS HTTP PACKET TO ALL PROCESSES

            'inform handler if this is the first call ever 

            For x = 0 To Me.processesCount - 1

                If Not Me.Info.direct_socket.isConnected() Or Me.Info.direct_socket.disconnectSocket() = True Then
                    retval.state_message = "OpenChainProcesses: Aborting. Socket is already closed."
                    retval.state = ChainResultState.SOCKET_CLOSED_PACKET
                    Return retval
                End If

                proc = Me.getProcessByIndex(x)

                If Not IsNothing(proc) Then
                    retval.state_affected_jclass_name = proc.getClassInfo.getName()
                    Msg("OpenChainProcesses: Activating & passing data to " & proc.getClassInfo.getName())

                    'pass all data
                    Call proc.getProtocolHandler().OnPacketReceived(proc.getProcess(), Me.Info, Me.bootPacket, Me.bootPacketsize)
                Else
                    Throw New Exception("Something is wrong. Process died.")
                    Exit For
                End If


            Next

            Msg("=================================")
        Catch ex As Exception
            retval.state_message = "A mapping point class or dependency generated an unhandled exception: " & ex.Message & ".  <BR><BR>Trace: " & ex.StackTrace.ToString
            retval.state_solution_message = "See exception."
            retval.state = ChainResultState.FATAL_ERROR
            Return retval
        End Try

        'Msg("OpenChainProcesses: Created" & Me.getClassCount().ToString & " instances on " & Me.URI & "  process_count= " & Me.processesCount.ToString & ").")

        retval.state = ChainResultState.OK
        Return retval
    End Function
    Public Sub setURI(ByVal val As String)

        'URI MUST end with / 

        Me.URI = val

        If Right(Me.URI, 1) <> "/" Then
            Me.URI = Me.URI & "/"
        End If

    End Sub

    Public Function getURI() As String
        Return Me.URI
    End Function

    Public Property isDebug() As Boolean
        Get
            Return Me.isdebugged
        End Get
        Set(ByVal value As Boolean)
            Me.isdebugged = value
        End Set
    End Property

    Public Sub setAsResidential(ByVal val As Boolean)
        Me.isdebugged = val
    End Sub
    Public Function hasClasses() As Boolean
        Dim total_count As Integer
        total_count = getClassCount()

        'hide JAlpha
        total_count = total_count - 1

        Return (total_count > 0)
    End Function
    Public Function getClasses() As List(Of mappingPointClass)
        Return Me.Classes
    End Function

    Public Function getActiveProcessIdByClass(ByVal target_className As String) As Integer

        Dim proc As mappingPointInstanceInfo

        Try

            For Each proc In Me.processes
                If proc.getClassInfo().getName() = target_className Then
                    If proc.haveProcess() Then
                        Return proc.getIndex()
                    End If
                End If
            Next

        Catch ex As Exception
            Debug.Print("getActiveProcessIdByClass['" & target_className & "'] error: " & ex.Message)
        End Try


        Return Nothing

    End Function

    Public Function getClassByIndex(ByVal x As Integer)
        Return Me.Classes(x)
    End Function

    Private Function getClassFriendlyName(ByVal full_class_name As String) As String
        Return JServiceLauncher.getClassFriendlyName(full_class_name)
    End Function

    Private ReadOnly Property processesCount As Integer
        Get
            Return Me.processes.Count
        End Get
    End Property
    Public Function getClassByName(ByVal classname As String) As mappingPointClass

        Dim classInfo As mappingPointClass

        If Not Me.hasClasses() Or Not Me.hasClass(classname) Then
            Return Nothing
        End If

        classname = getClassFriendlyName(classname)

        For Each classInfo In Me.getClasses()
            If classInfo.getName() = classname Then
                Return classInfo
            End If
        Next

        Return Nothing


    End Function
    Public Function hasClass(ByVal classname As String) As Boolean

        Dim classInfo As mappingPointClass

        If Not Me.hasClasses() Then
            Return Nothing
        End If

        classname = getClassFriendlyName(classname)

        For Each classInfo In Me.getClasses()
            If classInfo.getName() = classname Then
                Return True
            End If
        Next

        Return False


    End Function
    Public ReadOnly Property classCount As Integer
        Get
            Return Me.Classes.Count
        End Get
    End Property

    Public Function getClassCount() As Integer
        Return Me.classCount
    End Function
    Public Function removeInstance(ByVal index As Integer) As Boolean

        Dim procinfo As mappingPointInstanceInfo

        Try
            procinfo = Me.processes(index)

            If Not IsNothing(procinfo) Then
                Debug.Print("mappingPointRoot.removing instance " & index.ToString & " (count=" & Me.processesCount.ToString & ")")
                Me.processes(index) = Nothing
                Return True
            Else
                Err.Raise(100, Nothing, " Empty process at index " & index.ToString)
                Return False
            End If

        Catch ex As Exception
            Debug.Print("mappingPointRoot.removeInstance error: " & ex.Message)
            Return False
        End Try

        Return True
    End Function

    Public Function getIndex() As Integer
        Return Me.index
    End Function


    Public Sub setIndex(ByVal _index As Integer)
        Me.index = _index
    End Sub


    'getProcessByClass
    Public Function getProcessByIndex(ByVal x As Integer) As mappingPointInstanceInfo
        If x < Me.processesCount Then
            If Not IsNothing(Me.processes(x)) Then
                Return Me.processes.Item(x)
            End If
        End If


        Return Nothing

    End Function
    Public Function getProcessByClassName(ByVal className As String, ByVal activeProcessOnly As Boolean) As mappingPointInstanceInfo

        If Not haveProcesses(className) Then
            'error
            Return Nothing
        Else

            'find process by given classname.
            Dim x As Integer
            Dim proc As mappingPointInstanceInfo

            For x = 0 To processesCount - 1
                proc = Me.getInstanceInfo(x)
                If Not IsNothing(proc) Then
                    If Not IsNothing(proc.getProcess()) Then
                        If Not IsNothing(proc.getClassInfo()) Then
                            If Not IsNothing(className) Then
                                If proc.getClassInfo().getName() = className Then
                                    If activeProcessOnly And proc.haveProcess() Then
                                        Return proc
                                    ElseIf activeProcessOnly = False Then
                                        Return proc
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Next x

        End If

        Return Nothing

    End Function
    Public Function haveProcesses(Optional ByVal jclass_name As String = Nothing) As Boolean

        If Me.processesCount = 0 Then
            'no processes running or only JAlpha.
            Return False
        End If

        'check and skip stopped processes.
        Dim x As Integer
        Dim proc As mappingPointInstanceInfo

        For x = 0 To processesCount - 1
            proc = Me.getInstanceInfo(x)
            If Not IsNothing(proc) Then
                If Not IsNothing(proc.getProcess()) Then
                    If Not IsNothing(proc.getClassInfo()) Then
                        If Not IsNothing(jclass_name) Then
                            If proc.getClassInfo().getName() = jclass_name Then
                                'found this class running
                                Return True
                            End If
                        End If
                    Else
                        'something is alive.
                        Return True
                    End If
                End If
            End If
        Next x


        Return False

    End Function

    Public Sub listAllProcesses()

        Dim x As Integer
        Dim count As Integer = processesCount - 1

        For x = 0 To count
            Debug.Print("listAllProcesses debugger: Proc #" & x.ToString & "/" & count & " class= " & Me.processes(x).getClassInfo().getName())
        Next


    End Sub


    Public Function AddInstance(ByVal classInfo As mappingPointClass, ByVal instanceRef As JTriniDATWebService) As Integer
        'returns array index

        Dim entryIndex As Integer
        Dim new_item As mappingPointInstanceInfo

        Try


            entryIndex = processesCount

            'CREATE ENTRY

            Debug.Print(Me.URI & ": Adding #" & entryIndex.ToString & " =  " & classInfo.getName())

            new_item = New mappingPointInstanceInfo(classInfo, Me)
            new_item.setProcess(instanceRef)
            new_item.setIndex(entryIndex)

            Me.processes.Add(new_item)

            Return Me.processesCount - 1

        Catch ex As Exception
            Debug.Print("AddInstance: error " + ex.Message)
            Return -1
        End Try

    End Function

    Public Function configureInstance(ByVal instanceIndex As Integer, ByVal newInfo As mappingPointInstanceInfo) As Boolean
        'copies everything except the instance itself.

        Dim JProcess As JTriniDATWebService

        Try
            JProcess = processes(instanceIndex).getProcess()

            processes(instanceIndex) = newInfo
            processes(instanceIndex).setProcess(JProcess)

            Return True
        Catch ex As Exception
            Debug.Print("configureInstance: error " + ex.Message)
            Return False
        End Try

    End Function

    Public Function getInstanceInfo(ByVal instanceIndex As Integer) As mappingPointInstanceInfo

        Try

            Return processes(instanceIndex)

        Catch ex As Exception
            Debug.Print("getInstanceInfo: error " + ex.Message & " (index: " & instanceIndex.ToString & " count: " & Me.processesCount.ToString & ")")
            Return Nothing
        End Try

    End Function


    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class

Public Class mappingPointClass
    Private myClassName As String
    Private description_text As String

    Public Sub New(ByVal _class_name As String, ByVal _description As String)
        Me.setName(_class_name)
        Me.Description = _description
    End Sub

    Public ReadOnly Property getName() As String
        Get
            Return myClassName
        End Get
    End Property
    Public Property Description() As String
        Get
            Return Me.description_text
        End Get
        Set(ByVal value As String)
            description_text = value
        End Set
    End Property
    Public Sub setName(ByVal _ClassName As String, Optional ByVal User_Description As String = "")
        myClassName = _ClassName
        Me.Description = User_Description
    End Sub
    Public Sub New()
        myClassName = Nothing
    End Sub

    Public ReadOnly Property isReserved() As Boolean
        Get
            If Not IsNothing(getName()) Then
                Return (Me.getName() = mappingPointRoot.SystemReservedClassname)
            Else
                Return False
            End If

        End Get
    End Property
End Class

Public Class mappingPointInstanceInfo
    Inherits MarshalByRefObject
    'contains a running JTriniDATWebService-derived class.

    Public index As Integer
    Private classInfo As mappingPointClass
    Private myParent As mappingPointRoot
    Public Tag As String
    Public Stack As Object
    Private process As JTriniDATWebService
    Private startupArguments As StringDictionary
    Private TriggerURI As String
    Private protocolConfiguration As TriniDATClientConnectionManagerHTTP
    Public Sub setProcess(ByRef val As JTriniDATWebService)
        Me.process = val
    End Sub
    Public Function getProcess() As Object '=JTriniDATWebService
        Return Me.process
    End Function
    Public ReadOnly Property haveProcess As Boolean
        Get
            Return Not IsNothing(Me.process)
        End Get
    End Property
    Public Sub setParent(ByVal proc As Object) '=mappingPointRoot
        Me.myParent = proc
    End Sub
    Public ReadOnly Property getParent() As Object '=mappingPointRoot
        Get
            If Me.haveParent() Then
                Return Me.myParent
            Else
                Return Nothing
            End If
        End Get
    End Property
    Public ReadOnly Property haveParent() As Boolean
        Get
            Return Not IsNothing(Me.myParent)
        End Get
    End Property
    Public Function setProtocolHandler(ByVal _to As Object) 'to=TriniDATClientConnectionManagerHTTP
        Me.protocolConfiguration = _to
    End Function
    Public ReadOnly Property getProtocolHandler() As Object '=TriniDATClientConnectionManagerHTTP
        Get
            If haveProtocolHandler() Then
                Return Me.protocolConfiguration
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public ReadOnly Property haveProtocolHandler() As Boolean
        Get
            Return Not IsNothing(protocolConfiguration)
        End Get
    End Property

    Public Function getProtocolName() As String
        'Returns _protocol_type

        If Not haveProtocolHandler Then
            Return "(none)"
        Else
            Dim protocol_class As String

            protocol_class = getProtocolHandler().getName()
            Return protocol_class
        End If

    End Function
    Public Sub New(ByVal _classInfo As mappingPointClass, ByVal _parent As mappingPointRoot)
        'link to class header info 
        Me.setClassInfo(_classInfo)
        Me.setParent(_parent)
        'init vars
        process = Nothing
        startupArguments = New StringDictionary
        TriggerURI = Nothing
    End Sub

    Public Function haveParameters() As Boolean
        Return (startupArguments.Count > 0)
    End Function
    Public Sub setParameters(ByVal params As StringDictionary)
        startupArguments = params
    End Sub
    Public Function getParameters() As StringDictionary
        Return startupArguments
    End Function
    'Public Function isRunning() As Boolean
    '    Return Me.haveProcess()
    'End Function
    Public Sub setClassInfo(ByVal _classInfo As mappingPointClass)
        classInfo = _classInfo
    End Sub
    Public Function getClassInfo() As mappingPointClass
        Return classInfo
    End Function

    Public Function getIndex() As Integer
        Return Me.index
    End Function

    Public Sub setIndex(ByVal _index As Integer)
        Me.index = _index
    End Sub

End Class

Public Class ChainResult
    Public state As ChainResultState
    Public state_http_code As Integer
    Public state_message As String
    Public state_solution_message As String
    Public state_affected_jclass_name As String
    Public Sub New()
        Me.state_solution_message = "none"
        Me.state_affected_jclass_name = "unknown"
    End Sub
    Public Sub HTTP_OutputState(ByVal connie As TriniDATClientConnectionManagerHTTP)
        Dim footer As String

        connie.setHTTPResponse(Me.state_http_code)

        footer = "<BR><hr style=""border:1; background-color: #000;  height:1px; width:100%;""><I>TriniDAT Data Server " & Replace(GlobalObject.getVersionString(), "version ", "v.") & ". " & " Active on " & GlobalObject.CurrentServerConfiguration.server_ip.ToString & " port " & GlobalObject.CurrentServerConfiguration.server_port.ToString & ". " & GlobalObject.getCopyrightString() & "</I>"
        connie.writeRaw(True, "<b>Error in class '" & Me.state_affected_jclass_name & "'</b><BR><BR>" & state_message & footer, True)

        If Debugger.IsAttached Then
            Debug.Print("HTTP_OutputState: " & state_message)
        End If

    End Sub
End Class
Public Enum ChainResultState
    FALSE_FUNCTION_TABLE = -3
    FALSE_CONFIGURE = -2
    FATAL_ERROR = -1
    NEUTRAL = 0
    OK = 1
    SOCKET_CLOSED_CREATE = 2
    SOCKET_CLOSED_CONFIG = 3
    SOCKET_CLOSED_PACKET = 4
End Enum

Public Enum DEPENDENCY_LOADSTATE
    DL_ERR = -1
    DL_NEUTRAL = 0
    DL_OK = 1
End Enum