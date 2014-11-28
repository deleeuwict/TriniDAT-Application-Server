Imports System
Imports System.Runtime.CompilerServices

<Assembly: SuppressIldasmAttribute()> 

Public MustInherit Class TriniDATDictionaryUnit
    Protected dictid As String
    Protected last_match_index As Integer
    Protected last_match_length As Integer
 
    Public Sub New(ByVal _dictid As String)
        ' Me.kind = COMPILER_DICTIONARY_KIND.DICTIONARY_UNKNOWN
        Me.dictid = _dictid
        Me.last_match_index = 0
        Me.last_match_length = 0

    End Sub

    Protected Sub Msg(ByVal val As String)
        '  Call Me.logger(val)
    End Sub

    Public Sub NullEvent(ByVal val As String)

    End Sub

    Public ReadOnly Property getId As String
        Get
            Return dictid
        End Get
    End Property

    Public ReadOnly Property matched_index As Integer
        Get
            Return Me.last_match_index
        End Get
    End Property

    Public ReadOnly Property matched_wordlength As Integer
        Get
            Return Me.last_match_length
        End Get
    End Property

    Public Overridable ReadOnly Property Has(ByVal val As String) As Boolean
        Get
            Msg("ERROR: dictionary " & Me.getId() & " forgot to override Has(str) (matching against a char dict? (dictionary=" & Me.GetType().FullName.ToString & ")")
            Return False
        End Get
    End Property
    Public Overridable ReadOnly Property Has(ByVal val As Char) As Boolean
        Get
            Msg("ERROR: dictionary " & Me.getId() & " forgot to override Has(char)  (dictionary=" & Me.GetType().FullName.ToString & ")")
            Return False
        End Get
    End Property

    Public MustOverride ReadOnly Property getAll() As List(Of String)
    Public MustOverride ReadOnly Property getChars() As List(Of Char)
    Public MustOverride ReadOnly Property haveContents() As Boolean
    Public MustOverride ReadOnly Property Count() As Long

End Class


