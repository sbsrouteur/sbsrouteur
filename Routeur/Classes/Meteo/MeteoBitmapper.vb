Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Threading
Imports System.Math
Imports System.Drawing
Imports System.Collections.ObjectModel
Imports Routeur.VLM_Router

Public Class MeteoBitmapper

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _meteo As GribManager = Nothing
    Private _Img As WriteableBitmap = Nothing
    Private _Date As DateTime = Now
    Private _Viewer As _2D_Viewer
    Private _StopRender As Boolean = False
    Private _RenderThread As Thread
    Private _ImgData() As Integer
    Private _renderlock As New Object
    Private Const NbX As Integer = 20
    Private Const NbY As Integer = 20

    Private _MeteoCache(12 * 24 * 15)() As MeteoData
    Private _RefreshTimer As DispatcherTimer
    Private _CurImgIndex As Integer = 0
    Private _MaxImageIndex As Integer = 0
    Private _MeteoFont As New Font(SystemFonts.DefaultFont.OriginalFontName, 6, FontStyle.Regular)

    Private Class MeteoData
        Property Dir As Double
        Property Strength As Double = -1
    End Class

    Public Property Route As ObservableCollection(Of clsrouteinfopoints)


    Private _Arrow() As Integer = New Integer() {0, 4,
                                                 0, 3,
                                                 -1, 2, 0, 2, 1, 2,
                                                 -1, 1, 0, 1, 1, 1,
                                                 -1, 0, 0, 0, 1, 0,
                                                 -1, -1, 0, -1, 1, -1,
                                                 -2, -2, -1, -2, 0, -2, 1, -2, 2, -2,
                                                 -2, -3, -1, -3, 1, -3, 2, -3,
                                                 -2, -4, 2, -4
                                                 }

    Public Sub New(Meteo As GribManager, Viewer As _2D_Viewer, D As Dispatcher)
        _meteo = Meteo
        _Viewer = Viewer
        _RefreshTimer = New DispatcherTimer(New TimeSpan(0, 0, 0, 0, 30), DispatcherPriority.ApplicationIdle, AddressOf OnBitmapTimerRefresh, D)
    End Sub


    Public ReadOnly Property Image As WriteableBitmap
        Get
            Dim Start As DateTime = Now
            Try
                Dim W As Integer = CInt(Math.Ceiling(_Viewer.ActualWidth))
                Dim H As Integer = CInt(Math.Ceiling(_Viewer.ActualHeight))
                Dim ImgStride As Integer = CInt(W * ((PixelFormats.Pbgra32.BitsPerPixel) / 8))
                Dim R As New Int32Rect(0, 0, CInt(W), CInt(H))
                Dim CPt(3) As Point
                CPt(0) = New Point(0, -20)
                CPt(1) = New Point(-5, 5)
                CPt(2) = New Point(0, 0)
                CPt(3) = New Point(5, 5)
                Dim d(3) As Double

                For n As Integer = 0 To 3
                    d(n) = Sqrt(CPt(n).X * CPt(n).X + CPt(n).Y * CPt(n).Y)
                Next
                Debug.Assert(Thread.CurrentThread.ManagedThreadId = _Viewer.Dispatcher.Thread.ManagedThreadId)

                If _Img Is Nothing OrElse _Img.Width <> W OrElse _Img.Height <> H Then
                    _Img = New WriteableBitmap(W, H, 96, 96, PixelFormats.Pbgra32, Nothing)
                End If

                Using _Img.GetBitmapContext
                    '_Img.WritePixels(R, _ImgData, ImgStride, 0)
                    Dim ImgIndex As Integer = _CurImgIndex
                    _Img.Clear()
                    _Img.Lock()
                    Using bmp As New System.Drawing.Bitmap(_Img.PixelWidth, _Img.PixelHeight,
                     _Img.BackBufferStride, System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                         _Img.BackBuffer)
                        Using g As Graphics = System.Drawing.Graphics.FromImage(bmp)
                            g.TextRenderingHint = Text.TextRenderingHint.AntiAliasGridFit
                            For x As Integer = 0 To NbX - 1
                                For y As Integer = 0 To NbY - 1

                                    If _MeteoCache(ImgIndex) IsNot Nothing AndAlso _MeteoCache(ImgIndex)(x + NbX * y) IsNot Nothing AndAlso _MeteoCache(ImgIndex)(x + NbX * y).Strength <> -1 Then
                                        Dim pt(3) As Point
                                        Dim Scale As Double = Math.Log(_MeteoCache(ImgIndex)(x + NbX * y).Strength + 1)
                                        Dim WindColor As Integer = WindColors.GetColor(_MeteoCache(ImgIndex)(x + NbX * y).Strength)

                                        For n As Integer = 0 To 3
                                            'Scale Arrow
                                            pt(n) = New Point(CInt(CPt(n).X * Scale), CInt(CPt(n).Y * Scale))

                                            'Rotate
                                            Dim a As Double = Atan2(pt(n).Y, pt(n).X) - _MeteoCache(ImgIndex)(x + NbX * y).Dir
                                            pt(n).X = CInt(d(n) * Cos(a))
                                            pt(n).Y = CInt(d(n) * Sin(a))

                                            'Offset
                                            pt(n).X = CInt(pt(n).X + (W / (NbX - 1) * x))
                                            pt(n).Y = CInt(pt(n).Y + (H / (NbY - 1) * y))
                                        Next

                                        Using br As New System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(WindColor))
                                            g.FillPolygon(br, pt)
                                        End Using
                                        Dim TxtPt As New Point(CInt((W / (NbX - 1) * x)), CInt(H / (NbY - 1) * y + 20))
                                        Dim DString As String = (-(_MeteoCache(ImgIndex)(x + NbX * y).Dir / Math.PI * 180 - 180)).ToString("0°")
                                        g.DrawString(_MeteoCache(ImgIndex)(x + NbX * y).Strength.ToString("0.0"), _MeteoFont, Brushes.Black, TxtPt)
                                        TxtPt = New Point(CInt((W / (NbX - 1) * x)) - 20, CInt(H / (NbY - 1) * y + 20))
                                        g.DrawString(DString, _MeteoFont, Brushes.Black, TxtPt)

                                    End If

                                Next
                            Next

                            If Route IsNot Nothing Then
                                For Each RoutePoint In Route
                                    If Abs(Now.AddMinutes(5 * ImgIndex).Subtract(RoutePoint.T).TotalMinutes) < 5 Then
                                        Dim Pt1 As New Point
                                        Pt1.X = CInt(_Viewer.LonToCanvas(RoutePoint.P.N_Lon_Deg)) - 2
                                        Pt1.Y = CInt(_Viewer.LatToCanvas(RoutePoint.P.Lat_Deg)) - 2
                                        g.DrawEllipse(Pens.Red, Pt1.X, Pt1.Y, 4, 4)
                                        Exit For
                                    End If
                                Next
                            End If
                        End Using
                    End Using

                    _Img.Unlock()

                End Using

            Catch ex As Exception
                Dim i As Integer = 0
            End Try

            'ImageReady = False
            Console.WriteLine("ImageRender in " & Now.Subtract(Start).TotalMilliseconds)
            Return _Img
        End Get
    End Property

    Public Property ImageReady As Boolean = False

    Private Sub GetImageData(MeteoIndex As Integer)

        Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))
        Dim Dx As Integer = CInt(_Viewer.ActualWidth / 10)
        Dim Dy As Integer = CInt(_Viewer.ActualHeight / 10)
        Dim Start As DateTime = Now
        Dim MeteoCount As Integer = 0
        Dim MeteoMs As Double = 0
        Dim Ix As Integer = 0
        Dim Iy As Integer = 0

        If _MeteoCache(MeteoIndex) Is Nothing Then
            ReDim _MeteoCache(MeteoIndex)(NbX * NbY - 1)
        End If
        For Ix = 0 To NbX - 1
            Dim X As Integer = CInt(Ix * _Viewer.ActualWidth / NbX)

            For Iy = 0 To NbY - 1
                Dim Y As Integer = CInt(Iy * _Viewer.ActualHeight / NbY)
                Dim MiFail As Boolean = False

                If _StopRender Then Return
                Dim StartMeteo As DateTime = Now
                Dim mi As MeteoInfo = _meteo.GetMeteoToDate(New Coords(_Viewer.CanvasToLat(Y), _Viewer.CanvasToLon(X)), Me.Date.AddMinutes(5 * MeteoIndex), True)
                If _MeteoCache(MeteoIndex)(Ix + NbX * Iy) Is Nothing Then
                    _MeteoCache(MeteoIndex)(Ix + NbX * Iy) = New MeteoData
                End If

                If mi IsNot Nothing Then
                    _MeteoCache(MeteoIndex)(Ix + NbX * Iy).Strength = mi.Strength
                    _MeteoCache(MeteoIndex)(Ix + NbX * Iy).Dir = (180 - mi.Dir) / 180 * PI

                Else
                    _MeteoCache(MeteoIndex)(Ix + NbX * Iy).Strength = -1

                End If
                If _StopRender Then
                    Return
                End If
                MeteoCount += 1
                MeteoMs += Now.Subtract(StartMeteo).TotalMilliseconds

            Next

        Next
        'Console.WriteLine("Meteo avg" & MeteoMs / MeteoCount)


        'Console.WriteLine("Meteo duration" & Now.Subtract(Start).TotalSeconds)
    End Sub

    Public Property [Date] As DateTime
        Get
            Return _Date
        End Get
        Set(value As DateTime)
            _Date = value
            onpropertychanged("Date")
            StartImgRender()
        End Set
    End Property

    Public Property DateTicks As Long
        Get
            Return Me.Date.Ticks
        End Get
        Set(value As Long)
            Me.Date = New DateTime(value)
        End Set
    End Property


    Private Sub onpropertychanged(Prop As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(Prop))
    End Sub

    Public Sub StartImgRender()

        'Debug.Assert(Thread.CurrentThread.ManagedThreadId = _Viewer.Dispatcher.Thread.ManagedThreadId)
        SyncLock _renderlock
            If _RenderThread IsNot Nothing Then
                _StopRender = True
                Try
                    If Not _RenderThread.Join(100) Then
                        _RenderThread.Abort()
                    End If
                Catch
                    'ignore this one thread may be gone by itself actually
                End Try

                _RenderThread = Nothing
            End If
        End SyncLock
        Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))
        Try
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Image"))
            ReDim _ImgData(CInt(Math.Ceiling(_Viewer.ActualWidth)) * CInt(Math.Ceiling(_Viewer.ActualHeight)) - 1)
            '_Img.CopyPixels(R, _ImgData, 4 * CInt(_Viewer.ActualWidth), 0)
            _RenderThread = New Thread(AddressOf ImGRenderThread)

            _StopRender = False
            _RenderThread.Start()
        Catch
        End Try

    End Sub

    Private Sub ImGRenderThread()
        Try
            _MaxImageIndex = 0
            For i = 0 To _MeteoCache.Length - 1
                If _StopRender Then
                    Exit For
                End If
                GetImageData(i)
                _MaxImageIndex = i
            Next

        Finally
            'SyncLock _renderlock
            '_RenderThread = Nothing
            'End SyncLock

        End Try
    End Sub

    Private Sub OnBitmapTimerRefresh(Sender As Object, e As EventArgs)


        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Image"))
        _CurImgIndex += 1
        If _CurImgIndex >= _MaxImageIndex Then
            _CurImgIndex = 0
        End If
    End Sub




End Class
