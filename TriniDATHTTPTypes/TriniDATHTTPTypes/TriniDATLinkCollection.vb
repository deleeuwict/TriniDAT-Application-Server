Imports TriniDATDictionaries
Public Class TriniDATLinkCollection
    Inherits List(Of TriniDATLinkElement)


    Public Function getByURLsUnique() As TriniDATLinkCollection

        Dim retval As TriniDATLinkCollection
        Dim unique_list As TriniDATWordDictionary

        retval = New TriniDATLinkCollection
        unique_list = New TriniDATWordDictionary("", Nothing)

        For Each link_el In Me

            If Not unique_list.Has(link_el.getURL) Then
                unique_list.Add(link_el.getURL())
                retval.Add(link_el)
            End If

        Next

        Return retval
    End Function
End Class
