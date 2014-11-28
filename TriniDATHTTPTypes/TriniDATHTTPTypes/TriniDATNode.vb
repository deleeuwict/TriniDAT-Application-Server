Option Explicit On
Option Compare Text
Imports System.Collections
Imports System.Collections.Specialized

Public Class TriniDATNode

    Private blingid As Integer
    Private tagName As String
    Public html_src_startpos As Integer
    Public html_src_endpos As Integer
    Public xml_str_startpos As Integer
    Public xml_str_endpos As Integer
    Private charsWidth As Integer
    Public foundEndTag As Boolean
    Public Issingleton As Boolean
    Private tag_attributes As StringDictionary
    Private tag_attribute_container_char As Char

    Public ReadOnly Property getId() As Integer
        Get
            Return Me.blingid
        End Get
    End Property

    Public Sub setID(ByVal val As Integer)
        Me.blingid = val
    End Sub


    Public Sub New(ByVal _charsWidth As Integer)
        Me.Issingleton = False
        Me.tag_attributes = Nothing
        Me.charsWidth = _charsWidth + 1
        Me.tag_attribute_container_char = Chr(0)
    End Sub

    Public ReadOnly Property getTagName() As String
        Get
            Return Me.tagName
        End Get
    End Property

    Public Sub setTagName(ByVal val As String)
        Me.tagName = val
    End Sub


    Public Function Msg(ByVal txt As String)
        Debug.Print("NodeInfo " & Me.blingid.ToString & "." & Me.tagName & ": " & txt)

    End Function
    Public Function attributeContainerChar_IsSet() As Boolean
        Return Not (Asc(Me.getAttributeContainerchar()) = 0)
    End Function

    Public ReadOnly Property hasAttributeList() As Boolean
        Get
            Return Not IsNothing(Me.tag_attributes)
        End Get
    End Property

    Public ReadOnly Property hasAttribute(ByVal val As String) As Boolean
        Get
            If Me.hasAttributeList() Then
                Return Me.tag_attributes.ContainsKey(val)
            End If

            Return False
        End Get
    End Property

    Public ReadOnly Property getOrAttribute(ByVal key1 As String, ByVal key2 As String) As String
        Get
            Dim retval As String
            retval = Me.getAttribute(key1)

            If retval <> "" Then
                Return retval
            Else
                retval = Me.getAttribute(key2)
                Return retval
            End If

        End Get
    End Property

    Public ReadOnly Property getAttribute(ByVal val As String) As String
        Get
            Dim retval As String

            retval = ""

            If Me.hasAttributeList() Then
                If Me.tag_attributes.ContainsKey(val) Then
                    retval = Me.tag_attributes(val)
                End If
            End If

            Return retval
        End Get
    End Property
    Public Function setAttribute(ByVal name As String, ByVal val As String) As Boolean

        Dim retval As String

        retval = ""

        Try

            If Me.hasAttributeList() Then
                If Me.tag_attributes.ContainsKey(name) Then
                    Me.tag_attributes(name) = val
                Else
                    'create new 
                    Me.tag_attributes.Add(name, val)
                End If
            End If

            Return True

        Catch ex As Exception
            Msg("setAttribute err: " & ex.Message)
            Return False
        End Try

    End Function
    Public Sub setAttributes(ByVal val As StringDictionary, ByVal _tag_attribute_container_char As Char)
        Me.tag_attributes = val
        Me.setAttributeContainerchar(_tag_attribute_container_char)
    End Sub

    Public Sub setAttributeContainerchar(ByVal val As Char)
        Me.tag_attribute_container_char = val
    End Sub
    Public ReadOnly Property getAttributeContainerchar() As Char
        Get
            Return Me.tag_attribute_container_char
        End Get
    End Property

    Public ReadOnly Property getAttributes() As StringDictionary
        Get
            Return Me.tag_attributes
        End Get
    End Property

    Public Function renderXHTMLTag(Optional ByVal closetag As Boolean = False) As String

        Dim retval As String

        retval = "<" & tagName & " " & Me.formatAttribs()

        If closetag = True Then
            'singleton tag
            retval &= "/>"
        Else
            retval &= ">"
        End If

        retval &= " <!-- blingid " & Me.blingid.ToString & " -->"
        Return retval
    End Function
    Public Function renderXHTMLTag_debugmode(Optional ByVal closetag As Boolean = False) As String
        'called when error
        'ensure no empty tags

        Dim retval As String

        If IsNothing(Me.tagName) Then
            tagName = "BLINGEMPTY"
        ElseIf Me.tagName = "" Then
            tagName = "BLINGEMPTY"
        End If

        retval = "<" & tagName & " " & Me.formatAttribs()

        If closetag = True Then
            'singleton tag
            retval &= "/>"
        Else
            retval &= ">"
        End If

        retval &= " <!-- empty tag blingid " & Me.blingid.ToString & " -->"
        Return retval
    End Function



    Public Function formatAttribs() As String
        Return "blingid=" & Chr(34) & blingid.ToString & Chr(34) & " htmlpos=" & Chr(34) & html_src_startpos.ToString & Chr(34)
    End Function

    Public ReadOnly Property getAsEndTag() As String
        Get
            Return "</" & Me.tagName & ">"
        End Get
    End Property

    Public ReadOnly Property getCharsWidth() As Integer
        Get
            Return Me.charsWidth
        End Get
    End Property

End Class
