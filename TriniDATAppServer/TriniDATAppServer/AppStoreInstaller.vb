Option Explicit On
Option Compare Text
Imports ICSharpCode.SharpZipLib.BZip2
Imports System.IO
Imports System.Text
Imports ICSharpCode.SharpZipLib.Zip
Imports System.Collections

Public Class AppStoreInstaller


    Private archive_fullpath As String
    Private ext_installer_filepath As String
    Private new_app_name As String
    Private _app_homepath As String
    Private manifest_found As Boolean
    Private temp_license_file As String
    Private new_app_id As Long
    Private force_id As Boolean

    Public Sub New(ByVal _archive As String, Optional ByVal _force_id As Boolean = False)

        Me.archive_fullpath = _archive
        Me.ext_installer_filepath = Nothing
        Me.temp_license_file = Nothing
        Me._app_homepath = Nothing
        Me.force_id = _force_id
    End Sub

    Public Property NewId As Long
        Get
            Return Me.new_app_id
        End Get
        Set(ByVal value As Long)
            Me.new_app_id = value
        End Set
    End Property
    Public ReadOnly Property ArchivePath As String
        Get
            Return Me.archive_fullpath
        End Get
    End Property

    Public ReadOnly Property DefaultOutputFolder As String
        Get

            Return GlobalSetting.getTempDir()
        End Get
    End Property
    Private Property ForceNewId As Boolean
        Get
            Return Me.force_id
        End Get
        Set(ByVal value As Boolean)
            Me.force_id = value
        End Set
    End Property
    Private Function addToApplicationIndex(ByVal dll_files As List(Of FileInfo), ByVal app_xml_filepath As String, ByVal xmanifest As XElement) As Boolean

        'If Not GlobalObject.haveApplicationCache Then
        '    GlobalObject.MsgColored("AppInstaller: app cache is empty.", Color.Red)
        '    Return False
        'End If

        Dim xcache As XDocument
        Dim last_max_id As Long
        Dim new_xapp_path As String
        Dim new_app_node As XElement
        Dim new_app_node_string As String

        If Not GlobalObject.haveApplicationCache Then
            GlobalObject.MsgColored("Fatal error. No application cache available.", Color.Red)
            Return False
        End If

        xcache = GlobalObject.ApplicationCache.IndexDocument

        'SET APP ID.
        If Not Me.ForceNewId Then
            'find next logical app id.
            'get highest id.
            last_max_id = 1
            Dim q_apps = xcache.Descendants("app")

            For Each xapp In q_apps

                If Not IsNothing(xapp.@id) Then
                    If IsNumeric(xapp.@id.ToString) Then
                        If CLng(xapp.@id.ToString) > last_max_id Then
                            'guess new id.
                            last_max_id = CLng(xapp.@id.ToString)
                        End If
                    End If
                End If
            Next

            last_max_id = last_max_id + 1
            Me.NewId = last_max_id
        Else
            'Id is preset by NewId property and constructor parameter force_id=True.
        End If

        GlobalObject.MsgColored("AppInstaller: Assigning ID '" & Me.NewId.ToString & "'...", Color.Pink)

        If Not IsNothing(xmanifest.@name) Then
            Me.AppName = HttpUtility.UrlDecode(xmanifest.@name.ToString)

        Else
            Me.AppName = "Unknown Application With Id #" & Me.NewId.ToString
        End If


        'create new app directory

        Me.HomeDir = GlobalSetting.getAppsRoot() & Me.NewId.ToString & "\"
        new_xapp_path = Me.HomeDir & "liveapp.xml"

        If Directory.Exists(Me.HomeDir) Then
            GlobalObject.MsgColored("AppInstaller warning: Folder '" & Me.HomeDir & "' already exists.", Color.Orange)
        Else
            Try
                GlobalObject.MsgColored("AppInstaller Creating '" & Me.HomeDir & "'...", Color.Pink)

                Directory.CreateDirectory(Me.HomeDir)
            Catch ex As Exception
                GlobalObject.MsgColored("AppInstaller fatal error: Unable to home folder '" & Me.HomeDir & "'. check your file permissions and try again.", Color.Red)
                Return False
            End Try
        End If

        'copy all files from temp folder to app home path.

        For Each dll_file In dll_files

            Try
                GlobalObject.MsgColored("AppInstaller: Installing '" & new_app_name & "' File: " & dll_file.Name & "...", Color.Orange)

                FileCopy(dll_file.FullName, Me.HomeDir & dll_file.Name)

                xmanifest = XElement.Parse(Replace(xmanifest.ToString, dll_file.Name, Me.HomeDir & dll_file.Name))

            Catch ex As Exception
                GlobalObject.MsgColored("AppInstaller: Errir installing '" & new_app_name & "'. Failed to move " & dll_file.FullName & ". Check write permissions in '" & Me.HomeDir & "'. Aborting installation.", Color.Red)
                Return False
            End Try

        Next

        'rewrite manifest dependency

        GlobalObject.MsgColored("AppInstaller: Installing '" & new_app_name & "'...", Color.Orange)
        xmanifest.Save(new_xapp_path)

        'create new app node.
        new_app_node_string = "<app/>"
        new_app_node = XElement.Parse(new_app_node_string)

        If Not IsNothing(xmanifest.@name) Then
            new_app_node.@name = xmanifest.@name.ToString
        Else
            new_app_node.@name = "Application #" & Me.NewId.ToString
        End If

        new_app_node.@filepath = new_xapp_path
        new_app_node.@id = Me.NewId.ToString

        If xcache.Descendants("app").Count = 0 Then
            'insert
            ' xcache.Descendan
            xcache.Root().Add(new_app_node)
        Else
            xcache.Descendants("app").Last.AddAfterSelf(new_app_node)
        End If


        Try
            xcache.Save(GlobalSetting.getApplicationsIndexXMLFilePath())
        Catch ex As Exception
            GlobalObject.MsgColored("AppInstaller: Unable to modify application index: " & ex.Message, Color.Red)
        End Try

        Return True

    End Function
    Public ReadOnly Property haveLicense As Boolean
        Get
            Return Not IsNothing(Me.temp_license_file)
        End Get
    End Property
    Public Property ManifestFound As Boolean
        Get
            Return Me.manifest_found
        End Get
        Set(ByVal value As Boolean)
            Me.manifest_found = value
        End Set
    End Property

    Public ReadOnly Property haveInstaller As Boolean
        Get
            Return Not IsNothing(Me.ext_installer_filepath)
        End Get
    End Property
    Public Property TempLicenseFile As String
        Get
            Return Me.temp_license_file
        End Get
        Set(ByVal value As String)
            Me.temp_license_file = value
        End Set
    End Property
    Public Property AppName As String
        Get
            Return Me.new_app_name
        End Get
        Set(ByVal value As String)
            Me.new_app_name = value
        End Set
    End Property
    Public Property HomeDir As String
        Get
            Return Me._app_homepath
        End Get
        Set(ByVal value As String)
            Me._app_homepath = value
        End Set
    End Property
    Public Property ExtInstaller As String
        Get
            Return Me.ext_installer_filepath
        End Get
        Set(ByVal value As String)
            Me.ext_installer_filepath = value
        End Set
    End Property
    Public Function Install(Optional ByVal extract_license_only As Boolean = False) As Boolean

        Dim currentZipFile As String

        Me.ManifestFound = False

        currentZipFile = Me.ArchivePath

        Try

            Dim strmZipInputStream As ZipInputStream
            Dim objEntry As ZipEntry
            Dim console_msg As String
            Dim app_manifest_filepath As String
            Dim manifest_xmlbuffer As String
            Dim xmanifest As XElement
            Dim output_files As List(Of FileInfo)

            'INIT
            output_files = New List(Of FileInfo)


            Try
                strmZipInputStream = New ZipInputStream(File.OpenRead(currentZipFile))
            Catch ex As Exception
                Return False
            End Try

            If Not extract_license_only Then
                console_msg = "AppInstaller: " & "Reading : " & currentZipFile & "..."
                GlobalObject.MsgColored(console_msg, Color.Pink)
            End If

            objEntry = strmZipInputStream.GetNextEntry()
            app_manifest_filepath = Nothing
            manifest_xmlbuffer = ""

            While IsNothing(objEntry) = False

                If objEntry.IsFile Then

                    Dim outputFileWriter As FileStream
                    Dim output_folder As String
                    Dim output_path As String
                    Dim is_manifest As Boolean

                    is_manifest = False
                    output_folder = Me.DefaultOutputFolder
                    output_path = output_folder & objEntry.Name
                    Debug.Print(output_path)

                    'check for hardcoded common files.
                    If Not Me.haveInstaller Then
                        If (Left(objEntry.Name, 5) = "setup") Then
                            Me.ExtInstaller = output_path
                        End If
                    End If

                    If Not Me.haveLicense Then
                        If objEntry.Name = "license.txt" Then
                            Me.TempLicenseFile = output_path
                        End If
                    End If

                    If extract_license_only And Not Me.haveLicense Then
                        GoTo NEXT_ZIP_ENTRY
                    End If

                    If IsNothing(app_manifest_filepath) Then
                        is_manifest = (objEntry.Name = "AppManifest.xml")
                        If is_manifest Then
                            Me.ManifestFound = True
                            app_manifest_filepath = output_path
                        End If
                    End If

                    If Not Directory.Exists(output_folder) Then
                        Directory.CreateDirectory(output_path)
                    End If

                    If File.Exists(output_path) Then
                        File.Delete(output_path)
                    End If

                    Dim total_bytes As Long
                    Dim last_read_size As Integer
                    Dim bufferSize As Integer = 4048
                    Dim buffer(bufferSize) As Byte

                    outputFileWriter = File.Create(output_path)
                    total_bytes = 0
                    last_read_size = 0

                    While total_bytes < objEntry.Size
                        last_read_size = strmZipInputStream.Read(buffer, 0, buffer.Length)
                        If last_read_size > 0 Then
                            outputFileWriter.Write(buffer, 0, last_read_size)
                            total_bytes += last_read_size

                            If is_manifest Then
                                Dim mbuffer As String
                                mbuffer = System.Text.Encoding.ASCII.GetString(buffer)
                                mbuffer = Mid(mbuffer, 1, last_read_size)
                                manifest_xmlbuffer &= mbuffer
                            End If
                        End If
                    End While

                    If Not extract_license_only Then
                        console_msg = "AppInstaller: " & "Unzip: " & output_path & ". " & total_bytes.ToString & " byte(s) written."
                        GlobalObject.MsgColored(console_msg, Color.Pink)
                    End If

                    outputFileWriter.Close()

                    If extract_license_only Then
                        If Me.haveLicense Then
                            Exit While
                        End If
                    End If

                    If File.Exists(output_path) Then
                        output_files.Add(New FileInfo(output_path))
                    Else
                        Throw New Exception("App installer error: " & output_path & ": file dissapeared.")
                    End If


                End If

NEXT_ZIP_ENTRY:
                objEntry = strmZipInputStream.GetNextEntry()
            End While


            strmZipInputStream.Close()

            If extract_license_only Then
                Return Me.haveLicense
            End If

          
            If Me.ManifestFound = False Or Len(manifest_xmlbuffer) < 10 Then
                'archive file lacks AppManifest.xml
                MsgBox("Unable to install. File 'AppManifest.xml' not found in " & currentZipFile)
                Return False
            End If


            console_msg = "AppInstaller: " & currentZipFile & " successfully extracted. "
            GlobalObject.MsgColored(console_msg, Color.Pink)


            'verify manifest xml
            Try
                xmanifest = XElement.Parse(manifest_xmlbuffer)
            Catch ex As Exception
                console_msg = "Corrupt 'AppManifest.xml' in " & currentZipFile & ". " & vbNewLine & vbNewLine & "XML errors: " & vbNewLine & "'" & ex.Message & "'"
                GlobalObject.MsgColored(console_msg, Color.Red)
                MessageBox.Show(console_msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return False
            End Try


            'add to Application Index.
            If Me.addToApplicationIndex(output_files, app_manifest_filepath, xmanifest) Then


                If Me.haveInstaller Then

                    If MessageBox.Show("'" & Me.AppName & "' comes with an external installer. Do you want to execute it?", "Setup File detected", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.No Then
                        MessageBox.Show("Warning Installer not excuted. The application may not work correctly.", "Installation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
                    Else
                        'trigger
                        Try
                            GlobalObject.MsgColored("Setup: " & Me.ExtInstaller & "...", Color.Pink)

                            GlobalObject.ExecuteFile(Me.ExtInstaller, Me.HomeDir)
                        Catch ex As Exception

                        End Try

                    End If

                End If

                Return True
            End If


        Catch ex As Exception
            MsgBox("Error unpacking '" & currentZipFile & "' : " & ex.Message)

        End Try

        Return False

    End Function



End Class
