Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Reflection
Imports System.Collections.Specialized
Imports System
Imports System.Runtime.CompilerServices
Imports System.Net.NetworkInformation
Imports System.Net.NetworkInformation.IPAddressInformation

<Assembly: SuppressIldasmAttribute()> 

Public Class TriniDATTCPSocket


    Private _socket As TcpClient
    Private _accept_ts As DateTime
    Private _disconnect_socket As Boolean
    Private _disposed As Boolean

    'taggin' 
    Private _tagfirstPacket As Boolean
    Private _tagIsGetRequest As Boolean
    Private _tagIsPostRequest As Boolean
    Private _tagIsHTTPstream As Boolean
    Private _tagRequestURL As String
    Private _tagParameters As StringDictionary

    'optional - used for tracking streams
    Private recv_total_content_length As Long
    Private recv_bytes_left As Long

    Public Function getSocket() As TcpClient
        Return Me._socket
    End Function

    Public Property Receive_Bytes_Left As Long
        Get
            Return Me.recv_bytes_left
        End Get
        Set(ByVal value As Long)
            Me.recv_bytes_left = value
        End Set
    End Property
    Public Property Receive_Content_Length As Long
        Get
            Return Me.recv_total_content_length
        End Get
        Set(ByVal value As Long)
            Me.recv_total_content_length = value
        End Set
    End Property

    Public Sub tagIsGetRequest(ByVal value As Boolean)
        _tagIsGetRequest = value
        If IsHTTP() And value Then tagIsPostRequest(False)

        If (value) Then tagIsHTTP(True)
    End Sub
    Public Sub tagOriginalURL(ByVal value As String)
        _tagRequestURL = value
    End Sub
    Public Sub tagParameters(ByVal value As StringDictionary)
        _tagParameters = value
    End Sub
    Public Sub tagIsPostRequest(ByVal value As Boolean)
        _tagIsPostRequest = value
        If IsHTTP() And value Then tagIsGetRequest(False)
        If (value) Then tagIsHTTP(True)
    End Sub
    Public Sub tagIsHTTP(ByVal value As Boolean)
        _tagIsHTTPstream = value
        If Not value Then
            tagIsGetRequest(False)
            tagIsPostRequest(False)
        End If
    End Sub
    Public Function haveParameters() As Boolean
        Return (_tagParameters.Count > 0)
    End Function
    Public Function Parameters() As StringDictionary
        If haveParameters() Then
            Return _tagParameters
        Else
            Return Nothing
        End If
    End Function
    Public Function haveOriginalURL() As Boolean
        Return Not IsNothing(_tagRequestURL)
    End Function
    Public Function originalURL() As String
        Return _tagRequestURL
    End Function
    Public Function IsGetRequest() As Boolean
        Return _tagIsGetRequest
    End Function
    Public Function IsPostRequest() As Boolean
        Return _tagIsPostRequest
    End Function
    Public Function IsHTTP() As Boolean
        Return _tagIsHTTPstream
    End Function
    Public Sub Dispose(ByVal disposing As Boolean)

        If Not _disposed Then
            'TODO: this is a good time to delete any JClass.
        End If
    End Sub

    Public Sub New(ByVal socket As TcpClient)
        _socket = socket
        _tagRequestURL = Nothing
        _tagParameters = New StringDictionary 'system-wide policy: No parameters = empy string dictionary.
        'disable buffering
        socket.NoDelay = False
        Call setAcceptTime(Now())
    End Sub
    Public ReadOnly Property haveSocket() As Boolean
        Get
            Return Not IsNothing(Me._socket)
        End Get
    End Property
    Public ReadOnly Property RemoteIP As String
        Get
            If Not Me.haveSocket() Then Return Nothing
            Dim ip As String

            If Not Me.isConnected() Then Return Nothing

            ip = Me.getSocket().Client.RemoteEndPoint.ToString()
            ip = Mid(ip, 1, InStr(ip, ":") - 1)

            Return ip
        End Get
    End Property
    Public ReadOnly Property isLocalHostClient As Boolean
        Get
            Return CInt(Left(Me.RemoteIP, 3)) = 127
        End Get
    End Property
    Public ReadOnly Property isLocalInterfaceIP As Boolean
        Get
            ' Obtain the first address of local machine with addressing scheme
            For Each IP As IPAddress In Dns.GetHostEntry(Dns.GetHostName()).AddressList
                If IP.ToString = Me.RemoteIP Then
                    Return True
                End If
            Next IP

            Return False
        End Get
    End Property
    Public ReadOnly Property isIntranetClient As Boolean
        Get

            If Me.isLocalHostClient Then Return True

            Dim domain As Integer
            Dim pos As Integer
            Dim ip As String
            ip = Me.RemoteIP

            pos = InStr(ip, ".")
            If pos = 0 Then Return False

            domain = CInt(Mid(ip, 1, pos - 1))

            Return (domain = 10 Or domain = 172 Or domain = 192)
        End Get
    End Property

    Public ReadOnly Property isLocalOrIntranetClient As Boolean
        Get

            If Me.isLocalHostClient Then Return True
            If Me.isIntranetClient Then Return True

            Return False
        End Get
    End Property
    Public ReadOnly Property isInternetClient As Boolean
        Get

            Return Not Me.isLocalOrIntranetClient

        End Get
    End Property
    Public Sub setAcceptTime(ByVal val As DateTime)
        Me._accept_ts = val
    End Sub

    Public Function disconnectSocket() As Boolean
        Return _disconnect_socket
    End Function

    Public Function getAccepttime() As DateTime
        Return Me._accept_ts
    End Function
    Public Sub forceDisconnect()
        Try

            ''        If _socket.Connected() Then
            'set disconnect flag
            Me.DisconnectNow()
            _socket.Close()
            ''    End If

        Catch ex As Exception

            MsgBox("Error whiel closing socket: " & ex.Message & " @ " & ex.StackTrace.ToString)
        End Try

    End Sub

    Public Sub MSG(ByVal txt As String)
        Debug.Print("BlingConnectionState: " & txt)
    End Sub
    Public Sub DisconnectNow()
        'set flag 
        Me._disconnect_socket = True
    End Sub


    Public Sub setReceiveBufferSize(ByVal val As Integer)
        _socket.ReceiveBufferSize = val
    End Sub
    Public Function getReceiveBufferSize() As Integer
        Return _socket.ReceiveBufferSize
    End Function
    Public Function isConnected() As Boolean
        Return _socket.Connected()
    End Function
    Public Function canReceive() As Boolean
        Return (_socket.Connected)
    End Function
    Public Function canWrite() As Boolean
        Return (Me.isConnected())
    End Function

    Public Function isFirstPacket() As Boolean
        Return _tagfirstPacket
    End Function

    Public Function GetStream() As NetworkStream
        Return _socket.GetStream()
    End Function

    Public Sub tagIsFirstPacket(ByVal value As Boolean)
        _tagfirstPacket = value
    End Sub
    'Public Sub linkProcessInstance(ByVal mp_index_process_index As Integer)
    '    _mp_index_process_index = mp_index_process_index
    'End Sub
    'Public Function hasProcessInstance() As Boolean
    '    Return (_mp_index_process_index > -1)
    'End Function

    'Public Function getProcessInstance() As mappingPointInstanceInfo
    '    Try

    '        If isLinkedToMapping() And hasProcessInstance() Then
    '            Return getLinkedMappingInstance().getInstanceInfo(_mp_index_process_index)
    '        Else
    '            Err.Raise(100, 0, "getProcessInstance: Not linked.")
    '        End If

    '    Catch ex As Exception
    '        MsgBox("getProcessInstance exception: " & ex.Message & " at " & ex.TargetSite.ToString)
    '    End Try

    '    Return Nothing
    'End Function


    'Public Sub linkToMapping(ByRef mp_index As Integer)
    '    _mp_index = mp_index
    'End Sub
    'Public Function isLinkedToMapping() As Boolean
    '    Return (_mp_index > -1)
    'End Function

    'Public Function getLinkedMappingInstance() As mappingPointRoot
    '    If isLinkedToMapping() Then
    '        Return ServerMappingPointIO.Global_Instance.getByIndex(Me._mp_index)
    '    Else
    '        Return Nothing
    '    End If

    'End Function

    'Public Function getLinkedMappingIndex() As Integer
    '    If isLinkedToMapping() Then
    '        Return Me._mp_index
    '    Else
    '        Return -1
    '    End If

    'End Function
End Class