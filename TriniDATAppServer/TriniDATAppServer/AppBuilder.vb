Option Explicit On
Option Compare Text
Imports System.IO
Imports System.Xml
Imports System.Web
Imports System.Text
Imports TriniDATDictionaries
Imports Newtonsoft.Json
Imports System.Reflection
Imports TriniDATServerStatTypes

Public Class BosswaveVisualAppEditor

    Public app As BosswaveApplication

    Public dp1 As TabPage
    Public dp2 As TabPage

End Class

Public Class BosswaveApplication

    Public app_xdoc As XDocument
    Public app_filepath As String
    Public app_role_interface As Boolean
    Private app_inherited_interface As BosswaveApplication
    Public app_name As String
    Private app_payment As payment_descriptor
    Private app_author As String
    Private app_author_email As String
    Private app_author_website As String
    Private app_node As XElement
    Private app_description As String
    Private app_techdescription As String
    Private app_originating_business As String
    Private app_copyrightstr As String
    Private app_releasestr As String
    Private app_author_skypeid As String
    Private app_author_yahooid As String
    Private app_author_phone As String
    Public app_mappingPoints As BosswaveAppMappingPoints
    Public Initiailized As Boolean
    Private myTag As Object
    Private app_actions As BosswaveAppActions
    Private myid As Long  'known in the application index
    Private app_disabled As Boolean
    Private app_disabled_reason As BOSSWAVE_APP_DISABLE_REASON

    Public Property Id As Long
        Get
            Return Me.myid
        End Get
        Set(ByVal value As Long)
            Me.myid = value
        End Set
    End Property
    Public Property Disabled As Boolean
        Get
            Return Me.app_disabled
        End Get
        Set(ByVal value As Boolean)
            Me.app_disabled = value
        End Set
    End Property
    Public Function getMappingPointByUserId(ByVal val As String) As mapping_point_descriptor

        If Not Me.haveMappingPoints Then Return Nothing

        For Each mp_descriptor In Me.ApplicationMappingPoints
            If mp_descriptor.haveUserId Then
                If mp_descriptor.UserId = val Then
                    Return mp_descriptor
                End If
            End If
        Next

        Return Nothing
    End Function
    Public Property DisableReason As bosswave_app_disable_reason
        Get
            Return Me.app_disabled_reason
        End Get
        Set(ByVal value As bosswave_app_disable_reason)
            Me.app_disabled_reason = value
        End Set
    End Property
    Public Property InheritedApplication As BosswaveApplication
        Get
            Return Me.app_inherited_interface
        End Get
        Set(ByVal value As BosswaveApplication)
            Me.app_inherited_interface = value
        End Set
    End Property
    Public ReadOnly Property DoesInherit As Boolean
        Get
            Return Not IsNothing(InheritedApplication)
        End Get
    End Property
    'app_techdescription
    Public Property ApplicationOriginatingBusiness As String
        Get
            Return Me.app_originating_business
        End Get
        Set(ByVal value As String)
            Me.app_originating_business = value
        End Set
    End Property
    Public ReadOnly Property haveOriginatingBusiness As Boolean
        Get
            Return Not IsNothing(Me.app_originating_business)
        End Get
    End Property
    Public Property ApplicationPaymentInfo As payment_descriptor
        Get
            Return Me.app_payment
        End Get
        Set(ByVal value As payment_descriptor)
            Me.app_payment = value
        End Set
    End Property
    Public Property ApplicationActions As BosswaveAppActions
        Get
            Return Me.app_actions
        End Get
        Set(ByVal value As BosswaveAppActions)
            Me.app_actions = value
        End Set
    End Property

    Public Property IsInitialized As Boolean
        Get
            Return Initiailized
        End Get
        Set(ByVal value As Boolean)
            Initiailized = value
        End Set
    End Property
    Public ReadOnly Property IsInterface As Boolean
        Get
            Return Me.app_role_interface
        End Get
    End Property

    Public Property InterfaceFlag As Boolean
        Get
            Return Me.app_role_interface
        End Get
        Set(ByVal value As Boolean)
            Me.app_role_interface = value
        End Set
    End Property
    Public Property Tag As Object
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
    Public Property ApplicationAuthor As String
        Get
            Return Me.app_author
        End Get
        Set(ByVal value As String)
            Me.app_author = value
        End Set
    End Property
  
    Public Property ApplicationAuthorContactEmail As String
        Get
            Return Me.app_author_email
        End Get
        Set(ByVal value As String)
            Me.app_author_email = value
        End Set
    End Property
    Public Property ApplicationAuthorContactWebsite As String
        Get
            Return Me.app_author_website
        End Get
        Set(ByVal value As String)
            Me.app_author_website = value
        End Set
    End Property
    Public ReadOnly Property haveApplicationPaymentInfo As Boolean
        Get
            Return Not IsNothing(Me.ApplicationPaymentInfo)
        End Get
    End Property
    Public ReadOnly Property haveApplicationCopyright As Boolean
        Get
            Return Me.app_copyrightstr <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthor As Boolean
        Get
            Return Me.app_author <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthorContactPhone As Boolean
        Get
            Return Me.app_author_phone <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthorContactYahoo As Boolean
        Get
            Return Me.app_author_yahooid <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthorContactSkype As Boolean
        Get
            Return Me.app_author_skypeid <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthorContactEmail As Boolean
        Get
            Return Me.app_author_email <> ""
        End Get
    End Property
    Public ReadOnly Property haveApplicationDescription() As Boolean
        Get
            If Not IsNothing(Me.app_description) Then
                Return Me.app_description.Length > 0
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveApplicationAuthorContactWebsite As Boolean
        Get
            Return Me.app_author_website <> ""
        End Get
    End Property
    Public Property ApplicationNode() As XElement
        Get
            Return Me.app_node
        End Get
        Set(ByVal value As XElement)
            Me.app_node = value
        End Set
    End Property
    Public ReadOnly Property haveMappingPoints As Boolean
        Get
            Return Not IsNothing(Me.ApplicationMappingPoints)
        End Get
    End Property
    Public Property ApplicationMappingPoints As BosswaveAppMappingPoints
        Get
            Return Me.app_mappingPoints
        End Get
        Set(ByVal value As BosswaveAppMappingPoints)
            Me.app_mappingPoints = value
        End Set
    End Property
    Public ReadOnly Property ApplicationMappingPoint(ByVal url As String) As mapping_point_descriptor
        Get
            If Not Me.haveMappingPoints Then Return Nothing
            If IsNothing(url) Then Return Nothing
            If url = "" Then Return Nothing

            Return Me.ApplicationMappingPoints.getDescriptorByURL(url)
        End Get

    End Property
    Public Property ApplicationReleaseStr As String
        Get
            Return Me.app_releasestr
        End Get
        Set(ByVal value As String)
            Me.app_releasestr = value
        End Set
    End Property
    Public Property ApplicationName As String
        Get
            Return Me.app_name
        End Get
        Set(ByVal value As String)
            Me.app_name = value
        End Set
    End Property
    Public Property ApplicationCopyrightString As String
        Get
            Return Me.app_copyrightstr
        End Get
        Set(ByVal value As String)
            Me.app_copyrightstr = value
        End Set
    End Property
    Public Property ApplicationAuthorYahooID As String
        Get
            Return Me.app_author_yahooid
        End Get
        Set(ByVal value As String)
            Me.app_author_yahooid = value
        End Set
    End Property
    Public Property ApplicationAuthorPhone As String
        Get
            Return Me.app_author_phone
        End Get
        Set(ByVal value As String)
            Me.app_author_phone = value
        End Set
    End Property
    Public Property ApplicationAuthorSkypeID As String
        Get
            Return Me.app_author_skypeid
        End Get
        Set(ByVal value As String)
            Me.app_author_skypeid = value
        End Set
    End Property
    Public Property ApplicationTechDescription As String
        Get
            Return Me.app_techdescription
        End Get
        Set(ByVal value As String)
            Me.app_techdescription = value
        End Set
    End Property

    Public Property ApplicationDescription As String
        Get
            Return Me.app_description
        End Get
        Set(ByVal value As String)
            Me.app_description = value
        End Set
    End Property
    Public Property XML As XDocument
        Get
            Return Me.app_xdoc
        End Get
        Set(ByVal value As XDocument)
            Me.app_xdoc = value
        End Set
    End Property

    Public Sub New(Optional ByVal xml As XDocument = Nothing)
        Me.XML = xml
    End Sub
    Public ReadOnly Property haveXML As Boolean
        Get
            Return Not IsNothing(Me.XML)
        End Get
    End Property
    Public Property Filepath As String
        Get
            Return app_filepath
        End Get
        Set(ByVal value As String)
            Me.app_filepath = value
        End Set
    End Property
    Public ReadOnly Property HomeFolder As DirectoryInfo
        Get
            Return New DirectoryInfo(New FileInfo(Me.Filepath).DirectoryName)
        End Get
    End Property
    Public Function Load() As BOSSWAVE_APP_DISABLE_REASON
        'returns error descriptor constant or NONE on success.
        'note:  @id in the application manifest is the product of the appi index. It never sets the actual app id.

        Dim abstract As Object
        Me.IsInitialized = False

        If Not Me.haveXML() Then
            Return BOSSWAVE_APP_DISABLE_REASON.MISSING_APP
        End If

        If Me.Disabled Then
            Return BOSSWAVE_APP_DISABLE_REASON.FLAGGED_DISABLED
        End If


        Try
            Dim payment_node As XElement

            Me.ApplicationNode = XML.Descendants("app")(0)

            payment_node = Me.ApplicationNode.Descendants("paymentdata")(0)
            If Not IsNothing(payment_node) Then
                Me.ApplicationPaymentInfo = payment_descriptor.createFromXPayment(payment_node)
            End If

            Me.InterfaceFlag = False
            If Not IsNothing(Me.ApplicationNode.@interface) Then
                If Me.ApplicationNode.@interface.ToString = "true" Then
                    Me.InterfaceFlag = True
                End If
            End If

            Me.InheritedApplication = Nothing

            If Not IsNothing(Me.ApplicationNode.@inherits) Then
                Dim inherited_app_id_str As String
                Dim inherited_app_id As Long

                inherited_app_id_str = Me.ApplicationNode.@inherits

                If IsNumeric(inherited_app_id_str) Then
                    inherited_app_id = CLng(inherited_app_id_str)
                    Me.InheritedApplication = GlobalObject.ApplicationCache.AppById(inherited_app_id)

                    If IsNothing(InheritedApplication) Then
                        GlobalObject.Msg(Me.ApplicationName & " initialization error: inheritance error. application with id " & inherited_app_id_str & " does not exist.")
                        Return BOSSWAVE_APP_DISABLE_REASON.MISSING_PROTOTYPE
                    End If

                    If Not Me.InheritedApplication.Load() Then
                        GlobalObject.Msg(Me.ApplicationName & " initialization error:  Interface '" & Me.InheritedApplication.ApplicationName & "' failed to load. Aborting own initialization.")
                        Return BOSSWAVE_APP_DISABLE_REASON.PROTOTYPE_LOAD_ERR
                    End If

                Else
                    GlobalObject.Msg(Me.ApplicationName & " initialization error: Invalid application id must be a integer value. Found: " & inherited_app_id_str)
                    Return BOSSWAVE_APP_DISABLE_REASON.INVALID_XML
                End If

            End If

            'ApplicationOriginatingBusiness
            If Not IsNothing(Me.ApplicationNode.@businessname) Then
                Me.ApplicationOriginatingBusiness = HttpUtility.UrlDecode(Me.ApplicationNode.@businessname)
            End If

            If Not IsNothing(Me.ApplicationNode.@copyright) Then
                Me.ApplicationCopyrightString = HttpUtility.UrlDecode(Me.ApplicationNode.@copyright)
            End If

            If Not IsNothing(Me.ApplicationNode.@released) Then
                Me.ApplicationReleaseStr = HttpUtility.UrlDecode(Me.ApplicationNode.@released)
            End If

            If Not IsNothing(Me.ApplicationNode.@author) Then
                Me.ApplicationAuthor = HttpUtility.UrlDecode(Me.ApplicationNode.@author)
            End If

            If Not IsNothing(Me.ApplicationNode.@authorcontactemail) Then
                Me.ApplicationAuthorContactEmail = HttpUtility.UrlDecode(Me.ApplicationNode.@authorcontactemail)
            End If

            If Not IsNothing(Me.ApplicationNode.@authorcontactwebsite) Then
                Me.ApplicationAuthorContactWebsite = HttpUtility.UrlDecode(Me.ApplicationNode.@authorcontactwebsite)
            End If

            If Not IsNothing(Me.ApplicationNode.@authorcontactskypeid) Then
                Me.ApplicationAuthorSkypeID = HttpUtility.UrlDecode(Me.ApplicationNode.@authorcontactskypeid)
            End If

            If Not IsNothing(Me.ApplicationNode.@authorcontactyahooid) Then
                Me.ApplicationAuthorYahooID = HttpUtility.UrlDecode(Me.ApplicationNode.@authorcontactyahooid)
            End If

            If Not IsNothing(Me.ApplicationNode.@authorcontactphone) Then
                Me.ApplicationAuthorPhone = HttpUtility.UrlDecode(Me.ApplicationNode.@authorcontactphone)
            End If

            If Not IsNothing(Me.ApplicationNode.@description) Then
                Me.ApplicationDescription = HttpUtility.UrlDecode(Me.ApplicationNode.@description)
            End If

         
            Me.ApplicationActions = BosswaveAppActions.createFromXApp(Me.ApplicationNode)
            abstract = BosswaveAppMappingPoints.createFromXMPS(Me, XML.Descendants("mps")(0))

            If TypeOf abstract Is BosswaveAppMappingPoints Then
                'OK
                Me.ApplicationMappingPoints = CType(abstract, BosswaveAppMappingPoints)
            Else
                Return CType(abstract, BOSSWAVE_APP_DISABLE_REASON)
            End If

            'validate dependency file paths
            Dim mp_validate_error As BOSSWAVE_APP_DISABLE_REASON
            mp_validate_error = Me.ApplicationMappingPoints.Validate((GlobalObject.server.ServerMode = TriniDATServerTypes.TRINIDAT_SERVERMODE.MODE_DEV))

            If mp_validate_error = BOSSWAVE_APP_DISABLE_REASON.NONE Then

                'update dynamic vars/urls
                Dim var_appname As String
                var_appname = Replace(Me.ApplicationName.ToLower().Trim(), " ", "_")

                For Each mp_desc In Me.ApplicationMappingPoints
                    mp_desc.MappingPoint.setURI(Replace(mp_desc.MappingPoint.getURI(), "{appname}", var_appname))
                Next

                'format a quick full url attrib.
                Dim req As TriniDATRequestInfo

                req = New TriniDATRequestInfo
                req.associated_app = Me
                req.mapping_point_desc = Nothing
                Me.ApplicationNode.@fullurl = req.FullServerURL


                Me.IsInitialized = True

                Return BOSSWAVE_APP_DISABLE_REASON.NONE

            Else
                GlobalObject.MsgColored("Errors found in app " & Me.Filepath & ". Aborting initialization procedure.", Color.Red)
                Return BOSSWAVE_APP_DISABLE_REASON.INVALID_MAPPING_POINT_CONFIG
            End If

        Catch ex As Exception
            MsgBox("App Load error: " & ex.Message)
            Return False
        End Try

    End Function


    Public Shared Function updateRuntimeCount(ByVal appid As Long) As Boolean

        Dim app As BosswaveApplication
        Dim app_node As XElement
        Dim usage_count As Long

        app = GlobalObject.ApplicationCache.AppById(appid)
        app_node = app.ApplicationNode

        usage_count = 0


        If Not IsNothing(app_node.@runtimecount) Then
            If IsNumeric(app_node.@runtimecount) Then
                usage_count = CLng(app_node.@runtimecount)
            End If
        End If

        usage_count += 1
        app_node.@runtimecount = usage_count.ToString


        Try
            File.WriteAllText(app.Filepath, app_node.ToString)

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Public Function Write() As Boolean
        Try

            File.WriteAllText(Me.Filepath, Me.XML.ToString)
            Return True
        Catch ex As Exception

            Return False
        End Try
    End Function
    Public Sub Msg(ByVal txt As String)
        txt = "App build: " & txt

        GlobalObject.Msg(txt)

        MsgBox(txt)
    End Sub
    Public Shared Function createNew(Optional ByVal filename As String = "empty.xml") As BosswaveApplication

        Dim app_xml As String
        Dim app_xdoc As XDocument
        Dim retval As BosswaveApplication

        app_xml = "<app name="""" author=" & Chr(34) & My.User.Name & "@" & My.Computer.Name & Chr(34) & "><mp url=""""><dp id=""1""><action><parameters><parameter/><parameters/><StatDropConditions><StatDropCondition><parameter/></StatDropCondition></StatDropConditions></action></dp></mp><dp id=""2""><action><parameters><parameter/><parameters/><StatDropConditions><StatDropCondition><parameter/></StatDropCondition></StatDropConditions></action></dp></mp></app>"
        app_xdoc = XDocument.Parse(app_xml)

        retval = New BosswaveApplication(app_xdoc)
        retval.Filepath = GlobalSetting.getBuilderXMLPath() & filename

        Return retval

    End Function

    Public Function getMappingPointLevel() As XElement
        Return Me.getAppLevel().Descendants("mp")
    End Function
    Public Function getAppLevel() As XElement
        If Not Me.haveXML Then Return Nothing

        Return Me.XML.Descendants("App")
    End Function
    Public Function getDPLevel(ByVal x As Integer) As XElement
        If Not Me.haveXML Then Return Nothing

        Return Me.getMappingPointLevel().Descendants("dp" & x.ToString)
    End Function
    Public Function getActionLevel(ByVal dp_id As Integer) As XElement
        If Not Me.haveXML Then Return Nothing

        Return Me.getDPLevel(dp_id).Descendants("action")
    End Function

    Public Shared Function getBuilderFunctionListXML() As XDocument
        Return XDocument.Parse(File.ReadAllText(GlobalSetting.getBuilderXMLPath() & "allfunctions.xml"))
    End Function
    Public Shared Function getBuilderPrototypeListXML() As XDocument
        Return XDocument.Parse(File.ReadAllText(GlobalSetting.getappPrototypeDeclarationPath()))
    End Function
    Public Shared Function getBuilderOperatorListXML() As XDocument
        Return XDocument.Parse(File.ReadAllText(GlobalSetting.getBuilderXMLPath() & "alloperators.xml"))
    End Function
    Public Shared Function getBuilderConditionListXML() As XDocument
        Return XDocument.Parse(File.ReadAllText(GlobalSetting.getBuilderXMLPath() & "allconditions.xml"))
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class



Public Class BosswaveBoolean

    Public Shared Function IsValidBool(ByVal val As String) As Boolean

        If ((val = "true" Or val = "false") Or (val = "0" Or val = "1")) Then
            Return True
        End If

        Return False
    End Function
    Public Shared ReadOnly Property stringBool(ByVal val As String) As Boolean
        Get

            If val = "true" Or val = "1" Or val = "yes" Then
                Return True
            End If

            Return False
        End Get
    End Property
End Class
Public Class BosswaveOperator

    Public myid As String
    Public Inversed As Boolean
    Public myTag As Object
    Public myname As String
    Public lefthand_prototypeid As String
    Public righthand_prototypeid As String

    Public Function Match(ByVal val1 As BosswaveAppParameter, ByVal user_val As String) As BosswaveStatCompareResult

        Dim retval As BosswaveStatCompareResult

        If Me.Name = "TRUE" Then
            retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
            GoTo GOT_RESULT
        End If

        If Me.Name = "EQ" Then
            retval = Me.OperatorEQ(val1, user_val)
            GoTo GOT_RESULT
        End If

        If Left(Me.Name, 3) = "HAS" Then
            retval = Me.OperatorHAS(val1, user_val)
            GoTo GOT_RESULT
        End If


NO_RESULT:
        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.UNKNOWN_OPERATOR, Nothing)

GOT_RESULT:
        If Me.Inversed Then
            retval = retval.getInversedResult
        End If

        Return retval
    End Function
    Private Function OperatorHAS_String(ByVal system_value As BosswaveAppParameter, ByVal user_val As String) As BosswaveStatCompareResult
        Dim dictionarize As TriniDATWordDictionary

        dictionarize = New TriniDATWordDictionary("", New List(Of String)({system_value.Value}))

        If dictionarize.HasIn(user_val) Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
        Else
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
        End If
    End Function

    Private Function OperatorHAS_JSON(ByVal system_value As BosswaveAppParameter, ByVal json_field_value As String) As BosswaveStatCompareResult

        Dim dserialized_statistic As Object
        Dim reflection_field_name As String
        Dim sep As Integer
        Dim match_varname As String
        Dim match_varname_length As Integer

        'dserialized("email").ToString 

        Try
            'unpack object
            dserialized_statistic = JsonConvert.DeserializeObject(json_field_value)

            sep = InStr(system_value.LinkedObject.FieldName, ".") 'LinkedObject = bosswave stat condition

            If sep > 0 Then
                reflection_field_name = Mid(system_value.LinkedObject.FieldName, sep + 1)
            Else
                reflection_field_name = system_value.LinkedObject.FieldName
            End If

            If system_value.PrototypeId = "String" Then
                '"email": "empowermentmediagroup@yahoo.ca"
                match_varname = Chr(34) & reflection_field_name & Chr(34)
                match_varname_length = Len(match_varname)

                For Each fld In dserialized_statistic
                    Dim strfield As String

                    strfield = fld.ToString

                    If Left(strfield, match_varname_length) = match_varname Then
                        Dim value_start As Integer
                        Dim value_end As Integer
                        Dim realtime_json_value As String

                        value_start = InStr(match_varname_length + 1, strfield, Chr(34))
                        If value_start = 0 Then
                            Exit For
                        End If


                        value_start += 1 'skip chr(34)
                        value_end = Len(strfield)

                        realtime_json_value = Mid(strfield, value_start, value_end - value_start)
                        'found var
                        Dim string_parameter As BosswaveAppParameter
                        string_parameter = New BosswaveAppParameter("String", realtime_json_value)


                        Return OperatorHAS_String(string_parameter, system_value.Value)

                    End If

                    fld = fld

                Next

            Else
                Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.UNSUPPORTED_DATATYPE)
            End If


            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)


            'reflect and find what
        Catch ex As Exception
            'invalid json
            GlobalObject.Msg("Unable to decode statistic with JSON: " & ex.Message & " field.value: " & json_field_value)
        End Try

    End Function

    Private Function OperatorHAS(ByVal system_value As BosswaveAppParameter, ByVal user_val As String) As BosswaveStatCompareResult

        If Me.Name = "HASJSON" Then

            'search in JSON structure
            If system_value.PrototypeId = "String" Then
                Return OperatorHAS_JSON(system_value, user_val)
            End If

        End If

        'search in string
        If Me.Name = "HASSTRING" Then
            If system_value.PrototypeId = "String" Then
                Return OperatorHAS_String(system_value, user_val)
            End If
        End If

        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.UNSUPPORTED_DATATYPE)

    End Function


    Private Function OperatorEQ(ByVal system_value As BosswaveAppParameter, ByVal user_val As String) As BosswaveStatCompareResult

        If system_value.PrototypeId = "String" Then
            Return Me.MatchString(system_value.Value, user_val)
        End If

        If system_value.PrototypeId = "Boolean" Then
            Return Me.MatchBoolean(system_value.Value, user_val)
        End If

        If system_value.PrototypeId = "Integer" Then
            Return Me.MatchInt(system_value.Value, user_val)
        End If

        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.UNKNOWN_PROTOTYPE)

    End Function

    Private Function MatchInt(ByVal val1 As String, ByVal val2 As String) As BosswaveStatCompareResult

        If Not IsNumeric(val1) Then Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_LEFTHAND_DATATYPE)
        If Not IsNumeric(val2) Then Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_RIGHTHAND_DATATYPE)

        If CLng(val1) = CLng(val2) Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
        End If

        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
    End Function
    Private Function MatchString(ByVal val1 As String, ByVal val2 As String) As BosswaveStatCompareResult

        If val1 = val2 Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
        End If

        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
    End Function

    Private Function MatchBoolean(ByVal val1 As String, ByVal val2 As String) As BosswaveStatCompareResult

        If Not BosswaveBoolean.IsValidBool(val1) Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_LEFTHAND_DATATYPE)
        End If

        If Not BosswaveBoolean.IsValidBool(val2) Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_RIGHTHAND_DATATYPE)
        End If

        If BosswaveBoolean.stringBool(val1) = BosswaveBoolean.stringBool(val2) Then
            Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
        End If

        Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
    End Function
    Public Property Name As String
        Get
            Return Me.myname
        End Get
        Set(ByVal value As String)
            Me.myname = value
        End Set
    End Property
    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property

    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    Public Property OperatorId As String
        Get
            Return Me.myid
        End Get
        Set(ByVal value As String)
            Me.myid = value
        End Set
    End Property
    Public ReadOnly Property CompilerFunctionName As String
        Get

            Return "BosswaveApplicationLib.Compile" & Me.myid & IIf(Inversed, "Inversed", "")
        End Get
    End Property

    Public ReadOnly Property Declaration As XElement
        Get
            If Me.Inversed Then
                Return Me.InversalDeclaration
            Else
                Return Me.NormalDeclaration
            End If
        End Get
    End Property
    Public ReadOnly Property NormalDeclaration As XElement
        Get
            Dim prototype As XElement

            prototype = From operator_dec In BosswaveApplication.getBuilderOperatorListXML().Descendants("operator") Where operator_dec.@id.ToString = Me.OperatorId

            Return prototype

        End Get
    End Property

    Public ReadOnly Property InversalDeclaration As XElement
        Get
            Dim prototype As XElement

            prototype = From operator_dec In BosswaveApplication.getBuilderOperatorListXML().Descendants("operator") Where operator_dec.@id.ToString = Me.OperatorId And operator_dec.@inversaloperatorid.ToString = "1"

            Return prototype

        End Get
    End Property
    Public Shared Function createByID(ByVal x As Integer)
        '<parameter protypeid="ServerProfileSelector" usertitle="Select Server Profile"></parameter>
        Dim operator_item As BosswaveOperator
        Dim operator_xml As XDocument

        operator_xml = BosswaveApplication.getBuilderOperatorListXML()

        If IsNothing(operator_xml) Then
            Return Nothing
        End If

        Dim q = From operator_dec In operator_xml.Descendants("operators").Descendants("operator") Where operator_dec.@id.ToString = x.ToString
        Dim xop As XElement

        xop = q(0)

        operator_item = New BosswaveOperator
        operator_item.OperatorId = xop.@id
        operator_item.Name = xop.@operatorname
        operator_item.LeftHand = xop.@lefthand_prototypeid
        operator_item.RightHand = xop.@righthand_prototypeid

        'operator_item.myid = xstatdropcondition.@id

        If Not IsNothing(xop.@inversed) Then
            operator_item.Inversed = (xop.@inversed.ToString = "1")
        Else
            operator_item.Inversed = False
        End If

        Return operator_item

    End Function
    Public Property LeftHand() As String
        Get
            Return Me.lefthand_prototypeid
        End Get
        Set(ByVal value As String)
            Me.lefthand_prototypeid = value
        End Set
    End Property
    Public Property RightHand() As String
        Get
            Return Me.righthand_prototypeid
        End Get
        Set(ByVal value As String)
            Me.righthand_prototypeid = value
        End Set
    End Property
End Class

Public Class BosswaveAppMappingPoints
    Inherits mapping_point_descriptors

    Public myTag As Object



    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property
    Public Function getByUserId(ByVal val As String) As mapping_point_descriptor

        For Each mp In Me

            If mp.haveUserId Then
                If mp.UserId = val Then
                    Return mp
                End If
            End If
        Next

        Return Nothing
    End Function
    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property
    Public Shared Function updateRuntimeCount(ByVal appid As Long, ByVal mp_url As String) As Boolean

        Dim app As BosswaveApplication
        Dim app_node As XElement
        Dim usage_count As Long

        app = GlobalObject.ApplicationCache.AppById(appid)
        app_node = app.ApplicationNode


        Dim q = From mapping_point In app_node.Descendants("mps").Descendants("mp") Where mapping_point.@url.ToString = mp_url

        For Each mp_node As XElement In q
            usage_count = 0


            If Not IsNothing(mp_node.@runtimecount) Then
                If IsNumeric(mp_node.@runtimecount) Then
                    usage_count = CLng(mp_node.@runtimecount)
                End If
            End If

            usage_count += 1
            mp_node.@runtimecount = usage_count.ToString

        Next

        Try
            File.WriteAllText(app.Filepath, app_node.ToString)

            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Shared Function createFromXMPS(ByVal parent_app As BosswaveApplication, ByVal xmps As XElement) As Object
        Dim retval As BOSSWAVE_APP_DISABLE_REASON

        Dim q = From xmapping_point In xmps.Descendants("mp")
        Dim all_mappingpoints As BosswaveAppMappingPoints

        all_mappingpoints = New BosswaveAppMappingPoints

        For Each mp_node In q
            Dim mp_jclasses_dependencies As List(Of String)
            Dim mp_jclasses As List(Of mappingPointClass)
            Dim mp_info As mapping_point_descriptor

            mp_jclasses_dependencies = Nothing

            mp_info = New mapping_point_descriptor(parent_app.Id, mp_node)
            Dim mp_encoding As Encoding

            mp_encoding = Nothing

            If Not IsNothing(mp_node.@encoder) Then
                If mp_node.@encoder.ToString = "UTF-8" Then
                    mp_encoding = New UTF8Encoding
                End If
            End If

            If IsNothing(mp_encoding) Then
                mp_encoding = New ASCIIEncoding
            End If

            'maxthreadlifetime
            mp_info.ThreadToleranceSec = 50
            If Not IsNothing(mp_node.@maxthreadlifetime) Then
                If IsNumeric(mp_node.@maxthreadlifetime.ToString) Then
                    mp_info.ThreadToleranceSec = CInt(mp_node.@maxthreadlifetime)
                End If
            End If

            'INTERFACE MODE FLAGS
            '====
            If parent_app.IsInterface Then
                mp_info.MustOverrideFlag = False
                If Not IsNothing(mp_node.@mustoverride) Then
                    If mp_node.@mustoverride.ToString = "true" Then
                        mp_info.MustOverrideFlag = True
                    End If
                End If
            End If

            mp_jclasses = New List(Of mappingPointClass)

            'add core functionality + related dependencies
            mp_jclasses.Add(New mappingPointClass("JAlpha", "Mapping Point Bootstrap"))

            'stats enabled?
            If Not IsNothing(mp_node.@enablestats) Then
                If mp_node.@enablestats.ToString = "true" Then
                    If parent_app.IsInterface Then
                        GlobalObject.Msg("Warning: Ignored enablestats directive. Interfaces can't log statistics.")
                    Else
                        mp_jclasses.Add(New mappingPointClass("JKernelReflection", "Reflection messaging services"))
                        mp_jclasses.Add(New mappingPointClass("JStats", "Provides statistical services"))
                    End If
                End If
            End If

            If Not IsNothing(mp_node.@interactiveconsolefeatures) Then
                If mp_node.@interactiveconsolefeatures.ToString = "true" Then
                    If parent_app.IsInterface Then
                        GlobalObject.Msg("Warning: Ignored interactiveconsolefeatures directive. Interfaces do no have executable code.")
                    Else
                        mp_jclasses.Add(New mappingPointClass("JInteractiveConsole", "Output text from within webservices"))
                    End If
                End If
            End If

            Dim jclass_nodes = mp_node.Descendants("jclass")

            'add user classes
            For Each jclass_node As XElement In jclass_nodes
                Dim mp_jclass As mappingPointClass

                mp_jclass = New mappingPointClass()
                mp_jclass.setName(jclass_node.@id)

                If Not IsNothing(jclass_node.@description) Then
                    mp_jclass.Description = jclass_node.@description
                Else
                    mp_jclass.Description = mp_jclass.getName() & " instance."
                End If

                mp_jclasses.Add(mp_jclass)

            Next 'user class

            mp_jclasses.Add(New mappingPointClass("JOmega", "End point configurator"))

            'load external dependencies

            Dim mp_all_class_dependencies_node = mp_node.Descendants("dependencies")

            'load dependency list
            If mp_all_class_dependencies_node.Count > 0 Then

                Dim dependency_filepath As String
                Dim mp_single_class_dependency_node = mp_all_class_dependencies_node.Descendants("dependency")
                mp_jclasses_dependencies = New List(Of String)

                For Each jclass_dependency As XElement In mp_single_class_dependency_node
                    dependency_filepath = jclass_dependency.@path

                    If Not IsNothing(dependency_filepath) Then
                        If InStr(dependency_filepath, "\") = 0 Then
                            'relative path = home dir.
                            dependency_filepath = parent_app.HomeFolder.FullName & "\" & dependency_filepath
                        End If

                        mp_jclasses_dependencies.Add(dependency_filepath)
                    End If

                Next 'dependency

            End If

            Dim mp_url As String

            If Not IsNothing(mp_node.@url) Then
                mp_url = mp_node.@url
            Else
                GlobalObject.Msg("Error: Mapping point missing url @" & mp_node.ToString)
                retval = New BOSSWAVE_APP_DISABLE_REASON
                retval = BOSSWAVE_APP_DISABLE_REASON.INVALID_MAPPING_POINT_CONFIG
                Return retval
            End If

            'validate mapping point URL
            If Left(mp_url, 1) <> "/" Then
                GlobalObject.Msg("Error: invalid mapping point url. A mapping point url must start with forward slash / @ " & mp_node.ToString)
                retval = New BOSSWAVE_APP_DISABLE_REASON
                retval = BOSSWAVE_APP_DISABLE_REASON.INVALID_MAPPING_POINT_CONFIG
                Return retval
            End If

            If Right(mp_url, 1) <> "/" Then
                mp_url &= "/"
            End If

            mp_info.URL = mp_url
            mp_info.MappingPoint = New mappingPointRoot(Nothing, mp_jclasses, mp_encoding, Nothing, mp_jclasses_dependencies)

            If Not IsNothing(mp_node.@description) Then
                mp_info.MappingPoint.Description = mp_node.@description
            End If


            'format a quick full url attrib.
            Dim req As TriniDATRequestInfo

            req = New TriniDATRequestInfo
            req.associated_app = parent_app
            req.mapping_point_desc = mp_info
            mp_node.@fullurl = req.FullServerURL

            all_mappingpoints.Add(mp_info)

        Next 'mapping point


        Return all_mappingpoints
    End Function

    Public Function Validate(ByVal console_log_errors As Boolean) As BOSSWAVE_APP_DISABLE_REASON

        If Not Me.haveDescriptors Then
            Return BOSSWAVE_APP_DISABLE_REASON.NONE
        End If

        'check if dependency file paths exist.

        For Each mp_des In Me

            If mp_des.haveMappingPointInstance Then

                For Each dependency_path In mp_des.MappingPoint.getDependencyPaths()

                    'skip GAC strings.
                    If InStr(dependency_path, "culture") = 0 And dependency_path.Trim <> "" Then

                        If Not File.Exists(dependency_path) Then

                            If console_log_errors Then
                                GlobalObject.MsgColored("Dependency file '" & dependency_path & "' not found.", Color.Red)
                            End If

                            Return BOSSWAVE_APP_DISABLE_REASON.DEPENDENCY_FILE_NOT_FOUND
                        End If

                    End If

                Next

            End If

        Next

        Return BOSSWAVE_APP_DISABLE_REASON.NONE
    End Function

    Public Function ToXML() As String

        Dim xml As String

        'xml = "<mp"
        'xml &= " url="
        'xml &= Chr(34) & HttpUtility.UrlEncode(Me.URL) & Chr(34)
        'xml &= "> "
        'xml &= Me.DataProcessor1.ToXML
        'xml &= Me.DataProcessor2.ToXML
        'xml &= "</mp>"

        Return xml

    End Function

    Public Sub New()

    End Sub
End Class
Public Class BosswaveAppActions
    Inherits List(Of BosswaveAppAction)

    Public myid As Integer

    Public Shared Function createFromXApp(ByVal xapp As XElement)

        Dim q = From action_entry In xapp.Descendants("action")
        Dim retval As BosswaveAppActions

        retval = New BosswaveAppActions

        For Each action_entry As XElement In q
            retval.Add(BosswaveAppAction.createFromXAction(action_entry))

        Next

        Return retval
    End Function

    Public Function ToXML() As String

        Dim xml As String

        'xml = "<dp"
        'xml &= " id="
        'xml &= Chr(34) & Me.Id & Chr(34)
        'xml &= "> "
        'xml &= Me.Action.ToXML()
        'xml &= "</dp>"

        Return xml

    End Function
End Class

Public Class BosswaveOperators
    Inherits List(Of BosswaveOperator)
    Public myTag As Object
    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property

    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    'Public Shared Function createFromXOperators(ByVal xoperators As XElement)
    '    Dim operator_item As BosswaveOperator
    '    Dim retval As BosswaveOperators

    '    retval = New BosswaveOperators

    '    Dim q = From operator_node In xoperators.Descendants("operators")

    '    For Each op In q
    '        operator_item = BosswaveOperator.createFromXOperator(op)
    '        retval.Add(operator_item)

    '    Next

    '    Return retval
    'End Function

End Class


Public Class BosswaveAppParameterList
    Inherits List(Of BosswaveAppParameter)
    Public myTag As Object
    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property
    Public Function getByDisplayName(ByVal val As String) As BosswaveAppParameter

        For Each p In Me
            If p.displayName = val Then
                Return p
            End If
        Next

        Return Nothing
    End Function
    Public Function ToXML() As XElement

        Dim retval As XNode

        retval = XElement.Parse("<parameters/>")

        For Each para In Me

            retval.AddAfterSelf(para.ToXML())
        Next

        Return retval
    End Function
    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    Public Shared Function createFromXAction(ByVal xaction As XElement) As BosswaveAppParameterList

        Dim retval As BosswaveAppParameterList

        retval = New BosswaveAppParameterList

        Dim q = From parameter_node In xaction.Descendants("parameters").Descendants("parameter")

        For Each xparameter In q

            Dim np As BosswaveAppParameter
            np = BosswaveAppParameter.createFromXParameter(xparameter)


            retval.Add(np)

        Next

        Return retval
    End Function
    Public Shared Function createFromXConditionalStat(ByVal xcs As XElement) As BosswaveAppParameterList

        Dim retval As BosswaveAppParameterList

        retval = New BosswaveAppParameterList

        Dim q = From parameter_node In xcs.Descendants("parameter")

        For Each xparameter In q

            Dim np As BosswaveAppParameter
            np = BosswaveAppParameter.createFromXParameter(xparameter)


            retval.Add(np)

        Next

        Return retval
    End Function
End Class
Public Class BosswaveStatDropConditions
    Inherits List(Of BosswaveStatDropCondition)
    Public myTag As Object
    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property

    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    Public Shared Function createFromXStatDropConditions(ByVal xstatconditions As XElement) As BosswaveStatDropConditions
        Dim trigger_item As BosswaveStatDropCondition
        Dim retval As BosswaveStatDropConditions

        retval = New BosswaveStatDropConditions

        Dim q = From operator_node In xstatconditions.Descendants("statdropcondition")

        For Each op In q
            trigger_item = BosswaveStatDropCondition.createFromXStatDropCondition(op)
            retval.Add(trigger_item)
        Next

        Return retval

    End Function

    Public Function ToXML() As XElement
        Dim xml As String

        xml = "<StatDropConditions>"

        For Each trigger In Me
            xml &= trigger.ToXML().ToString
        Next

        xml &= "</StatDropConditions>"


        Return XElement.Parse(xml)
    End Function
End Class

Public Class BosswaveStatDropCondition

    Private field_name As String
    Private current_value_field As BosswaveAppParameter
    Private myoperator As BosswaveOperator
    Private mydescription As String
    Private lpc As BosswaveStatCompareResult
    Public Shadows myTag As Object
    Private parameters_list As BosswaveAppParameterList
    Private match_mode As String 'and or
    Private node_blueprint As XElement
    Private last_compare_result As BosswaveStatCompareResult
    Public Shadows Property MatchMode As String
        Get
            Return match_mode
        End Get
        Set(ByVal value As String)
            match_mode = value
        End Set
    End Property
    Public Property LastCompareResult As BosswaveStatCompareResult
        Get
            Return Me.last_compare_result
        End Get
        Set(ByVal value As BosswaveStatCompareResult)
            Me.last_compare_result = value
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
    Public Shadows Property OriginalNode As XElement
        Get
            Return node_blueprint
        End Get
        Set(ByVal value As XElement)
            node_blueprint = value
        End Set
    End Property
    Public Shadows Property LastParameterCompareResult As BosswaveStatCompareResult
        Get
            Return Me.lpc
        End Get
        Set(ByVal value As BosswaveStatCompareResult)
            Me.lpc = value
        End Set
    End Property
    Public ReadOnly Property Match(ByVal incoming_parameter As String) As BosswaveStatCompareResult
        Get
            Dim par_results As List(Of BosswaveStatCompareResultVal)
            Dim x As Integer
            Dim master_result As BosswaveStatCompareResult
            par_results = New List(Of BosswaveStatCompareResultVal)


            For x = 0 To Me.parameters_list.Count - 1

                Me.CurrentValue = Me.parameters_list.Item(x)
                Me.CurrentValue.LinkedObject = Me

                par_results.Add(Me.MatchCurrent(incoming_parameter).Result)

            Next

            If Me.match_mode = "OR" Then
                Dim or_result As Boolean
                Dim temp As Boolean

                For Each result In par_results

                    temp = (result = BosswaveStatCompareResultVal.MATCH)

                    If temp And Not or_result Then
                        or_result = True
                    End If
                Next

                If or_result Then
                    master_result = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
                Else
                    master_result = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
                End If

                Return master_result

            ElseIf Me.match_mode = "AND" Then
                Dim and_result As Boolean

                For Each result In par_results

                    and_result = (result = BosswaveStatCompareResultVal.MATCH)

                    If Not and_result Then
                        'and chain broken
                        master_result = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
                        Return master_result
                    End If
                Next

                master_result = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
                Return master_result
            Else
                GlobalObject.Msg("Unknown comparison operator: " & MatchMode.ToString & " @ " & Me.OriginalNode.ToString)
            End If


        End Get
    End Property

    Private ReadOnly Property MatchCurrent(ByVal incoming_parameter As String) As BosswaveStatCompareResult
        'parameter = me.value
        Get

            Me.LastParameterCompareResult = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)

            'INTERNAL/EXERNAL COMPARE
            If Not Me.CurrentValue.isValidValue(incoming_parameter) = BosswaveStatCompareResultVal.MATCH Then
                Me.LastParameterCompareResult = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_RIGHTHAND_DATATYPE)
                Return Me.LastParameterCompareResult
            End If

            'EXTERNAL COMPARE
            If Me.CurrentValue.haveValidDLLModule Then
                Dim external_prototype_operator As MethodInfo

                external_prototype_operator = Me.CurrentValue.getExternalPrototypeOperator(Me.FieldOperator.Name, Me.FieldOperator.Inversed, incoming_parameter)

                If Not IsNothing(external_prototype_operator) Then
                    'get instance
                    Dim external_prototype_obj As Object
                    Dim ext_retval As Boolean
                    external_prototype_obj = Me.CurrentValue.getPrototypeInstance(Me.CurrentValue)
                    ext_retval = external_prototype_operator.Invoke(external_prototype_obj, New Object() {incoming_parameter})

                    If ext_retval Then
                        Me.LastParameterCompareResult = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH)
                    Else
                        Me.LastParameterCompareResult = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH)
                    End If

                    Return Me.LastParameterCompareResult
                End If

            Else


                'INTERNAL COMPARE
                Me.LastParameterCompareResult = Me.FieldOperator.Match(Me.CurrentValue, incoming_parameter)
                Return Me.LastParameterCompareResult
            End If

        End Get
    End Property


    Public Shadows Property FieldName As String
        Get
            Return field_name
        End Get
        Set(ByVal value As String)
            field_name = value
        End Set
    End Property
    Public Shadows Property FieldOperator As BosswaveOperator
        Get
            Return myoperator
        End Get
        Set(ByVal value As BosswaveOperator)
            myoperator = value
        End Set
    End Property
    Public Function ToXML() As XElement
        Dim xml As String

        xml = "<StatDropCondition operator="
        xml &= Chr(34) & FieldOperator.OperatorId.ToString & Chr(34) & " "
        xml &= "usertitle="
        xml &= Chr(34) & HttpUtility.UrlEncode(Me.displayName.ToString) & Chr(34) & ">"

        Return XElement.Parse(xml)
    End Function
    Public Shadows ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    Public Property displayName As String
        Get
            Return Me.mydescription
        End Get
        Set(ByVal value As String)
            Me.mydescription = value
        End Set
    End Property
    Public Property currentInnerValue As String
        Get
            Return Me.CurrentValue.Value
        End Get
        Set(ByVal value As String)
            Me.CurrentValue.Value = value
        End Set
    End Property

    Public Property CurrentValue As BosswaveAppParameter
        Get
            Return Me.current_value_field
        End Get
        Set(ByVal value As BosswaveAppParameter)
            Me.current_value_field = value
        End Set
    End Property

    Public Shared Function createFromXStatDropCondition(ByVal xtrigger As XElement)

        Dim retval As BosswaveStatDropCondition

        retval = New BosswaveStatDropCondition
        If Not IsNothing(xtrigger.@name) And Not IsNothing(xtrigger.@operatorid) Then
          
            If Not IsNothing(xtrigger.@usertitle) Then
                retval.displayName = HttpUtility.UrlDecode(xtrigger.@usertitle)
            Else
                retval.displayName = "untitled drop condition"
            End If

            retval.FieldOperator = BosswaveOperator.createByID(xtrigger.@operatorid)
            retval.FieldName = xtrigger.@name
            retval.parameters_list = BosswaveAppParameterList.createFromXConditionalStat(xtrigger)
            retval.node_blueprint = xtrigger


            If Not IsNothing(xtrigger.@match) Then
                'defines how outcome of multiple parameter-conditions are handled
                retval.MatchMode = xtrigger.@match
            Else
                retval.MatchMode = "and"
            End If

            Return retval
        Else
            Return Nothing
        End If
    End Function

    Public Sub New()
        Me.CurrentValue = New BosswaveAppParameter()
        Me.CurrentValue.PrototypeId = "String"
    End Sub
End Class

Public Class BosswaveAppAction
    Private myid As String
    Private myJClassName As String
    Private name As String
    Private Type As String
    Private myparameters As BosswaveAppParameterList
    Private triggers As BosswaveStatDropConditions
    Private myTag As Object
    Private xactionNode As XElement
    Private execution_context As STAT_EXECUTION_CONTEXT
    Public action_mapping_point_url As String

    Public Property URL As String
        Get
            Return Me.action_mapping_point_url
        End Get
        Set(ByVal value As String)
            Me.action_mapping_point_url = value
        End Set
    End Property
    Public Property ActionExecutionContext As STAT_EXECUTION_CONTEXT
        Get
            Return Me.execution_context
        End Get
        Set(ByVal value As STAT_EXECUTION_CONTEXT)
            Me.execution_context = value
        End Set
    End Property

    Public ReadOnly Property Declaration As XElement
        Get
            Dim prototype As XElement

            prototype = From function_dec In BosswaveApplication.getBuilderOperatorListXML().Descendants("actions") Where function_dec.@name.ToString = Me.Id

            Return prototype

        End Get
    End Property
    Public Property JClassName As String
        Get
            Return Me.myJClassName
        End Get
        Set(ByVal value As String)
            Me.myJClassName = value
        End Set
    End Property

    Public Property ActionName As String
        Get
            Return Me.Name
        End Get
        Set(ByVal value As String)
            Me.name = value
        End Set
    End Property
    Public Function haveParameters() As Boolean
        Return Not IsNothing(Me.Parameters)
    End Function

    Public Function haveDropConditions() As Boolean
        Return Not IsNothing(Me.triggers)
    End Function

    Public Shared Function createFromXAction(ByVal xaction As XElement) As BosswaveAppAction
        Dim retval As BosswaveAppAction

        retval = New BosswaveAppAction
        retval.Id = xaction.@id

        If Not IsNothing(xaction.@triggeraction) Then
            retval.ActionName = xaction.@triggeraction
        End If

        If Not IsNothing(xaction.@jclass) Then
            retval.JClassName = xaction.@jclass
        End If

        If Not IsNothing(xaction.@filterurl) Then
            retval.URL = xaction.@filterurl
        End If

        'set exec context.
        retval.ActionExecutionContext = STAT_EXECUTION_CONTEXT.CONTEXT_UNKNOWN

        If Not IsNothing(xaction.@triggercontext) Then
            Select Case xaction.@triggercontext.ToString

                Case "webservice"
                    retval.ActionExecutionContext = STAT_EXECUTION_CONTEXT.CONTEXT_ACTIVEWEBSERVICE_LEVEL

                Case "socket"
                    retval.ActionExecutionContext = STAT_EXECUTION_CONTEXT.CONTEXT_SOCKET_LEVEL

                Case "mappingpoint"
                    retval.ActionExecutionContext = STAT_EXECUTION_CONTEXT.CONTEXT_MAPPING_POINT_LEVEL

                Case "stats"
                    retval.ActionExecutionContext = STAT_EXECUTION_CONTEXT.CONTEXT_STATS

                Case Else
                    GlobalObject.Msg("Warning: unknown execution context: " & xaction.@context.ToString & " in " & xaction.ToString)
            End Select
        End If

        retval.ActionNode = xaction

        If retval.developInnerStructure() Then
            Return retval
        Else
            Return Nothing
        End If

    End Function

    Public Property Parameters As BosswaveAppParameterList
        Get
            Return Me.myparameters
        End Get
        Set(ByVal value As BosswaveAppParameterList)
            Me.myparameters = value
        End Set
    End Property
    Public Function developInnerStructure() As Boolean

        Try

            'parse parameters
            Me.Parameters = BosswaveAppParameterList.createFromXAction(Me.xactionNode)
            Me.triggers = BosswaveStatDropConditions.createFromXStatDropConditions(Me.xactionNode.Descendants("statdropconditions")(0))

            Return (Me.haveParameters() And Me.haveDropConditions())
        Catch ex As Exception
            GlobalObject.MsgColored("Error loading application actions: " & ex.Message, Color.Red)
            Return False
        End Try
    End Function

   
    Public Function ToXML() As String

        Dim Xml As String
        Dim trigger_list As String
        Dim parameter_list As String

        Xml = "<action "
        Xml &= "id="
        Xml &= Chr(34) & HttpUtility.UrlEncode(Me.Id) & Chr(34) & " "
        Xml &= "jclass="
        Xml &= Chr(34) & Me.JClassName & Chr(34) & ">"

        '<action> inner xml
        trigger_list = Me.triggers.ToXML().ToString
        parameter_list = Me.Parameters.ToXML().ToString

        Xml &= trigger_list.ToString
        Xml &= parameter_list.ToString
        Xml &= "</action>"

        Return XElement.Parse(Xml)

    End Function

    Public Property Tag As Object
        Get
            Return myTag
        End Get
        Set(ByVal value As Object)
            myTag = value
        End Set
    End Property

    Public ReadOnly Property TagType As Type
        Get
            Return myTag.GetType()
        End Get
    End Property

    Public Property ActionNode As XElement
        Get
            Return Me.xactionNode
        End Get
        Set(ByVal value As XElement)
            Me.xactionNode = value
        End Set
    End Property

    Public Function getLVItemRole(ByVal lvItem As Object) As BosswaveAppEngineField
        'Action
        'Parameter : Value
        'Condition : Operator : Value

        'Dim retval As BosswaveAppEngineField

        'retval = BosswaveAppEngineField.Invalid

        'If TypeOf lvItem Is ListViewItem Then

        'ElseIf TypeOf lvItem Is ListViewItem.ListViewSubItem Then

        '    Dim child As ListViewItem.ListViewSubItem

        '    child = CType(lvItem, ListViewItem.ListViewSubItem)

        '    If Me.Name = "parameter" And child.GetHashCode() = Me.Item2.GetHashCode() Then
        '        Return BosswaveAppEngineField.ParameterValue
        '    ElseIf Me.Name = "condition" And child.GetHashCode() = Me.Item2.GetHashCode() Then
        '    End If

        'End If

    End Function
    'Public Function getConfigNode() As XElement
    '    Return Me.Node
    'End Function
    'Public ReadOnly Property haveConfigNode() As Boolean
    '    Get
    '        Return Not IsNothing(Node)
    '    End Get
    'End Property
    'Public Shared Function createSection(ByVal section As BosswaveAppSection, ByVal xnode As XElement) As BosswaveConfigItem

    '    Dim retval As BosswaveConfigItem

    '    retval = New BosswaveConfigItem(xnode)

    '    retval.configKey = New ListViewItem()
    '    retval.configOperator = New ListViewItem.ListViewSubItem
    '    retval.configSelector = New ListViewItem.ListViewSubItem

    '    If section = BosswaveAppSection.Action Then

    '    End If

    '    'tag all with config class
    '    retval.configKey.Tag = retval
    '    retval.configOperator.Tag = retval
    '    retval.configSelector.Tag = retval


    '    Return retval
    'End Function

    Public Property Id As String
        Get
            Return Me.myid
        End Get
        Set(ByVal value As String)
            Me.myid = value
        End Set
    End Property

    'Public ReadOnly Property isDynamicProperty As Boolean
    '    Get

    '        If Not Me.haveConfigNode() Then Return False
    '        Dim config As BosswaveConfigItem

    '        config = Me

    '        'check value type of right hand attribute
    '        If config.ItemType = BosswaveAppEngineField.ParameterName Then
    '            config = config.configSelector.Tag
    '        ElseIf config.ItemType = BosswaveAppEngineField.StatDropConditionPropertyName Or BosswaveAppEngineField.StatDropConditionPropertyValue Then
    '            config = config.configSelector.Tag
    '        End If

    '        If Not config.hasRightHandAttrib() Then Return False

    '        Return (Mid(config.RightHand, 1) = "$")
    '    End Get
    'End Property


    'Public ReadOnly Property hasRightHandAttrib() As Boolean
    '    Get
    '        If Not Me.haveConfigNode() Then Return False

    '        Return Not IsNothing(Me.Node.@righthand_prototypeid)
    '    End Get
    'End Property

    'Public ReadOnly Property hasLeftHandAttrib() As Boolean
    '    Get
    '        If Not Me.haveConfigNode() Then Return False

    '        Return Not IsNothing(Me.Node.@lefthand_prototypeid)
    '    End Get
    'End Property
    'Public ReadOnly Property hasInstructionAttrib() As Boolean
    '    Get
    '        If Not Me.haveConfigNode() Then Return False
    '        Return Not IsNothing(Me.Node.@instruction)
    '    End Get
    'End Property
    'Public ReadOnly Property hasInversalConditionAttrib() As Boolean
    '    Get
    '        If Not Me.haveConfigNode() Then Return False
    '        Return Not IsNothing(Me.Node.@inversalcondition)
    '    End Get
    'End Property
    'Public ReadOnly Property LeftHand() As String
    '    Get
    '        If Not Me.haveConfigNode() Then Return False
    '        Return Me.Node.@lefthand_prototypeid
    '    End Get
    'End Property
    'Public ReadOnly Property RightHand() As String
    '    Get
    '        If Not Me.haveConfigNode() Then Return False
    '        Return Me.Node.@righthand_prototypeid
    '    End Get
    'End Property
    'Public ReadOnly Property Instruction() As String
    '    Get
    '        If Not Me.haveConfigNode() Then Return False
    '        Return Me.Node.@instruction
    '    End Get
    'End Property


End Class
Public Enum BosswaveAppSection
    Action = 1
    MethodName = 2
    MethodParameter = 3
End Enum
Public Enum BosswaveAppEngineField
    Invalid = 0
    Action = 6
    ParameterName = 7
    ParameterValue = 8
    StatDropConditionPropertyName = 9
    StatDropConditionCondition = 10
    StatDropConditionPropertyValue = 11
End Enum

Public Class BosswaveConditionComboItem
    Private Node As XElement
    Public Sub New(ByVal _node As XElement)
        Me.Node = _node
    End Sub
    Public Overrides Function ToString() As String
        Return Me.Node.Value
    End Function


End Class

Public Class BosswaveStatCompareResult

    Private myval As BosswaveStatCompareResultVal
    Private realtime_fullstat As Preprocessing_result
    Private realtime_value As String
    Private associated_condition As BosswaveStatDropCondition

    Public Sub New(ByVal _result As BosswaveStatCompareResultVal, Optional ByVal stat_data As Object = Nothing)
        Me.Result = _result

        'attached stat data depends on calling context.
        If TypeOf stat_data Is String Then
            'value only.
            Me.StatRealTimeValue = stat_data
        ElseIf TypeOf stat_data Is Preprocessing_result Then
            'varname + value.
            Me.StatRealTime = stat_data
        End If

    End Sub
    Public Property StatCondition As BosswaveStatDropCondition
        Get
            Return Me.associated_condition
        End Get
        Set(ByVal value As BosswaveStatDropCondition)
            Me.associated_condition = value
        End Set
    End Property

    Public ReadOnly Property haveRealTimeStatistic As Boolean
        Get
            Return Not IsNothing(Me.StatRealTime)
        End Get
    End Property
    Public Property StatRealTime As Preprocessing_result
        Get
            Return Me.realtime_fullstat
        End Get
        Set(ByVal value As Preprocessing_result)
            Me.realtime_fullstat = value
            If Not IsNothing(Me.realtime_fullstat) Then
                Me.StatRealTimeValue = Me.realtime_fullstat.stat_value
            End If
        End Set
    End Property
    Public Property StatRealTimeValue As String
        Get
            Return Me.realtime_value
        End Get
        Set(ByVal value As String)
            Me.realtime_value = value
        End Set
    End Property
    Private ReadOnly Property InverseResult As BosswaveStatCompareResultVal
        Get
            If Result = BosswaveStatCompareResultVal.MATCH Then
                Return BosswaveStatCompareResultVal.NO_MATCH
            ElseIf Result = BosswaveStatCompareResultVal.NO_MATCH Then
                Return BosswaveStatCompareResultVal.MATCH
            Else
                Return Me.Result
            End If
        End Get
    End Property
    Public ReadOnly Property getInversedResult As BosswaveStatCompareResult
        Get
            Dim retval As BosswaveStatCompareResult
            retval = New BosswaveStatCompareResult(Me.InverseResult)
            Return retval
        End Get
    End Property
    Public Property Result As BosswaveStatCompareResultVal
        Get
            Return Me.myval
        End Get
        Set(ByVal value As BosswaveStatCompareResultVal)
            Me.myval = value
        End Set
    End Property

    Public Shared Operator =(ByVal val1 As BosswaveStatCompareResult, ByVal val2 As BosswaveStatCompareResultVal) As Boolean
        Return val1.Result = val2
    End Operator
    Public Shared Operator <>(ByVal val1 As BosswaveStatCompareResult, ByVal val2 As BosswaveStatCompareResultVal) As Boolean
        Return val1.Result <> val2
    End Operator

    Public Shared Operator =(ByVal val1 As BosswaveStatCompareResult, ByVal val2 As BosswaveStatCompareResult) As Boolean
        Return val1.Result = val2.Result
    End Operator

    Public Shared Operator <>(ByVal val1 As BosswaveStatCompareResult, ByVal val2 As BosswaveStatCompareResult) As Boolean
        Return val1.Result <> val2.Result
    End Operator
End Class

Public Enum BosswaveStatCompareResultVal
    UNSUPPORTED_DATATYPE = -5
    UNKNOWN_OPERATOR = -4
    UNKNOWN_PROTOTYPE = -3
    INVALID_LEFTHAND_DATATYPE = -2
    INVALID_RIGHTHAND_DATATYPE = -1
    NO_MATCH = 0
    MATCH = 1
End Enum

Public Enum BosswaveDLLState
    NotLoaded = 0
    ReflectionOnlyLoaded = 1
    Completely_Loaded = 2
End Enum

Public Enum BOSSWAVE_APP_DISABLE_REASON
    NONE = 0
    MISSING_APP = 1
    MISSING_PROTOTYPE = 2
    PROTOTYPE_LOAD_ERR = 3
    INVALID_XML = 10
    INVALID_PROTOTYPE = 11
    FLAGGED_DISABLED = 12
    INVALID_MAPPING_POINT_CONFIG = 13
    DEPENDENCY_FILE_NOT_FOUND = 14
End Enum