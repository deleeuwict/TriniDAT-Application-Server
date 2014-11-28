Option Explicit On
Option Compare Text

Imports TriniDATHTTPTypes
Imports System.Xml
Imports System.Web
Imports System.IO

Public Class frmQueryEditor

    'WIN32 API
Private Declare Function SendMessage Lib "user32.dll" Alias "SendMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As IntPtr) As IntPtr

   
    Public Delegate Sub EmptSubroutine()
    Private screen_location As Point

    Public Delegate Sub OnFormClosedthrd(ByVal lastpos As Point)

    Private event_onclose As OnFormClosedthrd
    Private event_logger As TriniDATTypeLogger
    Private mapping_xdoc As XDocument
    Private Node_MappingPointroot As XElement
    Private editbox As TextBox
    Private hitinfo As ListViewHitTestInfo

    Public BringToFrontThreaded As New EmptSubroutine(AddressOf BringtoFrontNow)

    Private dragging_tags As Boolean
    Private drag_startpos As Point
    Private all_conditions_lb As ComboBox
    Private all_mapping_point_stat_fields As ComboBox
    Private all_mapping_points As ComboBox
    Private custom_combo As ComboBox
    Private current_application As BosswaveVisualAppEditor
    Private app_index As BosswaveAppCache
    Private gapp_new_text As String
    Private gmapping_point_chainlevel_opt As String

    Private Sub BringtoFrontNow()
        Me.Activate()
    End Sub

    Private Property MappingPointStats As XDocument
        Get
            Return Me.mapping_xdoc
        End Get
        Set(ByVal value As XDocument)
            Me.mapping_xdoc = value

            If IsNothing(Me.mapping_xdoc) Then
                Me.Node_MappingPointroot = Nothing
            Else
                Me.Node_MappingPointroot = Me.mapping_xdoc.Nodes(0)
            End If

        End Set
    End Property
    Private Function getMappingPointRootNode() As XElement
        Return Me.Node_MappingPointroot
    End Function
    Private ReadOnly Property haveMappingPointStats As Boolean
        Get
            Return Not IsNothing(Me.MappingPointStats)
        End Get
    End Property
    Public ReadOnly Property getSelectedItem(ByVal lv As ListView) As ListViewItem
        Get
            If lv.SelectedIndices.Count < 1 Then Return Nothing

            Dim selitem As ListViewItem
            Try
                selitem = lv.Items(lv.SelectedIndices(0))
                Return selitem

            Catch ex As Exception
                Return Nothing
            End Try
        End Get
    End Property

    Private Sub createActionTabs(ByVal page As BosswaveTabbed, ByVal xml_actions As XDocument)

        Dim q = From action_tab In xml_actions.Descendants("actions").Descendants("action") Order By action_tab.@index Ascending

        For Each actionNode In q
            Call Me.createActionTab(page, actionNode)
        Next

    End Sub

    Private Sub createActionTab(ByVal tabctrl As BosswaveTabbed, ByVal xtab As XElement)

        Dim action_tab As TabPage
        Dim action_tab_size As Size
        Dim action_listview As ListView
        Dim action_listview_col1 As System.Windows.Forms.ColumnHeader
        Dim action_listview_col2 As System.Windows.Forms.ColumnHeader
        Dim action_listview_col3 As System.Windows.Forms.ColumnHeader

        action_tab_size = New Size
        '        action_tab_size = New System.Drawing.Size(627, 258)


        'TAB
        action_tab = New TabPage
        action_tab.BackColor = System.Drawing.Color.SlateGray
        action_tab.Text = tabctrl.Tag & ": " & xtab.@tabname
        action_tab.Location = New System.Drawing.Point(4, 4)
        action_tab.Name = Replace(action_tab.Text, " ", "_")
        action_tab.Padding = New System.Windows.Forms.Padding(3)
        action_tab_size = New Size(tabctrl.Width - action_tab.Padding.Horizontal - 13, tabctrl.Height - action_tab.Padding.Vertical - 3)
        action_tab.TabIndex = 0

        'LISTVIEW
        action_listview = New ListView

        action_listview_col1 = New System.Windows.Forms.ColumnHeader
        action_listview_col2 = New System.Windows.Forms.ColumnHeader
        action_listview_col3 = New System.Windows.Forms.ColumnHeader

        action_listview.BackColor = System.Drawing.Color.Beige
        action_listview.BorderStyle = System.Windows.Forms.BorderStyle.None
        action_listview.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {action_listview_col1, action_listview_col2, action_listview_col3})
        action_listview.ContextMenuStrip = Me.mnuMutations
        action_listview.ForeColor = System.Drawing.Color.Black
        action_listview.FullRowSelect = True
        action_listview.GridLines = True
        action_listview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        action_listview.LabelEdit = True
        action_listview.Location = New System.Drawing.Point(4, 0)
        action_listview.Margin = New System.Windows.Forms.Padding(4, 3, 4, 3)
        action_listview.Name = action_tab.Name & "Lv"
        action_listview.Scrollable = False
        action_listview.Size = New Size(action_tab_size.Width, action_tab_size.Height - 22)
        action_listview.UseCompatibleStateImageBehavior = False
        action_listview.View = System.Windows.Forms.View.Details
        '
        'COLUMNS
        '=======

        action_listview_col1.Text = "Object"
        action_listview_col1.Width = 200
        '
        'ColumnHeader7
        '
        action_listview_col2.Text = "Condition"
        action_listview_col2.Width = 200
        '
        'ColumnHeader8
        '
        action_listview_col3.Text = "Value"
        action_listview_col3.Width = 200

        action_listview.Groups.Clear()

        'add listview
        action_tab.Controls.Add(action_listview)

        'add page
        tabctrl.Controls.Add(action_tab)

        'Add Items
        '  Call AddAction(action_listview, xtab)
        '  Call AddActionParameters(action_listview, xtab)
        ' Call AddActionProperty(action_listview, xtab)


        'EVENT HANDLERS
        '======
        AddHandler action_listview.MouseMove, AddressOf Me.Datasets_MouseHover
        AddHandler action_listview.KeyPress, AddressOf Me.lvAction_KeyPress
        AddHandler action_listview.MouseDoubleClick, AddressOf Me.lvAction_MouseDoubleClick
        ' AddHandler action_listview.Click, AddressOf Me.lvAction_Click
        AddHandler action_listview.SelectedIndexChanged, AddressOf Me.lvAction_SelectedIndexChanged
        AddHandler action_listview.BeforeLabelEdit, AddressOf Me.lvAction_BeforeLabelEdit
        AddHandler action_listview.Enter, AddressOf Me.lvAction_Enter


    End Sub
    'Private Sub AddActionParameters(ByVal lv As ListView, ByVal xaction As XElement)

    '    lv.Groups.Add(New ListViewGroup("Parameters", HorizontalAlignment.Center))

    '    'PARAMETERS
    '    '=========
    '    Dim lvMethodParameterItem As ListViewItem
    '    Dim lvMethodParameterSubItem As ListViewItem.ListViewSubItem
    '    Dim lvMethodParameterItemConfigParameter As BosswaveConfigItem
    '    Dim lvMethodParameterSubItemConfigParameter As BosswaveConfigItem

    '    Dim q = From action_tab In xaction.Descendants("parameter") Order By action_tab.@index Ascending

    '    For Each actionParameter In q
    '        lvMethodParameterItemConfigParameter = New BosswaveConfigItem(actionParameter)
    '        lvMethodParameterSubItemConfigParameter = New BosswaveConfigItem(actionParameter)

    '        'Item tag
    '        lvMethodParameterItemConfigParameter.ItemType = BosswaveAppEngineField.ParameterName
    '        lvMethodParameterItemConfigParameter.Parent = lv
    '        lvMethodParameterItemConfigParameter.configKey = lvMethodParameterItem
    '        lvMethodParameterItemConfigParameter.configSelector = lvMethodParameterSubItem

    '        'Subitem tag
    '        lvMethodParameterSubItemConfigParameter.ItemType = BosswaveAppEngineField.ParameterValue
    '        lvMethodParameterSubItemConfigParameter.Parent = lv
    '        lvMethodParameterSubItemConfigParameter.configKey = lvMethodParameterItem
    '        lvMethodParameterSubItemConfigParameter.configSelector = lvMethodParameterSubItem

    '        'Lv Item
    '        'lvMethodParameterSubItem =lvMethodParameterItemConfigParameter.getAsListviewsubItem(
    '        lvMethodParameterSubItem.Text = "(none)"
    '        lvMethodParameterSubItem.Tag = lvMethodParameterSubItemConfigParameter


    '        lvMethodParameterItem.Text = actionParameter.@usertitle
    '        lvMethodParameterItem.SubItems.Add(lvMethodParameterSubItem)
    '        lvMethodParameterItem.Group = lv.Groups(1)
    '        lvMethodParameterItem.Tag = lvMethodParameterItemConfigParameter
    '        lv.Items.Add(lvMethodParameterItem)
    '    Next

    'End Sub

    'Private Sub AddAction(ByVal lv As ListView, ByVal xaction As XElement)

    '    lv.Groups.Add(New ListViewGroup("Action", HorizontalAlignment.Center))

    '    'ACTIONS
    '    '=======
    '    Dim lvActionItem As ListViewItem
    '    Dim lvActionItemTag As BosswaveConfigItem
    '    Dim lvActionSubitem1 As ListViewItem.ListViewSubItem
    '    Dim lvActionSubitem2 As ListViewItem.ListViewSubItem

    '    lvActionItemTag = New BosswaveConfigItem
    '    lvActionItemTag.Parent = lv
    '    lvActionItemTag.ItemType = BosswaveAppEngineField.Action

    '    lvActionItem = New ListViewItem
    '    lvActionItem.Text = xaction.@name

    '    lvActionSubitem1 = New ListViewItem.ListViewSubItem
    '    lvActionSubitem1.Text = "" 'condition
    '    lvActionSubitem1.Tag = lvActionItemTag

    '    lvActionSubitem2 = New ListViewItem.ListViewSubItem
    '    lvActionSubitem2.Text = "" 'value
    '    lvActionSubitem1.Tag = lvActionSubitem1.Tag

    '    lvActionItem.SubItems.Add(lvActionSubitem1)
    '    lvActionItem.SubItems.Add(lvActionSubitem2)

    '    lvActionItem.Group = lv.Groups(0)
    '    lvActionItem.Tag = lvActionItemTag

    '    lv.Items.Add(lvActionItem)




    'End Sub
    'Private Sub AddActionProperty(ByVal lv As ListView, ByVal xaction As XElement)
    '    lv.Groups.Add(New ListViewGroup("Triggered By Condition", HorizontalAlignment.Center))

    '    'PROPERTIES
    '    '=========

    '    Dim lvPropertyItem As ListViewItem
    '    Dim lvPropertyItemTag As BosswaveConfigItem

    '    Dim lvConditionSubitem As ListViewItem.ListViewSubItem
    '    Dim lvConditionSubItemTag As BosswaveConfigItem

    '    Dim lvValueSubitem As ListViewItem.ListViewSubItem
    '    Dim lvValueSubitemTag As BosswaveConfigItem


    '    lvPropertyItem = New ListViewItem
    '    lvConditionSubitem = New ListViewItem.ListViewSubItem
    '    lvValueSubitem = New ListViewItem.ListViewSubItem


    '    'LINK TAG CONFIG ITEMS
    '    '=======
    '    lvPropertyItemTag = New BosswaveConfigItem
    '    lvPropertyItemTag.Parent = lv
    '    lvPropertyItemTag.ItemType = BosswaveAppEngineField.TriggerConditionPropertyName
    '    lvPropertyItemTag.configKey = lvPropertyItem
    '    lvPropertyItemTag.configOperator = lvConditionSubitem
    '    lvPropertyItemTag.configSelector = lvValueSubitem

    '    lvConditionSubItemTag = New BosswaveConfigItem(xaction)
    '    lvConditionSubItemTag.Parent = lv
    '    lvConditionSubItemTag.ItemType = BosswaveAppEngineField.TriggerConditionCondition
    '    lvConditionSubItemTag.configKey = lvPropertyItem
    '    lvConditionSubItemTag.configOperator = lvConditionSubitem
    '    lvConditionSubItemTag.configSelector = lvValueSubitem

    '    lvValueSubitemTag = New BosswaveConfigItem
    '    lvValueSubitemTag.Parent = lv
    '    lvValueSubitemTag.ItemType = BosswaveAppEngineField.TriggerConditionPropertyValue
    '    lvValueSubitemTag.configKey = lvPropertyItem
    '    lvValueSubitemTag.configOperator = lvConditionSubitem
    '    lvValueSubitemTag.configSelector = lvValueSubitem

    '    'SUBITEM
    '    '=====

    '    lvConditionSubitem.Text = "(select)" 'condition
    '    lvConditionSubitem.Tag = lvConditionSubItemTag


    '    lvValueSubitem.Text = "(value)" 'condition
    '    lvValueSubitem.Tag = lvValueSubitemTag




    '    lvPropertyItem.Text = "(field name)"
    '    lvPropertyItem.SubItems.Add(lvConditionSubitem)
    '    lvPropertyItem.SubItems.Add(lvValueSubitem)
    '    lvPropertyItem.Group = lv.Groups(2)
    '    lvPropertyItem.Tag = lvPropertyItemTag
    '    lv.Items.Add(lvPropertyItem)

    'End Sub

    Private Sub frmQueryEditor_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'TABS
        '====
        Dim actions_xml As String


        Try

            '   Call createActionTabs(Me.tabsAction, XDocument.Parse(actions_xml))


        Catch ex As Exception
            MsgBox("Error creating tabs: " & ex.Message)
            Exit Sub
        End Try


        Call populateAppList()
        Call populateMappingPoints(Me.listMappingPoint, gmapping_point_chainlevel_opt)
        Call populateMappingPoints(Me.all_mapping_points)

        'CONDITIONS
        '===========

        'STATIC CONTROL CONFIG
        lstGetMethod.SelectedIndex = 0
        lstGetMethod.BackColor = Me.listMappingPoint.BackColor

        'COMPILER CONFIGURATION EDITOR
        lvCompilerConfigurationEditor.Groups.Clear()
        lvCompilerConfigurationEditor.Groups.Add(New ListViewGroup("Entry Point Parameters", HorizontalAlignment.Center))

        Dim lvConfigItem As ListViewItem

        lvConfigItem = New ListViewItem
        lvConfigItem.Text = "spider"
        lvConfigItem.SubItems.Add("http://www.microsoft.com/")
        lvConfigItem.Group = lvCompilerConfigurationEditor.Groups(0)
        Me.lvCompilerConfigurationEditor.Items.Add(lvConfigItem)




    End Sub
    Private Sub populateMappingPointDependencies()
        Dim selected_url As String
        Dim mapping_point As mappingPointRoot
        Dim jindex As Integer

        selected_url = listMappingPoint.SelectedItem


        'get the JClasses
        'mapping_point = BosswaveApplicationHost.getDirectory().getMappingPointByURL(selected_url)

        Dim query_classes = From class_name In mapping_point.getClasses()
        jindex = 0

        lvJClass.Items.Clear()

        'For jindex = 1 To query_classes.Count - 2 'ignores Alpha & OMega
        For jindex = 0 To query_classes.Count - 1 'ignore Alpha & OMega

            Dim lvItem As ListViewItem
            lvItem = New ListViewItem
            lvItem.Text = "#" & jindex.ToString
            lvItem.SubItems.Add(query_classes(jindex).getName())
            lvItem.SubItems.Add(query_classes(jindex).Description)
            lvJClass.Items.Add(lvItem)
        Next

        'load the stat file
        '    Me.MappingPointStats = StatParser.getByURL(selected_url)

        Me.lblNoData.Visible = Not Me.haveMappingPointStats

        If Me.haveMappingPointStats Then
            GlobalObject.Msg(Me.MappingPointStats.ToString)
        End If
    End Sub
    Private Sub populateAppList()

        'PROGRAMS
        lstProgram.Items.Clear()

        Dim x As Integer


        For x = 0 To GlobalObject.ApplicationCache.Count - 1
            Me.lstProgram.Items.Add(GlobalObject.ApplicationCache.Item(x).ApplicationName)
        Next

        lstProgram.Items.Add("(new)")
    End Sub

    Private Sub populateMappingPoints(ByVal cb As ComboBox, Optional ByVal firstItem As String = Nothing)
        'MAPPING POINTS
        Dim q '= From mp In BosswaveApplicationHost.getDirectory() Order By mp.MappingPoint.getURI() Ascending

        cb.Items.Clear()

        If Not IsNothing(firstItem) Then
            cb.Items.Add(firstItem)
        End If

        For Each mp_description In q
            cb.Items.Add(mp_description.MappingPoint.getURI())
        Next

    End Sub
    'Private Sub populateInvokeProgram()
    '    'ACTIONS
    '    '=======
    '    lvSet.Groups.Clear()
    '    lvSet.Groups.Add(New ListViewGroup("Action", HorizontalAlignment.Center))
    '    lvSet.Groups.Add(New ListViewGroup("Parameters", HorizontalAlignment.Center))
    '    lvSet.Groups.Add(New ListViewGroup("Trigger Condition", HorizontalAlignment.Center))

    '    Dim lvMethodItem As ListViewItem

    '    lvMethodItem = New ListViewItem
    '    lvMethodItem.Text = "Call New Profile"
    '    lvMethodItem.SubItems.Add("")
    '    lvMethodItem.SubItems.Add("AppName")
    '    lvMethodItem.Group = lvSet.Groups(0)
    '    Me.lvSet.Items.Add(lvMethodItem)

    '    'PARAMETERS
    '    '=========
    '    lvMethodItem = New ListViewItem
    '    lvMethodItem.Text = "Execution Mode"
    '    lvMethodItem.SubItems.Add("Any Match / One Time Only")
    '    lvMethodItem.Group = lvSet.Groups(1)
    '    Me.lvSet.Items.Add(lvMethodItem)


    '    'PROPERTIES
    '    '=========

    '    Dim lvPropertyItem As ListViewItem

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "(statistic name)"
    '    lvPropertyItem.SubItems.Add("Is")
    '    lvPropertyItem.SubItems.Add("unverified_email")
    '    lvPropertyItem.Group = lvSet.Groups(2)
    '    Me.lvSet.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "My Property"
    '    lvPropertyItem.SubItems.Add("Greater than")
    '    lvPropertyItem.SubItems.Add("10")
    '    lvPropertyItem.Group = lvSet.Groups(2)
    '    Me.lvSet.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "keyword"
    '    lvPropertyItem.SubItems.Add("has")
    '    lvPropertyItem.SubItems.Add("excitement")
    '    lvPropertyItem.Group = lvSet.Groups(2)
    '    Me.lvSet.Items.Add(lvPropertyItem)

    'End Sub

    'Private Sub populatePutStat()
    '    'ACTIONS
    '    '=======
    '    lvPutStatMethod.Groups.Clear()
    '    lvPutStatMethod.Groups.Add(New ListViewGroup("Action", HorizontalAlignment.Center))
    '    lvPutStatMethod.Groups.Add(New ListViewGroup("Trigger Condition", HorizontalAlignment.Center))

    '    Dim lvMethodItem As ListViewItem

    '    lvMethodItem = New ListViewItem
    '    lvMethodItem.Text = "Name"
    '    lvMethodItem.SubItems.Add("Value")
    '    lvMethodItem.SubItems.Add("")
    '    lvMethodItem.Group = lvPutStatMethod.Groups(0)
    '    Me.lvPutStatMethod.Items.Add(lvMethodItem)


    '    'PROPERTIES
    '    '=========

    '    Dim lvPropertyItem As ListViewItem

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "SetName"
    '    lvPropertyItem.SubItems.Add("")
    '    lvPropertyItem.SubItems.Add("SetA")
    '    lvPropertyItem.Group = lvPutStatMethod.Groups(1)
    '    Me.lvPutStatMethod.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "My Property"
    '    lvPropertyItem.SubItems.Add("Greater than")
    '    lvPropertyItem.SubItems.Add("10")
    '    lvPropertyItem.Group = lvPutStatMethod.Groups(1)
    '    Me.lvPutStatMethod.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "keyword"
    '    lvPropertyItem.SubItems.Add("has")
    '    lvPropertyItem.SubItems.Add("excitement")
    '    lvPropertyItem.Group = lvPutStatMethod.Groups(1)
    '    Me.lvPutStatMethod.Items.Add(lvPropertyItem)


    '    'LISTVIEW POSITION
    '    Me.lvPutStatMethod.Location = Me.lvSet.Location

    'End Sub
    'Private Sub populateCPU2Condition()
    '    'ACTIONS
    '    '=======
    '    lvCPU2Condition.Groups.Clear()
    '    lvCPU2Condition.Groups.Add(New ListViewGroup("Action", HorizontalAlignment.Center))
    '    lvCPU2Condition.Groups.Add(New ListViewGroup("Switch Condition", HorizontalAlignment.Center))


    '    Dim lvMethodItem As ListViewItem

    '    lvMethodItem = New ListViewItem
    '    lvMethodItem.Text = "Switch To Second CPU"
    '    lvMethodItem.SubItems.Add("")
    '    lvMethodItem.SubItems.Add("")
    '    lvMethodItem.Group = lvCPU2Condition.Groups(0)
    '    Me.lvCPU2Condition.Items.Add(lvMethodItem)


    '    'PROPERTIES
    '    '=========

    '    Dim lvPropertyItem As ListViewItem


    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "(statistic name)"
    '    lvPropertyItem.SubItems.Add("Is")
    '    lvPropertyItem.SubItems.Add("verified_email")
    '    lvPropertyItem.Group = lvCPU2Condition.Groups(1)
    '    Me.lvCPU2Condition.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "My Property"
    '    lvPropertyItem.SubItems.Add("Greater than")
    '    lvPropertyItem.SubItems.Add("10")
    '    lvPropertyItem.Group = lvCPU2Condition.Groups(1)
    '    Me.lvCPU2Condition.Items.Add(lvPropertyItem)

    '    lvPropertyItem = New ListViewItem
    '    lvPropertyItem.Text = "keyword"
    '    lvPropertyItem.SubItems.Add("has")
    '    lvPropertyItem.SubItems.Add("excitement")
    '    lvPropertyItem.Group = lvCPU2Condition.Groups(1)
    '    Me.lvCPU2Condition.Items.Add(lvPropertyItem)


    '    'LISTVIEW POSITION
    '    Me.lvCPU2Condition.Location = Me.lvSet.Location

    'End Sub
    Public Sub editbox_LostFocus(ByVal sender As Object, ByVal e As EventArgs)
        hitinfo.SubItem.Text = editbox.Text
        editbox.Hide()
    End Sub
    Private Sub all_propertyfields_DropDownClosed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call all_propertyfields_LostFocus(sender, Nothing)
    End Sub

    Public Sub all_propertyfields_LostFocus(ByVal sender As Object, ByVal e As EventArgs)

        If Not IsNothing(Me.all_mapping_point_stat_fields.SelectedItem) Then
            hitinfo.Item.Text = Me.all_mapping_point_stat_fields.SelectedItem.ToString
        End If

        Me.all_mapping_point_stat_fields.Hide()
        Me.getListviewFromSelTab(Me.tabsAction.SelectedTab).Focus()

    End Sub

    Public Sub New(ByVal _screen_location As Point, ByVal _logger As TriniDATTypeLogger, ByVal _close_handler As OnFormClosedthrd)
        InitializeComponent()

        Me.app_index = New BosswaveAppCache
        Me.gapp_new_text = "(new)"
        Me.gmapping_point_chainlevel_opt = "(global)"

        Me.event_logger = _logger
        Me.event_onclose = _close_handler
        Me.screen_location = _screen_location

        Me.lblNoData.BackColor = Me.lvJClass.BackColor


        'SET UP INLINE LISTVIEW EDITING
        '=================


        'LISTVIEW EDIT FIELDS - HELPER CONTROLS
        '====

        'COMBOBOX
        Me.all_mapping_point_stat_fields = New ComboBox
        Me.all_conditions_lb = New ComboBox
        Me.all_mapping_points = New ComboBox
        Me.custom_combo = New ComboBox

        'STYLES

        'TEXTBOX
        Me.editbox = New TextBox

        Me.all_mapping_point_stat_fields.Width = 200
        Me.all_mapping_points.Width = 200
        Call Me.setControlStyles(Me.all_mapping_points)
        Call Me.setControlStyles(Me.all_conditions_lb)
        Call Me.setControlStyles(Me.custom_combo)
        Call Me.setControlStyles(Me.editbox)

        AddHandler all_mapping_point_stat_fields.LostFocus, AddressOf Me.all_propertyfields_LostFocus
        AddHandler all_mapping_point_stat_fields.DropDownClosed, AddressOf Me.all_propertyfields_DropDownClosed

        AddHandler editbox.LostFocus, AddressOf Me.editbox_LostFocus
        AddHandler editbox.KeyPress, AddressOf Me.editbox_KeyPress

        AddHandler all_conditions_lb.LostFocus, AddressOf Me.all_conditions_lb_LostFocus
        AddHandler all_conditions_lb.DropDownClosed, AddressOf Me.all_conditions_lb_DropDownClosed

        AddHandler all_mapping_points.LostFocus, AddressOf Me.all_mappingpoint_selecter_lb_LostFocus
        AddHandler all_mapping_points.DropDownClosed, AddressOf Me.all_mappingpoint_selecter_lb_DropDownClosed

        AddHandler custom_combo.LostFocus, AddressOf Me.custom_combo_lb_LostFocus
        AddHandler custom_combo.DropDownClosed, AddressOf Me.custom_combo_DropDownClosed

        Me.tabsAction.Tag = "DP1"


        Me.tabsAction.Controls.Clear()

        Call Me.setControlStyles(editbox)
    End Sub

    Private Sub setControlStyles(ByVal ctrl As Object)

        If TypeOf ctrl Is ComboBox Then
            ctrl.BackColor = Color.Beige

            ctrl.FlatStyle = FlatStyle.Flat
            ctrl.DropDownStyle = ComboBoxStyle.DropDownList

            Exit Sub
        End If

        If TypeOf ctrl Is TextBox Then
            ctrl.BackColor = Color.Beige
            ctrl.BorderStyle = BorderStyle.FixedSingle
            ctrl.BackColor = Me.listMappingPoint.BackColor
            Exit Sub
        End If

    End Sub

    Private Sub cmdSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Close()
    End Sub

    Private Sub frmQueryEditor_FormClosed(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Me.event_onclose(Me.Location)


    End Sub

    Private Sub listMappingPoint_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles listMappingPoint.SelectedIndexChanged

        If listMappingPoint.SelectedIndex > 0 Then
            Call populateMappingPointDependencies()
        Else
            Me.lvJClass.Items.Clear()
            Me.lblNoData.Visible = False
        End If

    End Sub

    Private Sub lvJClass_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvJClass.SelectedIndexChanged

        If lvJClass.SelectedIndices.Count < 1 Then Exit Sub

        Dim selitem As ListViewItem

        Me.all_mapping_point_stat_fields.Items.Clear()

        selitem = Me.lvJClass.Items(Me.lvJClass.SelectedIndices(0))

        If Not Me.haveMappingPointStats() Or IsNothing(selitem) Then Exit Sub

        Dim selected_jclass As String

        selected_jclass = selitem.SubItems(1).Text

        If Not IsNothing(selected_jclass) Then

            Dim q = From statitems In Me.getMappingPointRootNode().Descendants(selected_jclass).Descendants()

            For Each statNode In q
                Dim lvItem As ListViewItem

                lvItem = New ListViewItem
                lvItem.Text = statNode.Name.ToString
                lvItem.SubItems.Add(HttpUtility.UrlDecode((statNode.Value.ToString)))

                If Me.all_mapping_point_stat_fields.FindStringExact(statNode.Name.ToString) = -1 Then
                    Me.all_mapping_point_stat_fields.Items.Add(statNode.Name.ToString)
                End If

            Next
        End If

    End Sub

    'DYNAMIC COMBOX EVENTS
    '===
    Public Sub all_conditions_lb_LostFocus(ByVal sender As Object, ByVal e As EventArgs)
        Me.all_conditions_lb.Hide()
        hitinfo.SubItem.Text = Me.all_conditions_lb.Text

        Dim sel_condition As BosswaveConditionComboItem
        sel_condition = Me.all_conditions_lb.SelectedItem


        Me.getListviewFromSelTab(Me.tabsAction.SelectedTab).Focus()

    End Sub
    Private Sub all_conditions_lb_DropDownClosed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call all_conditions_lb_LostFocus(sender, Nothing)
    End Sub
    Public Sub all_mappingpoint_selecter_lb_LostFocus(ByVal sender As Object, ByVal e As EventArgs)
        Me.all_mapping_points.Hide()
        hitinfo.SubItem.Text = Me.all_mapping_points.Text


        Me.getListviewFromSelTab(Me.tabsAction.SelectedTab).Focus()

    End Sub

    Private Sub custom_combo_DropDownClosed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call custom_combo_lb_LostFocus(sender, Nothing)
    End Sub
    Public Sub custom_combo_lb_LostFocus(ByVal sender As Object, ByVal e As EventArgs)
        Me.custom_combo.Hide()
        hitinfo.SubItem.Text = Me.custom_combo.Text

        Me.getListviewFromSelTab(Me.tabsAction.SelectedTab).Focus()

    End Sub
    Private Sub all_mappingpoint_selecter_lb_DropDownClosed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Call all_mappingpoint_selecter_lb_LostFocus(sender, Nothing)
    End Sub

    'DYNAMIC TB EVENTS
    '===
    Public Sub editbox_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            Dim ownerlistview As ListView
            editbox_LostFocus(sender, Nothing)
            e.Handled = True
            ownerlistview = getListviewFromSelTab(Me.tabsAction.SelectedTab)
            ownerlistview.Focus()
        End If
    End Sub

    Private Function getListviewFromSelTab(ByVal p As TabPage) As ListView

        For Each c In p.Controls

            If TypeOf c Is ListView Then
                Return c
            End If

        Next

        Return Nothing
    End Function
    Private Sub lvAction_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        Me.hitinfo = sender.HitTest(e.X, e.Y)
        If Not IsNothing(hitinfo.SubItem) Then
            Call ShowAction_EditBox(hitinfo.Item, hitinfo.SubItem)
        End If

    End Sub

    Public Function fillConditionalList(ByVal left_hand_itemtype As String, ByVal right_hand_itemtype As String) As Boolean

        'retval.DR
        Me.all_conditions_lb.Width = 200
        Me.all_conditions_lb.Items.Clear()

        Try
            Dim conditions_xml As XDocument

            conditions_xml = BosswaveApplication.getBuilderConditionListXML()

            Dim q = From condition_item In conditions_xml.Descendants("conditions").Descendants("condition") Where condition_item.@lefthand.ToString = left_hand_itemtype And condition_item.@righthand.ToString = right_hand_itemtype


            For Each conditionNode In q

                Dim condition_listbox_item As BosswaveConditionComboItem
                condition_listbox_item = New BosswaveConditionComboItem(conditionNode)

                Me.all_conditions_lb.Items.Add(condition_listbox_item)
            Next

            Return True

        Catch ex As Exception
            MsgBox("Error creating tabs: " & ex.Message)
            Return False
        End Try

    End Function
    'Private Function parseDynRightHandEditbox(ByVal righthand_attrib As String, ByVal selected_lvitem_tag As BosswaveConfigItem, ByVal LVItem As ListViewItem.ListViewSubItem) As Boolean
    '    Dim righthand_option() As String

    '    If Mid(righthand_attrib, 1, 8) = "$listof(" Then
    '        righthand_attrib = Mid(righthand_attrib, 9)
    '        righthand_attrib = Replace(righthand_attrib, "'", "")
    '        righthand_attrib = Replace(righthand_attrib, "(", "")
    '        righthand_attrib = Replace(righthand_attrib, ")", "")
    '        righthand_attrib = Replace(righthand_attrib, "$", "")
    '        righthand_option = Split(righthand_attrib, ",")

    '        'ADD OPTIONS FROM DIRECTIVE
    '        Me.custom_combo.Items.Clear()

    '        For Each opt In righthand_option
    '            Me.custom_combo.Items.Add(opt)
    '        Next

    '        Me.custom_combo.Parent = selected_lvitem_tag.Parent
    '        Me.custom_combo.Bounds = LVItem.Bounds
    '        Me.custom_combo.Focus()
    '        Me.custom_combo.Show()

    '        Me.DropDownConditionList(Me.custom_combo)
    '        Return True
    '    End If 'listof

    '    Return False
    'End Function

    'Private Function parseInstructionEditbox(ByVal instruction_attrib As String, ByVal selected_lvitem_tag As BosswaveConfigItem, ByVal LVItem As ListViewItem.ListViewSubItem) As Boolean
    '    If Mid(instruction_attrib, 1) = "$" Then
    '        '$MP
    '        If instruction_attrib = "$MP" Then
    '            'DROPDOWN - MAPPING POINTS
    '            Me.all_mapping_points.Parent = selected_lvitem_tag.Parent
    '            Me.all_mapping_points.Bounds = LVItem.Bounds
    '            Me.all_mapping_points.Focus()
    '            Me.all_mapping_points.Show()

    '            Me.DropDownConditionList(Me.all_mapping_points)
    '            Return True
    '        End If
    '    End If

    '    Return False
    'End Function
    Public Sub ShowAction_EditBox(ByVal parent As ListViewItem, ByVal LVItem As ListViewItem.ListViewSubItem)


        'Dim selected_lvitem_tag As Object 'BosswaveConfigItem
        'Dim isrootItem As Boolean

        'If IsNothing(LVItem.Tag) Then
        '    'root item
        '    selected_lvitem_tag = getConfigElementByListviewItem(parent)
        '    isrootItem = True

        'Else
        '    'subitem is selected
        '    selected_lvitem_tag = getConfigElementByListviewItem(LVItem.Tag)
        '    isrootItem = False
        'End If

        'If IsNothing(selected_lvitem_tag) Then
        '    MsgBox("Unknown selection.")
        '    Exit Sub
        'End If

        ''abstract 
        ''======

        ''inspect parameter kind
        'If selected_lvitem_tag.haveConfigNode() Then
        '    Dim instruction_attrib As String
        '    Dim righthand_attrib As String

        '    instruction_attrib = selected_lvitem_tag.Node.@instruction
        '    righthand_attrib = selected_lvitem_tag.Node.@righthand

        '    If Not IsNothing(instruction_attrib) Then
        '        '  If parseInstructionEditbox(instruction_attrib, selected_lvitem_tag, LVItem) Then Exit Sub
        '    End If

        '    If Not IsNothing(righthand_attrib) Then
        '        ' If parseDynRightHandEditbox(righthand_attrib, selected_lvitem_tag, LVItem) Then Exit Sub
        '    End If

        'End If 'custom ConfigNode

        'If selected_lvitem_tag.ItemType = BosswaveAppEngineField.TriggerConditionPropertyName Then
        '    'list is already filled by JClass listview events.
        '    Me.all_mapping_point_stat_fields.Parent = selected_lvitem_tag.Parent
        '    Me.all_mapping_point_stat_fields.Bounds = LVItem.Bounds
        '    Me.all_mapping_point_stat_fields.Focus()
        '    Me.all_mapping_point_stat_fields.Show()

        '    DropDownConditionList(Me.all_mapping_point_stat_fields)
        '    Exit Sub
        'ElseIf selected_lvitem_tag.ItemType = BosswaveAppEngineField.TriggerConditionCondition Then
        '    'get conditional list
        '    If Me.fillConditionalList("property", "text") Then
        '        Me.all_conditions_lb.Parent = selected_lvitem_tag.Parent
        '        Me.all_conditions_lb.Bounds = LVItem.Bounds
        '        Me.all_conditions_lb.SelectedIndex = 0
        '        Me.all_conditions_lb.Focus()
        '        Me.all_conditions_lb.Show()

        '        Me.DropDownConditionList(Me.all_conditions_lb)

        '    End If
        '    Exit Sub
        'ElseIf selected_lvitem_tag.ItemType = BosswaveAppEngineField.ParameterValue Then



        'ElseIf selected_lvitem_tag.ItemType = BosswaveAppEngineField.TriggerConditionPropertyValue Then
        '    'other types
        '    Me.editbox.Bounds = LVItem.Bounds
        '    Me.editbox.Text = LVItem.Text
        '    Me.editbox.Show()
        '    Me.editbox.Focus()
        'End If

    End Sub
    'Private Function getConfigElementByListviewItem(ByVal lvitem As ListViewItem) As BosswaveConfigItem
    '    Return lvitem.Tag
    'End Function
    'Private Function getConfigElementByListviewItem(ByVal lvitem As ListViewItem.ListViewSubItem) As BosswaveConfigItem
    '    Return lvitem.Tag
    'End Function
 
    Private Sub lvValues_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        'If lvValues.SelectedIndices.Count < 1 Then Exit Sub
        'Dim lvItem As ListViewItem
        'Dim sel_item As ListViewItem

        'sel_item = Me.getSelectedItem(lvValues)

        'If Not IsNothing(sel_item) Then
        '    lvItem = New ListViewItem
        '    lvItem.Group = lvSet.Groups(2)
        '    lvItem.Text = sel_item.Text
        '    lvItem.SubItems.Add("")
        '    lvItem.SubItems.Add(sel_item.SubItems(1).Text)
        '    Me.lvSet.Items.Add(lvItem)
        'End If

    End Sub

    'Private Sub lvAction_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    '    Call editbox.Hide()


    '    Dim kpe As KeyPressEventArgs

    '    kpe = New KeyPressEventArgs(Microsoft.VisualBasic.ChrW(Keys.Return))

    '    Call lvAction_KeyPress(sender, kpe)

    'End Sub

    Private Sub lvAction_ColumnClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs)
        Call editbox.Hide()
    End Sub

    Private Sub lvAction_BeforeLabelEdit(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LabelEditEventArgs)
        e.CancelEdit = True

        Dim mouse_event As MouseEventArgs
        mouse_event = New MouseEventArgs(Nothing, 1, Cursor.Position.X, Cursor.Position.Y, 0)
        Call lvAction_MouseDoubleClick(sender, mouse_event)

    End Sub

    Private Sub lvAction_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs)

        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            ' e.CancelEdit = True
            e.Handled = True
            If sender.SelectedIndices.Count > 0 Then
                Dim selitem As ListViewItem

                selitem = sender.Items(sender.SelectedIndices(0))
                Dim mouse_event As MouseEventArgs
                mouse_event = New MouseEventArgs(Nothing, 1, selitem.Bounds.X, selitem.Bounds.Y, 0)
                Call lvAction_MouseDoubleClick(sender, mouse_event)
            End If

        End If

    End Sub


    Private Sub mnuInsert_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuInsert.Click

        Dim srcLV As ListView
        Dim newitem As ListViewItem
        Dim sel_item As ListViewItem


        srcLV = mnuMutations.Tag

        If Not IsNothing(srcLV) Then
            sel_item = Me.getSelectedItem(srcLV)

            newitem = New ListViewItem
            newitem.Text = "new property"
            newitem.SubItems.Add("")
            newitem.SubItems.Add("")
            newitem.SubItems.Add("")

            If Not IsNothing(sel_item) Then
                newitem.Group = sel_item.Group
            Else
                'append last group
                newitem.Group = srcLV.Groups(srcLV.Groups.Count - 1)
            End If

            srcLV.Items.Add(newitem)
            Exit Sub
        End If
    End Sub


    Private Sub Datasets_MouseHover(ByVal sender As System.Object, ByVal e As System.EventArgs)
        mnuMutations.Tag = sender
    End Sub

    Private Sub mnuDelete_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuDelete.Click

        Dim srcLV As ListView
        Dim sel_item As ListViewItem

        srcLV = mnuMutations.Tag

        If Not IsNothing(srcLV) Then
            sel_item = Me.getSelectedItem(srcLV)
            If Not IsNothing(sel_item) Then
                Call srcLV.Items.Remove(sel_item)
            End If
        End If



    End Sub

    Private Sub lblMinimizeWindow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblMinimizeWindow.Click
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Private Sub lblCloseWindow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lblCloseWindow.Click
        Me.Close()
    End Sub
    Private Sub WindowBar_MouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles windowBar.MouseDown, lblWindowTitle.MouseDown

        If e.Button = MouseButtons.Left Then
            Me.dragging_tags = True
            Me.drag_startpos = PointToClient(Cursor.Position)
        Else
            Me.dragging_tags = False
        End If

    End Sub

    Private Sub WindowBar_MouseUp(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles windowBar.MouseUp, lblWindowTitle.MouseUp
        Me.dragging_tags = False
    End Sub

    Private Sub WindowBar_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles windowBar.MouseMove, lblWindowTitle.MouseMove
        If Me.dragging_tags = True And e.Button = MouseButtons.Left Then
            Me.Location = New Point(Cursor.Position.X - Me.drag_startpos.X, Cursor.Position.Y - Me.drag_startpos.Y)
        End If

    End Sub

    Private Sub WindowBar_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles windowBar.MouseLeave
        If Me.dragging_tags = True Then
            Me.Location = New Point(Cursor.Position.X - Me.drag_startpos.X, Cursor.Position.Y - Me.drag_startpos.Y)
        End If
    End Sub

    Private Sub lvAction_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)


    End Sub

    Private Sub cmdSave_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdSave.Click

    End Sub

    Private Sub tabsAction_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
    Private Sub lvAction_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvJClass.Enter
        Me.editbox.Parent = sender
        Me.editbox.Hide()
    End Sub
    Private Sub lvJClass_Enter(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lvJClass.Enter
        '    Me.editbox.Parent = sender
    End Sub


    Private Sub listMappingPoint_DropDownClosed(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub lvFunction_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub lstProgram_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstProgram.SelectedIndexChanged

        If lstProgram.Text <> gapp_new_text Then
            Dim applicationIndex As Integer
            applicationIndex = lstProgram.SelectedIndex

            If Not GlobalObject.ApplicationCache.Item(applicationIndex).IsInitialized Then
                GlobalObject.ApplicationCache.Item(applicationIndex).Load()
            End If

            Call populateMappingPoints(Me.listMappingPoint, gmapping_point_chainlevel_opt)

            'mark selected
            Dim x As Integer
            Dim functionIndex As Integer
            Dim mappingpointfound As Boolean

            mappingpointfound = False

            For x = 0 To Me.listMappingPoint.Items.Count - 1



                For functionIndex = 0 To GlobalObject.ApplicationCache.Item(applicationIndex).ApplicationActions.Count - 1

                    If GlobalObject.ApplicationCache.Item(applicationIndex).ApplicationActions.Item(functionIndex).URL = Me.listMappingPoint.Items(x).ToString Then
                        If Not mappingpointfound Then
                            'start
                            Me.listMappingPoint.Items(x) = Me.listMappingPoint.Items(x).ToString & " ->"
                        End If

                        Me.listMappingPoint.Items(x) = Me.listMappingPoint.Items(x).ToString & " " & GlobalObject.ApplicationCache.Item(applicationIndex).ApplicationActions.Item(functionIndex).JClassName & "." & GlobalObject.ApplicationCache.Item(applicationIndex).ApplicationActions.Item(functionIndex).Id & ","
                        mappingpointfound = True

                    End If

                Next

                If mappingpointfound Then
                    Me.listMappingPoint.Items(x) = Mid(Me.listMappingPoint.Items(x).ToString, 1, Len(Me.listMappingPoint.Items(x).ToString) - 1)
                    mappingpointfound = False
                End If

            Next

        End If

    End Sub
End Class

