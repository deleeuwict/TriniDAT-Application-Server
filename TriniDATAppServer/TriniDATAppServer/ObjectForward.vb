Option Explicit On
Option Compare Text
Imports System.Net
Imports TriniDATHTTPBrowser
Imports TriniDATServerTypes
Imports System.Threading
Imports System.Reflection
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports Microsoft.CSharp
Imports System.IO

Public Class ObjectForward
    Private primitive_json_object As JSONSmallObject
    Private mybrowser As TriniDATHTTPBrowser.TriniDATHTTPBrowser
    Private forward_classname As String
    Private forward_objectreply_forwarder As String
    Private target_mp_url As String
    Private sender_classname As String
    Private mysessionid As String
    Public Sub New()
        Me.forward_classname = Nothing
        Me.forward_objectreply_forwarder = Nothing
        Me.sender_classname = Nothing
        Me.mysessionid = Nothing
    End Sub
    Public ReadOnly Property haveSessionID As Boolean
        Get
            Return Not IsNothing(Me.mysessionid)
        End Get
    End Property
    Public Property Browser As TriniDATHTTPBrowser.TriniDATHTTPBrowser
        Get
            Return Me.mybrowser
        End Get
        Set(ByVal value As TriniDATHTTPBrowser.TriniDATHTTPBrowser)
            Me.mybrowser = value
        End Set
    End Property
    Public ReadOnly Property haveForwardingObject As Boolean
        Get
            Return Not IsNothing(Me.ForwardObject)
        End Get
    End Property
    Public Property SessionID As String
        Get
            Return Me.mysessionid
        End Get
        Set(ByVal value As String)
            Me.mysessionid = value
        End Set
    End Property
    Public Property ForwardObject As JSONSmallObject
        Get
            Return Me.primitive_json_object
        End Get
        Set(ByVal value As JSONSmallObject)
            Me.primitive_json_object = value
        End Set
    End Property
    Public Property FullTargetMappingPointURL As String
        Get
            Return Me.target_mp_url
        End Get
        Set(ByVal value As String)
            Me.target_mp_url = value
        End Set
    End Property
    Public ReadOnly Property haveSender() As Boolean
        Get
            Return Not IsNothing(Me.SenderClassName)
        End Get
    End Property
    Public Property SenderClassName As String
        Get
            Return Me.sender_classname
        End Get
        Set(ByVal value As String)
            Me.sender_classname = value
        End Set
    End Property
    Public Property ForwardReplyForwardClass As String
        Get
            Return Me.forward_objectreply_forwarder
        End Get
        Set(ByVal value As String)
            Me.forward_objectreply_forwarder = value
        End Set
    End Property
    Public ReadOnly Property haveReplyForwardClass() As Boolean
        Get
            Return Not IsNothing(Me.ForwardReplyForwardClass)
        End Get
    End Property
    Public ReadOnly Property haveForwardClassName
        Get
            Return Not IsNothing(Me.forward_classname)
        End Get
    End Property
    Public Property ForwardToClassName As String
        Get
            Return Me.forward_classname
        End Get
        Set(ByVal value As String)
            Me.forward_classname = value
        End Set
    End Property
    Public Function SendForward() As Boolean

        If Not Me.haveForwardClassName Or Not Me.haveForwardingObject Then
            Return False
        End If

        Dim object_json As String
        Dim encoded_object_pos As Integer

        Me.Browser = New TriniDATHTTPBrowser.TriniDATHTTPBrowser(GlobalObject.OfficialLicense)

        object_json = primitive_json_object.getPackedHTTPString(New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.RESPONSE_MODIFIED_OBJECT))
        encoded_object_pos = InStr(object_json, vbCrLf & vbCrLf) + 4

        object_json = Mid(object_json, encoded_object_pos)
        object_json = Replace(object_json, vbCrLf, "")

        If Not Me.haveSender Then
            Me.SenderClassName = "JInteractiveConsole"
        End If

        If Me.haveSessionID Then
            Me.Browser.Headers.Add("Cookie", GlobalSetting.SESSION_COOKIE_NAME & "=" & Me.SessionID & ";")
            'Me.Browser.Headers.Add("Cookie", "abc")
        End If

        Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT-FROM", Me.SenderClassName)
        Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT-TO", Me.ForwardToClassName)

        If Me.haveReplyForwardClass Then
            Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT-ENDPOINT", Me.ForwardReplyForwardClass)
        End If

        Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT", HttpUtility.UrlEncode(object_json))

        Try

            If Me.Browser.execGet(Me.FullTargetMappingPointURL) Then

                'Me.SimonsInfo.AddConsoleLine(b.Tag & ": object sending completed.")
                Dim response_html As String
                response_html = Me.Browser.getXMLDoc()

            End If

        Catch ex As Exception

        End Try

        Return True
    End Function


    Public Function SendBrowser() As Boolean

        If Not GlobalObject.haveHiddenExecutionForm Then
            Return False
        End If

        Dim object_json As String
        Dim encoded_object_pos As Integer
        Dim src_code As String
        Dim output_exe_path As String
        Dim bdev_mode As Boolean


        If GlobalObject.haveServerThread Then
            bdev_mode = (GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV)
        Else
            bdev_mode = True
        End If

        object_json = primitive_json_object.getPackedHTTPString(New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.RESPONSE_MODIFIED_OBJECT))
        encoded_object_pos = InStr(object_json, vbCrLf & vbCrLf) + 4

        object_json = Mid(object_json, encoded_object_pos)
        object_json = Replace(object_json, vbCrLf, "")


        'write HTTP fetcher source-code.
        src_code = "Imports System" & vbNewLine
        If bdev_mode Then
            src_code &= "Imports System.Web" & vbNewLine
        End If

        src_code &= "Imports System.Net" & vbNewLine & vbNewLine

        src_code &= "Module Module1" & vbNewLine

        src_code &= "Sub Main()" & vbNewLine

        src_code &= "Dim wc As WebClient" & vbNewLine
        src_code &= "Dim URL as String" & vbNewLine
        src_code &= "Dim encoded_object as String" & vbNewLine

        src_code &= "wc = New WebClient" & vbNewLine
        src_code &= "URL = '$TARGETURL' " & vbNewLine

        'Add GET request headers.
        If Me.haveSessionID Then
            src_code &= "wc.Headers.Add('Cookie', '" & GlobalSetting.SESSION_COOKIE_NAME & "=$SESSIONID') " & vbNewLine
        End If

        src_code &= "wc.Headers.Add('X-MAPPINGPOINT-OBJECT-FROM', '" & Me.SenderClassName & "') " & vbNewLine
        src_code &= "wc.Headers.Add('X-MAPPINGPOINT-OBJECT-TO', '" & Me.ForwardToClassName & "') " & vbNewLine
        src_code &= "encoded_object = '" & HttpUtility.UrlEncode(object_json) & "'" & vbNewLine
        src_code &= "wc.Headers.Add('X-MAPPINGPOINT-OBJECT',encoded_object) " & vbNewLine

        If Me.haveReplyForwardClass Then
            '  Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT-ENDPOINT", Me.ForwardReplyForwardClass)
            src_code &= "wc.Headers.Add('X-MAPPINGPOINT-OBJECT-ENDPOINT', '" & Me.ForwardReplyForwardClass & "') " & vbNewLine
        End If

        ' Me.Browser.Headers.Add("X-MAPPINGPOINT-OBJECT", HttpUtility.UrlEncode(object_json))

        src_code &= "Try" & vbNewLine

        src_code &= "" & vbNewLine
        src_code &= "Console.BackgroundColor = ConsoleColor.Black" & vbNewLine
        src_code &= "Console.ForegroundColor = ConsoleColor.Red" & vbNewLine
        src_code &= "Console.WriteLine('Fetch: ' & URL & '...')" & vbNewLine

        If bdev_mode Then
            src_code &= "Console.ForegroundColor = ConsoleColor.White" & vbNewLine
            src_code &= "Console.WriteLine('Object:')" & vbNewLine
            src_code &= "Console.WriteLine(HttpUtility.URLDecode(encoded_object))" & vbNewLine
        End If

        src_code &= "Console.WriteLine(wc.DownloadString(URL))" & vbNewLine

        src_code &= "" & vbNewLine

        src_code &= "Catch ex As Exception" & vbNewLine

        src_code &= "End Try" & vbNewLine

        src_code &= "" & vbNewLine

        src_code &= "End Sub" & vbNewLine

        src_code &= "End Module" & vbNewLine

        src_code = Replace(src_code, "$SESSIONID", Me.SessionID)
        src_code = Replace(src_code, "$TARGETURL", Me.FullTargetMappingPointURL)
        src_code = Replace(src_code, "'", Chr(34))

        'example:  CompileCode(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, "<srccode>", output_exe_file)

        output_exe_path = GlobalSetting.getTempDir() & GlobalSetting.EXECPREFIX_ACTION_OBJECTFORWARDER & GlobalObject.GetTickCount().ToString & ".exe"

        If SimpleCodeCompiler.VBSourceCodeCompiler(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, src_code, output_exe_path) Then
            'Call GlobalObject.ExecuteFile(output_exe_path)
            GlobalObject.AddAction(output_exe_path, "")

            If bdev_mode Then
                GlobalObject.MsgColored("Executing 'forward object' action..", Color.Red)
            End If

            If GlobalObject.ExecuteNextAction() Then
                If bdev_mode Then
                    GlobalObject.MsgColored(" 'forward object' action complete.", Color.Pink)
                End If
            End If
        Else
            src_code = src_code
        End If

        Return True

    End Function
End Class
