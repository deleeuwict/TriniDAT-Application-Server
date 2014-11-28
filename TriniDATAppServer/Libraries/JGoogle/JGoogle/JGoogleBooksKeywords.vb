'Empty JTriniDATWebService instance for COPY/PASTE purposes.

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATBrowserEvent
Imports TriniDATHTTPTypes
Imports TriniDATPrimitiveXMLDOM
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JGoogleBooksKeywords
    Inherits JTriniDATWebService
    'trinidatServer.CONFIG_ROOT_PATH

    Public Shared google_book_search_url As String = "https://www.google.nl/search?q=$KEYWORD&btnG=Boeken+zoeken&tbm=bks&tbo=1&hl=nl"
    Public Const max_follow_link_count As Integer = 5
    Private links_followed As Integer
    Private tst As TriniDATSpecializedHTTPHandler

    Public Sub New()
        MyBase.New()

    End Sub

    Public Sub addFollowed()
        Me.links_followed += 1
    End Sub
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean

        Dim get_prototype As TriniDAT_ServerFunctionParameterSpec
        Dim my_get_function As TriniDAT_ServerGETFunction

        get_prototype = New TriniDAT_ServerFunctionParameterSpec
        my_get_function = New TriniDAT_ServerGETFunction(AddressOf doKeywordSearch)

        get_prototype.ParameterName = "keyword"
        get_prototype.ParameterType = "String"
        get_prototype.Required = True

        my_get_function.Parameters.Add(get_prototype)
        my_get_function.FunctionURL = Me.makeRelative("/")

        http_function_table.Add(my_get_function)

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
        '    http_events.event_onget = AddressOf OnGet
        GetIOHandler().Configure(http_events)

        Return True

    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean
        Return False
    End Function
    Public Sub doKeywordSearch(ByVal processed_parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)
        Dim keyword As String

        keyword = processed_parameter_list.getById("keyword").ParameterValue

        Dim search_url As String
        Dim b As TriniDATHTTPBrowser.TriniDATHTTPBrowser
        Dim b_events As TriniDATBrowserEvents

        'INIT
        search_url = JGoogleBooksKeywords.google_book_search_url
        search_url = Replace(search_url, "$KEYWORD", keyword)

        b = New TriniDATHTTPBrowser.TriniDATHTTPBrowser(AddressOf Msg)
        b_events = New TriniDATBrowserEvents()
        b_events.OnLink = AddressOf Me.OnLink_Event
        b_events.OnDocumentComplete = AddressOf Me.OnDocumentCompleted
        b_events.OnDocumentEventsComplete = AddressOf Me.OnDocumentEventsCompleted

        b.setEventModel(b_events)
        If Not b.execGet(search_url) Then
            Me.getIOHandler().writeRaw(True, "conncetion failed", True)
        End If

    End Sub

    Public Sub OnDocumentCompleted(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser)

    End Sub
    Public Sub OnFetchCloudLink_Event(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATNode)

    End Sub


    Public Sub OnLink_Event(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATLinkElement)

        Msg(el.getURL())

        If InStr(el.getURL(), "/books?id=") And Me.links_followed < JGoogleBooksKeywords.max_follow_link_count Then

            Msg("book link: " & el.getURL())

            'fetch book with specialized link handler
            Dim bookpage_events As TriniDATBrowserEvents
            bookpage_events = New TriniDATBrowserEvents
            bookpage_events.Events_ondoc_link = AddressOf Me.OnBookPageLink_Event
            Me.addFollowed()
            Call el.Click(b, bookpage_events)
        End If

    End Sub

    Public Sub OnBookPageLink_Event(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATLinkElement)

        If el.hasAttribute("class") Then

            If Left(el.getAttribute("class"), 5) = "cloud" Then
                Msg("link class = " & el.getAttribute("class"))

                Msg(b.getXMLRoot().extractInnerText(el, b.getHTMLDoc()))

                Dim nodebrowser As TriniDATHTTPBrowser.TriniDATHTTPBrowser
                Dim nb_event_model As TriniDATBrowserEvents

                nb_event_model = New TriniDATBrowserEvents
                nb_event_model.OnElementHasPureText = AddressOf Me.OnCloudLinkBlockInnerElement

                'parse & fire OnCloudLinkTextElement
                nodebrowser = b.getNodeBrowser(el, nb_event_model, New String(Replace(el.getAttribute("class"), "cloud", "")))
            End If

        End If
    End Sub

    Public Sub OnCloudLinkBlockInnerElement(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATNode, ByVal txt As String)
        'get SPAN

        If el.getTagName().ToLower = "span" Then
            Dim cloud_text As String
            Dim cloud_rating As String

            cloud_text = txt
            cloud_rating = b.Tag

            Me.getIOHandler().addOutput("keyword = " & cloud_text & vbNewLine & "importance = " & cloud_rating & vbNewLine & vbNewLine)
            Exit Sub
        End If

    End Sub


    Public Sub OnDocumentEventsCompleted(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser)
        'Msg("OnDocumentEventsCompleted")
        'Me.getIOHandler().Flush(True, True)
    End Sub

End Class
