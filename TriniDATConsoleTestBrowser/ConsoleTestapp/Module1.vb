Option Explicit On
Option Compare Text

Imports TriniDATHTTPTypes
Imports TriniDATPrimitiveXMLDOM
Imports TriniDATHTTPBrowser
Imports TriniDATBrowserEvent
Imports TriniDATDictionaries
Imports TriniDATServerLicense

Module Module1

    Private links As List(Of String)
    Private ignore_links As TriniDATWordDictionary
    Private nofollow_domain As TriniDATWordDictionary
    Private ignore_tags As TriniDATWordDictionary
    Private ignore_captions As TriniDATWordDictionary
    Private Const debug_log As Boolean = False
    Public Const bing_search As String = "bhagavad%Gita"
    Public submitted As Boolean = False
    Public my_license As TriniDATServerLicense.TriniDATServerLicense

    '   Public Const MAX_DEPTH As Integer = 5
    'Public CUR_DEPTH As Integer
    Public stay_on_domain As Boolean = False
    Public stay_domain As String = "http://www.kpn.nl"
    Sub Main()

        msg("Loading...")
        Dim url As String
        '        Dim lt_free As TriniDATServerTypes.TRINIDAT_SERVER_LICENSE

        'CUR_DEPTH = 0

        my_license = New TriniDATServerLicense.TriniDATServerLicense(TriniDATServerTypes.TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE)


        links = New List(Of String)
        ignore_links = New TriniDATWordDictionary("", New List(Of String))
        nofollow_domain = New TriniDATWordDictionary("", New List(Of String))
        ignore_tags = New TriniDATWordDictionary("", New List(Of String)({"style", "script", "title"}))
        ignore_captions = New TriniDATWordDictionary("ignoretext", New List(Of String))

        links = links

        '  url = "http://www.google.com/search?tbm=blg&hl=en&source=hp&biw=1024&bih=655&q=" & blogsearch
        'url = "http://books.google.nl/books?id=wKk2A4vLCfgC&amp;printsec=frontcover&amp;dq=doodle&amp;hl=nl&amp;sa=X&amp;ei=f3I_UbT3E4iOO4nlgdgG&amp;ved=0CDgQ6AEwBQ"

        '../../gita/commentaar/lijst/v.html
        url = "http://www.youtube.com/watch?v=YJKESYJ4QW8"

        links.Add(url)

        'SET-UP LISTS
        '=========

        ignore_captions.getWordList().Add("forgot")
        ignore_captions.getWordList().Add("password")
        ignore_captions.getWordList().Add("account")
        ignore_captions.getWordList().Add("privacy")
        ignore_captions.getWordList().Add("login")
        ignore_captions.getWordList().Add("share")
        ignore_captions.getWordList().Add("policy")
        ignore_captions.getWordList().Add("disclaim")
        ignore_captions.getWordList().Add("sign")
        ignore_captions.getWordList().Add("mail")
        ignore_captions.getWordList().Add("join")
        ignore_captions.getWordList().Add("register")
        ignore_captions.getWordList().Add("help")
        ignore_captions.getWordList().Add("become")
        ignore_captions.getWordList().Add("aanmeld")
        ignore_captions.getWordList().Add("inlog")
        ignore_captions.sortByLength()

        Module1.nofollow_domain.getWordList().Add("ciao")
        Module1.nofollow_domain.getWordList().Add("google")
        Module1.nofollow_domain.getWordList().Add("facebook")
        Module1.nofollow_domain.getWordList().Add("live.com")
        Module1.nofollow_domain.getWordList().Add("microsoft")
        Module1.nofollow_domain.getWordList().Add("msdn")
        Module1.nofollow_domain.getWordList().Add("twitter")
        Module1.nofollow_domain.getWordList().Add("blogger")
        Module1.nofollow_domain.getWordList().Add("blogspot")
        Module1.nofollow_domain.getWordList().Add("bing")
        Module1.nofollow_domain.getWordList().Add("youtube")
        Module1.nofollow_domain.getWordList().Add("android")
        Module1.nofollow_domain.getWordList().Add("apple")
        Module1.nofollow_domain.sortByLength()


        Module1.ignore_tags.sortByLength()

        '    Module1.nofollow_domain.Add("yahoo.com")

        '   Module1.ignore_links.Add("http://www.facebook.com/")

        msg("Spider: Start.")
        Call spider(url)

        Msg("Klaar.")
        Console.In.Read()

    End Sub

    Public Sub OnLink_Event(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATLinkElement)
        Msg("pure link: " & el.getURL())

        Exit Sub
        If Not ignoreLink(el) Then
            'add handled
            Module1.links.Add(el.getURL())
            Spider(el.getURL())
        End If

    End Sub
    Public Function OnBeforeFrmSubmitted(ByVal fromb As Object, ByVal frmmethod As String, ByVal TriniDATformobj As Object) As Boolean

        If TypeOf (TriniDATformobj) Is TriniDATEditableForm Then

            msg("could delete form: " & frmmethod & "." & TriniDATformobj.getId())
            'show mutations
            msg("========")
            msg(TriniDATformobj.getId() & " OnBeforePost...")
            msg(TriniDATformobj.AllMutations)
            msg("========")
            Return False
        End If

        Return True

    End Function

    Public Function OnDecideIfFollowing(ByVal frombrowser As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATLinkElement, ByRef unfired_new_browser_object As TriniDATHTTPBrowser.TriniDATHTTPBrowser) As Boolean

        Return Not ignoreLink(el)

    End Function

    Public Function ignoreLink(ByVal el As TriniDATHTTPTypes.TriniDATLinkElement) As Boolean

        Dim lnk As String
        Dim lnkdomain As String

        ' Return (nofollow_domain.HasIn(el.getURL(), "/") > 0)

        'check domains
        For Each lnkdomain In Module1.nofollow_domain.getWordList()

            If InStr(el.getURL(), lnkdomain & "/") > 0 Then
                Return True
            End If
        Next


        'check specific links
        For Each lnk In Module1.links

            If Left(lnk, Len(el.getURL())) = el.getURL() Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Sub OnAddIgnoredCaption(ByVal caption As String)
        msg("Learning to ignore: " & caption)
        ignore_captions.getWordList().Add(caption)
        ignore_captions.sortByLength()
    End Sub

    Private Sub OnForm(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal TriniDATfrm As TriniDATHTTPTypes.TriniDATForm)

        Dim inputs As TriniDATInputElementCollection
        Dim frmid As String
        Dim submit As TriniDATFormSubmitButton
        Dim onsubmit As String
        Dim fieldvalue As String

        frmid = TriniDATfrm.getId()

        msg("=====")
        msg(TriniDATfrm.getMethod().ToString("G") & ".FORM ID: " & frmid)
        msg(TriniDATfrm.getMethod().ToString("G") & " = " & TriniDATfrm.getAction())

        frmid = TriniDATfrm.getMethod().ToString("G") & "." & frmid
        inputs = TriniDATfrm.getInputFields()
        submit = TriniDATfrm.getSubmit()
        onsubmit = TriniDATfrm.getFormOnSubmitAction()

        If onsubmit <> "" Then
            onsubmit = onsubmit
        End If

        For Each inp As TriniDATInputElement In inputs
            fieldvalue = inp.getAttribute("value")
            msg(frmid & "." & inp.getAttribute("name") & " = " & fieldvalue)
        Next

        msg("submit: " & IIf(IsNothing(submit), "found", "(" & onsubmit & ")"))

        msg("=====")

        If Not submitted Then

            If TriniDATfrm.getId() = "sb_form" Then

                Dim editable_form As TriniDATEditableForm

                editable_form = TriniDATEditableForm.createFrom(TriniDATfrm)
                editable_form.setDebuggerLog(AddressOf msg)
                msg("Submitting " & editable_form.getId())

                Dim srcfield As TriniDATEditableInputElement

                srcfield = editable_form.getByNameOrId("q")
                If Not srcfield.setAttribute("value", bing_search) Then
                    MsgBox("Error setting form field value. " & srcfield.getAttribute("name"))
                    Exit Sub
                End If

                'show mutations
                msg("========")
                msg("Listing Mutations...")
                msg(editable_form.AllMutations)
                msg("========")

                submitted = True
                ' CUR_DEPTH = CUR_DEPTH + 1
                '    msg("New depth = " & CUR_DEPTH.ToString)
                Dim newbrowser As TriniDATHTTPBrowser.TriniDATHTTPBrowser
                newbrowser = editable_form.Submit(b.getEventModel())

            End If
        End If
    End Sub

    Public Sub OnDownloadEvent(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser)

        msg("Downloading " & b.getURL() & "...")

    End Sub

    Public Sub OnDocumentCompleted(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser)

        Dim ds As List(Of TriniDATHTTPTypes.TriniDATGenericNodeFamily)
        Dim dsid As String
        Dim x As Integer


        ds = b.getXMLRoot().getallParential("ul", "li")

        'SPECIALIZED CODE 
        If Not IsNothing(ds) Then

            For Each DataSet In ds

                x = 0
                dsid = DataSet.getParent().getAttribute("id")
                If dsid = "" Then
                    dsid = "anonymous"
                End If
                msg("======")
                msg("Dataset id: " & dsid)
                For Each listitem In DataSet.getChildren()
                    Dim nodebrowser As TriniDATHTTPBrowser.TriniDATHTTPBrowser

                    nodebrowser = b.getNodeBrowser(listitem)

                    If Not IsNothing(nodebrowser) Then
                        Dim y As Integer
                        Dim nb_count As Integer
                        Dim nb_node As TriniDATHTTPTypes.TriniDATNode
                        Dim node_contents As String
                        Dim extract_element_count As Integer

                        x = x + 1
                        node_contents = ""
                        nb_count = nodebrowser.getXMLRoot().getNodeCount()
                        extract_element_count = 0

                        'extract_element_count
                        For y = 0 To nb_count - 1

                            nb_node = nodebrowser.getXMLRoot().getNodeByIndex(y)

                            If nodebrowser.getXMLRoot().nodeHasInnerHTML(nb_node, nodebrowser.getHTMLDoc()) = "" Then
                                'collect text items
                                Dim temp As String
                                temp = nodebrowser.getXMLRoot().extractInnerText(nb_node, nodebrowser.getHTMLDoc())
                                If TypeOf (temp) Is String Then
                                    node_contents &= Trim(temp) & " "
                                    extract_element_count = extract_element_count + 1
                                End If
                            End If

                        Next y

                        msg("dataset." & dsid & ".html(" & extract_element_count.ToString & ")" & x.ToString & " = " & node_contents)

                    Else
                        'non-html node
                        msg("dataset." & dsid & ".puretext" & x.ToString & "=" & Trim(b.getXMLRoot().extractInnerText(listitem, b.getHTMLDoc())))
                        x = x

                    End If
                Next

                msg("======")
            Next

        End If

    End Sub
    Public Sub OnTextElement(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATNode, ByVal caption As String)


        If Not ignore_tags.Has(el.getTagName()) Then
            Dim haspos As Integer

            caption = Trim(caption)
            msg(el.getId() & "." & el.getTagName() & ": " & caption)


            haspos = ignore_captions.HasIn(caption)

            If haspos = 0 And el.getTagName() = "a" Then


                Dim linkel As TriniDATHTTPTypes.TriniDATLinkElement

                linkel = TriniDATLinkElement.createFrom(el)

                'get rid of hashtag
                haspos = InStr(linkel.getURL(), "#")
                If haspos > 0 Then
                    linkel.setURL(Mid(linkel.getURL(), 1, haspos - 1))
                End If

                If stay_on_domain = True And Left(linkel.getURL(), Len(stay_domain)) <> stay_domain Then
                    Exit Sub
                End If

                'fix: record uri parts per url. if same uri x 20 and spider mode then deadlock.
                If linkel.getURL() <> "" And links.IndexOf(linkel.getURL()) = -1 Then
                    If Not ignoreLink(linkel) Then

                        If Not ignore_captions.Has(caption) Then

                            If Not IsNothing(linkel) Then
                                msg("=====" & vbNewLine & "following " & caption & " [" & linkel.getURL() & "]")
                                Module1.links.Add(linkel.getURL())

                                linkel.Click(b, b.getEventModel())
                                Exit Sub
                                'Call spider(newlink)
                            End If
                        End If
                    End If
                End If
            End If
        Else
            'partially matched block keyword. 
            If Len(caption) < 5 Then
                Call OnAddIgnoredCaption(caption)
                Exit Sub
            End If
        End If
    End Sub

    Public Sub Spider(ByVal url As String)

        Dim b As TriniDATHTTPBrowser.TriniDATHTTPBrowser
        Dim bevent As TriniDATBrowserEvents
        Dim els As List(Of TriniDATHTTPTypes.TriniDATNode)

        b = New TriniDATHTTPBrowser.TriniDATHTTPBrowser(my_license)
        b.setOpt(debug_log)

        'design event model
        bevent = New TriniDATBrowserEvents
        bevent.OnElementHasPureText = AddressOf OnTextElement
        bevent.OnFormFound = AddressOf OnForm
        bevent.OnDocumentComplete = AddressOf OnDocumentCompleted
        bevent.OnDocumentDownloading = AddressOf OnDownloadEvent
        bevent.OnLink = AddressOf OnLink_Event
        bevent.OnLinkClicked = AddressOf OnDecideIfFollowing
        bevent.OnBeforeSubmit = AddressOf OnBeforeFrmSubmitted
        b.setEventModel(bevent)


        'add 
        links.Add(url)

        Msg("Following " & url)

        If Not b.execGet(url) Then
            Module1.ignore_links.getWordList().Add(url)
        End If


    End Sub
    Public Sub Msg(ByVal txt As String)
        Console.WriteLine(txt)
        Debug.Print(txt)
    End Sub

End Module
