Option Explicit On
Option Compare Text
Imports System.IO
Imports System.Xml
Imports System.Web
Imports System.Text
Imports TriniDATDictionaries
Imports Newtonsoft.Json
Imports System.Reflection
Public Class BosswaveDataType

    Private myprototypeid As String
    Private myinherited_prototypeid As String
    Public myTag As Object
    Private declaration_dll As String
    Private declaration_dll_mod As Assembly
    Private declaration_dll_type As Type
    Private declaration_dll_load_state As BosswaveDLLState
    Private declaration_own_compare As Boolean
    Private declaration_dll_validate_ok As Boolean
    Private declaration_dll_mod_location As String

    Private prototype_chain As List(Of BosswaveDataType)
    Private ReadOnly Property DLLModuleOK As Boolean
        Get
            Return Me.declaration_dll_validate_ok
        End Get
    End Property
    Public Property PrototypeChain() As List(Of BosswaveDataType)
        Get
            Return Me.prototype_chain
        End Get
        Set(ByVal value As List(Of BosswaveDataType))
            Me.prototype_chain = value
        End Set
    End Property
    Public Function getPrototypeInstance(ByVal ParameterInstance As BosswaveAppParameter) As Object
        'From DLL
        Try
            If Not Me.DLLModuleOK Then
                Dim default_base_prototype As BosswaveDataType
                default_base_prototype = New BosswaveDataType()
                default_base_prototype.PrototypeId = "String"
                Return Nothing
            End If


            'since we are about to execute this assembly's code
            'ensure reflection only context load type  is off
            Call Me.ensureReflectionContextNone()

            'if the prototype inherits from other then String, 
            'pass an instance of its base type along with its initializator
            Dim last_base_type As BosswaveDataType
            Dim last_base_prototypeid As String
            Dim external_prototype_constructor_params() As Object
            Dim inheritance_list As New List(Of BosswaveDataType)

            'fill prototype chain
            inheritance_list = New List(Of BosswaveDataType)
            last_base_prototypeid = Me.PrototypeId

            Do Until BosswaveDataType.PrototypeIDIsCore(last_base_prototypeid) = True
                last_base_type = BosswaveDataType.getInheritedPrototypeInstance(last_base_prototypeid)
                'if base type is core type, nothing will be returned.
                If Not IsNothing(last_base_type) Then
                    inheritance_list.Add(last_base_type)
                    last_base_prototypeid = last_base_type.PrototypeId
                End If

            Loop

            Me.PrototypeChain = inheritance_list

            'call constructor
            external_prototype_constructor_params = New Object() {ParameterInstance.Initializor}
            Return Activator.CreateInstance(Me.declaration_dll_type, external_prototype_constructor_params)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Shared Function getInheritedPrototypeInstance(ByVal prototypeId As String) As BosswaveDataType
        'FROM definition file
        Dim xprototypes_doc As XDocument
        Dim default_base_prototype As BosswaveDataType

        default_base_prototype = New BosswaveDataType()
        default_base_prototype.PrototypeId = "String"


        xprototypes_doc = BosswaveApplication.getBuilderPrototypeListXML()

        If IsNothing(xprototypes_doc) Then
            Return default_base_prototype
        End If


        Dim q = From xprototype In xprototypes_doc.Descendants("prototypes")(0).Descendants("prototype") Where xprototype.@id.ToString = prototypeId


        For Each xprototype_element In q
            If Not IsNothing(xprototype_element.@inherits) Then
                default_base_prototype.PrototypeId = xprototype_element.@inherits
                Return default_base_prototype
            End If
        Next


        If Not BosswaveDataType.PrototypeIDIsCore(prototypeId) Then
            'this prototype does not declare a inherited type. Return string as base type.
            Return default_base_prototype
        Else
            'already a core type.
            Return Nothing
        End If

    End Function
    Private Function ensureReflectionContextNone() As Boolean


        If Not Me.declaration_dll_load_state = BosswaveDLLState.Completely_Loaded Then
            If Me.declaration_dll_load_state = BosswaveDLLState.ReflectionOnlyLoaded Then
                Dim filepath As String

                filepath = "(none)"
                Try
                    filepath = Me.declaration_dll_mod_location
                    Me.declaration_dll_mod = Assembly.LoadFrom(filepath)
                    Me.declaration_dll_type = Me.GetTypeFromCurrentDLL(Me.PrototypeId)
                    Return Not IsNothing(Me.declaration_dll_mod) And Not IsNothing(Me.declaration_dll_type)
                Catch ex As Exception
                    GlobalObject.Msg("Unable to locate " & filepath)
                End Try

            Else
                GlobalObject.Msg("Unknown assembly state.")
            End If
        End If

        Return False

    End Function
    Private Function GetTypeFromCurrentDLL(ByVal typename As String) As Type

        Dim declaration_dll_all_types() As Type


        If IsNothing(declaration_dll_mod) Then
            Return Nothing
        End If

        declaration_dll_all_types = declaration_dll_mod.GetTypes()

        For Each dec_type In declaration_dll_all_types
            If dec_type.IsClass = True And dec_type.Name = Me.PrototypeId Then
                Return dec_type
            End If
        Next

        Return Nothing
    End Function
    Public Property DLLModule As String
        Get
            Return Me.declaration_dll
        End Get
        Set(ByVal value As String)

            Try

                Dim valid_member As PropertyInfo

                'INIT
                Me.declaration_dll_load_state = BosswaveDLLState.NotLoaded
                Me.declaration_dll_validate_ok = False
                Me.declaration_dll_mod = TriniDAT.TriniDAT_Core.getReflectionOnlyAssemblyByFilename(value)

                If IsNothing(Me.declaration_dll_mod) Then
                    'load first time
                    Me.declaration_dll_mod = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(value))
                End If


                If Not IsNothing(Me.declaration_dll_mod) Then
                    'valid yes/no
                    Me.declaration_dll_mod_location = value
                    Me.declaration_dll_load_state = BosswaveDLLState.ReflectionOnlyLoaded
                    Me.declaration_dll_type = Nothing

                    Me.declaration_dll_type = Me.GetTypeFromCurrentDLL(Me.PrototypeId)
                    If Not IsNothing(declaration_dll_type) Then
                        Dim declaration_dll_all_props() As PropertyInfo

                        declaration_dll_all_props = Me.declaration_dll_type.GetProperties()

                        valid_member = Me.declaration_dll_type.GetProperty("isValidValue")

                        If IsNothing(valid_member) Then
                            Throw New Exception(Me.PrototypeId & ": forgot to implement string property isValidValue. ")
                        Else
                            'validate user code 
                            If Not valid_member.GetGetMethod(False).ReturnType = GetType(Boolean) Then
                                Throw New Exception(Me.PrototypeId & ": return value must be declared 'BosswaveStatCompareResult'. ")
                            End If

                            Me.declaration_dll_validate_ok = True
                            Me.declaration_dll = value
                        End If
                    Else
                        Throw New Exception("Unable to locate datatype '" & Me.PrototypeId & "' in assembly " & value & "!")
                    End If
                Else
                    Throw New Exception("Unable to load " & value & "!")
                End If
            Catch ex As Exception
                value = Nothing
                Me.declaration_dll_type = Nothing
                Me.declaration_dll_mod = Nothing

                'make this prototype a String 
                Me.PrototypeId = "String"
            End Try


        End Set
    End Property

    Public Function getExternalPrototypeOperator(ByVal operatorName As String, ByVal inversedOperator As Boolean, ByVal match_against As String) As MethodInfo

        'implementation e.g.
        'Operator_HAS
        'Operator_HAS_NOT
        'Operator_EQ
        'Operator_EQ_NOT

        Try

            If Not Me.haveValidDLLModule Then Return Nothing
            Me.ensureReflectionContextNone()

            Dim operator_func_name As String
            Dim operatorMethod As MethodInfo

            operator_func_name = "OPERATOR_" & operatorName.ToUpper()

            If inversedOperator Then
                operator_func_name &= "_NOT"
            End If

            operatorMethod = Me.declaration_dll_type.GetMethod(operator_func_name)

            If IsNothing(operatorMethod) Then
                Throw New Exception(Me.DLLModule & " error. Missing implementation of operator '" & operator_func_name & "'")
            End If

            If operatorMethod.ReturnType <> GetType(Boolean) Then
                GlobalObject.Msg("Prototype operator " & Me.declaration_dll_type.ToString & "." & operator_func_name & " error: Boolean return value expected, got " & operatorMethod.ReturnType.ToString & " instead.")
                Return Nothing
            Else
                Return operatorMethod
            End If

        Catch ex As Exception
            GlobalObject.Msg("Prototype.getExternalPrototypeOperator err: " & ex.Message)
        End Try


        Return Nothing
    End Function

    Public ReadOnly Property DeclaresExternalModule() As Boolean
        Get
            Return Me.haveValidDLLModule
        End Get
    End Property
    Public ReadOnly Property haveValidDLLModule As Boolean
        Get
            Return (Not IsNothing(Me.declaration_dll) And Not IsNothing(declaration_dll_type) And Me.DLLModuleOK)
        End Get
    End Property
    Public Property InheritedPrototypeId As String
        Get
            Return Me.myinherited_prototypeid
        End Get
        Set(ByVal value As String)
            Me.myinherited_prototypeid = value
        End Set
    End Property

    Public Property PrototypeId As String
        Get
            Return Me.myprototypeid
        End Get
        Set(ByVal value As String)
            Me.myprototypeid = value

            'load appriopate dll info
            Dim prototype_declaration As XElement
            prototype_declaration = Me.Declaration

            If Not IsNothing(prototype_declaration.@inherits) Then
                Me.InheritedPrototypeId = prototype_declaration.@inherits
            End If

            If Not IsNothing(prototype_declaration.@module) Then
                'if problems here, make this a String
                Dim dll_path As String
                Dim dll_err As Boolean

                dll_path = prototype_declaration.@module
                dll_err = True

                If dll_path <> "" Then
                    If File.Exists(dll_path) Then
                        'this will validate the DLL
                        Me.DLLModule = dll_path
                        dll_err = Not Me.declaration_dll_validate_ok
                    End If
                End If

                If dll_err Then

                    GlobalObject.Msg("Error: Module Loader: Error while loading " & dll_path & " or file not found. Please remove declaration '" & prototype_declaration.ToString & "' from " & GlobalSetting.getBuilderXMLPath())
                    GlobalObject.Msg("Warning: parameter type '" & value & "' degraded to String type.")
                    Me.myprototypeid = "String"
                End If
            End If

        End Set
    End Property
    Public ReadOnly Property Declaration As XElement
        Get
            Dim prototype
            prototype = From var_dec In BosswaveApplication.getBuilderPrototypeListXML().Descendants("prototypes").Descendants("prototype")
                        Where var_dec.@id.ToString = Me.PrototypeId
            Select var_dec

            For Each dec As XElement In prototype
                Return dec

            Next

        End Get
    End Property

    Private ReadOnly Property getExternalIsValidValue() As PropertyInfo
        Get
            If Not Me.haveValidDLLModule Then Return Nothing
            Dim propinfo As PropertyInfo

            'since we are about to execute this assembly's code
            'ensure reflection only context load type  is off
            Call Me.ensureReflectionContextNone()

            propinfo = Me.declaration_dll_type.GetProperty("ISVALIDVALUE")
            If Not IsNothing(propinfo) Then
                Return propinfo
            End If

            Return Nothing
        End Get
    End Property
    Private ReadOnly Property getExternalGENERATEHTMLFIELD() As PropertyInfo
        Get
            If Not Me.haveValidDLLModule Then Return Nothing
            Dim propinfo As PropertyInfo

            'since we are about to execute this assembly's code
            'ensure reflection only context load type  is off
            Call Me.ensureReflectionContextNone()

            propinfo = Me.declaration_dll_type.GetProperty("GENERATEHTMLFIELD")
            If Not IsNothing(propinfo) Then
                Return propinfo
            End If

            Return Nothing
        End Get
    End Property
    Private Shared Function PrototypeIDIsCore(ByVal prototype_id As String) As Boolean
        Return prototype_id = "Boolean" Or prototype_id = "String" Or prototype_id = "Integer"
    End Function
    Protected ReadOnly Property isCoreDataType() As Boolean
        Get
            Return BosswaveDataType.PrototypeIDIsCore(Me.PrototypeId)
        End Get
    End Property
    Protected ReadOnly Property generateHTMLField(ByVal param_instance As BosswaveAppParameter, ByVal field_id As String, ByVal field_value As String) As String
        Get
            Dim retval As String
            Dim pos As Integer

            retval = "invalid"

            If Me.haveValidDLLModule Then
                '
                Dim ext_instance As Object
                Dim ext_asHTMLFIELD_get As PropertyInfo

                Try

                    ext_instance = Me.getPrototypeInstance(param_instance)
                    If IsNothing(ext_instance) Then
                        Throw New Exception("Unable to create instance of '" & Me.declaration_dll_type.ToString & "'")
                    End If

                    ext_asHTMLFIELD_get = Me.getExternalGENERATEHTMLFIELD()

                    If IsNothing(ext_asHTMLFIELD_get) Then
                        'something went wrong with loading the ext type
                        'and the type has become String
                        GoTo INTERNAL_GET
                    End If

                    retval = ext_asHTMLFIELD_get.GetValue(ext_instance, {field_id, field_value})
                    Return retval

                Catch ex As Exception
                    GlobalObject.Msg("Error invoking external prototype '" & Me.PrototypeId & "' : " & ex.Message)
                End Try

            End If

INTERNAL_GET:
            Dim output As String

            output = "<input type=""text"" value=""" & field_value & """ id=""" & field_id & """ name=""" & field_id & """/>"

            Return output

DONE:
            Return retval

        End Get

    End Property
    Protected ReadOnly Property isValidValue(ByVal param_instance As BosswaveAppParameter, ByVal realtime_stat_value As String) As BosswaveStatCompareResult
        Get
            Dim retval As BosswaveStatCompareResult
            Dim pos As Integer

            retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.INVALID_RIGHTHAND_DATATYPE, realtime_stat_value)

            If Me.haveValidDLLModule Then
                '
                Dim ext_instance As Object
                Dim ext_isValidValue_get As PropertyInfo

                Try

                    ext_instance = Me.getPrototypeInstance(param_instance)
                    If IsNothing(ext_instance) Then
                        Throw New Exception("Unable to create instance of '" & Me.declaration_dll_type.ToString & "'")
                    End If

                    'DISABLED
                    '====
                    ''call on whole chain
                    'Dim x As Integer
                    'Dim topmost_prototype_index_count As Integer
                    'Dim prototype_index_count As Integer
                    'prototype_index_count = Me.PrototypeChain.Count - 1

                    ''note: this is done with the widening concept.

                    'For x = 0 To prototype_index_count

                    '    topmost_prototype_index_count = prototype_index_count - x
                    '    'first check is always a core type (String, Boolean, Integer).
                    '    If Not Me.PrototypeChain.Item(topmost_prototype_index_count).isValidValue(param_instance, realtime_stat_value).Result = BosswaveStatCompareResultVal.MATCH Then
                    '        'when base type does not recognize datatype, abort.
                    '        Return retval 'retval = invalid datatype
                    '    End If


                    'Next

                    'base type validation ok
                    '====

                    ext_isValidValue_get = Me.getExternalIsValidValue()

                    If IsNothing(ext_isValidValue_get) Then
                        'something went wrong with loading the ext type
                        'and the type has become String
                        GoTo INTERNAL_VALUE_COMPARE
                    End If

                    If ext_isValidValue_get.GetValue(ext_instance, {realtime_stat_value}) = True Then
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH, realtime_stat_value)
                    Else
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH, realtime_stat_value)
                    End If
                    Return retval

                Catch ex As Exception
                    GlobalObject.Msg("Error invoking external prototype '" & Me.PrototypeId & "' : " & ex.Message)
                End Try

            End If

INTERNAL_VALUE_COMPARE:
            'note: should be sorted with speed in mind / freq. of occurence.
            Select Case PrototypeId

                Case "JSON.Property"
                    pos = InStr(realtime_stat_value, ".")
                    If pos > 1 And pos < Len(realtime_stat_value) Then
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH, realtime_stat_value)
                    End If

                Case "String"
                    retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH, realtime_stat_value)

                Case "Integer"
                    If IsNumeric(realtime_stat_value) Then
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH, realtime_stat_value)
                    End If

                Case "Boolean"
                    If BosswaveBoolean.IsValidBool(realtime_stat_value) Then
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.MATCH, realtime_stat_value)
                    Else
                        retval = New BosswaveStatCompareResult(BosswaveStatCompareResultVal.NO_MATCH, realtime_stat_value)
                    End If

                Case Else
                    Return New BosswaveStatCompareResult(BosswaveStatCompareResultVal.UNKNOWN_PROTOTYPE, realtime_stat_value)

            End Select
DONE:
            Return retval

        End Get
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

    Public Sub New()
        declaration_dll = Nothing
        declaration_dll_type = Nothing
    End Sub

End Class