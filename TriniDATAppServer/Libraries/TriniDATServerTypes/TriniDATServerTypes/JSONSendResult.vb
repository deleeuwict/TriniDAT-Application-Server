Imports TriniDATSockets
Imports System.IO
Imports System.Net.Sockets

Public Class JSONSendResult

    Public success As Boolean
    Public response_buffer As String
    Public http_parsed_response As String
    Public socket As TriniDATTCPSocket
    Public socket_stream As networkStream
    Public err_msg As String

    Public Sub New()
        Me.socket = Nothing
        Me.response_buffer = Nothing
        Me.success = False
        Me.socket_stream = Nothing
        Me.err_msg = Nothing
    End Sub
    Public ReadOnly Property is_parsed As Boolean
        Get
            If Not IsNothing(Me.http_parsed_response) Then
                Return (Me.http_parsed_response.Length > 0)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveSocketStream() As Boolean
        Get
            Return Not IsNothing(Me.socket_stream)
        End Get
    End Property
    Public Property ErrorMessage As String
        Get
            Return Me.err_msg
        End Get
        Set(ByVal value As String)
            Me.err_msg = value
        End Set
    End Property
    Public ReadOnly Property haveErrorMessage As Boolean
        Get
            Return Not IsNothing(Me.ErrorMessage)
        End Get
    End Property
    Public ReadOnly Property haveResponse As Boolean
        Get
            If Not IsNothing(Me.response_buffer) Then
                Return Len(Me.response_buffer) > 0
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveSocket As Boolean
        Get
            Return Not IsNothing(socket)
        End Get
    End Property
End Class
