Option Compare Text
Option Explicit On

Imports System.Net.Sockets
Imports System.IO
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports TriniDATServerTypes 
Imports Newtonsoft.Json
Imports System.Xml

'generates the server index page.
Public Class JDefaultServerPage
    Inherits JTriniDATWebService

    Private my_configuration_info As MappingPointBootstrapData
    'used to generate a new app index cache with default settings.
    Public Const APP_XMLFILENAME As String = "ApplicationIndex.xml"
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
        Return getIOHandler().Configure(http_events)

    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean

        Dim get_function As TriniDAT_ServerGETFunction

        get_function = New TriniDAT_ServerGETFunction(AddressOf ShowAppIndex)
        get_function.FunctionURL = Me.makeRelative("/")

        http_function_table.AddGET(get_function, False)

        Return True
    End Function

    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        'catch message
        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then

            'store mapping point info
            Me.my_configuration_info = CType(obj.Attachment, MappingPointBootstrapData)

        End If

        Return False
    End Function
    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)

        Dim wwwroot_base As String
        Dim filepath As String
        Dim fullpath As String

        wwwroot_base = Me.my_configuration_info.static_path & "wwwroot\"

        filepath = Mid(Replace(HTTP_URI_Path, "/", "\"), 2)
        fullpath = wwwroot_base & filepath

        Dim msg As JSONObject
        msg = New JSONObject
        Msg.Directive = "UPLOAD"
        Msg.ObjectType = "JFileserverRequest"
        msg.Attachment = fullpath
        Me.getMailProvider().send(Msg, Nothing, "JFileserver")


    End Sub
    Public Sub ShowAppIndex(ByVal processed_parameter_list As TriniDATServerTypes.TriniDATGenericParameterCollection, ByVal AllParameters As System.Collections.Specialized.StringDictionary, ByVal Headers As System.Collections.Specialized.StringDictionary)


        Dim app_node As XElement
        Dim mapping_point_item_template As String = "{" & Chr(34) & "children" & Chr(34) & ": [ ], " & Chr(34) & "data" & Chr(34) & ": { " & Chr(34) & "runtimecount" & Chr(34) & ": " & Chr(34) & "$RUNTIMECOUNT" & Chr(34) & ", " & Chr(34) & "name" & Chr(34) & ": " & Chr(34) & "$MAPPING_POINT_NAME" & Chr(34) & "," & Chr(34) & "url" & Chr(34) & " : " & Chr(34) & "$MAPPING_POINT_URL" & Chr(34) & "," & Chr(34) & "image" & Chr(34) & ": " & Chr(34) & "http://userserve-ak.last.fm/serve/300x300/11393921.jpg" & Chr(34) & ", " & Chr(34) & "$area" & Chr(34) & ": $areasize }, " & Chr(34) & "id" & Chr(34) & ": " & Chr(34) & "$MAPPING_POINT_URL" & Chr(34) & ", " & Chr(34) & "name" & Chr(34) & ": " & Chr(34) & "$MAPPING_POINT_NAME" & Chr(34) & " }"
        Dim app_template As String = "{ " & Chr(34) & "children" & Chr(34) & ": [ $MP_CHILDREN ] , " & Chr(34) & "id" & Chr(34) & ": " & Chr(34) & "$APPID" & Chr(34) & ", " & Chr(34) & "name" & Chr(34) & ": " & Chr(34) & "$APPNAME" & Chr(34) & ", " & Chr(34) & "data" & Chr(34) & ": { }    }"
        Dim root_template As String = "{ " & Chr(34) & "children" & Chr(34) & ": [ $APPS ], " & Chr(34) & "data" & Chr(34) & ": {}, " & Chr(34) & "id" & Chr(34) & ": " & Chr(34) & "root" & Chr(34) & ",  " & Chr(34) & "name" & Chr(34) & ": " & Chr(34) & "applications" & Chr(34) & " }"
        Dim current_dataset_file As String
        Dim dataset_filebuffer As String
        Dim fs As FileStream

        'INITIALIZE DATASET (FILE).
        current_dataset_file = Me.my_configuration_info.static_path & "wwwroot\current_dataset.js"

        Try

            If File.Exists(current_dataset_file) Then
                'CLEAR PERVIOUS DATASET.
                File.Delete(current_dataset_file)
            End If

        Catch ex As Exception

        End Try

        Dim dataset_json As String
        Dim app_item As BosswaveApplication
        Dim all_xapps As List(Of XmlDocument)
        Dim xmldoc As XmlDocument
        Dim dep_old_node_name As String
        Dim dep_new_node_name As String

        dep_old_node_name = "<dependency path=" & Chr(34)
        dep_new_node_name = "<dependency name=" & Chr(34)

        dataset_json = ""
        all_xapps = New List(Of XmlDocument)


        For Each app_item In GlobalObject.ApplicationCache

            If Not app_item.Disabled And Not app_item.IsInterface And app_item.haveMappingPoints Then


                app_node = app_item.ApplicationNode
                app_node.@id = app_item.Id.ToString

                'replace items
                For Each mp_desc In app_item.ApplicationMappingPoints

                    If mp_desc.haveMappingPointInstance And mp_desc.haveNode Then

                        Dim xml_jclass_name As String
                        Dim new_mapping_point As XElement

                        new_mapping_point = XElement.Parse(mp_desc.Node.ToString)

                        Dim xmp_dependencies = From deps In new_mapping_point.Descendants("dependencies")

                        If xmp_dependencies.Count > 0 Then
                            Dim xmp_dependecy = From xdep In xmp_dependencies.Descendants("dependency")

                            If xmp_dependecy.Count > 0 Then
                                Dim dep_index As Integer
                                dep_index = 0

                                For Each xmp_dependency_node As XElement In xmp_dependecy
                                    Debug.Print(xmp_dependency_node.ToString)

                                    If Not IsNothing(xmp_dependency_node.@path) Then
                                        dep_index = dep_index + 1
                                        ''MsgBox(xmp_dependency.ToString)

                                        xml_jclass_name = xmp_dependency_node.@path.ToString

                                        If InStr(xml_jclass_name, "=") = 0 Then
                                            Dim fileinfo As FileInfo

                                            If File.Exists(xml_jclass_name) Then
                                                fileinfo = New FileInfo(xml_jclass_name)
                                                'replace file path with just a filename.
                                                xml_jclass_name = fileinfo.Name
                                                'Debug.Print(app_item.Id & " " & mp_desc.URL.ToString & " dep: " & dep_index.ToString)
                                            End If
                                        End If

                                        'set metadata for index javascript to parse.
                                        xmp_dependency_node.@name = "Module " & dep_index.ToString & " : " & xml_jclass_name

                                        '  MsgBox(xmp.ToString & " " & xmp_dependency.@name)
                                        Debug.Print(xmp_dependency_node.@name.ToString)
                                    Else
                                        Debug.Print(xmp_dependency_node.ToString)

                                    End If
                                Next
                            End If

                            Dim q_old_mpnode = From xoldmp_definition In app_node.Descendants("mps")(0).Descendants("mp") Where xoldmp_definition.@url.ToString = new_mapping_point.@url.ToString
                                          Select xoldmp_definition

                            If q_old_mpnode.Count > 0 Then
                                q_old_mpnode(0).ReplaceWith(new_mapping_point)
                            End If
                        End If
                    End If

                Next


                xmldoc = New XmlDocument()
                xmldoc.LoadXml(app_node.ToString)

                'add xdoc to parse list.
                all_xapps.Add(xmldoc)

            End If

        Next

        'convert xdoc array to JSON
        Dim serialized_xml_doc As String

        serialized_xml_doc = JsonConvert.SerializeObject(all_xapps)

        'URL encode JSON
        dataset_json = HttpUtility.UrlEncode(serialized_xml_doc, System.Text.Encoding.GetEncoding("utf-8"))
        dataset_json = Replace(dataset_json, "+", "%20")

        'WRITE DATASET FILE.
        Dim dataset_bytes() As Byte

        dataset_filebuffer = "/*" & vbNewLine
        dataset_filebuffer &= "     TriniDAT Data Server " & GlobalObject.getVersionString() & " ." & vbNewLine
        dataset_filebuffer &= "     Copyright (c) 2013 GertJan de Leeuw | De Leeuw ICT | http://www.deleeuwict.nl" & vbNewLine
        dataset_filebuffer &= "*/" & vbNewLine & vbNewLine & vbNewLine
        dataset_filebuffer &= "var dataset_json = ''; " & vbNewLine
        dataset_filebuffer &= "var master_list = unescape(" & Chr(34) & dataset_json & Chr(34) & ");" & vbNewLine & vbNewLine
        dataset_bytes = System.Text.Encoding.ASCII.GetBytes(dataset_filebuffer)


        Try
            fs = New FileStream(current_dataset_file, FileMode.Create)

            fs.Write(dataset_bytes, 0, dataset_bytes.Length)

        Catch ex As Exception

            If Not IsNothing(fs) Then
                fs.Dispose()
            End If

        End Try


        'SERVE INDEX FILE THAT WILL INCLUDE DATASET file.
        Dim msg As JSONObject
        msg = New JSONObject
        msg.Directive = "UPLOAD"
        msg.ObjectType = "JFileserverRequest"
        msg.Attachment = Me.my_configuration_info.static_path & "wwwroot\index.html"
        Me.getMailProvider().send(msg, Nothing, "JFileserver")

    End Sub

End Class
