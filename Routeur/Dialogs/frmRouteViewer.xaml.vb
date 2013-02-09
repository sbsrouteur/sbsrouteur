'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

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

    Private _Model As RouteurModel
    Private WithEvents _RouteViewModel As RouteViewModel

    Public Event RequestRouteReload()

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
        _RouteViewModel = Model.GetPilototoRoute
        DataContext = _RouteViewModel
        _Model = Model
    End Sub

    'Private Sub RoutePointDelete(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
    '    'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
    '    Dim RoutePoint As RoutePointView

    '    If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RoutePointView Then
    '        RoutePoint = CType(CType(sender, Button).DataContext, RoutePointView)
    '        RoutePoint.Delete()
    '    End If
    'End Sub

    'Private Sub RoutePointUpdate(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
    '    'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
    '    Dim RoutePoint As RoutePointView

    '    If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RoutePointView Then
    '        RoutePoint = CType(CType(sender, Button).DataContext, RoutePointView)
    '        RoutePoint.Update()
    '    End If
    'End Sub

    Private Sub AddNewPoint(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)

        Dim Route As RouteViewModel

        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RouteViewModel Then
            Route = CType(CType(sender, Button).DataContext, RouteViewModel)
            Route.AddPoint()
        End If
    End Sub

    Public Sub RefreshRoute()
        _RouteViewModel = _Model.GetPilototoRoute
        DataContext = _RouteViewModel
    End Sub

    Private Sub _RouteViewModel_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _RouteViewModel.PropertyChanged

        If e.PropertyName = "UPLOAD" Then
            RaiseEvent RequestRouteReload()
        End If
    End Sub

   

End Class
