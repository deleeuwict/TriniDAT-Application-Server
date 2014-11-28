Imports TriniDATServerTypes
Imports TriniDATSockets

'abstract class that is the internal trafficker for JSON objects.

Public Class JSONMailBoxServer

    Delegate Function PassObjectTemplate(ByVal obj As JSONObject) As Boolean
    Delegate Function ServerTerminator(ByVal current_obj As JSONObject) As Boolean

    'Private Shared Listeners As JSONMailBoxListeners
    Protected Shared global_live_or_die As ServerTerminator

    Public Shared Sub OnServerStartInit()
        JSONMailBoxServer.global_live_or_die = AddressOf nullIsLiveHandler
    End Sub
    Public Shared Sub OnServerStopping()
        'route all messages to a kill procedure.
        JSONMailBoxServer.global_live_or_die = AddressOf NullIsProcessKiller
    End Sub
    Public Shared Sub OnBeforeServerStopped()

    End Sub

    Public Shared Function NullIsLiveHandler(ByVal current_obj As JSONObject) As Boolean
        'keep going
        Return False
    End Function
    Public Shared Function NullIsProcessKiller(ByVal current_obj As JSONObject) As Boolean

        'intercept message and kill messenger
        Dim from_service As JTriniDATWebService

        from_service = current_obj.Sender

        If Not IsNothing(from_service) Then
            'disconnect its socket
            If from_service.haveIOHandler() Then

                'patch event table
                Dim null_event_table As TriniDATHTTP_EventTable
                Dim io_handler As TriniDATClientConnectionManagerHTTP
                Dim io_handler_socket As TriniDATTCPSocket

                io_handler_socket = Nothing
                io_handler = from_service.GetIOHandler()

                null_event_table = New TriniDATHTTP_EventTable
                null_event_table.setDefaults()
                io_handler.Configure(null_event_table)

                io_handler_socket = io_handler.getConnection()

                If Not IsNothing(io_handler_socket) Then
                    io_handler_socket.forceDisconnect()
                End If

                io_handler.deleteOutputBuffer()

            End If
            'from_service.GetIOHandler()
        End If

        'indicate a kill handler is in effect.
        Return True
    End Function

    ''Public Shared Sub addListener(ByVal sense As JSONMailBoxListener)
    ''    'listener code written but never tested: 
    ''    JSONMailBoxServer.Listeners.getListeners().Add(sense)
    ''End Sub

    'Public Shared Function hasListener(ByVal mp_index As Integer, ByVal className As String) As Boolean
    '    Return JSONMailBoxServer.Listeners.hasListener(mp_index, className)
    'End Function

    Public Shared Function PassObject(ByVal obj As JSONObject, Optional ByVal to_mappingPoint As mappingPointRoot = Nothing, Optional ByVal to_className As String = Nothing, Optional ByVal disable_identical_sender_and_recipient As Boolean = False) As Object

        If JSONMailBoxServer.global_live_or_die(obj) Then
            Return False
        End If

        'invoke all loaded processes and offer this object in a serial fashion.
        Dim JClassInfo As mappingPointClass
        Dim JClassName As String
        Dim mappingPoint As mappingPointRoot
        Dim mappingPointClasses As List(Of mappingPointClass)
        Dim header As JSONMailJob
        Dim sender_mp_process_id As Integer
        Dim sender_mp_id As Integer
        Dim found_process As Boolean
        Dim self_found As Boolean
        Dim mailbox_for_sender_call_back As JServicedMailBox

        'INIT
        found_process = False
        JClassName = "unknown"
        mappingPoint = to_mappingPoint
        mailbox_for_sender_call_back = obj.Sender.getMailProvider()

        header = New JSONMailJob()
        header.direction = JSONMailJob_DIRECTION.OUT_INTERNAL
        header.setTargetMP(mappingPoint)
        header.json_obj = obj
        header.setDeliverEvents(AddressOf mailbox_for_sender_call_back.OnDelivered, AddressOf mailbox_for_sender_call_back.OnFailedDelivery)

        Try
            sender_mp_process_id = obj.Sender.getProcessIndex
            sender_mp_id = obj.Sender.getMappingIndex()

            'fetch all processes in mapping point(s).
            If (Not IsNothing(to_className) And mappingPoint.hasClass(to_className)) Or (IsNothing(to_className)) Then

                If IsNothing(to_className) Then
                    'broadcast to all available classes
                    mappingPointClasses = mappingPoint.getClasses()
                Else
                    mappingPointClasses = New List(Of mappingPointClass)
                    mappingPointClasses.Add(mappingPoint.getClassByName(to_className))
                End If


                For Each JClassInfo In mappingPointClasses

                    'Find all running instances of the class.

                    Try
                        JClassName = JClassInfo.getName()

                        Dim proc_id As Integer

                        proc_id = mappingPoint.getActiveProcessIdByClass(JClassName)

                        If Not IsNothing(proc_id) Then

                            'enable to skip self sender
                            If disable_identical_sender_and_recipient Then
                                If (mappingPoint.getIndex() = sender_mp_id And sender_mp_process_id = proc_id) Then
                                    self_found = True
                                    GoTo DO_NEXT_PROCESS
                                End If
                            End If
                            '   If Not (mappingPoint.getIndex() = sender_mp_id And sender_mp_process_id = proc_id) Then

                            'get process 
                            Dim proc As mappingPointInstanceInfo
                            proc = mappingPoint.getInstanceInfo(proc_id)
                            found_process = Not IsNothing(proc)

                            If found_process Then
                                header.setTargetProcess(proc.getProcess())
                                header.DeliveryState = JSONMailJob_DELIVERY_STATE.DRAFT

                                'send if inbox is present
                                If proc.getProcess().isObjectListener() Then
                                    'MSG("Offering a piece of " & obj.ObjectType & " to linkpoint: " & proc.getParent().getURI() & "@" & to_className)
                                    header.attemptJSONDelivery()
                                Else
                                    'mark as sent anyway.
                                    header.DeliveryState = JSONMailJob_DELIVERY_STATE.SENT
                                End If

                                If GlobalObject.haveServerThread Then
                                    If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                                        'due to errors, traffic item may not be logged
                                        If Not header.Logged Then
                                            If Not header.sendtoTrafficLog() Then
                                                'something gone seriously wrong. abort sending message.
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            'End If 'self sender checking

                        Else
                            header = header

                        End If

                    Catch ex As Exception
                        MSG("PassObject:  error or exception in JClass. Err: " & ex.Message & "  Location: " & ex.StackTrace.ToString)
                        header.DeliveryState = JSONMailJob_DELIVERY_STATE.RECIPIENT_ERROR_NOT_FOUND
                        header.DeliveryStateMessage = ex.Message

                        Return False
                    End Try
DO_NEXT_PROCESS:
                Next
            End If

            If Not found_process And Not self_found Then
                header.DeliveryState = JSONMailJob_DELIVERY_STATE.RECIPIENT_ERROR_NOT_FOUND
                If Not IsNothing(to_className) Then
                    header.DeliveryStateMessage = "'" & to_className & "' not present."
                End If
                Return header
            Else
                Return True
            End If



        Catch ex As Exception
            MSG("PassObject: Error while linking " & JClassName & " to  " & mappingPoint.getURI())
            header.DeliveryState = JSONMailJob_DELIVERY_STATE.ERR_UNKNOWN
            header.DeliveryStateMessage = JClassName & " Internal error (" & ex.Message & ")"
        End Try
        Return False
        '     MSG("PassObject: successfully delivered object '" & obj.ObjectTypeName & "' to " & total_recipients.ToString & " recipient(s).")
    End Function
    Public Shared Sub MSG(ByVal TXT As String)
        'Log to main form in a different thread.
        Debug.Print("Global MailServer status: " & TXT)
    End Sub


End Class

