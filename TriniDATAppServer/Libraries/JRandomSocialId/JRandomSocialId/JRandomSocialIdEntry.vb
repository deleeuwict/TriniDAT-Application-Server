Option Compare Text
Public Class JRandomSocialIdEntry
    'Note: only public Fields are included in JSON serialization
    Public FirstName As String
    Public LastName As String
    Public StreetAddress As String
    Public ZipCode As String
    Public CityName As String
    Public StateShort As String
    Public StateLong As String
    Public CountryISO As String
    Public CountryLong As String
    Public birthDay As Integer
    Public birthMonth As Integer
    Public birthYear As Integer

    Public Sub setEntry(ByVal _FirstName As String, ByVal _LastName As String, ByVal _StreetAddress As String, ByVal _ZipCode As String, ByVal _CityName As String, ByVal _StateShort As String, ByVal _StateLong As String, ByVal _CountryISO As String, ByVal _CountryLong As String, ByVal _birthDay As Integer, ByVal _birthMonth As Integer, ByVal _birthYear As Integer)
        Me.FirstName = _FirstName
        Me.LastName = _LastName
        Me.birthDay = _birthDay
        Me.birthMonth = _birthMonth
        Me.birthYear = _birthYear
        Me.ZipCode = _ZipCode
        Me.StateLong = _StateLong
        Me.StateShort = _StateShort
        Me.StreetAddress = _StreetAddress
        Me.CityName = _CityName
        Me.CountryISO = _CountryISO
        Me.CountryLong = _CountryLong


    End Sub
    Public Function getFirstName() As String
        Return Me.FirstName
    End Function
    Public Function getStateLong() As String
        Return Me.StateLong
    End Function
    Public Function getStreetAddress() As String
        Return Me.StreetAddress
    End Function
    Public Function getLastName() As String
        Return Me.LastName
    End Function
    Public Function getZIPCode() As String
        Return Me.ZipCode
    End Function
    Public Function getCity() As String
        Return Me.CityName
    End Function
    Public Function getCountryLong() As String
        Return Me.CountryLong
    End Function
    Public Function getCountryISO() As String
        Return Me.CountryISO
    End Function
    Public Function getBirthDay() As Integer
        Return Me.birthDay
    End Function

    Public Function getBirthMonth() As Integer
        Return Me.birthMonth
    End Function

    Public Function getBirthYear() As Integer
        Return Me.birthYear
    End Function
    Public Sub setFirstName(ByVal val As String)
        Me.FirstName = val
    End Sub
    Public Sub setStreetAddress(ByVal val As String)
        Me.StreetAddress = val
    End Sub
    Public Sub setStateLong(ByVal val As String)
        Me.StateLong = val
    End Sub
    Public Sub setCity(ByVal val As String)
        Me.CityName = val
    End Sub
    Public Sub setZIPCode(ByVal val As String)
        Me.ZipCode = val
    End Sub
    Public Sub setLastName(ByVal val As String)
        Me.LastName = val
    End Sub
    Public Sub setCountryISO(ByVal val As String)
        Me.CountryISO = val
    End Sub
    Public Sub setCountryLong(ByVal val As String)
        Me.CountryLong = val
        If InStr(val, "United States") Then
            Me.setCountryISO("us")
        End If
    End Sub

    Public Sub setBirthDay(ByVal val As Integer)
        Me.birthDay = val
    End Sub
    Public Sub setBirthMonth(ByVal val As Integer)
        Me.birthMonth = val
    End Sub
    Public Sub setBirthYear(ByVal val As Integer)
        Me.birthYear = val
    End Sub
    Public Function getFormatedEntry(Optional ByVal delim As String = "|") As String
        'Public Sub setEntry(ByVal _FirstName As String, ByVal _LastName As String, ByVal _StreetAddress As String, ByVal _ZipCode As String, ByVal _CityName As String, ByVal _StateShort As String, ByVal _StateLong As String, ByVal _CountryISO As String, ByVal _CountryLong As String, ByVal _birthDay As Integer, ByVal _birthMonth As Integer, ByVal _birthYear As Integer)
        'must be in the same order as declared in setEntry function header
        Return Me.FirstName & delim & Me.LastName & delim & Me.StreetAddress & delim & Me.ZipCode & delim & Me.CityName & delim & Me.StateShort & delim & Me.StateLong & delim & Me.CountryISO & delim & Me.CountryLong & delim & Me.birthDay.ToString & delim & Me.birthMonth.ToString & delim & Me.birthYear.ToString
    End Function

    Public Shared Operator =(ByVal val1 As JRandomSocialIdEntry, ByVal val2 As JRandomSocialIdEntry) As Boolean

        If IsNothing(val1) Or IsNothing(val2) Then Return False

        Return (val1.getFirstName() = val2.getFirstName() And val1.getLastName() = val2.getLastName() And val1.getBirthYear() = val2.getBirthYear())
    End Operator

    Public Shared Operator <>(ByVal val1 As JRandomSocialIdEntry, ByVal val2 As JRandomSocialIdEntry) As Boolean
        Return Not (val1 = val2)
    End Operator


    Public Sub New()

    End Sub
End Class
