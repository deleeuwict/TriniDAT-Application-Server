Imports TriniDATDictionaries

Public Class TriniDATUser

    Public Username As String
    Public Password As String
    Private server_default As Boolean

    Public current_sessions As TrinidatUserSessions
    Private current_thread_count As Long
    Private max_thread_count As Long
    Private permission_table As TriniDATPermissionTable

    Public Sub makeAdmin()
        'set all permissions to true
        Me.permission_table.appdebug = True
        Me.permission_table.ReloadAppCache = True

    End Sub
    Public Property Permissions As TriniDATPermissionTable
        Get
            Return Me.permission_table
        End Get
        Set(ByVal value As TriniDATPermissionTable)
            Me.permission_table = value
        End Set
    End Property
    Public Sub setPermissionTableFromXPermissions(ByVal perm_table_node As XElement)

        If IsNothing(perm_table_node) Then Exit Sub

        Dim perm_node As XElement


        If perm_table_node.Descendants("appcachereload").Count = 1 Then
            perm_node = perm_table_node.Descendants("appcachereload")(0)

            If Not IsNothing(perm_node) Then
                Me.permission_table.ReloadAppCache = (perm_node.Value = "true")
                'GlobalObject.MsgColored("   -User '" & Me.Username & "': has reload flag.", Color.MidnightBlue)
            End If
        End If

        If perm_table_node.Descendants("appdebugging").Count = 1 Then
            perm_node = perm_table_node.Descendants("appdebugging")(0)

            If Not IsNothing(perm_node) Then
                Me.permission_table.appdebug = (perm_node.Value = "true")
                'GlobalObject.MsgColored("   -User '" & Me.Username & "': has debugging flag.", Color.MidnightBlue)
            End If
        End If

    End Sub
    Public Property ThreadCount As Long
        Get
            Return Me.current_thread_count
        End Get
        Set(ByVal value As Long)
            Me.current_thread_count = value
        End Set
    End Property
    Public Property MaxThreadCount As Long
        Get
            Return Me.max_thread_count
        End Get
        Set(ByVal value As Long)
            Me.max_thread_count = value
            ' GlobalObject.MsgColored("   -user '" & Me.Username & "' max. threading: " & Me.max_thread_count.ToString & ". ", Color.MidnightBlue)
        End Set
    End Property
    Public ReadOnly Property haveSession() As Boolean
        Get
            'true if at least one session is found
            Return Me.current_sessions.haveSessions()
        End Get
    End Property
    Public Property Sessions As TrinidatUserSessions
        Get
            Return Me.current_sessions
        End Get
        Set(ByVal value As TrinidatUserSessions)
            Me.current_sessions = value
        End Set
    End Property

    Public ReadOnly Property ownsSessionById(ByVal Id As String) As Boolean
        Get
            Return Not IsNothing(Me.Sessions.haveSessionId(Id))
        End Get
    End Property

    Public Sub New(ByVal user As String, ByVal pass As String, Optional ByVal is_default_user As Boolean = False, Optional ByVal _maxthread_count As Long = 0)
        '   GlobalSpeech.Text = "simon says user '" & user & "' online..."
        '  GlobalSpeech.SpeakThreaded()

        Me.Username = user
        Me.Password = pass
        Me.IsDefaultUser = is_default_user
        Me.Sessions = New TriniDATUserSessions(Me)
        Me.permission_table = New TriniDATPermissionTable

        If _maxthread_count > 0 Then
            Me.MaxThreadCount = _maxthread_count
        ElseIf GlobalObject.haveServerthread Then
            Me.MaxThreadCount = GlobalObject.CurrentServerConfiguration.DefaultMaxThread.ToString
        End If

    End Sub
    Public Property IsDefaultUser() As Boolean
        Get
            Return Me.server_default
        End Get
        Set(ByVal value As Boolean)
            Me.server_default = value
        End Set
    End Property
End Class
Public Class TriniDATUserSessions
    Inherits List(Of TrinidatUserSession)

    Private user As TriniDATUser
    Public Property relatedUser As TriniDATUser
        Get
            Return Me.User
        End Get
        Set(ByVal value As TriniDATUser)
            Me.User = value
        End Set
    End Property
    Public ReadOnly Property getMostRecent() As TrinidatUserSession
        Get

            Dim y As Integer
            Dim x As Integer
            Dim sess As TrinidatUserSession
            Dim recent_sess As TrinidatUserSession
            Dim recent_sess_age_sec As Long
            Dim age_sec As Long

            Dim session_count As Long

            If Me.Count < 1 Then
                Return Nothing
            End If

            'INIT
            session_count = Me.Count - 1
            recent_sess_age_sec = -1
            recent_sess = Nothing

            For x = 0 To session_count

                sess = Me.Item(session_count - x)
                age_sec = sess.AgeInSeconds

                If recent_sess_age_sec = -1 Then
                    'first 
                    recent_sess_age_sec = age_sec
                    recent_sess = sess
                ElseIf recent_sess_age_sec > -1 And recent_sess_age_sec > age_sec Then
                    recent_sess_age_sec = age_sec
                    recent_sess = sess
                    'since we count from the back we assume this is the most recent 
                    Return recent_sess
                End If

            Next x

            Return recent_sess

        End Get
    End Property
    Public Overloads Function Add(ByVal sess As TriniDATUserSession) As TriniDATUserSession

        sess.relatedUser = Me.relatedUser
        Call MyBase.Add(sess)

        Return sess

    End Function
    Public ReadOnly Property haveSessions() As Boolean
        Get
            Return Me.Count > 0
        End Get
    End Property
    Public ReadOnly Property haveSessionId(ByVal sessionid_str As String) As TrinidatUserSession
        Get

            For Each session In Me
                If session.ID = sessionid_str Then
                    Return session
                End If
            Next

            Return Nothing
        End Get
    End Property

    Public ReadOnly Property getSessionById(ByVal sessionid_str As String) As TriniDATUserSession
        Get

            For Each session In Me
                If session.ID = sessionid_str Then
                    'Dim usr As TriniDATUser
                    'usr = session.relatedUser

                    Return session
                End If
            Next

            Return Nothing
        End Get
    End Property

    Public Sub New(ByVal _user As TriniDATUser)
        Me.relatedUser = _user
    End Sub
End Class


