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

Partial Public Class RouteurMain

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private WithEvents _WallTimer As New System.Windows.Threading.DispatcherTimer With {.Interval = New TimeSpan(0, 0, 0, 0, 50)}

    Private _DragCanvas As Boolean = False
    Private _DragStartPoint As Point

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

        
        Dim M = CType(FindResource("RouteurModel"), RouteurModel)
        M.Init()

        If M.The2DViewer Is Nothing Then
            M.The2DViewer = Me.VOR2DViewer
            M.The2DViewer.InitViewer(Me)

        End If
        Dim sApp As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, My.Application.Info.AssemblyName & ".exe")
        Title = M.AppString & " - Build " & RetrieveLinkerTimestamp(sApp)

        _WallTimer.Start()
        M.VorHandler.Owner = Me
        

    End Sub

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


    Private Sub MouseStartDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        _DragCanvas = True
        _DragStartPoint = e.GetPosition(Me.VOR2DViewer)

    End Sub

    Private Sub MouseEndDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        _DragCanvas = False
        'Console.WriteLine("dragged to " & Me.SldLon.Value & " " & Me.SldLat.Value)
    End Sub

    Private Sub MouseMoveCanvas(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)


        If _DragCanvas Then
            Me.SldLon.Value += (e.GetPosition(Me.VOR2DViewer).X - _DragStartPoint.X) * SldZoom.Value
            Me.SldLat.Value += (e.GetPosition(Me.VOR2DViewer).Y - _DragStartPoint.Y) * SldZoom.Value

            _DragStartPoint = e.GetPosition(Me.VOR2DViewer)
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
            Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)
            Dlg = New Action(AddressOf M.VorHandler.RefreshTimes)
            DlgRefresh = New Action(AddressOf M.Refresh)
        End If
        Me.Dispatcher.BeginInvoke(Dlg)
        Me.Dispatcher.BeginInvoke(DlgRefresh)

    End Sub

    Private Sub MouseLeaveCanvas(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)
        _DragCanvas = False
    End Sub

    Private Sub cmdSimpleVMG(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)

        Dim Curpos As New Coords()
        Dim StartTicks As DateTime

        With M.VorHandler
            Curpos.Lon_Deg = .UserInfo.position.longitude
            Curpos.Lat_Deg = .UserInfo.position.latitude

            StartTicks = Now
            '.StartBestVMGRouter(Curpos, Now)
            Debug.WriteLine("Best VMG in " & Now.Subtract(StartTicks).Ticks / TimeSpan.TicksPerSecond)
        End With
    End Sub

    Private Sub MouseZoom(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseWheelEventArgs)

        If e.Delta > 0 Then
            Zoom(1.2, e.GetPosition(Me.VOR2DViewer))
        Else
            Zoom(1 / 1.2, e.GetPosition(Me.VOR2DViewer))
        End If

    End Sub

    Private Sub Zoom(ByVal Factor As Double, ByVal CenterPosition As Point)

        'If Factor > 1 Then
        '    SldLon.Value -= VOR2DViewer.Width / 2 / SldZoom.Value / Factor
        '    SldLat.Value -= VOR2DViewer.Height / 2 / SldZoom.Value / Factor
        'Else
        '    SldLon.Value += VOR2DViewer.Width / 2 / SldZoom.Value / Factor
        '    SldLat.Value += VOR2DViewer.Height / 2 / SldZoom.Value / Factor
        'End If
        'Dim W As Double = Me.ActualWidth
        'Dim H As Double = Me.ActualHeight
        'Dim P1 As Point = Nothing
        'Dim P2 As Point = Nothing

        'Me.VOR2DViewer.RenderTransformOrigin = New Point(CenterPosition.X / W, CenterPosition.Y / H)
        VOR2DViewer.RenderTransformOrigin = New Point(CenterPosition.X / VOR2DViewer.Width, CenterPosition.Y / VOR2DViewer.Height)

        SldZoom.Value *= Factor
        'SldLon.Value = CenterPosition.X * SldZoom.Value
        'SldLat.Value = CenterPosition.Y * SldZoom.Value
        'Console.WriteLine("Zoom to " & SldLon.Value & " " & SldLat.Value)

    End Sub


    Private Sub StartDrawBoatMap(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)
        M.VorHandler.DrawBoatMap()


    End Sub

    Private Sub AddPointToRoute(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)
        Dim _2DW As _2D_Viewer = CType(sender, _2D_Viewer)
        Dim P As New Coords

        With P
            .Lat = _2DW.CurCoords.Lat
            .Lon = -_2DW.CurCoords.Lon
        End With

        M.VorHandler.AddPointToRoute(P)

    End Sub

    Private Sub CheckGridRoute(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)

        M.VorHandler.startGridRoute(CBool(chkGridRoute.IsChecked))


    End Sub

    Private Sub RedrawClick(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)

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

    Private Sub _2D_Renderer_MouseMoveHandler(ByVal sender as Object, ByVal e as System.Windows.Input.MouseEventArgs)
        'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
        Dim P As New Point
        Dim M As RouteurModel = CType(FindResource("RouteurModel"), RouteurModel)
        P.X = e.GetPosition(Me._2D_Renderer_NOZoom).X + 1
        P.Y = e.GetPosition(Me._2D_Renderer_NOZoom).Y + 1

        M.The2DViewer.MouseP = P

    End Sub

    Private Sub ReloadPilototo(ByVal sender as Object, ByVal e as System.Windows.RoutedEventArgs)
    	'TODO : ajoutez ici l’implémentation du gestionnaire d’événements.
    End Sub

 
End Class
