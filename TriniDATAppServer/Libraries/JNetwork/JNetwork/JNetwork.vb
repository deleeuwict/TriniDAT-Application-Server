Imports System.Collections.Specialized
Imports System.Net
Imports System.Net.Sockets
Imports System.Net.NetworkInformation.IPAddressInformation
Imports System.Net.NetworkInformation
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class JNetwork
    Inherits JTriniDATWebService

    '
    Private adapters() As NetworkInterface

    Public Sub New()
        MyBase.New()
        ReDim adapters(0)

    End Sub


    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean

        Dim myip_function As TriniDAT_ServerGETFunction

        myip_function = New TriniDAT_ServerGETFunction(AddressOf Me.GetIPAddressRouted)
        myip_function.FunctionURL = Me.makeRelative("getip")

        'add to webservice function table.
        http_function_table.AddGET(myip_function, True)

        Return True
    End Function
  


    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        getIOHandler().Configure(http_events)

        Me.adapters = NetworkInterface.GetAllNetworkInterfaces()

        Return True

    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
        End If

        Return False
    End Function


    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        If IsNothing(HTTP_URI_Parameters) Then
            Me.getIOHandler().addOutput("no parameters.")
            Exit Sub
        End If

        Dim mp_root As String
        Dim service_dns_lookup As String
        Dim service_getip As String
        Dim service_adapter_all As String
        Dim service_adapter_dns As String
        Dim service_adapter_name As String
        Dim service_adapter_ipaddress As String
        Dim service_adapter_gateway As String
        Dim service_adapter_type As String
        Dim service_adapter_all_ethernet As String
        Dim service_adapter_all_wireless As String

        Dim pos As Integer

        mp_root = Me.getProcessDescriptor().getParent().getURI()
        service_dns_lookup = mp_root & "iplookup"
        service_adapter_all = mp_root & "adapters"
        service_adapter_all_ethernet = mp_root & "adapters/ethernet"
        service_adapter_all_wireless = mp_root & "adapters/wireless"
        service_getip = mp_root & "ipaddress"

        If HTTP_URI_Path = service_getip Then
            Me.GetIPAddress()
            Me.getIOHandler().setHTTPResponse(200)
            Me.getIOHandler().flushOutput(True, True)
            Exit Sub
        End If

        If HTTP_URI_Path = service_dns_lookup And HTTP_URI_Parameters.ContainsKey("host") Then
            Me.DNSLookup(HTTP_URI_Parameters("host"))
            Me.getIOHandler().setHTTPResponse(200)
            Me.getIOHandler().flushOutput(True, True)
            Exit Sub
        End If

        If HTTP_URI_Path = service_adapter_all Then
            Me.GetAllAdapters()
            Me.getIOHandler().setHTTPResponse(200)
            Me.getIOHandler().flushOutput(True, True)
            Exit Sub
        End If

        If HTTP_URI_Path = service_adapter_all_ethernet Then
            Me.GetAllEthernetAdapters()
            Me.getIOHandler().setHTTPResponse(200)
            Me.getIOHandler().flushOutput(True, True)
            Exit Sub
        End If

        If HTTP_URI_Path = service_adapter_all_wireless Then
            Me.GetAllWirelessAdapters()
            Me.getIOHandler().setHTTPResponse(200)
            Me.getIOHandler().flushOutput(True, True)
            Exit Sub
        End If

        pos = InStr(HTTP_URI_Path, "/network/adapter/")
        If pos > 0 Then
            Dim indexstr As String
            Dim adapter_index As Integer
            HTTP_URI_Path = Replace(HTTP_URI_Path, "/network/adapter/", "")

            'get index
            pos = InStr(HTTP_URI_Path, "/")
            indexstr = Mid(HTTP_URI_Path, 1, pos - 1)

            Try
                adapter_index = CType(indexstr, Integer)

                'test for valid index
                Dim dummy As NetworkInterface

                dummy = CType(Me.adapters(adapter_index), NetworkInterface)

            Catch ex As Exception
                'invalid index
                Me.getIOHandler().setHTTPResponse(404)
                Me.getIOHandler().writeRaw(True, "Error: invalid adapter index: " & ex.Message, True)
                Exit Sub
            End Try

            'get rid of the indexx
            HTTP_URI_Path = Replace(HTTP_URI_Path, adapter_index.ToString & "/", "")

            'parse the requested service..

            service_adapter_dns = "dns"
            service_adapter_name = "name"
            service_adapter_ipaddress = "ipaddress"
            service_adapter_gateway = "gateway"
            service_adapter_type = "type"

            'adapter context
            '===========

            If HTTP_URI_Path = service_adapter_dns Then
                Me.getAdapterDnsAddresses(adapter_index)
                Me.getIOHandler().setHTTPResponse(200)
                Me.getIOHandler().flushOutput(True, True)
                Exit Sub
            End If


            If HTTP_URI_Path = service_adapter_name Then
                Me.getAdapterName(adapter_index)
                Me.getIOHandler().setHTTPResponse(200)
                Me.getIOHandler().flushOutput(True, True)
                Exit Sub
            End If

            If HTTP_URI_Path = service_adapter_ipaddress Then
                Me.getAdapterIPAddress(adapter_index)
                Me.getIOHandler().setHTTPResponse(200)
                Me.getIOHandler().flushOutput(True, True)
                Exit Sub
            End If

            If HTTP_URI_Path = service_adapter_gateway Then
                Me.getAdapterGatewayIPAddress(adapter_index)
                Me.getIOHandler().setHTTPResponse(200)
                Me.getIOHandler().flushOutput(True, True)
                Exit Sub
            End If


            If HTTP_URI_Path = service_adapter_type Then
                Me.getAdapterType(adapter_index)
                Me.getIOHandler().setHTTPResponse(200)
                Me.getIOHandler().flushOutput(True, True)
                Exit Sub
            End If
        End If


    End Sub

    Public Sub GetAllAdapters()


        Dim adapter As NetworkInterface
        Dim x As Integer

        For x = 0 To Me.adapters.Count - 1
            adapter = Me.adapters(x)

            Me.getIOHandler().addOutput("/network/adapter/" & x.ToString & "/" & vbNewLine)
        Next x

    End Sub
    Public Sub GetAllEthernetAdapters()


        Dim adapter As NetworkInterface
        Dim x As Integer

        For x = 0 To Me.adapters.Count - 1
            adapter = Me.adapters(x)
            If InStr(adapter.NetworkInterfaceType.ToString.ToLower(), "ether") Then
                Me.getIOHandler().addOutput("/network/adapter/" & x.ToString & "/" & vbNewLine)
            End If

        Next x

    End Sub


    Public Sub GetAllWirelessAdapters()


        Dim adapter As NetworkInterface
        Dim x As Integer

        For x = 0 To Me.adapters.Count - 1
            adapter = Me.adapters(x)
            'Wireless80211
            If InStr(adapter.NetworkInterfaceType.ToString.ToLower(), "wireless") Then
                Me.getIOHandler().addOutput("/network/adapter/" & x.ToString & "/" & vbNewLine)
            End If

        Next x
    End Sub


    Public Sub getAdapterDnsAddresses(ByVal adapter_index As Integer)

        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As NetworkInterface


        adapter = Me.adapters(adapter_index)

        Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()
        Dim dnsServers As IPAddressCollection = adapterProperties.DnsAddresses


        If dnsServers.Count > 0 Then
            Dim dns As IPAddress

            For Each dns In dnsServers
                Me.getIOHandler().addOutput(dns.ToString())
                Me.getIOHandler().addOutput(vbNewLine)
            Next dns

        End If

    End Sub
    Public Sub getAdapterName(ByVal adapter_index As Integer)

        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As NetworkInterface


        adapter = Me.adapters(adapter_index)

        Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()
        Dim dnsServers As IPAddressCollection = adapterProperties.DnsAddresses

        Me.getIOHandler().addOutput(adapter.Name)

    End Sub

    Public Sub getAdapterType(ByVal adapter_index As Integer)

        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As NetworkInterface


        adapter = Me.adapters(adapter_index)

        Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()
        Dim dnsServers As IPAddressCollection = adapterProperties.DnsAddresses

        Me.getIOHandler().addOutput(adapter.NetworkInterfaceType.ToString)

    End Sub
    Public Sub getAdapterIPAddress(ByVal adapter_index As Integer)

        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As NetworkInterface


        adapter = Me.adapters(adapter_index)

        Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()

        Dim myGateways As GatewayIPAddressInformationCollection = Nothing

        myGateways = adapterProperties.GatewayAddresses

        For Each Gateway As GatewayIPAddressInformation In myGateways
            Me.getIOHandler().addOutput(Gateway.Address.ToString)
        Next



    End Sub
    Public Sub getAdapterGatewayIPAddress(ByVal adapter_index As Integer)

        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()
        Dim adapter As NetworkInterface


        adapter = Me.adapters(adapter_index)

        Dim adapterProperties As IPInterfaceProperties = adapter.GetIPProperties()

        Dim myGateways As GatewayIPAddressInformationCollection = Nothing

        myGateways = adapterProperties.GatewayAddresses

        For Each Gateway As GatewayIPAddressInformation In myGateways
            Me.getIOHandler().addOutput(Gateway.Address.ToString)
        Next



    End Sub

    Public Sub GetIPAddress()

        ' Obtain the first address of local machine with addressing scheme
        For Each IP As IPAddress In Dns.GetHostEntry(Dns.GetHostName()).AddressList
            If IP.AddressFamily = AddressFamily.InterNetwork Then
                Me.getIOHandler().addOutput(IP.ToString)
            End If
        Next IP

    End Sub

    Public Sub GetIPAddressRouted(ByVal processed_parameter_list As TriniDATGenericParameterCollection, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary)

        ' Obtain the first address of local machine with addressing scheme
        For Each IP As IPAddress In Dns.GetHostEntry(Dns.GetHostName()).AddressList
            If IP.AddressFamily = AddressFamily.InterNetwork Then
                Me.getIOHandler().addOutput(IP.ToString)
            End If
        Next IP

    End Sub
    Public Sub DNSLookup(ByVal hostString As [String])
        Try

            ' Get 'IPHostEntry' object which contains information like host name, IP addresses, aliases 
            ' for specified url 
            Dim hostInfo As IPHostEntry = Dns.GetHostEntry(hostString)



            Me.getIOHandler().addOutput((hostInfo.HostName & vbNewLine))
            Dim index As Integer

            For index = 0 To hostInfo.AddressList.Length - 1
                Me.getIOHandler().addOutput(hostInfo.AddressList(index).ToString & vbNewLine)
            Next index

        Catch e As SocketException
            Me.getIOHandler().addOutput("SocketException caught!!!")
            Me.getIOHandler().addOutput(("Source : " + e.Source))
            Me.getIOHandler().addOutput(("Message : " + e.Message))
        Catch e As ArgumentNullException
            Me.getIOHandler().addOutput("ArgumentNullException caught!!!")
            Me.getIOHandler().addOutput(("Source : " + e.Source))
            Me.getIOHandler().addOutput(("Message : " + e.Message))
        Catch e As Exception
            Me.getIOHandler().addOutput("Exception caught!!!")
            Me.getIOHandler().addOutput(("Source : " + e.Source))
            Me.getIOHandler().addOutput(("Message : " + e.Message))
        End Try
    End Sub 'GetIpAddressList
End Class
