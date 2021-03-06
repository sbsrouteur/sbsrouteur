﻿'This file is part of Routeur.
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
Imports System.IO
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Data
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Navigation
Imports System.ComponentModel
Imports System.Threading
Imports System.Math
Imports System.Data.SQLite

Partial Public Class RouteurMain

    Implements INotifyPropertyChanged
    Public Const ROUTEURMODELRESOURCENAME As String = "RouteurModel"

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private WithEvents _WallTimer As New System.Windows.Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 0, 0, 50)}

    Private _DragCanvas As Boolean = False
    Private _ZoomCanvas As Boolean = False
    Private _DragStartPoint As Point
    Private _LastDraggedPoint As Point
    Private _PrevWidth As Double = 0
    Private _PrevHeight As Double = 0

    Private _MouseCapture As Boolean = False
    Private _MouseCaptureMode As frmAutoPilotViewer.CaptureInfoRequest = frmAutoPilotViewer.CaptureInfoRequest.None

    Private WithEvents _RouteForm As frmAutoPilotViewer
    Private WithEvents _frmControlDeck As New frmControlDeck

    Public Shared ReadOnly TravelCalculatorProperty As DependencyProperty = _
                           DependencyProperty.Register("TravelCalculator", _
                           GetType(TravelCalculator), GetType(RouteurMain), _
                           New FrameworkPropertyMetadata(Nothing, AddressOf TravelCalculatorChanged))




    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d'objet sous ce point.

    End Sub

    Private Sub FormLoaded(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        If M.The2DViewer Is Nothing Then
            M.The2DViewer = Me.VOR2DViewer
            'RedrawClick(Nothing, Nothing)

        End If
        M.Init(Me, Dispatcher)
        M.The2DViewer.InitViewer(Me)

        'Race exclusion must happen after 2Dviewer init
        M.InitRaceExclusions()

        Dim sApp As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, My.Application.Info.AssemblyName & ".exe")
        Title = M.AppString & " - Build " & RetrieveLinkerTimestamp(sApp)

        _WallTimer.Start()
        M.VorHandler.Owner = Me
        CenterOnBoat(Me, Nothing)
        UpdateCoordsExtent(M, True, False)
        _frmControlDeck.DataContext = M
        _frmControlDeck.Owner = Me
        _frmControlDeck.Show()


    End Sub

    Public ReadOnly Property HideNonMapCanvas() As Boolean
        Get
            Return Not _DragCanvas
        End Get
    End Property

    Public Property DragCanvas() As Boolean
        Get
            Return _DragCanvas
        End Get
        Set(ByVal value As Boolean)
            If _DragCanvas <> value Then
                _DragCanvas = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DragCanvas"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("HideNonMapCanvas"))
            End If
        End Set
    End Property
    Public Property TravelCalculator() As TravelCalculator
        Get
            Return CType(GetValue(TravelCalculatorProperty), TravelCalculator)
        End Get

        Set(ByVal value As TravelCalculator)
            SetValue(TravelCalculatorProperty, value)
        End Set
    End Property


    Public Shared Sub TravelCalculatorChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        d.SetValue(TravelCalculatorProperty, e.NewValue)

    End Sub

    Private Sub UpdateCoordsExtent(ByVal M As RouteurModel, ByVal UseRaceDefinitionExtent As Boolean, ByVal RescaleMap As Boolean)

        If M.The2DViewer Is Nothing Then
            Return
        End If

        Dim dh As Double = Math.Abs(_PrevWidth - VOR2DViewer.ActualWidth)
        Dim dw As Double = Math.Abs(_PrevHeight - VOR2DViewer.ActualHeight)

        If dh <= 1 AndAlso dw <= 1 Then
            Return
        End If
        _PrevWidth = VOR2DViewer.ActualWidth
        _PrevHeight = VOR2DViewer.ActualHeight

        Dim Pos1 As New Point(0, 0)
        Dim Pos2 As New Point(Me.VOR2DViewer.ActualWidth, Me.VOR2DViewer.ActualHeight)
        Dim C1 As Coords
        Dim C2 As Coords

        If Not UseRaceDefinitionExtent Then
            C1 = New Coords
            C2 = New Coords
            Pos1 = _2DGrid.TranslatePoint(Pos1, VOR2DViewer)
            Pos2 = _2DGrid.TranslatePoint(Pos2, VOR2DViewer)

            C1.Lon_Deg = M.The2DViewer.MapTransform.CanvasToLon(Pos1.X)
            C1.Lat_Deg = M.The2DViewer.MapTransform.CanvasToLat(Pos1.Y)
            C2.Lon_Deg = M.The2DViewer.MapTransform.CanvasToLon(Pos2.X)
            C2.Lat_Deg = M.The2DViewer.MapTransform.CanvasToLat(Pos2.Y)
        Else
            C1 = RouteurModel._RaceRect(0)
            C2 = RouteurModel._RaceRect(2)
        End If

        If C2.Lat > C1.Lat Then
            Dim T As Double = C2.Lat
            C2.Lat = C1.Lat
            C1.Lat = T
        End If

        M.VorHandler.CoordsExtent(C1, C2, _2DGrid.ActualWidth, _2DGrid.ActualHeight)
        M.CoordsExtent(C1, C2, _2DGrid.ActualWidth, _2DGrid.ActualHeight)
        If RescaleMap Then
            M.UpdateRaceScale(C1, C2)
            'SldZoom.Value = 1
        End If
        RaiseEvent PropertyChanged(M, New PropertyChangedEventArgs("WPsPath"))
        RedrawClick(Nothing, Nothing)
    End Sub


    Private Sub MouseStartDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        If My.Computer.Keyboard.CtrlKeyDown Then
            _ZoomCanvas = True
        Else
            DragCanvas = True
        End If

        _DragStartPoint = e.GetPosition(Me.VOR2DViewer)
        _LastDraggedPoint = _DragStartPoint

    End Sub

    Private Sub MouseEndDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        Dim C1 As Coords = Nothing
        Dim C2 As Coords = Nothing
        Dim EndDragPoint As Point = e.GetPosition(Me.VOR2DViewer)

        If DragCanvas Then
            DragCanvas = False

        ElseIf _ZoomCanvas Then
            _ZoomCanvas = False
            'FixMe handle antemeridien
            Dim MinX As Double = Math.Min(_DragStartPoint.X, EndDragPoint.X)
            Dim MaxX As Double = Math.Max(_DragStartPoint.X, EndDragPoint.X)
            Dim MinY As Double = Math.Min(_DragStartPoint.Y, EndDragPoint.Y)
            Dim MaxY As Double = Math.Max(_DragStartPoint.Y, EndDragPoint.Y)

            C1 = New Coords(VOR2DViewer.MapTransform.CanvasToLat(MaxY), VOR2DViewer.MapTransform.CanvasToLon(MinX))
            C2 = New Coords(VOR2DViewer.MapTransform.CanvasToLat(MinY), VOR2DViewer.MapTransform.CanvasToLon(MaxX))

        End If
        If C1 IsNot Nothing AndAlso C2 IsNot Nothing Then
            Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
            M.UpdateRaceScale(C1, C2)
            RedrawClick(Nothing, Nothing)
        End If

    End Sub

    Private Sub MouseMoveCanvas(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)

        Dim C1 As Coords = Nothing
        Dim C2 As Coords = Nothing
        Dim EndDragPoint As Point = e.GetPosition(Me.VOR2DViewer)

        Dim Dx As Double = EndDragPoint.X - _DragStartPoint.X
        Dim Dy As Double = EndDragPoint.Y - _DragStartPoint.Y
        If Abs(Dx) < 5 And Abs(Dy) < 5 Then
            Return
        End If


        If e.LeftButton = MouseButtonState.Pressed AndAlso DragCanvas Then
            Dim P As Point = e.GetPosition(VOR2DViewer)

            VOR2DViewer.DragMap(_LastDraggedPoint, P)
            _LastDraggedPoint = P
        End If
        If DragCanvas Then

            _DragStartPoint = EndDragPoint

            C1 = New Coords(VOR2DViewer.MapTransform.CanvasToLat(-Dy), VOR2DViewer.MapTransform.CanvasToLon(-Dx))
            C2 = New Coords(VOR2DViewer.MapTransform.CanvasToLat(VOR2DViewer.ActualHeight - Dy), VOR2DViewer.MapTransform.CanvasToLon(VOR2DViewer.ActualWidth - Dx))
        ElseIf _ZoomCanvas Then

        End If
        If C1 IsNot Nothing AndAlso C2 IsNot Nothing Then
            Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
            M.UpdateRaceScale(C1, C2)
            'RedrawClick(Nothing, Nothing)
        End If

    End Sub

    Private Sub _WallTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles _WallTimer.Tick
        Static Dlg As Action
        Static DlgRefresh As Action
        Static LastSec As Integer

        If Now.Second = LastSec Then
            Return
        End If

        LastSec = Now.Second

        If Dlg Is Nothing Then
            Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
            Dlg = New Action(AddressOf M.VorHandler.RefreshTimes)
            DlgRefresh = New Action(AddressOf M.Refresh)
        End If
        Me.Dispatcher.BeginInvoke(Dlg)
        Me.Dispatcher.BeginInvoke(DlgRefresh)

    End Sub

    Private Sub MouseLeaveCanvas(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)
        DragCanvas = False
    End Sub

    Private Sub CloseApp()
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.StartIsoRoute(Me, False, False)
        My.Settings.Save()
    End Sub

    Private Sub cmdSimpleVMG(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        Dim Curpos As New Coords()
        Dim StartTicks As DateTime

        With M.VorHandler
            Curpos.Lon_Deg = .UserInfo.LON
            Curpos.Lat_Deg = .UserInfo.LAT

            StartTicks = Now
            '.StartBestVMGRouter(Curpos, Now)
            Debug.WriteLine("Best VMG in " & Now.Subtract(StartTicks).Ticks / TimeSpan.TicksPerSecond)
        End With
    End Sub

    Private Sub MouseZoom(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseWheelEventArgs)

        If e.Delta > 0 Then
            Zoom(1.5, e.GetPosition(Me.VOR2DViewer))
        Else
            Zoom(1 / 1.2, e.GetPosition(Me.VOR2DViewer))
        End If
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

    End Sub

    Private Sub Zoom(ByVal Factor As Double, ByVal CenterPosition As Point)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        If M.RouteManager IsNot Nothing Then

            Dim dx As Double = VOR2DViewer.ActualWidth / Factor
            Dim dx1 As Double = dx * CenterPosition.X / VOR2DViewer.ActualWidth
            Dim dx2 As Double = dx - dx1
            Dim dy As Double = dx * VOR2DViewer.ActualHeight / VOR2DViewer.ActualWidth
            Dim dy1 As Double = dy * CenterPosition.Y / VOR2DViewer.ActualHeight
            Dim dy2 As Double = dy - dy1

            Debug.WriteLine(dx & " / " & dy)

            Dim C1 As New Coords(VOR2DViewer.MapTransform.CanvasToLat(CenterPosition.Y - dy1), VOR2DViewer.MapTransform.CanvasToLon(CenterPosition.X - dx1))
            Dim C2 As New Coords(VOR2DViewer.MapTransform.CanvasToLat(CenterPosition.Y + dy2), VOR2DViewer.MapTransform.CanvasToLon(CenterPosition.X + dx2))
            'TODO handle antemeridien
            Dim ZoomCenter As New Coords(VOR2DViewer.MapTransform.CanvasToLat(CenterPosition.Y), VOR2DViewer.MapTransform.CanvasToLon(CenterPosition.X))
            Debug.WriteLine("ZoomCenter" & ZoomCenter.ToString & " vs " & (C1.Lat_Deg + C2.Lat_Deg) / 2 & " = " & (C1.Lat_Deg + C2.Lat_Deg) / 2 - ZoomCenter.Lat_Deg)
            Debug.WriteLine("ZoomCenter" & ZoomCenter.ToString & " vs " & (C1.Lon_Deg + C2.Lon_Deg) / 2 & " = " & (C1.Lon_Deg + C2.Lon_Deg) / 2 - ZoomCenter.Lon_Deg)
            Debug.WriteLine("zoom to " & C1.ToString & " / " & C2.ToString)
            'M.VorHandler.CoordsExtent(C1, C2, _2DGrid.ActualWidth, _2DGrid.ActualHeight)
            'M.CoordsExtent(C1, C2, _2DGrid.ActualWidth, _2DGrid.ActualHeight)
            Debug.Assert(C1.Lon < C2.Lon And C1.Lat > C2.Lat)
            M.UpdateRaceScale(C1, C2)
            'M.RouteManager.Rescale()
            RedrawClick(Nothing, Nothing)
            ZoomCenter = New Coords(VOR2DViewer.MapTransform.CanvasToLat(CenterPosition.Y), VOR2DViewer.MapTransform.CanvasToLon(CenterPosition.X))
            Debug.WriteLine("New mouse pos" & ZoomCenter.ToString)

        End If
    End Sub


    Private Sub StartDrawBoatMap(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        M.VorHandler.DrawBoatMap()


    End Sub

    Private Sub AddPointToRoute(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim _2DW As _2D_Viewer = CType(sender, _2D_Viewer)
        Dim P As New Coords

        With P
            .Lat = _2DW.CurCoords.Lat
            .Lon = -_2DW.CurCoords.Lon
        End With
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.AddPointToRoute(P)

    End Sub

    Private Sub CheckIsoRoute(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        If Not M.VorHandler.StartIsoRoute(Me, CBool(chkIsoRoute.IsChecked), False) Then
            chkIsoRoute.IsChecked = False
        End If

    End Sub


    Private Sub RedrawClick(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.UpdatePath(False)

    End Sub

    Function RetrieveLinkerTimestamp(ByVal filePath As String) As DateTime
        Const PeHeaderOffset As Integer = 60
        Const LinkerTimestampOffset As Integer = 8

        Dim b(2047) As Byte
        Dim s As Stream = Nothing
        Try
            s = New FileStream(filePath, FileMode.Open, FileAccess.Read)
            s.Read(b, 0, 2048)
        Catch ex As Exception
            Return New DateTime(0)
        Finally
            If Not s Is Nothing Then s.Close()
        End Try

        Dim i As Integer = BitConverter.ToInt32(b, PeHeaderOffset)

        Dim SecondsSince1970 As Integer = BitConverter.ToInt32(b, i + LinkerTimestampOffset)
        Dim dt As New DateTime(1970, 1, 1, 0, 0, 0)
        dt = dt.AddSeconds(SecondsSince1970)
        dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours)
        Return dt
    End Function

    Private Sub ShowPilototoRouteDlg(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        If _RouteForm Is Nothing Then
            Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

            _RouteForm = New frmAutoPilotViewer(M, M.VorHandler.PlayerInfo.RaceInfo.vacfreq)
            _RouteForm.Owner = Me

        End If

        _RouteForm.Show()

    End Sub

    Private Sub _2D_Renderer_MouseMoveHandler(ByVal sender As Object, ByVal e As System.Windows.Input.MouseEventArgs)
        Dim P As New Point
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        P.X = e.GetPosition(Me._2D_Renderer).X + 1
        P.Y = e.GetPosition(Me._2D_Renderer).Y + 1

        M.The2DViewer.MouseP = P

    End Sub

    Private Sub ReloadPilototo(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.getboatinfo(True)
    End Sub

    Private Sub Refresh(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.Refresh()

    End Sub

    Private Sub AppQuit(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        CloseApp()
        Close()
    End Sub

    Private Sub CenterOnBoat(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        If M.VorHandler.UserInfo Is Nothing Then
            Return
        End If

        Dim V As VLM_Router = M.VorHandler
        Dim MinLon As Double = M.The2DViewer.MapTransform.CanvasToLon(0)
        Dim MaxLon As Double = M.The2DViewer.MapTransform.CanvasToLon(M.The2DViewer.ActualWidth)
        Dim MaxLat As Double = M.The2DViewer.MapTransform.CanvasToLat(0)
        Dim MinLat As Double = M.The2DViewer.MapTransform.CanvasToLat(M.The2DViewer.ActualHeight)
        Dim DLon As Double = (MaxLon - MinLon) / 2
        Dim DLat As Double = (MaxLat - MinLat) / 2
        Dim p1 As New Coords(M.VorHandler.UserInfo.LAT + DLat, M.VorHandler.UserInfo.LON - DLon)
        Dim p2 As New Coords(M.VorHandler.UserInfo.LAT - DLat, M.VorHandler.UserInfo.LON + DLon)

        M.UpdateRaceScale(p1, p2)
        RedrawClick(Nothing, Nothing)

    End Sub

    Public Function RenderCanvasCoords(ByVal e As System.Windows.Input.MouseEventArgs) As Point

        Return e.GetPosition(Me.VOR2DViewer)

    End Function

    Private Sub RendererSizeChanged(ByVal sender As Object, ByVal e As System.Windows.SizeChangedEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        UpdateCoordsExtent(M, False, True)

    End Sub

    Private Sub AppQuit(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs)
        CloseApp()
    End Sub

    Private Sub ReScaleMap(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        UpdateCoordsExtent(M, False, True)

    End Sub

    Private Sub RestoreMapScale(ByVal sender As Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        UpdateCoordsExtent(M, True, True)
    End Sub

    Private Sub _RouteForm_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles _RouteForm.Closed
        _RouteForm = Nothing
    End Sub


    Private Sub _RouteForm_RequestRouteReload() Handles _RouteForm.RequestRouteReload
        ReloadPilototo(Nothing, Nothing)
        _RouteForm.RefreshRoute()
    End Sub


    Private Sub OnMapContextMenuOpening(ByVal sender As System.Object, ByVal e As System.Windows.Controls.ContextMenuEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        M.VorHandler.RefreshActionMenu()

    End Sub

    Private Sub ComputeRouteXTR(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.StartXTRAssessment()
    End Sub

    Private Sub ShowRouteMgr(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim frm As New frmRoutesManager
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)
        If M.P_Info(0) IsNot Nothing Then
            M.RouteManager.FilterRaceID = M.P_Info(0).RaceInfo.idraces
            M.RouteManager.Boat = M.VorHandler.UserInfo.POL
            frm.ShowForm(M.RouteManager, Me)
        End If

    End Sub

    Private Sub EvtSet(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        RouteurModel.DebugEvt.Set()
    End Sub

    Private Sub RouteurMain_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing

        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.VorHandler.ShutDown()

    End Sub


    Private Sub ShowOptionsDlg(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

    End Sub

    Private Sub ZoomToNextWP(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        Dim V As VLM_Router = M.VorHandler
        Dim MinLon As Double = M.VorHandler.UserInfo.LON
        Dim MaxLon As Double = MinLon
        Dim MaxLat As Double = M.VorHandler.UserInfo.LAT
        Dim MinLat As Double = MaxLat

        Dim WP As Integer = M.VorHandler.CurUserWP
        For index As Integer = 0 To 1
            If M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index) IsNot Nothing Then
                MinLon = Min(MinLon, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lon_Deg)
                MaxLon = Max(MaxLon, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lon_Deg)
                MinLat = Min(MinLat, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lat_Deg)
                MaxLat = Max(MaxLat, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lat_Deg)
            End If
        Next
        Dim p1 As New Coords(MaxLat, MinLon)
        Dim p2 As New Coords(MinLat, MaxLon)

        M.UpdateRaceScale(p1, p2)
        RedrawClick(Nothing, Nothing)
    End Sub

    Private Sub ZoomToRace(sender As System.Object, e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        Dim V As VLM_Router = M.VorHandler
        Dim MinLon As Double = M.VorHandler.UserInfo.LON
        Dim MaxLon As Double = MinLon
        Dim MaxLat As Double = M.VorHandler.UserInfo.LAT
        Dim MinLat As Double = MaxLat

        For WP As Integer = 0 To M.CurPlayer.RaceInfo.races_waypoints.Count - 1
            For index As Integer = 0 To 1
                If M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index) IsNot Nothing Then
                    MinLon = Min(MinLon, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lon_Deg)
                    MaxLon = Max(MaxLon, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lon_Deg)
                    MinLat = Min(MinLat, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lat_Deg)
                    MaxLat = Max(MaxLat, M.CurPlayer.RaceInfo.races_waypoints(WP).WPs(0)(index).Lat_Deg)
                End If
            Next
        Next
        Dim p1 As New Coords(MaxLat, MinLon)
        Dim p2 As New Coords(MinLat, MaxLon)

        M.UpdateRaceScale(p1, p2)
        RedrawClick(Nothing, Nothing)
    End Sub

    Private Sub ImportTrack(sender As System.Object, e As System.Windows.RoutedEventArgs)

        Dim RaceId As Integer
        Dim BoatNum As Integer
        Dim StrRaceId As String = InputBox("Enter Race ID:", "-1")
        Dim StrBoatNum As String = InputBox("Enter BoatNum:", "-1")

        If Not Integer.TryParse(StrRaceId, RaceId) Then
            Return
        End If
        If Not Integer.TryParse(StrBoatNum, BoatNum) Then
            Return
        End If

        If BoatNum = -1 OrElse RaceId = -1 Then
            Return
        End If

        WS_Wrapper.GetTrack(RaceId, BoatNum, 1, False)

    End Sub

    Private Sub OnClearTileDBCache(sender As Object, e As RoutedEventArgs)

        If MessageBox.Show("Are you sure you want to clear the Tiles DB cache?", "Clear local map tiles cache", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then

            Dim db As New DBWrapper()

            db.ClearMapTileCache()

        End If
    End Sub

    Private Sub OnExportVacsEstimateClick(sender As Object, e As RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource(ROUTEURMODELRESOURCENAME), RouteurModel)

        M.OnExportNext24Vacs(Nothing)

    End Sub
End Class
