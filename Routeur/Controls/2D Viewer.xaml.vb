Imports System
Imports System.Collections.Generic
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Media.Imaging
Imports System.Windows.Navigation
Imports System.Windows.Shapes
Imports System.Math

Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Threading

Partial Public Class _2D_Viewer

    Implements INotifyPropertyChanged
    Private Const DPI_RES As Integer = 96
    Private Const XBMP_RES As Integer = 360
    Private Const YBMP_RES As Integer = 180
    Public Const DEFINITION As Integer = 10

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged



    Private _WindArrows(30, 30) As Path
    Private _TrajPath As Path
    Private _RBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
    Private _BackDropBmp As RenderTargetBitmap
    Private _OpponentsBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
    Private _GridBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
    Private _RoutesBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
    'Private _gshhs As New GSHHS_Reader
    Private _CurCoords As New Coords
    Private _P As Point
    Private _Progress As MapProgressContext
    Private _ClearBgMap As Boolean = False

    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(48, 12), New Coords(48, -13), New Coords(55, -13), New Coords(55, -2), _
    '                                                     New Coords(59, 5), New Coords(62, 20), New Coords(48.1, 12)}
    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(58, 10), New Coords(48, 10), New Coords(48, 20), New Coords(60, 20), New Coords(60, 16.5), _
    '                                                     New Coords(56 + 17 / 60, 16 + 28 / 60), New Coords(58, 14) _
    '                                                     }

    Private Shared _RacePolygons As New LinkedList(Of Polygon)()
    Private Shared _RacePolygonsInited As Boolean = False
    Private WithEvents _Frm As frmRoutingProgress
    Private WithEvents _MapPg As New MapProgressContext("Drawing Map...")
    Private _TileCount As Integer = 0

    Private _Scale As Double
    Private _LonOffset As Double
    Private _LatOffset As Double
    Private Shared _CenterOnAnteMeridien As Boolean = False
    Private _HideIsochrones As Boolean = False
    Private _EraseIsoChrones As Boolean = False
    Private _EraseBoatMap As Boolean = False
    Private WithEvents _TileServer As TileServer
    Private _PendingTileRequestCount As Integer = 0
    Private _ReadyTilesQueue As New Queue(Of TileInfo)

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d'objet sous ce point.

        '_Model = CType(FindResource("2DModel"), _2DViewerModel)
        _TileServer = New TileServer(Me)

    End Sub

    Public Function CanvasToLat(ByVal C As Double) As Double
        Dim Ret As Double = (90 * DEFINITION - C) / Scale + LatOffset '90 / RouteurModel.SCALE - C / DEFINITION / RouteurModel.SCALE + RouteurModel.LAT_OFFSET
        Ret = Ret / 180 * PI
        Return Math.Atan(Math.Sinh(Ret)) / PI * 180
    End Function

    Public Function CanvasToLon(ByVal V As Double) As Double
        Return (V - 180 * DEFINITION) / Scale + LonOffset '(V - 180 * DEFINITION) / DEFINITION / RouteurModel.SCALE
    End Function


    Public Function LatToCanvas(ByVal V As Double) As Double

        V = V / 180 * PI
        V = Log(Tan(V) + 1 / Cos(V))
        V = V / PI * 180
        Return 90 * DEFINITION - (V - LatOffset) * Scale

    End Function

    Public Function LonToCanvas(ByVal V As Double) As Double

        If CenterMapOnAnteMeridien AndAlso V < 0 Then
            Return 180 * DEFINITION + (V - LonOffset + 360) * Scale
        Else
            Return 180 * DEFINITION + (V - LonOffset) * Scale
        End If

    End Function

    Private Sub RenDerBackDrop(ByVal D As DrawingVisual)

    End Sub

    Private Sub LoadPolygonComplete()

        _RacePolygonsInited = True

    End Sub

#If NO_TILES Then
    Private Function BgBackDropDrawing(ByVal state As Object) As Boolean


        Dim D As New DrawingVisual
        Dim DC As DrawingContext = D.RenderOpen()
        Dim FirstPoint As Boolean
        Dim P0 As Point
        Dim P1 As Point
        Dim Pen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Black), 0.3)
        Dim WPPen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), 2)
        Dim LocalBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
        Dim RaceZone As New List(Of Polygon)
        Dim polyindex As Integer = -1
        Dim MinLon As Double = 180
        Dim MaxLon As Double = -180
        Dim MinLat As Double = 90
        Dim MaxLat As Double = -90
        Dim drawn As Boolean
        Dim PrevIndex As Integer
        Dim WPs As List(Of VLM_RaceWaypoint) = CType(state, List(Of VLM_RaceWaypoint))


        While Not _RacePolygonsInited
            System.Threading.Thread.Sleep(100)
        End While


        RaceZone.Add(RouteurModel._RaceRect)

        'Dim Prevbmp As RenderTargetBitmap = Nothing
        Dim LineCount As Integer
        Dim PrevI As Integer = -1
        Static R As New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION)

        If GSHHS_Reader.AllPolygons.Count = 0 Then
            Return False
        End If
        Pen.Freeze()

        For Each LC In RaceZone
            For Each C In LC
                If Not C Is Nothing Then
                    If C.Lat < MinLat Then
                        MinLat = C.Lat
                    End If
                    If C.Lat > MaxLat Then
                        MaxLat = C.Lat
                    End If
                    If C.Lon < MinLon Then
                        MinLon = C.Lon
                    End If
                    If C.Lon > MaxLon Then
                        MaxLon = C.Lon
                    End If
                End If
            Next
        Next

        _MapPg.Start(GSHHS_Reader.AllPolygons.Count)
        For Each C_Array In GSHHS_Reader.AllPolygons
            polyindex += 1


            FirstPoint = True
            If C_Array.Count > 1 Then
                P0.X = LonToCanvas(C_Array(C_Array.Count - 1).Lon_Deg)
                P0.Y = LatToCanvas(C_Array(C_Array.Count - 1).Lat_Deg)
                PrevIndex = C_Array.Count - 1
                For i = 0 To C_Array.Count - 1


                    If C_Array(i) IsNot Nothing Then 'AndAlso C_Array(i - 1) IsNot Nothing Then
                        'If GSHHS_Reader.HitTest(C_Array(i), 0, RaceZone, False) Then
                        If C_Array(i).Lon >= MinLon AndAlso _
                            C_Array(i).Lon <= MaxLon AndAlso _
                            C_Array(i).Lat >= MinLat AndAlso _
                            C_Array(i).Lat <= MaxLat Then

                            'p0.X = LonToCanvas(C_Array(i - 1).Lon_Deg)
                            'p0.Y = LatToCanvas(C_Array(i - 1).Lat_Deg)
                            P1.X = LonToCanvas(C_Array(i).Lon_Deg)
                            P1.Y = LatToCanvas(C_Array(i).Lat_Deg)
                            If CInt(P0.X) <> CInt(P1.X) OrElse CInt(P0.Y) <> CInt(P1.Y) Then

                                SafeDrawLine(DC, C_Array(PrevIndex), C_Array(i), Pen, P0, P1)

                                LineCount += 1
                                drawn = True
                                P0.X = P1.X
                                P0.Y = P1.Y
                                PrevIndex = i
                            End If
                        End If

                    End If

                    If drawn AndAlso LineCount Mod 500 = 0 Then
                        drawn = False
                        DC.Close()
Render1:
                        Try
                            LocalBmp.Render(D)
                        Catch
                            GoTo Render1
                        End Try
                        LocalBmp.Freeze()
                        _BackDropBmp = LocalBmp
                        DC = D.RenderOpen
                        'Prevbmp = LocalBmp
                        DC.DrawImage(_BackDropBmp, R)
                        'LocalBmp.Clear()
                        LocalBmp = New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
                        'LocalBmp.Clear()
                        If LineCount Mod 2000 = 0 Then
                            GC.Collect()
                        End If
                    End If

                Next
                'If LineCount Mod 1000 = 0 Then
                '    DC.Close()
                '    LocalBmp.Render(D)
                '    LocalBmp.Freeze()
                '    _BackDropBmp = LocalBmp
                '    DC = D.RenderOpen
                '    'Prevbmp = LocalBmp
                '    DC.DrawImage(_BackDropBmp, R)
                '    LocalBmp = New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
                '    'LocalBmp.Clear()
                '    GC.Collect()
                'End If
            End If
            _MapPg.Progress(polyindex)
        Next

        Dim WP As VLM_RaceWaypoint

        For Each WP In WPs
            P0.X = LonToCanvas(WP.WPs(0)(0).Lon_Deg)
            P0.Y = LatToCanvas(WP.WPs(0)(0).Lat_Deg)
            P1.X = LonToCanvas(WP.WPs(0)(1).Lon_Deg)
            P1.Y = LatToCanvas(WP.WPs(0)(1).Lat_Deg)

            SafeDrawLine(DC, WP.WPs(0)(0), WP.WPs(0)(1), WPPen, P0, P1)
        Next

        DC.Close()
        LocalBmp.Render(D)
        LocalBmp.Freeze()
        _BackDropBmp = LocalBmp
        'GC.Collect()

        Return True

    End Function

#Else

    Private Sub BgBackDropDrawing(ByVal state As Object)

        'Scale = 360 / Math.Abs(C1.Lon_Deg - C2.Lon_Deg)
        Dim Width As Double = 360 / Scale ' ._RaceRect(1).Lon_Deg - RouteurModel._RaceRect(0).Lon_Deg
        Dim Z As Integer = CInt(Math.Floor(Math.Log(360 / Width) / Math.Log(2))) + 1
        Dim North As Double = CanvasToLat(0)
        Dim West As Double = CanvasToLon(0)
        _TileCount = 0
        Dim TI As TileInfo
        Dim tx1 As Integer
        Dim ty1 As Integer
        Dim tx2 As Integer
        Dim ty2 As Integer
        Dim W As Double = CanvasToLon(0)
        Dim N As Double = CanvasToLat(0)
        Dim E As Double = CanvasToLon(0 + TileServer.TILE_SIZE)
        Dim S As Double = CanvasToLat(0 + TileServer.TILE_SIZE)

#Const DEBUG_TILE_LEVEL = 0
#If DEBUG_TILE_LEVEL = 0 Then
        TI = New TileInfo(Z, N, S, E, W)
        tx1 = TI.TX
        ty1 = TI.TY
        W = CanvasToLon(XBMP_RES * DEFINITION - TileServer.TILE_SIZE)
        N = CanvasToLat(YBMP_RES * DEFINITION - TileServer.TILE_SIZE)
        E = CanvasToLon(XBMP_RES * DEFINITION + TileServer.TILE_SIZE)
        S = CanvasToLat(YBMP_RES * DEFINITION)
        TI = New TileInfo(Z, N, S, E, W)
        tx2 = TI.TX
        ty2 = TI.TY
        _MapPg.Start((tx2 - tx1) * (ty2 - ty1))
        For i As Integer = tx1 To tx2
            For j As Integer = ty1 To ty2
                TI = New TileInfo(Z, i, j)

                System.Threading.Interlocked.Increment(_PendingTileRequestCount)
                _TileServer.RequestTile(TI)
            Next
        Next
        _MapPg.Start(_PendingTileRequestCount)
        '_MapPg.Start(100)
        '_TileServer.Render()
        '_TileServer.RequestTile(New TileInfo(1, -1, 0))
        '_TileServer.RequestTile(New TileInfo(1, 0, 0))
        '_TileServer.RequestTile(New TileInfo(1, -1, -1))
        '_TileServer.RequestTile(New TileInfo(1, 0, -1))
#Else
        Const Max As Integer = 1
        For i As Integer = 0 - CInt(2.0 ^ Max - 1) To CInt(2.0 ^ Max - 1) - 1

            For j As Integer = 0 - CInt(2.0 ^ Max - 1) To CInt(2.0 ^ Max - 1) - 1
                TI = New TileInfo(Max, i, j)

                System.Threading.Interlocked.Increment(_PendingTileRequestCount)
                _TileServer.RequestTile(TI)

            Next

        Next
        _TileServer.Render 
#End If

    End Sub

#End If

    Public Sub InitViewer(ByVal owner As System.Windows.Window)


        Try
            If Not _RacePolygons Is Nothing Then
                _RacePolygons.AddLast(RouteurModel._RaceRect)
            End If

            _Progress = New MapProgressContext("Loading Maps Data...")
            _Frm = New frmRoutingProgress(100, _Progress)
            Dim TH As New System.Threading.Thread(AddressOf GSHHS_Reader.Read)
            Dim SI As New GSHHS_StartInfo With {.PolyGons = _RacePolygons, .StartPath = "..\gshhs\gshhs_" & RouteurModel.MapLevel & ".b", _
                                                .ProgressWindows = _Progress, .CompleteCallBack = AddressOf LoadPolygonComplete, _
                                                .NoExclusionZone = RouteurModel.NoExclusionZone}
            _Frm.Show(owner, _Progress)
            TH.Start(SI)
            'GSHHS_Reader.Read(", RacePolygons, "Map")

        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

    Private Delegate Sub UpdatePathDelegate(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VLM_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                                                ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint), ByVal ManagedRoutes As IList(Of RecordedRoute))


    Private Sub SafeDrawLine(ByVal dc As DrawingContext, ByVal PrevP As Coords, ByVal P As Coords, ByVal pe As Pen, ByVal Prevpoint As Point, ByVal NewP As Point)
        If (PrevP.Lon * P.Lon < 0 AndAlso Math.Abs(P.Lon - PrevP.Lon) >= Math.PI) Then
            Dim Pint As Point
            Dim DLon As Double
            Dim LonSpan As Double
            If PrevP.Lon < 0 Then
                Pint.X = LonToCanvas(-179.99)
                DLon = 180 + PrevP.Lon_Deg
                LonSpan = PrevP.Lon_Deg + 360 - P.Lon_Deg
            Else
                Pint.X = LonToCanvas(179.99)
                DLon = 180 - PrevP.Lon_Deg
                LonSpan = Abs(-180 - P.Lon_Deg) + 180 - PrevP.Lon_Deg
            End If

            'Pint.Y = LatToCanvas(PrevP.Lat_Deg + (P.Lat_Deg - PrevP.Lat_Deg) * (PrevP.Lon_Deg + 180) / (360 + PrevP.Lon_Deg - P.Lon_Deg))
            Pint.Y = LatToCanvas(PrevP.Lat_Deg + (P.Lat_Deg - PrevP.Lat_Deg) * DLon / LonSpan)
            dc.DrawLine(pe, Prevpoint, Pint)

            If P.Lon < 0 Then
                Pint.X = LonToCanvas(-179.99)
            Else
                Pint.X = LonToCanvas(179.99)
            End If

            dc.DrawLine(pe, Pint, NewP)
        Else

            dc.DrawLine(pe, Prevpoint, NewP)

        End If
    End Sub
    Public Sub UpdatePath(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VLM_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                          ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint), ByVal ManagedRoutes As IList(Of RecordedRoute))

        Static Invoking As Integer = 0
        Static lastinvoke As DateTime = New DateTime(0)
        Dim Start As DateTime = Now
        Static dlg As New UpdatePathDelegate(AddressOf UpdatePath)
        Static Q As New Queue(Of System.Windows.Threading.DispatcherOperation)
        Static CurPos As New Coords


        If Not Dispatcher.Thread Is System.Threading.Thread.CurrentThread Then
            'Invoking = System.Threading.Interlocked.CompareExchange(Invoking, 1, 0)
            'If Invoking = 0 Then
            '    If Not ClearGrid And Now.Subtract(lastinvoke).TotalSeconds < 5 Then
            '        System.Threading.Interlocked.Exchange(Invoking, 0)
            '        Return
            '    End If
            '    lastinvoke = Now
            Dim R As System.Windows.Threading.DispatcherOperation = Dispatcher.BeginInvoke(dlg, New Object() {PathString, Routes, _
                                                                                Opponents, Grid, ClearGrid, ClearBoats, IsoChrones, WPs, ManagedRoutes})

            While Q.Count > 0
                Dim R2 As System.Windows.Threading.DispatcherOperation = CType(Q.Dequeue, System.Windows.Threading.DispatcherOperation)
                R2.Abort()
            End While
            Q.Enqueue(R)
            'End If
            Return
        End If

        Try
            'Static StartPath As DateTime = Now
            ' Const MAX_DRAW_MS As Integer = 100
            'If Not _RacePolygonsInited Then
            '    Return
            'End If

            'System.Threading.Interlocked.Exchange(Invoking, 1)
            Dim Coords() As String = Nothing
            Dim FirstPoint As Boolean = True
            Dim FirstLine As Boolean = True
            Dim CoordValue() As String
            Dim P1 As Point
            Static D As New DrawingVisual
            Dim DC As DrawingContext = D.RenderOpen()

            Dim PrevPoint As Point
            Static Pen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Black), RouteurModel.PenWidth)
            Static PathPen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Brown), RouteurModel.PenWidth)
            Static opponentPenNoOption As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Lime), RouteurModel.PenWidth)
            Static opponentPenOption As New Pen(New SolidColorBrush(System.Windows.Media.Colors.DarkRed), RouteurModel.PenWidth)
            Static BlackBrush As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Black), RouteurModel.PenWidth / 2)
            Static GridBrushes() As Pen
            Static WindBrushes() As Pen

            Static LocalDash As New DashStyle(New Double() {0, 2}, 0)
            Static routePen() As Pen = New Pen() {New Pen(New SolidColorBrush(System.Windows.Media.Colors.Blue), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Green), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Purple), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Purple), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round} _
                                               }

            'Static SecondroutePen() As Pen = New Pen() {New Pen(New SolidColorBrush(System.Windows.Media.Colors.Blue), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
            '                                            New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round, .DashStyle = LocalDash}, _
            '                                           New Pen(New SolidColorBrush(System.Windows.Media.Colors.Lime), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round, .DashStyle = LocalDash}, _
            '                                           New Pen(New SolidColorBrush(System.Windows.Media.Colors.Pink), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round, .DashStyle = LocalDash} _
            '                                   }
            Static Frozen As Boolean = False

            If Not Frozen Then
                Frozen = True
                Pen.Freeze()
                PathPen.Freeze()
                opponentPenNoOption.Freeze()
                opponentPenOption.Freeze()
                BlackBrush.Freeze()
                LocalDash.Freeze()
                For Each rp In routePen
                    rp.Freeze()
                Next

                'For Each sr In SecondroutePen
                '    sr.Freeze()
                'Next
            End If
            Dim PenNumber As Integer = 0
            Dim CrossLine As Boolean = False

            Dim PrevSide As Double
            Dim b As Byte
            Dim ShownPoints As Integer
            Static LimitDate As New DateTime(3000, 1, 1)
            Dim OpponentMap As Boolean = False
            Static BgStarted As Boolean = False
            Dim PrevP As New Coords
            Dim ForceIsoRedraw As Boolean = False

            'GC.Collect()

            'Dim Pixels As Array
            'If Monitor.TryEnter(Me) Then
            _RBmp.Clear()

            If _ClearBgMap Then
                BgStarted = False
                _ClearBgMap = False
                _BackDropBmp = Nothing

            End If

            If Not BgStarted AndAlso _BackDropBmp Is Nothing Then
                BgStarted = True 'BgBackDropDrawing(0)
                ForceIsoRedraw = True
                _Frm.DataContext = _MapPg
                System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgBackDropDrawing, WPs)
#If NO_TILES = 0 Then
            ElseIf _ReadyTilesQueue.Count <> 0 Then
                SyncLock _ReadyTilesQueue
                    Dim loopcount As Double = 0
                    While _ReadyTilesQueue.Count > 0 ' AndAlso loopcount < 5
                        Dim ti As TileInfo = _ReadyTilesQueue.Dequeue
                        DrawTile(ti, WPs)
                        loopcount += 1
                    End While
                End SyncLock

#End If
            End If

#If NO_TILES = 1 Then
                If _BackDropBmp IsNot Nothing AndAlso _BackDropBmp.IsFrozen Then
#Else
            If _BackDropBmp IsNot Nothing Then 'AndAlso _BackDropBmp.IsFrozen Then
#End If

                DC.DrawImage(_BackDropBmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))
            End If




            'debug bsp grid
            '
#Const DEBUG_BSP_GRID = 0

#If DEBUG_BSP_GRID = 1 Then
                Dim x As Double
                Dim y As Double

                For y = 36 To 38 Step 0.005
                    For x = 23 To 25 Step 0.005
                        P1.X = LonToCanvas(x)
                        P1.Y = LatToCanvas(y)
                        Dim c As New Coords(y, x)
                        If GSHHS_Reader.HitTest(c, 0, GSHHS_Reader.Polygons(c), True) Then
                            DC.DrawEllipse(Nothing, opponentPenOption, P1, 1, 1)
                        Else

                            DC.DrawEllipse(Nothing, opponentPenNoOption, P1, 1, 1)
                        End If
                    Next
                Next
#End If
            If False AndAlso Not ManagedRoutes Is Nothing Then
                _RoutesBmp.Clear()
                'Dim StartRoutes As DateTime = Now
                For Each route In ManagedRoutes

                    If route.Visible Then
                        Dim Pe As New Pen(route.ColorBrush, 1)
                        For i As Integer = 1 To route.Route.Count - 1

                            Dim PrevPt As Point
                            Dim CurPt As Point
                            PrevPt.X = LonToCanvas(route.Route(i - 1).P.Lon_Deg)
                            PrevPt.Y = LatToCanvas(route.Route(i - 1).P.Lat_Deg)
                            CurPt.X = LonToCanvas(route.Route(i).P.Lon_Deg)
                            CurPt.Y = LatToCanvas(route.Route(i).P.Lat_Deg)

                            SafeDrawLine(DC, route.Route(i - 1).P, route.Route(i).P, Pen, PrevPt, CurPt)

                        Next
                    End If
                Next
                DC.Close()
                _RoutesBmp.Render(D)
                DC = D.RenderOpen
            End If

            

            '
            ' Draw IsoChrones
            '
            If Not HideIsochrones AndAlso Not _EraseIsoChrones Then
                Dim StartIsochrone As DateTime = Now
                Try
                    If WindBrushes Is Nothing Then
                        ReDim WindBrushes(70)

                        For i = 0 To 69
                            WindBrushes(i) = New Pen(New SolidColorBrush(WindColors.GetColor(i)), 0.5)
                            WindBrushes(i).Freeze()
                        Next


                    End If

                    If ClearGrid Then
                        _GridBmp.Clear()
                    End If


                    If Not IsoChrones Is Nothing Then
                        For Each iso As IsoChrone In IsoChrones
                            FirstPoint = True
                            If Not iso.Drawn Or ForceIsoRedraw Then
                                Dim MaxIndex As Integer = iso.MaxIndex
                                Dim index As Integer
                                'Dim PrevIndex As Integer
                                Dim CurP As Coords
                                FirstPoint = True
                                For index = 0 To MaxIndex
                                    If Not iso.Data(index) Is Nothing AndAlso Not iso.Data(index).P Is Nothing Then
                                        CurP = iso.Data(index).P
                                        P1.X = LonToCanvas(CurP.Lon_Deg)
                                        P1.Y = LatToCanvas(CurP.Lat_Deg)

                                        If Not FirstPoint Then 'And index - PrevIndex < 4 Then
                                            If iso.Data(index).WindStrength <> 0 Then
                                                SafeDrawLine(DC, PrevP, CurP, WindBrushes(CInt(iso.Data(index).WindStrength)), PrevPoint, P1)
                                            End If
                                        Else
                                            FirstPoint = False
                                        End If
                                        'PrevIndex = index
                                        PrevP.Lon = CurP.Lon
                                        PrevP.Lat = CurP.Lat
                                        PrevPoint = P1

                                    Else
                                        FirstPoint = True
                                    End If
                                Next
                                iso.Drawn = True
                            End If

                            If Now.Subtract(Start).TotalMilliseconds > 100 Then
                                Exit For
                            End If
                        Next
                    End If

                Catch ex As Exception
                Finally
                    DC.Close()
                    _GridBmp.Render(D)
                    DC = D.RenderOpen

                End Try
            ElseIf _EraseIsoChrones Then
                _GridBmp.Clear()
                _EraseIsoChrones = False
                If IsoChrones IsNot Nothing Then
                    For Each iso In IsoChrones
                        iso.Drawn = False
                    Next
                End If
            End If

            If ClearBoats OrElse _EraseBoatMap Then
                _OpponentsBmp.Clear()
                _EraseBoatMap = False
            End If

            If Not Opponents Is Nothing Then
                SyncLock Opponents
                    For Each op In Opponents
                        If Not op.Value.Drawn Then
                            P1.X = LonToCanvas(op.Value.CurPos.Lon_Deg)
                            P1.Y = LatToCanvas(op.Value.CurPos.Lat_Deg)

                            DC.DrawEllipse(Nothing, opponentPenNoOption, P1, 1, 1)

                            'op.Value.Drawn = True
                            'OpponentMap = True
                        End If
                    Next
                End SyncLock
                DC.Close()
                _OpponentsBmp.Render(D)
                DC = D.RenderOpen
                'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return
            End If

            'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return
            '
            ' Draw routing grid
            '
            If Not OpponentMap And Not Grid Is Nothing Then

                If GridBrushes Is Nothing Then
                    ReDim GridBrushes(255)

                    For i = 0 To 255
                        b = CByte(i)
                        GridBrushes(b) = New Pen(New SolidColorBrush(Color.FromRgb(128, b, 0)), 0.5)
                        GridBrushes(b).Freeze()
                    Next

                End If
                ShownPoints = 0
                Try
                    If ClearGrid Then
                        _GridBmp.Clear()
                    End If

                    Dim GridSize As Double = 60 * RouteurModel.GridGrain / 0.01
                    Dim SelectionOffset As Double = GridSize / 6

                    For Each R As RoutingGridPoint In Grid.ToArray


                        If Not R Is Nothing AndAlso R.Drawn = False Then
                            'If CInt(R.CurETA.Subtract(Now).TotalHours) Mod 6 = 0 AndAlso Math.Abs(CInt(R.CurETA.Subtract(Now).TotalHours) - R.CurETA.Subtract(Now).TotalHours) < 0.1 Then
                            'If (R.CurETA.Hour Mod 12) = (8 + RouteurModel.GribOffset) AndAlso R.CurETA.Minute <= 20 Then
                            If R.CurETA.Subtract(Now).TotalMinutes Mod GridSize < SelectionOffset Then
                                'duration = CInt(R.CurETA.Subtract(Now).Totalminut) Mod 256
                                ShownPoints += 1

                                P1.X = LonToCanvas(R.P.P.Lon_Deg)
                                P1.Y = LatToCanvas(R.P.P.Lat_Deg)

                                DC.DrawEllipse(Nothing, BlackBrush, P1, 0.2, 0.2)
                                'DC.DrawEllipse(Nothing, GridBrushes(duration), P1, 0.2, 0.2)
                                'Debug.WriteLine("Point Brushed " & duration)
                                R.Drawn = True
                                'End If
                            End If
                        End If

                        'If ShownPoints Mod 100 = 0 And ShownPoints > 0 Then
                        '    DC.Close()
                        '    _GridBmp.Render(D)
                        '    DC = D.RenderOpen
                        'End If

                    Next
                Catch ex As Exception
                End Try


                DC.Close()
                _GridBmp.Render(D)
                DC = D.RenderOpen
                'Debug.WriteLine("Grid : " & ShownPoints)
                'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return
            End If

            DC.DrawImage(_RoutesBmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))
            DC.DrawImage(_OpponentsBmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))
            DC.DrawImage(_GridBmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))



            '
            ' Draw the recorded path
            '

            If PathString IsNot Nothing Then
                Coords = PathString.Split(";"c)

            End If

            ShownPoints = 0
            If Not Coords Is Nothing Then
                PrevSide = 0
                Dim Ignorepoints As Integer = 0 'Coords.Count - 99
                Dim Ignored As Integer = 0
                Dim Pnt As New Coords
                For Each C In Coords
                    If C <> "" Then
                        If Ignored >= Ignorepoints Then
                            CoordValue = C.Split("!"c)
                            Dim lon As Double
                            Dim lat As Double

                            Double.TryParse(CoordValue(0), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, lon)
                            Double.TryParse(CoordValue(1), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, lat)

                            P1.X = LonToCanvas(lon)
                            P1.Y = LatToCanvas(lat)

                            CurPos.Lon_Deg = lon
                            CurPos.Lat_Deg = lat

                            CrossLine = (Val(CoordValue(0)) - PrevSide) > 180
                            If ShownPoints > 0 Then

                                'If PrevP.Lon * lon < 0 AndAlso Math.Abs(lon - PrevP.Lon) >= 180 Then
                                '    Dim Pint As Point
                                '    Pint.X = LonToCanvas(-180)
                                '    Pint.Y = LatToCanvas(PrevP.Lat_Deg + (lat - PrevP.Lat_Deg) * (PrevP.Lon_Deg + 180) / (360 + PrevP.Lon_Deg - lon))
                                '    DC.DrawLine(PathPen, PrevPoint, Pint)
                                '    Pint.X = LonToCanvas(180)
                                '    DC.DrawLine(routePen(PenNumber), Pint, P1)
                                'Else
                                '    DC.DrawLine(PathPen, PrevPoint, P1)
                                'End If
                                Pnt.Lon_Deg = lon
                                Pnt.Lat_Deg = lat

                                SafeDrawLine(DC, PrevP, Pnt, PathPen, PrevPoint, P1)
                            End If
                            PrevP.Lon_Deg = lon
                            PrevP.Lat_Deg = lat
                            PrevPoint = P1
                            PrevSide = Val(CoordValue(0))
                            'End If
                            ShownPoints += 1
                        Else
                            Ignored += 1
                        End If
                    End If

                    'If ShownPoints Mod 100 = 0 And ShownPoints > 0 Then
                    '    DC.Close()
                    '    _RBmp.Render(D)
                    '    DC = D.RenderOpen
                    'End If

                Next
            End If
            'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return

            '
            ' Draw the other routes
            '
            If Not OpponentMap And Not Routes Is Nothing Then
                Try
                    ShownPoints += 1
                    PenNumber = 0
                    Dim RouteIsWP As Boolean = True
                    Dim CurWP As Integer = 1
                    For Each R In Routes

                        If Not R Is Nothing Then
                            FirstPoint = True
                            For Each P In R
                                If Not P Is Nothing AndAlso Not P.P Is Nothing AndAlso (Not RouteIsWP OrElse CurWP >= RouteurModel.CurWP) Then
                                    P1.X = LonToCanvas(P.P.Lon_Deg)
                                    P1.Y = LatToCanvas(P.P.Lat_Deg)
                                    If FirstPoint Then

                                        If RouteIsWP Then 'Continue WP route from the last path point
                                            PrevP.Lon = CurPos.Lon
                                            PrevP.Lat = CurPos.Lat
                                            PrevPoint.X = LonToCanvas(CurPos.Lon_Deg)
                                            PrevPoint.Y = LatToCanvas(CurPos.Lat_Deg)
                                            SafeDrawLine(DC, PrevP, P.P, routePen(PenNumber), PrevPoint, P1)
                                        End If

                                        FirstPoint = False
                                    Else
                                        Dim Pe As Pen = routePen(PenNumber)


                                        SafeDrawLine(DC, PrevP, P.P, Pe, PrevPoint, P1)
                                    End If
                                    PrevP.Lon = P.P.Lon
                                    PrevP.Lat = P.P.Lat
                                    PrevPoint = P1
                                    'PrevSide = P.P.Lon < 0
                                End If
                                CurWP += 1
                            Next
                            ShownPoints += 1
                        End If
                        PenNumber += 1
                        PenNumber = PenNumber Mod routePen.Count
                        RouteIsWP = False 'Only the first route holds the waypoints

                        'If ShownPoints Mod 100 = 0 And ShownPoints > 0 Then
                        '    DC.Close()
                        '    _RBmp.Render(D)
                        '    DC = D.RenderOpen
                        'End If
                        'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Exit For
                    Next
                Catch
                End Try
            End If


            DC.Close()
            _RBmp.Render(D)
            'Monitor.Exit(Me)
            'Console.WriteLine("Update path complete in " & Now.Subtract(Start).TotalMilliseconds)
            'End If
        Catch ex As Exception

            Console.WriteLine("UpdatePath exception : " & ex.Message)
        Finally
            System.Threading.Interlocked.Exchange(Invoking, 0)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Drawn"))
        End Try


    End Sub

    Public Sub RedrawCanvas()

        SyncLock _RBmp
            Me.RenderBuffer.Source = _RBmp
        End SyncLock

    End Sub


    Private Sub MouseOverMap(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)

        If Not _RacePolygonsInited Then
            Return
        End If

        Static evtprops As New PropertyChangedEventArgs("CurCoords")
        Dim P As Point = e.GetPosition(CType(sender, IInputElement))

        CurCoords.Lon_Deg = CanvasToLon(P.X)
        CurCoords.Lat_Deg = CanvasToLat(P.Y)
        P.X += 5
        P.Y += 5
        'Debug.WriteLine("Coords : " & CurCoords.ToString)
        RaiseEvent PropertyChanged(Me, evtprops)


    End Sub

    Private Sub DrawGates(ByVal WPs As List(Of VLM_RaceWaypoint))
        Dim D As New DrawingVisual
        Dim DC As DrawingContext = D.RenderOpen()
        Dim WPPen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), 2)
        Dim P0 As Point
        Dim P1 As Point

        Dim WP As VLM_RaceWaypoint

        For Each WP In WPs
            P0.X = LonToCanvas(WP.WPs(0)(0).Lon_Deg)
            P0.Y = LatToCanvas(WP.WPs(0)(0).Lat_Deg)
            P1.X = LonToCanvas(WP.WPs(0)(1).Lon_Deg)
            P1.Y = LatToCanvas(WP.WPs(0)(1).Lat_Deg)

            SafeDrawLine(DC, WP.WPs(0)(0), WP.WPs(0)(1), WPPen, P0, P1)
        Next
        DC.Close()
        _BackDropBmp.Render(D)
    End Sub

    Public Sub ClearBgMap()

        _ClearBgMap = True
        _EraseIsoChrones = True
        _EraseBoatMap = True

    End Sub

    Public Property CenterMapOnAnteMeridien() As Boolean
        Get
            Return _CenterOnAnteMeridien
        End Get
        Set(ByVal value As Boolean)
            _CenterOnAnteMeridien = value
            If value Then
                LonOffset -= 180
            Else
                LonOffset += 180
            End If
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("CenterMapOnAnteMeridien"))
        End Set
    End Property

    Public Property CurCoords() As Coords
        Get
            Return _CurCoords
        End Get
        Set(ByVal value As Coords)

            Static evtprops As New PropertyChangedEventArgs("CurCoords")
            _CurCoords = value
            RaiseEvent PropertyChanged(Me, evtprops)
        End Set
    End Property

    Public Property HideIsochrones() As Boolean
        Get
            Return _HideIsochrones
        End Get
        Set(ByVal value As Boolean)
            _HideIsochrones = value
            _EraseIsoChrones = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HideIsochrones"))
        End Set
    End Property

    Public Property LatOffset() As Double
        Get
            Return _LatOffset
        End Get
        Set(ByVal value As Double)
            _LatOffset = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LatOffset"))
        End Set
    End Property

    Public Property LonOffset() As Double
        Get
            Return _LonOffset
        End Get
        Set(ByVal value As Double)
            _LonOffset = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LonOffset"))
        End Set
    End Property

    Public Property MouseP() As Point
        Get
            Return _P
        End Get
        Set(ByVal value As Point)
            _P = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MouseP"))
        End Set
    End Property

    Public Shared ReadOnly Property RacePolygons() As LinkedList(Of Polygon)
        Get
            If Not _RacePolygonsInited Then
                Return Nothing
            End If
            Return _RacePolygons
        End Get
    End Property

    Public Property Scale() As Double
        Get
            Return _Scale
        End Get
        Set(ByVal value As Double)
            _Scale = value
        End Set
    End Property

    Public ReadOnly Property TileServerBusy() As Boolean
        Get
            Return _TileServer.Busy 
        End Get
    End Property

    Private Sub DrawTile(ByVal ti As TileInfo, ByVal WPs As List(Of VLM_RaceWaypoint))

        Dim D As New DrawingVisual
        Dim DC As DrawingContext = D.RenderOpen
        
        If ti IsNot Nothing Then

            Dim Img As New BitmapImage(New Uri(ti.FileName))
            Dim N As Double = LatToCanvas(ti.North)
            Dim S As Double = LatToCanvas(ti.South)
            Dim W As Double = LonToCanvas(ti.West)
            Dim E As Double = LonToCanvas(ti.East)

            Dim R As New Rect(W, _
                              N, _
                              E - W, _
                              S - N)
            'Dim R As New Rect((ti.TX + 1) * 1024, _
            '                          (-ti.TY) * 1024, _
            '                          1024, _
            '                          1024)

            If _BackDropBmp Is Nothing Then
                _BackDropBmp = New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)

            End If
            DC.DrawImage(Img, R)
            DrawGates(WPs)
            DC.Close()
            _BackDropBmp.Render(D)
            'LocalBmp.Freeze()
        End If

        Interlocked.Increment(_TileCount)
        Interlocked.Decrement(_PendingTileRequestCount)

        _MapPg.Progress(_TileCount)
        If _PendingTileRequestCount = 0 Then
            _MapPg_RequestVisibility(Windows.Visibility.Hidden)
        End If
        'Debug.WriteLine("draw " & ti.TilePath & " pending " & _PendingTileRequestCount)

    End Sub

    Private Sub _TileServer_PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Handles _TileServer.PropertyChanged
        RaiseEvent PropertyChanged(sender, e)
    End Sub

    Private Sub _TileServer_TileProgress(ByVal pct As Double) Handles _TileServer.TileProgress
        _MapPg.Progress(CLng(pct))
    End Sub

    Private Sub _TileServer_TileReady(ByVal ti As TileInfo) Handles _TileServer.TileReady

        SyncLock _ReadyTilesQueue
            _ReadyTilesQueue.Enqueue(ti)
            _MapPg.Progress(_ReadyTilesQueue.Count)
        End SyncLock
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Refresh"))

    End Sub

    Private Sub _MapPg_RequestVisibility(ByVal Vis As System.Windows.Visibility) Handles _MapPg.RequestVisibility
        If Not _Frm.Visibility = Vis Then
            If _Frm.Dispatcher.Thread IsNot System.Threading.Thread.CurrentThread Then
                _Frm.Dispatcher.BeginInvoke(New Action(Of Visibility)(AddressOf _MapPg_RequestVisibility), New Object() {Vis})
            Else
                _Frm.Visibility = Vis
                If Vis = Windows.Visibility.Visible Then
                    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Refresh"))
                End If
            End If
        End If
    End Sub
End Class
