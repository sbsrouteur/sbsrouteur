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
                           New FrameworkPropertyMetadata(Nothing))


    Private _Owner As EasyNavControl
    Private _bCapture As Boolean
    Private _BearingDrag As Boolean = False
    Private _SpeedPolar As PathGeometry



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

    Private Shared Sub onBearingChange(ByVal O As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf O Is EasyNavControl AndAlso TypeOf e.NewValue Is Double Then
            CType(O, EasyNavControl).NewCourse = CDbl(e.NewValue)
        End If
    End Sub



    Private Shared Sub OnSailsChange(ByVal o As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)

        If TypeOf o Is EasyNavControl Then
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
        Dim AlphaStep As Integer = 3
        Dim S(180) As Double
        Dim i As Integer
        Dim MaxS As Double = 0
        For i = 0 To 180 Step AlphaStep

            S(i) = ClsSailManager.GetSpeed(BoatType, Routeur.clsSailManager.EnumSail.OneSail, i, MeteoStrength)
            If S(i) > MaxS Then
                MaxS = S(i)
            End If

        Next

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

    Private Sub MouseBearingDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseEventArgs)


        If _BearingDrag AndAlso TypeOf sender Is IInputElement Then

            Dim Pos = Mouse.GetPosition(Me)


            Dim X As Double = (Pos.X - 64)
            Dim Y As Double = (64 - Pos.Y)
            Dim Cap As Double = Atan2(X, Y) / PI * 180

            Console.WriteLine("X:" & X & ", Y " & Y & " Angle : " & Cap)
            NewCourse = Round(Cap, 1)

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

        _BearingDrag = True

    End Sub

    Private Sub EndBearingChangeDrag(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)

        _BearingDrag = False
    End Sub

End Class
