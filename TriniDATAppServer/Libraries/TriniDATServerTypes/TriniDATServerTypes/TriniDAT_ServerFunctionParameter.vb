Public Class TriniDAT_ServerFunctionParameter
    'aka BosswaveAppParameter
    Private _id As String 'parameter name
    Private _value_prototypeid As String
    Private _bosswave_parameter_instance As Object '=BosswaveAppParameter
    Private _required As Boolean
    Private _found As Boolean 'tagged true if initialization routine found it in http var list.
    Public ReadOnly Property haveBosswaveParameter As Boolean
        Get
            Return Not IsNothing(Me._bosswave_parameter_instance)
        End Get
    End Property
    Public Property Found As Boolean
        Get
            Return Me._found
        End Get
        Set(ByVal value As Boolean)
            Me._found = value
        End Set
    End Property
    Public Property Required As Boolean
        Get
            Return Me._required
        End Get
        Set(ByVal value As Boolean)
            Me._required = value
        End Set
    End Property
    Public Property BosswaveParameter As Object
        Get
            ' prototype name
            Return Me._bosswave_parameter_instance
        End Get
        Set(ByVal value As Object)
            Me._bosswave_parameter_instance = value

            If Me.haveBosswaveParameter Then
                Me.ParameterPrototypeID = Me.BosswaveParameter.PrototypeId
                'set internal parameter name
                Me.BosswaveParameter.displayName = Me.ID
            End If
        End Set
    End Property
    Public Property ParameterPrototypeID As String
        Get
            ' prototype name
            Return Me._value_prototypeid
        End Get
        Set(ByVal value As String)
            Me._value_prototypeid = value
        End Set
    End Property
    Public Property ParameterValue As String
        Get
            If Not haveBosswaveParameter Then
                Throw New Exception("Cannot determine value. Parameter Prototype is not initialized.")
            End If

            ' prototype name
            Return Me._bosswave_parameter_instance.value
        End Get
        Set(ByVal value As String)

            If Not haveBosswaveParameter Then
                Throw New Exception("Cannot determine value. Parameter Prototype is not initialized.")
            End If

            Me._bosswave_parameter_instance.value = value
        End Set
    End Property
    Public Property ID As String
        Get
            ' prototype name
            Return Me._id
        End Get
        Set(ByVal value As String)
            Me._id = value

            If Me.haveBosswaveParameter Then
                 'set internal parameter name
                Me.BosswaveParameter.displayName = Me.ID
            End If
        End Set
    End Property
End Class