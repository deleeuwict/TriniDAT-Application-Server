Imports System.IO
Imports System.Text
Imports Newtonsoft.Json

Public Class JRandomSocialIdFile

    'Note: only public Fields are included in JSON serialization
    Private listfilename As String
    Public listitems() As JRandomSocialIdEntry

    Private listitemscount As Integer
    Private Const delim As String = "|"
    Private msg_filename As String
    Public IDENTITIES_FOLDER As String

    Public Function getDelimiter() As String
        Return JRandomSocialIdFile.delim
    End Function

    Public Sub setFilename(ByVal val As String)
        Me.listfilename = val
    End Sub
    Public Function getFilename() As String
        Return Me.listfilename
    End Function
    Public Shared Function createNewFromNextId(ByVal entry As JRandomSocialIdEntry) As JRandomSocialIdFile

        Dim retval As JRandomSocialIdFile
        Dim nextfilename As String

        nextfilename = JRandomSocialIdFile.getNextId().ToString & ".dat"

        retval = New JRandomSocialIdFile(nextfilename, entry) 'note: will automatically create and write file.
        Return retval

    End Function

    Public Shared Function getRandomizedEntry() As JRandomSocialIdEntry
        Dim listfile As JRandomSocialIdFile
        Dim filename As String

        filename = JRandomSocialIdFile.getRandomRelativeFilename()

        If IsNothing(filename) Then
            Return Nothing
        End If

        'load file
        listfile = New JRandomSocialIdFile(filename)

        If listfile.getEntryCount() > 0 Then
            Return listfile.getEntry(0)
        Else
            Return Nothing
        End If

    End Function
    Public Shared Function getRandomFile() As JRandomSocialIdFile
        Dim listfile As JRandomSocialIdFile
        Dim filename As String

        filename = JRandomSocialIdFile.getRandomRelativeFilename()

        If IsNothing(filename) Then
            Return Nothing
        End If

        'load file
        listfile = New JRandomSocialIdFile(filename)

        If listfile.getEntryCount() > 0 Then
            Return listfile
        Else
            Return getRandomFile()
        End If

    End Function
    Public Shared Function getRandomRelativeFilename() As String
        Dim rnd As Random
        Dim total_files As Long
        Dim random_file_name As String
        Dim random_file_path As String
        Dim all_filenames As List(Of String)


        Dim id_dir As DirectoryInfo

        id_dir = New DirectoryInfo(JRandomSocialId.GlobalIdentitiesFolder)


        all_filenames = New List(Of String)

        For Each fl In id_dir.GetFiles()
            all_filenames.Add(fl.Name)
        Next

        total_files = all_filenames.Count
        If total_files = 0 Then
            Return Nothing
        End If

        rnd = New Random

        random_file_name = all_filenames.Item(rnd.Next(total_files))
        random_file_path = JRandomSocialId.GlobalIdentitiesFolder & random_file_name

        If System.IO.File.Exists(JRandomSocialId.GlobalIdentitiesFolder & random_file_name) Then
            Return random_file_name
        Else
            Return Nothing
        End If
    End Function

    Public Shared Function getNextId() As Integer
        Return JRandomSocialIdFile.getFileCount() + 1
    End Function
    Public Shared Function getFileCount() As Integer

        Dim id_dir As DirectoryInfo

        id_dir = New DirectoryInfo(JRandomSocialId.GlobalIdentitiesFolder)

        Return id_dir.GetFiles().Count

    End Function

    Public Sub New(ByVal _filename As String, Optional ByVal firstEntry As JRandomSocialIdEntry = Nothing)
        msg_filename = _filename
        Me.IDENTITIES_FOLDER = JRandomSocialId.GlobalIdentitiesFolder
        Me.setEntryCount(0)
        ReDim listitems(Me.getEntryCount())

        Me.setFilename(Me.IDENTITIES_FOLDER & _filename)

        If Not File.Exists(Me.getFilename()) Then
            Msg("Creating blank file " & _filename)
            Try

                File.CreateText(Me.getFilename()).Close()
            Catch ex As Exception
                Msg("Error creating file " & Me.getFilename() & "@ " & ex.StackTrace.ToString)
                Me.setFilename("")
            End Try
        Else
            Msg("Parsing " & _filename & "...")
            Me.Parse()
        End If


        If Not IsNothing(firstEntry) Then
            Me.addEntry(firstEntry)
        End If
    End Sub
    Public Function haveFile() As Boolean
        Return (Len(Me.listfilename) > 0)
    End Function
    Public Function GetAsJSON() As String
        Return JsonConvert.SerializeObject(Me)
    End Function
    Public Sub setList(ByVal val() As String)
        Dim x As Integer
        Dim entrystr() As String
        Dim formatted_entry As JRandomSocialIdEntry

        For x = 0 To val.Length - 1
            entrystr = Split(val(x), Me.getDelimiter())
            If entrystr.Length >= 2 Then
                formatted_entry = New JRandomSocialIdEntry


                Try
                    formatted_entry.setEntry(entrystr(0), entrystr(1), entrystr(2), entrystr(3), entrystr(4), entrystr(5), entrystr(6), entrystr(7), entrystr(8), CInt(entrystr(9)), CInt(entrystr(10)), CInt(entrystr(11)))
                Catch ex As Exception
                    Msg("Malformed entry '" & val(x) & "'")
                End Try

                'add to global list
                Me.addEntry(formatted_entry, False, False)
            End If 'valid entry
        Next
    End Sub
    Public Sub setList(ByVal val() As JRandomSocialIdEntry)
        Me.listitems = val
        Me.setEntryCount(Me.getList().Length)
    End Sub
    Public Function getList() As JRandomSocialIdEntry()
        Return Me.listitems
    End Function
    Public Sub setEntryCount(ByVal val As Integer)
        Me.listitemscount = val
    End Sub
    Public Function getEntryCount() As Integer
        Return Me.listitemscount
    End Function
    Public Function haveEmptySlot() As Integer
        Dim x As Integer

        For x = 0 To Me.getEntryCount() - 1
            If IsNothing(Me.listitems) Then
                Return x
            End If
        Next

        Return -1

    End Function
    Public Function Parse() As Boolean

        If Not Me.haveFile() Then
            Msg("Load err: no filename attached!")
            Return False
        End If

        ReDim Me.listitems(0)

        Try
            If Not File.Exists(Me.getFilename()) Then
                Err.Raise(100, 0, "ID file does not exist.")
            End If

            Me.setList(File.ReadAllLines(Me.getFilename()))

            Msg("Load " & Me.getEntryCount().ToString & " entries.")
            Return True

        Catch ex As Exception
            Msg("Error reading search query file " & Me.getFilename() & ": " & ex.Message & " @ " & ex.StackTrace.ToString)
            Return False
        End Try

    End Function
    Public Function addEntry(ByVal val As JRandomSocialIdEntry, Optional ByVal allowDup As Boolean = False, Optional ByVal writeFile As Boolean = True) As Boolean
        Dim index As Integer
        Dim addindex As Integer

        If Not allowDup Then

            If Me.isDuplicate(val) Then
                Msg("Not adding item. Duplicate = true.")
                Return False
            End If
        End If

        index = haveEmptySlot()

        Try

            If index = -1 Then
                index = Me.getEntryCount()
                ReDim Preserve Me.listitems(index)
                addindex = index
                index = index + 1
                Me.setEntryCount(index)
            Else
                index = index
            End If

            Me.listitems(addindex) = val

            If writeFile Then
                Return Me.writeList()
            Else
                Msg("Warning: file not written!")
            End If

            Return True

        Catch ex As Exception
            Msg("AddEntry err: " & ex.Message & " @ " & ex.StackTrace.ToString)
            Return False
        End Try
    End Function


    Public Function removeEntry(ByVal val As JRandomSocialIdEntry, Optional ByVal writeFile As Boolean = True) As Boolean
        Dim x As Integer
        Dim userentry As JRandomSocialIdEntry

        userentry = val

        For x = 0 To Me.getEntryCount() - 1

            If Me.getEntry(x) = userentry Then

                If x = Me.getEntryCount() - 1 Then
                    'pop array item
                    Call popLast()
                Else
                    setEntry(x, Nothing)
                End If

                If writeFile Then
                    Return Me.writeList()
                Else
                    Msg("Warning: file not written!")
                End If

                Return True
            End If
        Next

        Return False

    End Function
    Public Function isDuplicate(ByVal val As JRandomSocialIdEntry) As Boolean
        Dim x As Integer
        Dim userentry As JRandomSocialIdEntry

        userentry = val

        For x = 0 To Me.getEntryCount() - 1

            If Me.getEntry(x) = userentry Then
                Return True
            End If
        Next

        Return False

    End Function


    Private Sub popLast()
        'pop array item
        Dim count As Integer
        count = Me.listitems.Length - 1
        ReDim Preserve Me.listitems(count)
        Me.setEntryCount(Me.listitems.Length)

    End Sub

    Public Function writeList() As Boolean


        If Not Me.haveFile() Then
            Msg("writeList: no filename attached!")
            Return False
        End If

        Dim fs As FileStream
        Dim x As Integer
        Dim bytes() As Byte
        Dim entrycount As Integer

        fs = Nothing
        entrycount = 0

        Try

            fs = New FileStream(Me.getFilename(), FileMode.Create)

            For x = 0 To Me.getEntryCount() - 1
                If Not IsNothing(Me.listitems(x)) Then
                    bytes = Encoding.ASCII.GetBytes(Me.listitems(x).getFormatedEntry(Me.getDelimiter()) & vbNewLine)
                    fs.Write(bytes, 0, bytes.Length)
                    entrycount = entrycount + 1
                End If
            Next

            Msg("Succesfully wrote " & entrycount.ToString & " proxy entries to file.")
            fs.Close()

            Return True

        Catch ex As Exception
            Msg("Error writing: " & ex.Message & "@ " & ex.StackTrace.ToString)
            If Not IsNothing(fs) Then
                fs.Close()
            End If
            Return False
        End Try

    End Function
    Public Function GetAll() As String

        Dim x As Integer
        Dim entrycount As Integer
        Dim retval As String

        retval = ""
        entrycount = 0

        Try

            For x = 0 To Me.getEntryCount() - 1
                If Not IsNothing(Me.listitems(x)) Then
                    retval = retval & Me.listitems(x).getFormatedEntry(Me.getDelimiter()) & vbCrLf
                    entrycount = entrycount + 1
                End If
            Next

            Msg("Returned " & entrycount.ToString & " entries.")

            Return retval

        Catch ex As Exception
            Msg("Error returning proxy list: " & ex.Message & "@ " & ex.StackTrace.ToString)
            Return Nothing
        End Try

    End Function
    Public Sub setEntry(ByVal x As Integer, ByVal val As JRandomSocialIdEntry)
        Me.listitems(x) = val
    End Sub
    Public Function getEntry(ByVal x As Integer) As JRandomSocialIdEntry
        Return Me.listitems(x)
    End Function

    Private Sub Msg(ByVal txt As String)
        Debug.Print("BlingProxyFile[" & Me.msg_filename & "]: " & txt)

    End Sub


End Class
