Imports System.Math

Module GSHHS_Utils

    Public GRID_HIT_TOLERANCE As Double = RouteurModel.GridGrain / 5

    Public Function CheckSegmentValid(ByVal TC As TravelCalculator) As Boolean

        Static Px As Double = RouteurModel.GridGrain / 180 / BspRect.GRID_GRAIN_OVERSAMPLE * Math.PI * 2
#If GRID_STAT Then
        Static _CheckTCTickCount As Long = 0
        Static _CheckTCCount As Long = 0
        Static _CheckTCTotalTicks As Long = 0

        Dim StartTick As DateTime = Now
        Try
#End If

        If TC.SurfaceDistance = 0 Or RouteurModel.NoObstacle Then
            Return True
        End If

        If GSHHS_Reader.HitTest(TC.EndPoint, GRID_HIT_TOLERANCE, GSHHS_Reader.Polygons(TC.EndPoint), True) Then
            Return False
        End If

        Dim Dx As Double

        If TC.EndPoint.Lon * TC.StartPoint.Lon < 0 AndAlso Abs(TC.EndPoint.Lon * TC.StartPoint.Lon) > PI Then
            Dx = TC.EndPoint.Lon + TC.StartPoint.Lon
        Else
            Dx = TC.EndPoint.Lon - TC.StartPoint.Lon
        End If

        Dim Dy As Double = TC.EndPoint.Lat - TC.StartPoint.Lat
        Dim CurP As New Coords(TC.StartPoint)
        Dim SegmentStep As Integer
        Dim i As Integer
        If Math.Abs(Dx) > Math.Abs(Dy) Then
            SegmentStep = CInt(Math.Ceiling(Dx / Px))
        Else
            SegmentStep = CInt(Math.Ceiling(Dy / Px))
        End If

        If SegmentStep < 0 Then
            SegmentStep = -SegmentStep
        End If

        Dx /= SegmentStep
        Dy /= SegmentStep

        For i = 1 To SegmentStep
            CurP.Lon += Dx
            CurP.Lat += Dy
            Dim Polys = GSHHS_Reader.Polygons(CurP)
            If GSHHS_Reader.HitTest(CurP, GRID_HIT_TOLERANCE, Polys, True) Then
                Return False
            End If
        Next

        Return True

#If GRID_STAT Then
        Finally
            _CheckTCCount += 1
            _CheckTCTotalTicks += Now.Subtract(StartTick).Ticks
            Stats.SetStatValue(Stats.StatID.GridAvg_CheckSegmentMS) = _CheckTCTotalTicks / _CheckTCCount / TimeSpan.TicksPerMillisecond
        End Try
#End If

    End Function

End Module
