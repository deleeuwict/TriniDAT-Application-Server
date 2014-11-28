Imports System.Collections.Specialized
Imports BlingProxies
Imports TriniDATServerTypes
Imports System.Web
Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class JProxies
    Inherits JTriniDATWebService

    Public runtimeCount As Integer = 0
    Private Shared Unverifiedlist As BlingProxyFile = Nothing
    Private Shared Verifiedlist As BlingProxyFile = Nothing
    Private Shared Unverifiedlist_filename As String
    Private Shared Verifiedlist_filename As String
    Private Shared proxyfiles_entry_delimiter As String

    Public Sub setProxyFileDelimiter(ByVal val As String)
        JProxies.proxyfiles_entry_delimiter = val
    End Sub
    Public Function getProxyFileDelimiter() As String
        Return JProxies.proxyfiles_entry_delimiter
    End Function

    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub

    Public Function getVerifiedProxyList() As BlingProxyFile
        Return JProxies.Verifiedlist
    End Function

    Public Function getUnverifiedproxyList() As BlingProxyFile
        Return JProxies.Unverifiedlist
    End Function

    Public Sub setVerifiedProxyList(ByRef val As BlingProxyFile)
        JProxies.Verifiedlist = val
    End Sub

    Public Sub setUnverifiedproxyList(ByRef val As BlingProxyFile)
        JProxies.Unverifiedlist = val
    End Sub

    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()

     'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox
        mb_events.event_delivered = AddressOf delivered
        mb_events.event_err = AddressOf deliveryerr

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        http_events.event_onpost = AddressOf OnPost

        Me.GetIOHandler().Configure(http_events)

        Return True

    End Function
    Public Sub reloadProxyLists()
        'initialize proxy lists
        Call setProxyFileDelimiter(":")

        Verifiedlist_filename = "Verified.txt"
        Call Me.setVerifiedProxyList(New BlingProxyFile(Verifiedlist_filename, Me.getProxyFileDelimiter()))

        Unverifiedlist_filename = "Unverified.txt"
        Call Me.setunverifiedproxyList(New BlingProxyFile(Unverifiedlist_filename, Me.getProxyFileDelimiter()))

    End Sub
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> user inbox called.. Type=" & obj.ObjectTypeName)


        If obj.Sender.ToString = "JAlpha" And obj.ObjectTypeName = "MAPPING_POINT_START" Then
            Dim mp_config As MappingPointBootstrapData

            mp_config = CType(obj.Attachment, MappingPointBootstrapData)
            BlingProxies.BlingProxyFile.PROXY_ENVIRONMENT_ROOT_PATH = mp_config.static_path


            If Not IsNothing(Me.getVerifiedProxyList()) Then Return False
            If Not IsNothing(Me.getUnverifiedproxyList()) Then Return False

            Call reloadProxyLists()
            Return False
        End If

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            '            Msg("Omega notification received.")
            '            logline("Omega Flush call.")
            Return False
        End If

        Return False
    End Function
    Public Sub delivered(ByVal obj As JSONObject, destination_url as string)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub

    Public Function validaterequest(ByVal HTTP_URI_Parameters As StringDictionary, Optional ByVal validateType As Boolean = True) As Boolean
        Dim temp As String
        Dim proxytype As ProxyServerProtocols

        Try
            temp = HTTP_URI_Parameters("port")
            If Not IsNumeric(HTTP_URI_Parameters("port")) Then
                Err.Raise(200, 0, "Invalid or no port specified")
                Return False
            End If

            temp = HTTP_URI_Parameters("host")
            If Len(HTTP_URI_Parameters("host")) < 1 Then
                Err.Raise(200, 0, "Invalid or no hostname specified")
                Return False
            End If

            If validateType Then
                proxytype = CType([Enum].Parse(GetType(ProxyServerProtocols), HTTP_URI_Parameters("type")), ProxyServerProtocols)
            End If

            Return True

        Catch ex As Exception
            Me.GetIOHandler().setHTTPResponse(417)
            Me.GetIOHandler().writeRaw(True, ex.Message & " @ " & ex.StackTrace.ToString, True)
            Return False
        End Try
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        Dim unverified_add_uri As String
        Dim unverified_remove_uri As String
        Dim Verified_add_uri As String
        Dim reload_proxylist_uri As String
        Dim retval As Boolean


        runtimeCount = runtimeCount + 1
        HTTP_URI_Path = Me.getProcessDescriptor().getTriggerURI()

        Verified_add_uri = Me.getProcessDescriptor().getParent().getURI() & "verified/add"

        unverified_add_uri = Me.getProcessDescriptor().getParent().getURI() & "unverified/add"
        unverified_remove_uri = Me.getProcessDescriptor().getParent().getURI() & "unverified/remove"

        reload_proxylist_uri = Me.getProcessDescriptor().getParent().getURI() & "reload"

        'Verified add
        If HTTP_URI_Path = Verified_add_uri Then
            If Not Me.validaterequest(HTTP_URI_Parameters) Then Exit Sub
            Me.getVerifiedProxyList().addEntry(HTTP_URI_Parameters("host"), CType(HTTP_URI_Parameters("port"), Integer), CType([Enum].Parse(GetType(ProxyServerProtocols), HTTP_URI_Parameters("type")), ProxyServerProtocols), HttpUtility.UrlDecode(HTTP_URI_Parameters("geo")), False)
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().writeRaw(True, "OK", True)
            Exit Sub
        End If

        'unverified add
        If HTTP_URI_Path = unverified_add_uri Then
            If Not Me.validaterequest(HTTP_URI_Parameters) Then Exit Sub
            Me.getUnverifiedproxyList().addEntry(HTTP_URI_Parameters("host"), CType(HTTP_URI_Parameters("port"), Integer), CType([Enum].Parse(GetType(ProxyServerProtocols), HTTP_URI_Parameters("type")), ProxyServerProtocols), HttpUtility.UrlDecode(HTTP_URI_Parameters("geo")), False)
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().writeRaw(True, "OK", True)
            Exit Sub
        End If

        'unverified remove
        If HTTP_URI_Path = unverified_remove_uri Then
            If Not Me.validaterequest(HTTP_URI_Parameters, False) Then Exit Sub
            retval = Me.getUnverifiedproxyList().removeEntry(HTTP_URI_Parameters("host"), CType(HTTP_URI_Parameters("port"), Integer), , True)
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().writeRaw(True, IIf(retval, "REMOVED", "NOT REMOVED"), True)
            Exit Sub
        End If

        'Verified list
        If HTTP_URI_Path = Me.getProcessDescriptor().getParent().getURI() & "verified/list" Then
            Me.GetIOHandler().writeRaw(True, Me.getVerifiedProxyList().GetAll(), True)
            Exit Sub
        End If

        'unverified list
        If HTTP_URI_Path = Me.getProcessDescriptor().getParent().getURI() & "unverified/list" Then
            Me.GetIOHandler().writeRaw(True, Me.getUnverifiedproxyList().Getall(), True)
            Me.GetIOHandler().writeRaw(True, "unverified PROXY: " & HTTP_URI_Parameters("proxy") & "  port: " & HTTP_URI_Parameters("port"), True)
            Exit Sub
        End If

        'reload
        If HTTP_URI_Path = reload_proxylist_uri Then
            Call reloadProxyLists()
            Me.GetIOHandler().setHTTPResponse(200)
            Me.GetIOHandler().writeRaw(True, "OK", True)
            Exit Sub
        End If
    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")


        runtimeCount = runtimeCount + 1

    End Sub

End Class


