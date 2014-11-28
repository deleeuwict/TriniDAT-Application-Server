Public Class BosswaveConditionMatchList
    Inherits List(Of BosswaveStatDropCondition)

    Private mapping_point_action As BosswaveAppAction
    Private full_request As TriniDATRequestInfo
    Public Property AssociatedRequest As TriniDATRequestInfo
        Get
            Return Me.full_request
        End Get
        Set(ByVal value As TriniDATRequestInfo)
            Me.full_request = value
        End Set
    End Property
    Public ReadOnly Property haveAction As Boolean
        Get
            Return Not IsNothing(Me.AssociatedAction)
        End Get
    End Property
    Public Function getParameterByName(ByVal val As String) As BosswaveAppParameter

        If Not Me.haveAction Then Return Nothing
        If Not Me.AssociatedAction.haveParameters Then Return Nothing


        Return Me.AssociatedAction.Parameters.getByDisplayName(val)

    End Function
    Public ReadOnly Property haveRequestInformation As Boolean
        Get
            Return Not IsNothing(Me.full_request)
        End Get
    End Property
    Public Property AssociatedAction As BosswaveAppAction
        Get
            Return Me.mapping_point_action
        End Get
        Set(ByVal value As BosswaveAppAction)
            Me.mapping_point_action = value
        End Set
    End Property
    Public Sub ActionInitialize()
        'mp_var = current mapping point url

        If Not Me.haveAction Then Exit Sub

        For Each p In Me.AssociatedAction.Parameters
            'fill parameter values according to the first matched condition.
            If Left(p.Value, 1) = "$" Then
                p.Value = Replace(p.Value, "$MPURL", Me.AssociatedRequest.FullServerURL)

                If Me.haveFirstMatch Then
                    p.Value = Replace(p.Value, "$STATNAME", Me.FirstMatch.LastCompareResult.StatRealTime.stat_name)
                    p.Value = Replace(p.Value, "$STATVALUE", Me.FirstMatch.LastCompareResult.StatRealTimeValue)
                End If

            End If
        Next

    End Sub
    Public ReadOnly Property haveFirstMatch As Boolean
        Get
            Return Not IsNothing(Me.FirstMatch)
        End Get
    End Property

    Public ReadOnly Property FirstMatch As BosswaveStatDropCondition
        Get
            Return Me.Item(0)
        End Get
    End Property
End Class
