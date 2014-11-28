Imports System.Collections
Imports System.Collections.Generic
Imports System.Collections.ObjectModel


Public Class JSONMailBoxListeners

    Private _listeners As Collection(Of JSONMailBoxListener)

    Public Function getListeners() As Collection(Of JSONMailBoxListener)
        Return Me._listeners
    End Function

    Public Function getListeners(ByVal mappingPoint_index As Integer, ByVal className As String, Optional ByVal max_return_items As Integer = 0) As Collection(Of JSONMailBoxListener)
        Dim retval As Collection(Of JSONMailBoxListener)
        Dim sense As JSONMailBoxListener
        Dim mp As mappingPointRoot
        Dim counter As Integer

        mp = Nothing 'NOT IMPLEMENTED YET ServerMappingPointIO.getGlobalInstance.getByIndex(mappingPoint_index)

        If Not IsNothing(mp) Then
            retval = New Collection(Of JSONMailBoxListener)
            counter = 0

            For Each sense In Me.getListeners()
                If (sense.getWatchClassName() = className And sense.getWatchMappingPointIndex() = mappingPoint_index) Then
                    retval.Add(sense)
                    counter = counter + 1
                    If max_return_items > 0 And counter = max_return_items Then
                        Exit For
                    End If
                End If
            Next

            Msg("Returning " & retval.Count.ToString & " for class: " & className)

            Return retval
        End If

        Msg("Search: Given Mapping point not found. (search class: " & className & ")")

        Return Nothing

    End Function

    Public Function hasListener(ByVal mappingPoint_index As Integer, ByVal className As String) As Boolean
        Return Not IsNothing(getListeners(mappingPoint_index, className, 1))
    End Function
    Public Sub New()
        Me._listeners = New Collection(Of JSONMailBoxListener)

    End Sub

    Protected Sub Msg(ByVal txt As String)
        Debug.Print("JSONMailBoxListeners instance: " & txt)
    End Sub
End Class
