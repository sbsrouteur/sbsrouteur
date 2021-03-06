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

Partial Public Class frmRoutingProgress

    Private WithEvents _RefreshTimer As New Timers.Timer(500) With {.Enabled = True}

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Public Sub New(ByVal TimerPeriodms As Integer, ByVal ProgressContext As ProgressContext)
        Me.New()
        DataContext = ProgressContext
        _RefreshTimer.Interval = TimerPeriodms
        AddHandler ProgressContext.RequestVisibility, AddressOf RequestVisibility
    End Sub

    Public Overloads Sub Show(ByVal owner As Window, ByVal Data As Object)

        If Dispatcher.Thread Is System.Threading.Thread.CurrentThread Then
            DataContext = Data
            Me.Owner = owner
            Show()
        Else
            Dispatcher.BeginInvoke(New Action(Of Window, Object)(AddressOf Me.Show), New Object() {owner, Data})
        End If

    End Sub

    Private Sub _RefreshTimer_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _RefreshTimer.Elapsed

        Dispatcher.BeginInvoke(New Action(AddressOf DeferredGUIRefresh))

    End Sub

    Private Sub DeferredGUIRefresh()
        If DataContext IsNot Nothing AndAlso TypeOf DataContext Is ProgressContext Then

            Dim PC As ProgressContext = CType(DataContext, ProgressContext)

            If PC.Dirty Then
                PC.refresh()
            End If
        End If
    End Sub

    Private Sub frmRoutingProgress_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing

        e.Cancel = True
        Visibility = Windows.Visibility.Hidden

    End Sub

    Protected Sub RequestVisibility(ByVal Vis As Visibility)

        If Dispatcher.Thread IsNot System.Threading.Thread.CurrentThread Then
            Dispatcher.BeginInvoke(New Action(Of Visibility)(AddressOf RequestVisibility), New Object() {Vis})
        Else
            Visibility = Vis

        End If
    End Sub


End Class
