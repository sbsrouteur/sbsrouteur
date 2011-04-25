Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRaceSelection
	Public Sub New()
		MyBase.New()

		Me.InitializeComponent()

		' Insérez le code requis pour la création d’objet sous ce point.
	End Sub

    Private Sub OnCloseRequest(sender As System.Object, e As System.Windows.RoutedEventArgs)

        DialogResult = False
        Hide()

    End Sub

    Private Sub frmRaceSelection_Loaded(sender As Object, e As System.Windows.RoutedEventArgs) Handles Me.Loaded
        LoadRaceList()
    End Sub

    Private Sub LoadRaceList()

        ' Dim Races = GetRaceList()

    End Sub

End Class
