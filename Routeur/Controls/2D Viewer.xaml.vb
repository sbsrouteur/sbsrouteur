'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

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
Imports System.Windows.Threading

Partial Public Class _2D_Viewer

    Implements INotifyPropertyChanged
    Private Const DPI_RES As Integer = 96

#Const DBG_UPDATE_PATH = 0

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _WindArrows(30, 30) As Path
    Private _TrajPath As Path
    Private _RBmp As WriteableBitmap
    Private _BackDropBmp As WriteableBitmap
    Private _BgStarted As Boolean = False
    Private _BgReloadRequest As Boolean = False
    Private _AbortBGDrawing As Boolean = False
    Private _ThBgDraw As System.Threading.Thread = Nothing
    Private _ThreadLastStart As DateTime = Now
    'Private _OpponentsBmp As WriteableBitmap
    Private _ISOBmp As WriteableBitmap
    'Private _gshhs As New GSHHS_Reader
    Private _CurCoords As New Coords
    Private _P As Point
    Private _Progress As MapProgressContext
    Private _DBInit As MapProgressContext
    Private _ClearBgMap As Boolean = False
    Private _ZFactor As Integer

    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(48, 12), New Coords(48, -13), New Coords(55, -13), New Coords(55, -2), _
    '                                                     New Coords(59, 5), New Coords(62, 20), New Coords(48.1, 12)}
    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(58, 10), New Coords(48, 10), New Coords(48, 20), New Coords(60, 20), New Coords(60, 16.5), _
    '                                                     New Coords(56 + 17 / 60, 16 + 28 / 60), New Coords(58, 14) _
    '                                                     }

    Private Shared _RacePolygons As New LinkedList(Of Polygon)()
    Private Shared _RacePolygonsInited As Boolean = False
    Private WithEvents _Frm As frmRoutingProgress
    Private WithEvents _Frm2 As frmRoutingProgress
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

    Private WithEvents _BgDrawerTimer As DispatcherTimer
    Private _WPs As List(Of VLM_RaceWaypoint)

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d'objet sous ce point.

        '_Model = CType(FindResource("2DModel"), _2DViewerModel)
        _TileServer = New TileServer(Me)
        _BgDrawerTimer = New DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        _BgDrawerTimer.Interval = New TimeSpan(0, 0, 0, 0, 100)
        _BgDrawerTimer.Start()
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

        V = V / 180.0 * PI
        V = Log(Tan(V) + 1 / Cos(V))
        V = V / PI * 180.0
        If Double.IsNaN(V) Then
            Dim i As Integer = 0
        End If
        Return ActualHeight / 2.0 - (V - LatOffset) * Scale

    End Function

    Public Function LonToCanvas(ByVal V As Double) As Double

        'V = V Mod 360
        'If CenterMapOnAnteMeridien AndAlso V < 0 Then
        'Return ActualWidth / 2 + (V - LonOffset + 360) * Scale
        'Else
        Return ActualWidth / 2.0 + (V - LonOffset) * Scale
        'End If

    End Function

    Function LoadBitmapFromFile(path As String) As WriteableBitmap
        Dim img As New BitmapImage()
        Dim RetValue As WriteableBitmap = Nothing
        img.BeginInit()
        img.CreateOptions = BitmapCreateOptions.None
        Using rs As New IO.FileStream(path, IO.FileMode.Open)
            Try

                img.StreamSource = rs
                img.EndInit()
                RetValue = BitmapFactory.ConvertToPbgra32Format(img)
            Catch
                rs.Close()
                IO.File.Delete(path)
            End Try
            Return RetValue
        End Using

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
        Dim LocalBmp As New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Pbgra32)
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
                        LocalBmp = New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Pbgra32)
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
                '    LocalBmp = New RenderTargetBitmap(actualwidth, actualheight, DPI_RES, DPI_RES, PixelFormats.Pbgra32)
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
        ZFactor = CInt(Math.Floor(Math.Log(ActualWidth / Width) / Math.Log(2))) + 1

        'Limit zoom to 20 (at least for use with VLM cached tiles)
        If ZFactor > 20 Then
            ZFactor = 20
        ElseIf ZFactor < 1 Then
            ZFactor = 1
        End If

        Dim North As Double = CanvasToLat(0)
        Dim West As Double = CanvasToLon(0)
        _TileCount = 0
        Dim TI As TileInfo

#Const DEBUG_TILE_LEVEL = 0
#If DEBUG_TILE_LEVEL = 0 Then

        _TileServer.Clear()
        For x As Integer = CInt(-TileServer.TILE_SIZE / 2) To CInt(ActualWidth + TileServer.TILE_SIZE / 2) Step CInt(TileServer.TILE_SIZE / 2)
            Dim W As Double = CanvasToLon(x)
            Dim E As Double = CanvasToLon(x + TileServer.TILE_SIZE)
            For y As Integer = CInt(-TileServer.TILE_SIZE / 2) To CInt(ActualHeight + TileServer.TILE_SIZE / 2) Step CInt(TileServer.TILE_SIZE / 2)
                Dim N As Double = CanvasToLat(y)
                Dim S As Double = CanvasToLat(y + TileServer.TILE_SIZE)
                TI = New TileInfo(ZFactor, N, S, E, W)

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
            _DBInit = New MapProgressContext("Loading Database coast line. Do NOT stop application...")
            _Frm2 = New frmRoutingProgress(100, _DBInit)
            _Frm = New frmRoutingProgress(100, _Progress)
            Dim TH As New System.Threading.Thread(AddressOf GSHHS_Reader.Read)
            Dim SI As New GSHHS_StartInfo With {.PolyGons = _RacePolygons, .StartPath = "..\gshhs\gshhs_" & RouteurModel.MapLevel & ".b", _
                                                .ProgressWindows = _DBInit, .CompleteCallBack = AddressOf LoadPolygonComplete, _
                                                .NoExclusionZone = RouteurModel.NoExclusionZone}
            _Frm.Show(owner, _Progress)
            _Frm2.Show(owner, _DBInit)
            TH.Start(SI)
            'GSHHS_Reader.Read(", RacePolygons, "Map")

        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

    Private Sub SafeDrawLine(Bmp As WriteableBitmap, ByVal PrevP As Coords, ByVal P As Coords, ByVal Color As Integer)
        Dim MapSpan As Integer
        For MapSpan = -1 To 1
            Dim PrevPoint2 As New Point(LonToCanvas(PrevP.N_Lon_Deg + 360 * MapSpan), LatToCanvas(PrevP.N_Lat_Deg))
            Dim NewP2 As New Point(LonToCanvas(P.N_Lon_Deg + 360 * MapSpan), LatToCanvas(P.N_Lat_Deg))

            If OutOfCanvas(PrevPoint2) AndAlso outofcanvas(NewP2) Then
                Continue For
            End If

            If (PrevP.N_Lon * P.N_Lon < 0 AndAlso Math.Abs(P.N_Lon - PrevP.N_Lon) >= Math.PI) Then
                Dim Pint As Point
                Dim DLon As Double
                Dim LonSpan As Double
                If PrevP.N_Lon < 0 Then
                    Pint.X = LonToCanvas(-179.99 + 360 * MapSpan)
                    DLon = 180 + PrevP.N_Lon_Deg
                    LonSpan = PrevP.N_Lon_Deg + 360 - P.Lon_Deg
                Else
                    Pint.X = LonToCanvas(179.99 + 360 * MapSpan)
                    DLon = 180 - PrevP.N_Lon_Deg
                    LonSpan = Abs(-180 - P.N_Lon_Deg) + 180 - PrevP.Lon_Deg
                End If

                'Pint.Y = LatToCanvas(PrevP.Lat_Deg + (P.Lat_Deg - PrevP.Lat_Deg) * (PrevP.Lon_Deg + 180) / (360 + PrevP.Lon_Deg - P.Lon_Deg))
                Pint.Y = LatToCanvas(PrevP.N_Lat_Deg + (P.N_Lat_Deg - PrevP.N_Lat_Deg) * DLon / LonSpan)
                Bmp.DrawLine(CInt(Math.Floor(PrevPoint2.X)), CInt(Math.Floor(PrevPoint2.Y)), CInt(Math.Ceiling(Pint.X)), CInt(Math.Ceiling(Pint.Y)), Color)

                If P.N_Lon < 0 Then
                    Pint.X = LonToCanvas(-179.99 + 360 * MapSpan)
                Else
                    Pint.X = LonToCanvas(179.99 + 360 * MapSpan)
                End If

                Bmp.DrawLine(CInt(Pint.X), CInt(Pint.Y), CInt(NewP2.X), CInt(NewP2.Y), Color)
            Else


                Bmp.DrawLine(CInt(PrevPoint2.X), CInt(PrevPoint2.Y), CInt(NewP2.X), CInt(NewP2.Y), Color)

            End If
        Next
    End Sub

    Public Function UpdatePath(PathInfo As PathInfo) As Boolean

        Dim Start As DateTime = Now
        Dim CurPos As New Coords
        Dim Ret As Boolean = True

#If DBG_UPDATE_PATH = 1 Then
        Console.WriteLine("Update path start " & Now.Subtract(Start).ToString)
#End If

        Try

            Dim FirstPoint As Boolean = True
            Dim FirstLine As Boolean = True
            Dim P1 As Point

            Dim PrevPoint As Point
            Static Pen As Integer = &HFF000000
            Static PathPen As Integer = CInt(CLng(PathInfo.TrackColor.R) + (CLng(PathInfo.TrackColor.G) << 8) + (CLng(PathInfo.TrackColor.B) << 16)) Or &HFF000000
            Static IsoBorderPen As Integer = &HFFFFCC00
            Static opponentPenPassUp As Integer = &HFF00CC00
            Static opponentPenPassDown As Integer = &HFFFF0000
            Static opponentPenNeutral As Integer = &HFF0000FF
            Static opponentPenReal As Integer = &HFF7F7F00
            Static BlackBrush As Integer = &HFF000000
            'Static GridBrushes() As Pen
            Static WindBrushes() As Integer

            Static LocalDash As New DashStyle(New Double() {0, 2}, 0)
            Static routePen() As Integer = New Integer() {&HFF0000FF, &HFFFF0000, &HFF00CC00, &HFFFF26E9, &HFFFF26E9}

            Dim PenNumber As Integer = 0
            Dim CrossLine As Boolean = False

            Dim ShownPoints As Integer
            Static LimitDate As New DateTime(3000, 1, 1)
            Dim OpponentMap As Boolean = False
            Dim PrevP As New Coords
            Dim ForceIsoRedraw As Boolean = False

            'GC.Collect()
            _WPs = PathInfo.WPs

            'Dim Pixels As Array
            'If Monitor.TryEnter(Me) Then
            If _RBmp Is Nothing Then
                _RBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, System.Windows.Media.PixelFormats.Pbgra32, Nothing)
            Else
                _RBmp.Clear()
            End If

            Using _RBmp.GetBitmapContext


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

#If DBG_SEGMENTS = 1 Then
            Dim db As New DBWrapper
            db.MapLevel = 4
            Dim C1 As Coords = New Coords(33, 5, 53, Routeur.Coords.NORTH_SOUTH.S,
                                            27, 48, 12, Routeur.Coords.EAST_WEST.E)
            Dim C2 As Coords = New Coords(33, 4, 37, Routeur.Coords.NORTH_SOUTH.S,
                                            27, 48, 12, Routeur.Coords.EAST_WEST.E)

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

                    Dim tc As New TravelCalculator
                    tc.StartPoint = New Coords(PathInfo.Path(PathInfo.Path.Count - 1))
                    Dim StartIsochrone As DateTime = Now
                    Dim PrevLoxo As Double = 0
                    'Try
                    If WindBrushes Is Nothing Then
                        ReDim WindBrushes(70)

                        For i = 0 To 69
                            WindBrushes(i) = WindColors.GetColor(i)

                        Next


                    End If

                    If _ISOBmp Is Nothing Then
                        _ISOBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)

                    End If
                    If PathInfo.ClearGrid Then
                        _ISOBmp.Clear()
                    End If

                    If Not PathInfo.IsoChrones Is Nothing Then
                        Using _ISOBmp.GetBitmapContext
                            Dim IsoIndex As Integer = 0
                            For Each iso As IsoChrone In PathInfo.IsoChrones
                                IsoIndex += 1
                                If Not iso.Drawn Or ForceIsoRedraw Then
                                    Dim MaxIndex As Integer = iso.MaxIndex
                                    Dim index As Integer
                                    'Dim PrevIndex As Integer
                                    Dim CurP As Coords
                                    PrevLoxo = 0
#If DBG_ISO_POINT_SET Then
                                    If PathInfo.DbgIsoNumber = IsoIndex Then
                                        For index = 0 To iso.RawPointSet.Count - 1
                                            SafeDrawEllipse(_ISOBmp, iso.RawPointSet(index).P, &HFF0000FF, 2, 2)
                                        Next
                                    End If
#End If
                                    For index = 0 To MaxIndex
                                        If Not HideIsochronesLines Then
                                            If Not iso.PointSet(index) Is Nothing AndAlso Not iso.PointSet(index).P Is Nothing Then
                                                CurP = iso.PointSet(index).P
                                                P1.X = LonToCanvas(CurP.Lon_Deg)
                                                P1.Y = LatToCanvas(CurP.Lat_Deg)

                                                tc.EndPoint = CurP
                                                Dim NewLoxo As Double = tc.LoxoCourse_Deg
                                                If Abs(PrevLoxo - NewLoxo) > 3 Then
                                                    FirstPoint = True
                                                End If
                                                PrevLoxo = NewLoxo
                                                If Not FirstPoint Then 'And index - PrevIndex < 4 Then
                                                    If iso.PointSet(index).WindStrength <> 0 Then
                                                        SafeDrawLine(_ISOBmp, PrevP, CurP, WindBrushes(CInt(iso.PointSet(index).WindStrength)))
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
                                            If Not iso.PointSet(index) Is Nothing AndAlso Not iso.PointSet(index).P Is Nothing Then
                                                CurP = iso.PointSet(index).P
                                                SafeDrawEllipse(_ISOBmp, CurP, WindBrushes(CInt(iso.PointSet(index).WindStrength)), 2, 2)
                                            End If
                                        End If
                                    Next
                                    FirstPoint = True
                                    iso.Drawn = True
                                End If

                                If Now.Subtract(Start).TotalMilliseconds > 100 Then
                                    Ret = False
                                    Exit For
                                End If
                            Next
                        End Using
                    End If
                    tc.EndPoint = Nothing
                    tc.StartPoint = Nothing
                    tc = Nothing
                    'Catch ex As Exception
                    'Finally
                    'End Try
                ElseIf _EraseIsoChrones Then
                    _ISOBmp.Clear()
                    _EraseIsoChrones = False
                    If PathInfo.IsoChrones IsNot Nothing Then
                        For Each iso In PathInfo.IsoChrones
                            iso.Drawn = False
                        Next
                    End If
                End If

#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path IsoChrones Done " & Now.Subtract(Start).ToString)
#End If


                If Not PathInfo.Opponents Is Nothing Then
                    SyncLock PathInfo.Opponents
                        For Each op In PathInfo.Opponents
                            If Not op.Value.Drawn Then
                                'P1.X = LonToCanvas(op.Value.CurPos.Lon_Deg)
                                'P1.Y = LatToCanvas(op.Value.CurPos.Lat_Deg)

                                Dim DrawPen As Integer
                                If op.Value.PassUp Then
                                    DrawPen = opponentPenPassUp
                                ElseIf op.Value.PassDown Then
                                    DrawPen = opponentPenPassDown
                                ElseIf op.Value.Real Then
                                    DrawPen = opponentPenReal
                                Else

                                    DrawPen = opponentPenNeutral
                                End If
                                Dim OpSize As Integer = If(op.Value.MyTeam, 6, 4)
                                If op.Value.Real Then
                                    OpSize = 6
                                End If
                                SafeDrawEllipse(_RBmp, op.Value.CurPos, DrawPen, OpSize, OpSize)

                                'op.Value.Drawn = True
                                'OpponentMap = True
                            End If
                        Next
                    End SyncLock
                    'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return
                End If
                'End Using
#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path Opponents Done " & Now.Subtract(Start).ToString)
#End If


#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path BM Rendered " & Now.Subtract(Start).ToString)
#End If


                '
                ' Draw the recorded path
                '
                DrawPath(_RBmp, PathInfo.Path, PathPen)
                DrawPath(_RBmp, PathInfo.RoutingBorder, IsoBorderPen)

                'If Now.Subtract(Start).TotalMilliseconds > MAX_DRAW_MS Then Return
#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path Path Done " & Now.Subtract(Start).ToString)
#End If
                '
                ' Draw the other routes
                '
                If Not OpponentMap And Not PathInfo.Routes Is Nothing Then
                    'Try
                    ShownPoints += 1
                    PenNumber = 0
                    Dim CurWP As Integer = 1
                    Dim RouteIndex As Integer = 0
                    For Each R In PathInfo.Routes

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
                                        SafeDrawLine(_RBmp, PrevP, P.P, routePen(PenNumber))

                                        FirstPoint = False
                                    Else
                                        Dim Pe As Integer = routePen(PenNumber)
                                        SafeDrawLine(_RBmp, PrevP, P.P, Pe)
                                    End If
                                    If RouteIndex = PathInfo.EstimateRouteIndex Then
                                        SafeDrawEllipse(_RBmp, P.P, routePen(PenNumber), 2, 2)
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

                    Next
                    'Catch
                    'End Try
                End If

                ' Draw pilototo route change points
                If Not PathInfo.PilototoPoints Is Nothing Then
                    'Try
                    PenNumber = 0
                    For Each P In PathInfo.PilototoPoints

                        If Not P Is Nothing Then

                            SafeDrawEllipse(_RBmp, P, routePen(PenNumber), 3, 3)

                        End If

                    Next

                End If

#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path Routes Done " & Now.Subtract(Start).ToString)
#End If

#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path DCClosed Done " & Now.Subtract(Start).ToString)
#End If

#If DBG_UPDATE_PATH = 1 Then
                Console.WriteLine("Update path Render Done " & Now.Subtract(Start).ToString)
#End If

            End Using
        Catch ex As Exception

            Console.WriteLine("UpdatePath exception : " & ex.Message)
        Finally
            Console.WriteLine("Update path drawn in " & Now.Subtract(Start).ToString)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Drawn"))
            Stats.SetStatValue(Stats.StatID.DRAW_FPS) = 1 / Now.Subtract(Start).TotalSeconds
        End Try
        Return Ret

    End Function



    Public Sub RedrawCanvas()


    End Sub


    Private Sub MouseOverMap(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)

        If Not _RacePolygonsInited Then
            Return
        End If

        Static evtprops As New PropertyChangedEventArgs("CurCoords")
        Dim P As Point = e.GetPosition(CType(sender, IInputElement))

        CurCoords.Lon_Deg = CanvasToLon(P.X)
        CurCoords.Lat_Deg = CanvasToLat(P.Y)
        'P.X += 5
        'P.Y += 5
        'Debug.WriteLine("Coords : " & CurCoords.ToString)
        RaiseEvent PropertyChanged(Me, evtprops)


    End Sub

    Private Sub DragBitmap(Bmp As WriteableBitmap, Dx As Integer, Dy As Integer)

        Dim X As Integer
        Dim Y As Integer
        Dim DestX As Integer
        Dim DestY As Integer
        Dim W As Integer = CInt(Bmp.Width - Abs(Dx))
        Dim H As Integer = CInt(Bmp.Height - Abs(Dy))

        If Dx < 0 Then
            X = 0
            DestX = Dx
        Else
            X = -Dx
            DestX = 0
        End If
        If Dy > 0 Then
            Y = 0
            DestY = Dy
        Else
            Y = -Dy
            DestY = 0
        End If
        Dim SrcRect As New Rect(X, Y, W, H)
        Dim DstRect As New Rect(DestX, DestY, Bmp.Width - Abs(Dx), Bmp.Height - Abs(Dy))
        Dim FullRect As New Rect(0, 0, Bmp.Width, Bmp.Height)
        Dim TmpBmp As New WriteableBitmap(Bmp)
        Using TmpBmp.GetBitmapContext
            TmpBmp.Clear()
            Using Bmp.GetBitmapContext
                TmpBmp.Blit(DstRect, Bmp, SrcRect)
                Bmp.Clear()
                Bmp.Blit(FullRect, TmpBmp, FullRect)
            End Using
        End Using
    End Sub

    Sub DragMap(LastDraggedPoint As Point, P As Point)

        Dim Dx As Integer = CInt(P.X - LastDraggedPoint.X)
        Dim Dy As Integer = CInt(P.Y - LastDraggedPoint.Y)

        DragBitmap(_RBmp, Dx, Dy)
        DragBitmap(_BackDropBmp, Dx, Dy)
        DragBitmap(_ISOBmp, Dx, Dy)

    End Sub

    Private Sub DrawGates(ByVal WPs As List(Of VLM_RaceWaypoint))
        Dim FutureGates As Integer = &HFF0000FF
        Dim CurrentGate As Integer = &HFFFF0000
        Dim ValidatedGates As Integer = &HFF00FF00
        Dim WPPen As Integer

        Dim WP As VLM_RaceWaypoint
        Dim WPIndex As Integer = 1
        If WPs IsNot Nothing Then
            For Each WP In WPs

                If WPIndex < RouteurModel.CurWP Then
                    WPPen = ValidatedGates
                ElseIf WPIndex = RouteurModel.CurWP Then
                    WPPen = CurrentGate
                Else
                    WPPen = FutureGates
                End If
                SafeDrawLine(_BackDropBmp, WP.WPs(0)(0), WP.WPs(0)(1), WPPen)
                WPIndex += 1

            Next
        End If

    End Sub

    Public Sub ClearBgMap()

        _ClearBgMap = True
        _EraseIsoChrones = True
        _EraseBoatMap = True

    End Sub

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
            If Double.IsNaN(value) Then
                _LatOffset = 0
            Else
                _LatOffset = value
            End If
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

    Public Property TrackColor As Color

    Public ReadOnly Property TileServerBusy() As Boolean
        Get
            Return _TileServer.Busy
        End Get
    End Property

    Private Sub DrawTile(ByVal ti As TileInfo, ByVal WPs As List(Of VLM_RaceWaypoint))

        'Dim D As New DrawingVisual
        'Dim DC As DrawingContext = D.RenderOpen

        If ti IsNot Nothing Then

            Dim Img As WriteableBitmap = LoadBitmapFromFile(ti.FileName)

            If Img Is Nothing Then
#If TESTING <> 1 Then
                _TileServer.RequestTile(ti)
#End If

                Return
            End If
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


            Using _BackDropBmp.GetBitmapContext
                _BackDropBmp.Blit(R, Img, New Rect(0, 0, TileServer.TILE_SIZE, TileServer.TILE_SIZE))
                DrawGates(WPs)

            End Using
            'LocalBmp.Freeze()
        End If

        Interlocked.Increment(_TileCount)
        Interlocked.Decrement(_PendingTileRequestCount)

        _MapPg.Progress(_TileCount)
        If _PendingTileRequestCount = 0 Then
            _MapPg_RequestVisibility(Windows.Visibility.Hidden)
            'BackDropBuffer.Source = _BackDropBmp
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
        '_OpponentsBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)
        _RBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)
        _BackDropBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)
        _ISOBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)

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
        _BgReloadRequest = True

        Me.RenderBuffer.Source = _RBmp
        Me.BackDropBuffer.Source = _BackDropBmp
        Me.IsoBuffer.Source = _ISOBmp
    End Sub

    Private Sub SafeDrawEllipse(Bmp As WriteableBitmap, CurP As Coords, Color As Integer, XAxis As Integer, YAxis As Integer)
        Dim P1_X As Integer
        Dim P1_Y As Integer = CInt(LatToCanvas(CurP.N_Lat_Deg) - (YAxis / 2))
        Dim P2_X As Integer
        Dim P2_Y As Integer = CInt(LatToCanvas(CurP.N_Lat_Deg) + (YAxis / 2))

        If P1_Y = P2_Y Then
            P2_Y += 1
        End If

        For mapspan As Integer = -1 To 1
            Try
                P1_X = CInt(LonToCanvas(CurP.Lon_Deg + 360 * mapspan) - (XAxis / 2))
                P2_X = CInt(LonToCanvas(CurP.Lon_Deg + 360 * mapspan) + (XAxis / 2))
                If P1_X = P2_X Then
                    P2_X += 1
                End If
                If Not OutOfCanvas(New Point(P1_X, P1_Y)) AndAlso Not OutOfCanvas(New Point(P2_X, P2_Y)) Then
                    Bmp.DrawEllipse(P1_X, P1_Y, P2_X, P2_Y, Color)
                End If
            Catch ex As OverflowException
                'Ignore overflow exceptions (overzooming causes them)
            End Try
        Next

    End Sub

    Private Sub BgDrawer_Tick(sender As Object, e As System.EventArgs) Handles _BgDrawerTimer.Tick
        If _Frm Is Nothing Then
            Return
        End If
        If _BackDropBmp Is Nothing Then
            _BackDropBmp = New WriteableBitmap(CInt(ActualWidth), CInt(ActualHeight), DPI_RES, DPI_RES, PixelFormats.Pbgra32, Nothing)
        End If
        If _ClearBgMap Then
            _BgStarted = False
            _ClearBgMap = False

            _BackDropBmp.Clear()

            OnMapSizeChanged(Nothing, Nothing)
        End If

        'If Not _BgStarted AndAlso _BackDropBmp Is Nothing Then
        'If _ReadyTilesQueue.Count = 0 AndAlso _ThBgDraw Is Nothing AndAlso Now.Subtract(_ThreadLastStart).TotalMilliseconds > 200 Then
        If Not _BgStarted AndAlso _BgReloadRequest Then
            _BgStarted = True 'BgBackDropDrawing(0)
            _Frm.DataContext = _MapPg
            _ThBgDraw = New Thread(AddressOf BgBackDropDrawing)
            _ThreadLastStart = Now
            _ThBgDraw.Start(_WPs)
            'System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgBackDropDrawing, WPs)

#If NO_TILES = 0 Then
        ElseIf _ReadyTilesQueue.Count <> 0 Then
            SyncLock _ReadyTilesQueue
                Dim loopcount As Double = 0
                While _ReadyTilesQueue.Count > 0 ' AndAlso loopcount < 5
                    Dim ti As TileInfo = _ReadyTilesQueue.Dequeue
                    DrawTile(ti, _WPs)
                    loopcount += 1
                End While
            End SyncLock

#End If
        End If

    End Sub

    Public Property ZFactor As Integer
        Get
            Return _ZFactor
        End Get
        Set(value As Integer)
            If value <> _ZFactor Then
                _ZFactor = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ZFactor"))
            End If
        End Set
    End Property

    Private Function OutOfCanvas(Point As Point) As Boolean

        Return (Point.X < 0 OrElse Point.X > ActualWidth) OrElse
            (Point.Y < 0 OrElse Point.Y > ActualHeight)

    End Function

    Private Sub DrawPath(RBmp As WriteableBitmap, Path As LinkedList(Of Coords), PenColor As Integer)

        Dim P1 As Point
        Dim PrevPoint As Point

        If Path IsNot Nothing Then
            Dim Pnt As New Coords
            Dim CurPos As New Coords
            Dim PrevP As Coords = Nothing

            For Each C As Coords In Path
                If C IsNot Nothing Then

                    P1.X = LonToCanvas(C.Lon_Deg)
                    P1.Y = LatToCanvas(C.Lat_Deg)

                    CurPos.Lon_Deg = C.Lon_Deg
                    CurPos.Lat_Deg = C.Lat_Deg

                    If PrevP IsNot Nothing Then

                        Pnt.Lon_Deg = C.Lon_Deg
                        Pnt.Lat_Deg = C.Lat_Deg

                        SafeDrawLine(_RBmp, PrevP, Pnt, PenColor)
                    Else
                        PrevP = New Coords
                    End If
                    PrevP.Lon_Deg = C.Lon_Deg
                    PrevP.Lat_Deg = C.Lat_Deg
                    PrevPoint = P1
                    
                End If

            Next
        End If
    End Sub



End Class
