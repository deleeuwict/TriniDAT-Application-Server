Public Class TriniDATEditableInputCollection
    Inherits List(Of TriniDATEditableInputElement)

    Private my_mutation_passer As TriniDATEditableInputElement.MutationEvent

    Public Sub New()
        'configurable event handler
        Me.currentMutationHandler = AddressOf NullEvent

    End Sub
    Public Shared Function createFrom(ByVal val As TriniDATInputElementCollection) As TriniDATEditableInputCollection

        Dim retval As TriniDATEditableInputCollection
        Dim editable_type As TriniDATEditableInputElement

        retval = New TriniDATEditableInputCollection()

        For Each input_el As TriniDATInputElement In val

            'sets the default handler to NullEvent. Should become configured by parent EditableForm.
            editable_type = TriniDATEditableInputElement.createFrom(input_el, retval.currentMutationHandler)
            retval.Add(editable_type)
        Next

        Return retval

    End Function

    Public Sub NullEvent(ByVal inputname As String, ByVal old_value As String, ByVal new_value As String)
    End Sub

    Public Property currentMutationHandler As TriniDATEditableInputElement.MutationEvent
        Get
            Return Me.my_mutation_passer
        End Get
        Set(ByVal value As TriniDATEditableInputElement.MutationEvent)
            Me.my_mutation_passer = value
            'apply to all children

            For Each input_field As TriniDATEditableInputElement In Me
                input_field.setMutationCallback(Me.my_mutation_passer)
            Next

        End Set
    End Property


End Class
