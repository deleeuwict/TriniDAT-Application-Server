Option Explicit On
Option Compare Text
Imports TriniDATServerTypes
Imports System.Globalization
Imports System.Web
Imports System.Runtime.CompilerServices
<Assembly: SuppressIldasmAttribute()> 

Public Class TriniDATServerLicense

    Private _license_model As TRINIDAT_SERVER_LICENSE

    Public Sub New(ByVal _license_kind As TRINIDAT_SERVER_LICENSE)
        Me._license_model = _license_kind
    End Sub

    Public Sub New()
        Me._license_model = TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
    End Sub

    Public Function getLicenseName() As String

        Select Case Me.CurrentLicense

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE 'aka start-up.
                Return "Free Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                Return "Corporate Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                Return "University Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                Return "Grand University Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED
                Return "XTreme Edition"

        End Select

        Return "Error"
    End Function
    Public ReadOnly Property NextVersion() As Integer
        'Note: ALSO UPDATE SHADDOW FUNCTION Verify.
        'get thread count for current license.
        Get

            Select Case Me.CurrentLicense

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
                    Return TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                    Return TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                    Return TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                    Return TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED
                    Return TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED

            End Select

            Return 0
        End Get
    End Property

    Public ReadOnly Property getT() As Integer
        'Note: ALSO UPDATE SHADDOW FUNCTION Verify.
        'get thread count for current license.
        Get

            Select Case Me.CurrentLicense

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
                    Return 4

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                    Return 16

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                    Return 16

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                    Return 32

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED
                    Return 65

            End Select

            Return "Error"
        End Get
    End Property

    Public Function getSelfPath() As String
        Return Reflection.Assembly.GetExecutingAssembly().Location
    End Function
    Public Function Verify() As Boolean
        'get thread count for current license.
        'verify the license name and the max. thread count that should have been returned by the license model.
        Select Case Me.CurrentLicense

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
                Return Me.getT() = 4 And Me.getLicenseName() = "Free Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                Return Me.getT() = 16 And Me.getLicenseName() = "Corporate Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                Return Me.getT() = 16 And Me.getLicenseName() = "University Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                Return Me.getT() = 32 And Me.getLicenseName() = "Grand University Edition"

            Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED
                Return Me.getT() = 65 And Me.getLicenseName = "XTreme Edition"

        End Select

        Return False
    End Function
    Public Function getCurrentLanguageName() As String
        Dim cc As CultureInfo

        cc = System.Globalization.CultureInfo.CurrentCulture

        Return cc.EnglishName
    End Function

    Public Function getVerificationURL(ByVal w As String, ByVal e As String, ByVal t As String, ByVal l As String) As String

        Dim all_parameters As String
        Dim updateScript_URL As String

        all_parameters = "?w=" & HttpUtility.UrlEncode(w) 'GlobalSetting.getWindowsSerial()
        all_parameters &= "&e=" & HttpUtility.UrlEncode(e) 'md5_exec
        all_parameters &= "&type=" & HttpUtility.UrlEncode(t) 'md5_types
        all_parameters &= "&l=" & HttpUtility.UrlEncode(l) 'md5 of server license dll.

        If Not Me.isEnglish Then
            all_parameters &= "&operatingsystemlanguage=" & HttpUtility.UrlEncode(Me.getCurrentLanguageName()) 'md5_types
        End If

        updateScript_URL = "http://www.deleeuwict.nl/trinidat/updater/update.php" & all_parameters

        Return updateScript_URL

    End Function

    Private Function isEnglish() As Boolean
        Return Me.getCurrentLanguageName = "English"
    End Function
    Private Function isEuropeanUser() As Boolean
        Return False
    End Function

    Public Function getPrice(ByVal for_license As TRINIDAT_SERVER_LICENSE) As String

        Dim current_currency As String

        'Internationaal
        If Not Me.isEuropeanUser Then

            'show pricing in US dollars
            current_currency = "$ "

            Select Case for_license

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
                    Return current_currency & "0"

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                    Return current_currency & "7999" '~ eu 5000 +  23% BTW 1150  = 6150 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                    Return current_currency & "27000" 'EU 16000 + eu 3680   23% BTW = 19680 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                    Return current_currency & "42000" 'EU 32000 + eu 7360 23% BTW = 39360 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED 'government
                    Return current_currency & "83300" 'EU 48000 eu + eu 11040 23% BTW = 59040 eu

            End Select

        Else
            'Europa.
            current_currency = "€ "

            Select Case for_license

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE
                    Return current_currency & "0"

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE
                    Return current_currency & "6150" '~ eu 5000 +  23% BTW 1150  = 6150 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_MEDIUM
                    Return current_currency & "19680" 'EU 16000 + eu 3680   23% BTW = 19680 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_CORPORATE_UNIVERSITY_LARGE
                    Return current_currency & "39360" 'EU 32000 + eu 7360 23% BTW = 39360 eu

                Case TRINIDAT_SERVER_LICENSE.T_LICENSE_LIBERATED 'government
                    Return current_currency & "59040" 'EU 48000 eu + eu 11040 23% BTW = 59040 eu

            End Select

        End If

        Return "error!"
    End Function

    Public ReadOnly Property CurrentLicense As TRINIDAT_SERVER_LICENSE
        Get
            Return Me._LICENSE_MODEL
        End Get
    End Property
End Class

