Public Class TriniDATWordDictionaries
    Inherits List(Of TriniDATWordDictionary)

    Public listid As String

    Public ReadOnly Property getId As String
        Get
            Return Me.listid
        End Get
    End Property


    Public Sub New(ByVal _id As String, ByVal dicts As List(Of TriniDATWordDictionary))
        MyBase.New()

        Me.listid = _id

        'all dictionaries are sorted on their lists length
        Dim dict As TriniDATWordDictionary

        If Not IsNothing(dicts) Then
            'add single dictionaries
            For Each dict In dicts
                Me.Add(dict)
            Next
        End If



    End Sub
    Public ReadOnly Property HasIn(ByVal word As String) As Boolean
        Get


            Dim dict As TriniDATWordDictionary
            Dim retval As Boolean

            For Each dict In Me

                'string match
                retval = dict.HasIn(word)

                If retval = True Then
                    Return True
                End If

            Next

            Return False
        End Get
    End Property
    Public ReadOnly Property Has(ByVal word As String) As Boolean
        Get


            Dim dict As TriniDATWordDictionary
            Dim retval As Boolean

            For Each dict In Me

                'string match
                   retval = dict.Has(word)

                    If retval = True Then
                        Return True
                    End If

            Next

            Return False
        End Get
    End Property
    
    Public Overridable Function hasDictionaryById(ByVal dictid As String) As Boolean
        Return Not IsNothing(Me.getDictionaryById(dictid))
    End Function
    Public Overridable Function getDictionaryById(ByVal dictid As String) As TriniDATWordDictionary
        Dim dict As TriniDATWordDictionary

        For Each dict In Me

            'char match
            If dict.getId() = dictid Then
                Return dict
            End If
        Next

        Return Nothing
    End Function

    Public ReadOnly Property AllCount() As Integer
        Get


            Dim dict As TriniDATWordDictionary
            Dim retval As Integer
            retval = 0

            For Each dict In Me

                'string match
                retval += dict.Count

            Next

            Return retval

        End Get
    End Property
    Public Function getAllDictionaries() As List(Of TriniDATWordDictionary)

        Dim retval As List(Of TriniDATWordDictionary)
        Dim dict As TriniDATWordDictionary

        retval = New List(Of TriniDATWordDictionary)

        For Each dict In Me
            retval.Add(dict)
        Next


        Return retval

    End Function

  

End Class
