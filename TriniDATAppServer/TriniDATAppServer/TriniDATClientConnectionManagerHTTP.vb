Option Explicit On
Option Compare Text

Imports System.Text
Imports System.Collections.Specialized
Imports System.Net.Sockets
Imports TriniDATServerTypes
Imports TriniDATSockets
Imports TriniDATDictionaries

''CLEANUP:
''        
''        

''
Public Class TriniDATClientConnectionManagerHTTP
    Inherits TriniDATClientConnectionManagerBaseType

    Private encoding As System.Text.Encoding

    Private output_content_type As String
    Private output_response_code As Integer
    Private output_302_url As String


    Public Shared counter As Integer = 0

    Public listenPOST As Boolean
    Public listenGET As Boolean

    Protected Const defaultHTTPResponse As String = "$HTTPCODESTR" & vbCrLf & "Server: Trinidad App Server c 2013 De Leeuw ICT dot NL" & vbCrLf & "Content-Type: $MIMETYPE" & vbCrLf & "Content-Length: $CONTENTLENGTH" & vbCrLf & "Connection: $CONNECTIONSTATE" & vbCrLf & "Cache-Control: $CACHE" & vbCrLf & "$COOKIELINE" & vbCrLf & "$LAST" & vbCrLf & "$CONTENTDATA"
    Protected output_buffer As StringCollection 'for buffered write calls.
    Protected parameters As StringDictionary

    'PARAMETERS CONFIGURED BY JCLASSES.
    Private _IsBinaryStream As Boolean

    Private parent As mappingPointInstanceInfo
    Private firstProcess As Boolean
    Private io_event_table As TriniDATHTTP_EventTable
    Private http_session As TrinidatUserSession
    Private http_request_headers As StringDictionary
    Private http_cookies As StringDictionary
    Private buffer_flushed As Boolean
    Private http_master As TriniDATClientConnectionManagerHTTP
    Private user_output_headers As StringDictionary

    Public Property Session As TrinidatUserSession
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.Session
            End If

            'master socket
            Return Me.http_session
        End Get
        Set(ByVal value As TriniDATUserSession)

            If Me.haveMasterSocket Then
                'slave socket
                Me.MasterSocket.Session = value
                Exit Property
            End If

            'master socket
            Me.http_session = value
        End Set
    End Property
    Public Property Flushed As Boolean
        Get
            Return Me.buffer_flushed
        End Get
        Set(ByVal value As Boolean)
            Me.buffer_flushed = value
        End Set
    End Property
    Public Property OutputHeaders As StringDictionary
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.OutputHeaders
                Exit Property
            End If

            Return Me.user_output_headers
        End Get
        Set(ByVal value As StringDictionary)
            Me.user_output_headers = value          
        End Set
    End Property
    Public Property Headers As StringDictionary
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.Headers
                Exit Property
            End If

            Return Me.http_request_headers
        End Get
        Set(ByVal value As StringDictionary)
            Me.http_request_headers = value
            If Not IsNothing(value) Then
                Me.Cookies = Me.parseCookies()
            End If
        End Set
    End Property
    Public Sub StreamNullHandler(ByVal buffer As String)

    End Sub
    'Receive * interfaces socket object  stats for easy access.

    'regulates the instance that can modify underlying socket's receive properties

    Public Property Receive_Bytes_Left As Long
        Get
            Return Me.getConnection().Receive_Bytes_Left
        End Get
        Set(ByVal value As Long)
            Me.getConnection().Receive_Bytes_Left = value
        End Set
    End Property
    Public Property Receive_Content_Length As Long
        Get
            Return Me.getConnection().Receive_Content_Length
        End Get
        Set(ByVal value As Long)
            Me.getConnection().Receive_Content_Length = value
        End Set
    End Property
    Public Function supportsNetworkStream() As Boolean
        If Not IsNothing(Me.getEventTable()) Then
            Return Me.getEventTable().haveOnIncomingStreamDataHandler()
        Else
            Return False
        End If
    End Function
    Public ReadOnly Property haveConnection() As Boolean
        Get
            Return Not IsNothing(Me.getConnection())
        End Get
    End Property
    Public ReadOnly Property getStreamingHandler() As TriniDATHTTP_EventTable.OnIncomingStreamingData
        Get
            Dim target_stream_del As TriniDATHTTP_EventTable.OnIncomingStreamingData

            target_stream_del = Me.getEventTable().event_onstream

            If Me.getEventTable().haveOnIncomingInternetStreamDataHandler Or Me.getEventTable().haveOnIncomingIntranetStreamDataHandler Or Me.getEventTable().haveOnIncomingLocalhostStreamDataHandler Then

                If Me.getEventTable().haveOnIncomingInternetStreamDataHandler And CType(Me.getConnection().getConnection(), TriniDATTCPSocket).isInternetClient Then
                    target_stream_del = Me.getEventTable().event_onstream_internet
                ElseIf Me.getEventTable().haveOnIncomingIntranetStreamDataHandler And CType(Me.getConnection().getConnection(), TriniDATTCPSocket).isIntranetClient Then
                    target_stream_del = Me.getEventTable().event_onstream_intranet
                ElseIf Me.getEventTable().haveOnIncomingLocalhostStreamDataHandler And CType(Me.getConnection().getConnection(), TriniDATTCPSocket).isLocalHostClient Then
                    target_stream_del = Me.getEventTable().event_onstream_localhost
                End If
            End If

            Return target_stream_del
        End Get
    End Property

    Public Function getEncoding() As Encoding
        If Me.haveMasterSocket Then
            Return Me.MasterSocket.getEncoding
        End If

        Return Me.encoding
    End Function
    Public Sub setEncoding(ByVal val As Encoding)
        Me.encoding = val
    End Sub

    Public Function getParent() As mappingPointInstanceInfo
        Return Me.parent
    End Function
    Public Sub setparent(ByVal val As mappingPointInstanceInfo)
        Me.parent = val
    End Sub
    Public Function IsBinaryStream() As Boolean
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.IsBinaryStream
            Exit Function
        End If

        Return _IsBinaryStream

    End Function

    Public Sub setIsBinaryStream(ByVal value As Boolean)
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.setIsBinaryStream(value)
            Exit Sub
        End If

        _IsBinaryStream = value
    End Sub
    Public Sub setOutput(ByVal val As String)

        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.setOutput(val)
            Exit Sub
        End If

        'master socket
        Me.deleteOutputBuffer()
        Me.output_buffer.Add(val)
    End Sub
    Public Sub setOutputMime(ByVal val As String)
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.setOutputMime(val)
            Exit Sub
        End If

        'master socket
        Me.output_content_type = val
    End Sub
    Public Function getOutputContent() As String
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getOutputContent()
        End If

        'master socket
        Return Me.output_content_type
    End Function
    Public ReadOnly Property haveMasterSocket As Boolean
        Get
            Return Not IsNothing(Me.http_master)
        End Get
    End Property
    Public Property MasterSocket As TriniDATClientConnectionManagerHTTP
        Get
            Return Me.http_master
        End Get
        Set(ByVal value As TriniDATClientConnectionManagerHTTP)
            'all slave sockets proxy their operators to the master socket.
            Me.http_master = value
        End Set
    End Property
    Public Sub New(ByRef con As Object, Optional ByVal _encoding As Encoding = Nothing, Optional ByVal _sess As TriniDATUserSession = Nothing)
        MyBase.new(Nothing)

        'con 
        'mastersocket: TriniDATClientConnectionManagerHTTP
        'low-level socket: TriniDATTCPSocket

        If TypeOf con Is TriniDATTCPSocket Then
            MyBase.setConnection(con)
            Me.MasterSocket = Nothing
        ElseIf TypeOf con Is TriniDATClientConnectionManagerHTTP Then
            Me.MasterSocket = con
        End If

        Me.output_buffer = New StringCollection
        Me.user_output_headers = New StringDictionary

        If Not Me.haveMasterSocket Then
            'master constructor
            Me.Headers = Nothing
            Me.Session = _sess

            Call Me.resetParameters()

            'set application defaults.
            Me.setOutputMime("text/html; charset=$ENCODINGNAME")
            Me.setHTTPResponse(200)

            'encoding options
            If IsNothing(_encoding) Then
                _encoding = New ASCIIEncoding
            End If

            Me.setEncoding(_encoding)
        End If

    End Sub
    Public ReadOnly Property haveCookies() As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveCookies
            End If

            Return Not IsNothing(Me.Cookies)
        End Get
    End Property
    Public ReadOnly Property haveOutputHeaders() As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveOutputHeaders()
            End If

            'master socket

            If Not IsNothing(Me.OutputHeaders) Then
                Return Me.OutputHeaders.Count > 0
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveHeaders() As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveHeaders()
            End If

            'master socket
            Return Not IsNothing(Me.Headers)
        End Get
    End Property
    Public ReadOnly Property HaveActiveSession() As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.HaveActiveSession
            End If

            Return Not IsNothing(Me.Session)
        End Get
    End Property

    Public ReadOnly Property HaveSessionCookie() As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.HaveSessionCookie
            End If


            If Not Me.haveCookies() Then
                Return False
            End If


            Return Me.Cookies.ContainsKey(GlobalSetting.SESSION_COOKIE_NAME)

        End Get
    End Property
    Public ReadOnly Property SessionCookie() As String
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.SessionCookie
            End If

            If Not Me.haveCookies() Then Return ""

            Return Me.Cookies(GlobalSetting.SESSION_COOKIE_NAME)

        End Get
    End Property
    Public Property Cookies As StringDictionary
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.Cookies
            End If

            Return Me.http_cookies
        End Get
        Set(ByVal value As StringDictionary)
            Me.http_cookies = value

        End Set
    End Property
    Public Shadows Function getConnection() As Object

        If Not Me.haveMasterSocket Then
            'return low-level tcp socket
            Return Me._connie
        Else
            'access denied
            Return Nothing
        End If
    End Function

    Public Function parseCookies() As StringDictionary

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.parseCookies
        End If

        'master socket

        If Not Me.haveCookieHeader Then Return Nothing

        Dim cookie_str As String
        Dim cookies_str() As String

        cookie_str = Me.getHeader("Cookie") & ";"

        cookies_str = Split(cookie_str, ";")
        Return parseNestedKeyValPair(cookies_str, "=", True)

    End Function

    Public ReadOnly Property haveCookieHeader() As Boolean
        Get
            Return Me.haveHeader("Cookie")
        End Get
    End Property
    Public ReadOnly Property haveOutputHeader(ByVal val As String) As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveOutputHeader(val)
            End If

            'master socket
            If Not Me.haveOutputHeaders() Then Return False

            Return Me.OutputHeaders.ContainsKey(val)
        End Get
    End Property
    Public ReadOnly Property haveHeader(ByVal val As String) As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveHeader(val)
            End If

            'master socket
            If Not Me.haveHeaders() Then Return False

            Return Me.Headers.ContainsKey(val)
        End Get
    End Property
    Public ReadOnly Property getHeader(ByVal val As String) As String
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.getHeader(val)
            End If

            'master socket
            If Not Me.haveHeader(val) Then Return ""

            If Not Me.Headers.ContainsKey(val) Then Return ""

            Return Me.Headers(val)

        End Get
    End Property
    Public Function sendRedirect(ByVal url As String) As Boolean
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.sendRedirect(url)
        End If

        Me.setHTTPResponse(302)
        Me.setHTTP302URL(url)
        Return Me.writeRaw(True, "302 Found", True)
    End Function

    Public Sub setHTTP302URL(ByVal val As String)
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.setHTTP302URL(val)
            Exit Sub
        End If

        'master socket
        Me.output_302_url = val
    End Sub
    Public Function getHTTP302URL() As String
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getHTTP302URL()
        End If

        'master socket

        Return Me.output_302_url
    End Function

    Public Sub setHTTPResponse(ByVal val As Integer)
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.setHTTPResponse(val)
            Exit Sub
        End If

        'master socket
        Me.output_response_code = val
    End Sub
    Public Function getHTTPResponse() As Integer
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getHTTPResponse
        End If

        'master socket

        Return Me.output_response_code
    End Function

    Public Function getHTTPResponseStr() As String
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getHTTPResponseStr
        End If

        'master socket
        Dim code As Integer
        Dim template As String
        Dim msg As String
        Dim retval As String

        template = "HTTP/1.1 $CODE $MSG"

        code = Me.getHTTPResponse()

        If code = 200 Then
            msg = "OK"
        ElseIf code = 302 Then
            msg = "Found"
        ElseIf code = 400 Then
            msg = "Bad Request"
        ElseIf code = 401 Then
            msg = "Unauthorized"
        ElseIf code = 404 Then
            msg = "Not Found"
        ElseIf code = 429 Then
            msg = "Too Many Requests"
        ElseIf code = 500 Then
            msg = "Internal Server Error"
        Else
            code = 418
            msg = "Simon says code teapot"
        End If

        retval = Replace(template, "$CODE", code.ToString)
        retval = Replace(retval, "$MSG", msg.ToString)

        Return retval
    End Function

    Public Sub deleteOutputBuffer()
        'flush both master/slave buffers.
        If Me.haveMasterSocket() Then
            Me.MasterSocket.Flush()
        End If

        Me.output_buffer.Clear()
    End Sub

    Public Function SessionDat() As StringDictionary
        If Me.HaveActiveSession Then
            Return Me.Session.UserVars
        Else
            Return Nothing
        End If
    End Function
    Public Function Flush(Optional ByVal serve_headers As Boolean = True, Optional ByVal disconnect_on_success As Boolean = True) As String
        'write all buffered data to connection and reset buffer.
        '<DERIVED CLASS MODIFIES PACKET HERE>'
        'returns what is written

        Dim retval As String

        If Me.haveMasterSocket() Then
            'slave mode
            retval = Me.MasterSocket.Flush(serve_headers, disconnect_on_success)
            Me.Flushed = Me.MasterSocket.Flushed
            If Me.Flushed Then
                Me.deleteOutputBuffer()
            End If
        Else
            'master socket mode

            If output_buffer.Count = 0 Then
                Me.output_buffer.Add("(No output data.)")
            End If

            retval = Me.writeRaw(serve_headers, output_buffer, disconnect_on_success)
            Me.Flushed = True
            Me.deleteOutputBuffer()
        End If

        Return retval
    End Function

    Public Function getOutputBuffer() As StringCollection
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getOutputBuffer()
        End If

        'master socket
        Return Me.output_buffer

    End Function

    Public ReadOnly Property getEventTable() As TriniDATHTTP_EventTable
        Get
            Return Me.io_event_table
        End Get
    End Property
    Public Function Configure(ByVal event_table As TriniDATHTTP_EventTable) As Boolean

        Me.io_event_table = event_table

        Return True

    End Function


    Public Function formatHTTPHeaders(ByVal content_length As Long, Optional ByVal connection_state As String = "close") As String

        Dim http_headers_formated As String
        'text/html
        http_headers_formated = TriniDATClientConnectionManagerHTTP.defaultHTTPResponse

        'replace constants
        If getHTTPResponse() = 302 Then
            http_headers_formated = Replace(http_headers_formated, "$HTTPCODESTR", Me.getHTTPResponseStr() & vbCrLf & "Location: " & Me.getHTTP302URL() & vbCrLf)
            'remove body placeholder.
            http_headers_formated = Replace(http_headers_formated, vbCrLf & vbCrLf & "$CONTENTDATA", "")
        Else
            http_headers_formated = Replace(http_headers_formated, "$HTTPCODESTR", Me.getHTTPResponseStr())
        End If

        If Me.haveOutputHeader("Cache-Control") Then
            http_headers_formated = Replace(http_headers_formated, "$CACHE", Me.OutputHeaders("Cache-Control"))
        Else
            'no-cache, must-revalidate
            http_headers_formated = Replace(http_headers_formated, "$CACHE", "no-cache, must-revalidate")
        End If

        If Me.haveOutputHeader("Content-Type") Then
            http_headers_formated = Replace(http_headers_formated, "$MIMETYPE", Me.OutputHeaders("Content-Type"))
        Else
            http_headers_formated = Replace(http_headers_formated, "$MIMETYPE", Me.getOutputContent())
        End If

        http_headers_formated = Replace(http_headers_formated, "$CONTENTLENGTH", content_length.ToString)
        http_headers_formated = Replace(http_headers_formated, "$CONNECTIONSTATE", connection_state)
        http_headers_formated = Replace(http_headers_formated, "$ENCODINGNAME", Me.getEncoding().WebName)

        If Me.HaveActiveSession Then
            http_headers_formated = Replace(http_headers_formated, "$COOKIELINE", "Set-Cookie: " & GlobalSetting.SESSION_COOKIE_NAME & "=" & Me.Session.ID)
        Else
            http_headers_formated = Replace(http_headers_formated, "$COOKIELINE", "Set-Cookie: ")
        End If

        'when user sets headers, we need to make sure the default one's are overwritten.

        If Me.haveOutputHeaders Then
            Dim default_server_output_dic As StringDictionary
            Dim all_headers As TriniDATWordDictionary
            Dim hdr_index As Integer

            'make sure user headers are appended as last.
            all_headers = New TriniDATWordDictionary("", Nothing)

            default_server_output_dic = TriniDATClientConnectionManagerHTTP.parseHeadersByPacket(http_headers_formated)

            'all_headers

            For Each usr_hdr_name In OutputHeaders.Keys
                all_headers.Add(usr_hdr_name & ": " & Trim(Me.OutputHeaders(usr_hdr_name)))
            Next

            http_headers_formated = Replace(http_headers_formated, "$LAST", all_headers.ToString(vbCrLf))
            http_headers_formated = http_headers_formated
        Else
            http_headers_formated = Replace(http_headers_formated, "$LAST", "")
        End If

        Return http_headers_formated

    End Function

    Public Overloads Function writeRaw(ByVal inc_http_server_headers As Boolean, ByVal output_buffer As StringCollection, Optional ByVal close_connection As Boolean = False) As String

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.writeRaw(inc_http_server_headers, output_buffer, close_connection)
        End If

        'master socket
        TriniDATClientConnectionManagerHTTP.counter = TriniDATClientConnectionManagerHTTP.counter + 1

        Dim line As String
        Dim output As NetworkStream
        Dim output_data As String
        Dim bErr As Boolean
        output_data = ""
        bErr = True

        Try

            'buffer user data
            For Each line In output_buffer
                output_data = output_data & line
            Next

            If inc_http_server_headers Then
                Dim contentLength As Integer
                'get buffer size
                contentLength = output_data.Length
                MSG("Content-length=" & contentLength.ToString & ". Appending to buffer...")
                output_data = Replace(formatHTTPHeaders(contentLength), "$CONTENTDATA", output_data)
            End If


            'write
            If Not Me.getConnection().canWrite() Then
                Throw New Exception("Write error: Connection closed while writing.")
            Else
                output = Me.getConnection().GetStream()
                output.Write(Me.getEncoding().GetBytes(output_data), 0, Me.getEncoding().GetByteCount(output_data))
            End If


            bErr = False

        Catch ex As Exception
            MSG("writeRaw(stringcollection) error: " & ex.Message)


        End Try
        output_buffer.Clear()
        If close_connection Then
            Me.Flushed = True
            Me.getConnection().DisconnectNow()
        End If

        If Not bErr Then
            Return output_data
        Else
            Return Nothing
        End If



    End Function
    Public Shared Function parseHeadersByPacket(ByVal packet As String) As StringDictionary


        'master socket

        ''midrrev 'Content-Length:
        Dim hdr() As String
        Dim x As Integer
        Dim retval As StringDictionary


        'extract header info
        hdr = Split(packet, vbCrLf)
        retval = TriniDATClientConnectionManagerHTTP.parseNestedKeyValPair(hdr, ":", True, 1) '1=skip GET/POSt line 


        Return retval

    End Function
    Public Function parseHeaders(ByVal packet As String) As Integer

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.parseHeaders(packet)
        End If

        'master socket

        ''midrrev 'Content-Length:
        Dim hdr() As String
        Dim x As Integer


        'extract header info
        hdr = Split(packet, vbCrLf)
        Me.Headers = parseNestedKeyValPair(hdr, ":", True, 1) '1=skip GET/POSt line 


        Return Me.Headers.Count

    End Function
    Public Shared Function parseNestedKeyValPair(ByVal lines() As String, ByVal delim As String, Optional ByVal quit_on_empty As Boolean = False, Optional ByVal offset As Integer = 0, Optional ByVal url_decode_value_part As Boolean = False) As StringDictionary

        ''midrrev 'Content-Length:
        Dim key As String
        Dim val As String
        Dim entity() As String
        Dim x As Integer
        Dim retval As StringDictionary

        retval = New StringDictionary

        For x = offset To lines.Length - 1

            lines(x) = Trim(lines(x))

            If quit_on_empty And lines(x) = "" Then
                GoTo DONE
            End If

            'extra header key,value
            entity = Split(lines(x), delim)

            key = Trim(entity(0))
            val = Mid(lines(x), Len(key) + Len(delim) + 1)
            val = Trim(val)

            ' MSG("parseNestedKeyValPair: " & key & "=" & val)

            If Not retval.ContainsKey(key) Then
                If url_decode_value_part Then
                    val = HttpUtility.UrlDecode(val)
                End If
                retval.Add(key, val)
            End If


        Next

DONE:
        Return retval

    End Function
    Public Function parsePOSTParameters(ByVal packet As String) As StringDictionary

        'master socket
        Dim contentlength_str As String
        Dim post_body As String
        Dim retval As StringDictionary
        Dim form_encoding_type As String
        Dim receive_content_length_local As Integer
        Dim x As Integer

        retval = New StringDictionary
        'Content-Type: application/x-www-form-urlencoded
        form_encoding_type = ""
        receive_content_length_local = 0

        Me.Receive_Content_Length = 0



        If Me.haveHeader("content-length") Then
            contentlength_str = Me.Headers("content-length")
            If IsNumeric(contentlength_str) Then
                receive_content_length_local = CLng(contentlength_str)
                'initialize receive statistics  on socket
                Me.Receive_Content_Length = receive_content_length_local
            End If
        End If

        If Me.haveHeader("content-type") Then
            form_encoding_type = Me.Headers("content-type")
        End If


        'multipart/form-data = fileupload
        'application/x-www-form-urlencoded
        'text/plain

        If Me.Receive_Content_Length > 0 And form_encoding_type = "application/x-www-form-urlencoded" Then

            post_body = Me.getRequestBody(packet) & "&"

            retval = TriniDATClientConnectionManagerHTTP.parseNestedKeyValPair(post_body.Split("&"), "=", True, 0, True)

            'return key/val pair
            Return retval
        Else
            'file upload
            post_body = Me.getRequestBody(packet)


            Me.Receive_Bytes_Left = Me.Receive_Content_Length - Len(post_body)
            If Me.Receive_Bytes_Left < 0 Then
                Me.Receive_Bytes_Left = 0
            End If

            'wrap in a single var
            retval.Add("alldata", post_body)
            retval.Add("alldata_length", Len(post_body).ToString)
            Return retval
        End If

        'nothing 
        Return New StringDictionary

    End Function

    Public Overloads Sub writeRaw()
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.writeRaw()
            Exit Sub
        End If

        'master socket
        writeRaw(False, output_buffer)
        Me.deleteOutputBuffer()

    End Sub
    Public Function writeFile(ByVal inc_http_server_headers As Boolean, ByVal path As String, Optional ByVal close_connection As Boolean = False) As Boolean

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.writeFile(inc_http_server_headers, path, close_connection)
        End If

        'master socket

        Dim output As NetworkStream
        Dim header_str As String
        Dim fs As IO.FileStream
        Dim buffer_size As Integer

        buffer_size = GlobalObject.CurrentServerConfiguration.receive_buffer_size

        Dim buffer(buffer_size) As Byte
        Dim readSize As Integer



        fs = Nothing
        header_str = ""

        Try
            MSG("Writefile: Uploading " & path & " to client socket.")

            fs = New IO.FileStream(path, IO.FileMode.Open)


            output = Me.getConnection().GetStream()

            If inc_http_server_headers Then
                header_str = Replace(formatHTTPHeaders(fs.Length), "$CONTENTDATA", "")

                'write header
                output.Write(Me.getEncoding().GetBytes(header_str), 0, Me.getEncoding().GetByteCount(header_str))
            End If

            readSize = -1

            While Me.getConnection().isConnected() = True And readSize <> 0
                readSize = fs.Read(buffer, 0, buffer.Length)
                If readSize = 0 Then Exit While

                'upload to socket
                output.Write(buffer, 0, readSize)
            End While

            fs.Close()

            MSG("Writefile: Upload complete.")

            If close_connection Then
                output.Flush()
                Me.Flushed = True
                Me.getConnection().DisconnectNow()
            End If

            Return True

        Catch ex As Exception
            MSG("writeFile(string) error: " & ex.Message)
            If Not IsNothing(fs) Then
                fs.Dispose()
            End If
        End Try

        Return False

    End Function
    Public Overloads Function writeRaw(ByVal inc_http_server_headers As Boolean, ByVal line As String, Optional ByVal close_connection As Boolean = False) As Boolean

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.writeRaw(inc_http_server_headers, line, close_connection)
        End If

        'master socket
        Dim output As NetworkStream
        Dim content_bytes() As Byte
        Dim bytecount As Long

        If IsNothing(line) Then
            line = ""
        End If

        Try

            output = Me.getConnection().GetStream()

            If inc_http_server_headers Then
                line = Replace(formatHTTPHeaders(line.Length), "$CONTENTDATA", line)
            End If

            content_bytes = Me.getEncoding().GetBytes(line)
            bytecount = Me.getEncoding().GetByteCount(line)

            'write user data
            output.Write(content_bytes, 0, bytecount)

            If close_connection Then
                'Me.getConnection().DisconnectNow()
                Me.Flushed = True
                Me.getConnection().forceDisconnect()
            End If


            Return True

        Catch ex As Exception
            MSG("writeRaw(string) error: " & ex.Message)
        End Try

        Return False

    End Function
    Public Function sendHTML(ByVal html As String) As Boolean

        Dim retval As Boolean = False

        Try
            Dim myWriteBuffer As Byte() = Me.getEncoding().GetBytes(defaultHTTPResponse & vbCrLf & vbCrLf & html)
            getConnection().GetStream().Write(myWriteBuffer, 0, myWriteBuffer.Length)

            retval = True


        Catch ex As Exception
            MSG("sendHTML err: " & ex.Message)
        End Try

        Return retval
    End Function

    Public ReadOnly Property haveSocket() As Boolean
        Get

            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.haveSocket
            End If

            'master socket
            If IsNothing(Me._connie) Then
                Return False
            End If

            Return Me._connie.haveSocket
        End Get
    End Property
    Public ReadOnly Property RemoteIP As String
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.RemoteIP
            End If

            'master socket
            If Not Me.haveSocket() Then Return Nothing
            Return _connie.RemoteIP
        End Get
    End Property
    Public ReadOnly Property IsGetRequest As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.IsGetRequest
            Else
                'master socket
                Return Me.getConnection().IsGetRequest()
            End If
        End Get
    End Property
    Public ReadOnly Property IsPOSTRequest As Boolean
        Get
            If Me.haveMasterSocket Then
                'slave socket
                Return Me.MasterSocket.IsPOSTRequest
            Else
                'master socket
                Return Me.getConnection().IsPostRequest()
            End If
        End Get
    End Property

    Public Overrides Function OnPacketReceived(ByVal current_jservice As Object, ByVal info As TriniDATRequestInfo, ByVal packet As Byte(), ByVal packetSize As Long) As Boolean


        'var stripping related 
        Dim x As Integer = 0
        Dim count As Integer = 0
        Dim mappingsFound As Boolean = False
        Dim request_url As String

        request_url = info.recreateRelativeURL

        ' stringifiedPacket = Me.getEncoding().GetString(packet)


        'pass to mapping point process.

        If Me.IsGetRequest() Then
            Dim target_get_del As TriniDATHTTP_EventTable.OnHTTPRequestGetTemplate

            'note: when no user declared function is found the packet is passed to a declared lower level OnGet* function or default OnGet.

            If current_jservice.haveGETFunctions Then
                Dim url_related_get_functions As TriniDATServerFunctionTable
                url_related_get_functions = CType(current_jservice, JTriniDATWebService).WebserviceFunctionTable.AllGETFunctions.getAllByURL(info.FullMappingPointPath & info.unparsed_url_part)

                If Not IsNothing(url_related_get_functions) Then

                    For Each get_webservice_function As TriniDAT_ServerGETFunction In url_related_get_functions

                        Dim parm_init_result As TRINIDAT_HTTP_INITIALIZATION_RESULT

                        If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                            'invoke
                            GlobalObject.MsgColored("'" & info.associated_app.ApplicationName & "' -> '" & info.recreateRelativeURL & "': Invoking user proc @ " & get_webservice_function.FunctionURL & "...", Color.Green)
                        End If

                        parm_init_result = current_jservice.initializeGETFunction(get_webservice_function, Me.getParameters())

                        If parm_init_result.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS Then
                            Return current_jservice.TriggerUserGETFunction(get_webservice_function, Me.getParameters(), Me.Headers)
                        Else
                            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                                'return invalid parameter message
                                info.http_connection_handler.writeRaw(True, parm_init_result.errormsg, True)
                            Else
                                'hide the complete situation in production mode.
                                info.http_connection_handler.setHTTPResponse(404)
                                info.http_connection_handler.writeRaw(True, "Invalid request.", True)
                            End If

                            Return False
                        End If
                    Next
                End If
            End If

            'route to packet-level filters

            'default
            target_get_del = Me.getEventTable().event_onget

            If Me.getEventTable().haveInternetGETEventHandler Or Me.getEventTable().haveIntranetGETEventHandler Or Me.getEventTable().haveLocalhostGETEventHandler Then

                If Me.getEventTable().haveInternetGETEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isInternetClient Then
                    target_get_del = Me.getEventTable().event_onget_internet
                ElseIf Me.getEventTable().haveIntranetGETEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isIntranetClient Then
                    target_get_del = Me.getEventTable().event_onget_intranet
                ElseIf Me.getEventTable().haveLocalhostGETEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isLocalHostClient Then
                    target_get_del = Me.getEventTable().event_onget_localhost
                End If

            End If



            Dim get_parameters As StringDictionary
            get_parameters = Me.getParameters()

            'fire found OnGET handler
            target_get_del(request_url, get_parameters, Me.Headers)
            Return True

        ElseIf Me.IsPOSTRequest() Then

            Dim target_post_del As TriniDATHTTP_EventTable.OnHTTPRequestPostTemplate

            'default
            target_post_del = Me.getEventTable().event_onpost

            If current_jservice.havePOSTFunctions Then

                For Each post_webservice_function As TriniDAT_ServerPOSTFunction In CType(current_jservice, JTriniDATWebService).WebserviceFunctionTable.AllPOSTFunctions.getAllByURL(info.FullMappingPointPath & info.unparsed_url_part)

                    Dim parm_init_result As TRINIDAT_HTTP_INITIALIZATION_RESULT

                    If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                        'invoke
                        GlobalObject.MsgColored("'" & info.associated_app.ApplicationName & "' => '" & info.recreateRelativeURL & "': Invoking user proc @ " & post_webservice_function.FunctionURL & "...", Color.Green)
                    End If

                    parm_init_result = current_jservice.initializepostFunction(post_webservice_function, Me.getParameters)

                    If parm_init_result.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS Then
                        Return current_jservice.TriggerUserPOSTFunction(post_webservice_function, Me.getParameters(), Me.Headers)
                    Else
                        If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                            'return invalid parameter message
                            info.http_connection_handler.writeRaw(True, parm_init_result.errormsg, True)
                        Else
                            'hide the complete situation in production mode.
                            info.http_connection_handler.setHTTPResponse(404)
                            info.http_connection_handler.writeRaw(True, "Invalid POST.", True)
                        End If

                        Return False
                    End If

                Next

            End If
            'trigger packet-level function.

            If Me.getEventTable().haveInternetPOSTEventHandler Or Me.getEventTable().haveIntranetPOSTEventHandler Or Me.getEventTable().haveLocalHostPOSTEventHandler Then

                If Me.getEventTable().haveInternetPOSTEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isInternetClient Then
                    target_post_del = Me.getEventTable().event_onpost_internet
                ElseIf Me.getEventTable().haveIntranetPOSTEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isIntranetClient Then
                    target_post_del = Me.getEventTable().event_onpost_intranet
                ElseIf Me.getEventTable().haveLocalHostPOSTEventHandler And CType(info.http_connection_handler.getConnection(), TriniDATTCPSocket).isLocalHostClient Then
                    target_post_del = Me.getEventTable().event_onpost_localhost
                End If
            End If

            target_post_del(request_url, Me.getParameters, Me.Headers)
            Return True

        Else
            GlobalObject.Msg("Unknown http packet: " & Me.getEncoding().GetString(packet))
        End If

        Return False
    End Function

    Public Shared Function getResponseBody(ByVal packet As String) As String

        'master socket
        Dim content_start As Long
        Dim content_length As Integer
        Dim temp_headers As StringDictionary

        content_start = InStr(packet, vbCrLf & vbCrLf)

        temp_headers = TriniDATClientConnectionManagerHTTP.parseHeadersByPacket(packet)

        If temp_headers.ContainsKey("Content-Length") Then
            content_length = CLng(temp_headers("Content-Length"))
        Else
            content_length = Len(packet) - content_start
        End If

        Return Mid(packet, content_start + 4, content_length)
    End Function
    Public Function getRequestBody(ByVal packet As String) As String
        Return TriniDATClientConnectionManagerHTTP.getResponseBody(packet)
    End Function

    Public Function haveParameters() As Boolean
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.haveParameters
        End If

        'master socket
        Return (parameters.Count > 0)
    End Function
    Public Sub resetParameters()
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.resetParameters()
            Exit Sub
        End If

        'master socket
        parameters = New StringDictionary
    End Sub

    Public Sub setParameters(ByVal params As StringDictionary)
        parameters = params
    End Sub
    Public Function getParameters() As StringDictionary
        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getParameters
        End If

        'master socket

        Return Me.parameters
    End Function

    Public Sub addOutput(ByVal data As String)
        If Me.haveMasterSocket Then
            'slave socket
            Me.MasterSocket.addOutput(data)
            Exit Sub
        End If

        'master socket
        output_buffer.Add(data)
        Me.Flushed = False
    End Sub
    Public Function getRequestParameters() As StringDictionary
        'gets the current request's parameters -> getLiveRequestParameters

        If Me.haveMasterSocket Then
            'slave socket
            Return Me.MasterSocket.getRequestParameters()
        End If

        'master socket
        Dim socket As TriniDATSockets.TriniDATTCPSocket

        socket = Me.getConnection()

        If socket.haveParameters() Then
            Return socket.Parameters
        Else
            Return Nothing
        End If

    End Function
    Public Shared Function getURLPart(ByVal part As String, ByVal http_packet As String) As Object
        'part = method   GET/POST / ret String
        'part = URI     e.g. http://www.google.com/home/  / ret String
        'part = parameter e.g. var1=a&var2=b / ret Collection

        Dim URI_Request_Type As String
        Dim firstLine() As String
        Dim packetLines() As String

        Dim URIParameterStart As Integer
        Dim URLPart() As String

        'lineify packet
        packetLines = http_packet.Split(vbCrLf)

        Try

            If InStr(packetLines(0), " ") = 0 Then
                Err.Raise("Too small to be true [no spaces]")
            End If


            'split request on space
            firstLine = Split(packetLines(0), " ")

            'firstLine(0) = GET/POST
            URI_Request_Type = firstLine(0)

            If part = "method" Then
                Return URI_Request_Type
            End If


            'firstLine(1) = URI?vara=1&varb=2
            'firstLine(2) = HTTP/1.0

            'parse url
            URIParameterStart = InStr(firstLine(1), "?")

            If URIParameterStart > 0 Then
                'easyfy parsing by using the same char everywhere.
                firstLine(1) = Replace(firstLine(1), "?", "&") 'URI&vara=1&varb=2
                'used later on for passing parameter string to URL listeners.
                URIParameterStart = URIParameterStart + 1
            Else
                'no parameters found
                If part = "parameter" Then Return New StringDictionary
            End If

            URLPart = Split(firstLine(1), "&")

            If part = "uri" Then
                Return URLPart(0)
            ElseIf part = "parameter" Then

                Dim uri_parameter_string As String = ""

                If URIParameterStart > 0 Then
                    Dim retval As New StringDictionary
                    Dim pars() As String
                    Dim key As String
                    Dim val As String
                    Dim seperator As Integer

                    uri_parameter_string = Mid(firstLine(1), URIParameterStart)

                    pars = uri_parameter_string.Split("&")

                    For Each pair In pars
                        seperator = InStr(pair, "=")
                        If seperator > 0 Then
                            key = Mid(pair, 1, seperator - 1)
                            val = HttpUtility.UrlDecode(Mid(pair, seperator + 1))
                        Else
                            key = pair
                            val = ""
                        End If
                        retval.Add(key, val)
                    Next

                    Return retval

                Else
                    'return empty
                    Return New StringDictionary
                End If

            End If


        Catch ex As Exception
            MSG("TriniDATClientConnectionManagerHTTP.getURLPart error:" & ex.Message)
            Return Nothing

        End Try

        Return ""
    End Function

End Class
