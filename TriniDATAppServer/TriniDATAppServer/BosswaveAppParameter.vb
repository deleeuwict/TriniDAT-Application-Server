Public Class BosswaveAppParameter
    Inherits BosswaveDataType

    Private myname As String
    Private myvalue As String
    Private mydescription As String
    Private exernal_initializer_value As String
    Public Shadows myTag As Object
    Public Shadows myLinkedObject As Object 'e.g. stat condition
    Public Sub New(ByVal protoid As String, ByVal initvalue As String)
        MyBase.new()
        Me.PrototypeId = protoid
        Me.Value = initvalue
    End Sub
    Public Overloads ReadOnly Property isValidValue(ByVal realtime_value As String) As BosswaveStatCompareResult
        Get
            Return MyBase.isValidValue(Me, realtime_value)
        End Get
    End Property
    Public Overloads ReadOnly Property generateHTMLField(ByVal realtime_value As String) As String
        Get
            Return MyBase.generateHTMLField(Me, Me.displayName, realtime_value)
        End Get
    End Property
    Public Property Initializor As String
        Get
            Return Me.exernal_initializer_value
        End Get
        Set(ByVal value As String)
            Me.exernal_initializer_value = value
        End Set
    End Property
    Public Function ToXML() As XElement
        Dim xml As String

        xml = "<parameter typeid="
        xml &= Chr(34) & MyBase.PrototypeId.ToString & Chr(34) & " "
        xml &= "usertitle="
        xml &= Chr(34) & HttpUtility.UrlEncode(Me.displayName.ToString) & Chr(34) & ">"
        xml &= "value="
        xml &= Chr(34) & HttpUtility.UrlEncode(Me.Value.ToString) & Chr(34) & ">"

        Return XElement.Parse(xml)
    End Function
    'LinkedObject
    Public Shadows Property LinkedObject As Object
        Get
            Return myLinkedObject
        End Get
        Set(ByVal value As Object)
            myLinkedObject = value
        End Set
    End Property

    Public Shadows Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property

    Public Shadows ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    'Public Property Name As String
    '    Get
    '        Return Me.myname
    '    End Get
    '    Set(ByVal value As String)
    '        Me.myname = value
    '    End Set
    'End Property

    Public Property displayName As String
        Get
            Return Me.mydescription
        End Get
        Set(ByVal value As String)
            If IsNothing(value) Then
                value = "anonymous"
            End If
            Me.mydescription = value
        End Set
    End Property

    Public Property Value As String
        Get
            Return Me.myvalue
        End Get
        Set(ByVal value As String)
            Me.myvalue = value
        End Set
    End Property

    Public Shared Function createFromXParameter(ByVal xparameter As XElement)

        '<parameter protypeid="ServerProfileSelector" usertitle="Select Server Profile"></parameter>
        Dim new_parameter As BosswaveAppParameter

        new_parameter = New BosswaveAppParameter
        If Not IsNothing(xparameter.@prototypeid) Then
            new_parameter.PrototypeId = xparameter.@prototypeid
            new_parameter.displayName = xparameter.@usertitle

            If Not IsNothing(xparameter.@value) Then
                new_parameter.Value = HttpUtility.UrlDecode(xparameter.@value.ToString)
            Else
                new_parameter.Value = ""
            End If

            If Not IsNothing(xparameter.@initializedby) Then
                If new_parameter.DeclaresExternalModule Then
                    new_parameter.Initializor = xparameter.@initializedby.ToString

                Else
                    GlobalObject.Msg("Parameter '" & new_parameter.displayName & "' note: Only non-core prototype's can have initializors. Ignored '" & xparameter.@initializedby.ToString & "' @ " & xparameter.ToString & " .")
                End If
            End If

            Return new_parameter
        Else
            'String default
            new_parameter.PrototypeId = "String"

            Return new_parameter
        End If
    End Function

    Public Sub New()
        MyBase.new()
    End Sub
End Class