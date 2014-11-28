Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 
Public Class SimonConsoleState
    Private ctx As SimonConsoleContext

    Public Property CommandContext As SimonConsoleContext
        Get
            Return Me.ctx
        End Get
        Set(ByVal value As SimonConsoleContext)
            Me.ctx = value
        End Set
    End Property

    Public ReadOnly Property asXMLIdentifierString As String
        Get
            If Me.ctx = SimonConsoleContext.SERVER_LIVE Then
                Return "server_live"
            ElseIf Me.ctx = SimonConsoleContext.SERVER_DEV Then
                Return "server_dev"
            ElseIf Me.ctx = SimonConsoleContext.DEBUG_CONTEXT_APP Then
                Return "debug_app"
            ElseIf Me.ctx = SimonConsoleContext.DEBUG_CONTEXT_APP_ACTION Then
                Return "debug_action"
            ElseIf Me.ctx = SimonConsoleContext.DEBUG_CONTEXT_APP_MAPPING_POINT Then
                Return "debug_mp"
            ElseIf Me.ctx = SimonConsoleContext.DEBUG_CONTEXT_DEBUGOBJECT Then
                Return "debug_object"
            Else
                Return "all"
            End If
        End Get
    End Property
End Class

Public Enum SimonConsoleContext
    SERVER_LIVE = 1
    SERVER_DEV = 2
    DEBUG_CONTEXT_APP = 3
    DEBUG_CONTEXT_APP_ACTION = 4
    DEBUG_CONTEXT_APP_MAPPING_POINT = 5
    DEBUG_CONTEXT_DEBUGOBJECT = 6
End Enum
