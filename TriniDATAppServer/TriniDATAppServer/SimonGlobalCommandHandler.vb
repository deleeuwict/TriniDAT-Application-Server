Option Compare Text

Imports System.IO
Imports System.Speech.Synthesis
Imports SimonTypes
Imports TriniDATServerTypes
Imports TriniDATSockets

Public Class SimonGlobalCommandHandler
    'handles console commandline

    Private action_frm As frmServerMain
    Private console_state As SimonConsoleState
    Private current_debug_app As XDocument
    Private current_debugger_objects As SimonDebugFrames
    Private expecting_debug_objectid As Boolean
    Private latestframeid As Long
    Public Function OnIncomingDebugFrame(ByVal debug_frame As SimonDebugFrame) As Boolean
        GlobalObject.MsgColored("Source app: '" & debug_frame.App.applicationname & "'. mapping point '" & debug_frame.direct_mapping_point.URL & "' frozen.", Color.MidnightBlue)
        debug_frame.id = GlobalObject.nextDebuggingObjectId

        Debug.Print("Assigning object id " & debug_frame.id.ToString)

        'put associated mapping point in debug mode.
        If debug_frame.direct_mapping_point.haveMappingPointInstance Then
            debug_frame.direct_mapping_point.MappingPoint.isdebug = True
        End If

        Me.Frames.Add(debug_frame)

        'set as current 
        Me.CurrentFrameId = debug_frame.id

        'change console context
        Me.setConsoleContext(SimonTypes.SimonConsoleContext.DEBUG_CONTEXT_DEBUGOBJECT, Nothing)

        Return True
    End Function

    Public Sub setConsoleContext(ByVal val As SimonConsoleContext, ByVal speech_str As String)
        'change console context
        Me.State.CommandContext = val

        Dim additional_log_msg As String

        If val <> SimonConsoleContext.SERVER_DEV And val <> SimonConsoleContext.SERVER_LIVE Then
            additional_log_msg = "Enter 'debugdic' for debug command list."
        Else
            additional_log_msg = ""
        End If

        GlobalObject.MsgColored("Console context changed to '" & Me.State.asXMLIdentifierString & "'. " & additional_log_msg, GlobalObject.CONSOLE_DEFAULT_COLOR)

        If Not IsNothing(speech_str) Then
            GlobalSpeech.Text = speech_str
            GlobalSpeech.SpeakEliteThreaded()
        End If
        
    End Sub

    Public Property ExpectingObjectID() As Boolean
        Get
            Return Me.expecting_debug_objectid
        End Get
        Set(ByVal value As Boolean)
            Me.expecting_debug_objectid = value
        End Set
    End Property

    Public Sub New(ByVal frm As frmServerMain)
        Me.GUIObject = frm
        Me.State = New SimonConsoleState
        Me.State.CommandContext = SimonConsoleContext.SERVER_LIVE
        If GlobalObject.haveServerThread Then
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                Me.State.CommandContext = SimonConsoleContext.SERVER_DEV
            End If
        End If
        Me.Frames = New SimonDebugFrames
    End Sub
    Public Function EraseAllFrames() As Boolean

        Try

            For Each frame In Me.Frames

                If frame.haveConnection Then
                    Dim socket As TriniDATTCPSocket

                    socket = frame.direct_http_client.getConnection()

                    If Not IsNothing(socket) Then
                        If socket.isConnected Then
                            GlobalObject.MsgColored("Dropping debug socket.", Color.Red)
                            socket.forceDisconnect()
                        End If
                    End If
                End If
            Next

        Catch ex As Exception
            Return False
        End Try
        Return True

    End Function
    Public Property CurrentFrameId As Long
        Get
            Return Me.latestframeid
        End Get
        Set(ByVal value As Long)
            Me.latestframeid = value

            GlobalObject.MsgColored("Now in debugging mode. object Id: '" & Me.latestframeid.ToString & "'. " & Me.generateSimonsSessionStruct().getTranslated("FROZEN_DUMP_HINT"), Color.MidnightBlue)
        End Set
    End Property
    Public ReadOnly Property CurrentFrame As SimonDebugFrame
        Get
            Return Me.Frames.GetById(Me.CurrentFrameId)
        End Get
    End Property
    Public Property Frames As SimonDebugFrames
        Get
            Return Me.current_debugger_objects
        End Get
        Set(ByVal value As SimonDebugFrames)
            Me.current_debugger_objects = value
        End Set
    End Property
    Public ReadOnly Property haveDebugObject As Object
        Get
            Return Not IsNothing(Me.current_debugger_objects)
        End Get
    End Property

    Public Property CurrentDebugApplication As XDocument
        Get
            Return Me.current_debug_app
        End Get
        Set(ByVal value As XDocument)
            Me.current_debug_app = value
        End Set
    End Property

    Public Property State As SimonConsoleState
        Get
            Return Me.console_state
        End Get
        Set(ByVal value As SimonConsoleState)
            Me.console_state = value
        End Set
    End Property

    Public Shared ReadOnly Property Speaker As SpeechSynthesizer
        Get
            'function reserved for future use.
            Return GlobalSpeech.Speaker
        End Get
    End Property

    Public Property GUIObject As frmServerMain
        Get
            Return Me.action_frm
        End Get
        Set(ByVal value As frmServerMain)
            Me.action_frm = value
        End Set
    End Property


    Public Function Execute(ByVal original_user_text As String) As Boolean
        'returns: valid command executed true/false. 
        Dim q
        Dim xsimon As XDocument
        Dim command_executed_retval As SimonsReturnValue
        Dim command_executed As Boolean
        Dim command_valid As Boolean
        Dim cmd As String
        Dim parameters As String
        Dim space_pos As Integer
        Dim old_console As String
        Dim is_object_command As Boolean
        Dim object_command_objectname As String
        Dim temp As SimonsSession
        Dim dot_pos As Integer
        Dim preparse_parameters As Boolean


        'init
        cmd = original_user_text.ToUpper
        command_executed_retval = SimonsReturnValue.NEUTRAL
        command_valid = False
        command_executed = False
        old_console = ""
        parameters = ""
        object_command_objectname = ""
        space_pos = InStr(cmd, " ")

        If space_pos > 0 Then
            parameters = Mid(cmd, space_pos + 1)
            cmd = Mid(cmd, 1, space_pos - 1)
        End If

        dot_pos = InStr(cmd, ".")
        is_object_command = (dot_pos > 0)

        If is_object_command Then
            Dim command_end_point_pos As Integer
            command_end_point_pos = space_pos
            If command_end_point_pos < 1 Then
                command_end_point_pos = Len(cmd)
            Else
                command_end_point_pos = space_pos - 1
            End If

            object_command_objectname = Mid(cmd, 1, dot_pos - 1)
            cmd = "OBJECTMETHOD_" & Mid(cmd, dot_pos + 1, command_end_point_pos - dot_pos)
        End If

        xsimon = SimonGlobalCommandHandler.getRealTimeCommandsXMLDoc()

        If IsNothing(xsimon) Then
            GlobalObject.MsgColored("aborting command interpretation.", Color.MediumPurple)
            Return False
        End If

        Try
            Dim qInitial = From mods In xsimon.Descendants("commands").Descendants("command") Where Not IsNothing(mods.@action) And Not IsNothing(mods.@context)
            q = From valid_cmd_handler In qInitial Where valid_cmd_handler.@action.ToString.Length > 0 And valid_cmd_handler.@action.ToString = cmd And (valid_cmd_handler.@context.ToString = Me.State.asXMLIdentifierString Or valid_cmd_handler.@context.ToString = "all")

        Catch ex As Exception
            GlobalObject.MsgColored("Malformed console XML: " & ex.Message & " in  " & GlobalSetting.getSimonXMCommandFile(), Color.Red)
            Return False
        End Try

        For Each cmd_handler As XElement In q
            Dim simons_handler As SimonExternalCommandWrap

            If Not IsNothing(cmd_handler.@action) And Not IsNothing(cmd_handler.@module) And Not IsNothing(cmd_handler.@container) Then
                Dim speakstr As String
                speakstr = ""

                If Not IsNothing(cmd_handler.@completed) Then
                    speakstr = cmd_handler.@completed.ToString
                End If

                simons_handler = New SimonExternalCommandWrap(Me, cmd, cmd_handler.@module, cmd_handler.@container, speakstr, is_object_command, object_command_objectname)

                If simons_handler.State = ReflectResultResult.COMMAND_AVAILABLE Then

                    If GlobalObject.haveServerForm Then
                        old_console = GlobalObject.serverForm.txtServerActivity.Text
                    End If

                    preparse_parameters = IsNothing(cmd_handler.@parameters)

                    If Not preparse_parameters Then
                        'set predefined parameter list.
                        parameters = cmd_handler.@parameters
                    End If

                    If InStr(parameters, "{$") > 0 Then
                        parameters = Replace(parameters, "{$SETTING.CMDXML}", xsimon.ToString)
                    End If

                    GlobalObject.MsgNewLine()
                    command_executed_retval = simons_handler.ExecCommand(parameters, preparse_parameters)
                    command_executed = True

                    If CType(command_executed_retval, Integer) < 0 Then
                        GlobalObject.MsgColored("command '" & original_user_text & "' generated errors.", Color.Red)
                    Else
                        'success - read output value
                        command_valid = True

                        If command_executed_retval = SimonsReturnValue.VALIDCOMMAND_TOGGLE_DEBUG_MODE Then
                            temp = Me.generateSimonsSessionStruct()

                            'enter/leave debug mode
                            If Me.State.CommandContext = SimonConsoleContext.SERVER_DEV Then
                                GlobalObject.MsgColored("application debugger enabled. Enter 'dic' for clarification of terms. Type 'debug_app' again to leave.", GlobalObject.CONSOLE_DEFAULT_COLOR)
                                GlobalSpeech.Text = temp.getTranslated("DEBUGENABLED")
                                GlobalSpeech.SpeakEliteThreaded()
                                Me.State.CommandContext = SimonConsoleContext.DEBUG_CONTEXT_APP
                            ElseIf Me.State.CommandContext = SimonConsoleContext.DEBUG_CONTEXT_APP Then
                                GlobalObject.MsgColored("leaving application debugger.", GlobalObject.CONSOLE_DEFAULT_COLOR)
                                GlobalSpeech.Text = temp.getTranslated("DEBUGDISABLED")
                                GlobalSpeech.SpeakEliteThreaded()
                                Me.State.CommandContext = SimonConsoleContext.SERVER_DEV
                            End If

                            'done with this command
                            Exit For
                        ElseIf command_executed_retval = SimonsReturnValue.VALIDCOMMAND_NEXT_CONTEXT Then
                            'level up in context.

                            If Me.State.CommandContext = SimonConsoleContext.DEBUG_CONTEXT_DEBUGOBJECT Then
                                'go to server mode
                                Me.setConsoleContext(SimonConsoleContext.SERVER_DEV, Nothing)
                                Exit For
                            End If

                        ElseIf command_executed_retval = SimonsReturnValue.VALIDCOMMAND_SHOWHELP Then
                            'automagically display help.
                            Me.Execute("help " & cmd)
                            Return False
                        ElseIf command_executed_retval = SimonsReturnValue.VALIDCOMMAND_DEBUG_READTAG_EXPECT_OBJECTID Then
                            'SUCCESS_DEBUG_READTAG_EXPECT_OBJECTID
                        End If

                        If GlobalObject.haveServerForm Then
                            If old_console = GlobalObject.serverForm.txtServerActivity.Text Then
                                GlobalObject.MsgColored("no output.", GlobalObject.CONSOLE_DEFAULT_COLOR)
                            End If
                        End If

                        If Not IsNothing(cmd_handler.@overrides) Then
                            If cmd_handler.@overrides.ToString = "true" Then
                                'stop executing similair handlers.
                                Exit For
                            End If
                        End If
                    End If
                Else
                    Dim specific_err_msg As String

                    specific_err_msg = "error unknown"
                    If simons_handler.State = ReflectResultResult.ERR_EXTERNAL_METHOD_NOT_EXIST Then
                        If Not simons_handler.haveASMModulePath Then
                            specific_err_msg = "Module path not specified for '" & cmd & "' or make sure command handler's class method declaration is 'public'."
                        Else
                            specific_err_msg = "the specified handler function '" & Replace(cmd, " ", "_") & " with return type SimonsSession does not exist."
                        End If
                    ElseIf simons_handler.State = ReflectResultResult.ERR_EXTERNAL_MODULE_LOADER_ERR Then
                        specific_err_msg = "there was an error during .NET reflection of module '" & simons_handler.module_name.ToString & "'. check your module path."
                    ElseIf simons_handler.State = ReflectResultResult.ERR_EXTERNAL_MODULE_NOT_FOUND Then
                        specific_err_msg = "invalid module path specified: " & simons_handler.module_name.ToString
                    ElseIf simons_handler.State = ReflectResultResult.ERR_EXTERNAL_TYPE_NOT_FOUND Then
                        specific_err_msg = "the specified container type '" & simons_handler.AsmClassName.ToString & "' not found in " & simons_handler.module_name.ToString & ". Specify container in format: assemblyname.classname"
                    ElseIf simons_handler.State = ReflectResultResult.LOADED Then
                        specific_err_msg = "module loaded but not useful. class specified in container not found."
                    End If

                    GlobalObject.MsgColored("command handler failure: " & specific_err_msg, Color.Red)
                End If

            Else
                GlobalObject.MsgColored("Missing attributes in simon's handler: " & cmd_handler.ToString, Color.Red)
            End If

        Next

        If Not command_executed Then
            command_valid = False
            temp = Me.generateSimonsSessionStruct()
            GlobalObject.MsgColored("unknown command '" & original_user_text & "'. Enter 'help' for command list.", Color.Orange)
            GlobalSpeech.Text = temp.getTranslated("ERROR")
            GlobalSpeech.SpeakEliteThreaded()
        End If

        'todo: needs change with mult-command -execution.
        Return command_valid
    End Function


    Public Shared Function getRealTimeCommandsXMLDoc(Optional ByVal retry_count As Integer = 0) As XDocument
        Dim simon_path As String

        simon_path = GlobalSetting.getSimonXMCommandFile

        Try

            If File.Exists(simon_path) Then
                Return XDocument.Parse(File.ReadAllText(simon_path))
            End If

        Catch ex As Exception
            If retry_count < 5 Then
                'retry: .NET or Windows might be locking the file.
                retry_count += 1
                Return getRealTimeCommandsXMLDoc(retry_count)
            Else
                GlobalObject.MsgColored("read xml failure " & simon_path & ": " & ex.Message, Color.Red)
            End If
        End Try

    End Function
    Public Shared ReadOnly Property haveTranslationTable() As Boolean
        Get
            Return File.Exists(GlobalSetting.getSimonXMLTranslationTableFile())
        End Get
    End Property
    Public Function generateSimonsSessionStruct() As SimonsSession

        Dim retval As SimonsSession

        If SimonGlobalCommandHandler.haveTranslationTable Then
            retval = New SimonsSession(Threading.Thread.CurrentThread.CurrentCulture, SimonGlobalCommandHandler.Speaker, GlobalObject.simon.GUIObject.txtServerActivity, State, GlobalObject.CONSOLE_DEFAULT_COLOR, GlobalObject.server.ServerMode, Me.CurrentDebugApplication, Me.Frames, Me.CurrentFrameId)
            retval.ObjectServer = GlobalObject.CurrentServerConfiguration.server_ip.ToString
            retval.ObjectServerPort = GlobalObject.CurrentServerConfiguration.server_port
            retval.ServerIsOn = GlobalObject.haveServerThread
            If Not retval.Configure(XDocument.Parse(File.ReadAllText(GlobalSetting.getSimonXMLTranslationTableFile()))) Then
                retval = Nothing
            End If

        Else
            retval = Nothing
        End If

        Return retval
    End Function
    Public Function getSpeechTxtByCommand(ByVal cmd As String) As String

        Dim xsimon As XDocument

        xsimon = Me.getRealTimeCommandsXMLDoc()

        If IsNothing(xsimon) Then
            GlobalObject.Msg("getSpeechTxtByCommand: load failure.")
            Return Nothing
        End If


        Dim q = From xsimon_command In xsimon.Descendants("commands").Descendants("command") Where xsimon_command.@action.ToString = cmd

        If q.Count = 0 Then
            Return Nothing
        Else
            Return q(0).@speak.ToString
        End If

    End Function

End Class

