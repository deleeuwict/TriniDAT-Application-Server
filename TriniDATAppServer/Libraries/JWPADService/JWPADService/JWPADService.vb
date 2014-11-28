Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JWPADService
    Inherits JTriniDATWebService
    'serves a WPAD file.
    '

    '/wpad.dat


    'BlingServer.CONFIG_ROOT_PATH

    '

    Private Const WPAD_PROXYSERVER_IP As String = "192.168.2.4"
    Private Const WPAD_PROXYSERVER_PORT As Integer = 8118


    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        GetIOHandler().Configure(http_events)


        Return True

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        'send a WPAD script
        Me.GetIOHandler().setHTTPResponse(200)
        Me.GetIOHandler().setOutputMime("application/x-ns-proxy-autoconfig")
        Me.writeProxyConfigFile(JWPADService.WPAD_PROXYSERVER_IP, JWPADService.WPAD_PROXYSERVER_PORT)

        Me.GetIOHandler().flush()

    End Sub


    Private Sub writeProxyConfigFile(ByVal host As String, ByVal port As Integer)
        'write full proxy address to a javascript file in WPAD format.
        '
        'http://en.wikipedia.org/wiki/Proxy_auto-config
        '
        '  function FindProxyForURL(url, host)
        '  {
        '    return "PROXY proxy.example.com:8080; DIRECT";
        ' }

        Dim proxy_jsscript As String

        proxy_jsscript = "function FindProxyForURL(url, host)"
        proxy_jsscript &= "{"
        'action list is seperated by;
        proxy_jsscript &= "return ""PROXY " & host & ":" & port.ToString & "; DIRECT"";"
        proxy_jsscript &= "}"

        Me.GetIOHandler().addOutput(proxy_jsscript.ToString)

    End Sub



End Class
