Imports TriniDATServerTypes

Public Class StatWebserviceFunctions
    Public Const STAT_ACTION_LOGTOCONSOLE As String = "consolelog"
    Public Const STAT_ACTION_SPEAKSTAT_VALUE As String = "speakstatvalue"
    Public Const STAT_ACTION_SPEAKSTAT_CONDITION As String = "speakstatcondition"
    Public Const STAT_ACTION_SPEAKSTAT_NAMEVALUE As String = "speakstatnamevalue"
    Public Const STAT_ACTION_FORWARDOBJECT As String = "forwardobject"

    Public Shared Sub ForwardObject(ByVal action_session As BosswaveConditionMatchList)

        Dim debugmsg As String
        Dim object_forwarder As ObjectForward

     
        object_forwarder = New ObjectForward()
        Try

            If action_session.AssociatedRequest.haveSession Then
                object_forwarder.SessionID = action_session.AssociatedRequest.direct_session.ID
            End If

            object_forwarder.ForwardObject = New JSONSmallObject(GlobalObject.CurrentServerConfiguration.server_ip.ToString, GlobalObject.CurrentServerConfiguration.server_port, Nothing)
            object_forwarder.ForwardObject.ObjectType = action_session.getParameterByName("Forward.Object.Type").Value
            object_forwarder.ForwardObject.Directive = action_session.getParameterByName("Forward.Object.Directive").Value
            object_forwarder.ForwardObject.ObjectAttachment = action_session.getParameterByName("Forward.Object.Attachment").Value

            object_forwarder.FullTargetMappingPointURL = action_session.getParameterByName("Forward.URL").Value

            object_forwarder.SenderClassName = action_session.getParameterByName("Forward.Sender").Value
            object_forwarder.ForwardToClassName = action_session.getParameterByName("Forward.To").Value

            If GlobalObject.haveServerThread Then
                If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                    debugmsg = "StatsFilter: Forwarding object " & object_forwarder.ForwardObject.ObjectType & " => " & object_forwarder.FullTargetMappingPointURL & "..."
                    Call GlobalObject.MsgColored(debugmsg, Color.Gold)
                End If
            End If


            If object_forwarder.SendBrowser() = True Then

            End If
        Catch ex As Exception
            debugmsg = "StatsFilter error: " & ex.Message
            Call GlobalObject.Msg(debugmsg)

        End Try

    End Sub

    Public Shared Sub ConsoleLog(ByVal action_session As BosswaveConditionMatchList)

        For Each matched_stat As BosswaveStatDropCondition In action_session

            Dim debugmsg As String


            debugmsg = matched_stat.OriginalNode.ToString & " MATCH: " & matched_stat.LastCompareResult.StatRealTimeValue

            If matched_stat.FieldOperator.Inversed Then
                debugmsg &= " NOT "
            End If

            Call GlobalObject.Msg(debugmsg)


        Next 'condition

    End Sub

    Public Shared Sub SpeakMatchedStatValue(ByVal action_session As BosswaveConditionMatchList)


        For Each matched_stat As BosswaveStatDropCondition In action_session
            'speak the value of matched stat


            'matched_stat.displayName 
            GlobalSpeech.Text = matched_stat.LastCompareResult.StatRealTimeValue
            GlobalSpeech.SpeakThreaded()

        Next 'condition

    End Sub
    Public Shared Sub SpeakMatchedStatNameValue(ByVal action_session As BosswaveConditionMatchList)


        For Each matched_stat As BosswaveStatDropCondition In action_session
            'speak the name and value of matched stat

            'matched_stat.displayName 
            GlobalSpeech.Text = matched_stat.LastCompareResult.StatRealTime.stat_name & " is " & matched_stat.LastCompareResult.StatRealTime.stat_value
            GlobalSpeech.SpeakThreaded()

        Next 'condition

    End Sub
    Public Shared Sub SpeakMatchedStatCondition(ByVal action_session As BosswaveConditionMatchList)


        For Each matched_stat As BosswaveStatDropCondition In action_session
            'speak the usertitle of matched stat

            'matched_stat.displayName 
            GlobalSpeech.Text = matched_stat.displayName
            GlobalSpeech.SpeakThreaded()


        Next 'condition

    End Sub

End Class
