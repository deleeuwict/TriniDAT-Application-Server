Option Explicit On
Option Compare Text

Imports System.Reflection
Imports System.Text
Imports System.Web


Public Class TriniDATForm
    Inherits TriniDATGenericNodeFamily

    Protected Shadows children As TriniDATInputElementCollection
    Protected source_url As String

    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal owner_url As String, ByVal _parent As TriniDATNode, ByVal _child_nodes As TriniDATInputElementCollection)
        MyBase.New()
        MyBase.setParent(_parent)
        Me.children = _child_nodes
        Me.setSourceURL(owner_url)
    End Sub

    Public Shared Function createFrom(ByVal nodefamily As TriniDATGenericNodeFamily, ByVal page_url As String) As TriniDATForm
        Dim retval As TriniDATForm
        Dim blingnodes As List(Of TriniDATNode)
        Dim new_child_collection As TriniDATInputElementCollection
        Dim new_input_el As TriniDATInputElement

        new_child_collection = New TriniDATInputElementCollection
        blingnodes = nodefamily.getChildren()


        For Each htmlnode As TriniDATNode In blingnodes
            new_input_el = TriniDATInputElement.createFrom(htmlnode)
            new_child_collection.Add(new_input_el)
        Next

        'Create the form object
        retval = New TriniDATForm(page_url, nodefamily.getParent(), new_child_collection)
        Return retval

    End Function

    Public ReadOnly Property getId As String
        Get
            Dim retval As String
            retval = Me.getProperty("name")

            If retval <> "" Then
                Return retval
            Else
                retval = Me.getProperty("id")
                Return retval
            End If

        End Get
    End Property
    Public Function setId(ByVal val As String) As Boolean
        Return Me.setProperty("id", val)
    End Function
    Public Function getFormNode() As TriniDATNode
        Return Me.getParent()
    End Function
    Public ReadOnly Property getFormOnSubmitAction() As String
        Get
            Return Me.getProperty("onsubmit")
        End Get
    End Property
    Public ReadOnly Property getMethod() As BlingFormMethod
        Get
            Dim retvalstr As String
            Dim retval As BlingFormMethod

            retvalstr = Me.getProperty("method")
            '       retvalstr = Trim(retvalstr)

            If retvalstr <> "" Then
                retvalstr = retvalstr.ToUpper
            Else
                retval = BlingFormMethod.METHOD_GET
                Return retval
            End If

            Select Case retvalstr
                Case "GET"
                    retval = BlingFormMethod.METHOD_GET
                Case "POST"
                    retval = BlingFormMethod.METHOD_POST
            End Select

            Return retval
        End Get
    End Property
    Public ReadOnly Property getEncoding() As String
        'e.g. application/x-www-form-urlencoded
        Get
            Return Me.getProperty("enctype")
        End Get
    End Property
    Public ReadOnly Property getAction() As String
        Get
            Return Me.getProperty("action")
        End Get
    End Property

    Public ReadOnly Property getSourceURL As String
        Get
            Return Me.source_url
        End Get
    End Property
    Public ReadOnly Property getSourceURI As Uri
        Get
            Try
                Return New Uri(Me.source_url)
            Catch ex As Exception
                Return Nothing
            End Try
        End Get
    End Property
    Public Function Submit(Optional ByVal new_lion_browser_event_model As Object = Nothing) As Object
        'returns a new LionBrowser instance with the target page loaded.

        Dim client_asm As Assembly
        Dim b As Object

        'create Lion instance dynamically because it is not accessible from this assmebly due to circular references
        client_asm = System.Reflection.Assembly.GetCallingAssembly()

        'create browser object
        b = TriniDATHTMLTools.createLionBrowserInstance(client_asm)

        If IsNothing(b) Then
            Err.Raise(0, 0, "Form.Submit(): Unable to create BlingBrowser obj.")
            Return Nothing
        End If


        If Not IsNothing(new_lion_browser_event_model) Then
            Call b.setEventModel(new_lion_browser_event_model)
        End If



        Dim final_url As String
        Dim page_uri As Uri

        Dim target As String

        'parse general form action
        target = Me.getAction()

        'parent url to get domain information from
        page_uri = Me.getSourceURI()


        If target.ToLower() <> "http" Then
            final_url = page_uri.Scheme & "://"

            'get domain name
            final_url &= page_uri.Host

            'port
            If Not page_uri.IsDefaultPort() Then
                final_url &= ":" & page_uri.Port.ToString
            End If


            If Left(target, 1) <> "/" Then
                final_url &= "/"
            End If

            'append relative url
            final_url &= target

        Else
            'form target is absolute url
            final_url = target
        End If


        If Me.getMethod() = BlingFormMethod.METHOD_POST Then
            b.setURL(final_url)
            Return Me.doSubmitPOST(b, final_url)
        ElseIf Me.getMethod() = BlingFormMethod.METHOD_GET Then
            Return Me.doSubmitGET(b, New Uri(final_url))
        End If

        Return Nothing
    End Function

    Protected Overridable Function doSubmitGET(ByVal new_browser As Object, ByVal absolute_uri As Uri) As Object
        'returns loaded browser obj
        Dim bfirst As Boolean
        Dim form_field As TriniDATNode
        Dim final_get_url_str As String

        final_get_url_str = absolute_uri.AbsoluteUri()

        'should have absolute url in form_url
        If absolute_uri.Query = "" Then
            'append ? before variables
            final_get_url_str &= "?"
        End If

        'encode all GET parameters
        bfirst = True
        For Each form_field In Me.getInputFields()
            Dim input_name As String
            Dim input_value As String

            input_name = form_field.getAttribute("name")
            If input_name <> "" Then
                If bfirst Then
                    bfirst = False
                Else
                    final_get_url_str &= "&"
                End If

                'add get pair to final url
                final_get_url_str &= input_name & "="

                input_value = form_field.getAttribute("value")
                If input_value <> "" Then
                    final_get_url_str &= System.Web.HttpUtility.UrlEncode(input_value)
                End If
            End If
        Next

        new_browser.setURL(final_get_url_str)

        If new_browser.execGet(final_get_url_str, Me) Then
            Return new_browser
        Else
            Return Nothing
        End If
    End Function
    Protected Overridable Function doSubmitPOST(ByVal new_browser As Object, ByVal post_url As String) As Object
        'returns browser obj

        If new_browser.execPost(Me) Then
            Return new_browser
        Else
            Return Nothing
        End If

    End Function
    Protected ReadOnly Property getProperty(ByVal attrib_name As String) As String
        Get
            Dim frm As TriniDATNode

            frm = Me.getFormNode()

            If Not IsNothing(frm) Then
                Return frm.getAttribute(attrib_name)
            Else
                Return ""
            End If

        End Get
    End Property
    Public Function setProperty(ByVal attrib_name As String, ByVal val As String) As Boolean

        Dim frm As TriniDATNode

        frm = Me.getFormNode()

        If Not IsNothing(frm) Then
            frm.setAttribute(attrib_name, val)
            Return True
        End If

        Return False
    End Function
    Public ReadOnly Property haveButtons() As Boolean
        Get
            Dim found As Boolean

            found = Not IsNothing(Me.getInputFields("button", True))

            Return found

        End Get
    End Property
    Public ReadOnly Property haveHiddenFields() As Boolean
        Get
            Dim found As Boolean

            found = Not IsNothing(Me.getInputFields("hidden", True))

            Return found

        End Get
    End Property

    Public ReadOnly Property haveInputs() As Boolean
        Get
            Dim found As Boolean

            found = Not IsNothing(Me.getInputFields(Nothing, True))

            Return found

        End Get
    End Property

    Public ReadOnly Property haveSubmitButton() As Boolean
        Get
            Dim found As Boolean

            found = Not IsNothing(Me.getSubmit())

            Return found

        End Get
    End Property

    Public Sub setSourceURL(ByVal val As String)
        Me.source_url = val
    End Sub

    Public ReadOnly Property getSubmit() As TriniDATFormSubmitButton
        Get

            Dim form_fields As TriniDATInputElementCollection
            Dim submit_node As TriniDATFormSubmitButton


            form_fields = Me.getInputFields("submit")

            If IsNothing(form_fields) Then
                Return Nothing
            End If

            submit_node = TriniDATFormSubmitButton.createFrom(form_fields(0))

            If Not IsNothing(submit_node) Then
                Return submit_node
            Else
                Return Nothing
            End If

        End Get
    End Property
    Public Overridable ReadOnly Property getButtons(Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Return Me.getInputFields("button", stop_at_results)
        End Get
    End Property
    Public Overridable ReadOnly Property getHiddenFields(Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Return Me.getInputFields("hidden", stop_at_results)
        End Get
    End Property

    Public Overridable ReadOnly Property getByNameOrId(ByVal form_field_name_or_id As String, Optional ByVal input_type As String = Nothing, Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Dim retval As TriniDATInputElement
            Dim found As Boolean

            retval = Nothing


            For Each fld As TriniDATInputElement In Me.children

                If fld.Issingleton Then
                    If fld.getTagName() = "input" Then
                        If fld.getOrAttribute("name", "id") = form_field_name_or_id Then
                            If Not IsNothing(input_type) Then
                                If fld.hasAttribute("type") Then
                                    If fld.getAttribute("type") = input_type Then
                                        retval = fld
                                        GoTo FINISH
                                    End If
                                End If
                            Else
                                'no strict mode
                                retval = fld
                                GoTo FINISH
                            End If
                        End If
                    End If
                End If
            Next

FINISH:
            found = Not (IsNothing(retval))

            If found Then
                Return retval
            Else
                Return Nothing
            End If

        End Get
    End Property


    Public Overridable ReadOnly Property getInputFields(Optional ByVal input_type As String = Nothing, Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Dim retval As TriniDATInputElementCollection
            Dim found As Boolean

            retval = New TriniDATInputElementCollection


            For Each fld As TriniDATInputElement In Me.children

                If fld.Issingleton Then
                    If fld.getTagName().ToLower = "input" Then
                        If Not IsNothing(input_type) Then
                            If fld.hasAttribute("type") Then
                                If fld.getAttribute("type").ToLower = input_type.ToLower Then
                                    retval.Add(fld)

                                    If stop_at_results Then
                                        GoTo FINISH
                                    End If
                                End If
                            End If
                        Else
                            retval.Add(fld)
                            If stop_at_results Then
                                GoTo FINISH
                            End If
                        End If
                    End If
                End If
            Next

FINISH:
            found = (retval.Count > 0)

            If found Then
                Return retval
            Else
                Return Nothing
            End If

        End Get
    End Property


    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class


Public Enum BlingFormMethod
    METHOD_GET = 1
    METHOD_POST = 2

End Enum