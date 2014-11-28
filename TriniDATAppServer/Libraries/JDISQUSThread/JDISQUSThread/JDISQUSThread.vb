Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports Newtonsoft.Json
Imports TriniDATServerTypes
Imports System.Web
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 
Public Class JDISQUSThread
    Inherits JTriniDATWebService

    Private DISQUS_THREAD_API As String = "http://disqus.com/api/3.0/users/listActivity?user=$ID&api_key=E8Uh5l5fHZ6gD8U3KycjAIAk46f68Zw7C6eW8WSjZvCLXebZ7p0r1yrYDrLilk2F"
    ' Private disqus_global_retrycount As Integer
    '
    Public runtimeCount As Integer = 0

    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub

    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox
        mb_events.event_delivered = AddressOf delivered
        mb_events.event_err = AddressOf deliveryerr

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet
        http_events.event_onpost = AddressOf OnPost
        getIOHandler().Configure(http_events)

        Return True

        ' Me.disqus_global_retrycount = 0
    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean
        Msg(">> user inbox called.. Type=" & obj.ObjectTypeName)


        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("Omega notification received.")

        End If

        Return False
    End Function
    Public Sub delivered(ByVal obj As JSONObject, ByVal destination_url As String)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub

    Public Function dobing_translate(ByVal langcode As String, ByVal txt As String) As String
        Dim bing_translate_url As String
        Dim wc As WebClient
        Dim translation As String
        Dim jsondata As String
        Dim xmldoc As XDocument


        txt = Replace(txt, Chr(13), "")
        txt = Replace(txt, Chr(10), "")

        bing_translate_url = "http://api.microsofttranslator.com/v2/ajax.svc/TranslateArray?appId=%22TU_t5KEyaohL_gpfb3ZBrXVdR5CBDhW2QQ0YRv93qN_00GTT54t0bbdV4VrelufJm%22&texts=[%22" & HttpUtility.UrlEncode(txt, System.Text.Encoding.UTF8) & ")%22]&from=%22%22&to=%22" & langcode & "%22&oncomplete=_mstc1&onerror=_mste1&loc=en&ctr=&rgp=14bdf28c"

        wc = New WebClient

        Try

            wc.Encoding = System.Text.Encoding.UTF8
            jsondata = wc.DownloadString(bing_translate_url)

            If InStr(jsondata, "_mste") > 0 Then
                Msg("Bing translate err: " & jsondata)
                Return "error"
            End If

            'remove wrapped Javascript function call _mstc1
            jsondata = Replace(jsondata, "_mstc1(", "")
            jsondata = Replace(jsondata, ");", "")

            'transform
            jsondata = Replace(jsondata, "/", "_")
            jsondata = "{'?xml': { '@version' : '1.0', '@standalone' : 'no' },'root' : " & jsondata & " }"
            xmldoc = XDocument.Parse(JsonConvert.DeserializeXmlNode(jsondata).OuterXml)

            translation = xmldoc.<root>.Value
            Return translation


        Catch ex As Exception
            Msg("Bing translate error: " & ex.Message)
        End Try

        Return ""
    End Function

    Public Function getAThread() As String
        Dim wc As WebClient
        Dim url As String
        Dim rnd As Random
        Dim disqus_userid As Integer
        Dim xmldoc As XDocument
        Dim jsondata As String
        Dim connection_retries_count As Integer

        wc = New WebClient
        wc.Encoding = System.Text.Encoding.UTF8

        connection_retries_count = 0
        jsondata = ""
        Randomize()

        Do Until connection_retries_count = 10
            On Error Resume Next
            Err.Clear()
            rnd = New Random
            url = Me.DISQUS_THREAD_API
            jsondata = ""
            disqus_userid = rnd.Next(65000)
            url = Replace(url, "$ID", disqus_userid.ToString)
            jsondata = wc.DownloadString(url)

            If Err.Number = 0 And InStr(jsondata, "raw_message") > 0 Then Exit Do
            connection_retries_count = connection_retries_count + 1
        Loop

        If connection_retries_count = 10 Then
            Msg("Error connecting to disqus userid")
            Return Nothing
        End If

        'Try


        jsondata = Replace(jsondata, "/", "_")
        jsondata = "{'?xml': { '@version' : '1.0', '@standalone' : 'no' },'root' : " & jsondata & " }"
        xmldoc = XDocument.Parse(JsonConvert.DeserializeXmlNode(jsondata).OuterXml)

        For Each comment In xmldoc.<root>.<response>
            Dim msg As String
            msg = comment.<object>.<raw_message>.Value

            If Len(msg) > 5 Then
                If InStr(msg, "?") = 0 Then
                    'no questions
                    If InStr(msg, ":") = 0 Then
                        'no replies
                        If InStr(msg, "blog") = 0 And InStr(msg, "dont") = 0 And InStr(msg, "yes") = 0 And InStr(msg, "don't") = 0 And InStr(msg, "comment") = 0 And InStr(msg, "you") = 0 And InStr(msg, "interest") = 0 And InStr(msg, "mine") = 0 And InStr(msg, " think ") = 0 And InStr(msg, " he ") = 0 And InStr(msg, " his ") = 0 And InStr(msg, " her ") = 0 And InStr(msg, " we ") = 0 And InStr(msg, " will ") = 0 And InStr(msg, "indeed") = 0 And InStr(msg, " she ") = 0 And InStr(msg, " this ") = 0 And InStr(msg, " them ") = 0 And InStr(msg, " it ") = 0 And InStr(msg, " it's ") = 0 And InStr(msg, " you're ") = 0 And InStr(msg, "love") = 0 And InStr(msg, "hate") = 0 And InStr(msg, "thanks") = 0 And InStr(msg, "maybe") = 0 Then                            'no blog awareness
                            Me.putSingle = "retries" & connection_retries_count.ToString & "++"
                            Return msg
                        End If


                    End If
                End If
            End If

        Next


        'nothing found, find again
        'If Me.disqus_global_retrycount < 25 Then
        Me.putSingle = "retriesbeyond10++"
        'Me.disqus_global_retrycount = Me.disqus_global_retrycount + 1
        Return getAThread()
        ' Else
        'Return ""
        'End If



    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnGet")
        runtimeCount = runtimeCount + 1
        Dim speak_request As JSONObject

        speak_request = New JSONObject
        speak_request.ObjectType = "JTextToSpeech"
        speak_request.Directive = "SPEAK"
        speak_request.Attachment = "Somebody just visited my website, isn't that just awefully wonderful."
        Me.getMailProvider().send(speak_request, Nothing, "JTextToSpeech")

        Dim random_disqus_thread As String

        random_disqus_thread = getAThread()

        If Not IsNothing(random_disqus_thread) Then

            'set UTF-8 encoding
            Me.getIOHandler().addOutput("<html><head>")

            'header
            Me.getIOHandler().addOutput("<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />")

            'doc start
            Me.getIOHandler().addOutput("</head><body>")


            If HTTP_URI_Parameters("lang") <> "" And random_disqus_thread <> "" Then
                Dim langcode As String
                langcode = HTTP_URI_Parameters("lang")
                random_disqus_thread = random_disqus_thread & vbCrLf & vbCrLf & Me.dobing_translate(langcode, random_disqus_thread)
            End If

            'thread data
            Me.getIOHandler().addOutput(random_disqus_thread)

            'doc end
            Me.getIOHandler().addOutput("</body></html>")
            getIOHandler.setEncoding(New UTF8Encoding())
            Me.getIOHandler().Flush(True, True)


        Else
            Me.getIOHandler().writeRaw(True, "x", True)
        End If

    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")
        runtimeCount = runtimeCount + 1

    End Sub

End Class
