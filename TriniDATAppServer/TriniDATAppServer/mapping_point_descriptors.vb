Option Compare Text
Option Explicit On

Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Threading


'Public Class BosswaveApplicationHost
'    'lists all available mapping point
'    'urls + classnames only
'    Private Shared current_application As BosswaveApplication = Nothing


'    Public Shared Function getDirectory() As mapping_point_descriptors
'        If BosswaveApplicationHost.ApplicationPresent Then
'            Return BosswaveApplicationHost.HostedApplication.ApplicationMappingPoints
'        End If

'        Return Nothing
'    End Function
'    Public Shared ReadOnly Property ApplicationPresent As Boolean
'        Get
'            Return Not IsNothing(BosswaveApplicationHost.current_application)
'        End Get
'    End Property

'    Public Shared Property HostedApplication As BosswaveApplication
'        Get
'            Return BosswaveApplicationHost.current_application
'        End Get
'        Set(ByVal value As BosswaveApplication)
'            BosswaveApplicationHost.current_application = value
'            If Not IsNothing(BosswaveApplicationHost.current_application) Then
'                If Not BosswaveApplicationHost.current_application.Initiailized Then
'                    Try
'                        BosswaveApplicationHost.current_application.Load()

'                        For Each mp_desc In BosswaveApplicationHost.HostedApplication.ApplicationMappingPoints
'                            Msg("Mapping point is online: " & mp_desc.MappingPoint.getURI())

'                            If mp_desc.MappingPoint.Description <> "" Then
'                                Msg(mp_desc.MappingPoint.getURI() & " info: " & mp_desc.MappingPoint.Description)
'                            End If
'                        Next

'                    Catch ex As Exception
'                        Msg("HostedApplication.Load() err: " & ex.Message)
'                        BosswaveApplicationHost.current_application = Nothing
'                    End Try
'                End If
'            End If
'        End Set
'    End Property

'    Public Shared Function getbyClassName(ByVal friendly_classname As String) As mappingPointRoot
'        Dim mp As mapping_point_descriptor
'        Dim x As Integer

'        'TODO:return array

'        Try

'            For Each mp In BosswaveApplicationHost.getDirectory()

'                If Not IsNothing(mp) Then
'                    For Each mp_class As mappingPointClass In mp.MappingPoint.getClasses()

'                        If JServiceLauncher.getClassFriendlyName(mp_class.getName()) = friendly_classname Then
'                            Return mp.MappingPoint
'                        End If

'                    Next
'                End If

'            Next

'            Return Nothing

'        Catch ex As Exception
'            Msg("error: " & ex.Message)
'        End Try

'        Return Nothing
'    End Function
'    Public Shared Function getByURI(ByVal URLPart As String) As mappingPointRoot

'        Return BosswaveApplicationHost.getDirectory().getMappingPointByURL(URLPart)

'    End Function
'    Private Shared Sub Msg(ByVal txt As String)
'        txt = "App Initialized: " & txt
'        Debug.Print(txt)
'        GlobalObject.Msg(txt)
'    End Sub

'    Public Sub New()

'    End Sub
'End Class

Public Class mapping_point_descriptors
    Inherits Collection(Of mapping_point_descriptor)

    'Public Overloads Sub Add(ByVal val As mappingPointRoot)
    '    Dim retval As mapping_point_descriptor
    '    retval = New mapping_point_descriptor
    '    retval.MappingPoint = val
    '    MyBase.Add(retval)
    'End Sub

    Public ReadOnly Property haveDescriptors As Boolean
        Get
            Return Me.Count > 0
        End Get
    End Property
    Public Function getDescriptorIndexByURI(ByVal val As String) As Long
        Dim mp As mapping_point_descriptor
        Dim x As Long

        For x = 0 To Me.Count - 1
            mp = Me.Items(x)

            If Not IsNothing(mp) Then
                If mp.URL = val Then
                    Return x
                End If
            End If
        Next

        Return -1
    End Function

    Public Function getDescriptorByURL(ByVal val As String) As mapping_point_descriptor

        For Each mp_description In Me
            If mp_description.URL = val Then
                Return mp_description
            End If
        Next
        Return Nothing
    End Function

    Public Function getMappingPointByURL(ByVal val As String) As mappingPointRoot
        Dim retval As mapping_point_descriptor

        retval = Me.getDescriptorByURL(val)

        If Not IsNothing(retval) Then
            Return retval.MappingPoint
        End If

        Return Nothing

    End Function

End Class

Public Class payment_descriptor
    Public gatewayid As Long 'regulates currency/method etc
    Public handle As String 'e.g B3028294767E2D3BBBA68D0EFB10FC70
    '  Public affiliateid As Long

    Public Function getAsNode() As XElement
        Return XElement.Parse("<paymentdata><paymenthandle>" & Me.handle & "</paymenthandle></paymentdata>")
    End Function
    Public Shared Function createFromXPayment(ByVal mypaymentinfo As XElement)

        If IsNothing(mypaymentinfo) Then Return Nothing

        Dim retval As payment_descriptor
        Dim temp As XElement

        retval = New payment_descriptor

        'temp = mypaymentinfo.Descendants("paymentgateway")(0)
        'If Not IsNothing(temp) Then
        '    If IsNumeric(temp.Value) Then
        '        retval.gatewayid = CLng(temp.Value)
        '    End If
        'End If

        temp = mypaymentinfo.Descendants("paymenthandle")(0)
        If Not IsNothing(temp) Then
            retval.handle = temp.Value
        End If

        'temp = mypaymentinfo.Descendants("paymentaffiliatehandle")(0)
        'If Not IsNothing(temp) Then
        '    If IsNumeric(temp.Value) Then
        '        retval.affiliateid = CLng(temp.Value)
        '    End If
        'End If

        Return retval
    End Function
End Class

Public Class mapping_point_descriptor
    Private parent_applicationid As Long
    Private mapping_point As mappingPointRoot
    Private MappingPointThread As Threading.Thread
    Private waitThread As Threading.Thread
    Private must_override_interface_flag As Boolean
    Public boot_milliseconds As List(Of Long)
    Private Const DEFAULT_THREAD_DELAYSEC As Integer = 1
    Private DEFAULT_THREAD_LIFETIME_TOLERANCESEC As Integer
    Private Const DEFAULT_THREAD_DELAYMSEC As Long = 250
    'only for conveniant 's sake that mapping point url is repeated here
    Private cached_url As String
    Private last_thread_killed As Boolean
    Public jomega_buffer As Boolean
    Private xnode As XElement
    Private Const DEFAULT_ID_VALUE As String = ""

    Public ReadOnly Property haveUserId As Boolean
        Get
            Return Me.UserId <> mapping_point_descriptor.DEFAULT_ID_VALUE
        End Get
    End Property
    Public ReadOnly Property UserId As String
        Get

            If Not Me.haveNode Then
                Return DEFAULT_ID_VALUE
            End If

            If Not IsNothing(Me.Node.@id) Then
                Return Me.Node.@id.ToString
            Else
                Return DEFAULT_ID_VALUE
            End If

        End Get
    End Property
    Public ReadOnly Property haveNode As Boolean
        Get
            Return Not IsNothing(Me.Node)
        End Get
    End Property
    Public Property Node As XElement
        Get
            Return Me.xnode
        End Get
        Set(ByVal value As XElement)
            Me.xnode = value
        End Set
    End Property
    Public ReadOnly Property wasKilled As Boolean
        Get
            Return Me.last_thread_killed
        End Get
    End Property
    Public Property Killed As Boolean
        Get
            Return Me.last_thread_killed
        End Get
        Set(ByVal value As Boolean)
            Me.last_thread_killed = value
        End Set
    End Property
    Public Sub New(ByVal parentappid As Long, ByVal _definition_node As XElement)
        Me.MappingPointThread = Nothing
        Me.mapping_point = Nothing
        Me.waitThread = Nothing
        Me.boot_milliseconds = New List(Of Long)
        Me.ApplicationID = parentappid
        Me.Killed = False
        Me.Node = _definition_node
    End Sub
    Public Property MustOverrideFlag As Boolean
        Get
            Return Me.must_override_interface_flag
        End Get
        Set(ByVal value As Boolean)
            Me.must_override_interface_flag = value
        End Set
    End Property

    Public Property ApplicationID As Long
        Get
            Return Me.parent_applicationid
        End Get
        Set(ByVal value As Long)
            Me.parent_applicationid = value
        End Set
    End Property


    Public Property URL As String
        Get
            Return Me.cached_url
        End Get
        Set(ByVal value As String)
            Me.cached_url = value
        End Set
    End Property
    Public Property ThreadToleranceSec As Integer
        Get
            Return Me.DEFAULT_THREAD_LIFETIME_TOLERANCESEC
        End Get
        Set(ByVal value As Integer)
            Me.DEFAULT_THREAD_LIFETIME_TOLERANCESEC = value
        End Set
    End Property

    Public Shared Function getWaitThreadInterval(ByVal mp_des As mapping_point_descriptor) As Long
        Dim avg_delay_ms As Long

        avg_delay_ms = mp_des.getAveragetimeMillisecond

        If avg_delay_ms > 1000 Then
            'get fastest recorded instance
            Return mp_des.getFastestRuntime()
        ElseIf avg_delay_ms > 0 Then
            'wait time / 2
            Return avg_delay_ms / 2
        Else
            Return mapping_point_descriptor.DEFAULT_THREAD_DELAYMSEC
        End If
    End Function
    Private ReadOnly Property getFastestRuntime() As Long
        Get
            Dim fast_record_ms As Long
            fast_record_ms = mapping_point_descriptor.DEFAULT_THREAD_DELAYMSEC

            For Each runtime_ms As Long In Me.boot_milliseconds
                If runtime_ms < fast_record_ms And fast_record_ms > 0 Then
                    fast_record_ms = runtime_ms
                End If
                'future: if record count > high number, do sometihng about it
            Next

            Return fast_record_ms

        End Get
    End Property
    Public Shared Sub CPUWait(ByVal startup_info As mapping_point_threading_parameters)


        If Not startup_info.bootInfo.direct_socket.isConnected() Then
            'abort
            GoTo ABORT_THREAD
        End If


        'queue all vars
        Dim old_deps As List(Of String)
        Dim old_classes As List(Of mappingPointClass)
        Dim old_init As Type
        Dim old_encoding As System.Text.Encoding
        Dim mp_descriptor As mapping_point_descriptor

        mp_descriptor = startup_info.bootInfo.mapping_point_desc

        If IsNothing(mp_descriptor) Then
            'fatal error
            GoTo ABORT_THREAD
        End If

        old_deps = mp_descriptor.MappingPoint.getDependencyPaths()
        old_classes = mp_descriptor.MappingPoint.getClasses()
        old_init = mp_descriptor.MappingPoint.Initializor
        old_encoding = mp_descriptor.MappingPoint.EncodingPreference


        'ENTER WAIT STATE
        Dim wait_delay_ms As Long

        wait_delay_ms = mapping_point_descriptor.getWaitThreadInterval(mp_descriptor)

        'wait until thread is finished
        Do Until mp_descriptor.Killed Or Not mp_descriptor.haveActiveMappingPointThread Or GlobalObject.serverState <> BosswaveServerState.ONLINE
            'THREAD ALREADY RUNNING
            Call Thread.Sleep(wait_delay_ms)
        Loop

        'INIT
        'lineage mode: whatever has been inserted into the EntryPointMappingPointDir is continiously copied from mapping point to mapping point.
        If mp_descriptor.Killed Then
            GlobalObject.Msg(mp_descriptor.URL & ": Starting due to previous kill.")
            mp_descriptor.Killed = False
        End If

        mp_descriptor.MappingPoint = New mappingPointRoot(startup_info.bootInfo, old_classes, old_encoding, old_init, old_deps)
        mp_descriptor.MappingPoint.setURI(mp_descriptor.URL)

        'FIRE
        mp_descriptor.MappingPointThread = Nothing
        mp_descriptor.MappingPoint.bootPacket = startup_info.bootPacket
        mp_descriptor.MappingPoint.bootPacketsize = startup_info.bootPacketsize
        mp_descriptor.MappingPointThread = New Thread(AddressOf mp_descriptor.MappingPoint.bootHTTPChain)

        If GlobalObject.serverState <> BosswaveServerState.ONLINE Then
            GoTo ABORT_THREAD
        End If


        mp_descriptor.MappingPointThread.Start()

        ' GlobalObject.Msg(mp_descriptor.MappingPoint.getURI() & ": Fired latent thread.")

ABORT_THREAD:
        If startup_info.multi_threaded_waitthread Then
            Try
                Threading.Thread.CurrentThread.Abort()
            Catch ex As Exception

            End Try

            Exit Sub
        End If
    End Sub
    Public Property MappingPoint As mappingPointRoot
        Get
            Return Me.mapping_point
        End Get
        Set(ByVal value As mappingPointRoot)
            Me.mapping_point = value
        End Set
    End Property
    Public Property RunningThread As Threading.Thread
        Get
            Return Me.MappingPointThread
        End Get
        Set(ByVal value As Threading.Thread)
            Me.MappingPointThread = value
        End Set
    End Property
    Public Property currentWaitThread As Threading.Thread
        Get
            Return Me.waitThread
        End Get
        Set(ByVal value As Threading.Thread)
            Me.waitThread = value
        End Set
    End Property
    Public Function AddRuntimeRecord(ByVal end_time_milliseconds As Long) As Long
        Try
 
            Dim thread_runtime_record_ms As Long

            thread_runtime_record_ms = Math.Abs(end_time_milliseconds - Me.MappingPoint.boot_time_start_ms)
            Me.boot_milliseconds.Add(thread_runtime_record_ms)

            Return thread_runtime_record_ms
        Catch ex As Exception
            Return -1
        End Try
    End Function
    Public ReadOnly Property haveMappingPointInstance As Boolean
        Get
            Return Not IsNothing(Me.MappingPoint)
        End Get
    End Property
    Public ReadOnly Property KillActiveThread() As Boolean
        Get
            Try
                GlobalObject.Msg(Me.MappingPoint.getURI() & " killed.")

                If Me.haveActiveMappingPointThread() Then
                    TriniDATServerEvent.OnMappingPointStopped(Me.MappingPoint, False)
                    Me.Killed = True
                    Return True
                Else
                    Return False
                End If

            Catch ex As Exception

            End Try
        End Get

    End Property

    Public ReadOnly Property shouldKillActiveThread() As Boolean
        Get
            Dim threadBootTime As Long
            Dim currentTime As Long
            Dim thread_seconds_active As Long

            If Not Me.haveActiveMappingPointThread() Then Return False
            threadBootTime = Me.MappingPoint.boot_time_start_ms
            currentTime = GlobalObject.GetTickCount()

            thread_seconds_active = currentTime - threadBootTime

            If thread_seconds_active < 1000 Then Return False

            thread_seconds_active = CLng(thread_seconds_active / 1000)

            If thread_seconds_active < 2 Then Return False

            'calculate amount of time this thread is running
            Dim experience_count As Long
            Dim tolerance_level_sec As Integer
            Dim avg_ms As Long

            experience_count = Me.boot_milliseconds.Count - 1
            tolerance_level_sec = 0

            If experience_count > 9 Then
                'draw tolerance level from avg * 2
                avg_ms = Me.getAveragetimeMillisecond

                If avg_ms > 1000 Then
                    tolerance_level_sec = CInt(avg_ms / 1000)
                Else
                    'this is a fast thread.
                    'set 1 second average
                    tolerance_level_sec = 1
                End If

                tolerance_level_sec = tolerance_level_sec * 2
            Else
                tolerance_level_sec = Me.ThreadToleranceSec
            End If

            If tolerance_level_sec < Me.ThreadToleranceSec Then
                'because threads are executed as stacked, even fast threads in the background have to wait.
                tolerance_level_sec = Me.ThreadToleranceSec
            End If

            If thread_seconds_active >= tolerance_level_sec Then
                GlobalObject.Msg("========THREAD INFO====================")
                GlobalObject.Msg(Me.MappingPoint.getURI() & " about to be killed. Tolerance level of " & tolerance_level_sec.ToString & " sec. exceeded.")
                Return True
            Else
                GlobalObject.Msg("========THREAD INFO====================")
                GlobalObject.Msg(Me.MappingPoint.getURI() & ": Expected TTL for thread: " & (tolerance_level_sec - thread_seconds_active).ToString & " second(s). Tolerance level is " & tolerance_level_sec.ToString & " second(s).")
                Return False
            End If

        End Get
    End Property
    Public ReadOnly Property haveActiveWaitThread() As Boolean
        Get
            Dim retval
            retval = ReturnThreadActiveOrNothing(Me.waitThread)
            If IsNothing(retval) Then Return False
            Return CBool(retval)
        End Get
    End Property
    Public ReadOnly Property haveActiveMappingPointThread() As Boolean
        Get
            Dim retval
            retval = ReturnThreadActiveOrNothing(Me.MappingPointThread)
            If IsNothing(retval) Then Return False
            Return CBool(retval)
        End Get
    End Property
    Private ReadOnly Property ReturnThreadActiveOrNothing(ByVal thrd As Thread) As Boolean
        Get
            If IsNothing(thrd) Then
                Return Nothing
            Else
                Return Me.ThreadIsActive(thrd)
            End If
        End Get
    End Property
    Public ReadOnly Property ThreadIsActive(ByVal thrd As Thread) As Boolean
        Get
            Return (thrd.ThreadState = ThreadState.Background Or thrd.ThreadState = ThreadState.Running)
        End Get
    End Property

    Public ReadOnly Property getAveragetimeSeconds As Long
        Get
            Dim total_time As Long

            total_time = Me.getAveragetimeMillisecond
            If total_time > 0 Then
                total_time /= 1000
            End If


            Return total_time
        End Get

    End Property
    Public ReadOnly Property getAveragetimeMillisecond As Long
        Get
            Dim total_runtime_ms As Long
            Dim total_runs As Long
            Dim retval As Long

            total_runtime_ms = 0
            total_runs = 0

            For Each runtime_record_msec In Me.boot_milliseconds
                total_runtime_ms += runtime_record_msec
                total_runs += 1
            Next

            If total_runtime_ms > 0 And total_runs > 0 Then
                retval = CLng(total_runtime_ms / total_runs)
            Else
                'add default expected runtime
                retval = mapping_point_descriptor.DEFAULT_THREAD_DELAYMSEC
            End If

            Return retval
        End Get

    End Property
End Class

Public Class mapping_point_threading_parameters
    'Public mp_descriptor_index As Integer
  
    Public bootInfo As TriniDATRequestInfo
    Public bootPacket As Byte()
    Public bootPacketsize As Long
    Public multi_threaded_waitthread As Boolean
    'Public ReadOnly Property getMappingPointDescriptorBySessionId() As mapping_point_descriptor
    '    Get
    '        Return BosswaveApplicationHost.getDirectory().Item(Me.mp_descriptor_index)
    '    End Get
    'End Property


End Class