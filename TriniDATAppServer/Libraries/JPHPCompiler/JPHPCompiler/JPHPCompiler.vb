Option Explicit On
Imports System
Imports System.Threading
Imports System.Globalization
Imports System.Text
Imports System.Net
Imports System.IO
Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System.Web
Imports PHP.core
Imports PHP.Library
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ApplicationServices
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class JPHPCompiler
    Inherits JTriniDATWebService

    Public Overrides Function DoConfigure() As Boolean

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)
        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        GetIOHandler().Configure(http_events)


        Return True

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "PHPCompileRequest" Then
            Dim script_file As String
            Dim reply As JSONObject
            Dim compiled_phpcode As String

            script_file = obj.Directive 

            If File.Exists(script_file) Then

                compiled_phpcode = compileScript(New FileInfo(script_file))

                reply = New JSONObject
                reply.ObjectType = "PHPCompiledScript"
                reply.Directive = compiled_phpcode
                reply.Sender = Me
                Me.getMailProvider().Send(reply, Nothing, obj.Sender.getClassName())
                Return False
            End If

        End If

    End Function

    Public Function compileScript(ByVal script_file As FileInfo) As String

        Dim php_core_local_config As PHP.Core.LocalConfiguration

        Dim script_context As ScriptContext
        Dim result As MemoryStream
        Dim out_bytes() As Byte
        Dim text_encoding As Encoding
        Dim output As StreamWriter

        text_encoding = Me.getProcessDescriptor().getParent().EncodingPreference

        result = New MemoryStream()
        output = New StreamWriter(result, text_encoding)


        Try
            ' Thread.CurrentThread.CurrentUICulture = New System.Globalization.CultureInfo("en")
            script_context = ScriptContext.CurrentContext
            script_context.WorkingDirectory = script_file.DirectoryName
            script_context.Output = output
            script_context.Include(script_file.FullName, True)
            script_context.Output.Close()
            result.Close()

            out_bytes = result.ToArray

            Return text_encoding.GetString(out_bytes)

        Catch ex As Exception
            Return ex.Message & " @ " & ex.Message
        End Try

    End Function
End Class
