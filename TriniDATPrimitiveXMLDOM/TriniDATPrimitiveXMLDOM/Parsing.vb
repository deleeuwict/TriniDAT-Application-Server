Option Explicit On
Option Compare Text

Imports System.Xml
Imports System.IO
Imports System.Collections.Specialized
Imports TriniDATHTTPTypes


Public Class Parsing



    Public Shared Function Msg(ByVal txt As String)
        Debug.Print("Parser: " & txt)

    End Function

    Public Shared Function isHTLMStandardSingleton(ByVal tagname As String)
        Return (tagname = "area" Or tagname = "base" Or tagname = "br" Or tagname = "col" Or tagname = "command" Or tagname = "embed" Or tagname = "hr" Or tagname = "img" Or tagname = "input" Or tagname = "link" Or tagname = "meta" Or tagname = "param" Or tagname = "source")
    End Function


    Public Shared Function repeatStr(ByVal seed As String, ByVal x As Integer)
        Dim y As Integer
        Dim retval As String

        retval = ""

        If x > 0 Then

            For y = 1 To x
                retval = retval & seed
            Next

        End If

        Return retval
    End Function

    Public Shared Function getTagFromChunk(ByVal block As String) As String
        Dim nodeendpos As Integer
        Dim retval As String

        nodeendpos = findTill(block, {" ", ">"})

        If nodeendpos = -1 Then
            nodeendpos = Len(block)
        End If

        retval = Trim(Mid(block, 1, nodeendpos - 1))

        Return retval
    End Function


    Public Shared Function findTill(ByVal str As String, ByVal chars As Char()) As Integer
        'seek till occurence of multiple chars
        Dim x As Integer
        Dim y As Integer
        Dim currentchar As Char

        For x = 1 To Len(str)

            currentchar = Mid(str, x, 1)

            For y = 0 To chars.Length - 1
                If currentchar = chars(y) Then
                    Return x
                End If
            Next
        Next

        Return -1
    End Function
End Class
