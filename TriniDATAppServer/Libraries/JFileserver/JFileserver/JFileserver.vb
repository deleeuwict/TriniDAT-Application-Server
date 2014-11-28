Option Explicit On
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATServerTypes
Imports System.Runtime.CompilerServices
Imports System
<Assembly: SuppressIldasmAttribute()> 

Public Class JFileserver
    Inherits JTriniDATWebService
    'Class for entry point /admin/json/dataset/<key>/<value>/key/value
    '
    'searches datastory for json objects with key:value
    '
    Private global_mapping_point_config As MappingPointBootstrapData
    Private localuri As String
    ' Private forward_to_rewriter As Boolean 'defines if packets are forwarded or written to io handled
    ' Private forward_class_name As String
    Private homedir As String
    Private Const DEFAULT_PAGE = "index.html"
    Private JPHPCompileRequest_installed As Boolean

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

        Me.localuri = Left(baseURI, baseURI.Length - 1)

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet

        GetIOHandler().Configure(http_events)

       
        Return True

    End Function
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "JAlpha" Then

            If obj.Directive = "MAPPING_POINT_START" Then
                'store mapping point info
                Me.global_mapping_point_config = CType(obj.Attachment, MappingPointBootstrapData)
                Me.homedir = Me.global_mapping_point_config.static_path
                Return False
            ElseIf obj.ObjectType = "HAVEMODULEREPLY" And obj.Directive = "REPLIED" Then
                Me.JPHPCompileRequest_installed = CType(obj.Attachment, Boolean)

                If Me.JPHPCompileRequest_installed Then
                    'send compile request.
                    Dim req As JSONObject
                    req = New JSONObject
                    req.ObjectType = "PHPCompileRequest"
                    req.Directive = obj.Tag
                    req.Sender = Me
                    Me.getMailProvider().Send(req, Nothing, "JPHPCompiler")
                Else
                    Me.getIOHandler().addoutput("Error. JPHPCompiler missing.")
                    Return False
                End If
            End If
            Return False
        End If


        If obj.ObjectType = "JFileserverRequest" And obj.haveDirective Then

            If obj.haveAttachment Then
                If obj.Directive = "UPLOAD" Then
                    Me.sendFileByFullPath(obj.Attachment.ToString)
                    Return False
                End If
            End If
        End If
        If obj.ObjectType = "PHPCompiledScript" And obj.Directive <> "" Then
            Dim php_mime_type As String
            php_mime_type = Me.getMimeTypeByExtension(".php")

            Call Me.getIOHandler().setHTTPResponse(200)
            Call Me.getIOHandler().setOutputMime(php_mime_type)

            Me.getIOHandler().addoutput(obj.Attachment)
            Return False
        End If


        Return False
    End Function

    'Public Sub sendFile(ByVal relative_path As String)

    '    Dim localPath As String
    '    Dim obj As New JSONObject
    '    Dim my_mp As Object '=mappingPointRoot
    '    Dim redirect_url As String

    '    my_mp = getProcessDescriptor().getParent()

    '    If relative_path = my_mp.getURI() Then
    '        localPath = Replace(relative_path, "/", "\")
    '        localPath = homedir & localPath
    '        localPath &= DEFAULT_PAGE
    '        If File.Exists(localPath) Then
    '            'send a redirect to index.html
    '            ' OLD 8 apr 13: Me.disableJOMEGA()
    '            Debug.Print("Sending 302 redirect to app page.")
    '            redirect_url = obj.Directive
    '            redirect_url &= Me.getProcessDescriptor().getParent().applicationid.ToString & "/" & Me.getProcessDescriptor().getParent().getURI() & JFileserver.DEFAULT_PAGE
    '            redirect_url = Replace(redirect_url, "/" & Me.getProcessDescriptor().getParent().getURI(), Me.getProcessDescriptor().getParent().getURI()) 'due to serverurl terminated with / while mapping point always starts with /
    '            Me.getIOHandler().sendRedirecT(redirect_url)
    '            Exit Sub
    '        End If
    '    End If


    '    If Left(relative_path, 1) = "/" Then
    '        'remove leading slash for filepath conversion.
    '        relative_path = Mid(relative_path, 2)
    '    End If

    '    localPath = homedir & Replace(relative_path, "/", "\")

    '    Msg("Request for: " & localPath & "...")

    '    If File.Exists(localPath) Then
    '        Dim fileext As String
    '        Dim mime_type As String

    '        'get MIME-type
    '        fileext = IO.Path.GetExtension(localPath)
    '        mime_type = Me.getMimeTypeByExtension(fileext)

    '        Msg("Serving MIME type: " & mime_type)

    '        If fileext = ".php" Then
    '            'check if PHP Phalanger is installed.
    '            Dim req As JSONObject
    '            req = New JSONObject
    '            req.ObjectType = "JAlpha"
    '            req.Directive = "HAVEMODULE"
    '            req.Attachment = "JPHPCompiler"
    '            req.Tag = localPath
    '            req.Sender = Me
    '            Me.getMailProvider().Send(req, Nothing, "JPHPCompiler")
    '            Exit Sub
    '        End If

    '        If Not Me.forward_to_rewriter Then

    '            If InStr(mime_type, "image/") > 0 Then
    '                'BINARY FILE

    '                Msg("Uploading binary file...")
    '                'write mime-type
    '                Call Me.GetIOHandler().setHTTPResponse(200)
    '                Call Me.GetIOHandler().setOutputMime(mime_type)
    '                Call Me.GetIOHandler().writeFile(True, localPath, True)
    '            Else
    '                'HTML FILE
    '                Me.GetIOHandler().addOutput(File.ReadAllText(localPath))
    '                Call Me.GetIOHandler().setHTTPResponse(200)
    '                Call Me.GetIOHandler().setOutputMime(mime_type)
    '            End If

    '            Exit Sub
    '        End If



    '        'determine if this file type  needs rewrite
    '        If Left(mime_type, 9) = "text/html" Then
    '            'send to rewriter
    '            obj.ObjectType = "JTemplateHTML"
    '            obj.Directive = File.ReadAllText(localPath)
    '        ElseIf mime_type = "text/css" Then
    '            'send through rewriter
    '            obj.ObjectType = "JTemplateCSS"
    '            obj.Directive = File.ReadAllText(localPath)
    '        ElseIf InStr(mime_type, "image/") > 0 Then
    '            Msg("Uploading binary file...")
    '            'write mime-type
    '            Call Me.GetIOHandler().setHTTPResponse(200)
    '            Call Me.GetIOHandler().setOutputMime(mime_type)
    '            Call Me.GetIOHandler().writeFile(True, localPath, True)
    '            Exit Sub
    '        ElseIf mime_type = "text/css" Then
    '            'send through rewriter
    '            obj.ObjectType = "JTemplateCSS"
    '            obj.Directive = File.ReadAllText(localPath)
    '        End If

    '        obj.Directive = Replace(obj.attachment, "$APPID", Me.getProcessDescriptor().getParent().ApplicationId.ToString)

    '        obj.Tag = mime_type
    '        Me.getMailProvider().Send(obj, my_mp, Me.forward_class_name)

    '    Else
    '        Dim err_message As String

    '        If Me.global_mapping_point_config.server_mode = TRINIDAT_SERVERMODE.MODE_DEV Then
    '            err_message = "File not found : " & localPath
    '        Else
    '            err_message = "File not found"
    '        End If

    '        Me.getIOHandler().setHTTPResponse(404)
    '        Me.getIOHandler().writeRaw(True, err_message, True)
    '    End If

    'End Sub
    Public Sub sendFileByFullPath(ByVal localPath As String)

        Dim obj As New JSONObject
        Dim my_mp As Object '=mappingPointRoot

        Msg("sendFileByFullPath: Request for: " & localPath & "...")

        If File.Exists(localPath) Then
            Dim fileext As String
            Dim mime_type As String

            'get MIME-type
            fileext = IO.Path.GetExtension(localPath)
            mime_type = Me.getMimeTypeByExtension(fileext)

            Msg("Serving MIME type: " & mime_type)

            If fileext = ".php" Then
                'check if PHP Phalanger is installed.
                Dim req As JSONObject
                req = New JSONObject
                req.ObjectType = "JAlpha"
                req.Directive = "HAVEMODULE"
                req.Attachment = "JPHPCompiler"
                req.Tag = localPath
                req.Sender = Me
                Me.getMailProvider().Send(req, Nothing, "JPHPCompiler")
                Exit Sub
            End If

            If InStr(mime_type, "image/") > 0 Then
                'BINARY FILE

                Msg("Uploading binary file...")
                'write mime-type
                Call Me.getIOHandler().setHTTPResponse(200)
                Call Me.getIOHandler().setOutputMime(mime_type)
                Call Me.getIOHandler().writeFile(True, localPath, True)
            Else
                'HTML FILE
                Me.getIOHandler().addOutput(File.ReadAllText(localPath))
                Call Me.getIOHandler().setHTTPResponse(200)
                Call Me.getIOHandler().setOutputMime(mime_type)
            End If

        Else
            Dim err_message As String

            If Me.global_mapping_point_config.server_mode = TRINIDAT_SERVERMODE.MODE_DEV Then
                err_message = "File not found : " & localPath
            Else
                err_message = "File not found"
            End If

            Me.getIOHandler().setHTTPResponse(404)
            Me.getIOHandler().writeRaw(True, err_message, True)
        End If

    End Sub
    Public Function getMimeTypeByExtension(ByVal ext As String, Optional ByVal mime_default As String = "text/html;  charset=$ENCODINGNAME") As String


        Select Case ext.ToLower()

            Case ".css"
                Return "text/css"

            Case ".html"
                Return mime_default

            Case ".htm"
                Return mime_default

            Case ".gif"
                Return "image/gif"

            Case ".jpg"
                Return "image/jpeg"

            Case ".jpeg"
                Return "image/jpeg"

            Case ".png"
                Return "image/png"

            Case ".tiff"
                Return "image/tiff"

            Case ".php"
                Return mime_default

            Case Else
                Return "*/*"

        End Select
    End Function


    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnGet")

        '  sendFile(HTTP_URI_Path)


    End Sub

End Class
