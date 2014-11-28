Imports TriniDATServerTypes
Public Class SimonDebugFrame
    Public id As Long
    Public header As JSONSmallObject
    Public url As String
    Public appid As Long
    Public direct_http_client As Object ' TriniDATClientConnectionManagerHTTP
    Public direct_mapping_point As Object ' mapping_point_descriptor
    Public associated_app As Object '  BosswaveApplication
    Public associated_packet As String
    Public sessionid As String
    Public incomingdatetime As DateTime
    Public JSON As String
    Public Sub New()
        Me.json = ""
    End Sub
    Public Property App() As Object ' BosswaveApplication
        Get
            Return Me.associated_app
        End Get
        Set(ByVal value As Object)
            Me.associated_app = value
        End Set
    End Property
    Public ReadOnly Property haveJSON() As Boolean
        Get
            Return (Me.json <> "")
        End Get
    End Property
    Public ReadOnly Property haveApp() As Boolean
        Get
            Return Not IsNothing(Me.associated_app)
        End Get
    End Property
    Public ReadOnly Property haveDirectMappingPoint As Boolean
        Get
            Return Not IsNothing(Me.direct_mapping_point)
        End Get
    End Property

    Public ReadOnly Property haveConnection As Boolean
        Get
            Return Not IsNothing(Me.direct_http_client.getConnection().isConnected())
        End Get
    End Property


    'Public ReadOnly Property CurrentDebuggerObjectKind As Type
    '    Get
    '        If Not Me.haveDebugObject Then Return Nothing
    '        Return CurrentDebuggerObjects.GetType
    '    End Get
    'End Property
End Class
