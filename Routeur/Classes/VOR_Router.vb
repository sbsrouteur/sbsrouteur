Imports System.Net
Imports System.Xml.Serialization
Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class VOR_Router


    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event bruted()
    Public Event BoatShown()
    Public Event BoatClear()

    Private _UserInfo As user
    Private _WebClient As New WebClient()
    Private _CookiesContainer As CookieContainer = Nothing
    Private _LastRefreshTime As DateTime
    Private _LastDataDate As DateTime

    Private _WindChangeDist As Double
    Private _WindChangeDate As DateTime
    Private _WindCoords As New Coords

    'Private _Meteo As New clsMeteoOrganizer
    'Private _VMGMeteo As New clsMeteoOrganizer
    Private _Meteo As New clsMeteoOrganizer
    Private _Sails As New clsSailManager

    Private _CurrentRoute As New TravelCalculator
    Private _RouteToGoal As New TravelCalculator

    'Private _RoutingPoints() As Coords
    Private _VMG As Double
    Private _ETA As DateTime

    Private _BestRouteAtPoint As New ObservableCollection(Of clsrouteinfopoints)
    Private _TempVMGRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _CurBestRoute As ObservableCollection(Of clsrouteinfopoints)
    Private _BruteRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _TempRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _PlannedRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _LastBestVMGCalc As TimeSpan = New TimeSpan(0, 0, 15)
    Private _LastCommunication As DateTime = New DateTime(0)
    Private _lastflush As DateTime = New DateTime(0)


    Private _Login As String
    Private _Code As String
    Private _Version As String
    Private _Pass As String
    Private _U As String

    Private WithEvents _gr As GridRouter

#If BOTS = 1 Then
    Private WithEvents _Pilote As New clsPilote
    Private _PiloteCleared As Boolean = True
    Private _AutoRouteFailures As Integer = 0
#End If

    Private _StopBruteForcing As Boolean = False

    Private _DebugHttp As Boolean = False

    Private WithEvents _PlayerInfo As clsPlayerInfo
    Private _DiffEvolution As New ObservableCollection(Of Decimal)
    Private _Log As New ObservableCollection(Of String)
    Private _LogQueue As New Queue(Of String)
    Private _ThreadSetterCurPos As New Coords

    Private _Opponents As New Dictionary(Of String, BoatInfo)
    Private _FriendsGraph As New Dictionary(Of String, SortedList(Of String, String))
    Private _BoatsToTest(NB_WORKERS) As eHashTable
    Private Const NB_WORKERS As Integer = 0 '15
    Private _VMGStarted As Integer
    Private _HasPosition As Boolean = False
    Private _Workerscount As Integer
    Private _StopRouting As Boolean = False
    Private _CurWPDist As Double = 0
    Private WithEvents _RoutingRestart As New Timers.Timer
    Private _RoutingRestartPending As Boolean = False
    Private Shared _Track As String = Nothing
    Private _DrawOpponnents As Boolean = False
    Private _CurUserWP As Integer = 0

    Private _AllureAngle As Double = 0
    Private _AllureDuration As Double = 0
    Private _AllureRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _PilototoRoute As New ObservableCollection(Of clsrouteinfopoints)
    Private _LastGridRouteStart As New DateTime(0)

    Public Shared BrokenSails As Integer

    Private _BoatUnderMouse As New ObservableCollection(Of BoatInfo)
    Private _MeteoUnderMouse As New ObservableCollection(Of RoutingGridPoint)
    Private _Pilototo(5) As String
    Private _HideWindArrow As Boolean = True

    Private _WayPointDest As New Coords
    Private _GridProgressWindow As New frmRoutingProgress
    Private _Owner As Window
    Private Shared _PosValide As Boolean = False


    Public Class Route
        Inherits ObservableCollection(Of Coords)
    End Class

    Public Class clsrouteinfopoints

        Implements INotifyPropertyChanged

        Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
        Private _P As Coords
        Private _Cap As Double
        Private _T As DateTime
        Private _Sail As clsSailManager.EnumSail
        Private _Speed As Double
        Private _WindDir As Double
        Private _WindStrength As Double
        'Private _CurTC As New TravelCalculator
        Private _DTF As New Double
        Private _CapFromPos As Double
        Private _DistFromPos As Double

        Public Shared PProps As New PropertyChangedEventArgs("P")
        Public Shared CapProps As New PropertyChangedEventArgs("Cap")
        Public Shared TProps As New PropertyChangedEventArgs("T")
        Public Shared SailProps As New PropertyChangedEventArgs("Sail")
        Public Shared WindDirProps As New PropertyChangedEventArgs("WindDir")
        Public Shared WindStrengthProps As New PropertyChangedEventArgs("WindStrength")
        Public Shared SpeedProps As New PropertyChangedEventArgs("Speed")
        Public Shared CurTCProps As New PropertyChangedEventArgs("CurTC")
        Public Shared CurDTFProps As New PropertyChangedEventArgs("DTF")



        Public Property Cap() As Double
            Get
                Return _Cap
            End Get
            Set(ByVal value As Double)
                _Cap = value
                RaiseEvent PropertyChanged(Me, CapProps)

            End Set
        End Property

        Public Property CapFromPos() As Double
            Get
                Return _CapFromPos
            End Get
            Set(ByVal value As Double)
                _CapFromPos = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CapFromPos"))
            End Set
        End Property

        Public Property DistFromPos() As Double
            Get
                Return _DistFromPos
            End Get
            Set(ByVal value As Double)
                _DistFromPos = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DistFromPos"))
            End Set
        End Property

        'Public Property CurTC() As TravelCalculator
        '    Get
        '        Return _CurTC
        '    End Get
        '    Set(ByVal value As TravelCalculator)
        '        _CurTC.StartPoint = Nothing
        '        _CurTC.EndPoint = Nothing
        '        _CurTC = value
        '        RaiseEvent PropertyChanged(Me, CurTCProps)
        '    End Set
        'End Property

        Public Property DTF() As Double
            Get
                Return _DTF

            End Get
            Set(ByVal value As Double)
                _DTF = value
                RaiseEvent PropertyChanged(Me, CurDTFProps)
            End Set
        End Property
        Public Property P() As Coords
            Get
                Return _P
            End Get
            Set(ByVal value As Coords)
                _P = value
                RaiseEvent PropertyChanged(Me, PProps)
            End Set
        End Property
        Public Property Sail() As clsSailManager.EnumSail
            Get
                Return _Sail
            End Get
            Set(ByVal value As clsSailManager.EnumSail)
                _Sail = value
                RaiseEvent PropertyChanged(Me, SailProps)
            End Set
        End Property
        Public Property Speed() As Double
            Get
                Return _Speed
            End Get
            Set(ByVal value As Double)
                _Speed = value
                RaiseEvent PropertyChanged(Me, SpeedProps)
            End Set
        End Property

        'Public Sub RefreshTC(ByVal CurPos As Coords)

        '    _CurTC.EndPoint = _P
        '    _CurTC.StartPoint = CurPos
        '    RaiseEvent PropertyChanged(Me, CurTCProps)


        'End Sub

        Public Property T() As DateTime
            Get
                Return _T
            End Get
            Set(ByVal value As DateTime)
                _T = value
            End Set
        End Property

        Public Overrides Function ToString() As String

            Return T.ToString & " ; " & Cap.ToString("f2") & " ; " & DTF.ToString & " ; " & P.ToString & " ; " & WindDir.ToString("f2") & " ; " & WindStrength.ToString("f2")

        End Function

        Public Property WindDir() As Double
            Get
                Return _WindDir
            End Get
            Set(ByVal value As Double)
                _WindDir = value
                RaiseEvent PropertyChanged(Me, WindDirProps)
            End Set
        End Property

        Public Property WindStrength() As Double
            Get
                Return _WindStrength
            End Get
            Set(ByVal value As Double)
                _WindStrength = value
                RaiseEvent PropertyChanged(Me, WindStrengthProps)
            End Set
        End Property


    End Class

    Public Property AllureAngle() As Double
        Get
            Return _AllureAngle
        End Get
        Set(ByVal value As Double)

            If _AllureAngle <> value Then
                _AllureAngle = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AllureAngle"))
                ComputeAllure()
            End If
        End Set
    End Property

    Public Property AllureDuration() As Double
        Get
            Return _AllureDuration
        End Get
        Set(ByVal value As Double)

            If _AllureDuration <> value Then
                _AllureDuration = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AllureDuration"))
                ComputeAllure()
            End If
        End Set
    End Property

    Public Property AllureRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _AllureRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _AllureRoute = value
        End Set
    End Property


    Public Property CurUserWP() As String
        Get
            Dim i As Integer = 0

            If _CurUserWP <> 0 AndAlso _CurUserWP < RouteurModel.WPList.Count Then

                Return RouteurModel.WPList(_CurUserWP)
            ElseIf _CurUserWP = 0 AndAlso RouteurModel.CurWP < RouteurModel.WPList.Count Then
                If RouteurModel.WPList(RouteurModel.CurWP) Is Nothing Then
                    RouteurModel.WPList(0) = "<Auto> ??"
                Else
                    RouteurModel.WPList(0) = "<Auto> " & RouteurModel.WPList(RouteurModel.CurWP)
                End If

                Return RouteurModel.WPList(0)
            Else
                Return "??"
            End If
        End Get
        Set(ByVal value As String)
            Dim Index As Integer = 0

            While RouteurModel.WPList(Index) <> value
                Index += 1
                If Index > RouteurModel.WPList.Count Then
                    Return
                End If
            End While

            If _CurUserWP <> Index Then
                _CurUserWP = Index - 1
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurUserWP"))
                _LastGridRouteStart = New DateTime(0)
            End If
        End Set
    End Property

    Public Property DrawOpponnents() As Boolean
        Get
            Return _DrawOpponnents
        End Get
        Set(ByVal value As Boolean)

            If _DrawOpponnents <> value Then
                _DrawOpponnents = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DrawOpponnents"))
                If value Then
                    DrawBoatMap()
                Else
                    SyncLock _Opponents
                        _Opponents.Clear()
                    End SyncLock

                    RaiseEvent BoatClear()

                End If
            End If
        End Set
    End Property

    Public Property HideWindArrow() As Boolean
        Get
            Return _HideWindArrow
        End Get
        Set(ByVal value As Boolean)
            If _HideWindArrow <> value Then
                _HideWindArrow = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HideWindArrow"))
                _MeteoUnderMouse.Clear()
            End If
        End Set
    End Property

    Public Property Owner() As Window
        Get
            Return _Owner
        End Get
        Set(ByVal value As Window)
            _Owner = value
        End Set
    End Property

    Public Property PilototoRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _PilototoRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _PilototoRoute = value
        End Set
    End Property

    Public Shared Property PosValide() As Boolean
        Get
            Return _PosValide
        End Get
        Set(ByVal value As Boolean)
            _PosValide = value
        End Set
    End Property

    Public Property TempVMGRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _TempVMGRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            ThreadSafeSetter(_TempVMGRoute, value, New PropertyChangedEventArgs("TempVMGRoute"), False, _Meteo)
        End Set
    End Property


    Public Property BestRouteAtPoint() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _BestRouteAtPoint
        End Get

        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            BestRouteAtPoint(_Meteo) = value
        End Set
    End Property

    Private WriteOnly Property BestRouteAtPoint(ByVal meteo As clsMeteoOrganizer) As ObservableCollection(Of clsrouteinfopoints)
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            ThreadSafeSetter(_BestRouteAtPoint, value, New PropertyChangedEventArgs("BestRouteAtPoint"), False, meteo)
        End Set
    End Property

    Public Property BruteRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _BruteRoute
        End Get

        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            BruteRoute(_Meteo) = value
        End Set
    End Property

    Private WriteOnly Property BruteRoute(ByVal meteo As clsMeteoOrganizer) As ObservableCollection(Of clsrouteinfopoints)
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))

            ThreadSafeSetter(_BruteRoute, value, New PropertyChangedEventArgs("BruteRoute"), False, meteo)

        End Set
    End Property

    Public Shared Function CrossWindChange(ByVal StartDate As DateTime, ByRef TargetDate As DateTime, ByVal Meteo As clsMeteoOrganizer) As Boolean

        Dim NextWindChange As DateTime

        If TargetDate > Meteo.MaxDate Then
            Return False
        End If

        If StartDate.Hour >= 8 And StartDate.Hour < 20 Then
            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 20, 0, 1)
        ElseIf StartDate.Hour < 8 Then

            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 8, 0, 1)
        Else
            NextWindChange = New DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 8, 0, 1).AddDays(1)
        End If

        If TargetDate.Subtract(StartDate) > NextWindChange.Subtract(StartDate) Then
            TargetDate = NextWindChange
            Return True
        Else
            Return False
        End If
    End Function

    Public Function CheckLogin(ByVal User As String, ByVal Password As String) As Boolean

        Dim cookies As CookieContainer = Login(User, Password)

        Return cookies IsNot Nothing

    End Function

    Private Sub ComputeAllure()

        Dim C As New Coords With {.Lat_deg = _UserInfo.position.latitude, .Lon_Deg = _UserInfo.position.longitude}
        Dim Mi As MeteoInfo
        Dim Dte As DateTime = Now
        Dim Speed As Double = 0
        Dim TC As New TravelCalculator
        _AllureRoute.Clear()
        For i = 0 To AllureDuration Step RouteurModel.VacationMinutes
            Mi = _Meteo.GetMeteoToDate(C, Dte, False)
            Speed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, Math.Abs(AllureAngle), Mi.Strength, 0)
            TC.StartPoint = C
            C = TC.ReachDistance(Speed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Mi.Dir + AllureAngle))
            Dte.AddMinutes(RouteurModel.VacationMinutes)
            Dim NewP As New clsrouteinfopoints With {.P = New Coords(C)}
            _AllureRoute.Add(NewP)
        Next
        TC.StartPoint = Nothing
        TC = Nothing
    End Sub

    Private Sub ComputePilototo()

        Dim CurDate As DateTime = Now
        Dim CurPos As Coords = New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude)
        Dim P As clsrouteinfopoints
        Dim Tc As New TravelCalculator
        Dim Mi As MeteoInfo = Nothing
        Dim BoatSpeed As Double
        Dim CurIndex As Integer = 1
        Dim RouteComplete As Boolean = False
        Dim CurWPDest As Coords = New Coords(_WayPointDest)

        Dim PrevMode As Integer = _UserInfo.position.ModePilote
        Dim PrevValue As Double

        Dim OrderDateSecs As Long
        Dim OrderDate As Date
        Dim OrderType As Integer
        Dim OrderValue As Double
        Dim Fields() As String
        Dim CurWPNUm As Integer = -1

        PilototoRoute.Clear()
        Select Case PrevMode

            Case 1
                'Cap fixe
                PrevValue = _UserInfo.position.cap

            Case 2
                'Allure fixe
                PrevValue = _UserInfo.position.AngleAllure

        End Select

        While Not RouteComplete

            If CurIndex <= 5 AndAlso _Pilototo(CurIndex) IsNot Nothing Then
                Fields = _Pilototo(CurIndex).Split(","c)
            Else
                ReDim Fields(0)
                RouteComplete = True
                OrderDate = OrderDate.AddHours(10)
            End If

            If Fields.Count >= 5 AndAlso _Pilototo(CurIndex).ToLowerInvariant.Contains("pending") Then



                If Not Long.TryParse(Fields(1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, OrderDateSecs) _
                   OrElse Not Integer.TryParse(Fields(2), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, OrderType) Then

                    Continue While
                End If

                Select Case OrderType
                    Case 1, 2
                        If Not Double.TryParse(Fields(3), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, OrderValue) Then
                            Continue While
                        End If

                    Case 3, 4, 5

                        Dim Lon As Double
                        Dim Lat As Double
                        If Not Double.TryParse(Fields(3), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat) OrElse _
                            Not Double.TryParse(Fields(3), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon) Then
                            If Lat = 0 And Lon = 0 Then
                                CurWPNUm = RouteurModel.CurWP
                            Else
                                CurWPNUm = -1
                                CurWPDest = New Coords(Lat, Lon)
                            End If
                        End If

                End Select

                OrderDate = New Date(1970, 1, 1).AddSeconds(OrderDateSecs).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours)
            End If

            While CurDate < OrderDate
                Mi = _Meteo.GetMeteoToDate(CurPos, CurDate, True)
                If Mi Is Nothing Then
                    Exit While
                End If
                Tc.StartPoint = CurPos
                Select Case PrevMode
                    Case 1
                        'Cap fixe
                        BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(PrevValue, Mi.Dir), Mi.Strength, 0)
                        CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, PrevValue)

                    Case 2
                        'Angle fixe
                        BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, Math.Abs(PrevValue), Mi.Strength, 0)
                        CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Mi.Dir + PrevValue))

                    Case 3
                        'TODO

                    Case 4

                        'BVMG

                        If CurWPNUm <> -1 Then


                            If CurWPNUm >= _PlayerInfo.RouteWayPoints.Count Then
                                CurWPNUm = _PlayerInfo.RouteWayPoints.Count - 1
                            End If
                            'GSHHS_Reader.HitDistance(Tc.StartPoint, _PlayerInfo.RouteWayPoints(CurWPNUm), True)
                            'TODO Handle proper computation of the WP point in the WP range

                            CurWPDest = _PlayerInfo.RouteWayPoints(CurWPNUm)(0)(0)
                        End If

                        Tc.EndPoint = CurWPDest
                        Dim CapOrtho As Double = Tc.TrueCap
                        Dim Angle As Double
                        Dim MaxSpeed As Double
                        Dim BestAngle As Double
                        Dim dir As Integer
                        Dim WPDist As Double = Tc.SurfaceDistance

                        For Angle = 0 To 90 Step 0.5

                            For dir = -1 To 1 Step 2
                                BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho + Angle * dir, Mi.Dir), Mi.Strength, 0)

                                If BoatSpeed * Math.Cos(Angle / 180 * Math.PI) > MaxSpeed Then
                                    MaxSpeed = BoatSpeed * Math.Cos(Angle / 180 * Math.PI)
                                    BestAngle = CapOrtho + Angle * dir
                                End If
                            Next

                        Next
                        CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(BestAngle))

                        'Check WP completion
                        Tc.EndPoint = CurPos
                        If Tc.SurfaceDistance > WPDist Then
                            'WP Reached change for next WP
                            If CurWPNUm = -1 Then
                                CurWPNUm = RouteurModel.CurWP
                            Else
                                CurWPNUm += 1
                                If CurWPNUm >= RouteurModel.WPList.Count Then
                                    Return
                                End If

                            End If
                        End If



                    Case Else
                        Return

                End Select

                CurDate = CurDate.AddMinutes(RouteurModel.VacationMinutes)
            End While

            PrevMode = OrderType
            PrevValue = OrderValue

            If Not Mi Is Nothing Then
                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}
                PilototoRoute.Add(P)
            End If

            CurIndex += 1
        End While

    End Sub


    Public Property CurrentRoute() As TravelCalculator
        Get
            Return _CurrentRoute
        End Get
        Set(ByVal value As TravelCalculator)
            _CurrentRoute = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentRoute"))
        End Set
    End Property

    Public Property CurWPDist() As Double
        Get
            Return _CurWPDist
        End Get
        Set(ByVal value As Double)
            _CurWPDist = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurWPDist"))

        End Set
    End Property

    Public ReadOnly Property DiffEvolution() As ObservableCollection(Of Decimal)
        Get
            Return _DiffEvolution
        End Get
    End Property

    Private Sub dofriends(ByVal key As String, ByVal List As userBoat2())

        Dim L As SortedList(Of String, String)


        For Each boat In List
            If Not _FriendsGraph.ContainsKey(boat.pseudo) Then

                L = New SortedList(Of String, String)
                _FriendsGraph.Add(boat.pseudo, L)
            Else
                L = _FriendsGraph(boat.pseudo)
            End If
            If Not L.ContainsKey(key) Then
                L.Add(key, key)

            End If
        Next
    End Sub

    Public Sub DrawBoatMap()

        Dim i As Integer


        For i = 0 To NB_WORKERS
            _BoatsToTest(i) = New eHashTable
        Next

        'Init the boats to test list

        SyncLock _Opponents
            _Opponents.Clear()
        End SyncLock



        For i = 0 To NB_WORKERS
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf DrawBoatMapWorker, i)
        Next

    End Sub

    Private Sub DrawBoatMapWorker(ByVal state As Object)

        Dim Id As Integer = CInt(state)
        Dim CurFistBoat As Integer = 1
        Dim PageEmpty As Boolean = False
        Dim wc As New WebClient
        Dim Cookies As CookieContainer = Login()

        Do
            Dim Page As String = GetHTTPResponse(RouteurModel.BASE_GAME_URL & "/races.php?lang=fr&type=racing&idraces=" & _UserInfo.RaceID & "&startnum=" & CurFistBoat, Cookies)

            PageEmpty = ParseRanking(Page, Cookies)

            CurFistBoat += 100


        Loop Until PageEmpty

        AddLog("Worker " & Id & " complete")
        OnBoatWorkerComplete()

    End Sub

    Public Property ETA() As DateTime
        Get
            Return _ETA
        End Get
        Set(ByVal value As DateTime)
            _ETA = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ETA"))
        End Set
    End Property

    Private Delegate Sub dlgthreadsafesetter(ByVal Pv As ObservableCollection(Of clsrouteinfopoints), ByVal Value As ObservableCollection(Of clsrouteinfopoints), ByVal e As PropertyChangedEventArgs, ByVal RefreshBoatInfo As Boolean, ByVal meteo As clsMeteoOrganizer)

    Private Sub ThreadSafeSetter(ByVal Pv As ObservableCollection(Of clsrouteinfopoints), ByVal Value As ObservableCollection(Of clsrouteinfopoints), ByVal e As PropertyChangedEventArgs, ByVal RefreshBoatInfo As Boolean, ByVal meteo As clsMeteoOrganizer)

        Static SelfDlg As New dlgthreadsafesetter(AddressOf ThreadSafeSetter)

        If Not System.Threading.Thread.CurrentThread Is Application.Current.Dispatcher.Thread Then
            Application.Current.Dispatcher.BeginInvoke(SelfDlg, New Object() {Pv, Value, e, RefreshBoatInfo, meteo})

        Else

            _ThreadSetterCurPos.Lon_Deg = _UserInfo.position.longitude
            _ThreadSetterCurPos.Lat_Deg = _UserInfo.position.latitude
            Pv.Clear()
            For Each V In Value
                Pv.Add(V)
                '    V.RefreshTC(_ThreadSetterCurPos)
            Next

            'If RefreshBoatInfo Then
            '    getboatinfo(meteo)
            'End If
            RaiseEvent PropertyChanged(Me, e)


        End If
    End Sub

    Public ReadOnly Property Track() As String
        Get
            If Not _Track Is Nothing Then
                Dim C As New Coords(UserInfo.position.latitude, UserInfo.position.longitude)
                Dim CurPos As String = C.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "!" & C.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ";"
                If Not _Track.EndsWith(CurPos) Then
                    _Track &= CurPos
                End If
                Return _Track
            Else
                _Track = ""
                Try
                    If System.IO.File.Exists(GetTrackFileName) Then
                        Dim S As New System.IO.StreamReader(GetTrackFileName)
                        _Track = S.ReadToEnd
                        S.Close()
                    End If
                Catch ex As Exception
                    _Track = Nothing
                End Try
                Return _Track

            End If
        End Get
    End Property

    Public Function StorePath(ByVal C As Coords, ByVal cap As Double) As String

        Static LastC As New Coords
        Static lastcap As Double
        Static LastStore As DateTime
        Dim Trace As System.IO.StreamWriter = Nothing
        Dim RetString As String = ""
        Try
            If PosValide AndAlso (cap <> lastcap OrElse Now.Subtract(LastStore).TotalHours > 1) Then
                Trace = New System.IO.StreamWriter(GetTrackFileName, True)
                RetString = C.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "!" & C.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ";"
                Trace.Write(RetString)
                _Track &= RetString
                LastC.Lon = C.Lon
                LastC.Lat = C.Lat
                lastcap = cap
                LastStore = Now
            End If
        Catch ex As Exception
        Finally
            If Not Trace Is Nothing Then
                Trace.Close()
            End If
        End Try
        Return RetString

    End Function


    Public ReadOnly Property Log() As ObservableCollection(Of String)
        Get
            Return _Log
        End Get
    End Property

#If BOTS = 1 Then
    Public ReadOnly Property Pilote() As clsPilote
        Get
            Return _Pilote
        End Get
    End Property
#End If



    Public ReadOnly Property PlannedRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            If _UserInfo Is Nothing OrElse _UserInfo.position Is Nothing Then
                Return Nothing
            End If

            If _PlannedRoute.Count = 0 Then
                Dim P As New clsrouteinfopoints() With {.P = New Coords(_UserInfo.position.latitude, UserInfo.position.longitude)}

                '_PlannedRoute.Add(P)
                For Each C In _PlayerInfo.Route
                    P = New clsrouteinfopoints() With {.P = C}
                    _PlannedRoute.Add(P)
                Next
                'Else
                '    _PlannedRoute(0).P.Lat_Deg = _UserInfo.position.latitude
                '    _PlannedRoute(0).P.Lon_Deg = _UserInfo.position.longitude
            End If
            Return _PlannedRoute
        End Get
    End Property

    Public Property PlayerInfo() As clsPlayerInfo
        Get
            Return _PlayerInfo
        End Get
        Set(ByVal value As clsPlayerInfo)
            If _PlayerInfo IsNot value Then
                _PlayerInfo = value
                If PlayerInfo.Route.Count >= 1 Then
                    CurrentRoute.EndPoint = PlayerInfo.Route(0)
                Else
                    CurrentRoute.EndPoint = Nothing
                End If
                'Login()
                getboatinfo(_Meteo)
            End If
        End Set
    End Property

    Public ReadOnly Property TempRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _TempRoute
        End Get
    End Property

    Private WriteOnly Property TempRoute(ByVal meteo As clsMeteoOrganizer) As ObservableCollection(Of clsrouteinfopoints)
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            ThreadSafeSetter(_TempRoute, value, New PropertyChangedEventArgs("TempRoute"), False, meteo)
        End Set
    End Property

    Public Property VMG() As Double
        Get
            Return _VMG
        End Get
        Set(ByVal value As Double)
            _VMG = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("VMG"))
        End Set
    End Property
    Public Property WindCoords() As Coords
        Get
            Return _WindCoords


        End Get

        Set(ByVal value As Coords)
            _WindCoords = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindCoords"))
        End Set
    End Property


    Public ReadOnly Property WindDir() As Double
        Get

            Dim MI As MeteoInfo
            Dim C As New Coords

            If _UserInfo Is Nothing Then
                Return 0
            End If

            C.Lat_Deg = _UserInfo.position.latitude
            C.Lon_Deg = _UserInfo.position.longitude

            MI = _Meteo.GetMeteoToDate(C, Now, False)

            Return MI.Dir

        End Get

    End Property



    Public ReadOnly Property GridRoute() As Queue(Of RoutingGridPoint)
        Get
            If Not _gr Is Nothing Then
                Return _gr.TodoList
            Else
                Return Nothing
            End If


        End Get
    End Property

    Public Property MeteoInfoList() As ObservableCollection(Of RoutingGridPoint)
        Get
            Return _MeteoUnderMouse
        End Get
        Set(ByVal value As ObservableCollection(Of RoutingGridPoint))

        End Set
    End Property

    Public Sub MouseOver(ByVal C As Coords)

        Dim TC As New TravelCalculator
        Dim StartCount As Integer = _BoatUnderMouse.Count
        TC.StartPoint = C
        SyncLock _BoatUnderMouse
            _BoatUnderMouse.Clear()
            SyncLock _Opponents
                For Each O In Opponents.Values
                    TC.EndPoint = O.CurPos
                    If TC.SurfaceDistance < 1 Then
                        _BoatUnderMouse.Add(O)
                    End If

                Next
            End SyncLock
        End SyncLock

        If Not HideWindArrow AndAlso Not _gr Is Nothing Then

            _MeteoUnderMouse.Clear()
            Dim C2 As New Coords(C)
            C2.RoundTo(RouteurModel.GridGrain)

            If _gr.GridPointsList.ContainsKey(C2) Then
                _MeteoUnderMouse.Add(_gr.GridPointsList(C2))
            End If

        End If

        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        If StartCount <> _BoatUnderMouse.Count Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfoVisible"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfo"))
        End If

        'If GSHHS_Reader.HitTest(C, 0, GSHHS_Reader.Polygons(C), True) Then
        '    AddLog(C.ToString & " is inland")
        'Else
        '    AddLog(C.ToString & " is in sea")
        'End If

    End Sub

    Public ReadOnly Property BoatInfoVisible() As Boolean
        Get
            Return _BoatUnderMouse.Count > 0
        End Get
    End Property

    Public Property BoatInfoList() As System.Collections.IList
        Get
            Return _BoatUnderMouse
            
        End Get
        Set(ByVal value As System.Collections.IList)

            '_BoatUnderMouse = value

        End Set
    End Property



    Private Function STR_GetUserInfo(Optional ByVal IsLogin As Boolean = False, Optional ByVal user As String = "", Optional ByVal password As String = "") As String

        'Dim code As String = GetSHACheckSum("get_user.php" & _PlayerInfo.Nick & _Pass & _U)
        'Return RouteurModel.BASE_GAME_URL & "/get_user.php?&checksum=" & code & "&clef=" & _Pass & "&pseudo=" & _PlayerInfo.Nick
        If IsLogin Then
            If user = "" Then
                Return RouteurModel.BASE_GAME_URL & "/myboat.php?pseudo=" & _PlayerInfo.Nick & "&password=" & _PlayerInfo.Password & "&lang=fr&type=login"
            Else
                Return RouteurModel.BASE_GAME_URL & "/myboat.php?pseudo=" & user & "&password=" & password & "&lang=fr&type=login"
            End If

        Else
            Return RouteurModel.BASE_GAME_URL & "/getinfo.php?idu=" & _PlayerInfo.NumBoat & "&pseudo=" & _PlayerInfo.Nick & "&password=" & _PlayerInfo.Password
        End If
    End Function


    Private Function Login(Optional ByVal User As String = "", Optional ByVal Password As String = "") As CookieContainer

        Dim LoginURL As String = STR_GetUserInfo(True, User, Password)
        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(LoginURL), HttpWebRequest)
        Dim wr As WebResponse
        Dim Encoder As New System.Text.ASCIIEncoding
        Dim rs As System.IO.StreamReader
        Dim Response As String
        Http.CookieContainer = New CookieContainer
        wr = Http.GetResponse()
        rs = New System.IO.StreamReader(wr.GetResponseStream())
        Response = rs.ReadToEnd
        If Response.Contains("Access restricted") Then
            Return Nothing
        Else
            Return Http.CookieContainer
        End If

    End Function

    Private Function ParsePlayPhpPage(ByVal rs As String) As Boolean

        Dim i As Integer = rs.IndexOf("?id_user=")
        If i = -1 Then
            Return False
        End If

        Dim S As String = rs.Substring(i + 9)
        Dim i2 As Integer = S.IndexOf("&checksum")
        If i2 = -1 Then
            Return False
        End If
        Dim i3 As Integer = S.Substring(i2 + 10).IndexOf("&")
        If i3 = -1 Then
            Return False
        End If
        _Login = S.Substring(0, i2)
        _Code = S.Substring(i2 + 10, i3)
        Return True

    End Function

    Private Function ParseRanking(ByVal Page As String, ByVal cookies As CookieContainer) As Boolean
        Dim RankingIndex As Integer
        Dim RetVal As Boolean = True
        Dim CurString As String = Page
        Dim Boat As String
        RankingIndex = CurString.IndexOf("<tr class=""ranking"">")
        If RankingIndex = -1 Then
            Return True
        End If
        CurString = CurString.Substring(RankingIndex)
        Do
            RankingIndex = CurString.IndexOf("<tr")

            If RankingIndex <> -1 Then
                CurString = CurString.Substring(RankingIndex + 3)

                Dim TitleIndex As Integer = CurString.IndexOf("title=""")
                Boat = CurString.Substring(TitleIndex + 7, CurString.Substring(TitleIndex + 7).IndexOf(""""))

                Dim LatIndex As Integer = CurString.IndexOf("lat=")
                Dim LonIndex As Integer = CurString.IndexOf("long=")
                Dim Lat As Double
                Dim Lon As Double
                Dim ClassIndex As Integer = CurString.IndexOf("<td>") + 4
                Dim Classement As Integer
                Dim ImgIndex As Integer = CurString.IndexOf("/flagimg.php")
                Dim ImgName As String = CurString.Substring(ImgIndex + 21)
                ImgName = ImgName.Substring(0, ImgName.IndexOf(""""))

                Double.TryParse(CurString.Substring(LatIndex + 4, CurString.Substring(LatIndex + 4).IndexOf("&")), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat)
                Double.TryParse(CurString.Substring(LonIndex + 5, CurString.Substring(LonIndex + 5).IndexOf("&")), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)

                Integer.TryParse(CurString.Substring(ClassIndex, CurString.IndexOf("</td>") - ClassIndex), _
                                System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Classement)

                RetVal = False

                Dim BInfo As New BoatInfo
                With BInfo
                    .CurPos.Lat_Deg = Lat
                    .CurPos.Lon_Deg = Lon
                    .Name = Boat
                    .Classement = Classement

                    If Not BoatInfo.ImgList.ContainsKey(ImgName) Then
                        'Console.WriteLine("Ask " & ImgName & " for " & .Name)
                        BoatInfo.ImgList.Add(ImgName, Nothing)
                        'If Img Is Nothing Then
                        '    Console.WriteLine("Request failed for " & ImgName)
                        'End If

                    End If

                    .Flagname = ImgName
                    '.Flag = http
                    '.ProOption = B.position.option_voiles_pro = 1 Or B.position.option_program = 1 Or B.position.option_regulateur = 1 Or B.position.option_voile_auto = 1 _
                    '            Or B.option_repair_kits = 1 Or B.position.option_assistance = 1 Or B.position.option_waypoints = 1 Or B.full_option = 1
                    '.Classement = B.position.classement
                    'If Double.TryParse(B.position.temps_etape, Temps) Then
                    '    .Temps = DepartEtape.AddSeconds(Temps)
                    'End If
                End With

                SyncLock _Opponents
                    If Not _Opponents.ContainsKey(BInfo.Name) Then
                        Try
                            'Console.WriteLine("Add" & BInfo.Name)
                            _Opponents.Add(BInfo.Name, BInfo)
                        Catch
                        End Try

                    End If

                End SyncLock

            End If
            'CurString = CurString.Substring(RankingIndex + 20)
        Loop Until RankingIndex = -1

        Return RetVal
    End Function

    Private Function ParseVersionAndPass(ByVal rs As String) As Boolean

        Dim i1 As Integer = rs.IndexOf("application.swf?version=")

        Dim i As Integer = rs.IndexOf("&id_user=")
        If i = -1 Then
            Return False
        End If

        Dim vString As String = rs.Substring(i1 + 24, i - i1 - 24)

        Dim S As String = rs.Substring(i + 9)
        Dim i2 As Integer = S.IndexOf("&pass")
        If i2 = -1 Then
            Return False
        End If
        Dim i3 As Integer = S.Substring(i2 + 6).IndexOf("&")
        If i3 = -1 Then
            Return False
        End If
        RouteurModel.URL_Version = CInt(vString)
        _Version = "17"
        _Pass = S.Substring(i2 + 6, i3)
        Dim i4 As Integer = S.IndexOf("&U=")
        Dim i5 As Integer = S.IndexOf("""")
        If i4 = -1 Or i5 = -1 Then
            Return False
        End If
        _U = S.Substring(i4 + 3, i5 - i4 - 3)
        Return True

    End Function

    Private Function ParseVLMBoatInfoString(ByVal Str As String) As user

        Dim lines() As String = Str.Split(CChar(vbLf))
        Dim TmpDbl As Double
        Dim TmpInt As Integer
        Dim RetUser As New user
        Dim Lup As Double
        Dim Nup As Double
        Dim Loch As Double

        RetUser.position = New userPosition
        PosValide = False

        For Each Line In lines

            Line = Line.Trim
            Dim EqualPos As Integer = Line.Trim.IndexOf("="c)

            If Line.Trim.Length > 0 AndAlso EqualPos = 3 Then
                'Line = Line.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator)
                Select Case Line.Substring(0, 4)

                    Case "BSP="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl)
                        RetUser.position.vitesse = CType(TmpDbl, Decimal)

                    Case "DNM="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl)
                        If TmpDbl <= 0.25 Then
                            RouteurModel.CurWP += 1
                        End If

                    Case "HDG="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.cap)

                    Case "LAT="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.latitude)
                        RetUser.position.latitude /= 1000

                    Case "LOC="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Loch)
                        RetUser.Loch = Loch

                    Case "LON="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.longitude)
                        RetUser.position.longitude = RetUser.position.longitude / 1000

                    Case "LUP="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lup)

                        If Not _PlayerInfo.ShowAutorouting AndAlso (_PlayerInfo.Nick = "sbs" OrElse _PlayerInfo.Nick = "sbs_2") Then
                            _PlayerInfo.ShowAutorouting = True
                        End If
                    Case "PIM="
                        RetUser.position.ModePilote = CInt(Line.Substring(4))

                    Case "PIP="
                        If Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl) Then
                            If RetUser.position.ModePilote = 2 Then
                                RetUser.position.AngleAllure = TmpDbl
                            End If
                        End If

                    Case "POL="
                        RetUser.type = Line.Substring(4)

                    Case "NUP="
                        Static NbSwitch As Integer = 0

                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Nup)
                        If Nup < -10 AndAlso Loch > 0 Then
                            If RouteurModel.BASE_GAME_URL = RouteurModel.S10_SERVER Then
                                RouteurModel.BASE_GAME_URL = RouteurModel.S11_SERVER
                                AddLog(TmpDbl & "NUP Wrong switch to S11 : " & Nup)
                                _CookiesContainer = Nothing
                            Else
                                RouteurModel.BASE_GAME_URL = RouteurModel.S10_SERVER
                                AddLog(TmpDbl & "NUP Wrong switch to S10" & Nup)
                                _CookiesContainer = Nothing
                            End If

                            NbSwitch += 1
                            Nup = 5 * 2 ^ NbSwitch

                            If Nup / 60 > RouteurModel.VacationMinutes Then
                                Nup = 60 * RouteurModel.VacationMinutes
                            End If

                        Else
                            PosValide = True
                            NbSwitch = 0
                        End If
                        RetUser.date = Now.AddSeconds(+Nup - RouteurModel.VacationMinutes * 60)

                    Case "NWP="
                        Integer.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpInt)
                        RouteurModel.CurWP = TmpInt

                    Case "POS="
                        Double.TryParse(Line.Substring(4, Line.IndexOf("/"c) - 4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl)
                        RetUser.position.classement = CInt(TmpDbl)

                    Case "TWD="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.wind_angle)

                    Case "RAC="
                        If RouteurModel.GRID_FILE = "" Then
                            RouteurModel.GRID_FILE = "VLM-" & Line.Substring(4) & ".csv"
                        End If

                        RetUser.RaceID = Line.Substring(4)

                    Case "TWS="
                        Double.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl)
                        RetUser.position.wind_speed = CType(TmpDbl, Decimal)

                    Case "VAC="
                        Integer.TryParse(Line.Substring(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpInt)
                        RouteurModel.VacationMinutes = TmpInt / 60

                    Case "no-d" 'no-data:not engaged on any race
                        RetUser.position.latitude = RouteurModel.START_LAT
                        RetUser.position.longitude = RouteurModel.START_LON


                End Select
            ElseIf EqualPos = 4 AndAlso Line.Substring(0, 3) = "PIL" Then

                Dim PilIndex As Integer = CInt(Line.Substring(3, 1))
                _Pilototo(PilIndex) = Line.Substring(5)

            ElseIf EqualPos = 5 AndAlso Line.StartsWith("WPL") Then

                Select Case Line.Substring(0, 5)
                    Case "WPLAT"
                        If Double.TryParse(Line.Substring(6), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl) Then
                            _WayPointDest.Lat_Deg = TmpDbl
                        End If

                    Case "WPLON"
                        If Double.TryParse(Line.Substring(6), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, TmpDbl) Then
                            _WayPointDest.Lon_Deg = TmpDbl
                        End If

                End Select

            End If
        Next

        If Loch = 0 Then
            RetUser.date = Now
        End If

        RetUser.trajectoire = Track
        RetUser.DateOffset = Now.AddHours(-System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours).Subtract(New Date(1970, 1, 1).AddSeconds(Lup + RouteurModel.VacationMinutes * 60 - Nup)).TotalSeconds

        If Not _UserInfo Is Nothing AndAlso Not _UserInfo.position Is Nothing Then
            RetUser.position.diffClassement = _UserInfo.position.classement - RetUser.position.classement
        Else
            RetUser.position.diffClassement = 0
        End If

        Return RetUser
    End Function

    Public Shared Function GetHTTPResponse(ByVal url As String, ByVal Cookies As CookieContainer) As String

        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)

        Http.CookieContainer = Cookies
        Dim WR As WebResponse = Http.GetResponse()
        Dim ResponseStream As New System.IO.StreamReader(WR.GetResponseStream)
        Dim Response As String = ResponseStream.ReadToEnd
        ResponseStream.Close()

        Return Response

    End Function

    Public Shared Function GetHttpImage(ByVal url As String, ByVal cookies As CookieContainer) As BitmapImage
        Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)

        Http.CookieContainer = cookies
        Try
            Dim WR As WebResponse = Http.GetResponse()
            Dim Response As New BitmapImage()
            Response.BeginInit()
            'Dim ImgData(CInt(WR.ContentLength) - 1) As Byte
            'WR.GetResponseStream.Read(ImgData, 0, WR.ContentLength)
            'Response.CreateOptions = BitmapCreateOptions.None
            Response.CacheOption = BitmapCacheOption.OnLoad
            Response.StreamSource = WR.GetResponseStream
            Response.EndInit()

            'While Not Response.IsDownloading
            '    System.Threading.Thread.Sleep(1)
            'End While
            'While Response.IsDownloading
            '    System.Threading.Thread.Sleep(1)
            'End While
            'SR.Close()
            Return Response
        Catch ex As Exception
        End Try

        Return Nothing

    End Function

    Public Shared Function GetSHACheckSum(ByVal HashString As String) As String

        Dim sha As New System.Security.Cryptography.SHA1CryptoServiceProvider
        Dim Buffer1() As Byte = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(HashString)

        'Dim Buffer2() As Byte
        'If Not Pass Is Nothing Then
        '    Buffer2 = System.Text.Encoding.ASCII.GetBytes(Pass)
        'Else
        '    Buffer2 = System.Text.Encoding.ASCII.GetBytes("How would I know?")
        'End If
        'Dim Buffer5() As Byte = System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(nick)
        'Dim Buffer3(Buffer1.Length + Buffer5.Length + Buffer2.Length - 1) As Byte
        'Array.Copy(Buffer1, Buffer3, Buffer1.Length)
        'Array.Copy(Buffer5, 0, Buffer3, Buffer1.Length, Buffer5.Length)
        'Array.Copy(Buffer2, 0, Buffer3, Buffer1.Length + Buffer5.Length, Buffer2.Length)
        Dim buffer4() As Byte = sha.ComputeHash(Buffer1)

        Dim sb As New System.Text.StringBuilder

        For Each b As Byte In buffer4
            sb.Append(b.ToString("X2"))
        Next

        Dim retstring As String = sb.ToString

        Return retstring.ToLowerInvariant

    End Function

    Private Function GetTrackFileName() As String

        If _UserInfo Is Nothing Then
            Return Nothing
        End If

        Dim RetValue As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur\track_" & _UserInfo.RaceID & ".dat"

        If Not System.IO.Directory.Exists(Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur") Then
            System.IO.Directory.CreateDirectory(Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur")
        End If

        Return RetValue

    End Function

    Private Sub OnBoatWorkerComplete()

        System.Threading.Interlocked.Decrement(_Workerscount)
        If _Workerscount = 0 Then

            RaiseEvent BoatShown()


        End If

    End Sub
    Public ReadOnly Property Opponents() As Dictionary(Of String, BoatInfo)
        Get
            Return _Opponents
        End Get
    End Property


    Public ReadOnly Property PositionDataAge() As TimeSpan
        Get
            Return Now.Subtract(LastDataDate)
        End Get
    End Property

    Public Sub getboatinfo()
        getboatinfo(_Meteo)
    End Sub

    Public Sub GetBoatInfo(ByVal meteo As clsMeteoOrganizer, Optional ByVal force As Boolean = False)

        Dim ErrorCount As Integer = 0
        Dim ResponseString As String = ""


        If _PlayerInfo Is Nothing Then
            Return
        End If

#If BOTS = 1 Then
        If _PiloteCleared AndAlso _PlayerInfo.AutoRouting AndAlso _UserInfo.date.AddMinutes(RouteurModel.VacationMinutes).Subtract(Now).TotalSeconds < 30 Then

            'If _cookiesContainer Is Nothing Then
            _CookiesContainer = Login()
            'End If
            If _CookiesContainer Is Nothing Then
                Return
            End If
            _Pilote.CurrentRoute = BruteRoute
            Dim Res = _Pilote.CheckDirectionAndSail(_PlayerInfo.NumBoat, _CookiesContainer)
            _PiloteCleared = False
            Select Case Res
                Case clsPilote.enumroutingcheck.NOK
                    _AutoRouteFailures += 1
                    AddLog("Auto routing NOK " & _AutoRouteFailures)
                    _CookiesContainer = Nothing
                    If _AutoRouteFailures > 5 Then
                        _PlayerInfo.AutoRouting = False
                    End If
                Case clsPilote.enumroutingcheck.OffCourse

                    AddLog("Je suis perdu HORS COURSE, je continue...")

            End Select
        End If
#End If

        If Not force AndAlso Not _UserInfo Is Nothing AndAlso Now.Subtract(_UserInfo.date).TotalMinutes < RouteurModel.VacationMinutes Then
            Return
        End If

        Try

            ResponseString = _WebClient.DownloadString(STR_GetUserInfo)

            UserInfo(meteo) = ParseVLMBoatInfoString(ResponseString)

#If BOTS = 1 Then
            _PiloteCleared = True
#End If


            Dim VLMInfoMessage As String = "Meteo read D:" & UserInfo.position.wind_angle.ToString("f2")
            Dim i As Integer = 0
            Dim STart As DateTime = Now.AddSeconds(-UserInfo.DateOffset)
            If Math.Abs(Now.Subtract(STart).TotalHours) > 24 Then
                STart = Now
            End If
            Dim mi As MeteoInfo = meteo.GetMeteoToDate(New Coords(UserInfo.position.latitude, UserInfo.position.longitude), STart, False)

            If Not mi Is Nothing Then
                Dim V As Double = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, VOR_Router.WindAngle(_UserInfo.position.cap, _UserInfo.position.wind_angle), _UserInfo.position.wind_speed, _UserInfo.position.voiles_cassees)
                If Math.Abs(UserInfo.position.wind_angle - mi.Dir) > 0.1 Then
                    VLMInfoMessage &= " (exp.:" & mi.Dir.ToString("f2") & ")"
                End If

                VLMInfoMessage &= " S:" & UserInfo.position.wind_speed.ToString("f2")
                If Math.Abs(UserInfo.position.wind_speed - mi.Strength) > 0.05 Then
                    VLMInfoMessage &= " (exp: " & mi.Strength.ToString("f2") & ")"
                End If

                VLMInfoMessage &= " Speed:" & UserInfo.position.vitesse.ToString("f2")
                If Math.Abs(UserInfo.position.vitesse - V) > 0.05 Then
                    VLMInfoMessage &= " (exp: " & V.ToString("f2") & ")"
                End If
                AddLog(VLMInfoMessage)
            Else
                AddLog("No Meteo data !!")
            End If
            _LastCommunication = Now

            If Now.Subtract(_lastflush).TotalMinutes > 15 Then

                Dim BaseDir As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur"
                If Not System.IO.Directory.Exists(BaseDir) Then
                    System.IO.Directory.CreateDirectory(BaseDir)
                End If

                Dim S1 As New System.IO.StreamWriter(BaseDir & "\TempRoute" & Now.Minute Mod 7 & ".csv")

                For Each P In TempRoute
                    S1.WriteLine(P.ToString)
                Next

                S1.Close()

                Dim S2 As New System.IO.StreamWriter(BaseDir & "\BestRoute" & Now.Minute Mod 7 & ".csv")

                For Each P In BruteRoute
                    S2.WriteLine(P.ToString)
                Next

                S2.Close()

                _lastflush = Now
            End If

            StorePath(New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude), UserInfo.position.cap)

            If _RoutingRestartPending Then
                _RoutingRestart.Interval = 100
                _RoutingRestart.Start()
            End If

            If DrawOpponnents Then
                RaiseEvent BoatClear()
                DrawBoatMap()
            End If

            If _CurUserWP = 0 Then
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurUserWP "))
            End If

            Dim PilototoThread As New System.Threading.Thread(AddressOf ComputePilototo)
            PilototoThread.Start()
            ComputeAllure()

        Catch ex As Exception
            AddLog("GetboartInfo : " & ex.Message)
        End Try

        If _DebugHttp Then
            Debug.WriteLine("GetBoatInfo " & ResponseString)
        End If


    End Sub

    Public Property LastDataDate() As DateTime
        Get
            Return _LastDataDate
        End Get
        Set(ByVal value As DateTime)

            If value <> _LastDataDate Then
                _LastDataDate = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LastDataDate"))
            End If
        End Set
    End Property

    Public Sub RedrawComplete()

        If Not _gr Is Nothing Then
            _gr.StepGrid()
        End If

    End Sub

    Public Sub RefreshTimes()


        getboatinfo(_Meteo)

        'AddLog("synclock refreshtimes " & System.Threading.Thread.CurrentThread.ManagedThreadId)
        SyncLock _LogQueue

            While _LogQueue.Count > 0

                _Log.Insert(0, _LogQueue.Dequeue)
                If _Log.Count = 200 Then
                    _Log.RemoveAt(199)
                End If

            End While
        End SyncLock
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Log"))

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PositionDataAge"))


    End Sub

    Private Sub SetUserInfoThreadSafe(ByVal value As user, ByVal Meteo As clsMeteoOrganizer)
        Static SelfDlg As New Action(Of user, clsMeteoOrganizer)(AddressOf SetUserInfoThreadSafe)
        Dim MI As MeteoInfo
        Dim P As Coords
        Static LastPos As New Coords
        Static bInvoking As Boolean = False
        Static PrevPos As New Coords

        If value Is Nothing Then
            Return
        End If

        If Not System.Threading.Thread.CurrentThread Is Application.Current.Dispatcher.Thread Then
            If Not bInvoking Then
                'bInvoking = True
                Application.Current.Dispatcher.BeginInvoke(SelfDlg, New Object() {value})

            End If

        Else

            _UserInfo = value

            If _UserInfo.position Is Nothing Then
                _UserInfo.position = New userPosition
                _UserInfo.position.longitude = RouteurModel.START_LON
                _UserInfo.position.latitude = RouteurModel.START_LAT
                _UserInfo.position.date = Now.ToString
                UserInfo(Meteo) = _UserInfo
            End If

            LastDataDate = _UserInfo.date 'DateConverter.GetDate(_UserInfo.position.date)

            If Now.Subtract(LastDataDate).TotalHours > 1 Then
                LastDataDate = Now
                _UserInfo.date = Now
                If _UserInfo.Loch > 0 Then
                    AddLog("Data too old falling back to now : " & LastDataDate)
                End If
            End If

            If CurrentRoute.StartPoint Is Nothing Then
                CurrentRoute.StartPoint = New Coords
            End If

            CurrentRoute.StartPoint.Lat_Deg = _UserInfo.position.latitude
            CurrentRoute.StartPoint.Lon_Deg = _UserInfo.position.longitude

            If PlayerInfo.Route.Count >= 1 Then
                _RouteToGoal.StartPoint = CurrentRoute.StartPoint
                _RouteToGoal.EndPoint = PlayerInfo.Route(PlayerInfo.Route.Count - 1)
            End If

            VMG = _UserInfo.position.vitesse * Math.Cos((_UserInfo.position.cap - CurrentRoute.Cap) / 180 * Math.PI)
            If VMG <> 0 Then
                ETA = Now.AddHours(CurrentRoute.SurfaceDistance / VMG)
            Else
                ETA = Now
            End If

            If UserInfo.full_option = 0 And UserInfo.position.option_voiles_pro = 0 Then
                BrokenSails = &HFFFFFFF And Not clsSailManager.EnumSail.SPI And Not clsSailManager.EnumSail.JIB
            Else
                BrokenSails = _UserInfo.position.voiles_cassees
            End If


            P = New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude)
            'If _UserInfo.trajectoire Is Nothing Then
            '    _UserInfo.trajectoire = ""
            'End If
            '_UserInfo.trajectoire &= StorePath(P, _UserInfo.position.cap)
            MI = Meteo.GetMeteoToDate(P, Now, False)
            If MI Is Nothing Then
                Return
            End If

#If BOTS = 1 Then
            _Pilote.ActualPosition = New clsrouteinfopoints() With {.P = P, .Cap = _UserInfo.position.cap, _
                                                                    .Sail = CType(_UserInfo.position.voile, clsSailManager.EnumSail), _
                                                                    .WindDir = MI.Dir, .WindStrength = MI.Strength _
                                                                    }



            Dim TC As New TravelCalculator With {.StartPoint = _Pilote.ActualPosition.P}
            Try
                Dim RP As clsrouteinfopoints
                For Each RP In TempRoute
                    TC.EndPoint = RP.P
                    RP.CapFromPos = TC.Cap
                    RP.DistFromPos = TC.SurfaceDistance
                Next
                For Each RP In BruteRoute
                    TC.EndPoint = RP.P
                    RP.CapFromPos = TC.Cap
                    RP.DistFromPos = TC.SurfaceDistance
                Next
            Catch ex As Exception
                AddLog("getBoat route postcompute..." & ex.ToString)
            Finally
                TC.EndPoint = Nothing
                TC.StartPoint = Nothing
                TC = Nothing
            End Try
#End If

            If _DiffEvolution.Count = 0 OrElse _DiffEvolution(0) <> _UserInfo.position.diffClassement Then
                _DiffEvolution.Insert(0, _UserInfo.position.diffClassement)
            End If

            If _DiffEvolution.Count = 6 Then
                _DiffEvolution.RemoveAt(5)
            End If
            'SendInGameMessages()

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UserInfo"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindDir"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurrentRoute"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("VMG"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ETA"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RefreshCanvas"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DiffEvolution"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Log"))
            bInvoking = False
            End If
    End Sub

    Public Property UserInfo() As user
        Get
            Return _UserInfo
        End Get
        Set(ByVal value As user)
            UserInfo(_Meteo) = value
        End Set
    End Property

    Public Sub UpdatePath(ByVal Clear As Boolean)

        If Clear Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ClearGrid"))
        Else
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("TempRoute"))

        End If
    End Sub

    Public Sub UpdateWPDist(ByVal c As Coords)

        If Not _gr Is Nothing Then
            BestRouteAtPoint = _gr.RouteToPoint(c)
        End If

        'todo make this safe
        Dim WP As Integer

        If _CurUserWP = 0 Then
            WP = RouteurModel.CurWP - 1
        Else
            WP = _CurUserWP
        End If

        If WP < 0 OrElse WP >= _PlayerInfo.RouteWayPoints.Count Then
            Return
        End If
        Dim D As Double = GSHHS_Reader.HitDistance(c, _PlayerInfo.RouteWayPoints(WP), False)

        If D = Double.MaxValue Then

            Dim TC As New TravelCalculator
            TC.StartPoint = c
            TC.EndPoint = _PlayerInfo.RouteWayPoints(WP)(0)(0)
            D = TC.SurfaceDistance
            TC.EndPoint = _PlayerInfo.RouteWayPoints(WP)(0)(1)
            Dim D1 As Double = TC.SurfaceDistance
            If D1 < D Then
                D = D1
            End If
            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing
        End If
        CurWPDist = D

    End Sub

    Private WriteOnly Property userinfo(ByVal meteo As clsMeteoOrganizer) As user
        Set(ByVal value As user)
            SetUserInfoThreadSafe(value, meteo)
        End Set

    End Property



    '    Private Function SimpleBestVMGTo(ByVal from As Coords, ByVal dest As Coords, ByVal CurDate As DateTime, ByVal Route As ObservableCollection(Of clsrouteinfopoints), ByVal NoTimeOptimization As Boolean, ByVal Precision As Double, ByVal TargetDistance As Double, ByVal StartTick As Long, ByRef meteo As clsMeteoOrganizer) As Boolean

    '        Dim CurBestDir As Integer
    '        Dim CurVMG As Double
    '        Dim CurSail As clsSailManager.EnumSail
    '        Dim CapLeft As Integer
    '        Dim CurMeteo As MeteoInfo
    '        Dim CurFactor As Double
    '        Dim VMG As Double
    '        Dim CurSpeed As Double
    '        Dim speed As Double
    '        Dim CapOrtho As Double
    '        Dim CurDist As Double
    '        Dim v As clsSailManager.EnumSail
    '        Dim T As New TravelCalculator With {.StartPoint = New Coords(from), .EndPoint = New Coords(dest)}
    '        Dim T2 As New TravelCalculator
    '        Dim PrevLegLength As Double
    '        Dim i As Integer
    '        Dim MaxSpeed As Double
    '        Dim RouteOK As Boolean
    '        Dim MaxRetries As Integer = 0
    '        Static VMGCounter As Long = 0
    '        Dim PrevDir As Integer = -400
    '        Dim HitDistance As Double = GSHHS_Reader.HitDistance(from, GSHHS_Reader.Polygons(from))
    '        Dim P As clsrouteinfopoints = Nothing
    '        Static LastRefresh As DateTime = Now
    '        Dim Tests(360) As Boolean
    '        Dim TestedOne As Boolean

    '        Static MinDistance As Double = Double.MaxValue

    '        If HitDistance < MinDistance Then
    '            '            i = 0
    '            MinDistance = HitDistance
    '        End If
    '        If HitDistance > GSHHS_Reader.MIN_DELTA Then
    '            HitDistance = GSHHS_Reader.MIN_DELTA
    '        Else
    '            Return False
    '        End If



    '        If Route.Count > 0 Then
    '            T2.StartPoint = Route(Route.Count - 1).P
    '        Else
    '            T2.StartPoint = from
    '        End If

    '        CurDist = T.SurfaceDistance
    '        If CurDist <= TargetDistance Then
    '            Return True
    '        End If

    '        If T.StartPoint.Lat_Deg < -65 Or T.StartPoint.Lat_Deg > 65 Then
    '            Return False
    '        End If

    '        If Route.Count > 450 Then
    '            Return False
    '        End If

    '        CurBestDir = CInt(T.Cap)
    '        CapOrtho = T.Cap
    '        CurSail = clsSailManager.Sails(0)
    '        CurMeteo = meteo.GetMeteoToDate(T.StartPoint, CurDate, False)
    '        If CurMeteo Is Nothing Then
    '            'Debug.WriteLine("Meteo not found for " & CurDate & " " & T.StartPoint.ToString)
    '            Return False
    '        End If

    '        MaxSpeed = 100
    '        If _LastBestVMGCalc.TotalMilliseconds < 300 Then
    '            _LastBestVMGCalc = New TimeSpan(0, 0, 1)
    '        ElseIf _LastBestVMGCalc.TotalSeconds > 3 Then
    '            _LastBestVMGCalc = New TimeSpan(0, 0, 3)

    '        End If

    'LOOPSTART:
    '        If StartTick <> 0 AndAlso (Now.Ticks - StartTick) > 4 * _LastBestVMGCalc.Ticks Then
    '            Return False
    '        ElseIf StartTick = 0 And Now.Subtract(LastRefresh).TotalSeconds > 5 Then
    '            LastRefresh = Now
    '            TempVMGRoute = New ObservableCollection(Of clsrouteinfopoints)(Route)
    '        End If

    '        Const DELTA_VMG As Double = 0.0
    '        VMGCounter += 1
    '        MaxRetries += 1

    '        'If MaxRetries > 50 Then
    '        '    Return False
    '        'End If

    '        CurVMG = -40 ' _Sails.GetSpeed(CurSail, WindAngle(CurBestDir, CurMeteo.Dir), CurMeteo.Strength, _UserInfo.position.voiles_cassees)
    '        CurSpeed = -40
    '        'For Each V In clsSailManager.Sails
    '        TestedOne = False
    '        For i = 0 To 360

    '            If Not Tests(CInt(i / 10)) Then
    '                TestedOne = True
    '                CapLeft = i '(CurMeteo.Dir - i + 360) Mod 180
    '                CurFactor = Math.Cos((CapLeft - CapOrtho) / 180 * Math.PI)

    '                speed = _Sails.GetBestSailSpeed(_UserInfo.type, v, WindAngle(CapLeft, CurMeteo.Dir), CurMeteo.Strength, BrokenSails)
    '                VMG = CurFactor * speed
    '                If VMG > CurVMG And VMG < MaxSpeed Then
    '                    CurVMG = VMG
    '                    CurSail = v
    '                    CurBestDir = CapLeft
    '                    CurSpeed = speed
    '                End If
    '            End If

    '        Next

    '        If Not TestedOne And Route.Count > 1 Then
    '            'Undo the route for 1/3 of what has been computer
    '            Dim TargetReturnDist As Double = CInt(Route(Route.Count - 1).DTF) \ 100 * 100 - 100
    '            While Route.Count > 1 AndAlso Route(Route.Count - 1).DTF > TargetReturnDist
    '                Route.RemoveAt(Route.Count - 1)
    '            End While
    '            GC.Collect()
    '            Return False
    '        ElseIf Not TestedOne Then
    '            Return False

    '        End If

    '        'Next
    '        'Debug.WriteLine("CurBestDir " & CurBestDir)
    '        'If CurSpeed = CurVMG And CurSpeed = 0 Then
    '        '    Return False
    '        'End If

    '        If MaxSpeed < -40 Or CurBestDir = PrevDir Then
    '            Return False
    '        End If
    '        Tests(CInt(CurBestDir / 10)) = True
    '        PrevDir = CurBestDir
    '        If CurSpeed = 0 Or CurSpeed = -40 Then
    '            MaxSpeed = CurVMG - DELTA_VMG
    '            GoTo LOOPSTART
    '        End If


    '        If P Is Nothing Then
    '            P = New clsrouteinfopoints
    '            P.P = New Coords
    '        End If

    '        With P
    '            .P.Lat = T.StartPoint.Lat
    '            .P.Lon = T.StartPoint.Lon
    '            .Cap = CurBestDir
    '            .T = CurDate
    '            .Sail = CurSail
    '            .WindDir = CurMeteo.Dir
    '            .WindStrength = CurMeteo.Strength
    '            .Speed = CurSpeed
    '            .DTF = CurDist
    '        End With


    '        'If CurDist < TargetDistance Then
    '        '    Route.Add(P)
    '        '    Return True
    '        'End If

    '        T.FindNextWindChangePoint(CInt(CurBestDir), CurSpeed)

    '        If T.SurfaceDistance > Precision * CurSpeed Then
    '            T.EndPoint = T.ReachDistance(Precision * CurSpeed, CurBestDir)
    '        End If

    '        If CurDist < T.SurfaceDistance Then
    '            T.EndPoint = T.ReachDistance(CurDist, CurBestDir)
    '        End If


    '        If Not T.EndPoint Is Nothing Then
    '            Dim NextDate As DateTime = CurDate.AddHours(T.SurfaceDistance / CurSpeed)
    '            If CrossWindChange(CurDate, NextDate, meteo) Then

    '                T.ReachDistance(NextDate.Subtract(CurDate).Ticks / TimeSpan.TicksPerHour / CurSpeed, CurBestDir)


    '            End If

    '            T2.EndPoint = from
    '            PrevLegLength = T2.SurfaceDistance
    '            T2.EndPoint = T.EndPoint
    '            If T2.SurfaceDistance < PrevLegLength Then
    '                MaxSpeed = CurVMG - DELTA_VMG
    '                'Route.Remove(P)
    '                GoTo LOOPSTART
    '            End If

    '            If HitDistance > GSHHS_Reader.HitDistance(T.EndPoint, GSHHS_Reader.Polygons(T.EndPoint)) Then
    '                MaxSpeed = CurVMG - DELTA_VMG
    '                'Route.Remove(P)
    '                GoTo LOOPSTART
    '            End If
    '            If GSHHS_Reader.HitTest(T.EndPoint, HitDistance, GSHHS_Reader.Polygons(T.EndPoint), True) Then
    '                MaxSpeed = CurVMG - DELTA_VMG
    '                'Route.Remove(P)
    '                GoTo LOOPSTART

    '            End If
    '            Route.Add(P)

    '            'If NextDate > Now.AddHours(48) Then
    '            '    RouteOK = SimpleBestVMGTo(New Coords(T.EndPoint), dest, NextDate, Route, NoTimeOptimization, 6, TargetDistance, StartTick, meteo)
    '            '    'Return RouteOK
    '            'Else

    '            RouteOK = SimpleBestVMGTo(New Coords(T.EndPoint), dest, NextDate, Route, NoTimeOptimization, Precision, TargetDistance, StartTick, meteo)
    '            'Return RouteOK
    '            'End If

    '            If Not RouteOK AndAlso Route(Route.Count - 1).P = from Then
    '                MaxSpeed = CurVMG - DELTA_VMG
    '                Route.Remove(P)
    '                GoTo LOOPSTART
    '            Else
    '                Return RouteOK
    '            End If

    '        Else
    '            Return False
    '        End If


    '    End Function

    Private Sub SendInGameMessages()

        If _UserInfo.messages.Count > 0 Then
            'Dim Mailer As New System.Net.Mail.SmtpClient()
            Dim BodyString As String = ""

            For Each msg In _UserInfo.messages
                BodyString = BodyString & "Msg from $" & msg.sender.Value & "$ --> " & msg.text & vbCrLf
                AddLog("Msg from $" & msg.sender.Value & "$ --> " & msg.text)
            Next

            'Mailer.Port = 465
            'Mailer.Host = "smtp.gmail.com"
            'Mailer.EnableSsl = True
            'Mailer.Credentials = New NetworkCredential("sbarthes@gmail.com", "disk+bolt")
            'Mailer.Timeout =

            'Mailer.SendAsync("VR Routeur", "stephane.barthes@intest.info", "InGame Message", BodyString, Nothing)
            'Mailer.Send("sbarthes@gmail.com", "stephane.barthes@intest.info", "InGame Message", BodyString)
            'ad()

        End If

    End Sub



    Public Sub startGridRoute(ByVal StartRouting As Boolean)



        If StartRouting AndAlso Now.Subtract(_LastGridRouteStart).TotalMinutes > RouteurModel.VacationMinutes Then
            _LastGridRouteStart = Now
            Dim start As New Coords(New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude))
            Dim Tc As New TravelCalculator()
            Static currentmgr As New CurrentMgr


            _StopRouting = False


            Tc.StartPoint = start



            If _gr Is Nothing Then
                _gr = New GridRouter(start, LastDataDate, _Meteo, _Sails, _UserInfo.type)
            Else
                _gr.Start(LastDataDate) = start
            End If

            _GridProgressWindow.Show(_owner, _gr.ProgressInfo)

            Dim WP As Integer

            If _CurUserWP = 0 Then
                WP = RouteurModel.CurWP - 1
            Else
                WP = _CurUserWP
            End If

            If _PlayerInfo.RouteWayPoints.Count = 0 Then
                AddLog("No WP in route, rouuting aborted!")
                Return
            ElseIf WP < 0 Then
                WP = 0
            ElseIf WP >= _PlayerInfo.RouteWayPoints.Count Then
                WP = _PlayerInfo.RouteWayPoints.Count - 1
            End If


            'Trim the WP to be "in-sea"
            Dim P0 As Coords = _PlayerInfo.RouteWayPoints(WP)(0)(0)
            If GSHHS_Reader.HitTest(P0, 0, GSHHS_Reader.Polygons(P0), True) Then
                Dim Tcl As New TravelCalculator With {.StartPoint = P0, .EndPoint = _PlayerInfo.RouteWayPoints(WP)(0)(1)}
                Dim Cap As Double = Tcl.TrueCap

                Tcl.EndPoint = Nothing
                While GSHHS_Reader.HitTest(P0, 0, GSHHS_Reader.Polygons(P0), True)
                    P0 = Tcl.ReachDistance(RouteurModel.GridGrain, Cap)
                    Tcl.StartPoint = P0
                End While
                _PlayerInfo.RouteWayPoints(WP)(0)(0) = P0
            End If

            Dim P1 As Coords = _PlayerInfo.RouteWayPoints(WP)(0)(1)
            If GSHHS_Reader.HitTest(P1, 0, GSHHS_Reader.Polygons(P1), True) Then
                Dim Tcl As New TravelCalculator With {.StartPoint = P1, .EndPoint = _PlayerInfo.RouteWayPoints(WP)(0)(0)}
                Dim Cap As Double = Tcl.TrueCap

                Tcl.EndPoint = Tc.StartPoint
                While GSHHS_Reader.HitTest(P1, 0, GSHHS_Reader.Polygons(P1), True)
                    P1 = Tcl.ReachDistance(RouteurModel.GridGrain, Cap)
                    Tcl.StartPoint = P1
                End While
                _PlayerInfo.RouteWayPoints(WP)(0)(1) = P1
            End If
            _gr.ComputeBestRoute(1, RouteurModel.GridGrain, BrokenSails, _PlayerInfo.RouteWayPoints(WP))
            'End If
        ElseIf Not StartRouting Then
            _gr.Stop()
            _StopRouting = True
        Else
            'Fast routing start a timer
            _RoutingRestartPending = True
        End If


    End Sub

    Private Sub AddLog(ByVal Log As String)

        SyncLock _LogQueue
            _LogQueue.Enqueue(Now.ToShortDateString & " " & Now.ToLongTimeString & " : " & Log)
        End SyncLock

    End Sub

    Public Sub AddPointToRoute(ByVal P As Coords)

        'Dim T As New ObservableCollection(Of clsrouteinfopoints)
        'Dim TC As New TravelCalculator
        'Dim NewDist As Double
        'Dim PointAdded As Boolean = False

        'TC.StartPoint = _Pilote.ActualPosition.P
        'TC.EndPoint = P
        'NewDist = TC.SurfaceDistance

        'For Each Pt In _PlannedRoute

        '    If Not PointAdded Then
        '        TC.EndPoint = Pt.P
        '        If NewDist < TC.SurfaceDistance Then
        '            T.Add(New clsrouteinfopoints() With {.P = New Coords(P)})
        '            PointAdded = True
        '        End If

        '    End If

        '    T.Add(Pt)
        'Next

        '_PlannedRoute = T
        ''
    End Sub


    Public Sub DebugTest()

        Return

    End Sub


    Public Shared Function WindAngle(ByVal Cap As Double, ByVal Wind As Double) As Double

        Dim I As Double = (360 - Cap + Wind) Mod 180

        If Cap >= Wind Then
            If Cap - Wind <= 180 Then
                I = Cap - Wind
            Else
                I = 360 - Cap + Wind
            End If
        Else
            If Wind - Cap <= 180 Then
                I = Wind - Cap
            Else
                I = 360 - Wind + Cap
            End If

        End If

        Return I

    End Function

    Public Property WindChangeDate() As DateTime
        Get
            Return _WindChangeDate
        End Get
        Set(ByVal value As DateTime)
            _WindChangeDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindChangeDate"))
        End Set
    End Property

    Public Property WindChangeDist() As Double
        Get
            Return _WindChangeDist
        End Get
        Set(ByVal value As Double)
            _WindChangeDist = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindChangeDist"))
        End Set
    End Property




    Private Sub _PlayerInfo_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _PlayerInfo.PropertyChanged

        If e.PropertyName = "AutoRouting" And _PlayerInfo.AutoRouting Then
            getboatinfo(_Meteo)
        End If
    End Sub

#If BOTS = 1 Then
    Private Sub _Pilote_Log(ByVal log As String) Handles _Pilote.Log

        AddLog(log)

    End Sub
#End If

    Private Sub _gr_Log(ByVal Str As String) Handles _gr.Log

        AddLog(Str)

    End Sub

    Private Sub _gr_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _gr.PropertyChanged

        Static lastchange As New DateTime

        If e.PropertyName = "TmpRoute" And Now.Subtract(lastchange).TotalSeconds > 1 Then
            lastchange = Now
            TempRoute(_Meteo) = _gr.Route
        End If

    End Sub

    Private Sub _gr_StepComputed(ByVal GridListSize As Long, ByVal TodoListSize As Integer) Handles _gr.StepComputed


        If TodoListSize = 0 Then
            AddLog("GridComputationComplete")
            TempRoute(_Meteo) = _gr.Route
            If Not TempRoute Is Nothing AndAlso TempRoute.Count > 0 Then
                BruteRoute(_Meteo) = TempRoute
                If Not BruteRoute Is Nothing AndAlso BruteRoute.Count > 0 Then
                    _CurBestRoute = BruteRoute
                    AddLog("Updating Brute route from Grid ETA " & BruteRoute(BruteRoute.Count - 1).T.ToString)
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Cleared"))
                Else
                    AddLog("Grid COmplete, no route")
                End If
                UpdatePath(TodoListSize = 0)
                If Not _StopRouting Then
                    _RoutingRestartPending = True
                End If
            End If
        Else
            AddLog("Grid Progress " & GridListSize & " / " & TodoListSize)

        End If

    End Sub

    Public Sub New()
        AddHandler GSHHS_Reader.BspEvt, AddressOf OnBspEvt
        AddHandler GSHHS_Reader.Log, AddressOf OnGsHHSLog

    End Sub

    Private Sub OnGsHHSLog(ByVal Msg As String)
        AddLog(Msg)
    End Sub

    Private Sub OnBspEvt(ByVal Count As Long)
        AddLog("BspStats : " & Count)
    End Sub

    Private Sub _RoutingRestart_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles _RoutingRestart.Elapsed

        _RoutingRestart.Stop()
        _RoutingRestartPending = False
        startGridRoute(True)

    End Sub

End Class

Public Class BoatInfo

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _CurPos As New Coords
    Private _Name As String
    Public Drawn As Boolean = False
    Public ProOption As Boolean
    Private _Classement As Integer
    Public Temps As DateTime
    Public TotalPoints As Decimal
    Private _Flag As BitmapImage
    Private _FlagName As String

    Private Shared _ImgList As New SortedList(Of String, BitmapImage)

    Public Property Classement() As Integer
        Get
            Return _Classement
        End Get
        Set(ByVal value As Integer)
            _Classement = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Classement"))
        End Set
    End Property

    Public Property CurPos() As Coords
        Get
            Return _CurPos
        End Get
        Set(ByVal value As Coords)
            _CurPos = value
        End Set
    End Property

    Public ReadOnly Property Flag() As BitmapImage
        Get
            Return ImgList(FlagName)
        End Get
    End Property

    Public Property FlagName() As String
        Get
            Return _FlagName
        End Get
        Set(ByVal value As String)
            _FlagName = value
        End Set
    End Property

    Public Shared ReadOnly Property ImgList() As SortedList(Of String, BitmapImage)
        Get
            Return _ImgList
        End Get
    End Property

    Public Shared Property ImgList(ByVal Index As String) As BitmapImage
        Get
            If _ImgList(Index) Is Nothing OrElse _ImgList(Index).Width < 10 Then
                Dim Img As BitmapImage = VOR_Router.GetHttpImage(RouteurModel.BASE_GAME_URL & "/flagimg.php?idflags=" & Index, Nothing)
                _ImgList(Index) = Img

            End If
            Return _ImgList(Index)
        End Get
        Set(ByVal value As BitmapImage)
            _ImgList(Index) = value
        End Set
    End Property

    Public Property Name() As String
        Get
            Return _Name
        End Get
        Set(ByVal value As String)
            _Name = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
        End Set
    End Property



End Class