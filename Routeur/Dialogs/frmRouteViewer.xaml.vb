﻿Imports System
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation

Partial Public Class frmRouteViewer

    Private _Model As RouteurModel

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub cmdCloseClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        Close()
    End Sub

    Public Sub New(ByVal Model As RouteurModel)

        Me.new()
        DataContext = Model.GetPilototoRoute
        _Model = Model
    End Sub

    Private Sub RoutePointDelete(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        Dim RoutePoint As RoutePointViewBase

        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RoutePointViewBase Then
            RoutePoint = CType(CType(sender, Button).DataContext, RoutePointViewBase)
            RoutePoint.Delete()
        End If
    End Sub

    Private Sub RoutePointUpdate(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
    End Sub

End Class
