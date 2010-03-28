Imports System.ComponentModel
Imports System.Collections.Specialized
Imports System.Collections.ObjectModel
Imports System.Math


Public Class GridRouter

    Implements INotifyPropertyChanged
    Public Shared GRID_HIT_TOLERANCE As Double = RouteurModel.GridGrain / 5


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event Log(ByVal Str As String)

    Private Const THREAD_COUNT As Integer = 1

    Private _Start As Coords
    Private _Dest As Coords
    Public Shared CoordsComparer As New CoordsComparer
    Public Shared RoutingComparer As New RoutingGridPointComparer
    Private _GridPointsList As New BSPList 'Dictionary(Of Coords, RoutingGridPoint)(CoordsComparer)
    'Private _ToDoList As New SortedList(Of RoutingGridPoint, RoutingGridPoint)(New RoutingGridPointComparer)
    Private _ToDoList As New Queue(Of RoutingGridPoint)
    Private WithEvents _MeteoProvider As clsMeteoOrganizer
    Private _SailManager As clsSailManager
    Private _CurBestTarget As RoutingGridPoint

    Private _AngleStep As Integer
    Private _GridGrain As Double
    Private _StepEvent As New System.Threading.AutoResetEvent(False)
    Private _BrokenSails As Integer
    Public Event StepComputed(ByVal GridListSize As Long, ByVal TodoListSize As Integer)
    Private _StartDate As DateTime
    Private _RouteStartDate As DateTime
    Private _ActiveThreads As Integer = 0
    Private _Radius As Double
    Private _LoopCount As Long = 0
    Private _BoatType As String
    Private _WP As List(Of Coords())

    Private _ProcessTotalTicks As Long
    Private _ProcessCount As Long
    Private _GetETATotalTicks As Long
    Private _GetETATotalLoops As Long
    Private _GetETACount As Long
    Private Shared _CheckTCTotalTicks As Long
    Private Shared _CheckTCCount As Long
    Private _ProgressInfo As New GridProgressContext()
    Private _Owner As Window


    Public Sub New(ByVal Start As Coords, ByVal StartDate As DateTime, ByVal MeteoProvider As clsMeteoOrganizer, ByVal Sail As clsSailManager, ByVal BoatType As String)

        Me.Start(StartDate) = Start
        _MeteoProvider = MeteoProvider
        _SailManager = Sail
        _BoatType = BoatType
    End Sub


    Public ReadOnly Property GridPointsList() As BSPList
        Get
            Return _GridPointsList
        End Get
    End Property

    Public Property Owner() As Window
        Get
            Return _Owner
        End Get
        Set(ByVal value As Window)
            _Owner = value
        End Set
    End Property

    Public Property ProgressInfo() As GridProgressContext
        Get
            Return _ProgressInfo
        End Get
        Set(ByVal value As GridProgressContext)
            _ProgressInfo = value
        End Set
    End Property

    Public Property TodoList() As Queue(Of RoutingGridPoint)
        'Public Property TodoList() As SortedList(Of RoutingGridPoint, RoutingGridPoint)
        Get
            If Not RouteurModel.ShowGrid Then
                Return Nothing
            Else
                Return _ToDoList
            End If
        End Get
        Set(ByVal value As Queue(Of RoutingGridPoint))
            _ToDoList = value
        End Set
    End Property


    Private Function GetETATo(ByVal From As Coords, ByVal Dest As Coords, ByVal StartDate As DateTime, ByVal meteo As clsMeteoOrganizer, ByVal BoatType As String, _
                                ByVal BrokenSails As Integer, _
                                ByRef Sail As clsSailManager.EnumSail, ByRef speed As Double) As DateTime

        Dim StartTick As DateTime = Now
        Dim NbLoop As Long = 0
        Try
            Dim ETA As DateTime = StartDate
            Dim TC As New TravelCalculator
            Dim TC2 As New TravelCalculator

            TC.StartPoint = From
            TC.EndPoint = Dest

            Dim Cap As Double = TC.TrueCap
            Dim ReachDist As Double = TC.SurfaceDistance
            Dim Mi As MeteoInfo
            Dim NoWindCount As Integer = 0
            Dim NoSpeedCound As Integer = 0
            Dim Start As DateTime = ETA
            Dim STepTime As Double = RouteurModel.VacationMinutes
            Static NbCall As Long = 0

            NbCall += 1
            TC.EndPoint = From
            Do
                NbLoop += 1
                Do
                    Mi = meteo.GetMeteoToDate(TC.EndPoint, ETA, True)
                    If Mi Is Nothing Then
                        System.Threading.Thread.Sleep(20)
                    End If
                Loop Until Mi IsNot Nothing

                If Mi.Strength = 0 Then
                    'No wind
                    NoWindCount += 1
                    ETA.AddHours(0.5)
                Else
                    NoWindCount = 0
                    speed = _SailManager.GetBestSailSpeed(BoatType, Sail, VOR_Router.WindAngle(Cap, Mi.Dir), Mi.Strength, BrokenSails)
                    If speed < 1 AndAlso speed < Mi.Strength / 10 Then
                        'NoSpeed()
                        Exit Do
                        NoSpeedCound += 1
                        ETA.AddHours(0.5)
                    Else
                        NoSpeedCound = 0
                        TC2.StartPoint = TC.EndPoint
                        If ReachDist - TC.SurfaceDistance >= speed / 60 * STepTime Then
                            TC.EndPoint = TC2.ReachDistance(speed / 60 * STepTime, Cap) ' RouteurModel.VacationMinutes, Cap)
                            ETA = ETA.AddMinutes(STepTime)
                        ElseIf NbLoop > 1 Then
                            ETA = ETA.AddHours((ReachDist - TC.SurfaceDistance) / speed)
                            TC.EndPoint = TC2.ReachDistance(ReachDist - TC.SurfaceDistance, Cap)
                        Else
                            Dim i As Integer = 0
                            Exit Do
                        End If
                    End If
                End If
            Loop Until ReachDist < TC.SurfaceDistance OrElse ReachDist - TC.SurfaceDistance < RouteurModel.GridGrain OrElse NoWindCount > 10 OrElse NoSpeedCound > 20 OrElse ETA.Subtract(Start).TotalDays > 1


            If Double.IsNaN(TC.SurfaceDistance) OrElse ReachDist - TC.SurfaceDistance > RouteurModel.GridGrain Then 'OrElse ETA.Subtract(Start).TotalMinutes * 0.9 < RouteurModel.VacationMinutes Then
                ETA = New DateTime(3000, 12, 12)
            End If

            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC2.StartPoint = Nothing
            TC2.EndPoint = Nothing
            TC2 = Nothing
            TC = Nothing
            Return ETA
        Finally
            _GetETACount += 1
            _GetETATotalTicks += Now.Subtract(StartTick).Ticks
            _GetETATotalLoops += NbLoop
            Stats.SetStatValue(Stats.StatID.GridAvgGetETAMS) = _GetETATotalTicks / _GetETACount / TimeSpan.TicksPerMillisecond
            Stats.SetStatValue(Stats.StatID.GridAvgGetETALoops) = _GetETATotalLoops / _GetETACount
        End Try
    End Function


    Public Function RouteToPoint(ByVal c As ICoords) As ObservableCollection(Of VOR_Router.clsrouteinfopoints)

        Dim c2 As Coords = New Coords(c)
        c2.RoundTo(RouteurModel.GridGrain)
        If _GridPointsList.Contains(c2) Then
            Return RouteToPoint(CType(_GridPointsList(c2), RoutingGridPoint))
        Else
            Return New ObservableCollection(Of VOR_Router.clsrouteinfopoints)
        End If
    End Function

    Public Function RouteToPoint(ByVal c As RoutingGridPoint) As ObservableCollection(Of VOR_Router.clsrouteinfopoints)

        Dim RetRoute As New ObservableCollection(Of VOR_Router.clsrouteinfopoints)
        Try
            If c Is Nothing Then
                Return RetRoute
            End If

            SyncLock c
                Dim P As RoutingGridPoint = c

                Dim CurCap As Double
                Dim CurSail As clsSailManager.EnumSail

                If Not P Is Nothing Then
                    If Not P.P Is Nothing Then
                        CurCap = CDbl(P.P.Cap.ToString("0.0"))
                        CurSail = P.P.Sail
                    End If
                End If

                RetRoute.Insert(0, P.P)
                While P.From IsNot Nothing And RetRoute.Count < 1000
                    'R = New VOR_Router.clsrouteinfopoints With {.P = New Coords(P)}

                    If CDbl(P.P.Cap.ToString("0.0")) <> CurCap Or P.P.Sail <> CurSail Then
                        RetRoute.Insert(0, P.P)
                        CurCap = CDbl(P.P.Cap.ToString("0.0"))
                        CurSail = P.P.Sail
                    End If
                    P = P.From
                End While
            End SyncLock
        Catch ex As Exception

            Dim i As Integer = 0
            RaiseEvent Log("Arrgh exception " & ex.Message & " " & ex.StackTrace)
        End Try

        Return RetRoute
    End Function

    Public ReadOnly Property Route() As ObservableCollection(Of VOR_Router.clsrouteinfopoints)
        Get

            Return RouteToPoint(_CurBestTarget)

        End Get
    End Property


    Private Sub SeedGrid(ByVal gr As BSPList, ByVal Level As Integer)

        Dim GridLevel As Integer = 1
        Dim GRStepX As Double
        Dim GRStepY As Double
        Dim X As Double
        Dim Y As Double

        GRStepX = RouteurModel._RaceRect(0).Lon - RouteurModel._RaceRect(2).Lon
        GRStepY = RouteurModel._RaceRect(0).Lat - RouteurModel._RaceRect(2).Lat
        While GridLevel < Level


            For X = 0 To GridLevel
                For Y = 0 To GridLevel
                    Dim G As New RoutingGridPoint(New Coords() With {.Lon = RouteurModel._RaceRect(2).Lon + X * GRStepX / (GridLevel + 1), .Lat = RouteurModel._RaceRect(2).Lat + Y * GRStepY / (GridLevel + 1)}, 0, 0)
                    If Not gr.Contains(G) Then
                        gr.Add(G)
                    End If
                Next
            Next

            GridLevel += 1

        End While


    End Sub

    Public ReadOnly Property Start() As Coords

        Get
            Return _Start
        End Get
    End Property



    Public WriteOnly Property Start(ByVal Dte As DateTime) As Coords
        Set(ByVal value As Coords)
            _Start = value
            _StartDate = Dte
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Start"))
        End Set

    End Property

    Public Sub [Stop]()

        SyncLock _ToDoList
            _ToDoList.Clear()
        End SyncLock

    End Sub



    'Public Shared Function CheckSegmentValid(ByVal TC As TravelCalculator) As Boolean

    '    Dim StartTick As DateTime = Now
    '    Try
    '        Dim IntC As Coords
    '        Dim CurD As Double = 0
    '        Dim Increment As Double = RouteurModel.GridGrain / 2 ' 4 * Math.Sqrt(GRID_HIT_TOLERANCE) '0.5
    '        Dim TrueCap As Double = TC.TrueCap

    '        If TC.SurfaceDistance = 0 Then
    '            Return True
    '        End If

    '        If GSHHS_Reader.HitTest(TC.EndPoint, GRID_HIT_TOLERANCE, GSHHS_Reader.Polygons(TC.EndPoint), True) Then
    '            Return False
    '        End If
    '        'If 10 * Increment < TC.SurfaceDistance Then
    '        '    Increment = TC.SurfaceDistance / 10
    '        'End If

    '        While CurD <= TC.SurfaceDistance
    '            IntC = TC.ReachDistance(CurD, TrueCap)
    '            If GSHHS_Reader.HitTest(IntC, GRID_HIT_TOLERANCE, GSHHS_Reader.Polygons(IntC), True) Then
    '                Return False
    '            End If
    '            CurD += Increment
    '        End While
    '        Return True
    '    Finally
    '        _CheckTCCount += 1
    '        _CheckTCTotalTicks += Now.Subtract(StartTick).Ticks
    '        Stats.SetStatValue(Stats.StatID.GridAvg_CheckSegmentMS) = _CheckTCTotalTicks / _CheckTCCount / TimeSpan.TicksPerMillisecond
    '    End Try

    'End Function

    Public Shared Function CheckSegmentValid(ByVal TC As TravelCalculator) As Boolean

        Static Px As Double = RouteurModel.GridGrain / 180 / BspRect.GRID_GRAIN_OVERSAMPLE * Math.PI * 2
        Dim StartTick As DateTime = Now
        Try

            If TC.SurfaceDistance = 0 Or RouteurModel.NoObstacle Then
                Return True
            End If

            If GSHHS_Reader.HitTest(TC.EndPoint, GRID_HIT_TOLERANCE, GSHHS_Reader.Polygons(TC.EndPoint), True) Then
                Return False
            End If

            Dim Dx As Double = TC.EndPoint.Lon - TC.StartPoint.Lon
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
        Finally
            _CheckTCCount += 1
            _CheckTCTotalTicks += Now.Subtract(StartTick).Ticks
            Stats.SetStatValue(Stats.StatID.GridAvg_CheckSegmentMS) = _CheckTCTotalTicks / _CheckTCCount / TimeSpan.TicksPerMillisecond
        End Try

    End Function



    Public Sub ComputeBestRoute(ByVal AngleStep As Integer, ByVal GridGrain As Double, ByVal BrokenSails As Integer, ByVal WP As List(Of Coords()))

        Dim G As New RoutingGridPoint(Start, 0, 0)
        Dim i As Integer
        Static WorkersStarted As Boolean = False
        'Dim MI As MeteoInfo

        Dim TC As New TravelCalculator

        TC.StartPoint = Start
        TC.EndPoint = New Coords(Start)
        'tc.EndPoint.Lon_Deg += GridGrain
        TC.EndPoint.Lat_Deg -= GridGrain
        _Radius = TC.SurfaceDistance / 2
        _GridPointsList.Clear()
        'For Each P As RoutingGridPoint In _GridPointsList.Values

        '    P.ClearNeighboors()
        'Next
        _GridPointsList.Clear()
        'SeedGrid(_GridPointsList, 3)
        _ToDoList.Clear()
        _RouteStartDate = Now
        _WP = WP

        GC.Collect()
        
        G.CurETA = _StartDate
        RaiseEvent Log("Starting from " & Start.ToString & " at " & _StartDate.ToString & " grid size " & _GridPointsList.Count)

        G.BuildNeighBoorsList(1, AngleStep, GridGrain, _GridPointsList, _SailManager, Start, Start, WP)
        If Not _CurBestTarget Is Nothing Then
            SyncLock _CurBestTarget
                _CurBestTarget = Nothing
            End SyncLock
        End If
        _GridGrain = GridGrain
        _AngleStep = AngleStep
        _BrokenSails = BrokenSails
        _ToDoList.Enqueue(G)
        G.Dist = TC.WPDistance(WP)
        _ProgressInfo.Start(G.Dist)

        If Not WorkersStarted Then
            WorkersStarted = True
            'System.Threading.ThreadPool.QueueUserWorkItem(AddressOf DoSteps, True)
            For i = 0 To THREAD_COUNT - 1
                'System.Threading.Thread.Sleep(1000)

                'While _ToDoList.Count = 0
                '    System.Threading.Thread.Sleep(1000)
                'End While
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf DoSteps, If(i = 0, True, False))
            Next
        End If

    End Sub

    Private Sub DoSteps(ByVal State As Object)
        Static LastRefresh As DateTime = Now
        Dim Master As Boolean = CBool(State)


        System.Threading.Thread.CurrentThread.Priority = System.Threading.ThreadPriority.BelowNormal
        'System.Threading.Interlocked.Increment(_ActiveThreads)

        While True

            Try
                While ProcessNextPoint(_BoatType, _BrokenSails)

                    System.Threading.Interlocked.Increment(_LoopCount)
                    If Now.Subtract(LastRefresh).TotalSeconds > 60 Then
                        LastRefresh = Now
                        RaiseEvent StepComputed(_LoopCount, _ToDoList.Count)
                        '_StepEvent.WaitOne()
                    End If
                End While

                'System.Threading.Interlocked.Decrement(_ActiveThreads)
                If Master Then
                    System.Threading.Interlocked.Exchange(_LoopCount, 0)
                    RaiseEvent Log("RouteComplete in " & Now.Subtract(_RouteStartDate).ToString)
                    RaiseEvent StepComputed(_LoopCount, _ToDoList.Count)
                    'End If

                End If

                'SyncLock _ToDoList
                While _ToDoList.Count = 0
                    System.Threading.Thread.Sleep(1000)
                End While
                'End SyncLock

            Catch ex As Exception
            End Try

        End While

    End Sub

    Public Sub StepGrid()
        _StepEvent.Set()
    End Sub

    Private Function ProcessNextPoint(ByVal BoatType As String, ByVal brokensails As Integer) As Boolean

        Dim RG As RoutingGridPoint
        Dim N As RoutingGridPoint
        Dim mi As MeteoInfo
        Dim CurSpeed As Double
        Dim CurSail As clsSailManager.EnumSail
        Dim eta As DateTime
        Static Counts As Long
        Static CurWP As Long = 0
        Dim StartTick As DateTime = Now

        Try

            'RaiseEvent Log("Synclock todolist th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
            SyncLock _ToDoList
                RG = Nothing
                Do
                    If _ToDoList.Count <> 0 Then

                        RG = _ToDoList.Dequeue

                    Else
                        Return False
                    End If
                Loop Until _ToDoList.Count = 0 Or (RG IsNot Nothing AndAlso Not RG.UpToDate)
            End SyncLock

            If RG Is Nothing Then
                Return True
            End If


            'RaiseEvent Log("Synclock RG th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
            If System.Threading.Monitor.TryEnter(RG) Then
                Try

                    If RG.UpToDate Then
                        Return True
                    End If

                    If Double.IsNaN(RG.P.P.Lat) Or Double.IsNaN(RG.P.P.Lon) Then
                        Return True
                    End If


                    Do
                        mi = _MeteoProvider.GetMeteoToDate(RG.P.P, RG.CurETA, True)
                        If mi Is Nothing Then
                            System.Threading.Thread.Sleep(100)
                        ElseIf mi.Strength <= 0 Then
                            Dim i As Integer = 0
                            Return True
                        End If
                    Loop Until mi IsNot Nothing
                    Dim TC As New TravelCalculator
                    TC.StartPoint = RG.P.P
                    Dim GridStep As Integer = CInt(Math.Floor(RG.CurETA.Subtract(_StartDate).TotalHours / 24)) + 1

                    If _CurBestTarget Is Nothing OrElse RouteurModel.CurWP <> CurWP Then
                        RG.BuildNeighBoorsList(GridStep, _AngleStep, _GridGrain, _GridPointsList, _SailManager, Start, Start, _WP)
                        CurWP = RouteurModel.CurWP
                    Else
                        RG.BuildNeighBoorsList(GridStep, _AngleStep, _GridGrain, _GridPointsList, _SailManager, Start, _CurBestTarget.P.P, _WP)
                    End If

                    'Dim log As New System.IO.FileStream("log.log", IO.FileMode.Append)
                    'Dim SR As New System.IO.StreamWriter(Log)
                    'SR.WriteLine("routing points from " & RG.P.P.ToString & " cureta " & RG.CurETA)


                    For Each C In RG.Neighboors
                        If Not C Is Nothing Then
                            N = CType(_GridPointsList(C), RoutingGridPoint)

                            eta = GetETATo(RG.P.P, N.P.P, RG.CurETA, _MeteoProvider, BoatType, brokensails, CurSail, CurSpeed)

                            If eta < N.CurETA Then 'AndAlso (_CurBestTarget Is Nothing OrElse N.CurETA.Ticks - StartDate.Ticks < 1.3 * (_CurBestTarget.CurETA.Ticks - StartDate.Ticks)) Then
                                TC.EndPoint = N.P.P

                                N.CurETA = eta
                                N.From = RG
                                N.P.Cap = (TC.TrueCap + 360) Mod 360
                                N.P.WindDir = mi.Dir
                                N.P.WindStrength = mi.Strength
                                N.P.Speed = CurSpeed
                                N.P.Sail = CurSail
                                N.P.T = eta
                                N.P.DTF = N.Dist

                                N.Iteration = RG.Iteration + 1
                                N.Loch = RG.Loch + TC.SurfaceDistance

                                'RaiseEvent Log("Synclock todolist (dans RG) th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
                                SyncLock _ToDoList

                                    _ToDoList.Enqueue(N)

                                End SyncLock

                                'If _CurBestTarget Is Nothing OrElse _CurBestTarget.Score > N.Score Then

                                '    'SR.WriteLine("Changed Target " & N.P.P.ToString & " " & N.CurETA & " " & N.CrossedLine & " " & N.Dist & " " & N.P.Cap)
                                '    _CurBestTarget = N
                                '    RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
                                'End If

                                If N.Improve(_CurBestTarget, CurSpeed, _WP) Then
                                    'SR.WriteLine("Changed Target " & N.P.P.ToString & " " & N.CurETA & " " & N.CrossedLine & " " & N.Dist & " " & N.P.Cap)
                                    _CurBestTarget = N
                                    RaiseEvent PropertyChanged(Me, RouteurModel.PropTmpRoute)
                                    'ElseIf Math.Abs(N.Dist - _CurBestTarget.Dist) <= RouteurModel.GridGrain Then
                                    '    Dim i As Integer = 0
                                    '    'SR.WriteLine("Ignored Target " & N.P.P.ToString & " " & N.CurETA & " " & N.CrossedLine & " " & N.Dist & " " & N.P.Cap)
                                    _ProgressInfo.Progress(_GridPointsList.Count, _ToDoList.Count, _CurBestTarget.Dist)


                                End If

                            End If

                        End If



                    Next

                    'SR.Flush()
                    'log.Close()

                    If Counts Mod 20000 = 0 Then
                        GC.Collect()
                    End If
                    Counts += 1

                    RG.UpToDate = True
                    TC.StartPoint = Nothing
                    TC.EndPoint = Nothing
                    TC = Nothing
                    Return True
                Catch ex As Exception

                    MessageBox.Show(ex.Message & " " & ex.StackTrace)

                Finally

                    System.Threading.Monitor.Exit(RG)
                End Try
            Else
                Return True
            End If
        Finally

            _ProcessTotalTicks += Now.Subtract(StartTick).Ticks
            _ProcessCount += 1
            Stats.SetStatValue(Stats.StatID.GridAvgProcesLengthMS) = _ProcessTotalTicks / _ProcessCount / TimeSpan.TicksPerMillisecond

        End Try

    End Function
    Public ReadOnly Property StartDate() As DateTime
        Get
            Return _StartDate
        End Get

    End Property


    Private Sub _MeteoProvider_log(ByVal msg As String) Handles _MeteoProvider.log
        RaiseEvent Log(msg)
    End Sub
End Class

Public Class RoutingGridPointComparer
    Implements IComparer(Of RoutingGridPoint)
    Implements IEqualityComparer(Of RoutingGridPoint)
    


    Public Function Compare(ByVal x As RoutingGridPoint, ByVal y As RoutingGridPoint) As Integer Implements System.Collections.Generic.IComparer(Of RoutingGridPoint).Compare

        If x.Dist * x.Iteration > y.Dist * y.Iteration Then
            Return 1
        ElseIf x.Dist * x.Iteration < y.Dist * y.Iteration Then
            Return -1
        Else
            'Static CC As New CoordsComparer
            Return CoordsComparer.Compare(x.P.P, y.P.P)
        End If

    End Function

    Public Function Equals1(ByVal x As RoutingGridPoint, ByVal y As RoutingGridPoint) As Boolean Implements System.Collections.Generic.IEqualityComparer(Of RoutingGridPoint).Equals

        Return x.P.P.Lon = y.P.P.Lon AndAlso x.P.P.Lat = y.P.P.Lat

    End Function

    Public Function GetHashCode1(ByVal obj As RoutingGridPoint) As Integer Implements System.Collections.Generic.IEqualityComparer(Of RoutingGridPoint).GetHashCode

        Return obj.P.P.GetHashCode

    End Function
End Class
