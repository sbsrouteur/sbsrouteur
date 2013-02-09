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

Imports System.ComponentModel

Public Class RouterPrefs

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _TrackBackAngle As Double
    Private _ForwardAngle As Double
    Private _UseCustomWP As Boolean
    Private _CustWP As Coords

    Public Property CustWP() As Coords
        Get
            Return _CustWP
        End Get
        Set(ByVal value As Coords)
            _CustWP = value
        End Set
    End Property

    Public Property ForwardAngle() As Double
        Get
            Return _ForwardAngle
        End Get
        Set(ByVal value As Double)
            _ForwardAngle = value
        End Set
    End Property

    Public Property TrackBackAngle() As Double
        Get
            Return _TrackBackAngle
        End Get
        Set(ByVal value As Double)
            _TrackBackAngle = value
        End Set
    End Property

    Public Property UseCustomWP() As Boolean
        Get
            Return _UseCustomWP
        End Get
        Set(ByVal value As Boolean)
            _UseCustomWP = value
        End Set
    End Property

End Class
