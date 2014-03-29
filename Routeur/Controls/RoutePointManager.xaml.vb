' This file is part of Routeur.
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
'along with Foobar.  If not, see<http://www.gnu.org/licenses/> .

Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes

Partial Public Class RoutePointManager



    Public Property ActiveRoute As RecordedRoute
        Get
            Return CType(GetValue(ActiveRouteProperty), RecordedRoute)
        End Get

        Set(ByVal value As RecordedRoute)
            SetValue(ActiveRouteProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ActiveRouteProperty As DependencyProperty = _
                           DependencyProperty.Register("ActiveRoute", _
                           GetType(RecordedRoute), GetType(RoutePointManager), _
                           New PropertyMetadata(Nothing))




    Public Property ActiveRoutePoints As IList
        Get
            Return CType(GetValue(ActiveRoutePointProperty), IList)
        End Get

        Set(ByVal value As IList)
            SetValue(ActiveRoutePointProperty, value)
        End Set
    End Property

    Public Shared ReadOnly ActiveRoutePointProperty As DependencyProperty = _
                           DependencyProperty.Register("ActiveRoutePoints", _
                           GetType(IList), GetType(RoutePointManager), _
                           New PropertyMetadata(Nothing))




    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub DeleteRoutePointFromRoute(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        If ActiveRoutePoints IsNot Nothing Then

            If ActiveRoutePoints.Count = 1 AndAlso MessageBox.Show("Are you sure you want to delete point from route?", "Delete point from route", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
                ActiveRoute.Route.Remove(CType(ActiveRoutePoints(0), RoutePointView))
                ActiveRoute.RecomputeRoute(Nothing, ActiveRoute.Model.VorHandler.Meteo, ActiveRoute.Model.VorHandler.UserInfo.POL, VLM_Router.Sails)
            ElseIf ActiveRoutePoints.Count > 1 AndAlso MessageBox.Show("Are you sure you want to delete point from route?", "Delete point from route", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
                Dim DeleteList As New List(Of RoutePointView)()

                For Each Pt As RoutePointView In ActiveRoutePoints
                    DeleteList.Add(Pt)
                Next

                For Each pt As RoutePointView In DeleteList
                    ActiveRoute.Route.Remove(pt)
                Next
            End If
        End If

    End Sub

    Private Sub RouteToNextRoutePoint(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l'implémentation du gestionnaire d'événements.
    End Sub

    Private Sub RouteToActiveRaceWP(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l'implémentation du gestionnaire d'événements.
    End Sub

    Private Sub ClearPointsHistory(sender As Object, e As RoutedEventArgs)
        If ActiveRoute IsNot Nothing AndAlso MessageBox.Show("Are you sure you want to clear all points in route before now?", "Delete point from route", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            ActiveRoute.ClearPastPoints()
        End If
    End Sub
End Class
