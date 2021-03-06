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
Imports System.Threading

Public Class MeteoInfo

    Implements INotifyPropertyChanged


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Const NO_VALUE As Integer = -100

    Private _Dir As Double = NO_VALUE
    Private _Strength As Double = NO_VALUE
    Private _UGRD As Double
    Private _VGRD As Double
    Private _DataOK As Boolean = False
    Private Shared DirProp As New PropertyChangedEventArgs("Dir")
    Private Shared StrengthProp As New PropertyChangedEventArgs("Strength")
    'Private Shared DateProp As New PropertyChangedEventArgs("GribDate")
    Private _GribDate As DateTime
    'Private _SyncEvent As New ManualResetEvent(False)


    Public ReadOnly Property DataOK() As Boolean
        Get
            Return _DataOK
        End Get
    End Property

    Public Property Dir() As Double
        Get

            Return _Dir
        End Get
        Set(ByVal value As Double)
            _Dir = value
            RaiseEvent PropertyChanged(Me, DirProp)
        End Set
    End Property

    Public Property GribDate() As DateTime
        Get
            Return _GribDate
        End Get
        Set(ByVal value As DateTime)
            _GribDate = value
            '      RaiseEvent PropertyChanged(Me, DateProp)
        End Set
    End Property

    Public Property Strength() As Double
        Get
            If _Strength < 0 Then
                Dim i As Integer = 0
            End If
            Return _Strength
        End Get
        Set(ByVal value As Double)
            If value < 0 Then
                Dim i As Integer = 0
            End If
            _Strength = value
            RaiseEvent PropertyChanged(Me, StrengthProp)
        End Set
    End Property

    'Public ReadOnly Property SyncEvent As ManualResetEvent
    '    Get
    '        Return _SyncEvent
    '    End Get
    'End Property

    Private Function GetDir() As Double
        Dim dir As Double
        'If _UGRD = 0 Then
        'dir = 0
        'If _VGRD < 0 Then
        ' dir = 180
        'End If
        'Else

        'Dim atan = Math.Atan2(_UGRD, _VGRD)
        'dir = (CInt(atan / Math.PI * 180) + 180 + 360) Mod 360
        '            # define _transform_u_v(a, b)                   \
        '402	  t_speed = sqrt(a*a+b*b);                      \
        '403	  b = acos(-b/t_speed);                         \
        '404	  if (a > 0.0) {                                \
        '405	    b = TWO_PI - b;                             \
        '406	  }

        Dim t_speed As Double = Math.Sqrt(_UGRD * _UGRD + _VGRD * _VGRD)
        dir = Math.Acos(-_VGRD / t_speed)
        If _UGRD > 0 Then
            dir = 2 * Math.PI - dir
        End If

        dir = (dir / Math.PI * 180) Mod 360

        'End If

        Return dir

    End Function
    Private Function GetStrength() As Double
        Return Math.Sqrt(_UGRD * _UGRD + _VGRD * _VGRD) * 3.6 / 1.852
    End Function

    Public Property UGRD() As Double
        Get
            Return _UGRD
        End Get
        Set(ByVal value As Double)
            _UGRD = value
            _Strength = GetStrength()
            _Dir = GetDir()
            _DataOK = True
            '    RaiseEvent PropertyChanged(Me, StrengthProp)
            '   RaiseEvent PropertyChanged(Me, DirProp)
        End Set
    End Property

    Public Property VGRD() As Double
        Get
            Return _VGRD
        End Get
        Set(ByVal value As Double)
            _VGRD = value
            _Strength = GetStrength()
            _Dir = GetDir()
            _DataOK = True
            '           RaiseEvent PropertyChanged(Me, StrengthProp)
            '            RaiseEvent PropertyChanged(Me, DirProp)
        End Set
    End Property


End Class

