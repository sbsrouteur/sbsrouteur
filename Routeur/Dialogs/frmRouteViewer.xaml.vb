Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRouteViewer
	Public Sub New()
		MyBase.New()

		Me.InitializeComponent()

		' Insérez le code requis pour la création d’objet sous ce point.
	End Sub

	Private Sub cmdCloseClick(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
		'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        Close()
    End Sub

    Public Sub New(ByVal Context As RouteViewModel)

        Me.new()
        DataContext = Context

    End Sub

End Class
