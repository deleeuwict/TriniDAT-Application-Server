Option Explicit On
Option Compare Text

Public Class TriniDATGenericNodeFamily

    Protected parentnode As TriniDATNode
    Protected children As List(Of TriniDATNode)

    Public Sub New()
        Me.parentnode = Nothing
        Me.children = Nothing
    End Sub

    Public Sub New(ByVal _parent As TriniDATNode, ByVal _child_nodes As List(Of TriniDATNode))
        Me.parentnode = _parent
        Me.setChildren(_child_nodes)

    End Sub

    Public Function haveChildren() As Boolean

        If Not IsNothing(Me.children) Then
            Return True
        Else
            Return False
        End If

    End Function
    Public Overridable ReadOnly Property getParent() As TriniDATNode
        Get
            Return Me.parentnode
        End Get
    End Property
    Public Overridable ReadOnly Property getChildren() As List(Of TriniDATNode)
        Get
            Return Me.children
        End Get
    End Property

    Public Overridable Sub setChildren(ByVal val As List(Of TriniDATNode))
        Me.children = val
    End Sub

    Public Overridable Sub setParent(ByVal val As TriniDATNode)
        Me.parentnode = val
    End Sub

End Class
