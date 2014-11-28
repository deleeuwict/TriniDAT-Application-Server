Imports System.Collections.Specialized

Public Class TriniDATCharDictionary
    Inherits TriniDATDictionaryUnit

    Private chrs() As Char


    Public Sub New(ByVal ID As String, ByVal val As Char())
        MyBase.new(ID)
        Me.chrs = val
        '  Me.setDictionaryKind(COMPILER_DICTIONARY_KIND.DICTIONARY_CHAR)

        'always zero for chars
        Me.last_match_length = IIf(Me.haveContents() = True, 0, 1)
    End Sub

    Public Overrides ReadOnly Property haveContents() As Boolean
        Get
            Return Not IsNothing(Me.chrs)
        End Get
    End Property

    Public Function getChar(ByVal x As Integer) As Char
        If Me.haveContents() Then
            Return Me.chrs(x)
        Else
            Return Nothing
        End If
    End Function
    Public Overrides ReadOnly Property getChars() As List(Of Char)
        Get
            If Me.haveContents() Then
                Return New List(Of Char)(Me.chrs)
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Overrides ReadOnly Property Has(ByVal val As Char) As Boolean
        Get
            If Me.haveContents Then

                Dim found As Boolean

                Me.last_match_index = Array.IndexOf(chrs, val)

                found = (Me.last_match_index <> -1)

                If found Then
                    Msg("chardict " & Me.getId() & ".dictindex#" & Me.last_match_index.ToString & " matches: " & Me.getChar(Me.last_match_index).ToString)
                End If

                Return found
            Else
                Return False
            End If
        End Get
    End Property

    Public Overrides ReadOnly Property Has(ByVal val As String) As Boolean
        Get
            'match all charachters against dictionary

            If Not IsNothing(chrs) Then

                Dim found As Boolean
                Dim curchar As Char
                Dim x As Integer

                For x = 1 To val.Length
                    curchar = Mid(val, x, 1)

                    Me.last_match_index = Array.IndexOf(Me.chrs, curchar)

                    found = (Me.last_match_index <> -1)

                    If Not found Then
                        Msg("chardict " & Me.getId() & ".dictindex#" & Me.last_match_index.ToString & " word does not match: " & Me.getChar(Me.last_match_index).ToString)
                        Return False
                    End If

                Next

                Return found
            Else
                Return False
            End If
        End Get
    End Property


    Public Shared Operator =(ByVal val1 As TriniDATCharDictionary, ByVal val2 As String)
        Return val1.Has(val2)
    End Operator

    Public Shared Operator <>(ByVal val1 As TriniDATCharDictionary, ByVal val2 As String)
        Return Not val1.Has(val2)
    End Operator

    Public Overrides ReadOnly Property getAll() As List(Of String)
        Get
            Dim retval As List(Of String)
            Dim mychrs As List(Of Char)

            retval = New List(Of String)

            mychrs = Me.getChars()

            If IsNothing(mychrs) Then
                Return Nothing
            Else
                For Each c In mychrs
                    retval.Add(c)
                Next

                Return retval
            End If

        End Get

    End Property
    Public Overrides ReadOnly Property Count() As Long
        Get
            Return Me.chrs.Count
        End Get
    End Property
    ' Public Overrides Function getDictionaryKind() As COMPILER_DICTIONARY_KIND
    '     Return COMPILER_DICTIONARY_KIND.DICTIONARY_CHAR
    ' End Function
End Class
