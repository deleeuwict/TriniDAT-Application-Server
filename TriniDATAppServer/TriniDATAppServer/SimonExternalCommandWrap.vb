Option Compare Text
Imports SimonTypes
Imports System.Reflection
Imports System.IO
Public Class SimonExternalCommandWrap
    Public cmd_text_to_speak As String
    Public module_name As String '= path
    Public class_name As String
    Public method_name As String
    Public general_state As ReflectResultResult
    Public general_state_msg As String
    Public src_module As Assembly
    Public loaded_class_type As Type
    Public loaded_class_instance As Object
    Public loaded_cmd_method As MethodInfo
    Private parent_console As SimonGlobalCommandHandler
    Private object_methodparameter_objectname As String
    Private object_command As Boolean

    Public Sub New(ByVal _parent As SimonGlobalCommandHandler, ByVal _cmd_name As String, ByVal _mod_name As String, ByVal _class_name As String, ByVal _textspeak As String, ByVal _is_object_method As Boolean, ByVal _object_method_objectname As String)
        Me.State = ReflectResultResult.NEUTRAL
        Me.StateMessage = "None"
        Me.cmd_text_to_speak = _textspeak
        Me.parent_console = _parent
        Me.object_command = _is_object_method
        Me.object_methodparameter_objectname = _object_method_objectname

        'execute reflection by setting the property.
        Me.AsmModulePath = _mod_name
        'directly load and reflect all
        If Me.State = ReflectResultResult.LOADED Then
            Me.AsmClassName = _class_name

            If Me.haveClassType Then
                Me.AsmMethod = _cmd_name
            Else
                Me.State = ReflectResultResult.ERR_EXTERNAL_TYPE_NOT_FOUND
            End If
        End If

    End Sub
    Public ReadOnly Property isObjectCommand As Boolean
        Get
            Return Me.object_command
        End Get
    End Property
    Public ReadOnly Property TargetObjectName As String
        Get
            'if isObjectCommand = true , e.g. twitter.send speak "aaaaaaaa" 

            Return Me.object_methodparameter_objectname
        End Get
    End Property
    Public ReadOnly Property haveClassType As Boolean
        Get
            Return Not IsNothing(Me.loaded_class_type)
        End Get
    End Property
    Public ReadOnly Property Parent As SimonGlobalCommandHandler
        Get
            Return Me.parent_console
        End Get
    End Property

    Public Property AsmClassName As String
        Get
            Return Me.class_name
        End Get
        Set(ByVal value As String)
            Me.class_name = value
            If InStr(Me.class_name, ".") = 0 Then
                'prefix assembly.
                Me.class_name = "TriniDAT." & Me.class_name
            End If
            Me.loaded_class_type = Nothing

            If Not Me.State = ReflectResultResult.LOADED Then
                Exit Property
            End If

            'obtain the type
            For Each asm_type In Me.src_module.GetTypes()

                If asm_type.IsClass Then
                    Debug.Print(asm_type.ToString)
                    If asm_type.ToString = Me.AsmClassName Then
                        Me.loaded_class_type = asm_type
                        Exit Property
                    End If
                End If

            Next

        End Set
    End Property
    Public Property AsmMethod As String
        Get
            'spaces will be underscore function names.
            Return Replace(Me.method_name, " ", "_")
        End Get
        Set(ByVal value As String)
            Me.method_name = value

            If Me.haveClassType Then
                'Get Command method.
                Try
                    Me.Command = Me.loaded_class_type.GetMethod(Me.AsmMethod)
                Catch ex As Exception
                    Me.State = ReflectResultResult.ERR_EXTERNAL_METHOD
                    GlobalObject.MsgColored("Error reflecting command handler '" & Me.AsmMethod & "'. " & ex.Message, Color.Red)
                    Exit Property
                End Try


                If IsNothing(Me.Command) Then
                    Me.State = ReflectResultResult.ERR_EXTERNAL_METHOD_NOT_EXIST
                Else
                    If Me.Command.ReturnType = GetType(SimonsReturnValue) Then
                        If Me.Command.IsPublic Then
                            Me.State = ReflectResultResult.COMMAND_AVAILABLE
                        Else
                            GlobalObject.MsgColored("Error in command handler '" & Me.AsmMethod & "'. command handler function should be declared PUBLIC.", Color.Red)
                            Me.State = ReflectResultResult.ERR_EXTERNAL_METHOD
                        End If
                    Else
                        Me.State = ReflectResultResult.ERR_EXTERNAL_METHOD

                        GlobalObject.MsgColored("Error in command handler '" & Me.AsmMethod & "'. return type should be of 'SIMONSRETURNVALUE'. Found: '" & Me.Command.ReturnType.ToString & "'. @ " & Me.loaded_class_type.ToString & "->" & Me.AsmMethod & " in " & Me.AsmClassName & ".", Color.Red)
                    End If
                End If
            End If
        End Set
    End Property
    Public Property Command As MethodInfo
        Get
            Return Me.loaded_cmd_method
        End Get
        Set(ByVal value As MethodInfo)
            Me.loaded_cmd_method = value

            'create instance of class
            If Not IsNothing(Me.Command) Then
                Dim simons_cmdhandler_session As SimonTypes.SimonsSession

                If SimonGlobalCommandHandler.haveTranslationTable Then
                    simons_cmdhandler_session = Me.Parent.generateSimonsSessionStruct
                Else
                    simons_cmdhandler_session = Nothing
                End If

                Me.loaded_class_instance = Activator.CreateInstance(Me.loaded_class_type, {simons_cmdhandler_session})
            End If

        End Set
    End Property

    Public ReadOnly Property haveSomethingToSay As Boolean
        Get
            Return Me.cmd_text_to_speak <> ""
        End Get
    End Property
    Public ReadOnly Property haveASMModulePath As Boolean
        Get
            Return Me.AsmModulePath <> "IN" And Me.AsmModulePath <> ""
        End Get
    End Property
    Public Shared Function getUnquoted(ByVal parameters As String) As List(Of String)
        'strips strings like "abc" "abc"
        '   "\"abc\""

        Dim inchar() As Char
        Dim outchar() As Char
        Dim x As Integer
        Dim charmax As Long
        Dim char_handled As Boolean
        Dim out_list As List(Of String)
        Dim quote_state As Integer '0=none 1=start 2=end 
        Dim quote_pos(10) As Integer
        Dim start_offset As Long
        Dim current_word As String

        current_word = ""
        start_offset = 0
        quote_state = 0
        out_list = New List(Of String)
        inchar = parameters.ToArray()
        charmax = inchar.Length - 1

        ReDim outchar(inchar.Length * 2)

STRIPPER:
        For x = start_offset To charmax
            char_handled = False

            If inchar(x) = Chr(34) Then

                If quote_state = 0 And x <= charmax Then
                    'check empty argument.
                    If inchar(x + 1) = Chr(34) Then
                        current_word = ""
                        quote_state = 0
                        char_handled = False
                        start_offset = x + 2 'skip escaped string.
                        GoTo STRIPPER
                    End If
                End If

                If x > 0 Then
                    If inchar(x - 1) <> "\" Then
                        quote_state = quote_state + 1
                        quote_pos(quote_state) = x + 1 'mid's absolute pos.
                    End If
                ElseIf x = 0 Then
                    quote_state = quote_state + 1
                    quote_pos(quote_state) = x + 1 'mid's absolute pos.
                End If

                If quote_state = 2 And Not char_handled Then
                    'quote end
                    Dim startpos As Integer
                    Dim endpos As Integer
                    Dim strlen As Integer
                    Dim word_parsed As List(Of String)
                    Dim y As Long

                    startpos = quote_pos(1) + 1
                    endpos = quote_pos(2)
                    strlen = endpos - startpos
                    current_word = Mid(parameters, startpos, strlen)
                    'parse extraction.
                    word_parsed = SimonExternalCommandWrap.getUnquoted(current_word)

                    For y = 0 To word_parsed.Count - 1
                        out_list.Add(word_parsed(y))
                    Next

                    current_word = ""
                    quote_state = 0
                    char_handled = True
                    start_offset = endpos + 1
                    GoTo STRIPPER
                End If
            End If

            If x + 1 < charmax Then
                If inchar(x) = "\" And inchar(x + 1) = Chr(34) Then
                    current_word &= Chr(34)
                    char_handled = True
                    start_offset = x + 2 'skip escaped string.
                    GoTo STRIPPER
                End If
            End If

            If Not char_handled Then

                If inchar(x) = " " And quote_state = 0 Then
                    out_list.Add(current_word)
                    current_word = ""
                    char_handled = True
                End If

            End If

            If Not char_handled Then
                current_word &= inchar(x)
                char_handled = True
            End If

        Next

        If current_word <> "" Then
            out_list.Add(current_word)
        End If

        Return out_list
    End Function
    Public Function ExecCommand(ByVal params As String, ByVal preparse_parameters As Boolean) As SimonsReturnValue

        Dim retval As SimonsReturnValue
        Dim param_list() As String

        retval = SimonsReturnValue.EXTERNAL_INVOKE_ERROR

        If Not Me.State = ReflectResultResult.COMMAND_AVAILABLE Then
            Return SimonsReturnValue.GENERAL_ERROR
        End If

        Try

            If preparse_parameters Then
                param_list = SimonExternalCommandWrap.getUnquoted(params).ToArray
            Else
                ReDim param_list(1)
                param_list(0) = params
            End If


            If Not Me.isObjectCommand Then
                'pass parameter + count.
                retval = Me.loaded_cmd_method.Invoke(Me.loaded_class_instance, {param_list, param_list.Count - 1})
            Else
                'pass objectname +  parameter + count.
                retval = Me.loaded_cmd_method.Invoke(Me.loaded_class_instance, {Me.TargetObjectName, param_list, param_list.Count - 1})
            End If

            If CType(retval, Integer) > 0 Then
                'success
                If Me.haveSomethingToSay() And retval <> SimonsReturnValue.VALIDCOMMAND_NORESULTS Then
                    GlobalSpeech.Text = Me.cmd_text_to_speak
                    GlobalSpeech.SpeakThreaded()
                End If
            Else
                'command failed
            End If

            Return retval

        Catch ex As Exception
            GlobalObject.Msg("Error in command handler '" & Me.loaded_class_type.ToString & "->" & Me.AsmMethod & "' in " & Me.AsmClassName & " : " & ex.Message)
        End Try
        Return retval
    End Function


    Public Property AsmModulePath As String
        Get
            Return Me.module_name
        End Get
        Set(ByVal value As String)
            Me.module_name = value
            Me.src_module = Nothing

            'load and set state
            Try
                If Not (Me.isInternalModule) Then
                    If Not File.Exists(module_name) Then
                        Me.State = ReflectResultResult.ERR_EXTERNAL_MODULE_NOT_FOUND
                        Me.module_name = ""
                    End If

                    Me.src_module = Assembly.LoadFile(module_name)
                Else
                    Me.src_module = Assembly.GetExecutingAssembly()
                End If

                Me.State = ReflectResultResult.LOADED

            Catch ex As Exception
                Me.State = ReflectResultResult.ERR_EXTERNAL_MODULE_LOADER_ERR
            End Try
        End Set
    End Property
    Public Property State As ReflectResultResult
        Get
            Return Me.general_state
        End Get
        Set(ByVal value As ReflectResultResult)
            Me.general_state = value
        End Set
    End Property
    Public Property StateMessage As String
        Get
            Return Me.general_state_msg
        End Get
        Set(ByVal value As String)
            Me.general_state_msg = value
        End Set
    End Property
    Public ReadOnly Property isInternalModule()
        Get
            Return Me.module_name = "IN"
        End Get
    End Property

End Class

Public Enum ReflectResultResult
    ERR_EXTERNAL_METHOD = -6
    ERR_EXTERNAL_METHOD_NOT_EXIST = -5
    ERR_EXTERNAL_MODULE_LOADER_ERR = -4
    ERR_INTERNAL_TYPE_NOT_FOUND = -3
    ERR_EXTERNAL_TYPE_NOT_FOUND = -2
    ERR_EXTERNAL_MODULE_NOT_FOUND = -1
    NEUTRAL = 0
    LOADED = 1
    COMMAND_AVAILABLE = 2
End Enum

Public Structure ReflectResult
    Public found_type As Type
    Public reflectresult As ReflectResultResult
End Structure
