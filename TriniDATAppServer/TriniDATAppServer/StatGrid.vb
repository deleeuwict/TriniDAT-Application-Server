Option Compare Text
Option Explicit On

Imports System.IO
Imports System.Web
Imports TriniDATServerStatTypes

Public Class StatGrid

    Public Shared Function InWebServiceContext(ByVal affected_mapping_point As mappingPointRoot, ByVal jclass_friendly_name As String, ByVal stat_data As Preprocessing_result, ByVal unique As Boolean, Optional ByVal event_retries As Integer = 0) As StatMonResult
        'executed in mapping-thread context.

        Dim xcurrentapp As XDocument
        Dim results As Boolean
        Dim abstract_app As BosswaveApplication
        Dim current_execution_context As STAT_EXECUTION_CONTEXT

        current_execution_context = STAT_EXECUTION_CONTEXT.CONTEXT_ACTIVEWEBSERVICE_LEVEL

        abstract_app = affected_mapping_point.Session.Application(affected_mapping_point.ApplicationId)

        'get abstract application template
        If IsNothing(abstract_app) Then
            Return StatMonResult.NO_APP
        End If


        'execute any action that affects this stat variable.

        Try

            'read XML
            xcurrentapp = XDocument.Parse(File.ReadAllText(abstract_app.Filepath))

            'global
            Dim q = From app_actions In xcurrentapp.Descendants("action") Where app_actions.@triggercontext.ToString = "webservice" And (app_actions.@filterurl.ToString = "(global)" Or app_actions.@filterurl.ToString = affected_mapping_point.Info.recreateRelativeURL Or app_actions.@filterurl.ToString = affected_mapping_point.getURI()) And (app_actions.@jclass.ToString = "(global)" Or app_actions.@jclass.ToString = jclass_friendly_name) Order By app_actions.@filterurl.ToString Ascending, app_actions.@jclass Ascending

            results = False


            For Each app_action_node As XElement In q
                results = True

                Dim continious_match As Boolean
                Dim conditional_stats = From stat_elements In app_action_node.Descendants("statdropconditions")


                If Not IsNothing(conditional_stats.@matchall) Then
                    continious_match = (conditional_stats.@trymatchall.ToString = "true")
                Else
                    continious_match = False
                End If

                If conditional_stats.Count < 1 Then
                    GoTo SCAN_APP_CONDITIONS
                End If

                Dim all_stat_conditions = From stat_element In conditional_stats.Descendants("statdropcondition") Where (stat_element.@name.ToString = stat_data.stat_name Or stat_element.@name.ToString = "(any)") Or (InStr(stat_element.@name.ToString, stat_data.stat_name & ".", vbTextCompare) = 1)
                Dim action_session As BosswaveConditionMatchList

                action_session = New BosswaveConditionMatchList
                action_session.AssociatedRequest = affected_mapping_point.Info

                For Each conditional_stat In all_stat_conditions

                    Dim bosswave_stat_condition As BosswaveStatDropCondition


                    'recreate the condition in memory
                    bosswave_stat_condition = BosswaveStatDropCondition.createFromXStatDropCondition(conditional_stat)
                    If Not IsNothing(bosswave_stat_condition) Then
                        'GlobalObject.Msg("HIT " & BosswaveApplicationHost.HostedApplication.ApplicationName & "[action monitorurl: " & app_action.@filterurl.ToString & "     jclass: " & app_action.@jclass.ToString & "    current mp:" & affected_mapping_point.getURI() & "] Field  " & stat_data.stat_name)

                        Dim parameter_compare_result As BosswaveStatCompareResult

                        stat_data.stat_value = HttpUtility.UrlDecode(stat_data.stat_value)

                        'compare
                        parameter_compare_result = bosswave_stat_condition.Match(stat_data.stat_value)

                        If parameter_compare_result = BosswaveStatCompareResultVal.MATCH Then
                            'pass original stat to function
                            parameter_compare_result.StatRealTime = stat_data
                            parameter_compare_result.StatCondition = bosswave_stat_condition
                            bosswave_stat_condition.LastCompareResult = parameter_compare_result

                            'execute associated action
                            action_session.Add(bosswave_stat_condition)
                            action_session.AssociatedAction = BosswaveAppAction.createFromXAction(app_action_node)
                            'fire programmed  action
                            StatActions.TryExecuteApplicationAction_WebServiceContext(action_session)

                            'continous match will fire associated action for each result.
                            If Not continious_match Then
                                Return BosswaveStatCompareResultVal.MATCH
                            End If
                        Else
                            '      GlobalObject.Msg("NOT MATCHED: " & stat_data.stat_name & " = " & stat_data.stat_value)
                        End If
                    End If
                Next 'condition
SCAN_APP_CONDITIONS:
            Next 'app

            If Not results Then
                Return StatMonResult.NO_MATCH
            End If

        Catch ex As System.IO.IOException
            If event_retries < 5 Then
                Threading.Thread.Sleep(500)
                event_retries += 1
                Return StatGrid.InWebServiceContext(affected_mapping_point, jclass_friendly_name, stat_data, unique, event_retries)
            Else
                Throw New Exception("Warning: Multiple attempts to read " & abstract_app.Filepath & " failed due file locks. Giving up. Stat actions may not have fired.")
            End If

        Catch ex As Exception
            GlobalObject.Msg(abstract_app.ApplicationName & " run-time error on " & affected_mapping_point.getURI() & ": " & ex.Message)
            Return StatMonResult.APP_ERR
        End Try


        Return StatMonResult.MATCH

    End Function
    Public Shared Function InStatsContext(ByVal affected_mapping_point As mappingPointRoot, ByVal jclass_friendly_name As String, ByVal stat_data As Preprocessing_result, ByVal unique As Boolean, Optional ByVal event_retries As Integer = 0) As StatMonResult
        'executed in mapping-thread context.
        'executed by JStats when stat was created.

        Dim xcurrentapp As XDocument
        Dim results As Boolean
        Dim abstract_app As BosswaveApplication
        Dim current_execution_context As STAT_EXECUTION_CONTEXT

        current_execution_context = STAT_EXECUTION_CONTEXT.CONTEXT_ACTIVEWEBSERVICE_LEVEL

        abstract_app = affected_mapping_point.Session.Application(affected_mapping_point.ApplicationId)

        'get abstract application template
        If IsNothing(abstract_app) Then
            Return StatMonResult.NO_APP
        End If


        'execute any action that affects this stat variable.

        Try

            'read XML
            xcurrentapp = XDocument.Parse(File.ReadAllText(abstract_app.Filepath))

            'global
            Dim q = From app_actions In xcurrentapp.Descendants("action") Where app_actions.@triggercontext.ToString = "stats" And (app_actions.@filterurl.ToString = "(global)" Or app_actions.@filterurl.ToString = affected_mapping_point.Info.recreateRelativeURL Or app_actions.@filterurl.ToString = affected_mapping_point.getURI()) And (app_actions.@jclass.ToString = "(global)" Or app_actions.@jclass.ToString = jclass_friendly_name) Order By app_actions.@filterurl.ToString Ascending, app_actions.@jclass Ascending

            results = False


            For Each app_action_node As XElement In q
                results = True

                Dim continious_match As Boolean
                Dim conditional_stats = From stat_elements In app_action_node.Descendants("statdropconditions")


                If Not IsNothing(conditional_stats.@matchall) Then
                    continious_match = (conditional_stats.@trymatchall.ToString = "true")
                Else
                    continious_match = False
                End If

                If conditional_stats.Count < 1 Then
                    GoTo SCAN_APP_CONDITIONS
                End If

                If Not IsNothing(app_action_node.@option) Then
                    Dim options() As String
                    options = Split(app_action_node.@option, ";")

                    For Each action_option In options
                        If action_option = "ignorerevisions" And stat_data.stat_type_unique And stat_data.state_unique_revision_changed Then
                            'ignore revision changes.
                            GoTo SCAN_APP_CONDITIONS
                        End If
                        If action_option = "ignorevaluedups" And Not stat_data.stat_type_unique Then
                            'if more then 1 entry of this statname+key in urlencoded form, then skip action.
                            Dim q_recorded_stat_jclass_node = stat_data.file.Descendants(jclass_friendly_name)

                            If q_recorded_stat_jclass_node.Count = 1 Then
                                Dim q_recorded_stat_jclass_unique = q_recorded_stat_jclass_node(0).Descendants(stat_data.stat_name)

                                If q_recorded_stat_jclass_unique.Count > 0 Then
                                    Dim same_statname_count As Integer
                                    same_statname_count = q_recorded_stat_jclass_unique.Count

                                    If same_statname_count > 1 Then
                                        'check dupliate urlencoded values.
                                        ' same_statname_count = same_statname_count
                                        Dim dup_check = From statistical_item In q_recorded_stat_jclass_unique Where statistical_item.Value = stat_data.stat_value
                                        If dup_check.Count > 1 Then
                                            'duplicate found.
                                            same_statname_count = dup_check.Count
                                            GoTo SCAN_APP_CONDITIONS
                                        End If
                                    End If
                                End If
                            End If


                        End If
                    Next

                End If

                Dim all_stat_conditions = From stat_element In conditional_stats.Descendants("statdropcondition") Where (stat_element.@name.ToString = stat_data.stat_name Or stat_element.@name.ToString = "(any)") Or (InStr(stat_element.@name.ToString, stat_data.stat_name & ".", vbTextCompare) = 1)
                Dim action_session As BosswaveConditionMatchList

                action_session = New BosswaveConditionMatchList
                action_session.AssociatedRequest = affected_mapping_point.Info

                For Each conditional_stat In all_stat_conditions

                    Dim bosswave_stat_condition As BosswaveStatDropCondition


                    'recreate the condition in memory
                    bosswave_stat_condition = BosswaveStatDropCondition.createFromXStatDropCondition(conditional_stat)
                    If Not IsNothing(bosswave_stat_condition) Then
                        'GlobalObject.Msg("HIT " & BosswaveApplicationHost.HostedApplication.ApplicationName & "[action monitorurl: " & app_action.@filterurl.ToString & "     jclass: " & app_action.@jclass.ToString & "    current mp:" & affected_mapping_point.getURI() & "] Field  " & stat_data.stat_name)

                        Dim parameter_compare_result As BosswaveStatCompareResult

                        stat_data.stat_value = HttpUtility.UrlDecode(stat_data.stat_value)

                        'compare
                        parameter_compare_result = bosswave_stat_condition.Match(stat_data.stat_value)

                        If parameter_compare_result = BosswaveStatCompareResultVal.MATCH Then
                            'pass original stat to function
                            parameter_compare_result.StatRealTime = stat_data
                            parameter_compare_result.StatCondition = bosswave_stat_condition
                            bosswave_stat_condition.LastCompareResult = parameter_compare_result

                            'execute associated action
                            action_session.Add(bosswave_stat_condition)
                            action_session.AssociatedAction = BosswaveAppAction.createFromXAction(app_action_node)
                           
                            'fire programmed  action
                            StatActions.TryExecuteApplicationAction_StatsContext(action_session)

                            'continous match will fire associated action for each result.
                            If Not continious_match Then
                                Return BosswaveStatCompareResultVal.MATCH
                            End If
                        Else
                            '      GlobalObject.Msg("NOT MATCHED: " & stat_data.stat_name & " = " & stat_data.stat_value)
                        End If
                    End If
                Next 'condition
SCAN_APP_CONDITIONS:
            Next 'app

            If Not results Then
                Return StatMonResult.NO_MATCH
            End If

        Catch ex As System.IO.IOException
            If event_retries < 5 Then
                Threading.Thread.Sleep(500)
                event_retries += 1
                Return StatGrid.InWebServiceContext(affected_mapping_point, jclass_friendly_name, stat_data, unique, event_retries)
            Else
                Throw New Exception("Warning: Multiple attempts to read " & abstract_app.Filepath & " failed due file locks. Giving up. Stat actions may not have fired.")
            End If

        Catch ex As Exception
            GlobalObject.Msg(abstract_app.ApplicationName & " run-time error on " & affected_mapping_point.getURI() & ": " & ex.Message)
            Return StatMonResult.APP_ERR
        End Try


        Return StatMonResult.MATCH

    End Function
    Public Shared Function InSocketLevelContext(ByVal current_connection As TriniDATRequestInfo) As StatMonResult

    End Function
    Public Shared Function InMappingPointLevelContext(ByVal current_connection As TriniDATRequestInfo) As StatMonResult

    End Function

    Public Shared Function getJStatFilePath(ByVal mapping_point_url As String) As String

        Dim filename As String
        filename = GlobalSetting.getStaticDataRoot() & "JStats\"
        filename &= JStats.getFilenameByMappingPointUri(mapping_point_url)

        GlobalObject.Msg(mapping_point_url & ".statfile = " & filename)

        Return filename
    End Function

End Class

Public Enum StatMonResult
    APP_ERR = -1
    NO_APP = 0
    NO_MATCH = 1
    MATCH = 2
End Enum