﻿'This file is part of Routeur.
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

Imports System.ComponentModel

Partial Public Class frmUserPicker

    Private _PlayerInfo As clsPlayerInfo

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Sub SelectedItemChanged(ByVal sender As Object, ByVal e As System.Windows.Controls.SelectionChangedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        If Not Me.lstUsers.SelectedItem Is Nothing Then
            _PlayerInfo = CType(Me.lstUsers.SelectedItem, RegistryPlayerInfo).Playerinfo
            GetBoatInfo(_PlayerInfo)
        Else
            _PlayerInfo = Nothing
        End If

    End Sub

    Public ReadOnly Property PlayerInfo() As RegistryPlayerInfo
        Get
            If lstUsers.SelectedItem Is Nothing Then
                Return Nothing
            Else
                Return CType(lstUsers.SelectedItem, RegistryPlayerInfo)

            End If
        End Get
    End Property

    Private Sub AddUser(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim BoatName As String
        Dim R As RouteurModel = CType(DataContext, RouteurModel)

        'If sender IsNot cmdAddNewOldStyle Then

        Dim frm As New frmNewBoat

        If frm.ShowDialog Then
            Dim IDP As String = frm.UserName
            Dim IDU As Integer = frm.Boat.idu
            BoatName = frm.Boat.boatpseudo
            Dim Password As String = frm.Password

            Dim P As RegistryPlayerInfo

            For Each P In R.RegisteredPlayers
                If P.Nick = BoatName Then
                    MessageBox.Show("This boat is already declared, use another name")
                    Return
                End If
            Next

            P = New RegistryPlayerInfo(BoatName)
            P.NewStyle = True
            P.Email = IDP
            P.IDU = IDU
            P.Password = Password
            SaveUserInfo(P)
            R.RegisteredPlayersUpdated()
        End If
        'Else


    End Sub

    Private Sub PassLostFocus(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim P As RegistryPlayerInfo = CType(CType(sender, PasswordBox).DataContext, RegistryPlayerInfo)

        P.Password = CType(sender, PasswordBox).Password

    End Sub

    Private Sub cmdOK_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles cmdOK.Click

        If Me.lstUsers.SelectedItem Is Nothing Then
            MessageBox.Show("You must select a boat to start")
        Else
            Dim P As RegistryPlayerInfo = CType(Me.lstUsers.SelectedItem, RegistryPlayerInfo)

            If Not P.IsRacing Then
                MessageBox.Show("The selected boat is not engaged in a race")
            Else
                RegistryHelper.SetLastPlayer(P.Nick)
                Hide()
            End If
        End If

    End Sub

    Private Sub cmdCancel_Click(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs) Handles cmdCancel.Click

        Me.lstUsers.SelectedItem = Nothing
        Hide()

    End Sub

    Private Sub DeleteBoat(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.

        Dim P As RegistryPlayerInfo = CType(CType(sender, Button).DataContext, RegistryPlayerInfo)

        If MessageBox.Show("Are you sure you want to delete " & P.Nick & " boat information?", "Delete Boat Info", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.None) = MessageBoxResult.Yes Then
            DeleteUser(P)
            P.deleted = True
            Dim R As RouteurModel = CType(DataContext, RouteurModel)
            R.RegisteredPlayers.Remove(P)
        End If

    End Sub

    Private Sub ShowRacePrefs(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)

        Dim PlayerInfo As RegistryPlayerInfo = CType(CType(sender, Button).DataContext, RegistryPlayerInfo)

        Dim frm As New frmRouterPrefs(PlayerInfo.RaceNum)
        frm.ShowDialog()

    End Sub


    Private Sub frmUserPicker_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing
        e.Cancel = True
    End Sub

    Private Sub OnRaceListRequest(sender As System.Object, e As System.Windows.RoutedEventArgs)

        Dim frm As New frmRaceSelection

        frm.Owner = Me

        frm.Boat = CType(CType(sender, Button).DataContext, RegistryPlayerInfo)

        If frm.ShowDialog() Then
            Dim R As RouteurModel = CType(DataContext, RouteurModel)
            R.RegisteredPlayersUpdated()
        End If
        frm.Close()
    End Sub
End Class
