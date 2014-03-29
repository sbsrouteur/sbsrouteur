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

Partial Public Class frmRouterConfiguration

    Private _Context As RacePrefs
    Private _ID As String
    Private _Owner As RouteurMain
    Private _bCapture As Boolean
    'Private _StartBrush As Brush
    'Shared _AlphaBrush As Brush = New SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 255))
    Private _DragStart As Boolean

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Sub New(ByVal Parent As RouteurMain, ByVal ID As String)

        Me.New()
        _ID = ID
        _Context = RacePrefs.GetRaceInfo(_ID)
        DataContext = _Context
        _Owner = Parent
    End Sub

    Private Sub cmdOKClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim P As RacePrefs = CType(DataContext, RacePrefs)
        
        RacePrefs.Save(_ID, _Context)
        DialogResult = True
        Hide()

    End Sub

    Private Sub cmdCancelClick(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        _Context = RacePrefs.GetRaceInfo(_ID)
        DialogResult = False
        Hide()

    End Sub

    Private Sub StartTargetCaptureGrab(ByVal sender As Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        _DragStart = sender Is Me.imageStart
        Mouse.OverrideCursor = Cursors.Cross
        _bCapture = Mouse.Capture(CType(e.Source, IInputElement))
        '_StartBrush = LayoutRoot.Background
        'LayoutRoot.Background = _AlphaBrush

    End Sub

    Private Sub EndTargetCaptureGrab(ByVal sender as Object, ByVal e as System.Windows.Input.MouseButtonEventArgs)
    	'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        Mouse.Capture(Nothing)
        Mouse.OverrideCursor = Nothing
        _bCapture = Nothing
        'LayoutRoot.Background = _StartBrush
    End Sub

    Private Sub AlternateDestCapture(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        If Not _Owner Is Nothing AndAlso _bCapture Then

            Dim P As Point = _Owner.RenderCanvasCoords(e)
            Dim M As RouteurModel = CType(_Owner.FindResource("RouteurModel"), RouteurModel)
            If _DragStart Then
                _Context.RouteStart = M.CanvasToCoords(P)
            Else
                _Context.RouteDest = M.CanvasToCoords(P)
            End If

        End If

    End Sub
End Class
