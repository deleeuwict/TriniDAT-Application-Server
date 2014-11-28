Option Compare Text
Option Explicit On

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports TriniDATHTTPBrowser
Imports TriniDATBrowserEvent


Public Class JWebBrowser
    Inherits JTriniDATWebService
    Private browser_events As TriniDATBrowserEvents
    Private collected_links As TriniDATHTTPTypes.TriniDATLinkCollection
    Private cur_op As String
    Private cur_op_org_request As JSONObject
    Private b As TriniDATHTTPBrowser.TriniDATHTTPBrowser
    Private collected_text As String

    Private Property CurrentOperation As String
        Get
            Return Me.cur_op
        End Get
        Set(ByVal value As String)
            Me.cur_op = value
        End Set
    End Property

    Private Property CurrentOperationRequest As JSONObject
        Get
            Return Me.cur_op_org_request
        End Get
        Set(ByVal value As JSONObject)
            Me.cur_op_org_request = value
        End Set
    End Property
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
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
        getIOHandler().Configure(http_events)

        browser_events = New TriniDATBrowserEvents
        browser_events.Events_ondoc_complete = AddressOf OnBrowser_DocumentCompleted
        browser_events.Events_ondoc_download_err = AddressOf OnBrowser_OnDownloadError_Event

        Dim l As Object
        l = GlobalObject.OfficialLicense

        b = New TriniDATHTTPBrowser.TriniDATHTTPBrowser(l, AddressOf Me.Msg)

        b.AnalyticalEventsEnabled = True
        b.setEventModel(browser_events)

        Return True
    End Function
    Public Sub OnBrowserLink_Event(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATLinkElement)
        collected_links.Add(el)
        '   GlobalObject.Msg("Link: " & el.getURL())
    End Sub
    Public Sub OnBrowser_DocumentCompleted(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser)
        'send message

        Call FinishOperation(True)


    End Sub
    Public Sub OnBrowser_OnDownloadError_Event(ByVal b As Object, ByVal msg As String)
        Call FinishOperation(False, "Connection failure.")
    End Sub

    Public Sub OnBrowser_OnTextElement(ByVal b As TriniDATHTTPBrowser.TriniDATHTTPBrowser, ByVal el As TriniDATHTTPTypes.TriniDATNode, ByVal text As String)
        Me.collected_text &= text & vbNewLine
    End Sub

    Public Sub FinishOperation(ByVal success As Boolean, Optional ByVal err_msg As String = "")
        Dim reply As JSONObject

        reply = New JSONObject

        If success Then
            If Me.IsLinkOperation Then
                reply.Attachment = Me.collected_links.getByURLsUnique()
            End If

            If Me.IsTextOperation Then
                reply.Attachment = Trim(Me.collected_text)
            End If

        Else
            reply.Attachment = err_msg
        End If

        'send reply
        reply.ObjectType = Me.CurrentOperation & "Result"
        reply.Directive = b.getURL()
        reply.Tag = Me.CurrentOperationRequest.Tag
        reply.Tag = b.getURL()
        Me.getMailProvider().Send(reply, Nothing, Me.CurrentOperationRequest.Sender.getClassNameFriendly())


        CurrentOperation = ""
        Me.CurrentOperationRequest = Nothing

    End Sub

    Public ReadOnly Property IsLinkOperation As Boolean
        Get
            Return InStr(Me.CurrentOperation, "link") > 0
        End Get
    End Property
    Public ReadOnly Property IsTextOperation As Boolean
        Get
            Return InStr(Me.CurrentOperation, "text") > 0
        End Get
    End Property

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.haveObjectTypeName Then
            If obj.ObjectTypeName = "JBrowseRequest" Then
                If obj.haveDirective And obj.haveDirective And obj.haveSender Then

                    Dim url As String

                    url = obj.Attachment

                    If Left(obj.Directive, 3) = "Get" Then
                        'URL =  obj.Content

                        If Not IsNothing(Me.CurrentOperationRequest) Then
                            Dim error_reply As JSONObject
                            error_reply = New JSONObject
                            error_reply.Attachment = Me.getClassNameFriendly() & " must be invoked serially - not multi-threaded."
                            'bounce request.
                            error_reply.ObjectType = "ERROR"
                            error_reply.Directive = obj.Directive
                            error_reply.Sender = Me
                            Me.getMailProvider().send(error_reply, Nothing, obj.Sender.getClassNameFriendly())
                            Return False
                        Else
                            Me.CurrentOperation = obj.Directive
                            Me.CurrentOperationRequest = obj
                        End If

                        If Me.IsLinkOperation Then
                            Me.collected_links = New TriniDATHTTPTypes.TriniDATLinkCollection
                            browser_events.Events_ondoc_link = AddressOf OnBrowserLink_Event
                        End If

                        If Me.IsTextOperation Then
                            Me.collected_text = ""
                            browser_events.Events_ondoc_text_element = AddressOf OnBrowser_OnTextElement
                        End If


                        If Not b.execGet(url) Then
                            'inform of error

                            Dim error_reply As JSONObject
                            error_reply = New JSONObject
                            error_reply.Attachment = "GETERR"
                            error_reply.Directive = obj.Directive
                            error_reply.Sender = Me
                            Me.getMailProvider().send(error_reply, Nothing, obj.Sender.getClassNameFriendly())
                        End If

                        Return False
                    End If
                End If
            End If
        End If


        Return False
    End Function


End Class
