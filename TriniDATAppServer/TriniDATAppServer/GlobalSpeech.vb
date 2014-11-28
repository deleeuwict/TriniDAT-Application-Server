Imports System.Threading
Imports System.Speech
Imports System.Speech.Synthesis

Public Class GlobalSpeech
    Private Shared current_speech_thread As ThreadExceptionDialog
    Private Shared current_speak_text As String
    Private Shared speak_enabled As Boolean
    Public Const SPEECH_WELCOME_TEXT As String = "Welcome to Trinidad data application server."
    Public Const SSML_TEMPLATE_FILENAME = "talk.xml"
    Private Shared tts_speaker As SpeechSynthesizer

    Public Shared Function Initialize() As Boolean
         speaker = New Speech.Synthesis.SpeechSynthesizer
        speaker.SelectVoiceByHints(Synthesis.VoiceGender.Male, Synthesis.VoiceAge.Adult)
        Return True

    End Function
    Public Shared ReadOnly Property haveSpeaker As Boolean
        Get
            Return Not IsNothing(GlobalSpeech.Speaker)
        End Get
    End Property

    Public Shared Property Speaker As Speech.Synthesis.SpeechSynthesizer
        Get
            Return GlobalSpeech.tts_speaker
        End Get
        Set(ByVal value As Speech.Synthesis.SpeechSynthesizer)
            GlobalSpeech.tts_speaker = value
        End Set
    End Property
    Public Shared Function initSSMLByTemplate(ByVal ssml_template_xml As String, ByVal text_to_speak As String) As String
        Try
            Return Replace(ssml_template_xml.ToString, "$TEXT", HttpUtility.HtmlEncode(text_to_speak))
        Catch ex As Exception

        End Try
    End Function
    Public Shared Sub SpeakWelcome()
        GlobalSpeech.current_speak_text = SPEECH_WELCOME_TEXT
        GlobalSpeech.SpeakThreaded()
    End Sub
    Public Shared Property Enabled As Boolean
        Get

            Return GlobalSpeech.speak_enabled
        End Get
        Set(ByVal value As Boolean)
            GlobalSpeech.speak_enabled = value
            If GlobalSpeech.speak_enabled Then
                Call GlobalSpeech.Initialize()
            Else
                Try
                    'kill speech 
                    GlobalSpeech.Speaker = Nothing
                Catch ex As Exception

                End Try
            End If

            'trigger GUI event
            If GlobalObject.haveServerThread Then
                If GlobalObject.haveServerForm Then
                    ' Call GlobalObject.serverForm.OnSpeechModeChangedDirect(GlobalSpeech.speak_enabled)
                    Call GlobalObject.serverForm.Invoke(GlobalObject.serverForm.onSpeechModeChangedThreaded, {GlobalSpeech.speak_enabled})
                End If
            End If
        End Set
    End Property
    Public Shared Sub SpeakThreaded()

        If GlobalSpeech.Enabled Then
            Dim speak_thread As New Thread(AddressOf GlobalSpeech.NowSpeakThread)
            speak_thread.SetApartmentState(ApartmentState.STA)
            speak_thread.Start()
        End If

    End Sub
    Public Shared Sub SpeakEliteThreaded()

        If GlobalSpeech.Enabled Then
            Dim speak_thread As New Thread(AddressOf GlobalSpeech.NowSpeakEliteThread)
            speak_thread.SetApartmentState(ApartmentState.STA)
            speak_thread.Start()
        End If

    End Sub
    Public Shared Property Text As String
        Get
            Return GlobalSpeech.current_speak_text
        End Get
        Set(ByVal value As String)
            GlobalSpeech.current_speak_text = value
        End Set
    End Property
    Private Shared Sub Speak(ByVal speaktxt As String, ByVal ssml_filepath As String)

        speaktxt = Trim(speaktxt)
        If IsNothing(speaktxt) Then Exit Sub

        Dim speaker As Speech.Synthesis.SpeechSynthesizer
        speaker = GlobalSpeech.Speaker

        speaker.SpeakSsml(JTextToSpeech.initSSMLByTemplate(IO.File.ReadAllText(ssml_filepath), "simon says " & speaktxt))

    End Sub
    Public Shared Sub SpeakElite(ByVal speaktxt As String, ByVal ssml_filepath As String)

        speaktxt = Trim(speaktxt)
        If IsNothing(speaktxt) Then Exit Sub

        Dim speaker As Speech.Synthesis.SpeechSynthesizer
        speaker = GlobalSpeech.Speaker

        speaker.SpeakSsml(JTextToSpeech.initSSMLByTemplate(IO.File.ReadAllText(ssml_filepath), speaktxt))

    End Sub
    Public Shared Sub NowSpeakEliteThread()

        If GlobalSpeech.Enabled Then
            If GlobalObject.haveServerForm Then
                GlobalSpeech.SpeakElite(GlobalSpeech.current_speak_text, GlobalSetting.getSpeechPath() & GlobalSpeech.SSML_TEMPLATE_FILENAME)
            End If
        End If
    End Sub
    Public Shared Sub NowSpeakThread()

        If GlobalSpeech.Enabled Then
            If GlobalObject.haveServerForm Then
                GlobalSpeech.Speak(GlobalSpeech.current_speak_text, GlobalSetting.getSpeechPath() & GlobalSpeech.SSML_TEMPLATE_FILENAME)
            End If
        End If
    End Sub
End Class
