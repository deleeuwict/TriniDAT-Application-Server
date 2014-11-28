Option Compare Text

Public Class TriniDATWordDictionary
    Inherits TriniDATDictionaryUnit


    Private lst As List(Of String)
    Private indice_by_len As List(Of Integer)
    'Private logger
    Public Sub New(ByVal ID As String, ByVal val As List(Of String))
        MyBase.new(ID)
        'Me.setDictionaryKind(COMPILER_DICTIONARY_KIND.DICTIONARY_WORDS)
        Me.lst = Nothing
        Me.indice_by_len = Nothing
        Me.setWordList(val)
        Call sortByLength()
    End Sub
    Public Sub setWordList(ByVal val As List(Of String))
        Me.lst = val
    End Sub

    Public ReadOnly Property getWordList() As List(Of String)
        Get
            Return Me.lst
        End Get
    End Property

    Public Sub setLengthIndex(ByVal val As List(Of Integer))
        Me.indice_by_len = val
    End Sub

    Public ReadOnly Property getLengthIndice() As List(Of Integer)
        Get
            Return Me.indice_by_len
        End Get
    End Property
    Public Overrides ReadOnly Property Count() As Long
        Get
            Return Me.lst.Count
        End Get
    End Property
    Public Sub sortByLength()
        'sort by smallest -> lengthiest

        Dim x As Integer
        Dim sorted_list As List(Of String)
        Dim max_word_len As Integer
        Dim word As String

        If Not Me.haveContents() Then
            Me.indice_by_len = New List(Of Integer)(0)
            Exit Sub
        End If

        Me.indice_by_len = New List(Of Integer)()

        'find longest wordcount
        For Each word In Me.lst
            x = Len(word)
            If x > max_word_len Then
                max_word_len = x
            End If
        Next

        sorted_list = New List(Of String)(Me.lst.Count)
        'zero based array vs normal count - init index 0 first
        Me.indice_by_len.Add(0)

        For curlen = 1 To max_word_len

            'pad array to match long count order
            Me.indice_by_len.Add(-1)

            For x = 0 To lst.Count - 1

                If Len(lst(x)) = curlen Then

                    Dim lastindex As Integer

                    sorted_list.Add(lst(x))
                    lastindex = sorted_list.Count - 1

                    If Me.indice_by_len(curlen) = -1 Then
                        'note the beginning index of this length class
                        Me.indice_by_len(curlen) = sorted_list.Count - 1
                        sorted_list(lastindex) = sorted_list(lastindex) ' & " | set len." & curlen.ToString & ".index = #" & Me.indice_by_len(curlen).ToString
                    End If

                End If


            Next x

        Next curlen

        Me.setWordList(sorted_list)
    End Sub

    Public ReadOnly Property getLengthIndexByWord(ByVal word_length As Integer) As Integer
        Get

            If Me.getWordCount() = 1 Then Return 0

            If Me.haveLengthIndex(word_length) Then
                Return Me.indice_by_len(word_length)
            Else
                Return -1
            End If

        End Get
    End Property
    Public Sub Add(ByVal val As String)

        If Not Me.haveContents Then
            'create new list
            Me.lst = New List(Of String)
        End If

        Me.lst.Add(val)
        Me.sortByLength()

    End Sub
    Public Overrides ReadOnly Property haveContents() As Boolean
        Get
            Return Not IsNothing(Me.lst)
        End Get
    End Property
    Public Overrides ReadOnly Property getChars() As List(Of Char)
        Get
            Return Nothing
        End Get
    End Property

    Public Function listinitialized() As Boolean
        Return Not IsNothing(Me.lst)
    End Function
    Public ReadOnly Property getWordCount() As Integer
        Get

            If Not Me.listinitialized Then
                Return 0
            Else
                Return Me.lst.Count
            End If
        End Get
    End Property

    Public ReadOnly Property haveLengthIndex(ByVal word_length As Integer) As Boolean
        Get

            If Me.listinitialized Then

                If Me.getWordCount = 1 Then Return True

                If Not IsNothing(indice_by_len) Then
                    Return (Me.indice_by_len.Count > word_length)
                Else
                    Return False
                End If
            Else
                Return False
            End If
        End Get
    End Property

    Public Shared Operator =(ByVal val1 As TriniDATWordDictionary, ByVal val2 As String)
        Return val1.Has(val2)
    End Operator

    Public Shared Operator <>(ByVal val1 As TriniDATWordDictionary, ByVal val2 As String)
        Return Not val1.Has(val2)
    End Operator

    Public Overrides ReadOnly Property Has(ByVal val As String) As Boolean
        Get
            If IsNothing(Me.getWordList()) Then
                Msg("Keyword set is not initialized.")
            End If

            Dim target_word_len As Integer
            Dim len_index As Integer
            Dim x As Integer


            target_word_len = Len(val)
            len_index = Me.getLengthIndexByWord(target_word_len)

            If len_index = -1 Then
                Return False
            End If

            'compare start with equal length words

            For x = len_index To Me.getWordList().Count - 1

                If Me.getWordList().Item(x) = val Then
                    Msg("TriniDATWordDictionary," & Me.getId() & ": dictindex#" & x.ToString & " matches: " & Me.getWordList().Item(x))
                    Me.last_match_index = x
                    Me.last_match_length = target_word_len
                    Return True
                End If

            Next x

            'nothing found
            Return False
        End Get
    End Property
    Public Sub ReplaceAt(ByVal x As Integer, ByVal val As String)
        Me.lst(x) = val
    End Sub
    Public Function RemoveWord(ByVal x As String)

        If Me.haveContents Then
            Me.lst.Remove(x)
            Return True
        Else
            Return False
        End If

    End Function
    Public ReadOnly Property HasIn(ByVal val As String, Optional ByVal extra_not_a_sentence_condition As String = "") As Integer
        'Instr based match
        'retval:    -1 = error
        '               0 = not found
        '             >0= match

        Get
            If Not Me.haveContents() Then
                Msg("HasIn err: Keyword set is not initialized.")
                Return -1
            End If

            'don't bother if its a single word
            If InStr(val, " ") = 0 And InStr(val, extra_not_a_sentence_condition) = 0 Then
                Dim retval As Boolean
                retval = Me.Has(val)
                If retval Then
                    Return 1
                Else
                    Return 0
                End If
            End If

            Dim x As Integer
            Dim found_pos As Integer
            Dim word As String
            Dim match_len As Long

            match_len = Len(val)

            'compare start with equal length words

            For Each word In Me.lst

                If Len(word) > match_len Then
                    found_pos = InStr(word, val, CompareMethod.Text)
                Else
                    found_pos = InStr(val, word, CompareMethod.Text)
                End If

                If found_pos Then
                    Msg("Stringdict " & Me.getId() & ".dictitem hasin match: " & word & " @ pos " & found_pos.ToString)
                    Me.last_match_length = match_len
                    Return found_pos
                End If

            Next word

            'nothing found
            Return 0
        End Get
    End Property
    Public ReadOnly Property HasStart(ByVal val As String) As Long
        'Left() based match
        'retvals:   
        '               -1 = not found
        '             >0= index

        Get
            If Not Me.haveContents() Then
                Msg("HasIn err: Keyword set is not initialized.")
                Return -1
            End If


            Dim x As Integer
            Dim left_match As Boolean
            Dim word As String
            Dim match_len As Long

            match_len = Len(val)

            'compare start with equal length words

            For x = 0 To Me.lst.Count - 1

                word = Me.lst(x)

                If Len(word) > match_len Then
                    left_match = (Left(word, match_len) = val)
                Else
                    left_match = (Left(val, match_len) = word)
                End If

                If left_match Then
                    Msg("Stringdict " & Me.getId() & ".dictindex#" & x.ToString & " hasin match: " & Me.getWordList().Item(x))
                    Me.last_match_index = x
                    Me.last_match_length = Len(val)
                    Return x
                End If

            Next x

            'nothing found
            Return -1
        End Get
    End Property
    Public Overrides ReadOnly Property getAll() As List(Of String)
        Get
            Return Me.getWordList()
        End Get
    End Property

    Public Shadows Function ToString(Optional ByVal delim As String = "") As String
        Dim output_buffer As String
        Dim x As Integer

        output_buffer = ""

        For x = 0 To Me.lst.Count - 1
            output_buffer &= Me.lst(x) & delim
        Next

        Return output_buffer
    End Function
    '  Public Overrides Function getDictionaryKind() As COMPILER_DICTIONARY_KIND
    '     Return COMPILER_DICTIONARY_KIND.DICTIONARY_WORDS
    '   End Function
End Class

