Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRouterConfiguration

    Private _Context As RacePrefs
    Private _ID As Integer

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Sub New(ByVal ID As Integer)

        Me.New()
        _ID = ID
        _Context = RacePrefs.GetRaceInfo(_ID)
        DataContext = _Context

    End Sub

    Private Sub cmdOKClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        RacePrefs.Save(_ID, _Context)
        DialogResult = True
        Hide()

    End Sub

    Private Sub cmdCancelClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        _Context = RacePrefs.GetRaceInfo(_ID)
        DialogResult = False
        Hide()

    End Sub
End Class
