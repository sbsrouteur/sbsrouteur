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

Imports System.ComponentModel

Public Class clsPlayerInfo

    Implements INotifyPropertyChanged


    Private _Nick As String
    Private _Email As String
    Private _Password As String
    Private _NumBoat As Integer
    Private _RaceInfo As New VLMRaceInfo
    

    'Private _Route() As Coords

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Property Email() As String
        Get
            Return _Email
        End Get
        Set(ByVal value As String)
            _Email = value
        End Set
    End Property


    Public Property Nick() As String
        Get
            Return _Nick
        End Get
        Set(ByVal value As String)
            _Nick = value

        End Set
    End Property

    Public Property NumBoat() As Integer
        Get
            Return _NumBoat
        End Get
        Set(ByVal value As Integer)
            _NumBoat = value
        End Set
    End Property

    Public Property Password() As String
        Get
            Return _Password
        End Get
        Set(ByVal value As String)
            _Password = value
        End Set
    End Property

    Public Property RaceInfo() As VLMRaceInfo
        Get
            Return _RaceInfo
        End Get
        Set(ByVal value As VLMRaceInfo)
            _RaceInfo = value
        End Set
    End Property

    Public ReadOnly Property Route() As Coords()
        Get
            If RaceInfo IsNot Nothing Then
                Return RaceInfo.Route
            Else
                Return Nothing
            End If
        End Get
        'Set(ByVal value As Coords())
        '    _Route = value
        'End Set
    End Property

    Public Property TrackColor As Color

End Class
