'Empty JTriniDATWebService instance for COPY/PASTE purposes.
Option Compare Text
Option Explicit On

Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System.Xml
Imports System.IO
Imports System.Web
Imports System.Threading
Imports Newtonsoft.Json
Imports TriniDATDictionaries

Public Class JStats
    Inherits JTriniDATWebService

    Private Const format_date_str As String = "MM/dd/yy"
    Private Const format_time_str As String = "H:mm:ss zzz"
    Private mp_global_config As MappingPointBootstrapData
    Private valid_nodename_chars As TriniDATCharDictionaries

    Private Sub InitStack()
        Me.getProcessDescriptor().Stack = New JStats_Stack
    End Sub
    Public ReadOnly Property getStack() As JStats_Stack
        Get
            Return Me.getProcessDescriptor().Stack
        End Get
    End Property
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        Call InitStack()

        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable

        getIOHandler().Configure(http_events)

        Dim upper_dic As TriniDATCharDictionary
        Dim lower_dic As TriniDATCharDictionary
        Dim special_char_dic As TriniDATCharDictionary

        upper_dic = New TriniDATCharDictionary("ClassicASCIILowercase", {ChrW(&H61), ChrW(&H62), ChrW(&H63), ChrW(&H64), ChrW(&H65), ChrW(&H66), ChrW(&H67), ChrW(&H68), ChrW(&H69), ChrW(&H6A), ChrW(&H6B), ChrW(&H6C), ChrW(&H6D), ChrW(&H6E), ChrW(&H6F), ChrW(&H70), ChrW(&H71), ChrW(&H72), ChrW(&H73), ChrW(&H74), ChrW(&H75), ChrW(&H76), ChrW(&H77), ChrW(&H78), ChrW(&H79), ChrW(&H7A)})
        lower_dic = New TriniDATCharDictionary("ClassicASCIIUppercase", {ChrW(&H41), ChrW(&H42), ChrW(&H43), ChrW(&H44), ChrW(&H45), ChrW(&H46), ChrW(&H47), ChrW(&H48), ChrW(&H49), ChrW(&H4A), ChrW(&H4B), ChrW(&H4C), ChrW(&H4D), ChrW(&H4E), ChrW(&H4F), ChrW(&H50), ChrW(&H51), ChrW(&H52), ChrW(&H53), ChrW(&H54), ChrW(&H55), ChrW(&H56), ChrW(&H57), ChrW(&H58), ChrW(&H59), ChrW(&H5A)})
        special_char_dic = New TriniDATCharDictionary("ClassicASCIISpecialChars", {"_"})

        Me.valid_nodename_chars = New TriniDATCharDictionaries("", New List(Of TriniDATCharDictionary)({special_char_dic, upper_dic, lower_dic}))

        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean
        'INIT
        If obj.ObjectTypeName = "JAlpha" Then
            If obj.Directive = "MAPPING_POINT_START" Then
                'open xml file.
                Msg("Open XML revision..")
                mp_global_config = obj.Attachment
                Me.setFilePaths()
                If Not Me.loadChainStatistics() Then
                    If Not createStatsFile(obj.Sender.getClassNameFriendly()) Then
                        Msg("Cannot open stats file ")
                    End If
                End If
            End If
            Return False
        End If

        If obj.ObjectTypeName = "JOmega" Then
            If obj.Directive = "MAPPING_POINT_STOP" Then
                'save xml file.
                Msg("Closing XML revision..")
                If Me.getStack().have_stats = True Then
                    Thread.Sleep(50)
                    Call writeStatistics()
                    Me.getStack().mapping_point_xml_doc = Nothing
                End If
                Return False
            End If
            Return False
        End If

        If Left(obj.ObjectTypeName, 4) <> "JPut" Then
            Return False
        End If

        If Not IsNothing(Me.getStack().mapping_point_xml_doc) Then
            Me.addStatistic(obj.Sender.getClassNameFriendly(), PreProcess_Statistic_String(obj.Sender.getClassNameFriendly(), obj.Attachment, (obj.ObjectTypeName = "JPutStatUnique")), (obj.ObjectTypeName = "JPutStatUnique"))
        Else
            'stats file does not exist
            obj = obj
        End If


        Return False
    End Function

    Private Function PreProcess_Statistic_String(ByVal for_class_friendly_name As String, ByVal unproccessed_val As Object, ByVal unique_var As Boolean) As Preprocessing_result

        Dim pos As Integer
        Dim retval As Preprocessing_result
        Dim stat_node As XElement
        Dim stat_varname As String
        Dim string_stat_value As String
        Dim is_anonymous_stat As Boolean
        Dim is_string_stat As Boolean

        retval = New Preprocessing_result

        is_anonymous_stat = False
        is_string_stat = False

        If TypeOf unproccessed_val Is String Then
            string_stat_value = unproccessed_val
            is_string_stat = True

            pos = InStr(string_stat_value, "=")

            If pos > 0 Then
                'key=value
                'name/value pair
                stat_varname = Mid(string_stat_value, 1, pos - 1)
                retval.stat_name = stat_varname
                retval.stat_value = HttpUtility.UrlEncode(Mid(string_stat_value, pos + 1))
                Return retval
            ElseIf Right(string_stat_value, 2) = "++" Then
                'key_value++
                'value=number
                'todo: find current value in xdoc and ++ it
                stat_varname = Mid(string_stat_value, 1, Len(string_stat_value) - 2)
                retval.stat_name = stat_varname
                stat_node = getExistingStat(for_class_friendly_name, stat_varname, unique_var)
                If Not IsNothing(stat_node) Then
                    'up the value
                    If IsNumeric(stat_node.Value) Then
                        retval.stat_value = (CLng(stat_node.Value) + 1).ToString
                        Return retval
                    End If
                Else
                    Msg("++ Statistic does not exist.")
                    'act as initializer
                    Return PreProcess_Statistic_String(for_class_friendly_name, stat_varname & "=1", False)
                End If
                'should not come here
            ElseIf Right(string_stat_value, 2) = "--" Then
                'value=number
                'todo: find current value in xdoc and ++ it
                stat_varname = Mid(string_stat_value, 1, Len(string_stat_value) - 2)
                retval.stat_name = stat_varname
                stat_node = getExistingStat(for_class_friendly_name, stat_varname, unique_var)
                If Not IsNothing(stat_node) Then
                    'down the value
                    If IsNumeric(stat_node.Value) Then
                        retval.stat_value = (CLng(stat_node.Value) - 1).ToString
                        Return retval
                    End If
                Else
                    Msg("--  Statistic does not exist.")
                    'act as initializer
                    Return PreProcess_Statistic_String(for_class_friendly_name, stat_varname & "=1", False)
                End If
            Else
                'invalid : no assigment operator
                Me.Msg("Invalid stat directive: " & unproccessed_val & ".")
                is_anonymous_stat = True
            End If

        Else
            is_anonymous_stat = True
        End If


        If Not is_anonymous_stat And Not is_string_stat Then
            GlobalObject.Msg("Error: unknown statdata.")
            Return Nothing
        ElseIf unique_var = True Then
            'Stat is anonymous object
            'serialize this object
            GlobalObject.Msg("Error: anonymous statistic can't be of unique type.")
            Return Nothing
        End If

        retval.stat_name = "anonymous"
        If Not is_string_stat Then
            'encode JSON object as field
            string_stat_value = JsonConvert.SerializeObject(unproccessed_val)

        Else
            string_stat_value = unproccessed_val
        End If

        retval.stat_value = HttpUtility.UrlEncode(string_stat_value)

        Return retval
    End Function

    Private Function validateStatName(ByVal val As String)
        Dim fiChar As Char

        If Trim(val) = "" Then Return False

        For Each fiChar In val.ToCharArray
            If Not Me.valid_nodename_chars.Has(fiChar) Then
                Return False
            End If

        Next

        Return True
    End Function
    Private Function getExistingStat(ByVal for_class_friendly_name As String, ByVal stat_varname As String, ByVal unique_value As Boolean) As XElement

        Dim q As System.Collections.Generic.IEnumerable(Of XElement)

        Try
            If IsNothing(Me.getStack().mapping_point_xml_doc) Then
                Throw New Exception("Unable to retrieve XML document.")
            End If

            If Not Me.validateStatName(stat_varname) Then
                'prevent LINQ errors
                Return Nothing
            End If


            If unique_value Then
                'SELECT UNIQUE
                q = From n In Me.getStack().mapping_point_xml_doc.Descendants(for_class_friendly_name).Descendants()
        Where n.@unique = 1 And n.Name = stat_varname

            Else
                'SELECT GENERAL
                q = From n In Me.getStack().mapping_point_xml_doc.Descendants(for_class_friendly_name).Descendants()
                       Where n.Name = stat_varname
            End If


            If q.Count > 0 Then
                'select first element
                Return q(0)
            Else
                Return Nothing
            End If


        Catch ex As Exception
            Msg("getExistingValue Error: " & ex.Message)
            Me.getStack().have_stats = False
            Return Nothing
        End Try

    End Function

    Private Function addStatistic(ByVal for_class_friendly_name As String, ByVal stat_data As Preprocessing_result, ByVal is_unique As Boolean) As Boolean

        Dim xstatistics As XDocument
        Dim xml_template As String
        Dim stat_element As XElement
        Dim all_xml As String
        Dim unique_exists As Boolean
        Dim current_date As String
        Dim current_time As String
        Dim current_ip As String

        stat_data.stat_type_unique = is_unique
        current_ip = Me.getIOHandler().RemoteIP
        current_date = Now.ToString(JStats.format_date_str)
        current_time = Now.ToString(JStats.format_time_str)

        xstatistics = Me.getXMLDoc()

        If IsNothing(xstatistics) Then
            xstatistics = xstatistics
            Return False
        End If

        If Not Me.validateStatName(stat_data.stat_name) Then Return False

        'create node or update  
        stat_element = Nothing

        If stat_data.stat_type_unique Then
            'get existing node
            stat_data.state_unique_revision_changed = False
            stat_element = Me.getExistingStat(for_class_friendly_name, stat_data.stat_name, True)
            unique_exists = Not IsNothing(stat_element)
        End If

        If (Not stat_data.stat_type_unique) Or (stat_data.stat_type_unique And Not unique_exists) Then
            'create a new node
            xml_template = "<" & stat_data.stat_name & ">" & stat_data.stat_value & "</" & stat_data.stat_name & ">"
            stat_element = XElement.Parse(xml_template)
        End If

        If stat_data.stat_type_unique Then
            If Not unique_exists Then
                stat_element.@unique = 1
            Else
                stat_data.state_unique_revision_changed = Not (stat_element.Value = stat_data.stat_value)

                If stat_data.state_unique_revision_changed Then
                    'record revision
                    Dim revision_count As Long
                    Dim revision_str As String
                    revision_count = 1

                    If Not IsNothing(stat_element.@revision) Then
                        revision_str = stat_element.@revision.ToString

                        If IsNumeric(revision_str) Then
                            revision_count = CLng(revision_str)
                            revision_count += 1
                        End If
                    End If

                    stat_element.@previousvalue = stat_element.Value
                    stat_element.@revision = revision_count.ToString
                    stat_element.@lastrevisiondate = current_date
                    stat_element.@lastrevisiontime = current_time
                    stat_element.@lastrevisionip = current_ip
                Else
                    'nothing happened
                    Return True
                End If
            End If

        End If

        stat_element.@dropip = current_ip

        If (Not stat_data.stat_type_unique) Or (stat_data.stat_type_unique And Not unique_exists) Then
            stat_element.@dropdate = current_date
            stat_element.@droptime = current_time
        End If

        stat_element.SetValue(stat_data.stat_value)

ADD_STAT_NODE_UNDER_CLASS:
        Dim class_node As XElement
        Dim mapping_point As XElement
        Dim jclass_nodes As System.Collections.Generic.IEnumerable(Of XElement)

        mapping_point = xstatistics.Nodes(0)
        jclass_nodes = From n In mapping_point.DescendantsAndSelf(for_class_friendly_name)

        If (Not stat_data.stat_type_unique) Or (stat_data.stat_type_unique And Not unique_exists) Then
            If jclass_nodes.Count = 0 Then
                Dim for_class_xml As String

                'add class
                for_class_xml = Me.getJClassXML(for_class_friendly_name)
                class_node = XElement.Parse(for_class_xml)

                'add class to mapping point
                mapping_point.Add(class_node)
            Else
                'add to existing class
                class_node = jclass_nodes(0)
            End If


            'add stat to class node
            'class_node.Add(stat_element)
            If Not IsNothing(class_node.LastNode) Then
                class_node.LastNode.AddAfterSelf(stat_element)
            Else
                class_node.Add(stat_element)
            End If


        Else
            'unique value change
            stat_element = stat_element
        End If
        'jclass_node.
        'stat_element
        all_xml = xstatistics.ToString

        Me.getStack().have_stats = True

        If (Me.haveStackDocument) And (Not stat_data.stat_type_unique) Or (stat_data.stat_type_unique And Not unique_exists) Or (stat_data.stat_type_unique And stat_data.state_unique_revision_changed) Then
            'invoke stat engine events
            '=========
            stat_data.file = Me.getStack().mapping_point_xml_doc

            Call StatGrid.InStatsContext(Me.getProcessDescriptor().getParent(), for_class_friendly_name, stat_data, stat_data.stat_type_unique, 0)
            Call StatGrid.InWebServiceContext(Me.getProcessDescriptor().getParent(), for_class_friendly_name, stat_data, stat_data.stat_type_unique, 0)
        End If

        Return True

    End Function

    Private Function getJClassXML(ByVal for_class_friendly_name As String)
        Dim xml_header_template As String
        Dim xml_header_template_out As String

        xml_header_template = "<$JCLASS$ type=""class"">"
        xml_header_template &= "</$JCLASS$>"

        xml_header_template_out = Replace(xml_header_template, "$JCLASS$", for_class_friendly_name)

        Return xml_header_template_out
    End Function
    Private Function createStatsFile(ByVal for_class_friendly_name As String) As Boolean
        Dim xml_header_template As String
        Dim xml_header_template_out As String
        Dim mp_name As String

        Try
            'generate stats file if required.
            'get doc with header first

            Msg("Creating new stats doc...")

            mp_name = Replace(JStats.getfilenameByMappingPointUri(Me.getProcessDescriptor().getParent().getURI()), ".xml", "")
            xml_header_template = "<$MP_NAME$ type=""root"" url=""$MP_PATH$"" sessionid=""$SESSION$$"" date=""$DATE$"" time=""$TIME$"" applicationid=""$APPID$"">"
            xml_header_template &= getJClassXML(for_class_friendly_name)
            xml_header_template &= "</$MP_NAME$>"

            'transform
            xml_header_template_out = xml_header_template
            xml_header_template_out = Replace(xml_header_template_out, "$MP_PATH$", Me.getProcessDescriptor().getParent().getURI())
            xml_header_template_out = Replace(xml_header_template_out, "$MP_NAME$", mp_name)
            xml_header_template_out = Replace(xml_header_template_out, "$SESSION$", Me.getProcessDescriptor().getParent().Session.Id)
            xml_header_template_out = Replace(xml_header_template_out, "$DATE$", Now.ToString(JStats.format_date_str))
            xml_header_template_out = Replace(xml_header_template_out, "$TIME$", Now.ToString(JStats.format_time_str))
            xml_header_template_out = Replace(xml_header_template_out, "$APPID$", Me.getProcessDescriptor().getPArent().ApplicationId)

            'create new document
            Me.getStack().mapping_point_xml_doc = XDocument.Parse(xml_header_template_out)
            Me.getStack().have_stats = True
            Return True

        Catch ex As Exception
            Msg("createOrGetStatsFile err: " & ex.Message)
            Me.getStack().have_stats = False
        End Try

        Return False
    End Function

    Private ReadOnly Property StatisticsFileExists() As Boolean
        Get
            Return File.Exists(Me.getStack().chainstats_filepath)
        End Get
    End Property
    Private Function loadChainStatistics(Optional ByVal retry_count As Integer = 0) As Boolean
        Dim retval As Boolean

        If StatisticsFileExists Then
            Dim contents As String
            Try
                contents = File.ReadAllText(Me.getStack().chainstats_filepath)
            Catch ex As Exception
                'see writeStatistics
                If retry_count < 5 Then
                    retry_count += 1
                    Thread.Sleep(20)
                    Return loadChainStatistics(retry_count)
                Else
                    Msg("loadChainStatistics error: cannot read " & Me.getStack().chainstats_filepath)
                    Return False
                End If
            End Try

            Me.getStack().mapping_point_xml_doc = XDocument.Parse(contents)
            retval = True
        Else
            retval = False
        End If

        Return retval
    End Function
    Private ReadOnly Property haveStackDocument As Boolean
        Get
            Return Not IsNothing(Me.getStack().mapping_point_xml_doc)
        End Get
    End Property
    Private Function writeStatistics(Optional ByVal retry_count As Integer = 0) As Boolean

        Try

            If Me.haveStackDocument Then
                File.WriteAllText(Me.getStack().chainstats_filepath, Me.getStack().mapping_point_xml_doc.ToString)
                Return True
            Else
                Err.Raise(200, 0, "Nothing to write")
            End If

        Catch ex As Exception
            Msg("writeStatistics err: " & ex.Message)
            If retry_count < 5 Then
                'retry: .NET or Windows might be locking the file.
                retry_count += 1
                Thread.Sleep(5)
                Return writeStatistics(retry_count)
            Else
                Msg("writeStatistics error: cannot write " & Me.getStack().chainstats_filepath)
                retry_count = retry_count
            End If
        End Try

        Return False
    End Function
    Public Sub setFilePaths()
        'get the mp's xml file
        Dim mapping_point_filename As String
        mapping_point_filename = getfilenameByMappingPointUri(Me.getProcessDescriptor().getParent().getURI)
        Me.getStack().jstat_module_path = mp_global_config.my_session_path
        Me.getStack().chainstats_filepath = Me.getStack().jstat_module_path & "jStats\" & mapping_point_filename

    End Sub

    Public Shared Function getFilenameByMappingPointUri(ByVal val As String)
        Dim mapping_point_sanitized_url As String
        mapping_point_sanitized_url = val
        mapping_point_sanitized_url = Replace(Mid(mapping_point_sanitized_url, 2), "/", "_") 'skip first forward slash in nameing

        If IsNothing(mapping_point_sanitized_url) Then
            'for / mapping points.
            mapping_point_sanitized_url = "index"
        End If

        Return mapping_point_sanitized_url & ".xml"

    End Function

    Public ReadOnly Property getXMLDoc As XDocument
        Get
            Return Me.getStack().mapping_point_xml_doc
        End Get
    End Property
End Class

Public Class JStats_Stack
    Public mapping_point_xml_doc As XDocument
    Public jstat_module_path As String
    Public chainstats_filepath As String
    Public have_stats As Boolean
    Public Sub New()
        Me.mapping_point_xml_doc = Nothing
    End Sub
End Class

Public Structure Preprocessing_result
    Public stat_name As String
    Public stat_value As String
    Public parsed_successfully As Boolean
    Public stat_type_unique As Boolean
    Public state_unique_revision_changed As Boolean
    Public file As XDocument
End Structure