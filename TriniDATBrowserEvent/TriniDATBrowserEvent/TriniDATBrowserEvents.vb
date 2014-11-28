Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class TriniDATBrowserEvents

    'abstract declarations
    Public Delegate Sub OnDocumentDownloading_Event(ByVal b As Object)
    Public Delegate Sub OnDocumentEventsComplete_Event(ByVal b As Object)
    Public Delegate Sub OnDownloadError_Event(ByVal b As Object, ByVal msg As String)
    Public Delegate Sub OnDocumentComplete_Event(ByVal b As Object)
    Public Delegate Sub OnDocumentParseStart_Event(ByVal b As Object, ByVal src As String)
    Public Delegate Sub OnElementHasPureText_Event(ByVal b As Object, ByVal blingnode As Object, ByVal eltxt As String)
    Public Delegate Sub OnLink_Event(ByVal b As Object, ByVal el As Object)

    Public Delegate Sub OnForm_Event(ByVal b As Object, ByVal blingform As Object)

    'OnLinkClicked_Event: return false = cancel click event.
    Public Delegate Function OnLinkClicked_Event(ByVal frombrowser As Object, ByVal blinglinkelement As Object, ByRef unfired_new_browser_object As Object) As Boolean
    'OnLinkClicked_Event: return false = cancel form submission.
    Public Delegate Function OnBeforeSubmit_Event(ByVal frmbrowser As Object, ByVal frmmethod As String, ByVal blingform As Object) As Boolean

    Public Events_ondoc_complete As TriniDATBrowserEvents.OnDocumentComplete_Event
    Public Events_ondoc_downloading As TriniDATBrowserEvents.OnDocumentDownloading_Event
    Public Events_ondoc_download_err As TriniDATBrowserEvents.OnDownloadError_Event
    Public Events_ondoc_parse_start As TriniDATBrowserEvents.OnDocumentParseStart_Event
    Public Events_ondoc_text_element As TriniDATBrowserEvents.OnElementHasPureText_Event
    Public Events_ondoc_found_form As TriniDATBrowserEvents.OnForm_Event
    Public Events_ondoc_link As TriniDATBrowserEvents.OnLink_Event
    Public Events_ondoc_events_complete As OnDocumentEventsComplete_Event
    Public Events_ondoc_onlinkclicked As OnLinkClicked_Event
    Public Events_ondoc_onbeforesubmit As OnBeforeSubmit_Event

    Public Sub New()
        Me.ClearEvents()
    End Sub
    Public Property OnDocumentEventsComplete As OnDocumentEventsComplete_Event
        Get
            Return Me.Events_ondoc_events_complete
        End Get
        Set(ByVal value As OnDocumentEventsComplete_Event)
            Me.Events_ondoc_events_complete = value
        End Set
    End Property

    Public Property OnDocumentDownloading As OnDocumentDownloading_Event
        Get
            Return Me.Events_ondoc_downloading
        End Get
        Set(ByVal value As OnDocumentDownloading_Event)
            Me.Events_ondoc_downloading = value
        End Set
    End Property
    Public Property OnDocumentComplete As OnDocumentComplete_Event
        Get
            Return Me.Events_ondoc_complete
        End Get
        Set(ByVal value As OnDocumentComplete_Event)
            Me.Events_ondoc_complete = value
        End Set
    End Property
    Public Property OnLink As OnLink_Event
        Get
            Return Me.Events_ondoc_link
        End Get
        Set(ByVal value As OnLink_Event)
            Me.Events_ondoc_link = value
        End Set
    End Property
    Public Property OnLinkClicked As OnLinkClicked_Event
        Get
            Return Me.Events_ondoc_onlinkclicked
        End Get
        Set(ByVal value As OnLinkClicked_Event)
            Me.Events_ondoc_onlinkclicked = value
        End Set
    End Property
    Public Property OnBeforeSubmit As OnBeforeSubmit_Event
        Get
            Return Me.Events_ondoc_onbeforesubmit
        End Get
        Set(ByVal value As OnBeforeSubmit_Event)
            Me.Events_ondoc_onbeforesubmit = value
        End Set
    End Property

    Public Property OnDocumentParseStart As OnDocumentParseStart_Event
        Get
            Return Me.Events_ondoc_parse_start
        End Get
        Set(ByVal value As OnDocumentParseStart_Event)
            Me.Events_ondoc_parse_start = value
        End Set
    End Property
    Private Sub ClearEvents()
        Me.Events_ondoc_complete = AddressOf Me.NullEvent
        Me.Events_ondoc_download_err = AddressOf Me.NullEvent
        Me.Events_ondoc_downloading = AddressOf Me.NullEvent
        Me.Events_ondoc_parse_start = AddressOf Me.NullEvent
        Me.Events_ondoc_text_element = AddressOf Me.NullEvent
        Me.Events_ondoc_found_form = AddressOf Me.NullEvent
        Me.Events_ondoc_link = AddressOf Me.NullEvent
        Me.Events_ondoc_events_complete = AddressOf Me.NullEvent
        Me.Events_ondoc_onlinkclicked = AddressOf Me.TrueNullEvent
        Me.Events_ondoc_onbeforesubmit = AddressOf Me.TrueNullEvent
    End Sub

    Public Property OnFormFound As OnForm_Event
        Get
            Return Me.Events_ondoc_found_form
        End Get
        Set(ByVal value As OnForm_Event)
            Me.Events_ondoc_found_form = value
        End Set
    End Property
    Public Property OnElementHasPureText As OnElementHasPureText_Event
        Get
            Return Me.Events_ondoc_text_element
        End Get
        Set(ByVal value As OnElementHasPureText_Event)
            Me.Events_ondoc_text_element = value
        End Set
    End Property

    Public Property OnDocumentFetchError As OnDownloadError_Event
        Get
            Return Me.Events_ondoc_download_err
        End Get
        Set(ByVal value As OnDownloadError_Event)
            Me.Events_ondoc_download_err = value
        End Set
    End Property
    Public Sub NullEvent()

    End Sub

    Public Sub NullEvent(ByVal b As Object)

    End Sub
    Public Sub NullEvent(ByVal b As Object, ByVal val As String)

    End Sub
    Public Sub NullEvent(ByVal b As Object, ByVal val As Object)

    End Sub
    Public Sub NullEvent(ByVal b As Object, ByVal el As Object, ByVal caption As String)

    End Sub
    Public Function TrueNullEvent(ByVal fromb As Object, ByVal el As Object, ByRef tob As Object) As Boolean
        Return True
    End Function
    Public Function TrueNullEvent(ByVal fromb As Object, ByVal frmmethod As String, ByVal blingformobj As Object) As Boolean
        Return True
    End Function

End Class
