Option Explicit On
Option Compare Text
Imports System.drawing
Imports System.Globalization
Imports System.Speech.Synthesis
Imports SimonTypes
Imports TriniDATServerTypes
Imports Newtonsoft.Json
Imports System.Xml.Serialization
Imports System.IO
Imports System.Text
Imports TriniDATDictionaries
Imports System.Threading
Imports TriniDATSockets


Public Class SimonsDebuggingCommands
    Private info As SimonsSession
    Private myTag As String
    Private editable_jsonfields As TriniDATWordDictionary

    Public Sub New(ByVal _simons_info As SimonsSession)
        Me.info = _simons_info
        Me.editable_jsonfields = New TriniDATWordDictionary("jsoneditablefields", New List(Of String)({"Directive", "ObjectType", "PaymentId", "ObjectAttachment"}))
    End Sub

    Private ReadOnly Property SimonsInfo As SimonsSession
        Get
            Return Me.info
        End Get
    End Property
    Public ReadOnly Property isJSONFieldEditable(ByVal field_name As String) As Boolean
        Get
            Return Me.editable_jsonfields.Has(field_name)
        End Get
    End Property
    Public Function DUMPJSON(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If Me.SimonsInfo.haveDebugFrames Then
            If Me.SimonsInfo.CurrentDebuggingFrame.haveJSON Then
                Me.SimonsInfo.AddConsoleLine("JSON: " & Me.SimonsInfo.CurrentDebuggingFrame.JSON, Color.Gold)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If

        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_INVALID"), Color.Red)
        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function

    Private Function ReplaceJSONValue(ByVal JSON As String, ByVal field_name As String, ByVal new_value As String) As String
        'returns new json string

        Dim old_value As String
        Dim findstr As String
        Dim newstr As String

        old_value = Me.JSON_getFieldValue(Me.SimonsInfo.CurrentDebuggingFrame.JSON, field_name)

        findstr = Chr(34) & field_name & Chr(34) & ":" & Chr(34) & old_value & Chr(34)
        newstr = Chr(34) & field_name & Chr(34) & ":" & Chr(34) & new_value & Chr(34)

        Return Replace(JSON, findstr, newstr)

    End Function
    Public Function LEAVE(ByVal val As String) As SimonsReturnValue

        Dim socket As TriniDATTCPSocket
        socket = Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getConnection()

        If socket.isConnected() Then
            Call DROPSOCKET("")

        End If

        Return SimonsReturnValue.VALIDCOMMAND_NEXT_CONTEXT

    End Function
    Public Function DROPSOCKET(ByVal val As String) As SimonsReturnValue

        Dim socket As TriniDATTCPSocket

        socket = Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getConnection()

        If Not IsNothing(socket) Then
            If socket.isConnected() Then
                Try
                    Me.SimonsInfo.AddConsoleLine("dropping debug socket.", Color.Gold)
                    socket.forceDisconnect()
                    Return SimonsReturnValue.VALIDCOMMAND_NEXT_CONTEXT
                Catch ex As Exception

                End Try
            End If
        End If


        Me.SimonsInfo.AddConsoleLine("no socket to drop.", Color.Red)

        Return SimonsReturnValue.VALIDCOMMAND_NEXT_CONTEXT

    End Function
    Public Function DIRECTIVE(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Dim deserialized As Object
        Dim text_encoding As Encoding


        'validate object
        If Me.SimonsInfo.haveDebugFrames Then
            If Not Me.SimonsInfo.CurrentDebuggingFrame.haveJSON Then
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_INVALID"), Color.Red)
                Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
            End If
        End If

        text_encoding = New UTF8Encoding

        deserialized = JsonConvert.DeserializeObject(Me.SimonsInfo.CurrentDebuggingFrame.JSON)

        '        deserialized.Directive = "ABC"

        For Each jobj In deserialized
            Dim prop As Newtonsoft.Json.Linq.JProperty
            prop = DirectCast(jobj, Newtonsoft.Json.Linq.JProperty)

            Me.SimonsInfo.AddConsoleLine(prop.Name.ToString)
            Me.SimonsInfo.AddConsoleLine(prop.Value.ToString)

        Next

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function WRITEOBJ(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'write json object back to socket and ends the object debugging context.

        'Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client
        Dim socket As TriniDATTCPSocket
        Dim socket_ok As Boolean
        Dim serialized_transport_object As JSONSmallObject

        If Not Me.SimonsInfo.CurrentDebuggingFrame.haveJSON Then
            Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("INVALIDJSON"), Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND
        End If


        socket = Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getConnection()
        socket_ok = socket.isConnected()


        If Not socket_ok Then
            Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("INVALIDSOCKET"), Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND
        End If

        'garantuee valid pack

        Dim serialized_user_packet As Object

        Try
            serialized_user_packet = JsonConvert.SerializeObject(Me.SimonsInfo.CurrentDebuggingFrame.JSON)

        Catch ex As Exception
            Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("INVALIDJSON" & ":" & ex.Message), Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND

        End Try

        Dim paymentid As String
        paymentid = ""

        'extract payment settings from associated app.
        If Me.SimonsInfo.CurrentDebuggingFrame.haveApp Then
            If Me.SimonsInfo.CurrentDebuggingFrame.associated_app.haveApplicationPaymentInfo Then
                paymentid = Me.SimonsInfo.CurrentDebuggingFrame.associated_app.ApplicationPaymentInfo.handle.ToString
            End If
        End If

        serialized_transport_object = New JSONSmallObject(Me.SimonsInfo.ObjectServer, Me.SimonsInfo.ObjectServerPort, paymentid)

        'fill object
        serialized_transport_object.Directive = Me.JSON_getFieldValue(Me.SimonsInfo.CurrentDebuggingFrame.JSON, "Directive")
        serialized_transport_object.PaymentID = Me.JSON_getFieldValue(Me.SimonsInfo.CurrentDebuggingFrame.JSON, "PaymentID")
        serialized_transport_object.ObjectType = Me.JSON_getFieldValue(Me.SimonsInfo.CurrentDebuggingFrame.JSON, "ObjectType")
        serialized_transport_object.ObjectAttachment = Me.JSON_getFieldValue(Me.SimonsInfo.CurrentDebuggingFrame.JSON, "ObjectAttachment")

        Try
            Me.Tag = serialized_transport_object.getPackedHTTPString(New JTRANSPORTABLE_METHOD(JTRANSPORT_METHODINFO.RESPONSE_MODIFIED_OBJECT))

            If Me.Tag.ToString = "err" Or Len(Me.Tag.ToString) < 1 Then
                Throw New Exception("Error during transport initialization. .")
            End If


            'Dim send_thread As Thread

            'send_thread = New Thread(AddressOf Me.sendObjectThread)
            'send_thread.Start(serialized_transport_object)

            Dim http_response As JSONSendResult
            Dim parsed_http_response As String


            http_response = serialized_transport_object.Send(Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getConnection(), Me.Tag.ToString, False)

            Me.Tag = Me.Tag

            If Not http_response.success Then
                If http_response.haveErrorMessage Then
                    Throw New Exception("Error: " & http_response.ErrorMessage)
                Else
                    Throw New Exception("unexpected socket error")
                End If
            End If

            parsed_http_response = Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getResponseBody(http_response.response_buffer)
            parsed_http_response = parsed_http_response

            Me.SimonsInfo.AddConsoleLine("Mapping point response: " & http_response.response_buffer, Color.Gold)


        Catch ex As Exception
            Me.SimonsInfo.AddConsoleLine("WRITESOCKET command error: " & ex.Message, Color.Gold)

        End Try
        'object_packet = Me.SimonsInfo.CurrentDebuggingFrame.header.getPackedHTTPString(



        Return SimonsReturnValue.VALIDCOMMAND_NEXT_CONTEXT
    End Function

    Private Sub sendObjectThread(ByVal serialized_transport_object As JSONSmallObject)

        'Dim http_response As JSONSendResult
        'Dim parsed_http_response As String



        'http_response = serialized_transport_object.Send(Me.Tag.ToString, True)

        'Me.Tag = Me.Tag

        'If Not http_response.success Then
        '    If http_response.haveErrorMessage Then
        '        Throw New Exception("Error: " & http_response.ErrorMessage)
        '    Else
        '        Throw New Exception("unexpected socket error")
        '    End If
        'End If

        'parsed_http_response = Me.SimonsInfo.CurrentDebuggingFrame.direct_http_client.getResponseBody(http_response.response_buffer)
        'parsed_http_response = parsed_http_response

        'Me.SimonsInfo.AddConsoleLine("Mapping point response: " & parsed_http_response, Color.Gold)


    End Sub
    Private Function getUnquoted(ByVal parameters As String) As List(Of String)
        'strips strings like "abc" "abc"
        '   "\"abc\""

        Dim inchar() As Char
        Dim outchar() As Char
        Dim x As Integer
        Dim charmax As Long
        Dim char_handled As Boolean
        Dim out_list As List(Of String)
        Dim quote_state As Integer '0=none 1=start 2=end 
        Dim quote_pos(10) As Integer
        Dim start_offset As Long
        Dim current_word As String

        current_word = ""
        start_offset = 0
        quote_state = 0
        out_list = New List(Of String)
        inchar = parameters.ToArray()
        charmax = inchar.Length - 1

        ReDim outchar(inchar.Length * 2)

STRIPPER:
        For x = start_offset To charmax
            char_handled = False

            If inchar(x) = Chr(34) Then
                If x > 0 Then
                    If inchar(x - 1) <> "\" Then
                        quote_state = quote_state + 1
                        quote_pos(quote_state) = x + 1 'mid's absolute pos.
                    End If
                ElseIf x = 0 Then
                    quote_state = quote_state + 1
                    quote_pos(quote_state) = x + 1 'mid's absolute pos.
                End If

                If quote_state = 2 And Not char_handled Then
                    'quote end
                    Dim startpos As Integer
                    Dim endpos As Integer
                    Dim strlen As Integer
                    Dim word_parsed As List(Of String)
                    Dim y As Long

                    startpos = quote_pos(1) + 1
                    endpos = quote_pos(2)
                    strlen = endpos - startpos
                    current_word = Mid(parameters, startpos, strlen)
                    'parse extraction.
                    word_parsed = Me.getUnquoted(current_word)

                    For y = 0 To word_parsed.Count - 1
                        out_list.Add(word_parsed(y))
                    Next

                    current_word = ""
                    quote_state = 0
                    char_handled = True
                    start_offset = endpos + 1
                    GoTo STRIPPER
                End If
            End If

            If x + 1 < charmax Then
                If inchar(x) = "\" And inchar(x + 1) = Chr(34) Then
                    current_word &= Chr(34)
                    char_handled = True
                    start_offset = x + 2 'skip escaped string.
                    GoTo STRIPPER
                End If
            End If

            If Not char_handled Then

                If inchar(x) = " " Then
                    out_list.Add(current_word)
                    current_word = ""
                    char_handled = True
                End If

            End If

            If Not char_handled Then
                current_word &= inchar(x)
                char_handled = True
            End If

        Next

        If current_word <> "" Then
            out_list.Add(current_word)
        End If

        Return out_list
    End Function
    Public Function DPONG(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
     
        For x = 0 To param.Count - 1
            Me.SimonsInfo.AddConsoleLine(x.ToString & "#: " & param(x), Color.Red)
        Next

        Return SimonsReturnValue.VALIDCOMMAND
    End Function


    Public Function FIELD(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'field <fieldname>
        'prints field value.

        'field <fieldname> "new value"
        'changes field value

        If actual_count = -1 Then
            Return SimonsReturnValue.VALIDCOMMAND_SHOWHELP
        End If

        Dim field_value As String
        Dim field_name As String
        Dim parameter_count As Integer
        Dim parameter_sections() As String
        Dim set_new_value As Boolean
        Dim quote_start As Integer
        Dim quote_end As Integer

        set_new_value = False
        parameter_sections = param
        parameter_count = actual_count + 1

        'check if syntaxis error in field set attempt.
        If parameter_count > 1 Then

            'trying to set new value
            Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("FIELD_ERR1"), Color.Red)
            Return SimonsReturnValue.VALIDCOMMAND_SHOWHELP

        Else
            Return displayFieldValue(parameter_sections(0))
        End If

        quote_start += 1

        field_name = parameter_sections(0)
        field_value = parameter_sections(1)
        
        Me.SimonsInfo.currentDebuggingFrame.JSON = Me.ReplaceJSONValue(Me.SimonsInfo.currentDebuggingFrame.JSON, field_name, field_value)

        Me.SimonsInfo.AddConsoleLine("Changed.")

        'Me.SimonsInfo.AddConsoleLine(field_name & " to " & field_value)
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Private Function displayFieldValue(ByVal field_name As String) As SimonsReturnValue
        Dim field_value As String

        'validate object
        If Me.SimonsInfo.haveDebugFrames Then
            If Not Me.SimonsInfo.currentDebuggingFrame.haveJSON Then
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_INVALID"), Color.Red)
                Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
            End If
        End If

        field_value = Me.JSON_getFieldValue(Me.SimonsInfo.currentDebuggingFrame.JSON, field_name)

        If IsNothing(field_value) Then
            Me.SimonsInfo.AddConsoleLine("Error: field '" & field_name & "' does not exist.", Color.Red)
        ElseIf Me.editable_jsonfields.Has(field_name) Then
            Me.SimonsInfo.AddConsoleLine(field_name & ": " & Chr(34) & field_value & Chr(34))
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function FIELDS(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        'validate object
        If Me.SimonsInfo.haveDebugFrames Then
            If Not Me.SimonsInfo.currentDebuggingFrame.haveJSON Then
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_INVALID"), Color.Red)
                Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
            End If
        End If

        Dim json_fields As TriniDATWordDictionary

        json_fields = Me.JSON_getFieldNames(Me.SimonsInfo.currentDebuggingFrame.JSON)

        For Each fld_name In json_fields.getWordList()
            If Me.isJSONFieldEditable(fld_name) Then
                Me.SimonsInfo.AddConsoleLine(fld_name, Color.Red)
            End If
        Next

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function DUMP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        'validate object
        If Me.SimonsInfo.haveDebugFrames Then
            If Not Me.SimonsInfo.currentDebuggingFrame.haveJSON Then
                Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_INVALID"), Color.Red)
                Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
            End If
        End If

        Dim json_fields As TriniDATWordDictionary

        json_fields = Me.JSON_getFieldNames(Me.SimonsInfo.currentDebuggingFrame.JSON)


        If json_fields.Has("ObjectType") Then
            Call Me.displayFieldValue("ObjectType")
        End If

        If json_fields.Has("Directive") Then
            Call Me.displayFieldValue("Directive")
        End If

        If json_fields.Has("PaymentId") Then
            Call Me.displayFieldValue("PaymentId")
        End If

        If json_fields.Has("ObjectAttachment") Then
            Call Me.displayFieldValue("ObjectAttachment")
        End If

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Private Function JSON_getFieldValue(ByVal JSON As String, ByVal field_name As String) As String

        Dim deserialized As Object
        Dim json_fields As TriniDATWordDictionary

        json_fields = Me.JSON_getFieldNames(JSON)

        If json_fields.Has(field_name) Then
            deserialized = JsonConvert.DeserializeObject(JSON)

            For Each jobj In deserialized
                Dim prop As Newtonsoft.Json.Linq.JProperty
                prop = DirectCast(jobj, Newtonsoft.Json.Linq.JProperty)

                If prop.Name = field_name Then
                    Return prop.Value.ToString
                End If

            Next
        Else
            Return Nothing
        End If
    End Function
    Private Function JSON_getFieldNames(ByVal JSON As String) As TriniDATWordDictionary

        Dim deserialized As Object
        Dim retval As TriniDATWordDictionary

        'for the sake of alphabatic ordering.
        Dim prop_directive As String
        Dim prop_objecttype As String
        Dim prop_objectattachment As String
        Dim prop_objectpayment_handle As String

        prop_directive = Nothing
        prop_objectattachment = Nothing
        prop_objectpayment_handle = Nothing
        prop_objecttype = Nothing

        retval = New TriniDATWordDictionary("json fieldnames", Nothing)

        deserialized = JsonConvert.DeserializeObject(JSON)

        For Each jobj In deserialized
            Dim prop As Newtonsoft.Json.Linq.JProperty
            prop = DirectCast(jobj, Newtonsoft.Json.Linq.JProperty)

            If prop.Name = "Directive" Then
                prop_directive = prop.Name
            ElseIf prop.Name = "ObjectType" Then
                prop_objecttype = prop.Name
            ElseIf prop.Name = "ObjectAttachment" Then
                prop_objectattachment = prop.Name
            ElseIf prop.Name = "PaymentId" Then
                prop_objectpayment_handle = prop.Name
            End If

        Next

        If Not IsNothing(prop_objecttype) Then
            retval.Add(prop_objecttype)
        End If

        If Not IsNothing(prop_directive) Then
            retval.Add(prop_directive)
        End If

        If Not IsNothing(prop_objectpayment_handle) Then
            retval.Add(prop_objectpayment_handle)
        End If

        If Not IsNothing(prop_objectattachment) Then
            retval.Add(prop_objectattachment)
        End If

        Return retval
    End Function
    Public Function SETFIELD(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue
        'alias for FIELD A "VALUE"

        Return Me.FIELD(param, actual_count)

    End Function
    Public Function DUMPALL(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        Try
            Call ID(param, actual_count)
            Call app_as_owner(param, actual_count)
            Call MP(param, actual_count)
            Call DUMP(param, actual_count)
            Call DUMPJSON(param, actual_count)
        Catch ex As Exception

        End Try
        Return SimonsReturnValue.VALIDCOMMAND
    End Function
    Public Function ID(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If Me.SimonsInfo.haveDebugFrames Then
            Me.SimonsInfo.AddConsoleLine("Object ID: '" & Me.SimonsInfo.currentDebuggingFrame.id.ToString & "'", Color.MidnightBlue)
            Return SimonsReturnValue.VALIDCOMMAND
        End If


        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_UNKNOWN"), Color.Red)

        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function
    Private Function app_as_owner(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If Me.SimonsInfo.haveDebugFrames Then
            If Me.SimonsInfo.currentDebuggingFrame.haveApp Then
                Me.SimonsInfo.AddConsoleLine("Owner: '" & Me.SimonsInfo.currentDebuggingFrame.App.ApplicationName & "'", Color.MidnightBlue)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If


        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_UNKNOWN"), Color.Red)

        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function
    Public Function APP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If Me.SimonsInfo.haveDebugFrames Then
            If Me.SimonsInfo.currentDebuggingFrame.haveApp Then
                Me.SimonsInfo.AddConsoleLine("'" & Me.SimonsInfo.currentDebuggingFrame.App.ApplicationName & "'", Color.MidnightBlue)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If


        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_UNKNOWN"), Color.Red)

        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function
    Public Function MP(ByVal param() As String, ByVal actual_count As Integer) As SimonsReturnValue

        If Me.SimonsInfo.haveDebugFrames Then
            If Me.SimonsInfo.currentDebuggingFrame.haveApp Then
                Me.SimonsInfo.AddConsoleLine("mapping point: '" & Me.SimonsInfo.currentDebuggingFrame.direct_mapping_point.URL & "'", Color.MidnightBlue)
                Return SimonsReturnValue.VALIDCOMMAND
            End If
        End If


        Me.SimonsInfo.AddConsoleLine(Me.SimonsInfo.getTranslated("DEBUG_OBJECT_NOMPASSOCIATED"), Color.Red)
        Return SimonsReturnValue.VALIDCOMMAND_NORESULTS
    End Function
    Public Property Tag As String
        Get
            Return myTag
        End Get
        Set(ByVal value As String)
            myTag = value
        End Set
    End Property


End Class
