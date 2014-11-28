Public Class SimonDebugFrames
    Inherits List(Of SimonDebugFrame)
    Public ReadOnly Property haveApplication(ByVal x As Long) As Boolean
        Get

            For Each debug_obj In Me
                If debug_obj.haveApp Then
                    If debug_obj.App.id = x Then
                        Return True
                    End If
                End If
            Next

            Return False
        End Get
    End Property
    Public ReadOnly Property ByApplication(ByVal x As Long) As Object
        Get

            For Each debug_obj In Me
                If debug_obj.haveApp Then
                    If debug_obj.App.id = x Then
                        Return debug_obj
                    End If
                End If
            Next

            Return Nothing
        End Get
    End Property
    Public ReadOnly Property GetById(ByVal x As Long) As Object
        Get

            For Each debug_obj In Me
                If debug_obj.id = x Then
                    Return debug_obj
                End If
            Next

            Return Nothing
        End Get
    End Property
End Class
