Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.IO
Imports Routeur.RacePrefs
Imports Routeur.Commands
Imports System.Threading
Imports System.Windows.Threading
Imports Routeur.VLM_Router

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
    Private WithEvents tmrRefresh As DispatcherTimer
    Private WithEvents _stats As New Stats
    Private Shared _NoObstacle As Boolean
    Private _ClearBoats As Boolean = False
    Private Shared _WPList As New List(Of String)
    Private Shared _PlayerList As New ObservableCollection(Of RegistryPlayerInfo)
    Private Shared _RouteExtensionHours As Double = RacePrefs.RACE_COURSE_EXTENSION_HOURS
    Private _ShowEasyNav As Boolean = False

    Private WithEvents _MapLayer As MeteoLayer

    Public Shared DebugEvt As New AutoResetEvent(True)
    Private Shared _BaseFileDir As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur"
    Private _MapMenuEnabled As Boolean = True

    Private Shared _Scale As Double = 1
    Public _LatOffset As Double = 0
    Public _LonOffset As Double = 0

    Public Const PenWidth As Double = 0.3

#If TESTING = 1 Then
    Public Const S11_SERVER As String = "http://testing.virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://testing.virtual-loup-de-mer.org"
#Else
    Public Const S11_SERVER As String = "http://www.virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://virtual-loup-de-mer.org"

#End If

    Private Shared _Servers As String() = New String() {S10_SERVER, S11_SERVER}
    'Private Shared _BASE_GAME_URL As String = S11_SERVER

    Public Shared VacationMinutes As Double = 5
    Public Shared MapLevel As String = "l"

    Private Shared _P_Info(0) As clsPlayerInfo



    Public Shared START_LON As Double = 1.186
    Public Shared START_LAT As Double = 46.142


    Public Shared _RaceRect As New Polygon


    Public Shared GRID_FILE As String

    Public Shared SouthTrimList() As Integer = New Integer() {}
    Public Shared NorthTrimList() As Integer = New Integer() {}
    Public Shared EastTrimList() As Integer = New Integer() {}
    Public Shared WestTrimList() As Integer = New Integer() {}

    Public Shared NoExclusionZone As Boolean = False
    Private Shared _Exclusions()() As Double

    Public Shared HasCurrents As Boolean = False

    Public Shared GridGrain As Double = 0.02
    Public Shared EllipseFactor As Double = 1.3

    Public Const InvertMeteoLon As Boolean = True
    Public Const METEO_GIRD_SIZE As Integer = 10
    Public Const METEO_GRID_STEP As Double = 1


    Public Const LAKE_RACE As Boolean = False

    ''
    '' Exclusion Lon, Lat deg neg West, neg south
    ''

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

    'B2B Exclusion zones
    Public Const B2B_RACEID = "20101231"

    Public Shared B2B_EXCL_1() As Double = {105, -70, _
                                            105, -46, _
                                            120, -46, _
                                            120, -70}



    Private Shared _CurWP As Integer = 0


    Private _Busy As Boolean = False
    Private _ClearGrid As Boolean
    Private _IsoRouterActive As Boolean

    Private _WPPath As String = ""

    Private _NOPoint As Coords
    Private _SEPoint As Coords
    Private _Width As Double
    Private _Height As Double

    Private WithEvents _RouteManager As RouteManager

    Private _2DViewerLock As New Object

    Private _MeteoMapper As MeteoBitmapper

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

    Public Sub New()
        'THIS DEFAULT CONSTRUCTOR IS REQUIRED FOR THE XML!!!
    End Sub


    Public Sub UpdateRaceScale(ByVal C1 As Coords, ByVal C2 As Coords)
        'ReDim RouteurModel._RaceRect(3)
        If The2DViewer Is Nothing Then
            Return
        End If
        
            
        _LonOffset = (C1.Lon_Deg + C2.Lon_Deg) / 2
        If C1.Lon = C2.Lon Then
            Scale = 1
        Else
            Scale = Math.Min(The2DViewer.ActualWidth / Math.Abs(C1.Lon_Deg - C2.Lon_Deg),
                                The2DViewer.ActualHeight / Math.Abs(C1.Lat_Deg - C2.Lat_Deg))
        End If

        _LatOffset = (C1.Mercator_Y_Deg + C2.Mercator_Y_Deg) / 2
       
        Debug.Assert(Scale <> 0)

        VorHandler.CoordsExtent(C1, C2, _2DViewer.ActualWidth, _2DViewer.ActualHeight)
        CoordsExtent(C1, C2, _2DViewer.ActualWidth, _2DViewer.ActualHeight)
        
        _2DViewer.Scale = Scale
        _2DViewer.LonOffset = _LonOffset
        _2DViewer.LatOffset = _LatOffset
        _2DViewer.ClearBgMap()

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPsPath"))
        If Not RouteManager Is Nothing Then
            RouteManager.Rescale()
        End If

        'Update Meteo
        If _MapLayer Is Nothing Then
            _MapLayer = New MeteoLayer(VorHandler.Meteo)
        End If

        If _MapLayer.ViewPort Is Nothing Then
            _MapLayer.ViewPort = _2DViewer
        End If

        If VorHandler.MeteoVisible Then
            _MapLayer.RefreshInfo(C1, C2, VorHandler.MeteoArrowDate)
            If MeteoMapper IsNot Nothing Then

                If Now.Subtract(MeteoMapper.Date).TotalMinutes > VacationMinutes Then
                    MeteoMapper.Date = Now
                Else
                    MeteoMapper.StartImgRender()
                End If

            End If
        End If

    End Sub

    Private Property Dispatcher As Windows.Threading.Dispatcher

    Public Shared Property Exclusions() As Double()()
        Get

            Return _Exclusions

        End Get
        Set(ByVal value As Double()())
            _Exclusions = value
        End Set
    End Property

    Public ReadOnly Property GetPilototoRoute() As RouteViewModel
        Get

            If Not VorHandler Is Nothing Then

                Return VorHandler.PilototoRouteView

            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Sub Init(ByVal D As Windows.Threading.Dispatcher)

        _Dispatcher = D
        _RouteManager.Dispatcher = _Dispatcher
        Dim frm As New frmUserPicker
        frm.DataContext = Me
        frm.ShowDialog()
        If frm.PlayerInfo Is Nothing Then
            End
        End If

        Me.Dispatcher = Dispatcher
        Cron = New Cron(Dispatcher)
        InitCron()
        tmrRefresh = New DispatcherTimer(New TimeSpan(0, 0, 0, 0, 500), DispatcherPriority.Render, AddressOf tmrRefresh_Elapsed, Dispatcher)

        RouteManager.Load()
        _P_Info(0) = frm.PlayerInfo.Playerinfo
        LoadRaceInfo(frm.PlayerInfo)
        InitRaceExclusions()
        LoadParams()
        CurPlayer = _P_Info(0)
        OnWPListUpdate()

        Dim C1 As New Coords(90, 180)
        Dim C2 As New Coords(-90, -179.999)
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

    Private Sub InitRaceExclusions()

        Select Case P_Info(0).RaceInfo.idraces

            Case B2B_RACEID
                Exclusions = New Double()() {B2B_EXCL_1}
            Case Else
                Exclusions = New Double()() {DeepSouthE, DeepSouthW}

        End Select
    End Sub

    Public Shared ReadOnly Property Base_Game_Url() As String
        Get
            'Return S10_SERVER
            If _Servers Is Nothing Then
                Return S10_SERVER
            End If
            Static index As Integer = 0

            index += 1
            If index > 50000 Then
                index = 0
            End If
            Return _Servers(index Mod _Servers.Length)
        End Get
    End Property

    Public Shared ReadOnly Property BASE_SAIL_URL() As String
        Get
            Return Base_Game_Url & "/Polaires/"
        End Get
    End Property



    Public Shared ReadOnly Property BaseFileDir() As String
        Get
            Static Checked As Boolean = False
            If Not Checked AndAlso Not System.IO.Directory.Exists(_BaseFileDir) Then
                Try
                    System.IO.Directory.CreateDirectory(_BaseFileDir)
                Catch ex As Exception
                    MessageBox.Show(ex.Message, "Error creating the application data folder", MessageBoxButton.OK, MessageBoxImage.Error)
                    Throw ex
                End Try
            End If
            Checked = True
            Return _BaseFileDir
        End Get
    End Property

    Public ReadOnly Property BoatCanvasX() As Double
        Get
            If VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.position Is Nothing Then
                Return 0
            End If
            Return _2DViewer.LonToCanvas(VorHandler.UserInfo.LON)
        End Get
    End Property

    Public ReadOnly Property BoatCanvasY() As Double
        Get
            If VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.position Is Nothing Then
                Return 0
            End If

            Return _2DViewer.LatToCanvas(VorHandler.UserInfo.LAT)
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

    Public ReadOnly Property MapLayer As ObservableCollection(Of ZoneMeteoInfo)
        Get
            If _MapLayer Is Nothing Then
                Return Nothing
            End If
            Return _MapLayer.MeteoInfoList
        End Get
    End Property

    Public Property MapMenuEnabled() As Boolean
        Get
            Return _MapMenuEnabled
        End Get
        Set(ByVal value As Boolean)
            _MapMenuEnabled = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MapMenuEnabled"))
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

        If Not _MeteoMapper Is Nothing AndAlso _MeteoMapper.ImageReady Then
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoMapper"))
        End If

        If Not MeteoMapper Is Nothing AndAlso Now.Subtract(MeteoMapper.Date).TotalMinutes > VacationMinutes Then
            MeteoMapper.Date = Now
        End If

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoValidSpan"))

    End Sub

    Public Shared Property CurWP() As Integer
        Get
            Return _CurWP
        End Get
        Set(ByVal value As Integer)
            _CurWP = value
            If WPList.Count >= 1 AndAlso _CurWP > 0 AndAlso _CurWP < WPList.Count Then
                _WPList(0) = "<Auto> " & _P_Info(0).RaceInfo.races_waypoints(_CurWP).ToString
            End If

        End Set

    End Property

    Public ReadOnly Property RecordRoute() As DelegateCommand(Of Object)
        Get
            Return _RecordRoute
        End Get
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
        NoExclusionZone = RacePrefs.NoExclusionZone
        Dim i As Integer

        For i = 0 To 3
            _RaceZoneOffsets(i) = RacePrefs.RaceOffset(i)
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RacePrefs"))


    End Sub

    Private Sub LoadRaceInfo(ByVal P As RegistryPlayerInfo)

        Dim RaceNum As String = P.RaceNum
#If TESTING Then
        Dim RaceFileName As String = BaseFileDir & "\T_RI_" & RaceNum & ".ini"
#Else
        Dim RaceFileName As String = BaseFileDir & "\RI_" & RaceNum & ".ini"

#End If
        Dim RaceInfo As String

        If System.IO.File.Exists(RaceFileName) Then
            'Get the race list to check fro any update since last file desc load
            Dim RaceUpdateDate = (From R In WS_Wrapper.GetRaceList() Where R.idraces = RaceNum Select R.updated)

            If RaceUpdateDate.Any Then
                Try
                    Dim RaceLastUpdate As DateTime
                    Dim RaceLastDownload As DateTime = New System.IO.FileInfo(RaceFileName).CreationTime
                    If Not Date.TryParse(RaceUpdateDate(0), RaceLastUpdate) OrElse RaceLastUpdate > RaceLastDownload Then
                        'File is outdate or else Something's wrong delete the file
                        System.IO.File.Delete(RaceFileName)
                    End If
                Catch ex As Exception
                    'just swallow that and proceed
                End Try
            End If
        End If
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
            Info = Parse(RaceInfo)
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Exception loading Json formatted race info!!")
        End Try


        If Info IsNot Nothing AndAlso Info.ContainsKey(JSONDATA_BASE_OBJECT_NAME) Then
            LoadJSonDataToObject(_P_Info(0).RaceInfo, Info(JSONDATA_BASE_OBJECT_NAME))
            RouteurModel.VacationMinutes = _P_Info(0).RaceInfo.vacfreq
        End If

    End Sub

    Public ReadOnly Property MeteoMapper As MeteoBitmapper
        Get
            If _MeteoMapper Is Nothing AndAlso VorHandler IsNot Nothing AndAlso VorHandler.Meteo IsNot Nothing AndAlso _2DViewer IsNot Nothing Then
                _MeteoMapper = New MeteoBitmapper(VorHandler.Meteo, _2DViewer)
            End If
            Return _MeteoMapper
        End Get
    End Property


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
        _ClearBoats = True

    End Sub

    Private Property Cron As Cron

    Public Shared Property CourseExtensionHours() As Double
        Get
            Return _RouteExtensionHours
        End Get
        Set(ByVal value As Double)
            _RouteExtensionHours = value
        End Set
    End Property

    Public ReadOnly Property RouteManager() As RouteManager
        Get
            If _RouteManager Is Nothing Then
                _RouteManager = New RouteManager(Me, VorHandler.Meteo, VLM_Router.Sails)
            End If
            Return _RouteManager
        End Get
    End Property

    Public Property Scale() As Double
        Get
            Return _Scale
        End Get
        Set(ByVal value As Double)
            _Scale = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Scale"))
        End Set
    End Property

    Public Property ShowEasyNav() As Boolean
        Get
            Return _ShowEasyNav
        End Get
        Set(ByVal value As Boolean)
            _ShowEasyNav = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ShowEasyNav"))
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

    Private Sub _VorHandler_IsoComplete(ByVal Pt As VLM_Router.clsrouteinfopoints) Handles _VorHandler.IsoComplete
        If RacePrefs.GetRaceInfo(CurPlayer.RaceInfo.idraces).SaveRoute Then
            Dim Route As New RoutePointInfo("IsoRoute " & Now.ToString, Pt)
            _RouteManager.AddNewRoute(CurPlayer.RaceInfo.idraces, "IsoRoute " & Now.ToString, Route)
            _RouteManager.Save()
        End If
        If Not RacePrefs.GetRaceInfo(CurPlayer.RaceInfo.idraces).AutoRestartRouter Then
            IsoRouterActive = False
        End If


    End Sub


    Private Sub _VorHandler_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _VorHandler.PropertyChanged


        Select Case e.PropertyName

            Case "UserInfo", "BruteRoute", "BestRouteAtPoint", "BestVMGRoute", "TempRoute", "TempVMGRoute", "GridRoute", "ClearGrid"
                If e.PropertyName = "ClearGrid" Then
                    _ClearGrid = True
                ElseIf e.PropertyName = "UserInfo" Then
                    If Not RouteManager Is Nothing Then
                        RouteManager.Curpos = New Coords(VorHandler.UserInfo.Position)
                    End If

                End If
                If tmrRefresh IsNot Nothing Then
                    tmrRefresh.IsEnabled = True
                End If

            Case "MeteoArrowDate"
                If Not _MapLayer Is Nothing AndAlso VorHandler.MeteoVisible Then
                    _MapLayer.RefreshInfo(_NOPoint, _SEPoint, _VorHandler.MeteoArrowDate)
                End If

            Case "WPList"
                CurWP = CurWP
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WPList"))

            Case Else
                RaiseEvent PropertyChanged(sender, e)
        End Select
    End Sub

    Private Sub _2DViewer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _2DViewer.PropertyChanged

        Select Case e.PropertyName
            Case "Drawn"

                SyncLock _2DViewerLock
                    _Busy = False
                End SyncLock

            Case "CurCoords"
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf VorHandler.DeferedUpdateWPDist, _2DViewer.CurCoords)
                VorHandler.MouseOver(_2DViewer.CurCoords)
            Case "Refresh"
                tmrRefresh.IsEnabled = True

            Case "Busy"
                MapMenuEnabled = Not The2DViewer.TileServerBusy

        End Select
    End Sub

    Private Sub tmrRefresh_Elapsed(ByVal sender As Object, ByVal e As EventArgs)

        tmrRefresh.IsEnabled = False
        If System.Threading.Monitor.TryEnter(Me) Then
            If Not The2DViewer Is Nothing And Not _Busy Then
                _Busy = True
                Dim routes(6) As ObservableCollection(Of VLM_Router.clsrouteinfopoints)
                'Static Moorea As ObservableCollection(Of VLM_Router.clsrouteinfopoints) = Nothing

                routes(0) = VorHandler.PlannedRoute
                routes(1) = VorHandler.BestRouteAtPoint
                routes(2) = VorHandler.BruteRoute
                routes(3) = VorHandler.TempRoute
                routes(4) = VorHandler.TempVMGRoute
                routes(5) = VorHandler.AllureRoute
                routes(6) = VorHandler.PilototoRoute
                'If Moorea Is Nothing Then
                '    Moorea = New ObservableCollection(Of clsrouteinfopoints)
                '    Dim db As New DBWrapper
                '    db.MapLevel = 5
                '    Dim segments As List(Of MapSegment) = db.SegmentList(-150, -17, -149.45, -17.6, True)
                '    For Each seg In segments
                '        Moorea.Add(New clsrouteinfopoints() With {.P = New Coords(seg.Lat1, seg.Lon1)})
                '    Next
                'End If
                'routes(7) = Moorea

                Dim Traj As String = ""

                If VorHandler Is Nothing OrElse VorHandler.UserInfo Is Nothing OrElse VorHandler.UserInfo.track Is Nothing Then
                    Traj = ""
                Else
                    Traj = VorHandler.Track

                End If


                Dim Rtes As List(Of ObservableCollection(Of VLM_Router.clsrouteinfopoints)) = New List(Of ObservableCollection(Of VLM_Router.clsrouteinfopoints))(routes)
                'ReDim Rtes(routes.Length)
                'Dim Index As Integer = 0
                'For Each Item In routes
                'Rtes(Index) = New ObservableCollection(Of VLM_Router.clsrouteinfopoints)(Item)
                'Index += 1
                'Next
                The2DViewer.UpdatePath(Traj, Rtes, 6, VorHandler.Opponents, _ClearGrid, _ClearBoats, VorHandler.IsoChrones, CurPlayer.RaceInfo.races_waypoints, _RouteManager.VisibleRoutes)
                _ClearGrid = False
                _ClearBoats = False
            Else
                tmrRefresh.IsEnabled = True
            End If
            System.Threading.Monitor.Exit(Me)
        Else
            tmrRefresh.Start()
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

#Region "ContextMenuHandlers"

    Private _BearingMenu As New DelegateCommand(Of Object)(AddressOf OnSetBearingHandler, Function(o) True)
    Private _WindAngleMenu As New DelegateCommand(Of Object)(AddressOf OnSetWindAngleHandler, Function(o) True)
    Private _SetNAVWPMenu As New DelegateCommand(Of Object)(AddressOf OnSetWPNAVMenuHandler, Function(o) True)
    Private _SetNAVModeMenu As New DelegateCommand(Of Object)(AddressOf OnSetNAVMenuHandler, Function(o) True)
    Private _SetNAVToMenu As New DelegateCommand(Of Object)(AddressOf OnSetNAVToMenuHandler, Function(o) True)

    Private _RecordRoute As New DelegateCommand(Of Object)(AddressOf OnRecordRouteMenuHandler, Function(o) True)

    Public ReadOnly Property SetBearingMenu() As ICommand
        Get
            Return _BearingMenu
        End Get
    End Property

    Public ReadOnly Property SetWindAngleMenu() As ICommand
        Get
            Return _WindAngleMenu
        End Get
    End Property

    Public ReadOnly Property SetNAVModeMenu() As ICommand
        Get
            Return _SetNAVModeMenu
        End Get
    End Property

    Public ReadOnly Property SetNAVToMenu() As ICommand
        Get
            Return _SetNAVToMenu
        End Get
    End Property

    Public ReadOnly Property SetNAVWPMenu() As ICommand
        Get
            Return _SetNAVWPMenu
        End Get
    End Property

    Private Sub OnAutoStartIsoRouter()

        'If VorHandler.StartIsoRoute Then
    End Sub

    Private Sub OnRecordRouteMenuHandler(ByVal o As Object)
        Dim RouteIndex As Integer = 0

        If VorHandler.RoutesUnderMouse Is Nothing OrElse VorHandler.RoutesUnderMouse.Count = 0 Then
            Return
        End If
        Dim Index As Integer
        For Each R In VorHandler.RoutesUnderMouse
            If R.RouteName = VLM_Router.KEY_ROUTE_THIS_POINT Then
                RouteIndex = Index
                Exit For
            End If
            Index += 1
        Next

        RouteManager.AddNewRoute(_P_Info(0).RaceInfo.idraces, _P_Info(0).RaceInfo.racename, VorHandler.RoutesUnderMouse(RouteIndex))
    End Sub

    Private Sub OnSetBearingHandler(ByVal O As Object)
        VorHandler.SetBearing()
    End Sub

    Private Sub OnSetWindAngleHandler(ByVal o As Object)
        If o Is Nothing Then
            VorHandler.SetWindAngle()
        ElseIf TypeOf o Is String AndAlso CStr(o) = "WP" Then
            VorHandler.SetWPWindAngle()
        End If
    End Sub

    Private Sub OnSetNAVMenuHandler(ByVal o As Object)
        If Not TypeOf o Is String Then
            Return
        End If

        Select Case CStr(o)
            Case "ORTHO", "VMG", "VBVMG"
                VorHandler.SetNav(CStr(o))

            Case Else
                Return
        End Select

    End Sub

    Private Sub OnSetNAVToMenuHandler(ByVal o As Object)
        If Not TypeOf o Is String Then
            Return
        End If

        Select Case CStr(o)
            Case "ORTHO", "VMG", "VBVMG"
                VorHandler.SetNAVWP(False, CStr(o))


            Case Else
                Return
        End Select

    End Sub

    Private Sub OnSetWPNAVMenuHandler(ByVal o As Object)

        If o Is Nothing Then
            VorHandler.SetNAVWP(False)
        ElseIf TypeOf o Is String AndAlso CStr(o) = "RAZ" Then
            VorHandler.SetNAVWP(True)
        End If
    End Sub


#End Region

    Protected Overrides Sub Finalize()

        MyBase.Finalize()
    End Sub

    Private Sub _MapLayer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _MapLayer.PropertyChanged

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MapLayer"))
    End Sub

    Private Sub InitCron()

        Cron.Stop()
        Cron.ClearTasks()

        Dim AppPrefs As New AppPrefs
        AppPrefs.Load(AppPrefs)

        If AppPrefs.AutoStartRouterAtMeteoUpdate Then
            Dim T As New CronTask(AddressOf OnAutoStartIsoRouter)
            Dim CurTime As New DateTime(CLng(Math.Floor(Now.Ticks / TimeSpan.TicksPerHour)), DateTimeKind.Utc)
            CurTime = CurTime.AddMinutes(-CurTime.Minute).AddSeconds(-CurTime.Second).AddHours(Math.Floor(CurTime.Hour / 6) * 6)
            CurTime = TimeZoneInfo.ConvertTimeFromUtc(CurTime, TimeZoneInfo.Local)
            CurTime = CurTime.AddMinutes(5)
            T.NextStartTime = CurTime
            T.Period = New TimeSpan(6, 0, 0)

        End If


        Cron.Start()
    End Sub

End Class
