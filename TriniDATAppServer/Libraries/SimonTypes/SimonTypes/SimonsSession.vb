Option Compare Text
Option Explicit On
Imports System.Globalization
Imports System.Xml
Imports System.Speech
Imports System.Speech.Synthesis
Imports System.Windows.Forms
Imports System.drawing
Imports TriniDATServerTypes

Public Class SimonsSession
    Private doc As XDocument
    Private culture_obj As CultureInfo
    Private spk As SpeechSynthesizer
    Private ConsoleTxt As RichTextBox
    Private ConsoleColor As Color
    Private current_console_context As SimonConsoleState
    Private current_servermode As TRINIDAT_SERVERMODE
    Private server_listening As Boolean
    Protected debug_xml As XDocument
    Private available_debugging_frames As SimonDebugFrames
    Private current_debug_frameid As Long
    Protected object_serverhost As String
    Protected object_serverport As Integer

    Public Sub New(ByVal _culture As CultureInfo, ByVal _speaker As SpeechSynthesizer, ByVal _ConsoleTxt As RichTextBox, ByVal _state As SimonConsoleState, ByVal _console_text_color As Color, ByVal _servermode As TRINIDAT_SERVERMODE, Optional ByVal _debug_app As XDocument = Nothing, Optional ByVal _current_debug_framelist As SimonDebugFrames = Nothing, Optional ByVal _current_debug_frameid As Long = 0)
        Me.culture_obj = _culture
        Me.spk = _speaker
        Me.ConsoleTxt = _ConsoleTxt
        Me.current_console_context = _state
        Me.ConsoleColor = _console_text_color
        Me.debug_xml = _debug_app
        Me.current_servermode = _servermode

        If IsNothing(_current_debug_framelist) Then
            'init available debugging_frames
            _current_debug_framelist = New SimonDebugFrames
        End If

        Me.available_debugging_frames = _current_debug_framelist
        Me.current_debug_frameid = _current_debug_frameid
    End Sub
    Public Property ServerIsOn As Boolean
        Get
            Return Me.server_listening
        End Get
        Set(ByVal value As Boolean)
            Me.server_listening = value
        End Set
    End Property
    Public Property ObjectServer As String
        Get
            Return Me.object_serverhost
        End Get
        Set(ByVal value As String)
            Me.object_serverhost = value
        End Set
    End Property
    Public ReadOnly Property ServerMode As TRINIDAT_SERVERMODE
        Get
            Return Me.current_servermode
        End Get
    End Property
    Public ReadOnly Property haveObjectServer As Boolean
        Get
            If Not IsNothing(Me.object_serverhost) Then
                Return Len(Me.object_serverhost) > 0
            Else
                Return False
            End If
        End Get
    End Property
    Public Property ObjectServerPort As Integer
        Get
            Return Me.object_serverport
        End Get
        Set(ByVal value As Integer)
            Me.object_serverport = value
        End Set
    End Property
    Public ReadOnly Property haveObjectServerPort As Boolean
        Get
            Return Me.ObjectServerPort > 0
        End Get
    End Property
    Public ReadOnly Property currentDebugFrameId As Long
        Get
            Return Me.current_debug_frameid
        End Get
    End Property
    Public ReadOnly Property currentDebuggingFrame As SimonDebugFrame
        Get
            Return Me.DebuggingFrames.GetById(Me.CurrentDebugFrameId)
        End Get
    End Property
    Public ReadOnly Property haveDebugFrames As Boolean
        Get
            Return Not IsNothing(Me.available_debugging_frames)
        End Get
    End Property

    Public ReadOnly Property haveDebugApp As Boolean
        Get
            Return Not IsNothing(Me.CurrentDebugApplication)
        End Get
    End Property
    Public ReadOnly Property DebuggingFrames As SimonDebugFrames
        Get
            Return Me.available_debugging_frames
        End Get
    End Property

    Public ReadOnly Property currentDebugApplication As XDocument
        Get
            Return Me.debug_xml
        End Get
    End Property
    Public Property TextColor As Color
        Get
            Return Me.ConsoleColor
        End Get
        Set(ByVal value As Color)
            Me.ConsoleColor = value
        End Set
    End Property
    Public Function ClearConsole(Optional ByVal init_text As String = "") As Boolean
        If Me.haveConsole() Then
            Me.ConsoleTextBox.Clear()
            Me.AddConsoleLine(init_text)
            Return True
        End If

        Return False
    End Function
    Public Function AddConsoleLine(ByVal line As String) As Boolean
        Return AddConsoleLine(line, Me.TextColor)
    End Function
    Public Function AddConsoleLine(ByVal line As String, ByVal TextColor As Color) As Boolean
        If Me.haveConsole() Then
            Dim last_color As Color
            'set color
            last_color = Me.ConsoleTxt.SelectionColor
            Me.ConsoleTxt.SelectionColor = TextColor
            Me.ConsoleTxt.AppendText(line & vbNewLine & vbNewLine)
            Me.ConsoleTxt.ScrollToCaret()

            'restore color
            Me.ConsoleTxt.SelectionColor = last_color
            Return True
        End If

        Return False
    End Function
    Public ReadOnly Property Context As SimonConsoleState
        Get
            Return Me.current_console_context
        End Get
    End Property
    Public ReadOnly Property haveConsole As Boolean
        Get
            Return Not IsNothing(Me.ConsoleTxt)
        End Get
    End Property
    Public Function Configure(ByVal xtrans As XDocument) As Boolean
        Me.doc = xtrans

        Return Not IsNothing(Me.doc)

    End Function
    Public ReadOnly Property Speaker() As SpeechSynthesizer
        Get
            Return Me.spk
        End Get
    End Property
    Public ReadOnly Property ConsoleTextBox() As RichTextBox
        Get
            Return Me.ConsoleTxt
        End Get
    End Property
    Public ReadOnly Property Culture As CultureInfo
        Get
            Return Me.culture_obj
        End Get
    End Property
    Public Function getTranslated(ByVal speechid As String, Optional ByVal cultureid As String = "(any)")

        If speechid = "" Then Return Nothing
        If IsNothing(speechid) Then Return Nothing

        Dim retval As String

        'first try to get the cultural version
        retval = Me.getTranslationByCulture(speechid, cultureid)

        'get the internal standard.
        If IsNothing(retval) And cultureid <> "(any)" Then
            'try any
            retval = Me.getTranslationByCulture(speechid, "(any)")
        End If

        Return retval
    End Function
   
    Public Function getTranslationByCulture(ByVal speechid As String, ByVal cultureid As String) As String

        If speechid = "" Then Return Nothing
        If IsNothing(speechid) Then Return Nothing

        Try

            Dim q = From translation_item In Me.doc.Descendants("translationtable").Descendants("translationitem") Where translation_item.@id.ToString = speechid.ToString And Not IsNothing(translation_item.@culturestring) And translation_item.@culturestring.ToString = cultureid

     

        If q.Count = 1 Then
            Dim translation_item As XElement

            translation_item = q(0)

            If Not IsNothing(translation_item.@text) Then
                Return translation_item.@text.ToString
            End If
        Else
            Debug.Print("results: " & q.Count.ToString)
        End If

        Catch ex As Exception

        End Try

        Return Nothing
    End Function
End Class
