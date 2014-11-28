Option Compare Text
Option Explicit On

'TriniDAT Application Server - Webservice Sample Code for COPY/PASTE purposes.
'(c) 2013 GertJan de Leeuw | De Leeuw ICT | www.deleeuwict.nl | Visit the Developer Community Forum for more code examples. 

Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System.Net
Imports System.Text
Imports System.Web
Imports Newtonsoft.Json


Public Class JTwitterParser
    Inherits JTriniDATWebService

    Private my_mapping_point As MappingPointBootStrapData

    Public Overrides Function DoConfigure() As Boolean

        'Create local inbox to receive mapping point objects.
        Dim my_mailbox As TriniDATObjectBox_EventTable

        my_mailbox = New TriniDATObjectBox_EventTable
        my_mailbox.event_inbox = AddressOf Me.myInbox
        getMailProvider().Configure(my_mailbox, False)

        'Set-up a bare bone HTTP event table.
        Dim my_http_events As TriniDATHTTP_EventTable

        my_http_events = New TriniDATHTTP_EventTable
        my_http_events.event_onget = AddressOf OnGet
        my_http_events.event_onpost = AddressOf OnPost

        Return getIOHandler().Configure(my_http_events) 'True.
    End Function

    Public Overrides Function OnRegisterWebserviceFunctions(ByVal servers_function_table As TriniDATServerFunctionTable) As Boolean

        Dim twitter_searchurl As TriniDAT_ServerGETFunction
        Dim twitter_search_parameter As TriniDAT_ServerFunctionParameterSpec

        Dim twitter_speak_messageurl As TriniDAT_ServerGETFunction
        Dim twitter_speak_message_parameter As TriniDAT_ServerFunctionParameterSpec

        'add '/search?keywords=..' sub-mapping point.
        twitter_searchurl = New TriniDAT_ServerGETFunction(AddressOf MyTwitterSearchUrlHandler)
        twitter_searchurl.FunctionURL = Me.makeRelative("/search")

        'Require this URI to be called with at least one parameter.
        twitter_search_parameter = New TriniDAT_ServerFunctionParameterSpec()
        twitter_search_parameter.ParameterName = "keywords"
        twitter_search_parameter.ParameterType = "String"
        twitter_search_parameter.Required = True 'default.

        'Add keyword parameter.
        twitter_searchurl.Parameters.Add(twitter_search_parameter)


        'add '/speak?keywords=..' sub-mapping point.
        twitter_speak_messageurl = New TriniDAT_ServerGETFunction(AddressOf MyTwitterSpeakUrlHandler)
        twitter_speak_messageurl.FunctionURL = Me.makeRelative("/speak")

        twitter_speak_message_parameter = New TriniDAT_ServerFunctionParameterSpec()
        twitter_speak_message_parameter.ParameterName = "keywords"
        twitter_speak_message_parameter.ParameterType = "String"
        twitter_speak_message_parameter.Required = True 'default.

        'Add keyword parameter.
        twitter_speak_messageurl.Parameters.Add(twitter_speak_message_parameter)

        'Register our URLS .
        servers_function_table.Add(twitter_searchurl)
        servers_function_table.Add(twitter_speak_messageurl)

        'Completed.
        Return True
    End Function

    Public Sub MyTwitterSearchUrlHandler(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        'set encoding if you plan to use non-english.
        Me.getIOHandler().setEncoding(New UTF8Encoding)

        'write tweet to browser.
        Me.getIOHandler().addOutput(twitterSearch(parameter_list.getById("keywords").ParameterValue))

    End Sub
    Public Sub MyTwitterSpeakUrlHandler(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        Dim twitter_text As String
        Dim speak_msg As JSONObject

        twitter_text = twitterSearch(parameter_list.getById("keywords").ParameterValue)

        If InStr(twitter_text, " says ") > 0 Then
            'let's send a message to our friendly neighbor class JTextToSpeech
            '=========================================
            'JTextToSpeech knows everything about TTS stuff and we don't.  
            'This perfectly illustrates the power of TriniDAT app development. 
            'Our class is specialized in Twitter, while the neighbor  is specialized in tts etc.

            'craft a speak request message.
            '=====================

            speak_msg = New JSONObject
            speak_msg.ObjectType = "JTextToSpeech"
            speak_msg.Directive = "SPEAK"

            'Tell it to speak the tweet text.
            speak_msg.Attachment = twitter_text

            Me.getMailProvider().send(speak_msg, Nothing, "JTextToSpeech")

            'Also send the tweet to the browser.
            Me.getIOHandler().addOutput(twitter_text)
        Else
            Me.getIOHandler().addOutput("Error")
        End If

    End Sub

    Public Function myInbox(ByRef msg As JSONObject, ByVal from_url As String) As Boolean

        'Catch mapping point startup messages.
        If msg.ObjectTypeName = "JAlpha" And msg.Directive = "MAPPING_POINT_START" Then

            'Store all mapping point config locally.
            Me.my_mapping_point = CType(msg.Attachment, MappingPointBootstrapData)
            Return False
        End If

        'Catch mapping point shutdown messages.
        If msg.ObjectTypeName = "JOmega" And msg.Directive = "MAPPING_POINT_STOP" Then

            Return False
        End If

        Return False
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        'Your GET code handler goes here.
        Me.getIOHandler().addOutput("Hello world @ GET handler.")

        'Add JTextToSpeech to your mapping point dependency list to make this object exchange example work:
        'Dim speak_request As JSONObject

        'speak_request = New JSONObject
        'speak_request.ObjectType = "JTextToSpeech"
        'speak_request.Directive = "SPEAK"
        'speak_request.Attachment = "Somebody just visited my website, isn't that just wonderful."
        'Me.getMailProvider().send(speak_request, Nothing, "JTextToSpeech")

    End Sub

    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        'Your POST code handler goes there.

        Me.getIOHandler().addOutput("Hello world @ POST handler.")

    End Sub

    Public Function twitterSearch(ByVal query As String) As String

        Dim search_url As String
        Dim wc As New WebClient
        Dim jsondata As String
        Dim jxml As XDocument
        Dim user_alias As String
        Dim twitter_result As XElement

        Try
            'retrieve tweet.
            search_url = "http://search.twitter.com/search.json?q=" & query & "g&rpp=1&include_entities=true&result_type=recent"

            'set encoding.
            wc.Encoding = System.Text.Encoding.UTF8

            'Wrap output in parential nodes to get the correct XML.
            jsondata = "{'?xml': { '@version' : '1.0', '@standalone' : 'no' },'root' : " & wc.DownloadString(search_url) & " }"

            'Convert JSON -> XML.
            jxml = XDocument.Parse(JsonConvert.DeserializeXmlNode(jsondata).OuterXml)

            'Extract result.
            twitter_result = jxml.<root>.<results>(0)

            If Not IsNothing(twitter_result) Then
                Dim twitter_msg_text As String

                'Parse & clean up tweet
                twitter_msg_text = twitter_result.<text>.Value
                twitter_msg_text = Replace(twitter_msg_text, "...", " ")
                twitter_msg_text = Replace(twitter_msg_text, ";", "")
                twitter_msg_text = Replace(twitter_msg_text, "@", "")
                twitter_msg_text = Replace(twitter_msg_text, "..", " ")
                twitter_msg_text = Trim(twitter_msg_text)
                twitter_msg_text = WebUtility.HtmlDecode(twitter_msg_text)

                user_alias = twitter_result.<from_user_name>.Value 'fullname

                'return formatted tweet text to caller.
                Return user_alias & " says " & Chr(34) & twitter_msg_text & Chr(34)
            Else
                Return "nobody speaks that kind of language"
            End If

        Catch ex As Exception
            Return "There was an error: " & ex.Message
        End Try
    End Function

End Class

