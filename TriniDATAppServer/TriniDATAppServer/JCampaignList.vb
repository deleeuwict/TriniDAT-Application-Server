Option Explicit On

Imports MySql.Data.MySqlClient

Public Class JCampaignList

    Private campaigns() As JCampaignListItem
    Private campaigncount As Integer

    Public Sub New(ByVal absolute_root As String, ByVal relative_root As String)
        campaigncount = 0
        ReDim campaigns(0)

    End Sub
    Public Function getByIndex(ByVal x As Integer) As JCampaignListItem

     
    End Function

    Public Function getCount() As Integer

        'DISABLED
        Return 0

        'Return retval

    End Function

End Class


Public Class JCampaignListItem

    Public Index As Integer
    Public Name As String

End Class