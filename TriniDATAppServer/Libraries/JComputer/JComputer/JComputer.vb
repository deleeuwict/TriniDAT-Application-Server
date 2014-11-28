Imports System.Net
Imports System.Net.Sockets
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class JComputer
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    '
    Public Sub New()
        MyBase.New()
    End Sub
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox
        mb_events.event_delivered = AddressOf delivered
        mb_events.event_err = AddressOf deliveryerr

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        GetIOHandler().Configure(http_events)


        Return True

    End Function

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
       
        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")
        End If

        If obj.ObjectTypeName = "JReflectError" Then
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().addOutput("err")
            Me.GetIOHandler().Flush(True, True)
            Return False
        End If

        If obj.ObjectTypeName = "JReflectResponseForDOT_NET_COREPROPERTY" Then
            'obj.Attachment = MethodInfo object


            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().addOutput(obj.Attachment.ToString)
            Me.GetIOHandler().Flush(True, True)
            Return False
        End If

        Return False
    End Function
    Public Sub delivered(ByVal obj As JSONObject, ByVal destination_url As String)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub



    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Dim pos As Integer
        Dim mp_root As String
        Dim service_proxysettings_set As String
        Dim service_proxysettings_set_wpad As String
        Dim service_proxysettings_disable As String
        Dim service_property_reflect As String
        Dim service_drives_all As String


        mp_root = Me.getProcessDescriptor().getParent().getURI()
        service_proxysettings_set = mp_root & "proxy/set"
        service_proxysettings_set_wpad = mp_root & "proxy/setwpad"
        service_proxysettings_disable = mp_root & "proxy/disable"
        service_property_reflect = mp_root & "property/"
        service_drives_all = mp_root & "drives"

        If HTTP_URI_Path = service_drives_all Then
            Me.getAllDriveLetters()
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().Flush(True, True)
            Exit Sub
        End If

        pos = InStr(HTTP_URI_Path, "/computer/drive/")
        If pos > 0 Then
            Dim service_drive_freebytes As String
            Dim service_drive_label As String
            Dim indexstr As String
            Dim drive_letter As Char

            HTTP_URI_Path = Replace(HTTP_URI_Path, "/computer/drive/", "")

            'get index
            pos = InStr(HTTP_URI_Path, "/")
            indexstr = Mid(HTTP_URI_Path, 1, pos - 1)

            Try
                drive_letter = CType(indexstr, Char)

            Catch ex As Exception
                'invalid index
                Me.GetIOHandler().setHTTPResponse(404)
                Me.GetIOHandler().writeRaw(True, "Error: invalid adapter index: " & ex.Message, True)
                Exit Sub
            End Try

            'get rid of the indexx
            HTTP_URI_Path = Replace(HTTP_URI_Path, drive_letter.ToString & "/", "")

            'parse the requested service..

            service_drive_freebytes = "freespace"
            service_drive_label = "name"
            'adapter context
            '===========

            If HTTP_URI_Path = service_drive_freebytes Then
                Me.getDriveAvailableSpace(drive_letter)
                Me.GetIOHandler().setHTTPResponse(200)
                Me.GetIOHandler().Flush(True, True)
                Exit Sub
            End If

            If HTTP_URI_Path = service_drive_label Then
                Me.getDriveLabel(drive_letter)
                Me.GetIOHandler().setHTTPResponse(200)
                Me.GetIOHandler().Flush(True, True)
                Exit Sub
            End If

        End If

        If Left(HTTP_URI_Path, Len(service_property_reflect)).ToLower() = service_property_reflect Then
            'send reflection request to JKernel
            '/computer/property/System.Globalization.CultureInfo.CurrentCulture  = languageid
            '/computer/property/System.Globalization.RegionInfo.CurrentRegion = country id
            'System.Environment.UserName or System.Environment.MachineName
            'System.Environment.UserName or System.Environment.UserName = windows username

            Dim reflectPath As String
            Dim req As JSONObject

            req = New JSONObject

            reflectPath = Mid(HTTP_URI_Path, InStrRev(HTTP_URI_Path, "/") + 1)
            req.ObjectType = "JReflectServiceForDOT_NET_COREPROPERTY"
            req.Directive = reflectPath
            req.Tag = reflectPath
            Me.getMailProvider().Send(req, Nothing, "JKernelReflection")

            Exit Sub
        End If


        If HTTP_URI_Path = service_proxysettings_disable Then
            Me.DisableProxy()
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().Flush(True, True)
            Exit Sub
        End If

        If Not IsNothing(HTTP_URI_Parameters) Then

            If HTTP_URI_Path = service_proxysettings_set_wpad And HTTP_URI_Parameters.ContainsKey("url") Then
                Me.setWPAD(HTTP_URI_Parameters("url"))
                Me.GetIOHandler().setHTTPResponse(200)
                Me.GetIOHandler().Flush(True, True)
                Exit Sub
            End If

            If HTTP_URI_Path = service_proxysettings_set And HTTP_URI_Parameters.ContainsKey("proxy") Then
                Me.setProxy(HTTP_URI_Parameters("proxy"))
                Me.GetIOHandler().setHTTPResponse(200)
                Me.GetIOHandler().Flush(True, True)
                Exit Sub
            End If

        End If
    End Sub
 
    Private Sub getDriveAvailableSpace(ByVal val As Char)
        Dim allDrives() As DriveInfo = DriveInfo.GetDrives()

        Dim d As DriveInfo
        Dim drive_letter As String

        For Each d In allDrives
            drive_letter = Left(d.Name, 1)

            If drive_letter.ToLower() = val Then
                If Not d.IsReady Then
                    Me.GetIOHandler().addOutput("busy")
                Else
                    Me.GetIOHandler().addOutput(d.TotalFreeSpace.ToString)
                    Exit For
                End If

            End If


            'Console.WriteLine("  File type: {0}", d.DriveType)
            'If d.IsReady = True Then
            '    Console.WriteLine("  Volume label: {0}", d.VolumeLabel)
            '    Console.WriteLine("  File system: {0}", d.DriveFormat)
            '    Console.WriteLine( _
            '        "  Available space to current user:{0, 15} bytes", _
            '        d.AvailableFreeSpace)

            '    Console.WriteLine( _
            '        "  Total available space:          {0, 15} bytes", _
            '        d.TotalFreeSpace)

            '    Console.WriteLine( _
            '        "  Total size of drive:            {0, 15} bytes ", _
            '        d.TotalSize)
            'End If
        Next
    End Sub
    Private Sub getDriveLabel(ByVal val As Char)
        Dim allDrives() As DriveInfo = DriveInfo.GetDrives()

        Dim d As DriveInfo
        Dim drive_letter As String

        For Each d In allDrives
            drive_letter = Left(d.Name, 1)

            If drive_letter.ToLower() = val Then
                If Not d.IsReady Then
                    Me.GetIOHandler().addOutput("busy")
                Else
                    Me.GetIOHandler().addOutput(d.VolumeLabel.ToString)
                    Exit For
                End If

            End If


            'Console.WriteLine("  File type: {0}", d.DriveType)
            'If d.IsReady = True Then
            '    Console.WriteLine("  Volume label: {0}", d.VolumeLabel)
            '    Console.WriteLine("  File system: {0}", d.DriveFormat)
            '    Console.WriteLine( _
            '        "  Available space to current user:{0, 15} bytes", _
            '        d.AvailableFreeSpace)

            '    Console.WriteLine( _
            '        "  Total available space:          {0, 15} bytes", _
            '        d.TotalFreeSpace)

            '    Console.WriteLine( _
            '        "  Total size of drive:            {0, 15} bytes ", _
            '        d.TotalSize)
            'End If
        Next
    End Sub
    Private Sub getAllDriveLetters()
        Dim allDrives() As DriveInfo = DriveInfo.GetDrives()

        Dim d As DriveInfo
        For Each d In allDrives
            Me.GetIOHandler().addOutput(Left(d.Name, 1) & vbNewLine)

            'Console.WriteLine("  File type: {0}", d.DriveType)
            'If d.IsReady = True Then
            '    Console.WriteLine("  Volume label: {0}", d.VolumeLabel)
            '    Console.WriteLine("  File system: {0}", d.DriveFormat)
            '    Console.WriteLine( _
            '        "  Available space to current user:{0, 15} bytes", _
            '        d.AvailableFreeSpace)

            '    Console.WriteLine( _
            '        "  Total available space:          {0, 15} bytes", _
            '        d.TotalFreeSpace)

            '    Console.WriteLine( _
            '        "  Total size of drive:            {0, 15} bytes ", _
            '        d.TotalSize)
            'End If
        Next
    End Sub

    Private Sub setWPAD(ByVal val As String)
        'e.g. http://192.168.2.1/computer/proxy/setwpad?url=http://192.168.2.1/wpadservice/proxy.dat
        'this file contains the full proxy address in javascript notation

        Dim proxy As ModProxy
        Dim inet_retval As Boolean
        proxy = New ModProxy

        inet_retval = proxy.SetWPADProxyIE8(val)

        If Not inet_retval Then
            inet_retval = proxy.SetWPADProxy(val)
        End If

        Me.GetIOHandler().addOutput(inet_retval.ToString)

    End Sub
    Private Sub setProxy(ByVal val As String)

        Dim proxy As ModProxy
        Dim inet_retval As Boolean
        proxy = New ModProxy

        inet_retval = proxy.SetProxyIE8(val)

        If Not inet_retval Then
            inet_retval = proxy.SetProxy(val)
        End If

        Me.GetIOHandler().addOutput(inet_retval.ToString)

    End Sub

    Private Sub DisableProxy()
        Dim proxy As ModProxy
        Dim inet_retval As Boolean
        proxy = New ModProxy

        inet_retval = proxy.DisableProxyIE8()
        If Not inet_retval Then
            inet_retval = proxy.DisableProxy()
        End If

        Me.GetIOHandler().addOutput(inet_retval.ToString)

    End Sub
End Class
