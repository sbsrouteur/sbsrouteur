Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRoutesManager
	Public Sub New()
		MyBase.New()

		Me.InitializeComponent()

		' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Sub ShowForm(ByVal Mgr As RouteManager, ByVal Parent As Window)
        DataContext = Mgr
        Owner = Parent
        Show()
    End Sub

    Private Sub OnClose(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Close()
    End Sub
End Class
