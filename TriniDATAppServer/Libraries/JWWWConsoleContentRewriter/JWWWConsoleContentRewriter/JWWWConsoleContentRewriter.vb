Option Explicit On
Imports <xmlns="http://www.w3.org/1999/xhtml">
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Collections.Specialized
Imports System.IO
Imports System.Reflection
Imports TriniDATServerTypes
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JWWWConsoleContentRewriter
    Inherits JTriniDATWebService
    '
    'searches datastory for json objects with key:value
  
    Public runtimeCount As Integer
    'buffered output html
    Private html_page As String
    ''Private reflect_blocks_resolved_count As Integer
    Private html_mimetype As String
    'new
    Private current_ReflectBlocks() As String ''blocks of user code are handled uniquely.
    Private reflect_block_to_cleanup As StringCollection
    Public Sub New()
        MyBase.New()
        Me.runtimeCount = 0
        Me.html_page = ""
        Me.reflect_block_to_cleanup = New StringCollection

    End Sub

    Public Overrides Function DoConfigure() As Boolean
        'configure mailbox
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

        GetIOHandler().Configure(http_events)

    End Sub

    Public Sub OnGet(ByVal HTTP_URI_Path As String, ByVal HTTP_URI_Parameters As StringDictionary, ByVal HTTP_URI_Headers As StringDictionary)
        runtimeCount = runtimeCount + 1
    End Sub
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Msg("JOmega FLUSH_OUTPUT received. Buffering output HTML...")

            'do hardcoded rewrites.
            Me.html_page = ReWrite(Me.html_page)

            'replace placeholders with dynamic content

            GetIOHandler().setOutputMime(Me.html_mimetype)
            GetIOHandler().setOutput(Me.html_page)
            Me.GetIOHandler().setHTTPResponse(200)

            'flush output buffer
            GetIOHandler().flushOutput(True, True)

            Return False

        End If

        If obj.ObjectTypeName = "JReflectResponseForARRAY" Then
            'obj.Attachment = MethodInfo object

            Dim globalinstance As Object
            Dim globalinstanceKind As Type

            If Not IsNothing(obj.Attachment) Then

                Dim arrayfuncGetByIndex As MethodInfo
                Dim arrayfuncCount As MethodInfo
                Dim userX As Integer
                Dim userCount As Integer
                Dim userXMLNodeID As Integer
                Dim ReplacementXMLNodes As String
                Dim ptr As MethodInfo



                'globalinstance must point to A Class that exposes getByIndex() and getCount() methods. 
                If obj.Attachment.GetType().Name = "System.Reflection.MethodInfo" Then
                    'A Pointer to a shared method  - obtain interface class by executing it.
                    ptr = CType(obj.Attachment, System.Reflection.MethodInfo)
                    globalinstance = ptr.Invoke(Nothing, Nothing)
                    globalinstanceKind = ptr.ReturnType
                Else
                    'operate on create class instance
                    globalinstance = obj.Attachment
                    globalinstanceKind = globalinstance.GetType()
                End If

                userXMLNodeID = CType(obj.Tag, Integer)
                ReplacementXMLNodes = ""


                '        Dim userXMLNodes = From block In Me.xml_doc...<Reflect>
                ''Where(block.@object_name = globalinstanceKind.Name And block.@object_type = "array" And block.@id = userXMLNodeID)
                ''                   Select block

                Dim userXMLTemplateChunk As String
                Dim user_content_start As Integer
                Dim user_content_end As Integer

                ''userXMLTemplateChunk = userXMLNodes(0).ToString
                user_content_start = InStr(current_ReflectBlocks(userXMLNodeID), ">") + 1
                user_content_end = InStrRev(current_ReflectBlocks(userXMLNodeID), "</Reflect>") - 1
                userXMLTemplateChunk = Mid(Me.current_ReflectBlocks(userXMLNodeID), user_content_start, user_content_end - user_content_start)


                Debug.Print(userXMLTemplateChunk)

                'get the globalinstance.GetByIndex() return value
                arrayfuncGetByIndex = globalinstanceKind.GetMethod("getByIndex")
                arrayfuncCount = globalinstanceKind.GetMethod("getCount")
                userCount = 0

                'todo: if err / count = 0 then remove html snippet from html buffer

                If Not IsNothing(arrayfuncCount) Then
                    'execute the count function
                    userCount = arrayfuncCount.Invoke(globalinstance, Nothing)
                End If

                If (Not IsNothing(arrayfuncGetByIndex) And userCount > 0) Then


                    For userX = 0 To userCount - 1

                        Dim expression_lines() As String
                        Dim expression As String
                        Dim expressionX As Integer
                        Dim expression_end As Integer
                        Dim vars() As String
                        Dim varX As Integer
                        Dim varOrfuncVariants() As MemberInfo
                        Dim stackLevels As Integer
                        Dim memberX As Integer
                        Dim parentObject As Object
                        Dim expressionIsFunction As Boolean
                        Dim func_retval As Object

                        Dim arrayCurrentElementType As Type
                        Dim arrayGetByIndexLatestReturnValue As Object
                        Dim gotValue As Boolean
                        Dim varvalue As String

                        'allocate a template node for each array item.
                        ReplacementXMLNodes = ReplacementXMLNodes & userXMLTemplateChunk & vbCrLf

                        'get user array index
                        arrayGetByIndexLatestReturnValue = arrayfuncGetByIndex.Invoke(globalinstance, New Object() {userX})
                        'return value of globalinstance.getByIndex()
                        arrayCurrentElementType = arrayfuncGetByIndex.ReturnType

                        'parse all variables
                        expression_lines = Split(userXMLTemplateChunk, "$")
                        expression_end = 0

                        For expressionX = 1 To expression_lines.Length - 1
                            expression = extractExpression(Mid(expression_lines(expressionX), 2))

                            If IsNothing(expression) Then
                                Msg("Dynamic script syntax error: expression lacks terminator >> " + expression_lines(expressionX))
                                Exit For
                            End If

                            'var1.var2.var3.GetName()

                            parentObject = arrayGetByIndexLatestReturnValue
                            vars = Split(expression, ".")
                            stackLevels = vars.Length


                            For varX = 0 To stackLevels
                                gotValue = False
                                varvalue = ""

                                expressionIsFunction = InStr(vars(varX), "()")
                                If expressionIsFunction Then
                                    'strip ()
                                    vars(varX) = Replace(vars(varX), "(", "")
                                    vars(varX) = Replace(vars(varX), ")", "")
                                End If

                                Msg("Looking up variable or function " & parentObject.GetType().Name & "[" & userX.ToString & "]." & vars(varX))

                                varOrfuncVariants = parentObject.GetType().GetMember(vars(varX))
                                For memberX = 0 To 1 ' HARDCODED because we do not support complex funcs aka overloads , realtime value= varOrfuncVariants.Length - 1

                                    If varX = stackLevels - 1 Then
                                        'this is the final element 
                                        'to produce a return value or is a variable.

                                        If varOrfuncVariants(varX).MemberType = MemberTypes.Method And expressionIsFunction Then
                                            'format: vara.varb.getName() - invoke
                                            Dim func_ptr As MethodInfo
                                            func_ptr = CType(varOrfuncVariants(varX), MethodInfo)
                                            func_retval = func_ptr.Invoke(parentObject, Nothing)
                                            varvalue = CType(func_retval, String)
                                            gotValue = True
                                        Else
                                            'this is a var
                                            Dim varinfo As FieldInfo

                                            varinfo = CType(varOrfuncVariants(varX), FieldInfo)
                                            varvalue = CType(varinfo.GetValue(parentObject), String)
                                            gotValue = True
                                        End If

                                        If gotValue Then
                                            Msg(parentObject.GetType().Name & "[" & userX.ToString & "]." & vars(varX) & " = " & varvalue)
                                        Else
                                            Msg("Error parsing " & parentObject.GetType().Name & ". Members do not match user variables!")
                                        End If

                                        'done parsing
                                        Exit For
                                    ElseIf varOrfuncVariants(varX).GetType().MemberType = MemberTypes.Method Then
                                        'SET NEW PARENT = function return value
                                        Dim methinfo As MethodInfo
                                        methinfo = CType(varOrfuncVariants(varX), MethodInfo)

                                        parentObject = methinfo.Invoke(parentObject, Nothing)
                                    Else
                                        'SET NEW PARENT = a variable
                                        Dim varinfo As FieldInfo

                                        varinfo = CType(varOrfuncVariants(varX), FieldInfo)
                                        parentObject = varinfo.GetValue(parentObject)
                                    End If


                                Next 'next var enumerated types

                                If varX = stackLevels - 1 And gotValue Then
                                    'replace html -> actual value
                                    ReplacementXMLNodes = Replace(ReplacementXMLNodes, "$." & expression, varvalue)

                                    Exit For
                                End If

                                'parentObject = vars(varx)
                            Next 'next var level


                        Next 'expression_lines

                    Next 'for userX to userCount
                End If 'if getbyIndex


                Try

                    Dim unparsed_blocks() As String
                    Dim endtag As Integer
                    Dim unparsed_block As String

                    unparsed_blocks = Split(Me.html_page, "<Reflect ")
                    unparsed_block = unparsed_blocks(userXMLNodeID) 'because parsedynamicHTML() shoots blocks out serially and at once, the actual index is now lower

                    endtag = InStr(unparsed_block, "</Reflect>") - 1
                    unparsed_block = Mid(unparsed_block, 1, endtag)
                    unparsed_block = "<Reflect " & unparsed_block & "</Reflect>"

                    'remove tags from rendered block

                    Me.html_page = Replace(Me.html_page, unparsed_block, ReplacementXMLNodes)

                    'parse next 
                    Call parseDynamicHTML()

                Catch ex As Exception
                    Msg("XML thingy error: " & ex.Message & " @ " & ex.StackTrace.ToString)
                End Try

            End If 'If Not IsNothing(ptr) Then



            Exit Function
        End If 'eo ReflectARRAY

       
        If obj.ObjectTypeName = "JTemplateHTML" Then
            'rewrite
            Msg("Template page, rewriting. [content-type: " & obj.Tag & "]")
            'mime-type
            Me.html_mimetype = obj.Tag
            Me.html_page = obj.Directive 

            If InStr(html_page, "<Reflect") > 0 And InStr(html_page, "$") Then
                Msg("Parsing XML...")
                Call parseDynamicHTML()
            End If

            Exit Function
        ElseIf obj.ObjectTypeName = "JTemplateCSS" Then
            'no rewrite but add to output buffer

            Msg("CSS received. [content-type: " & obj.Tag & "]")
 
            'mime-type
            GetIOHandler().setOutputMime(obj.Tag)
            GetIOHandler().addOutput(ReWrite(obj.Content))
            Me.GetIOHandler().setHTTPResponse(200)
            GetIOHandler().flushOutput(True, True)
            Exit Function
        ElseIf obj.ObjectTypeName = "JTemplateFile" Then
            'todo: send binary
            Msg("Generic file received. [content-type: " & obj.Tag & "]")
            'mime-type
            GetIOHandler().setOutputMime(obj.Tag)
            GetIOHandler().addOutput(ReWrite(obj.Content))
            Me.GetIOHandler().setHTTPResponse(200)
            GetIOHandler().flushOutput(True, True)
        End If

        Return False
    End Function

    Private Function extractExpression(ByVal expression As String) As String

        Dim y As Integer
        Dim ascii_code As Integer
        Dim terminator As Integer
        Dim func_start_found As Boolean

        terminator = -1
        func_start_found = False
        For y = 0 To expression.Length - 1
            ascii_code = Asc(expression.Chars(y))

            If ascii_code = 40 Then
                func_start_found = True
            End If

            If (ascii_code > 64 And ascii_code < 91) Or (ascii_code > 96 And ascii_code < 123) Or (ascii_code = 46) Or (ascii_code = 40) Or (ascii_code = 41) Then
                'A..Z / a..z . ()
                If func_start_found = False And ascii_code = 41 Then
                    ') but no (
                    terminator = y
                    Exit For
                End If
            Else
                terminator = y
                Exit For
            End If
        Next

        If terminator = -1 Then
            Return Nothing
        Else
            Return Mid(expression, 1, terminator)
        End If
    End Function

    Private Function convertHTML2XML(ByVal html As String)

        Dim doctype_pos As Integer
        Dim doctype_end As Integer

        html = Replace(html, "&nbsp;", " ")
        html = Replace(html, "&", "&amp;")

        'STRIP DOCTYPE tag
        doctype_pos = InStr(html, "<!DOCTYPE html")
        If doctype_pos > 0 Then

            doctype_end = InStr(doctype_pos, html, ">")
            html = Replace(html, Mid(html, doctype_pos, doctype_end - doctype_pos + 1), "")
        End If

        html = Replace(html, "<html>", "<html xmlns=""http://www.w3.org/1999/xhtml"">")

        'single attribute arguments
        html = Replace(html, "nowrap", "nowrap=""nowrap""")
        html = Replace(html, "<BR>", "<BR/>")

        Return html
    End Function


    Public Sub parseDynamicHTML()
        '1. scan HTML doc for XML tags
        '2. send reflection requests to JKernelReflection.

        Dim stringparse As StringReader

        Dim ReflectBlockIndex As Integer
        Dim endtag As Integer

        Dim xml_doc As XDocument

        Try

            Me.current_ReflectBlocks = Split(Me.html_page, "<Reflect ")


            For ReflectBlockIndex = 1 To current_ReflectBlocks.Length - 1

                Me.current_ReflectBlocks(ReflectBlockIndex) = "<Reflect " & Me.current_ReflectBlocks(ReflectBlockIndex)
                endtag = InStr(Me.current_ReflectBlocks(ReflectBlockIndex), "</Reflect>") - 1

                If endtag > 0 Then
                    'make XML compatbile.
                    Me.current_ReflectBlocks(ReflectBlockIndex) = Mid(Me.current_ReflectBlocks(ReflectBlockIndex), 1, endtag)
                    Me.current_ReflectBlocks(ReflectBlockIndex) = Me.current_ReflectBlocks(ReflectBlockIndex) & "</Reflect>"
                    stringparse = New StringReader(convertHTML2XML(Me.current_ReflectBlocks(ReflectBlockIndex)))

                    'set before sending any requests.
                    xml_doc = New XDocument
                    xml_doc = XDocument.Load(stringparse)


                    Dim reflections As IEnumerable(Of XElement) = xml_doc.Elements()
                    Dim Reflect As XElement

                    For Each Reflect In reflections
                        Dim object_name As String
                        Dim object_type As String
                        Dim req As New JSONObject


                        If InStr(Reflect.ToString, "$") > 0 Then
                            'only process if actually used
                            object_name = Reflect.@object_name
                            object_type = Reflect.@object_type

                            Reflect.SetAttributeValue("id", ReflectBlockIndex.ToString)
                            'send reflection request to JKernelReflection

                            req.ObjectType = "JReflectServiceFor" & object_type.ToUpper()
                            req.Directive = object_name
                            req.Tag = ReflectBlockIndex
                            Me.getMailProvider().Send(req, Nothing, "JKernelReflection")

                            ''    Me.reflect_blocks_resolved_count = reflect_blocks_resolved_count + 1
                            Exit Sub

                        End If
                    Next 'parse reflect block
                Else
                    'show scripting error on user html page
                    Dim temp_lines() As String
                    Dim absolutepos As Integer
                    Dim err_col As Integer

                    absolutepos = InStr(Me.html_page, current_ReflectBlocks(ReflectBlockIndex)) + 10
                    temp_lines = Split(Mid(Me.html_page, 1, absolutepos), vbCrLf)

                    err_col = InStrRev(temp_lines(temp_lines.Length - 1), "<Reflect") - 1

                    Me.html_page = Replace(html_page, current_ReflectBlocks(ReflectBlockIndex), "<span style='color: red;'> Line: " & temp_lines.Length.ToString & " Col:  " & err_col.ToString & ": Bosswave dynamic script error: Missing endtag.</SPAN>" & current_ReflectBlocks(ReflectBlockIndex))
                End If 'eof reflectblock err
            Next 'eof reflectblock

        Catch ex As Exception
            Msg("Error parsing page as XML document!" & ex.Message & " @ " & ex.StackTrace.ToString & vbCrLf & "ERR HTML=" & Me.html_page)
        End Try


    End Sub
    Public Shared Function XMLToHTML(ByVal node As XElement) As String
        ' Remove all xmlns:* instances from the passed XmlDocument
        ' to simplify our xpath expressions.
        Return (System.Text.RegularExpressions.Regex.Replace(node.ToString, "(xmlns:?[^=]*=[""""][^""""]*[""""])", "", (System.Text.RegularExpressions.RegexOptions.IgnoreCase Or System.Text.RegularExpressions.RegexOptions.Multiline)))

    End Function

    Public Sub delivered(ByVal obj As JSONObject, destination_url as string)
        Msg("delivered>> object " & obj.ObjectTypeName & " successfully sent.")

    End Sub

  
    Public Sub deliveryerr(ByVal obj As JSONObject)
        Msg(">> error sending object r. Type=" & obj.ObjectTypeName)

    End Sub

    Public Function ReWrite(ByVal html_source As String) As String
        Dim retval As String

        retval = Replace(html_source, "Boss", "B0ss")

        Return retval

    End Function
End Class

