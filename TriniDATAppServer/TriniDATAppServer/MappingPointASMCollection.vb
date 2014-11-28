Option Explicit On
Option Compare Text
Imports TriniDATDictionaries
Imports System.Reflection
Imports System.IO

Public Class GlobalMappingPointASMCollection

    Private Shared loaded_asm_list As List(Of Assembly)
    Private Shared loaded_asm_hashlist As TriniDATDictionaries.TriniDATWordDictionary

    Public Shared Function Initialize() As Boolean
        Try
            GlobalMappingPointASMCollection.loaded_asm_list = New List(Of Assembly)
            GlobalMappingPointASMCollection.loaded_asm_hashlist = New TriniDATWordDictionary("assembly md5 hashes", Nothing)

        Catch ex As Exception
            Return False
        End Try

        Return True
    End Function

    Public Shared ReadOnly Property haveAsmList() As Boolean
        Get
            Return Not IsNothing(GlobalMappingPointASMCollection.loaded_asm_list)
        End Get
    End Property

    Public Shared ReadOnly Property AsmList() As List(Of Assembly)
        Get
            Return GlobalMappingPointASMCollection.loaded_asm_list
        End Get
    End Property
    Public Shared ReadOnly Property HashList() As TriniDATWordDictionary
        Get
            Return GlobalMappingPointASMCollection.loaded_asm_hashlist
        End Get
    End Property
    Public Shared Function haveHash(ByVal val As String) As Boolean
        Return GlobalMappingPointASMCollection.HashList.Has(val)
    End Function
    Public Shared Function haveAssemblyByFile(ByVal val As String) As Object

        If Not GlobalMappingPointASMCollection.haveAsmList Then Return False
        If Not File.Exists(val) Then Return False

        Dim inspect_md5 As String

        inspect_md5 = GlobalObject.getMD5CheckSum(New FileInfo(val), True)
        If GlobalMappingPointASMCollection.haveHash(inspect_md5) Then Return True

        'check filename matching - weak.
        For Each asm In GlobalMappingPointASMCollection.loaded_asm_list

            If asm.Location = val Then
                Return True
            End If
        Next


        Return inspect_md5
    End Function
    Public Shared Function haveAssemblyByFullName(ByVal val As String) As Boolean

        If Not GlobalMappingPointASMCollection.haveAsmList Then Return False

        For Each asm In GlobalMappingPointASMCollection.loaded_asm_list

            If asm.FullName = val Then
                Return True
            End If
        Next

        Return False
    End Function
    Public Shared Function AddByFilepath(ByVal val As String) As Boolean

        Debug.Print("Attempt to load: " & val)

        Try
            If InStr(val, "Culture=") > 0 Then
                'GAC string
                If Not GlobalMappingPointASMCollection.haveAssemblyByFullName(val) Then
                    GlobalMappingPointASMCollection.AsmList.Add(Assembly.Load(val))
                End If

            Else
                'filename was passed.
                Dim retval As Object
                'true or md5 hash
                retval = GlobalMappingPointASMCollection.haveAssemblyByFile(val)

                If TypeOf retval Is Boolean Then
                    Return True
                Else
                    'store hash.
                    GlobalMappingPointASMCollection.HashList.Add(CType(retval, String))

                    'add new assembly
                    GlobalMappingPointASMCollection.AsmList.Add(Assembly.LoadFile(val))
                End If
            End If

            Return True

        Catch ex As Exception
            Return False
        End Try

    End Function
    Public Shared Function getAsmType(ByVal class_name As String) As Type

        For Each asm In GlobalMappingPointASMCollection.AsmList
            Debug.Print("checking " & asm.Location & "...")
            For Each dep_asm_type As Type In asm.GetTypes()

                If dep_asm_type.Name.ToString = class_name Then
                    Return dep_asm_type
                End If

            Next
        Next

        Return Nothing

    End Function
    Public Shared Function getFileNameByType(ByVal val As Type) As String


        For Each asm In GlobalMappingPointASMCollection.AsmList

            For Each dep_asm_type As Type In asm.GetTypes()

                If dep_asm_type = val Then
                    Return asm.Location
                End If

            Next
        Next

        Return Nothing
    End Function

End Class
