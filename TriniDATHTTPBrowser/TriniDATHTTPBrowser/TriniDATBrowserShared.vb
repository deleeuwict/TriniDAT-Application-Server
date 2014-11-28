Option Explicit On
Imports TriniDATHTTPTypes
Imports TriniDATDictionaries


Public Class TriniDATBrowserShared
    Public Shared ASCIILetters As TriniDATCharDictionary = Nothing
    Public Shared ASCIIUppercase As TriniDATCharDictionary = Nothing
    Public Shared ASCIIDigit As TriniDATCharDictionary = Nothing
    Public Shared ASCIISlashChars As TriniDATCharDictionary = Nothing

    Public Shared ReadOnly Property needIinitialization As Boolean
        Get
            Return IsNothing(TriniDATBrowserShared.ASCIILetters)
        End Get
    End Property


    Public Shared Function CreateDictionaries(Optional ByVal _err_logger As TriniDATHTTPTypes.TriniDATTypeLogger = Nothing) As Boolean

        Try

            ASCIILetters = New TriniDATCharDictionary("ASCIILetters", {ChrW(&H61), ChrW(&H62), ChrW(&H63), ChrW(&H64), ChrW(&H65), ChrW(&H66), ChrW(&H67), ChrW(&H68), ChrW(&H69), ChrW(&H6A), ChrW(&H6B), ChrW(&H6C), ChrW(&H6D), ChrW(&H6E), ChrW(&H6F), ChrW(&H70), ChrW(&H71), ChrW(&H72), ChrW(&H73), ChrW(&H74), ChrW(&H75), ChrW(&H76), ChrW(&H77), ChrW(&H78), ChrW(&H79), ChrW(&H7A), ChrW(&H41), ChrW(&H42), ChrW(&H43), ChrW(&H44), ChrW(&H45), ChrW(&H46), ChrW(&H47), ChrW(&H48), ChrW(&H49), ChrW(&H4A), ChrW(&H4B), ChrW(&H4C), ChrW(&H4D), ChrW(&H4E), ChrW(&H4F), ChrW(&H50), ChrW(&H51), ChrW(&H52), ChrW(&H53), ChrW(&H54), ChrW(&H55), ChrW(&H56), ChrW(&H57), ChrW(&H58), ChrW(&H59), ChrW(&H5A)})
            ASCIIDigit = New TriniDATCharDictionary("ASCIIDigit", {ChrW(&H30), ChrW(&H31), ChrW(&H32), ChrW(&H33), ChrW(&H34), ChrW(&H35), ChrW(&H36), ChrW(&H37), ChrW(&H38), ChrW(&H39)})
            ASCIISlashChars = New TriniDATCharDictionary("ASCIISlashChars", {"/", "\"})

            Return True

        Catch ex As Exception
            If Not IsNothing(_err_logger) Then
                _err_logger("CreateDictionaries err: " & ex.Message)
            End If
            Return False
        End Try

    End Function
End Class
