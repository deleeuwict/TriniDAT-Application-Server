Option Explicit On
Option Compare Text
Imports System
Imports System.Runtime.CompilerServices

Imports System.Collections
Imports System.Collections.Specialized
Imports System.Uri
Imports System.Reflection
Imports System.Web
Imports System.IO
Imports System.Net
Imports System.Xml
Imports TriniDATHTTPTypes
Imports TriniDATBrowserEvent
Imports TriniDATPrimitiveXMLDOM
Imports TriniDATDictionaries
Imports TriniDATServerTypes

<Assembly: SuppressIldasmAttribute()> 

'28/10/13 TODO: Als ExecGet/ExecPost een redirect maakt dan moet Me.current_url + Me.current_uri_obj bijgewerkt worden.

Public Class TriniDATHTTPBrowser

    Private _wc As TriniDATNETWebClient
    Private rendered_xmldoc As XDocument
    Private primitive_xml_doc As PrimitiveXML
    Private htmldoc As String
    Private window As Object
    Private xml_source As String
    Private logger As TriniDATHTTPTypes.TriniDATTypeLogger
    Private initial_logger As TriniDATHTTPTypes.TriniDATTypeLogger 'switch on/off with verbose opt
    Private current_url As String
    Private current_uri_obj As Uri
    Private header_list As WebHeaderCollection

    Private opt_verbose As Boolean
    Private event_model As TriniDATBrowserEvents
    Private my_license As Object

    Private mytag As Object
    Private fire_analatyical_events As Boolean

    Public Sub New(ByVal l As Object, Optional ByVal _logfunc As TriniDATHTTPTypes.TriniDATTypeLogger = Nothing)


        If Not l.Verify Then
            Throw New Exception("Invalid license file.")
            Exit Sub
        Else
            Me.my_license = l
            If Not Me.L.Verify Then
                Throw New Exception("Invalid license file.")
                Exit Sub
            End If
        End If

        Me.event_model = New TriniDATBrowserEvents
        Me.window = Nothing
        Me.NetClient = Nothing
        Me.rendered_xmldoc = Nothing
        Me.current_uri_obj = Nothing
        Me.AnalyticalEventsEnabled = True
        Me.header_list = New WebHeaderCollection()

        If IsNothing(_logfunc) Then
            _logfunc = AddressOf NullEvent
        End If

        Me.setLogger(_logfunc)

        If TriniDATBrowserShared.needIinitialization() = True Then
            TriniDATBrowserShared.CreateDictionaries(AddressOf Msg)
        End If

    End Sub
    Private ReadOnly Property haveNetClient
        Get
            Return Not IsNothing(Me.NetClient)
        End Get
    End Property
    Private Property NetClient As TriniDATNETWebClient
        Get
            'de interne .NET WebClient die gebruikt is om HTML te downloaden. Bedoeld om de actuale URL bijhouden.
            Return Me._wc
        End Get
        Set(ByVal value As TriniDATNETWebClient)
            Me._wc = value
        End Set
    End Property

    Public ReadOnly Property Headers As WebHeaderCollection
        Get
            Return Me.header_list
        End Get
    End Property
    Public Property AnalyticalEventsEnabled As Boolean
        Get
            Return Me.fire_analatyical_events
        End Get
        Set(ByVal value As Boolean)
            Me.fire_analatyical_events = value
        End Set
    End Property

    Public Property Tag As Object
        Get
            Return Me.mytag
        End Get
        Set(ByVal value As Object)
            Me.mytag = value
        End Set
    End Property

    Public ReadOnly Property getLogger() As TriniDATTypeLogger
        Get
            Return Me.logger
        End Get
    End Property

    Public Function getEventModel() As TriniDATBrowserEvents
        Return Me.event_model
    End Function

    Public Sub setEventModel(ByVal e As TriniDATBrowserEvents)
        If Not IsNothing(e) Then
            Me.event_model = e
        Else
            Me.event_model = New TriniDATBrowserEvents
        End If

    End Sub

    Private Sub NullEvent()

    End Sub
    Public Sub setLogger(ByVal _logfunc As TriniDATTypeLogger)
        Me.initial_logger = _logfunc
        Me.logger = Me.initial_logger
    End Sub
    Public Sub setOpt(ByVal verbose_log As Boolean)
        Me.opt_verbose = verbose_log

        If Not Me.opt_verbose Then
            Me.logger = AddressOf NullEvent
        Else
            Me.logger = Me.initial_logger
        End If

    End Sub

    Public ReadOnly Property haveXDocument As Boolean
        Get
            Return Not IsNothing(Me.rendered_xmldoc)
        End Get
    End Property


    Public Function getXMLDoc() As String
        Return Me.xml_source
    End Function
    Public Sub setXMLDoc(ByVal val As String)
        '    Msg("setXMLDoc:  " & val)
        Me.xml_source = val
    End Sub

    Public Sub setURL(ByVal val As String)
        Me.current_url = val
        Me.current_uri_obj = New Uri(val)
        Msg("setURL:  " & current_url)
    End Sub

    Public Function getAbsoluteURL(ByVal val As String) As String
        If Me.haveNetClient() Then
            Try
                'oud:  Me.current_uri_obj = New Uri(Me.current_uri_obj, val)
                Dim temp As Uri
                temp = New Uri(Me.NetClient.currentURI, val)
                Return temp.AbsoluteUri
                'oud: Return Me.current_uri_obj.AbsoluteUri
            Catch ex As Exception
                Return False
            End Try
        End If
    End Function

    Public Function getRelativeURI(ByVal val As String) As String
        If Me.haveURI() Then
            Try
                Return Me.current_uri_obj.PathAndQuery
            Catch ex As Exception
                Return "err"
            End Try
        End If
    End Function


    Private Function haveURI() As Boolean
        Return Not IsNothing(Me.current_uri_obj)
    End Function

    Public Function getURL() As String
        Return Me.current_url
    End Function

    Private Function getURI() As Uri
        If Me.haveURI() Then
            Return Me.current_uri_obj
        End If

        Return Nothing

    End Function


    Public Sub setParentWindow(ByVal frm As System.Windows.Forms.Form)
        Me.window = frm
    End Sub


    'OnDocumentParseStart_Event(byval b as LionBrowser, ByVal src As String)
    Public Function getXMLRoot() As TriniDATPrimitiveXMLDOM.PrimitiveXML
        Return Me.primitive_xml_doc
    End Function
    Public Sub setXMLRoot(ByVal val As TriniDATPrimitiveXMLDOM.PrimitiveXML)
        Me.primitive_xml_doc = val
    End Sub
    Public Function getParentWindow() As Object
        Return Me.window
    End Function

    Public Function execPost(ByVal srcForm As Object) As Boolean
        If Not Me.L.Verify Then
            Exit Function
        End If

        If Not IsNothing(srcForm) Then
            If Not Me.getEventModel().Events_ondoc_onbeforesubmit(Me, "POST", srcForm) Then
                Msg("Form POST submission passed.")
                Return False
            End If
        End If

    End Function

    Private ReadOnly Property downloadURL() As String
        Get
            Dim htmldoc As String

            Try

                Me.NetClient = New TriniDATNETWebClient()
                Me.NetClient.Headers = Me.Headers

                'Set proper encoding
                Me.NetClient.Encoding = System.Text.Encoding.UTF8
                'fetch from server
                htmldoc = Me.NetClient.DownloadString(Me.getURL())

                Return htmldoc

            Catch ex As Exception

                Msg("downloadURL: error: " & ex.Message)
                Return Nothing
            End Try

        End Get

    End Property

    Public Function execGet(ByVal url As String, Optional ByVal srcForm As Object = Nothing) As Boolean


        Try
            If Not Me.L.Verify Then
                Exit Function
            End If

            'INIT
            Me.current_uri_obj = New Uri(url)

            If Not Me.current_uri_obj.IsAbsoluteUri() Then
                Msg("Start err: Relative URL was passed, please pass absolute URL.")
                Return False
            End If

            Me.setURL(Me.getURI().AbsoluteUri)

            If Not IsNothing(srcForm) Then
                If Not Me.getEventModel().Events_ondoc_onbeforesubmit(Me, "GET", srcForm) Then
                    Msg("Form GET submission passed.")
                    Return False
                End If
            End If


            Msg("Start: Downloading " & Me.getURL() & "...")
            'Fire events
            Me.getEventModel().Events_ondoc_downloading(Me)

            htmldoc = Me.downloadURL()
            If IsNothing(htmldoc) Then
                Throw New Exception("Connection failed.")
                Return False
            End If


            Msg("Parsing...")
            Me.setHTMLDoc(htmldoc)
            'Fire events
            Me.getEventModel().Events_ondoc_parse_start(Me, htmldoc)
            Call Me.onIncomingHTML()

            Msg("Start: " & Me.getURL() & " completed")


            If Me.AnalyticalEventsEnabled Then
                'Fire events
                Call Me.fireDocumentAnalyticEvents()
                'fire events completed event
                Me.getEventModel().Events_ondoc_events_complete(Me)
            End If


            'fire on document complete
            Me.getEventModel().Events_ondoc_complete(Me)
            Return True

        Catch ex As Exception
            Msg("Start: Error: " & ex.Message)
            Return False
        End Try
    End Function
    Public Shared Function createFromHTML(ByVal l As Object, ByVal htmldoc As String, ByVal from_url As String, Optional ByVal new_event_model As TriniDATBrowserEvents = Nothing, Optional ByVal _logfunc As TriniDATTypeLogger = Nothing) As TriniDATHTTPBrowser

        Dim retval As TriniDATHTTPBrowser
        retval = New TriniDATHTTPBrowser(l, _logfunc)

        If IsNothing(new_event_model) Then
            new_event_model = New TriniDATBrowserEvents
        End If

        Try

            'INIT
            retval.setEventModel(new_event_model)
            retval.setURL(from_url)

            'simulate download event
            retval.getEventModel().Events_ondoc_downloading(retval)

            If IsNothing(htmldoc) Then
                Err.Raise(0, 0, "No HTML was passed.")
                Return Nothing
            End If


            retval.Msg("Parsing...")
            retval.setHTMLDoc(htmldoc)
            'Fire events
            retval.getEventModel().Events_ondoc_parse_start(retval, htmldoc)
            Call retval.onIncomingHTML()

            retval.Msg("Start: " & retval.getURL() & " completed")

            'Fire events
            Call retval.fireDocumentAnalyticEvents()

            'fire events completed event
            retval.getEventModel().Events_ondoc_events_complete(retval)

            'fire on document compelte
            retval.getEventModel().Events_ondoc_complete(retval)

            Return retval

        Catch ex As Exception
            retval.Msg("Start: Error: " & ex.Message)
            Return Nothing
        End Try
    End Function
    Public Sub fireDocumentAnalyticEvents()

        Dim nullevents As TriniDATBrowserEvents

        Dim default_textel_handler As TriniDATBrowserEvents.OnElementHasPureText_Event
        Dim default_onform_handler As TriniDATBrowserEvents.OnForm_Event
        Dim default_link_handler As TriniDATBrowserEvents.OnLink_Event

        Dim html As String

        nullevents = New TriniDATBrowserEvents
        html = Me.getHTMLDoc()

        default_textel_handler = AddressOf nullevents.NullEvent
        default_onform_handler = AddressOf nullevents.NullEvent
        default_link_handler = AddressOf nullevents.NullEvent

        'ABSOLUTIZE ALL LINKS: 


        Dim x As Integer
        Dim lnkcollection As TriniDATHTTPTypes.TriniDATLinkCollection
        Dim current_link As TriniDATHTTPTypes.TriniDATLinkElement
        Dim ltr As Char
        Dim newlink As String

        lnkcollection = Me.getXMLRoot().getAllLinks()

        For x = 0 To lnkcollection.Count - 1

            current_link = lnkcollection.Item(x)

            newlink = current_link.getURL()

            If current_link.LinkType <> TriniDATLinkType.LINK_TYPE_ABSOLUTE Then


                ltr = Mid(newlink, 1, 1)

                'rewrite all relative urls
                If ltr <> "/" Then

                    If Left(ltr, 1) <> "h" Then

                        'check for special urls.
                        If ltr = "j" Or ltr = "m" Then
                            'check javascirpt,mailto etc.
                            If current_link.isProtocolHandler() Then GoTo VERIFY_NEXT_LINK
                        End If

                        'check for human readable text
                        If TriniDATBrowserShared.ASCIILetters.Has(ltr) Or TriniDATBrowserShared.ASCIIDigit.Has(ltr) Then
                            'abc.html
                            newlink = Me.getAbsoluteURL(newlink)
                        Else
                            Msg("Invalid link: " & newlink)
                            GoTo VERIFY_NEXT_LINK
                        End If

                        'mutate the url
                        current_link.setURL(newlink)
                    End If
                Else
                    'append domain etc
                    current_link.setURL(Me.getAbsoluteURL(newlink))
                End If

                current_link.LinkType = TriniDATLinkType.LINK_TYPE_ABSOLUTE
            End If

            If Not Me.getEventModel().OnLink = default_link_handler Then

                'fire browser event
                Me.getEventModel().Events_ondoc_link(Me, current_link)
            End If

VERIFY_NEXT_LINK:
        Next


        If Not Me.getEventModel().OnFormFound = default_onform_handler Then

            Dim all_forms As TriniDATHTTPTypes.TriniDATFormCollection

            all_forms = Me.getXMLRoot().getForms(Me.getURL())

            If Not IsNothing(all_forms) Then

                For x = 0 To all_forms.Count - 1

                    'fire
                    Call Me.getEventModel().Events_ondoc_found_form(Me, all_forms(x))

                Next

            End If

        End If


        If Me.L.CurrentLicense <> TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE Then

            If Not Me.getEventModel().Events_ondoc_text_element = default_textel_handler Then

                'check
                Dim els As List(Of TriniDATHTTPTypes.TriniDATNode)

                els = Me.getXMLRoot().getAllTextualNodes(html)

                If Not IsNothing(els) Then

                    For x = 0 To els.Count - 1
                        'fire event
                        Me.getEventModel().Events_ondoc_text_element(Me, els(x), Me.getXMLRoot().extractInnerText(els(x), html))
                    Next
                End If
            End If
        End If
    End Sub

    Public Sub onIncomingHTML()

        'convert HTML to XML
        Dim parsed_xml As TriniDATPrimitiveXMLDOM.PrimitiveXML

        ' Me.getParentWindow().txtHTML.Text = Me.getHTMLDoc()
        Me.setHTMLDoc(Me.preparseHTML(Me.getHTMLDoc()))

        parsed_xml = TriniDATPrimitiveXMLDOM.PrimitiveXML.CreateFromHTMLSourceV2(Me.getHTMLDoc())

        Me.setXMLRoot(parsed_xml)
        Me.setXMLDoc(Me.getXMLRoot().getFormattedSource())

    End Sub

    Public Sub Msg(ByVal txt As String)
        Me.logger(Me.GetType().ToString & ":  " & txt)
    End Sub

    Public Function preparseHTML(ByVal unparsed_html As String) As String
        Dim htmlsrc As String
        'delete  evil chars with spaces here
        htmlsrc = unparsed_html
        htmlsrc = Replace(htmlsrc, ChrW(0), "")
        Return htmlsrc

    End Function
    Public Function getHTMLDoc() As String
        Return Me.htmldoc
    End Function
    Public Sub setHTMLDoc(ByVal val As String)
        Me.htmldoc = val
    End Sub
    Public Function getPrettyXMLDoc() As XDocument
        'not always available
        Return Me.rendered_xmldoc
    End Function
    Public Sub setPrettyXML(ByVal val As XDocument)
        Me.rendered_xmldoc = val
    End Sub
    Public ReadOnly Property L As Object
        Get
            Return Me.my_license
        End Get
    End Property
    Public ReadOnly Property getNodeBrowser(ByVal ni As TriniDATHTTPTypes.TriniDATNode, Optional ByVal new_event_model As TriniDATBrowserEvents = Nothing, Optional ByVal new_browser_tag As Object = Nothing) As TriniDATHTTPBrowser
        'create a new browser object form a node's children aka getInnerHTML -> new XML DOM
        Get

            Dim xml_doc As PrimitiveXML
            Dim inner_html As String
            Dim b As TriniDATHTTPBrowser

            inner_html = Me.getXMLRoot().nodeHasInnerHTML(ni, Me.getHTMLDoc())

            If IsNothing(inner_html) Then
                Return Nothing
            End If

            xml_doc = PrimitiveXML.CreateFromHTMLSourceV2(inner_html)

            b = New TriniDATHTTPBrowser(Me.L, Me.getLogger())
            Try

                b.setHTMLDoc(inner_html)
                b.setXMLRoot(xml_doc)
                b.setXMLDoc(b.getXMLRoot().getFormattedSource())
                b.Tag = new_browser_tag

                If Not IsNothing(new_event_model) Then
                    'fire regular events on this browser as if document was done downloading
                    b.setEventModel(new_event_model)
                    b.getEventModel().Events_ondoc_parse_start(b, b.getHTMLDoc())
                    b.getEventModel().Events_ondoc_complete(b)
                    Call b.fireDocumentAnalyticEvents()
                    b.getEventModel().Events_ondoc_events_complete(b)
                End If

                Return b

            Catch ex As Exception
                Msg("getInnerHTMLFromNode: error creating browser. " & ex.Message)
                Return Nothing
            End Try

        End Get
    End Property
End Class
