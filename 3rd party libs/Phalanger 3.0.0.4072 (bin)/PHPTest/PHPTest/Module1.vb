Option Explicit On
Imports System.Web
Imports PHP.core
Imports PHP.Library


Module Module1

    Sub Main()
        Dim context As ScriptContext
        Dim script_file As String

        context = ScriptContext.CurrentContext

        context.Output = Console.Out
        context.OutputStream = Console.OpenStandardOutput()

        script_file = "C:\Users\gertjan\Documents\Visual Studio 2010\Projects\BlingBlingServor\BlingBlingServor\www\console\index.php"

        context.Include(script_file, True)

        Dim x As Integer
        x = x

    End Sub

End Module
