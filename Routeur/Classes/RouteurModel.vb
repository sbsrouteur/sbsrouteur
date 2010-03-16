Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class RouteurModel

    Implements INotifyPropertyChanged


    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Event BoatsDrawn()

    Private WithEvents _VorHandler As VOR_Router
    Private WithEvents _2DViewer As _2D_Viewer

    Private WithEvents _CurPlayer As clsPlayerInfo
    Public Shared PropTmpRoute As New PropertyChangedEventArgs("TmpRoute")
    Private _RaceZoneOffsets() As Double = New Double() {0.5, 0.5, 0.5, 0.5}
    Private WithEvents tmrRefresh As New System.Timers.Timer(500) With {.Enabled = False}
    Private WithEvents _stats As New Stats
    Private Shared _NoObstacle As Boolean
    Private _ClearBoats As Boolean = False
    Private Shared _WPList As New List(Of String)

    Public Shared BaseFileDir As String = Environment.GetEnvironmentVariable("APPDATA") & "\sbs\Routeur"

    Private Enum RaceZoneDirs As Integer
        East = 0
        North = 1
        West = 2
        South = 3
    End Enum

    Public Shared SCALE As Double = 1
    Public Shared LAT_OFFSET As Double = 0
    Public Shared LON_OFFSET As Double = 0

    Public Const PenWidth As Double = 0.3

    Public Const S11_SERVER As String = "http://virtual-loup-de-mer.org"
    Public Const S10_SERVER As String = "http://tcv.virtual-loup-de-mer.org"
    Public Const BASE_SAIL_URL As String = "http://www.virtual-loup-de-mer.org/Polaires/"

    Public Shared BASE_GAME_URL As String = S11_SERVER

    Public Shared VacationMinutes As Double = 5
    Public Shared CurWP As Integer = 0
    Public Shared MapLevel As String = "l"

    Private Shared _P_Info(0) As clsPlayerInfo



    Public Shared START_LON As Double = 1.186
    Public Shared START_LAT As Double = 46.142

    Public Shared URL_Version As Integer = 7

    Public Shared _RaceRect As Coords()


    Public Shared GRID_FILE As String

    Public Shared SouthTrimList() As Integer = New Integer() {}
    Public Shared NorthTrimList() As Integer = New Integer() {}
    Public Shared EastTrimList() As Integer = New Integer() {}
    Public Shared WestTrimList() As Integer = New Integer() {}

    Public Shared Exclusions()() As Double

    Public Shared HasCurrents As Boolean = False

    Public Shared GridGrain As Double = 0.02
    Public Shared ShowGrid As Boolean = False

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

        _P_Info(0) = New clsPlayerInfo
        LoadParams()
        CheckPassword()

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

        For Each WPs In P_Info(0).RouteWayPoints

            For Each WP In WPs
                For Each P In WP
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

        ReDim RouteurModel._RaceRect(3)
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

    Private Sub ReadInfoSection(ByVal S As System.IO.StreamReader)
        Dim Line As String = S.ReadLine.Trim
        Dim StartLat As Double
        Dim StartLon As Double

        Do

            If Not Line.IndexOf("=") = -1 Then

                Dim PosEq As Integer = Line.IndexOf("="c)
                Select Case Line.Substring(0, PosEq).ToLowerInvariant

                    Case "gridgrain"
                        Double.TryParse(Line.Substring(PosEq + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, GridGrain)

                    Case "maplevel"
                        MapLevel = Line.Substring(PosEq + 1).ToLower

                    Case "nick"
                        P_Info(0).Nick = Line.Substring(PosEq + 1)

                    Case "numboat"
                        Integer.TryParse(Line.Substring(PosEq + 1), P_Info(0).NumBoat)

                    Case "showgrid"
                        Dim tmpInt As Integer
                        Integer.TryParse(Line.Substring(PosEq + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, tmpInt)
                        If tmpInt = 42 Then
                            RouteurModel.ShowGrid = True
                        End If
                    Case "startlat"
                        Double.TryParse(Line.Substring(PosEq + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, StartLat)

                    Case "startlon"
                        Double.TryParse(Line.Substring(PosEq + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, StartLon)


                End Select

            End If

            Line = S.ReadLine.Trim
        Loop Until S.EndOfStream OrElse Line = ""

        ReDim P_Info(0).Route(0)

        RouteurModel.START_LAT = StartLat
        RouteurModel.START_LON = StartLon
        RouteurModel.ShowGrid = True

    End Sub

    Private Sub ReadRaceZoneOffsets(ByVal S As System.IO.StreamReader)
        Dim Line As String = S.ReadLine.Trim.ToLowerInvariant
        Dim dValue As Double
        Dim zone As RaceZoneDirs

        Do

            If Not Line.IndexOf("=") = -1 Then

                Dim PosEq As Integer = Line.IndexOf("="c)
                Double.TryParse(Line.Substring(PosEq + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, dValue)

                Select Case Line.Substring(0, PosEq).ToLowerInvariant

                    Case "east"
                        zone = RaceZoneDirs.East

                    Case "west"
                        zone = RaceZoneDirs.West

                    Case "north"
                        zone = RaceZoneDirs.North

                    Case "south"
                        zone = RaceZoneDirs.South

                End Select

            End If
            _RaceZoneOffsets(zone) += dValue

            Line = S.ReadLine
            If Not Line Is Nothing Then
                Line = Line.Trim
            End If

        Loop Until S.EndOfStream OrElse Line = ""

    End Sub

    Private Sub ReadRouteSection(ByVal S As System.IO.StreamReader)
        Dim Line As String = S.ReadLine.Trim
        Dim CurWP As Integer = 0
        _WPList.Clear()
        _WPList.Add("<Auto>")

        ReDim P_Info(0).Route(10)
        Do

            If Not Line.IndexOf(CChar(vbTab)) = -1 Then

                Dim PosEq As Integer = Line.IndexOf(CChar(vbTab))
                If Line.ToLowerInvariant.StartsWith("wp") Then
                    Dim Lat(1) As Double
                    Dim Lon(1) As Double
                    Dim i As Integer

                    Dim Fields() As String = Line.Substring(PosEq + 1).Split(CChar(vbTab))

                    For i = 0 To 1
                        Double.TryParse(Fields(2 * i), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lat(i))
                        Double.TryParse(Fields(2 * i + 1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, Lon(i))

                    Next
                    _WPList.Add(Fields(5))
                    P_Info(0).Route(CurWP) = New Coords(Lat(0), Lon(0))
                    Dim L As New List(Of Coords())
                    Dim C() As Coords = New Coords() {New Coords(Lat(0), Lon(0)), New Coords(Lat(1), Lon(1))}
                    L.Add(C)
                    P_Info(0).RouteWayPoints.Add(L)
                    CurWP += 1
                    If CurWP = P_Info(0).Route.Length Then
                        ReDim Preserve P_Info(0).Route(CurWP + 10)
                    End If

                End If

            End If
            If Not S.EndOfStream Then
                Line = S.ReadLine.Trim
            Else
                Line = ""
            End If
        Loop Until Line = "" 'OrElse S.EndOfStream

        ReDim Preserve P_Info(0).Route(CurWP - 1)

    End Sub


    Public Sub Refresh()

        If Not The2DViewer Is Nothing Then
            The2DViewer.RedrawCanvas()
        End If


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

        Dim S As System.IO.StreamReader = Nothing
        Try
            If Not System.IO.File.Exists(BaseFileDir & "\Routeur.ini") Then
                If Not System.IO.Directory.Exists(BaseFileDir) Then
                    System.IO.Directory.CreateDirectory(BaseFileDir)
                End If

                System.IO.File.Copy(".\Routeur.ini", BaseFileDir & "\Routeur.ini")
                MessageBox.Show("Edit your Routeur ini file and restart Routeur")
                Process.Start(BaseFileDir & "\Routeur.ini")
                End
            End If

            S = New System.IO.StreamReader(basefiledir & "\Routeur.ini")

            Dim Line As String = ""

            While Not S.EndOfStream
                Line = S.ReadLine.Trim.ToLowerInvariant

                If Not Line = "" Then

                    If Line(0) = "["c Then

                        Select Case Line.Substring(1, Line.IndexOf("]"c) - 1)

                            Case "info"
                                ReadInfoSection(S)

                            Case "route"
                                ReadRouteSection(S)

                            Case "racezoneoffset"
                                ReadRaceZoneOffsets(S)
                        End Select

                    End If



                End If
            End While

        Catch ex As Exception
            MessageBox.Show(Nothing, ex.Message, "Error reading parameters", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.None)
        Finally
            If Not S Is Nothing Then
                S.Close()
                S = Nothing
            End If
        End Try
    End Sub

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

End Class
