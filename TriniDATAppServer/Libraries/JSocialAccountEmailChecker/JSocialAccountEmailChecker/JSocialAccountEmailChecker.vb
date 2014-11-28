Imports System.Collections.Specialized
Imports System.Text.RegularExpressions
Imports TriniDATServerTypes
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public Class JSocialAccountEmailChecker
    Inherits JTriniDATWebService
    'BlingServer.CONFIG_ROOT_PATH



    Public Sub New()
        MyBase.New()
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
        GetIOHandler().Configure(http_events)

        Return True
    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function
    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        If obj.ObjectTypeName = "JSocialAccountEmailChecker" And obj.Directive = "VALIDATENAMEEMAIL" Then
            Dim parsed_result As JEmailcheckResult
            Dim nameinfo As Object  '=JNameCheckResult
            Dim resp As JSONObject

            ' Msg("Starting name/email ranking of '" & obj.Tag & "' ")
            nameinfo = obj.Attachment ' CType(obj.Attachment, JNameCheckResult)

            parsed_result = Me.doEmailValidationWithNamingRank(obj.Tag, nameinfo)
            parsed_result.Tag = nameinfo 'can be anything///

            'send reply
            resp = New JSONObject
            resp.ObjectType = "JEmailcheckResult"
            resp.Sender = Me
            resp.Attachment = parsed_result

            'send reply
            Me.getMailProvider().Send(resp)
            Return False
        End If

        If obj.ObjectTypeName = "JOmega" And obj.Directive = "FLUSH_OUTPUT" Then
            Return False
        End If

        Return False
    End Function

    'emails passed should at least have a dot and @ char in it.

    Public Sub doValidate(ByVal email_str As String)
        Dim retval As JEmailcheckResult

        retval = New JEmailcheckResult

        retval.rank = "discard"


    End Sub

    Public Function doEmailValidationWithNamingRank(ByVal raw_email_str As String, ByVal nameinfo As Object) As JEmailcheckResult 'nameinfo As JNameCheckResult
        Dim retval As JEmailcheckResult
        Dim ascii_code As Integer
        Dim BADCHAR As Boolean

        retval = New JEmailcheckResult

        retval.rank = "ego"
        retval.setUnparsed(raw_email_str)
        retval.setparsed(raw_email_str)

        'clean up
        For x = 1 To retval.email.Length
            '45, 46 48-57, 64 - 90, 97-122


            BADCHAR = True
            ascii_code = retval.getEmailAsc(x)

            If ascii_code = 127 Then
                'USED AS PLACEHOLDER - WILL BECOME REPLACED
                BADCHAR = False
            ElseIf ascii_code = 45 And x > 1 Then
                BADCHAR = False
            ElseIf ascii_code = 46 And x > 1 Then
                BADCHAR = False
            ElseIf ascii_code >= 48 And ascii_code <= 57 And x > 1 Then
                BADCHAR = False
            ElseIf ascii_code >= 64 And ascii_code <= 90 Then
                BADCHAR = False
            ElseIf ascii_code >= 97 And ascii_code <= 122 Then
                BADCHAR = False
            End If

            If BADCHAR Then
                retval.email = Replace(retval.email, Chr(ascii_code), Chr(127))
            End If
        Next

        retval.email = Replace(retval.email, Chr(127), "")
        retval.email = Trim(retval.email).ToLower


        If nameinfo.count = 2 Then
            If nameinfo.getFirstNameLength() >= 3 And nameinfo.getLastName().Length >= 3 And Not nameinfo.specialchar_in_name Then
                'FIRSt-LAST Name
                If InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 And InStr(retval.email.ToLower, nameinfo.getLastName().ToLower) > 0 Then
                    retval.rank = "high"
                ElseIf InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 Or InStr(retval.email.ToLower, nameinfo.getLastName().ToLower) > 0 Then
                    retval.rank = "medium"
                End If

            End If
        ElseIf nameinfo.count = 3 And Not nameinfo.specialchar_in_name Then
            'properly capitalized FIRSt-LAST Name 

            If InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 And InStr(retval.email.ToLower, nameinfo.getMiddleName().ToLower) > 0 And InStr(retval.email.ToLower, nameinfo.getLastName().ToLower) > 0 Then
                retval.rank = "elite"
            ElseIf InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 And InStr(retval.email.ToLower, nameinfo.getMiddleName().ToLower) > 0 Then
                retval.rank = "high"
            ElseIf InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 Or InStr(retval.email.ToLower, nameinfo.getMiddleName().ToLower) > 0 Then
                retval.rank = "medium"
            End If
        Else

            'check e-mail rank
            If InStr(retval.email.ToLower, nameinfo.getFirstName().ToLower) > 0 Then
                retval.rank = "cocky"
            End If
        End If

        Return retval

    End Function
End Class


Public Class JEmailcheckResult

    Public email As String
    Public rank As String
    Public specialchar_in_name As Boolean
    Public unparsed As String

    'because this object is passed around a lot among different classes
    'it will come handy for this object to transport addiditional objects.
    Private email_tag As Object

    Public Property Tag()
        Get
            Return Me.email_tag
        End Get
        Set(ByVal Val As Object)
            Me.email_tag = Val
        End Set
    End Property

    Public ReadOnly Property hasTag() As Boolean
        Get
            Return Not IsNothing(Me.Tag)
        End Get
    End Property

    Public Sub setUnparsed(ByVal Val As String)
        Me.unparsed = Val
    End Sub

    Public Sub setparsed(ByVal Val As String)
        Me.email = Val
    End Sub
    Public Function getEmailAsc(ByVal x As Integer) As Integer
        Return Asc(Mid(Me.email, x, 1))
    End Function

    Public Function isValid() As Boolean
        Dim reg As Regex
        reg = New Regex("^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$")
        Return reg.IsMatch(Me.email)
    End Function

    Public ReadOnly Property getEmail() As String
        Get
            Return Me.email
        End Get
    End Property

    Public ReadOnly Property getRank() As String
        Get
            Return Me.rank
        End Get
    End Property

    Public ReadOnly Property hasRank() As Boolean
        Get
            Return Not IsNothing(Me.rank)
        End Get
    End Property

    Public Function getEmailIfValid(Optional ByVal invalid_retval As Object = Nothing) As Object

        If Me.isValid() Then
            Return Me.email
        Else
            Return invalid_retval
        End If

    End Function
End Class