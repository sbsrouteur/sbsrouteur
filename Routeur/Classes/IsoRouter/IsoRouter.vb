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

        Dim RetIsoChrone As IsoChrone = Nothing
        Dim alpha As Double
        Dim P As clsrouteinfopoints
        Dim Index As Integer
        Dim PrevIndex As Integer
        Dim OldP As clsrouteinfopoints
        Dim AStep As Double
        'Dim tc2 As New TravelCalculator

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


            For Each rp In Iso.Data
                If Not rp Is Nothing Then
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
                        RetIsoChrone = New IsoChrone(AStep)
                    End If
                    Dim Loxo As Double
                    'Set destination on closest point of gate if routing to gate from current point on isochrone
                    tc.StartPoint = rp.P
                    If _DestPoint2 IsNot Nothing Then
                        tc.EndPoint = GSHHS_Reader.PointToSegmentIntersect(rp.P, _DestPoint1, _DestPoint2)
                    Else
                        tc.EndPoint = _DestPoint1
                    End If

                    Loxo = tc.LoxoCourse_Deg

                    'set TC startpoint to routing start point
                    tc.StartPoint = _StartPoint.P
                    'tc2.StartPoint = _StartPoint.P
                    'Loop around loxo to destination
                    For alpha = Loxo - _SearchAngle To Loxo + _SearchAngle Step _AngleStep

                        'If WindAngle(Ortho, alpha) < _SearchAngle Then
                        P = ReachPoint(rp, alpha, CurStep)
                        If Not P Is Nothing Then
                            tc.EndPoint = P.P
                            If tc.SurfaceDistance > 0 Then
                                'Get loxo to point reached from routing start position
                                alpha2 = tc.LoxoCourse_Deg

                                Index = RetIsoChrone.IndexFromAngle(alpha2)
                                If Not OuterIso Is Nothing Then
                                    PrevIndex = OuterIso.IndexFromAngle(alpha2)
                                End If
                                'Get current point from IsoChrone at this index
                                OldP = RetIsoChrone.Data(Index)
                                'If OldP IsNot Nothing Then
                                'tc2.EndPoint = OldP.P
                                'End If
                                'If OldP Is Nothing OrElse (P.DTF < OldP.DTF AndAlso tc2.SurfaceDistance > tc.SurfaceDistance) Then
                                If OldP Is Nothing OrElse P.DTF < OldP.DTF Then
                                    'if new point is closer to dest than previous for this isochrone
                                    RetIsoChrone.Data(Index) = P
                                    If OuterIso Is Nothing OrElse OuterIso.Data(PrevIndex) Is Nothing _
                                        OrElse (Not OuterIso.Data(PrevIndex) Is Nothing AndAlso _
                                                (P.DistFromPos > OuterIso.Data(PrevIndex).DistFromPos OrElse P.DTF <= OuterIso.Data(PrevIndex).DTF)) Then
                                        'if the point improve the outer-isochrone then use it; ignore otherwise
                                        If Not OuterIso Is Nothing Then
                                            'Update outer isochrone
                                            OuterIso.Data(PrevIndex) = P
                                        End If
                                    End If
                                End If
                            End If
                        End If

                        'End If

                    Next
                End If
            Next
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
        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = _DestPoint1
        Dim CurDTF As Double = Double.MaxValue
        Dim CurBestEta As DateTime = New Date(3000, 1, 1)

        Dim Loxo As Double = TC.LoxoCourse_Deg
        Dim Dist As Double = TC.SurfaceDistance

        RaiseEvent Log("Isochrone router started at " & Start)
        While Not RouteComplete AndAlso Not _CancelRequested
            P = Nothing
            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone, OuterIsochrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            If Not CurIsoChrone Is Nothing Then
                _IsoChrones.AddLast(CurIsoChrone)
                For Each rp As clsrouteinfopoints In CurIsoChrone.Data
                    If Not rp Is Nothing Then
                        If (CurDTF < 1 AndAlso P IsNot Nothing AndAlso P.T < CurBestEta) OrElse (CurDTF > rp.DTF) Then
                            P = rp
                            CurDTF = P.DTF
                            CurBestEta = P.T
                        End If
                    End If
                Next
            End If


            If Not P Is Nothing Then
                _CurBest = P
                'ElseIf CurDTF < 5 Then
                '    RouteComplete = True
            End If

            If CurIsoChrone Is Nothing Then
                RouteComplete = True
            ElseIf Not CurIsoChrone.Data(Loxo) Is Nothing Then
                TC.EndPoint = CurIsoChrone.Data(Loxo).P
                If TC.SurfaceDistance >= Dist Then
                    'RouteComplete = True
                End If
            End If
            RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)

        End While
        RaiseEvent Log("Isochrone routing completed in  " & Now.Subtract(Start).ToString)
        RaiseEvent RouteComplete()
    End Sub

    Private Function ReachPoint(ByVal Start As clsrouteinfopoints, ByVal Cap As Double, ByVal Duration As TimeSpan) As clsrouteinfopoints

        If Start Is Nothing OrElse Start.P Is Nothing Then
            Return Nothing
        End If

        Dim RetPoint As New clsrouteinfopoints
        Dim TC As New TravelCalculator
        Dim Speed As Double
        Dim MI As MeteoInfo = Nothing
        Dim i As Long
        Dim CurDate As DateTime
        Dim TotalDist As Double = 0
        TC.StartPoint = Start.P


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

            Speed = _SailManager.GetSpeed(_BoatType, clsSailManager.EnumSail.OneSail, WindAngle(Cap, MI.Dir), MI.Strength)
            TotalDist += Speed / 60 * RouteurModel.VacationMinutes

            'TC.StartPoint = TC.EndPoint
        Next

        TC.EndPoint = TC.ReachDistance(TotalDist, Cap)
        If GridRouter.CheckSegmentValid(TC) Then

            With RetPoint
                .P = New Coords(TC.EndPoint)
                .T = CurDate
                .Speed = Speed
                .WindStrength = MI.Strength
                .WindDir = MI.Dir
                .DistFromPos = TC.SurfaceDistance
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

        End If

        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing

        Return RetPoint

    End Function

    Public Function RouteToPoint(ByVal C As Coords) As ObservableCollection(Of VLM_Router.clsrouteinfopoints)

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
                    If TC.SurfaceDistance < 1 Then
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
                mi = _Meteo.GetMeteoToDate(StartDate, From.Lon_Deg, From.Lat_Deg, False)
                System.Threading.Thread.Sleep(250)
            End While
            With _StartPoint
                .P = New Coords(From)
                .T = StartDate
                .DTF = _TC.SurfaceDistance
                .WindDir = mi.Dir
                .WindStrength = mi.Strength
            End With

            _DestPoint1 = New Coords(WP1)
            _DestPoint2 = New Coords(WP2)
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
