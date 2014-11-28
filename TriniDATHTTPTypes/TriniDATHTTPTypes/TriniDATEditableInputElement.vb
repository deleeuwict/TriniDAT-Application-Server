'shadows all mutation functions of base class.
Public Class TriniDATEditableInputElement
    Inherits TriniDATInputElement

    Public Delegate Sub MutationEvent(ByVal name As String, ByVal old_value As String, ByVal new_value As String)

    Private original_input_field As TriniDATInputElement
    Private my_mutation_callback As MutationEvent

    Public Sub New(ByVal _charsWidth As Integer, ByVal _callback As MutationEvent)
        MyBase.New(_charsWidth)
        Me.setMutationCallback(_callback)
        Me.setTagName("input")
    End Sub

    Public Sub setMutationCallback(ByVal val As MutationEvent)
        Me.my_mutation_callback = val
    End Sub

    Public ReadOnly Property getMutationCallback() As MutationEvent
        Get
            Return Me.my_mutation_callback
        End Get
    End Property

    Public Sub setOriginal(ByVal inputel As TriniDATInputElement)
        Me.original_input_field = inputel
    End Sub

    Public ReadOnly Property getOriginal() As TriniDATInputElement
        Get
            Return Me.original_input_field
        End Get
    End Property

    Public ReadOnly Property haveOriginalInput() As Boolean
        Get
            Return Not IsNothing(Me.original_input_field)
        End Get
    End Property

    Public ReadOnly Property getOriginalId() As String
        Get
            If Me.haveOriginalInput() Then
                Return Me.getOriginal().getOrAttribute("name", "id")
            Else
                Return "unknown"
            End If
        End Get
    End Property

    Public Shadows Function setAttribute(ByVal name As String, ByVal new_value As String) As Boolean
        'invoke mutation logger
        Dim retval As Boolean
        Dim old_value As String

        old_value = ""
        If MyBase.hasAttribute(name) Then
            old_value = MyBase.getAttribute(name)
        End If

        retval = MyBase.setAttribute(name, new_value)

        If retval = True Then
            Me.my_mutation_callback(Me.getOriginalId(), old_value, new_value)
        End If

        Return retval
    End Function


    Public Shared Shadows Function createFrom(ByVal val As TriniDATInputElement, ByVal _callback_func As MutationEvent) As TriniDATEditableInputElement
        Dim retval As TriniDATEditableInputElement

        retval = New TriniDATEditableInputElement(val.getCharsWidth(), _callback_func)
        'copy all node attributes
        retval.setID(val.getId())
        retval.setTagName("input")
        retval.setOriginal(val)
        retval.foundEndTag = val.foundEndTag
        retval.setAttributes(val.getAttributes(), val.getAttributeContainerchar())
        retval.Issingleton = val.Issingleton
        'copy parser information
        retval.html_src_endpos = val.html_src_endpos
        retval.html_src_startpos = val.html_src_startpos
        retval.xml_str_endpos = val.xml_str_endpos
        retval.xml_str_startpos = val.xml_str_startpos

        'done
        Return retval

    End Function
End Class
