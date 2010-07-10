﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.IO
Imports Routeur.RacePrefs

Public Class RouteurModel

    Implements INotifyPropertyChanged


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Event BoatsDrawn()

    Private WithEvents _VorHandler As VLM_Router
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
    Private Shared _RouteExtensionHours As Double = RacePrefs.RACE_COURSE_EXTENSION_HOURS


    Private Shared _BaseFileDir As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur"


    Private _Scale As Double = 1
    Public _LatOffset As Double = 0
    Public _LonOffset As Double = 0

    Public Const PenWidth As Double = 0.3

#If TESTING = 1 Then
    Public Const S11_SERVER As String = "http://testing.virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://testing.virtual-loup-de-mer.org"
#Else
    Public Const S11_SERVER As String = "http://virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://tcv.virtual-loup-de-mer.org"

#End If

    Public Shared BASE_GAME_URL As String = S11_SERVER
    Public Shared BASE_SAIL_URL As String = BASE_GAME_URL & "/Polaires/"

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


    Private Shared _CurWP As Integer = 0


    Private _Busy As Boolean = False
    Private _ClearGrid As Boolean
    Private _IsoRouterActive As Boolean

    Private _WPPath As String = ""

    Private _NOPoint As Coords
    Private _SEPoint As Coords
    Private _Width As Double
    Private _Height As Double


    Public Function CanvasToCoords(ByVal X As Double, ByVal Y As Double) As Coords

        If Not _VorHandler Is Nothing AndAlso Not _2DViewer Is Nothing Then
            Return New Coords(_2DViewer.CanvasToLat(Y), _2DViewer.CanvasToLon(X))
        End If

        Return Nothing
    End Function

    Public Function CanvasToCoords(ByVal P As Point) As Coords

        Return CanvasToCoords(P.X, P.Y)

    End Function

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

    Public Sub UpdateRaceScale(ByVal C1 As Coords, ByVal C2 As Coords)
        'ReDim RouteurModel._RaceRect(3)
        _LonOffset = (C1.Lon_Deg + C2.Lon_Deg) / 2
        _LatOffset = (C1.Mercator_Y_Deg + C2.Mercator_Y_Deg) / 2
        Scale = 360 / Math.Abs(C1.Lon_Deg - C2.Lon_Deg)
        Dim Scale2 As Double = 180 / 1.1 / Math.Abs(C1.Mercator_Y_Deg - C2.Mercator_Y_Deg)
        If Scale2 < Scale Then
            Scale = Scale2
        End If
        Scale *= _2D_Viewer.DEFINITION

        If Not _2DViewer Is Nothing Then
            _2DViewer.Scale = Scale
            _2DViewer.LonOffset = _LonOffset
            _2DViewer.LatOffset = _LatOffset
            _2DViewer.clearBgMap()
        End If
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPsPath"))
    End Sub

    Public ReadOnly Property GetPilototoRoute() As RouteViewModel
        Get

            If Not VorHandler Is Nothing Then

                Return VorHandler.PilototoRouteView

            Else
                Return Nothing
            End If
        End Get
    End Property

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
        OnWPListUpdate()

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

        For Each WPs In P_Info(0).RaceInfo.races_waypoints

            If RouteurModel.WPList.Count = 0 Then
                RouteurModel.WPList.Add("<Auto> " & WPs.ToString)

            End If

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

        'Hack to force refreshing the WP
        CurWP = CurWP
        _WPList(0) = "<Auto> " & RouteurModel.WPList(CurWP).ToString
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
        VorHandler.CurUserWP = 0

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
        If C1.Lon_Deg - _RaceZoneOffsets(RaceZoneDirs.West) > -180 Then
            C1.Lon_Deg -= _RaceZoneOffsets(RaceZoneDirs.West)
        Else
            C1.Lon_Deg = -180
        End If


        C2.Lat_Deg += _RaceZoneOffsets(RaceZoneDirs.North)
        If C2.Lon_Deg + _RaceZoneOffsets(RaceZoneDirs.East) < 180 Then
            C2.Lon_Deg += _RaceZoneOffsets(RaceZoneDirs.East)
        Else
            C2.Lon_Deg = 180
        End If

        RouteurModel._RaceRect(0) = New Coords(C1)
        RouteurModel._RaceRect(1) = New Coords(C1.Lat_Deg, C2.Lon_Deg)
        RouteurModel._RaceRect(2) = New Coords(C2)
        RouteurModel._RaceRect(3) = New Coords(C2.Lat_Deg, C1.Lon_Deg)

        UpdateRaceScale(C1, C2)

    End Sub

    Public Shared ReadOnly Property BaseFileDir() As String
        Get
            If Not System.IO.Directory.Exists(_BaseFileDir) Then
                Try
                    System.IO.Directory.CreateDirectory(_BaseFileDir)
                Catch ex As Exception
                    MessageBox.Show(ex.Message, "Error creating the application data folder", MessageBoxButton.OK, MessageBoxImage.Error)
                    Throw ex
                End Try
            End If
            Return _BaseFileDir
        End Get
    End Property

    Public ReadOnly Property BoatCanvasX() As Double
        Get
            If VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.position Is Nothing Then
                Return 0
            End If
            Return _2DViewer.LonToCanvas(VorHandler.UserInfo.position.longitude)
        End Get
    End Property

    Public ReadOnly Property BoatCanvasY() As Double
        Get
            If VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.position Is Nothing Then
                Return 0
            End If

            Return _2DViewer.LatToCanvas(VorHandler.UserInfo.position.latitude)
        End Get
    End Property

    'Private Sub BuildWPPath()
    '    Dim X As Double
    '    Dim Y As Double

    '    If _CurPlayer Is Nothing OrElse _CurPlayer.RaceInfo Is Nothing Or _2DViewer Is Nothing Then
    '        Return
    '    End If
    '    _WPPath = ""
    '    For Each WP In _CurPlayer.RaceInfo.races_waypoints
    '        X = (WP.WPs.Item(0)(0).Lon - _NOPoint.Lon) / (_SEPoint.Lon - _NOPoint.Lon) * _Width
    '        Y = _Height - (WP.WPs.Item(0)(0).Lat - _SEPoint.Lat) / (_NOPoint.Lat - _SEPoint.Lat) * _Height
    '        _WPPath &= "M " & GetCoordsString(X, Y)
    '        X = (WP.WPs.Item(0)(1).Lon - _NOPoint.Lon) / (_SEPoint.Lon - _NOPoint.Lon) * _Width
    '        Y = _Height - (WP.WPs.Item(0)(1).Lat - _SEPoint.Lat) / (_NOPoint.Lat - _SEPoint.Lat) * _Height
    '        _WPPath &= "L " & GetCoordsString(X, Y)
    '    Next

    'End Sub


    Public Property IsoRouterActive() As Boolean
        Get
            Return _IsoRouterActive
        End Get
        Set(ByVal value As Boolean)
            If value <> _IsoRouterActive Then
                _IsoRouterActive = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsoRouterActive"))
            End If

        End Set
    End Property

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
            If WPList.Count >= 1 AndAlso _CurWP > 0 AndAlso _CurWP < WPList.Count AndAlso _CurWP <> value Then
                _WPList(0) = "<Auto> " & _P_Info(0).RaceInfo.races_waypoints(_CurWP - 1).ToString
            End If
            _CurWP = value

        End Set

    End Property

    Public ReadOnly Property RegisteredPlayers() As ObservableCollection(Of RegistryPlayerInfo)
        Get


            _PlayerList.Clear()
            LoadPlayers(_PlayerList)

            Return _PlayerList

        End Get
    End Property

    Public Sub RegisteredPlayersUpdated()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RegisteredPlayers"))
    End Sub

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
        CourseExtensionHours = RacePrefs.CourseExtensionHours
        Dim i As Integer

        For i = 0 To 3
            _RaceZoneOffsets(i) = RacePrefs.RaceOffset(i)
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RacePrefs"))


    End Sub

    Private Sub LoadRaceInfo(ByVal P As RegistryPlayerInfo)

        Dim RaceNum As Integer = P.RaceNum
        Dim RaceFileName As String = BaseFileDir & "\RI_" & RaceNum & ".ini"
        Dim RaceInfo As String

        If Not System.IO.File.Exists(RaceFileName) Then

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
        Dim Info As Dictionary(Of String, Object) = Nothing
        Try
            info = Parse(RaceInfo)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Exception loading Json formatted race info!!")
        End Try


        If Info IsNot Nothing AndAlso Info.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
            LoadJSonDataToObject(_P_Info(0).RaceInfo, Info(JSONDATA_BASE_OBJECT_NAME))
            RouteurModel.VacationMinutes = _P_Info(0).RaceInfo.vacfreq
        End If

    End Sub


    Public Shared Function MeteoHeight() As Double

    End Function

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

    Public Property PrevSelectedPlayer() As RegistryPlayerInfo
        Get
            Dim LB As String = RegistryHelper.GetLastPlayer
            If LB <> "" Then
                For Each Item In RegisteredPlayers

                    If Item.Nick = LB Then
                        Return Item
                    End If
                Next
            End If
            Return Nothing
        End Get

        Set(ByVal value As RegistryPlayerInfo)

        End Set
    End Property

    Public Sub CoordsExtent(ByVal C1 As Coords, ByVal C2 As Coords, ByVal Width As Double, ByVal Height As Double)

        _NOPoint = C1
        _SEPoint = C2
        _Width = Width
        _Height = Height
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPsPath"))

    End Sub

    Public Shared Property CourseExtensionHours() As Double
        Get
            Return _RouteExtensionHours
        End Get
        Set(ByVal value As Double)
            _RouteExtensionHours = value
        End Set
    End Property

    Public Property Scale() As Double
        Get
            Return _Scale
        End Get
        Set(ByVal value As Double)
            _Scale = value
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

            _2DViewer.Scale = Scale
            _2DViewer.LonOffset = _LonOffset
            _2DViewer.LatOffset = _LatOffset

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("The2DViewer"))
        End Set
    End Property

    Public Property VorHandler() As VLM_Router
        Get

            If _VorHandler Is Nothing Then
                _VorHandler = New VLM_Router
                '_VorHandler.GetBoatInfo()
                _VorHandler.DebugTest()
            End If

            If Not CurPlayer Is Nothing Then
                _VorHandler.PlayerInfo = CurPlayer
            End If

            Return _VorHandler
        End Get

        Set(ByVal value As VLM_Router)
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

    Private Sub _VorHandler_IsoComplete() Handles _VorHandler.IsoComplete
        IsoRouterActive = False
    End Sub


    Private Sub _VorHandler_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _VorHandler.PropertyChanged


        Select Case e.PropertyName

            Case "UserInfo", "BruteRoute", "BestRouteAtPoint", "BestVMGRoute", "TempRoute", "TempVMGRoute", "GridRoute", "ClearGrid"
                If e.PropertyName = "ClearGrid" Then
                    _ClearGrid = True
                End If
                tmrRefresh.Enabled = True

            Case "WPList"
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))

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

                Static routes As ObservableCollection(Of VLM_Router.clsrouteinfopoints)() = New ObservableCollection(Of VLM_Router.clsrouteinfopoints)() _
                                {VorHandler.PlannedRoute, VorHandler.BestRouteAtPoint, VorHandler.BruteRoute, VorHandler.TempRoute, _
                                 VorHandler.TempVMGRoute, VorHandler.AllureRoute, VorHandler.PilototoRoute}
                Dim Traj As String

                If VorHandler Is Nothing OrElse VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.trajectoire Is Nothing Then
                    Traj = ""
                Else
                    Traj = VorHandler.Track

                End If
                _Busy = True
                The2DViewer.UpdatePath(Traj, routes, VorHandler.Opponents, VorHandler.GridRoute, _ClearGrid, _ClearBoats, VorHandler.IsoChrones, CurPlayer.RaceInfo.races_waypoints)
                _ClearGrid = False
                _ClearBoats = False
            End If
            System.Threading.Monitor.Exit(Me)
        End If
    End Sub

    Private Sub _stats_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _stats.PropertyChanged
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Stats"))
    End Sub

    'Public ReadOnly Property WPsPath() As String
    '    Get
    '        'If _WPPath = "" Then
    '        'BuildWPPath()
    '        'End If
    '        Return _WPPath
    '    End Get
    'End Property
    Public Shared ReadOnly Property WPList() As List(Of String)
        Get
            Return _WPList
        End Get

    End Property

    Public Sub OnWPListUpdate()
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))
    End Sub

End Class
