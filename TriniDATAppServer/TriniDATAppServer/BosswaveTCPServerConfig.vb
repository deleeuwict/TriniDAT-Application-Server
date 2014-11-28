Imports System.Net
Imports System.IO
Public Class BosswaveTCPServerConfig
    Public server_ip As IPAddress
    Public server_port As Integer
    Public default_address_set As Boolean 'is set when 127.0.0.1 is auto-set
    Public socket_timeout As Integer 'seconds
    Public receive_buffer_size As Long 'bytes
    Public succes As Boolean
    Private default_max_thread_count As Long
    Public config_file_exists As Boolean
    Public Property DefaultMaxThread As Long
        Get
            Return Me.default_max_thread_count
        End Get
        Set(ByVal value As Long)
            Me.default_max_thread_count = value
        End Set
    End Property
    Public Function Write(Optional ByVal retry_count = 0) As Boolean

        Dim default_httpconfig_savepath As String
        Dim httpconfiguration_template As String

        default_httpconfig_savepath = GlobalSetting.getHTTPServerConfigPath()


        httpconfiguration_template = "<httpserver><listenon>$IP</listenon><listenat>$PORT</listenat><requesttimeoutsec>$TIMEOUT</requesttimeoutsec><receivebuffersize>$RECV</receivebuffersize><defaultmaxthread>$THREAD</defaultmaxthread><userpaykey>$PAYKEY</userpaykey></httpserver>"
        httpconfiguration_template = Replace(httpconfiguration_template, "$IP", Me.server_ip.ToString)
        httpconfiguration_template = Replace(httpconfiguration_template, "$PORT", Me.server_port.ToString)
        httpconfiguration_template = Replace(httpconfiguration_template, "$TIMEOUT", Me.socket_timeout.ToString)
        httpconfiguration_template = Replace(httpconfiguration_template, "$RECV", Me.receive_buffer_size.ToString)
        httpconfiguration_template = Replace(httpconfiguration_template, "$THREAD", Me.default_max_thread_count.ToString)
        httpconfiguration_template = Replace(httpconfiguration_template, "$PAYKEY", GlobalSetting.PayKey)

        Try
            File.WriteAllText(default_httpconfig_savepath, XElement.Parse(httpconfiguration_template).ToString)
        Catch ex As Exception
            If retry_count < 5 Then
                '.NET buffer errors are common in threaded applications.
                retry_count += 1
                Return Write(retry_count)
            Else
                GlobalObject.MsgColored("Unable to write to http configuraiton file ''" & default_httpconfig_savepath & ".", Color.DarkRed)
            End If
        End Try


        Return True

    End Function
    Public Shared ReadOnly Property getDefault() As BosswaveTCPServerConfig
        Get
            Dim retval As BosswaveTCPServerConfig

            retval = New BosswaveTCPServerConfig

            retval.server_port = GlobalSetting.DEFAULT_HTTPSERVER_PORT
            retval.default_address_set = True
            retval.receive_buffer_size = 10 * 1024
            retval.socket_timeout = 25
            retval.default_max_thread_count = GlobalObject.OfficialLicense.getT

            Try
                retval.server_ip = IPAddress.Parse("127.0.0.1")
            Catch ex As Exception
                GlobalObject.Msg("127.0.0.1 not available. Install a valid network adapter or edit httpconfig.xml manually to run server.")
            End Try

            Return retval

        End Get
    End Property

End Class