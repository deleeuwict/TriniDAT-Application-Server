Public Class JTRANSPORTABLE_METHOD
    Public method_info As JTRANSPORT_METHODINFO

    Public Sub New(ByVal _method_type As JTRANSPORT_METHODINFO)
        Me.method_info = _method_type

    End Sub
    Public ReadOnly Property hasPayload As Boolean
        'declares if the transport method requires embodying the full object.

        Get

            Select Case method_info

                Case JTRANSPORT_METHODINFO.REQUEST_DEBUG_OBJECT
                    Return True

                Case JTRANSPORT_METHODINFO.REQUEST_CREATEOBJECT
                    Return False

                Case JTRANSPORT_METHODINFO.RESPONSE_MODIFIED_OBJECT
                    Return True

                Case Else
                    Return True
            End Select

        End Get
    End Property
    Public ReadOnly Property MethodString As String
        Get

            Select Case method_info

                Case JTRANSPORT_METHODINFO.REQUEST_DEBUG_OBJECT
                    Return "DEBUGOBJECT"

                Case JTRANSPORT_METHODINFO.REQUEST_CREATEOBJECT
                    Return "CREATE"

                Case JTRANSPORT_METHODINFO.RESPONSE_MODIFIED_OBJECT
                    Return "MODIFIED"

                Case Else
                    Return "UNKNOWN"
            End Select

        End Get
    End Property

End Class

Public Enum JTRANSPORT_METHODINFO
    REQUEST_DEBUG_OBJECT = 1
    REQUEST_CREATEOBJECT = 2
    RESPONSE_MODIFIED_OBJECT = 3

End Enum

