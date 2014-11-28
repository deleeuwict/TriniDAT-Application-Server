Public Class TriniDATCharDictionaries
    Inherits List(Of TriniDATCharDictionary)

    Public listid As String

    Public ReadOnly Property getId As String
        Get
            Return Me.listid
        End Get
    End Property


    Public Sub New(ByVal _id As String, ByVal dicts As List(Of TriniDATCharDictionary))
        MyBase.New()

        Me.listid = _id

        'all dictionaries are sorted on their lists length
        Dim dict As TriniDATCharDictionary

        If Not IsNothing(dicts) Then
            'add single dictionaries
            For Each dict In dicts
                Me.Add(dict)
            Next
        End If



    End Sub
    Public ReadOnly Property Has(ByVal c As Char) As Boolean
        Get


            Dim dict As TriniDATCharDictionary
            Dim retval As Boolean

            For Each dict In Me

                'string match
                retval = dict.Has(c)

                If retval = True Then
                    Return True
                End If

            Next

            Return False
        End Get
    End Property
    Public ReadOnly Property AllCount() As Integer
        Get


            Dim dict As TriniDATCharDictionary
            Dim retval As Integer
            retval = 0

            For Each dict In Me

                'string match
                retval += dict.Count

            Next

            Return retval

        End Get
    End Property

    Public ReadOnly Property getCharByIndex(ByVal x As Integer) As Char
        Get
            Dim dict As TriniDATCharDictionary
            Dim master_indice As List(Of Char)
            Dim index As Integer

            master_indice = New List(Of Char)

            index = 0

            For Each dict In Me

                index += dict.Count

                master_indice.AddRange(dict.getChars)

                'stop search
                If index > x Then Exit For

            Next

            If x <= master_indice.Count - 1 Then
                Return master_indice.Item(x)
            Else
                Return "A"
            End If


        End Get

    End Property

    Public Overridable Function hasDictionaryById(ByVal dictid As String) As Boolean
        Return Not IsNothing(Me.getDictionaryById(dictid))
    End Function
    Public Overridable Function getDictionaryById(ByVal dictid As String) As TriniDATCharDictionary
        Dim dict As TriniDATCharDictionary

        For Each dict In Me

            'char match
            If dict.getId() = dictid Then
                Return dict
            End If
        Next

        Return Nothing
    End Function
    Public Function getAllDictionaries() As List(Of TriniDATCharDictionary)

        Dim retval As List(Of TriniDATCharDictionary)
        Dim dict As TriniDATCharDictionary

        retval = New List(Of TriniDATCharDictionary)

        For Each dict In Me
            retval.Add(dict)
        Next


        Return retval

    End Function



End Class
