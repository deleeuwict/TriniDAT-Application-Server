Option Explicit On
Option Compare Text
Imports TriniDATServerTypes
Imports TriniDATHTTPBrowser
Imports TriniDATPrimitiveXMLDOm
Imports System.Collections.Specialized
Imports TriniDATHTTPTypes
Imports System.Text

Public Class JCNNDemo
    Inherits JTriniDATWebService

    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable
        getIOHandler().Configure(http_events)

        Return True
    End Function
    Public Sub MyCNNTextUrlHandler(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        'set encoding if you want to parse non-english.
        Me.getIOHandler().setEncoding(New UTF8Encoding)

        Dim url As String
        Dim req As JSONObject

        url = "http://www.youtube.com"

        req = New JSONObject
        req.ObjectType = "JBrowseRequest"
        req.Directive = "GetText"
        req.Attachment = url

        req.Sender = Me
        Me.getMailProvider().send(req, Nothing, "JWebBrowser")

    End Sub
    Public Sub MyCNNLinksUrlHandler(ByVal parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)

        'set encoding if you want to parse non-english.
        Me.getIOHandler().setEncoding(New UTF8Encoding)

        'write tweet to browser.       Me.getIOHandler().addOutput(twitterSearch(parameter_list.getById("keywords").ParameterValue))

        Dim url As String
        Dim req As JSONObject

        '   url = "http://rt.com"
        url = "http://www.youtube.com/results?search_query=dnb"

        req = New JSONObject
        req.ObjectType = "JBrowseRequest"
        req.Directive = "GetLinks"
        req.Attachment = url
        req.Sender = Me
        Me.getMailProvider().send(req, Nothing, "JWebBrowser")

    End Sub
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal servers_function_table As TriniDATServerFunctionTable) As Boolean

        Dim CNN_texturl As TriniDAT_ServerGETFunction
        Dim CNN_links_messageurl As TriniDAT_ServerGETFunction
     
        'add '/text?keywords=..' sub-mapping point.
        CNN_texturl = New TriniDAT_ServerGETFunction(AddressOf MyCNNTextUrlHandler)
        CNN_texturl.FunctionURL = Me.makeRelative("/text")


        'add '/links?keywords=..' sub-mapping point.
        CNN_links_messageurl = New TriniDAT_ServerGETFunction(AddressOf MyCNNLinksUrlHandler)
        CNN_links_messageurl.FunctionURL = Me.makeRelative("/links")

        'Register our URLS .
        servers_function_table.Add(CNN_texturl)
        servers_function_table.Add(CNN_links_messageurl)

        'Completed.
        Return True
    End Function
 
    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean


        'catch message
        If obj.ObjectTypeName = "JOmega" And obj.Directive = "MAPPING_POINT_STOP" Then

            'store mapping point info
            Dim x As Integer
            x = x

        End If

        If Not obj.haveObjectTypeName Then
            Return False
        End If


        If obj.ObjectTypeName = "GetLinksResult" Then

            If TypeOf obj.Attachment Is TriniDATHTTPTypes.TriniDATLinkCollection Then

                For Each current_link As TriniDATHTTPTypes.TriniDATLinkElement In obj.Attachment
                    Dim log_obj As JSONObject

                    log_obj = New JSONObject
                    log_obj.Directive = "LOG"
                    log_obj.ObjectType = "JTEXTITEM"
                    log_obj.Attachment = current_link.getURL()
                    Me.getMailProvider().Send(log_obj, "JInteractiveConsole")

                    'temp   Me.putStat = "link=" & current_link.getURL()
                    Me.getIOHandler().addOutput(current_link.getURL() & vbNewLine)
                Next

                Me.getIOHandler().Flush(True, True)
            End If
        End If

        If obj.ObjectTypeName = "GetTextResult" Then

            If TypeOf obj.Attachment Is String Then
                Dim content_text As String
                Dim lines() As String
                Dim words() As String

                content_text = obj.Attachment
                lines = Split(content_text, vbNewLine)

                For Each line In lines
                    words = Split(line, " ")

                    For Each word In words

                        If Len(word) > 1 Then
                            'temp   Me.putSingle = Me.getClean(word) & "=" & word
                        End If
                    Next
                Next

                Me.getIOHandler().writeRaw(True, obj.Attachment, True)
            Else
                Dim x As Integer
                x = x

            End If

            Return False
        End If

        Return False
    End Function

  

End Class
