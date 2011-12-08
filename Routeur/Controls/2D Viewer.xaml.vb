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
    
#Const DBG_UPDATE_PATH = 0

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _WindArrows(30, 30) As Path
    Private _TrajPath As Path
    Private _RBmp As RenderTargetBitmap
    Private _BackDropBmp As RenderTargetBitmap
    Private _BgStarted As Boolean = False
    Private _AbortBGDrawing As Boolean = False
    Private _ThBgDraw As System.Threading.Thread = Nothing
    Private _ThreadLastStart As DateTime = Now
    Private _OpponentsBmp As RenderTargetBitmap
    Private _ISOBmp As RenderTargetBitmap
    Private _RoutesBmp As RenderTargetBitmap
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
    Private _HideIsochroneLines As Boolean = False
    Private _HideIsochroneDots As Boolean = False
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
        Debug.Assert(Scale <> 0)
        Dim Ret As Double = (ActualHeight / 2 - C) / Scale + LatOffset '90 / RouteurModel.SCALE - C / DEFINITION / RouteurModel.SCALE + RouteurModel.LAT_OFFSET
        Ret = Ret / 180 * PI
        Return (Math.Atan(Math.Sinh(Ret)) / PI * 180)
    End Function

    Public Function CanvasToLon(ByVal V As Double) As Double
        Debug.Assert(Scale <> 0)
        Dim Ret As Double = ((V - ActualWidth / 2) / Scale + LonOffset) '(V - 180 * DEFINITION) / DEFINITION / RouteurModel.SCALE
        'If Ret > 180 Then
        '    While Ret > 180
        '        Ret -= 180
        '    End While
        'ElseIf Ret < -180 Then
        '    While Ret < -180
        '        Ret += 180
        '    End While
        'End If
        Return Ret
    End Function


    Public Function LatToCanvas(ByVal V As Double) As Double

        V = V / 180 * PI
        V = Log(Tan(V) + 1 / Cos(V))
        V = V / PI * 180
        Return ActualHeight / 2 - (V - LatOffset) * Scale

    End Function

    Public Function LonToCanvas(ByVal V As Double) As Double

        V = V Mod 360
        If CenterMapOnAnteMeridien AndAlso V < 0 Then
            Return ActualWidth / 2 + (V - LonOffset + 360) * Scale
        Else
            Return ActualWidth / 2 + (V - LonOffset) * Scale
        End If

    End Function


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
        Dim LocalBmp As New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Default)
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
        Static R As New Rect(0, 0, actualwidth, actualheight)

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
                        LocalBmp = New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Default)
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
                '    LocalBmp = New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Default)
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

#If NO_TILE = 1 Then
        Return
#End If


        'Scale = 360 / Math.Abs(C1.Lon_Deg - C2.Lon_Deg)
        Dim Width As Double = CanvasToLon(ActualWidth) - CanvasToLon(0)
        Dim Z As Integer = CInt(Math.Floor(Math.Log(ActualWidth / Width) / Math.Log(2))) + 1

        'Limit zoom to 20 (at least for use with VLM cached tiles)
        If Z > 20 Then
            Z = 20
        End If

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
        W = CanvasToLon(ActualWidth - TileServer.TILE_SIZE)
        N = CanvasToLat(ActualHeight - TileServer.TILE_SIZE)
        E = CanvasToLon(ActualWidth + TileServer.TILE_SIZE)
        S = CanvasToLat(ActualHeight)
        TI = New TileInfo(Z, N, S, E, W)
        tx2 = TI.TX
        ty2 = TI.TY
        _MapPg.Start((tx2 - tx1) * (ty2 - ty1))
        For i As Integer = tx1 To tx2
            For j As Integer = ty1 To ty2
                TI = New TileInfo(Z, i, j)

                System.Threading.Interlocked.Increment(_PendingTileRequestCount)
                _TileServer.RequestTile(TI)
                If _AbortBGDrawing Then
                    _AbortBGDrawing = False
                    Return
                End If
            Next
        Next
        _MapPg.Start(_PendingTileRequestCount)

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
        _ThBgDraw = Nothing
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
                                                 ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint), ByVal ManagedRoutes As IList(Of RecordedRoute))


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
    Public Sub UpdatePath(ByVal PathString As String, ByVal Routes As IList(Of ObservableCollection(Of VLM_Router.clsrouteinfopoints)), EstimateRouteIndex As Integer, ByVal Opponents As Dictionary(Of String, BoatInfo), _
                          ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint), ByVal ManagedRoutes As IList(Of RecordedRoute))

        Dim Start As DateTime = Now
        Dim CurPos As New Coords

#If DBG_UPDATE_PATH = 1 Then
        Console.WriteLine("Update path start " & Now.Subtract(Start).ToString)
#End If

        Try

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
            Static opponentPenPassUp As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Green), RouteurModel.PenWidth)
            Static opponentPenPassDown As New Pen(New SolidColorBrush(System.Windows.Media.Colors.DarkRed), RouteurModel.PenWidth)
            Static opponentPenNeutral As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Blue), RouteurModel.PenWidth)
            Static BlackBrush As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Black), RouteurModel.PenWidth / 2)
            'Static GridBrushes() As Pen
            Static WindBrushes() As Pen

            Static LocalDash As New DashStyle(New Double() {0, 2}, 0)
            Static routePen() As Pen = New Pen() {New Pen(New SolidColorBrush(System.Windows.Media.Colors.Blue), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Green), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Purple), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round}, _
                                                  New Pen(New SolidColorBrush(System.Windows.Media.Colors.Purple), RouteurModel.PenWidth) With {.LineJoin = PenLineJoin.Round} _
                                               }


            Static Frozen As Boolean = False

            If Not Frozen Then
                Frozen = True
                Pen.Freeze()
                PathPen.Freeze()
                opponentPenNeutral.Freeze()
                opponentPenPassDown.Freeze()
                opponentPenPassUp.Freeze()
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
            'Dim b As Byte
            Dim ShownPoints As Integer
            Static LimitDate As New DateTime(3000, 1, 1)
            Dim OpponentMap As Boolean = False
            Dim PrevP As New Coords
            Dim ForceIsoRedraw As Boolean = False

            'GC.Collect()

            'Dim Pixels As Array
            'If Monitor.TryEnter(Me) Then
            _RBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)

            If _ClearBgMap Then
                _BgStarted = False
                _ClearBgMap = False
                _BackDropBmp = Nothing
                OnMapSizeChanged(Nothing, Nothing)
            End If

            'If Not _BgStarted AndAlso _BackDropBmp Is Nothing Then
            If _ReadyTilesQueue.Count = 0 AndAlso _ThBgDraw Is Nothing AndAlso _BackDropBmp Is Nothing AndAlso Now.Subtract(_ThreadLastStart).TotalMilliseconds > 200 Then
                _BgStarted = True 'BgBackDropDrawing(0)
                ForceIsoRedraw = True
                _Frm.DataContext = _MapPg
                _ThBgDraw = New Thread(AddressOf BgBackDropDrawing)
                _ThreadLastStart = Now
                _ThBgDraw.Start(WPs)
                'System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgBackDropDrawing, WPs)

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
#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Tiles Done " & Now.Subtract(Start).ToString)
#End If

            '#If NO_TILES = 1 Then
            '            If _BackDropBmp IsNot Nothing AndAlso _BackDropBmp.IsFrozen Then


            '                DC.DrawImage(_BackDropBmp, New Rect(0, 0, actualwidth, actualheight))
            '            End If
            '#Else
            '            DC.DrawImage(_BackDropBmp, New Rect(0, 0, ActualWidth, ActualHeight))
            '#End If

#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Tiles Rendered " & Now.Subtract(Start).ToString)
#End If



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
            'If False AndAlso Not ManagedRoutes Is Nothing Then
            '    _RoutesBmp.Clear()
            '    'Dim StartRoutes As DateTime = Now
            '    For Each route In ManagedRoutes

            '        If route.Visible Then
            '            Dim Pe As New Pen(route.ColorBrush, 1)
            '            For i As Integer = 1 To route.Route.Count - 1

            '                Dim PrevPt As Point
            '                Dim CurPt As Point
            '                PrevPt.X = LonToCanvas(route.Route(i - 1).P.Lon_Deg)
            '                PrevPt.Y = LatToCanvas(route.Route(i - 1).P.Lat_Deg)
            '                CurPt.X = LonToCanvas(route.Route(i).P.Lon_Deg)
            '                CurPt.Y = LatToCanvas(route.Route(i).P.Lat_Deg)

            '                SafeDrawLine(DC, route.Route(i - 1).P, route.Route(i).P, Pen, PrevPt, CurPt)

            '            Next
            '        End If
            '    Next
            '    DC.Close()
            '    _RoutesBmp.Render(D)
            '    DC = D.RenderOpen
            'End If

#If DBG_SEGMENTS = 1 Then
            Dim db As New DBWrapper
            db.MapLevel = 3
            Dim C1 As Coords = New Coords(33, 43, 52, Routeur.Coords.NORTH_SOUTH.S,
                                            18, 26, 31, Routeur.Coords.EAST_WEST.E)
            Dim C2 As Coords = New Coords(34, 49, 57, Routeur.Coords.NORTH_SOUTH.S,
                                            18, 28, 58, Routeur.Coords.EAST_WEST.E)
            
            'Dim C1 As New Coords(CanvasToLat(0), CanvasToLon(0))
            'Dim C2 As New Coords(CanvasToLat(ActualHeight), CanvasToLon(ActualWidth))
            Dim PenSegs As New Pen(New SolidColorBrush(Color.FromRgb(255, 128, 0)), 1)
            'Dim Segs = db.SegmentList(C1.Lon_Deg, C1.Lat_Deg, C2.Lon_Deg, C2.Lat_Deg)
            Dim segs = GSHHS_Reader._Tree.GetSegments(C1, C2, db)
            Dim SegCount As Integer = 0
            segs.Add(New MapSegment() With {.Lon1 = C1.Lon_Deg, .Lat1 = C1.Lat_Deg, .Lon2 = C2.Lon_Deg, .Lat2 = C2.Lat_Deg})
            For Each seg In segs
                Dim lP1 As New Coords(seg.Lat1, seg.Lon1)
                Dim lP2 As New Coords(seg.Lat2, seg.Lon2)
                Dim pp1 As New Point(LonToCanvas(seg.Lon1), LatToCanvas(seg.Lat1))
                Dim pp2 As New Point(LonToCanvas(seg.Lon2), LatToCanvas(seg.Lat2))

                SafeDrawLine(DC, lP1, lP2, PenSegs, pp1, pp2)
                SegCount += 1
                If SegCount > 150 Then
                    Exit For
                End If
            Next
#End If


            '
            ' Draw IsoChrones
            '
            If Not _EraseIsoChrones Then
                Dim StartIsochrone As DateTime = Now
                'Try
                If WindBrushes Is Nothing Then
                    ReDim WindBrushes(70)

                    For i = 0 To 69
                        WindBrushes(i) = New Pen(New SolidColorBrush(WindColors.GetColor(i)), 0.5)
                        WindBrushes(i).Freeze()
                    Next


                End If

                If ClearGrid OrElse _ISOBmp Is Nothing Then
                    _ISOBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)

                End If


                If Not IsoChrones Is Nothing Then
                    For Each iso As IsoChrone In IsoChrones
                        If Not iso.Drawn Or ForceIsoRedraw Then
                            Dim MaxIndex As Integer = iso.MaxIndex
                            Dim index As Integer
                            'Dim PrevIndex As Integer
                            Dim CurP As Coords
                            For index = 0 To MaxIndex
                                If Not HideIsochronesLines Then
                                    If Not iso.Data(index) Is Nothing AndAlso Not iso.Data(index).P Is Nothing Then
                                        CurP = iso.Data(index).P
                                        P1.X = LonToCanvas(CurP.Lon_Deg)
                                        P1.Y = LatToCanvas(CurP.Lat_Deg)

                                        If Not FirstPoint Then 'And index - PrevIndex < 4 Then
                                            If iso.Data(index).WindStrength <> 0 Then
                                                SafeDrawLine(DC, PrevP, CurP, WindBrushes(CInt(iso.Data(index).WindStrength)), PrevPoint, P1)
                                            End If
                                        End If
                                        'PrevIndex = index
                                        PrevP.Lon = CurP.Lon
                                        PrevP.Lat = CurP.Lat
                                        PrevPoint = P1
                                        FirstPoint = False
                                    Else
                                        FirstPoint = True
                                    End If
                                End If

                                If Not HideIsochronesDots Then
                                    If Not iso.Data(index) Is Nothing AndAlso Not iso.Data(index).P Is Nothing Then
                                        CurP = iso.Data(index).P
                                        P1.X = LonToCanvas(CurP.Lon_Deg)
                                        P1.Y = LatToCanvas(CurP.Lat_Deg)
                                        DC.DrawEllipse(Nothing, WindBrushes(CInt(iso.Data(index).WindStrength)), P1, 1, 1)
                                    End If
                                End If
                            Next
                            FirstPoint = True
                            iso.Drawn = True
                        End If

                        'If Now.Subtract(Start).TotalMilliseconds > 100 Then
                        'Exit For
                        'End If
                    Next
                End If

                'Catch ex As Exception
                'Finally
                DC.Close()
                _ISOBmp.Render(D)
                DC = D.RenderOpen
                'End Try
            ElseIf _EraseIsoChrones Then
                _ISOBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)
                _EraseIsoChrones = False
                If IsoChrones IsNot Nothing Then
                    For Each iso In IsoChrones
                        iso.Drawn = False
                    Next
                End If
            End If

#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path IsoChrones Done " & Now.Subtract(Start).ToString)
#End If


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

                            Dim DrawPen As Pen
                            If op.Value.PassUp Then
                                DrawPen = opponentPenPassUp
                            ElseIf op.Value.PassDown Then
                                DrawPen = opponentPenPassDown
                            Else
                                DrawPen = opponentPenNeutral
                            End If
                            DC.DrawEllipse(Nothing, DrawPen, P1, 1, 1)

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

#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Opponents Done " & Now.Subtract(Start).ToString)
#End If

            DC.DrawImage(_RoutesBmp, New Rect(0, 0, ActualWidth, ActualHeight))
            DC.Close()
            _RBmp.Render(D)
            DC = D.RenderOpen
            DC.DrawImage(_OpponentsBmp, New Rect(0, 0, ActualWidth, ActualHeight))
            DC.Close()
            _RBmp.Render(D)
            DC = D.RenderOpen
            DC.DrawImage(_ISOBmp, New Rect(0, 0, ActualWidth, ActualHeight))
            DC.Close()
            _RBmp.Render(D)
            DC = D.RenderOpen
#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path BM Rendered " & Now.Subtract(Start).ToString)
#End If


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
                    If C <> "" AndAlso C <> "0!0" Then
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
#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Path Done " & Now.Subtract(Start).ToString)
#End If
            DC.Close()
            _RBmp.Render(D)
            DC = D.RenderOpen
            '
            ' Draw the other routes
            '
            If Not OpponentMap And Not Routes Is Nothing Then
                'Try
                ShownPoints += 1
                PenNumber = 0
                Dim CurWP As Integer = 1
                Dim RouteIndex As Integer = 0
                For Each R In Routes

                    If Not R Is Nothing Then
                        FirstPoint = True
                        For Each P In R

                            If Not P Is Nothing AndAlso Not P.P Is Nothing Then
                                P1.X = LonToCanvas(P.P.Lon_Deg)
                                P1.Y = LatToCanvas(P.P.Lat_Deg)
                                If FirstPoint Then
                                    PrevP.Lon = CurPos.Lon
                                    PrevP.Lat = CurPos.Lat
                                    PrevPoint.X = LonToCanvas(CurPos.Lon_Deg)
                                    PrevPoint.Y = LatToCanvas(CurPos.Lat_Deg)
                                    SafeDrawLine(DC, PrevP, P.P, routePen(PenNumber), PrevPoint, P1)

                                    FirstPoint = False
                                Else
                                    Dim Pe As Pen = routePen(PenNumber)
                                    SafeDrawLine(DC, PrevP, P.P, Pe, PrevPoint, P1)
                                End If
                                If RouteIndex = EstimateRouteIndex Then
                                    DC.DrawEllipse(Nothing, routePen(PenNumber), PrevPoint, 1, 1)
                                End If
                                PrevP.Lon = P.P.Lon
                                PrevP.Lat = P.P.Lat
                                PrevPoint = P1

                            End If
                            CurWP += 1
                        Next
                        ShownPoints += 1
                    End If
                    RouteIndex += 1
                    PenNumber += 1
                    PenNumber = PenNumber Mod routePen.Count

                    'If ShownPoints Mod 100 = 0 And ShownPoints > 0 Then
                    '    DC.Close()
                    '    _RBmp.Render(D)
                    '    DC = D.RenderOpen
                    'End If
                    'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Exit For
                Next
                'Catch
                'End Try
            End If

#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Routes Done " & Now.Subtract(Start).ToString)
#End If
            DC.Close()
#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path DCClosed Done " & Now.Subtract(Start).ToString)
#End If
            _RBmp.Render(D)
#If DBG_UPDATE_PATH = 1 Then
            Console.WriteLine("Update path Render Done " & Now.Subtract(Start).ToString)
#End If
            'Monitor.Exit(Me)
            'Console.WriteLine("Update path complete in " & Now.Subtract(Start).TotalMilliseconds)
            'End If
        Catch ex As Exception

            Console.WriteLine("UpdatePath exception : " & ex.Message)
        Finally
            Console.WriteLine("Update path drawn in " & Now.Subtract(Start).ToString)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Drawn"))
            Stats.SetStatValue(Stats.StatID.DRAW_FPS) = 1 / Now.Subtract(Start).TotalSeconds
        End Try


    End Sub

    Public Sub RedrawCanvas()

        Me.RenderBuffer.Source = _RBmp

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
        Dim FutureGates As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Blue), 0.5)
        Dim CurrentGate As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Red), 1)
        Dim ValidatedGates As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Green), 0.5)
        Dim WPPen As Pen
        Dim P0 As Point
        Dim P1 As Point

        Dim WP As VLM_RaceWaypoint
        Dim WPIndex As Integer = 1
        For Each WP In WPs
            P0.X = LonToCanvas(WP.WPs(0)(0).Lon_Deg)
            P0.Y = LatToCanvas(WP.WPs(0)(0).Lat_Deg)
            P1.X = LonToCanvas(WP.WPs(0)(1).Lon_Deg)
            P1.Y = LatToCanvas(WP.WPs(0)(1).Lat_Deg)
            If WPIndex < RouteurModel.CurWP Then
                WPPen = ValidatedGates
            ElseIf WPIndex = RouteurModel.CurWP Then
                WPPen = CurrentGate
            Else
                WPPen = FutureGates
            End If
            SafeDrawLine(DC, WP.WPs(0)(0), WP.WPs(0)(1), WPPen, P0, P1)
            WPIndex += 1
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

    Public Property HideIsochronesDots() As Boolean
        Get
            Return _HideIsochroneDots
        End Get
        Set(ByVal value As Boolean)
            _HideIsochroneDots = value
            _EraseIsoChrones = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HideIsochronesDots"))
        End Set
    End Property


    Public Property HideIsochronesLines() As Boolean
        Get
            Return _HideIsochroneLines
        End Get
        Set(ByVal value As Boolean)
            _HideIsochroneLines = value
            _EraseIsoChrones = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HideIsochronesLines"))
        End Set
    End Property

    Public Property LatOffset() As Double
        Get
            Return _LatOffset
        End Get
        Set(ByVal value As Double)
            Debug.Assert(Not Double.IsNaN(value))
            _LatOffset = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("LatOffset"))
        End Set
    End Property

    Public Property LonOffset() As Double
        Get
            Return _LonOffset
        End Get
        Set(ByVal value As Double)
            Debug.Assert(Not Double.IsNaN(value))
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
            Debug.Assert(value <> 0)
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
                _BackDropBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)

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
            BackDropBuffer.Source = _BackDropBmp
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


    Private Sub OnMapSizeChanged(sender As System.Object, e As System.Windows.SizeChangedEventArgs)
        _OpponentsBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)
        _RoutesBmp = New RenderTargetBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Default)
        '_RBmp = Nothing
        _BackDropBmp = Nothing
        _ISOBmp = Nothing

        _AbortBGDrawing = True
        If _ThBgDraw IsNot Nothing Then
            If Not _ThBgDraw.Join(2000) Then
                _ThBgDraw.Abort()
                _ThBgDraw = Nothing
            End If
        End If
        _ThBgDraw = Nothing
        _AbortBGDrawing = False
        _TileServer.Clear()
        _BgStarted = False

    End Sub
End Class
