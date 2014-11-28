Public Class TriniDATFormSubmitButton
    Inherits TriniDATInputElement

    Public Sub New(ByVal _charsWidth As Integer)
        MyBase.New(_charsWidth)
        Me.setTagName("input")
    End Sub

    Public Shared Shadows Function createFrom(ByVal inputel As TriniDATInputElement) As TriniDATFormSubmitButton
        Dim retval As TriniDATFormSubmitButton

        retval = New TriniDATFormSubmitButton(inputel.getCharsWidth())
        'copy all node attributes
        retval.setID(inputel.getId())
        retval.setTagName("input")
        retval.foundEndTag = inputel.foundEndTag
        retval.setAttributes(inputel.getAttributes(), inputel.getAttributeContainerchar())
        retval.Issingleton = inputel.Issingleton
        'copy parser information
        retval.html_src_endpos = inputel.html_src_endpos
        retval.html_src_startpos = inputel.html_src_startpos
        retval.xml_str_endpos = inputel.xml_str_endpos
        retval.xml_str_startpos = inputel.xml_str_startpos

        'done
        Return retval

    End Function
End Class
