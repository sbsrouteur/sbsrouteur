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
Imports System.Collections.ObjectModel
Imports System.Xml
Imports System.Math
Imports System.Xml.Serialization



Public Class Coords
    Implements ICoords

    Public Enum NORTH_SOUTH As Integer
        N = 1
        S = -1
    End Enum

    Public Enum EAST_WEST As Integer
        E = 1
        W = -1
    End Enum

    Private _Lat As Double
    Private _n_lat As Double
    Private _n_lat_deg As Double
    Private _Lat_Deg As Double
    Private _Lon As Double
    Private _n_lon As Double
    Private _n_lon_deg As Double
    Private _Lon_Deg As Double
    
    Sub New(PrevP As DotSpatial.Topology.Coordinate)
        Me.Lat_Deg = PrevP.Y
        Me.Lon_Deg = PrevP.X
    End Sub
    'Private _HashCode As Long = 0


    Public Property Lat() As Double Implements ICoords.Lat
        Get
            Return _Lat
        End Get
        Set(ByVal value As Double)
            _Lat = value
            _Lat_Deg = value / PI * 180

            If Math.Abs(_Lat_Deg) > 90 Then

                If _Lat_Deg >= 0 Then
                    _n_lat_deg = 90 - (_Lat_Deg - 90)
                    _n_lat = (PI / 2) - (_Lat - (PI / 2))
                Else
                    _n_lat_deg = -90 - (_Lat_Deg + 90)
                    _n_lat = -(PI / 2) - (_Lat + (PI / 2))
                End If

            Else
                _n_lat_deg = _Lat_Deg
            End If

        End Set
    End Property

    <XmlIgnore()> _
    Public Property Lat_Deg() As Double
        Get
            Return _Lat_Deg
        End Get
        Set(ByVal value As Double)
            Lat = value / 180 * Math.PI
        End Set
    End Property

    Public Property Lon() As Double Implements ICoords.Lon
        Get
            Return _Lon
        End Get
        Set(ByVal value As Double)

            If Double.IsNaN(value) Then
                Dim bp As Integer = 0
            End If
            _Lon = value
            _n_lon = _Lon Mod (2 * PI)
            _Lon_Deg = value / PI * 180
            _n_lon_deg = _Lon_Deg Mod 360
            If _n_lon > PI Then
                _n_lon -= 2 * PI
                _n_lon_deg -= 360
            ElseIf _n_lon < -PI Then
                _n_lon += 2 * PI
                _n_lon_deg += 360
            End If
        End Set
    End Property

    <XmlIgnore()> _
    Public Property Lon_Deg() As Double
        Get
            Return _Lon_Deg
        End Get
        Set(ByVal value As Double)
            Lon = value / 180 * Math.PI
        End Set
    End Property


    Public Shared Operator =(ByVal v1 As Coords, ByVal v2 As Coords) As Boolean

        If v1 Is Nothing And v2 Is Nothing Then
            Return True
        ElseIf v1 Is Nothing Or v2 Is Nothing Then
            Return False
        Else
            Return v1.Lat = v2.Lat AndAlso v1.Lon = v2.Lon
        End If

    End Operator

    Public Shared Operator <>(ByVal v1 As Coords, ByVal v2 As Coords) As Boolean
        Return Not v1 = v2
    End Operator

    <XmlIgnore()> _
    Public ReadOnly Property N_Lon_Deg As Double
        Get
            Return _n_lon_deg
        End Get
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property N_Lon As Double Implements ICoords.N_Lon
        Get
            Return _n_lon
            
        End Get
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property N_Lat As Double Implements ICoords.N_Lat
        Get
            Return _n_lat
        End Get
    End Property

    <XmlIgnore()> _
    Public ReadOnly Property N_Lat_Deg As Double
        Get
            Return _n_lat_deg
        End Get
    End Property


    Public Sub RoundTo(ByVal RoundingGrain As Double)

        If RoundingGrain = 0 Then
            Return
        End If

        Lat_Deg = CInt(Lat_Deg / RoundingGrain) * RoundingGrain
        Lon_Deg = CInt(Lon_Deg / RoundingGrain) * RoundingGrain

        Return

    End Sub


    Public ReadOnly Property Mercator_X_Deg() As Double
        Get
            Return Lon_Deg
        End Get
    End Property

    Public Shared ReadOnly Property Mercator_Y_Deg(Lat_Deg As Double) As Double
        Get
            Dim ret As Double = Log(Tan(Lat_Deg) + 1 / Cos(Lat_Deg)) / PI * 180
            'Debug.Assert(Not Double.IsNaN(ret))
            If ret < -90 Then
                ret = -90
            ElseIf ret > 90 Then
                ret = 90
            End If
            Return ret

        End Get
    End Property

    Public ReadOnly Property Mercator_Y_Deg() As Double
        Get
            Dim ret As Double = Log(Tan(N_Lat) + 1 / Cos(N_Lat)) / PI * 180
            'Debug.Assert(Not Double.IsNaN(ret))
            If ret < -90 Then
                ret = -90
            ElseIf ret > 90 Then
                ret = 90
            End If
            Return ret
        End Get
    End Property


    Public Function [Module]() As Double

        Return Sqrt(X * X + Y * Y + Z * Z)

    End Function

    Public Sub New()

    End Sub

    Public Sub New(ByVal C As ICoords)

        If C Is Nothing Then
            Return
        End If
        Lat = C.Lat
        Lon = C.Lon

    End Sub

    Public Sub New(ByVal C As Coords)
        If C Is Nothing Then
            Return
        End If
        Lat = C.Lat
        Lon = C.Lon

    End Sub

    Public Sub New(ByVal LatDeg As Double, ByVal LonDeg As Double)

        Debug.Assert(Not Double.IsNaN(LonDeg))
        Debug.Assert(Not Double.IsNaN(LatDeg))
        Me.Lat_Deg = LatDeg
        Me.Lon_Deg = LonDeg

    End Sub

    Public Sub New(ByVal Lat_Deg As Decimal, ByVal Lon_Deg As Decimal)

        Debug.Assert(Not Double.IsNaN(Lon_Deg))
        Debug.Assert(Not Double.IsNaN(Lat_Deg))
        Me.Lat_Deg = Lat_Deg
        Me.Lon_Deg = Lon_Deg


    End Sub

    Public Sub New(ByVal Lat_Deg As Integer, ByVal Lat_Min As Integer, ByVal Lat_Sec As Integer, ByVal NS As NORTH_SOUTH, ByVal Lon_Deg As Integer, ByVal Lon_Min As Integer, ByVal Lon_Sec As Integer, ByVal EW As EAST_WEST)

        Me.Lat_Deg = (CInt(NS) * (Lat_Deg + Lat_Min / 60 + Lat_Sec / 3600)) ' Mod 180
        Me.Lon_Deg = (CInt(EW) * (Lon_Deg + Lon_Min / 60 + Lon_Sec / 3600)) ' Mod 180

    End Sub

    Public Sub New(ByVal x As Double, ByVal y As Double, ByVal z As Double)

        Lat = Atan2(z, Sqrt(x ^ 2 + y ^ 2))
        _Lon = -Atan2(-y, x)

    End Sub

    Public Sub Normalize()

        Dim M As Double = [Module]()
        Dim _x As Double = X / M
        Dim _y As Double = Y / M
        Dim _z As Double = Z / M

        Lat = Atan2(Z, Sqrt(_x ^ 2 + _y ^ 2))
        _Lon = -Atan2(-_y, _x)

    End Sub

    Public Overrides Function ToString() As String

        Static Cv As New CoordsConverter

        Return CStr(Cv.Convert(Me, GetType(String), 1, System.Threading.Thread.CurrentThread.CurrentUICulture))

    End Function

    Public ReadOnly Property X() As Double
        Get
            Return Cos(_Lat) * Cos(-_Lon)
        End Get
    End Property

    Public ReadOnly Property Y() As Double
        Get
            Return -Cos(Lat) * Sin(-Lon)
        End Get
    End Property

    Public ReadOnly Property Z() As Double
        Get
            Return Sin(Lat)
        End Get
    End Property

    Public Function Equals1(ByVal C As ICoords) As Boolean Implements ICoords.Equals
        Return CoordsComparer.Equals1(Me, C)
    End Function

    Public Function GetHashCode1() As Long Implements ICoords.GetHashCode
        Return CoordsComparer.GetHashCode1(Me)

    End Function
End Class



