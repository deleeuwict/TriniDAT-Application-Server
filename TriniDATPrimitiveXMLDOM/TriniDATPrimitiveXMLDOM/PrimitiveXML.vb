Option Explicit On
Option Compare Text
Imports System
Imports System.Runtime.CompilerServices
Imports System.Xml
Imports System.Collections
Imports System.Collections.Specialized
Imports System.Web
Imports TriniDATDictionaries
Imports TriniDATHTTPTypes

<Assembly: SuppressIldasmAttribute()> 


Public Class PrimitiveXML

    'Renders a skeleton xml doc from html code 
    ' that at least got the node name/positions right.
    Private currentTriniDATID As Integer

    Private primitive_xml_document As String
    Private all_elements As List(Of TriniDATHTTPTypes.TriniDATNode)
    Private all_elements_last_index As Integer

    Private links As TriniDATHTTPTypes.TriniDATLinkCollection

    Private error_elements As List(Of TriniDATHTTPTypes.TriniDATNode)
    Public Shared GUI_Enabled As Boolean

    Public Sub setErrors(ByVal val As List(Of TriniDATHTTPTypes.TriniDATNode))
        Me.error_elements = val
    End Sub

    Public Property GUIMode As Boolean
        Get
            Return PrimitiveXML.GUI_Enabled
        End Get
        Set(ByVal value As Boolean)
            PrimitiveXML.GUI_Enabled = value
        End Set
    End Property
    Public Function getNodeBlockWith(ByVal ni As TriniDATHTTPTypes.TriniDATNode, Optional ByVal include_endtag As Boolean = True) As Integer
        'include_endtag: if true mark node including the full end-tag representation.

        Dim StartPos As Long
        Dim endpos As Long

        StartPos = ni.html_src_startpos
        endpos = ni.html_src_endpos

        If include_endtag Then
            endpos += Len(ni.getAsEndTag())
        End If


        If endpos < StartPos Then
            StartPos = StartPos
            Return 0
        End If

        Return endpos - StartPos

    End Function

    Public ReadOnly Property getAllLinks(Optional ByVal link_type As TriniDATLinkType = Nothing) As TriniDATHTTPTypes.TriniDATLinkCollection
        Get
            Dim retval As TriniDATHTTPTypes.TriniDATLinkCollection

            retval = New TriniDATHTTPTypes.TriniDATLinkCollection


            For Each lnk In Me.links

                If Trim(lnk.getURL()) <> "" Then
                    If (link_type = 0) Or (lnk.LinkType = link_type) Then
                        retval.Add(lnk)
                    End If
                End If
            Next

            Return retval

        End Get
    End Property

    Public Function getNodeBlockWith(ByVal TriniDATid As Integer, Optional ByVal include_endtag As Boolean = True) As Integer

        Dim ni As TriniDATHTTPTypes.TriniDATNode

        ni = Me.getNodeById(TriniDATid)

        Return Me.getNodeBlockWith(ni, include_endtag)

    End Function

    Public Function getNextNode(ByVal TriniDATid As Integer) As TriniDATHTTPTypes.TriniDATNode
        Dim nextid As Integer

        nextid = TriniDATid + 1

        Return Me.getNodeById(nextid)

    End Function

    Public ReadOnly Property getNewTriniDATId() As Integer
        Get
            Me.currentTriniDATID = Me.currentTriniDATID + 1
            Return currentTriniDATID
        End Get
    End Property

    Public ReadOnly Property getErrors() As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Return Me.error_elements
        End Get
    End Property

    Public ReadOnly Property getXMLNodes() As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Return Me.all_elements
        End Get
    End Property

    Public Sub setXMLNodesInfo(ByVal val As List(Of TriniDATHTTPTypes.TriniDATNode))
        Me.all_elements = val

        'INIT second hang tag lists.
        Me.links = New TriniDATHTTPTypes.TriniDATLinkCollection
    End Sub

    Public Sub addXMLNode(ByVal val As TriniDATHTTPTypes.TriniDATNode)
        Me.all_elements.Add(val)
        Me.setLastInsertedNodeIndex(Me.all_elements.Count - 1)

        Msg("output TriniDATid " & val.getId().ToString)
    End Sub

    Public Sub setLastInsertedNodeIndex(ByVal x As Integer)
        Me.all_elements_last_index = x
    End Sub
    Public Function getLastInsertedNodeIndex() As Integer
        Return Me.all_elements_last_index
    End Function

    Public Function setNewXMLOpenNode(ByVal tagname As String, ByVal html_src_startpos As Integer, ByVal XML_src_startpos As Integer, ByVal chars_width As Integer) As TriniDATHTTPTypes.TriniDATNode
        'add tag
        Dim taginfo As TriniDATHTTPTypes.TriniDATNode

        taginfo = New TriniDATHTTPTypes.TriniDATNode(chars_width)
        taginfo.Issingleton = Parsing.isHTLMStandardSingleton(tagname) 'img, input, etc do not close
        taginfo.setTagName(tagname)
        taginfo.setID(Me.getNewTriniDATId())
        taginfo.html_src_startpos = html_src_startpos 'char_pos
        taginfo.xml_str_startpos = XML_src_startpos '  Len(output_xml)


        If taginfo.Issingleton = True Then
            'manually close tag
            taginfo.foundEndTag = True
        Else
            'expect a closing node
            taginfo.foundEndTag = False
        End If

        Me.addXMLNode(taginfo)
        Msg("==============")
        Return taginfo

        ' output_xml = output_xml & taginfo.renderXHTMLTag(taginfo.foundEndTag) & vbCrLf

    End Function

    Public Sub setXMLNodeClosed(ByVal X As Integer, ByVal closetag_html_src_pos As Integer, ByVal closetag_xml_src_pos As Integer)
        Me.all_elements(X).foundEndTag = True
        Me.all_elements(X).html_src_endpos = closetag_html_src_pos
        Me.all_elements(X).xml_str_endpos = closetag_xml_src_pos

        If Me.all_elements(X).getTagName() = "a" Then
            Me.links.Add(TriniDATHTTPTypes.TriniDATLinkElement.createFrom(Me.all_elements(X)))
        End If
    End Sub


    Public Function getLastUnclosedNodeFromTheBack(ByVal tagname As String) As Integer

        Dim z As Integer
        Dim elindex As Integer
        Dim totcount As Integer
        totcount = all_elements.Count - 1

        For z = 0 To totcount
            'look backwards for first unclosed node
            elindex = totcount - z
            If all_elements.Item(elindex).foundEndTag = False And all_elements.Item(elindex).getTagName() = tagname Then
                Return elindex
                Exit Function
            End If
        Next

        Return -1

    End Function

    Public ReadOnly Property getNodeByIndex(ByVal index As Integer) As TriniDATHTTPTypes.TriniDATNode
        Get

            If index >= Me.all_elements.Count Then
                Return Nothing
            End If

            Return Me.all_elements(index)

        End Get
    End Property

    Public ReadOnly Property getNodeById(ByVal TriniDATid As Integer) As TriniDATHTTPTypes.TriniDATNode
        Get

            Dim node As TriniDATHTTPTypes.TriniDATNode

            For Each node In Me.getNodes()

                If node.getId() = TriniDATid Then
                    Return node
                End If

            Next

            Return Nothing

        End Get
    End Property

    Public ReadOnly Property extractInnerText(ByVal TriniDATid As Integer, ByVal html As String) As String
        Get
            Dim ni As TriniDATHTTPTypes.TriniDATNode

            ni = Me.getNodeById(TriniDATid)

            If Not IsNothing(ni) Then
                Return Me.extractInnerText(ni, html)
            End If

            Return ""

        End Get
    End Property

    Public ReadOnly Property haveNodes() As Boolean
        Get
            Return Not IsNothing(Me.all_elements)
        End Get
    End Property

    Public ReadOnly Property getAllScript() As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Return Me.getallByTagName("script")
        End Get
    End Property
    Public ReadOnly Property getElementsByTagName(ByVal val As String) As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Return Me.getallByTagName(LCase(val))
        End Get
    End Property
    Public ReadOnly Property getForms(ByVal form_src_url As String) As TriniDATHTTPTypes.TriniDATFormCollection
        Get

            Dim forms As List(Of TriniDATHTTPTypes.TriniDATGenericNodeFamily)
            Dim retval As TriniDATHTTPTypes.TriniDATFormCollection
            Dim form_entry As TriniDATForm

            forms = Me.getallParential("form", "input")

            If IsNothing(forms) Then
                Return Nothing
            End If

            Dim x As Integer

            'copy
            retval = New TriniDATHTTPTypes.TriniDATFormCollection

            For x = 0 To forms.Count - 1
                form_entry = TriniDATForm.createFrom(forms(x), form_src_url)
                retval.Add(form_entry)
            Next x

            Return retval
        End Get
    End Property


    Public ReadOnly Property getAllTextualNodes(ByVal htmlsrc As String) As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Dim retval As List(Of TriniDATHTTPTypes.TriniDATNode)
            Dim el As TriniDATHTTPTypes.TriniDATNode
            Dim eltxt As String
            Dim non_text_tags As TriniDATWordDictionary
            Dim parent_node_html As String
            Dim last_parent As TriniDATHTTPTypes.TriniDATNode

            parent_node_html = ""
            last_parent = Nothing

            non_text_tags = New TriniDATWordDictionary(".", New List(Of String)({"script", "style"}))

            retval = New List(Of TriniDATHTTPTypes.TriniDATNode)

            For x = 0 To Me.all_elements.Count - 1

                el = Me.all_elements(x)

                If Not el.Issingleton Then 'ignore <BR> etc
                    If Not non_text_tags.Has(el.getTagName) Then
                        eltxt = Me.extractInnerText(el, htmlsrc)

                        'validate as true if there is no html present

                        If eltxt <> "" Then
                            If InStr(eltxt, "<") = 0 Then
                                retval.Add(el)

                                'todo: PARSE <SPAN> and <DIV> as new HTML documents
                                'and extract text from inner HTML.

                                '    If Not IsNothing(last_parent) Then

                                '        'todo: remove all HTML of this node from $parent_node_html
                                '        Dim org_parent_node_html_len As Integer
                                '        Dim new_parent_node_html_len As Integer
                                '        Dim cur_el_html As String

                                '        cur_el_html = Mid(htmlsrc, el.html_src_startpos + 1, (el.html_src_endpos + 1) - (el.html_src_startpos + 1))
                                '        cur_el_html &= el.getAsEndTag

                                '        org_parent_node_html_len = Len(parent_node_html)

                                '        parent_node_html = Replace(parent_node_html, cur_el_html, "")

                                '        new_parent_node_html_len = Len(parent_node_html)

                                '        If Len(parent_node_html) > 0 And (org_parent_node_html_len <> new_parent_node_html_len) And InStr(parent_node_html, "<") = 0 And InStr(parent_node_html, ">") = 0 Then
                                '            'success
                                '            'todo: scan for > and don't add?
                                '            'todo: return plain text in parent_node_html, not element collection.
                                '            retval.Add(last_parent)
                                '            parent_node_html = ""
                                '            last_parent = Nothing
                                '        End If


                                '    End If
                                '    'if parent_node_html_len <> oldparent_node_html
                                'Else
                                '    '1-12-2013:
                                '    'save parent node inner HTML, then after pure child is hit, extract the child element's HTML from the parent innerText. If any char remains, then append as text.
                                '    'e.g.
                                '    '<div class="s-element-content s-text linkify"><b>Update 9:40 a.m. ET: </b>Steve Kronenberg, a nearby resident, told 1010WINS radio the derailment sounded like a plane engine: “I was at my desk at my computer, and I thought a plane was coming in. I jumped away. Then after the noise stopped, I looked out the window and saw the train derailment, and I called 911. They put me on with the fire department. I told them what had happened, where it was, so on and so forth. ... I told them there [weren't] any flames. There was a little bit of smoke coming out from one of the cars, and they got here pretty quickly.”<br></div>
                                '    parent_node_html = eltxt
                                '    last_parent = el
                                'end if
                            End If
                        End If
                    End If
                End If
            Next

            If retval.Count = 0 Then
                Return Nothing
            Else
                Return retval
            End If

        End Get

    End Property


    Public ReadOnly Property getallParential(ByVal parent_tag As String, ByVal child_tag As String) As List(Of TriniDATHTTPTypes.TriniDATGenericNodeFamily)
        Get
            Dim retval As List(Of TriniDATHTTPTypes.TriniDATGenericNodeFamily)
            Dim parentnodes As List(Of TriniDATHTTPTypes.TriniDATNode)
            Dim all_child_nodes As List(Of TriniDATHTTPTypes.TriniDATNode)
            Dim parentnode As TriniDATHTTPTypes.TriniDATNode
            Dim x As Integer

            'only set when there are actual return values.
            retval = Nothing
            parentnodes = Me.getallByTagName(parent_tag)

            If IsNothing(parentnodes) Then Return Nothing

            all_child_nodes = Me.getallByTagName(child_tag)

            For x = 0 To parentnodes.Count - 1

                parentnode = parentnodes(x)

                If Not parentnode.Issingleton And parentnode.foundEndTag Then
                    'check child nodes whose start tag is before the parent end tag

                    Dim y As Integer
                    Dim matching_child_nodes As List(Of TriniDATHTTPTypes.TriniDATNode)

                    matching_child_nodes = Nothing

                    For y = x To all_child_nodes.Count - 1
                        If all_child_nodes(y).html_src_startpos < parentnode.html_src_endpos Then

                            If IsNothing(matching_child_nodes) Then
                                matching_child_nodes = New List(Of TriniDATHTTPTypes.TriniDATNode)
                            End If

                            matching_child_nodes.Add(all_child_nodes(y))
                        Else
                            'done
                            Exit For
                        End If

                    Next


                    If Not IsNothing(matching_child_nodes) Then
                        If IsNothing(retval) Then
                            retval = New List(Of TriniDATHTTPTypes.TriniDATGenericNodeFamily)
                        End If


                        Dim newcol As TriniDATHTTPTypes.TriniDATGenericNodeFamily
                        newcol = New TriniDATHTTPTypes.TriniDATGenericNodeFamily(parentnode, matching_child_nodes)

                        'add to end result
                        retval.Add(newcol)
                    End If

                End If
            Next

            Return retval
        End Get
    End Property

    Private ReadOnly Property getallByTagName(ByVal val As String) As List(Of TriniDATHTTPTypes.TriniDATNode)
        Get
            Dim retval As List(Of TriniDATHTTPTypes.TriniDATNode)

            retval = Nothing
            val = val.ToLower

            If Me.haveNodes() Then

                retval = New List(Of TriniDATHTTPTypes.TriniDATNode)

                Dim x As Integer
                For x = 0 To Me.all_elements.Count - 1

                    If Me.all_elements(x).getTagName().ToLower = val Then
                        retval.Add(Me.all_elements(x))
                    End If

                Next

            End If

            Return retval

        End Get
    End Property

    Public ReadOnly Property extractInnerText(ByVal ni As TriniDATHTTPTypes.TriniDATNode, ByVal html As String) As String
        Get
            If ni.Issingleton Then
                Return ""
            End If

            Dim buffer As String
            Dim propkey As String
            Dim startpos As Integer
            Dim total_strip_len As Integer

            startpos = ni.html_src_startpos + 1

            buffer = Mid(html, startpos, Me.getNodeBlockWith(ni.getId(), False)) 'false option eliminates end tag inclusions.

            total_strip_len = 2 '<>
            total_strip_len += Len(ni.getTagName())


            'strip all attributes
            If ni.hasAttributeList() Then

                For Each propkey In ni.getAttributes().Keys
                    total_strip_len += 1 'seperating space
                    total_strip_len += Len(propkey)
                    total_strip_len += 1 'attrib =
                    total_strip_len += Len(ni.getAttribute(propkey))


                    If Not IsNothing(ni.getAttributeContainerchar) Then
                        total_strip_len += 2 'attrib open/close
                    End If

                Next

                'go lower in buffer than original length and strip after end tag detection
                buffer = Mid(buffer, total_strip_len - 1)
                startpos = InStr(buffer, ">")
                If startpos > 0 Then
                    buffer = Mid(buffer, startpos + 1)
                End If

                buffer = Trim(buffer)


            Else
                'no attributes.
                'remove start tag
                buffer = Replace(buffer, "<" & ni.getTagName() & ">", "")

            End If

            'finishing touch
            buffer = HttpUtility.HtmlDecode(buffer)

            'todo: if <SPAN> or <DIV> 
            'loop all chars in innerHTML and see if the char position is owned by a element by checking html_src_start/end pos.

            Return buffer
        End Get

    End Property

    Public ReadOnly Property nodeHasInnerHTML(ByVal ni As TriniDATHTTPTypes.TriniDATNode, ByVal html As String) As String
        Get

            Dim contains_html As Boolean
            Dim inner_html As String
            inner_html = Me.extractInnerHTML(ni, html)

            contains_html = (InStr(inner_html, "<") > 0)

            If Not contains_html Then
                Return Nothing
            Else
                Return inner_html
            End If

        End Get
    End Property


    Public ReadOnly Property extractInnerHTML(ByVal ni As TriniDATHTTPTypes.TriniDATNode, ByVal html As String) As String
        Get
            If ni.Issingleton Then
                Return ""
            End If

            Dim buffer As String
            Dim propkey As String
            Dim startpos As Integer
            Dim total_strip_len As Integer

            startpos = ni.html_src_startpos + 1

            buffer = Mid(html, startpos, Me.getNodeBlockWith(ni.getId(), False)) 'false option eliminates end tag inclusions.

            total_strip_len = 2 '<>
            total_strip_len += Len(ni.getTagName())


            'strip all attributes
            If ni.hasAttributeList() Then

                For Each propkey In ni.getAttributes().Keys
                    total_strip_len += 1 'seperating space
                    total_strip_len += Len(propkey)
                    total_strip_len += 1 'attrib =
                    total_strip_len += Len(ni.getAttribute(propkey))


                    If Not IsNothing(ni.getAttributeContainerchar) Then
                        total_strip_len += 2 'attrib open/close
                    End If

                Next

                'go lower in buffer than original length and strip after end tag detection
                buffer = Mid(buffer, total_strip_len - 1)
                startpos = InStr(buffer, ">")
                If startpos > 0 Then
                    buffer = Mid(buffer, startpos + 1)
                End If

                buffer = Trim(buffer)


            Else
                'no attributes.
                'remove start tag
                buffer = Replace(buffer, "<" & ni.getTagName() & ">", "")

            End If

            'finishing touch
            buffer = HttpUtility.HtmlDecode(buffer)

            Return buffer
        End Get

    End Property
    Public Function getNodes() As List(Of TriniDATHTTPTypes.TriniDATNode)
        Return Me.all_elements
    End Function

    '    Public Shared Function CreateFromHTMLSource(ByVal html As String) As PrimitiveXML
    '        'Constructs primitive XML based on HTML code
    '        Dim retval As PrimitiveXML
    '        Dim html_splitted() As String
    '        Dim html_tag As String
    '        Dim ident_level As Integer
    '        Dim nodename As String
    '        Dim char_pos As Integer
    '        Dim xmldoc_str As String
    '        Dim currentTriniDATID As Integer
    '        Dim last_TriniDATid As Integer
    '        Dim all_elements As List(Of TriniDATHTTPTypes.TriniDATNode)
    '        Dim no_node_until As String

    '        'keep track of processed nodes
    '        all_elements = New List(Of TriniDATHTTPTypes.TriniDATNode)


    '        'TODO:  
    '        '1. grab comment/script blocks -> replace with AAAA..   --> script blocks should be threated as different nodes
    '        '2. Get unending tagnames from the next node start position INSTRREV(">")  --> user sensitive text MUST be html encoded so this will work. there is always a next tag until </html>
    '        '3. once start-end is known, complete split tag by KNOWN properties/events e,g, onclick, onkeypress etc. (TODO: xml definition file)
    '        '4.

    '        Msg(html)

    '        html_splitted = html.Split("<")

    '        '1. CREATE PRIMITIVE XML DOCUMENT
    '        xmldoc_str = ""
    '        ident_level = 0
    '        currentTriniDATID = 0
    '        char_pos = 0
    '        no_node_until = ""

    '        For Each html_tag In html_splitted

    '            If Len(html_tag) > 0 Then

    '                char_pos = char_pos + 1
    '                'NODE TERMINATOR: space or >
    '                nodename = Parsing.getTagFromChunk(html_tag).ToLower()

    '                'filter out comments and invalid names
    '                If nodename <> "" Then


    '                    'If Left(nodename, 2) = "!-" Then
    '                    '    '<!-- -->
    '                    '    no_node_until = "-->"
    '                    'End If


    '                    'If no_node_until <> "" Then
    '                    '    'check for terminator
    '                    '    Dim terminator_pos As Integer
    '                    '    terminator_pos = InStr(html_tag, no_node_until)

    '                    '    If terminator_pos = 0 Then
    '                    '        char_pos = char_pos + Len(html_tag)
    '                    '        GoTo PROCESS_NEXT
    '                    '    Else
    '                    '        '
    '                    '    End If

    '                    'End If


    '                    If Left(nodename, 1) <> "!" And InStr(nodename, "--") = 0 And InStr(nodename, "html") = 0 Then
    '                        If Left(nodename, 1) = "/" Then
    '                            Dim xml_str_pos As Integer
    '                            xml_str_pos = Len(xmldoc_str) 'DO NOT MOVE...
    '                            'PROCESS END TAG 
    '                            nodename = Mid(nodename, 2)
    '                            xmldoc_str = xmldoc_str & "</" & nodename & ">"
    '                            ident_level = ident_level - 1

    '                            'remove the closing node log entry
    '                            Dim z As Integer
    '                            Dim elindex As Integer
    '                            Dim totcount As Integer
    '                            totcount = all_elements.Count - 1

    '                            For z = 0 To totcount
    '                                'look backwards for first unclosed node
    '                                elindex = totcount - z
    '                                If all_elements.Item(elindex).foundEndTag = False And all_elements.Item(elindex).tagName = nodename Then
    '                                    all_elements(elindex).foundEndTag = True
    '                                    all_elements(elindex).html_src_endpos = char_pos - 1
    '                                    all_elements(elindex).xml_str_endpos = xml_str_pos
    '                                    xmldoc_str = xmldoc_str & " <!-- eo " & all_elements(elindex).getID().toString & " -->"
    '                                    Exit For
    '                                End If
    '                            Next

    '                        Else
    '                            'PROCESS BEGIN TAG
    '                            Dim taginfo As New TriniDATHTTPTypes.TriniDATNode(tag_start - 1)
    '                            Dim noclosetag As Boolean

    '                            noclosetag = Parsing.isHTLMStandardSingleton(nodename) 'img, input, etc do not close
    '                            taginfo.tagName = nodename
    '                            taginfo.TriniDATid = currentTriniDATID
    '                            taginfo.html_src_startpos = char_pos - 1
    '                            taginfo.xml_str_startpos = Len(xmldoc_str) + 1 '1 for vbcrlf

    '                            ident_level = ident_level + 1
    '                            xmldoc_str = xmldoc_str & vbCrLf & Parsing.repeatStr(" ", ident_level) & taginfo.renderXHTMLTag(noclosetag)

    '                            If noclosetag = True Then
    '                                'manually close tag
    '                                taginfo.foundEndTag = True
    '                            Else
    '                                'expect a closing node
    '                                taginfo.foundEndTag = False
    '                            End If

    '                            all_elements.Add(taginfo)
    '                            currentTriniDATID = currentTriniDATID + 1

    '                        End If

    '                    End If

    '                    char_pos = char_pos + Len(html_tag)
    '                End If 'if nodename
    '            End If

    'PROCESS_NEXT:
    '        Next 'tag

    '        '   Exit Function

    '        '        html = Replace(html, "<html>", "<html xmlns=""http://www.w3.org/1999/xhtml"">")

    '        'single attribute arguments
    '        '        html = Replace(html, "nowrap", "nowrap=""nowrap""")

    '        '    html = Replace(html, "&nbsp;", " ")
    '        '       html = Replace(html, "&", "&amp;")



    '        'return document
    '        retval = New PrimitiveXML(xmldoc_str)
    '        retval.setXMLNodesInfo(all_elements)

    '        Dim error_count As Integer
    '        Dim error_nodes As List(Of TriniDATHTTPTypes.TriniDATNode)

    '        error_nodes = New List(Of TriniDATHTTPTypes.TriniDATNode)

    '        'validate document
    '        error_count = 0
    '        For Each n In all_elements
    '            If n.foundEndTag = False Then
    '                error_nodes.Add(n)

    '                xmldoc_str = xmldoc_str & vbCrLf & "<ERRORID>tag= " & n.tagName & " TriniDATid=" & n.getID().toString & " srchtmlpos=" & n.html_src_startpos.ToString & "</ERRORID>"
    '                error_count += 1
    '            End If
    '        Next

    '        If error_count > 0 Then
    '            MsgBox(error_count.ToString & " Document errors found")
    '            retval.setErrors(error_nodes)
    '        End If

    '        Return retval

    '    End Function
    Public Shared Function CreateFromHTMLSourceV2(ByVal html As String) As PrimitiveXML
        'Constructs primitive XML based on HTML code
        Dim retval As PrimitiveXML
        Dim tagname As String
        Dim output_xml As String
        Dim attribute_name As String
        Dim attribute_assignment As String
        Dim char_pos As Integer
        Dim current_char As Char
        Dim current_char_ascii As Integer
        Dim current_state As String
        Dim attrib_name_start As Integer
        Dim attrib_name_end As Integer
        Dim tag_start As Integer
        Dim tag_start_offset As Integer
        Dim tag_end As Integer
        Dim attrib_assign_start As Integer
        Dim attrib_assign_end As Integer
        Dim attrib_assign_container As Char 'defaults to  "
        Dim current_attributes As StringDictionary
        Dim b_strict_endtag_matching As Boolean
        '     Dim strict_match_tagname As String
        Dim current_strict_match_info As StrictMatchInfo

        'NOTE@ march 12, 2013: strict match stuff NOT IN USE.
        current_strict_match_info = New StrictMatchInfo


        retval = New PrimitiveXML("")
        retval.setXMLNodesInfo(New List(Of TriniDATHTTPTypes.TriniDATNode))


        Msg(html)

        'INIT
        b_strict_endtag_matching = False
        output_xml = ""
        current_state = ""
        current_char_ascii = 0
        tagname = ""
        attribute_name = ""
        attribute_assignment = ""
        tag_start = 0 'used for string parsing
        tag_end = 0
        tag_start_offset = 0 'used for file position
        current_attributes = Nothing

        'WALK THROUGH DOM AND TRACK WHAT WE SEE
        For char_pos = 1 To html.Length

            current_char = Mid(html, char_pos, 1)
            current_char_ascii = Asc(current_char)
            If (current_state = "tagstart.attributes" And current_char = ">") Or (current_state = "tagstart.attributes.openassignmentstart" And current_char = ">") Then
                'attribute is a single element and closed the tag
                Dim add_node As Boolean


                If attribute_name <> "" Then
                    Msg("end attrib " & attribute_name & " of tag " & tagname)

                    'COLLECT FULL ATTRIBUTE KEY/VAL
                    If Not IsNothing(current_attributes) Then
                        current_attributes.Add(attribute_name, attribute_assignment)
                    Else
                        GUIMsg("Could add attribs but collection is empty")
                    End If

                Else
                    Msg("no further attribs found for this tag.")
                End If

                'add?
                add_node = Not PrimitiveXML.ignoreNode(tagname)


                'jump?

                current_strict_match_info = PrimitiveXML.getstrictMatchingByNodeNameInfo(tagname)

                If current_strict_match_info.Enabled Then
                    Dim newpos As Integer

                    newpos = InStr(char_pos, html, current_strict_match_info.endFormat)
                    If newpos > 0 Then

                        Msg("Add & Jumping to end tag for block " & char_pos.ToString & " - " & newpos & " for tag " & current_strict_match_info.startFormat & " .. " & current_strict_match_info.endFormat)


                        'jump to beginning of end tag
                        'and add end tag
                        char_pos = newpos - 1

                    Else
                        Dim last_piece As String
                        Dim back_piece As String
                        back_piece = Mid(html, char_pos - 200)
                        last_piece = Mid(html, char_pos)
                        GUIMsg("Cannot jump on " & tagname)
                    End If
                End If



                If add_node Then
                    Dim ni As TriniDATHTTPTypes.TriniDATNode

                    ni = retval.setNewXMLOpenNode(tagname, tag_start_offset, Len(output_xml), char_pos - (tag_start - 1))
                    ni.setAttributes(current_attributes, attrib_assign_container)
                    output_xml = output_xml & ni.renderXHTMLTag(ni.foundEndTag) & vbCrLf
                End If

                'reset
                current_state = ""
                current_attributes = Nothing
                tag_start_offset = 0
                tag_start = 0
                tag_end = 0
                attrib_name_start = 0
                attrib_name_end = 0
                attrib_assign_start = 0
                attrib_assign_end = 0
                attribute_name = ""
                attribute_assignment = ""
                tagname = ""
                current_strict_match_info.Enabled = False
                GoTo DONEXT
            ElseIf current_state = "tagstart.attributes" And current_char = "=" Then
                'ASSIGNMENT OPERATOR
                'end of attributename -> assignment start
                attribute_assignment = ""
                attrib_name_end = char_pos - 1

                'look ahead for the assignment value container
                If char_pos + 1 < html.Length Then
                    attrib_assign_container = Mid(html, char_pos + 1, 1)

                    'set special state for quoteless assignments
                    'a-z , 0-9, A-Z, ?@
                    If (Asc(attrib_assign_container) > 96 And Asc(attrib_assign_container) < 123) Or (Asc(attrib_assign_container) > 47 And Asc(attrib_assign_container) < 58) Or (Asc(attrib_assign_container) > 63 And Asc(attrib_assign_container) < 91) Then
                        Msg("assignment with space as terminator found. ")
                        current_state = "tagstart.attributes.openassignmentstart"
                        attrib_assign_container = ""

                        If tagname = "span" And attribute_name = "class" Then
                            tagname = tagname
                        End If

                    Else

                        Msg("Value assignment container char: " & attrib_assign_container)
                        current_state = "tagstart.attributes.assignmentstart"

                    End If
                End If

                '     Msg("attrib: " & attribute_name & " assignment start")
                GoTo DONEXT
            End If

            If current_state = "tagstart.attributes" And (current_char_ascii > 64 And current_char_ascii < 91) Or (current_char_ascii > 96 And current_char_ascii < 123) Then
                'keep collecting attributename a-z A-Z

                If (current_state = "tagstart.attributes") Then
                    attribute_name &= current_char
                    GoTo DONEXT
                End If
            End If

            If current_state = "tagstart.attributes.assignmentstart" And current_char = attrib_assign_container Then
                current_state = "tagstart.attributes.assignmentstart.quotedcontainer"
                attrib_assign_start = char_pos + 1
                '   Msg("attrib: " & attribute_name & " first quote at " & attrib_assign_start)
                GoTo DONEXT
            End If

            If (current_state = "tagstart.attributes.assignmentstart.quotedcontainer" And current_char = attrib_assign_container) Or (current_state = "tagstart.attributes.openassignmentstart" And current_char = " ") Then
                'wipe out attribute state
                attrib_assign_end = char_pos
                '       Msg("attrib: " & attribute_name & " last quote at " & attrib_assign_end.ToString)
                Msg("Attrib (" & tagname & ").(" & attribute_name & ") = " & attribute_assignment)

                If Not IsNothing(current_attributes) Then
                    If Not current_attributes.ContainsKey(attribute_name) Then
                        current_attributes.Add(attribute_name, attribute_assignment)
                    Else
                        'err - double attrib
                        Msg("Double Attrib '" & attribute_name & "' on tag " & tagname & "!")
                        current_attributes(attribute_name) = attribute_assignment
                    End If

                Else
                    GUIMsg("Could add attribs but collection is empty")
                End If

                'reset to tag level.
                current_state = "tagstart.attributes"
                attrib_name_start = 0
                attrib_name_end = 0
                attrib_assign_start = 0
                attrib_assign_end = 0
                attribute_name = ""
                attribute_assignment = ""
                GoTo DONEXT
            End If

            If (current_state = "tagstart.attributes.assignmentstart.quotedcontainer" And current_char <> attrib_assign_container) Or (current_state = "tagstart.attributes.openassignmentstart" And current_char <> " ") Then
                'collect attribute assignment value
                attribute_assignment &= current_char
                GoTo DONEXT
            End If


            If (current_state = "tagstart" And current_char = ">") Or (current_char = ">" And current_state <> "" And current_state <> "tagstart.attributes.assignmentstart.quotedcontainer") Then '        'if no assignment is in progress then we have to terminate
                'tag end

                If Left(tagname, 1) = "/" Then
                    tagname = Right(tagname, Len(tagname) - 1)


                    Msg("end Tag: " & tagname)
                    If tagname = "html" Then
                        tagname = tagname
                    End If


                    If Not Parsing.isHTLMStandardSingleton(tagname) Then
                        'report closed node 
                        Dim index As Integer
                        index = retval.getLastUnclosedNodeFromTheBack(tagname)
                        If index > -1 Then
                            retval.setXMLNodeclosed(index, tag_start_offset, Len(output_xml))
                            output_xml = output_xml & "</" & tagname & "> <!-- " & retval.getNodeByIndex(index).getId().ToString & " -->"
                        End If
                    End If

                Else  'got a <TAG> without attributes

                    Dim add_node As Boolean
                    Dim isCustomSingletonNode As Boolean


                    add_node = False
                    Msg("start Tag: " & tagname)

                    '
                    add_node = Not PrimitiveXML.ignoreNode(tagname)

                    If add_node Then

                        isCustomSingletonNode = (Right(tagname, 1) = "/")
                        If isCustomSingletonNode Then
                            'strip singleton slash and verify if built in
                            tagname = Left(tagname, Len(tagname) - 1)
                            isCustomSingletonNode = Not Parsing.isHTLMStandardSingleton(tagname)

                            If isCustomSingletonNode Then
                                Msg("Singleton node: " & tagname & " -> " & Left(tagname, Len(tagname) - 1))
                            End If
                        End If
                    End If

                    If tagname = "" Then
                        'invalid state
                        Msg("Invalid state > empty tag @" & char_pos.ToString)
                        If current_strict_match_info.isNonHTMLEntity Then
                            current_state = current_state
                        End If
                        'reset
                        current_state = ""
                        tag_start_offset = 0
                        tag_start = 0
                        tag_end = 0
                        attrib_name_start = 0
                        attrib_name_end = 0
                        attrib_assign_start = 0
                        attrib_assign_end = 0
                        attribute_assignment = ""
                        attribute_name = ""
                        tagname = ""
                        current_strict_match_info.Enabled = False

                        GoTo DONEXT
                    End If


                    'jump?

                    current_strict_match_info = PrimitiveXML.getstrictMatchingByNodeNameInfo(tagname)

                    If current_strict_match_info.Enabled Then
                        Dim newpos As Integer

                        newpos = InStr(char_pos, html, current_strict_match_info.endFormat)
                        If newpos > 0 Then

                            Msg("Add & Jumping to end tag for block " & char_pos.ToString & " - " & newpos & " for tag " & current_strict_match_info.startFormat & " .. " & current_strict_match_info.endFormat)

                            'jump to beginning of end tag
                            'and add end tag
                            char_pos = newpos - 1

                        Else
                            GUIMsg("Cannot jump on " & tagname)
                        End If
                    End If 'strict match enabled



                    If add_node Then
                        Dim ni As TriniDATHTTPTypes.TriniDATNode

                        ni = retval.setNewXMLOpenNode(tagname, tag_start_offset, Len(output_xml), char_pos - (tag_start - 1))
                        output_xml = output_xml & ni.renderXHTMLTag(ni.foundEndTag) & vbCrLf


                        If isCustomSingletonNode Then
                            Msg("Adding end for singleton " & tagname)
                            'report closed node 
                            Dim index As Integer
                            index = retval.getLastUnclosedNodeFromTheBack(tagname)
                            If index > -1 Then
                                retval.setXMLNodeclosed(index, tag_start_offset, Len(output_xml))
                                output_xml = output_xml & "</" & tagname & "> <!-- " & retval.getNodeByIndex(index).getId().ToString & " -->"
                            Else
                                GUIMsg("Error adding singleton node")
                            End If
                        End If
                    End If


                End If 'is closed / open node 

                'wipe out state
                current_state = ""
                tagname = ""
                attribute_name = ""
                attribute_assignment = ""
                current_state = ""
                tag_start_offset = 0
                tag_start = 0
                tag_end = 0
                attrib_name_start = 0
                attrib_name_end = 0
                attrib_assign_start = 0
                attrib_assign_end = 0
                attribute_name = ""
                tagname = ""
                current_strict_match_info.Enabled = False
                GoTo DONEXT
                'ElseIf (current_char = ">" And current_state = "tagstart.attributes.assignmentstart.quotedcontainer") Then
                '    Msg("javascript presence in " & tagname & "." & attribute_name)
                '    GoTo DONEXT
            End If


            If current_state = "tagstart" And ((current_char_ascii > 64 And current_char_ascii < 91) Or (current_char_ascii > 96 And current_char_ascii < 123) Or current_char = "/") Then
                'collect tagname a-z  A-Z
                tagname &= current_char
                GoTo DONEXT
            ElseIf current_state = "tagstart" And (current_char_ascii > 47 And current_char_ascii < 58) Then
                'add numeric e.g. H1
                'collect tagname 0-9
                tagname &= current_char
                GoTo DONEXT
            ElseIf (current_state = "tagstart") And (((current_char_ascii > 32 And current_char_ascii < 47)) Or (current_char_ascii > 57 And current_char_ascii < 65)) Then
                'special char range, excludes space and /

                'is user code/comment
                'wipe out state
                current_state = ""
                tagname = ""
                attribute_name = ""
                attribute_assignment = ""
                current_state = ""
                tag_start_offset = 0
                tag_start = 0
                tag_end = 0
                attrib_name_start = 0
                attrib_name_end = 0
                attrib_assign_start = 0
                attrib_assign_end = 0
                attribute_name = ""
                tagname = ""
                GoTo DONEXT
            End If

            If current_char = "<" And current_state = "" Then
                current_state = "tagstart"

                'INIT
                attrib_assign_container = Chr(34)
                tag_start_offset = char_pos - 1
                tag_start = char_pos + 1
                current_attributes = New StringDictionary

                Msg("Setting state to 'Tagstart' @ " & char_pos.ToString & "   guess=" & Mid(html, char_pos, 10))

                GoTo DONEXT
            End If

            If current_char = " " And current_state = "tagstart" Then
                tag_end = char_pos

                'lets see what tag we got

                tagname = Mid(html, tag_start, tag_end - tag_start)


                If PrimitiveXML.ignoreNode(tagname) Then
                    Msg("Do not look at " & tagname & " with multiple attributes ")
                    'wipe out state
                    current_state = ""
                    tagname = ""
                    attribute_name = ""
                    attribute_assignment = ""
                    current_state = ""
                    tag_start_offset = 0
                    tag_start = 0
                    tag_end = 0
                    attrib_name_start = 0
                    attrib_name_end = 0
                    attrib_assign_start = 0
                    attrib_assign_end = 0
                    attribute_name = ""
                    tagname = ""


                    GoTo DONEXT
                Else


                    'if non html entity
                    'then attribute scan is useless
                    current_strict_match_info = PrimitiveXML.getstrictMatchingByNodeNameInfo(tagname)

                    Msg("Looking at node with multiple attributes " & tagname.ToUpper() & " / strict match=" & current_strict_match_info.Enabled.ToString)

                    If current_strict_match_info.Enabled And current_strict_match_info.isNonHTMLEntity = True Then
                        'jump
                        Dim newpos As Integer

                        newpos = InStr(char_pos, html, current_strict_match_info.endFormat)
                        If newpos > 0 Then

                            Msg("Stop attribute scan for non-entity " & char_pos.ToString & " - " & newpos & " for tag " & current_strict_match_info.startFormat & " .. " & current_strict_match_info.endFormat & " (next chunk at " & char_pos.ToString & "=" & Mid(html, char_pos, 10) & ")")

                            'this entity's end tag cannot be interpreted as a end tag
                            'therefore add START AND end tag now and jump over it

                            Dim ni As TriniDATHTTPTypes.TriniDATNode

                            ni = retval.setNewXMLOpenNode(current_strict_match_info.NonHTML_RenderAs_Tag, tag_start_offset, Len(output_xml), char_pos - (tag_start - 1))
                            ni.setTagName(current_strict_match_info.NonHTML_RenderAs_Tag)
                            output_xml = output_xml & ni.renderXHTMLTag(ni.foundEndTag) & vbCrLf

                            'report closed node 
                            Dim index As Integer

                            index = retval.getLastUnclosedNodeFromTheBack(current_strict_match_info.NonHTML_RenderAs_Tag)
                            If index > -1 Then
                                retval.setXMLNodeclosed(index, tag_start_offset, Len(output_xml))
                                output_xml = output_xml & "</" & current_strict_match_info.NonHTML_RenderAs_Tag & ">  <!-- " & retval.getNodeByIndex(index).getId().ToString & " -->"
                            Else
                                GUIMsg("Error finding non-entity's end-tag. current_state = " & current_state)
                            End If


                            char_pos = newpos + Len(current_strict_match_info.endFormat)

                            Msg("reset state (next data at jump position " & char_pos.ToString & " =" & Mid(html, char_pos, 10) & ")")


                            'reset
                            current_state = ""
                            tag_start_offset = 0
                            tag_start = 0
                            tag_end = 0
                            attrib_name_start = 0
                            attrib_name_end = 0
                            attrib_assign_start = 0
                            attrib_assign_end = 0
                            attribute_name = ""
                            tagname = ""
                            attribute_assignment = ""
                            GoTo DONEXT
                        End If
                    End If

                    'begin of tag attribute (NAME)
                    current_state = "tagstart.attributes"
                    attrib_name_start = char_pos + 1
                    GoTo DONEXT

                End If
            End If

            If current_char = attrib_assign_container And current_state = "tagstart" Then
                current_state = "tagstart.propertyorevents"
                GoTo DONEXT
            End If

DONEXT:

        Next



        'return document

        Dim error_count As Integer
        Dim error_nodes As List(Of TriniDATHTTPTypes.TriniDATNode)

        error_nodes = New List(Of TriniDATHTTPTypes.TriniDATNode)

        'validate document
        error_count = 0
        For Each n In retval.getNodes()
            If n.foundEndTag = False Then
                error_nodes.Add(n)


                output_xml = output_xml & vbCrLf & "<ERRORID>tag= " & n.getTagName() & " TriniDATid=" & n.getId().ToString & " srchtmlpos=" & n.html_src_startpos.ToString & "</ERRORID>"
                error_count += 1

                'make singleton node
                'replace in xml document
                output_xml = Replace(output_xml, n.renderXHTMLTag(False), n.renderXHTMLTag_debugmode(True) & " <!-- FIXED " & n.getId().ToString & " -->")
                n.Issingleton = True
                n.foundEndTag = True
            End If
        Next

        If error_count > 0 Then
            GUIMsg(error_count.ToString & " Document errors found & fixed.")
            retval.setErrors(error_nodes)
        Else
            GUIMsg("No errors")
        End If

        retval.setSourceStr(output_xml)


        Return retval

    End Function
    Public Shared Sub GUIMsg(ByVal txt As String)

        If PrimitiveXML.GUI_Enabled = True Then
            MsgBox(txt)
        End If

    End Sub
    Public Function hasError() As Boolean
        Return Not IsNothing(Me.error_elements)
    End Function

    Public Sub New(ByVal src As String)
        Me.all_elements = Nothing
        Me.links = Nothing
        Me.setErrors(Nothing)
        Me.setSourceStr(src)

        Me.currentTriniDATID = 0
    End Sub

    Public ReadOnly Property getNodeCount() As Integer
        Get
            If Me.haveNodes() Then
                Return Me.all_elements.Count
            Else
                Return 0
            End If
        End Get
    End Property

    Public ReadOnly Property getSourceStr() As String
        Get
            Return Me.primitive_xml_document
        End Get
    End Property


    'StrictMatchInfo

    Public Shared Function getstrictMatchingByNodeNameInfo(ByVal tagname As String) As StrictMatchInfo

        Dim retval As StrictMatchInfo

        retval = New StrictMatchInfo

        retval.Enabled = False

        If tagname = "script" Then
            retval.Enabled = True
            retval.startFormat = "<script"
            retval.endFormat = "</script>"
            retval.isNonHTMLEntity = False
        End If

        If Left(tagname, 1) = "!" Then '<- is invalid
            'COMMENT
            retval.Enabled = True
            retval.startFormat = "<!--"
            retval.endFormat = "-->"
            retval.NonHTML_RenderAs_Tag = "comment"
            retval.isNonHTMLEntity = True
        End If

        Return retval
    End Function

    Public Shared Function ignoreNode(ByVal tagname As String) As Boolean

        Return (tagname = "!DOCTYPE") 'comment
    End Function

    Public Sub setSourceStr(ByVal val As String)
        Me.primitive_xml_document = val
    End Sub

    Public Function getFormattedSource() As String

        Dim xdoc As XDocument

        xdoc = Me.getAsDocument()
        If Not IsNothing(xdoc) Then
            Return xdoc.ToString()
        Else
            Return Me.getSourceStr()
        End If

    End Function

    Public ReadOnly Property getAsDocument() As XDocument
        Get
            Dim retval As XDocument
            Dim dom As XElement

            retval = New XDocument

            dom = Nothing

            Try

                'add top-level node
                dom = XElement.Parse("<Document>" & Me.getSourceStr() & "</Document>")

            Catch ex As XmlException

                Return Nothing

                Dim xml_lines() As String
                Dim error_line As String
                Dim TriniDATpos As Integer
                Dim TriniDATpos_end As Integer
                Dim TriniDATid As Integer
                Dim temp As String
                xml_lines = Me.getSourceStr().Split(vbNewLine)
                error_line = xml_lines(ex.LineNumber)
                TriniDATpos = InStr(error_line, "TriniDATid=")
                TriniDATid = -1
                If TriniDATpos > 0 Then
                    TriniDATpos = TriniDATpos + 9
                    TriniDATpos_end = InStr(TriniDATpos, error_line, Chr(34))
                    If TriniDATpos_end > 0 Then
                        temp = Mid(error_line, TriniDATpos, TriniDATpos_end - TriniDATpos)
                        If IsNumeric(temp) Then
                            Dim ni As TriniDATHTTPTypes.TriniDATNode
                            TriniDATid = CType(temp, Integer)
                            ni = Me.getNodeById(TriniDATid)
                            If Not IsNothing(ni) Then
                                temp = Mid(Me.getSourceStr(), ni.xml_str_startpos, 100) & " ...'"
                                GUIMsg("affected TriniDATid: " & TriniDATid.ToString)

                                Dim nextelement As TriniDATHTTPTypes.TriniDATNode
                                nextelement = Me.getNextNode(ni.getId())

                                If Not IsNothing(nextelement) Then
                                    Dim affected_block_len As Integer
                                    Dim end_tag_str As String
                                    Dim xml_dom_copy() As Char
                                    Dim x As Long
                                    Dim endpos As Long
                                    Dim cur_xml_dom As String

                                    cur_xml_dom = Me.getSourceStr()
                                    xml_dom_copy = cur_xml_dom.ToCharArray()

                                    affected_block_len = nextelement.xml_str_startpos - ni.xml_str_startpos
                                    'we calc from the next element so dont hit it's opening statement
                                    affected_block_len = affected_block_len - 1
                                    temp = Mid(cur_xml_dom, ni.xml_str_startpos, affected_block_len)

                                    endpos = ni.xml_str_startpos + affected_block_len
                                    MsgBox(temp & " start tag = " & (endpos - ni.xml_str_startpos).ToString)
                                    For x = ni.xml_str_startpos To endpos
                                        '                              Msg("DeletE: " & xml_dom_copy(x))
                                        xml_dom_copy(x) = ""
                                    Next

                                    end_tag_str = ni.getAsEndTag()

                                    affected_block_len = Len(end_tag_str)
                                    temp = Mid(cur_xml_dom, ni.xml_str_endpos + 1, affected_block_len)
                                    MsgBox(temp & " end tag = " & temp)

                                    endpos = ni.xml_str_endpos + affected_block_len
                                    For x = ni.xml_str_endpos To endpos
                                        Msg("Delete end: " & xml_dom_copy(x))
                                        xml_dom_copy(x) = ""
                                    Next


                                    temp = New String(xml_dom_copy)

                                    'delete end tag comment
                                    temp = Replace(temp, "<!-- " & ni.getId().ToString & " -->", "")

                                    'set new DOM and try again..
                                    Me.setSourceStr(temp)

                                    Msg("OLD XML DOM: " & cur_xml_dom)

                                    Msg("NEW XML DOM: " & Me.getSourceStr())
                                    Return Me.getAsDocument()

                                Else
                                    GUIMsg("Cannot retrieve next node after TriniDATid " & ni.getId())
                                End If
                            Else
                                GUIMsg("Cannot retrieve affected nodeid " & TriniDATid.ToString & " in exception handler...")
                            End If
                            'get end tag snippet

                        End If
                    End If
                End If


                GUIMsg("[TriniDATid: " & TriniDATid.ToString & "] Error loading XML doc" & ex.Message & " at line " & ex.LineNumber.ToString & " (line=" & xml_lines(ex.LineNumber).ToString & ")  linecol = " & ex.LinePosition.ToString)
                Return Nothing
            End Try


            retval.Add(dom)

            Return retval
        End Get
    End Property


    Public Shared Function Msg(ByVal txt As String)
        Debug.Print("PrimitiveXMLDoc: " & txt)
    End Function
End Class
