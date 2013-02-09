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

Public Class VLM_RaceWaypoint

    Implements IComparable(Of VLM_RaceWaypoint)

    'WP Type masks
    '#define WP_DEFAULT              0
    '#define WP_ICE_GATE_N           (1 <<  4)
    '#define WP_ICE_GATE_S           (1 <<  5)
    '#define WP_ICE_GATE_E           (1 <<  6)
    '#define WP_ICE_GATE_W           (1 <<  7)
    '#define WP_GATE_KIND_MASK       0x00F0
    '/* allow crossing in one direction only */
    '#define WP_CROSS_CLOCKWISE      (1 <<  8)
    '#define WP_CROSS_ANTI_CLOCKWISE (1 <<  9)
    '/* for future releases */
    '#define WP_CROSS_ONCE           (1 << 10)

    Public Enum Enum_WP_TypeMasks As Integer
        WP_TWO_BUOYS = 0
        WP_ONE_BUOY = 1
        ICE_GATE_N = (1 << 4)
        WP_ICE_GATE_S = (1 << 5)
        WP_ICE_GATE_E = (1 << 6)
        WP_ICE_GATE_W = (1 << 7)
        WP_GATE_KIND_MASK = &HF
        WP_NOT_GATE_KIND_MASK = &HF0
        WP_CROSS_CLOCKWISE = (1 << 8)
        WP_CROSS_ANTI_CLOCKWISE = (1 << 9)
        WP_CROSS_ONCE = (1 << 10)
    End Enum

    Private _wporder As Integer
    Private _laisser_au As Integer
    Private _wptype As String
    Private _wpformat As Integer
    Private _latitude1 As Double
    Private _longitude1 As Double
    Private _latitude2 As Double
    Private _longitude2 As Double
    Private _libelle As String
    Private _maparea As Integer



    Public Property laisser_au() As Integer
        Get
            Return _laisser_au
        End Get
        Set(ByVal value As Integer)
            _laisser_au = value
        End Set
    End Property

    Public Property latitude1() As Double
        Get
            Return _latitude1
        End Get
        Set(ByVal value As Double)
            _latitude1 = value
        End Set
    End Property

    Public Property latitude2() As Double
        Get
            Return _latitude2
        End Get
        Set(ByVal value As Double)
            _latitude2 = value
        End Set
    End Property

    Public Property libelle() As String
        Get
            Return _libelle
        End Get
        Set(ByVal value As String)
            _libelle = value
        End Set
    End Property

    Public Property longitude1() As Double
        Get
            Return _longitude1
        End Get
        Set(ByVal value As Double)
            _longitude1 = value
        End Set
    End Property

    Public Property longitude2() As Double
        Get
            Return _longitude2
        End Get
        Set(ByVal value As Double)
            _longitude2 = value
        End Set
    End Property

    Public Property maparea() As Integer
        Get
            Return _maparea
        End Get
        Set(ByVal value As Integer)
            _maparea = value
        End Set
    End Property

    Public Property wpformat() As Integer
        Get
            Return _wpformat
        End Get
        Set(ByVal value As Integer)
            _wpformat = value
        End Set
    End Property

    Public Property wporder() As Integer
        Get
            Return _wporder
        End Get
        Set(ByVal value As Integer)
            _wporder = value
        End Set
    End Property

    Public ReadOnly Property WPs() As List(Of Coords())
        Get
            Dim WP(1) As Coords
            Dim L As New List(Of Coords())
            WP(0) = New Coords(latitude1 / 1000, longitude1 / 1000)
            If (wpformat And Enum_WP_TypeMasks.WP_GATE_KIND_MASK) = Enum_WP_TypeMasks.WP_ONE_BUOY OrElse (latitude1 = latitude2 AndAlso longitude1 = longitude2) Then
                Dim TC As New TravelCalculator
                TC.StartPoint = WP(0)
                WP(1) = TC.ReachDistance(1500, 180 + _laisser_au)
                TC.StartPoint = Nothing
                TC.EndPoint = Nothing
                TC = Nothing
            Else
                '2 buoys
                WP(1) = New Coords(latitude2 / 1000, longitude2 / 1000)
            End If
            L.Add(WP)
            Return L
        End Get
    End Property


    Public Property wptype() As String
        Get
            Return _wptype
        End Get
        Set(ByVal value As String)
            _wptype = value
        End Set
    End Property

    Public Function CompareTo(ByVal other As VLM_RaceWaypoint) As Integer Implements System.IComparable(Of VLM_RaceWaypoint).CompareTo

        If wporder = other.wporder Then
            Return 0
        ElseIf wporder > other.wporder Then
            Return 1
        Else
            Return -1
        End If

    End Function

    Public Overrides Function ToString() As String

        Return "WP " & wporder & " - " & libelle

    End Function

End Class
