Imports System.Web
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.IO
Imports System
Imports System.Runtime.CompilerServices
Imports TriniDATServerTypes

<Assembly: SuppressIldasmAttribute()> 

Public Class JRandomSocialId
    Inherits JTriniDATWebService

    Private new_id As JRandomSocialIdEntry
    Private IdFile As JRandomSocialIdFile

    'describes what we are doing 
    Private service_state As String
    Private Shared current_GlobalIdentitiesFolder As String

    Private Property CurState As String
        Get
            Return Me.service_state
        End Get
        Set(ByVal value As String)
            Me.service_state = value
        End Set
    End Property
    Public Shared ReadOnly Property GlobalIdentitiesFolder() As String
        Get
            Return JRandomSocialId.current_GlobalIdentitiesFolder & "JRandomSocialId\id\"
        End Get
    End Property

    Public Sub ListAllIDFiles()

        Dim id_dir As DirectoryInfo
        Dim x As Integer

        id_dir = New DirectoryInfo(JRandomSocialId.GlobalIdentitiesFolder)
        x = 0

        For Each curfile As FileInfo In id_dir.GetFiles()

            Me.GetIOHandler().addOutput(curfile.FullName)
            x = x + 1
        Next

        Me.GetIOHandler().addOutput("Total " & x.ToString & " files.")

        Me.GetIOHandler().flushOutput(True, True)


    End Sub
    Public Sub setIdentityList(ByRef val As JRandomSocialIdFile)
        Me.IdFile = val
    End Sub
    Private Sub createBlank()
        Me.new_id = New JRandomSocialIdEntry()
    End Sub

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
        http_events.event_onpost = AddressOf OnPost

        getIOHandler().Configure(http_events)

        Return True

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "Setting_ModulePath" Then
            JRandomSocialId.current_GlobalIdentitiesFolder = obj.Directive 
            Return False
        End If

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Return False
        End If

        If obj.ObjectTypeName = "JFreebaseXMLDocument" And Me.CurState = "create.fromdeceased" Then
            Dim xdoc As XDocument

            xdoc = CType(obj.Attachment, XDocument)
            Call createFromDeceasedResume(xdoc)
            Return False
        ElseIf obj.ObjectTypeName = "JFreebaseError" And Me.CurState = "create.fromdeceased" Then
            Dim server_err As String

            server_err = CType(obj.Attachment, String)
            Call createFromDeceasedFailed(server_err)

            Return False
        End If


        Return False
    End Function

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        Dim createSocialID As String
        Dim createSocialIDByDeceasedName As String
        Dim listFiles As String
        Dim nextIdTest As String
        Dim randomentry As String
        Dim randomjsonfile As String

        nextIdTest = Me.getProcessDescriptor().getParent().getURI() & "nextid"
        randomentry = Me.getProcessDescriptor().getParent().getURI() & "rnd"
        randomjsonfile = Me.getProcessDescriptor().getParent().getURI() & "rndjsonfile"

        listFiles = Me.getProcessDescriptor().getParent().getURI() & "files"
        createSocialID = Me.getProcessDescriptor().getParent().getURI() & "create/new"
        createSocialIDByDeceasedName = Me.getProcessDescriptor().getParent().getURI() & "create/new/fromdeceased"

        'list all filenames
        If HTTP_URI_Path = listFiles Then
            Call ListAllIDFiles()
            Exit Sub
        End If

        If HTTP_URI_Path = randomentry Then
            Dim rnd_entry As JRandomSocialIdEntry
            rnd_entry = JRandomSocialIdFile.getRandomizedEntry()
            If Not IsNothing(rnd_entry) Then
                Me.GetIOHandler().writeRaw(True, rnd_entry.getFormatedEntry(), True)
            Else
                Me.GetIOHandler().writeRaw(True, "No identities", True)
            End If
            Exit Sub
        End If

        If HTTP_URI_Path = randomjsonfile Then
            Dim rnd_entry As JRandomSocialIdFile
            rnd_entry = JRandomSocialIdFile.getRandomFile()

            If Not IsNothing(rnd_entry) Then
                Me.GetIOHandler().writeRaw(True, rnd_entry.GetAsJSON(), True)
            Else
                Me.GetIOHandler().writeRaw(True, "No identities", True)
            End If
            Exit Sub
        End If
        'getRandomizedEntry

        If HTTP_URI_Path = nextIdTest Then
            GetIOHandler().writeRaw(True, JRandomSocialIdFile.getNextId().ToString, True)
            Exit Sub
        End If

        'Verified add
        If HTTP_URI_Path = createSocialID Then
            Me.GetIOHandler().writeRaw(True, "Create new uri", True)
            Exit Sub
        End If


        If HTTP_URI_Path = createSocialIDByDeceasedName Then
            'Me.GetIOHandler().writeRaw(True, "Create new from decased", True)
            'got fullname, need STREET ADDRESS, CITY, STATE, ZIP, COUNTRY
            Me.createFromDeceased(HttpUtility.UrlDecode(HTTP_URI_Parameters("firstname")), HttpUtility.UrlDecode(HTTP_URI_Parameters("lastname")))
            Exit Sub
        End If

        Me.GetIOHandler().writeRaw(True, "unknown uri", True)

    End Sub
    Private Function getBlank() As JRandomSocialIdEntry
        Return Me.new_id
    End Function
    Public Sub createFromDeceased(ByVal first As String, ByVal last As String)

        Dim freebase_req As JSONObject

        'INIT
        Me.createBlank()

        'save
        Me.getBlank().setFirstName(first)
        Me.getBlank().setLastName(last)

        'now query for random company address

        freebase_req = New JSONObject
        freebase_req.ObjectType = "JFreebaseXMLQuery"
        freebase_req.Directive = "[{" & Chr(34) & "id" & Chr(34) & " : null," & Chr(34) & "name" & Chr(34) & " : null, " & Chr(34) & "type" & Chr(34) & ":" & Chr(34) & "/business/business_operation" & Chr(34) & "," & Chr(34) & "headquarters" & Chr(34) & " : [{" & Chr(34) & "street_address" & Chr(34) & " : [], " & Chr(34) & "street_address!=" & Chr(34) & " : " & Chr(34) & "" & Chr(34) & "," & Chr(34) & "citytown" & Chr(34) & " : []," & Chr(34) & "postal_code" & Chr(34) & " : []," & Chr(34) & "postal_code!=" & Chr(34) & " : " & Chr(34) & "" & Chr(34) & " ," & Chr(34) & "citytown!=" & Chr(34) & " : " & Chr(34) & "" & Chr(34) & "," & Chr(34) & "state_province_region" & Chr(34) & " : [], " & Chr(34) & "state_province_region!=" & Chr(34) & " :" & Chr(34) & "" & Chr(34) & ", " & Chr(34) & "country" & Chr(34) & " : [], " & Chr(34) & "country!=" & Chr(34) & " : " & Chr(34) & "" & Chr(34) & " }]}]"
        freebase_req.Sender = Me

        Me.CurState = "create.fromdeceased"
        Me.getMailProvider().Send(freebase_req, Nothing, "JFreebaseQuery")

        'wait for reply
    End Sub
    Public Sub createFromDeceasedResume(ByVal xdoc As XDocument)

        Dim company_address As XElement
        Dim total_entries As Long
        Dim random_index As Integer
        Dim rnd As Random
        Dim x As Integer

        Dim q = From c In xdoc.Descendants("root").Descendants("result").Descendants("headquarters")
                Where InStr(c.Element("country"), "America") > 0
                Select c

        rnd = New Random
        total_entries = q.Count - 1
        random_index = rnd.Next(total_entries)
        'For Each address In q
        '    Me.GetIOHandler().addOutput(address & vbNewLine)
        'Next



        For Each company_address In q

            If x = random_index Then
                Me.getBlank().setStreetAddress(company_address.Element("street_address"))
                Me.getBlank().setZIPCode(company_address.Element("postal_code"))
                Me.getBlank().setCountryLong(company_address.Element("country"))
                Me.getBlank().setCity(company_address.Element("citytown"))
                Me.getBlank().setStateLong(company_address.Element("state_province_region"))
            End If

            x = x + 1
        Next

        JRandomSocialIdFile.createNewFromNextId(Me.getBlank())

        Dim tst As JRandomSocialIdFile

        tst = New JRandomSocialIdFile((JRandomSocialIdFile.getNextId() - 1) & ".dat".ToString)
        tst = tst

        Me.GetIOHandler.addOutput(Me.getBlank().getFormatedEntry())
        Me.GetIOHandler().flushOutput(True, True)



    End Sub
    Public Sub createFromDeceasedFailed(ByVal server_err As String)

        Me.GetIOHandler().addOutput("There was an error retrieving address via Freebase ")
        Me.GetIOHandler().addOutput(vbNewLine & server_err)
        Me.GetIOHandler().flushOutput(True, True)

    End Sub
    Public Sub OnPost(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        Msg("OnPost")
        Dim firstname As String

        firstname = HTTP_URI_Parameters("fullname")

        If firstname <> "" Then
            Msg("New social id seed entrY: " & firstname)
        End If

        'disconnect
        If Me.GetIOHandler().getConnection().isConnected() Then
            Me.GetIOHandler().getConnection().forceDisconnect()
        End If
    End Sub

End Class
