Public Class TriniDAT_ServerFunctionParameterSpec
    'aka BosswaveAppParameter
    Private _id As String 'parameter name
    Private _value_prototypeid As String
    Private _required As Boolean
    Public Property Required As Boolean
        Get
            Return Me._required
        End Get
        Set(ByVal value As Boolean)
            Me._required = value
        End Set
    End Property
    Public ReadOnly Property haveParameterType As Boolean
        Get
            Return Len(Me.ParameterType) > 0
        End Get
    End Property
    Public ReadOnly Property haveParameterName As Boolean
        Get
            Return Len(Me.ParameterName) > 0
        End Get
    End Property
    Public Property ParameterType As String
        Get
            ' prototype name
            Return Me._value_prototypeid
        End Get
        Set(ByVal value As String)
            Me._value_prototypeid = value
        End Set
    End Property

    Public Property ParameterName As String
        Get
            ' prototype name
            Return Me._id
        End Get
        Set(ByVal value As String)
            Me._id = value
        End Set
    End Property

    Public Sub New()
        Me.Required = True
    End Sub
End Class