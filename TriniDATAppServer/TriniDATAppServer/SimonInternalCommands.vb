Option Compare Text
Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.Speech.Synthesis
Imports SimonTypes
Imports System.Web
Imports System.Threading
Imports System.Net
Imports TriniDATServerTypes
Imports TriniDATDictionaries
Imports System.IO
Imports Microsoft.Win32

Public Class SimonInternalCommands

    Private info As SimonsSession
    Private Shared last_delete_char As Char

    Public Sub New(ByVal _simons_info As SimonsSession)
        Me.info = _simons_info
    End Sub

    Public ReadOnly Property SimonsInfo As SimonsSession
        Get
            Return Me.info
        End Get
    End Property
    Public Function CLR(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        SimonsInfo.ClearConsole(".")

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function MISSION(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "Find and eliminate John Connor."
        GlobalSpeech.SpeakThreaded()
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function DUTCHY(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "joy joy feelings"
        GlobalSpeech.SpeakThreaded()
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function IP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.serverState = BosswaveServerState.ONLINE Then
            If GlobalObject.haveServerthread Then
                GlobalSpeech.Text = GlobalObject.CurrentServerConfiguration.server_ip.ToString
                GlobalSpeech.SpeakThreaded()
                Me.SimonsInfo.AddConsoleLine(GlobalObject.CurrentServerConfiguration.server_ip.ToString)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If

        Return False
    End Function
    Public Function PORT(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.serverState = BosswaveServerState.ONLINE Then
            If GlobalObject.haveServerthread Then
                GlobalSpeech.Text = GlobalObject.CurrentServerConfiguration.server_port.ToString
                GlobalSpeech.SpeakThreaded()
                Me.SimonsInfo.AddConsoleLine(GlobalObject.CurrentServerConfiguration.server_port.ToString)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If

        Return False
    End Function
    Public Function STATE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.haveServerthread Then

            If GlobalObject.serverState = BosswaveServerState.ONLINE Then
                GlobalSpeech.Text = Me.SimonsInfo.getTranslated("SERVERSTATEONLINE")
                GlobalSpeech.SpeakThreaded()
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("SERVERSTATEONLINE"))
                Return SimonsReturnValue.VALIDCOMMAND
            ElseIf GlobalObject.serverState = BosswaveServerState.OFFLINE Then
                GlobalSpeech.Text = Me.SimonsInfo.getTranslated("SERVERSTATEOFFLINE")
                GlobalSpeech.SpeakThreaded()
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("SERVERSTATEOFFLINE"))
                Return SimonsReturnValue.VALIDCOMMAND
            End If

        Else
            GlobalSpeech.Text = Me.SimonsInfo.getTranslated("SERVERSTATEOFFLINE")
            GlobalSpeech.SpeakThreaded()
            Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("SERVERSTATEOFFLINE"))

            Return SimonsReturnValue.VALIDCOMMAND
        End If

        Return False
    End Function
    Public Function TODAY(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim datestr As String
        datestr = Me.SimonsInfo.getTranslated("PREFIXTODAY") & " " & Now.ToString(Me.SimonsInfo.getTranslated("TODAYFORMAT")) & "."

        GlobalSpeech.Text = datestr
        GlobalSpeech.SpeakThreaded()
        Me.SimonsInfo.AddConsoleLine(datestr, Color.Red)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function MOVESTACK(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        GlobalSpeech.Text = "cannot locate J EDGAR HOOVER."
        GlobalSpeech.SpeakThreaded()
        Me.SimonsInfo.ConsoleTextBox.Text &= "error" & vbNewLine
        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function HELP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim xsimon As XDocument
        Dim current_contextid As String

        xsimon = SimonGlobalCommandHandler.getRealTimeCommandsXMLDoc()

        If actual_count > -1 Then
            'show command help
            Return ShowSingleItemHELPItem(param(0))
        End If

        'show general commands + context dependant commands

        Dim help_query = From my_context_cmd In xsimon.Descendants("commands").Descendants("command") Where (Not IsNothing(my_context_cmd.@help) And Not IsNothing(my_context_cmd.@action) And Not IsNothing(my_context_cmd.@context)) Order By my_context_cmd.@action Ascending
        Dim valid = From my_context_cmd In help_query Where my_context_cmd.@help.ToString <> "" Order By my_context_cmd.@action Ascending

        current_contextid = Me.SimonsInfo.Context.asXMLIdentifierString

        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("HELPHEADER"))

        For Each help_enabled_cmd As XElement In valid
            If help_enabled_cmd.@context.ToString = "all" Or help_enabled_cmd.@context.ToString = current_contextid Then
                Me.outputHelpItem(help_enabled_cmd)
            End If
        Next

        Me.SimonsInfo.AddConsoleLine("<PAGE-UP> or <MOUSEWHEEL-UP>: Scroll up." & vbNewLine & "<PAGE-DOWN> or <MOUSEWHEEL-DOWN>: Scroll down.")

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Private Sub outputHelpItem(ByVal help_enabled_cmd As XElement)
        GlobalObject.MsgColored(help_enabled_cmd.@action.ToString & ": " & HttpUtility.HtmlDecode(help_enabled_cmd.@help.ToString), Color.MidnightBlue)
    End Sub
    Private Function ShowSingleItemHELPItem(ByVal action_name As String) As SimonsReturnValue

        Dim xsimon As XDocument

        xsimon = SimonGlobalCommandHandler.getRealTimeCommandsXMLDoc()

        'show specific help entry
        Dim q = From my_context_cmd In xsimon.Descendants("commands").Descendants("command") Where (Not IsNothing(my_context_cmd.@help) And Not IsNothing(my_context_cmd.@action)) And Not IsNothing(my_context_cmd.@context) Order By my_context_cmd.@context Ascending, my_context_cmd.@action Ascending
        Dim valid = From my_context_cmd In q Where my_context_cmd.@help.ToString <> "" And my_context_cmd.@action = action_name
        Dim current_contextid As String


        current_contextid = Me.SimonsInfo.Context.asXMLIdentifierString


        For Each help_enabled_cmd As XElement In valid
            If help_enabled_cmd.@context.ToString = "all" Or help_enabled_cmd.@context.ToString = current_contextid Then
                Me.outputHelpItem(help_enabled_cmd)
            End If

        Next

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function PLANTLIFE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        GlobalSpeech.Text = "take her to sea mr murdoch"
        GlobalSpeech.SpeakEliteThreaded()
        Return SimonsReturnValue.VALIDCOMMAND

    End Function

    Public Function TIME(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim datestr As String
        datestr = Now.ToString(Me.SimonsInfo.getTranslated("TIMEFORMAT")) & "."

        GlobalSpeech.Text = datestr
        GlobalSpeech.SpeakThreaded()
        Me.SimonsInfo.AddConsoleLine(datestr, Color.Red)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function GOSPASTIC(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim start_tick As Long
        Dim org_back_color As Color
        Dim org_log_color As Color
        Dim org_title As String
        Dim new_color As Color
        Dim new_title As String
        Dim y As Long

        org_title = GlobalObject.serverForm.lblWindowTitle.Text
        org_log_color = Me.SimonsInfo.ConsoleTextBox.BackColor
        start_tick = GlobalObject.GetTickCount

        GlobalSpeech.Text = "got got got it lost lost lllll lost area 51 YYYY why? don't know but I did fff fff found find located retrieved  it yesterday and not tommorow next day next 24 hours .. retrieved and got it again lost it got last summer disk error disk error disk error it ddddd deleted it nnnnn no bbb backups brother was nasty today uncle got it formatted it got got got last last. there is a war dialer on line 33. last delete copy paste invalid user nasty users lost the hardware in funk ware since yesterday not tommorow my nephew is speaking in retrograde tongues planetary movement thirty three and heavenly bodies. hell  now dial plural timer. spacing a smoking philosopher. network people. RETRO phaser.. error. kill it."
        GlobalSpeech.SpeakThreaded()

        org_back_color = GlobalObject.serverForm.BackColor
        y = 0

        Do Until GlobalObject.GetTickCount - start_tick > 70000

            Threading.Thread.CurrentThread.Sleep(50)

            If GlobalObject.haveServerForm Then


                Dim rnd As Random

                rnd = New Random

                If rnd.Next(2) = 1 Then
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                Else
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 1, 0) 'down
                End If


                If y > 2 Then
                    new_color = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeWinColorThreaded, {new_color})
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeLogBackgroundColorThreaded, {new_color})
                    y = 0
                Else
                    new_title = (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeWinTitleThreaded, {new_title})
                End If

                y += 1
            End If
        Loop

        If GlobalObject.haveServerForm Then
            'restore server form
            GlobalObject.serverForm.BackColor = org_back_color
            GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeRestoreTitleThreaded)
            Me.SimonsInfo.ConsoleTextBox.BackColor = org_log_color
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function TECHNIQUE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "simon, what is the proper technique?"
        GlobalSpeech.SpeakEliteThreaded()

        Thread.CurrentThread.Sleep(444)

        GlobalSpeech.Text = ": the water technique!"
        GlobalSpeech.SpeakThreaded()

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function CODESTUB(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Dim code_copy As String
        Try

            code_copy = GlobalSetting.getTempDir() & "mywebservice.txt"

            If IO.File.Exists(GlobalSetting.getConfigRoot() & "webservice.txt") Then
                FileCopy(GlobalSetting.getConfigRoot() & "webservice.txt", code_copy)
                GlobalObject.ExecuteFile("notepad.exe", code_copy)
            Else
                GlobalObject.MsgColored("'" & GlobalSetting.getConfigRoot() & "webservice.txt' was not found.", Color.Red)
            End If

        Catch ex As Exception
            GlobalObject.MsgColored("File copy error: " & ex.Message, Color.Red)

        End Try

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function APPSTUB(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Dim code_copy As String
        Try

            code_copy = GlobalSetting.getTempDir() & "app.txt"

            If IO.File.Exists(GlobalSetting.getConfigRoot() & "app.txt") Then
                FileCopy(GlobalSetting.getConfigRoot() & "app.txt", code_copy)
                GlobalObject.ExecuteFile("notepad.exe", code_copy)
            Else
                GlobalObject.MsgColored("'" & GlobalSetting.getConfigRoot() & "app.txt' was not found.", Color.Red)
            End If

        Catch ex As Exception
            GlobalObject.MsgColored("File copy error: " & ex.Message, Color.Red)

        End Try

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function OBJECTMETHOD_SEND(ByVal object_name As String, ByVal all_parameters() As String, ByVal parameter_count As Integer) As SimonsReturnValue
        'object_name = mapping point (user) id.

        Dim app_prototype As BosswaveApplication
        Dim mp As mapping_point_descriptor

        If Not Me.SimonsInfo.ServerIsOn Then
            GlobalObject.MsgColored("Server not running.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        app_prototype = GlobalObject.ApplicationCache.getByMappingPointUserId(object_name)

        If IsNothing(app_prototype) Then
            Me.SimonsInfo.AddConsoleLine("Invalid mapping point id. Use APPS and APPINFO commands to discover ids")
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        mp = app_prototype.getMappingPointByUserId(object_name)

        If IsNothing(app_prototype) Then
            Me.SimonsInfo.AddConsoleLine("Internal error. Use APPS and APPINFO commands to discover ids")
            Return SimonsReturnValue.GENERAL_ERROR
        End If

        Dim full_request As TriniDATRequestInfo
        Dim object_forward_classname As String
        Dim object_type As String
        Dim object_directive As String
        Dim object_attachment As String
        Dim object_forward_replies_to As String
        Dim mapping_point_slotname As String

        full_request = New TriniDATRequestInfo
        full_request.associated_app = app_prototype
        full_request.mapping_point_desc = mp

        'parse parameter list
        object_directive = Nothing
        object_attachment = Nothing
        object_type = Nothing
        object_forward_classname = Nothing
        mapping_point_slotname = Nothing
        object_forward_replies_to = Nothing

        mapping_point_slotname = all_parameters(0)

        If parameter_count >= 1 Then
            object_forward_classname = all_parameters(1)
        End If

        If parameter_count >= 2 Then
            object_type = all_parameters(2)
        End If

        If parameter_count >= 3 Then
            object_directive = all_parameters(3)
        End If

        If parameter_count >= 4 Then
            object_attachment = all_parameters(4)
        End If

        If parameter_count >= 5 Then
            object_forward_replies_to = all_parameters(5)
        End If


        If parameter_count = -1 Then
            GlobalObject.MsgColored("Missing mapping point sub-url", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If


        If IsNothing(object_forward_classname) Then
            GlobalObject.MsgColored("Missing object recipient", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        If IsNothing(object_type) Then
            GlobalObject.MsgColored("Missing object type", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        If IsNothing(object_directive) Then
            GlobalObject.MsgColored("Missing directive", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        If IsNothing(object_attachment) Then
            GlobalObject.MsgColored("Missing attachment", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If


        Me.SimonsInfo.AddConsoleLine("Send to mapping point " & full_request.FullServerURL & " IN NAME OF: JInteractiveConsole. OBJECT TYPE: " & object_type & " OBJECT DIRECTIVE: " & object_directive & " OBJECT ATTACHMENT: " & object_attachment & ".")

        'prepare json object for transport.
        Dim object_forwarder As ObjectForward
        Dim primitive_json_object As JSONSmallObject

        object_forwarder = New ObjectForward

        primitive_json_object = New JSONSmallObject(GlobalObject.CurrentServerConfiguration.server_ip.ToString, GlobalObject.CurrentServerConfiguration.server_port, Nothing)
        primitive_json_object.Directive = object_directive
        primitive_json_object.ObjectType = object_type
        primitive_json_object.ObjectAttachment = object_attachment

        'recreate absolute url.
        If Left(mapping_point_slotname, 1) = "/" Then
            mapping_point_slotname = Mid(mapping_point_slotname, 2)
        End If

        object_forwarder.ForwardObject = primitive_json_object
        object_forwarder.FullTargetMappingPointURL = full_request.FullServerURL & mapping_point_slotname.ToLower
        object_forwarder.ForwardToClassName = object_forward_classname
        object_forwarder.ForwardReplyForwardClass = object_forward_replies_to

        Dim fw_thread As Thread

        fw_thread = New Thread(AddressOf object_forwarder.SendForward)
        fw_thread.Start()


        Return SimonsReturnValue.VALIDCOMMAND
    End Function


    Public Function EDIT(ByVal Param() As String, ByVal actual_pcount As Integer) As SimonsReturnValue
        'parameter: appname or appid.
        'display app header

        Dim app_found As Boolean
        Dim app_prototype As BosswaveApplication
        Dim prototype_name_or_id As String


        If actual_pcount = -1 Then
            GlobalObject.MsgColored("Need application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        prototype_name_or_id = Param(0)
        prototype_name_or_id = Replace(prototype_name_or_id, Chr(34), "")
        prototype_name_or_id = Trim(prototype_name_or_id)

        app_found = False
        app_prototype = Nothing

        Try

            If IsNumeric(prototype_name_or_id) Then
                app_found = GlobalObject.ApplicationCache.haveApplication(CLng(prototype_name_or_id))

                If app_found Then
                    app_prototype = GlobalObject.ApplicationCache.AppById(CLng(prototype_name_or_id))
                End If
            Else
                app_found = GlobalObject.ApplicationCache.haveApplicationByName(prototype_name_or_id)
                If app_found Then
                    app_prototype = GlobalObject.ApplicationCache.AppByName(prototype_name_or_id)
                End If
            End If

        Catch ex As Exception
            Return SimonsReturnValue.GENERAL_ERROR
        End Try

        If Not app_found Or IsNothing(app_prototype) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Call Me.APPS({""}, -1)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        GlobalObject.ExecuteFile("notepad.exe", app_prototype.Filepath)

        GlobalObject.MsgColored("Editing '" & app_prototype.Filepath & "'.", Color.Green)
        GlobalObject.MsgColored("Enter 'reload' after edit completion.", Color.Green)

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function APPEDIT(ByVal params() As String, ByVal c As Integer) As SimonsReturnValue
        Dim meta_app As BosswaveApplication
        Dim meta_line As String
        Dim prototype_name_or_id As String

        If c = -1 Then
            GlobalObject.MsgColored("Need application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        prototype_name_or_id = params(0)
        prototype_name_or_id = Replace(prototype_name_or_id, Chr(34), "")
        prototype_name_or_id = Trim(prototype_name_or_id)


        meta_app = Me.getAppByNameOrId(prototype_name_or_id)


        If IsNothing(meta_app) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS

        End If

        GlobalObject.MsgColored("Editing app '" & meta_app.ApplicationName & "' meta-data.", Color.Gold)

        'display required.
        meta_line = "Id: " & meta_app.Id.ToString
        GlobalObject.MsgColored(meta_line, Color.Gold)

        GlobalObject.ExecuteFile("notepad.exe", meta_app.app_filepath)

        Return SimonsReturnValue.VALIDCOMMAND

    End Function
    Public Function APPMETA(ByVal params() As String, ByVal c As Integer) As SimonsReturnValue
        'parameter: appname or appid.
        'displays credential information about a app

        Dim meta_app As BosswaveApplication
        Dim meta_line As String
        Dim prototype_name_or_id As String

        If c = -1 Then
            GlobalObject.MsgColored("Need application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        prototype_name_or_id = params(0)
        prototype_name_or_id = Replace(prototype_name_or_id, Chr(34), "")
        prototype_name_or_id = Trim(prototype_name_or_id)


        meta_app = Me.getAppByNameOrId(prototype_name_or_id)


        If IsNothing(meta_app) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        Else
            GlobalObject.MsgColored("App '" & meta_app.ApplicationName & "' meta-data.", Color.Gold)
            'display required.
            meta_line = "Id: " & meta_app.Id.ToString
            GlobalObject.MsgColored(meta_line, Color.Gold)
        End If

        'display required.
        meta_line = " "
        If meta_app.haveApplicationDescription Then
            meta_line &= meta_app.ApplicationDescription
        Else
            meta_line &= "Untitled"
        End If
        GlobalObject.MsgColored(meta_line, Color.Gold)

        'display required.
        meta_line = "Author: "
        If meta_app.haveApplicationAuthor Then
            meta_line &= meta_app.ApplicationAuthor
        Else
            meta_line &= "Untitled"
        End If
        GlobalObject.MsgColored(meta_line, Color.Gold)

        'display required.
        meta_line = "Copyright: "
        If meta_app.haveApplicationCopyright Then
            meta_line &= meta_app.ApplicationCopyrightString
        Else
            meta_line &= "information not present."
        End If
        GlobalObject.MsgColored(meta_line, Color.Gold)

        'display required.
        meta_line = "Company: "
        If meta_app.haveOriginatingBusiness Then
            meta_line &= meta_app.ApplicationOriginatingBusiness
        Else
            meta_line &= "information not present."
        End If
        GlobalObject.MsgColored(meta_line, Color.Gold)

        'display optional info.
        meta_line = "Phone: "
        If meta_app.haveApplicationAuthorContactPhone Then
            meta_line &= meta_app.ApplicationAuthorPhone
            GlobalObject.MsgColored(meta_line, Color.Gold)
        End If

        'display optional info.
        meta_line = "Website: "
        If meta_app.haveApplicationAuthorContactWebsite Then
            meta_line &= meta_app.ApplicationAuthorContactWebsite
            GlobalObject.MsgColored(meta_line, Color.Gold)
        End If

        'display optional info.
        meta_line = "Email: "
        If meta_app.haveApplicationAuthorContactEmail Then
            meta_line &= meta_app.ApplicationAuthorContactEmail
            GlobalObject.MsgColored(meta_line, Color.Gold)
        End If


        'display optional info.
        meta_line = "Yahoo! Id: "
        If meta_app.haveApplicationAuthorContactYahoo Then
            meta_line &= meta_app.ApplicationAuthorYahooID
            GlobalObject.MsgColored(meta_line, Color.Blue)
        End If

        'display optional info.
        meta_line = "Skype ID: "
        If meta_app.haveApplicationAuthorContactSkype Then
            meta_line &= meta_app.ApplicationAuthorSkypeID
            GlobalObject.MsgColored(meta_line, Color.Blue)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Private Function getAppByNameOrId(ByVal prototype_name_or_id As String) As BosswaveApplication

        Dim app_found As Boolean
        Dim app_prototype As BosswaveApplication

        prototype_name_or_id = Replace(prototype_name_or_id, Chr(34), "")
        prototype_name_or_id = Trim(prototype_name_or_id)

        If Len(prototype_name_or_id) = 0 Then
            Return Nothing
        End If

        app_found = False
        app_prototype = Nothing

        Try

            If IsNumeric(prototype_name_or_id) Then
                app_found = GlobalObject.ApplicationCache.haveApplication(CLng(prototype_name_or_id))

                If app_found Then
                    app_prototype = GlobalObject.ApplicationCache.AppById(CLng(prototype_name_or_id))
                End If
            Else
                app_found = GlobalObject.ApplicationCache.haveApplicationByName(prototype_name_or_id)
                If app_found Then
                    app_prototype = GlobalObject.ApplicationCache.AppByName(prototype_name_or_id)
                End If
            End If

        Catch ex As Exception
            Return Nothing
        End Try

        Return app_prototype
    End Function
    Public Function APPINFO(ByVal params() As String, ByVal c As Integer) As SimonsReturnValue
        'parameter: appname or appid.
        'display app header

        Dim app_prototype As BosswaveApplication
        Dim prototype_name_or_id As String

        If c = -1 Then
            GlobalObject.MsgColored("Need application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        prototype_name_or_id = params(0)
        prototype_name_or_id = Trim(prototype_name_or_id)
        app_prototype = Me.getAppByNameOrId(prototype_name_or_id)


        If IsNothing(app_prototype) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Call Me.APPS({""}, -1)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app_type_string As String
        Dim mp_count As Long
        Dim mp_count_string As String
        Dim dependency_count As Integer
        Dim dep_count_string As String

        app_type_string = "normal"

        If app_prototype.IsInterface Then
            app_type_string = "Interface"
        End If


        For Each mp_info In app_prototype.ApplicationMappingPoints
            If mp_info.MappingPoint.HaveDependencyList() Then
                dependency_count += mp_info.MappingPoint.getDependencyPaths.Count
            End If
        Next

        If dependency_count > 0 Then
            If dependency_count = 1 Then
                dep_count_string = "one class dependency"
            Else
                dep_count_string = dependency_count.ToString & " class dependencies"
            End If
        Else
            dep_count_string = "no class dependencies found"
        End If

        If app_prototype.haveMappingPoints Then

            mp_count = app_prototype.ApplicationMappingPoints.Count
            mp_count_string = mp_count.ToString
            If mp_count > 1 Then
                mp_count_string &= " mapping points"
            Else
                mp_count_string &= " mapping point"
            End If
        Else
            mp_count_string = "no mapping points found"
        End If

        Dim t_url As TriniDATRequestInfo
        t_url = New TriniDATRequestInfo
        t_url.associated_app = app_prototype
        t_url.mapping_point_desc = Nothing

        GlobalObject.MsgColoredNonSpaced("'" & app_prototype.ApplicationName & "' Header Info", Color.MidnightBlue)
        GlobalObject.MsgColoredNonSpaced("id: " & app_prototype.Id.ToString & ". '" & app_prototype.ApplicationName & "' type: " & app_type_string & ", " & mp_count_string & ", " & dep_count_string & ".", Color.MidnightBlue)
        GlobalObject.MsgColoredNonSpaced("Full URL: " & t_url.FullServerURL & ".", Color.MidnightBlue)
        GlobalObject.MsgColoredNonSpaced("Physical path: " & app_prototype.Filepath & "." & vbNewLine, Color.MidnightBlue)

        If app_prototype.haveMappingPoints Then
            For Each mp In app_prototype.ApplicationMappingPoints

                Dim mp_id As String

                If mp.haveUserId Then
                    mp_id = mp.UserId
                Else
                    mp_id = "(none)"
                End If

                t_url.mapping_point_desc = mp

                GlobalObject.MsgColoredNonSpaced("Mapping point id: " & mp_id & " . Relative URL: " & mp.URL & ".  Internet URL: " & t_url.FullServerURL, Color.MidnightBlue)

            Next
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    'haveApplicationCache
    Public Function RELOAD(ByVal Params() As String, ByVal cnt As Integer) As SimonsReturnValue
        'reloads the app cache.
        'Note: Ensure always called from GUI thread.
        'todo: validate user permissions.
     
        If Not GlobalObject.haveApplicationCache Then
            GlobalObject.MsgColoredNonSpaced("Invalid state.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        Dim app_count As Long

        If GlobalObject.haveServerForm Then
            GlobalObject.serverForm.setSimonsProgressVisible(True)
        End If

        app_count = GlobalObject.ApplicationCache.Reload()

        If GlobalObject.haveServerForm Then
            GlobalObject.serverForm.setSimonsProgressVisible(False)
        End If

        If GlobalObject.haveServerThread Then
            GlobalObject.MsgColoredNonSpaced("Flushing session cache...", Color.Red)
            GlobalObject.server.FlushSessions()
        End If

        Me.SimonsInfo.AddConsoleLine(app_count.ToString & " app(s) cached and in place.")
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function PUBLISH(ByVal param() As String, ByVal param_count As Integer) As SimonsReturnValue

        Dim publish_app As BosswaveApplication
        Dim retval As SimonsReturnValue
        Dim prototype_name_or_id As String

        If Not GlobalSetting.havePayKey Then
            GlobalObject.MsgColored("You need to configure a pay key first before you can use this feature.", Color.Gold)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        If param_count = -1 Then
            GlobalObject.MsgColored("Needs application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        prototype_name_or_id = param(0)
        prototype_name_or_id = Replace(prototype_name_or_id, Chr(34), "")
        prototype_name_or_id = Trim(prototype_name_or_id)      

        publish_app = Me.getAppByNameOrId(prototype_name_or_id)

        If IsNothing(publish_app) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        Else
            If publish_app.IsInterface Then
                GlobalObject.MsgColored("The current application type cannot be published.", Color.Red)
                GlobalSpeech.Text = Me.SimonsInfo.getTranslated("NOTPUB")
                GlobalSpeech.SpeakThreaded()
                Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
            End If
        End If

        GlobalObject.CurrentAppPublisherForm = New frmSellPackageWizard
        GlobalObject.CurrentAppPublisherForm.PreSelectApp = publish_app
        GlobalObject.CurrentAppPublisherForm.ShowDialog()

        If GlobalObject.CurrentAppPublisherForm.Tag = "OK" Then
            retval = SimonsReturnValue.VALIDCOMMAND
        Else
            GlobalObject.MsgColored("The TriniDAT Appstore publishing wizard was aborted by user.", Color.Red)
            GlobalSpeech.Text = Me.SimonsInfo.getTranslated("NOTPUB")
            GlobalSpeech.SpeakThreaded()
            retval = SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        'reset form objects.
        GlobalObject.CurrentAppPublisherForm = Nothing
        GlobalObject.CurrentUploadProgressForm = Nothing

        Return retval
    End Function
    'GlobalSetting.Latest_SetupURL
    Public Function SETUPURL(ByVal param() As String, ByVal param_count As Integer) As SimonsReturnValue

        GlobalObject.MsgColored(GlobalSetting.Latest_SetupURL, Color.LightPink)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Private Function deleteApp(ByVal app As BosswaveApplication) As Boolean

        Dim temp As String
        Dim deletefile_list As TriniDATWordDictionary
        Dim app_folder As String

        deletefile_list = New TriniDATWordDictionary("", Nothing)

        'Dim filestoDelete As 
        If app.haveMappingPoints Then

            For Each mp_desc In app.ApplicationMappingPoints

                If mp_desc.haveMappingPointInstance Then

                    'delete dependencies
                    For Each dep_file In mp_desc.MappingPoint.getDependencyPaths(True)

                        If IO.File.Exists(dep_file) Then
                            If Not deletefile_list.Has(dep_file) Then
                                GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Deleting dependency: '" & dep_file & "'...", Color.Gold)
                                deletefile_list.Add(dep_file)
                            End If
                        End If

                    Next

                End If

            Next

        End If

        'rewrite application cache
        If GlobalObject.haveApplicationCache Then

            If GlobalObject.ApplicationCache.deleteById(app.Id) Then
                GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Rewriting app cache...ok", Color.Red)
            Else
                GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Uninstallation failed.", Color.Red)
                Return False
            End If

        End If

        app_folder = New FileInfo(app.Filepath).DirectoryName

        temp = Replace(app_folder, GlobalSetting.getAppsRoot(), "")

        If InStr(temp, "\") = 0 Then
            'delete automatic generated app folder.
            Dim native_app_files As ReadOnlyCollection(Of String)

            native_app_files = My.Computer.FileSystem.GetFiles(app_folder, FileIO.SearchOption.SearchAllSubDirectories, {"*.*"})

            'mark for deletion
            For Each f In native_app_files
                Call deletefile_list.Add(f)
            Next
        End If

        If IO.File.Exists(app.Filepath) Then
            'add the manifest xml for deletion.
            GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Deleting manifest: '" & app.Filepath & "'...", Color.Gold)
            deletefile_list.Add(app.Filepath)
        End If

        For Each delete_filepath In deletefile_list.getWordList()
            Try
                File.Delete(delete_filepath)
            Catch ex As Exception

            End Try
        Next

        'app_folder
        GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Deleting folder '" & app_folder & "'...", Color.Gold)

        Try
            Directory.Delete(app_folder)
        Catch ex As Exception
            GlobalObject.MsgColored("[uninstalling '" & app.ApplicationName & "'] Unable to deleting folder: " & ex.Message & "...", Color.Red)
        End Try

        GlobalObject.MsgColored("Done. Deleted " & deletefile_list.getWordCount().ToString & " file(s).", Color.Gold)
        Return True
    End Function
    Public Function WRITELOG(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'writes console to text file.

        Dim fo As SaveFileDialog

        fo = New SaveFileDialog

        fo.Filter = "Text Files|*.txt"

        If Not fo.ShowDialog() Then
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End If

        Try
            File.WriteAllLines(fo.FileName, GlobalObject.serverForm.txtServerActivity.Lines)
            Me.SimonsInfo.AddConsoleLine("Saved to '" & fo.FileName & "'.", Color.AliceBlue)
        Catch ex As Exception
            Me.SimonsInfo.AddConsoleLine("Unable to write to '" & fo.FileName & "': " & ex.Message, Color.Red)
        End Try

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function DOWNLOADFILE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'downloads file: 
        'par1: URL
        'par2: DEST FILE

        If actual_count <> 1 Then
            Return SimonsReturnValue.VALIDCOMMAND_SHOWHELP
        End If

        Dim wc As WebClient
        Dim URL As String
        Dim filepath As String

        URL = param(0).ToLower()
        filepath = param(1)

        'Note: server must not be sensitive to lower-case url format since simon commands get the original char cased changed.
        GlobalObject.MsgColored("Downloading '" & URL & "'...", Color.Gold)

        wc = New WebClient

        Try

            wc.DownloadFile(URL, filepath)

            If File.Exists(filepath) Then
                GlobalObject.MsgColored("Download complete. " & filepath & ". " & New FileInfo(filepath).Length.ToString & " bytes.", Color.Gold)
            End If

        Catch ex As Exception
            GlobalObject.MsgColored("DOWNLOADFILE error: " & ex.Message & ". Check file and/or connection permissions.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        End Try


        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function UNINSTALL(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'user must type: deleteapp x randomletter to confirm

        Dim parts() As String
        Dim msg As String
        Dim delete_app As BosswaveApplication

        parts = param

        If parts.Length = 0 Then

        ElseIf parts.Length = 1 Then

            delete_app = Me.getAppByNameOrId(parts(0))

            If IsNothing(delete_app) Then
                GlobalObject.MsgColored("Invalid application id.", Color.Red)
                Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
            End If

            SimonInternalCommands.last_delete_char = Left(TriniDATUserSession.generateNewSessionId(), 1).ToLower()
            msg = "Note: this action will reload the application cache. Enter 'UNINSTALL " & delete_app.Id.ToString & " " & SimonInternalCommands.last_delete_char.ToString & "' to confirm application deletion."
            GlobalObject.MsgColored(msg, Color.Orange)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        ElseIf parts.Length = 2 Then

            If parts(1) <> SimonInternalCommands.last_delete_char Then
                msg = "Invalid challenge. Permission denied."
                GlobalObject.MsgColored(msg, Color.Red)
                Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
            Else
                'wipe out security challenge
                SimonInternalCommands.last_delete_char = ""
                delete_app = Me.getAppByNameOrId(parts(0))

                If IsNothing(delete_app) Then
                    GlobalObject.MsgColored("Invalid application id.", Color.Red)
                    Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
                Else
                    'protect the application index.
                    If delete_app.Id = 1 Then
                        GlobalObject.MsgColored("Permission denied.", Color.Red)
                        Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
                    End If
                End If

                msg = "Uninstalling app '" & delete_app.ApplicationName & "'..."
                GlobalObject.MsgColored(msg, Color.Red)

                If Me.deleteApp(delete_app) Then
                    'reload the application index.
                    Me.RELOAD(Nothing, -1)
                    Return SimonsReturnValue.VALIDCOMMAND
                End If
            End If
        End If

        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function
    Public Function APPS(ByVal params() As String, ByVal c As Integer) As SimonsReturnValue
        'parameter: appname
        'display app names 

        GlobalObject.MsgColored("Displaying all applications.", Color.Gold)

        For Each app_prototype In GlobalObject.ApplicationCache

            If Not app_prototype.Disabled Then
                GlobalObject.MsgColoredNonSpaced("app id: " & app_prototype.Id.ToString & " full name: " & app_prototype.ApplicationName, Color.MidnightBlue)
            End If

        Next


        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function FINDMP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'find mapping point by id
        Dim result_count As Long
        Dim search_key As TriniDATWordDictionary
   
        If actual_count = -1 Then
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        Else
            If Len(param(0)) > 1 Then
                search_key = New TriniDATWordDictionary("", New List(Of String)({param(0)}))
                result_count = 0
            Else
                GlobalObject.MsgColored("Search key too short. ", Color.Red)
                Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
            End If
        End If

        If GlobalObject.haveApplicationCache Then

            For Each app_proto In GlobalObject.ApplicationCache

                If app_proto.haveMappingPoints Then

                    For Each mp_desc In app_proto.ApplicationMappingPoints

                        If mp_desc.haveNode Then
                            If Not IsNothing(mp_desc.Node.@id) Then
                              
                                If search_key.HasIn(mp_desc.Node.@id) Then
                                    Dim req_info As TriniDATRequestInfo
                                    req_info = New TriniDATRequestInfo
                                    req_info.associated_app = app_proto
                                    req_info.mapping_point_desc = mp_desc
                                    result_count = result_count + 1
                                    GlobalObject.MsgColored("Found: " & req_info.FullServerURL, Color.LightPink)
                                End If

                            End If
                        End If

                    Next

                End If

            Next

        End If

        If result_count < 1 Then
            GlobalObject.MsgColored("Nothing found. ", Color.Orange)
            GlobalSpeech.Text = "nothing is found."
            GlobalSpeech.SpeakThreaded()

        Else

            GlobalObject.MsgColored(result_count.ToString & " result(s).", Color.Orange)
            GlobalSpeech.Text = result_count.ToString & " results."
            GlobalSpeech.SpeakThreaded()
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function


    Public Function DESC(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'change's the application description.
        Dim app_prototype As BosswaveApplication
        Dim prototype_name_or_id As String
        Dim new_app_xml As XDocument

        If actual_count <> 1 Then
            Return SimonsReturnValue.VALIDCOMMAND_SHOWHELP
        End If

        prototype_name_or_id = param(0)
        prototype_name_or_id = Trim(prototype_name_or_id)
        app_prototype = Me.getAppByNameOrId(prototype_name_or_id)


        If IsNothing(app_prototype) Then
            GlobalObject.MsgColored(prototype_name_or_id & " not found. Valid name or numeric id required.", Color.Red)
            Call Me.APPS({""}, -1)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If 'set new application description.


        If actual_count = -1 Then
            GlobalObject.MsgColored("Need application name or id.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        app_prototype.ApplicationNode.@description = param(1)
        new_app_xml = XDocument.Parse(app_prototype.ApplicationNode.ToString)
        app_prototype.XML = new_app_xml

        If app_prototype.Write() Then
            GlobalObject.MsgColored("Saved.", Color.LightPink)
        Else
            GlobalObject.MsgColored("Could not save '" & app_prototype.Filepath & "'.", Color.LightPink)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function SEZZLE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim txt As String
        txt = "simon says simon says simon says simon says simon says simon says"
        GlobalSpeech.Text = txt
        GlobalSpeech.SpeakThreaded()

    End Function

    Public Function WWW(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Dim oPro As New Process
        Dim httpcfg As BosswaveTCPServerConfig

        Me.SimonsInfo.AddConsoleLine("Starting the web browser...")

        httpcfg = GlobalSetting.getHTTPServerConfig()

        GlobalObject.OpenURL("http://" & httpcfg.server_ip.ToString & ":" & httpcfg.server_port.ToString)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function APPINDEX(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Return Me.WWW(Nothing, -1)
    End Function
    Public Function SERVERSTART(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.serverState = BosswaveServerState.ONLINE Then
            GlobalSpeech.Text = Me.SimonsInfo.getTranslated("SERVERSTARTERR")
            GlobalSpeech.SpeakThreaded()
            Return False
        Else
            GlobalSpeech.Text = "starting server"
            GlobalSpeech.SpeakThreaded()
            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.cmdServerStart_Click(Nothing, Nothing)
            End If

            Return SimonsReturnValue.VALIDCOMMAND
        End If
    End Function
    Public Function GEORGE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.SpeakElite("cheese it george", GlobalSetting.getSpeechPath() & GlobalSpeech.SSML_TEMPLATE_FILENAME)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function KAAS(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "I'll buy that for a euro."
        GlobalSpeech.SpeakThreaded()
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function EUROTIME(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "hello, meow."
        GlobalSpeech.SpeakThreaded()
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function WISDOM(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "my grandpa used to say, son.. Knobbing two birds with a single connie... it frankly is gold"
        GlobalSpeech.SpeakThreaded()

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function RAINBOW(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalSpeech.Text = "there is a connie over the rainbow"
        GlobalSpeech.SpeakThreaded()

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function HAMMERTIME(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Dim start_tick As Long
        Dim org_back_color As Color
        Dim org_log_color As Color
        Dim org_title As String
        Dim new_color As Color
        Dim new_title As String
        Dim y As Integer
        Dim x As Integer

        org_title = GlobalObject.serverForm.lblWindowTitle.Text
        org_log_color = Me.SimonsInfo.ConsoleTextBox.BackColor
        start_tick = GlobalObject.GetTickCount

        org_back_color = GlobalObject.serverForm.BackColor

        If GlobalObject.haveServerForm Then

            GlobalSpeech.Text = "stop"
            GlobalSpeech.SpeakThreaded()

            Dim rnd As Random

            rnd = New Random

            For x = 1 To 5
                y = x

                If rnd.Next(2) = 1 Then
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 0, 0)
                Else
                    GlobalObject.SendMessage(Me.SimonsInfo.ConsoleTextBox.Handle, GlobalObject.EM_SCROLL, 1, 0) 'down
                End If


                If y > 2 Then
                    new_color = Color.FromArgb(rnd.Next(255), rnd.Next(255), rnd.Next(255))
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeWinColorThreaded, {new_color})
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeLogBackgroundColorThreaded, {new_color})
                    y = 0
                Else
                    new_title = (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString & (y * GlobalObject.GetTickCount).ToString
                    GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeWinTitleThreaded, {new_title})
                End If

                y += 1
            Next
        End If
        Thread.Sleep(20)

        If GlobalObject.haveServerForm Then
            'restore server form
            GlobalObject.serverForm.BackColor = org_back_color
            GlobalObject.serverForm.Invoke(GlobalObject.serverForm.changeRestoreTitleThreaded)
            Me.SimonsInfo.ConsoleTextBox.BackColor = org_log_color
        End If


        GlobalSpeech.Text = "its hammer time."
        GlobalSpeech.SpeakThreaded()
        '
        '        GlobalSpeech.Text = "stop ... ... .. its hammer time."
        '        GlobalSpeech.SpeakThreaded()

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function VERSION(ByVal params() As String, ByVal actual_count As Integer) As SimonsReturnValue

        GlobalObject.MsgColored("TriniDAT Data Application Server " & GlobalObject.getVersionString(), Color.LightBlue)
        GlobalObject.MsgColored("License: " & GlobalObject.OfficialLicense.getLicenseName(), Color.LightBlue)
        GlobalObject.MsgColored("Server Certificate: " & GlobalObject.getServerCertificateStr, Color.LightBlue)
        'show exec md5 hash.
        Return EXEC(Nothing, -1)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function APPSDIR(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Try
            GlobalObject.ExecuteFile(New FileInfo(GlobalSetting.getApplicationsIndexXMLFilePath()).DirectoryName)
        Catch ex As Exception
            GlobalObject.MsgColored("Error: " & ex.Message, Color.Red)
            Return SimonsReturnValue.GENERAL_ERROR
        End Try

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function SESSIONSDIR(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Try
            GlobalObject.ExecuteFile(GlobalSetting.getSessionRoot())
        Catch ex As Exception
            GlobalObject.MsgColored("Error: " & ex.Message, Color.Red)
            Return SimonsReturnValue.GENERAL_ERROR
        End Try

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function SERVERSTOP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.serverState = BosswaveServerState.OFFLINE Then
            GlobalSpeech.Text = Me.SimonsInfo.getTranslated("SERVERSTOPERR")
            GlobalSpeech.SpeakThreaded()
            Return False
        Else
            GlobalSpeech.Text = "stopping server"
            GlobalSpeech.SpeakThreaded()
            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.cmdServerStop_Click(Nothing, Nothing)
            End If

            Return SimonsReturnValue.VALIDCOMMAND
        End If
    End Function
    Private Function isExecutableExt(ByVal ext As String) As Boolean

        ext = ext.ToLower

        Return (ext = ".exe" Or ext = ".bat" Or ext = ".msi")
    End Function
    Public Function APPDOC(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'executes the app's documentation file.

        If actual_count <> 0 Then
            GlobalObject.MsgColored("Need app id or name.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app As BosswaveApplication
        Dim doc_filepath As String
        Dim file_info As FileInfo
        Dim appfolder As String

        doc_filepath = Nothing
        app = Me.getAppByNameOrId(param(0))

        If IsNothing(app) Then
            GlobalObject.MsgColored("Invalid app id specified.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        appfolder = app.HomeFolder.FullName & "\"

        For Each filename In My.Computer.FileSystem.GetFiles(appfolder, FileIO.SearchOption.SearchTopLevelOnly)
            file_info = New FileInfo(filename)

            If Left(file_info.Name, 4) = "doc." Then
                If Not Me.isExecutableExt(file_info.Extension) Then
                    doc_filepath = file_info.FullName
                    GlobalObject.ExecuteFile(doc_filepath)
                    Exit For
                End If
            End If
        Next

        If IsNothing(doc_filepath) Then
            GlobalObject.MsgColored("The application has no documentation files.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        Else
            GlobalObject.MsgColored("Opening '" & doc_filepath & "'...", Color.LightGray)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function APPICON(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If actual_count <> 0 Then
            GlobalObject.MsgColored("Need app id or name.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app As BosswaveApplication
        Dim doc_filepath As String
        Dim file_info As FileInfo
        Dim appfolder As String

        doc_filepath = Nothing
        app = Me.getAppByNameOrId(param(0))

        If IsNothing(app) Then
            GlobalObject.MsgColored("Invalid app id specified.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        appfolder = app.HomeFolder.FullName & "\"

        For Each filename In My.Computer.FileSystem.GetFiles(appfolder, FileIO.SearchOption.SearchTopLevelOnly)
            file_info = New FileInfo(filename)

            If Left(file_info.Name, 5) = "icon." Then
                If Not Me.isExecutableExt(file_info.Extension) Then
                    doc_filepath = file_info.FullName
                    GlobalObject.ExecuteFile(doc_filepath)
                    Exit For
                End If
            End If
        Next

        If IsNothing(doc_filepath) Then
            GlobalObject.MsgColored("This application does not have an icon file.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS

        Else
            GlobalObject.MsgColored("Opening '" & doc_filepath & "'...", Color.LightGray)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function APPSRC(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'executes the app's src archive file.

        If actual_count <> 0 Then
            GlobalObject.MsgColored("Need app id or name.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app As BosswaveApplication
        Dim doc_filepath As String
        Dim file_info As FileInfo
        Dim appfolder As String

        doc_filepath = Nothing
        app = Me.getAppByNameOrId(param(0))

        If IsNothing(app) Then
            GlobalObject.MsgColored("Invalid app id specified.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        appfolder = app.HomeFolder.FullName & "\"

        For Each filename In My.Computer.FileSystem.GetFiles(appfolder, FileIO.SearchOption.SearchTopLevelOnly)
            file_info = New FileInfo(filename)

            If Left(file_info.Name, 11) = "sourcecode." Then
                If Not Me.isExecutableExt(file_info.Extension) Then
                    doc_filepath = file_info.FullName
                    GlobalObject.ExecuteFile(doc_filepath)
                    Exit For
                End If
            End If
        Next

        If IsNothing(doc_filepath) Then
            GlobalObject.MsgColored("This application does not provide sourcecode files.", Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
        Else
            GlobalObject.MsgColored("Opening '" & doc_filepath & "'...", Color.LightGray)
        End If
        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function EXEC(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        GlobalObject.MsgColored("executable file: " & GlobalSetting.ExecMD5 & ".", Color.Yellow)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function EXPLAIN(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If actual_count = -1 Then
            Return SimonsReturnValue.VALIDCOMMAND_SHOWHELP
        End If

        Dim xsimon As XDocument

        xsimon = SimonGlobalCommandHandler.getRealTimeCommandsXMLDoc()

        'descending order garantuees that an in context node gets selected.
        Dim q = From my_context_cmd In xsimon.Descendants("commands").Descendants("command") Where (Not IsNothing(my_context_cmd.@explain) And Not IsNothing(my_context_cmd.@action)) Order By my_context_cmd.@context Descending

        If q.Count > 0 Then


            Dim valid = From my_context_cmd In q Where my_context_cmd.@action.ToString = param(0)

            If valid.Count > 0 Then

                For Each explain_enabled_cmd In valid

                    GlobalObject.MsgColored("Explanation of command '" & explain_enabled_cmd.@action.ToString & "'.", Color.White)
                    GlobalSpeech.Text = explain_enabled_cmd.@explain
                    GlobalSpeech.SpeakEliteThreaded()
                    Me.SimonsInfo.AddConsoleLine(explain_enabled_cmd.@explain.ToString, Color.Gold)
                    Exit For
                Next
            Else
                GlobalObject.MsgColored(Me.SimonsInfo.getTranslated("NOASSISTANCE"), Color.Red)
            End If
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function

    Public Function CONTEXT(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'return's console command context
        Me.SimonsInfo.AddConsoleLine("Context: " & Me.SimonsInfo.Context.asXMLIdentifierString)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function BOSS(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Return Q(Nothing, 0)
    End Function
    Public Function ENABLEAPP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If actual_count <> 0 Then
            GlobalObject.MsgColored("Need app id or name.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app As BosswaveApplication

        app = Me.getAppByNameOrId(param(0))

        If IsNothing(app) Then
            GlobalObject.MsgColored("Invalid app id specified.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        If GlobalObject.haveApplicationCache Then
            GlobalObject.ApplicationCache.ReEnableapp(app)
            GlobalObject.MsgColored("'" & app.ApplicationName & "' is now enabled.", Color.LightPink)
        Else
            GlobalObject.MsgColored("Error: No application cache available.", Color.Red)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function DISABLEAPP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If actual_count <> 0 Then
            GlobalObject.MsgColored("Need app id or name.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        Dim app As BosswaveApplication

        app = Me.getAppByNameOrId(param(0))

        If IsNothing(app) Then
            GlobalObject.MsgColored("Invalid app id specified.", Color.Red)
            Return SimonsReturnValue.INVALIDCOMMAND_PARAMETERS
        End If

        If GlobalObject.haveApplicationCache Then
            GlobalObject.ApplicationCache.permanentlyDisableApp(app)
            GlobalObject.MsgColored("'" & app.ApplicationName & "' is now disabled.", Color.LightPink)
        Else
            GlobalObject.MsgColored("Error: No application cache available.", Color.Red)
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function DEBUG_APP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        Return SimonsReturnValue.VALIDCOMMAND_TOGGLE_DEBUG_MODE
    End Function
    Public Function Q(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        If GlobalObject.haveServerForm Then
            GlobalObject.serverForm.Quit()
            Return SimonsReturnValue.VALIDCOMMAND
        End If
        Return False
    End Function

End Class



