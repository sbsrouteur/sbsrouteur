Imports System.Threading
Imports Routeur.VOR_Router
Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class IsoRouter
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event Log(ByVal msg As String)
    Public Event RouteComplete()

    Private _AngleStep As Double
    Private _IsoStep As TimeSpan
    Private _IsoStep_24 As TimeSpan
    Private _IsoStep_48 As TimeSpan
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


    Public Sub New(ByVal BoatType As String, ByVal SailManager As clsSailManager, ByVal Meteo As GribManager, ByVal AngleStep As Double, ByVal IsoStep As TimeSpan, ByVal IsoStep_24 As TimeSpan, ByVal IsoStep_48 As TimeSpan)
        _AngleStep = AngleStep
        _IsoStep = IsoStep
        _IsoStep_24 = IsoStep_24
        _IsoStep_48 = IsoStep_48

        _Meteo = Meteo
        _SailManager = SailManager
        _BoatType = BoatType
    End Sub


    Private Function ComputeNextIsoChrone(ByVal Iso As IsoChrone) As IsoChrone

        Dim RetIsoChrone As IsoChrone = Nothing
        Dim alpha As Double
        Dim P As clsrouteinfopoints
        Dim Index As Integer
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
                        RetIsoChrone.Data(Index) = P
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
                    Dim Ortho As Double
                    tc.StartPoint = rp.P
                    tc.EndPoint = _DestPoint
                    Ortho = tc.TrueCap
                    tc.StartPoint = _StartPoint.P
                    For alpha = 0 To 360 - _AngleStep Step _AngleStep

                        If WindAngle(Ortho, alpha) < 180 - 120 Then
                            P = ReachPoint(rp, alpha, CurStep)
                            If Not P Is Nothing Then
                                tc.EndPoint = P.P
                                alpha2 = tc.Cap

                                Index = RetIsoChrone.IndexFromAngle(alpha2)
                                OldP = RetIsoChrone.Data(Index)
                                If OldP Is Nothing OrElse P.DTF < OldP.DTF Then
                                    RetIsoChrone.Data(Index) = P
                                End If
                            End If
                        End If

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
        Dim Start As DateTime = Now
        Dim P As clsrouteinfopoints = Nothing
        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = _DestPoint

        Dim Loxo As Double = TC.Cap
        Dim Dist As Double = TC.SurfaceDistance

        RaiseEvent Log("Isochrone router started at " & Start)
        While Not RouteComplete AndAlso Not _CancelRequested
            CurIsoChrone = ComputeNextIsoChrone(CurIsoChrone)
            'RouteComplete = CheckCompletion(CurIsoChrone)
            _IsoChrones.AddLast(CurIsoChrone)
            For Each rp As clsrouteinfopoints In CurIsoChrone.Data
                If Not rp Is Nothing Then
                    If P Is Nothing OrElse (P.DTF > rp.DTF) Then
                        P = rp
                    End If
                End If
            Next

            If Not P Is Nothing Then
                _CurBest = P
            End If

            If Not CurIsoChrone.Data(Loxo) Is Nothing Then
                TC.EndPoint = CurIsoChrone.Data(Loxo).P
                If TC.SurfaceDistance >= Dist Then
                    RouteComplete = True
                End If
            End If
            RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)

        End While
        RaiseEvent Log("Isochrone routing completed in  " & Now.Subtract(Start).ToString)
        RaiseEvent RouteComplete()
    End Sub

    Private Function ReachPoint(ByVal Start As clsrouteinfopoints, ByVal Cap As Double, ByVal Duration As TimeSpan) As clsrouteinfopoints

        Dim RetPoint As New clsrouteinfopoints
        Dim TC As New TravelCalculator
        Dim Speed As Double
        Dim MI As MeteoInfo = Nothing
        Dim i As Long
        Dim CurDate As DateTime
        TC.StartPoint = Start.P


        For i = 0 To Duration.Ticks Step CLng(RouteurModel.VacationMinutes * TimeSpan.TicksPerMinute)
            CurDate = Start.T.AddTicks(i)
            MI = _Meteo.GetMeteoToDate(CurDate, TC.StartPoint.Lon_Deg, TC.StartPoint.Lat_Deg, True)
            Speed = _SailManager.GetSpeed(_BoatType, clsSailManager.EnumSail.OneSail, WindAngle(Cap, MI.Dir), MI.Strength)
            TC.EndPoint = TC.ReachDistance(Speed / 60 * RouteurModel.VacationMinutes, Cap)

            If Not GridRouter.CheckSegmentValid(TC) Then
                Return Nothing
            End If

            TC.StartPoint = TC.EndPoint

        Next

        With RetPoint
            .P = New Coords(TC.StartPoint)
            .T = CurDate
            .Speed = Speed
            .WindStrength = MI.Strength
            .WindDir = MI.Dir
            TC.EndPoint = _DestPoint
            .DTF = TC.SurfaceDistance
            .From = Start
            .Cap = Cap
        End With

        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        TC = Nothing

        Return RetPoint
    End Function

    Public Function RouteToPoint(ByVal C As Coords) As ObservableCollection(Of VOR_Router.clsrouteinfopoints)

        Dim TC As New TravelCalculator
        TC.StartPoint = _StartPoint.P
        TC.EndPoint = C
        Dim Loxo As Double = TC.Cap
        Dim RP As clsrouteinfopoints = Nothing

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

        If RP Is Nothing Then
            Return Nothing
        Else
            Return RouteToPoint(RP)
        End If
    End Function

    Public Function RouteToPoint(ByVal c As clsrouteinfopoints) As ObservableCollection(Of VOR_Router.clsrouteinfopoints)

        Dim RetRoute As New ObservableCollection(Of VOR_Router.clsrouteinfopoints)
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
    Public ReadOnly Property Route() As ObservableCollection(Of VOR_Router.clsrouteinfopoints)
        Get

            Return RouteToPoint(_CurBest)

        End Get
    End Property



    Public Sub StartIsoRoute(ByVal From As Coords, ByVal Dest As Coords, ByVal StartDate As Date)

        If _IsoRouteThread Is Nothing Then
            _IsoRouteThread = New Thread(AddressOf IsoRouteThread)
            _CancelRequested = False
            _StartPoint = New VOR_Router.clsrouteinfopoints
            _TC.StartPoint = From
            _TC.EndPoint = Dest

            Dim mi As MeteoInfo = _Meteo.GetMeteoToDate(StartDate, From.Lon_Deg, From.Lat_Deg, False)
            With _StartPoint
                .P = New Coords(From)
                .T = StartDate
                .DTF = _TC.SurfaceDistance
                .WindDir = mi.Dir
                .WindStrength = mi.Strength
            End With

            _DestPoint = New Coords(Dest)
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
