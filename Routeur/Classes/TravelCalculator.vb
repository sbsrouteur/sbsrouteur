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
Imports System.Math
Imports System.ComponentModel

Public Class TravelCalculator


    Public Const Earth_Radius As Double = 3443.84
    Public Const EPSILON As Double = 0.0000000001

    Private _Cap As Double
    Private _DistanceAngle As Double

    Dim ownertype As Control

    Private WithEvents _StartPoint As Coords
    Private WithEvents _EndPoint As Coords

    Private Const PRECISION As Integer = 100000


    Public ReadOnly Property LoxoCourse_Deg As Double
        Get
            If StartPoint Is Nothing OrElse EndPoint Is Nothing Then
                Return Double.NaN
            ElseIf StartPoint.N_Lon * EndPoint.N_Lon < 0 AndAlso Abs(StartPoint.N_Lon - EndPoint.N_Lon) > PI Then
                Return LoxoCourse_Deg_Aviat_withante
            Else
                Return LoxoCourse_Deg_Aviat
            End If
        End Get
    End Property


    Private ReadOnly Property LoxoCourse_Deg_Aviat As Double
        Get

            Dim Lon1 As Double = -StartPoint.N_Lon
            Dim Lon2 As Double = -EndPoint.N_Lon
            Dim Lat1 As Double = StartPoint.N_Lat
            Dim Lat2 As Double = EndPoint.N_Lat

            Dim tc As Double = Atan2(Lon1 - Lon2, Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))) Mod (2 * PI)

            Dim ret As Double = ((tc / PI * 180) + 360) Mod 360
            If Double.IsNaN(ret) Then
                Dim i As Integer = 0
            End If
            Return ret

        End Get
    End Property

    Private ReadOnly Property LoxoCourse_Deg_Aviat_withante As Double
        Get

            Dim Lon1 As Double = -StartPoint.N_Lon
            Dim Lon2 As Double = -EndPoint.N_Lon
            Dim Lat1 As Double = StartPoint.N_Lat
            Dim Lat2 As Double = EndPoint.N_Lat

            If Lon1 > 0 Then
                Lon2 += 2 * PI
            Else
                Lon2 -= 2 * PI
            End If

            'dlon_W=mod(lon2-lon1,2*pi)
            'dlon_E=mod(lon1-lon2,2*pi)

            'dphi = Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))

            'if (abs(lat2-lat1) < sqrt(TOL)){
            '              q = Cos(Lat1)
            '} else {
            '              q = (Lat2 - Lat1) / dphi
            '}
            'if (dlon_W < dlon_E){// Westerly rhumb line is the shortest
            '    tc=mod(atan2(-dlon_W,dphi),2*pi)
            '                  d = Sqrt(q ^ 2 * dlon_W ^ 2 + (Lat2 - Lat1) ^ 2)
            '} else{
            '    tc=mod(atan2(dlon_E,dphi),2*pi)
            '                  d = Sqrt(q ^ 2 * dlon_E ^ 2 + (Lat2 - Lat1) ^ 2)
            '    }

            Dim dlon_w As Double = (Lon2 - Lon1) Mod (2 * PI)
            Dim dlon_e As Double = (Lon1 - Lon2) Mod (2 * PI)
            Dim dphi As Double = Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))
            'Dim q As Double
            'Dim d As Double
            Dim tc As Double

            'If (Abs(Lat2 - Lat1) < TOL) Then
            '    q = Cos(Lat1)
            'Else
            '    q = (Lat2 - Lat1) / dphi
            'End If

            If (dlon_w < dlon_e) Then '{// Westerly rhumb line is the shortest
                tc = Atan2(dlon_w, dphi) Mod (2 * PI)
                'd = Sqrt(q ^ 2 * dlon_w ^ 2 + (Lat2 - Lat1) ^ 2)
            Else
                tc = Atan2(-dlon_e, dphi) Mod (2 * PI)
                'd = Sqrt(q ^ 2 * dlon_e ^ 2 + (Lat2 - Lat1) ^ 2)

            End If

            'Dim c As Double = Cos(tc)
            'Dim s As Double = Sin(tc)
            'tc = Atan2(s, -c)

            Dim ret As Double = (720 - (tc / PI * 180)) Mod 360
            If Double.IsNaN(ret) Then
                Dim i As Integer = 0
            End If

            Return ret

        End Get
    End Property

    Private ReadOnly Property LoxoCourse_Deg_VLM() As Double
        Get

            If StartPoint Is Nothing Or EndPoint Is Nothing Then
                _Cap = 0
            Else
                Dim DA As Double = DistanceAngle
                Dim A As Double = ((Sin(EndPoint.N_Lat) - Sin(StartPoint.N_Lat) * Cos(DA)) / (Sin(DA) * Cos(StartPoint.N_Lat)))
                If A > 1 Then
                    A = 1
                ElseIf A < -1 Then
                    A = -1
                End If
                A = Acos(A)
                If -Sin(EndPoint.Lon - StartPoint.Lon) < 0 Then
                    _Cap = A / Math.PI * 180
                Else
                    _Cap = (2 * Math.PI - A) / Math.PI * 180
                End If

            End If
            Return _Cap

        End Get

    End Property

    Public ReadOnly Property DistanceAngleLoxo() As Double

        Get
            Dim Lat1 As Double = StartPoint.N_Lat
            Dim Lat2 As Double = EndPoint.N_Lat
            Dim Lon1 As Double = -StartPoint.Lon
            Dim Lon2 As Double = -EndPoint.Lon
            Dim TOL As Double = 0.000000000000001

            'Easy formula but fails @180E/W Workaround with return dist between 0 and PI
            Dim q As Double

            If (Abs(Lat2 - Lat1) < Sqrt(TOL)) Then
                q = Cos(Lat1)
            Else
                q = (Lat2 - Lat1) / Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))
            End If

            Dim ret As Double = Sqrt((Lat2 - Lat1) ^ 2 + q ^ 2 * (Lon2 - Lon1) ^ 2)

            If ret <= PI Then
                Return ret
            Else
                Return 2 * PI - ret
            End If

            'dlon_W=mod(lon2-lon1,2*pi)
            'dlon_E=mod(lon1-lon2,2*pi)
            'dphi = Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))
            'if (abs(lat2-lat1) < sqrt(TOL)){
            'q = Cos(Lat1)
            '} else {
            'q = (Lat2 - Lat1) / dphi
            '}
            'if (dlon_W < dlon_E){// Westerly rhumb line is the shortest
            'tc=mod(atan2(-dlon_W,dphi),2*pi)
            'd = Sqrt(q ^ 2 * dlon_W ^ 2 + (Lat2 - Lat1) ^ 2)
            '} else{
            'tc=mod(atan2(dlon_E,dphi),2*pi)
            'd = Sqrt(q ^ 2 * dlon_E ^ 2 + (Lat2 - Lat1) ^ 2)
            '}
            'Dim dlon_W As Double = Lon2 - Lon1 Mod (2 * PI)
            'Dim dlon_E As Double = (Lon1 - Lon2) Mod (2 * PI)
            'Dim dphi As Double = Log(Tan(Lat2 / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))
            'Dim q As Double
            'Dim tc As Double
            'Dim d As Double
            'If (Abs(Lat2 - Lat1) < Sqrt(TOL)) Then
            '    q = Cos(Lat1)
            'Else
            '    q = (Lat2 - Lat1) '/ dphi
            'End If
            'If (dlon_W < dlon_E) Then '{// Westerly rhumb line is the shortest
            '    tc = (Atan2(-dlon_W, dphi)) Mod (2 * PI)
            '    d = Sqrt(q ^ 2 * dlon_W ^ 2 + (Lat2 - Lat1) ^ 2)
            'Else
            '    tc = Atan2(dlon_E, dphi) Mod (2 * PI)
            '    d = Sqrt(q ^ 2 * dlon_E ^ 2 + (Lat2 - Lat1) ^ 2)
            'End If
            'Return d
        End Get
    End Property


    Public ReadOnly Property DistanceAngle() As Double
        Get

            If StartPoint Is Nothing Or EndPoint Is Nothing Then
                _DistanceAngle = 0
            ElseIf StartPoint.Lon = EndPoint.N_Lon AndAlso StartPoint.Lat = EndPoint.N_Lon Then
                _DistanceAngle = 0
            Else



                Dim dValue As Double = Math.Sin(StartPoint.Lat) * Math.Sin(EndPoint.Lat) + _
                  Math.Cos(StartPoint.Lat) * Math.Cos(EndPoint.Lat) * _
                  Math.Cos(-EndPoint.N_Lon + StartPoint.N_Lon)
                If dValue > 1 Then
                    dValue = 1
                ElseIf dValue < -1 Then
                    dValue = -1

                End If
                _DistanceAngle = Math.Acos(dValue)

            End If

            Return _DistanceAngle
        End Get
    End Property

    Public Function ReachDistanceortho(ByVal Dist As Double, ByVal tc_deg As Double) As Coords
        Const USE_AVIAT As Boolean = True
        If USE_AVIAT Then
            Return ReachDistanceAviat(Dist, tc_deg)
        Else
            Return ReachDistanceVLM(Dist, tc_deg)
        End If
    End Function

    Public Function ReachDistanceLoxo(ByVal Dist As Double, ByVal tc_deg As Double) As Coords
        Const USE_AVIAT As Boolean = False
        If USE_AVIAT Then
            Return ReachDistanceAviat(Dist, tc_deg)
        Else
            Return ReachDistanceVLM(Dist, tc_deg)
        End If
    End Function

    Public Function ReachDistanceLoxo_Aviat(Dist As Double, HeadingDeg As Double) As Coords

        Dim Lat1 As Double = StartPoint.N_Lat
        Dim Lon1 As Double = -StartPoint.N_Lon
        Dim d As Double = Dist / Earth_Radius
        Dim tc As Double = HeadingDeg / 180 * Math.PI
        Dim Lat As Double
        Dim lon As Double
        Dim TOL As Double = 0.000000000000001
        Dim q As Double
        Dim dPhi As Double
        Dim dlon As Double

        Lat = Lat1 + d * Cos(tc)
        If (Abs(Lat) > PI / 2) Then
            '"d too large. You can't go this far along this rhumb line!"
            Return Nothing
        End If
        If (Abs(Lat - Lat1) < Sqrt(TOL)) Then
            q = Cos(Lat1)
        Else
            dPhi = Log(Tan(Lat / 2 + PI / 4) / Tan(Lat1 / 2 + PI / 4))
            q = (Lat - Lat1) / dPhi
        End If
        dlon = -d * Sin(tc) / q
        lon = -(((Lon1 + dlon + PI) Mod 2 * PI) - PI)

        Return New Coords(CType(Lat, Decimal), CType(lon, Decimal), True)

    End Function

    Private Function ReachDistanceAviat(ByVal Dist As Double, ByVal tc_deg As Double) As Coords

        Dim retCoords As New Coords
        Dim dPhi As Double
        Dim q As Double
        Dim dlon As Double
        Dim tc_rad As Double

        Dim DistAngle As Double = Dist / Earth_Radius
        tc_rad = tc_deg / 180 * Math.PI

        If StartPoint.Lon >= PI - 0.0001 Then
            Dim i As Integer = 0
        End If

        With retCoords
            .Lat = StartPoint.Lat + DistAngle * Cos(tc_rad)
            dPhi = Log(Tan(.N_Lat / 2 + Math.PI / 4) / Tan(StartPoint.N_Lat / 2 + Math.PI / 4))
            If (Abs(.Lat - StartPoint.Lat) < Sqrt(EPSILON)) Then
                q = Cos(StartPoint.Lat)
            Else
                q = (.Lat - StartPoint.Lat) / dPhi
            End If
            dlon = DistAngle * Sin(tc_rad) / q
            .Lon = ((StartPoint.Lon + dlon + PI) Mod (2 * Math.PI)) - PI

            If .Lon < -PI Then
                .Lon += 2 * PI
            ElseIf .Lon > PI Then
                .Lon -= 2 * PI
            End If
        End With
        Return retCoords
    End Function

    Private Function ReachDistanceVLM(ByVal Dist As Double, ByVal tc_deg As Double) As Coords
        Dim RetCoords As New Coords


        Dim d As Double = Dist / 60 / 180.0 * PI
        Dim tc_rad As Double = tc_deg / 180 * PI

        '66	  latitude = aboat->latitude + d*cos(aboat->heading);
        RetCoords.Lat = StartPoint.Lat + d * Cos(tc_rad)

        '68	  t_lat = (latitude + aboat->latitude) / 2;
        Dim t_lat As Double = (RetCoords.Lat + StartPoint.Lat) / 2
        '69	  longitude = aboat->longitude + (d*sin(aboat->heading))/cos(t_lat);
        RetCoords.Lon = StartPoint.Lon + (d * Sin(tc_rad)) / Cos(t_lat)

        Return RetCoords
    End Function


    Public Property EndPoint() As Coords
        Get
            Return _EndPoint
        End Get

        Set(ByVal value As Coords)
            _EndPoint = value
        End Set
    End Property


    Public Property StartPoint() As Coords
        Get
            Return _StartPoint
        End Get

        Set(ByVal value As Coords)
            _StartPoint = value
        End Set
    End Property


    Public ReadOnly Property SurfaceDistance() As Double
        Get
            Return DistanceAngle * Earth_Radius
        End Get
    End Property

    Public ReadOnly Property SurfaceDistanceLoxo() As Double
        Get
            Return DistanceAngleLoxo * Earth_Radius
        End Get
    End Property


    ''' <summary>
    ''' Cap ortho en °
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property OrthoCourse_Deg() As Double
        Get
            Return (OrthoCourse_Rad() / Math.PI * 180 + 360) Mod 360

        End Get
    End Property

    ''' <summary>
    ''' Cap Ortho en radians
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function OrthoCourse_Rad() As Double
        If StartPoint Is Nothing Or EndPoint Is Nothing Then
            Return 0
        End If
        Dim retval As Double = Atan2(Sin(-StartPoint.Lon + EndPoint.Lon) * Cos(EndPoint.Lat), _
                     Cos(StartPoint.Lat) * Sin(EndPoint.Lat) - Sin(StartPoint.Lat) * Cos(EndPoint.Lat) * Cos(-StartPoint.Lon + EndPoint.Lon))

        retval = retval Mod (2 * PI)
        Return retval

    End Function


    Function ReachDistanceOrtho(f As Double) As Coords

        Dim RetP As New Coords
        Dim A As Double
        Dim B As Double
        Dim d As Double = DistanceAngle
        Dim x As Double
        Dim y As Double
        Dim z As Double
        Dim lat1 As Double = StartPoint.Lat
        Dim lon1 As Double = -StartPoint.Lon
        Dim lat2 As Double = EndPoint.Lat
        Dim lon2 As Double = -EndPoint.Lon
        A = Sin((1 - f) * d) / Sin(d)
        B = Sin(f * d) / Sin(d)
        x = A * Cos(lat1) * Cos(lon1) + B * Cos(lat2) * Cos(lon2)
        y = A * Cos(lat1) * Sin(lon1) + B * Cos(lat2) * Sin(lon2)
        z = A * Sin(lat1) + B * Sin(lat2)
        RetP.Lat = Atan2(z, Sqrt(x ^ 2 + y ^ 2))
        RetP.Lon = -Atan2(y, x)
        Return RetP
    End Function

End Class
