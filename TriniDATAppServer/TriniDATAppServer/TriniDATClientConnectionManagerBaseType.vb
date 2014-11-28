Imports System.Reflection
Imports System.Text
Imports System.Collections.Specialized
Imports System.Net.Sockets
Imports TriniDATSockets

Public MustInherit Class TriniDATClientConnectionManagerBaseType
    Protected _connie As TriniDATSockets.TriniDATTCPSocket
    Public Delegate Sub InvalidPacketErrorHandlerTemplate(ByVal rawPacket As String, ByVal Reason As String)

    Private _errorHandlerFunc As InvalidPacketErrorHandlerTemplate
    Public MustOverride Function OnPacketReceived(ByVal current_jservice As Object, ByVal info As TriniDATRequestInfo, ByVal packet As Byte(), ByVal packetSize As Long) As Boolean


    Public Sub New(ByRef connie As TriniDATSockets.TriniDATTCPSocket)
        _errorHandlerFunc = Nothing
        Call Me.setConnection(connie)
    End Sub
    Public Sub setErrorHandler(ByRef e As InvalidPacketErrorHandlerTemplate)
        Me._errorHandlerFunc = e
    End Sub
    Public Function errorHandlerPresent() As Boolean
        Return Not IsNothing(_errorHandlerFunc)
    End Function
    Protected Sub OnInvalidPacket(ByVal rawPacket As String, ByVal Reason As String)
        If errorHandlerPresent() Then
            _errorHandlerFunc.Invoke(rawPacket, Reason)
        End If
    End Sub
    Public Function getConnection() As TriniDATSockets.TriniDATTCPSocket
        Return Me._connie
    End Function
    Public Sub setConnection(ByRef connie As TriniDATSockets.TriniDATTCPSocket)
        Me._connie = connie
    End Sub
    Public Function getName() As String
        Dim protocol_class As String

        'return the protocol name which is derived from class name.s
        protocol_class = Replace(Me.GetType().ToString(), Assembly.GetExecutingAssembly().GetName().Name & ".", "")
        protocol_class = Replace(protocol_class, "ProtocolTranslator", "")
        Return protocol_class
    End Function

    Public Overridable Function writeRaw(ByVal output_buffer As StringCollection) As Boolean
        Dim line As String
        Dim output As NetworkStream

        Try

            output = Me.getConnection().GetStream()

            'write user data
            For Each line In output_buffer
                output.Write(Encoding.ASCII.GetBytes(line), 0, Encoding.ASCII.GetByteCount(line))
            Next

            Return True

        Catch ex As Exception
            MSG("writeRaw(stringcollection) error: " & ex.Message)
        End Try

        Return False

    End Function

    Public Shared Sub MSG(ByVal TXT As String)
        'Log to main form in a different thread.
        Debug.Print(TXT)
    End Sub

End Class
