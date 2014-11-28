Imports System.Xml

Public Class TriniDATUsers
    Inherits List(Of TriniDATUser)

    Public Shared Function Create(ByVal xusers As XDocument) As TriniDATUsers

        If Not GlobalObject.foundLicense Then
            Throw New Exception("Invalid license detected. Code: 1")
            Return Nothing
        End If

        If Not GlobalObject.OfficialLicense.Verify() Then
            Throw New Exception("Invalid license detected. Code 2.")
            Return Nothing
        End If

        Dim q = From usr In xusers.Descendants("serveruser")
        Dim retval As TriniDATUsers

        retval = New TriniDATUsers

        'add internal users

        Dim guest As TriniDATUser
        Dim usr_class As TriniDATUser
        Dim usr_thread_count As Long

        guest = New TriniDATUser("server", "", True, GlobalObject.OfficialLicense.getT)
        guest.makeadmin()

        retval.Add(guest)


        For Each usr As XElement In q
            Dim perm_node As XElement
            Dim perm As TriniDATPermissionTable
            perm = New TriniDATPermissionTable

            perm_node = Nothing
            usr_thread_count = 0 'if zero, will be set by constructor

            If Not IsNothing(usr.@maxthread) Then
                If IsNumeric(usr.@maxthread) Then
                    usr_thread_count = CLng(usr.@maxthread)

                    'diminish acc. to user
                    If Not GlobalObject.OfficialLicense.Verify() Then
                        Throw New Exception("Invalid license detected. Code 2.")
                        Return Nothing
                    Else
                        If usr_thread_count > GlobalObject.OfficialLicense.GetT Then
                            'set max. allowed threads for this license type.
                            usr_thread_count = CLng(GlobalObject.OfficialLicense.GetT)
                        End If
                    End If

                End If
            End If

            usr_class = New TriniDATUser(usr.@name, usr.@password, False, usr_thread_count)

            If usr.Descendants("permissiontable").Count = 1 Then
                usr_class.setPermissionTableFromXPermissions(usr.Descendants("permissiontable")(0))
            End If

            retval.Add(usr_class)
        Next

        Return retval
    End Function
    Public ReadOnly Property getBySessionID(ByVal val As String) As TriniDATUser
        Get

            For Each usr In Me
                If usr.ownsSessionById(val) = True Then
                    Return usr
                End If
            Next

            Return Nothing
        End Get
    End Property
    Public ReadOnly Property getSessionById(ByVal val As String) As TriniDATUserSession
        Get

            For Each usr In Me
                If usr.ownsSessionById(val) = True Then
                    Return usr.Sessions.getSessionById(val)
                End If
            Next

            Return Nothing
        End Get
    End Property
    Public ReadOnly Property getDefault() As TriniDATUser
        Get

            For Each usr In Me
                If usr.IsDefaultUser Then
                    Return usr
                End If
            Next

            Return Nothing
        End Get
    End Property


End Class