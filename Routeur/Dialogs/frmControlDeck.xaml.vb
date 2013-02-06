Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmControlDeck
	Public Sub New()
		MyBase.New()

		Me.InitializeComponent()

		' Insérez le code requis pour la création d’objet sous ce point.
	End Sub


    Private Sub OnDeckClosing(sender As System.Object, e As System.ComponentModel.CancelEventArgs)
        My.Settings.Save()
    End Sub

End Class
