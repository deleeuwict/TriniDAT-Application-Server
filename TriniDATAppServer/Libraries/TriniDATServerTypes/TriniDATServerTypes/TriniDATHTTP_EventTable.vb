Imports System.Collections.Specialized

Public Class TriniDATHTTP_EventTable

    Public Delegate Sub OnHTTPRequestGetTemplate(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
    Public Delegate Sub OnHTTPRequestPostTemplate(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
    Public Delegate Sub OnIncomingStreamingData(ByVal buffer As String)

    Public event_onget As OnHTTPRequestGetTemplate
    Public event_onget_internet As OnHTTPRequestGetTemplate
    Public event_onget_intranet As OnHTTPRequestGetTemplate
    Public event_onget_localhost As OnHTTPRequestGetTemplate

    Public event_onpost As OnHTTPRequestPostTemplate
    Public event_onpost_internet As OnHTTPRequestPostTemplate
    Public event_onpost_intranet As OnHTTPRequestPostTemplate
    Public event_onpost_localhost As OnHTTPRequestPostTemplate

    Public event_onstream As OnIncomingStreamingData
    Public event_onstream_internet As OnIncomingStreamingData
    Public event_onstream_intranet As OnIncomingStreamingData
    Public event_onstream_localhost As OnIncomingStreamingData

    Public Sub New()
        Me.setDefaults()
    End Sub
    Public Sub setDefaults()
        Me.event_onget = AddressOf Me.NullRequest
        Me.event_onget_internet = AddressOf Me.NullRequest
        Me.event_onget_intranet = AddressOf Me.NullRequest
        Me.event_onget_localhost = AddressOf Me.NullRequest

        Me.event_onpost = AddressOf Me.NullRequest
        Me.event_onpost_internet = AddressOf Me.NullRequest
        Me.event_onpost_intranet = AddressOf Me.NullRequest
        Me.event_onpost_localhost = AddressOf Me.NullRequest

        Me.event_onstream = AddressOf Me.NullRequest
        Me.event_onstream_internet = AddressOf Me.NullRequest
        Me.event_onstream_intranet = AddressOf Me.NullRequest
        Me.event_onstream_localhost = AddressOf Me.NullRequest

    End Sub
    Public Function haveGETEventHandler() As Boolean
        Dim dummy As OnHTTPRequestGetTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onget = dummy)
    End Function

    Public Function haveLocalhostGETEventHandler() As Boolean
        Dim dummy As OnHTTPRequestGetTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onget_localhost = dummy)
    End Function

    Public Function haveIntranetGETEventHandler() As Boolean
        Dim dummy As OnHTTPRequestGetTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onget_intranet = dummy)
    End Function

    Public Function haveInternetGETEventHandler() As Boolean
        Dim dummy As OnHTTPRequestGetTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onget_internet = dummy)
    End Function

    Public Function havePOSTEventHandler() As Boolean
        Dim dummy As OnHTTPRequestPostTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onpost = dummy)
    End Function

    Public Function haveInternetPOSTEventHandler() As Boolean
        Dim dummy As OnHTTPRequestPostTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onpost_internet = dummy)
    End Function

    Public Function haveLocalHostPOSTEventHandler() As Boolean
        Dim dummy As OnHTTPRequestPostTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onpost_localhost = dummy)
    End Function

    Public Function haveIntranetPOSTEventHandler() As Boolean
        Dim dummy As OnHTTPRequestPostTemplate
        dummy = AddressOf NullRequest
        Return Not (Me.event_onpost_intranet = dummy)
    End Function

    Public Function haveOnIncomingStreamDataHandler() As Boolean
        Dim dummy As OnIncomingStreamingData
        dummy = AddressOf NullRequest
        Return Not (Me.event_onstream = dummy)
    End Function

    Public Function haveOnIncomingLocalhostStreamDataHandler() As Boolean
        Dim dummy As OnIncomingStreamingData
        dummy = AddressOf NullRequest
        Return Not (Me.event_onstream_localhost = dummy)
    End Function

    Public Function haveOnIncomingIntranetStreamDataHandler() As Boolean
        Dim dummy As OnIncomingStreamingData
        dummy = AddressOf NullRequest
        Return Not (Me.event_onstream_intranet = dummy)
    End Function

    Public Function haveOnIncomingInternetStreamDataHandler() As Boolean
        Dim dummy As OnIncomingStreamingData
        dummy = AddressOf NullRequest
        Return Not (Me.event_onstream_internet = dummy)
    End Function
    Public Sub NullRequest(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

    End Sub
    Public Sub NullRequest(ByVal buffer As String)

    End Sub
End Class
