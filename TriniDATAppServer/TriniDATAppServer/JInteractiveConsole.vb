

Option Compare Text
Option Explicit On

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes

'Empty JTriniDATWebService instance for COPY/PASTE purposes.
Public Class JInteractiveConsole
    Inherits JTriniDATWebService

    Private my_configuration_info As MappingPointBootstrapData
    Private current_forward_classname As String
    Private do_forward As Boolean
    Private in_proxy_mode As Boolean
    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        Return getIOHandler().Configure(http_events)

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Function ProxySendObject(ByVal obj As JSONObject, ByVal to_classname As String)
        Me.ForwardingAddress = obj.Tag
        Me.do_forward = Me.ForwardingAddress <> "JInteractiveConsole"
        Me.in_proxy_mode = True
        Return Me.getMailProvider().send(obj, Nothing, to_classname)
    End Function
    Public Property ForwardingAddress As String
        Get
            Return Me.current_forward_classname
        End Get
        Set(ByVal value As String)
            Me.current_forward_classname = value
        End Set
    End Property
    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.haveObjectTypeName Then
            'catch message
            If obj.haveDirective Then

                'log messages directly to console
                If obj.haveSender Then

                    If obj.Directive = "LOG" And obj.ObjectTypeName = "JTEXTITEM" And obj.haveAttachment Then
                        If GlobalObject.haveServerThread Then
                            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                                GlobalObject.MsgColored(obj.Sender.ToString & " message: " & vbNewLine & obj.Attachment.ToString, Color.Gray)
                            End If
                        End If
                        Return False
                    End If

                    'do not process mapping point controller objects.
                    If Me.in_proxy_mode And obj.ObjectTypeName <> "JAlpha" And obj.ObjectTypeName <> "JOmega" Then

                        If Not obj.Sender.isjomega And Not obj.Sender.isjalpha Then
                            Dim attachment_output_str As String

                            GlobalObject.MsgColored("JInteractiveConsole: incoming message." & vbNewLine & "From: " & obj.Sender.getClassNameFriendly() & vbNewLine & "OBJECT TYPE: " & obj.ObjectTypeName.ToString & vbNewLine & "DIRECTIVE: " & obj.Directive.ToString & vbNewLine, Color.Red)

                            'INIT
                            attachment_output_str = ""

                            If Me.do_forward Then
                                'pass this output object to another class.
                                GlobalObject.MsgColored("JInteractiveConsole: Forwarding response object to '" & Me.ForwardingAddress & "'...", Color.DarkOrange)
                                obj.Sender = Me
                                Me.getMailProvider().send(obj, Nothing, Me.ForwardingAddress)
                                Return False
                            End If

                            If Not obj.haveAttachment Then
                                obj.Attachment = "(empty)"
                            Else

                                If TypeOf obj.Attachment Is String Then
                                    attachment_output_str = obj.Attachment.ToString
                                ElseIf TypeOf obj.Attachment Is TriniDATHTTPTypes.TriniDATLinkCollection Then
                                    For Each lnk In CType(obj.Attachment, TriniDATHTTPTypes.TriniDATLinkCollection)
                                        attachment_output_str &= lnk.getURL & vbNewLine
                                    Next
                                End If
                            End If

                            GlobalObject.MsgColored(attachment_output_str, Color.Black)
                        End If
                    End If
                End If
            End If
        End If
        Return False
    End Function

   
End Class
