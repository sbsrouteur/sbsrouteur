﻿Imports System
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


    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(48, 12), New Coords(48, -13), New Coords(55, -13), New Coords(55, -2), _
    '                                                     New Coords(59, 5), New Coords(62, 20), New Coords(48.1, 12)}
    'Private Shared _RaceRect As Coords() = New Coords() {New Coords(58, 10), New Coords(48, 10), New Coords(48, 20), New Coords(60, 20), New Coords(60, 16.5), _
    '                                                     New Coords(56 + 17 / 60, 16 + 28 / 60), New Coords(58, 14) _
    '                                                     }

    Public Shared RacePolygons As New List(Of Coords())


    Shared Sub New()

        If Not RacePolygons Is Nothing Then
            RacePolygons.Add(RouteurModel._RaceRect)
        End If

    End Sub


    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d'objet sous ce point.

        '_Model = CType(FindResource("2DModel"), _2DViewerModel)
        If Not RacePolygons Is Nothing Then
            InitViewer()
        End If

    End Sub

    Public Function CanvasToLat(ByVal C As Double) As Double
        Return (90 * DEFINITION - C) / RouteurModel.SCALE + RouteurModel.LAT_OFFSET '90 / RouteurModel.SCALE - C / DEFINITION / RouteurModel.SCALE + RouteurModel.LAT_OFFSET
    End Function

    Public Function CanvasToLon(ByVal V As Double) As Double
        Return (V - 180 * DEFINITION) / RouteurModel.SCALE + RouteurModel.LON_OFFSET '(V - 180 * DEFINITION) / DEFINITION / RouteurModel.SCALE
    End Function


    Public Shared Function LatToCanvas(ByVal V As Double) As Double

        Return 90 * DEFINITION - (V - RouteurModel.LAT_OFFSET) * RouteurModel.SCALE

    End Function

    Public Shared Function LonToCanvas(ByVal V As Double) As Double

        Return 180 * DEFINITION + (V - RouteurModel.LON_OFFSET) * RouteurModel.SCALE

    End Function

    Private Sub RenDerBackDrop(ByVal D As DrawingVisual)

    End Sub

    Private Function BgBackDropDrawing(ByVal state As Object) As Boolean

        Dim D As New DrawingVisual
        Dim DC As DrawingContext = D.RenderOpen()
        Dim FirstPoint As Boolean
        Dim p0 As Point
        Dim P1 As Point
        Dim Pen As New Pen(New SolidColorBrush(System.Windows.Media.Colors.Black), 0.3)
        Dim LocalBmp As New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
        Dim RaceZone As New List(Of Coords())
        Dim polyindex As Integer = -1
        RaceZone.Add(RouteurModel._RaceRect)

        'Dim Prevbmp As RenderTargetBitmap = Nothing
        Dim LineCount As Integer
        Static R As New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION)

        If GSHHS_Reader.AllPolygons.Count = 0 Then
            Return False
        End If
        Pen.Freeze()
        For Each C_Array In GSHHS_Reader.AllPolygons
            polyindex += 1
            'If C_Array.GetUpperBound(0) < 7 Then
            '    Exit For
            'End If

            'If Not Prevbmp Is Nothing Then
            'DC.DrawImage(Prevbmp, New Rect(0, 0, XBMP_RES * DEFINITION, YBMP_RES * DEFINITION))
            'End If

            FirstPoint = True
            For i = 1 To C_Array.GetUpperBound(0) - 1


                'p0 = New Point(LonToCanvas(C_Array(i - 1).Lon_Deg), LatToCanvas(C_Array(i - 1).Lat_Deg))
                'P1 = New Point(LonToCanvas(C_Array(i).Lon_Deg), LatToCanvas(C_Array(i).Lat_Deg))
                'If GSHHS_Reader.HitTest(C_Array(i), 0, RaceZone, False) Then
                If C_Array(i) IsNot Nothing AndAlso C_Array(i - 1) IsNot Nothing Then
                    p0.X = LonToCanvas(C_Array(i - 1).Lon_Deg)
                    p0.Y = LatToCanvas(C_Array(i - 1).Lat_Deg)
                    P1.X = LonToCanvas(C_Array(i).Lon_Deg)
                    P1.Y = LatToCanvas(C_Array(i).Lat_Deg)
                    If Math.Abs(p0.X - P1.X) < 1000 Then
                        DC.DrawLine(Pen, p0, P1)
                        LineCount += 1
                    End If
                End If
                'End If

                If LineCount Mod 500 = 0 Then
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
                    LocalBmp = New RenderTargetBitmap(XBMP_RES * DEFINITION, YBMP_RES * DEFINITION, DPI_RES, DPI_RES, PixelFormats.Default)
                    'LocalBmp.Clear()
                    GC.Collect()
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
        Next

        DC.Close()
        LocalBmp.Render(D)
        LocalBmp.Freeze()
        _BackDropBmp = LocalBmp
        GC.Collect()

        Return True

    End Function

    Public Sub InitViewer()



        Try
            GSHHS_Reader.Read("..\gshhs\gshhs_" & RouteurModel.MapLevel & ".b", RacePolygons, "Map")
            'GSHHS_Reader.Read("..\gshhs\gshhs_i.b", RacePolygons, "Map")
            'GSHHS_Reader.Read("..\gshhs\wdb_borders_i.b", RacePolygons, "Border")
           
        Catch ex As Exception
            Debug.WriteLine(ex)
        End Try
    End Sub

    Private Delegate Sub UpdatePathDelegate(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VOR_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                                                ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean)


    Private Sub SafeDrawLine(ByVal dc As DrawingContext, ByVal PrevP As Coords, ByVal P As Coords, ByVal pe As Pen, ByVal Prevpoint As Point, ByVal NewP As Point)
        If PrevP.Lon * P.Lon < 0 AndAlso Math.Abs(P.Lon - PrevP.Lon) >= Math.PI Then
            Dim Pint As Point
            If PrevP.Lon < 0 Then
                Pint.X = LonToCanvas(-179.9)
            Else
                Pint.X = LonToCanvas(179.9)
            End If
            Pint.Y = LatToCanvas(PrevP.Lat_Deg + (P.Lat_Deg - PrevP.Lat_Deg) * (PrevP.Lon_Deg + 180) / (360 + PrevP.Lon_Deg - P.Lon_Deg))
            DC.DrawLine(pe, Prevpoint, Pint)
            If PrevP.Lon < 0 Then
                Pint.X = LonToCanvas(179.9)
            Else
                Pint.X = LonToCanvas(-179.9)
            End If
            DC.DrawLine(pe, Pint, NewP)
        Else
            DC.DrawLine(pe, Prevpoint, NewP)
        End If
    End Sub
    Public Sub UpdatePath(ByVal PathString As String, ByVal Routes As ObservableCollection(Of VOR_Router.clsrouteinfopoints)(), ByVal Opponents As Dictionary(Of String, BoatInfo), _
                          ByVal Grid As Queue(Of RoutingGridPoint), ByVal ClearGrid As Boolean, ByVal ClearBoats As Boolean)

        Static Invoking As Integer = 0
        Static lastinvoke As DateTime = New DateTime(0)
        Dim Start As DateTime = Now
        Static dlg As New UpdatePathDelegate(AddressOf UpdatePath)
        Static Q As New Queue(Of System.Windows.Threading.DispatcherOperation)
        Static CurPos As New Coords

        'GC.Collect()

        If Not Dispatcher.Thread Is System.Threading.Thread.CurrentThread Then
            'Invoking = System.Threading.Interlocked.CompareExchange(Invoking, 1, 0)
            'If Invoking = 0 Then
            '    If Not ClearGrid And Now.Subtract(lastinvoke).TotalSeconds < 5 Then
            '        System.Threading.Interlocked.Exchange(Invoking, 0)
            '        Return
            '    End If
            '    lastinvoke = Now
            Dim R As System.Windows.Threading.DispatcherOperation = Dispatcher.BeginInvoke(dlg, New Object() {PathString, Routes, Opponents, Grid, ClearGrid, ClearBoats})

            While Q.Count > 0
                Dim R2 As System.Windows.Threading.DispatcherOperation = CType(Q.Dequeue, System.Windows.Threading.DispatcherOperation)
                R2.Abort()
            End While
            Q.Enqueue(R)
            'End If
            Return
        End If

        Try

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

                If Not BgStarted AndAlso _BackDropBmp Is Nothing Then
                    System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgBackDropDrawing, D)
                    BgStarted = True 'BgBackDropDrawing(0)

                End If


                'debug bsp grid
                '

                'Dim x As Double
                'Dim y As Double

                'For y = 30.5 To 40.5 Step 0.1
                '    For x = -7 To 48 Step 0.1
                '        P1.X = LonToCanvas(x)
                '        P1.Y = LatToCanvas(y)
                '        Dim c As New Coords(y, x)
                '        If GSHHS_Reader.HitTest(c, 0, GSHHS_Reader.Polygons(c), True) Then
                '            DC.DrawEllipse(Nothing, opponentPenOption, P1, 1, 1)
                '        Else

                '            DC.DrawEllipse(Nothing, PathPen, P1, 1, 1)
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


                            If R.Drawn = False Then
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

                                            'If RouteIsWP Then 'Continue WP route from the last path point
                                            PrevP.Lon = CurPos.Lon
                                            PrevP.Lat = CurPos.Lat
                                            PrevPoint.X = LonToCanvas(CurPos.Lon_Deg)
                                            PrevPoint.Y = LatToCanvas(CurPos.Lat_Deg)
                                            SafeDrawLine(DC, PrevP, P.P, routePen(PenNumber), PrevPoint, P1)
                                            'End If

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
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Drawn"))
        Catch ex As Exception

            Debug.WriteLine("UpdatePath exception : " & ex.Message)
        Finally
            System.Threading.Interlocked.Exchange(Invoking, 0)
        End Try


    End Sub

    Public Sub RedrawCanvas()

        SyncLock _RBmp
            Me.RenderBuffer.Source = _RBmp
        End SyncLock

    End Sub


    Private Sub MouseOverMap(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)

        Static evtprops As New PropertyChangedEventArgs("CurCoords")
        Dim P As Point = e.GetPosition(CType(sender, IInputElement))

        CurCoords.Lon_Deg = CanvasToLon(P.X)
        CurCoords.Lat_Deg = CanvasToLat(P.Y)
        P.X += 5
        P.Y += 5
        'Debug.WriteLine("Coords : " & CurCoords.ToString)
        RaiseEvent PropertyChanged(Me, evtprops)


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

    Public Property MouseP() As Point
        Get
            Return _P
        End Get
        Set(ByVal value As Point)
            _P = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MouseP"))
        End Set
    End Property
End Class
