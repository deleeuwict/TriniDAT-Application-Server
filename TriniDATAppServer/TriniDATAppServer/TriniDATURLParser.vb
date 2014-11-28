Imports TriniDATSockets

Public Class TriniDATURLParser

    Private myurl As String
    Private parts() As String

    Public Sub New(ByVal _url As String)
        Me.URL = _url
    End Sub

    Public Property URL As String
        Get
            Return Me.myurl
        End Get
        Set(ByVal value As String)
            Me.myurl = Replace(value, "https", "")
            Me.myurl = Replace(value, "http", "")
            Me.myurl = Replace(value, ":", "")
            Me.myurl = Replace(value, "//", "")
            If Len(Me.myurl) > 0 And Left(Me.myurl, 1) = "/" Then
                'avoid empty zero index
                Me.myurl = Mid(Me.myurl, 2)
            End If
            Me.parts = Split(Me.myurl, "/")
        End Set
    End Property

    Public ReadOnly Property isApplicationURI() As Boolean
        Get
            If Me.parts.Length < 1 Then Return False

            Return IsNumeric(Me.parts(0))
        End Get
    End Property
    Public ReadOnly Property isDebugApplicationURI() As Boolean
        Get
            If Me.parts.Length < 1 Then Return False
            If InStr(Me.parts(0), "@") = 0 Then Return False

            Return IsNumeric(Replace(Me.parts(0), "@", ""))
        End Get
    End Property
    Public ReadOnly Property getAssociatedApplicationPrototype() As BosswaveApplication
        Get
            If Not Me.isApplicationURI() Then Return Nothing

            Return GlobalObject.ApplicationCache.AppById(CLng(Me.parts(0)))
        End Get
    End Property
    Public ReadOnly Property getAssociatedApplicationPrototypeId() As Long
        Get
            If Not Me.isApplicationURI() Then Return Nothing

            Return CLng(Me.parts(0))
        End Get
    End Property
    Private ReadOnly Property getMappingPointURL() As String
        Get
            If Not Me.isApplicationURI() And Not Me.isDebugApplicationURI() Then Return Nothing

            Dim x As Integer
            Dim retval As String

            retval = ""

            For x = 1 To Me.parts.Count - 1
                retval &= Me.parts(x)
                If x < Me.parts.Count - 1 Then
                    retval &= "/"
                End If
            Next

            If Left(retval, 1) <> "/" Then
                retval = "/" & retval
            End If

            'if filename or parameter present, remove
            Dim pos As Integer

            pos = InStrRev(retval, "?")
            If pos > 0 Then
                retval = Mid(retval, 1, pos - 1)
            End If

            pos = -1

            While pos <> 0

                'remove filenames
                pos = InStrRev(retval, ".")
                If pos > 0 Then
                    'strip till last dir
                    pos = InStrRev(retval, "/")
                    retval = Mid(retval, 1, pos - 1)
                End If

            End While

            If Right(retval, 1) <> "/" Then
                retval &= "/"
            End If

            Return retval
        End Get
    End Property

    Public Function Parse(ByVal http_connection_handler As TriniDATClientConnectionManagerHTTP) As TriniDATRequestInfo

        Dim retval As TriniDATRequestInfo
        Dim app_prototype As BosswaveApplication
        Dim app_prototype_id As Long

        retval = New TriniDATRequestInfo
        retval.parse_result = TriniDATRequestInfoType.ERR_LOAD
        retval.is_new_app_instance = True
        retval.associated_app = Nothing
        retval.mapping_point_desc = Nothing
        retval.unparsed_url_part = Me.getMappingPointURL()
        If IsNothing(retval.unparsed_url_part) Then Return retval

        retval.http_connection_handler = http_connection_handler
        retval.direct_socket = retval.http_connection_handler.getConnection()
        retval.direct_session = retval.http_connection_handler.Session

        'load prototype
        app_prototype_id = Me.getAssociatedApplicationPrototypeId()

        If app_prototype_id = 0 Then
            If Me.isDebugApplicationURI Then
                retval.is_debug_request = True
                Me.parts(0) = Replace(Me.parts(0), "@", "")
                'retry
                app_prototype_id = Me.getAssociatedApplicationPrototypeId()

                If retval.unparsed_url_part = "/" Then
                    retval.unparsed_url_part = Nothing
                End If

            End If
        End If

        'see if app is already running
        If http_connection_handler.HaveActiveSession() Then

            If GlobalObject.haveServerthread() Then
                'get realtime session data
                Dim realtime_session_user As TriniDATUser
                Dim realtime_session As TriniDATUserSession

                realtime_session_user = GlobalObject.server.Users.getBySessionID(http_connection_handler.Session.ID)
                If Not IsNothing(realtime_session_user) Then
                    realtime_session = realtime_session_user.Sessions.getSessionById(http_connection_handler.Session.ID)
                    If Not IsNothing(realtime_session) Then
                        'have application?
                        If realtime_session.haveApplicationById(app_prototype_id) Then
                            retval.associated_app = realtime_session.Application(app_prototype_id)
                            retval.is_new_app_instance = False
                            GlobalObject.Msg("Resume application '" & retval.associated_app.ApplicationName & "' in session '" & http_connection_handler.Session.ID & "'...")
                            'transfer application to current session
                        Else
                        End If
                    Else
                        GlobalObject.Msg("Unable to find realtime session " & http_connection_handler.Session.ID & "...")
                    End If
                Else
                    GlobalObject.Msg("Unable to find user by realtime session " & http_connection_handler.Session.ID & "...")
                End If
            End If

        End If

        If Not retval.haveApp() Then
            Dim notfound As Boolean

            notfound = True
            app_prototype = retval.associated_app
            app_prototype = Me.getAssociatedApplicationPrototype()

            notfound = IsNothing(app_prototype)

            If Not notfound Then

                If app_prototype.Disabled Then
                    'refuse to load
                    retval.parse_result = TriniDATRequestInfoType.APP_IS_DISABLED
                    Return retval
                Else
                    app_prototype.DisableReason = app_prototype.Load()

                    If app_prototype.DisableReason <> BOSSWAVE_APP_DISABLE_REASON.NONE Then
                        GlobalObject.Msg("Error loading application " & app_prototype.ApplicationName)
                        retval.parse_result = TriniDATRequestInfoType.ERR_LOAD
                        Return retval
                    End If
                End If
            Else
                retval.parse_result = TriniDATRequestInfoType.ERR_INVALID_ID
                Return retval
            End If

            retval.associated_app = app_prototype
            GlobalObject.Msg("New application '" & retval.associated_app.ApplicationName & "' in session ID '" & http_connection_handler.Session.ID & "'...")

            If Not retval.is_debug_request Then
                If Not BosswaveApplication.updateRuntimeCount(retval.associated_app.Id) Then
                    GlobalObject.Msg("Error updating application usage count.")
                End If
            End If

        End If

        retval.parse_result = TriniDATRequestInfoType.APP_ONLY
        retval = Me.stripMappingPoint(retval) ' retval.app.ApplicationMappingPoints.getDescriptorByURL(retval.unparsed_url_part)

        'first check for index
        If IsNothing(retval.mapping_point_desc) Then
            'see if there is a mapping point that declares /.
            retval.mapping_point_desc = retval.associated_app.ApplicationMappingPoints.getDescriptorByURL("/")
        End If

        If Not IsNothing(retval.mapping_point_desc) Then
            If Not retval.haveUnparsedSection Then
                'full match
                retval.parse_result = TriniDATRequestInfoType.APP_PLUS_MP
            Else
                retval.parse_result = TriniDATRequestInfoType.APP_PARTIAL_MP
            End If

            'restore filename
            Dim filename As String
            filename = Me.parts(Me.parts.Length - 1)

            If InStr(filename, ".") > 0 Then
                If retval.haveUnparsedSection Then
                    retval.unparsed_url_part &= "/"
                End If
                retval.unparsed_url_part &= filename
            End If
        End If

        If retval.associated_app.IsInterface Then
            retval.parse_result = TriniDATRequestInfoType.APP_IS_INTERFACE
        End If

        Return retval


    End Function

    Private Function stripMappingPoint(ByVal retval As TriniDATRequestInfo) As TriniDATRequestInfo
        'Resolve mapping points from the back like this:
        '/phpmyadmin/images
        '/phpmyadmin
        '/
        Dim uriCandidate() As String
        Dim temp As String
        Dim count As Integer
        Dim mp As mapping_point_descriptor
        Dim is_debug_uri As Boolean

        uriCandidate = Split(retval.unparsed_url_part & "/", "/")

        temp = retval.unparsed_url_part
        count = uriCandidate.Length - 1


        For x = 1 To count - 1
            'discover mapping point.

            'skip files
            If InStr(1, temp, ".") = 0 Then

                '.e.g /@blocks/ will show the mapping point xml
                is_debug_uri = (Left(temp, 2) = "/@")

                If is_debug_uri Then
                    temp = Replace(temp, "@", "")
                    retval.is_debug_request = True
                End If

                Try
                    mp = retval.associated_app.ApplicationMappingPoints.getDescriptorByURL(temp)
                Catch ex As Exception
                    mp = Nothing
                End Try


                If Not IsNothing(mp) Then
                    retval.mapping_point_desc = mp
                    If is_debug_uri Then
                        temp = "/@" & Mid(temp, 2)
                    End If
                    retval.unparsed_url_part = Replace(retval.unparsed_url_part, temp, "")
                    If Right(retval.unparsed_url_part, 1) = "/" Then
                        retval.unparsed_url_part = Mid(retval.unparsed_url_part, 1, Len(retval.unparsed_url_part) - 1)
                    End If
                    Return retval
                End If

            Else
                Debug.Print("No mappings exist at " & temp)
            End If

            Dim searchPath As String

            If InStr(uriCandidate(count - x), ".") = 0 Then
                'dir
                searchPath = "/" & uriCandidate(count - x) & "/"
            Else
                'file
                searchPath = "/" & uriCandidate(count - x)
            End If

            temp = Replace(temp, searchPath, "/")

        Next x
        retval.mapping_point_desc = Nothing

        Return retval
    End Function
End Class

Public Class TriniDATRequestInfo
    Public requestType As String
    Public is_new_app_instance As Boolean
    Public is_new_session As Boolean
    Public is_debug_request As Boolean 'contains @ directives
    Public http_connection_handler As TriniDATClientConnectionManagerHTTP
    Public direct_socket As TriniDATTCPSocket
    Public mapping_point_desc As mapping_point_descriptor
    Public unparsed_url_part As String
    Public associated_app As BosswaveApplication
    Public direct_session As TriniDATUserSession
    Public parse_result As TriniDATRequestInfoType
    Public ReadOnly Property session_path As String
        Get
            If Not haveApp Then Return Nothing
            If Not haveSession Then Return Nothing
            Dim app_root As String

            app_root = GlobalSetting.getSessionRoot()

            app_root &= Me.direct_session.ID
            app_root &= "\"
            app_root &= Me.associated_app.Id.ToString
            app_root &= "\"

            Return app_root
        End Get
    End Property
    Public ReadOnly Property relativeApplicationURL() As String
        Get
            Return "/" & Me.associated_app.Id & "/"
        End Get
    End Property
    Public ReadOnly Property MappingPointDescriptor As mapping_point_descriptor
        Get
            Return Me.mapping_point_desc
        End Get
    End Property

    Public ReadOnly Property FullServerURL() As String
        Get

            If Not GlobalObject.haveServerThread Then
                Return Nothing
            End If

            Dim retval As String

            retval = "http://" & GlobalObject.CurrentServerConfiguration.server_ip.ToString

            If GlobalObject.CurrentServerConfiguration.server_port <> 80 Then
                retval &= ":" & GlobalObject.CurrentServerConfiguration.server_port.ToString
            End If

            If Me.haveApp() Then
                retval &= "/" & Me.associated_app.Id.ToString

                If Me.haveMappingPoint() Then
                    retval &= Me.mapping_point_desc.URL()
                End If

            End If

            Return retval
        End Get
    End Property
    Public ReadOnly Property FullMappingPointPath() As String
        Get

            If Not Me.haveMappingPoint() Or Not Me.haveApp() Then
                Return Nothing
            End If

            Dim retval As String
            retval = "/" & Me.associated_app.Id.ToString & Me.mapping_point_desc.URL()

            Return retval
        End Get
    End Property
    Public ReadOnly Property recreateRelativeURL() As String
        Get
            'haveRemainder
            Dim retval As String
            retval = ""

            If Me.haveMappingPoint() Then
                retval = Me.mapping_point_desc.URL
            End If

            If Me.haveUnparsedSection Then
                If Right(retval, 1) <> "/" Then
                    retval &= "/"
                End If
                retval &= Me.unparsed_url_part
            End If

            Return retval
        End Get
    End Property
    Public ReadOnly Property App() As BosswaveApplication
        Get
            Return Me.associated_app
        End Get
    End Property
    Public ReadOnly Property haveApp() As Boolean
        Get
            Return Not IsNothing(Me.associated_app)
        End Get
    End Property
    Public ReadOnly Property haveUnparsedSection As Boolean
        Get
            If IsNothing(Me.unparsed_url_part) Then Return False
            Return Me.unparsed_url_part <> ""
        End Get
    End Property

    Public ReadOnly Property haveMappingPoint As Boolean
        Get
            Return Not IsNothing(Me.mapping_point_desc)
        End Get
    End Property

    Public ReadOnly Property haveConnection As Boolean
        Get
            Return Not IsNothing(Me.direct_session) And Not IsNothing(Me.http_connection_handler)
        End Get
    End Property

    Public ReadOnly Property haveSession As Boolean
        Get
            Return Not IsNothing(Me.direct_session)
        End Get
    End Property

    Public Sub New()

    End Sub
End Class

Public Enum TriniDATRequestInfoType
    ERR_LOAD = -2
    ERR_INVALID_ID = -1
    APP_IS_DISABLED = 0
    APP_ONLY = 2
    APP_PLUS_MP = 3
    APP_PARTIAL_MP = 4
    APP_IS_INTERFACE = 5
End Enum