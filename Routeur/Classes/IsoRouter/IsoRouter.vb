Imports System.Threading
Imports Routeur.VLM_Router
Imports System.ComponentModel
Imports System.Collections.ObjectModel

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


    Public Sub New(ByVal BoatType As String, ByVal SailManager As clsSailManager, ByVal Meteo As GribManager, ByVal AngleStep As Double, ByVal SearchAngle As Double, ByVal IsoStep As TimeSpan, ByVal IsoStep_24 As TimeSpan, ByVal IsoStep_48 As TimeSpan)
        _AngleStep = AngleStep
        _IsoStep = IsoStep
        _IsoStep_24 = IsoStep_24
        _IsoStep_48 = IsoStep_48
        _SearchAngle = SearchAngle
        _Meteo = Meteo
        _SailManager = SailManager
        _BoatType = BoatType
    End Sub


    Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone, ByVal OuterIso As IsoChrone) As IsoChrone
        'Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone) As IsoChrone

        Dim RetIsoChrone As IsoChrone = Nothing
        Dim alpha As Double
        Dim P As clsrouteinfopoints
        Dim Index As Integer
        Dim PrevIndex As Integer
        Dim OldP As clsrouteinfopoints
        Dim AStep As Double

        If Iso Is Nothing Then
            'Special Case for startpoint
            RetIsoChrone = New IsoChrone(_AngleStep)
            For alpha = 0 To 360 - _AngleStep Step _AngleStep
                P = ReachPoint(_StartPoint, alpha, _IsoStep)
                If Not P Is Nothing Then
                    Index = RetIsoChrone.IndexFromAngle(alpha)
                    OldP = RetIsoChrone.Data(Index)
                    If OldP Is Nothing OrElse P.DTF < OldP.DTF Then
                        'If PrevIso Is Nothing OrElse (Not PrevIso.Data(Index) Is Nothing AndAlso P.DTF <= PrevIso.Data(Index).DTF) Then
                        RetIsoChrone.Data(Index) = P
                        'End If
                    End If
                End If
            Next

        Else
            Dim tc As New TravelCalculator
            Dim alpha2 As Double
            Dim CurStep As TimeSpan
            Dim PIndex As Integer
            Dim rp As clsrouteinfopoints
            Dim MinWindAngle As Double
            Dim MaxWindAngle As Double

            'Dim IsoStart As DateTime = Now
            'Console.WriteLine("Iso len : " & Iso.Data.Length)
            For PIndex = 0 To Iso.Data.Length - 1

                If Not Iso.Data(PIndex) Is Nothing Then
                    rp = Iso.Data(PIndex)
                    Dim Ticks As Long = rp.T.Subtract(_StartPoint.T).Ticks
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

                        'For alpha = 0 To 360 Step _AngleStep
                        '    Index = Iso.IndexFromAngle(alpha)
                        '    Dim Src As clsrouteinfopoints = Iso.Data(Index)
                        '    Index = RetIsoChrone.IndexFromAngle(alpha)
                        '    If Src IsNot Nothing AndAlso RetIsoChrone.Data(Index) Is Nothing Then
                        '        RetIsoChrone.Data(Index) = New clsrouteinfopoints
                        '        With RetIsoChrone.Data(Index)
                        '            .P = New Coords(Src.P)
                        '            .T = Src.T.AddMinutes(CurStep.TotalMinutes)
                        '            .WindStrength = 0
                        '            .WindDir = 0
                        '            .Loch = Src.Loch
                        '            .DTF = Src.DTF
                        '            .From = Src.From
                        '            .Cap = Src.Cap
                        '        End With
                        '    End If

                        'Next
                    End If
                    Dim Loxo As Double
                    tc.StartPoint = rp.P
                    If _DestPoint2 IsNot Nothing Then
                        tc.EndPoint = GSHHS_Reader.PointToSegmentIntersect(rp.P, _DestPoint1, _DestPoint2)
                    Else
                        tc.EndPoint = _DestPoint1
                    End If

                    Loxo = tc.LoxoCourse_Deg
                    tc.EndPoint = _StartPoint.P

                    Dim RefAlpha As Double = (tc.LoxoCourse_Deg + 180) Mod 360
                    Dim MinAngle As Double = Loxo - _SearchAngle
                    'Index = (PIndex - 1 + Iso.Data.Length) Mod Iso.Data.Length
                    'If Not OuterIso.Data(Index) Is Nothing Then
                    '    tc.EndPoint = OuterIso.Data(Index).P
                    '    Dim Beta As Double = WindAngle(tc.LoxoCourse_Deg, (RefAlpha + 180) Mod 360)
                    '    If RefAlpha - Beta > MinAngle Then
                    '        MinAngle = RefAlpha - Beta
                    '    End If
                    'End If

                    Dim MaxAngle As Double = Loxo + _SearchAngle
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

                    tc.StartPoint = _StartPoint.P
                    ' Dim Start As DateTime = Now
                    'Dim Ite As Integer = 0
                    For alpha = MinAngle To MaxAngle Step _AngleStep

                        'If WindAngle(Ortho, alpha) < _SearchAngle Then
                        'Dim ReachPointStart As DateTime = Now
                        'Static ReachCount As Integer = 1
                        P = ReachPoint(rp, alpha, CurStep)
                        'If ReachCount Mod 2000 = 0 Then
                        ' ReachCount = 0
                        'Console.WriteLine("Reached in " & Now.Subtract(ReachPointStart).TotalMilliseconds / 2000)
                        'End If
                        'ReachCount += 1
                        If Not P Is Nothing Then
                            tc.EndPoint = P.P
                            If tc.SurfaceDistance > 0 Then
                                alpha2 = tc.LoxoCourse_Deg

                                Index = RetIsoChrone.IndexFromAngle(alpha2)
                                If Not OuterIso Is Nothing Then
                                    PrevIndex = OuterIso.IndexFromAngle(alpha2)
                                End If
                                OldP = RetIsoChrone.Data(Index)
                                If OldP Is Nothing OrElse P.Improve(OldP, _DTFRatio, _StartPoint) Then
                                    RetIsoChrone.Data(Index) = P
                                End If
                                If OuterIso.Data(PrevIndex) Is Nothing OrElse _
                                   P.Improve(OuterIso.Data(PrevIndex), _DTFRatio, _StartPoint) Then
                                    OuterIso.Data(PrevIndex) = P

                                End If
                            End If

                        End If
                        'Ite += 1
                    Next
                    'Console.WriteLine("Isochrone in " & Now.Subtract(Start).ToString & " per step " & Now.Subtract(Start).TotalMilliseconds / Ite)


                End If
            Next
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

            'Next
            For Index = 1 To RetIsoChrone.Data.Count - 1
                If RetIsoChrone.Data(Index - 1) IsNot Nothing AndAlso RetIsoChrone.Data(Index) IsNot Nothing Then
                    tc.EndPoint = RetIsoChrone.Data(Index - 1).P
                    Dim d1 As Double = tc.SurfaceDistance
                    tc.EndPoint = RetIsoChrone.Data(Index).P
                    Dim d2 As Double = tc.SurfaceDistance

                    If d2 <> 0 AndAlso d1 / d2 < 0.5 Then
                        RetIsoChrone.Data(Index - 1) = Nothing
                    ElseIf d1 <> 0 AndAlso d2 / d1 < 0.5 Then
                        RetIsoChrone.Data(Index) = Nothing

                    End If

                End If

            Next
            'Console.WriteLine("Iso complete2 " & Now.Subtract(IsoStart).ToString)


            tc.EndPoint = Nothing
            tc.StartPoint = Nothing
            tc = Nothing
        End If

        Return RetIsoChrone

    End Function


    Private Sub IsoRouteThread()

        Dim RouteComplete As Boolean = False
        Dim CurIsoChrone As IsoChrone = Nothing
        Dim OuterIsochrone As New IsoChrone(_AngleStep / 4)
        Dim Start As DateTime = Now
        Dim P As clsrouteinfopoints = Nothing
        Dim PrevP As clsrouteinfopoints = Nothing
        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = _DestPoint1
        Dim CurDTF As Double = Double.MaxValue

        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim Dist As Double = TC.SurfaceDistance * 0.995
        
        RaiseEvent Log("Isochrone router started at " & Start)
        While Not RouteComplete AndAlso Not _CancelRequested

            P = Nothing
            Dim LoopStart As DateTime = Now

            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone, OuterIsochrone)
            Console.WriteLine("IsoDone" & Now.Subtract(LoopStart).TotalMilliseconds)
            'CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            If Not CurIsoChrone Is Nothing Then
                _IsoChrones.AddLast(CurIsoChrone)
                For Each rp As clsrouteinfopoints In CurIsoChrone.Data
                    If Not rp Is Nothing Then
                        If rp.Improve(P, _DTFRatio, _StartPoint) Then
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
            RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
            'Console.WriteLine("TmpRouted" & Now.Subtract(LoopStart).TotalMilliseconds)
            'Console.WriteLine("FromStart" & Now.Subtract(Start).ToString)


        End While
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
        If False Then
            MI = Nothing
            For i = CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute) To Duration.Ticks Step CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute)
                CurDate = Start.T.AddTicks(i)
                MI = Nothing
                While MI Is Nothing And Not _CancelRequested
                    MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.Lon_Deg, TC.StartPoint.Lat_Deg, True)
                    If MI Is Nothing Then
                        System.Threading.Thread.Sleep(250)
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
                TotalDist += Speed / 60 * RouteurModel.VacationMinutes
                TC.StartPoint = TC.ReachDistance(Speed / 60 * RouteurModel.VacationMinutes, Cap)
                '    LoopCount += 1
                'TC.StartPoint = TC.EndPoint
            Next
        Else
            CurDate = Start.T
            MI = Nothing
            While MI Is Nothing And Not _CancelRequested
                MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.Lon_Deg, TC.StartPoint.Lat_Deg, True)
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
        If GridRouter.CheckSegmentValid(TC) Then
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



    Public Sub StartIsoRoute(ByVal From As Coords, ByVal WP1 As Coords, ByVal WP2 As Coords, ByVal StartDate As Date)

        If _IsoRouteThread Is Nothing Then
            _IsoRouteThread = New Thread(AddressOf IsoRouteThread)
            _CancelRequested = False
            _StartPoint = New VLM_Router.clsrouteinfopoints
            _TC.StartPoint = From
            _TC.EndPoint = WP1


            Dim mi As MeteoInfo = Nothing
            While mi Is Nothing
                mi = _Meteo.GetMeteoToDate(StartDate, From.Lon_Deg, From.Lat_Deg, False, False)
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
