 Imports System
Imports System.Net
Module Module1
    Sub Main()
        Dim wc As WebClient
        Dim URL As String
        wc = New WebClient
        URL = "http://192.168.2.1/2/demo/cnn/"
        wc.Headers.Add("Cookie", "X-SessionId=yGqDli3frv137fMrvBvuYZJMuu")
        Try

        Catch ex As Exception

        End Try
        Console.WriteLine(wc.DownloadString("$URL"))

    End Sub
End Module
