Option Explicit On
Option Compare Text
Imports System.IO
Imports System.Globalization
Imports System.Threading
Imports TriniDATServerTypes


Public Class frmSellPackageWizard

    Private upload_package As PackagedApp
    Private selected_app As BosswaveApplication
    Private ReadOnly Property havePreSelectedApp() As Boolean
        Get
            Return Not IsNothing(Me.selected_app)
        End Get
    End Property
    Public Property PreSelectApp() As BosswaveApplication
        'passed by form owners.
        Get
            Return Me.selected_app
        End Get
        Set(ByVal value As BosswaveApplication)
            Me.selected_app = value
        End Set
    End Property


    Private Sub frmSellPackageWizard_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.fileBrowser.FileName = ""

        Me.upload_package = New PackagedApp()

        For Each c In Me.Controls

            If TypeOf c Is TextBox Or TypeOf c Is ComboBox Or TypeOf c Is DateTimePicker Then
                CType(c, Control).BackColor = Color.LavenderBlush
            End If

        Next

        If Not GlobalObject.haveApplicationCache Then
            MsgBox("No applications available.")
            Close()
        Else
            Call fillApps()
            Call fillRootCat()
            Call fillLanguages()
            Call fillSellZone()
        End If

    End Sub

    Private Sub fillSellZone()

        Me.chkSellToFree.Enabled = True
        Me.chkSellToEdu.Enabled = (Not GlobalObject.OfficialLicense.CurrentLicense = TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE)
        Me.chkSellToCorporate.Enabled = (Not GlobalObject.OfficialLicense.CurrentLicense = TRINIDAT_SERVER_LICENSE.T_LICENSE_FREE)

    End Sub
    Private Sub fillLanguages()
        Dim allitem As ListViewItem
        Dim langitem As ListViewItem
        Dim column_lang As System.Windows.Forms.ColumnHeader

        'init
        Me.lvLanguages.Items.Clear()
        Me.lvLanguages.Columns.Clear()

        column_lang = New System.Windows.Forms.ColumnHeader
        column_lang.Text = "Language"
        column_lang.Width = 350

        Me.lvLanguages.Columns.Add(column_lang)


        allitem = New ListViewItem
        allitem.Text = "All"
        allitem.Tag = Nothing

        Me.lvLanguages.FullRowSelect = True
        Me.lvLanguages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.lvLanguages.Items.Add(allitem)


        Dim ci As CultureInfo
        For Each ci In CultureInfo.GetCultures(CultureTypes.NeutralCultures)
            If InStr(ci.EnglishName, "invariant") = 0 Then
                langitem = New ListViewItem
                langitem.Text = ci.DisplayName
                langitem.Checked = True
                langitem.Tag = ci
                Me.lvLanguages.Items.Add(langitem)
            End If
        Next ci

        Me.lvLanguages.Items.Item(0).Checked = True
    End Sub
    Private Sub fillRootCat()
        Me.lstMainCat.Items.Clear()

        For x = 0 To PackagedApp_CategoryDescriptions.ROOTMAX Step 10
            Dim root_name As String
            root_name = PackagedApp_CategoryDescriptions.getRootName(x)

            If Not IsNothing(root_name) Then
                ' If root_name <> "Reserved" Then
                Me.lstMainCat.Items.Add(root_name)
                'End If
            End If

        Next

    End Sub
    Private Sub fillApps()

        Dim presel_index As Integer

        presel_index = -1

        Me.lstApps.Items.Clear()

        For Each app As BosswaveApplication In GlobalObject.ApplicationCache

            If Not app.Disabled Then
                Me.lstApps.Items.Add("Id " & app.Id.ToString & ": " & app.ApplicationName)
                If Me.havePreSelectedApp Then
                    If app.Id = Me.PreSelectApp.Id Then
                        presel_index = Me.lstApps.Items.Count - 1
                    End If
                End If
            End If

        Next

        If presel_index <> -1 Then
            Me.lstApps.SelectedIndex = presel_index
        End If
    End Sub

    Private Sub cmdBrowseIcon_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdBrowseIcon.Click
        fileBrowser.Filter = "Pictures|*.png;*.ico;*.jpg"
        If fileBrowser.ShowDialog() = DialogResult.OK Then
            Me.txtFileIcon.Text = fileBrowser.FileName

            If Not validateCurrentIcon() Then
                Me.txtFileIcon.Text = ""
                Me.txtFileIcon.BackColor = Color.Red
            Else
                Me.txtFileIcon.BackColor = Color.LightGreen
                Me.imgIcon.BackColor = Me.BackColor
            End If
        End If


    End Sub

    Private Function validateCurrentIcon() As Boolean
        Try

            If Not File.Exists(Me.txtFileIcon.Text) Then
                Throw New Exception("This file does not exist.")
            End If

            Return AutosizeImage(Me.txtFileIcon.Text, Me.imgIcon)

        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
            Return False
        End Try



    End Function
    Public Function AutosizeImage(ByVal ImagePath As String, ByVal picBox As PictureBox, Optional ByVal pSizeMode As PictureBoxSizeMode = PictureBoxSizeMode.CenterImage) As Boolean
        Try
            picBox.Image = Nothing
            picBox.SizeMode = pSizeMode
            Dim imgOrg As Bitmap
            Dim imgShow As Bitmap
            Dim g As Graphics
            Dim divideBy, divideByH, divideByW As Double
            imgOrg = DirectCast(Bitmap.FromFile(ImagePath), Bitmap)

            divideByW = imgOrg.Width / picBox.Width
            divideByH = imgOrg.Height / picBox.Height
            If divideByW > 1 Or divideByH > 1 Then
                If divideByW > divideByH Then
                    divideBy = divideByW
                Else
                    divideBy = divideByH
                End If

                imgShow = New Bitmap(CInt(CDbl(imgOrg.Width) / divideBy), CInt(CDbl(imgOrg.Height) / divideBy))
                imgShow.SetResolution(imgOrg.HorizontalResolution, imgOrg.VerticalResolution)
                g = Graphics.FromImage(imgShow)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.DrawImage(imgOrg, New Rectangle(0, 0, CInt(CDbl(imgOrg.Width) / divideBy), CInt(CDbl(imgOrg.Height) / divideBy)), 0, 0, imgOrg.Width, imgOrg.Height, GraphicsUnit.Pixel)
                g.Dispose()
            Else
                imgShow = New Bitmap(imgOrg.Width, imgOrg.Height)
                imgShow.SetResolution(imgOrg.HorizontalResolution, imgOrg.VerticalResolution)
                g = Graphics.FromImage(imgShow)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.DrawImage(imgOrg, New Rectangle(0, 0, imgOrg.Width, imgOrg.Height), 0, 0, imgOrg.Width, imgOrg.Height, GraphicsUnit.Pixel)
                g.Dispose()
            End If
            imgOrg.Dispose()

            picBox.Image = imgShow

            Return True

        Catch ex As Exception
            MsgBox(ex.ToString)
        End Try
        Return False

    End Function

    Private Sub lstApps_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstApps.Click

        'If MessageBox.Show("Warning: your existing information will be cleared. Are you sure?", "Publishing Wizard", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) <> Windows.Forms.DialogResult.Yes Then
        '    Me.Focus()
        '    Exit Sub
        'End If

        If Not Me.upload_package.haveApp Then Exit Sub

        Me.txtAppDescription.Text = Me.upload_package.SellApp.ApplicationDescription
        Me.txtTechSpec.Text = Me.upload_package.SellApp.ApplicationTechDescription
        Me.txtSellerBusiness.Text = Me.upload_package.SellApp.ApplicationOriginatingBusiness
        Me.txtSellAuthor.Text = Me.upload_package.SellApp.ApplicationAuthor
        Me.txtSellerEmail.Text = Me.upload_package.SellApp.ApplicationAuthorContactEmail
        Me.txtSellerWebsite.Text = Me.upload_package.SellApp.ApplicationAuthorContactWebsite
        Me.txtSellerPhone.Text = Me.upload_package.SellApp.ApplicationAuthorPhone
        Me.txtSellerSkypeId.Text = Me.upload_package.SellApp.ApplicationAuthorSkypeID
        Me.txtSellerYahooId.Text = Me.upload_package.SellApp.ApplicationAuthorYahooID


    End Sub

    Private Sub lstApps_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstApps.SelectedIndexChanged

        If Me.lstApps.SelectedIndex = -1 Then Exit Sub

        Me.lstApps.BackColor = Color.LightGreen

        setSelectedAppId()

        Me.txtAppDescription.Focus()

    End Sub


    Private Function setSelectedAppId() As Long

        If Not GlobalObject.haveApplicationCache Then Return -1

        For x = 0 To Me.lstApps.Items.Count - 1

            If x = Me.lstApps.SelectedIndex Then
                Dim appid_str As String
                Dim appid As Long
                Dim pos_end As Integer
                Dim pos_start As Integer

                appid_str = Me.lstApps.Items(x).ToString
                pos_end = InStr(appid_str, ":")
                If pos_end = 0 Then Return -1
                pos_end -= 0
                pos_start = InStrRev(appid_str, " ", pos_end)
                If pos_start = 0 Then Return -1
                pos_start += 1

                appid_str = Mid(appid_str, pos_start, pos_end - pos_start)
                appid = CLng(appid_str)

                If GlobalObject.ApplicationCache.haveApplication(appid) Then

                    Me.upload_package.SellApp = GlobalObject.ApplicationCache.AppById(appid)
                    Me.upload_package.genDependencyHashList()

                    Me.txtAppName.Text = Me.upload_package.SellApp.ApplicationName
                    Me.txtSellAuthor.Text = Me.upload_package.SellApp.ApplicationAuthor
                    Me.txtSellerWebsite.Text = Me.upload_package.SellApp.ApplicationAuthorContactWebsite
                    Me.txtSellerEmail.Text = Me.upload_package.SellApp.ApplicationAuthorContactEmail
                    Me.txtAppDescription.Text = Me.upload_package.SellApp.ApplicationDescription
                    Me.txtSellerBusiness.Text = Me.upload_package.SellApp.ApplicationOriginatingBusiness
                    Me.txtTechSpec.Text = Me.upload_package.SellApp.ApplicationTechDescription
                    If Me.upload_package.SellApp.haveApplicationPaymentInfo Then
                        Me.txtPaymentHandle.Text = Me.upload_package.SellApp.ApplicationPaymentInfo.handle.ToString
                    Else
                        Me.txtPaymentHandle.Text = GlobalSetting.PayKey
                    End If
                End If

            End If

        Next
    End Function

    Private Function getSelectedrootCategoryId() As Integer
        Return (lstMainCat.SelectedIndex) * 10
    End Function
    Private Sub lstMainCat_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstMainCat.SelectedIndexChanged
        If lstMainCat.SelectedIndex = -1 Then Exit Sub

        Me.lstMainCat.BackColor = Color.LightGreen
        Me.lstSubCat.Items.Clear()

        Dim x As Integer
        Dim sub_catname As String
        Dim sub_cats As TriniDATDictionaries.TriniDATWordDictionary
        Dim rangeid As Integer
        Dim haveItems As Boolean

        rangeid = Me.getSelectedrootCategoryId()
        sub_cats = PackagedApp_CategoryDescriptions.getSubCatNamesByRange(rangeid)

        If IsNothing(sub_cats) Then Exit Sub

        Dim y As Integer

        For y = 0 To sub_cats.getWordCount() - 1
            sub_catname = sub_cats.getWordList().Item(y)
            If Not IsNothing(sub_catname) Then
                Me.lstSubCat.Items.Add(sub_catname)
                haveItems = True
            End If
        Next

        If haveItems Then
            'always select
            Me.lstSubCat.SelectedIndex = 0
        End If
    End Sub

    Private Sub cmdLicenseAgreement_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmBrowseLicenseAgreement.Click
        fileBrowser.Filter = "Text Files|*.txt"
        If fileBrowser.ShowDialog() = DialogResult.OK Then
            Me.txtFileLicenseAgreement.Text = fileBrowser.FileName
            Me.txtFileLicenseAgreement.BackColor = Color.LightGreen
            Me.txtFileLicenseAgreement.Focus()
        End If

    End Sub

    Private Sub cmBrowseSetup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmBrowseSetup.Click
        fileBrowser.Filter = "Installation files|*.exe;*.msi"
        If fileBrowser.ShowDialog() = DialogResult.OK Then
            Me.txtFileSetup.Text = fileBrowser.FileName
            Me.txtFileSetup.BackColor = Color.LightGreen
            Me.txtFileSetup.Focus()
        End If
    End Sub

    Private Sub cmBrowseSrc_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmBrowseSrc.Click
        fileBrowser.Filter = "ZIP files|*.zip;*.rar;"
        If fileBrowser.ShowDialog() = DialogResult.OK Then
            Me.txtFileSrcZip.Text = fileBrowser.FileName
            Me.txtFileSrcZip.BackColor = Color.LightGreen
            Me.txtFileSrcZip.Focus()
        End If
    End Sub

    Private Sub cmBrowseDoc_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmBrowseDoc.Click
        fileBrowser.Filter = "Documents|*.doc;*.odt;*.pdf;*.xls;*.xlsx;"

        If fileBrowser.ShowDialog() = DialogResult.OK Then
            Me.txtFileDocs.Text = fileBrowser.FileName
            Me.txtFileDocs.BackColor = Color.LightGreen
            Me.txtFileDocs.Focus()
            Me.txtFileDocs.Focus()
        End If
    End Sub

    Private Sub startProgressThreaded(ByVal upload_data As PackagedApp)


        GlobalObject.CurrentUploadProgressForm = New frmUploadPackage
        GlobalObject.CurrentUploadProgressForm.UploadPackage = upload_data
        System.Windows.Forms.Application.Run(GlobalObject.CurrentUploadProgressForm)


    End Sub
    Private Function getCurrentValutaShortName() As String

        Select Case lstCurrency.Text

            Case "DOLLAR"
                Return "$"

            Case "EURO"
                Return "EU "

        End Select

        Return "UNKNOWN"

    End Function
    Private Sub cmdPublish_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdPublish.Click

        Dim banned_title As TriniDATDictionaries.TriniDATWordDictionary

        banned_title = New TriniDATDictionaries.TriniDATWordDictionary("", New List(Of String)({"trinidat", "hooker", "phising", "phiser", "fool", "bitch", "slut", "tramp", "suck", "porn", "krist", "reserved", "rserved", "pr0n", "pron", "sex", "teen", "lul", "child", "phony", "phreak", "jesus", "god", "christ", "leak", "xploit", "black", "sploit", "sploit", "shit", "piss", "fuck", "gertjan", "leeuw"}))

        '  Me.cmdPublish.Enabled = False

        'validate form.
        If Not Me.upload_package.haveApp Or Me.lstApps.SelectedIndex = -1 Then
            MsgBox("Please select your application.")
            Me.cmdPublish.Enabled = True
            Exit Sub
        End If

        If Me.lstCurrency.SelectedIndex = -1 Then
            If Me.txtPrice.Text <> "0" Then
                MsgBox("Please select your currency.")
                Me.lstCurrency.Focus()
                Exit Sub
            Else
                'default 0 DOLLAR.
                Me.lstCurrency.SelectedIndex = 0
            End If
        End If


        If Me.lstMainCat.SelectedIndex = -1 Then
            MsgBox("Please select a store category.")
            Me.lstMainCat.Focus()
            Exit Sub
        Else

            If Me.lstMainCat.Text = "Reserved" Then
                MsgBox("This app category is unavailable.")
                Me.lstMainCat.Focus()
                Exit Sub
            End If
        End If

        If Me.txtAppName.Text.Length < 1 Then
            MsgBox("Enter application name.")
            Me.txtAppName.Focus()
            Exit Sub
        Else
            For Each wrd As String In Me.txtAppName.Text.Split(" ")

                If banned_title.HasIn(wrd) Then
                    MessageBox.Show("Blocked entry: '" & wrd & "'. Change phrasing in order to continue.", "Language Filter", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Me.txtAppName.BackColor = Color.Red
                    Me.txtAppName.Focus()
                    Exit Sub
                End If

            Next

            Me.upload_package.SellApp.ApplicationNode.@name = Me.txtAppName.Text
        End If

        If Me.txtAppDescription.Text.Length < 1 Then
            MsgBox("Enter application description.")
            Me.txtAppDescription.Focus()
            Exit Sub
        Else

            For Each wrd As String In Me.txtAppDescription.Text.Split(" ")

                If banned_title.HasIn(wrd) Then
                    MessageBox.Show("Blocked entry: '" & wrd & "'. Change phrasing in order to continue.", "Language Filter", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Me.txtAppDescription.BackColor = Color.Red
                    Me.txtAppDescription.Focus()
                    Exit Sub
                End If

            Next

            Me.upload_package.SellApp.ApplicationNode.@description = HttpUtility.UrlEncode(HttpUtility.HtmlEncode(Me.txtAppDescription.Text))
        End If


        'TARGET LANGUAGES
        Dim language_string As String
        language_string = ""

        If lvLanguages.Items(0).Checked = True Then
            language_string = "any"
        Else
            For Each lvitem In Me.lvLanguages.Items
                If Not IsNothing(lvitem.tag) Then
                    language_string &= lvitem.tag.ThreeLetterISOLanguageName & ";"
                End If
            Next

        End If

        Dim target_editions As String

        'TARGET EDITIONS
        If Not Me.haveAnyVisibilityChecked Then
            Me.lblVisibilityHelp.ForeColor = Color.Red
            Me.cmdPublish.Enabled = True
            MessageBox.Show("Please set checkboxes in application visibility section.", "App Publisher", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Exit Sub
        Else
            target_editions = ""

            If Me.chkSellToFree.Checked Then
                target_editions &= "any;"
            End If

            If Me.chkSellToCorporate.Enabled And Me.chkSellToCorporate.Checked Then
                target_editions &= "corporate;"
            End If

            If Me.chkSellToEdu.Enabled And Me.chkSellToEdu.Checked Then
                target_editions &= "edu;"
            End If

        End If


        Me.upload_package.DocumentationFile = Me.txtFileDocs.Text
        Me.upload_package.InstallerFile = Me.txtFileSetup.Text
        Me.upload_package.SourceCodeFile = Me.txtFileSrcZip.Text
        Me.upload_package.LicenseFile = Me.txtFileLicenseAgreement.Text
        Me.upload_package.IconFile = Me.txtFileIcon.Text

        'upload resized version.
        If Me.upload_package.haveIconFile Then
            Dim resizedTempIcon As String
            Try
                resizedTempIcon = GlobalSetting.getTempDir()
                resizedTempIcon &= "manifesticon" & Me.upload_package.IconFile_Info.Extension()
                Me.imgIcon.Image.Save(resizedTempIcon)

            Catch ex As Exception
                MsgBox("Unable to use icon file. Please choose a different file.")
                MsgBox(ex.Message)
                Exit Sub
            End Try

            Me.upload_package.IconFile = resizedTempIcon
        End If

    

        'read to go.
        Dim confirm_msg As String

        If CLng(Me.txtPrice.Text) = 0 Then
            confirm_msg = "Publish '" & Me.upload_package.SellApp.ApplicationNode.@name & "' as freeware?"
        Else
            confirm_msg = "Sell '" & Me.upload_package.SellApp.ApplicationNode.@name & "' for " & Me.getCurrentValutaShortName() & Me.txtPrice.Text & "?"
            Me.upload_package.SellPrice = CDbl(Me.txtPrice.Text)
        End If


        If MessageBox.Show(confirm_msg, "App Store Confirmation", MessageBoxButtons.OKCancel) <> DialogResult.OK Then
            Me.cmdPublish.Enabled = True
            Exit Sub
        End If

        If Not Me.upload_package.SellApp.haveApplicationPaymentInfo Then
            Dim payment_xml As String
            payment_xml = "<paymentdata><paymenthandle>" & Me.txtPaymentHandle.Text & "</paymenthandle></paymentdata>"
            Me.upload_package.SellApp.ApplicationPaymentInfo = payment_descriptor.createFromXPayment(XElement.Parse(payment_xml))
        End If

        Me.upload_package.SellApp.ApplicationNode.Add(Me.upload_package.SellApp.ApplicationPaymentInfo.getAsNode())
        Me.upload_package.SellApp.ApplicationNode.@originatinglicense = HttpUtility.UrlEncode(CInt(GlobalObject.OfficialLicense.CurrentLicense).ToString)
        Me.upload_package.SellApp.ApplicationNode.@price = Me.txtPrice.Text
        Me.upload_package.SellApp.ApplicationNode.@currency = Me.lstCurrency.Text
        Me.upload_package.SellApp.ApplicationNode.@technicaldescription = HttpUtility.UrlEncode(HttpUtility.HtmlEncode(Me.txtTechSpec.Text))
        Me.upload_package.SellApp.ApplicationNode.@released = HttpUtility.UrlEncode(Me.datReleaseDate.Text)
        Me.upload_package.SellApp.ApplicationNode.@copyright = HttpUtility.UrlEncode(HttpUtility.HtmlEncode(Me.txtCopyright.Text))
        Me.upload_package.SellApp.ApplicationNode.@businessname = HttpUtility.UrlEncode(Me.txtSellerBusiness.Text)
        Me.upload_package.SellApp.ApplicationNode.@editions = target_editions
        Me.upload_package.SellApp.ApplicationNode.@author = HttpUtility.UrlEncode(Me.txtSellAuthor.Text)
        Me.upload_package.SellApp.ApplicationNode.@authorcontactemail = HttpUtility.UrlEncode(Me.txtSellerEmail.Text)
        Me.upload_package.SellApp.ApplicationNode.@authorcontactwebsite = HttpUtility.UrlEncode(Me.txtSellerWebsite.Text)
        Me.upload_package.SellApp.ApplicationNode.@authorcontactskypeid = HttpUtility.UrlEncode(Me.txtSellerSkypeId.Text)
        Me.upload_package.SellApp.ApplicationNode.@authorcontactyahooid = HttpUtility.UrlEncode(Me.txtSellerYahooId.Text)
        Me.upload_package.SellApp.ApplicationNode.@authorcontactphone = HttpUtility.UrlEncode(Me.txtSellerPhone.Text)
        Me.upload_package.SellApp.ApplicationNode.@rootcat = Me.getSelectedrootCategoryId().ToString
        Me.upload_package.SellApp.ApplicationNode.@language = language_string
        Me.upload_package.SellApp.ApplicationNode.@version = GlobalObject.getVersionShort()
        Me.upload_package.genDependencyHashList()
        Me.upload_package.SellApp.ApplicationNode.@sellhash = Me.upload_package.MD5Secret

        If Not Me.lstSubCat.SelectedIndex = -1 Then
            Me.upload_package.SellApp.ApplicationNode.@subcat = Me.lstSubCat.SelectedIndex.ToString
        End If

        'MessageBox.Show(Me.upload_package.SellApp.ApplicationNode.ToString)
        'MessageBox.Show(Me.upload_package.SellApp.XML.ToString)


        Call uploadPackage()

        Me.cmdPublish.Enabled = True
        Me.Tag = "OK"

    End Sub

    Private Function uploadPackage() As Boolean
        Dim progress_thread As Thread

        progress_thread = New Thread(AddressOf startProgressThreaded)
        progress_thread.SetApartmentState(System.Threading.ApartmentState.STA)
        progress_thread.Start(Me.upload_package)

    End Function
    Private Sub lvLanguages_ItemChecked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ItemCheckedEventArgs) Handles lvLanguages.ItemChecked

        Dim new_check_state As Boolean

        If IsNothing(e.Item.Tag) Then

            'all checkstate changed.

            new_check_state = e.Item.Checked
            lvLanguages.Items(0).Tag = "ignore"

            For Each lvitem In Me.lvLanguages.Items
                lvitem.checked = new_check_state
            Next
            lvLanguages.Items(0).Tag = Nothing
        Else
            'deselect all languages item.
            If lvLanguages.Items(0).Checked And Not TypeOf lvLanguages.Items(0).Tag Is String Then
                lvLanguages.Items(0).Tag = "ignore"
                lvLanguages.Items(0).Checked = False
                lvLanguages.Items(0).Tag = Nothing
            End If

        End If
    End Sub

    Private Sub txtPrice_Leave(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtPrice.Leave
        Me.txtPrice.Text = Trim(txtPrice.Text)

        If txtPrice.Text.ToString.Length < 1 Then
            Me.txtPrice.Text = "0"
        Else
            If Not IsNumeric(Me.txtPrice.Text) Then
                Me.txtPrice.Text = "0"
            Else
                If CLng(Me.txtPrice.Text) < 0 Then
                    Me.txtPrice.Text = "0"
                End If
            End If
        End If
    End Sub

    Private Sub txtPrice_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPrice.TextChanged

    End Sub

    Private Sub txtAppDescription_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAppDescription.Leave
        If txtAppDescription.Text.Length = 0 Then
            Me.txtAppDescription.BackColor = Color.Red
        End If
    End Sub

    Private Sub txtTechSpec_Leave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtTechSpec.Leave
        If txtTechSpec.Text.Length = 0 Then
            Me.txtTechSpec.BackColor = Color.LavenderBlush
        End If
    End Sub

    Private Sub txtAppName_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtAppName.TextChanged
        If txtAppName.Text.trim.Length > 0 Then
            Me.txtAppName.BackColor = Color.LightGreen
        Else
            Me.txtAppName.BackColor = Color.Red
        End If
    End Sub

    Private Sub lstValuta_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstCurrency.SelectedIndexChanged
        Me.lstCurrency.BackColor = Color.LightGreen
    End Sub

    Private Sub lstSubCat_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstSubCat.SelectedIndexChanged
        If lstSubCat.SelectedIndex > -1 Then
            lstSubCat.BackColor = Color.LightBlue

        End If
    End Sub

    Private Sub txtPaymentHandle_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPaymentHandle.TextChanged
        If txtPaymentHandle.Text.trim.Length > 0 Then
            txtPaymentHandle.BackColor = Color.LightGreen
        Else
            txtPaymentHandle.BackColor = Color.Red
        End If

    End Sub

    Private Sub txtSellAuthor_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSellAuthor.TextChanged

        If txtSellAuthor.Text.trim.Length > 0 Then
            txtSellAuthor.BackColor = Color.LightGreen
        Else
            txtSellAuthor.BackColor = Color.Red
        End If

    End Sub

    Private Sub chkSellToFree_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkSellToFree.CheckedChanged

        If Me.haveAnyVisibilityChecked Then
            Me.lblVisibilityHelp.ForeColor = Color.Green
        Else
            Me.lblVisibilityHelp.ForeColor = Color.Red
        End If

    End Sub

    Private ReadOnly Property haveAnyVisibilityChecked() As Boolean
        Get

            Return Me.chkSellToCorporate.Checked Or Me.chkSellToFree.Checked Or Me.chkSellToEdu.Checked
        End Get
    End Property

    Private Sub chkSellToEdu_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSellToEdu.CheckedChanged
        If Me.haveAnyVisibilityChecked Then
            Me.lblVisibilityHelp.ForeColor = Color.Green
        Else
            Me.lblVisibilityHelp.ForeColor = Color.Red
        End If

    End Sub

    Private Sub chkSellToEdu_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSellToEdu.Click

    End Sub

    Private Sub chkSellToCorporate_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkSellToCorporate.CheckedChanged
        If Me.haveAnyVisibilityChecked Then
            Me.lblVisibilityHelp.ForeColor = Color.Green
        Else
            Me.lblVisibilityHelp.ForeColor = Color.Red
        End If
    End Sub

    Private Sub cmdCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdCancel.Click
        Me.Tag = "ABORT"
        Me.DialogResult = DialogResult.Abort

    End Sub

    Private Sub txtAppDescription_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtAppDescription.TextChanged
        If txtAppDescription.Text.trim.Length > 0 Then
            Me.txtAppDescription.BackColor = Color.LightGreen
        Else
            Me.txtAppDescription.BackColor = Color.Red
        End If
    End Sub
End Class