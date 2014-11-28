Option Explicit On
Option Compare Text

Imports System.Reflection
Imports System.Text
Imports System.Web

Public Class TriniDATHTMLTools

    Public Shared Function getLionBrowserType(ByVal referencing_asm As Assembly) As Type

        Dim client_reference_name() As AssemblyName
        Dim browser_asm As Assembly
        Dim lionbrowser_type As Type

        lionbrowser_type = Nothing
        client_reference_name = referencing_asm.GetReferencedAssemblies() 'System.Reflection.Assembly.GetCallingAssembly()

        Try

            For Each ref_name In client_reference_name

                ref_name = ref_name

                If ref_name.Name = "BlingBrowser" Then

                    browser_asm = Assembly.Load(ref_name)

                    For Each t In browser_asm.GetExportedTypes()

                        If t.Name = "LionBrowser" Then
                            lionbrowser_type = t
                            GoTo done
                            Exit For
                        End If

                    Next
                End If

            Next

            If IsNothing(lionbrowser_type) Then
                Err.Raise(0, 0, "Unable to load BlingBrowser assembly.")
            End If

        Catch ex As Exception
            MsgBox("getLionBrowserType Error: " & ex.Message)
            Return Nothing
        End Try

DONE:
        Return lionbrowser_type

    End Function

    Public Shared Function createLionBrowserInstance(ByVal owning_asm As Assembly) As Object
        'owning_asm: reference to a asm that has run-time references to BlingBrowser.
        '                   usually System.Reflection.Assembly.GetCallingAssembly()


        'create Lion instance dynamically because it is not accessible from this assmebly due to circular references
        'create browser object

        Return Activator.CreateInstance(TriniDATHTMLTools.getLionBrowserType(owning_asm), New Object() {Nothing})

    End Function

End Class
