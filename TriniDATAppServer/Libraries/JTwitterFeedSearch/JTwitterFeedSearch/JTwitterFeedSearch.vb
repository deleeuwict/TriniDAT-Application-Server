Option Explicit On
Option Compare Text
Imports System.Net.Sockets
Imports System.Text
Imports System.Web
Imports System.Net
Imports System.Collections.Specialized
Imports System.IO
Imports Newtonsoft.Json
Imports System.Xml
Imports TriniDATServerTypes
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 


Public Class JTwitterFeedSearch
    Inherits JTriniDATWebService
    'GlobalSetting.getStaticDataRoot()

    '
    Public email_count As Long = 0
    Public Shared Search_Query_dictionary As String()
    Public Shared ROTATION_ENTRY As Integer
    Public Const CONFIG_FILE_QUERYSTRING As String = "search_queries.txt"
    Public server_module_path As String
    Private extract_type As JTwitterFeedSearch_ExtractType
    Public seed_string As String

    Public Sub New()
        MyBase.New()

        ' Call testfunction(Nothing, Nothing, Nothing)
    End Sub
    Public Property TwitterSearchQuery As String
        Get
            Return Me.seed_string
        End Get
        Set(ByVal value As String)
            Me.seed_string = value
        End Set
    End Property
    Public Property StripType As JTwitterFeedSearch_ExtractType
        Get
            Return Me.extract_type
        End Get
        Set(ByVal value As JTwitterFeedSearch_ExtractType)
            Me.extract_type = value

        End Set
    End Property

    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean

        Dim conversation_function As TriniDAT_ServerGETFunction
        Dim email_function As TriniDAT_ServerGETFunction
        Dim debug_function As TriniDAT_ServerGETFunction

        conversation_function = New TriniDAT_ServerGETFunction(AddressOf doConversation)
        conversation_function.FunctionURL = Me.makeRelative("/conversation")

        email_function = New TriniDAT_ServerGETFunction(AddressOf doEmails)
        email_function.FunctionURL = Me.makeRelative("/email")

        debug_function = New TriniDAT_ServerGETFunction(AddressOf testfunction)
        debug_function.FunctionURL = Me.makeRelative("/test")

        http_function_table.Add(conversation_function)
        http_function_table.Add(debug_function)
        http_function_table.Add(email_function)

        Return True
    End Function

    Public Sub testfunction(ByVal processed_parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)


        Dim buffer As String

        buffer = File.ReadAllText("C:\temp\Main.xml")

        Me.getIOHandler().addOutput(buffer)

    End Sub


    Public Overrides Function DoConfigure() As Boolean
        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        http_events.event_onget = AddressOf OnGet

        getIOHandler().Configure(http_events)

        'default
        Me.extract_type = JTwitterFeedSearch_ExtractType.CONVERSATION
        Me.email_count = 0

        Return True

    End Function
    Public ReadOnly Property getServerModulePath As String
        Get
            Return Me.server_module_path
        End Get
    End Property
    Public Function getEmailQueryFile(ByVal session_path As String) As Boolean

        'init search query file
        Dim search_query_file As String

        search_query_file = session_path & Me.getClassNameFriendly() & "\" & CONFIG_FILE_QUERYSTRING
        ReDim Search_Query_dictionary(0)

        Try
            If Not File.Exists(search_query_file) Then
                Throw New Exception("Query file does not exist.")
            End If

            JTwitterFeedSearch.Search_Query_dictionary = File.ReadAllLines(search_query_file)
            JTwitterFeedSearch.ROTATION_ENTRY = 0

            Return True
        Catch ex As Exception
            Msg("Error reading search query file " & search_query_file & ": " & ex.Message & " @ " & ex.StackTrace.ToString)
        End Try

        Return False
    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.ObjectTypeName = "JNameCheckResult" Then
            Dim parsed_name As Object  '=JNameCheckResult

            'email address exists as tag in result

            parsed_name = obj.Attachment ' CType(obj.Attachment, JNameCheckResult)
            Msg("Received parsed name: " + parsed_name.getfullnameStr())

            Dim email_req As JSONObject

            email_req = New JSONObject
            email_req.ObjectType = "JSocialAccountEmailChecker"
            email_req.Directive = "VALIDATENAMEEMAIL"
            email_req.Attachment = parsed_name
            email_req.Tag = parsed_name.Tag 'email address
            'send email validation request.
            '   Msg("Got '" & parsed_name.getfullnameStr() & "'. Sending e-mail validation request for " + email_req.Tag)
            Me.getMailProvider().Send(email_req, Nothing, "JSocialAccountEmailChecker")
            Return False
        End If

        If obj.ObjectTypeName = "JEmailcheckResult" Then
            'entry_output_text = ""
            Dim email_validation_res As Object  'JEmailcheckResult
            Dim name_validation_res As Object  'JNameCheckResult
            Dim entry_output_jscript_entry As String

            email_validation_res = obj.Attachment 'CType(obj.Attachment, JEmailcheckResult)
            name_validation_res = email_validation_res.Tag

            If email_validation_res.isValid() Then
                'Msg("Keeping e-mail '" & email_validation_res.getEmail() & "' from user '" & name_validation_res.getFullNameStr() & "' name_rank=" & name_validation_res.getrank() & "' email_rank=" & email_validation_res.getrank())
                Dim statname As String
                statname = Me.getClean(email_validation_res.getEmail())

                '                Me.putStat = "verified_email=" & email_validation_res.getEmail()
                '                Me.putStat = email_validation_res
                Me.email_count += 1

                If Me.email_count = 1 Then
                    Me.putSingle = statname & "=" & email_validation_res.getEmail()
                End If

                Call Me.Log(name_validation_res.getFirstName() & "' " & name_validation_res.getMiddleName() & "' '" & name_validation_res.getLastName() & "' : " & email_validation_res.getEmail() & ".  Score: '" & name_validation_res.getrank() & " " & email_validation_res.getrank() & "")

                entry_output_jscript_entry = "parent.addEntry('" & HttpUtility.UrlEncode(name_validation_res.getFirstName()) & "','" & HttpUtility.UrlEncode(name_validation_res.getMiddleName()) & "','" & HttpUtility.UrlEncode(name_validation_res.getLastName()) & "', '" & email_validation_res.getEmail() & "','-', '" & name_validation_res.getrank() & "', '" & email_validation_res.getrank() & "');" & vbCrLf
                Me.getIOHandler().addOutput(entry_output_jscript_entry)

            Else
                Msg("Discarding invalid e-mail '" & email_validation_res.getEmail() & "' from user '" & name_validation_res.getFullNameStr() & "'")
            End If

            ''TODO: fullname check : twitter_result.<from_user_name>.Value
            Return False
        End If

        If obj.ObjectTypeName = "JAlpha" Then
            If obj.Directive = "MAPPING_POINT_START" Then
                'open xml file.
                Msg("Open XML revision..")
                Dim mp_global_config As MappingPointBootstrapData
                mp_global_config = obj.Attachment
                server_module_path = mp_global_config.static_path
                Return False
            End If
        End If

        Return False
    End Function

    Public Function getRootPoint() As String
        Return Me.getProcessDescriptor().getParent().getURI
    End Function

    Public Sub doConversation(ByVal processed_parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        Me.StripType = JTwitterFeedSearch_ExtractType.CONVERSATION
        Me.seed_string = "have"
        Call twitterSearch()

    End Sub

    Public Sub doEmails(ByVal processed_parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        Dim rd As Random

        Me.StripType = JTwitterFeedSearch_ExtractType.EMAIL

        If Not getEmailQueryFile(server_module_path) Then
            Me.getIOHandler().writeraw(True, "No query file exists.", False)
            Me.sendDestroyRequest()
            Exit Sub
        End If

        'pick random line
        rd = New Random

        Do Until Me.seed_string <> ""
            Me.seed_string = Me.Search_Query_dictionary(rd.Next(JTwitterFeedSearch.Search_Query_dictionary.Count - 1))
        Loop

        Call twitterSearch()

    End Sub
    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)


        'If Not IsNothing(HTTP_URI_Parameters) Then

        '    If HTTP_URI_Parameters("type") = "conversation" Then
        '        Me.StripType = JTwitterFeedSearch_ExtractType.CONVERSATION
        '        Me.seed_string = "have"
        '        Call twitterSearch()

        '    ElseIf HTTP_URI_Parameters("type") = "email" Then


        '        Dim rd As Random

        '        Me.StripType = JTwitterFeedSearch_ExtractType.EMAIL

        '        If Not getEmailQueryFile(server_module_path) Then
        '            Me.GetIOHandler().writeraw(True, "No query file exists.", False)
        '            Me.sendDestroyRequest()
        '            Exit Sub
        '        End If

        '        'pick random line
        '        rd = New Random

        '        Do Until Me.seed_string <> ""
        '            Me.seed_string = Me.Search_Query_dictionary(rd.Next(JTwitterFeedSearch.Search_Query_dictionary.Count - 1))
        '        Loop

        '        Call twitterSearch()

        '    End If
        'Else
        '    'default
        '    Me.StripType = JTwitterFeedSearch_ExtractType.CONVERSATION
        '    Me.seed_string = "have"
        '    Call twitterSearch()
        'End If


    End Sub

    Public Sub twitterSearch()

        Dim twitter_url As String
        Dim wc As New WebClient
        Dim jsondata As String
        Dim domain_ext_start As Integer
        Dim at_sign_start As Integer
        Dim space_pos As Integer
        Dim twitter_msg_text As String
        Dim twitter_msg_words() As String
        Dim email_str As String
        Dim twitter_msg_word As String
        Dim xdoc As XDocument
        Dim user_alias As String
        Dim msg_index As Integer
        Dim results As Boolean
        Dim parsed_msg_text As String


        results = False
        msg_index = 0


        Try

            twitter_url = "http://search.twitter.com/search.json?q=" & System.Web.HttpUtility.UrlEncode(Me.TwitterSearchQuery) & "&rpp=100&include_entities=true&result_type=recent"

            wc.Encoding = System.Text.Encoding.UTF8

            jsondata = "{'?xml': { '@version' : '1.0', '@standalone' : 'no' },'root' : " & wc.DownloadString(twitter_url) & " }"
            xdoc = XDocument.Parse(JsonConvert.DeserializeXmlNode(jsondata).OuterXml)

            Me.getIOHandler().addOutput("<html><body>" & vbCrLf)

            If StripType = JTwitterFeedSearch_ExtractType.EMAIL Then
                Me.getIOHandler().addoutput("<script language=""Javascript"">")
            End If


            For Each twitter_result In xdoc.<root>.<results>
                twitter_msg_text = twitter_result.<text>.Value

                'basic strippin' 
                twitter_msg_text = Replace(twitter_msg_text, "...", " ")
                twitter_msg_text = Replace(twitter_msg_text, "*", "")
                twitter_msg_text = Replace(twitter_msg_text, "..", " ")

                parsed_msg_text = ""
                For Each twitter_msg_word In twitter_msg_text.Split(" ")

                    'parse slang
                    If twitter_msg_word = "omg" Then
                        twitter_msg_word = "oh my god"
                    ElseIf twitter_msg_text = "lol" Then
                        twitter_msg_word = "chuckles"
                    ElseIf twitter_msg_text = "lmao" Then
                        twitter_msg_word = "laughing"
                    ElseIf twitter_msg_text = "rofl" Or twitter_msg_text = "rotfl" Then
                        twitter_msg_word = "hard chuckles"
                    End If

                    If twitter_msg_word <> "RT" And Left(twitter_msg_word, 1) <> "#" And Left(twitter_msg_word, 1) <> "@" And Left(twitter_msg_word, 1) <> "#" And Left(twitter_msg_word, 4) <> "http" Then
                        parsed_msg_text &= " " & twitter_msg_word
                    End If


                Next

                user_alias = twitter_result.<from_user_name>.Value 'fullname

                If parsed_msg_text <> "" Then

                    msg_index += 1

                    If StripType = JTwitterFeedSearch_ExtractType.CONVERSATION Then
                        twitter_msg_text = HttpUtility.HtmlDecode(twitter_msg_text)
                        Me.putStat = "message=" & twitter_msg_text

                        Call Me.Log(twitter_msg_text)

                        Me.getIOHandler().addOutput("<p>" & parsed_msg_text & "</p>")

                        If msg_index = 1 Then Exit For

                    ElseIf StripType = JTwitterFeedSearch_ExtractType.EMAIL Then


                        twitter_msg_words = Split(parsed_msg_text, " ")

                        'STRIP E-MAIL FROM MSG TEXT
                        For Each twitter_msg_word In twitter_msg_words

                            email_str = "" 'found email address
                            at_sign_start = InStr(twitter_msg_word, "@")

                            If at_sign_start > 0 And space_pos = 0 Then
                                domain_ext_start = InStr(at_sign_start, twitter_msg_word, ".")
                                'if between @ and ext is ivnalid chart, quit
                                If domain_ext_start > at_sign_start Then

                                    email_str = twitter_msg_word
                                    If Len(email_str) < 4 Then
                                        'invalidate
                                        email_str = ""
                                    End If ' too small
                                End If 'has . 
                            End If 'has @ and no space

                            If email_str <> "" Then

                                'send name + email validation requests
                                results = True

                                Dim msg As JSONObject

                                msg = New JSONObject

                                msg.ObjectType = "JSocialNameVerify"
                                msg.Directive = "VALIDATENAME"
                                msg.Tag = user_alias
                                msg.Attachment = email_str  ''will get forwarded to mail checker, immediately after namecheck result

                                'continue in async mode.
                                Me.getMailProvider().Send(msg, Nothing, "JSocialNameVerify")

                            End If

                        Next 'found word in msg text
                    End If
                End If

            Next 'twitter message x


            If StripType = JTwitterFeedSearch_ExtractType.EMAIL Then
                JTwitterFeedSearch.ROTATION_ENTRY = JTwitterFeedSearch.ROTATION_ENTRY + 1

                If JTwitterFeedSearch.ROTATION_ENTRY >= JTwitterFeedSearch.Search_Query_dictionary.Length Then
                    JTwitterFeedSearch.ROTATION_ENTRY = 0
                End If

                Dim req As JSONObject

                req = New JSONObject
                req.ObjectType = "JTextToSpeech"
                req.Directive = "SPEAK"

                If Me.email_count > 0 Then
                    req.Attachment = "send " & Me.email_count.ToString & " emails. simon has won."
                Else
                    req.Attachment = "simon thinks bored"
                End If

                Me.getMailProvider().send(req, Nothing, "JTextToSpeech")

            End If

            Me.getIOHandler().addOutput("</script> </body></html>")


            If Me.StripType = JTwitterFeedSearch_ExtractType.EMAIL Then
                Me.getIOHandler().addOutput("send " & Me.email_count.ToString.ToString & " email(s).")
            End If

            Me.getIOHandler().setEncoding(New UTF8Encoding)
            Me.getIOHandler().Flush(True, True)


        Catch ex As Exception
            Msg("Error downloading Twiter feed: " & ex.Message & " @ " & ex.StackTrace.ToString)
            Me.getIOHandler().writeraw(True, "Error: " & ex.Message, True)
        End Try

    End Sub

End Class


Public Structure SocialProfile
    Public firstname As String
    Public lastname As String
    Public userid As String
    Public email As String
    Public website As String

End Structure

Public Enum JTwitterFeedSearch_ExtractType
    CONVERSATION = 0
    EMAIL = 1
End Enum