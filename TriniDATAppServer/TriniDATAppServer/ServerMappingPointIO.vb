'Option Explicit On
'Imports System.Net.Sockets
'Imports System.Text


'Public Class ServerMappingPointIO
'    '    'Mapping point headers.
'    Private residential_mappings As mappingPointRoot()
'    '    Private mpcount As Integer
'    Private Server As TriniDATServer

'    '    'GLOBAL CONST
'    '    Public Shared Global_Instance As ServerMappingPointIO

'    '    Public Shared Function getGlobalInstance() As ServerMappingPointIO
'    '        Return ServerMappingPointIO.Global_Instance
'    '    End Function

'    '    Public Function getResidentialMappingPoints() As mappingPointRoot()
'    '        If Not IsNothing(residential_mappings) Then
'    '            Return residential_mappings
'    '        Else
'    '            Return Nothing
'    '        End If
'    '    End Function
'    Public Sub New(ByVal _TriniDATserver As TriniDATServer)
'        ReDim residential_mappings(0)
'        mpcount = 0
'        Me.Server = _TriniDATserver

'    End Sub
'    Public Function getServer() As TriniDATServer
'        Return Me.Server
'    End Function
'    Private Sub Msg(ByVal txt As String)
'        Me.Server.msg(txt)

'    End Sub

'    '    Public Function getCount() As Integer
'    '        Return Me.mpcount

'    '    End Function

'    '    Public Sub data2ProcessIOHandler(ByRef proc_io_handler As TriniDATClientConnectionManagerHTTP, ByVal packet As Byte(), ByVal packetSize As Long)
'    '        Call proc_io_handler.OnPacketReceived(packet, packetSize)
'    '    End Sub

'    '    Public Sub bootChain(ByVal mp_index As Integer)
'    '        '=====================================================================
'    '        'Creates a residential background chain and invokes nothing.
'    '        '=====================================================================

'    '        Dim JClassInfo As mappingPointClass
'    '        Dim JClassName As String
'    '        Dim CreateNewInstance As Boolean
'    '        Dim mappingPoint As mappingPointRoot

'    '        JClassName = "unknown"
'    '        mappingPoint = Me.getByIndex(mp_index)

'    '        '   mappingPoint.listAllProcesses()

'    '        For Each JClassInfo In mappingPoint.getClasses()

'    '            CreateNewInstance = False

'    '            ' 1.CREATE
'    '            Try
'    '                JClassName = JClassInfo.getName()

'    '                'running instances discovered?
'    '                CreateNewInstance = IsNothing(mappingPoint.getProcessByClassName(JClassName, True))

'    '                If CreateNewInstance Then
'    '                    Call launchResidential(JClassInfo, mappingPoint)
'    '                End If

'    '            Catch ex As Exception
'    '                Msg("bootChain: local error or exception occured in JClass constructor or doConfig routine. Err: " & ex.Message & "  Location: " & ex.StackTrace.ToString)
'    '                Exit Sub
'    '            End Try

'    '        Next



'    '    End Sub

'    '    Public Sub OnMappedDataIncoming(ByVal mp_index As Integer, ByRef connie As TriniDATSockets.TriniDATTCPSocket, ByVal packet() As Byte, ByVal packetSize As Long)
'    '        '=====================================================================
'    '        'Packet is in queue and we know the socket is connected to a mapping point.
'    '        'find out if running processes and pass data when found.
'    '        '
'    '        'Connie may or may not be linked to a process.
'    '        'Its irrelevant since the mapping is known, its only a matter of scanning the existing process list & class states.

'    '        'JClasses are not subject to the lifespan of sockets. Sockets are merely linked to existing processes.
'    '        '
'    '        'PARAMETER:
'    '        'mp_index = mapping point index aka the URL the socket is connected on.
'    '        '=====================================================================

'    '        Dim JClassInfo As mappingPointClass
'    '        Dim JClassName As String
'    '        Dim CreateNewInstance As Boolean
'    '        Dim mappingPoint As mappingPointRoot

'    '        JClassName = "unknown"
'    '        mappingPoint = Me.getByIndex(mp_index)

'    '        'provide no browser access to background mapping since they areo HTTP-less classes.

'    '        If mappingPoint.isResidential() Then
'    '            Msg("Notice: Background mapping point called by browser. Aborting request!")
'    '            'get rid of it.
'    '            connie.DisconnectNow()
'    '            Exit Sub
'    '        End If

'    '        '   mappingPoint.listAllProcesses()

'    '        Try
'    '            For Each JClassInfo In mappingPoint.getClasses()

'    '                CreateNewInstance = False

'    '                ' 1.CREATE
'    '                Try
'    '                    JClassName = JClassInfo.getName()

'    '                    'running instances discovered?
'    '                    CreateNewInstance = IsNothing(mappingPoint.getProcessByClassName(JClassName, True))

'    '                    If CreateNewInstance Then
'    '                        Call launchProcess(JClassInfo, mappingPoint, connie, packet, packetSize)
'    '                    Else

'    '                        'Update socket info for all chain residents BEFORE triggering their events.
'    '                        Dim proc_info As mappingPointInstanceInfo
'    '                        proc_info = mappingPoint.getProcessByClassName(JClassName, True)

'    '                        If Not IsNothing(proc_info) Then
'    '                            'pass URL parameters
'    '                            If connie.haveParameters() Then
'    '                                proc_info.getProtocolHandler().setParameters(connie.Parameters)
'    '                            Else
'    '                                'clear
'    '                                proc_info.getProtocolHandler().resetParameters()
'    '                            End If

'    '                            'update protocolhandler's internal socket connection to current.
'    '                            Call proc_info.getProtocolHandler().setConnection(connie)
'    '                        End If

'    '                    End If

'    '                Catch ex As Exception
'    '                    Msg("createOrPassDataByMapping local error or exception in JClass constructor or doConfig routine. Err: " & ex.Message & "  Location: " & ex.StackTrace.ToString)
'    '                    Exit Sub
'    '                End Try

'    '            Next

'    '            'Start raising events.

'    '            Dim invoke_index As Integer
'    '            invoke_index = 0

'    '            For Each JClassInfo In mappingPoint.getClasses()

'    '                'abort if socket is already handled.
'    '                If Not connie.isConnected() Or connie.disconnectSocket() = True Then
'    '                    Msg("OnMappedDataIncoming: Aborting. Socket is already closed.")
'    '                    Exit Sub
'    '                End If

'    '                invoke_index = invoke_index + 1
'    '                JClassName = JClassInfo.getName()

'    '                '2. PASS DATA
'    '                Msg("JLauncher: About to invoke running process #" & invoke_index.ToString & ": " & JClassName & " -> " & mappingPoint.URI)

'    '                Dim proc_info As mappingPointInstanceInfo
'    '                proc_info = mappingPoint.getProcessByClassName(JClassName, True)

'    '                If Not IsNothing(proc_info) Then

'    '                    'ensure socket becomes linked with this mapping poing & process.
'    '                    connie.linkProcessInstance(proc_info.getIndex())
'    '                    connie.linkToMapping(mappingPoint.getIndex())


'    '                    'execute attached protocolhandler's OnPacket event.
'    '                    Call data2ProcessIOHandler(proc_info.getProcess().GetIOHandler(), packet, packetSize)
'    '                Else
'    '                    Err.Raise(100, 0, "getProcessByClassName returned empty process descriptor for class " & JClassName & ". ")
'    '                End If

'    '            Next
'    '        Catch ex As Exception

'    '            Msg("OnMappedDataIncoming: Error while linking " & JClassName & " to  " & mappingPoint.URI)
'    '        End Try

'    '    End Sub

'    '    Private Function launchProcess(ByVal JClassInfo As mappingPointClass, ByRef mappingPoint As mappingPointRoot, ByRef connie As TriniDATSockets.TriniDATTCPSocket, ByVal packet As Byte(), ByVal packetSize As Long) As Boolean

'    '        'Dim jlauncher As JServiceLauncher
'    '        'Dim jprocess As JTriniDATWebService
'    '        'Dim process_index_at_mapping_point As Integer
'    '        'Dim http_method As String

'    '        'Try

'    '        '    'Create a new instance.
'    '        '    Msg("JLauncher: creating new instance of " & JClassInfo.getName() & " for URI " & mappingPoint.URI & "...")

'    '        '    process_index_at_mapping_point = -1
'    '        '    jlauncher = New JServiceLauncher(mappingPoint)

'    '        '    'instantiate the JClass and call its constructor.
'    '        '    'pass packet as byte array
'    '        '    jprocess = jlauncher.CreateJInstance(JClassInfo.getName(), mappingPoint.getIndex())

'    '        '    If Not IsNothing(jprocess) Then
'    '        '        Dim trigger_url As String

'    '        '        trigger_url = IIf(connie.haveOriginalURL(), connie.originalURL, mappingPoint.URI)

'    '        '        'add to mappingpointInfo collection entry
'    '        '        process_index_at_mapping_point = mappingPoint.AddInstance(JClassInfo, jprocess)
'    '        '        'report instance id to the created process itself
'    '        '        Dim process_info As mappingPointInstanceInfo
'    '        '        process_info = mappingPoint.getInstanceInfo(process_index_at_mapping_point)
'    '        '        process_info.getProcess().saveProcessIndex(process_index_at_mapping_point)

'    '        '        'add parameters, if any
'    '        '        http_method = TriniDATClientConnectionManagerHTTP.getURLPart("method", Encoding.ASCII.GetString(packet))
'    '        '        If http_method = "GET" Or http_method = "POST" Then
'    '        '            connie.tagIsGetRequest((http_method = "GET"))

'    '        '            'attach startup parameters to process from URL
'    '        '            If connie.haveParameters() Then
'    '        '                process_info.setParameters(connie.Parameters)
'    '        '            End If

'    '        '            process_info.setTriggerURI(trigger_url)
'    '        '        Else
'    '        '            connie.tagIsHTTP(False)
'    '        '        End If

'    '        '        'Update connection with new info
'    '        '        connie.linkToMapping(mappingPoint.getIndex())
'    '        '        connie.linkProcessInstance(process_index_at_mapping_point)

'    '        '        'add protocol handler
'    '        '        process_info.setProtocolHandler(New TriniDATClientConnectionManagerHTTP(connie))
'    '        '        process_info.getProtocolHandler().setErrorHandler(AddressOf getServer().OnInvalidPacket)

'    '        '        Call process_info.getProcess().DoConfigure()

'    '        '    Else
'    '        '        Err.Raise(100, Nothing, "There was an error initializing a new " & JClassInfo.getName() & " instance.")
'    '        '    End If

'    '        'Catch ex As Exception
'    '        '    Msg("launchProcess: error creating JClass or class itself generated exception: " & ex.Message & "  where:" & ex.StackTrace.ToString)
'    '        '    Return False
'    '        'End Try

'    '        'Msg("Register complete for new listener " & mappingPoint.URI & " -> " & JClassInfo.getName() & " (process id: " & (process_index_at_mapping_point).ToString & "  count: " & mappingPoint.getProcessCount().ToString & ").")

'    '        Return True
'    '    End Function
'    '    Private Function launchResidential(ByVal JClassInfo As mappingPointClass, ByRef mappingPoint As mappingPointRoot) As Boolean

'    '        'Dim jlauncher As JServiceLauncher
'    '        'Dim jprocess As JTriniDATWebService
'    '        'Dim process_index_at_mapping_point As Integer

'    '        'Try

'    '        '    'Create a new instance.
'    '        '    Msg("JLauncher: creating new instance of " & JClassInfo.getName() & " for URI " & mappingPoint.URI & "...")

'    '        '    process_index_at_mapping_point = -1
'    '        '    jlauncher = New JServiceLauncher(mappingPoint)

'    '        '    'instantiate the JClass and call its constructor.
'    '        '    'pass packet as byte array
'    '        '    jprocess = jlauncher.CreateJInstance(JClassInfo.getName(), mappingPoint.getIndex())

'    '        '    If Not IsNothing(jprocess) Then
'    '        '        'add to mappingpointInfo collection entry
'    '        '        process_index_at_mapping_point = mappingPoint.AddInstance(JClassInfo, jprocess)
'    '        '        'report instance id to the created process itself
'    '        '        Dim process_info As mappingPointInstanceInfo
'    '        '        process_info = mappingPoint.getInstanceInfo(process_index_at_mapping_point)
'    '        '        process_info.getProcess().saveProcessIndex(process_index_at_mapping_point)

'    '        '        'empty protocol handler
'    '        '        process_info.setProtocolHandler(Nothing)
'    '        '        process_info.setTriggerURI(mappingPoint.URI)

'    '        '        'let the instance configure itself.
'    '        '        Call process_info.getProcess().DoConfigure()

'    '        '    Else
'    '        '        Err.Raise(100, Nothing, "There was an error initializing a new " & JClassInfo.getName() & " background process.")
'    '        '    End If

'    '        'Catch ex As Exception
'    '        '    Msg("launchProcess: error creating JClass or class itself generated exception: " & ex.Message & "  where:" & ex.StackTrace.ToString)
'    '        '    Return False
'    '        'End Try

'    '        'Msg("Register complete for new listener " & mappingPoint.URI & " -> " & JClassInfo.getName() & " (process id: " & (process_index_at_mapping_point).ToString & "  count: " & mappingPoint.getProcessCount().ToString & ").")

'    '        'Return True
'    '    End Function


'    'Public Function registerMapping(ByVal mapping As mappingPointRoot) As Boolean
'    '    'map Form to /<uri>
'    '    'this is either called by frmMain.startServer or an app who wishes to specify his namespace.
'    '    Dim tmp As String = ""

'    '    'URI MUST end with / 
'    '    If Right(mapping.URI, 1) <> "/" Then
'    '        mapping.URI = mapping.URI & "/"
'    '    End If

'    '    If mappingPointExists(mapping.URI) Then
'    '        Msg("registerMapping Error: Mapping point already exists: " & mapping.URI)
'    '        Return False
'    '    End If

'    '    'verbose
'    '    Msg("Registering new mapping points for url '" & mapping.URI & "...")

'    '    Call Me.Add(mapping)

'    '    Return True

'    'End Function
'    '    Private Sub Add(ByVal mp_info As mappingPointRoot)
'    '        Dim x As Integer

'    '        x = getCount()
'    '        ReDim Preserve mappingPoints(x)
'    '        Me.mappingPoints(x) = mp_info
'    '        Me.mappingPoints(x).setIndex(x)
'    '        Me.mpcount = Me.mpcount + 1

'    '        Msg("Registered new mapping point: '" & mp_info.URI & "'")

'    '    End Sub
'    '    Public Function mappingPointExists(ByVal URI As String) As Boolean
'    '        Dim mp As mappingPointRoot


'    '        For Each mp In Me.mappingPoints

'    '            If Not IsNothing(mp) Then
'    '                If mp.URI = URI Then
'    '                    Return True
'    '                End If
'    '            End If
'    '        Next

'    '        Return False

'    '    End Function

'    '    Public Function getMappingPointByURI(ByVal URLPart As String) As mappingPointRoot
'    '        Dim mp As mappingPointRoot
'    '        Dim temp As String
'    '        Dim x As Integer

'    '        'TODO:return array

'    '        Try

'    '            temp = URLPart.ToLower

'    '            For x = 0 To Me.mpcount - 1
'    '                mp = Me.getByIndex(x)

'    '                If Not IsNothing(mp) Then
'    '                    If mp.URI.ToLower = temp Then
'    '                        'TODO:return array
'    '                        Return mp
'    '                    End If

'    '                End If
'    '            Next x


'    '        Catch ex As Exception
'    '            Msg("ServerMappingPointIO.getMappingPointByURI error: " & ex.Message)
'    '        End Try

'    '        Return Nothing
'    '    End Function

'    '    Public Function indexExists(ByVal index As Integer) As Boolean
'    '        Return (index < Me.mpcount And index > -1)
'    '    End Function
'    '    Public Function getByIndex(ByVal index As Integer) As mappingPointRoot

'    '        Try

'    '            If indexExists(index) Then
'    '                If Not IsNothing(mappingPoints(index)) Then
'    '                    Return mappingPoints(index)
'    '                Else
'    '                    Err.Raise(100, 0, "Mapping point is empty.")
'    '                End If

'    '            Else
'    '                Err.Raise(100, 0, "Mapping point index exceeds total count.")
'    '            End If
'    '        Catch ex As Exception
'    '            Msg("getByIndex: invalid index specified: #" & index.ToString & " additional info: " & ex.Message)
'    '            Return Nothing
'    '        End Try

'    '    End Function

'    '    Public Function getMappingProcessCount(ByVal index As Integer) As Integer

'    '        Try

'    '            If indexExists(index) Then
'    '                If Not IsNothing(mappingPoints(index)) Then
'    '                    Return mappingPoints(index).getProcessCount()
'    '                Else
'    '                    Err.Raise(100, 0, "Mapping point is empty.")
'    '                End If

'    '            Else
'    '                Err.Raise(100, 0, "Mapping point index exceeds total count.")
'    '            End If
'    '        Catch ex As Exception
'    '            Msg("getByIndex: invalid index specified: #" & index.ToString & " additional info: " & ex.Message)
'    '            Return -1
'    '        End Try

'    '        Return -1
'    '    End Function

'End Class













