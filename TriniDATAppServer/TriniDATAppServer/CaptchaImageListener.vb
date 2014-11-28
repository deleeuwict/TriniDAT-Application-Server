Option Compare Text
Option Explicit On

Imports System.Threading
Imports System.IO
Public Class CaptchaImageListener

    Private captcha_fs_listener As FileSystemWatcher
    Private JCaptchaDrop As Object
    Private JCaptchaDrop_mapping_point As mappingPointRoot

    Public Sub StartListening()
        If Not InstallCaptchaListener() Then
            Try
                Thread.CurrentThread.Abort()
            Catch ex As Exception

            End Try
        End If
    End Sub
    Private Function InstallCaptchaListener() As Boolean

        'need mapping point to obtain the DLL filepath for Ticket Master class.
        'JCaptchaDrop_mapping_point = BosswaveApplicationHost.getbyClassName("JCaptchaDrop")

        If IsNothing(JCaptchaDrop_mapping_point) Then
            MsgBox("unable to load class JCaptchaDrop")
            Return False
        End If

        'JCaptchaDrop = JServiceLauncher.createJService(.getExternalType("JCaptchaDrop", JCaptchaDrop_mapping_point.getDependencyPaths()))

        If IsNothing(JCaptchaDrop) Then
            MsgBox("unable to load class JCaptchaDrop")
            Return False
        End If


        captcha_fs_listener = New FileSystemWatcher(JCaptchaDrop.CAPTCHA_DROP_PATH)
        AddHandler captcha_fs_listener.Created, AddressOf OnIncomingCaptchaFile

        With captcha_fs_listener
            '    .WaitForChanged(WatcherChangeTypes.Created)
            .Filter = "*.*"
            .NotifyFilter = (NotifyFilters.LastAccess Or _
                                    NotifyFilters.LastWrite Or _
                                    NotifyFilters.FileName Or _
                                    NotifyFilters.DirectoryName)
            .IncludeSubdirectories = True
            .EnableRaisingEvents = True
        End With
        Return True

    End Function
    Private Sub OnIncomingCaptchaFile(ByVal source As Object, ByVal e As FileSystemEventArgs)

        Dim thrd As Thread
        thrd = New Thread(AddressOf CaptchaformThread.CreateForm)
        thrd.SetApartmentState(System.Threading.ApartmentState.STA)
        thrd.Start(e.FullPath)
        '        captcha_dialog.Show()
        '        captcha_dialog.setCaptchaImage(e.FullPath)

    End Sub
End Class

Public Class CaptchaformThread

    Public Shared Sub CreateForm(ByVal image_url As String)
        Dim captcha_dialog As frmCaptchaInput
        captcha_dialog = New frmCaptchaInput(image_url)

        System.Windows.Forms.Application.Run(captcha_dialog)

    End Sub

End Class