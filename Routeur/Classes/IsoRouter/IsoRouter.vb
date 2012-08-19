Imports System.Threading
Imports Routeur.VLM_Router
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Threading.Tasks

Public Class IsoRouter
    Implements INotifyPropertyChanged
    
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event Log(ByVal msg As String)
    Public Event RouteComplete()

    Private _SearchAngle As Double
    Private _AngleStep As Double
    Private _IsoStep As TimeSpan
    Private _IsoStep_24 As TimeSpan
    Private _IsoStep_48 As TimeSpan
    Private _EllipseExt As Double = 1.3
    Private _IsoChrones As New LinkedList(Of IsoChrone)

    Private _IsoRouteThread As Thread
    Private _CancelRequested As Boolean

    Private _StartPoint As clsrouteinfopoints
    Private _DestPoint1 As Coords
    Private _DestPoint2 As Coords
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
    
#Const DBG_ISO = 1

    Public Sub New(ByVal BoatType As String, ByVal SailManager As clsSailManager, ByVal Meteo As GribManager, ByVal AngleStep As Double, ByVal SearchAngle As Double, ByVal IsoStep As TimeSpan, ByVal IsoStep_24 As TimeSpan, ByVal IsoStep_48 As TimeSpan, MapLevel As Integer, EllipseExcent As Double)
        _AngleStep = AngleStep
        _IsoStep = IsoStep
        _IsoStep_24 = IsoStep_24
        _IsoStep_48 = IsoStep_48
        _SearchAngle = SearchAngle
        _EllipseExt = EllipseExcent
        _Meteo = Meteo
        _SailManager = SailManager
        _BoatType = BoatType
        _DB = New DBWrapper()
        _DB.MapLevel = MapLevel
        _MapLevel = MapLevel
    End Sub


    Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone) As IsoChrone
        'Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone) As IsoChrone

        Dim RetIsoChrone As IsoChrone = Nothing
        Dim P1 As clsrouteinfopoints
        Dim Index1 As Integer
        Dim OldP1 As clsrouteinfopoints
        Dim ReachPointDuration As Long = 0
        Dim ReachPointCount As Long = 0
        Dim MaxEllipseDist As Double


        If Iso Is Nothing Then
            Dim TcInit As New TravelCalculator With {.StartPoint = _StartPoint.P}
            'Special Case for startpoint
            RetIsoChrone = New IsoChrone(_AngleStep)
            For alpha = 0 To 360 - _AngleStep Step _AngleStep
                P1 = ReachPoint(_StartPoint, alpha, _IsoStep)
                If Not P1 Is Nothing Then
                    TcInit.EndPoint = P1.P
                    P1.DistFromPos = TcInit.SurfaceDistance
                    P1.Cap = alpha
                    Index1 = RetIsoChrone.IndexFromAngle(alpha)
                    OldP1 = RetIsoChrone.Data(Index1)
                    If OldP1 Is Nothing OrElse P1.DTF < OldP1.DTF Then

                        RetIsoChrone.Data(Index1) = P1

                    End If
                End If
            Next

        Else
            
            Dim StartIndex As Integer = 0
            Dim CurStep As TimeSpan
            Dim FirstIndex As Integer = -1
            'Dim _IsoSegs As New List(Of MapSegment)
            Dim PrevPoint As Coords = Nothing
            Dim CurPoint As Coords = Nothing
            Dim MaxDist As Double = 0
            Dim MaxSpeed As Double = 0
            Dim SumSpeed As Double = 0
            Dim NbSpeed As Double = 0

            For i = 0 To Iso.Data.Length - 1
                If Iso.Data(i) IsNot Nothing Then
                    MaxDist = Math.Max(MaxDist, Iso.Data(i).DistFromPos)
                    MaxSpeed = Math.Max(MaxSpeed, Iso.Data(i).Speed)
                    SumSpeed += Iso.Data(i).Speed
                    NbSpeed += 1
                End If
            Next

            Dim AvgSpeed As Double = SumSpeed / NbSpeed

            For StartIndex = 0 To Iso.Data.Length - 1
                If Not Iso.Data(StartIndex) Is Nothing Then
                    Dim Ticks As Long = Iso.Data(StartIndex).T.Subtract(_StartPoint.T).Ticks

                   
                    If RetIsoChrone Is Nothing Then
                        Dim VacCount As Long = CLng(Ticks / (RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute))
                        Dim DFP As Double = Iso.Data(StartIndex).DistFromPos + 1
                        'TODO Add param for min angle
                        Dim AStep As Double = Math.Max(0.1, 5 - 1 * Math.Log10(DFP)) 'Math.min((36 + 1.05 * VacCount),
                        RetIsoChrone = New IsoChrone(AStep)
                    End If

                    'If Ticks > TimeSpan.TicksPerHour * 48 Then
                    '    CurStep = _IsoStep_48
                    '    'AStep = _AngleStep '/ 4
                    'ElseIf Ticks > TimeSpan.TicksPerHour * 24 Then
                    '    CurStep = _IsoStep_24
                    '    'AStep = _AngleStep '/ 2
                    'Else
                    '    'AStep = _AngleStep
                    '    CurStep = _IsoStep
                    'End If

                    'Dim NbMinutes As Double = New TimeSpan(CLng(2 * Math.PI * MaxDist / RetIsoChrone.Data.Count / MaxSpeed * 0.6 * TimeSpan.TicksPerHour)).TotalMinutes
                    Dim NbMinutes As Double = New TimeSpan(CLng(2 * 2 * Math.PI * MaxDist / RetIsoChrone.Data.Count / MaxSpeed * TimeSpan.TicksPerHour)).TotalMinutes
                    NbMinutes = Math.Ceiling(NbMinutes / RouteurModel.VacationMinutes) * RouteurModel.VacationMinutes
                    CurStep = New TimeSpan(0, CInt(NbMinutes), 0)
#If DBG_ISO = 1 Then
                    Console.WriteLine("Iso Starting for " & RetIsoChrone.Data.Length - 1 & " point, duration:" & CurStep.ToString & " AngleStep : " & _AngleStep)
#End If

                    Exit For
                End If
            Next

            
            Dim tc2 As New TravelCalculator
            tc2.StartPoint = _StartPoint.P
            tc2.EndPoint = GSHHS_Reader.PointToSegmentIntersect(_StartPoint.P, _DestPoint1, _DestPoint2)
            MaxEllipseDist = tc2.SurfaceDistance * _EllipseExt

            Parallel.For(StartIndex, Iso.Data.Length,
                         Sub(Pindex As Integer)

                             Dim tcfn1 As New TravelCalculator
                             Dim CurDest As Coords
                             Dim rp As clsrouteinfopoints

                             If Not Iso.Data(Pindex) Is Nothing Then
                                 rp = Iso.Data(Pindex)

                                 Dim Ortho As Double
                                 tcfn1.StartPoint = rp.P
                                 If _DestPoint2 IsNot Nothing Then
                                     tcfn1.EndPoint = GSHHS_Reader.PointToSegmentIntersect(rp.P, _DestPoint1, _DestPoint2)
                                 Else
                                     tcfn1.EndPoint = _DestPoint1
                                 End If
                                 CurDest = tcfn1.EndPoint
                                 Ortho = tcfn1.OrthoCourse_Deg
                                 tcfn1.EndPoint = _StartPoint.P

                                 Dim TcAngleTrim As New TravelCalculator()
                                 Dim MinSearch As Double = 180
                                 Dim MaxSearch As Double = 180
                                 Dim PrevIndex As Integer = (Pindex + Iso.Data.Length - 1) Mod Iso.Data.Length
                                 Dim NextIndex As Integer = (Pindex + 1) Mod Iso.Data.Length
                                 If Iso.Data(PrevIndex) IsNot Nothing Then
                                     TcAngleTrim.EndPoint = Iso.Data(PrevIndex).P
                                     TcAngleTrim.StartPoint = rp.P
                                     MinSearch = WindAngleWithSign(TcAngleTrim.LoxoCourse_Deg, Ortho)
                                 End If
                                 If Iso.Data(NextIndex) IsNot Nothing Then
                                     TcAngleTrim.StartPoint = rp.P
                                     TcAngleTrim.EndPoint = Iso.Data(NextIndex).P
                                     MaxSearch = WindAngleWithSign(TcAngleTrim.LoxoCourse_Deg, Ortho)
                                 End If

                                 Dim MinAngle As Double = Ortho + MinSearch
                                 Dim maxAngle As Double = Ortho + MaxSearch

                                 If MinAngle > maxAngle Then
                                     maxAngle += 360
                                 End If

                                 tcfn1.StartPoint = _StartPoint.P
                                 'MinAngle = 0
                                 'MaxAngle = 360
                                 Dim StepCount As Integer = CInt(Math.Floor((maxAngle - MinAngle) / _AngleStep) + 1)
#If DBG_ISO = 1 Then
                                 'Console.WriteLine("StepCount " & StepCount & " @ " & Pindex)
#End If
                                 Parallel.For(0, StepCount,
                                 Sub(AlphaIndex As Integer)

                                     Dim alpha As Double = (MinAngle + _AngleStep * AlphaIndex) Mod 360
                                     Dim P As clsrouteinfopoints
                                     Dim tc As New TravelCalculator
                                     Dim Index As Integer
                                     Dim OldP As clsrouteinfopoints

                                     'If WindAngle(alpha, rp.Cap) > 140 Then
                                     '    Return
                                     'End If

                                     P = ReachPoint(rp, alpha, CurStep)

                                     If Not P Is Nothing Then

                                         Dim IntersectPrevIso As Boolean = False

                                         If Not IntersectPrevIso AndAlso (RouteurModel.NoObstacle OrElse Not _DB.IntersectMapSegment(rp.P, P.P, GSHHS_Reader._Tree)) Then

                                             tc.StartPoint = _StartPoint.P
                                             tc.EndPoint = P.P
                                             P.DistFromPos = tc.SurfaceDistance
                                             P.CapFromPos = tc.LoxoCourse_Deg
                                             P.Cap = alpha
                                             'alpha2 = P.CapFromPos 'tc.LoxoCourse_Deg
                                             tc.StartPoint = rp.P
                                             P.Speed = tc.SurfaceDistance / CurStep.TotalHours

                                             Index = RetIsoChrone.IndexFromAngle(P.CapFromPos)
                                             'Dim ANew As Double = RetIsoChrone.AngleFromIndex(Index)
                                             'Debug.Assert(Math.Abs(ANew - alpha2) < RetIsoChrone.Data.Length / 630)
                                             SpinLockEnter(RetIsoChrone, Index)
                                             'SyncLock RetIsoChrone.Locks(Index)
                                             OldP = RetIsoChrone.Data(Index)
                                             If OldP Is Nothing OrElse P.Improve(OldP, _DTFRatio, _StartPoint, _Meteo, _DestPoint1, _DestPoint2, _RouterPrefs.RouteurMode) Then
                                                 Dim IgnorePoint As Boolean = False
                                                 If _IsoChrones.Count > 3 Then
                                                     For i = _IsoChrones.Count - 3 To Math.Max(0, _IsoChrones.Count - 100) Step -1
                                                         Dim PrevP = _IsoChrones(i).Data(_IsoChrones(i).IndexFromAngle(P.CapFromPos))
                                                         If PrevP IsNot Nothing AndAlso PrevP.DistFromPos > P.DistFromPos Then
                                                             IgnorePoint = True
                                                             Exit For
                                                         End If

                                                     Next
                                                 End If
                                                 If Not IgnorePoint Then
                                                     RetIsoChrone.Data(Index) = P
                                                 End If

                                             End If

                                             'End SyncLock
                                             SpinLockExit(RetIsoChrone, Index)
                                         End If

                                     End If

                                     tc.StartPoint = Nothing
                                     tc.EndPoint = Nothing
                                     tc = Nothing
                                 End Sub)


                             End If

                             tcfn1.EndPoint = Nothing
                             tcfn1.StartPoint = Nothing
                             tcfn1 = Nothing
                         End Sub)

            'Console.WriteLine("Iso complete " & Now.Subtract(IsoStart).ToString)


            'Clean up bad points
            If RetIsoChrone IsNot Nothing Then
                Dim CurDest As Coords
                If _DestPoint2 IsNot Nothing Then
                    CurDest = GSHHS_Reader.PointToSegmentIntersect(_StartPoint.P, _DestPoint1, _DestPoint2)
                Else
                    CurDest = _DestPoint1
                End If

                For Index1 = 0 To RetIsoChrone.Data.Count - 1
                    If RetIsoChrone.Data(Index1) IsNot Nothing Then
                        tc2.StartPoint = RetIsoChrone.Data(Index1).P
                        tc2.EndPoint = _StartPoint.P
                        Dim EllipseDist As Double = tc2.SurfaceDistance
                        tc2.EndPoint = CurDest
                        EllipseDist += tc2.SurfaceDistance

                        If EllipseDist > MaxEllipseDist Then
                            RetIsoChrone.Data(Index1) = Nothing
                        End If
                    End If
                Next
                'Console.WriteLine("Iso complete2 " & Now.Subtract(IsoStart).ToString)
            End If

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
        TC.EndPoint = _DestPoint1
        Dim CurDTF As Double = Double.MaxValue
        Dim LastUpdate As DateTime = Now

        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim Dist As Double = TC.SurfaceDistance * 0.995

        RaiseEvent Log("Isochrone router started at " & Start)
        While Not RouteComplete AndAlso Not _CancelRequested

            P = Nothing
            Dim LoopStart As DateTime = Now

            Dim isostart As DateTime = Now
            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            Dim IsoDuratoin As Double = Now.Subtract(isostart).TotalMilliseconds

#If DBG_ISO = 1 Then
            If CurIsoChrone IsNot Nothing Then
                Console.WriteLine("Isochrone computed in " & IsoDuratoin & " rate : " & IsoDuratoin / CurIsoChrone.Data.Length)
            End If
            'Console.WriteLine("IsoDone" & Now.Subtract(LoopStart).TotalMilliseconds)
#End If
            'CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            P = _CurBest
            If Not CurIsoChrone Is Nothing Then
                _IsoChrones.AddLast(CurIsoChrone)
                For Each rp As clsrouteinfopoints In CurIsoChrone.Data
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

            If CurIsoChrone Is Nothing Then
                RouteComplete = True
            ElseIf P IsNot Nothing AndAlso P.DTF < Dist * 0.01 Then
                RouteComplete = True
            ElseIf Not CurIsoChrone.Data(Loxo) Is Nothing Then
                TC.EndPoint = CurIsoChrone.Data(Loxo).P
                If TC.SurfaceDistance >= Dist Then
                    RouteComplete = True

                End If
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

    Private Function ReachPoint(ByVal Start As clsrouteinfopoints, ByVal Cap As Double, ByVal Duration As TimeSpan) As clsrouteinfopoints

        Static CumDuration As Long = 0
        Static CumWait As Long = 0
        Static HitCount As Long = 1
        Static LoopCount As Long = 0
        Dim StartTick As DateTime = Now

        Try
            If Start Is Nothing OrElse Start.P Is Nothing Then
                Return Nothing
            End If

            Dim RetPoint As clsrouteinfopoints = Nothing
            Dim TC As New TravelCalculator
            Dim Speed As Double
            Dim MI As MeteoInfo = Nothing
            Dim i As Long
            Dim CurDate As DateTime
            Dim TotalDist As Double = 0
            Static PrevDist As Double = 0
            Dim MinWindAngle As Double = 0
            Dim MaxWindAngle As Double = 0
            Dim StartTicks As DateTime = Start.T

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
                    CurDate = StartTicks.AddTicks(i)
                    MI = Nothing
                    Dim WaitStart As DateTime = Now
                    While MI Is Nothing And Not _CancelRequested
                        MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.N_Lon_Deg, TC.StartPoint.Lat_Deg, False)
                        'If MI Is Nothing Then
                        'System.Threading.Thread.Sleep(GribManager.METEO_SLEEP_DELAY)
                        'End If
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
                    TotalDist += Speed / 60 * RouteurModel.VacationMinutes
                    TC.StartPoint = TC.ReachDistance(Speed / 60 * RouteurModel.VacationMinutes, Cap)
                    '    LoopCount += 1
                    'TC.StartPoint = TC.EndPoint

                Next
            Else
                CurDate = Start.T
                MI = Nothing
                While MI Is Nothing And Not _CancelRequested
                    MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.N_Lon_Deg, TC.StartPoint.Lat_Deg, True)
                    If MI Is Nothing Then
                        System.Threading.Thread.Sleep(50)
                    End If
                End While
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
                TC.StartPoint = TC.ReachDistance(TotalDist, Cap)
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
            If RouteurModel.NoObstacle OrElse (TC.SurfaceDistance > 0 AndAlso Not _DB.IntersectMapSegment(TC.StartPoint, TC.EndPoint, GSHHS_Reader._Tree)) Then
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
                    If _DestPoint2 Is Nothing Then
                        TC.StartPoint = _DestPoint1
                        .DTF = TC.SurfaceDistance
                    Else
                        TC.StartPoint = GSHHS_Reader.PointToSegmentIntersect(.P, _DestPoint1, _DestPoint2)
                        .DTF = TC.SurfaceDistance
                    End If
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
            CumDuration += Now.Subtract(StartTick).Ticks
            Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPointMS) = CumDuration / HitCount / TimeSpan.TicksPerMillisecond
            Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPointMeteoWaitMS) = CumWait / HitCount / TimeSpan.TicksPerMillisecond
            Routeur.Stats.SetStatValue(Stats.StatID.ISO_ReachPointAvgLoopCount) = LoopCount / HitCount
            HitCount += 1
        End Try

    End Function

    Public Function RouteToPoint(ByVal C As Coords, ByVal PixelSize As Double) As ObservableCollection(Of VLM_Router.clsrouteinfopoints)

        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = C
        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim RP As clsrouteinfopoints = Nothing
        Try
            For Each iso As IsoChrone In _IsoChrones
                Dim index As Integer = iso.IndexFromAngle(Loxo)

                If Not iso.Data(index) Is Nothing Then
                    TC.StartPoint = iso.Data(index).P
                    If TC.SurfaceDistance < 4 * PixelSize Then
                        RP = iso.Data(index)
                        Exit For
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

    Private Sub SpinLockEnter(Iso As IsoChrone, Index As Integer)

        Dim GotLock As Boolean = False
        
        Do
            Iso.Locks(Index).Enter(GotLock)
            If Not GotLock Then
                Thread.Sleep(1)
            End If

        Loop Until GotLock
        
    End Sub

    Private Sub SpinLockExit(Iso As IsoChrone, Index As Integer)
        If Iso.Locks(Index).IsHeldByCurrentThread Then
            Iso.Locks(Index).Exit()
        End If
    End Sub



    Public Sub StartIsoRoute(ByVal From As Coords, ByVal WP1 As Coords, ByVal WP2 As Coords, ByVal StartDate As Date, Prefs As RacePrefs)

        If _IsoRouteThread Is Nothing Then
            _IsoRouteThread = New Thread(AddressOf IsoRouteThread)
            _IsoRouteThread.Priority = ThreadPriority.BelowNormal
            _CancelRequested = False
            _StartPoint = New VLM_Router.clsrouteinfopoints
            _TC.StartPoint = From
            _TC.EndPoint = WP1
            _RouterPrefs = Prefs

            Dim mi As MeteoInfo = Nothing
            While mi Is Nothing
                mi = _Meteo.GetMeteoToDate(StartDate, From.N_Lon_Deg, From.Lat_Deg, False, False)
                System.Threading.Thread.Sleep(250)
            End While

            With _StartPoint
                .P = New Coords(From)
                .T = StartDate
                If WP2 IsNot Nothing Then
                    _TC.EndPoint = GSHHS_Reader.PointToSegmentIntersect(.P, WP1, WP2)
                End If
                .DTF = _TC.SurfaceDistance
                .Loch = 0
                .LochFromStart = 0
                .WindDir = mi.Dir
                .WindStrength = mi.Strength
            End With

            _DestPoint1 = New Coords(WP1)
            If WP2 IsNot Nothing Then
                _DestPoint2 = New Coords(WP2)
            Else
                _DestPoint2 = Nothing
            End If

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
