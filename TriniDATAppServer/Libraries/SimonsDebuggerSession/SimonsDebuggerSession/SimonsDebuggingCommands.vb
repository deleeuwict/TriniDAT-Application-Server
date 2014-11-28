Option Compare Text
Imports System.drawing
Imports System.Globalization
Imports System.Speech.Synthesis
Imports SimonTypes
Imports TriniDATServerTypes

Public Class SimonsDebuggingCommands
    Private info As SimonsSession
    Private myTag As String

    Public Sub New(ByVal _simons_info As SimonsSession)
        Me.info = _simons_info
    End Sub

    Private ReadOnly Property SimonsInfo As SimonsSession
        Get
            Return Me.info
        End Get
    End Property
  
    Public Function DEBUG_APP(ByVal param() As String, actual_count as integer) As SimonsReturnValue
        'leave debugger mode
        Return SimonsReturnValue.VALIDCOMMAND_TOGGLE_DEBUG_MODE
    End Function

    Public Function DEBUGDIC(ByVal param() As String, actual_count as integer) As SimonsReturnValue
        'show command list.

        Dim xsimon As XDocument
        Dim xml As String
        xml = Join(param)

        xsimon = XDocument.Parse(xml)

        Dim q = From my_context_cmd In xsimon.Descendants("commands").Descendants("command") Where (my_context_cmd.@context.ToString = Me.SimonsInfo.Context.asXMLIdentifierString) And (Not IsNothing(my_context_cmd.@help) And Not IsNothing(my_context_cmd.@action)) Order By my_context_cmd.@action Ascending

        Dim valid = From my_context_cmd In q Where my_context_cmd.@help.ToString <> ""

        For Each help_enabled_cmd In valid

            Me.SimonsInfo.AddConsoleLine(help_enabled_cmd.@action.ToString & ": " & help_enabled_cmd.@help.ToString, Color.Gold)

        Next

        Return SimonsReturnValue.VALIDCOMMAND
    End Function
  
    Public Property Tag As String
        Get
            Return myTag
        End Get
        Set(ByVal value As String)
            myTag = value
        End Set
    End Property


End Class
