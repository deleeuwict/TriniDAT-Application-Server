Option Compare Text
Imports System.CodeDom
Imports System.CodeDom.Compiler
Imports System.IO
Imports System.Collections.Specialized

Public Class Form1

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim src_file As String
        Dim exe_file As String

        exe_file = "C:\temp\test.exe"

        src_file = getTempDir() & "src.txt"

        File.WriteAllText(src_file, Me.TextBox1.Text)

        MsgBox(Me.CompileCode(New VBCodeProvider, {"System.Web.DLL", "System.Net.DLL"}, src_file, exe_file).ToString)

    End Sub
    Public Function getTempDir() As String
        Dim default_output_folder As String

        default_output_folder = Environment.GetEnvironmentVariable("Temp")

        If Mid(default_output_folder, Len(default_output_folder) - 1, 1) <> "\" Then
            default_output_folder &= "\"
        End If

        If Not Directory.Exists(default_output_folder) Then
            Try
                Directory.CreateDirectory(default_output_folder)
            Catch ex As Exception

            End Try
        End If

        Return default_output_folder
    End Function
    Public Function CompileCode(ByVal provider As CodeDomProvider, ByVal ReferencedAssemblies() As String, ByVal sourceFile As String, ByVal exeFile As String) As Boolean

        Dim cp As New CompilerParameters()

        ' Generate an executable instead of  
        ' a class library.
        cp.GenerateExecutable = True

        ' Set the assembly file name to generate.
        cp.OutputAssembly = exeFile

        ' Generate debug information.
        cp.IncludeDebugInformation = True

        ' Add an assembly reference.
        For Each asm_path In ReferencedAssemblies
            cp.ReferencedAssemblies.Add(asm_path)
        Next

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
        cp.TempFiles = New TempFileCollection(".", True)

        If provider.Supports(GeneratorSupport.EntryPointMethod) Then
            ' Specify the class that contains 
            ' the main method of the executable.
            '   cp.MainClass = "MyClass"
        End If




        ' Invoke compilation. 
        Dim cr As CompilerResults = _
            provider.CompileAssemblyFromFile(cp, sourceFile)

        If cr.Errors.Count > 0 Then
            ' Display compilation errors.
            Console.WriteLine("Errors building {0} into {1}", _
                sourceFile, cr.PathToAssembly)
            Dim ce As CompilerError
            For Each ce In cr.Errors
                Console.WriteLine("  {0}", ce.ToString())
                Console.WriteLine()
            Next ce
        Else
            Console.WriteLine("Source {0} built into {1} successfully.", _
                sourceFile, cr.PathToAssembly)
            Console.WriteLine("{0} temporary files created during the compilation.", _
                    cp.TempFiles.Count.ToString())
        End If

        ' Return the results of compilation. 
        If cr.Errors.Count > 0 Then

            For Each ce In cr.Errors
                MsgBox(ce.ToString())
            Next

            Return False
        Else
            Return True
        End If
    End Function 'CompileCode
End Class
