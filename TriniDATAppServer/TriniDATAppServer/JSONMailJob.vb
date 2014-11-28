Imports TriniDATServerTypes

Public Class JSONMailJob
    'encapsulates a JSON object for passing to JSONMailBoxServer.

    Private callback_ondelivered As TriniDATObjectBox_EventTable.OnDeliveredTemplate
    Private callback_ondelivery_err As TriniDATObjectBox_EventTable.OnDeliveryErrTemplate

    Public json_obj As JSONObject

    Public Shared global_delivery_success As Integer
    Public Shared global_delivery_fail As Integer

    Public errmsg As String = ""
    Private state As JSONMailJob_DELIVERY_STATE
    Public sender_URL As String
    Public targetURL As Uri
    Public targetProcess As JTriniDATWebService
    Public targetMappingPoint As mappingPointRoot
    Public direction As JSONMailJob_DIRECTION
    Private islogged As Boolean
    Public Property Logged As Boolean
        Get
            Return Me.islogged
        End Get
        Set(ByVal value As Boolean)
            Me.islogged = value
        End Set
    End Property
    Public Property SenderMappingPointURL As String
        Get
            Return Me.sender_URL
        End Get
        Set(ByVal value As String)
            Me.sender_URL = value
        End Set
    End Property
    Public Function sendtoTrafficLog()
        Me.Logged = False
        Try
            TrafficMonitor.LogJSONTraffic(Me)
            Me.Logged = True
            Return Me.Logged
        Catch ex As Exception
            Me.DeliveryState = JSONMailJob_DELIVERY_STATE.ERR_UNKNOWN
            Me.DeliveryStateMessage = "Internal Error: " & ex.Message
        End Try

        Return Me.Logged

    End Function
    Public Property DeliveryState As JSONMailJob_DELIVERY_STATE
        Get
            Return Me.state
        End Get
        Set(ByVal value As JSONMailJob_DELIVERY_STATE)
            Me.state = value

            If Not IsNothing(value) Then
                If value = JSONMailJob_DELIVERY_STATE.SENT Then
                    Me.DeliveryStateMessage = "Delivered"
                ElseIf value = JSONMailJob_DELIVERY_STATE.DRAFT Then
                    Me.DeliveryStateMessage = "Undelivered"
                ElseIf value = JSONMailJob_DELIVERY_STATE.RECIPIENT_ERROR_NOT_FOUND Then
                    Me.DeliveryStateMessage = "Recipient not found"
                End If
            End If
        End Set
    End Property
    Public Sub setDeliverEvents(ByVal onsuccess As TriniDATObjectBox_EventTable.OnDeliveredTemplate, ByVal onerr As TriniDATObjectBox_EventTable.OnDeliveryErrTemplate)
        Me.callback_ondelivered = onsuccess
        Me.callback_ondelivery_err = onerr
    End Sub

    Public ReadOnly Property DirectionStr
        Get
            Select Case Me.direction
                Case JSONMailJob_DIRECTION.IN_HTTP
                    Return "HTTP In"

                Case JSONMailJob_DIRECTION.IN_INTENAL
                    Return "In"

                Case JSONMailJob_DIRECTION.OUT_HTTP
                    Return "HTTP Out"

                Case JSONMailJob_DIRECTION.OUT_INTERNAL
                    Return "Out"

            End Select

            Return "errx"
        End Get
    End Property


    Public Sub attemptJSONDelivery()
        'threaded func.
        'contact the target JService's mailbox
        'and trigger user handler if set

        Try
            ' MSG("attemptDelivery: Delivery Thread Started.")

            If Not Me.haveTargetMP Then
                Throw New Exception("Header: Delivery err. No target MP specified.")
            End If

            If Not Me.haveTargetProcess Then
                Throw New Exception("Header: Delivery err. No target process specified.")
            End If


            'start delivery
            Me.direction = JSONMailJob_DIRECTION.OUT_INTERNAL
         
            Me.targetProcess.getMailProvider().OnIncomingDelivery(Me)
            Me.DeliveryState = JSONMailJob_DELIVERY_STATE.SENT

            If Me.haveSuccessEventHandler Then
                '   MSG("attemptDelivery: Delivery OK. Calling OnDelivered callback.")
                'trigger delivery sucess event
                'System.Threading.Thread.Sleep(300)
                callback_ondelivered.Invoke(Me, Me.SenderMappingPointURL)
            End If

        Catch ex As Exception
            If Me.haveErrEventHandler Then
                'NOTE: in non-threaded mode this is triggered when the mail* functions of the target class generates ANY exception.
                'trigger delivery failure event.
                Me.DeliveryState = JSONMailJob_DELIVERY_STATE.ERR_UNKNOWN
                Me.DeliveryState = ex.Message
                callback_ondelivery_err.Invoke(Me, Me.SenderMappingPointURL)
                Exit Sub
            End If
        End Try

        If GlobalObject.haveServerThread Then
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                Me.sendtoTrafficLog()
                'log
            End If
        End If

    End Sub
    Protected Sub MSG(ByVal txt As String)
        Debug.Print("JSONMailJob " & txt)
    End Sub

    Public Sub OnErr()
        'trigger user err handler if set
    End Sub

    Public Sub setTargetProcess(ByVal tp As JTriniDATWebService)
        Me.targetProcess = tp
    End Sub

    Public Sub setTargetMP(ByVal mp As mappingPointRoot)
        Me.targetMappingPoint = mp
    End Sub

    Public Sub setErrorMsg(ByVal e As String)
        Me.errmsg = e
    End Sub
    Public Property DeliveryStateMessage As String
        Get
            Return Me.errmsg
        End Get
        Set(ByVal value As String)
            Me.errmsg = value
        End Set
    End Property
    Public ReadOnly Property haveTargetMP() As Boolean
        Get
            Return Not IsNothing(targetMappingPoint)
        End Get
    End Property
    Public ReadOnly Property haveTargetProcess() As Boolean
        Get
            Return Not IsNothing(targetProcess)
        End Get
    End Property

    Public ReadOnly Property haveSuccessEventHandler() As Boolean
        Get
            Return Not IsNothing(callback_ondelivered)
        End Get
    End Property

    Public ReadOnly Property haveErrEventHandler() As Boolean
        Get
            Return Not IsNothing(callback_ondelivery_err)
        End Get
    End Property

    Public Sub New()
        Me.DeliveryState = JSONMailJob_DELIVERY_STATE.DRAFT
    End Sub
End Class

Public Enum JSONMailJob_DIRECTION
    IN_INTENAL = 1 'from another class
    IN_HTTP = 2
    OUT_INTERNAL = 3 'To another class
    OUT_HTTP = 4
End Enum

Public Enum JSONMailJob_DELIVERY_STATE
    DRAFT = 0
    SENT = 1
    RECIPIENT_ERROR_NOT_FOUND = 2
    ERR_UNKNOWN = 3
End Enum