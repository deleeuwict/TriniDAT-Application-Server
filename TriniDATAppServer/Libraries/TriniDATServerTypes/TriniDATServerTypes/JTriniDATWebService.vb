Option Explicit On
Option Compare Text
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Reflection
Imports System.Collections.Specialized
Imports System.IO
Imports TriniDATDictionaries

Public MustInherit Class JTriniDATWebService

    Protected mappedurl As String
    Protected mp_index As Integer 'mappingPoint index.
    Private mymailbox As Object '=JServicedMailBox
    Private myProcess As Object '=mappingPointInstanceInfo
    Private serial_number As String = "(NONE)" 'must be overriden in app XML or ap will not run
    Private _type_creator As object_creator
    Private http_function_table As TriniDATServerFunctionTable

    Public killed As Boolean

    'LOGGIN
    Public MustOverride Function DoConfigure() As Boolean  'triggered right after construction where user code must set up all handlers for desired events.
    Public MustOverride Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean 'triggered right after construction where user code must set up all handlers for desired events.

    Protected object_serverhost As String
    Protected object_serverport As Integer
    Public Property ObjectServer As String
        Get
            Return Me.object_serverhost
        End Get
        Set(ByVal value As String)
            Me.object_serverhost = value
        End Set
    End Property
    Public ReadOnly Property haveObjectServer As Boolean
        Get
            Return Me.object_serverhost <> "" And Me.object_serverport > 0
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
    Public Sub setServerTypeCreatorGate(ByVal val As object_creator)
        Me._type_creator = val
        Call CreateInnerObjects()
    End Sub
    'must be overriden via app XML or a distributed app will not run
    Public Property SerialNumber As String
        Get
            Return Me.serial_number
        End Get
        Set(ByVal value As String)
            Me.serial_number = value
        End Set
    End Property
    Public ReadOnly Property isJAlpha
        Get
            Return Me.getClassNameFriendly() = "JAlpha"
        End Get
    End Property
    Public ReadOnly Property isJOmega
        Get
            Return Me.getClassNameFriendly() = "JOmega"
        End Get
    End Property
    Public ReadOnly Property ServerTypeCreator As object_creator
        Get
            'creates object types that are exclusively native for the server assembly 
            'e.g. BlingClientConnectionManagerHTTP
            Return Me._type_creator
        End Get
    End Property
    Public ReadOnly Property haveServerTypeCreator As Boolean
        Get
            Return Not IsNothing(Me._type_creator)
        End Get
    End Property

    Private Sub CreateInnerObjects()
        'flag for BlingServer to stop parsing our packets.
        Me.mymailbox = ServerTypeCreator.Invoke("JServicedMailBox", {Me})

        Me.http_function_table = New TriniDATServerFunctionTable

    End Sub
    Public WriteOnly Property putStat
        Set(ByVal value As Object)
            Dim req As JSONObject
            req = New JSONObject

            req.ObjectType = "JPutStat"
            req.Attachment = value
            req.Sender = Me
            Me.getMailProvider().Send(req, Nothing, "JStats")
        End Set
    End Property
    Public WriteOnly Property putSingle
        Set(ByVal value As Object)
            Dim req As JSONObject
            req = New JSONObject

            req.ObjectType = "JPutStatUnique"
            req.Attachment = value
            req.Sender = Me
            Me.getMailProvider().Send(req, Nothing, "JStats")
        End Set
    End Property

    Public Sub askServerHostName()
        Dim req As JSONObject
        req = New JSONObject

        req.ObjectType = "Setting_ServerHostname"
        req.Sender = Me
        Me.getMailProvider().Send(req, Nothing, "JKernelReflection")
    End Sub
    Public Sub askServerPort()
        Dim req As JSONObject
        req = New JSONObject

        req.ObjectType = "Setting_ServerPort"
        req.Sender = Me
        Me.getMailProvider().Send(req, Nothing, "JKernelReflection")
    End Sub
    Public Sub askServerURL()
        Dim req As JSONObject
        req = New JSONObject

        req.ObjectType = "Setting_ServerURL"
        req.Sender = Me
        Me.getMailProvider().Send(req, Nothing, "JKernelReflection")
    End Sub
    Public Function isObjectListener() As Boolean
        Return Me.getMailProvider().getEventTable().haveInboxEventHandler()
    End Function
    Public Function getMailProvider() As Object  '= JServicedMailBox
        'exposes a json object mail system.
        Return Me.mymailbox
    End Function
    Public ReadOnly Property haveWebserviceFunctionTable As Boolean
        Get
            Return Not IsNothing(Me.http_function_table)
        End Get
    End Property
    Public ReadOnly Property WebserviceFunctionTable As TriniDATServerFunctionTable
        Get
            Return Me.http_function_table
        End Get
    End Property

    Public ReadOnly Property haveGETFunctions As Boolean
        Get

            If Not Me.haveWebserviceFunctionTable Then
                Return False
            End If

            Return Me.WebserviceFunctionTable.haveGETFunctions

        End Get
    End Property
    Public ReadOnly Property havePOSTFunctions As Boolean
        Get

            If Not Me.haveWebserviceFunctionTable Then
                Return False
            End If

            Return Me.WebserviceFunctionTable.havePOSTFunctions

        End Get
    End Property

    Public Function doOnRegisterWebserviceFunctionTableEvent() As Boolean
        Try

            If Not Me.haveServerTypeCreator Then
                Return False
            End If

            If Not Me.OnRegisterWebserviceFunctions(Me.http_function_table) Then
                'programmer ought to return TRUE
                Return False
            End If

            If Not Me.haveWebserviceFunctionTable Then
                'a crazy programmer deleted my function table.
                Return False
            End If

        Catch ex As Exception
            'function not implemented.
            Return False
        End Try

PARAMETER_SPECIFICATION_TO_PROTOTYPE:

        'create actual BosswaveAppParameter instances for the specified prototypes.
        For Each webservice_function_declaration As Object In Me.WebserviceFunctionTable
            ' Public Sub New(ByVal protoid As String, ByVal initvalue As String)

            If (TypeOf webservice_function_declaration Is TriniDAT_ServerGETFunction) Or (TypeOf webservice_function_declaration Is TriniDAT_ServerPOSTFunction) Then

                Dim webservice_parameter_specifications As TriniDATParameterSpecCollection
                webservice_parameter_specifications = TriniDATParameterSpecCollection.createFrom(webservice_function_declaration.parameters)

                For Each webservice_function_parameter_specification As TriniDAT_ServerFunctionParameterSpec In webservice_parameter_specifications

                    If webservice_function_parameter_specification.haveParameterName And webservice_function_parameter_specification.haveParameterType Then
                        Dim actual_webservice_parameter_prototype As TriniDAT_ServerFunctionParameter

                        actual_webservice_parameter_prototype = New TriniDAT_ServerFunctionParameter
                        actual_webservice_parameter_prototype.BosswaveParameter = Me.ServerTypeCreator.Invoke("BosswaveAppParameter", {webservice_function_parameter_specification.ParameterType, Nothing})
                        actual_webservice_parameter_prototype.ID = webservice_function_parameter_specification.ParameterName
                        actual_webservice_parameter_prototype.Required = webservice_function_parameter_specification.Required
                        If actual_webservice_parameter_prototype.haveBosswaveParameter Then
                            If webservice_function_declaration.parameters.ReplaceSpecWithInstance(webservice_function_parameter_specification, actual_webservice_parameter_prototype) Then
                                'do until all specs are gone.
                                GoTo PARAMETER_SPECIFICATION_TO_PROTOTYPE
                            End If
                        Else
                            Throw New Exception("Error creating instance of prototype with ID '" & webservice_function_parameter_specification.ParameterType.ToString & "' or prototype not registered with server. You may need to add it to the server's intenral prototype list.")
                        End If
                    End If
                Next
            Else
                'user add wrong kind of item 
                Throw New Exception("Invalid function table object '" & webservice_function_declaration.GetType().ToString & "'")
                Return False
            End If
        Next

DONE:
        Return True
    End Function
    Public ReadOnly Property getFunctionTable()
        Get
            Return Me.http_function_table
        End Get
    End Property
    Protected Sub LocalBroadcast(ByVal iobj As JSONObject)
        'send to 
        Call getMailProvider().LocalBroadcast(iobj, False)
    End Sub
    Protected Sub SendAll(ByVal iobj As JSONObject)
        'send to all incl. self
        Call getMailProvider().LocalBroadcast(iobj, True)
    End Sub
    Public Function getStartArguments() As StringDictionary
        'Note: returns the parameters of the URL that created this service instace.
        'to get the current request's parameters -> getLiveRequestParameters
        If Not haveProcessIndex() Then
            Msg("Cannot access IO handler, process Id is unknown (streaming from constructor?)")
            Return Nothing
        End If

        Return Me.getProcessDescriptor().getParameters()

    End Function
    Public ReadOnly Property haveIOHandler() As Boolean
        Get
            If Not Me.haveProcessIndex() Then
                Return False
            End If
            Return Not IsNothing(Me.getProcessDescriptor().getProtocolHandler())
        End Get
    End Property
    Public ReadOnly Property getIOHandler() As Object '=BlingClientConnectionManagerHTTP
        Get


            'get HTTP IO
            'returns Nothing in case of a background mapping.

            If Not haveProcessIndex() Then
                Msg("Cannot access IO handler, process Id is unknown (streaming from constructor?)")
                Return Nothing
            End If

            Return Me.getProcessDescriptor().getProtocolHandler()

        End Get
    End Property
    Public Sub Log(ByVal str As String)
        Dim logrequest As JSONObject

        logrequest = New JSONObject
        logrequest.Directive = "LOG"
        logrequest.ObjectType = "JTEXTITEM"
        logrequest.Sender = Me
        logrequest.Attachment = str
        Me.getMailProvider().Send(logrequest, Nothing, "JInteractiveConsole")
        'logrequest. = "LOG"

    End Sub
    Public ReadOnly Property getMappingIndex() As Integer
        Get
            Return Me.mp_index   'index of our mappingpointdescriptor in static EntryPointMappingPointDir class.
        End Get
    End Property
    Public Function makeRelative(ByVal myurl As String) As String
        'return: /appid/mpurl/<userurl>

        If Not Me.haveProcessIndex Then Return Nothing

        If Left(myurl, 1) = "/" Then
            myurl = Mid(myurl, 2)
        End If

        Dim mp_root As Object
        Dim app_plus_mp_url As String

        mp_root = Me.getProcessDescriptor().getParent()
        app_plus_mp_url = "/" & mp_root.ApplicationID.ToString & mp_root.info.MappingPointDescriptor.url

        myurl = app_plus_mp_url & myurl

        Return myurl

    End Function

    Public Function initializeGETFunction(ByVal webservice_get_function As TriniDAT_ServerGETFunction, ByVal HTTP_URI_Parameters As StringDictionary) As TRINIDAT_HTTP_INITIALIZATION_RESULT

        Dim retval As TRINIDAT_HTTP_INITIALIZATION_RESULT
        retval = New TRINIDAT_HTTP_INITIALIZATION_RESULT(TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.NEUTRAL)

        If IsNothing(HTTP_URI_Parameters) Then
            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS
            GoTo done
        End If

        Try
            If webservice_get_function.haveParameters Then


                For Each raw_parameter_id As String In HTTP_URI_Parameters.Keys

                    If webservice_get_function.haveParameterByName(raw_parameter_id) Then

                        Dim realtime_parameter As TriniDAT_ServerFunctionParameter
                        Dim raw_value As String

                        raw_value = HTTP_URI_Parameters(raw_parameter_id)
                        realtime_parameter = webservice_get_function.getParameterByName(raw_parameter_id)
                        realtime_parameter.Found = True

                        If realtime_parameter.BosswaveParameter.isValidValue(raw_value).Result = 1 Then '1=MATCH
                            'OK
                            realtime_parameter.ParameterValue = raw_value
                        Else
                            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.PARAMETER_NOT_ACCEPTED
                            retval.affected_parameter_id = raw_parameter_id
                            retval.affected_parameter_value = raw_value
                            retval.ErrorMessage = "TriniDAT Data Server: Invalid '" & realtime_parameter.BosswaveParameter.PrototypeId & "' value. Affected parameter: '" & raw_parameter_id & "' value: '" & raw_value & "'"
                            Return retval
                        End If
                    End If
                Next

                'validate
                If webservice_get_function.haveMissingParameters Then
                    For Each webservice_get_function_missing_parameter In webservice_get_function.getMissingParameters
                        retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.REQUIRED_PARAMETER_MISSING
                        retval.affected_parameter_id = webservice_get_function_missing_parameter.id
                        retval.ErrorMessage = "TriniDAT Data Server: Missing parameter of type GET. Name: '" & webservice_get_function_missing_parameter.id & "'. Declaration Type: " & webservice_get_function_missing_parameter.BosswaveParameter.PrototypeId
                        Return retval
                    Next
                End If

            End If
            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS

        Catch ex As Exception
            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.ERR
            retval.ErrorMessage = ex.Message
        End Try

DONE:
        Return retval
    End Function
    Public Function initializePOSTFunction(ByVal webservice_post_function As TriniDAT_ServerPOSTFunction, ByVal HTTP_URI_Parameters As StringDictionary) As TRINIDAT_HTTP_INITIALIZATION_RESULT

        Dim retval As TRINIDAT_HTTP_INITIALIZATION_RESULT
        retval = New TRINIDAT_HTTP_INITIALIZATION_RESULT(TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.NEUTRAL)

        If IsNothing(HTTP_URI_Parameters) Then
            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS
            GoTo done
        End If

        Try

            If webservice_post_function.haveParameters Then

                For Each raw_parameter_id As String In HTTP_URI_Parameters.Keys

                    If webservice_post_function.haveParameterByName(raw_parameter_id) Then

                        Dim realtime_parameter As TriniDAT_ServerFunctionParameter
                        Dim raw_value As String

                        raw_value = HTTP_URI_Parameters(raw_parameter_id)
                        realtime_parameter = webservice_post_function.getParameterByName(raw_parameter_id)
                        realtime_parameter.Found = True

                        If realtime_parameter.BosswaveParameter.isValidValue(raw_value).Result = 1 Then '1=MATCH
                            'OK
                            realtime_parameter.ParameterValue = raw_value
                        Else
                            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.PARAMETER_NOT_ACCEPTED
                            retval.affected_parameter_id = raw_parameter_id
                            retval.affected_parameter_value = raw_value
                            retval.ErrorMessage = "TriniDAT Data Server: Invalid '" & realtime_parameter.BosswaveParameter.PrototypeId & "' value. Affected parameter: '" & raw_parameter_id & "' value: '" & raw_value & "'"
                            Return retval
                        End If
                    End If
                Next

                'validate
                If webservice_post_function.haveMissingParameters Then
                    For Each webservice_post_function_missing_parameter In webservice_post_function.getMissingParameters
                        Dim realtime_parameter As TriniDAT_ServerFunctionParameter
                        realtime_parameter = webservice_post_function.getParameterByName(webservice_post_function_missing_parameter.id)
                        retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.REQUIRED_PARAMETER_MISSING
                        retval.affected_parameter_id = webservice_post_function_missing_parameter.id
                        retval.ErrorMessage = "TriniDAT Data Server: Missing parameter of type POST. Name: '" & webservice_post_function_missing_parameter.id & "'. Declaration Type: " & webservice_post_function_missing_parameter.BosswaveParameter.PrototypeId
                        Return retval
                    Next
                End If
            End If

            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.SUCCESS

        Catch ex As Exception
            retval.state = TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE.ERR
            retval.ErrorMessage = ex.Message
        End Try

DONE:
        Return retval
    End Function

    Public Function triggerUserGETFunction(ByVal user_func As Object, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary) As Boolean
        'no differences with POST as of yet.
        Try

            Call user_func.TriggerFunction(AllParameters, Headers)

            If user_func.ReceiveCallNotification Then
                Dim msg As JSONObject
                msg = New JSONObject
                msg.Sender = Me
                msg.Directive = "GET " & user_func.functionurl
                msg.Attachment = user_func.Parameters
                Me.getMailProvider().send(msg, Nothing, Me.getClassNameFriendly())
            End If

            Return True

        Catch ex As Exception

        End Try

        Return False
    End Function
    Public Function triggerUserPOSTFunction(ByVal user_func As Object, ByVal AllParameters As StringDictionary, ByVal Headers As StringDictionary) As Boolean
        Try

            Call user_func.TriggerFunction(AllParameters, Headers)

            If user_func.ReceiveCallNotification Then
                Dim msg As JSONObject
                msg = New JSONObject
                msg.Sender = Me
                msg.Directive = "POST " & user_func.functionurl
                msg.Attachment = user_func.Parameters
                Me.getMailProvider().send(msg, Nothing, Me.getClassNameFriendly())
            End If

            Return True

        Catch ex As Exception

        End Try

        Return False
    End Function
    Public ReadOnly Property getProcessDescriptor() As Object  '=mappingPointInstanceInfo
        Get
            Return Me.myProcess
        End Get
    End Property
    Public Sub setProcessDescriptor(ByVal val As Object) 'val=mappingPointInstanceInfo
        Me.myProcess = val
    End Sub
    Public ReadOnly Property getProcessIndex() As Integer
        Get
            Return Me.getProcessDescriptor().getIndex()   'index of our mappingPoint.processes() group in a ServerMappingPointManager instance.

        End Get
    End Property
    Public ReadOnly Property haveProcessIndex() As Boolean
        Get
            Return Not (Me.getProcessDescriptor().getIndex())
        End Get
    End Property

    Protected Function getClean(ByVal stat_name As String)

        Dim upper_dic As TriniDATCharDictionary
        Dim lower_dic As TriniDATCharDictionary
        Dim special_char_dic As TriniDATCharDictionary
        Dim all_dictionaries As TriniDATCharDictionaries
        Dim retval As String
        Dim c As Char

        upper_dic = New TriniDATCharDictionary("ClassicASCIILowercase", {ChrW(&H61), ChrW(&H62), ChrW(&H63), ChrW(&H64), ChrW(&H65), ChrW(&H66), ChrW(&H67), ChrW(&H68), ChrW(&H69), ChrW(&H6A), ChrW(&H6B), ChrW(&H6C), ChrW(&H6D), ChrW(&H6E), ChrW(&H6F), ChrW(&H70), ChrW(&H71), ChrW(&H72), ChrW(&H73), ChrW(&H74), ChrW(&H75), ChrW(&H76), ChrW(&H77), ChrW(&H78), ChrW(&H79), ChrW(&H7A)})
        lower_dic = New TriniDATCharDictionary("ClassicASCIIUppercase", {ChrW(&H41), ChrW(&H42), ChrW(&H43), ChrW(&H44), ChrW(&H45), ChrW(&H46), ChrW(&H47), ChrW(&H48), ChrW(&H49), ChrW(&H4A), ChrW(&H4B), ChrW(&H4C), ChrW(&H4D), ChrW(&H4E), ChrW(&H4F), ChrW(&H50), ChrW(&H51), ChrW(&H52), ChrW(&H53), ChrW(&H54), ChrW(&H55), ChrW(&H56), ChrW(&H57), ChrW(&H58), ChrW(&H59), ChrW(&H5A)})
        special_char_dic = New TriniDATCharDictionary("ClassicASCIISpecialChars", {"_"})

        all_dictionaries = New TriniDATCharDictionaries("", New List(Of TriniDATCharDictionary)({special_char_dic, upper_dic, lower_dic}))

        retval = ""
        For Each c In stat_name
            If Not all_dictionaries.Has(c) Then
                'replace invalid char
                retval &= "_"
            Else
                retval &= c
            End If
        Next

        Return retval
    End Function
    Public Overridable Function getClassName() As String
        Return Replace(Me.GetType().ToString(), Assembly.GetExecutingAssembly().GetName().Name & ".", "")
    End Function

    Public Function getClassNameFriendly() As String

        Dim dot As Integer
        Dim full_class_name As String

        full_class_name = Me.getClassName

        dot = InStr(full_class_name, ".")
        If dot > 0 Then
            Return Mid(full_class_name, dot + 1)
        Else
            Return full_class_name
        End If
    End Function

    Protected Sub Msg(ByVal txt As String)
        Debug.Print(Me.getClassName() & " " & txt)
    End Sub

    Public Overridable Sub sendDestroyRequest()

        'disable JOmega
        Dim req As JSONObject

        req = New JSONObject
        req.ObjectType = "JOMEGA"
        req.Directive = "DESTROY"
        req.Sender = Me

        Call Me.getMailProvider().Send(req, Nothing, "JOmega")

    End Sub

    Public ReadOnly Property LibVersion As String
        Get
            Return "1.02 42013"
        End Get
    End Property

    Public Sub New()

    End Sub
End Class

Public Class TRINIDAT_HTTP_INITIALIZATION_RESULT

    Public state As TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE
    Public affected_parameter_id As String
    Public affected_parameter_value As String
    Public errormsg As String

    Public Sub New(ByVal status_code As TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE)
        Me.state = status_code
    End Sub

    Public ReadOnly Property haveErrorMessage As Boolean
        Get
            Return Len(Me.ErrorMessage) > 0
        End Get
    End Property
    Public Property ErrorMessage As String
        Get
            Return Me.errormsg
        End Get
        Set(ByVal value As String)
            Me.errormsg = value
        End Set
    End Property
End Class

Public Enum TRINIDAT_HTTP_INITIALIZATION_RESULT_CODE
    REQUIRED_PARAMETER_MISSING = -3
    PARAMETER_NOT_ACCEPTED = -2
    ERR = -1
    NEUTRAL = 0
    SUCCESS = 1
End Enum