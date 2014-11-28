
Option Explicit On


Public Class JConsoleMenuItems

    Private menu_items() As JConsoleMenuItem
    Private menu_count As Integer

    Public Sub New(ByVal absolute_root As String, ByVal relative_root As String)
        Me.menu_count = 0
        ReDim Me.menu_items(0)

        'BUILD MENU LIST

        Dim item As JConsoleMenuItem

        item = New JConsoleMenuItem
        item.Name = "SYSTEM"
        item.Link = relative_root
        Me.add(item)

        item = New JConsoleMenuItem
        item.Name = "MODULES"
        item.Link = "modulelist"
        Me.add(item)

        item = New JConsoleMenuItem
        item.Name = "TWITTER"
        item.Link = relative_root & "twitter.html" '"/seeds/tweetfeed/"
        Me.add(item)

        item = New JConsoleMenuItem
        item.Name = "ARCHETYPE DESIGN"
        item.Link = "economicclassmodels"
        Me.add(item)

    End Sub
    Public Function getByIndex(ByVal x As Integer) As JConsoleMenuItem

        Return Me.menu_items(x)

    End Function

    Public Function getCount() As Integer
        Return Me.menu_count
    End Function

    Private Sub add(ByVal item As JConsoleMenuItem)
        ReDim Preserve Me.menu_items(Me.menu_count)

        Me.menu_items(menu_count) = item
        Me.menu_items(menu_count).Link = "http://" & GlobalObject.CurrentServerConfiguration.server_ip.ToString & ":" & GlobalObject.CurrentServerConfiguration.server_port.ToString & item.Link
        Me.menu_count = Me.menu_count + 1
    End Sub

End Class


Public Class JConsoleMenuItem

    Public Name As String
    Public Link As String

End Class

