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

Imports System.Threading
Imports Routeur.VLM_Router
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Threading.Tasks
Imports DotSpatial.Topology

Public Class IsoRouter
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event Log(ByVal msg As String)
    Public Event RouteComplete()

    Private _SearchAngle As Double
    Private _AngleStep As Double
    Private _VacLength As TimeSpan
    Private _EllipseExt As Double = 1.3
    Private _IsoChrones As New LinkedList(Of IsoChrone)

    Private _IsoRouteThread As Thread
    Private _CancelRequested As Boolean

    Private _StartPoint As clsrouteinfopoints
    Private _DestPoint As Coords
    Private _TC As New TravelCalculator
    Private _Meteo As GribManager
    Private _SailManager As clsSailManager
    Private _BoatType As String
    Private _CurBest As clsrouteinfopoints
    Private _DTFRatio As Double = 0.9
    Private _RouterPrefs As RacePrefs
    Private IsoRouterLock As New Object
    Private _DB As DBWrapper
    Private _MapLevel As Integer

#Const DBG_ISO = 0

    Public Sub New(ByVal BoatType As String, ByVal SailManager As clsSailManager, ByVal Meteo As GribManager, ByVal AngleStep As Double, ByVal SearchAngle As Double, ByVal VacLength As TimeSpan, MapLevel As Integer, EllipseExcent As Double)
        _AngleStep = AngleStep
        _VacLength = VacLength
        _SearchAngle = SearchAngle
        _EllipseExt = EllipseExcent
        _Meteo = Meteo
        _SailManager = SailManager
        _BoatType = BoatType
        _DB = New DBWrapper()
        _DB.MapLevel = MapLevel
        _MapLevel = MapLevel
    End Sub


    Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone, Optional DoNotCleanUp As Boolean = False) As IsoChrone
        'Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone) As IsoChrone

        Dim RetIsoChrone As IsoChrone = Nothing
        Dim P1 As clsrouteinfopoints
        Dim ReachPointDuration As Long = 0
        Dim ReachPointCount As Long = 0
        Dim MaxEllipsisDist As Double
        Dim CurThreadCount1 As Integer = 0
        Dim CurThreadCount2 As Integer = 0


        If Iso Is Nothing Then
            Dim TcInit As New TravelCalculator With {.StartPoint = _StartPoint.P}
            'Special Case for startpoint
            RetIsoChrone = New IsoChrone(_SailManager, _StartPoint.P, IsoChrone.ANGLE_STEP)
            For alpha = 0 To 360 - _AngleStep Step IsoChrone.ANGLE_STEP '_AngleStep
                P1 = ReachPoint(_StartPoint, alpha, _VacLength, False)
                If Not P1 Is Nothing Then

                    Dim tc As New TravelCalculator
                    tc.StartPoint = _StartPoint.P
                    tc.EndPoint = P1.P
                    P1.DistFromPos = tc.SurfaceDistance
                    P1.CapFromPos = tc.LoxoCourse_Deg

                    RetIsoChrone.ReplacePointAtIndex(RetIsoChrone.IndexFromAngle(tc.LoxoCourse_Deg), P1)

                End If
            Next

            'Dim tP1 As New Coords(38, 8, 11, Coords.NORTH_SOUTH.N, 118, 2, 13, Coords.EAST_WEST.E)
            'Dim tP2 As New Coords(38, 7, 34, Coords.NORTH_SOUTH.N, 118, 1, 5, Coords.EAST_WEST.E)
            'Dim ret As Boolean = _DB.IntersectMapSegment(tP2, tP1, GSHHS_Reader._Tree)
            'If ret Then
            '    MessageBox.Show("intersect")
            'End If
        Else
            RetIsoChrone = New IsoChrone(_SailManager, _StartPoint.P, IsoChrone.ANGLE_STEP, Iso)
            Dim StartIndex As Integer = 0
            Dim CurStep As TimeSpan = _VacLength
            Dim FirstIndex As Integer = -1
            'Dim _IsoSegs As New List(Of MapSegment)
            Dim PrevPoint As Coords = Nothing
            Dim CurPoint As Coords = Nothing
            'Dim MaxDist As Double = 0
            'Dim MinDist As Double = Double.MaxValue
            'Dim MaxSpeed As Double = 0
            'Dim SumSpeed As Double = 0
            'Dim NbSpeed As Double = 0

            Dim TcEllipsis As New TravelCalculator With {.StartPoint = _StartPoint.P, .EndPoint = _DestPoint}
            MaxEllipsisDist = TcEllipsis.SurfaceDistance * _EllipseExt
            TcEllipsis.StartPoint = Nothing
            TcEllipsis.EndPoint = Nothing
            TcEllipsis = Nothing

            'For Each Point In Iso.PointSet.Values
            '    If Point IsNot Nothing Then
            '        MaxDist = Math.Max(MaxDist, Point.DistFromPos)
            '        MinDist = Math.Min(MinDist, Point.DistFromPos)
            '        MaxSpeed = Math.Max(MaxSpeed, Point.Speed)
            '        SumSpeed += Point.Speed
            '        NbSpeed += 1
            '    End If
            'Next

            'If MaxSpeed = 0 Then
            '    Return Nothing
            'End If
            Dim NbMinutes As Double '= 'New TimeSpan(CLng(2 * 2 * Math.PI * MaxDist / 360 / MaxSpeed * TimeSpan.TicksPerHour)).TotalMinutes
            NbMinutes = RouteurModel.VacationMinutes ' * Math.Ceiling(NbMinutes / RouteurModel.VacationMinutes) 
            CurStep = New TimeSpan(0, CInt(NbMinutes), 0)


            Dim tc2 As New TravelCalculator

            'Dim IsoLine As New LinkedList(Of Coordinate)
            'Dim IsoPoly As New DotSpatial.Topology.Polygon(IsoLine)
            'For Each Iso In IsoChrones
            '    For Each P In Iso.PointSet
            '        IsoLine.UpdatePoint(P.Key, New Coordinate(P.Value.P.Lon, P.Value.P.Lat))
            '    Next
            'Next

            For Each pindex As Double In Iso.PointSet.Keys
                'Parallel.For(StartIndex, Iso.PointSet.Count - 1,
                '           Sub(Pindex As Integer)
                Dim tcfn1 As New TravelCalculator
                Dim tcfn2 As New TravelCalculator
                Dim CurDest As Coords
                Dim rp As clsrouteinfopoints

                'Routeur.Stats.SetStatValue(Stats.StatID.Isocrhone_ThreadCount1) = Interlocked.Increment(CurThreadCount1)
                'While CurThreadCount1 > 8
                '    System.Threading.Thread.Sleep(1)
                'End While
                If Not Iso.PointSet(pindex) Is Nothing Then
                    rp = Iso.PointSet(pindex)

                    Dim Ortho As Double
                    tcfn1.StartPoint = _StartPoint.P ' rp.P
                    tcfn1.EndPoint = rp.P '_DestPoint
                    CurDest = _DestPoint 'tcfn1.EndPoint
                    Ortho = tcfn1.LoxoCourse_Deg
                    tcfn1.StartPoint = rp.P
                    tcfn1.EndPoint = _StartPoint.P

                    'Dim TcAngleTrim As New TravelCalculator()
                    'Dim MinSearch As Double = 180
                    'Dim MaxSearch As Double = 180
                    'Dim PrevIndex As Integer = (pindex + Iso.PointSet.Count - 1) Mod Iso.PointSet.Count
                    'Dim NextIndex As Integer = (pindex + 1) Mod Iso.PointSet.Count
                    'If Iso.PointSet(PrevIndex) IsNot Nothing Then
                    '    TcAngleTrim.EndPoint = Iso.PointSet(PrevIndex).P
                    '    TcAngleTrim.StartPoint = rp.P
                    '    MaxSearch = WindAngleWithSign(TcAngleTrim.LoxoCourse_Deg, Ortho)
                    'End If
                    'If Iso.PointSet(NextIndex) IsNot Nothing Then
                    '    TcAngleTrim.StartPoint = rp.P
                    '    TcAngleTrim.EndPoint = Iso.PointSet(NextIndex).P
                    '    MinSearch = WindAngleWithSign(TcAngleTrim.LoxoCourse_Deg, Ortho)
                    'End If

                    Dim MinAngle As Double = Ortho - 90 '+ MinSearch
                    Dim maxAngle As Double = Ortho + 90 '+ MaxSearch

                    If MinAngle > maxAngle Then
                        maxAngle += 360
                    End If

                    tcfn1.StartPoint = _StartPoint.P
                    tcfn2.StartPoint = _StartPoint.P

                    'Dim StepCount As Integer = CInt(Math.Floor((maxAngle - MinAngle) / _AngleStep) + 1)
#If DBG_ISO = 1 Then
                    'Console.WriteLine("StepCount " & StepCount & " @ " & Pindex)
#End If
                    For alphaindex As Double = MinAngle To maxAngle Step CInt(_AngleStep) 'StepCount
                        'Parallel.For(0, StepCount,
                        'Sub(AlphaIndex As Integer)
                        'Dim IsoBucket As Integer = Interlocked.Increment(CurThreadCount2)
                        'Routeur.Stats.SetStatValue(Stats.StatID.Isocrhone_ThreadCount2) = IsoBucket
                        Dim alpha As Double = alphaindex '(MinAngle + _AngleStep * alphaindex) Mod 360
                        Dim P As clsrouteinfopoints
                        Dim tc As New TravelCalculator
                        Dim EllipsisDit As Double = 0

                        P = ReachPoint(rp, alpha, CurStep, True)
                        Dim CurDist As Double = 0
                        Dim LoxoCourse As Double = 0
                        If P IsNot Nothing Then
                            tc.StartPoint = _StartPoint.P
                            tc.EndPoint = P.P
                            CurDist = tc.SurfaceDistance
                            LoxoCourse = tc.LoxoCourse_Deg
                            EllipsisDit = tc.SurfaceDistance

                            tc.StartPoint = _DestPoint
                            EllipsisDit += tc.SurfaceDistance

                            tc.StartPoint = Nothing
                            tc.EndPoint = Nothing


                            'If Not P Is Nothing AndAlso P.P.Lat_Deg < 88 AndAlso CurDist > MinDist AndAlso EllipsisDit <= MaxEllipsisDist AndAlso (RouteurModel.NoObstacle OrElse Not _DB.IntersectMapSegment(rp.P, P.P, GSHHS_Reader._Tree)) AndAlso Not IsoPoly.Contains(New Point(P.P.Lon, P.P.Lat)) Then
                            If P.P.Lat_Deg < 88 AndAlso EllipsisDit <= MaxEllipsisDist Then

                                'Check that point is outside of previous isochrone
                                'tcfn1.EndPoint = P.P
                                Dim idx As Double = Iso.IndexFromAngle(LoxoCourse)
                                Dim AddPoint As Boolean = False

                                If RetIsoChrone.PointSet.ContainsKey(idx) Then
                                    tcfn2.EndPoint = RetIsoChrone.PointSet(idx).P

                                    If tcfn2.EndPoint Is Nothing OrElse CurDist >= tcfn2.SurfaceDistanceLoxo Then
                                        AddPoint = True
                                    End If
                                Else
                                    AddPoint = True
                                End If

                                If AddPoint = True AndAlso (RouteurModel.NoObstacle Or Not _DB.IntersectMapSegment(rp.P, P.P, GSHHS_Reader._Tree)) Then
                                    AddPoint = True
                                Else
                                    AddPoint = False
                                End If

                                If AddPoint Then

                                    P.DistFromPos = CurDist
                                    P.CapFromPos = LoxoCourse
                                    RetIsoChrone.ReplacePointAtIndex(idx, P)
                                End If



                            End If
                        End If
                        tc.StartPoint = Nothing
                        tc.EndPoint = Nothing
                        tc = Nothing
                        'Routeur.Stats.SetStatValue(Stats.StatID.Isocrhone_ThreadCount2) = Interlocked.Decrement(CurThreadCount2)


                        'End Sub)
                    Next

                End If

                tcfn1.EndPoint = Nothing
                tcfn1.StartPoint = Nothing
                tcfn1 = Nothing

                tcfn2.EndPoint = Nothing
                tcfn2.StartPoint = Nothing
                tcfn2 = Nothing

                'Routeur.Stats.SetStatValue(Stats.StatID.Isocrhone_ThreadCount1) = Interlocked.Decrement(CurThreadCount1)

                'End Sub)
            Next


            'Clean up bad points
            'If RetIsoChrone IsNot Nothing Then

            '    RetIsoChrone.CleanUp(Iso, DoNotCleanUp, 1)

            'End If

            tc2.EndPoint = Nothing
            tc2.StartPoint = Nothing
            tc2 = Nothing
        End If

        Return RetIsoChrone

    End Function


    Private Sub IsoRouteThread()

        Dim RouteComplete As Boolean = False
        Dim CurIsoChrone As IsoChrone = Nothing
        'Dim OuterIsochrone As New IsoChrone(_AngleStep / 4)
        Dim Start As DateTime = Now
        Dim P As clsrouteinfopoints = Nothing
        Dim PrevP As clsrouteinfopoints = Nothing
        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = _DestPoint
        Dim CurDTF As Double = Double.MaxValue
        Dim LastUpdate As DateTime = Now

        'Get the start meteo conditions TODO handle cancellation during the loadmeteo lockup
        Dim mi As MeteoInfo = Nothing
        While mi Is Nothing
            mi = _Meteo.GetMeteoToDate(_StartPoint.T, _StartPoint.P.N_Lon_Deg, _StartPoint.P.Lat_Deg, False, False)
            System.Threading.Thread.Sleep(50)
        End While

        _StartPoint.WindDir = mi.Dir
        _StartPoint.WindStrength = mi.Strength

        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim Dist As Double = TC.SurfaceDistance * 0.995

        RaiseEvent Log("Isochrone router started at " & Start)
        Dim LoopStart As DateTime = Now
        While Not RouteComplete AndAlso Not _CancelRequested

            P = Nothing

            Dim isostart As DateTime = Now

#If DBG_ISO = 1 Then
            Const MAX_DBG_ISO = 1000
            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone, _IsoChrones.Count = MAX_DBG_ISO)

#Else
            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
#End If
            Dim IsoDuration As Double = Now.Subtract(isostart).TotalMilliseconds

            Routeur.Stats.SetStatValue(Stats.StatID.Isochrone_ComputeTimeMS) = IsoDuration
            Routeur.Stats.SetStatValue(Stats.StatID.Isochrone_Rate) = Now.Subtract(LoopStart).TotalMilliseconds / _IsoChrones.Count
            Routeur.Stats.SetStatValue(Stats.StatID.Isochrone_Count) = _IsoChrones.Count
            If CurIsoChrone IsNot Nothing AndAlso CurIsoChrone.PointSet IsNot Nothing AndAlso CurIsoChrone.PointSet.Count > 0 Then
                Dim Idx As Double = CurIsoChrone.IndexFromAngle(0)
                If (CurIsoChrone.PointSet.ContainsKey(Idx)) Then
                    Routeur.Stats.SetStatValue(Stats.StatID.Isochrone_TimeHorizon) = CurIsoChrone.PointSet(Idx).T.Subtract(Now).TotalHours
                End If
            End If


#If DBG_ISO = 1 Then
            If CurIsoChrone IsNot Nothing Then
                Console.WriteLine("Isochrone computed in " & IsoDuration & " rate : " & IsoDuration / CurIsoChrone.PointSet.Count)
            End If
            'Console.WriteLine("IsoDone" & Now.Subtract(LoopStart).TotalMilliseconds)
#End If
            'CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            P = _CurBest
            If Not CurIsoChrone Is Nothing Then
                _IsoChrones.AddLast(CurIsoChrone)
                For Each rp As clsrouteinfopoints In CurIsoChrone.PointSet.Values
                    If Not rp Is Nothing Then
                        If rp.ImproveDTF(P) Then
                            P = rp
                            CurDTF = P.DTF

                        End If
                    End If
                Next
            End If
            'Console.WriteLine("Iso Added" & Now.Subtract(LoopStart).ToString)


            If Not P Is Nothing Then
                _CurBest = P
                'ElseIf CurDTF < 5 Then
                '    RouteComplete = True
            End If
#If DBG_ISO = 1 Then
            If _IsoChrones.Count > MAX_DBG_ISO OrElse CurIsoChrone Is Nothing OrElse CurIsoChrone.PointSet.Count = 0 Then
#Else
            If CurIsoChrone Is Nothing OrElse CurIsoChrone.PointSet.Count = 0 Then
#End If

                RouteComplete = True
            ElseIf P IsNot Nothing AndAlso P.DTF < Dist * 0.01 Then
                RouteComplete = True
                'ElseIf Not CurIsoChrone.Data(Loxo) Is Nothing Then
                '    TC.EndPoint = CurIsoChrone.Data(Loxo).P
                '    If TC.SurfaceDistance >= Dist Then
                '        RouteComplete = True

                '    End If
            End If
            If Now.Subtract(LastUpdate).TotalSeconds > 2 Then
                RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
                LastUpdate = Now
            End If
            'Console.WriteLine("TmpRouted" & Now.Subtract(LoopStart).TotalMilliseconds)
            'Console.WriteLine("FromStart" & Now.Subtract(Start).ToString)
#If DBG_ISO = 1 Then
            'MessageBox.Show("Iso")
#End If

        End While
        RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
        RaiseEvent Log("Isochrone routing completed in  " & Now.Subtract(Start).ToString)
        RaiseEvent RouteComplete()
    End Sub

    Private Function ReachPoint(ByVal Start As clsrouteinfopoints, ByVal Cap As Double, ByVal Duration As TimeSpan, IgnoreCollisions As Boolean) As clsrouteinfopoints

        Static CumDuration As Long = 0
        Static CumWait As Long = 0
        Static HitCount As Long = 1
        Static LoopCount As Long = 0
        Dim DebugTimingStartTick As DateTime = Now

        Debug.Assert(Duration.Ticks > 0)

        Try
            If Start Is Nothing OrElse Start.P Is Nothing Then
                Return Nothing
            End If

            Dim RetPoint As clsrouteinfopoints = Nothing
            Dim TC As New TravelCalculator
            Dim Speed As Double
            Dim MI As MeteoInfo = Nothing
            Dim i As Long
            Dim TotalDist As Double = 0
            Static PrevDist As Double = 0
            Dim MinWindAngle As Double = 0
            Dim MaxWindAngle As Double = 0
            Dim StartTicks As DateTime = Start.T
            Dim CurDate As DateTime = StartTicks

            TC.StartPoint = Start.P

            'normalize cap
            Cap = Cap Mod 360

            'Static ReachPointCounts As Long = 0
            'Dim LoopCount As Integer
            'Static LoopMs As Double = 0
            'Dim LoopStart As DateTime = Now
            If Not _RouterPrefs.FastRouteShortMeteo Then
                MI = Nothing
                For i = CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute) To Duration.Ticks Step CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute)
                    MI = Nothing
                    Dim WaitStart As DateTime = Now
                    Do

                        MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.N_Lon_Deg, TC.StartPoint.Lat_Deg, False)

                    Loop While MI Is Nothing And Not _CancelRequested

                    CumWait += Now.Subtract(WaitStart).Ticks
                    LoopCount += 1
                    If _CancelRequested Then
                        Return Nothing
                    End If
                    Dim Alpha As Double = WindAngle(Cap, MI.Dir)
                    If _RouterPrefs.FastRouteShortPolar Then
                        _SailManager.GetCornerAngles(MI.Strength, MinWindAngle, MaxWindAngle)
                        If Alpha < MinWindAngle OrElse Alpha > MaxWindAngle Then
                            Return Nothing
                        End If
                    End If

                    Speed = _SailManager.GetSpeed(_BoatType, clsSailManager.EnumSail.OneSail, Alpha, MI.Strength)
                    TotalDist += Speed / 60 * RouteurModel.VacationMinutes
                    TC.StartPoint = TC.ReachDistanceLoxo(Speed / 60 * RouteurModel.VacationMinutes, Cap)
                    If TC.StartPoint Is Nothing Then
                        'if point cannot be reached abort...
                        Return Nothing
                    End If
                    CurDate = StartTicks.AddTicks(i)
                    '    LoopCount += 1
                    'TC.StartPoint = TC.EndPoint

                Next
                If MI Is Nothing Then
                    Dim i2 As Integer = 0
                End If

            Else
                CurDate = Start.T
                MI = Nothing
                Dim WaitStart As DateTime = Now
                While MI Is Nothing And Not _CancelRequested
                    MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.N_Lon_Deg, TC.StartPoint.Lat_Deg, False)
                    If MI Is Nothing Then
                        System.Threading.Thread.Sleep(GribManager.METEO_SLEEP_DELAY)
                    End If
                End While
                CumWait += Now.Subtract(WaitStart).Ticks
                LoopCount += 1
                If _CancelRequested Then
                    Return Nothing
                End If

                If _RouterPrefs.FastRouteShortPolar Then
                    _SailManager.GetCornerAngles(MI.Strength, MinWindAngle, MaxWindAngle)
                    Dim Alpha As Double = WindAngle(Cap, MI.Dir)
                    If Alpha < MinWindAngle OrElse Alpha > MaxWindAngle Then
                        Return Nothing
                    End If
                End If

                Speed = _SailManager.GetSpeed(_BoatType, clsSailManager.EnumSail.OneSail, WindAngle(Cap, MI.Dir), MI.Strength)
                TotalDist = Speed * Duration.TotalHours
                TC.StartPoint = TC.ReachDistanceLoxo(TotalDist, Cap)
                CurDate = CurDate.Add(Duration)

            End If
            TC.EndPoint = TC.StartPoint
            TC.StartPoint = Start.P

            If Math.Abs(TC.EndPoint.Lon - TC.StartPoint.Lon) > Math.PI AndAlso TC.EndPoint.Lon * TC.StartPoint.Lon < 0 Then
                If TC.StartPoint.Lon < 0 Then
                    TC.EndPoint.Lon += (Math.Ceiling(TC.StartPoint.Lon / 2 / Math.PI) - 1) * 2 * Math.PI
                Else
                    TC.EndPoint.Lon += (1 + Math.Floor(TC.StartPoint.Lon / 2 / Math.PI)) * 2 * Math.PI
                End If

            End If


            'ReachPointCounts += 1
            'If PrevDist <> 0 AndAlso TotalDist / PrevDist > 20 Then
            '    Dim br As Integer = 0
            'ElseIf PrevDist <> 0 Then
            '    PrevDist = TotalDist

            'End If
            'If TotalDist < 0.0001 Then
            '    TotalDist = 0.0001
            'End If
            'TC.EndPoint = TC.ReachDistance(TotalDist, Cap)
            If RouteurModel.NoObstacle OrElse IgnoreCollisions OrElse (TC.SurfaceDistance > 0 AndAlso Not _DB.IntersectMapSegment(TC.StartPoint, TC.EndPoint, GSHHS_Reader._Tree)) Then
                'If CheckSegmentValid(TC) Then
                RetPoint = New clsrouteinfopoints
                With RetPoint
                    .P = New Coords(TC.EndPoint)
                    .T = CurDate
                    .Speed = Speed
                    .WindStrength = MI.Strength
                    .WindDir = MI.Dir
                    .Loch = TotalDist
                    .LochFromStart = .Loch + Start.LochFromStart
                    TC.StartPoint = _DestPoint
                    .DTF = TC.SurfaceDistance
                    .From = Start
                    .Cap = Cap
                End With

            Else
                Dim bp As Integer = 0

            End If

            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing
            'LoopMs += Now.Subtract(LoopStart).TotalMilliseconds
            'If ReachPointCounts Mod 500 = 0 Then
            'Console.WriteLine("ReachPointLoops " & LoopCount & " in " & LoopMs / 500 & " for " & Duration.ToString)
            'LoopMs = 0
            'End If
            Return RetPoint
        Finally
            'CumDuration += Now.Subtract(DebugTimingStartTick).Ticks
            'Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPtAvgMS) = CumDuration / HitCount / TimeSpan.TicksPerMillisecond
            'Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPtCumMS) = CumDuration / TimeSpan.TicksPerMillisecond
            'Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPtMetWaitMS) = CumWait / HitCount / TimeSpan.TicksPerMillisecond
            'Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPtAvgLpCnt) = LoopCount / HitCount
            'HitCount += 1
        End Try

    End Function

    Public Function RouteToPoint(ByVal C As Coords, ByVal PixelSize As Double) As ObservableCollection(Of VLM_Router.clsrouteinfopoints)

        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = C
        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim RP As clsrouteinfopoints = Nothing
        Dim DistToPoint As Double = Double.MaxValue
        Try
            For Each iso As IsoChrone In _IsoChrones
                Dim index As Double = iso.IndexFromAngle(Loxo)

                If iso.PointSet.ContainsKey(index) AndAlso Not iso.PointSet(index) Is Nothing Then
                    TC.StartPoint = iso.PointSet(index).P
                    If TC.SurfaceDistance < DistToPoint Then
                        RP = iso.PointSet(index)
                        DistToPoint = TC.SurfaceDistance

                    End If
                End If
            Next

        Catch ex As Exception
            RaiseEvent Log("Route to point exception :" & ex.Message)
        End Try

        If RP Is Nothing Then
            Return Nothing
        Else
            Return RouteToPoint(RP)
        End If
    End Function

    Public Function RouteToPoint(ByVal c As clsrouteinfopoints) As ObservableCollection(Of VLM_Router.clsrouteinfopoints)

        Dim RetRoute As New ObservableCollection(Of VLM_Router.clsrouteinfopoints)
        Try
            If c Is Nothing Then
                Return RetRoute
            End If

            SyncLock c
                Dim P As clsrouteinfopoints = c

                Dim CurCap As Double
                Dim CurSail As clsSailManager.EnumSail

                If Not P Is Nothing Then
                    If Not P.P Is Nothing Then
                        CurCap = CDbl(P.Cap.ToString("0.0"))
                        CurSail = P.Sail
                    End If
                End If

                RetRoute.Insert(0, P)
                While P.From IsNot Nothing And RetRoute.Count < 1000
                    'R = New VOR_Router.clsrouteinfopoints With {.P = New Coords(P)}

                    If CDbl(P.Cap.ToString("0.0")) <> CurCap Or P.Sail <> CurSail Then
                        RetRoute.Insert(0, P)
                        CurCap = CDbl(P.Cap.ToString("0.0"))
                        CurSail = P.Sail
                    End If
                    P = P.From
                End While
                'Add route 1st point
                RetRoute.Insert(0, P)
            End SyncLock
        Catch ex As Exception

            Dim i As Integer = 0
            RaiseEvent Log("Arrgh exception " & ex.Message & " " & ex.StackTrace)
        End Try

        Return RetRoute
    End Function

    Public ReadOnly Property IsoChrones() As LinkedList(Of IsoChrone)
        Get
            Return _IsoChrones
        End Get
    End Property
    Public ReadOnly Property Route() As ObservableCollection(Of VLM_Router.clsrouteinfopoints)
        Get

            Return RouteToPoint(_CurBest)

        End Get
    End Property

    'Private Sub SpinLockEnter(Iso As IsoChrone, Index As Integer)

    '    Dim GotLock As Boolean = False

    '    Do
    '        Iso.Locks(Index).Enter(GotLock)
    '        If Not GotLock Then
    '            Thread.Sleep(1)
    '        End If

    '    Loop Until GotLock

    'End Sub

    'Private Sub SpinLockExit(Iso As IsoChrone, Index As Integer)
    '    If Iso.Locks(Index).IsHeldByCurrentThread Then
    '        Iso.Locks(Index).Exit()
    '    End If
    'End Sub



    Public Sub StartIsoRoute(ByVal From As Coords, ByVal WP1 As Coords, ByVal WP2 As Coords, ByVal StartDate As Date, Prefs As RacePrefs)

        If _IsoRouteThread Is Nothing Then
            _IsoRouteThread = New Thread(AddressOf IsoRouteThread)
            _IsoRouteThread.Priority = ThreadPriority.BelowNormal
            _CancelRequested = False
            _StartPoint = New VLM_Router.clsrouteinfopoints
            _TC.StartPoint = From
            _TC.EndPoint = WP1
            _RouterPrefs = Prefs


            With _StartPoint
                .P = New Coords(From)
                .T = StartDate
                If WP2 IsNot Nothing Then
                    _DestPoint = GSHHS_Reader.PointToSegmentIntersect(.P, WP1, WP2)
                Else
                    _DestPoint = WP1
                End If
                _TC.EndPoint = _DestPoint
                .DTF = _TC.SurfaceDistance
                .Loch = 0
                .LochFromStart = 0
                .WindDir = -1 'mi.Dir
                .WindStrength = -1 ' mi.Strength
            End With

            _IsoChrones.Clear()
            _IsoRouteThread.Start()
        Else
            _CancelRequested = True
        End If

    End Sub

    Public Sub StopRoute()
        _CancelRequested = True
    End Sub

    Public ReadOnly Property CurBest() As clsrouteinfopoints
        Get
            Return _CurBest
        End Get
    End Property

End Class
