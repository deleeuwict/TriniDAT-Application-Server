Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports TriniDATServerTypes
Imports System.Threading

Public Class JServicedMailBox
    Private inbox_enabled As Boolean 'save messages in array

    Protected inbox As New Collection(Of JSONObject)
    Protected outbox As New Collection(Of JSONObject)
    Private owner As JTriniDATWebService

    Private mailbox_event_table As TriniDATObjectBox_EventTable

    Public Sub New(ByVal _owner As JTriniDATWebService)
        Me.mailbox_event_table = New TriniDATObjectBox_EventTable()
        Me.mailbox_event_table.setDefault()

        Me.owner = _owner
    End Sub
   
    Public Sub configureInboxFeature(ByVal store_all_messages As Boolean)

        Me.inbox.Clear()


        Me.inbox_enabled = store_all_messages

    End Sub
    Public Function getInbox() As Collection(Of JSONObject)

        If Not Me.getInboxEnabled() Then
            Return Nothing
        End If

        Return Me.inbox

    End Function
    Public Function getParent() As JTriniDATWebService
        Return Me.owner
    End Function
    Public Function getInboxEnabled() As Boolean
        Return Me.inbox_enabled
    End Function
    Public Sub Configure(ByVal event_table As TriniDATObjectBox_EventTable, ByVal reserved As Boolean)
        Me.mailbox_event_table = event_table
    End Sub
    Public ReadOnly Property getEventTable() As TriniDATObjectBox_EventTable
        Get
            Return Me.mailbox_event_table
        End Get
    End Property
    'Public Sub configureMailBox(ByVal inbox As OnUserInboxEventTemplate, ByVal sending As OnUserSendingEventTemplate, ByVal sent As OnUserSentEventTemplate, ByVal delivered As OUserDeliveredCallbackEventTemplate, ByVal delerr As OUserDeliveryFailureEventTemplate, Optional ByVal _async_send As Boolean = True)

    '    If Not IsNothing(inbox) Then
    '        event_inbox = inbox
    '    End If

    '    If Not IsNothing(sending) Then
    '        event_sending = sending
    '    End If

    '    If Not IsNothing(sent) Then
    '        event_sent = sent
    '    End If

    '    If Not IsNothing(delerr) Then
    '        event_err = delerr
    '    End If

    '    If Not IsNothing(delivered) Then
    '        event_delivered = delivered
    '    End If

    '    Me.async_send = _async_send

    'End Sub
    Protected Sub Msg(ByVal txt As String)
        Debug.Print(owner.getClassName() & ".MailBox notification: " & txt)
    End Sub
    Public Function LocalBroadcast(ByVal obj As JSONObject, Optional ByVal send_to_self As Boolean = True)
        'same as broadcast except not self-sender 
        Return Me.Send(obj, Nothing, Nothing, Not send_to_self)
    End Function
    Public Function Send(ByVal obj As JSONObject, ByVal to_className As String)
        Return Me.Send(obj, Nothing, to_className)
    End Function
    Public Function Send(ByVal obj As JSONObject, Optional ByVal to_mapping_point_urls As List(Of String) = Nothing, Optional ByVal to_className As String = Nothing, Optional ByVal disable_identical_sender_and_recipient As Boolean = False) As Boolean

        If Not owner.haveProcessIndex() Then
            Msg("Cannot access process info, process Id is unknown (passing from constructor?)")
            Return False
        End If

        Try

            Dim source_mapping_point As mappingPointRoot
            Dim destinationMappingPoints As List(Of mappingPointRoot)

            If IsNothing(obj) Then
                Err.Raise(100, 0, "JSON send failure: empty message was passed.")
                Return False
            End If

            source_mapping_point = Me.getParent().getProcessDescriptor().getParent()
            destinationMappingPoints = New List(Of mappingPointRoot)

            If IsNothing(to_mapping_point_urls) Then
                ''destination is self
                destinationMappingPoints.Add(source_mapping_point)
            Else
                'resolve current application's mapping point instances by urls
                Dim direct_user As TriniDATUser
                Dim direct_session As TriniDATUserSession

                For Each url In to_mapping_point_urls


                    direct_user = GlobalObject.server.Users.getBySessionID(source_mapping_point.Info.direct_session.ID)

                    If Not IsNothing(direct_user) Then
                        If direct_user.haveSession() Then
                            direct_session = direct_user.Sessions.getSessionById(source_mapping_point.Info.direct_session.ID)
                            If Not IsNothing(direct_session) Then
                                If direct_session.haveApplicationById(source_mapping_point.ApplicationId) Then
                                    Dim direct_mp_desc As mapping_point_descriptor
                                    Dim direct_mp_thread As mappingPointRoot
                                    If direct_session.Application(source_mapping_point.ApplicationId).haveMappingPoints Then
                                        direct_mp_desc = direct_session.Application(source_mapping_point.ApplicationId).ApplicationMappingPoints.getDescriptorByURL(url)
                                        If Not IsNothing(direct_mp_desc) Then
                                            If direct_mp_desc.haveActiveMappingPointThread Then
                                                direct_mp_thread = direct_mp_desc.MappingPoint
                                                destinationMappingPoints.Add(direct_mp_thread)
                                            Else
                                                GlobalObject.Msg(source_mapping_point.Info.FullMappingPointPath & " JSON delivery agent delay notice: message unable to be sent. mapping point is " & direct_mp_desc.URL & " offline.")
                                                If Me.getEventTable().event_delay(obj, url) = DELIVERY_DELAY_ACTION.RETRY Then
                                                    Thread.CurrentThread.Sleep(50)
                                                    GlobalObject.Msg(source_mapping_point.Info.FullMappingPointPath & " JSON delivery agent delay notice: delivery reattempt start.")
                                                    Return Me.Send(obj, to_mapping_point_urls, to_className)
                                                Else
                                                    GlobalObject.Msg(source_mapping_point.Info.FullMappingPointPath & " JSON delivery agent delay notice: message unable to be sent. message dropped.")
                                                End If
                                            End If
                                        Else
                                            GlobalObject.Msg(source_mapping_point.Info.FullMappingPointPath & " JSON delivery agent error:  " & Me.owner.getClassName & " specified incorrect destination. '" & url & "' is not a valid mapping point url.")
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                Next

            End If

            If IsNothing(obj.Sender) Then
                'add ourselves as sender
                obj.Sender = Me.getParent()
            End If

            Dim delivered_all As Boolean

            delivered_all = True

            For Each current_destination As mappingPointRoot In destinationMappingPoints
                Dim transport_return_value As Object
                Dim delivered As Boolean

                Debug.Print(obj.Directive)

                'prepare for transport
                Me.getEventTable().event_sending(obj, current_destination.getURI())


                'true on success, or JSOnMailJob on error.
                transport_return_value = JSONMailBoxServer.PassObject(obj, current_destination, to_className, disable_identical_sender_and_recipient)

                'send to server for headerization and passing

                If TypeOf transport_return_value Is Boolean Then
                    'sent
                    If CType(transport_return_value, Boolean) = True Then
                        delivered = True
                        Me.getEventTable().event_sent(obj, current_destination.getURI())
                    End If

                End If

                If GlobalObject.haveServerThread Then


                    If Not delivered Then
                        Dim header As JSONMailJob

                        If TypeOf transport_return_value Is JSONMailJob Then
                            header = CType(transport_return_value, JSONMailJob)
                            delivered_all = False

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

                    'trigger local error event.
                    Me.getEventTable().event_err(obj)
                End If

            Next

            Return delivered_all

        Catch ex As Exception
            Msg("OfferObject err: " & ex.Message & " @ " & ex.StackTrace.ToString)
            Me.getEventTable().event_err(obj)
        End Try

        Return False
    End Function

    'incoming events

    'incoming mail - called by mail server for delivery
    Public Sub OnIncomingDelivery(ByVal header As JSONMailJob)

        Dim attachment As JSONObject
        attachment = header.json_obj

        'centralized depostry
        ' Msg("MailBox incoming object '" & attachment.ObjectTypeName & "', received from " & attachment.Sender.getClassName())


        Dim retval As Boolean

        'çall user handler
        retval = Me.getEventTable().event_inbox(attachment, header.SenderMappingPointURL)

        'if the storage feature is on the return value indicates if the message is of interest and must be stored for later reference.

        If Me.getInboxEnabled() Then
            If retval = True Then
                Msg("OnIncomingDelivery: Storing message locally.")
                Me.getInbox().Add(attachment)
                Exit Sub
            End If
        End If

    End Sub

    'outgoing mail - replies to delivery status
    Public Sub OnFailedDelivery(ByVal header As JSONMailJob, ByVal destination_url As String)

        Dim attachment As JSONObject
        attachment = header.json_obj

        Msg("OnFailedDelivery triggered: unable to deliver object " & header.json_obj.ObjectTypeName)

        'çall user handler            
        Call Me.getEventTable().event_err(attachment)


    End Sub

    Public Sub OnDelivered(ByVal header As JSONMailJob, ByVal destination_url As String)

        Dim attachment As JSONObject
        attachment = header.json_obj

        'Msg("OnDelivered triggered: Class " & attachment.Sender.getClassName() & " successfully delivered object " & header.json_obj.ObjectTypeName)

        'notify JService by invoking user handle

        'çall user handler
        Call Me.getEventTable().event_delivered(attachment, header.SenderMappingPointURL)


    End Sub

End Class
