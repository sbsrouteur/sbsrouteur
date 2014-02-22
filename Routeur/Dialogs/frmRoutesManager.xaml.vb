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

        If DataContext IsNot Nothing AndAlso TypeOf DataContext Is RouteManager Then
            CType(DataContext, RouteManager).Save()
        End If
        Close()
    End Sub

    Private Sub frmRoutesManager_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Closed
        OnClose(sender, Nothing)
    End Sub

    Private Sub OnRouteDuplicate(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RecordedRoute Then

            Dim M As RouteManager = CType(DataContext, RouteManager)
            M.DuplicateRoute(CType(CType(sender, Button).DataContext, RecordedRoute))
        End If

    End Sub


    Private Sub OnRouteRecompute(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)


        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RecordedRoute Then

            Dim M As RouteManager = CType(DataContext, RouteManager)
            M.RecomputeRoute(CType(CType(sender, Button).DataContext, RecordedRoute))
        End If

    End Sub

    Private Sub OnRouteDelete(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RecordedRoute Then

            Dim M As RouteManager = CType(DataContext, RouteManager)
            M.DeleteRoute(CType(CType(sender, Button).DataContext, RecordedRoute))
        End If
    End Sub

    Private Sub AddNewRoute(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        If TypeOf sender Is Button AndAlso TypeOf CType(sender, Button).DataContext Is RouteManager Then

            Dim M As RouteManager = CType(DataContext, RouteManager)
            Dim R As RecordedRoute = M.AddNewRoute(M.FilterRaceID, "", "New route created " & Now.ToString)

        End If
    End Sub

    Private Sub OnAddPoint(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim i As Integer = 0

    End Sub

    Private Sub OnRouteExport(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim Dlg As New Microsoft.Win32.SaveFileDialog
        Dim R As RecordedRoute = CType(CType(sender, Button).DataContext, RecordedRoute)

        If R IsNot Nothing Then
            With Dlg
                .AddExtension = True
                .CheckFileExists = False
                .CheckPathExists = True
                .DefaultExt = ".xml"
                .Filter = "Router CSV File format (*.csv)|*.csv|VLM XML export File format (*.xml)|*.xml|FrogTool VLM Archive File format (*.json)|*.json"
                .FilterIndex = 1
                If .ShowDialog() Then
                    Dim M As RouteManager = CType(DataContext, RouteManager)

                    If .FileName.EndsWith(".csv") Then
                        R.ExportRoute_CSVFormat(.FileName)
                    ElseIf .FileName.EndsWith(".json") Then
                        R.ExportRoute_JSONFormat(.FileName)
                    Else
                        R.ExportRoute_XMLFormat(.FileName)

                    End If
                End If

            End With
        End If




    End Sub
End Class
