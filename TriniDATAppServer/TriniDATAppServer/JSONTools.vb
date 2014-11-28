Option Explicit On
Public Class JSONTools

    Public Shared Function Escape(ByVal jsondata As String) As String
        'escape only field boundries, not content.

        'first unescape everything so that we dont end up with a mess.
        jsondata = Replace(jsondata, "\" & Chr(34), Chr(34))
        jsondata = Replace(jsondata, Chr(34), "\" & Chr(34))

        Return jsondata

    End Function


End Class
