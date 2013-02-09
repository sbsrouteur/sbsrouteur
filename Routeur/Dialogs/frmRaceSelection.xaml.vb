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
Imports System.Collections.ObjectModel
Imports System.ComponentModel

Partial Public Class frmRaceSelection

    Implements INotifyPropertyChanged

    Public Property Boat As RegistryPlayerInfo

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _RaceList As ObservableCollection(Of VLMShortRaceInfo)

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub OnCloseRequest(sender As System.Object, e As System.Windows.RoutedEventArgs)

        DialogResult = False
        Hide()

    End Sub


    Private Sub LoadRaceList()

        RaceList = New ObservableCollection(Of VLMShortRaceInfo)(GetRaceList())

    End Sub

    Public Property RaceList() As ObservableCollection(Of VLMShortRaceInfo)
        Get
            If _RaceList Is Nothing Then
                LoadRaceList()
            End If
            Return _RaceList
        End Get
        Set(value As ObservableCollection(Of VLMShortRaceInfo))
            _RaceList = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceList"))
        End Set
    End Property


    Private Sub OnEngagementRequest(sender As System.Object, e As System.Windows.RoutedEventArgs)

        If TypeOf sender Is Button Then
            Dim B As Button = CType(sender, Button)

            If TypeOf B.DataContext Is VLMShortRaceInfo Then
                Dim RaceID As String = CType(B.DataContext, VLMShortRaceInfo).idraces
                If WS_Wrapper.EngageBoatInRace(Boat, RaceID) Then
                    DialogResult = True
                    Hide()
                End If
            End If
        End If


    End Sub
End Class
