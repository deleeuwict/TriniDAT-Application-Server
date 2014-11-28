Option Explicit On
Option Compare Text
Imports Newtonsoft
Imports TriniDATServerTypes
Imports TriniDATDictionaries
Imports System.Web
Imports TriniDATSockets
Imports System.Net.Sockets

Public Class JSONSmallObject

    Private Const DEFAULT_PACKET_SIZE As Integer = 2 * 1024

    ' Public parent As JSONObject
    Public Directive As String
    Public ObjectType As String
    Public PaymentID As String
    Public ObjectAttachment As Object
    Protected object_serverhost As String
    Protected object_serverport As Integer
    Public Property ObjectServer As String
        Get
            Return Me.object_serverhost
        End Get
        Set(ByVal value As String)
            Me.object_serverhost = value
        End Set
    End Property
    Public ReadOnly Property haveObjectServer As Boolean
        Get
            If Not IsNothing(Me.object_serverhost) Then
                Return Len(Me.object_serverhost) > 0
            Else
                Return False
            End If
        End Get
    End Property
    Public Property ObjectServerPort As Integer
        Get
            Return Me.object_serverport
        End Get
        Set(ByVal value As Integer)
            Me.object_serverport = value
        End Set
    End Property
    Public ReadOnly Property haveObjectServerPort As Boolean
        Get
            Return Me.ObjectServerPort > 0
        End Get
    End Property
    Public Sub New(ByVal _trinidatserver As String, ByVal _trinidatport As Integer, ByVal _payment_handle As String)
        Me.ObjectServer = _trinidatserver
        Me.ObjectServerPort = _trinidatport
        Me.PaymentHandle = _payment_handle
    End Sub

    Public Sub New(ByVal _original_objectparent As JSONObject)
        Dim sender_mp As Object '= mappingpointroot

        If Not IsNothing(_original_objectparent) Then
            If _original_objectparent.haveSender Then
                sender_mp = _original_objectparent.Sender.getProcessDescriptor().getparent()

                'copy properties from original JSON object.
                Me.ObjectServer = _original_objectparent.Sender.ObjectServer
                Me.ObjectServerPort = _original_objectparent.Sender.ObjectServerPort
                Me.PaymentHandle = _original_objectparent.PaymentHandle
            End If
        End If

    End Sub
    Public ReadOnly Property haveCompleteServerData As Boolean
        Get
            Return Me.haveObjectServer And Me.haveObjectServerPort
        End Get
    End Property
    Public Property PaymentHandle As String
        Get
            Return Me.paymentID
        End Get
        Set(ByVal value As String)
            Me.paymentID = value
        End Set
    End Property

    Public Shared Function createFrom(ByVal obj As JSONObject) As JSONSmallObject
        Dim retval As JSONSmallObject
        retval = New JSONSmallObject(obj)
        '  retval.parent = obj
        retval.Directive = obj.Directive
        retval.ObjectAttachment = obj.Attachment
        retval.ObjectType = obj.ObjectType
        Return retval
    End Function


    Public ReadOnly Property havePaymentHandle As Boolean
        Get
            If Not IsNothing(Me.paymentID) Then
                Return Me.paymentID.Length > 0
            Else
                Return False
            End If
        End Get
    End Property

    Public Function getPackedHTTPString(ByVal packaging_method As JTRANSPORTABLE_METHOD, ByVal source_mapping_point As Object) As String
        ' source_mapping_point As Object '= mappingpointroot
        Dim request_str As String

        If IsNothing(source_mapping_point) Then
            Return "err"
        End If

        Try

            request_str = "TRINIDATOBJECT " & packaging_method.MethodString & vbCrLf
            If source_mapping_point.info.app.haveApplicationAuthor Then
                request_str &= "X-TriniDAT-Author-Name: " & HttpUtility.UrlEncode(source_mapping_point.info.app.ApplicationAuthor.ToString) & vbCrLf
            End If

            If source_mapping_point.info.app.haveApplicationAuthorContactEmail Then
                request_str &= "X-TriniDAT-Author-Contact-Mail: " & HttpUtility.UrlEncode(source_mapping_point.info.app.ApplicationAuthorContactEmail.ToString) & vbCrLf
            End If

            If source_mapping_point.info.app.haveApplicationAuthorContactWebsite() Then
                request_str &= "X-TriniDAT-Author-Contact-WWW: " & HttpUtility.UrlEncode(source_mapping_point.info.app.ApplicationAuthorContactWebsite.ToString) & vbCrLf
            End If


            request_str &= "X-TriniDAT-Server-Host: " & Me.ObjectServer & vbCrLf
            request_str &= "X-TriniDAT-Server-Port: " & Me.ObjectServerPort.ToString & vbCrLf

            If Me.havePaymentHandle Then
                request_str &= "X-TriniDAT-Payment: " & Me.PaymentHandle & vbCrLf
            End If

            request_str &= "X-TriniDAT-Server-Timestamp: " & Now.ToString("MMMM dd yyyy H:mm:ss") & vbCrLf
            request_str &= "X-TriniDAT-Application-Id: " & source_mapping_point.applicationid.ToString & vbCrLf
            request_str &= "X-TriniDAT-Application-Name: " & HttpUtility.UrlEncode(source_mapping_point.info.app.applicationname.ToString) & vbCrLf
            request_str &= "X-TriniDAT-Server-Session: " & source_mapping_point.Info.direct_session.Id.ToString & vbCrLf
            request_str &= "X-TriniDAT-MappingPoint-URL: " & source_mapping_point.info.FullMappingPointPath & vbCrLf

            If packaging_method.hasPayload Then
                Dim transport_serialized As String


                Try
                    transport_serialized = Json.JsonConvert.SerializeObject(Me)
                    'fill object header
                    request_str &= "X-TriniDATObject-State: " & "Attachment" & vbCrLf
                    request_str &= "X-TriniDATObject-StateDescription: Serialized." & vbCrLf
                    request_str &= "X-TriniDATObject: " & Me.GetType.ToString & vbCrLf
                    request_str &= "Content-Length: " & transport_serialized.Length.ToString & vbCrLf
                    'add object as plain payload
                    request_str &= vbCrLf & transport_serialized
                Catch ex As Exception
                    'attach error information only.
                    request_str &= "X-TriniDATObject-State: " & "Error." & vbCrLf
                    request_str &= "X-TriniDATObject-StateDescription: " & HttpUtility.UrlEncode(ex.Message) & vbCrLf
                End Try
            End If

            request_str &= vbCrLf

            Return request_str
        Catch ex As Exception

            Return "ERR:" & ex.Message
        End Try

    End Function

    Public Function getPackedHTTPString(ByVal packaging_method As JTRANSPORTABLE_METHOD) As String
        'this version can not send metadata

        Dim request_str As String

        Try

            request_str = "TRINIDATOBJECT " & packaging_method.MethodString & vbCrLf
            request_str &= "X-TriniDAT-Server-Host: " & Me.ObjectServer & vbCrLf
            request_str &= "X-TriniDAT-Server-Port: " & Me.ObjectServerPort.ToString & vbCrLf

            If Me.havePaymentHandle Then
                request_str &= "X-TriniDAT-Payment: " & Me.PaymentHandle & vbCrLf
            End If

            If packaging_method.hasPayload Then
                Dim transport_serialized As String

                Try
                    transport_serialized = Json.JsonConvert.SerializeObject(Me)
                    'fill object header
                    request_str &= "X-TriniDATObject-State: " & "Attachment" & vbCrLf
                    request_str &= "X-TriniDATObject-StateDescription: Serialized." & vbCrLf
                    request_str &= "X-TriniDATObject: " & Me.GetType.ToString & vbCrLf
                    request_str &= "Content-Length: " & transport_serialized.Length.ToString & vbCrLf
                    'add object as plain payload
                    request_str &= vbCrLf & transport_serialized
                Catch ex As Exception
                    'attach error information only.
                    request_str &= "X-TriniDATObject-State: " & "Error." & vbCrLf
                    request_str &= "X-TriniDATObject-StateDescription: " & HttpUtility.UrlEncode(ex.Message) & vbCrLf
                End Try
            End If

            request_str &= vbCrLf

            Return request_str
        Catch ex As Exception

            Return "ERR:" & ex.Message
        End Try

    End Function

    Public Function Send(ByVal existing_connection As TriniDATTCPSocket, ByVal requestbuffer As String, ByVal auto_disconnect As Boolean) As JSONSendResult

        Dim retval As JSONSendResult

        retval = New JSONSendResult

        'post to server associated with JClass a.ka. this object's owner.
        retval.socket = New TriniDATTCPSocket(New TcpClient())
        retval.success = False

        Try
            If IsNothing(existing_connection) Then
                retval.socket.getSocket().Connect(Me.ObjectServer, Me.ObjectServerPort)
            Else
                retval.socket = existing_connection
            End If


            If retval.socket.isConnected() Then
                retval.socket_stream = retval.socket.GetStream

                If retval.socket.canWrite Then
                    Dim bytes_out() As Byte
                    Dim bytes_in(JSONSmallObject.DEFAULT_PACKET_SIZE) As Byte
                    Dim bytesread As Long

                    bytes_out = Text.Encoding.ASCII.GetBytes(requestbuffer)
                    retval.socket_stream.Write(bytes_out, 0, bytes_out.Length)

                    retval.response_buffer = ""

                    Do Until Not retval.socket_stream.CanRead Or Not retval.socket.isConnected()
                        Threading.Thread.Sleep(100)
                        bytesread = retval.socket_stream.Read(bytes_in, 0, JSONSmallObject.DEFAULT_PACKET_SIZE)

                        'blocking socket either data available or err.

                        If bytesread > 0 Then
                            retval.response_buffer &= Text.Encoding.ASCII.GetString(bytes_in)
                        End If

                        Exit Do
                    Loop
                End If
            End If

            retval.success = retval.haveResponse

        Catch ex As Exception
            retval.ErrorMessage = ex.Message
        End Try

        If auto_disconnect Then
            If retval.socket.isConnected() Then
                Try

                    retval.socket.forceDisconnect()
                Catch ex As Exception

                End Try
            End If
        End If

        Return retval

    End Function
End Class



