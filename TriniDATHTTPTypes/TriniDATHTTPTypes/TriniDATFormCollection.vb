
Public Class TriniDATFormCollection
    Inherits List(Of TriniDATGenericNodeFamily)


    Public ReadOnly Property haveForms() As Boolean
        Get
            Return Not (Me.Count = 0)
        End Get
    End Property

End Class
