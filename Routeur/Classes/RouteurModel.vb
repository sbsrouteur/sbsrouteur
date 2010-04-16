Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.IO
Imports Routeur.RacePrefs

Public Class RouteurModel

    Implements INotifyPropertyChanged


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Event BoatsDrawn()

    Private WithEvents _VorHandler As VOR_Router
    Private WithEvents _2DViewer As _2D_Viewer
    Private _Prefs As RacePrefs

    Private WithEvents _CurPlayer As clsPlayerInfo
    Public Shared PropTmpRoute As New PropertyChangedEventArgs("TmpRoute")
    Private _RaceZoneOffsets() As Double = New Double() {0.5, 0.5, 0.5, 0.5}
    Private WithEvents tmrRefresh As New System.Timers.Timer(500) With {.Enabled = False}
    Private WithEvents _stats As New Stats
    Private Shared _NoObstacle As Boolean
    Private _ClearBoats As Boolean = False
    Private Shared _WPList As New List(Of String)
    Private Shared _PlayerList As New ObservableCollection(Of RegistryPlayerInfo)


    Public Shared BaseFileDir As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur"


    Public Shared SCALE As Double = 1
    Public Shared LAT_OFFSET As Double = 0
    Public Shared LON_OFFSET As Double = 0

    Public Const PenWidth As Double = 0.3

    Public Const S11_SERVER As String = "http://virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://tcv.virtual-loup-de-mer.org"
    Public Const BASE_SAIL_URL As String = "http://www.virtual-loup-de-mer.org/Polaires/"

    Public Shared BASE_GAME_URL As String = S11_SERVER

    Public Shared VacationMinutes As Double = 5
    Public Shared MapLevel As String = "l"

    Private Shared _P_Info(0) As clsPlayerInfo



    Public Shared START_LON As Double = 1.186
    Public Shared START_LAT As Double = 46.142

    Public Shared URL_Version As Integer = 7

    Public Shared _RaceRect As New Polygon


    Public Shared GRID_FILE As String

    Public Shared SouthTrimList() As Integer = New Integer() {}
    Public Shared NorthTrimList() As Integer = New Integer() {}
    Public Shared EastTrimList() As Integer = New Integer() {}
    Public Shared WestTrimList() As Integer = New Integer() {}

    Public Shared Exclusions()() As Double

    Public Shared HasCurrents As Boolean = False

    Public Shared GridGrain As Double = 0.02
    Public Shared EllipseFactor As Double = 1.3
    
    Public Const InvertMeteoLon As Boolean = True
    Public Const METEO_GIRD_SIZE As Integer = 10
    Public Const METEO_GRID_STEP As Double = 1

    Public Const GribOffset As Integer = 0

    Public Const LAKE_RACE As Boolean = False

    'Exclusion Neg East/South
    Public Shared DeepSouthE() As Double = {-179.99, -60, _
                                           -179.99, -80, _
                                            -0.01, -80, _
                                            -0.01, -60, _
                                            -179.99, -60}

    Public Shared DeepSouthW() As Double = {179.99, -60, _
                                           179.99, -80, _
                                            0.01, -80, _
                                            0.01, -60, _
                                            179.99, -60}

    'Public Shared SouthLeewin() As Double = {-115, -59, _
    '                                           -115.264, -59, _
    '                                            -115.264, -70, _
    '                                            -115, -70, _
    '                                            -115, -59}

    'Public Shared CapeHorn() As Double = {67.271, -55.979, _
    '                                      69, -55, _
    '                                      67, -55, _
    '                                      67.271, -55.979}




    Private Shared _CurWP As Integer = 0


    Private _Busy As Boolean = False
    Private _ClearGrid As Boolean


    Private Sub CheckPassword()


        Dim Password As String = RegistryHelper.GetUserPassword(P_Info(0).Nick)
        Dim Retries As Integer = 0
        Do
            Password = RegistryHelper.GetUserPassword(P_Info(0).Nick)
            If VorHandler.CheckLogin(P_Info(0).Nick, Password) Then
                P_Info(0).Password = Password
                Return
            End If
            Password = InputBox("Password", "Enter password for user " & P_Info(0).Nick)
            If Password = "" Then
                End
            End If
            If Not RegistryHelper.SetUserPassword(P_Info(0).Nick, Password) Then
                MessageBox.Show("FATAL : Failed to store password in registry!!")
                End
            End If

            Retries += 1
        Loop Until Retries = 3


    End Sub

    Shared Sub New()

        Exclusions = New Double()() {DeepSouthE, DeepSouthW}

    End Sub

    Public Sub New()
    End Sub

    Public Sub Init()

        Dim frm As New frmUserPicker
        frm.DataContext = Me
        frm.ShowDialog()
        If frm.PlayerInfo Is Nothing Then
            End
        End If

        _P_Info(0) = frm.PlayerInfo.Playerinfo
        LoadRaceInfo(frm.PlayerInfo)
        LoadParams()
        CurPlayer = _P_Info(0)

        Dim C1 As New Coords(90, 180)
        Dim C2 As New Coords(-90, -180)
        Dim Offset As Double = 0.5 / 180 * Math.PI

        If _P_Info.Count = 0 OrElse P_Info(0).Route Is Nothing Then
            Return
        End If

        Dim PrevLon As Double
        If Not P_Info(0).Route Is Nothing AndAlso P_Info(0).Route.Count >= 1 AndAlso Not P_Info(0).Route(0) Is Nothing Then
            PrevLon = P_Info(0).Route(0).Lon_Deg
        Else
            PrevLon = 0
        End If

        RouteurModel.WPList.Clear()
        RouteurModel.WPList.Add("<Auto> ???")
        For Each WPs In P_Info(0).RaceInfo.races_waypoints

            RouteurModel.WPList.Add(WPs.ToString)
            For Each wp In WPs.WPs
                For Each P In wp
                    If P.Lat < C1.Lat Then
                        C1.Lat_Deg = P.Lat_Deg
                    End If
                    If P.Lon < C1.Lon Then
                        C1.Lon_Deg = P.Lon_Deg
                    End If

                    If P.Lat > C2.Lat Then
                        C2.Lat_Deg = P.Lat_Deg
                    End If
                    If P.Lon > C2.Lon Then
                        C2.Lon_Deg = P.Lon_Deg
                    End If

                    If PrevLon * P.Lon_Deg < 0 Then
                        If PrevLon < 0 Then
                            C1.Lon_Deg = -180
                            C2.Lon_Deg = 180
                        End If
                    End If
                    PrevLon = P.Lon_Deg
                Next
            Next

        Next

        START_LAT = _P_Info(0).RaceInfo.Start.Lat_Deg
        START_LON = _P_Info(0).RaceInfo.Start.Lon_Deg


        If START_LAT < C1.Lat_Deg Then
            C1.Lat_Deg = START_LAT
        End If
        If START_LAT > C2.Lat_Deg Then
            C2.Lat_Deg = START_LAT
        End If

        If START_LON < C1.Lon_Deg Then
            C1.Lon_Deg = START_LON
        End If
        If START_LON > C2.Lon_Deg Then
            C2.Lon_Deg = START_LON
        End If

        C1.Lat_Deg -= _RaceZoneOffsets(RaceZoneDirs.South)
        If C1.Lon_Deg - _RaceZoneOffsets(RaceZoneDirs.West) > -179.99 Then
            C1.Lon_Deg -= _RaceZoneOffsets(RaceZoneDirs.West)
        Else
            C1.Lon_Deg = -179.99
        End If


        C2.Lat_Deg += _RaceZoneOffsets(RaceZoneDirs.North)
        If C2.Lon_Deg + _RaceZoneOffsets(RaceZoneDirs.East) < 179.99 Then
            C2.Lon_Deg += _RaceZoneOffsets(RaceZoneDirs.East)
        Else
            C2.Lon_Deg = 179.99
        End If

        'ReDim RouteurModel._RaceRect(3)
        RouteurModel._RaceRect(0) = New Coords(C1)
        RouteurModel._RaceRect(1) = New Coords(C1.Lat_Deg, C2.Lon_Deg)
        RouteurModel._RaceRect(2) = New Coords(C2)
        RouteurModel._RaceRect(3) = New Coords(C2.Lat_Deg, C1.Lon_Deg)
        LON_OFFSET = (C1.Lon_Deg + C2.Lon_Deg) / 2
        LAT_OFFSET = (C1.Lat_Deg + C2.Lat_Deg) / 2
        SCALE = 360 / Math.Abs(C1.Lon_Deg - C2.Lon_Deg)
        Dim Scale2 As Double = 180 / Math.Abs(C1.Lat_Deg - C2.Lat_Deg)
        If Scale2 < SCALE Then
            SCALE = Scale2
        End If
        SCALE *= _2D_Viewer.DEFINITION

    End Sub

    Public ReadOnly Property RacePrefs() As RacePrefs
        Get
            If _Prefs Is Nothing AndAlso _P_Info(0) IsNot Nothing Then
                _Prefs = RacePrefs.GetRaceInfo(_P_Info(0).RaceInfo.idraces)
            End If

            Return _Prefs
        End Get
    End Property


    Public Sub Refresh()

        If Not The2DViewer Is Nothing Then
            The2DViewer.RedrawCanvas()
        End If

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoValidSpan"))

    End Sub

    Public Shared Property CurWP() As Integer
        Get
            Return _CurWP
        End Get
        Set(ByVal value As Integer)
            If _CurWP <> value Then
                _CurWP = value
                If WPList.Count >= 1 AndAlso _CurWP > 0 AndAlso _CurWP < WPList.Count Then
                    _WPList(0) = _P_Info(0).RaceInfo.races_waypoints(_CurWP - 1).ToString
                End If
            End If
        End Set

    End Property

    Public ReadOnly Property RegisteredPlayers() As ObservableCollection(Of RegistryPlayerInfo)
        Get


            _PlayerList.Clear()
            LoadPlayers(_PlayerList)

            Return _PlayerList

        End Get
    End Property

    Public ReadOnly Property AppString() As String
        Get
            Return "Routeur " & My.Application.Info.Version.ToString
        End Get
    End Property

    Public Property CurPlayer() As clsPlayerInfo
        Get
            Return _CurPlayer
        End Get
        Set(ByVal value As clsPlayerInfo)
            _CurPlayer = value
            If Not _VorHandler Is Nothing Then
                _VorHandler.PlayerInfo = value
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CurPlayer"))
        End Set
    End Property

    Private Sub LoadParams()

        GridGrain = RacePrefs.GridGrain
        MapLevel = RacePrefs.MapLevel.ToString.Substring(0, 1)
        EllipseFactor = RacePrefs.EllipseExtFactor
        Dim i As Integer

        For i = 0 To 3
            _RaceZoneOffsets(i) = RacePrefs.RaceOffset(i)
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RacePrefs"))

        
    End Sub

    Private Sub LoadRaceInfo(ByVal P As RegistryPlayerInfo)

        Dim RaceNum As Integer = P.RaceNum
        Dim RaceFileName As String = BaseFileDir & "RI_" & RaceNum & ".ini"
        Dim RaceInfo As String

        If Not System.IO.File.Exists(BaseFileDir & "RI_" & RaceNum & ".ini") Then

            'First race access download info from the WS
            RaceInfo = WS_Wrapper.GetRaceInfo(RaceNum)

            Dim SR As New StreamWriter(RaceFileName)
            SR.WriteLine(RaceInfo)
            SR.Flush()
            SR.Close()
        Else
            Dim SR As New StreamReader(RaceFileName)
            RaceInfo = SR.ReadToEnd
            SR.Close()
        End If

        Dim Info As Dictionary(Of String, Object) = Parse(RaceInfo)

        If Info IsNot Nothing AndAlso Info.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
            LoadJSonDataToObject(_P_Info(0).RaceInfo, Info(JSONDATA_BASE_OBJECT_NAME))

        End If

    End Sub

    Public ReadOnly Property MeteoValidSpan() As TimeSpan
        Get
            Return GribManager.GetCurGribDate(Now).AddHours(9.5).AddHours(System.TimeZone.CurrentTimeZone.GetUtcOffset(Now).TotalHours).Subtract(Now)
        End Get
    End Property

    Public Shared Property NoObstacle() As Boolean
        Get
            Return _NoObstacle
        End Get
        Set(ByVal value As Boolean)

            If _NoObstacle <> value Then
                _NoObstacle = value

            End If
        End Set
    End Property

    Public Property P_Info() As clsPlayerInfo()
        Get
            Return _P_Info
        End Get
        Set(ByVal value As clsPlayerInfo())
            _P_Info = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("P_Info"))
        End Set
    End Property

    Public ReadOnly Property Stats() As ObservableCollection(Of StatInfo)
        Get
            Return Routeur.Stats.Stats

        End Get
    End Property



    Public Property The2DViewer() As _2D_Viewer
        Get

            Return _2DViewer
        End Get
        Set(ByVal value As _2D_Viewer)
            _2DViewer = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("The2DViewer"))
        End Set
    End Property

    Public Property VorHandler() As VOR_Router
        Get

            If _VorHandler Is Nothing Then
                _VorHandler = New VOR_Router
                '_VorHandler.GetBoatInfo()
                _VorHandler.DebugTest()
            End If

            If Not CurPlayer Is Nothing Then
                _VorHandler.PlayerInfo = CurPlayer
            End If

            Return _VorHandler
        End Get

        Set(ByVal value As VOR_Router)
            _VorHandler = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("VorHandler"))
        End Set

    End Property

    Private Sub _VorHandler_BoatClear() Handles _VorHandler.BoatClear
        _ClearBoats = True
    End Sub

    Private Sub _VorHandler_BoatShown() Handles _VorHandler.BoatShown

        RaiseEvent BoatsDrawn()

    End Sub


    Private Sub _VorHandler_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _VorHandler.PropertyChanged


        Select Case e.PropertyName

            Case "UserInfo", "BruteRoute", "BestRouteAtPoint", "BestVMGRoute", "TempRoute", "TempVMGRoute", "GridRoute", "ClearGrid"
                If e.PropertyName = "ClearGrid" Then
                    _ClearGrid = True
                End If
                tmrRefresh.Enabled = True
            Case Else
                'Debug.WriteLine("UnHandled propertychange : " & e.PropertyName)
        End Select
    End Sub

    Private Sub _2DViewer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _2DViewer.PropertyChanged

        If e.PropertyName = "Drawn" Then
            VorHandler.RedrawComplete()
            SyncLock Me
                _Busy = False
            End SyncLock
        ElseIf e.PropertyName = "CurCoords" Then
            VorHandler.UpdateWPDist(_2DViewer.CurCoords)
            VorHandler.MouseOver(_2DViewer.CurCoords)

        End If
    End Sub

    Private Sub tmrRefresh_Elapsed(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles tmrRefresh.Elapsed

        tmrRefresh.Enabled = False
        If System.Threading.Monitor.TryEnter(Me) Then
            If Not The2DViewer Is Nothing And Not _Busy Then

                Static routes As ObservableCollection(Of VOR_Router.clsrouteinfopoints)() = New ObservableCollection(Of VOR_Router.clsrouteinfopoints)() _
                                {VorHandler.PlannedRoute, VorHandler.BestRouteAtPoint, VorHandler.BruteRoute, VorHandler.TempRoute, _
                                 VorHandler.TempVMGRoute, VorHandler.AllureRoute, VorHandler.PilototoRoute}
                Dim Traj As String

                If VorHandler Is Nothing OrElse VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.trajectoire Is Nothing Then
                    Traj = ""
                Else
                    Traj = VorHandler.Track

                End If
                _Busy = True
                The2DViewer.UpdatePath(Traj, routes, VorHandler.Opponents, VorHandler.GridRoute, _ClearGrid, _ClearBoats)
                _ClearGrid = False
                _ClearBoats = False
            End If
            System.Threading.Monitor.Exit(Me)
        End If
    End Sub

    Private Sub _stats_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _stats.PropertyChanged
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Stats"))
    End Sub

    Public Shared ReadOnly Property WPList() As List(Of String)
        Get
            Return _WPList
        End Get

    End Property

    Public Sub OnWPListUpdate()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
    End Sub

End Class
