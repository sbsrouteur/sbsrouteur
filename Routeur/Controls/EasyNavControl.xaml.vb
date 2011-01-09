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
Imports System.Text
Imports System.Math

Partial Public Class EasyNavControl

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
    Public Event UploadBearingChange(ByVal NewBearing As Double)
    Public Event RequestBearingPathUpdate(ByVal NavMode As RoutePointView.EnumRouteMode, ByVal Value As Double)




    Public Shared ReadOnly BearingTrackProperty As DependencyProperty = _
                           DependencyProperty.Register("BearingTrack", _
                           GetType(PathGeometry), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))


    Public Shared ReadOnly BestSpeedAngleProperty As DependencyProperty = _
                           DependencyProperty.Register("BestSpeedAngle", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))


    Public Shared ReadOnly BoatBearingProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatBearing", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf onBearingChange))


    Public Shared ReadOnly BoatLatProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatLat", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnBoatLatChanged))


    Public Shared ReadOnly BoatLonProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatLon", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnBoatLonChanged))



    Public Shared ReadOnly BoatSpeedProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatSpeed", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))





    Public Property BoatDest As Coords
        Get
            Return CType(GetValue(BoatDestProperty), Coords)
        End Get

        Set(ByVal value As Coords)
            SetValue(BoatDestProperty, value)
        End Set
    End Property

    Public Shared ReadOnly BoatDestProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatDest", _
                           GetType(Coords), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))


    Public Shared ReadOnly BoatTypeProperty As DependencyProperty = _
                           DependencyProperty.Register("BoatType", _
                           GetType(String), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnSailsChange))


    Public Shared ReadOnly MeteoDirProperty As DependencyProperty = _
                           DependencyProperty.Register("MeteoDir", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnSailsChange))


    Public Shared ReadOnly MeteoStrengthProperty As DependencyProperty = _
                           DependencyProperty.Register("MeteoStrength", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnSailsChange))

    Public Shared ReadOnly ClsSailManagerProperty As DependencyProperty = _
                           DependencyProperty.Register("ClsSailManager", _
                           GetType(clsSailManager), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnSailsChange))

    Public Shared ReadOnly RendererProperty As DependencyProperty = _
                           DependencyProperty.Register("Renderer", _
                           GetType(_2D_Viewer), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))

    Public Shared ReadOnly NewCourseProperty As DependencyProperty = _
                           DependencyProperty.Register("NewCourse", _
                           GetType(Double), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnNewCourseChange))




    Public Shared ReadOnly Track24hProperty As DependencyProperty = _
                           DependencyProperty.Register("Track24h", _
                           GetType(GeometryGroup), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))




    Public Shared ReadOnly TrackPointsProperty As DependencyProperty = _
                           DependencyProperty.Register("TrackPoints", _
                           GetType(List(Of Coords)), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(AddressOf OnTrackPointListChange))




    Public Shared ReadOnly UploadPendingProperty As DependencyProperty = _
                           DependencyProperty.Register("UploadPending", _
                           GetType(Boolean), GetType(EasyNavControl), _
                           New FrameworkPropertyMetadata(Nothing))




    Private _Owner As EasyNavControl
    Private _bCapture As Boolean
    Private _BearingDrag As Boolean = False
    Private _SpeedPolar As PathGeometry
    Private _BestAngleToDest As Double


    Public Property BearingTrack As PathGeometry
        Get
            Return CType(GetValue(BearingTrackProperty), PathGeometry)
        End Get

        Set(ByVal value As PathGeometry)
            SetValue(BearingTrackProperty, value)
        End Set
    End Property

    Public Property BestAngleToDest() As Double
        Get
            Return _BestAngleToDest
        End Get
        Set(ByVal value As Double)

            If value <> _BestAngleToDest Then
                _BestAngleToDest = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("BestAngleToDest"))
            End If
        End Set
    End Property


    Public Property BestSpeedAngle As Double
        Get
            Return CDbl(GetValue(BestSpeedAngleProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(BestSpeedAngleProperty, value)
        End Set
    End Property

    Public Property BoatBearing As Double
        Get
            Return CDbl(GetValue(BoatBearingProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(BoatBearingProperty, value)
        End Set
    End Property

    Public Property BoatLat As Double
        Get
            Return CDbl(GetValue(BoatLatProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(BoatLatProperty, value)
        End Set
    End Property

    Public Property BoatLon As Double
        Get
            Return CDbl(GetValue(BoatLonProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(BoatLonProperty, value)
        End Set
    End Property

    Public Property BoatSpeed As Double
        Get
            Return CDbl(GetValue(BoatSpeedProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(BoatSpeedProperty, value)
        End Set
    End Property

    Public Property BoatType As String
        Get
            Return CStr(GetValue(BoatTypeProperty))
        End Get

        Set(ByVal value As String)
            SetValue(BoatTypeProperty, value)
        End Set
    End Property

    Public Property ClsSailManager As clsSailManager
        Get
            Return CType(GetValue(ClsSailManagerProperty), clsSailManager)
        End Get

        Set(ByVal value As clsSailManager)
            SetValue(ClsSailManagerProperty, value)
        End Set
    End Property


    Private Sub EndBearingChangeDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        UploadPending = NewCourse <> BoatBearing
        _BearingDrag = False
        Mouse.Capture(Nothing)
    End Sub


    Private Function GetCanvasXfromLon(ByVal Lon As Double) As Double

        If Renderer IsNot Nothing Then
            Return Renderer.LonToCanvas(Lon)
        Else
            Return 0
        End If

    End Function


    Private Function GetCanvasYfromLat(ByVal Lat As Double) As Double

        If Renderer IsNot Nothing Then
            Return Renderer.LatToCanvas(Lat)
        Else
            Return 0
        End If

    End Function

    Public Property MeteoDir As Double
        Get
            Return CDbl(GetValue(MeteoDirProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(MeteoDirProperty, value)
        End Set
    End Property

    Public Property MeteoStrength As Double
        Get
            Return CDbl(GetValue(MeteoStrengthProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(MeteoStrengthProperty, value)
        End Set
    End Property

    Public Sub New()
        MyBase.New()

        Me.InitializeComponent()

        ' Insérez le code requis pour la création d’objet sous ce point.
    End Sub

    Private Shared Sub OnBoatLatChanged(ByVal o As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf o Is EasyNavControl Then
            Dim ctrl As EasyNavControl = CType(o, EasyNavControl)
            o.SetValue(Canvas.TopProperty, ctrl.GetCanvasYfromLat(CDbl(e.NewValue)) - 64)
        End If

    End Sub

    Private Shared Sub OnBoatLonChanged(ByVal o As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf o Is EasyNavControl Then
            Dim ctrl As EasyNavControl = CType(o, EasyNavControl)
            o.SetValue(Canvas.LeftProperty, ctrl.GetCanvasXfromLon(CDbl(e.NewValue)) - 64)
        End If

    End Sub

    Private Shared Sub OnBearingChange(ByVal O As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf O Is EasyNavControl AndAlso TypeOf e.NewValue Is Double Then

            CType(O, EasyNavControl).NewCourse = CDbl(e.NewValue)

        End If

    End Sub

    Private Sub OnPathChangeRequest()
        RaiseEvent RequestBearingPathUpdate(RoutePointView.EnumRouteMode.Bearing, NewCourse)
    End Sub

    Private Shared Sub OnNewCourseChange(ByVal O As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf O Is EasyNavControl AndAlso TypeOf e.NewValue Is Double Then

            Dim NC As EasyNavControl = CType(O, EasyNavControl)

            With nc
                .UploadPending = False
                .OnPathChangeRequest()
                .BoatSpeed = .ClsSailManager.GetSpeed(.BoatType, Routeur.clsSailManager.EnumSail.OneSail, VLM_Router.WindAngle(.NewCourse, .MeteoDir), .MeteoStrength)

            End With

        End If

    End Sub

    Private Shared Sub OnTrackPointListChange(ByVal o As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf o Is EasyNavControl Then
            CType(o, EasyNavControl).SetTrack()
        End If

    End Sub

    Private Shared Sub OnSailsChange(ByVal o As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf o Is EasyNavControl AndAlso TypeOf e.NewValue Is Double Then

            CType(o, EasyNavControl).SetSpeedPolar()

        End If

    End Sub

    Public Sub RefreshPosition()

        OnBoatLatChanged(Me, New DependencyPropertyChangedEventArgs(BoatLatProperty, 0, BoatLat))
        OnBoatLonChanged(Me, New DependencyPropertyChangedEventArgs(BoatLonProperty, 0, BoatLon))

    End Sub

    Public Property Renderer As _2D_Viewer
        Get
            Return CType(GetValue(RendererProperty), _2D_Viewer)
        End Get

        Set(ByVal value As _2D_Viewer)
            SetValue(RendererProperty, value)
        End Set
    End Property

    Public Property SpeedPolar As PathGeometry
        Get
            If _SpeedPolar Is Nothing Then
                SetSpeedPolar()
            End If
            Return _SpeedPolar
        End Get

        Set(ByVal value As PathGeometry)
            _SpeedPolar = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SpeedPolar"))
        End Set

    End Property


    Public Sub SetSpeedPolar()

        If ClsSailManager Is Nothing Then
            ClsSailManager = VLM_Router.Sails
            If ClsSailManager Is Nothing Then
                Return
            End If
        End If

        If BoatType Is Nothing Then
            Return
        End If

        Dim sb As New StringBuilder
        Dim MaxSpeedAngle As Double = 0
        Dim AlphaStep As Integer = 1
        Dim S(180) As Double
        Dim i As Integer
        Dim MaxS As Double = 0
        Dim MaxVMG As Double = 0
        Dim BestVMGAngle As Double = 0
        Dim TC As New TravelCalculator
        TC.StartPoint = New Coords(BoatLat, BoatLon)
        TC.EndPoint = BoatDest

        For i = 0 To 180 Step AlphaStep

            S(i) = ClsSailManager.GetSpeed(BoatType, Routeur.clsSailManager.EnumSail.OneSail, i, MeteoStrength)
            If S(i) > MaxS Then
                MaxS = S(i)
                MaxSpeedAngle = i
            End If

            Dim C1 As Double = VLM_Router.WindAngle(i + MeteoDir, TC.OrthoCourse_Deg) / 180 * PI
            Dim C2 As Double = VLM_Router.WindAngle(MeteoDir - i, TC.OrthoCourse_Deg) / 180 * PI
            If S(i) * Cos(C1) > MaxVMG Then
                MaxVMG = S(i) * Cos(C1)
                BestVMGAngle = i + MeteoDir
            End If
            If S(i) * Cos(C2) > MaxVMG Then
                MaxVMG = S(i) * Cos(C2)
                BestVMGAngle = MeteoDir - i
            End If

        Next

        If VLM_Router.WindAngleWithSign(BoatBearing, MeteoDir) < 0 Then
            BestSpeedAngle = (MeteoDir - MaxSpeedAngle + 720) Mod 360
        Else
            BestSpeedAngle = (MaxSpeedAngle + MeteoDir) Mod 360
        End If

        BestAngleToDest = (BestVMGAngle + 720) Mod 360


        If MaxS = 0 Then
            If _SpeedPolar IsNot Nothing Then
                _SpeedPolar.Figures = Nothing
            End If
            Return
        End If
        For i = -180 To 180 Step AlphaStep

            If sb.Length = 0 Then
                sb.Append(" M ")
            Else
                sb.Append(" L ")
            End If

            Dim ScaledSpeed As Double = S(CInt(Abs(i))) / MaxS

            Dim y As Double = -64 * ScaledSpeed * Cos(i / 180 * PI)
            Dim x As Double = 64 * ScaledSpeed * Sin(i / 180 * PI)
            sb.Append(GetCoordsString(x, y))
        Next

        Dim PC As New PathFigureCollectionConverter


        If _SpeedPolar Is Nothing Then
            _SpeedPolar = New PathGeometry
        End If

        _SpeedPolar.Figures = CType(PC.ConvertFromString(sb.ToString), PathFigureCollection)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("SpeedPolar"))
        Return
    End Sub

    Private Sub SetTrack()

        Dim sb As New StringBuilder

        Dim y As Double
        Dim x As Double

        For Each C In TrackPoints

            If Not C Is Nothing Then
                
                y = GetCanvasYfromLat(C.Lat_Deg)
                x = GetCanvasXfromLon(C.Lon_Deg)
                sb.Append(" L " & GetCoordsString(x, y))
            End If

        Next

        If sb.Length = 0 Then
            'If meteo is not available track 60nm in the proper direction
            Dim Tc As New TravelCalculator
            Tc.StartPoint = New Coords(BoatLat, BoatLon)
            Dim C As Coords = Tc.ReachDistance(60, NewCourse)

            y = GetCanvasYfromLat(C.Lat_Deg)
            x = GetCanvasXfromLon(C.Lon_Deg)
            sb.Append(" L " & GetCoordsString(x, y))
        End If
        x = GetCanvasXfromLon(BoatLon)
        y = GetCanvasYfromLat(BoatLat)
        sb.Insert(0, "M " & GetCoordsString(x, y))


        Dim PC As New PathFigureCollectionConverter

        If Track24h Is Nothing Then
            Track24h = New GeometryGroup
        Else
            Track24h.Children.Clear()
        End If
        Dim T As New PathGeometry
        T.Figures = CType(PC.ConvertFromString(sb.ToString), PathFigureCollection)
        Track24h.Children.Add(T)

        Dim Points As PathFigure = Nothing
        For Each C In TrackPoints

            If Not C Is Nothing Then

                Dim E As New EllipseGeometry
                y = GetCanvasYfromLat(C.Lat_Deg)
                x = GetCanvasXfromLon(C.Lon_Deg)
                E.Center = New Point(x, y)
                E.RadiusX = 3
                E.RadiusY = 3
                Track24h.Children.Add(E)
            End If

        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Track24h"))

    End Sub

    Private Sub MouseBearingDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)


        If _BearingDrag AndAlso TypeOf sender Is IInputElement Then

            Dim Pos = Mouse.GetPosition(Me)


            Dim X As Double = (Pos.X - 64)
            Dim Y As Double = (64 - Pos.Y)
            Dim Cap As Double = (Atan2(X, Y) / PI * 180 + 360) Mod 360

            'Console.WriteLine("X:" & X & ", Y " & Y & " Angle : " & Cap)
            NewCourse = Round(Cap, 2)


        End If
    End Sub


    Public Property NewCourse As Double
        Get
            Return CDbl(GetValue(NewCourseProperty))
        End Get

        Set(ByVal value As Double)
            SetValue(NewCourseProperty, value)
        End Set
    End Property

    Private Sub StartBearingChangeDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        _BearingDrag = Mouse.Capture(Me.boatNewCourse)

    End Sub


    Public Property Track24h As GeometryGroup
        Get
            Return CType(GetValue(Track24hProperty), GeometryGroup)
        End Get

        Set(ByVal value As GeometryGroup)
            SetValue(Track24hProperty, value)
        End Set
    End Property

    Public Property TrackPoints As List(Of Coords)
        Get
            Return CType(GetValue(TrackPointsProperty), List(Of Coords))
        End Get

        Set(ByVal value As List(Of Coords))
            SetValue(TrackPointsProperty, value)
        End Set
    End Property


    Public Property UploadPending As Boolean
        Get
            Return CBool(GetValue(UploadPendingProperty))
        End Get

        Set(ByVal value As Boolean)
            SetValue(UploadPendingProperty, value)
        End Set
    End Property

    Private Sub CancelUpload(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        NewCourse = BoatBearing
        UploadPending = False

    End Sub

    Private Sub UploadBearing(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)

        RaiseEvent UploadBearingChange(NewCourse)

    End Sub

End Class
