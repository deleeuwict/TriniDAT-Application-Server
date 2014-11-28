Option Explicit On
Option Compare Text

Imports System.Reflection
Imports System.Text
Imports System.Web
Imports TriniDATBrowserEvent

Public Class TriniDATInputElement
    Inherits TriniDATParsedHTMLElement

    Public Sub New(ByVal _charsWidth As Integer)
        MyBase.New(_charsWidth)
        Me.setTagName("input")
    End Sub

    Public Shared Function createFrom(ByVal _node As TriniDATNode) As TriniDATInputElement

        Dim retval As TriniDATInputElement

        retval = New TriniDATInputElement(_node.getCharsWidth())
        'copy all node attributes
        retval.setID(_node.getId())
        retval.setTagName("input")
        retval.foundEndTag = _node.foundEndTag
        retval.setAttributes(_node.getAttributes(), _node.getAttributeContainerchar())
        retval.Issingleton = _node.Issingleton
        'copy parser information
        retval.html_src_endpos = _node.html_src_endpos
        retval.html_src_startpos = _node.html_src_startpos
        retval.xml_str_endpos = _node.xml_str_endpos
        retval.xml_str_startpos = _node.xml_str_startpos

        'done
        Return retval
    End Function

End Class
