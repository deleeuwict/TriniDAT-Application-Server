Imports System.Net

Public Class TriniDATNETWebClient
    Inherits WebClient

    Private current_uri As Uri

    Public Property currentURI As Uri
        Get
            Return Me.current_uri
        End Get
        Set(ByVal value As Uri)
            Me.current_uri = value
        End Set
    End Property
    Protected Overrides Function GetWebRequest(ByVal address As System.Uri) As System.Net.WebRequest

        Dim req As HttpWebRequest
   
        'gzip etc direct uitpakken.
        req = MyBase.GetWebRequest(address)
        req.AutomaticDecompression = (DecompressionMethods.Deflate Or DecompressionMethods.GZip)

        'hou huidige URL bij.
        Me.currentURI = req.RequestUri

        Return req
    End Function

End Class
