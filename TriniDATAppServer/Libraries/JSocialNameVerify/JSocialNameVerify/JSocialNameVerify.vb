Imports System.Collections.Specialized
Imports TriniDATServerTypes
Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

'checks if full names or nicknames are used and classifies the authenticness.
Public Class JSocialNameVerify
    Inherits JTriniDATWebService

    Public runtimeCount As Integer = 0

    Public Sub New()
        MyBase.New()
        runtimeCount = 0

    End Sub

    Public Overrides Function DoConfigure() As Boolean
        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox


        getMailProvider().Configure(mb_events, False)

        Dim http_events As TriniDATHTTP_EventTable
        http_events = New TriniDATHTTP_EventTable

        GetIOHandler().Configure(http_events)

        Return True
    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Function myinbox(ByRef obj As JSONObject, ByVal from_url As String) As Boolean

        If obj.ObjectTypeName = "JSocialNameVerify" And obj.Directive = "VALIDATENAME" Then
            Dim parsed_result As JNameCheckResult
            Dim resp As JSONObject
            '            Msg("Starting validation of '" & obj.Tag & "' ")

            parsed_result = Me.doNameValidation(obj.Tag)
            parsed_result.Tag = obj.Attachment 'probably a xelement node ..
            'send reply
            resp = New JSONObject
            resp.ObjectType = "JNameCheckResult"
            resp.Sender = Me
            resp.Attachment = parsed_result
            'send reply
            Me.getMailProvider().Send(resp, Nothing, "JTwitterFeedSearch")
            Return False
        End If

        Return False
    End Function

    Public Function doNameValidation(ByVal unparsed_name As String) As JNameCheckResult

        Dim retval As JNameCheckResult
        Dim name_parts() As String


        retval = New JNameCheckResult
        retval.count = 0

        retval.unparsed = unparsed_name

        'FIX
        retval.unparsed = Replace(retval.unparsed, "_", " ")
        retval.unparsed = Trim(retval.unparsed)

        If Len(retval.unparsed) < 3 Then Return retval


        name_parts = Split(retval.unparsed, " ")
        retval.count = name_parts.Length

        'init
        retval.rank = "discard"

        retval.specialchar_in_name = True
        retval.first_part = ""
        retval.middle_part = ""
        retval.last_part = ""

        For x = 1 To retval.unparsed.Length
            Dim ascii_code As Integer

            ascii_code = Asc(Mid(retval.unparsed, x, 1))

            retval.specialchar_in_name = ((ascii_code > 33 And ascii_code < 65) And ascii_code <> 45 And ascii_code <> 46 And ascii_code < 166)
            If retval.specialchar_in_name Then
                retval.specialchar_in_name = retval.specialchar_in_name
                Exit For
            End If

        Next

        'RANKING
        retval.first_part = name_parts(0)
        retval.middle_part = ""
        retval.last_part = ""

        Try

            If retval.count = 2 Then
                retval.middle_part = ""
                retval.last_part = name_parts(1)
            ElseIf retval.count > 2 Then
                retval.first_part = name_parts(0)
                retval.middle_part = name_parts(1)
                If retval.count > 3 Then
                    'first middle last name
                    retval.last_part = name_parts(2)
                Else
                    ''beyond first middle last name
                    retval.last_part = name_parts(retval.count - 1)
                End If
            End If

        Catch ex As Exception
            MsgBox("Warning: error while parsing: " & ex.Message & " @ " & ex.StackTrace.ToString)
        End Try

        If retval.count = 2 Then
            If retval.getFirstNameLength() >= 3 And retval.getLastName().Length >= 3 And Not retval.specialchar_in_name Then
                'check for properly capitalized FIRSt-LAST Name
                If (retval.getFirstNameAsc(1) > 64 And retval.getFirstNameAsc(1) < 91) And (retval.getLastNameAsc(1) > 64 And retval.getLastNameAsc(1) < 91) Then
                    retval.rank = "classic authentic"
                Else
                    retval.rank = "sloppy authentic"
                End If

            End If
        ElseIf retval.count = 3 And Not retval.specialchar_in_name Then
            'check for properly capitalized FIRSt-LAST Name 
            If (retval.getFirstNameAsc(1) > 64 And retval.getFirstNameAsc(1) < 91) And (retval.getLastNameAsc(1) > 64 And retval.getLastNameAsc(1) < 91) Then
                retval.rank = "elite authentic"
            Else
                retval.rank = "good"
            End If

        ElseIf retval.specialchar_in_name Then
            retval.rank = "teenbot rank"
        Else

            retval.rank = "teen rank"
        End If

        Return retval
    End Function

End Class

Public Class JNameCheckResult

    Public rank As String
    Public specialchar_in_name As Boolean
    Public unparsed As String
    Public first_part As String
    Public middle_part As String
    Public last_part As String
    Public count As Integer

    'because this object is passed around a lot among different classes
    'it will come handy for this object to transport addiditional objects.
    Private name_tag As Object

    Public Sub New()
        Me.first_part = ""
        Me.middle_part = ""
        Me.last_part = ""

    End Sub

    Public Property Tag
        Get
            Return Me.name_tag
        End Get
        Set(ByVal val)
            Me.name_tag = val
        End Set
    End Property

    Public Function getTag() As Object
        Return Me.tag
    End Function
    Public Function hasTag() As Boolean
        Return Not IsNothing(Me.getTag())
    End Function

    Public Function haveName() As Boolean
        Return (Len(Me.first_part) > 0)
    End Function
    Public Function haveMiddleName() As Boolean
        Return (Len(Me.middle_part) > 0)
    End Function

    Public Function haveLastName() As Boolean
        Return (Len(Me.last_part) > 0)
    End Function

    Public Function getFirstName() As String
        Return Me.first_part
    End Function
    Public Function getFirstNameAsc(ByVal x As Integer) As Integer
        Return Asc(Mid(Me.first_part, x, 1))
    End Function
    Public Function getFirstNameLength() As Integer
        Return Len(Me.getFirstName)
    End Function
    Public Function getMiddleLength() As Integer
        Return Len(Me.getLastName)
    End Function

    Public Function getLastNameLength() As Integer
        Return Len(Me.getLastName)
    End Function
    Public Function getMiddleName() As String
        Return Me.middle_part
    End Function

    Public Function getrank() As String
        Return Me.rank
    End Function

    Public Function hasrank() As Boolean
        Return Not IsNothing(Me.rank)
    End Function

    Public Function getLastName() As String
        Return Me.last_part
    End Function
    Public Function getLastNameAsc(ByVal x As Integer) As Integer
        Return Asc(Mid(Me.last_part, x, 1))
    End Function

    Public Function getFullNameStr() As String

        If Me.haveMiddleName() And Me.haveLastName() Then
            Return Me.getFirstName() & " " & Me.getMiddleName() & " " & Me.getLastName()
        End If

        If Me.haveLastName() And Not Me.haveMiddleName() Then
            Return Me.getFirstName() & " " & Me.getLastName()
        End If

        Return Me.getFirstName()

    End Function
End Class