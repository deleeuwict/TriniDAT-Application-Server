Imports System.IO
Imports System.Text
Imports Newtonsoft
Imports System.Web
Imports TriniDATServerTypes

Public Class TrafficMonitor

    Private Shared log_stream As FileStream
    Private Const LOGFILE_PREFIX As String = "trinidat"
    Private Shared path As String
    Private Shared current_date As String
    Private Const MAX_WRITE_RETRIES As Integer = 3

    Public Shared ReadOnly Property haveStream()
        Get
            Return Not IsNothing(TrafficMonitor.log_stream)
        End Get
    End Property
    Public Shared Property FilePath() As String
        Get
            Return TrafficMonitor.path
        End Get
        Set(ByVal value As String)
            TrafficMonitor.path = value
        End Set
    End Property
    Public Shared Property LogStream() As FileStream
        Get
            Return TrafficMonitor.log_stream
        End Get
        Set(ByVal value As FileStream)
            TrafficMonitor.log_stream = value
        End Set
    End Property

    Public Shared Function haveLogFile() As Boolean
        Return File.Exists(TrafficMonitor.FilePath)
    End Function

    Public Shared Function garantueeLogFile() As Boolean
        Call TrafficMonitor.Initialize()
        Call TrafficMonitor.touchLogFile()

    End Function

    Public Shared Function closeStream() As Boolean

        If Not haveStream Then Return True

        Try
            TrafficMonitor.LogStream.Close()
            TrafficMonitor.LogStream = Nothing
            Return True

        Catch ex As Exception
            GlobalObject.Msg("Error closing log file '" & Path & "': " & ex.Message)
        End Try

    End Function
    Private Shared Sub Initialize()
        TrafficMonitor.currentDate = Now.ToString("MM/dd/yy")
        TrafficMonitor.FilePath = GlobalSetting.getLogfilePath() & LOGFILE_PREFIX & "_" & Replace(current_date, "/", "_") & ".xml"


    End Sub
    Public Shared Function touchLogFile() As Boolean
        Dim xml_template As String

        'CREATE
        Try

            'xml_template = "<traffic createdate=" & current_date & ">"
            xml_template = ""
            IO.File.WriteAllText(TrafficMonitor.FilePath, xml_template)

            Return True
        Catch ex As Exception

            GlobalObject.Msg("Error creating log file: " & ex.Message)
            Return False
        End Try


    End Function
    Public Shared Function createLogStream() As Boolean
        Dim xml_template As String


        'OPEN
        If File.Exists(TrafficMonitor.FilePath) Then

            Try
                TrafficMonitor.LogStream = New FileStream(path, FileMode.Open)
                Return True
            Catch ex As Exception
                GlobalObject.Msg("Error opening log file '" & path & "': " & ex.Message)
                Return False
            End Try

        End If

        'CREATE
        Try

            'xml_template = "<traffic createdate=" & current_date & ">"
            xml_template = ""
            IO.File.WriteAllText(TrafficMonitor.FilePath, xml_template)

            Return True
        Catch ex As Exception

            GlobalObject.Msg("Error creating log file: " & ex.Message)
            Return False
        End Try


    End Function
    Public Shared Function writeStringStatic(ByVal str As String, Optional ByVal retry_count As Integer = 0) As Boolean

        Try
            Call File.AppendAllText(TrafficMonitor.FilePath, str)
            If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                If GlobalObject.haveServerForm Then
                    Call logToGUI(XElement.Parse(str))
                End If
            End If
            Return True
        Catch ex As Exception
            If retry_count < MAX_WRITE_RETRIES Then
                retry_count += 1
                Threading.Thread.Sleep(50)
                Return writeStringStatic(str, retry_count)
            Else
                GlobalObject.MsgColored("Error append to log file: " & ex.Message, Color.Red)
                GlobalObject.MsgColored("Dropped log item due to error: " & str, Color.DarkGreen)
            End If
        End Try

        Return False
    End Function

    Public Shared Sub logToGUI(ByVal log_entry As XElement)
        'send to listview
        Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.serverLogLogObject, {log_entry})

    End Sub

    Public Shared Function writeStreamString(ByVal str As String) As Boolean

        If Not TrafficMonitor.haveStream() Then Return False

        Dim bt() As Byte
        Dim enc As UTF8Encoding

        enc = New UTF8Encoding

        bt = enc.GetBytes(str)

        Try
            Call TrafficMonitor.LogStream.Write(bt, 0, bt.Length)
            Return True
        Catch ex As Exception
            GlobalObject.Msg("Error writing to log file: " & ex.Message)
            GlobalObject.Msg("Dropped entry:" & str)
        End Try

        Return False
    End Function

    Public Shared Property currentDate
        Get
            Return TrafficMonitor.current_date
        End Get
        Set(ByVal value)
            TrafficMonitor.current_date = value
        End Set
    End Property

    Public Shared Sub LogJSONTraffic(ByVal obj As JSONMailJob)

        If Not haveLogFile() Then
            GlobalObject.Msg("Error obtaining logfile. Traffic item was not logged.")
            Exit Sub
        End If

        Dim current_time As String
        Dim xml_template As String
        Dim content_type As String
        Dim json_code As String

        Try

            current_time = Now.ToString("H:mm:ss")
            content_type = "Empty"
            json_code = ""

            xml_template = "<packet date=""$DATE"" time=""$TIME"" class=""$CLASS"" directive=""$DIRECTIVE"" headerobject=""$HEADEROBJECT"" content_type=""$CONTENT_TYPE"" direction=""$DIRECTION"" deliverystate=""$DELIVERYSTATE"" srcappname=""$SRCAPPNAME"" srcmpurl=""$SRCMPURL"" remoteip=""$REMOTEIP"" from=""$FROM"" to=""$TO"""
            xml_template = Replace(xml_template, "$DATE", TrafficMonitor.currentDate)
            xml_template = Replace(xml_template, "$TIME", current_time)
            xml_template = Replace(xml_template, "$CLASS", "Object")
            xml_template = Replace(xml_template, "$HEADEROBJECT", obj.json_obj.ObjectType)
            xml_template = Replace(xml_template, "$DIRECTIVE", HttpUtility.UrlEncode(obj.json_obj.Directive))

            If obj.json_obj.Directive <> "" Then
                content_type = "String"
                xml_template &= " stringvalue=""" & HttpUtility.UrlEncode(obj.json_obj.Directive.ToString) & """"
            End If

            xml_template &= ">$JSON</packet>"

            If Not IsNothing(obj.json_obj.Attachment) Then
                If content_type = "Empty" Then
                    content_type = obj.json_obj.ToString
                Else
                    content_type &= ", " & obj.json_obj.ToString
                End If

                'serialize attachment
                Try
                    'if not core, log datatype only
                    Dim attachment_type As Type
                    attachment_type = obj.json_obj.Attachment.GetType

                    If attachment_type Is GetType(String) Or attachment_type Is GetType(Integer) Or attachment_type Is GetType(Boolean) Or attachment_type Is GetType(Single) Or attachment_type Is GetType(Long) Or attachment_type Is GetType(Double) Then
                        json_code = obj.json_obj.Attachment.ToString
                    Else
                        json_code = "(" & obj.json_obj.Attachment.GetType.ToString & ")"
                    End If


                    'json_code = "todo: fix me" ' Json.JsonConvert.SerializeObject(obj.json_obj.Attachment)
                Catch ex As Exception
                    json_code = ex.Message
                End Try

            End If

            'FILL ALL
            xml_template = Replace(xml_template, "$CONTENT_TYPE", content_type)
            xml_template = Replace(xml_template, "$DIRECTION", obj.DirectionStr)
            If Not IsNothing(obj.json_obj.Sender) Then
                xml_template = Replace(xml_template, "$FROM", obj.json_obj.Sender.getClassNameFriendly())

                Dim original_req As TriniDATRequestInfo

                Try
                    original_req = obj.json_obj.Sender.getProcessDescriptor().getParent().Info

                    If Not IsNothing(original_req) Then
                        xml_template = Replace(xml_template, "$REMOTEIP", original_req.http_connection_handler.RemoteIP)

                        If original_req.haveApp Or original_req.haveMappingPoint Then
                            xml_template = Replace(xml_template, "$SRCMPURL", original_req.mapping_point_desc.URL)
                            xml_template = Replace(xml_template, "$SRCAPPNAME", original_req.App.ApplicationName)
                        Else
                            Throw New Exception("no application or mp associated")
                        End If
                    Else
                        Throw New Exception("application not found")
                    End If

                Catch ex As Exception
                    xml_template = Replace(xml_template, "$REMOTEIP", "NOT LOGGED")
                    xml_template = Replace(xml_template, "$SRCAPPNAME", "(ERROR)")
                    xml_template = Replace(xml_template, "$SRCMPURL", "(UNKNOWN)")
                End Try

                '$SRCMPURL
                '$SRCAPP
            Else
                xml_template = Replace(xml_template, "$FROM", "(UNKNOWN)")
            End If


            If Not IsNothing(obj.targetProcess) Then
                xml_template = Replace(xml_template, "$TO", obj.targetProcess.getClassNameFriendly())
            Else
                xml_template = Replace(xml_template, "$TO", "")
            End If

            xml_template = Replace(xml_template, "$JSON", HttpUtility.UrlEncode(json_code))
            xml_template = Replace(xml_template, "$DELIVERYSTATE", HttpUtility.UrlEncode(obj.DeliveryStateMessage))

            TrafficMonitor.writeStringStatic(xml_template & vbNewLine)

        Catch ex As Exception
            GlobalObject.Msg("Error formatting log entry: " & ex.Message)
        End Try
    End Sub

End Class
