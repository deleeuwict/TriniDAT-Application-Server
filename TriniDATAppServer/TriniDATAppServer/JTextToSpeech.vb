Imports TriniDATServerTypes
Imports System.Collections.Specialized
Imports System.Speech
Imports System.Xml
Imports System.IO


Public Class JTextToSpeech
    Inherits JTriniDATWebService

    Private my_configuration_info As MappingPointBootstrapData
    Private Shared TheSpeaker As Speech.Synthesis.SpeechSynthesizer = Nothing
    Private Shared last_err_msg As String = Nothing
    Private ssml_doc As XDocument
    Public Overrides Function DoConfigure() As Boolean
        'store relative path.
        Dim baseURI As String
        baseURI = Me.getProcessDescriptor().getParent().getURI()


        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        'use empty event table.
        getIOHandler().Configure(New TriniDATHTTP_EventTable)

        Return True
    End Function
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Shared ReadOnly Property haveErrorMessage() As Boolean
        Get

            Return Not IsNothing(JTextToSpeech.LastErrorMessage)
        End Get
    End Property

    Public Shared Function initSSMLByTemplate(ByVal ssml_template_xml As String, ByVal text_to_speak As String) As String
        Try
            Return Replace(ssml_template_xml.ToString, "$TEXT", HttpUtility.HtmlEncode(text_to_speak))
        Catch ex As Exception

        End Try
    End Function

    'Public Shared ReadOnly Property SSML_Template As XDocument
    '    Get
    '        Return Me.ssml_doc
    '    End Get
    'End Property
    'Public ReadOnly Property haveSSML_Template As Boolean
    '    Get
    '        Return Not IsNothing(Me.SSML_Template)
    '    End Get
    'End Property

    Public Shared Property LastErrorMessage As String
        Get
            Return JTextToSpeech.last_err_msg
        End Get
        Set(ByVal value As String)
            JTextToSpeech.last_err_msg = value
            If Not IsNothing(value) Then
                GlobalObject.Msg("Error initializing TTS engine. " & JTextToSpeech.last_err_msg & "Speech functionality will be unavailable.")
            End If
        End Set
    End Property

    Public Function myinbox(ByRef obj As JSONObject, from_url as string) As Boolean

        'catch message
        If obj.ObjectTypeName = "JAlpha" And obj.Directive = "MAPPING_POINT_START" Then

            'store mapping point info
            Me.my_configuration_info = CType(obj.Attachment, MappingPointBootstrapData)
            Return False
        End If

        If (obj.ObjectTypeName = "JTextToSpeech" Or obj.ObjectTypeName = "SPEAKER") And (obj.Directive = "SPEAK" Or obj.Directive = "SAY") Then
            Dim text_to_speak As String

            text_to_speak = CType(obj.Attachment, String)
            'indirect call to ensure synchronized + multithreaded speech.
            GlobalSpeech.Text = text_to_speak
            GlobalSpeech.SpeakThreaded()

            '  JTextToSpeech.Speak(text_to_speak, Me.my_configuration_info.static_path & Me.getClassNameFriendly() & SSML_TEMPLATE_FILENAME)

            Return False
        End If

        Return False
    End Function


End Class
