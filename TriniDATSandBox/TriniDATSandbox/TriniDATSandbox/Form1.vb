Imports System.Reflection
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System
Imports System.Security
Imports System.Security.Policy
Imports System.Security.Permissions
Imports System.IO

Public Class Form1

    Private tdat As Assembly
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Dim dll_files As ReadOnlyCollection(Of String)

        Dim exec_dir As String
        Dim app_path As String
        Dim ep As MethodInfo
        Dim sandbox As AppDomain
        Dim fulltrust_list As List(Of StrongName)
        Dim temp
        Dim temp_types() As Type

        fulltrust_list = New List(Of StrongName)

        exec_dir = "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\TriniDATAppServer\TriniDATAppServer\"
        app_path = exec_dir & "bin\Release\TriniDAT.exe"

        dll_files = My.Computer.FileSystem.GetFiles(exec_dir, FileIO.SearchOption.SearchAllSubDirectories, "*.dll")

        tdat = Assembly.Load(File.ReadAllBytes(app_path))

        temp = tdat.Evidence.GetHostEnumerator

        While temp.MoveNext()
            MsgBox(temp.Current.ToString())
        End While



        '  fulltrust_list.Add(temp.Assembly.Evidence.GetHostEvidence(Of StrongName))


        sandbox = createAppDomain(exec_dir, fulltrust_list)

        AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf MyResolver
        'sandbox.Load(File.ReadAllBytes(app_path))

        Try
            sandbox.ExecuteAssembly(app_path)
        Catch ex As Exception

            If TypeOf ex Is SecurityException Then
                ex = ex
                txtLog.Text = CType(ex, SecurityException).FirstPermissionThatFailed.ToString
                Exit Sub
            End If
        End Try


        'For Each dll_name In dll_files
        '    sandbox.Load(File.ReadAllBytes(dll_name))
        'Next


        'tdat = tdat

        'ep = tdat.EntryPoint
        'ep = ep
        'ep.Invoke(Nothing, Nothing)





    End Sub
    Function MyResolver(ByVal sender As Object, ByVal args As ResolveEventArgs) As System.Reflection.Assembly
        Dim domain As AppDomain = DirectCast(sender, AppDomain)
        domain = domain

        If Mid(args.Name, 1, 8) = "TriniDAT" Then
            Return tdat
        End If


    End Function




    Private Function createAppDomain(ByVal full_executable_path As String, ByVal fulltrustlist As List(Of StrongName)) As AppDomain
        Dim adSetup As AppDomainSetup
        Dim trinidat_executable_permissions As PermissionSet
        Dim fullTrustAssembly As StrongName
        Dim exec_path_info As FileInfo
        Dim ev As Evidence

        exec_path_info = New FileInfo(full_executable_path)

        ev = New Evidence
        ev = AppDomain.CurrentDomain.Evidence

        ev.AddHostEvidence(New Zone(SecurityZone.Internet))
        trinidat_executable_permissions = SecurityManager.GetStandardSandbox(ev)

        adSetup = New AppDomainSetup
        adSetup.ApplicationBase = exec_path_info.DirectoryName

        'add read access
        trinidat_executable_permissions.AddPermission(New FileIOPermission(FileIOPermissionAccess.Read, exec_path_info.FullName))
        trinidat_executable_permissions.AddPermission(New FileIOPermission(FileIOPermissionAccess.PathDiscovery, exec_path_info.DirectoryName))
        trinidat_executable_permissions.AddPermission(New SecurityPermission(SecurityPermissionFlag.Execution))
        'trinidat_executable_permissions.AddPermission(New AllowPartiallyTrustedCallersAttribute())

        Return AppDomain.CreateDomain("NewTriniDATSandbox", Nothing, adSetup, trinidat_executable_permissions, fulltrustlist.ToArray)


    End Function
End Class
