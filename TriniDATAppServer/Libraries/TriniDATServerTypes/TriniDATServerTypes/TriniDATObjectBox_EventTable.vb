Public Class TriniDATObjectBox_EventTable
    'message_obj = JSONObject
    Public Delegate Function OnUserInboxEventTemplate(ByRef message_obj As Object, ByVal from_url As String) As Boolean
    Public Delegate Sub OnUserSendingEventTemplate(ByRef message_obj As Object, ByVal destination_url As String)
    Public Delegate Sub OnUserSentEventTemplate(ByVal message_obj As Object, ByVal destination_url As String)
    Public Delegate Sub OUserDeliveryFailureEventTemplate(ByVal message_obj As Object)
    Public Delegate Sub OUserDeliveredCallbackEventTemplate(ByVal message_obj As Object, ByVal destination_url As String)

    Public Delegate Sub OnDeliveredTemplate(ByVal header As Object, ByVal destination_url As String) '=JSONMailJob
    Public Delegate Sub OnDeliveryErrTemplate(ByVal header As Object, ByVal destination_url As String)

    Public Delegate Function OUserDeliveryDelayEventTemplate(ByRef message_obj As Object, ByVal destination_url As String) As Boolean

    Public event_inbox As OnUserInboxEventTemplate
    Public event_sending As OnUserSendingEventTemplate
    Public event_sent As OnUserSentEventTemplate
    Public event_err As OUserDeliveryFailureEventTemplate
    Public event_delivered As OUserDeliveredCallbackEventTemplate
    Public event_delay As OUserDeliveryDelayEventTemplate

    Public Sub New()
        Me.setDefault()
    End Sub

    Public Sub setDefault()
        Me.event_inbox = AddressOf NullEventFunc
        Me.event_sending = AddressOf NullEventRef
        Me.event_sent = AddressOf NullEventSub
        Me.event_delivered = AddressOf NullEventSub
        Me.event_err = AddressOf NullEventByvalSub
        Me.event_delay = AddressOf NullDelayEventFunc
    End Sub
    Public Function haveInboxEventHandler() As Boolean
        Dim dummy As OnUserInboxEventTemplate
        dummy = AddressOf NullEventFunc
        Return Not (Me.event_inbox = dummy)
    End Function
    Public Function haveDelayEventHandler() As Boolean
        Dim dummy As OnUserInboxEventTemplate
        dummy = AddressOf NullDelayEventFunc
        Return Not (Me.event_delay = dummy)
    End Function
    Public Function haveFailureEventHandler() As Boolean
        Dim dummy As OnDeliveryErrTemplate
        dummy = AddressOf NullEventSub
        Return Not (Me.event_err = dummy)
    End Function
    Public Function haveSendingEventHandler() As Boolean
        Dim dummy As OnUserSendingEventTemplate
        dummy = AddressOf NullEventRef
        Return Not (Me.event_sending = dummy)
    End Function
    Public Function haveSentEventHandler() As Boolean
        Dim dummy As OnUserSentEventTemplate
        dummy = AddressOf NullEventSub
        Return Not (Me.event_sent = dummy)
    End Function
    Public Function haveDeliveredEventHandler() As Boolean
        Dim dummy As OUserDeliveredCallbackEventTemplate
        dummy = AddressOf NullEventSub
        Return Not (Me.event_delivered = dummy)
    End Function

    Public Sub NullEventByvalSub(ByVal msg As Object)

    End Sub
    Public Sub NullEventRef(ByRef obj As Object, ByVal destination_url As String)

    End Sub
    Public Sub NullEventSub(ByVal obj As Object, ByVal destination_url As String)

    End Sub
    Public Function NullEventFunc(ByRef obj As Object, ByVal destination_url As String) As Boolean

    End Function
    Public Function NullDelayEventFunc(ByRef obj As Object, ByVal destination_url As String) As DELIVERY_DELAY_ACTION
        Return DELIVERY_DELAY_ACTION.DROP
    End Function
End Class

Public Enum DELIVERY_DELAY_ACTION
    DROP = 0
    RETRY = 1
End Enum