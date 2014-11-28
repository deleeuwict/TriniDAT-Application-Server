Imports TriniDATDictionaries
Imports System.Collections.Specialized

Public Class TriniDATUserSession
    Inherits List(Of BosswaveApplication)

    Private user As TriniDATUser
    Private sessionId As String
    Private createTS As DateTime
    Private session_vars As StringDictionary

    Public ReadOnly Property CreateDate() As DateTime
        Get
            Return Me.createTS
        End Get
    End Property
  
    Public Property relatedUser As TriniDATUser
        Get
            Return Me.user
        End Get
        Set(ByVal value As TriniDATUser)
            Me.user = value
        End Set
    End Property
    Public Function createApplication(ByVal info As TriniDATRequestInfo) As TriniDATRequestInfo
        MyBase.Add(info.associated_app)
        info.direct_session = Me
        Return info
    End Function
    Public ReadOnly Property AgeInSeconds() As Long
        Get
            Return getAge(DateInterval.Second)
        End Get
    End Property
    Private ReadOnly Property getAge(ByVal interval As DateInterval) As Long
        Get
            Return DateDiff(interval, Now, Me.CreateDate)
        End Get
    End Property
    Public ReadOnly Property ID As String
        Get
            Return Me.sessionId
        End Get
    End Property
    Public Sub New(ByVal sessionid As String)
        Me.createTS = Now()
        Me.relatedUser = Nothing
        Me.sessionId = sessionid
        Me.session_vars = New StringDictionary
    End Sub
    Public ReadOnly Property haveApplicationById(ByVal val As Long) As Boolean
        Get

            For Each memory_app In Me

                If memory_app.Id = val Then
                    Return True
                End If
            Next

            Return False
        End Get
    End Property
    Public ReadOnly Property haveApplications As Boolean
        Get
            Return Me.Count > 0
        End Get
    End Property
    Public ReadOnly Property UserVars As StringDictionary
        Get
            Return Me.session_vars
        End Get
    End Property
    Public ReadOnly Property Application(ByVal val As String) As BosswaveApplication
        Get

            For Each memory_app In Me

                If memory_app.Id = val Then
                    Return memory_app
                End If
            Next

            Return Nothing
        End Get
    End Property
    Public Shared Function generateNewSessionId() As String

        Dim retval As String
        Dim session_path As String
        session_path = GlobalSetting.getSessionRoot()

        retval = ""
        Do While IO.Directory.Exists(session_path & retval)
            retval = TriniDATUserSession.generateUniqueSessionString()
        Loop

        Return retval

    End Function

    Private Shared Function generateUniqueSessionString() As String

        Dim new_id As String
        Dim myrnd As Random
        myrnd = New Random

        Dim ClassicASCIIUppercase As TriniDATCharDictionary
        Dim ClassicASCIILowercase As TriniDATCharDictionary
        Dim ClassicASCIIDigit As TriniDATCharDictionary
        Dim ASCII_ALL As TriniDATCharDictionaries
        Dim ascii_table_count As Integer
        Dim index As Integer
        Dim generated_id_length As Integer = 25

        ClassicASCIILowercase = New TriniDATCharDictionary("ClassicASCIILowercase", {ChrW(&H61), ChrW(&H62), ChrW(&H63), ChrW(&H64), ChrW(&H65), ChrW(&H66), ChrW(&H67), ChrW(&H68), ChrW(&H69), ChrW(&H6A), ChrW(&H6B), ChrW(&H6C), ChrW(&H6D), ChrW(&H6E), ChrW(&H6F), ChrW(&H70), ChrW(&H71), ChrW(&H72), ChrW(&H73), ChrW(&H74), ChrW(&H75), ChrW(&H76), ChrW(&H77), ChrW(&H78), ChrW(&H79), ChrW(&H7A)})
        ClassicASCIIUppercase = New TriniDATCharDictionary("ClassicASCIIUppercase", {ChrW(&H41), ChrW(&H42), ChrW(&H43), ChrW(&H44), ChrW(&H45), ChrW(&H46), ChrW(&H47), ChrW(&H48), ChrW(&H49), ChrW(&H4A), ChrW(&H4B), ChrW(&H4C), ChrW(&H4D), ChrW(&H4E), ChrW(&H4F), ChrW(&H50), ChrW(&H51), ChrW(&H52), ChrW(&H53), ChrW(&H54), ChrW(&H55), ChrW(&H56), ChrW(&H57), ChrW(&H58), ChrW(&H59), ChrW(&H5A)})
        ClassicASCIIDigit = New TriniDATCharDictionary("ClassicASCIIDigit", {ChrW(&H30), ChrW(&H31), ChrW(&H32), ChrW(&H33), ChrW(&H34), ChrW(&H35), ChrW(&H36), ChrW(&H37), ChrW(&H38), ChrW(&H39)})

        ASCII_ALL = New TriniDATCharDictionaries("ASCII_AZ", New List(Of TriniDATCharDictionary)({ClassicASCIILowercase, ClassicASCIIUppercase, ClassicASCIIDigit}))

        ascii_table_count = ASCII_ALL.AllCount - 1

        new_id = ""
        'getCharByIndex
        For x = 0 To generated_id_length

            index = myrnd.Next(0, ascii_table_count)
            new_id &= ASCII_ALL.getCharByIndex(index)

        Next

        'multiple calls to this function without sleep will generate duplicates
        Threading.Thread.Sleep(45)

        Debug.Print(new_id)
        Return new_id
    End Function

End Class

