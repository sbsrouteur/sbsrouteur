Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmAppOptions
	Public Sub New()
		MyBase.New()

		Me.InitializeComponent()

		' Insérez le code requis pour la création d’objet sous ce point.
	End Sub

    Private Sub DlgClose(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        If sender Is Me.cmdOK Then
            'TODO Conf
        End If

    End Sub
End Class
