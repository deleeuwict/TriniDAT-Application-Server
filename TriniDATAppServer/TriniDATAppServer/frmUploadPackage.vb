Imports System.IO
Imports System.Net
Imports System.Web
Imports ICSharpCode.SharpZipLib.Zip
Imports ICSharpCode.SharpZipLib.Core
Imports TriniDATDictionaries

Public Class frmUploadPackage

    Public Delegate Sub OnUploadProgChangeDelegate(ByVal current_upload_progress As UploadProgressChangedEventArgs)
    Public Delegate Sub OnUploadProgCompletedDelegate()

    Private wc As WebClient
    Private package_info As PackagedApp
    Public triggerUploadProgressGUIEvent As New OnUploadProgChangeDelegate(AddressOf OnUploadProgressChangedGUICallback)
    Public triggerUploadCompletedGUIEvent As New OnUploadProgCompletedDelegate(AddressOf OnUploadFileCompletedGUICallback)

    Public Sub New()


        ' This call is required by the designer.
        InitializeComponent()

        Me.wc = Nothing
        Me.lblProgress.Text = "Initializing..."

    End Sub
    Public ReadOnly Property haveFileUploader As Boolean
        Get
            Return Not IsNothing(Me.FileUploader)
        End Get
    End Property
    Public Property FileUploader As WebClient
        Get
            Return Me.wc
        End Get
        Set(ByVal value As WebClient)
            Me.wc = value
        End Set
    End Property
    Public Property UploadPackage As PackagedApp
        Get
            Return Me.package_info
        End Get
        Set(ByVal value As PackagedApp)
            Me.package_info = value
            If Not IsNothing(Me.package_info) Then
                ' Me.lblCaption.Text = ":: Publish '" & Me.UploadPackage.SellApp.ApplicationName & "'"
                Me.lblCaption.Text = ":: Publishing Your Application In The TriniDAT Appstore"
            End If
        End Set
    End Property

    Private Function doUploadFile(ByVal upload_file As String) As Boolean
        Try

            'INIT
            Me.FileUploader = New WebClient()
            AddHandler Me.FileUploader.UploadProgressChanged, AddressOf UploadProgressCallback
            AddHandler Me.FileUploader.UploadFileCompleted, AddressOf UploadCompleteCallback

            Me.lblProgress.Tag = "Uploading Manifest"

            Me.FileUploader.UploadFileAsync(New Uri(GlobalSetting.AppStore_UploadURL), upload_file)

        Catch ex As Exception
            Me.lblProgress.Text = "Cannot upload: " & ex.Message
            Return False
        End Try

        Return True

    End Function

    Private Sub OnUploadFileCompletedGUICallback()
        Me.setReadyState("Upload completed")
        Me.Hide()

        MsgBox("Thank you for publishing your app." & vbNewLine & " Your application will be reviewed by a moderator for approval.")
        Me.Close()
        GlobalObject.CurrentAppPublisherForm.Close()


    End Sub
    Private Sub OnUploadProgressChangedGUICallback(ByVal current_upload_progress As UploadProgressChangedEventArgs)
        'called by external Invoke routine only.

        If current_upload_progress.BytesSent < current_upload_progress.TotalBytesToSend Then
            Dim perc As Integer
            perc = ((current_upload_progress.BytesSent / current_upload_progress.TotalBytesToSend) * 100)
            Me.prog.Value = perc
            Me.lblProgress.Text = lblProgress.Tag.ToString & " %" & perc.ToString & ".."
        End If

    End Sub
    Private Shared Sub UploadProgressCallback(ByVal sender As Object, ByVal e As UploadProgressChangedEventArgs)

        If (GlobalObject.haveUploadProgressForm) Then
            GlobalObject.CurrentUploadProgressForm.Invoke(GlobalObject.CurrentUploadProgressForm.triggerUploadProgressGUIEvent, {e})
        End If

    End Sub
    Private Shared Sub UploadCompleteCallback(ByVal sender As Object, ByVal e As UploadFileCompletedEventArgs)

        If (GlobalObject.haveUploadProgressForm) Then
            GlobalObject.CurrentUploadProgressForm.Invoke(GlobalObject.CurrentUploadProgressForm.triggerUploadCompletedGUIEvent)
        End If

    End Sub

    Private Function createManifest() As FileInfo
        'return: output file

        Dim app_file_xml As String
        Dim new_manifest_xml_filepath As String


        'create new manifest
        new_manifest_xml_filepath = GlobalSetting.getTempDir() & "AppManifest.xml"

        Try

            '  FileCopy(Me.UploadPackage.SellApp.Filepath, new_manifest_xml_filepath)
            File.WriteAllText(new_manifest_xml_filepath, Me.UploadPackage.SellApp.XML.ToString)

        Catch ex As Exception
            Me.setReadyState("Insufficient file permissions to create manifest file.")
            GlobalObject.MsgColored("fatal Error: unable to create AppManifest.xml: " & ex.Message, Color.Red)
            Return Nothing
        End Try


        If Not Me.UploadPackage.SellApp.haveMappingPoints Then
            GoTo done
        End If


        Try


            'rewrite file
            app_file_xml = File.ReadAllText(new_manifest_xml_filepath)

            For Each mp_desc In Me.UploadPackage.SellApp.ApplicationMappingPoints

                If mp_desc.haveMappingPointInstance Then

                    If mp_desc.MappingPoint.HaveDependencyList Then

                        Dim all_dll_files As List(Of String)

                        all_dll_files = mp_desc.MappingPoint.getDependencyPaths()

                        For Each dll_filepath In all_dll_files

                            Dim dll_info As FileInfo

                            dll_info = New FileInfo(dll_filepath)

                            'get rid of all directory names in dependency declarations.
                            app_file_xml = Replace(app_file_xml, dll_info.DirectoryName & "\", "")

                        Next

                    End If

                End If

            Next

        Catch ex As Exception
            Me.setReadyState("Unable to parse '" & new_manifest_xml_filepath & "' .")
            GlobalObject.MsgColored("fatal Error: unable to prepare manifest: " & ex.Message & vbNewLine & "Cannot read '" & new_manifest_xml_filepath & "'.", Color.Red)
            Return Nothing
        End Try

        'validate XML

        Dim xdoc As XElement

        Try
            xdoc = XElement.Parse(app_file_xml)

        Catch ex As Exception
            Me.setReadyState("Corrupt manifest file. Please fix definition errors in '" & Me.UploadPackage.SellApp.Filepath & "' .")
            GlobalObject.MsgColored("fatal Error: manifest xml validation failure: " & ex.Message & vbNewLine & "'" & Me.UploadPackage.SellApp.Filepath & "' contains invalid xml.", Color.Red)
            Return Nothing
        End Try


        'write new xml file.
        Try
            File.WriteAllText(new_manifest_xml_filepath, app_file_xml)

        Catch ex As Exception
            Me.setReadyState("Unable to write new version '" & new_manifest_xml_filepath & "' .")
            GlobalObject.MsgColored("fatal Error: unable to prepare manifest: " & ex.Message & vbNewLine & "Cannot write '" & new_manifest_xml_filepath & "'.", Color.Red)
            Return Nothing
        End Try



DONE:

        Return New FileInfo(new_manifest_xml_filepath)

    End Function

    Private Function createZipFile(ByVal manifest_file As FileInfo, ByVal input_files As List(Of FileInfo), ByVal target_file As String) As Boolean

        Dim strmZipOutputStream As ZipOutputStream

        If File.Exists(target_file) Then
            Try
                File.Delete(target_file)
            Catch ex As Exception
                Return False
            End Try
        End If

        Dim fs_zip_file As FileStream
        Dim fs_add_file As FileStream
        Dim app_manifest_file As FileInfo

        Me.lblProgress.Text = "Preparing manifest file..."

        'create new manifest
        app_manifest_file = Me.createManifest()

        If IsNothing(app_manifest_file) Then
            Return False
        Else
            'add to zip input list.
            input_files.Add(app_manifest_file)
        End If

        Me.prog.Maximum += input_files.Count


        fs_zip_file = File.Create(target_file)
        strmZipOutputStream = New ZipOutputStream(fs_zip_file)

        ' Compression Level: 0-9
        ' 0: no(Compression)
        ' 9: maximum compression

        strmZipOutputStream.SetLevel(9)

        Try

            For Each target_file_name In input_files
                Dim abyBuffer(4096) As Byte

                If Not target_file_name.Exists() Then
                    Throw New Exception("Input file error: " & target_file_name.FullName)
                    Return False
                Else
                    Me.lblProgress.Text = "Packing " & target_file_name.Name & "..."
                End If

                fs_add_file = File.OpenRead(target_file_name.FullName)

                Dim objZipEntry As ZipEntry = New ZipEntry(target_file_name.Name)

                objZipEntry.DateTime = DateTime.Now
                objZipEntry.Size = fs_add_file.Length

                strmZipOutputStream.PutNextEntry(objZipEntry)

                StreamUtils.Copy(fs_add_file, strmZipOutputStream, abyBuffer)
                fs_add_file.Close()

                Me.prog.Value += 1
            Next

            strmZipOutputStream.Finish()
            strmZipOutputStream.Close()

        Catch ex As Exception
            strmZipOutputStream.Close()
            fs_zip_file.Close()
            Return False
        End Try


        Return True

    End Function

    Private Sub frmPackagingProgress_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        'start work.

        Dim x As Integer
        Dim tempPath As String
        Dim target_zip_fullpath As String
        Dim dependency_list As List(Of String)
        Dim unique_files As TriniDATWordDictionary


        'INIT
        Me.prog.Value = 0
        unique_files = New TriniDATWordDictionary("", Nothing)
        tempPath = GlobalSetting.getTempDir()
        target_zip_fullpath = tempPath & "trinidat_publisher.zip"

        If Not Directory.Exists(tempPath) Then
            Try
                Directory.CreateDirectory(tempPath)
            Catch ex As Exception
                lblProgress.Text = ex.Message
                Exit Sub
            End Try
        End If

        ' Add any initialization after the InitializeComponent() call.
        Me.prog.Maximum = package_info.SellApp.ApplicationMappingPoints.Count

        For Each mp_desc In package_info.SellApp.ApplicationMappingPoints
            x += 1
            Me.prog.Value = x

            If Not mp_desc.haveMappingPointInstance Then
                Call setReadyState("Invalid mapping point configuration")
            End If

            dependency_list = mp_desc.MappingPoint.getDependencyPaths(True)

            For Each dep_file In dependency_list

                If Not File.Exists(dep_file) Then
                    Call setReadyState("Invalid dependency path " & dep_file)
                    Exit Sub
                Else
                    If Not unique_files.Has(dep_file) Then
                        Call unique_files.Add(dep_file)
                    End If
                End If
            Next

        Next


        'ZIP unique files
        Dim unique_file_infos As List(Of FileInfo)

        'file list to send.
        unique_file_infos = New List(Of FileInfo)

        If unique_files.haveContents Then

            'INIT 2
            dependency_list = unique_files.getWordList()

            For Each unique_filepath In dependency_list
                unique_file_infos.Add(New FileInfo(unique_filepath))
            Next
        End If

        'ADD CUSTOM FILES.

        'add icon
        If Me.UploadPackage.haveIconFile Then
            Dim temp_icon As String
            Dim temp_icon_file As FileInfo
            Dim original_icon As FileInfo

            original_icon = Me.UploadPackage.IconFile_Info
            temp_icon = GlobalSetting.getTempDir() & "icon" & original_icon.Extension

            Try

                'copy original icon 
                FileCopy(original_icon.FullName, temp_icon)

                temp_icon_file = New FileInfo(temp_icon)

                If Not temp_icon_file.Exists Then
                    Throw New Exception(temp_icon)
                End If

            Catch ex As Exception
                MsgBox("Unable to package icon file: " + ex.Message + ". Please verify file permissions.")
                Me.setReadyState("Unable to package icon file.")
                Exit Sub
            End Try

                unique_file_infos.Add(temp_icon_file)
        End If

        'add license
        If Me.UploadPackage.haveLicenseFile Then
            Dim temp_license As String
            Dim temp_license_file As FileInfo
            Dim original_license As FileInfo

            original_license = Me.UploadPackage.LicenseFile_Info
            temp_license = GlobalSetting.getTempDir() & "license" & original_license.Extension

            Try

                'copy original icon 
                FileCopy(original_license.FullName, temp_license)

                temp_license_file = New FileInfo(temp_license)

                If Not temp_license_file.Exists Then
                    Throw New Exception(temp_license)
                End If

            Catch ex As Exception
                MsgBox("Unable to package license file: " + ex.Message + ". Please verify file permissions.")
                Me.setReadyState("Unable to package license.")
                Exit Sub
            End Try

            unique_file_infos.Add(temp_license_file)
        End If


        'add src
        If Me.UploadPackage.haveSourceCodeFile Then
            Dim temp_src As String
            Dim temp_src_file As FileInfo
            Dim original_src As FileInfo

            original_src = Me.UploadPackage.SourceCodeFile_Info
            temp_src = GlobalSetting.getTempDir() & "sourcecode" & original_src.Extension

            Try

                'copy original icon 
                FileCopy(original_src.FullName, temp_src)

                temp_src_file = New FileInfo(temp_src)

                If Not temp_src_file.Exists Then
                    Throw New Exception(temp_src)
                End If

            Catch ex As Exception
                MsgBox("Unable to package source code archive: " + ex.Message + ". Please verify file permissions.")
                Me.setReadyState("Unable to package source code.")
                Exit Sub
            End Try

            unique_file_infos.Add(temp_src_file)
        End If

        'add installer
        If Me.UploadPackage.haveInstallerFile Then
            Dim temp_installer As String
            Dim temp_installer_file As FileInfo
            Dim original_installer As FileInfo

            original_installer = Me.UploadPackage.InstallerFile_Info
            temp_installer = GlobalSetting.getTempDir() & "setup" & original_installer.Extension

            Try

                'copy original icon 
                FileCopy(original_installer.FullName, temp_installer)

                temp_installer_file = New FileInfo(temp_installer)

                If Not temp_installer_file.Exists Then
                    Throw New Exception(temp_installer)
                End If

            Catch ex As Exception
                MsgBox("Unable to package installer file: " + ex.Message + ". Please verify file permissions.")
                Me.setReadyState("Unable to installer file.")
                Exit Sub
            End Try

            unique_file_infos.Add(temp_installer_file)
        End If

        'add documentation
        If Me.UploadPackage.haveDocumentationFile Then
            Dim temp_documentation As String
            Dim temp_documentation_file As FileInfo
            Dim original_documentation As FileInfo

            original_documentation = Me.UploadPackage.DocumentationFile_Info
            temp_documentation = GlobalSetting.getTempDir() & "doc" & original_documentation.Extension

            Try

                'copy original icon 
                FileCopy(original_documentation.FullName, temp_documentation)

                temp_documentation_file = New FileInfo(temp_documentation)

                If Not temp_documentation_file.Exists Then
                    Throw New Exception(temp_documentation)
                End If

            Catch ex As Exception
                MsgBox("Unable to package documentation file: " + ex.Message + ". Please verify file permissions.")
                Me.setReadyState("Unable to documentation.")
                Exit Sub
            End Try

            unique_file_infos.Add(temp_documentation_file)
        End If


        'create packaged app.
        If createZipFile(New FileInfo(Me.UploadPackage.SellApp.Filepath), unique_file_infos, target_zip_fullpath) Then

            'upload it.
            prog.Value = 0
            prog.Maximum = 100

            If Not Me.doUploadFile(target_zip_fullpath) Then
                MsgBox("Unable to upload manifest. Try again later.")
                Me.Close()
            End If
        Else
            Me.setReadyState("Error creating archive.")
        End If

    End Sub

    Private Sub prog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles prog.Click

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click

        Try
            If Me.haveFileUploader Then
                Me.FileUploader.Dispose()
            End If

        Catch ex As Exception

        Finally
            Me.Close()
        End Try

    End Sub

    Private Sub setReadyState(ByVal prog_msg As String)
        Me.cmdCancel.Text = "ok"
        Me.prog.Text = prog_msg

    End Sub
End Class