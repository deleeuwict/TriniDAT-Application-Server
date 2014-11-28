Imports System.Web
Imports System.IO

Public Class frmPostDetails

    'APPLICATION CONSTANTS
    Public Const app_NOTEPAD As String = "C:\Program Files\Notepad++\notepad++.exe"

    'TEMPLATE CONSTANTS
    Public Const templatefieldsFormTargetUri As String = "$TARGET"
    Public Const templatefieldsFormStartIndicator As String = "<form"
    Public Const templatePostForm As String = "C:\Users\gertjan\Documents\seo programming project\Generator\templates\postform.html"
    Public Const templateContentControlIndicator As String = "<!-- #"
    Public Const templateContentControlFormFieldsStart As String = "<!-- #INPUTFIELDS_START -->"

    'GENERATED FILES
    Public Const generatedOutputDir As String = "C:\Users\gertjan\Desktop\"

    Public Const urlencodedIndicator As String = "%"
    Public lastTextControlLocation As Point
    Public Const newTextControlSizeWidth As Integer = 300
    Public lastLabelControlLocation As Point
    Public newcontrolsCount As Integer
    Public Const controlHeightAdjust As Integer = 17 'generic vertical spacer.
    Public Const controlWidthAdjust As Integer = 1 'generic horizontal spacer.
    Public Const groupboxWidthAdjust As Integer = 35 'horzitonal space betweeh groupbox and postdata values.
    Public Const formWidthdAdjust As Integer = 15 'horzitonal space between postcontrols and edge of form.
    Public Const formHeightaAdjust As Integer = 100 'vertical space between postcontrols and edge of form.
    Public Const buttonsHeightAdjust As Integer = 25 'vertical difference between last post field and button.
    Public Const decodeButtonsWidth As Integer = 14
    Public Const headerDataTextboxPrefix As String = "Header"
    Public Const headerDataLabelPrefix As String = "Header"
    Public Const postDataTextboxPrefix As String = "data"
    Public Const postDataLabelPrefix As String = "data"
    Public Const decodeButtonNamePrefix As String = "cmdURLDecode"
    Public Const postfieldURLTemplateTBName As String = "txtURL"
    Public Const queryvarsLabelSuffix As String = ":"
    Public allTextboxNames As Collection
    Public initialPositionXLabel As Integer
    Public initialPositionXTB As Integer
    Public datafieldcount As Integer

    Private Sub cmdParse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdParse.Click

        Dim line As String
        Dim bInHeader As Boolean = True
        Dim bFirstLine As Boolean = True
        Dim ctrl As Object

        'INIT
        txtPostData.Text = Replace(txtPostData.Text, Chr(9), "")

ELIMINATE:
        'KEEP ONLY STATICLY DESIGNED CONTROLS.
        For Each ctrl In postcontrols.Controls
            If ctrl.name <> postfieldURLTemplateTBName And ctrl.name <> "lblURL" And InStr(ctrl.name, "lblHdr") = 0 Then
                Debug.Print("deleting control " & ctrl.name)
                postcontrols.Controls.Remove(ctrl)

                GoTo ELIMINATE
            End If
        Next

        'RESTORE DEFAULTS
        lblURL.Location = New Point(initialPositionXLabel, lblURL.Location.Y)
        txtURL.Location = New Point(initialPositionXTB, txtURL.Location.Y)


        'TEMPLATE
        lastTextControlLocation = txtURL.Location
        lastLabelControlLocation = lblURL.Location


        newcontrolsCount = 0
        allTextboxNames = New Collection
        allTextboxNames.Add(postfieldURLTemplateTBName)

        For Each line In txtPostData.Lines

            line = Trim(line)


            'means we are in DATA
            If line = "" Then
                bInHeader = False
            End If


            If bFirstLine Then
                Dim fieldIsURLEncoded As Boolean
                Dim URLdecodeButton As Button

                txtURL.Text = Mid(line, 6, InStrRev(line, " ") - 6)
                txtURL.BackColor = txtNotUrlEncoded.BackColor
                fieldIsURLEncoded = isURLEncoded(txtURL.Text)

                'create button of url encoded.
                If fieldIsURLEncoded Then
                    URLdecodeButton = addDecodeButton("URL")
                    URLdecodeButton.Location = New Point(postcontrols.Width - controlWidthAdjust, txtURL.Location.Y)

                    'decode field immediately
                    txtURL.Tag = txtURL.Text
                    Call OnURLDecodeFieldClick(URLdecodeButton, Nothing)
                End If

                bFirstLine = False
            ElseIf Not bFirstLine And line <> "" Then

                Dim controlName As String

                If bInHeader Then
                    controlName = headerDataLabelPrefix

                    newcontrolsCount = newcontrolsCount + 1
                    controlName = controlName & newcontrolsCount
                    createControls(controlName, controlName, line, True)
                Else
                    'parse all variables

                    Dim allvars() As String = line.Split("&")
                    Dim pair() As String
                    Dim x As Integer
                    Dim value As String = ""

                    For x = 0 To allvars.Length - 1

                        pair = allvars(x).Split("=")
                        pair(0) = Trim(pair(0))

                        If pair.Length = 2 Then
                            value = Trim(pair(1))
                        Else
                            'ensure empty
                            value = ""
                        End If

                        If pair(0) <> "" Then
                            newcontrolsCount = newcontrolsCount + 1
                            createControls(postDataTextboxPrefix & newcontrolsCount, pair(0), value, True)
                            ''MsgBox(allvars(x))
                        End If
                    Next

                End If

            End If

        Next

        'rsize form controls.
        Call resizeAll()
       
    End Sub
    Public Sub resizeAll()
        Dim totalHeight As Integer
        Dim biggestWidth As Integer = 0
        Dim newTBY As Integer = 0
        Dim formResized As Boolean = False
        Dim lastcreatedLabelName As String 'needed in order to re-adjust controls under post field data 
        Dim lastcreatedLabel As Object = Nothing
        Dim allvars As Collection

        allvars = getAllQueryVarNames()

        lastcreatedLabelName = "lbl" & postDataLabelPrefix & newcontrolsCount

        For Each ctrl In postcontrols.Controls



            If ctrl.GetType.ToString = "System.Windows.Forms.Label" Then
                'note: multiple unrelated operations occur in this single code block 

                If ctrl.name = lastcreatedLabelName Then
                    lastcreatedLabel = ctrl
                End If

                If ctrl.width > biggestWidth Then
                    biggestWidth = ctrl.width
                End If
            ElseIf ctrl.GetType.ToString = "System.Windows.Forms.TextBox" Then
                totalHeight = totalHeight + ctrl.size.height
            End If
        Next

        If allvars.Count > 0 Then

            'readadjust textbox widths

            For Each ctrl In postcontrols.Controls
                If ctrl.GetType.ToString = "System.Windows.Forms.TextBox" Then
                    ctrl.location = New Point(ctrl.location.x + (biggestWidth / 2), ctrl.location.y)
                    If postcontrols.Width - (ctrl.location.x + ctrl.size.width) < groupboxWidthAdjust Then
                        postcontrols.Width = ctrl.location.x + ctrl.size.width + groupboxWidthAdjust
                    End If
                End If

            Next
        End If

        'readjust form size in proportion to groupbox
        If Me.Width - (postcontrols.Width + postcontrols.Location.X) < controlWidthAdjust Then
            Me.Width = postcontrols.Width + postcontrols.Location.X + controlWidthAdjust + formWidthdAdjust
            formResized = True
        End If

        'readjust groupbox
        If postcontrols.Height - totalHeight < formHeightaAdjust Then
            postcontrols.Size = New Size(postcontrols.Width, totalHeight + formHeightaAdjust)
        End If

        For Each ctrl In postcontrols.Controls
            If ctrl.GetType.ToString = "System.Windows.Forms.Button" Then
                ctrl.location = New Point(postcontrols.Width - ((groupboxWidthAdjust / 100) * 85), ctrl.Location.Y)
            End If
        Next

        'resize main form
        Me.Height = postcontrols.Height + (controlHeightAdjust * 2) + postcontrols.Location.Y

        'readjust user input paste fields
        Me.line.Height = postcontrols.Height
        txtPostData.Height = CInt(postcontrols.Height / 100 * 75)
        cmdParse.Location = New Point(cmdParse.Location.X, txtPostData.Height + controlHeightAdjust)

        're-adjust control buttons relative to last created label
        If Not IsNothing(lastcreatedLabel) Then
            'Note: buttons are NOT relative to groupbox (parent container=form)
            Me.cmdPluginDisqus.Location = New Point(postcontrols.Location.X + (controlWidthAdjust * 5), postcontrols.Location.Y + lastcreatedLabel.location.y + buttonsHeightAdjust)
            Me.cmdDummy.Location = New Point(Me.cmdPluginDisqus.Width + cmdPluginDisqus.Location.X + controlWidthAdjust, cmdPluginDisqus.Location.Y)
        End If

        lblHdrValue.Location = New Point(txtURL.Location.X, lblHdrValue.Location.Y)

        If formResized Then
            Me.CenterToScreen()
        End If
    End Sub
    Public Sub createControls(ByVal controlname As String, ByVal labelvalue As String, ByVal textvalue As String, ByVal addTextBoxEventHandler As Boolean)

        Dim newTextControl As TextBox
        Dim newLabelControl As Label
        Dim is_urlencoded As Boolean

        is_urlencoded = isURLEncoded(textvalue)

        'add header / variable name
        newLabelControl = New Label
        newLabelControl.Name = "lbl" & controlname
        newLabelControl.Size = lblURL.Size
        newLabelControl.Location = New Point(lastLabelControlLocation.X, lastLabelControlLocation.Y + controlHeightAdjust)
        newLabelControl.Text = labelvalue & queryvarsLabelSuffix

        newLabelControl.AutoSize = True

        If InStr(controlname, postDataLabelPrefix) = 0 Then
            newLabelControl.ForeColor = lblURL.ForeColor
            newLabelControl.Font = lblURL.Font
        Else
            newLabelControl.ForeColor = txtDataLabelTemplate.ForeColor
            newLabelControl.Font = txtDataLabelTemplate.Font
        End If

        postcontrols.Controls.Add(newLabelControl)
        lastLabelControlLocation = newLabelControl.Location

        'add post data
        newTextControl = New TextBox

        If addTextBoxEventHandler Then
            AddHandler newTextControl.Enter, AddressOf OnPostFieldClick
        End If

        newTextControl.Name = "txt" & controlname
        newTextControl.Size = New Size(newTextControlSizeWidth, txtURL.Size.Height)
        newTextControl.Location = New Point(lastTextControlLocation.X, lastTextControlLocation.Y + controlHeightAdjust)
        newTextControl.Text = textvalue
        newTextControl.BackColor = txtNotUrlEncoded.BackColor
        newTextControl.HideSelection = txtURL.HideSelection
        newTextControl.Font = txtURL.Font
        newTextControl.ForeColor = txtURL.ForeColor

        newTextControl.SendToBack()
        postcontrols.Controls.Add(newTextControl)
        lastTextControlLocation = newTextControl.Location

        allTextboxNames.Add(newTextControl.Name)

        If isURLEncoded(textvalue) Then

            Dim decodeButton As Button = addDecodeButton(controlname)

            decodeButton.Location = New Point(postcontrols.Width - controlWidthAdjust, newTextControl.Location.Y)

            newTextControl.Tag = textvalue
            'decode field immediately
            OnURLDecodeFieldClick(decodeButton, Nothing)
        End If

    End Sub
    Public Function addDecodeButton(ByVal id As String) As Button

        Dim newDecodeButton As New Button
        'Add decode button - location x,y is set manually by caller

        AddHandler newDecodeButton.Click, AddressOf OnURLDecodeFieldClick
        newDecodeButton.Name = decodeButtonNamePrefix & id
        newDecodeButton.Size = New Size(decodeButtonsWidth, 15)
        newDecodeButton.Font = txtURL.Font
        newDecodeButton.Text = "'"
        newDecodeButton.Tag = 0 'for color toggle.
        postcontrols.Controls.Add(newDecodeButton)

        addDecodeButton = newDecodeButton
    End Function
    Public Sub OnURLDecodeFieldClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim associatedTB As Object = Nothing
        Dim textboxName As String
        Dim ctrl As Object

        textboxName = Replace(sender.name, "cmdURLDecode", "txt")

        For Each ctrl In postcontrols.Controls
            If ctrl.name = textboxName Then
                associatedTB = ctrl
                Exit For
            End If
        Next

        If IsNothing(associatedTB) Then Exit Sub

        ''TOGGLE COLOR
        If CInt(sender.tag) = 1 Then
            associatedTB.text = associatedTB.Tag
            associatedTB.BackColor = txtUrlEncoded.BackColor
            sender.tag = 0
        Else
            associatedTB.text = Uri.UnescapeDataString(associatedTB.text)
            associatedTB.BackColor = txtTemporaryURLDecoded.BackColor
            sender.tag = 1
        End If
    End Sub


    Public Sub OnPostFieldClick(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim startPos As Integer
        Dim labelName As String
        Dim findtext As String
        Dim associatedLabel As Object = Nothing
        Dim ctrl As Object


        labelName = Replace(sender.name, "txt", "lbl")

        'if text field is part of header data then ignore query string
        If InStr(labelName, headerDataTextboxPrefix) = 0 And labelName <> "lblURL" Then
            'parse as post data


            For Each ctrl In postcontrols.Controls
                If ctrl.name = labelName Then
                    associatedLabel = ctrl
                    Exit For
                End If
            Next

            If IsNothing(associatedLabel) Then Exit Sub

            findtext = Replace(associatedLabel.text, queryvarsLabelSuffix, "")
            findtext = findtext & "=" & IIf(Not IsNothing(sender.tag), sender.tag, sender.text)
        Else
            'parse as header
            findtext = IIf(Not IsNothing(sender.tag), sender.tag, sender.text)
        End If


        startPos = InStr(txtPostData.Text, findtext)
        If startPos = 0 Then
            'try without = (probably a empty var).
            startPos = InStr(txtPostData.Text, Mid(findtext, 1, findtext.Length - 1))
        End If

        Me.Text = startPos.ToString
        If startPos > 0 Then
            txtPostData.SelectionStart = startPos - 1
            txtPostData.SelectionLength = findtext.Length
        End If

    End Sub
    Private Sub frmPostDetails_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'SAVE DEFAULT STATE
        initialPositionXLabel = lblURL.Location.X
        initialPositionXTB = txtURL.Location.X

        txtPostData.SelectionStart = 0
        txtPostData.SelectionLength = 0

        AddHandler txtURL.Enter, AddressOf OnPostFieldClick
    End Sub

    Public Sub postDatatoHTMLForm(ByVal output_filename As String)
        'Warning: uses $templatePostForm to write a HTML form 

        Dim outputline As String        
        Dim formHost As String = ""

        output_filename = generatedOutputDir & output_filename

        'create / open output file 
        If System.IO.File.Exists(output_filename) = True Then System.IO.File.Delete(output_filename)

        'post data discovery to build-up POST form.

        'POST target is relative url.
        If Mid(Me.txtURL.Text, 1, 1) = "/" Then
            'append from host
            formHost = getHeaderValueByName("Host")
            If formHost <> "" Then
                If Mid(formHost, 1, 7) <> "http://" Then
                    formHost = "http://" & formHost
                End If
            End If
        End If

        Dim formPostTargetUri As String = formHost & Me.txtURL.Text

        ' MsgBox("Form target: " & formTarget)
        Dim templateFF As Integer = FreeFile()
        Dim outputStream As StreamWriter = System.IO.File.CreateText(output_filename)
        Dim rewriteOppertunity As Boolean
        Dim lineIsContentControlArg As Boolean

        ' Open template file.
        FileOpen(templateFF, templatePostForm, OpenMode.Input)
        ' Loop until end of file. 
        While Not EOF(templateFF)

            'read line from template.
            outputline = LineInput(templateFF)
            Debug.Print(outputline)

            lineIsContentControlArg = (InStr(outputline, templateContentControlIndicator) > 0)
            rewriteOppertunity = (Not lineIsContentControlArg) ' ' 'indicates if line should be written

            If lineIsContentControlArg Then
                'discovery of control kind.

                If InStr(outputline, templateContentControlFormFieldsStart) > 0 Then
                    'WRITE ALL QUERY VARS
                    Dim input_fields As Collection = getAllQueryVarNames()
                    Dim id As String = ""

                    For Each id In input_fields
                        'create templated entry.
                        outputline = "<input id='$QUERYVAR' name='$QUERYVAR' type='text' value='$CONTENTS'>"
                        outputline = Replace(outputline, "'", Chr(34))
                        outputline = Chr(9) & Chr(9) & Chr(9) & outputline
                        'set dynamic vars
                        outputline = Replace(outputline, "$QUERYVAR", id)
                        outputline = Replace(outputline, "$CONTENTS", getQueryVarContent(id))
                        outputStream.WriteLine(outputline)
                    Next
                End If
            Else
                'attempt dynamic rewrite of template starts here.
                If rewriteOppertunity And InStr(outputline, templatefieldsFormStartIndicator) > 0 Then
                    'rewrite form
                    outputline = Replace(outputline, templatefieldsFormTargetUri, formPostTargetUri)
                    'copy to Stream
                    rewriteOppertunity = True
                End If
            End If



            'all rewrites done.
            If rewriteOppertunity Then outputStream.WriteLine(outputline)
        End While

        'close output.
        outputStream.Close()

        'close template file.
        FileClose(templateFF)

        'launch text-editor.
        Process.Start(app_NOTEPAD, output_filename)
    End Sub

    Public Function getHeaderValueByName(ByVal strName As String) As String

        Dim retval As String = ""

        'e.g. Content-type:
        strName = strName.ToLower & ":"

        getHeaderValueByName = getPostDataFieldValueByPattern(headerDataTextboxPrefix, strName)
    End Function
    Public Function getPostDataFieldValueByPattern(ByVal postTBFieldNamePrefix As String, ByVal strPattern As String) As String
        Dim ctrl As Object
        Dim tbname As String
        Dim retval As String = ""

        'e.g. Content-type:
        strPattern = strPattern.ToLower

        For Each tbname In allTextboxNames
            If InStr(tbname, postTBFieldNamePrefix) Then
                'Look for <STRNAME>:
                ctrl = getPostDataControlByName(tbname, "TextBox")
                If Mid(ctrl.text.ToString.ToLower, 1, strPattern.Length) = strPattern Then
                    'APPEND IF RETURN VALUE EXISTS
                    If retval <> "" Then retval = retval & vbCrLf
                    retval = retval & Mid(ctrl.text.ToString.ToLower, strPattern.Length + 1).Trim
                End If
            End If
        Next

        getPostDataFieldValueByPattern = retval
    End Function
    Public Function getAllQueryVarNames() As Collection
        Dim ctrl As Object
        Dim var_name As String = ""
        Dim retval As New Collection

        For Each ctrl In postcontrols.Controls
            If ctrl.GetType.ToString = "System.Windows.Forms.Label" And InStr(ctrl.name, postDataTextboxPrefix) > 0 Then
                var_name = Replace(ctrl.Text, queryvarsLabelSuffix, "") 'delete label suffix from varname.
                retval.Add(var_name)
            End If
        Next

        getAllQueryVarNames = retval
    End Function

    Public Function getQueryVarContent(ByVal strVarName As String) As String
        Dim ctrl As Object
        Dim tbname As String = ""
        Dim retval As String = ""

        'Scan all labels for var name.
        strVarName = strVarName.ToLower

        For Each ctrl In postcontrols.Controls
            If ctrl.GetType.ToString = "System.Windows.Forms.Label" And ctrl.text.tolower = strVarName & queryvarsLabelSuffix Then
                tbname = Replace(ctrl.name, "lbl", "txt")
            End If
        Next

        If tbname = "" Then Return Nothing

        ctrl = getPostDataControlByName(tbname, "TextBox")

        If IsNothing(ctrl) Then Return Nothing

        getQueryVarContent = ctrl.text
    End Function
    Public Function getPostDataControlByName(ByVal name As String, ByVal type As String) As Control
        Dim ctrl As Object

        For Each ctrl In postcontrols.Controls
            If ctrl.name = name And ctrl.GetType.ToString = "System.Windows.Forms." & type Then
                getPostDataControlByName = ctrl
                Exit Function
            End If
        Next

        getPostDataControlByName = Nothing
    End Function

    Private Sub cmdPluginDisqus_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPluginDisqus.Click
        Dim output_filename As String
        output_filename = "formpost.html"
        Call postDatatoHTMLForm(output_filename)

    End Sub

    Public Function isURLEncoded(ByVal val As String) As Boolean
        isURLEncoded = (InStr(val, urlencodedIndicator) > 0)
    End Function

    Private Sub txtURL_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtURL.TextChanged

    End Sub

    Private Sub lblURL_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblURL.Click

    End Sub
End Class
