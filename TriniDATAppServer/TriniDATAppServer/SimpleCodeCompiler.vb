Imports System.Reflection
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports Microsoft.CSharp

Public Class SimpleCodeCompiler

    Public Shared Function VBSourceCodeCompiler(ByVal provider As CodeDomProvider, ByVal ReferencedAssemblies() As String, ByVal vbsource As String, ByVal exeFile As String) As Boolean
        'example:  SourceCompileCode(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, "<srccode>", output_exe_file)


        Dim cp As New CompilerParameters()

        ' Generate an executable instead of  
        ' a class library.
        cp.GenerateExecutable = True

        ' Set the assembly file name to generate.
        cp.OutputAssembly = exeFile

        ' Generate debug information.
        cp.IncludeDebugInformation = True
        cp.CompilerOptions = ""

        ' Add an assembly reference.
        For Each asm_path In ReferencedAssemblies
            cp.ReferencedAssemblies.Add(asm_path)
        Next

        vbsource = Replace(vbsource, "Imports System" & vbNewLine, "Imports System" & vbNewLine & "Imports System.Runtime.CompilerServices" & vbNewLine)
        vbsource = Replace(vbsource, vbNewLine & vbNewLine, vbNewLine & "<Assembly: SuppressIldasmAttribute()> " & vbNewLine, 1, 1)


        ' Save the assembly as a physical file.
        cp.GenerateInMemory = False

        ' Set the level at which the compiler  
        ' should start displaying warnings.
        cp.WarningLevel = 3

        ' Set whether to treat all warnings as errors.
        cp.TreatWarningsAsErrors = False

        ' Set compiler argument to optimize output.
        cp.CompilerOptions = "/optimize"

        ' Set a temporary files collection. 
        ' The TempFileCollection stores the temporary files 
        ' generated during a build in the current directory, 
        ' and does not delete them after compilation.
        cp.TempFiles = New TempFileCollection(GlobalSetting.getTempDir(), False)

        ' Invoke compilation. 
        Dim cr As CompilerResults = _
            provider.CompileAssemblyFromSource(cp, vbsource)

        'delete temp file.
        Try
            cp.TempFiles.Delete()
        Catch ex As Exception

        End Try

        If cr.Errors.Count > 0 Then
            ' Display compilation errors.
            ' Console.WriteLine("Errors building {0} into {1}",   sourceFile, cr.PathToAssembly)
            'Dim ce As CompilerError
            'For Each ce In cr.Errors
            '    Console.WriteLine("  {0}", ce.ToString())
            '    Console.WriteLine()
            'Next ce
        Else
            '  Console.WriteLine("Source {0} built into {1} successfully.", sourceFile, cr.PathToAssembly)
            'Console.WriteLine("{0} temporary files created during the compilation.", cp.TempFiles.Count.ToString())
        End If
        Chr(10)

        ' Return the results of compilation. 
        If cr.Errors.Count > 0 Then

            For Each ce In cr.Errors
                MsgBox(ce.ToString())
            Next

            Return False
        Else
            Return True
        End If
    End Function 'Compile
    Public Function Test() As Boolean

        Dim codeProvider As CodeDomProvider = New CSharpCodeProvider()
        Dim CompilerParameters As CompilerParameters = New CompilerParameters

        CompilerParameters.GenerateExecutable = False
        CompilerParameters.GenerateInMemory = True
        CompilerParameters.IncludeDebugInformation = False
        CompilerParameters.WarningLevel = 3
        CompilerParameters.TreatWarningsAsErrors = False
        CompilerParameters.CompilerOptions = "/optimize"
        '   CompilerParameters.ReferencedAssemblies.Add("")

        'future: eventually a .net programmer's security score
        'will depend on his total usage of TriniDAT* core classes.
        '
        'the planned 'vender-lock' is 100% object that exist in the JWebservice base class. Exactly like GetIOHandler() and GetMailProvider().
        'e.g. Me.GetFile() instead of File.* Api's <- kind of app should be labeled a native .NET app.


        Dim assemblyCode As String = "using System;  public class MyClass { }"
        Dim compileResults As CompilerResults = codeProvider.CompileAssemblyFromSource(CompilerParameters, {assemblyCode})

        Dim myAssembly As Assembly = compileResults.CompiledAssembly
        Dim exported_type() As Type

        myAssembly = myAssembly
        exported_type = myAssembly.GetExportedTypes()
        exported_type = exported_type

        Return True

    End Function
End Class
