Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRouterPrefs

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

    Private Sub cmdOKClick(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        RacePrefs.Save(_ID, _Context)
        Hide()
    End Sub

    Private Sub cmdCancelClick(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        _Context = RacePrefs.GetRaceInfo(_ID)
        Hide()
    End Sub

End Class
