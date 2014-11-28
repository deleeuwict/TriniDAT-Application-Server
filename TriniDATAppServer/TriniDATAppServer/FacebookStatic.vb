Imports TriniDATDictionaries

Public Class FacebookStatic

    Public Shared ignore_links As TriniDATWordDictionary
    Public Shared domains As TriniDATWordDictionary
    Public Shared ignore_tags As TriniDATWordDictionary
    Public Shared ignore_captions As TriniDATWordDictionary
    Public Shared logger As TriniDATHTTPTypes.TriniDATTypeLogger

    Public Shared Function Initialize(ByVal _logger As TriniDATHTTPTypes.TriniDATTypeLogger)
        ignore_tags = New TriniDATWordDictionary("", New List(Of String)({"style", "script", "title"}))
        domains = New TriniDATWordDictionary("", New List(Of String)({"facebook.com"}))

        If IsNothing(_logger) Then
            _logger = AddressOf FacebookStatic.NullHandler
        End If

        FacebookStatic.logger = _logger

        Msg("Facebook module initialized")
        Return True

    End Function

    Public Shared Function needsInitialization() As Boolean
        Return IsNothing(FacebookStatic.ignore_tags)
    End Function

    Public Shared Sub Msg(ByVal txt As String)
        FacebookStatic.logger(txt)
    End Sub

    Public Shared Sub NullHandler(ByVal txt As String)

    End Sub

End Class
