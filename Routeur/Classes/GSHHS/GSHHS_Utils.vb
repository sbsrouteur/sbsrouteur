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

Module GSHHS_Utils

    Public GRID_HIT_TOLERANCE As Double = RouteurModel.GridGrain / 5

    Public Structure LineParam
        Public a As Double
        Public b As Double
    End Structure

    '    Public Function CheckSegmentValid(ByVal TC As TravelCalculator) As Boolean

    '        Static Px As Double = RouteurModel.GridGrain / 180 / BspRect.GRID_GRAIN_OVERSAMPLE * Math.PI * 2
    '#If GRID_STAT Then
    '        Static _CheckTCTickCount As Long = 0
    '        Static _CheckTCCount As Long = 0
    '        Static _CheckTCTotalTicks As Long = 0

    '        Dim StartTick As DateTime = Now
    '        Try
    '#End If

    '        If TC.SurfaceDistance = 0 Or RouteurModel.NoObstacle Then
    '            Return True
    '        End If

    '        If GSHHS_Reader.HitTest(TC.EndPoint, GRID_HIT_TOLERANCE, GSHHS_Reader.Polygons(TC.EndPoint), True) Then
    '            Return False
    '        End If

    '        Dim Dx As Double

    '        If TC.EndPoint.Lon * TC.StartPoint.Lon < 0 AndAlso Abs(TC.EndPoint.Lon * TC.StartPoint.Lon) > PI Then
    '            Dx = TC.EndPoint.Lon + TC.StartPoint.Lon
    '        Else
    '            Dx = TC.EndPoint.Lon - TC.StartPoint.Lon
    '        End If

    '        Dim Dy As Double = TC.EndPoint.Lat - TC.StartPoint.Lat
    '        Dim CurP As New Coords(TC.StartPoint)
    '        Dim SegmentStep As Integer
    '        Dim i As Integer
    '        If Math.Abs(Dx) > Math.Abs(Dy) Then
    '            SegmentStep = CInt(Math.Ceiling(Dx / Px))
    '        Else
    '            SegmentStep = CInt(Math.Ceiling(Dy / Px))
    '        End If

    '        If SegmentStep < 0 Then
    '            SegmentStep = -SegmentStep
    '        End If

    '        Dx /= SegmentStep
    '        Dy /= SegmentStep

    '        For i = 1 To SegmentStep
    '            CurP.Lon += Dx
    '            CurP.Lat += Dy
    '            Dim Polys = GSHHS_Reader.Polygons(CurP)
    '            If GSHHS_Reader.HitTest(CurP, GRID_HIT_TOLERANCE, Polys, True) Then
    '                Return False
    '            End If
    '        Next

    '        Return True

    '#If GRID_STAT Then
    '        Finally
    '            _CheckTCCount += 1
    '            _CheckTCTotalTicks += Now.Subtract(StartTick).Ticks
    '            Stats.SetStatValue(Stats.StatID.GridAvg_CheckSegmentMS) = _CheckTCTotalTicks / _CheckTCCount / TimeSpan.TicksPerMillisecond
    '        End Try
    '#End If

    '    End Function

    Private Sub DenormalizeSegmentForAnte(S1_P1 As Coords, S1_P2 As Coords)
        If S1_P1.Lon * S1_P2.Lon < 0 And Abs(S1_P1.Lon * S1_P2.Lon) > Math.PI Then
            If S1_P1.Lon < 0 Then
                S1_P1.Lon += 2 * PI
            Else
                S1_P2.Lon += 2 * PI
            End If
        End If

    End Sub

    Private Function GetLineCoefs(P1 As Coords, P2 As Coords) As LineParam
        Dim RetCoords As LineParam
        If P1.N_Lon <> P2.N_Lon Then
            RetCoords.a = (P2.Lat - P1.Lat) / (P2.Lon - P1.Lon)
            RetCoords.b = P1.Lat - (RetCoords.a * P1.Lon)

        Else
            RetCoords.a = Double.NaN
            RetCoords.b = P1.N_Lon

        End If
        Return RetCoords
    End Function

    Public Function IntersectSegments(S1_P1 As Coords, S1_P2 As Coords, S2_P1 As Coords, S2_P2 As Coords) As Boolean

        If S1_P1.Equals1(S2_P1) OrElse
           S1_P1.Equals1(S2_P1) OrElse
           S2_P1.Equals(S1_P2) OrElse
           S2_P2.Equals1(S1_P2) Then
            Return True
        End If

        'Denormalize coords around ante-meridien
        DenormalizeSegmentForAnte(S1_P1, S1_P2)
        DenormalizeSegmentForAnte(S2_P1, S2_P2)


        Dim CoefS1 As LineParam = GetLineCoefs(S1_P1, S1_P2)
        Dim CoefS2 As LineParam = GetLineCoefs(S2_P1, S2_P2)

        If CoefS1.a = CoefS2.a Then
            Return CoefS1.b = CoefS2.b
        ElseIf Not Double.IsNaN(CoefS1.a) And Not Double.IsNaN(CoefS2.a) Then
            Dim x As Double = (CoefS2.b - CoefS1.b) / (CoefS1.a - CoefS2.a)
            Dim SegmentIntersect As Boolean = x >= (Min(S1_P1.Lon, S1_P2.Lon)) AndAlso x <= (Max(S1_P1.Lon, S1_P2.Lon)) AndAlso x >= (Min(S2_P1.Lon, S2_P2.Lon)) AndAlso x <= (Max(S2_P1.Lon, S2_P2.Lon))
            If SegmentIntersect = True Then
                Dim bp As Integer = 0
            End If
            Return SegmentIntersect
        ElseIf Double.IsNaN(CoefS1.a) AndAlso CoefS2.a = 0 Then
            Dim x As Double = (CoefS2.b)
            Dim SegmentIntersect As Boolean = x >= (Min(S1_P1.Lat, S1_P2.Lat)) AndAlso x <= (Max(S1_P1.Lat, S1_P2.Lat))
            'Dim SegmentIntersect As Boolean = CoefS1.b >= (Min(S2_P1.Lon, S2_P2.Lon)) And CoefS1.b <= (Max(S2_P1.Lon, S2_P2.Lon))
            If SegmentIntersect = True Then
                Dim bp As Integer = 0
            End If
            Return SegmentIntersect
        ElseIf Double.IsNaN(CoefS1.a) AndAlso Not Double.IsNaN(CoefS2.a) Then
            Dim y As Double = CoefS2.b + CoefS1.b * CoefS2.a
            Dim SegmentIntersect As Boolean = y >= (Min(S1_P1.Lat, S1_P2.Lat)) AndAlso y <= (Max(S1_P1.Lat, S1_P2.Lat)) AndAlso y >= (Min(S2_P1.Lat, S2_P2.Lat)) AndAlso y <= (Max(S2_P1.Lat, S2_P2.Lat))
            'Dim SegmentIntersect As Boolean = CoefS1.b >= (Min(S2_P1.Lon, S2_P2.Lon)) And CoefS1.b <= (Max(S2_P1.Lon, S2_P2.Lon))
            If SegmentIntersect = True Then
                Dim bp As Integer = 0
            End If
            Return SegmentIntersect
        ElseIf Double.IsNaN(CoefS2.a) And CoefS1.a = 0 Then
            Dim x As Double = CoefS1.b
            Dim SegmentIntersect As Boolean = x >= (Min(S2_P1.Lat, S2_P2.Lat)) AndAlso x <= (Max(S2_P1.Lat, S2_P2.Lat))
            'Dim SegmentIntersect As Boolean = CoefS2.b >= (Min(S1_P1.Lon, S1_P2.Lon)) And CoefS2.b <= (Max(S1_P1.Lon, S1_P2.Lon))
            If SegmentIntersect = True Then
                Dim bp As Integer = 0
            End If
            Return SegmentIntersect
        ElseIf Double.IsNaN(CoefS2.a) Then
            Dim y As Double = CoefS1.b + CoefS2.b * CoefS1.a
            Dim SegmentIntersect As Boolean = y >= (Min(S1_P1.Lat, S1_P2.Lat)) AndAlso y <= (Max(S1_P1.Lat, S1_P2.Lat)) AndAlso y >= (Min(S2_P1.Lat, S2_P2.Lat)) AndAlso y <= (Max(S2_P1.Lat, S2_P2.Lat))
            'Dim SegmentIntersect As Boolean = CoefS2.b >= (Min(S1_P1.Lon, S1_P2.Lon)) And CoefS2.b <= (Max(S1_P1.Lon, S1_P2.Lon))
            If SegmentIntersect = True Then
                Dim bp As Integer = 0
            End If
            Return SegmentIntersect
        Else

            Throw New NotImplementedException
        End If
        ''Normalize S1 for antemeridian
        'Dim X11 As Double
        'Dim X12 As Double
        'Dim X21 As Double
        'Dim X22 As Double
        'Dim Y11 As Double
        'Dim Y12 As Double
        'Dim Y21 As Double
        'Dim Y22 As Double
        'If S1_P1.Lon * S1_P2.Lon < 0 AndAlso Abs(S1_P1.Lon * S1_P2.Lon) > PI Then
        '    X11 = (S1_P1.Lon + PI) Mod (2 * PI)
        '    X12 = (S1_P2.Lon + PI) Mod (2 * PI)
        'Else
        '    X11 = S1_P1.Lon
        '    X12 = S1_P2.Lon
        'End If

        'If S2_P1.Lon * S2_P2.Lon < 0 AndAlso Abs(S2_P1.Lon * S2_P2.Lon) > PI Then
        '    X21 = (S2_P1.Lon + PI) Mod (2 * PI)
        '    X22 = (S2_P2.Lon + PI) Mod (2 * PI)
        'Else
        '    X21 = S2_P1.Lon
        '    X22 = S2_P2.Lon
        'End If

        'Y11 = S1_P1.Lat
        'Y12 = S1_P2.Lat
        'Y21 = S2_P1.Lat
        'Y22 = S2_P2.Lat

        'Dim P1 As Double = (X21 - X11) * (X22 - X11) + (Y21 - Y11) * (Y22 - Y11)
        'If P1 = 0 Then
        '    Return True
        'End If
        'Dim P2 As Double = (X21 - X12) * (X22 - X12) + (Y21 - Y12) * (Y22 - Y12)
        'If P2 = 0 Then
        '    Return True
        'End If

        'Return P1 * P2 < 0

    End Function



End Module
