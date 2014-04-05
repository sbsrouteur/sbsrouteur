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

Partial Public Class frmAutoPilotViewer

    Private _Model As RouteurModel
    Private WithEvents _RouteViewModel As RouteViewModel
    Private _MouseCaptured As Boolean = False
    Private _UpdatePoint As RoutePointView
    Private _VacSecs As Integer = 900

    Public Enum CaptureInfoRequest As Integer

        None = 0
        RouteDate = 1
        BearingToPoint = 2
        AngleToPoint = 3

    End Enum


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

    Public Sub New(ByVal Model As RouteurModel, VacMins As Integer)

        Me.New()
        _RouteViewModel = Model.GetPilototoRoute
        DataContext = _RouteViewModel
        _Model = Model
        _VacSecs = VacMins * 60

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

    Private Sub AddNewPoint(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

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

    Private Sub StartDateDrag(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        If TypeOf (CType(sender, Image).DataContext) Is RoutePointView Then

            Debug.WriteLine(Now & " captured")
            _MouseCaptured = CType(sender, UIElement).CaptureMouse()

            _UpdatePoint = CType(CType(sender, Image).DataContext, RoutePointView)
        End If
    End Sub

    Private Sub EndDateDrag(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        Debug.WriteLine(Now & " Released")
        _MouseCaptured = False
        ReleaseMouseCapture()

    End Sub

    Private Sub MouseMoveHandler(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        'TODO: Add event handler implementation here.
        If _MouseCaptured Then
            Dim P As Point = e.GetPosition(_Model.The2DViewer)
            Debug.WriteLine(Now & " moving " & P.ToString)
            _Model.HandleCapture(Me, CaptureInfoRequest.RouteDate, P)
        End If
    End Sub

    Sub StopCapture()
        _MouseCaptured = False
    End Sub

    Sub NotifyMousePositionInfo(p1 As Date, captureInfoRequest As CaptureInfoRequest)
        If _MouseCaptured AndAlso Not _UpdatePoint Is Nothing Then
            _UpdatePoint.ActionDate = New DateTime(CLng(p1.Ticks - (((p1.Ticks / TimeSpan.TicksPerSecond) Mod _VacSecs) + _VacSecs / 2) * TimeSpan.TicksPerSecond))
        End If
    End Sub

    Private Sub RefreshPilototoList(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        _RouteViewModel = _Model.GetPilototoRoute
        DataContext = _RouteViewModel
    End Sub





End Class
