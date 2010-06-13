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
    Private _Frm As frmRoutingProgress
    Private _MapPg As New MapProgressContext("Drawing Map...")

    Private _Scale As Double
    Private _LonOffset As Double
    Private _LatOffset As Double


    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d'objet sous ce point.

        '_Model = CType(FindResource("2DModel"), _2DViewerModel)

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
        Return 180 * DEFINITION + (V - LonOffset) * Scale

    End Function

    Private Sub RenDerBackDrop(ByVal D As DrawingVisual)

    End Sub

    Private Sub LoadPolygonComplete()

        _RacePolygonsInited = True

    End Sub

    Private Function BgBackDropDrawing(ByVal state As Object) As Boolean

        Dim D As New DrawingVisual
        Dim DC As DrawingContext = D.RenderOpen()
        Dim FirstPoint As Boolean
        Dim p0 As Point
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

            Next
        Next

        _MapPg.Start(GSHHS_Reader.AllPolygons.Count)
        For Each C_Array In GSHHS_Reader.AllPolygons
            polyindex += 1


            FirstPoint = True
            For i = 1 To C_Array.Count - 1


                If C_Array(i) IsNot Nothing AndAlso C_Array(i - 1) IsNot Nothing Then
                    'If GSHHS_Reader.HitTest(C_Array(i), 0, RaceZone, False) Then
                    If C_Array(i).Lon >= MinLon AndAlso _
                        C_Array(i).Lon <= MaxLon AndAlso _
                        C_Array(i).Lat >= MinLat AndAlso _
                        C_Array(i).Lat <= MaxLat Then

                        p0.X = LonToCanvas(C_Array(i - 1).Lon_Deg)
                        p0.Y = LatToCanvas(C_Array(i - 1).Lat_Deg)
                        P1.X = LonToCanvas(C_Array(i).Lon_Deg)
                        P1.Y = LatToCanvas(C_Array(i).Lat_Deg)

                        SafeDrawLine(DC, C_Array(i - 1), C_Array(i), Pen, p0, P1)

                        LineCount += 1
                        drawn = True
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
            _MapPg.Progress(polyindex)
        Next

        Dim WP As VLM_RaceWaypoint

        For Each WP In WPs
            p0.X = LonToCanvas(WP.WPs(0)(0).Lon_Deg)
            p0.Y = LatToCanvas(WP.WPs(0)(0).Lat_Deg)
            P1.X = LonToCanvas(WP.WPs(0)(1).Lon_Deg)
            P1.Y = LatToCanvas(WP.WPs(0)(1).Lat_Deg)

            SafeDrawLine(DC, WP.WPs(0)(0), WP.WPs(0)(1), WPPen, p0, P1)
        Next

        DC.Close()
        LocalBmp.Render(D)
        LocalBmp.Freeze()
        _BackDropBmp = LocalBmp
        'GC.Collect()

        Return True

    End Function

    Public Sub InitViewer(ByVal owner As System.Windows.Window)


        Try
            If Not _RacePolygons Is Nothing Then
                _RacePolygons.AddLast(RouteurModel._RaceRect)
            End If

            _Progress = New MapProgressContext("Loading Maps Data...")
            _Frm = New frmRoutingProgress(100) With {.DataContext = _Progress}
            Dim TH As New System.Threading.Thread(AddressOf GSHHS_Reader.Read)
            Dim SI As New GSHHS_StartInfo With {.PolyGons = _RacePolygons, .StartPath = "..\gshhs\gshhs_" & RouteurModel.MapLevel & ".b", _
                                                .ProgressWindows = _Progress, .CompleteCallBack = AddressOf LoadPolygonComplete}
            _Frm.Show(owner, _Progress)
            TH.Start(SI)
            'GSHHS_Reader.Read(", RacePolygons, "Map")

        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

    Private Delegate Sub UpdatePathDelegate(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VOR_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                                                ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint))


    Private Sub SafeDrawLine(ByVal dc As DrawingContext, ByVal PrevP As Coords, ByVal P As Coords, ByVal pe As Pen, ByVal Prevpoint As Point, ByVal NewP As Point)
        If (PrevP.Lon * P.Lon < 0 AndAlso Math.Abs(P.Lon - PrevP.Lon) >= Math.PI)  Then
            Dim Pint As Point

            If PrevP.Lon < 0 Then
                Pint.X = LonToCanvas(-179.99)
            Else
                Pint.X = LonToCanvas(179.99)
            End If

            Pint.Y = LatToCanvas(PrevP.Lat_Deg + (P.Lat_Deg - PrevP.Lat_Deg) * (PrevP.Lon_Deg + 180) / (360 + PrevP.Lon_Deg - P.Lon_Deg))
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
    Public Sub UpdatePath(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VOR_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                          ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean, ByVal IsoChrones As LinkedList(Of IsoChrone), ByVal WPs As List(Of VLM_RaceWaypoint))

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
                                                                                Opponents, Grid, ClearGrid, ClearBoats, IsoChrones, WPs})

            While Q.Count > 0
                Dim R2 As System.Windows.Threading.DispatcherOperation = CType(Q.Dequeue, System.Windows.Threading.DispatcherOperation)
                R2.Abort()
            End While
            Q.Enqueue(R)
            'End If
            Return
        End If

        Try

            If Not _RacePolygonsInited Then
                Return
            End If

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

            'GC.Collect()

            'Dim Pixels As Array
            SyncLock Me
                _RBmp.Clear()

                If _ClearBgMap Then
                    BgStarted = False
                    _ClearBgMap = False
                    _BackDropBmp = Nothing
                    _GridBmp.Clear()
                End If

                If Not BgStarted AndAlso _BackDropBmp Is Nothing Then
                    BgStarted = True 'BgBackDropDrawing(0)

                    _Frm.DataContext = _MapPg
                    System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgBackDropDrawing, WPs)

                End If


                'debug bsp grid
                '

                'Dim x As Double
                'Dim y As Double

                'For y = 19 To 23.5 Step 0.05
                '    For x = -73.5 To -83.5 Step -0.1
                '        P1.X = LonToCanvas(x)
                '        P1.Y = LatToCanvas(y)
                '        Dim c As New Coords(y, x)
                '        If GSHHS_Reader.HitTest(c, 0, GSHHS_Reader.Polygons(c), True) Then
                '            DC.DrawEllipse(Nothing, opponentPenOption, P1, 1, 1)
                '        Else

                '            DC.DrawEllipse(Nothing, opponentPenNoOption, P1, 1, 1)
                '        End If
                '    Next
                'Next

                '
                ' Draw opponents map
                ' 

                If ClearBoats Then
                    _OpponentsBmp.Clear()
                End If
                If Not Opponents Is Nothing Then
                    SyncLock Opponents
                        For Each op In Opponents
                            If Not op.Value.Drawn Or op.Value.Name.ToLowerInvariant = "mifou" Then
                                P1.X = LonToCanvas(op.Value.CurPos.Lon_Deg)
                                P1.Y = LatToCanvas(op.Value.CurPos.Lat_Deg)
                                If op.Value.ProOption Then
                                    DC.DrawEllipse(Nothing, opponentPenOption, P1, 0.2, 0.2)
                                ElseIf op.Value.Name.ToLowerInvariant = "mifou" Then

                                    DC.DrawEllipse(Nothing, PathPen, P1, 1, 1)
                                Else

                                    DC.DrawEllipse(Nothing, opponentPenNoOption, P1, 0.5, 0.5)

                                End If
                                'op.Value.Drawn = True
                                'OpponentMap = True
                            End If
                        Next
                    End SyncLock
                    DC.Close()
                    _OpponentsBmp.Render(D)
                    DC = D.RenderOpen

                End If

                '
                ' Draw IsoChrones
                '
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
                            If Not iso.Drawn Or ClearGrid Then
                                Dim MaxIndex As Integer = iso.MaxIndex
                                Dim index As Integer
                                Dim PrevIndex As Integer
                                Dim CurP As Coords
                                For index = 0 To MaxIndex
                                    If Not iso.Data(index) Is Nothing Then
                                        CurP = iso.Data(index).P
                                        P1.X = LonToCanvas(CurP.Lon_Deg)
                                        P1.Y = LatToCanvas(CurP.Lat_Deg)

                                        If Not FirstPoint And index - PrevIndex < 20 Then
                                            SafeDrawLine(DC, PrevP, CurP, WindBrushes(CInt(iso.Data(index).WindStrength)), PrevPoint, P1)
                                        Else
                                            FirstPoint = False
                                        End If
                                        PrevIndex = index
                                        PrevP.Lon = CurP.Lon
                                        PrevP.Lat = CurP.Lat
                                        PrevPoint = P1

                                    Else
                                        FirstPoint = True
                                    End If
                                Next
                                iso.Drawn = True
                            End If

                        Next
                    End If

                Catch ex As Exception
                Finally
                    DC.Close()
                    _GridBmp.Render(D)
                    DC = D.RenderOpen

                End Try

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

                End If

                If _BackDropBmp IsNot Nothing AndAlso _BackDropBmp.IsFrozen Then
                    DC.DrawImage(_BackDropBmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))
                End If
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
                                    Pnt.Lon = lon
                                    Pnt.Lat = lat

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
                        Next
                    Catch
                    End Try
                End If


                DC.Close()
                _RBmp.Render(D)

                'Console.WriteLine("Update path complete in " & Now.Subtract(Start).TotalMilliseconds)
            End SyncLock
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

    Public Sub ClearBgMap()

        _ClearBgMap = True

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

    Public Property LatOffset() As Double
        Get
            Return _LatOffset
        End Get
        Set(ByVal value As Double)
            _LatOffset = value
        End Set
    End Property

    Public Property LonOffset() As Double
        Get
            Return _LonOffset
        End Get
        Set(ByVal value As Double)
            _LonOffset = value
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
End Class
