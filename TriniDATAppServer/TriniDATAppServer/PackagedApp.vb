Imports TriniDATDictionaries
Imports System.IO


Public Class PackagedApp

    Private title As String
    Private description_general As String
    Private description_requirement As String
    Private local_app As BosswaveApplication
    Private file As String
    Private icon_file As String
    Private docs_file As String
    Private license_file As String
    Private setup_file As String
    Private sourcecode_file As String
    Private app_file_root As PackagedApp_SubCat
    Private app_file_direct_category As PackagedApp_SubCat
    Private price As Double
    Private currencyid As String
    Private paymentid As String
    Private secret_id As String 'hash list of all dep files.

    Public Sub New()
        Me.local_app = Nothing
        Me.icon_file = Nothing
    End Sub

    Public Function genDependencyHashList() As Boolean

        If Not Me.haveApp Then Return False

        Me.MD5Secret = ""

        If Not Me.SellApp.haveMappingPoints Then
            Return True
        End If

        Dim processed_md5s As TriniDATWordDictionary

        processed_md5s = New TriniDATWordDictionary("", Nothing)



        For Each mp_desc In Me.SellApp.ApplicationMappingPoints

            'should always be valid.
            If mp_desc.haveMappingPointInstance Then

                If mp_desc.MappingPoint.HaveDependencyList Then

                    For Each dep_path In mp_desc.MappingPoint.getDependencyPaths(True)
                        Dim md5 As String

                        Try
                            md5 = GlobalObject.getMD5CheckSum(New FileInfo(dep_path), True)

                            If IsNothing(md5) Then
                                Throw New Exception("Unable to read file.")
                            Else
                                If Not processed_md5s.Has(md5) Then
                                    Me.MD5Secret &= md5 & ";"
                                    processed_md5s.Add(md5)
                                End If
                            End If

                        Catch ex As Exception
                            Return False
                        End Try
                    Next
                End If
            End If
        Next

        Return True
    End Function
    Public ReadOnly Property haveLicenseFile As Boolean
        Get
            If Not IsNothing(Me.license_file) Then
                Return IO.File.Exists(Me.LicenseFile)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveSourceCodeFile As Boolean
        Get
            If Not IsNothing(Me.sourcecode_file) Then
                Return IO.File.Exists(Me.SourceCodeFile)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveInstallerFile As Boolean
        Get
            If Not IsNothing(Me.setup_file) Then
                Return IO.File.Exists(Me.InstallerFile)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveDocumentationFile As Boolean
        Get
            If Not IsNothing(Me.docs_file) Then
                Return IO.File.Exists(Me.DocumentationFile)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveIconFile As Boolean
        Get
            If Not IsNothing(Me.icon_file) Then
                Return IO.File.Exists(Me.IconFile)
            Else
                Return False
            End If
        End Get
    End Property
    Public ReadOnly Property haveApp As Boolean
        Get
            Return Not IsNothing(Me.local_app)
        End Get
    End Property
    Public Property LicenseFile As String
        Get
            Return Me.license_file
        End Get
        Set(ByVal value As String)
            Me.license_file = value
        End Set
    End Property
    Public Property InstallerFile As String
        Get
            Return Me.setup_file
        End Get
        Set(ByVal value As String)
            Me.setup_file = value
        End Set
    End Property
    Public Property DocumentationFile As String
        Get
            Return Me.docs_file
        End Get
        Set(ByVal value As String)
            Me.docs_file = value
        End Set
    End Property
    Public Property SourceCodeFile As String
        Get
            Return Me.sourcecode_file
        End Get
        Set(ByVal value As String)
            Me.sourcecode_file = value
        End Set
    End Property
    Public Property IconFile As String
        Get
            Return Me.icon_file
        End Get
        Set(ByVal value As String)
            Me.icon_file = value
        End Set
    End Property



    Public ReadOnly Property LicenseFile_Info As FileInfo
        Get
            Return New FileInfo(Me.license_file)
        End Get
    End Property
    Public ReadOnly Property InstallerFile_Info As FileInfo
        Get
            Return New FileInfo(Me.setup_file)
        End Get
    End Property
    Public ReadOnly Property DocumentationFile_Info As FileInfo
        Get
            Return New FileInfo(Me.docs_file)
        End Get
    End Property
    Public ReadOnly Property SourceCodeFile_Info As FileInfo
        Get
            Return New FileInfo(Me.sourcecode_file)
        End Get
    End Property
    Public ReadOnly Property IconFile_Info As FileInfo
        Get
            Return New FileInfo(Me.IconFile)
        End Get
    End Property

    Public Property MD5Secret As String
        Get
            Return Me.secret_id
        End Get
        Set(ByVal value As String)
            Me.secret_id = value
        End Set
    End Property
    Public Property PaymentHandle As String
        Get
            Return Me.paymentid
        End Get
        Set(ByVal value As String)
            Me.paymentid = value
        End Set
    End Property
    Public Property SellPrice As Double
        Get
            Return Me.price
        End Get
        Set(ByVal value As Double)
            Me.price = value
        End Set
    End Property
    Public Property SellCurrencyID As String
        Get
            Return Me.currencyid
        End Get
        Set(ByVal value As String)
            Me.currencyid = value
        End Set
    End Property
    Public Property SellApp As BosswaveApplication
        Get
            Return Me.local_app
        End Get
        Set(ByVal value As BosswaveApplication)
            Me.local_app = value
        End Set
    End Property

End Class

Public Class PackagedApp_CategoryDescriptions

    Public Const ROOTMAX = 100

    Public Shared Function getRootName(ByVal offsetid As Integer) As String

        Select Case offsetid

            Case 0
                Return "General"

            Case 10
                Return "Reserved"

            Case 20
                Return "Social Media"

            Case 30
                Return "Data Mining"

            Case 40
                Return "Government Related"

            Case 50
                Return "Security Related"

            Case 60
                Return "Educational"

            Case 70
                Return "Webservice Tool"

            Case 80
                Return "Visual Data"
            Case Else
                Return Nothing

        End Select

    End Function

    Public Shared Function getSubCatNamesByRange(ByVal offset As Integer) As TriniDATDictionaries.TriniDATWordDictionary
        'offset = 1 .. 10 ..20 

        Dim x As Integer
        Dim retval As TriniDATWordDictionary

        retval = New TriniDATWordDictionary("", Nothing)

        For x = offset To offset + 10
            Dim subcat_name As String
            subcat_name = PackagedApp_CategoryDescriptions.getSubName(x)

            If IsNothing(subcat_name) Then Exit For

            retval.Add(subcat_name)

        Next

        Return retval

    End Function

    Public Shared Function getSubName(ByVal subcat_id As PackagedApp_SubCat) As String

        Select Case subcat_id

            Case PackagedApp_SubCat.PACKAGED_APP_GENERAL
                Return "General Application"

            Case PackagedApp_SubCat.PACKAGED_APP_RESERVED
                Return "Reserved"

            Case PackagedApp_SubCat.PACKAGED_APP_OTHER
                Return "Other"

            Case PackagedApp_SubCat.PACKAGED_APP_MAILINGLIST_TOOLS
                Return "Mailing List Processor"


            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_NETWORKS
                Return "Social Networks - General"

            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_NETWORKS_POSTER
                Return "Social Networks - Posters"

            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_NETWORKS_DATAHARVESTER
                Return "Social Networks - Data Harvester"

            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_MEDIA_BLOG_SCANNER
                Return "Social Media - Blog Scanner"

            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_MEDIA_BLOG_POSTER
                Return "Social Media - Blog Poster"

            Case PackagedApp_SubCat.PACKAGED_APP_SOCIAL_MEDIA_GRAPHICSRELATED
                Return "Social Media - Image Processor"

            Case PackagedApp_SubCat.PACKAGED_APP_DATAMINING_BOT
                Return "Data Mining - Bot"

            Case PackagedApp_SubCat.PACKAGED_APP_DATAMINING_EMAILHARVESTER
                Return "Data Mining - Email"

            Case PackagedApp_SubCat.PACKAGED_APP_DATAMINING_CONTENTRIPPER
                Return "Data Mining - Content Parser"


            Case PackagedApp_SubCat.PACKAGED_APP_DATAMINING_CONTENTSPINNER
                Return "Data Mining - Content Spinner"


            Case PackagedApp_SubCat.PACKAGED_APP_DATAMINING_OTHER
                Return "Data Mining - Content - Other"

            Case PackagedApp_SubCat.PACKAGED_APP_GOVERNMENTRELATED_GENERAL
                Return "Government Related"

            Case PackagedApp_SubCat.PACKAGED_APP_GOVERNMENTRELATED_MILITARY
                Return "Government - Military"

            Case PackagedApp_SubCat.PACKAGED_APP_GOVERNMENTRELATED_BUSINESSINTELLIGENCE
                Return "Government - Intelligence"

            Case PackagedApp_SubCat.PACKAGED_APP_GOVERNMENTRELATED_DATAOPERATIONS
                Return "Government - Data Op"

            Case PackagedApp_SubCat.PACKAGED_APP_GOVERNMENTRELATED_LAWENFORCEMENT_RELATED
                Return "Government - Law Enforcement"

            Case PackagedApp_SubCat.PACKAGED_APP_ITSECURITY_GENERAL
                Return "Computer Security"

            Case PackagedApp_SubCat.PACKAGED_APP_EDUCATIONAL_DEMOAPP
                Return "Educative Software"

            Case PackagedApp_SubCat.PACKAGED_APP_EDUCATIONAL_VISUALSTATS
                Return "Educative Software - Statistical"

            Case PackagedApp_SubCat.PACKAGED_APP_WEBSERVICE_CMSTOOL
                Return "CMS Portal"

            Case PackagedApp_SubCat.PACKAGED_APP_WEBSERVICE_SMSTOOL
                Return "SMS Service"

            Case PackagedApp_SubCat.PACKAGED_APP_WEBSERVICE_SEOTOOL
                Return "SEO Service"

            Case PackagedApp_SubCat.PACKAGED_APP_WEBSERVICE_MASSPOSTER
                Return "Mass Posting Service"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_GENERAL
                Return "Visual Data Interface"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_INTRANET
                Return "Intranet"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_DESKTOP
                Return "Desktop"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_CORPORATE
                Return "Corporate"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_UNIVERSITY
                Return "University"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_GOVERNMENT
                Return "Government"

            Case PackagedApp_SubCat.PACKAGED_APP_INTERFACETOOL_STATS
                Return "Statistics"

            Case Else
                Return Nothing
        End Select

    End Function

End Class
Public Enum PackagedApp_SubCat
    PACKAGED_APP_GENERAL = 0
    PACKAGED_APP_MAILINGLIST_TOOLS = 1

    PACKAGED_APP_RESERVED = 10

    PACKAGED_APP_SOCIAL_NETWORKS = 20
    PACKAGED_APP_SOCIAL_NETWORKS_POSTER = 21
    PACKAGED_APP_SOCIAL_NETWORKS_DATAHARVESTER = 22
    PACKAGED_APP_SOCIAL_MEDIA_BLOG_SCANNER = 23
    PACKAGED_APP_SOCIAL_MEDIA_BLOG_POSTER = 24
    PACKAGED_APP_SOCIAL_MEDIA_GRAPHICSRELATED = 25

    PACKAGED_APP_DATAMINING_BOT = 30
    PACKAGED_APP_DATAMINING_EMAILHARVESTER = 31
    PACKAGED_APP_DATAMINING_CONTENTRIPPER = 32
    PACKAGED_APP_DATAMINING_CONTENTSPINNER = 33
    PACKAGED_APP_DATAMINING_OTHER = 34

    PACKAGED_APP_GOVERNMENTRELATED_GENERAL = 40
    PACKAGED_APP_GOVERNMENTRELATED_MILITARY = 41
    PACKAGED_APP_GOVERNMENTRELATED_BUSINESSINTELLIGENCE = 42
    PACKAGED_APP_GOVERNMENTRELATED_DATAOPERATIONS = 43
    PACKAGED_APP_GOVERNMENTRELATED_LAWENFORCEMENT_RELATED = 44

    PACKAGED_APP_ITSECURITY_GENERAL = 50

    PACKAGED_APP_EDUCATIONAL_DEMOAPP = 60
    PACKAGED_APP_EDUCATIONAL_VISUALSTATS = 61

    PACKAGED_APP_WEBSERVICE_CMSTOOL = 70
    PACKAGED_APP_WEBSERVICE_SMSTOOL = 71
    PACKAGED_APP_WEBSERVICE_SEOTOOL = 72
    PACKAGED_APP_WEBSERVICE_MASSPOSTER = 73

    'PACKAGED_APP_CMS_GENERAL = 90
    'PACKAGED_APP_CMS_PORTAL = 91

    PACKAGED_APP_INTERFACETOOL_GENERAL = 80
    PACKAGED_APP_INTERFACETOOL_INTRANET = 81
    PACKAGED_APP_INTERFACETOOL_DESKTOP = 82
    PACKAGED_APP_INTERFACETOOL_CORPORATE = 83
    PACKAGED_APP_INTERFACETOOL_UNIVERSITY = 84
    PACKAGED_APP_INTERFACETOOL_GOVERNMENT = 85
    PACKAGED_APP_INTERFACETOOL_STATS = 86

    PACKAGED_APP_OTHER = 1000

End Enum