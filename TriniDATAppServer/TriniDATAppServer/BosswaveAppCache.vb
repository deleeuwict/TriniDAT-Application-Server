Option Compare Text
Option Explicit On
Imports System.IO

Public Class BosswaveAppCache
    Inherits List(Of BosswaveApplication)

    Private allapps_index_doc As XDocument
    Private default_app_index As Integer
    Public myTag As Object
    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property
    Public Function SaveXML() As Boolean

        'getAppsRoot

        Try
            IO.File.WriteAllText(GlobalSetting.getApplicationsIndexXMLFilePath(), Me.IndexDocument.ToString)
            Return True

        Catch ex As Exception
            GlobalObject.MsgColored("Application index Error: cannot write index file: " & ex.Message, Color.Red)
        End Try

        Return False
    End Function
    Public ReadOnly Property XMLAppList As Object
        Get
            Dim q = From app_entry In allapps_index_doc.Descendants("app") 'Where InStr(app_entry.@listenfields.ToString, stat_data.stat_name) > 0
            Return q

        End Get
    End Property
    Public Property IndexDocument As XDocument
        Get
            Return Me.allapps_index_doc
        End Get
        Set(ByVal value As XDocument)
            Me.allapps_index_doc = value
        End Set
    End Property
    Public Sub New()
        Me.Clear()
        Me.IndexDocument = GlobalSetting.getApplicationsIndexXML()

    End Sub

    Public ReadOnly Property DefaultIndex() As Long
        Get
            Return Me.default_app_index
        End Get
    End Property
    Public Function haveApplicationByName(ByVal val As String) As Boolean
        For Each app In Me

            If app.ApplicationName = val Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function getByMappingPointUserId(ByVal val As String) As BosswaveApplication

        For Each app_prototype In Me

            If app_prototype.haveMappingPoints Then
                For Each mp_descriptor In app_prototype.ApplicationMappingPoints
                    If mp_descriptor.haveUserId Then
                        If mp_descriptor.UserId = val Then
                            Return app_prototype
                        End If
                    End If
                Next
            End If
        Next

        Return Nothing
    End Function

    Public ReadOnly Property haveApplication(ByVal x As Long) As Boolean
        Get

            For Each app In Me
                If app.Id = x Then
                    Return True
                End If
            Next

            Return False
        End Get
    End Property
    Public Function AppByName(ByVal val As String) As BosswaveApplication

        For Each app In Me
            If app.ApplicationName = val Then
                Return app
            End If
        Next

        Return Nothing

    End Function

    Public ReadOnly Property AppById(ByVal x As Long) As BosswaveApplication
        Get
            If Not Me.haveApplication(x) Then Return Nothing

            For Each app In Me
                If app.Id = x Then
                    Return app
                End If
            Next

            Return Nothing
        End Get
    End Property

    Public Function Reload() As Long
        Me.IndexDocument = GlobalSetting.getApplicationsIndexXML()
        Return LoadAll(AppIndexLoadAll_ReturnValue.APPLICATION_COUNT)
    End Function

    Public Function LoadAll(ByVal retval As AppIndexLoadAll_ReturnValue) As Integer

        Dim filepath As String
        Dim load_count As Integer
        Dim x As Integer
        Dim all_apps = From app_entry In allapps_index_doc.Descendants("app")
        
        Me.Clear()

        load_count = 0

        If all_apps.Count = 0 Then
            GoTo APP_INDEX_LOAD_COMPLETE
        Else
            'query - ordered.
            all_apps = From app_entry In allapps_index_doc.Descendants("app") Order By CLng(app_entry.@id.ToString) Ascending
        End If

        For x = 0 To all_apps.Count - 1

            Dim app_entry As XElement
            Dim app_entry_instance As BosswaveApplication

            app_entry = all_apps(x)

            If GlobalObject.haveServerForm Then
                GlobalObject.serverForm.Invoke(GlobalObject.serverForm.setSimonProgressBarThreaded, {x + 1, all_apps.Count})
            End If

            filepath = app_entry.@filepath

            If InStr(filepath, "\") = 0 Then
                'relative path
                filepath = GlobalSetting.getAppsRoot() & filepath
            End If

            If File.Exists(filepath) Then
                Try
                    app_entry_instance = New BosswaveApplication(XDocument.Parse(File.ReadAllText(filepath)))

                Catch ex As Exception
                    GlobalObject.MsgColored("Application Index error: Malformed XML in '" & filepath & "': " & ex.Message, Color.Red)
                    GoTo APP_INDEX_LOAD_NEXT
                End Try

                'load header
                app_entry_instance.Filepath = filepath
                app_entry_instance.ApplicationName = app_entry.@name

                If Not IsNothing(app_entry.@disabled) Then
                    If app_entry.@disabled.ToString = "true" Then
                        GlobalObject.Msg("Application '" & app_entry_instance.ApplicationName & "' has disabled attribute set. Edit '" & GlobalSetting.getApplicationsIndexXMLFilePath() & "' to enable.")
                        GoTo APP_INDEX_LOAD_NEXT
                    End If
                End If

                If Not IsNothing(app_entry.@id) Then
                    If IsNumeric(app_entry.@id) Then
                        app_entry_instance.Id = CLng(app_entry.@id)
                        If Not logAppDiagnosticInfo(app_entry_instance) Then
                            If Not Me.permanentlyDisableApp(app_entry_instance) Then
                                Return -1
                            Else
                                'reload cache.
                                Return LoadAll(retval)
                            End If
                        End If
                    Else
                        GlobalObject.Msg("Application Index error: " & app_entry_instance.ApplicationName & " has no id. Application is unavailable.")
                        app_entry_instance.Id = -1
                    End If
                End If

                Me.Add(app_entry_instance)
                If retval = AppIndexLoadAll_ReturnValue.APPLICATION_DEFAULT_INDEX Then
                    If Not IsNothing(app_entry.@default) Then
                        If app_entry.@default.ToString = "1" Then
                            default_app_index = load_count
                            Return default_app_index
                        End If
                    End If
                End If

                load_count = load_count + 1
            Else
                GlobalObject.MsgColored("warning: " & filepath & " not found.", Color.Red)
            End If
APP_INDEX_LOAD_NEXT:
        Next

APP_INDEX_LOAD_COMPLETE:
        If retval = AppIndexLoadAll_ReturnValue.APPLICATION_COUNT Then
            Return load_count
        ElseIf retval = AppIndexLoadAll_ReturnValue.APPLICATION_DEFAULT_INDEX Then
            'not triggered
            Return -1
        End If

    End Function

    Public Function deleteById(ByVal appId As Long) As Boolean

        Dim q_apps = From app_node In Me.IndexDocument.Descendants("app") Where app_node.@id.ToString = appId.ToString

        Try

         If q_apps.Count < 1 Then
            'not found.
            Return False
        End If

        Dim delete_app_node As XElement

        delete_app_node = q_apps(0)
        delete_app_node.Remove()

        Catch ex As Exception
            GlobalObject.MsgColored("Error while deleting application '" & appId.ToString & "': " & ex.Message, Color.Red)
            Return False
        End Try

        'rewrite cache
        Return Me.SaveXML()
    End Function
    Public Function toggleEnableState(ByVal app As BosswaveApplication, ByVal x As Boolean) As Boolean
        'flag app as disabled in index file

        Dim q_apps = From apps_node In Me.IndexDocument.Descendants("app") Where apps_node.@id.ToString = app.Id.ToString
        Dim app_node As XElement

        If q_apps.Count < 1 Then
            'even worse.
            Return False
        End If

        app.Disabled = True

        app_node = q_apps(0)

        If Not x Then
            app_node.@disabled = "true"
        Else
            app_node.@disabled = "false"
        End If


        'write xml
        Return Me.SaveXML()

    End Function
    Public Function ReEnableapp(ByVal app As BosswaveApplication) As Boolean
        'flag app as enabled in index file.
        Return Me.toggleEnableState(app, True)

    End Function
    Public Function permanentlyDisableApp(ByVal app As BosswaveApplication) As Boolean
        'flag app as disabled in index file.
        Return Me.toggleEnableState(app, False)

    End Function
    Public Function logAppDiagnosticInfo(ByRef app As BosswaveApplication) As Boolean

        If IsNothing(app) Then
            app.DisableReason = BOSSWAVE_APP_DISABLE_REASON.MISSING_APP
            Return False
            Exit Function
        End If


        Dim mp_count As Long
        Dim dependency_count As Long

        Dim mp_count_string As String
        Dim dep_count_string As String
        Dim app_type_string As String


        mp_count = -1
        dependency_count = 0
        mp_count_string = "no mapping points"
        dep_count_string = "no dependencies"
        app_type_string = "normal"
        app.DisableReason = app.Load()

        If Not app.DisableReason = BOSSWAVE_APP_DISABLE_REASON.NONE Then
            GlobalObject.MsgColored("   Application Index: There were errors initializing '" & app.ApplicationName & "'.", Color.Red)
            Return False
        End If

        If app.IsInterface Then
            app_type_string = "Interface"
        End If


        If app.haveMappingPoints Then
            mp_count = app.ApplicationMappingPoints.Count
            mp_count_string = mp_count.ToString
            If mp_count > 1 Then
                mp_count_string &= " mapping points"
            Else
                mp_count_string &= " mapping point"
            End If

            'count dependencies
            For Each mp_info In app.ApplicationMappingPoints
                If mp_info.MappingPoint.HaveDependencyList() Then
                    dependency_count += mp_info.MappingPoint.getDependencyPaths.Count
                End If
            Next

            If dependency_count > 0 Then
                If dependency_count = 1 Then
                    dep_count_string = "one dependency"
                Else
                    dep_count_string = dependency_count.ToString & " dependencies"
                End If

            End If

        End If

        'LOG DIAGNOSTIC INFO
        '================

        GlobalObject.MsgColored("   " & app.Id.ToString & ". App '" & app.ApplicationName & "' type: " & app_type_string & ", " & mp_count_string & ", " & dep_count_string & ".", Color.MidnightBlue)

        Return True


    End Function

    Public Shared Function getEntryPoint() As BosswaveApplication

        ''load the default application
        'Dim app_directory As BosswaveApplicationIndex
        'Dim default_app_index As Integer

        'app_directory = New BosswaveApplicationIndex()
        'default_app_index = app_directory.LoadAll(AppIndexLoadAll_ReturnValue.APPLICATION_DEFAULT_INDEX)

        'If default_app_index <> -1 Then
        '    Return app_directory.Apps(default_app_index)
        'Else
        '    Return Nothing
        'End If

    End Function
End Class

Public Enum AppIndexLoadAll_ReturnValue
    APPLICATION_COUNT = 1
    APPLICATION_DEFAULT_INDEX = 2
End Enum