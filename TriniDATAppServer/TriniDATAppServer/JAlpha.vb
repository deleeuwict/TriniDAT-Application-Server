Option Explicit On
Option Compare Text
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes

Public Class JAlpha 'first class in every chain
    Inherits JTriniDATWebService

    Private mappingpointBytesReceived As Long
    Private mappingpointBytesReceivedTotal As Long
    Private global_mapping_point_config As MappingPointBootstrapData
    'Auto intercepts created mapping points and becomes first class.
    'MUST NEVER USE  SOCKET

    Public Property MPConfig() As MappingPointBootstrapData
        Get
            Return Me.global_mapping_point_config
        End Get
        Set(ByVal value As MappingPointBootstrapData)
            Me.global_mapping_point_config = value
        End Set
    End Property

    Public Sub New()
        MyBase.New()
        Call Init()
    End Sub
    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        'catch message
        If obj.haveSender Then
            If obj.haveDirective Then
                If obj.Directive = "HAVEMODULE" And obj.haveAttachment Then
                    'user asking for mapping point class existance.
                    'Attachment: classname as configured in the application xml file.

                    Dim requested_className As String
                    Dim reply As JSONObject


                    requested_className = obj.Attachment.ToString

                    reply = New JSONObject
                    reply.ObjectType = "HAVEMODULEREPLY"
                    reply.Directive = "REPLIED"
                    reply.Attachment = Me.getProcessDescriptor().getParent().hasClass(requested_className)

                    If obj.haveTag Then
                        reply.Tag = obj.Tag
                    End If

                    Me.getMailProvider().send(reply, Nothing, obj.Sender.getClassNameFriendly())

                    If Me.global_mapping_point_config.server_mode = TRINIDAT_SERVERMODE.MODE_DEV Then
                        GlobalObject.MsgColored("Incoming classname query for '" & requested_className & "'. Reply is sent to '" & obj.Sender.getclassnamefriendly() & "'. ", Color.Orange)
                    End If
                End If
            End If
        End If

        Return False
    End Function
    Private Sub Init()

        mappingpointBytesReceived = 0
        mappingpointBytesReceivedTotal = 0

    End Sub

    Public Overrides Function DoConfigure() As Boolean


        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        'http_events.event_onget = AddressOf OnGet
        http_events.event_onpost = AddressOf OnPost
        Return getIOHandler().Configure(http_events)

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    'called directly
    Public Sub MappingPointStart()

        Dim msg As JSONObject

        Me.Msg("Broadcast Start...")
        msg = New JSONObject
        msg.Sender = Me
        msg.ObjectType = "JAlpha"
        msg.Directive = "MAPPING_POINT_START"
        msg.Attachment = Me.MPConfig

        Call Me.LocalBroadcast(msg)
    End Sub

    'Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

    'End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
      
        Me.putSingle = "http_post_count++"
        Me.putSingle = "http_last_formpost=" & Now.ToString
    End Sub
End Class
