Imports System.Net
Imports System.Xml.Serialization
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Math
Imports Routeur.RoutePointView

Public Class VLM_Router


    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event bruted()
    Public Event BoatShown()
    Public Event BoatClear()
    Public Event IsoComplete()

    Private _UserInfo As user
    Private _WebClient As New WebClient()
    Private _CookiesContainer As CookieContainer = Nothing
    Private _LastRefreshTime As DateTime
    Private _LastDataDate As DateTime

    Private _WindChangeDist As Double
    Private _WindChangeDate As DateTime
    Private _WindCoords As New Coords

    Private _Meteo As New clsMeteoOrganizer

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
    Private WithEvents _iso As IsoRouter


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


    Private _BoatUnderMouse As New ObservableCollection(Of BoatInfo)
    Private _MeteoUnderMouse As New ObservableCollection(Of RoutingGridPoint)
    Private _RoutesUnderMouse As New ObservableCollection(Of RoutePointInfo)
    Private _Pilototo(5) As String
    Private _HideWindArrow As Boolean = True

    Private _WayPointDest As New Coords
    Private _GridProgressWindow As New frmRoutingProgress
    Private _Owner As Window

    Private _MeteoArrowSize As Double = 50
    Private _MeteoArrowDate As Date
    Private _MeteoVisible As Boolean
    Private _MeteoNOPoint As Coords
    Private _MeteoSEPoint As Coords
    Private _MeteoWidth As Double
    Private _MeteoHeight As Double
    Private _MeteoPath As String

    Private _CurMousePos As Coords


    Private Shared _PosValide As Boolean = False
    Private Shared _Sails As New clsSailManager

    Private _EnableManualRefresh As Boolean = True
    Private _ManualRefreshCount As Integer = 0

    Public Shared BoatType As String



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
        Private _From As clsrouteinfopoints

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


        Public Property DTF() As Double
            Get
                Return _DTF

            End Get
            Set(ByVal value As Double)
                _DTF = value
                RaiseEvent PropertyChanged(Me, CurDTFProps)
            End Set
        End Property


        Public Property From() As clsrouteinfopoints
            Get
                Return _From
            End Get
            Set(ByVal value As clsrouteinfopoints)
                _From = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("From"))
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


        Public Property T() As DateTime
            Get
                Return _T
            End Get
            Set(ByVal value As DateTime)
                _T = value
            End Set
        End Property

        Public Overrides Function ToString() As String

            Return T.ToString("dd-MMM HH:mm") & " ; " & Cap.ToString("f1") & " ; " & DTF.ToString("f1") & " ; " & P.ToString & " ; " & WindDir.ToString("f2") & " ; " & WindStrength.ToString("f2")

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


    Public Property CurUserWP() As Integer
        Get
            Dim i As Integer = 0

            'If _CurUserWP <> 0 AndAlso _CurUserWP < RouteurModel.WPList.Count Then

            '    Return RouteurModel.WPList(_CurUserWP)
            'ElseIf _CurUserWP = 0 AndAlso RouteurModel.WPList.Count > 0 Then
            '    Return RouteurModel.WPList(0)
            'Else
            '    Return "??"
            'End If
            Return _CurUserWP
        End Get

        Set(ByVal value As Integer)
            'Dim Index As Integer = 0

            'If value.StartsWith("<Auto>") Then
            '    Index = 0
            'ElseIf value.StartsWith("WP") Then

            '    Dim MinIndex As Integer = value.IndexOf("-"c)
            '    Integer.TryParse(value.Substring(2, MinIndex - 2), Index)
            'Else
            '    Return
            'End If

            ''While RouteurModel.WPList(Index) <> value
            ''    Index += 1
            ''    If Index >= RouteurModel.WPList.Count Then
            ''        Return
            ''    End If
            ''End While
            If _CurUserWP <> value Then
                'Reset the router last start when changing dest
                _CurUserWP = value
                _LastGridRouteStart = New DateTime(0)
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurUserWP"))
        End Set
    End Property

    Public ReadOnly Property CurVMGEnveloppe() As String
        Get
            If _UserInfo Is Nothing OrElse _UserInfo.position Is Nothing Then
                Return ""
            End If

            Dim P As New Coords(_UserInfo.position.longitude, _UserInfo.position.latitude)
            Dim Tc As New TravelCalculator With {.StartPoint = _CurMousePos}

            Try
                Dim Mi As MeteoInfo = _Meteo.GetMeteoToDate(Tc.StartPoint, _UserInfo.date, True)
                If Mi Is Nothing Then
                    Return Nothing
                End If
                If _UserInfo Is Nothing OrElse _UserInfo.position Is Nothing Then
                    Return ""
                End If

                If _CurUserWP = 0 Then
                    Tc.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(0)
                Else
                    Tc.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(_CurUserWP - 1).WPs(0)(0)
                End If

                Dim CapOrtho As Double = Tc.OrthoCourse_Deg


                Return CurVMGEnveloppe(Mi, P, CapOrtho, UserInfo.type)
            Finally
                Tc.StartPoint = Nothing
                Tc.EndPoint = Nothing
                Tc = Nothing
            End Try

        End Get
    End Property


    Public Shared ReadOnly Property CurVMGEnveloppe(ByVal MI As MeteoInfo, ByVal P As Coords, ByVal CapOrtho As Double, ByVal BoatType As String) As String
        Get

            Dim RetString As String = ""
            Dim X As Double
            Dim Y As Double

            Dim Speed As Double

            For i As Double = 0 To 360 Step 2.5
                If MI.Strength > 0 Then
                    Speed = _Sails.GetSpeed(BoatType, clsSailManager.EnumSail.OneSail, WindAngle(GribManager.CheckAngleInterp(CapOrtho + i), MI.Dir), MI.Strength)
                    Speed = Speed * Cos(i / 180 * PI)
                    If Speed < 0 Then
                        Speed = -50 * Speed / MI.Strength
                    Else
                        Speed = 50 * Speed / MI.Strength
                    End If
                Else
                    Speed = 0
                End If
                If i = 0 Then
                    RetString &= " M "
                Else
                    RetString &= " L "
                End If
                X = Speed * Cos((CapOrtho + i - 90) / 180 * PI) '- 150
                Y = Speed * Sin((CapOrtho + i - 90) / 180 * PI) '- 150
                RetString &= (X).ToString("f2").Replace(",", ".") & "," & (Y).ToString("f2").Replace(",", ".")
                'If i Mod 5 = 0 And i <> 0 Then

                '    RetString &= " L " & ((2 + Speed) * Cos((CapOrtho + i - 90) / 180 * PI)).ToString("f2").Replace(",", ".") & "," & ((2 + Speed) * Sin((CapOrtho + i - 90) / 180 * PI)).ToString("f2").Replace(",", ".")
                '    RetString &= " L " & ((Speed) * Cos((CapOrtho + i - 90) / 180 * PI)).ToString("f2").Replace(",", ".") & "," & ((Speed) * Sin((CapOrtho + i - 90) / 180 * PI)).ToString("f2").Replace(",", ".")

                'End If
            Next

            Return RetString

        End Get
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

    Public ReadOnly Property IsoChrones() As LinkedList(Of IsoChrone)
        Get
            If Not _gr Is Nothing Then
                Return _gr.IsoChrones
            End If
            If _iso IsNot Nothing Then
                Return _iso.IsoChrones
            End If
            Return Nothing
        End Get
    End Property

    Public Property MeteoArrowDate() As Date
        Get
            Return _MeteoArrowDate
        End Get
        Set(ByVal value As Date)
            If value <> _MeteoArrowDate Then
                _MeteoArrowDate = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDate"))
                UpdateMeteoArrows()
                'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
            End If
        End Set
    End Property
    Public Property MeteoArrowDateTicks() As Long
        Get
            Return _MeteoArrowDate.Ticks
        End Get
        Set(ByVal value As Long)
            If value <> _MeteoArrowDate.Ticks Then
                _MeteoArrowDate = New Date(value)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDate"))
                'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
                UpdateMeteoArrows()
            End If
        End Set
    End Property

    Public ReadOnly Property MeteoArrowDateStart() As Long
        Get
            If UserInfo Is Nothing Then
                Return Now.Ticks
            End If

            Return _UserInfo.date.Ticks
        End Get
    End Property

    Public ReadOnly Property MeteoArrowDateEnd() As Long
        Get
            'FIXME Use meteo span const from grib manager!!
            Return MeteoArrowDateStart + TimeSpan.TicksPerDay * GribManager.GRIB_MAX_DAY
        End Get
    End Property

    Public Property MeteoArrowSize() As Double
        Get
            Return _MeteoArrowSize
        End Get
        Set(ByVal value As Double)
            If _MeteoArrowSize <> value Then
                _MeteoArrowSize = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowSize"))
                'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
                UpdateMeteoArrows()
            End If
        End Set

    End Property


    Public Property MeteoVisible() As Boolean
        Get
            Return _MeteoVisible
        End Get
        Set(ByVal value As Boolean)
            If _MeteoVisible <> value Then
                _MeteoVisible = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoVisible"))
            End If
        End Set
    End Property
    Public Shared ReadOnly Property TimeArrowDateSmallTick() As Long
        Get
            Return TimeSpan.TicksPerMinute * 5
        End Get
    End Property

    Public ReadOnly Property TickFreq() As Double
        Get
            Return TimeSpan.TicksPerDay / 2
        End Get
    End Property

    Public Shared ReadOnly Property TimeArrowDateLargeTick() As Long
        Get
            Return TimeSpan.TicksPerHour
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

    Public Property PilototoRoute() As ObservableCollection(Of clsrouteinfopoints)
        Get
            Return _PilototoRoute
        End Get
        Set(ByVal value As ObservableCollection(Of clsrouteinfopoints))
            _PilototoRoute = value
        End Set
    End Property

    Public ReadOnly Property PilototoRouteView() As RouteViewModel
        Get
            Return New RouteViewModel(_PlayerInfo.NumBoat, _Pilototo, RouteViewModel.NB_MAX_POINTS_PILOTOTO)
        End Get
    End Property

    Public Shared Property PosValide() As Boolean
        Get
            Return _PosValide
        End Get
        Set(ByVal value As Boolean)
            _PosValide = value
        End Set
    End Property

    Public ReadOnly Property RoutesUnderMouse() As ObservableCollection(Of RoutePointInfo)
        Get
            Return _RoutesUnderMouse
        End Get
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

        Try
            Dim C As New Coords With {.Lat_deg = _UserInfo.position.latitude, .Lon_Deg = _UserInfo.position.longitude}
            Dim Mi As MeteoInfo
            Dim Dte As DateTime = _UserInfo.date.AddMinutes(RouteurModel.VacationMinutes)
            Dim Speed As Double = 0
            Dim TC As New TravelCalculator
            _AllureRoute.Clear()
            For i = 0 To AllureDuration Step RouteurModel.VacationMinutes
                Mi = _Meteo.GetMeteoToDate(C, Dte, False)
                If Mi Is Nothing Then
                    Exit For
                End If
                Speed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, Math.Abs(AllureAngle), Mi.Strength)
                TC.StartPoint = C
                C = TC.ReachDistance(Speed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Mi.Dir + AllureAngle))
                Dte = Dte.AddMinutes(RouteurModel.VacationMinutes)
                Dim NewP As New clsrouteinfopoints With {.P = New Coords(C), .T = Dte}
                _AllureRoute.Add(NewP)

            Next
            TC.StartPoint = Nothing
            TC = Nothing
        Catch ex As Exception
            AddLog("Compute allure exception" & ex.Message)
        End Try
    End Sub

    Private Sub ComputePilototo()
        Static Computing As Boolean = False

        If Not Computing Then
            Dim Tc As New TravelCalculator

            Dim LastPTick As Long = 0

            Try
                Computing = True

                Dim CurDate As DateTime = _UserInfo.date.AddMinutes(RouteurModel.VacationMinutes)
                Dim CurPos As Coords = New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude)
                Dim P As clsrouteinfopoints
                Dim Mi As MeteoInfo = Nothing
                Dim BoatSpeed As Double
                Dim CurIndex As Integer = 0
                Dim RouteComplete As Boolean = False
                Dim CurWPDest As Coords = Nothing
                Dim PrevMode As Integer = _UserInfo.position.ModePilote
                Dim PrevWPDest As Coords = Nothing
                Dim PrevWPNum As Integer = Nothing

                If _PlayerInfo.RaceInfo.deptime > CurDate Then
                    'Race has not started, start route from race start time
                    CurDate = _PlayerInfo.RaceInfo.deptime
                End If

                If (_WayPointDest.Lon = 0 AndAlso _WayPointDest.Lat = 0) Then

                    Dim Retries As Integer = 0
                    While Retries < 2
                        Try
                            If _CurUserWP = 0 Then
                                PrevWPDest = GSHHS_Reader.PointToSegmentIntersect(CurPos, _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(0) _
                                                                                 , _PlayerInfo.RaceInfo.races_waypoints(RouteurModel.CurWP).WPs(0)(1))
                            Else
                                PrevWPDest = GSHHS_Reader.PointToSegmentIntersect(CurPos, _PlayerInfo.RaceInfo.races_waypoints(_CurUserWP - 1).WPs(0)(0), _PlayerInfo.RaceInfo.races_waypoints(_CurUserWP - 1).WPs(0)(1))
                            End If
                            Exit While
                        Catch ex As Exception
                            AddLog(ex.Message)
                            Retries = Retries + 1
                            If Retries = 3 Then
                                Return
                            Else
                                System.Threading.Thread.Sleep(250)
                            End If
                        End Try
                    End While

                Else
                    PrevWPDest = New Coords(_WayPointDest)

                End If

                Dim PrevValue As Double

                Dim OrderDate As Date
                Dim OrderType As EnumRouteMode
                Dim OrderValue As Double
                Dim Fields() As String = Nothing
                Dim CurWPNUm As Integer = -1
                Dim CapAtWP As Double = 0
                Dim ModeAtWP As Integer = PrevMode
                Dim WPDist As Double
                Dim RouteToEnd As Boolean = False

                PilototoRoute.Clear()
                Select Case PrevMode

                    Case 1
                        'Cap fixe
                        PrevValue = _UserInfo.position.cap

                    Case 2
                        'Allure fixe
                        PrevValue = _UserInfo.position.AngleAllure

                    Case 3, 4, 5
                        'Route to a WP
                        CurWPDest = PrevWPDest

                End Select

                While Not RouteComplete

                    While CurIndex <= 5
                        If _Pilototo(CurIndex) IsNot Nothing Then
                            Fields = _Pilototo(CurIndex).Split(","c)
                            If Fields.Count >= 5 AndAlso _Pilototo(CurIndex).ToLowerInvariant.Contains("pending") Then
                                Exit While
                            End If
                        End If
                        CurIndex += 1
                    End While

                    If CurIndex > 5 Then
                        If OrderDate.Ticks = 0 Then
                            ReDim Fields(0)
                            If PrevMode < 4 Then
                                OrderDate = CurDate.AddHours(RouteurModel.CourseExtensionHours)
                            Else
                                RouteToEnd = True
                            End If
                        Else

                            ReDim Fields(0)
                            RouteComplete = True
                            OrderDate = OrderDate.AddHours(RacePrefs.RACE_COURSE_EXTENSION_HOURS)
                        End If
                    End If


                    If Fields IsNot Nothing AndAlso Fields.Count >= MIN_PILOTOTO_FIED_COUNT AndAlso OrderIsPending(_Pilototo(CurIndex)) Then

                        If Not GetOrderDate(Fields(FLD_DATEVALUE), OrderDate) _
                           OrElse Not GetOrderType(Fields(2), OrderType) Then

                            AddLog("Invalid pilototo string : " & _Pilototo(CurIndex))
                            RouteComplete = True
                            Continue While
                        End If

                        Select Case OrderType
                            Case EnumRouteMode.Angle, EnumRouteMode.Bearing
                                If Not Double.TryParse(Fields(3), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, OrderValue) Then
                                    AddLog("Invalid pilototo string : " & _Pilototo(CurIndex))
                                    RouteComplete = True
                                    Continue While
                                End If

                            Case EnumRouteMode.VBVMG, EnumRouteMode.Ortho, EnumRouteMode.VMG

                                Dim Lon As Double = 0
                                Dim Lat As Double = 0
                                Double.TryParse(Fields(3), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat)
                                If Fields(4).Contains("@") Then
                                    Double.TryParse(Fields(4).Substring(0, Fields(4).IndexOf("@"c)), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)
                                    Double.TryParse(Fields(4).Substring(Fields(4).IndexOf("@"c) + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, CapAtWP)
                                    ModeAtWP = If(CapAtWP > 0, 1, OrderType)
                                Else
                                    Double.TryParse(Fields(4), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon)
                                    ModeAtWP = OrderType
                                End If
                                If Lat = 0 And Lon = 0 Then
                                    PrevWPNum = RouteurModel.CurWP
                                    PrevWPDest = New Coords(_PlayerInfo.RaceInfo.races_waypoints(CurWPNUm).WPs(0)(0))

                                Else
                                    PrevWPNum = -1
                                    PrevWPDest = New Coords(Lat, Lon)
                                End If

                            Case Else
                                AddLog("Invalid pilototo string (unknown routing mode: " & _Pilototo(CurIndex))
                                RouteComplete = True
                                Continue While

                        End Select


                        'OrderDate = New Date(1970, 1, 1).AddSeconds(OrderDateSecs).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours)
                    End If

                    'move index forward, otherwise, each wp is iterated twice
                    CurIndex += 1

                    While Not RouteComplete AndAlso (CurDate < OrderDate OrElse RouteToEnd)
                        Mi = _Meteo.GetMeteoToDate(CurPos, CurDate, True)
                        If Mi Is Nothing Then
                            'If there is no meteo, try on the next loop
                            AddLog("Pilototo route aborted, not enough meteo yet, try again later (meteo date : " & CurDate & " )")
                            RouteComplete = True
                            Exit While
                        End If
                        Tc.StartPoint = CurPos
                        P = Nothing

                        Select Case PrevMode
                            Case 1
                                'Cap fixe
                                BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(PrevValue, Mi.Dir), Mi.Strength)
                                CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, PrevValue)
                                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}

                            Case 2
                                'Angle fixe
                                BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, Math.Abs(PrevValue), Mi.Strength)
                                CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(Mi.Dir + PrevValue))
                                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}

                            Case 3
                                'Ortho
                                Tc.EndPoint = CurWPDest
                                Dim CapOrtho As Double = Tc.OrthoCourse_Deg
                                BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho, Mi.Dir), Mi.Strength)
                                CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, CapOrtho)
                                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}

                            Case 4

                                'BVMG

                                If CurWPNUm <> -1 Then


                                    'If CurWPNUm >= _PlayerInfo.RaceInfo.races_waypoints.Count Then
                                    CurWPDest = GSHHS_Reader.PointToSegmentIntersect(CurPos, _PlayerInfo.RaceInfo.races_waypoints(CurWPNUm).WPs(0)(0) _
                                                                                     , _PlayerInfo.RaceInfo.races_waypoints(CurWPNUm).WPs(0)(1))
                                    'CurWPNUm = _PlayerInfo.RaceInfo.races_waypoints.Count - 1
                                    'End If

                                End If

                                Tc.EndPoint = CurWPDest
                                Dim CapOrtho As Double = Tc.OrthoCourse_Deg
                                Dim Angle As Double
                                Dim MaxSpeed As Double = 0
                                Dim BestAngle As Double = 0
                                Dim dir As Integer
                                WPDist = Tc.SurfaceDistance

                                For Angle = 0 To 90 Step 0.5

                                    For dir = -1 To 1 Step 2
                                        BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho + Angle * dir, Mi.Dir), Mi.Strength)

                                        If BoatSpeed * Math.Cos(Angle / 180 * Math.PI) > MaxSpeed Then
                                            MaxSpeed = BoatSpeed '* Math.Cos(Angle / 180 * Math.PI)
                                            BestAngle = CapOrtho + Angle * dir
                                        End If
                                    Next

                                Next
                                CurPos = Tc.ReachDistance(MaxSpeed / 60 * RouteurModel.VacationMinutes, GribManager.CheckAngleInterp(BestAngle))

                                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}



                            Case 5

                                If CurWPNUm <> -1 Then


                                    'If CurWPNUm >= _PlayerInfo.RaceInfo.races_waypoints.Count Then
                                    'CurWPNUm = _PlayerInfo.RaceInfo.races_waypoints.Count - 1
                                    CurWPDest = GSHHS_Reader.PointToSegmentIntersect(CurPos, _PlayerInfo.RaceInfo.races_waypoints(CurWPNUm).WPs(0)(0) _
                                                                                         , _PlayerInfo.RaceInfo.races_waypoints(CurWPNUm).WPs(0)(1))
                                    'End If

                                End If
                                Dim angle As Double = do_vbvmg(CurPos, CurWPDest, Mi)
                                Tc.EndPoint = CurWPDest
                                WPDist = Tc.SurfaceDistance

                                BoatSpeed = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(angle, Mi.Dir), Mi.Strength)
                                CurPos = Tc.ReachDistance(BoatSpeed / 60 * RouteurModel.VacationMinutes, angle)
                                P = New clsrouteinfopoints With {.P = New Coords(CurPos), .WindDir = Mi.Dir, .WindStrength = Mi.Strength}


                            Case Else
                                Return

                        End Select

                        If PrevMode = 3 OrElse PrevMode = 4 OrElse PrevMode = 5 Then
                            'Check WP completion
                            Tc.EndPoint = CurPos
                            If Tc.SurfaceDistance > WPDist Then
                                'WP Reached change for next WP
                                If ModeAtWP <> 1 Then
                                    If CurWPNUm = -1 Then
                                        CurWPNUm = RouteurModel.CurWP
                                    Else
                                        CurWPNUm += 1
                                        If CurWPNUm >= RouteurModel.WPList.Count Then
                                            RouteComplete = True
                                        End If

                                    End If
                                Else
                                    PrevMode = 1
                                    PrevValue = CapAtWP
                                End If
                            End If
                        End If
                        If Not P Is Nothing Then
                            If PilototoRoute.Count = 0 OrElse ((CurDate.Ticks - LastPTick > TimeSpan.TicksPerMinute * 5 AndAlso _
                                CurDate.Ticks - PilototoRoute(0).T.Ticks < TimeSpan.TicksPerHour * 24) OrElse _
                                CurDate.Ticks - LastPTick > TimeSpan.TicksPerHour * 3) Then

                                P.T = CurDate
                                PilototoRoute.Add(P)
                                LastPTick = Now.Ticks
                                If PilototoRoute.Count > 1500 Then
                                    AddLog("Pilototo route stopped at 1500 vac.")
                                    RouteComplete = True
                                End If
                            End If
                        End If
                        CurDate = CurDate.AddMinutes(RouteurModel.VacationMinutes)
                    End While

                    'Extend route 10h, unless pilototo contradicts
                    OrderDate = New DateTime(0)

                    If OrderType <> 0 Then
                        PrevMode = OrderType
                        PrevValue = OrderValue
                        CurWPNUm = PrevWPNum
                        CurWPDest = PrevWPDest
                    End If

                    'Exit if all orders have been executed
                    If CurIndex > 5 Then
                        RouteComplete = True
                    End If

                End While

            Catch ex As Exception

                AddLog("Exception in ComputePilototo : " & ex.Message)

            Finally
                Computing = False
                Tc.StartPoint = Nothing
                Tc.EndPoint = Nothing
                Tc = Nothing

            End Try

        End If
    End Sub

    Public Sub CoordsExtent(ByVal C1 As Coords, ByVal C2 As Coords, ByVal Width As Double, ByVal Height As Double)

        _MeteoNOPoint = C1
        _MeteoSEPoint = C2
        _MeteoWidth = Width
        _MeteoHeight = Height
        'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
        UpdateMeteoArrows()
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

    'TDB v>21
    'Public Sub DebugBSP(ByVal p As Point)

    '    Dim C As New Coords
    '    C.Lon_Deg = _2D_Viewer.CanvasToLon(p.X)
    '    C.Lat_Deg = _2D_Viewer.CanvasToLat(p.X)

    '    AddLog(GSHHS_Reader._Tree.DebugInfo(C))

    'End Sub

    Public ReadOnly Property DiffEvolution() As ObservableCollection(Of Decimal)
        Get
            Return _DiffEvolution
        End Get
    End Property

    Public Function do_vbvmg(ByVal Start As Coords, ByVal Dest As Coords, ByVal mi As MeteoInfo) As Double

        '254	  double alpha, beta;
        '255	  double speed, speed_t1, speed_t2, l1, l2, d1, d2;
        '256	  double angle, maxangle, t, t1, t2, t_min;
        Dim t_min As Double
        '257	  double wanted_heading;
        '258	  double w_speed, w_angle;
        '259	  double dist, tanalpha, d1hypotratio;
        '260	  double b_alpha, b_beta, b_t1, b_t2, b_l1, b_l2;
        '261	  double b1_alpha, b1_beta;
        '262	  double speed_alpha, speed_beta;
        '263	  double vmg_alpha, vmg_beta;
        '264	  int i,j, min_i, min_j, max_i, max_j;
        Dim i As Integer
        Dim j As Integer
        Dim alpha As Double
        Dim TanAlpha As Double
        Dim TanBeta As Double
        Dim beta As Double
        Dim D1HypotRatio As Double
        Dim SpeedT1 As Double
        Dim SpeedT2 As Double
        Dim ISigne As Integer = 1
        Dim D1 As Double
        Dim D2 As Double
        Dim L1 As Double
        Dim L2 As Double
        Dim T As Double
        Dim T1 As Double
        Dim T2 As Double
        Dim b_Alpha As Double
        Dim b_Beta As Double
        Dim b_L1 As Double
        Dim b_L2 As Double
        Dim b_T1 As Double
        Dim b_T2 As Double
        Dim SpeedAlpha As Double
        Dim SpeedBeta As Double
        Dim VMGAlpha As Double
        Dim VMGBeta As Double
        Dim angle As Double




        '265:
        '266	  b_t1 = b_t2 = b_l1 = b_l2 = b_alpha = b_beta = beta = 0.0;
        '267:
        Dim Tc As New TravelCalculator With {.StartPoint = Start, .EndPoint = Dest}
        Dim Dist As Double = Tc.SurfaceDistance
        Dim CapOrtho As Double = Tc.OrthoCourse_Deg
        '268	  dist = ortho_distance(aboat->latitude, aboat->longitude,
        '269	                        aboat->wp_latitude, aboat->wp_longitude);
        '270:

        '271	  get_wind_info_context(context, aboat, &aboat->wind);
        '272	  set_heading_ortho_nowind(aboat);
        '273:
        '274	  wanted_heading = aboat->heading;
        '275	  maxangle = wanted_heading;
        '276:
        '277	  w_speed = aboat->wind.speed;
        '278	  w_angle = aboat->wind.angle;
        '279:
        '280	  /* first compute the time for the "ortho" heading */
        '281	  speed = find_speed(aboat, w_speed, w_angle - wanted_heading);
        Dim Speed As Double = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho, mi.Dir), mi.Strength)
        If Speed > 0 Then
            t_min = Dist / Speed
        Else
            t_min = 365 * 24
        End If
        '282	  if (speed > 0.0) {
        '283	    t_min = dist / speed;
        '284	  } else {
        '285	    t_min = 365.0*24.0; /* one year :) */
        '286	  }
        '287:
        '288:#If DEBUG Then
        '289	  printf("VBVMG: Wind %.2fkts %.2f\n", w_speed, radToDeg(w_angle));
        '290	  printf("VBVMG Direct road: heading %.2f time %.2f\n", 
        '291	         radToDeg(wanted_heading), t_min);
        '292	  printf("VBVMG Direct road: wind angle %.2f\n", 
        '293	         radToDeg(w_angle-wanted_heading));
        '294	#endif /* DEBUG */
        '295	
        angle = mi.Dir - CapOrtho

        If angle < -90 Then
            angle += 360
        ElseIf angle > 90 Then
            angle -= 360
        End If
        '296	  angle = w_angle - wanted_heading;
        '297	  if (angle < -PI ) {
        '298	    angle += TWO_PI;
        '299	  } else if (angle > PI) {
        '300	    angle -= TWO_PI;
        '301	  }
        If angle > 0 Then
            ISigne = -1
            '302	  if (angle < 0.0) {
            '303	    min_i = 1;
            '304	    min_j = -89;
            '305	    max_i = 90;
            '306	    max_j = 0;
            '307	  } else {
            '308	    min_i = -89;
            '309	    min_j = 1;
            '310	    max_i = 0;
            '311	    max_j = 90;
            '312	  }
        End If
        '313	
        '314	  for (i=min_i; i<max_i; i++) {
        For i = 1 To 90
            alpha = i * Math.PI / 180
            TanAlpha = Tan(alpha)
            D1HypotRatio = Sqrt(1 + TanAlpha * TanAlpha)
            '315	    alpha = degToRad((double)i);
            '316	    tanalpha = tan(alpha);
            '317	    d1hypotratio = hypot(1, tan(alpha));
            '318	    speed_t1 = find_speed(aboat, w_speed, angle-alpha);
            SpeedT1 = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho - i * ISigne, mi.Dir), mi.Strength)
            '319	    if (speed_t1 <= 0.0) {
            '320	      continue;
            '321	    }
            If SpeedT1 > 0 Then

                For j = -89 To 0
                    beta = j * PI / 180
                    D1 = Dist * (Tan(-beta) / (TanAlpha + Tan(-beta)))
                    L1 = D1 * D1HypotRatio
                    '322	    for (j=min_j; j<max_j; j++) {
                    '323	      beta = degToRad((double)j);
                    '324	      d1 = dist * (tan(-beta) / (tanalpha + tan(-beta)));
                    '325	      l1 =  d1 * d1hypotratio;
                    '326	      t1 = l1 / speed_t1;
                    T1 = L1 / SpeedT1
                    If T1 < 0 OrElse T1 > t_min Then
                        Continue For
                    End If
                    '327	      if ((t1 < 0.0) || (t1 > t_min)) {
                    '328	        continue;
                    '329	      }
                    D2 = Dist - D1
                    '330	      d2 = dist - d1; 
                    SpeedT2 = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho - j * ISigne, mi.Dir), mi.Strength)
                    '331	      speed_t2 = find_speed(aboat, w_speed, angle-beta);
                    If SpeedT2 <= 0 Then
                        Continue For
                    End If
                    '332	      if (speed_t2 <= 0.0) {
                    '333	        continue;
                    '334	      }
                    '335	      l2 =  d2 * hypot(1, tan(-beta));
                    TanBeta = Tan(-beta)
                    L2 = D2 * Sqrt(1 + TanBeta * TanBeta)
                    '336	      t2 = l2 / speed_t2;
                    T2 = L2 / SpeedT2
                    '337	      if (t2 < 0.0) {
                    '338	        continue;
                    '339	      }

                    T = T1 + T2
                    If T < t_min Then
                        '340	      t = t1 + t2;
                        '341	      if (t < t_min) {
                        '342	        t_min = t;
                        '343	        b_alpha = alpha;
                        '344	        b_beta  = beta;
                        '345	        b_l1 = l1;
                        '346	        b_l2 = l2;
                        '347	        b_t1 = t1;
                        '348	        b_t2 = t2;
                        t_min = T
                        b_Alpha = i
                        b_Beta = j
                        b_L1 = L1
                        b_L2 = L2
                        b_T1 = T1
                        b_T2 = T2
                        '349	      }
                        '350	    }
                    End If
                Next
            End If
            '351	  }
        Next
        '352	#if DEBUG
        '353	  printf("VBVMG: alpha=%.2f, beta=%.2f\n", radToDeg(b_alpha), radToDeg(b_beta));
        '354	#endif /* DEBUG */
        '355	  if (mode) {
        '356	    b1_alpha = b_alpha;
        '357	    b1_beta = b_beta;
        '358	    for (i=-9; i<=9; i++) {
        '359	      alpha = b1_alpha + degToRad(((double)i)/10.0);
        '360	      tanalpha = tan(alpha);
        '361	      d1hypotratio = hypot(1, tan(alpha));
        '362	      speed_t1 = find_speed(aboat, w_speed, angle-alpha);
        '363	      if (speed_t1 <= 0.0) {
        '364	        continue;
        '365	      }
        '366	      for (j=-9; j<=9; j++) {
        '367	        beta = b1_beta + degToRad(((double)j)/10.0);
        '368	        d1 = dist * (tan(-beta) / (tanalpha + tan(-beta)));
        '369	        l1 =  d1 * d1hypotratio;
        '370	        t1 = l1 / speed_t1;
        '371	        if ((t1 < 0.0) || (t1 > t_min)) {
        '372	          continue;
        '373	        }
        '374	        d2 = dist - d1; 
        '375	        speed_t2 = find_speed(aboat, w_speed, angle-beta);
        '376	        if (speed_t2 <= 0) {
        '377	          continue;
        '378	        }
        '379	        l2 =  d2 * hypot(1, tan(-beta));
        '380	        t2 = l2 / speed_t2;
        '381	        if (t2 < 0.0) {
        '382	          continue;
        '383	        }
        '384	        t = t1 + t2;
        '385	        if (t < t_min) {
        '386	          t_min = t;
        '387	          b_alpha = alpha;
        '388	          b_beta  = beta;
        '389	          b_l1 = l1;
        '390	          b_l2 = l2;
        '391	          b_t1 = t1;
        '392	          b_t2 = t2;
        '393	        }
        '394	      }    
        '395	    }
        '396	#if DEBUG
        '397	    printf("VBVMG: alpha=%.2f, beta=%.2f\n", radToDeg(b_alpha), 
        '398	           radToDeg(b_beta));
        '399	#endif /* DEBUG */
        '400	  }
        SpeedAlpha = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho - b_Alpha * ISigne, mi.Dir), mi.Strength)
        SpeedBeta = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, WindAngle(CapOrtho - b_Beta * ISigne, mi.Dir), mi.Strength)
        VMGAlpha = SpeedAlpha * Cos(b_Alpha * PI / 180)
        VMGBeta = SpeedBeta * Cos(b_Beta * PI / 180)

        If VMGAlpha > VMGBeta Then
            Return CapOrtho - b_Alpha * ISigne
        Else
            Return CapOrtho - b_Beta * ISigne
        End If
        '401	  speed_alpha = find_speed(aboat, w_speed, angle-b_alpha);
        '402	  vmg_alpha = speed_alpha * cos(b_alpha);
        '403	  speed_beta = find_speed(aboat, w_speed, angle-b_beta);
        '404	  vmg_beta = speed_beta * cos(b_beta);
        '405	
        '406	#if DEBUG
        '407	    printf("VBVMG: speedalpha=%.2f, speedbeta=%.2f\n", speed_alpha, speed_beta);
        '408	    printf("VBVMG: vmgalpha=%.2f, vmgbeta=%.2f\n", vmg_alpha, vmg_beta);
        '409	    printf("VBVMG: headingalpha %.2f, headingbeta=%.2f\n",
        '410	           radToDeg(fmod(wanted_heading + b_alpha, TWO_PI)),
        '411	           radToDeg(fmod(wanted_heading + b_beta, TWO_PI)));
        '412	#endif /* DEBUG */
        '413	
        '414	  if (vmg_alpha > vmg_beta) {
        '415	    *heading1 = fmod(wanted_heading + b_alpha, TWO_PI);
        '416	    *heading2 = fmod(wanted_heading + b_beta, TWO_PI);
        '417	    *time1 = b_t1;
        '418	    *time2 = b_t2;
        '419	    *dist1 = b_l1;
        '420	    *dist2 = b_l2;
        '421	  } else {
        '422	    *heading2 = fmod(wanted_heading + b_alpha, TWO_PI);
        '423	    *heading1 = fmod(wanted_heading + b_beta, TWO_PI);
        '424	    *time2 = b_t1;
        '425	    *time1 = b_t2;
        '426	    *dist2 = b_l1;
        '427	    *dist1 = b_l2;
        '428	  }
        '429	  if (*heading1 < 0 ) {
        '430	    *heading1 += TWO_PI;
        '431	  }
        '432	  if (*heading2 < 0 ) {
        '433	    *heading2 += TWO_PI;
        '434	  }
        '435	    
        '436	  *wangle1 = fmod(*heading1 - w_angle, TWO_PI);
        '437	  if (*wangle1 > PI ) {
        '438	    *wangle1 -= TWO_PI;
        '439	  } else if (*wangle1 < -PI ) {
        '440	    *wangle1 += TWO_PI;
        '441	  }
        '442	  *wangle2 = fmod(*heading2 - w_angle, TWO_PI);
        '443	  if (*wangle2 > PI ) {
        '444	    *wangle2 -= TWO_PI;
        '445	  } else if (*wangle2 < -PI ) {
        '446	    *wangle2 += TWO_PI;
        '447	  }
        '448	#if DEBUG
        '449	  printf("VBVMG: wangle1=%.2f, wangle2=%.2f\n", radToDeg(*wangle1),
        '450	         radToDeg(*wangle2));
        '451	  printf("VBVMG: heading1 %.2f, heading2=%.2f\n", radToDeg(*heading1),
        '452	         radToDeg(*heading2));
        '453	  printf("VBVMG: dist=%.2f, l1=%.2f, l2=%.2f, ratio=%.2f\n", dist, *dist1,
        '454	         *dist2, (b_l1+b_l2)/dist);
        '455	  printf("VBVMG: t1 = %.2f, t2=%.2f, total=%.2f\n", *time1, *time2, t_min);
        '456	  printf("VBVMG: heading %.2f\n", radToDeg(*heading1));
        '457	  printf("VBVMG: wind angle %.2f\n", radToDeg(*wangle1));
        '458	#endif /* DEBUG */
        '459	}

    End Function


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

            Dim Page As String = GetHTTPResponse(RouteurModel.BASE_GAME_URL & "/races.php?lang=fr&type=arrived&idraces=" & _PlayerInfo.RaceInfo.idraces, Cookies)
            Dim ArrivedOffset As Integer = GetArrivedCount(Page)

            'Get the current positions
            Page = GetHTTPResponse(RouteurModel.BASE_GAME_URL & "/races.php?lang=fr&type=racing&idraces=" & _PlayerInfo.RaceInfo.idraces & "&startnum=" & CurFistBoat, Cookies)

            PageEmpty = ParseRanking(Page, ArrivedOffset, Cookies)

            CurFistBoat += 100


        Loop Until PageEmpty

        AddLog("Worker " & Id & " complete")
        OnBoatWorkerComplete()

    End Sub

    Public Property EnableManualRefresh() As Boolean
        Get
            Return _EnableManualRefresh
        End Get

        Set(ByVal value As Boolean)
            _EnableManualRefresh = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("EnableManualRefresh"))
        End Set
    End Property

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
        If Application.Current Is Nothing Then
            Return
        End If
        If Not System.Threading.Thread.CurrentThread Is Application.Current.Dispatcher.Thread Then
            Application.Current.Dispatcher.BeginInvoke(SelfDlg, New Object() {Pv, Value, e, RefreshBoatInfo, meteo})

        Else

            _ThreadSetterCurPos.Lon_Deg = _UserInfo.position.longitude
            _ThreadSetterCurPos.Lat_Deg = _UserInfo.position.latitude
            Pv.Clear()
            If Not Value Is Nothing Then
                For Each V In Value
                    Pv.Add(V)
                    '    V.RefreshTC(_ThreadSetterCurPos)
                Next
            End If

            RaiseEvent PropertyChanged(Me, e)


        End If
    End Sub

    Public ReadOnly Property Track() As String
        Get
            If Not _Track Is Nothing AndAlso _Track.Length > 0 Then
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
                _Track = Track 'RetString
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

    Private Sub MeteoArrowDeferred(ByVal state As Object)

        Dim x As Double
        Dim y As Double
        Dim retstring As String = ""
        Dim C As New Coords
        Dim Mi As MeteoInfo
        Dim Scale As Double = MeteoArrowSize
        Dim Dte As Date = MeteoArrowDate

        If _UserInfo Is Nothing OrElse Dte.Ticks = 0 OrElse _MeteoNOPoint Is Nothing OrElse _MeteoSEPoint Is Nothing Then
            Return
        End If

        Dim MeteoRange As Double = 1
        Dim MinLon As Double = _MeteoNOPoint.Lon
        Dim MaxLon As Double = _MeteoSEPoint.Lon
        Dim DeltaLon As Double = (MaxLon - MinLon) / 10
        Dim MinLat As Double = _MeteoSEPoint.Lat
        Dim MaxLat As Double = _MeteoNOPoint.Lat
        Dim DeltaLat As Double = (MaxLat - MinLat) / 10
        Dim PX As Double
        Dim PY As Double

        For x = 0 To 10
            For y = 0 To 10
                C.Lat = MinLat + y * DeltaLat
                C.Lon = MinLon + x * DeltaLon
                Mi = _Meteo.GetMeteoToDate(C, Dte, True)
                If Not Mi Is Nothing AndAlso Mi.Strength >= 0.1 Then
                    PX = x * _MeteoWidth / 10
                    PY = (10 - y) * _MeteoHeight / 10

                    retstring &= GetMeteoArrowString(PX, PY, Scale, Mi)
                End If
            Next
        Next
        _MeteoPath = retstring
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))

    End Sub


    Public ReadOnly Property MeteoArrow() As String
        Get
            Return _MeteoPath
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
        _CurMousePos = C
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

            If _gr.GridPointsList.Contains(C2) Then
                _MeteoUnderMouse.Add(CType(_gr.GridPointsList(C2), RoutingGridPoint))
            End If

        End If

        System.Threading.ThreadPool.QueueUserWorkItem(AddressOf GetRoutesUnderMouse, C)

        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        If StartCount <> _BoatUnderMouse.Count Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfoVisible"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatInfo"))
        End If

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

        'Dim PwdString As String = urlencode(_PlayerInfo.Password)
        If IsLogin Then
            If user = "" Then
                Return RouteurModel.BASE_GAME_URL & "/myboat.php?pseudo=" & _PlayerInfo.Nick & "&password=" & _PlayerInfo.Password & "&lang=fr&type=login"
            Else
                Return RouteurModel.BASE_GAME_URL & "/myboat.php?pseudo=" & user & "&password=" & password & "&lang=fr&type=login"
            End If

        Else
            Return RouteurModel.BASE_GAME_URL & "/getinfo.php?idu=" & _PlayerInfo.NumBoat & "&pseudo=" & System.Web.HttpUtility.UrlEncode(_PlayerInfo.Nick) & "&password=" & System.Web.HttpUtility.UrlEncode(_PlayerInfo.Password)
        End If
    End Function


    Private Function Login(Optional ByVal User As String = "", Optional ByVal Password As String = "") As CookieContainer
        Try
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
        Catch ex As Exception
            AddLog("Exception during Login : " & ex.Message)
        End Try

        'If failure, just return nothing, hopefully the caller will handle that gracefully
        Return Nothing


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

    Private Function ParseRanking(ByVal Page As String, ByVal ArrivedOffset As Integer, ByVal cookies As CookieContainer) As Boolean
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
                    .Classement = Classement + ArrivedOffset

                    If Not BoatInfo.ImgList.ContainsKey(ImgName) Then
                        'Console.WriteLine("Ask " & ImgName & " for " & .Name)
                        BoatInfo.ImgList.Add(ImgName, Nothing)
                        'If Img Is Nothing Then
                        '    Console.WriteLine("Request failed for " & ImgName)
                        'End If

                    End If

                    .FlagName = ImgName
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

    Private Function ParseVLMBoatInfoString() As user

        Dim TmpDbl As Double

        Dim RetUser As New user
        Dim Lup As Double
        Dim Nup As Double
        Static NbSwitch As Integer = 0

        RetUser.position = New userPosition
        PosValide = False
        Dim BoatInfo As New VLMBoatInfo
        Dim Data As Dictionary(Of String, Object) = WS_Wrapper.GetBoatInfo(_PlayerInfo)
        If Data.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
            JSonHelper.LoadJSonDataToObject(BoatInfo, JSonHelper.GetJSonObjectValue(WS_Wrapper.GetBoatInfo(_PlayerInfo), JSONDATA_BASE_OBJECT_NAME))
        End If
        RetUser.position.vitesse = BoatInfo.BSP

        RouteurModel.CurWP = BoatInfo.NWP

        RetUser.position.cap = BoatInfo.HDG
        RetUser.position.latitude = BoatInfo.LAT / 1000
        RetUser.Loch = BoatInfo.LOC
        RetUser.position.longitude = BoatInfo.LON / 1000
        Lup = BoatInfo.LUP
        RetUser.position.ModePilote = BoatInfo.PIM
        If BoatInfo.PIM = 2 Then
            RetUser.position.AngleAllure = CDbl(BoatInfo.PIP)
            'Double.TryParse(BoatInfo.PIP, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, RetUser.position.AngleAllure)
        Else
            RetUser.position.AngleAllure = WindAngleWithSign(BoatInfo.HDG, BoatInfo.TWD)
        End If

        RetUser.type = BoatInfo.POL
        BoatType = BoatInfo.POL

        Nup = BoatInfo.NUP
        If Nup < -10 AndAlso RetUser.Loch > 0 Then
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
        RouteurModel.CurWP = BoatInfo.NWP
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
        'If BoatInfo.POS.IndexOf("/"c) >= 0 Then
        '    RetUser.position.classement = CInt(BoatInfo.POS.Substring(0, BoatInfo.POS.IndexOf("/"c)))
        'End If
        RetUser.position.WP_latitude = BoatInfo.WPLAT
        RetUser.position.WP_longitude = BoatInfo.WPLON
        RetUser.position.classement = BoatInfo.RNK
        RetUser.position.wind_angle = BoatInfo.TWD
        RetUser.position.wind_speed = BoatInfo.TWS
        _Pilototo(0) = BoatInfo.PIL1
        _Pilototo(1) = BoatInfo.PIL2
        _Pilototo(2) = BoatInfo.PIL3
        _Pilototo(3) = BoatInfo.PIL4
        _Pilototo(4) = BoatInfo.PIL5

        _WayPointDest.Lat_Deg = BoatInfo.WPLAT
        _WayPointDest.Lon_Deg = BoatInfo.WPLON


        If RetUser.Loch = 0 Then
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

        Dim response As String = ""
        Dim Retries As Integer = 0
        While Retries < 3
            Try
                Dim Http As HttpWebRequest = CType(HttpWebRequest.Create(url), HttpWebRequest)

                Http.CookieContainer = Cookies
                Dim WR As WebResponse = Http.GetResponse()
                Dim ResponseStream As New System.IO.StreamReader(WR.GetResponseStream)
                response = ResponseStream.ReadToEnd
                ResponseStream.Close()
                Exit While
            Catch ex As Exception
                Retries += 1
            End Try
        End While


        Return response

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

        Dim RetValue As String = RouteurModel.BaseFileDir & "\track_" & _PlayerInfo.RaceInfo.idraces & "_" & _PlayerInfo.NumBoat & ".dat"

        If Not System.IO.Directory.Exists(RouteurModel.BaseFileDir) Then
            System.IO.Directory.CreateDirectory(RouteurModel.BaseFileDir)
        End If

        Return RetValue

    End Function

    Private Function GetRoutePointAtCoords(ByVal Points As ObservableCollection(Of clsrouteinfopoints), ByVal TargetC As Coords, ByRef RetC As clsrouteinfopoints) As Boolean

        Dim TC As New TravelCalculator() With {.StartPoint = TargetC}
        Dim CurDist As Double = Double.MaxValue
        Dim BestP As clsrouteinfopoints = Nothing
        Dim CurT As DateTime = New DateTime(3000, 1, 1)
        Try
            For Each P In Points

                TC.EndPoint = P.P
                If CurDist > TC.SurfaceDistance OrElse (Abs(CurDist - TC.SurfaceDistance) < 5 AndAlso CurT > P.T) Then
                    CurDist = TC.SurfaceDistance
                    CurT = P.T
                    BestP = P
                    'ElseIf CurDist <> Double.MaxValue AndAlso 3 * CurDist > TC.SurfaceDistance Then
                    '    Exit For
                End If


            Next
            TC.StartPoint = Nothing
            TC.EndPoint = Nothing
            TC = Nothing

            If CurDist < 10 Then
                RetC = BestP
                Return True
            Else
                Return False
            End If
        Catch
            'inore exception, will try later
            Return False
        End Try

    End Function

    Private Sub GetRoutesUnderMouse(ByVal O As Object)

        Dim RoutePoints As New ObservableCollection(Of RoutePointInfo)
        Dim C As Coords = CType(O, Coords)
        Dim RetC As clsrouteinfopoints = Nothing
        Dim P As RoutePointInfo

        If GetRoutePointAtCoords(PlannedRoute, C, RetC) Then
            P = New RoutePointInfo("Planned", RetC)
            RoutePoints.Add(P)
        End If

        If GetRoutePointAtCoords(BruteRoute, C, RetC) Then
            P = New RoutePointInfo("Best Route", RetC)
            RoutePoints.Add(P)
        End If

        If GetRoutePointAtCoords(TempRoute, C, RetC) Then
            P = New RoutePointInfo("Temp Route", RetC)
            RoutePoints.Add(P)
        End If


        If GetRoutePointAtCoords(AllureRoute, C, RetC) Then
            P = New RoutePointInfo("Allure Route", RetC)
            RoutePoints.Add(P)
        End If


        If GetRoutePointAtCoords(PilototoRoute, C, RetC) Then
            P = New RoutePointInfo("Pilototo Route", RetC)
            RoutePoints.Add(P)
        End If

        _RoutesUnderMouse = RoutePoints
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RoutesUnderMouse"))

    End Sub

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


    Private Function GetArrivedCount(ByVal Page As String) As Integer

        Dim RetValue As Integer = 0
        Dim ClassIndex As Integer
        Dim CurString As String = Page
        Dim Complete As Boolean = False
        Const STR_TrClassranking As String = "tr class=""ranking"""
        ClassIndex = CurString.IndexOf(STR_TrClassranking)
        CurString = CurString.Substring(ClassIndex + 19)

        While Not Complete

            ClassIndex = CurString.IndexOf(STR_TrClassranking)
            If ClassIndex = -1 Then
                Complete = True
            Else
                CurString = CurString.Substring(ClassIndex + 19)
                Dim Indextd As Integer = CurString.IndexOf("<td>")
                Dim indexstd As Integer = CurString.IndexOf("</td>")
                Dim CurPos As Integer

                If Integer.TryParse(CurString.Substring(Indextd + 4, indexstd - 4 - Indextd), CurPos) Then
                    If CurPos > RetValue Then
                        RetValue = CurPos
                    End If
                End If
            End If

        End While


        Return RetValue

    End Function

    Public Sub getboatinfo()
        getboatinfo(_Meteo)

    End Sub

    Public Sub getboatinfo(ByVal Force As Boolean)

        If Force And _ManualRefreshCount < RouteurModel.VacationMinutes * 4 Then
            EnableManualRefresh = False
            _ManualRefreshCount += 1
        End If
        getboatinfo(_Meteo, Force)
    End Sub

    Public Sub GetBoatInfo(ByVal meteo As clsMeteoOrganizer, Optional ByVal force As Boolean = False)

        Dim ErrorCount As Integer = 0
        Dim ResponseString As String = ""


        If _PlayerInfo Is Nothing Then
            Return
        End If



        If Not force AndAlso Not _UserInfo Is Nothing AndAlso Now.Subtract(_UserInfo.date).TotalMinutes < RouteurModel.VacationMinutes Then
            Return
        End If

        If Not _UserInfo Is Nothing AndAlso Now.Subtract(_UserInfo.date).TotalMinutes < RouteurModel.VacationMinutes Then
            EnableManualRefresh = True
            _ManualRefreshCount = 0
        End If

        Try
            'ResponseString = WS_Wrapper.GetBoatInfo(_PlayerInfo)
            'ResponseString = _WebClient.DownloadString(STR_GetUserInfo)
            Dim prevwp As Integer = RouteurModel.CurWP
            UserInfo(meteo) = ParseVLMBoatInfoString()

            If RouteurModel.CurWP <> prevwp Then
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
            End If


            Dim VLMInfoMessage As String = "Meteo read D:" & UserInfo.position.wind_angle.ToString("f2")
            Dim i As Integer = 0
            Dim STart As DateTime = Now.AddSeconds(-UserInfo.DateOffset)
            If Math.Abs(Now.Subtract(STart).TotalHours) > 24 Then
                STart = Now
            End If
            Dim mi As MeteoInfo = meteo.GetMeteoToDate(New Coords(UserInfo.position.latitude, UserInfo.position.longitude), STart, False)

            If Not mi Is Nothing Then
                Dim V As Double = _Sails.GetSpeed(_UserInfo.type, clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(_UserInfo.position.cap, _UserInfo.position.wind_angle), _UserInfo.position.wind_speed)
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
                'RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrow"))
                UpdateMeteoArrows()
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDateStart"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoArrowDateEnd"))
                If _MeteoArrowDate < _UserInfo.date Then
                    MeteoArrowDate = _UserInfo.date
                End If
            Else
                AddLog("No Meteo data !!")
            End If
            _LastCommunication = Now

            If Now.Subtract(_lastflush).TotalMinutes > 15 Then

                Dim BaseDir As String = RouteurModel.BaseFileDir
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

            VMG = _UserInfo.position.vitesse * Math.Cos((_UserInfo.position.cap - CurrentRoute.LoxoCourse_Deg) / 180 * Math.PI)
            If VMG <> 0 Then
                ETA = Now.AddHours(CurrentRoute.SurfaceDistance / VMG)
            Else
                ETA = Now
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
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurVMGEnveloppe"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatCanvasX"))
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BoatCanvasY"))
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

    Private Sub UpdateMeteoArrows()

        If MeteoVisible Then
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf MeteoArrowDeferred, Nothing)
        End If

    End Sub
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

        If Not _iso Is Nothing Then
            BestRouteAtPoint = _iso.RouteToPoint(c)
        End If

        'todo make this safe
        Dim WP As Integer

        If _CurUserWP = 0 Then
            WP = RouteurModel.CurWP
        Else
            WP = _CurUserWP
        End If

        Dim D As Double = GSHHS_Reader.HitDistance(c, _PlayerInfo.RaceInfo.races_waypoints(WP).WPs, False)

        If D = Double.MaxValue Then

            Dim TC As New TravelCalculator
            TC.StartPoint = c
            TC.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0)
            D = TC.SurfaceDistance
            TC.EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1)
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


    Public Sub startGridRoute(ByVal StartRouting As Boolean)



        If StartRouting AndAlso Not _UserInfo Is Nothing AndAlso Now.Subtract(_LastGridRouteStart).TotalMinutes > RouteurModel.VacationMinutes Then

            _LastGridRouteStart = Now
            Dim start As New Coords(New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude))
            Dim Tc As New TravelCalculator()
            Static currentmgr As New CurrentMgr


            _StopRouting = False


            Tc.StartPoint = start

            Dim StartDate As DateTime

            If Now > _PlayerInfo.RaceInfo.deptime Then
                StartDate = LastDataDate
            Else
                StartDate = _PlayerInfo.RaceInfo.deptime
            End If


            If _gr Is Nothing Then
                _gr = New GridRouter(start, StartDate, _Meteo, _Sails, _UserInfo.type)
            Else
                _gr.Start(StartDate) = start
            End If

            _GridProgressWindow.Show(_Owner, _gr.ProgressInfo)

            Dim WP As Integer

            If _CurUserWP = 0 Then
                WP = RouteurModel.CurWP - 1
            Else
                WP = _CurUserWP
            End If

            If _PlayerInfo.RaceInfo.races_waypoints.Count = 0 Then
                AddLog("No WP in route, routing aborted!")
                Return
            ElseIf WP < 0 Then
                WP = 0
            ElseIf WP >= _PlayerInfo.RaceInfo.races_waypoints.Count Then
                WP = _PlayerInfo.RaceInfo.races_waypoints.Count - 1
            End If


            'Trim the WP to be "in-sea"
            Dim P0 As Coords = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0)
            If GSHHS_Reader.HitTest(P0, 0, GSHHS_Reader.Polygons(P0), True) Then
                Dim Tcl As New TravelCalculator With {.StartPoint = P0, .EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1)}
                Dim Cap As Double = Tcl.OrthoCourse_Deg

                Tcl.EndPoint = Nothing
                While GSHHS_Reader.HitTest(P0, 0, GSHHS_Reader.Polygons(P0), True)
                    P0 = Tcl.ReachDistance(RouteurModel.GridGrain, Cap)
                    Tcl.StartPoint = P0
                End While
                _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0) = P0
            End If

            Dim P1 As Coords = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1)
            If GSHHS_Reader.HitTest(P1, 0, GSHHS_Reader.Polygons(P1), True) Then
                Dim Tcl As New TravelCalculator With {.StartPoint = P1, .EndPoint = _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0)}
                Dim Cap As Double = Tcl.OrthoCourse_Deg

                Tcl.EndPoint = Tc.StartPoint
                While GSHHS_Reader.HitTest(P1, 0, GSHHS_Reader.Polygons(P1), True)
                    P1 = Tcl.ReachDistance(RouteurModel.GridGrain, Cap)
                    Tcl.StartPoint = P1
                End While
                _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1) = P1
            End If

            _gr.ComputeBestRoute(1, RouteurModel.GridGrain, _PlayerInfo.RaceInfo.races_waypoints(WP).WPs)
            'End If
        ElseIf Not StartRouting Then
            If Not _gr Is Nothing Then
                _gr.Stop()
            End If
            _StopRouting = True
        Else
            'Fast routing start a timer
            _RoutingRestartPending = True
        End If


    End Sub

    Public Sub StartIsoRoute(ByVal Owner As RouteurMain, ByVal StartRouting As Boolean)

        If StartRouting Then
            Dim frm As New frmRouterConfiguration(Owner, _PlayerInfo.RaceInfo.idraces)
            Dim StartDate As DateTime
            Dim StartCoords As Coords
            Dim EndCoords As Coords

            If Not frm.ShowDialog() Then
                Return
            End If
            Dim prefs As RacePrefs = CType(frm.DataContext, RacePrefs)

            _iso = New IsoRouter(_UserInfo.type, _Sails, _Meteo.GribMeteo, prefs.IsoAngleStep, prefs.IsoLookupAngle, prefs.IsoStep, _
                                 prefs.IsoStep_24, prefs.IsoStep_48)
            Dim WP As Integer

            If CurUserWP = 0 Then
                WP = RouteurModel.CurWP
            Else
                WP = _CurUserWP
            End If



            If Now > _PlayerInfo.RaceInfo.deptime Then
                StartDate = LastDataDate
            Else
                StartDate = _PlayerInfo.RaceInfo.deptime
            End If

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ClearGrid"))
            Dim start As New Coords(New Coords(_UserInfo.position.latitude, _UserInfo.position.longitude))
            Dim Dest As Coords = GSHHS_Reader.PointToSegmentIntersect(start, _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(0) _
                                                                                 , _PlayerInfo.RaceInfo.races_waypoints(WP).WPs(0)(1))
            If prefs.UseCustomDest Then
                EndCoords = prefs.RouteDest
            Else
                EndCoords = Dest
            End If

            If prefs.UseCustomStart Then
                StartCoords = prefs.RouteStart
            Else
                StartCoords = start
            End If

            _iso.StartIsoRoute(StartCoords, EndCoords, StartDate)
            'End If
        ElseIf Not _iso Is Nothing Then
            _iso.StopRoute()

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

    Public Shared Function WindAngleWithSign(ByVal heading As Double, ByVal Wind As Double) As Double

        Dim V As Double = WindAngle(heading, Wind)

        If (heading + V) Mod 360 = Wind Then
            Return -V
        Else
            Return V
        End If
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

    Private Sub _iso_Log(ByVal msg As String) Handles _iso.Log
        AddLog(msg)
    End Sub

    Private Sub _iso_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _iso.PropertyChanged
        If e.PropertyName = "TmpRoute" Then 'And Now.Subtract(lastchange).TotalSeconds > 1 Then
            'lastchange = Now
            TempRoute(_Meteo) = _iso.Route
        End If

    End Sub

    Private Sub _iso_RouteComplete() Handles _iso.RouteComplete
        BruteRoute(_Meteo) = _iso.Route
        RaiseEvent IsoComplete()
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
                Dim Img As BitmapImage = VLM_Router.GetHttpImage(RouteurModel.BASE_GAME_URL & "/flagimg.php?idflags=" & Index, Nothing)
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