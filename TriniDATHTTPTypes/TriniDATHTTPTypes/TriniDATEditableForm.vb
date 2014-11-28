Imports System.Collections.Specialized


Public Class TriniDATEditableForm
    Inherits TriniDATForm

    Private mutatations As TriniDATMutationRecord
    Private debug_logger As TriniDATTypeLogger
    Protected Shadows children As TriniDATEditableInputCollection

    Public Sub New(Optional ByVal _debug_logger As TriniDATTypeLogger = Nothing)
        MyBase.New()
        Me.mutatations = New TriniDATMutationRecord

        If IsNothing(_debug_logger) Then
            _debug_logger = AddressOf NullEvent
        End If

        Me.debug_logger = _debug_logger
        Call Init()

    End Sub
    Public Sub setDebuggerLog(ByVal _logfunc As TriniDATTypeLogger)
        Me.debug_logger = _logfunc
        Me.mutatations.setDebuggerLog(_logfunc)
    End Sub
    Public Shadows Function setProperty(ByVal name As String, ByVal val As String) As Boolean
        Dim retval As Boolean


        retval = MyBase.setProperty(name, val)

        If retval Then
            '   Me.OnformFieldMutation(Me.getId(), "", val)
            Me.mutatations.Record("setfrmprop=" & Me.getId() & "." & name, val)
        End If

        Return retval
    End Function
    Protected Overrides Function doSubmitGET(ByVal new_browser As Object, ByVal absolute_uri As Uri) As Object
        Dim retval As Object

        Me.mutatations.Record("frmsubmit=" & Me.getId(), "GET")

        retval = MyBase.doSubmitGET(new_browser, absolute_uri)

        Return retval

    End Function

    Protected Overrides Function doSubmitPOST(ByVal new_browser As Object, ByVal post_url As String) As Object
        Dim retval As Object

        Me.mutatations.Record("frmsubmit=" & Me.getId(), "POST")

        retval = MyBase.doSubmitPOST(new_browser, post_url)

        Return retval

    End Function


    Private Sub Msg(ByVal txt As String)
        Call Me.debug_logger(txt)
    End Sub
    Private Sub NullEvent(ByVal txt As String)

    End Sub

    Private Sub Init()
        Me.mutatations = New TriniDATMutationRecord(Me.debug_logger)

    End Sub
    Public Sub OnFormFieldMutation(ByVal inputname As String, ByVal old_value As String, ByVal new_value As String)
        'log
        Me.mutatations.Record("set=" & inputname, new_value)
    End Sub


    Public Sub setInputs(ByVal val As TriniDATEditableInputCollection)
        Me.children = val
    End Sub

    Public Shared Shadows Function createFrom(ByVal classic_form As TriniDATForm) As TriniDATEditableForm

        Dim new_form As TriniDATEditableForm
        Dim new_form_inputs As TriniDATEditableInputCollection

        new_form = New TriniDATEditableForm
        new_form_inputs = New TriniDATEditableInputCollection

        'transform regular input elements to editable
        new_form_inputs = TriniDATEditableInputCollection.createFrom(classic_form.getInputFields())
        'attach form sub to all input fields through the collection
        new_form_inputs.currentMutationHandler = AddressOf new_form.OnformFieldMutation

        new_form.setSourceURL(classic_form.getSourceURL())
        new_form.setParent(classic_form.getParent())
        new_form.setInputs(new_form_inputs)

        Return new_form
    End Function

    Public ReadOnly Property AllMutations(Optional ByVal clearRecord As Boolean = False) As String
        Get

            Return Me.mutatations.MutationList(clearRecord)

        End Get
    End Property
    Public Overrides ReadOnly Property getByNameOrId(ByVal form_field_name_or_id As String, Optional ByVal input_type As String = Nothing, Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Dim retval As TriniDATEditableInputElement
            Dim found As Boolean

            retval = Nothing


            For Each fld As TriniDATEditableInputElement In Me.children

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
    Public Overrides ReadOnly Property getButtons(Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Return Me.getInputFields("button", stop_at_results)
        End Get
    End Property
    Public Overrides ReadOnly Property getHiddenFields(Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Return Me.getInputFields("hidden", stop_at_results)
        End Get
    End Property
    Public Overrides ReadOnly Property getInputFields(Optional ByVal input_type As String = Nothing, Optional ByVal stop_at_results As Boolean = False) As Object
        Get
            Dim retval As TriniDATEditableInputCollection
            Dim found As Boolean

            retval = New TriniDATEditableInputCollection


            For Each fld As TriniDATEditableInputElement In Me.children

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
End Class
