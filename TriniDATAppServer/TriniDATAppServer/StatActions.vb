Option Explicit On
Option Compare Text

Imports System.Xml
Imports System.IO

Public Class StatActions

  

    Public Shared Function TryExecuteApplicationAction_WebServiceContext(ByVal action_session As BosswaveConditionMatchList) As Boolean

        Dim retval As Boolean

        retval = False


        Select Case action_session.AssociatedAction.ActionName

            Case StatWebserviceFunctions.STAT_ACTION_LOGTOCONSOLE
                StatWebserviceFunctions.ConsoleLog(action_session)
                retval = True

        End Select


        Return retval

    End Function

    Public Shared Function TryExecuteApplicationAction_StatsContext(ByVal action_session As BosswaveConditionMatchList) As Boolean

        Dim retval As Boolean

        retval = False

        If action_session.AssociatedAction.haveParameters Then
            action_session.ActionInitialize()
        End If

        Select Case action_session.AssociatedAction.ActionName

            Case StatWebserviceFunctions.STAT_ACTION_FORWARDOBJECT
                StatWebserviceFunctions.ForwardObject(action_session)
                retval = True

            Case StatWebserviceFunctions.STAT_ACTION_SPEAKSTAT_VALUE
                StatWebserviceFunctions.SpeakMatchedStatValue(action_session)
                retval = True


            Case StatWebserviceFunctions.STAT_ACTION_SPEAKSTAT_CONDITION
                StatWebserviceFunctions.SpeakMatchedStatCondition(action_session)
                retval = True

            Case StatWebserviceFunctions.STAT_ACTION_SPEAKSTAT_NAMEVALUE
                StatWebserviceFunctions.SpeakMatchedStatNameValue(action_session)
                retval = True

                'SpeakMatchedStatNameValue


        End Select


        Return retval

    End Function
End Class

