Option Explicit On
Option Compare Text

Imports Newtonsoft.Json
Imports TriniDATServerTypes
Imports TriniDATDictionaries
Imports System.Web
Imports TriniDATSockets
Imports System.Net.Sockets

Public Class JSONObject
    Private myid As Integer
    Protected myowner As Object  '=JTriniDATWebService
    Public ObjectType As String
    Public Directive As String
    Public Attachment As Object
    Public Tag As String 'additional data
    Private payment_tag As String
    Public Sub New()
        '  stats = New JSONObjectSentStats
        Me.Sender = Nothing
        Me.Attachment = Nothing
        Me.payment_tag = ""
    End Sub
    Public Property Sender As Object
        Get
            Return Me.myowner
        End Get
        Set(ByVal value As Object)
            Me.myowner = value

            If Not IsNothing(value) Then
                Me.myowner = Me.myowner
            End If
        End Set
    End Property
    Public ReadOnly Property haveObjectTypeName As Boolean
        Get
            Return Not IsNothing(Me.ObjectTypeName)
        End Get
    End Property
    Public ReadOnly Property haveTag As Boolean
        Get
            Return Not IsNothing(Me.Tag)
        End Get
    End Property
    Public ReadOnly Property haveAttachment As Boolean
        Get
            Return Not IsNothing(Me.Attachment)
        End Get
    End Property
    Public ReadOnly Property havePaymentHandle As Boolean
        Get
            Return Me.payment_tag.Length > 0
        End Get
    End Property
    Public Property PaymentHandle
        Get
            Return Me.payment_tag
        End Get
        Set(ByVal value)
            Me.payment_tag = value
        End Set
    End Property
    Protected ReadOnly Property Id()
        Get
            Return Me.myid
        End Get
    End Property
    Public ReadOnly Property haveSender As Boolean
        Get
            Return Not IsNothing(Me.Sender)
        End Get
    End Property


    Protected Function doObjectRequest(ByVal packaging_method As JTRANSPORTABLE_METHOD, ByVal auto_disconnect As Boolean) As JSONSendResult
        Dim request_str As String
        Dim http_parser As Object
        Dim sender_mp As Object
        Dim packable_object As JSONSmallObject
        Dim response_info As JSONSendResult

        'get server side instance of TriniDATClientConnectionManagerHTTP
        http_parser = Me.Sender.ServerTypeCreator.Invoke("TriniDATClientConnectionManagerHTTP", {Nothing, Nothing, Nothing})

        'fetch JClass for extraction of server information.
        sender_mp = Me.Sender.getProcessDescriptor().getParent()

        If IsNothing(http_parser) Or IsNothing(sender_mp) Then
            Return Nothing
        End If


        'create request
        packable_object = JSONSmallObject.createFrom(Me)
        request_str = packable_object.getPackedHTTPString(packaging_method, sender_mp)
        response_info = packable_object.Send(Nothing, request_str, auto_disconnect)

        If response_info.success And response_info.haveResponse Then
            'parse response
            response_info.http_parsed_response = http_parser.getResponseBody(response_info.response_buffer)
            Return response_info
        End If

        Return Nothing

    End Function

    Public ReadOnly Property haveDirective As Boolean
        Get
            If Not IsNothing(Me.Directive) Then
                Return Me.Directive.Length > 0
            Else
                Return False
            End If

        End Get
    End Property
    Public Function getUniqueObjectId() As Long

        Dim resp As JSONSendResult

        resp = Me.doObjectRequest(New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.REQUEST_CREATEOBJECT), True)
        If resp.is_parsed Then
            If IsNumeric(resp.http_parsed_response) Then
                Return CLng(resp.http_parsed_response)
            End If
        End If

        Return 0
    End Function
    Private Sub MutateSelf(ByVal json_dserialized_object As Object)

        For Each jobj In json_dserialized_object
            Dim prop As Newtonsoft.Json.Linq.JProperty
            prop = DirectCast(jobj, Newtonsoft.Json.Linq.JProperty)

            If prop.Name = "Directive" Then
                Me.Directive = prop.Value.ToString
            End If

            If prop.Name = "ObjectAttachment" Then
                Me.Attachment = prop.Value.tostring
            End If

            If prop.Name = "ObjectType" Then
                Me.ObjectType = prop.Value.ToString
            End If

            If prop.Name = "PaymentID" Then
                Me.PaymentHandle = prop.Value.ToString
            End If

        Next

    End Sub

    Public Function debugObject() As Boolean
        'set ID to -6
        'post object as JSON
        Dim resp As JSONSendResult

        If Not Me.haveSender Then
            Throw New Exception("Cannot debug object. Object's Sender property is empty.")
            Return False
        End If

        Try

            'send object
            resp = Me.doObjectRequest(New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.REQUEST_DEBUG_OBJECT), False)

            'parse new self version
            If resp.is_parsed Then
                'response = new object
                If resp.socket.canWrite Then
                    'modify self
                    Dim new_object As Object
                    Try
                        new_object = JsonConvert.DeserializeObject(resp.http_parsed_response)
                    Catch ex As Exception
                        Return False
                    End Try

                    Call Me.MutateSelf(new_object)

                    'reply ok message
                    If resp.haveSocketStream Then
                        Dim response_msg As String
                        Dim resp_bytes() As Byte
                        response_msg = "OK" & vbCrLf
                        resp_bytes = Text.Encoding.ASCII.GetBytes(response_msg)

                        Try
                            resp.socket_stream.Write(resp_bytes, 0, resp_bytes.Length)
                            resp.socket.forceDisconnect()

                        Catch ex As Exception

                        End Try

                        Return True
                    End If
                End If

                Return False
            End If

        Catch ex As Exception
            Dim msg As String
            msg = ex.Message

            msg = msg


        End Try

        Return False

    End Function
    Public ReadOnly Property ObjectTypeName() As String
        Get
            Return ObjectType
        End Get
    End Property

End Class


