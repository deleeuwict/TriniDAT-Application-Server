Option Compare Text
Option Explicit On
Imports System.Net.Sockets
Imports System.Text
Imports System.Net
Imports System.Reflection
Imports TriniDATServerTypes
Imports TriniDATHTTPTypes

'Launches a JService described in mappingPoint.

Public Class JServiceLauncher

    Private mp As mappingPointRoot


    Public Sub New(ByVal _mp As mappingPointRoot)
        'Must be instance due to threading.

        Me.mp = _mp
    End Sub

    Public Shared Function getClassInfo(ByVal classNameToReflect As String) As System.Type
        Dim this_assembly_name As String
        Dim retval As Type

        retval = Nothing
        this_assembly_name = Assembly.GetExecutingAssembly().GetName().Name

        Try
            retval = Type.GetType(this_assembly_name & "." & classNameToReflect)
        Catch ex As Exception
            msg(classNameToReflect & " is not present.")
        End Try


        Return retval

    End Function


    Public Shared Sub Msg(ByVal txt As String)
        Debug.Print("JServiceLauncher static:" & txt)
    End Sub

    Public Shared Function createJService(ByVal JClassName As String) As JTriniDATWebService

        Dim newinstance As Object
        Dim jclass As System.Type

        jclass = JServiceLauncher.getClassInfo(JClassName)

        Try


            If IsNothing(jclass) Then
                Debug.Print("Unable to find class '" & JClassName & "'!")
                ' Err.Raise(100, Nothing, "Unable to find Jclass locally. Will lookup mp dependencies.")
            Else
                'construct using JService defaults.
                newinstance = JServiceLauncher.createJService(jclass)
                Return newinstance
            End If

        Catch ex As Exception
            Debug.Print("CreateJInstance: unable to create instance of " & JClassName & "!")

        End Try

        Return Nothing

    End Function

    Public Shared Function getCoreType(ByVal class_name As String, ByVal Params() As Object) As Object

        Dim retval As Object
        Dim the_type As Type

        retval = Nothing

        Try
            the_type = JServiceLauncher.getClassInfo(class_name)
            retval = Activator.CreateInstance(the_type, Params)

        Catch ex As Exception
            Msg("getCoreType error: " & ex.Message)
        End Try

        Return retval
    End Function
    Public Shared Function createJService(ByVal class_type As Type) As Object

        Dim retval As Object

        retval = Nothing

        Try
            'try create local type.
            retval = Activator.CreateInstance(class_type, {})

        Catch ex As Exception

            Msg("getCoreType error: " & ex.Message)
        End Try

        'pass function to allow core type creation by proxy
        retval.setServerTypeCreatorGate(TriniDATServer.getServerTypeCreatorGate)

        Return retval
    End Function

    Public Shared Function getExternalType(ByVal class_name As String, ByVal external_asm_paths As List(Of String))

        Dim asm_to_load As String
        Dim retval As Type

        retval = Nothing

        For Each asm_to_load In external_asm_paths

            Dim dep_asm As Assembly

            dep_asm = Nothing

            Try
                If InStr(asm_to_load, "Culture") = 0 Then
                    dep_asm = Assembly.LoadFile(asm_to_load)
                Else
                    dep_asm = Assembly.Load(asm_to_load)
                End If


                For Each dep_asm_type As Type In dep_asm.GetTypes()

                    If dep_asm_type.Name.ToString = class_name Then

                        retval = dep_asm_type
                        Return retval
                    End If

                Next

            Catch ex As Exception
                If GlobalObject.haveServerThread Then

                    If GlobalObject.server.ServerMode = TRINIDAT_SERVERMODE.MODE_DEV Then
                        'note: may yield a load exception error when dependencies of this assembly need a rebuild.
                        If TypeOf ex Is System.Reflection.ReflectionTypeLoadException Then
                            For Each ex_ldr In CType(ex, System.Reflection.ReflectionTypeLoadException).LoaderExceptions
                                GlobalObject.MsgColored(".NET reflection type error. Assembly:  " & asm_to_load & ". Message: " & ex_ldr.Message, Color.Red)
                            Next

                        Else
                            GlobalObject.MsgColored(".NET reflection error. Assembly:  " & asm_to_load & ". Message: " & ex.Message & " at " & ex.StackTrace, Color.Red)
                        End If
                    End If
                End If
           End Try

        Next

        Return Nothing

    End Function
    

    Public Shared Function getClassFriendlyName(ByVal full_class_name As String) As String

        Dim dot As Integer

        dot = InStr(full_class_name, ".")
        If dot > 0 Then
            Return Mid(full_class_name, dot + 1)
        Else
            Return full_class_name
        End If
    End Function
End Class
