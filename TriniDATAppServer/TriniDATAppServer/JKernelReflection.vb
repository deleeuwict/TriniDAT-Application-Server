Imports System.Reflection
Imports TriniDATServerTypes

Public Class JKernelReflection
    Inherits JTriniDATWebService

    Public Const KERNEL_MAPPINGPOINT_INDEX As Integer = 0
    Public Const KERNEL_REFLECTION_PROCESSINDEX As Integer = 0

    Public Sub New()
        MyBase.New()

    End Sub
    Public Overrides Function OnRegisterWebserviceFunctions(ByVal http_function_table As TriniDATServerFunctionTable) As Boolean
        Return True
    End Function

    Public Overrides Function DoConfigure() As Boolean

        'configure mailbox
        'configure mailbox
        Dim mb_events As TriniDATObjectBox_EventTable
        mb_events = New TriniDATObjectBox_EventTable
        mb_events.event_inbox = AddressOf myinbox

        getMailProvider().Configure(mb_events, False)

        'null handler for http requests
        Dim null_events As TriniDATHTTP_EventTable
        null_events = New TriniDATHTTP_EventTable
        Me.getIOHandler().Configure(null_events)

        'store messages automatically / locally
        getMailProvider().configureInboxFeature(True)

        Return True

    End Function

    Public Function myinbox(ByRef req As JSONObject, ByVal from_url As String) As Boolean

        If req.ObjectTypeName = "Setting_ModulePath" Then
            Dim reply As JSONObject
            reply = New JSONObject
            reply.ObjectType = "Setting_ModulePath"
            reply.Directive = GlobalSetting.getStaticDataRoot()
            reply.Sender = Me
            reply.Tag = req.Tag
            Me.getMailProvider().Send(reply, Nothing, req.Sender.getClassName())
            Return False
        ElseIf req.ObjectTypeName = "Setting_ServerHostname" Then
            Dim reply As JSONObject
            reply = New JSONObject
            reply.ObjectType = "Setting_ServerHostname"
            reply.Directive = GlobalObject.CurrentServerConfiguration.server_ip.ToString
            reply.Sender = Me
            reply.Tag = req.Tag
            Me.getMailProvider().Send(reply, Nothing, req.Sender.getClassName())
            Return False
        ElseIf req.ObjectTypeName = "Setting_ServerPort" Then
            Dim reply As JSONObject
            reply = New JSONObject
            reply.ObjectType = "Setting_ServerPort"
            reply.Directive = GlobalObject.CurrentServerConfiguration.server_port.ToString
            reply.Sender = Me
            reply.Tag = req.Tag
            Me.getMailProvider().Send(reply, Nothing, req.Sender.getClassName())
            Return False
        ElseIf req.ObjectTypeName = "Setting_ServerURL" Then
            Dim reply As JSONObject
            reply = New JSONObject
            reply.ObjectType = "Setting_ServerURL"
            reply.Directive = "http://" & GlobalObject.CurrentServerConfiguration.server_ip.ToString
            If GlobalObject.CurrentServerConfiguration.server_port <> 80 Then
                reply.Directive &= ":" & GlobalObject.CurrentServerConfiguration.server_port.ToString
            End If
            reply.Directive &= "/"
            reply.Sender = Me
            reply.Tag = req.Tag
            Me.getMailProvider().Send(reply, Nothing, req.Sender.getClassName())
            Return False
        End If



        If Len(req.ObjectTypeName) > 18 Then

            Dim reflectObjectType As String

            reflectObjectType = Mid(req.ObjectTypeName, 19)


            If Left(req.ObjectTypeName, 18) = "JReflectServiceFor" And reflectObjectType = "DOT_NET_COREPROPERTY" Then
                'this module enumerates static properties on general .NET objects by reflection.
                Dim fullPath As String
                Dim parts() As String
                Dim asm_dotnet_system As Assembly
                Dim reflecttype As Type
                Dim propertyName As String
                Dim propertyfield As PropertyInfo
                Dim reflect_error As Boolean

                'Target class / owner of property should be static.
                'e.g. System.Environment.UserName or System.Environment.MachineName
                fullPath = req.Directive
                parts = fullPath.Split(".")
                'remove last identifier
                propertyName = parts(parts.Length - 1)
                fullPath = Replace(fullPath, "." & propertyName, "")
                reflect_error = True

                asm_dotnet_system = System.Reflection.Assembly.Load("mscorlib.dll")
                If Not IsNothing(asm_dotnet_system) Then
                    reflecttype = asm_dotnet_system.GetType(fullPath)
                    If Not IsNothing(reflecttype) Then
                        If reflecttype.IsClass() Or reflecttype.IsAnsiClass = True Then

                            propertyfield = reflecttype.GetProperty(propertyName)

                            If Not IsNothing(propertyfield) Then

                                'reply with property value
                                Dim resp As New JSONObject
                                resp.ObjectType = "JReflectResponseFor" & reflectObjectType.ToUpper
                                resp.Attachment = propertyfield.GetValue(Nothing, Nothing)
                                resp.Tag = req.Tag

                                reflect_error = False
                                Me.getMailProvider().Send(resp, Nothing, req.Sender.getClassName())


                                Return False
                            End If
                        End If
                    End If
                End If

                If reflect_error Then
                    'reply with error object
                    Dim resp As New JSONObject
                    resp.ObjectType = "JReflectError"
                    resp.Attachment = req.Attachment
                    resp.Tag = req.Tag

                    reflect_error = False
                    Me.getMailProvider().Send(resp, Nothing, req.Sender.getClassName())
                    Return False
                End If
            End If


            If Left(req.ObjectTypeName, 18) = "JReflectServiceFor" And reflectObjectType = "ARRAY" Then
                'find and return the desired object in a JSONObject.BinaryData
                'all reflectable objects must 
                'expose global shared instance by getGlobalInstance()
                'and must implement getByIndex and getCount(), when a collection.

                Dim className As String
                Dim classInfo As System.Type
                Dim class_global_method As MethodInfo
                Dim resp As New JSONObject

                className = req.Directive
                classInfo = JServiceLauncher.getClassInfo(className)

                If Not IsNothing(classInfo) Then

                    class_global_method = classInfo.GetMethod("getGlobalInstance")

                    If Not IsNothing(class_global_method) Then
                        'CALL A STATIC METHOD THAT RETURNS AN ARRAY
                        'SEND RESPONSE
                        resp.ObjectType = "JReflectResponseFor" & reflectObjectType.ToUpper
                        resp.Attachment = class_global_method
                        resp.Tag = req.Tag

                        Me.getMailProvider().Send(resp, Nothing, req.Sender.getClassName())

                        Return False
                        'Recipient can now do:  classInstance = resp.Attachment.Invoke(Nothing, Nothing)
                    End If
                End If


                'CREATE CLASS INSTANCE
                '==================
                Dim mp_relative_path As String
                Dim mp_absolute_path As String

                mp_relative_path = Me.getProcessDescriptor().getParent().getApplicationURL()
                mp_absolute_path = "http://" & GlobalObject.CurrentServerConfiguration.server_ip.ToString & ":" & GlobalObject.CurrentServerConfiguration.server_port.ToString & mp_relative_path

                resp.ObjectType = "JReflectResponseFor" & reflectObjectType.ToUpper

                If IsNothing(classInfo) Then
                    'LOAD CLASS FROM EXTERNAL ASSEMBLY

                    classInfo = JServiceLauncher.getExternalType(className, Me.getProcessDescriptor().getParent().getDependencyPaths())
                End If

                'Constructor(absolute_path,relative_path)
                resp.Attachment = Activator.CreateInstance(classInfo, New Object() {mp_absolute_path, mp_relative_path})
                resp.Tag = req.Tag

                Me.getMailProvider().Send(resp, Nothing, req.Sender.getClassName())

                Return False

            End If
        End If

        Return False
    End Function

End Class
