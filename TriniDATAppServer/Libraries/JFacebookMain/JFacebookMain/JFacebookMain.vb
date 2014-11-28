Option Explicit On
Option Compare Text
Imports System.Web
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Reflection
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATHTTPTypes
Imports BlingBrowser
Imports TriniDATBrowserEvent
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 


Public Class JFacebookMain
    Inherits JTriniDATWebService

    'BlingServer.CONFIG_ROOT_PATH

    '
    Public runtimeCount As Integer = 0

    Private all_fb_html As String
    Private current_ticketid As String
    Private facebook_page_is_snippet As Boolean
    Private facebook_page_is_main As Boolean
    Private snippet_type As FacebookRemoteType
    Private current_page_url As String
    Private null_instruction As Boolean
    Private fb_email As String
    Private fb_password As String
    Private fb_current_user As String
    Private friend_count As Integer

    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub
    Public Property Facebook_current_User As String
        Get
            Return Me.fb_current_user
        End Get
        Set(ByVal value As String)
            Me.fb_current_user = value
        End Set
    End Property
    Public Property UserName As String
        Get
            Return Me.fb_email
        End Get
        Set(ByVal value As String)
            Me.fb_email = value
        End Set
    End Property
    Public Property Password As String
        Get
            Return Me.fb_password
        End Get
        Set(ByVal value As String)
            Me.fb_password = value
        End Set
    End Property

    Public ReadOnly Property getSnippetType() As FacebookRemoteType
        Get
            Return Me.snippet_type
        End Get
    End Property
    Public Sub setSnippetType(ByVal val As FacebookRemoteType)
        Me.snippet_type = val
    End Sub


    Public ReadOnly Property isSnippet() As Boolean
        Get
            Return (Me.getSnippetType() = FacebookRemoteType.Snippet)
        End Get
    End Property

    Public ReadOnly Property isFullPage() As Boolean
        Get
            Return (Me.getSnippetType() = FacebookRemoteType.Page)
        End Get
    End Property
    Public ReadOnly Property isSnapShot() As Boolean
        Get
            Return (Me.getSnippetType() = FacebookRemoteType.Snapshot)
        End Get
    End Property
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
        http_events.event_onpost = AddressOf OnPost
        http_events.event_onstream = AddressOf OnNetworkStream
        GetIOHandler().Configure(http_events)

        Me.all_fb_html = ""
    End Sub

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean
        Msg(">> user inbox called.. Type=" & obj.ObjectTypeName)


        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")
            logline("Omega Flush call.")
        End If

        If obj.ObjectTypeName = "JOMEGAREPLY" And obj.Directive = "SUCCES" Then
            Msg("Omega succesfully disabled.")
        End If


        Return False
    End Function
    Public Sub delivered(ByVal obj As JSONObject, destination_url as string)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub

    Public Sub OnNetworkStream(ByVal dat As String)

        Dim end_of_stream As Boolean
        Dim buffer As String

        end_of_stream = IsNothing(dat)

        If Not end_of_stream Then

            buffer = dat
            Me.all_fb_html = Me.all_fb_html & buffer

            Msg("Incoming stream : " & buffer)
        Else
            Call Me.OnDataComplete()
        End If



    End Sub

    Public Property Facebook_URL As String
        Get
            Return Me.current_page_url
        End Get
        Set(ByVal value As String)
            Me.current_page_url = value
        End Set
    End Property

    Public Sub OnDataComplete()
        Msg("Stream done.")
        Msg("Facebook HTML: " & Trim(Me.all_fb_html))
        Me.all_fb_html = Trim(Me.all_fb_html)

        'strip parameters
        Dim page_url As String

        page_url = Me.Facebook_URL

        Me.null_instruction = True

        Call ParseRemoteHTML(page_url)
       
    End Sub

    Public Sub OnPageEventsDone(ByVal b As Object)
        If Me.null_instruction = True Then
            If Me.isSnapShot Then
                'cancel

                '  Me.writeTicket(Me.getTicket(), "snapshot=false" & vbCrLf)
                'PONG
                Me.writeTicket(Me.getTicket(), "NULL=" & HttpUtility.UrlEncode(Me.Facebook_URL) & vbCrLf)

            ElseIf Me.isSnippet() Then
                '                Me.writeTicket(Me.getTicket(), "snippets=false" & vbCrLf)
                'PONG
                Me.writeTicket(Me.getTicket(), "NULL=" & HttpUtility.UrlEncode(Me.Facebook_URL) & vbCrLf)

            Else
                'PONG
                Me.writeTicket(Me.getTicket(), "NULL=" & HttpUtility.UrlEncode(Me.Facebook_URL) & vbCrLf)
            End If

            Me.GetIOHandler().writeRaw(True, "DONE " & Me.getTicket(), True)
        End If

    End Sub
    Public Function writeTicket(ByVal ticketid As String, ByVal contents As String) As Boolean
        Dim ticket_write_request As JSONObject

        Try

            ticket_write_request = New JSONObject

            ticket_write_request.ObjectType = "JWriteTicket"
            ticket_write_request.Directive = ticketid
            ticket_write_request.Attachment = contents
            Me.getMailProvider().Send(ticket_write_request, Nothing, "JTicketMaster")
            Return True

        Catch ex As Exception
            Msg("writeTicket: Error requesting ticketwrite: " & ex.Message)
        End Try

        Return False
    End Function

    Public Sub ParseRemoteHTML(ByVal current_remote_url As String)

        Dim b As LionBrowser
        Dim global_fb_events As TriniDATBrowserEvents

        global_fb_events = New TriniDATBrowserEvents

        global_fb_events.OnFormFound = AddressOf Me.OnFacebook_FormDetected
        global_fb_events.OnBeforeSubmit = AddressOf Me.OnFacebook_Form_BeforeSubmit
        global_fb_events.OnElementHasPureText = AddressOf Me.OnFacebook_TextElement
        global_fb_events.OnDocumentEventsComplete = AddressOf Me.OnPageEventsDone
        global_fb_events.OnLink = AddressOf Me.OnFacebookLink_Event
        b = LionBrowser.createFromHTML(Me.all_fb_html, current_remote_url, global_fb_events, AddressOf Msg)

        If IsNothing(b) Then
            'Me.GetIOHandler().writeRaw(True, "Done", True)
            Msg("Error parsing facebook signin page.")
            writeErr("Failed.")
            Exit Sub
        End If

        If Me.isSnapShot() Then

            Msg("Incoming snapshot...")

            Exit Sub
            Me.all_fb_html = Me.all_fb_html

            Dim lnk As TriniDATHTTPTypes.BlingLinkElement
            Dim alllinks

            alllinks = b.getXMLRoot().getAllLinks

            If Not IsNothing(alllinks) Then

                For Each lnk In b.getXMLRoot().getAllLinks
                    Dim innertxt As String
                    innertxt = b.getXMLRoot().extractInnerHTML(lnk, b.getHTMLDoc())
                    '                        If lnk.hasAttribute("data-hovercard") Then

                    'End If

                Next

            Else
                Exit Sub
            End If

        End If

    End Sub

    Public Sub writeErr(ByVal msg As String, Optional ByVal http_status As Integer = 404)
        Me.GetIOHandler().setHTTPResponse(http_status)
        Me.GetIOHandler().writeRaw(True, msg, True)
    End Sub

    Private Sub OnPrivateMessageCounter(ByVal available_messages As Integer)

        If available_messages > 0 Then
            available_messages = available_messages
        End If
    End Sub

    Private Sub OnFacebook_FormDetected(ByVal b As LionBrowser, ByVal blingfrm As TriniDATHTTPTypes.BlingForm)

        Dim frmid As String
        Dim login_page_result As Boolean

        frmid = blingfrm.getId()

        If frmid = "login_form" Then
            login_page_result = Follow_Login_Form(b, blingfrm)
        Else
            frmid = frmid
        End If


    End Sub
    Public Sub OnPendingNotificationsStatus(ByVal count As Integer)

        If count > 0 Then
            'there is activity on account
            count = count
        End If
    End Sub
    Public Sub OnPendingFriendRequestsStatus(ByVal count As Integer)

        If count > 0 Then
            'add friends?
            count = count
        End If
    End Sub
    Public Sub OnFacebookLink_Event(ByVal b As LionBrowser, ByVal el As Object)

        Dim tagid As String
        Dim tagclassname As String
        Dim friend_name As String


        tagclassname = el.getAttribute("class")
        tagid = el.getAttribute("id")

        If tagclassname = "ego_title" Then
            'people you may know item

            friend_name = b.getXMLRoot().extractInnerText(el, b.getHTMLDoc())
            Msg("people you may know item: " & friend_name)
            friend_name = friend_name
        End If

        If el.hasAttribute("datahovercard") Then
            Dim hover_attribute As String
            Dim linktxt As String
            linktxt = b.getXMLRoot().extractInnerHTML(el, b.getHTMLDoc())

            'PROCESS SHARED BY OTHERS - VISIBLE ON OWN TIMELINE.

            hover_attribute = el.getattribute("datahovercard")
            If InStr(hover_attribute, "user.php") Then
                'ANY person on the timeline
                'likely to be a girlfriend etc
                Msg("Person " & linktxt & "  profile link: " & " -> " & el.getAttribute("datahovercard") & " id: " & tagid & " class: " & tagclassname & " link: " & el.getURL())

            ElseIf InStr(hover_attribute, "page.php") Then
                'esp. ANY page link on the timeline.
                Msg("Page name: " & linktxt & " link: " & linktxt & " -> " & el.getAttribute("datahovercard") & " id: " & tagid & " class: " & tagclassname & " link: " & el.getURL())
            Else

                'profile link
                '/ajax/hovercard/hovercard.php?id=100001960666171  == profile picture
                'Leanna Angel Julia -> /ajax/hovercard/user.php?id=100001960666171 == user
                ' Mundo Cuervo -> /ajax/hovercard/page.php?id=400917921517 == page

                Msg("hover link: " & linktxt & " -> " & el.getAttribute("datahovercard") & " id: " & tagid & " class: " & tagclassname & " link: " & el.getURL())
                linktxt = linktxt
            End If

        End If

    End Sub
    Public Sub OnFacebook_TextElement(ByVal b As LionBrowser, ByVal el As TriniDATHTTPTypes.BlingNode, ByVal tagtxt As String)

        Dim stay_on_domain As Boolean
        Dim tagname As String
        Dim tagid As String
        Dim tagclassname As String
        Dim friend_name As String
        Dim linkto As String

        'INIT
        stay_on_domain = True
        tagname = el.getTagName()
        tagid = el.getAttribute("id")
        tagclassname = el.getAttribute("class")
        linkto = el.getAttribute("href")

        If tagname = "span" And tagid <> "" And Not IsNumeric(tagtxt) And InStr(tagtxt, "more") = 0 And Not IsNumeric(tagtxt) And InStr(tagtxt, "like") = 0 And Not IsNumeric(tagtxt) And InStr(tagtxt, "press") = 0 And Not IsNumeric(tagtxt) And InStr(tagtxt, "via") = 0 And Not IsNumeric(tagtxt) And InStr(tagtxt, "view") = 0 And tagtxt <> "??" Then
            'comment author: link  id: *{comment449712631775436_1233369}*
            'comment text: span  id: id: .*{comment449712631775436_1233369}*
            If tagclassname = "UFICommentBody" Then
                linkto = linkto
            End If

            If InStr(tagid, "reactRoot") Then
                Dim comment As String

                comment = tagtxt
                comment = comment

            End If
            Exit Sub
        End If

        If tagname = "a" Then
            Msg(tagtxt & " -> " & linkto & " id: " & tagid & " class: " & tagclassname)


            If tagclassname = "ego_title" Or InStr(linkto, "ref=pymk") > 0 Then
                'people you may know item

                friend_name = tagtxt
                Msg("people you may know item: " & friend_name & " -> " & linkto)
                friend_name = friend_name
            ElseIf InStr(linkto, "profile.php") > 0 Then
                'people you may know item

                friend_name = tagtxt
                Msg("person: " & friend_name & " -> " & linkto)
                friend_name = friend_name
            End If

            If InStr(tagclassname, "Actor") > 0 Or InStr(tagclassname, "passive") > 0 Or el.hasAttribute("data-hovercard") = True Then

                If tagclassname = "UFICommentActorName" Then
                    'person on timeline
                    friend_name = tagtxt
                    Msg("friend thread commenter: " & friend_name & " -> " & linkto)
                    friend_name = friend_name

                Else
                    'person on timeline
                    friend_name = tagtxt
                    Msg("existing friend: " & friend_name & " -> " & linkto)
                    friend_name = friend_name


                End If
            End If

            If InStr(linkto, "/pages") > 0 Then
                Msg("Facebook page: name: " & tagtxt & " -> " & linkto)
            End If
        End If

        If tagname = "div" Then


            If tagclassname = "name" Then
                friend_name = tagtxt
                Msg(Facebook_current_User & " -> friend: " & friend_name)

                Me.friend_count = Me.friend_count + 1

                Dim friend_div_html As String
                Dim friend_div_b As LionBrowser

                friend_div_html = b.getXMLRoot().extractInnerHTML(el, b.getHTMLDoc())
                friend_div_b = b.getNodeBrowser(el, Nothing, "friend_browser")

                If Not IsNothing(friend_div_b) Then


                    For Each lnk As TriniDATHTTPTypes.BlingLinkElement In friend_div_b.getXMLRoot().getAllLinks()
                        linkto = lnk.getURL()
                        linkto = linkto
                    Next

                End If
            End If
        End If

        If tagname = "ul" And InStr(tagid, "chat") > 0 Then

            tagname = tagname

        End If


        'PARSE STATES
        If Me.isFullPage() Then

            Msg(b.getURL() & " page element: " & tagname & "." & tagid & " = " & tagtxt)
            If tagname = "span" Then


                If tagid = "requestsCountValue" Then
                    Call OnPendingFriendRequestsStatus(CInt(tagtxt))
                ElseIf tagid = "notificationsCountValue" Then
                    Call OnPendingNotificationsStatus(CInt(tagtxt))
                ElseIf tagid = "mercurymessagesCountValue" Then
                    Call onLoggedIn()
                    'parse message count.
                    If IsNumeric(tagtxt) Then
                        Call Me.OnPrivateMessageCounter(CInt(tagtxt))
                    End If
                End If
            End If
        ElseIf Me.isSnippet() Then

            Msg(b.getURL() & " snippet element: " & tagname & "." & tagid & " = " & tagtxt)

            tagid = tagid

        End If

    End Sub


    Public Function Follow_Login_Form(ByVal b As LionBrowser, ByVal login_frm As TriniDATHTTPTypes.BlingForm) As Boolean
        Dim frmid As String
        Dim inputs As BlingInputElementCollection
        Dim submit As BlingFormSubmitButton
        Dim onsubmit As String
        Dim fieldvalue As String

        Msg("=====")
        Msg(login_frm.getMethod().ToString("G") & ".FORM ID: " & frmid)
        Msg(login_frm.getMethod().ToString("G") & " = " & login_frm.getAction())

        frmid = login_frm.getMethod().ToString("G") & "." & frmid
        inputs = login_frm.getInputFields()
        submit = login_frm.getSubmit()
        onsubmit = login_frm.getFormOnSubmitAction()

        If onsubmit <> "" Then
            onsubmit = onsubmit
        End If

        For Each inp As BlingInputElement In inputs
            fieldvalue = inp.getAttribute("value")
            Msg(frmid & "." & inp.getAttribute("name") & " = " & fieldvalue)
        Next

        Msg("submit: " & IIf(IsNothing(submit), "found", "(" & onsubmit & ")"))

        Msg("=====")


        Dim facebook_login_frm As BlingEditableForm


        facebook_login_frm = BlingEditableForm.createFrom(login_frm)
        facebook_login_frm.setDebuggerLog(AddressOf Msg)
        Msg("Submitting " & facebook_login_frm.getId())

        Dim current_login_field As BlingEditableInputElement

        current_login_field = facebook_login_frm.getByNameOrId("email")
        If Not current_login_field.setAttribute("value", Me.UserName) Then
            MsgBox("Error setting form field value. ")
            Return False
        End If

        current_login_field = facebook_login_frm.getByNameOrId("pass")
        If Not current_login_field.setAttribute("value", Me.Password) Then
            MsgBox("Error setting form field value. ")
            Return False
        End If

        'simulate submit 
        Dim fb_login_submit_browser As LionBrowser
        fb_login_submit_browser = facebook_login_frm.Submit(b.getEventModel())

        Return Not IsNothing(fb_login_submit_browser)
    End Function

    Public Function generatefollowupTicketId() As String

        Dim from_ticketid As String
        Dim retval As String
        Dim x As Integer

        from_ticketid = Me.getTicket()

        retval = ""

        For x = 1 To (Len(from_ticketid) / 2)

            retval = retval & AscW(Mid(from_ticketid, x, 1))

        Next

        Return retval
    End Function

    Public Function OnFacebook_Form_BeforeSubmit(ByVal fromb As Object, ByVal frmmethod As String, ByVal blingformobj As Object) As Boolean
        'return false to cancel navigation.

        Dim frmid As String
        frmid = blingformobj.getId()

        If TypeOf (blingformobj) Is BlingEditableForm Then
            Dim mutations As String

            If frmid = "login_form" Then
                Dim tab_directives As String
                Dim followup_ticket As String

                'flag as handled
                Me.null_instruction = False

                mutations = blingformobj.AllMutations
                Msg("could delete form: " & frmmethod & "." & blingformobj.getId())
                'show mutations
                Msg("========")
                Msg(blingformobj.getId() & " OnBeforePost...")
                Msg(blingformobj.AllMutations)
                Msg("========")

                'set boot ticket to enable snippet sending.
                followup_ticket = generatefollowupTicketId()

                'add snippet directive
                tab_directives = "boot_ticket=" & followup_ticket & vbCrLf
                tab_directives &= "xusername=" & HttpUtility.UrlEncode(Me.UserName) & vbCrLf
                tab_directives &= blingformobj.AllMutations & vbCrLf

                'write tickets and exit
                Me.writeTicket(Me.getTicket(), tab_directives)
                Me.writeTicket(followup_ticket, "snapshot=true" & vbCrLf)

                Me.GetIOHandler().writeRaw(True, "OK " & Me.getTicket(), True)
                onLogIn()
                Return False
            Else
                writeErr("Unknown login form: " & frmid)
            End If
        Else
            writeErr("Facebook form submission: " & frmid)
        End If

    End Function

    Public Sub onLogIn()
        Msg("Logging in to Facebook...")
    End Sub
    Public Sub onLoggedIn()
        Msg("Logged in to Facebook.")
    End Sub
    Public Function getTicket() As String
        Return Me.current_ticketid
    End Function
    Public Sub setTicket(ByVal ticketid As String)
        Me.current_ticketid = ticketid
    End Sub
    Public Sub setLoginCredentials()
        Me.UserName = "bennybannasi1313@gmail.com"
        Me.Password = "doodle18"
    End Sub

    Private Sub OnFacebookBotSessionStart()
        setLoginCredentials()
    End Sub
    Private Sub OnFacebookBotResumeSession(ByVal username As String)
        Msg("In session: " & username)
        Me.Facebook_current_User = username
    End Sub



    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")
        ' runtimeCount = CInt(HTTP_URI_Headers("Content-Length"))

        Dim ticketid As String
        Dim fb_context As String
        Dim xusername As String

        fb_context = HTTP_URI_Headers("X-Facebook-Context")
        Me.Facebook_URL = HTTP_URI_Headers("X-URL")
        xusername = HTTP_URI_Headers("X-USER")

        If xusername = "undefined" Or InStr(Facebook_URL, "facebook.com/index.php") Then
            Call OnFacebookBotSessionStart()
        Else
            xusername = HttpUtility.UrlDecode(xusername)
            Call OnFacebookBotResumeSession(xusername)
        End If

        'EXTRACT URL PARAMETERS
        ticketid = Mid(HTTP_URI_Path, InStrRev(HTTP_URI_Path, "/") + 1)

        If ticketid = "" Then
            Me.GetIOHandler().setHTTPResponse(404)
            Me.GetIOHandler().writeRaw(True, "Error: Unique TicketId required.", True)
            Exit Sub
        End If

        Me.current_ticketid = ticketid

        Select Case fb_context

            Case "Snippet"
                Me.setSnippetType(FacebookRemoteType.Snippet)

            Case "Page"
                Me.setSnippetType(FacebookRemoteType.Page)

            Case "Snapshot"
                Me.setSnippetType(FacebookRemoteType.Snapshot)


        End Select

        Msg("Incoming request. Ticket: " & Me.getTicket() & " fb page-context: " & fb_context)
        'Dim pagekey As String

        'receive login page html
        'pagekey = HTTP_URI_Parameters("pagekey")
        Me.all_fb_html = HTTP_URI_Parameters("alldata")

        'this might take some time..
        Me.disableJOMEGA()

        If Me.isSnapShot() Then

            Msg("Incoming snapshot...")
            Me.all_fb_html = Me.all_fb_html
        End If

    End Sub

End Class


Public Enum FacebookRemoteType

    Snippet = 1
    Page = 2
    Snapshot = 3

End Enum