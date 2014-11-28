Option Explicit On
Option Compare Text

Imports System.Reflection
Imports System.Text
Imports System.Web
Imports TriniDATBrowserEvent


Public Class TriniDATLinkElement
    Inherits TriniDATParsedHTMLElement

    Private link_type As TriniDATLinkType

    Public Sub New(ByVal _charsWidth As Integer)
        MyBase.New(_charsWidth)
        Me.setTagName("a")
    End Sub

    Public Property LinkType As TriniDATLinkType
        Get
            Return Me.link_type
        End Get
        Set(ByVal value As TriniDATLinkType)
            Me.link_type = value
        End Set
    End Property
    Public ReadOnly Property getURL As String
        Get
            If Me.hasAttribute("href") Then
                Return Me.getAttribute("href")
            Else
                Return ""
            End If
        End Get
    End Property
    Public ReadOnly Property UniqueId() As String
        Get
            Dim retval As String
            retval = Me.getURL()
            retval = Replace(retval, "?", "_")
            retval = Replace(retval, ":", "_")
            retval = Replace(retval, "/", "_")
            retval = Replace(retval, "\", "_")
            Return retval
        End Get
    End Property
    Public Sub setURL(ByVal val As String)
        Me.setAttribute("href", val)
    End Sub

    Public ReadOnly Property isProtocolHandler() As Boolean
        Get
            Dim newlink As String
            newlink = Me.getURL()

            If Left(newlink, 10) = "javascript" Then Return True
            If Left(newlink, 6) = "mailto" Then Return True

            Return False

        End Get
    End Property

    Public Function Click(ByVal parent_browser As Object, Optional ByVal new_lion_browser_event_model As TriniDATBrowserEvent.TriniDATBrowserEvents = Nothing) As Object
        'returns a new LionBrowser instance of the target page.

        Dim client_asm As Assembly
        Dim b As Object

        client_asm = System.Reflection.Assembly.GetCallingAssembly()

        b = TriniDATHTMLTools.createLionBrowserInstance(client_asm)

        If IsNothing(b) Then
            Err.Raise(0, 0, "Form.Submit(): Unable to create BlingBrowser obj.")
            Return Nothing
        End If

        'Configure the Lion browser instance.
        If Not IsNothing(parent_browser) Then
            If IsNothing(new_lion_browser_event_model) Then
                'copy event model from parent browser.
                new_lion_browser_event_model = parent_browser.getEventModel()
            End If

            'apply event model
            If Not IsNothing(new_lion_browser_event_model) Then
                Call b.setEventModel(new_lion_browser_event_model)
            End If
        End If

        Try

            b.setURL(Me.getURL())

            'Fire OnClicked - must return true.
            Dim parentEventModel As TriniDATBrowserEvents

            parentEventModel = parent_browser.getEventModel()

            If parentEventModel.Events_ondoc_onlinkclicked(parent_browser, Me, b) Then
                'the owning LionBrowser of this element should replace relative links with absolute ones, thus making them appear as navigatable.
                If b.execGet(Me.getURL()) Then
                    Return b
                End If
            Else
                Return Nothing
            End If

        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    Public Shared Function createFrom(ByVal _node As TriniDATNode) As TriniDATLinkElement

        Dim retval As TriniDATLinkElement
        Dim url As String

        retval = New TriniDATLinkElement(_node.getCharsWidth())
        'copy all node attributes
        retval.setID(_node.getId())
        retval.setTagName("a")
        retval.foundEndTag = _node.foundEndTag
        retval.setAttributes(_node.getAttributes(), _node.getAttributeContainerchar())
        retval.Issingleton = _node.Issingleton
        'copy parser information
        retval.html_src_endpos = _node.html_src_endpos
        retval.html_src_startpos = _node.html_src_startpos
        retval.xml_str_endpos = _node.xml_str_endpos
        retval.xml_str_startpos = _node.xml_str_startpos

        url = Trim(retval.getURL())

        If url <> "" Then
            'classify url type 
            If Left(url, 1) = "#" Then
                retval.LinkType = TriniDATLinkType.LINK_TYPE_HASHCODE
            Else

                If Left(url, 4) = "http" Then
                    retval.LinkType = TriniDATLinkType.LINK_TYPE_ABSOLUTE
                Else
                    retval.LinkType = TriniDATLinkType.LINK_TYPE_RELATIVE
                End If

            End If

        Else
            retval.LinkType = TriniDATLinkType.LINK_TYPE_OTHER
        End If

        'done
        Return retval
    End Function

End Class
