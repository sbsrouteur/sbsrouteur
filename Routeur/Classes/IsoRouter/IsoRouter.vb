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
    Private _FastRoute As Boolean = False
    Private IsoRouterLock As New Object
    Private _DB As DBWrapper
    Private _MapLevel As Integer

#Const DBG_ISO = 1

    Public Sub New(ByVal BoatType As String, ByVal SailManager As clsSailManager, ByVal Meteo As GribManager, ByVal AngleStep As Double, ByVal SearchAngle As Double, ByVal IsoStep As TimeSpan, ByVal IsoStep_24 As TimeSpan, ByVal IsoStep_48 As TimeSpan, MapLevel As Integer)
        _AngleStep = AngleStep
        _IsoStep = IsoStep
        _IsoStep_24 = IsoStep_24
        _IsoStep_48 = IsoStep_48
        _SearchAngle = SearchAngle
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


        If Iso Is Nothing Then
            'Special Case for startpoint
            RetIsoChrone = New IsoChrone(_AngleStep)
            For alpha = 0 To 360 - _AngleStep Step _AngleStep
                P1 = ReachPoint(_StartPoint, alpha, _IsoStep)
                If Not P1 Is Nothing Then
                    Index1 = RetIsoChrone.IndexFromAngle(alpha)
                    OldP1 = RetIsoChrone.Data(Index1)
                    If OldP1 Is Nothing OrElse P1.DTF < OldP1.DTF Then
                        'If PrevIso Is Nothing OrElse (Not PrevIso.Data(Index) Is Nothing AndAlso P.DTF <= PrevIso.Data(Index).DTF) Then
                        RetIsoChrone.Data(Index1) = P1
                        'End If
                    End If
                End If
            Next

        Else
            'Dim PIndex As Integer
            
            'Dim IsoStart As DateTime = Now
            'Console.WriteLine("Iso len : " & Iso.Data.Length)
            Dim StartIndex As Integer = 0
            Dim AStep As Double
            Dim CurStep As TimeSpan

            For StartIndex = 0 To Iso.Data.Length - 1
                If Not Iso.Data(StartIndex) Is Nothing Then
                    Dim Ticks As Long = Iso.Data(StartIndex).T.Subtract(_StartPoint.T).Ticks
                    If Ticks > TimeSpan.TicksPerHour * 48 Then
                        CurStep = _IsoStep_48
                        AStep = _AngleStep / 4
                    ElseIf Ticks > TimeSpan.TicksPerHour * 24 Then
                        CurStep = _IsoStep_24
                        AStep = _AngleStep / 2
                    Else
                        AStep = _AngleStep
                        CurStep = _IsoStep
                    End If

                    If RetIsoChrone Is Nothing Then
                        RetIsoChrone = New IsoChrone(_AngleStep)
                    End If
                    Exit For
                End If
            Next

            Dim IsoPoly As New Polygon
            For Each Pt In Iso.Data
                If Pt IsNot Nothing AndAlso Not Pt.P Is Nothing Then
                    IsoPoly.Add(Pt.P)
                End If
            Next

            Dim LP As New LinkedList(Of Polygon)
            LP.AddFirst(IsoPoly)
            Dim tc2 As New TravelCalculator
            tc2.StartPoint = _StartPoint.P


            Parallel.For(StartIndex, Iso.Data.Length, Sub(Pindex As Integer)
                                                          'For PIndex = 0 To Iso.Data.Length - 1
                                                          'Dim P As clsrouteinfopoints
                                                          Dim tcfn1 As New TravelCalculator

                                                          Dim alpha2 As Double
                                                          Dim rp As clsrouteinfopoints

                                                          If Not Iso.Data(Pindex) Is Nothing Then
                                                              rp = Iso.Data(Pindex)
                                                              'Dim Ticks As Long = rp.T.Subtract(_StartPoint.T).Ticks
                                                              'If Ticks > TimeSpan.TicksPerHour * 48 Then
                                                              '    CurStep = _IsoStep_48
                                                              '    AStep = _AngleStep / 4
                                                              'ElseIf Ticks > TimeSpan.TicksPerHour * 24 Then
                                                              '    CurStep = _IsoStep_24
                                                              '    AStep = _AngleStep / 2
                                                              'Else
                                                              '    AStep = _AngleStep
                                                              '    CurStep = _IsoStep
                                                              'End If

                                                              'If RetIsoChrone Is Nothing Then
                                                              '    SyncLock IsoRouterLock
                                                              '        If RetIsoChrone Is Nothing Then
                                                              '            RetIsoChrone = New IsoChrone(_AngleStep)
                                                              '        End If
                                                              '    End SyncLock
                                                              'End If
                                                              Dim Ortho As Double
                                                              tcfn1.StartPoint = rp.P
                                                              If _DestPoint2 IsNot Nothing Then
                                                                  tcfn1.EndPoint = GSHHS_Reader.PointToSegmentIntersect(rp.P, _DestPoint1, _DestPoint2)
                                                              Else
                                                                  tcfn1.EndPoint = _DestPoint1
                                                              End If

                                                              Ortho = tcfn1.OrthoCourse_Deg
                                                              tcfn1.EndPoint = _StartPoint.P

                                                              'Dim RefAlpha As Double = tcfn1.LoxoCourse_Deg
                                                              Dim MinAngle As Double = Ortho - _SearchAngle
                                                              'Index = (PIndex - 1 + Iso.Data.Length) Mod Iso.Data.Length
                                                              'If Not OuterIso.Data(Index) Is Nothing Then
                                                              '    tc.EndPoint = OuterIso.Data(Index).P
                                                              '    Dim Beta As Double = WindAngle(tc.LoxoCourse_Deg, (RefAlpha + 180) Mod 360)
                                                              '    If RefAlpha - Beta > MinAngle Then
                                                              '        MinAngle = RefAlpha - Beta
                                                              '    End If
                                                              'End If

                                                              Dim MaxAngle As Double = Ortho + _SearchAngle
                                                              'Index = (PIndex + 1 + Iso.Data.Length) Mod Iso.Data.Length
                                                              'If Not OuterIso.Data(Index) Is Nothing Then
                                                              '    tc.EndPoint = OuterIso.Data(Index).P
                                                              '    Dim Beta As Double = WindAngle(tc.LoxoCourse_Deg, (RefAlpha + 800) Mod 360)
                                                              '    If RefAlpha + Beta < MaxAngle Then
                                                              '        MaxAngle = RefAlpha + Beta
                                                              '    End If
                                                              'End If

                                                              'If MinAngle > MaxAngle Then
                                                              '    Dim tmp As Double = MinAngle
                                                              '    MinAngle = MaxAngle
                                                              '    MaxAngle = MinAngle
                                                              'End If

                                                              tcfn1.StartPoint = _StartPoint.P
                                                              ' Dim Start As DateTime = Now
                                                              'Dim Ite As Integer = 0
                                                              'Parallel.For(MinAngle, MaxAngle + 3 * _AngleStep,
                                                              Dim StepCount As Integer = CInt(Math.Floor((MaxAngle - MinAngle) / 3 / _AngleStep) + 1)
                                                              ' Console.WriteLine("SC" & StepCount)
                                                              'Console.WriteLine("L" & Loxo & " min " & MinAngle & " max " & MaxAngle)
                                                              Parallel.For(0, StepCount,
                                                              Sub(AlphaIndex As Integer)
                                                                  'For alpha = MinAngle To MaxAngle Step 3 * _AngleStep
                                                                  Dim alpha As Double = MinAngle + 3 * _AngleStep * AlphaIndex
                                                                  Dim P As clsrouteinfopoints
                                                                  Dim tc As New TravelCalculator
                                                                  Dim Index As Integer
                                                                  Dim OldP As clsrouteinfopoints

                                                                  'If WindAngle(Ortho, alpha) < _SearchAngle Then
                                                                  'Dim ReachPointStart As DateTime = Now
                                                                  'Static ReachCount As Integer = 1
                                                                  'Dim start As DateTime = Now
                                                                  P = ReachPoint(rp, alpha, CurStep)
                                                                  'Interlocked.Add(ReachPointDuration, Now.Subtract(start).Ticks)
                                                                  'Interlocked.Increment(ReachPointCount)
                                                                  'If ReachCount Mod 2000 = 0 Then
                                                                  ' ReachCount = 0
                                                                  'Console.WriteLine("Reached in " & Now.Subtract(ReachPointStart).TotalMilliseconds / 2000)
                                                                  'End If
                                                                  'ReachCount += 1
                                                                  If Not P Is Nothing Then
                                                                      SyncLock Iso
                                                                          If Not _DB.IntersectMapSegment(rp.P, P.P, GSHHS_Reader._Tree) Then

                                                                              'If Not GSHHS_Reader.HitTest(P.P, 0, LP, False, True) Then 'tc.SurfaceDistance > 0 Then
                                                                              tc.StartPoint = tcfn1.StartPoint
                                                                              tc.EndPoint = P.P
                                                                              alpha2 = tc.LoxoCourse_Deg

                                                                              Index = RetIsoChrone.IndexFromAngle(alpha2)
                                                                              'If Not OuterIso Is Nothing Then
                                                                              '    PrevIndex = OuterIso.IndexFromAngle(alpha2)


                                                                              'End If
                                                                              SyncLock RetIsoChrone.Locks(Index)
                                                                                  OldP = RetIsoChrone.Data(Index)
                                                                                  If OldP Is Nothing OrElse P.Improve(OldP, _DTFRatio, _StartPoint, _Meteo, _DestPoint1, _DestPoint2) Then
                                                                                      RetIsoChrone.Data(Index) = P

                                                                                      'Update Polygon
                                                                                      Dim NewIsoPoly As New Polygon
                                                                                      Dim IsoAngle As Double
                                                                                      For IsoAngle = 0 To 360 Step 360 / Iso.Data.Length
                                                                                          Dim RetIsoAlpha As Integer = RetIsoChrone.IndexFromAngle(IsoAngle)
                                                                                          If RetIsoChrone.Data(RetIsoAlpha) IsNot Nothing Then
                                                                                              NewIsoPoly.Add(RetIsoChrone.Data(RetIsoAlpha).P)
                                                                                          Else
                                                                                              Dim IsoIndex As Integer = Iso.IndexFromAngle(IsoAngle)
                                                                                              If Iso.Data(IsoIndex) IsNot Nothing Then
                                                                                                  NewIsoPoly.Add(Iso.Data(IsoIndex).P)
                                                                                              End If
                                                                                          End If
                                                                                      Next
                                                                                      LP.Clear()
                                                                                      LP.AddFirst(NewIsoPoly)

                                                                                  End If
                                                                                  'If OuterIso.Data(PrevIndex) Is Nothing OrElse _
                                                                                  '   P.Improve(OuterIso.Data(PrevIndex), _DTFRatio, _StartPoint) Then
                                                                                  '    SyncLock OuterIso.Locks(PrevIndex)
                                                                                  '        OuterIso.Data(PrevIndex) = P
                                                                                  '    End SyncLock

                                                                                  'End If
                                                                              End SyncLock

                                                                          End If
                                                                      End SyncLock
                                                                  End If
                                                                  'Ite += 1
                                                                  tc.StartPoint = Nothing
                                                                  tc.EndPoint = Nothing
                                                                  tc = Nothing
                                                              End Sub)
                                                              'Console.WriteLine("Isochrone in " & Now.Subtract(Start).ToString & " per step " & Now.Subtract(Start).TotalMilliseconds / Ite)


                                                          End If
                                                          'Next
                                                          tcfn1.EndPoint = Nothing
                                                          tcfn1.StartPoint = Nothing
                                                          tcfn1 = Nothing
                                                      End Sub)

            'Console.WriteLine("Iso complete " & Now.Subtract(IsoStart).ToString)
            'Clean up bad points
            'Dim PrevDtf As Double
            'Dim PrevLoxo As Double = 0
            'For alpha = 0 To 360 Step _AngleStep
            '    Index = RetIsoChrone.IndexFromAngle(alpha)
            '    If RetIsoChrone.Data(Index) IsNot Nothing AndAlso RetIsoChrone.Data(Index).DTF <> 0 AndAlso PrevDtf <> 0 Then
            '        Dim R As Double = If(PrevDtf < RetIsoChrone.Data(Index).DTF, PrevDtf / RetIsoChrone.Data(Index).DTF, RetIsoChrone.Data(Index).DTF / PrevDtf)
            '        If R < 0.9 Then
            '            RetIsoChrone.Data(Index) = Nothing
            '        End If

            '    End If

            '    If RetIsoChrone.Data(Index) IsNot Nothing Then
            '        PrevDtf = RetIsoChrone.Data(Index).DTF
            '        PrevIndex = Index
            '    Else
            '        PrevDtf = 0
            '    End If
            'Console.WriteLine("Avg ReachPoint (ms)" & (ReachPointDuration / ReachPointCount / TimeSpan.TicksPerMillisecond).ToString("f3"))
            'Console.WriteLine("Total ReachPoint (ms)" & (ReachPointDuration / TimeSpan.TicksPerMillisecond).ToString("f3") & " on " & ReachPointCount)
            'Next

            'For Index1 = 1 To RetIsoChrone.Data.Count - 1
            '    If RetIsoChrone.Data(Index1 - 1) IsNot Nothing AndAlso RetIsoChrone.Data(Index1) IsNot Nothing Then
            '        tc2.EndPoint = RetIsoChrone.Data(Index1 - 1).P
            '        Dim d1 As Double = tc2.SurfaceDistance
            '        tc2.EndPoint = RetIsoChrone.Data(Index1).P
            '        Dim d2 As Double = tc2.SurfaceDistance

            '        If d2 <> 0 AndAlso d1 / d2 < 0.5 Then
            '            RetIsoChrone.Data(Index1 - 1) = Nothing
            '        ElseIf d1 <> 0 AndAlso d2 / d1 < 0.5 Then
            '            RetIsoChrone.Data(Index1) = Nothing
            '        ElseIf GSHHS_Reader.HitTest(RetIsoChrone.Data(Index1 - 1).P, 0, LP, False, True) Then
            '            RetIsoChrone.Data(Index1 - 1) = Nothing
            '            ''Keep point, check it does not cross the previous ISO Line
            '            'Parallel.For(0, Iso.Data.Count - 1, Sub(IsoIndex As Integer)
            '            '                                        Dim tcRet As New TravelCalculator
            '            '                                        Dim SP1 As clsrouteinfopoints = RetIsoChrone.Data(Index1 - 1)
            '            '                                        Dim SP2 As clsrouteinfopoints = RetIsoChrone.Data(Index1)

            '            '                                        If SP1 IsNot Nothing AndAlso SP2 IsNot Nothing Then
            '            '                                            tcRet.StartPoint = SP1.P
            '            '                                            tcRet.EndPoint = SP2.P
            '            '                                            If Iso.Data(IsoIndex) IsNot Nothing AndAlso Iso.Data(IsoIndex + 1) IsNot Nothing Then
            '            '                                                Dim TcIso As New TravelCalculator
            '            '                                                TcIso.StartPoint = tcRet.StartPoint
            '            '                                                TcIso.EndPoint = Iso.Data(IsoIndex).P

            '            '                                                Dim Alpha1 As Double = WindAngleWithSign(TcIso.LoxoCourse_Deg, tcRet.LoxoCourse_Deg)
            '            '                                                TcIso.EndPoint = Iso.Data(IsoIndex + 1).P
            '            '                                                Dim Alpha2 As Double = WindAngleWithSign(TcIso.LoxoCourse_Deg, tcRet.LoxoCourse_Deg)

            '            '                                                If Alpha1 * Alpha2 <= 0 Then
            '            '                                                    Dim iP1 As Coords = Iso.Data(IsoIndex).P
            '            '                                                    Dim ip2 As Coords = Iso.Data(IsoIndex + 1).P
            '            '                                                    Dim rP1 As Coords = SP1.P
            '            '                                                    Dim rp2 As Coords = SP2.P

            '            '                                                    Dim T As Double = (rP1.Lon - iP1.Lon) / (rP1.Lon - rp2.Lon - iP1.Lon + ip2.Lon)

            '            '                                                    If T >= 0 And T <= 1 Then
            '            '                                                        SyncLock RetIsoChrone.Locks(Index1)
            '            '                                                            RetIsoChrone.Data(Index1) = Nothing
            '            '                                                        End SyncLock
            '            '                                                    End If

            '            '                                                    TcIso.StartPoint = Nothing
            '            '                                                    TcIso.EndPoint = Nothing

            '            '                                                End If

            '            '                                                tcRet.StartPoint = Nothing
            '            '                                                tcRet.EndPoint = Nothing
            '            '                                            End If

            '            '                                        End If
            '            '                                    End Sub)
            '        End If

            '    End If

            'Next
            'Console.WriteLine("Iso complete2 " & Now.Subtract(IsoStart).ToString)


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

            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
#If DBG_ISO = 1 Then
            Console.WriteLine("IsoDone" & Now.Subtract(LoopStart).TotalMilliseconds)
#End If
            'CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            If Not CurIsoChrone Is Nothing Then
                _IsoChrones.AddLast(CurIsoChrone)
                For Each rp As clsrouteinfopoints In CurIsoChrone.Data
                    If Not rp Is Nothing Then
                        If rp.Improve(P, _DTFRatio, _StartPoint, _Meteo, _DestPoint1, _DestPoint2) Then
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
            If Now.Subtract(LastUpdate).TotalSeconds > 3 Then
                RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
            End If
            'Console.WriteLine("TmpRouted" & Now.Subtract(LoopStart).TotalMilliseconds)
            'Console.WriteLine("FromStart" & Now.Subtract(Start).ToString)


        End While
        RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
        RaiseEvent Log("Isochrone routing completed in  " & Now.Subtract(Start).ToString)
        RaiseEvent RouteComplete()
    End Sub

    Private Function ReachPoint(ByVal Start As clsrouteinfopoints, ByVal Cap As Double, ByVal Duration As TimeSpan) As clsrouteinfopoints

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

        TC.StartPoint = Start.P

        'normalize cap
        Cap = Cap Mod 360

        'Static ReachPointCounts As Long = 0
        'Dim LoopCount As Integer
        'Static LoopMs As Double = 0
        'Dim LoopStart As DateTime = Now
        If Not _FastRoute Then
            MI = Nothing
            For i = CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute) To Duration.Ticks Step CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute)
                CurDate = Start.T.AddTicks(i)
                MI = Nothing
                While MI Is Nothing And Not _CancelRequested
                    MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.N_Lon_Deg, TC.StartPoint.Lat_Deg, True)
                    If MI Is Nothing Then
                        System.Threading.Thread.Sleep(GribManager.METEO_SLEEP_DELAY)
                    End If
                End While
                If _CancelRequested Then
                    Return Nothing
                End If

                'If _FastRoute Then
                _SailManager.GetCornerAngles(MI.Strength, MinWindAngle, MaxWindAngle)
                Dim Alpha As Double = Math.Abs(WindAngle(Cap, MI.Dir))
                If Alpha < MinWindAngle OrElse Alpha > MaxWindAngle Then
                    Return Nothing
                End If
                'End If

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
            _SailManager.GetCornerAngles(MI.Strength, MinWindAngle, MaxWindAngle)
            Dim Alpha As Double = WindAngle(Cap, MI.Dir)
            If Alpha < MinWindAngle OrElse Alpha > MaxWindAngle Then
                Return Nothing
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
        If Not _DB.IntersectMapSegment(TC.StartPoint, TC.EndPoint, GSHHS_Reader._Tree) Then
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



    Public Sub StartIsoRoute(ByVal From As Coords, ByVal WP1 As Coords, ByVal WP2 As Coords, ByVal StartDate As Date, ByVal FastRoute As Boolean)

        If _IsoRouteThread Is Nothing Then
            _IsoRouteThread = New Thread(AddressOf IsoRouteThread)
            _CancelRequested = False
            _StartPoint = New VLM_Router.clsrouteinfopoints
            _TC.StartPoint = From
            _TC.EndPoint = WP1
            _FastRoute = FastRoute


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
