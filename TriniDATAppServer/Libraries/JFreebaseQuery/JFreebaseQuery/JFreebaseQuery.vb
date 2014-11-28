Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.Web
Imports Newtonsoft.Json
Imports System.Xml
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JFreebaseQuery

    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH

    Public Const freebase_mqlread As String = "https://www.googleapis.com/freebase/v1/mqlread"
    Public Const OWN_MAPPING_POINT As String = "/freebase/"

    Private last_error_string As String
    Public runtimeCount As Integer = 0

    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub
    Public Property LastServerResponse As String
        Get
            Return Me.last_error_string
        End Get
        Set(ByVal value As String)
            Me.last_error_string = value
        End Set
    End Property

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
        http_events.event_onget = AddressOf OnGet

        GetIOHandler().Configure(http_events)


    End Sub

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "JFreebaseXMLQuery" And obj.Directive <> "" Then
            'ML QUERY should be in Obj.Content
            'replies XDocument in resp objectdata
            'otherwise, JFreebaseError and server response 

            Dim reply As JSONObject
            reply = New JSONObject

            reply.Attachment = freebasequeryAsXML(obj.Content)


            If Not IsNothing(reply.Attachment) Then
                reply.ObjectType = "JFreebaseXMLDocument"
            Else
                reply.ObjectType = "JFreebaseError"
                reply.Directive = Me.LastServerResponse
                reply.Attachment = reply.Content
            End If

            reply.Sender = Me
            Me.getMailProvider().Send(reply, Nothing, obj.Sender.getClassName())
            Return False
        End If



        Return False
    End Function
    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        If Left(HTTP_URI_Path, Len(JFreebaseQuery.OWN_MAPPING_POINT)) <> JFreebaseQuery.OWN_MAPPING_POINT Then
            'ignore get module when embedded in other mapping point
            Exit Sub
        End If


        Dim freebase_query_str As String
        Dim retval As String
        Dim XML_SERVICE_URL As String

        'INIT
        XML_SERVICE_URL = Me.getProcessDescriptor().getParent().getURI() & "xml"

        freebase_query_str = HttpUtility.UrlDecode(HTTP_URI_Parameters("q"))

        If freebase_query_str = "" Then
            Me.GetIOHandler().writeRaw(True, "no work!", True)
            Exit Sub
        End If


        If HTTP_URI_Path = XML_SERVICE_URL Then
            Dim retvalxmldoc As XDocument
            retvalxmldoc = freebasequeryAsXML(freebase_query_str)
            Me.GetIOHandler().writeRaw(True, retvalxmldoc.ToString, True)
            Exit Sub
        End If


        retval = freebasequery(freebase_query_str)
        Me.GetIOHandler().writeRaw(True, retval, True)


    End Sub

    Public Function freebasequeryAsXML(ByVal query As String) As XDocument

        Dim xmldoc As XDocument
        Dim jsondata As String


        Dim retval As String

        retval = Nothing

        Try
            retval = freebasequery(query)

            'replace illegal XML chars
            retval = Replace(retval, "/", "_")

            If Not IsNothing(retval) Then
                jsondata = "{'?xml': { '@version' : '1.0', '@standalone' : 'no' },'root' : " & retval & " }"
                xmldoc = XDocument.Parse(JsonConvert.DeserializeXmlNode(jsondata).OuterXml)

                If Not IsNothing(xmldoc) Then
                    Me.LastServerResponse = ""
                End If

                Return xmldoc
            Else
                Err.Raise(0, 0, "No results or error: " & Me.LastServerResponse)
            End If

        Catch ex As Exception
            Msg("Freebase query error: " & ex.Message)
        End Try

        Return Nothing
    End Function

    Public Function freebasequery(ByVal query As String) As String


        Dim wc As WebClient
        Dim freebase_api As String
        Dim result As String
        Dim errlog As String

        'EXAMPLES: 
        '=========
        '
        'GET PERSONS FROM BELGIUM:
        '
        '[{ "id" : null,   "name" : null,   "type" : "/people/person",   "nationality" : "Belgium" }]
        ' 
        ' only queries embedded in [ ] will return arrays, otherwise a unique blah blah error is returned.
        ' 
        'GET RANDOM BUSINESS ADDRESS:
        '[{
        '"id" : null,
        '"name" : null, 
        '"type":"/business/business_operation",
        '"headquarters" : [{"street_address" : [], "street_address!=" : "", "citytown" : [],"postal_code" : [],"postal_code!=" : "" ',"citytown!=" : "","state_province_region" : [], "state_province_region!=" : "", "country" : [], "country!=" : "" }]
        '}]

        result = Nothing

        'init
        query = HttpUtility.UrlEncode(query)
        freebase_api = JFreebaseQuery.freebase_mqlread
        freebase_api = freebase_api & "?query=" & query

        Me.putStat = "query=" & query

        wc = New WebClient
        Try


            result = wc.DownloadString(freebase_api)
            LastServerResponse = result
            Return result

        Catch wex As WebException

            errlog = "Freebase query error: " & wex.Message
            Msg(errlog)
            result = Nothing
        End Try

        Return result
    End Function


End Class
