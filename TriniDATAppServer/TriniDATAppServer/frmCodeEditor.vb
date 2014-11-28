Imports System.IO

Public Class frmCodeEditor
    Private pattern_found As Integer
    Private classNodes As Collection
    Private event_onexecutestart_identifier As String = "<!-- ##Event: OnExecute -->"
    Private event_onexecutestart_replacement As String = "document.write(""<script src='$JS_FILE'\></script\>"");"

    Private Sub frmCodeEditor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        lvWorkspace.ExpandAll()
        tvReference.ExpandAll()

    End Sub

    Private Sub popupOnAddStepWorker_Opening(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles popupOnAddStepWorker.Opening

    End Sub
    Private Sub markClassesRecursive(ByVal n As TreeNode, ByVal lastId As Integer)
        System.Diagnostics.Debug.WriteLine(n.Text)
        'MessageBox.Show(n.Text)

        If InStr(n.Text, "class") > 0 Then
            'assign id to every class
            lastId = lastId + 1
            n.Tag = lastId
        End If

        Dim aNode As TreeNode
        For Each aNode In n.Nodes
            markClassesRecursive(aNode, lastId)
        Next
    End Sub
    Private Sub findPatternRecursive(ByVal n As TreeNode, ByVal pattern As String)
        System.Diagnostics.Debug.WriteLine(n.Text)
        'MessageBox.Show(n.Text)

        If InStr(n.Text, pattern) > 0 Then
            'assign id to every class
            pattern_found = pattern_found + 1
        End If

        Dim aNode As TreeNode
        For Each aNode In n.Nodes
            findPatternRecursive(aNode, pattern)
        Next
    End Sub
    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        Dim n As TreeNode = Nothing
        Dim lastId As Integer = 0



        'TAG ALL CLASSES.
        For Each n In lvWorkspace.Nodes
            markClassesRecursive(n, lastId)
        Next

        GoTo WRITE_ALL

        'find by pattern
        pattern_found = 0
        For Each n In lvWorkspace.Nodes
            findPatternRecursive(n, "class")
        Next
        pattern_found = pattern_found - 1
        MsgBox("Nodes found: " & pattern_found.ToString)

        'find node by id 
        Dim findId As Integer = 7
        n = Nothing

        For findId = 21 To 22


            For Each n In lvWorkspace.Nodes
                n = CheckNodeRecursive(n, findId)
                If Not IsNothing(n) Then
                    Exit For
                End If
            Next
            If Not IsNothing(n) Then
                MsgBox(findId.ToString & " = " & n.Text)
            Else
                MsgBox(findId.ToString & " not found!")
            End If

        Next

WRITE_ALL:
        'Todo. 
        'Compilation process:
        '1. delete project folder
        '2. generate class names from whole listview (tag them)
        '3. copy all class templates by classtype to project folder.
        '4. merge with user code.
        '5. rewrite for linking includes.

        'collect all class nodes.

        Dim output_path As String = "C:\temp\TriniDATscript\"
        Dim classFilename As String
        Dim parentFile As String
        Dim classid As Integer
        Dim parentClassId As Integer
        Dim parentNode As TreeNode = Nothing
        Dim hasParentClass As Boolean
        Dim classHasChildren As Boolean = False
        Dim classChildNode As TreeNode = Nothing
        Dim parentLines() As String
        Dim classBuffer As String
        Dim line As String
        Dim classFileTitle As String
        Dim className As String
        Dim childclassName As String = ""

        'init
        n = Nothing
        classNodes = New Collection

        'find by pattern
        pattern_found = 0
        For Each n In lvWorkspace.Nodes
            collectClassNodesRecursive(n)
        Next

        If classNodes.Count < 1 Then
            MsgBox("No classes defined.")
        End If

        For Each n In classNodes
            'rewrite class
            classid = n.Tag
            className = getClassNameFromNode(n)
            Debug.Print("Compiling " & className & "...")

            'Parent / Children look-up.
            parentNode = classNodesFindParentNode(classid)
            hasParentClass = Not IsNothing(parentNode)
            classHasChildren = classNodeHasChildren(classid)
            classChildNode = classNodegetChild(classid)
            If classHasChildren Then
                childclassName = getClassNameFromNode(classChildNode)
            End If

            'Define filename
            classFileTitle = className
       
            If hasParentClass Then
                classFileTitle = classFileTitle & ".js"
            Else
                'Top-level Campaign
                classFileTitle = classFileTitle & ".html"
            End If

            classFilename = output_path & classFileTitle



            If hasParentClass Then
                'current class must be written inside parent function / assume onExecute
                parentClassId = parentNode.Tag
                parentFile = output_path & getClassNameFromNode(parentNode)
                If classNodesHasParentNode(parentClassId) Then
                    parentFile = parentFile & ".js"
                Else
                    parentFile = parentFile & ".html"
                End If

                Debug.Print("Rewriting parent " & parentFile & "...")

                'buffer parent file and replace
                parentLines = System.IO.File.ReadAllLines(parentFile)

                For lineIndex = 0 To parentLines.Length - 1
                    line = parentLines(lineIndex)

                    If InStr(line, event_onexecutestart_identifier) Then
                        ''replace placeholder with my the include file of this class
                        parentLines(lineIndex) = Replace(line, event_onexecutestart_identifier, Replace(event_onexecutestart_replacement, "$JS_FILE", classFileTitle))
                    End If

                Next


                'overwrite parent file with buffer
                System.IO.File.WriteAllLines(parentFile, parentLines)

                ReDim parentLines(0)

            End If

            'write class file
            Debug.Print("Writing " & classFileTitle & "...")

            classBuffer = "<!-- Class Type: " & className & " -->" & vbCrLf

            'Special conditions for top-level class. (Campaign Class)
            If Not hasParentClass Then
                classBuffer = classBuffer & vbCrLf & "<html>"
                classBuffer = classBuffer & vbCrLf & getTabs(1) & "<head>"
                classBuffer = classBuffer & vbCrLf & getTabs(2) & "<script src=""core/bot.js""></script>"
                classBuffer = classBuffer & vbCrLf & getTabs(1) & "</head>"
                classBuffer = classBuffer & vbCrLf & "<body>"
                classBuffer = classBuffer & vbCrLf & getTabs(1) & "<script language=""Javascript"">"
            End If

            classBuffer = classBuffer & vbCrLf & getTabs(1) & "function " & className & "(){"
            classBuffer = classBuffer & vbCrLf
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "<!-- Non-returnable Code -->"
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "this.botSendAway = function (tourl){};"
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "this.botSendCodeStub = function (code){};"
            classBuffer = classBuffer & vbCrLf
            classBuffer = classBuffer & vbCrLf
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "<!-- Error Code -->"
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "this.onLimbo = function (reason){};"
            classBuffer = classBuffer & vbCrLf
            classBuffer = classBuffer & vbCrLf

            classBuffer = classBuffer & vbCrLf & getTabs(2) & "<!-- Start of Execution Code-->"
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "this.Executor = function (){"

            If classHasChildren Then
                'insert placeholder for child class.
                classBuffer = classBuffer & vbCrLf & getTabs(3) & "<!-- Insert Placeholder for " & childclassName & " -->"
                classBuffer = classBuffer & vbCrLf & getTabs(3) & event_onexecutestart_identifier
            Else
                classBuffer = classBuffer & vbCrLf & getTabs(3) & "<!-- No Children -->"
            End If
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "alert('hi from " & className & "');"
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "};" 'End of Executor.
            classBuffer = classBuffer & vbCrLf & getTabs(2) & "<!-- End of Code-->"
            classBuffer = classBuffer & vbCrLf & getTabs(1) & "}"
            classBuffer = classBuffer & vbCrLf & "<!-- Go -->"
            classBuffer = classBuffer & vbCrLf & "var " & className & "Instance = new " & className & "();"
            classBuffer = classBuffer & vbCrLf & className & "Instance.Executor();" & vbCrLf

            'Special conditions for top-level class. (Campaign Class)
            If Not hasParentClass Then
                classBuffer = classBuffer & vbCrLf & getTabs(1) & "</script>"
                classBuffer = classBuffer & vbCrLf & "</body>"
                classBuffer = classBuffer & vbCrLf & "</html>"
            End If


            System.IO.File.WriteAllText(classFilename, classBuffer)

            classBuffer = ""

        Next

        Debug.Print("Done!")
    End Sub

    Public Function getTabs(ByVal amount As Integer) As String
        Dim retval As String = ""

        For x = 1 To amount
            retval = retval & Chr(9)
        Next

        Return retval
    End Function
    Public Function classNodesHasParentNode(ByVal classid As Integer) As Boolean
        Return Not IsNothing(classNodesFindParentNode(classid))
    End Function
    Public Function classNodesFindParentNode(ByVal classid As Integer) As TreeNode

        Dim n As TreeNode
        Dim parent_classid As Integer = classid - 1

        If parent_classid < 1 Then Return Nothing

        For Each n In classNodes
            If n.Tag = parent_classid Then
                Return n
            End If
        Next

        Return Nothing

    End Function
    Public Function classNodeHasChildren(ByVal classid As Integer) As Boolean
        Dim n As TreeNode
  
        For Each n In classNodes
            If n.Tag = classid + 1 Then
                Return True
            End If
        Next

        Return False
    End Function
    Public Function classNodegetChild(ByVal classid As Integer) As TreeNode
        Dim n As TreeNode

        For Each n In classNodes
            If n.Tag = classid + 1 Then
                Return n
            End If
        Next

        Return Nothing
    End Function

    Public Sub collectClassNodesRecursive(ByVal n As TreeNode)
        'wrapper

        If InStr(n.Text, "class") > 0 Then
            'assign id to every class
            classNodes.Add(n)
        End If

        Dim aNode As TreeNode
        For Each aNode In n.Nodes
            collectClassNodesRecursive(aNode)
        Next
    End Sub

    Private Function CheckNodeRecursive(ByVal n As TreeNode, ByVal findId As Integer) As TreeNode
        System.Diagnostics.Debug.WriteLine(n.Text)
        'MessageBox.Show(n.Text)

        If n.Tag = findId Then

            Return n

        End If

        Dim aNode As TreeNode
        Dim retval As TreeNode = Nothing


        For Each aNode In n.Nodes
            retval = CheckNodeRecursive(aNode, findId)
            If Not IsNothing(retval) Then
                Return retval
            End If
        Next

        If Not IsNothing(retval) Then
            Return retval
        End If

        Return Nothing

    End Function

    Public Function getClassNameFromNode(ByVal n As TreeNode) As String
        Dim retval As String = ""

        retval = Replace(n.Text.Trim(), "class", "")
        retval = Replace(retval, "(", "")
        retval = Replace(retval, ")", "")
        retval = Replace(retval, " ", "")
        retval = retval & "Id" & n.Tag.ToString
        Return retval
    End Function
End Class