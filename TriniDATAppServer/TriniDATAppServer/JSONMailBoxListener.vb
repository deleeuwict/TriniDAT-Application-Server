Imports TriniDATServerTypes
Public Class JSONMailBoxListener
    Private jserviceInstance As JTriniDATWebService
    Private classnametoWatch As String
    Private listener_description As String
    Private mp_index As Integer

    Public Sub New(ByVal watchfor_className As String, ByVal jProcess As JTriniDATWebService, ByVal watchin_mappingPoint_index As Integer, Optional ByVal description As String = "")
        Me.setWatchMappingPointIndex(watchin_mappingPoint_index)
        Me.setWatchClassName(watchfor_className)
        Me.setCaller(jProcess)
        Me.setDescription(description)
    End Sub

    Public Sub setWatchClassName(ByVal classname As String)
        Me.classnametoWatch = classname
    End Sub

    Public Function getWatchClassName() As String
        Return Me.classnametoWatch
    End Function

    Public Sub setWatchMappingPointIndex(ByVal _mp_index As Integer)
        Me.mp_index = _mp_index
    End Sub

    Public Function getWatchMappingPointIndex() As Integer
        Return Me.mp_index
    End Function

    Public Function getCaller() As JTriniDATWebService
        Return jserviceInstance
    End Function

    Public Sub setCaller(ByVal val As JTriniDATWebService)
        Me.jserviceInstance = val
    End Sub

    Public Function getDescription() As String
        Return listener_description
    End Function

    Public Sub setDescription(ByVal val As String)
        Me.listener_description = val
    End Sub
End Class
