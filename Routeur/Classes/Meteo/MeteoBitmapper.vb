﻿Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Threading
Imports System.Math

Public Class MeteoBitmapper

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _meteo As clsMeteoOrganizer = Nothing
    Private _Img As WriteableBitmap = Nothing
    Private _Date As DateTime = Now
    Private _Viewer As _2D_Viewer
    Private _StopRender As Boolean = False
    Private _RenderThread As Thread
    Private _ImgData() As Integer
    Private _renderlock As New Object
    Private Const NbX As Integer = 20
    Private Const NbY As Integer = 20
    Dim WindDir(NbX * NbY - 1) As Double
    Dim WindSpeed(NbX * NbY - 1) As Double


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

    Public Sub New(Meteo As clsMeteoOrganizer, Viewer As _2D_Viewer)
        _meteo = Meteo
        _Viewer = Viewer
    End Sub

    Public ReadOnly Property Image As WriteableBitmap
        Get

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
                    _Img.Clear()
                    For x As Integer = 0 To NbX - 1
                        For y As Integer = 0 To NbY - 1
                            Dim pt(3) As Point
                            Dim Scale As Double = Math.Log(WindSpeed(x + NbX * y) + 1)
                            Dim WindColor As Integer = WindColors.GetColor(WindSpeed(x + NbX * y))

                            For n As Integer = 0 To 3
                                'Scale Arrow
                                pt(n) = New Point(CPt(n).X * Scale, CPt(n).Y * Scale)

                                'Rotate
                                Dim a As Double = Atan2(pt(n).Y, pt(n).X) - WindDir(x + NbX * y)
                                pt(n).X = d(n) * Cos(a)
                                pt(n).Y = d(n) * Sin(a)

                                'Offset
                                pt(n).X += W / (NbX - 1) * x
                                pt(n).Y += H / (NbY - 1) * y
                            Next

                            _Img.FillQuad(CInt(pt(0).X), CInt(pt(0).Y), CInt(pt(1).X), CInt(pt(1).Y), CInt(pt(2).X), CInt(pt(2).Y), CInt(pt(3).X), CInt(pt(3).Y), WindColor)

                            'Dim Txt As New TextBlock
                            'Txt.Text = WindSpeed(x + NbX * y).ToString("0Kts") & "/" & (WindDir(x + NbX * y) / PI * 180).ToString("0.0°")
                            'Txt.Foreground = New SolidColorBrush(Color.FromRgb(0, 0, 0))
                            'Txt.FontSize = 7
                            'Dim tbmp As New WriteableBitmap(40, 10, 96, 96, PixelFormats.Pbgra32, Nothing)
                            'tbmp.render()
                        Next
                    Next
                End Using

            Catch ex As Exception
                Dim i As Integer = 0
            End Try

            ImageReady = False

            Return _Img
        End Get
    End Property

    Public Property ImageReady As Boolean = False

    Private Sub GetImageData()
        Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))
        Dim Dx As Integer = CInt(_Viewer.ActualWidth / 10)
        Dim Dy As Integer = CInt(_Viewer.ActualHeight / 10)
        Dim Start As DateTime = Now
        Dim MeteoCount As Integer = 0
        Dim MeteoMs As Double = 0
        Dim Ix As Integer = 0
        Dim Iy As Integer = 0
        For Ix = 0 To NbX - 1
            Dim X As Integer = CInt(Ix * _Viewer.ActualWidth / NbX)

            For Iy = 0 To NbY - 1
                Dim Y As Integer = CInt(Iy * _Viewer.ActualHeight / NbY)
                Dim MiFail As Boolean = False

                If _StopRender Then Return
                Dim StartMeteo As DateTime = Now
                Dim mi As MeteoInfo = _meteo.GetMeteoToDate(New Coords(_Viewer.CanvasToLat(Y), _Viewer.CanvasToLon(X)), Me.Date, True)
                If mi IsNot Nothing Then
                    WindSpeed(CInt(Ix + NbX * Iy)) = mi.Strength
                    Dim Index As Integer = NbX * Iy + Ix
                    If Index < WindDir.Count Then
                        WindDir(Index) = (180 - mi.Dir) / 180 * PI
                    End If
                Else
                    MiFail = True
                    Exit For
                End If
                If _StopRender Then
                    Return
                End If
                MeteoCount += 1
                MeteoMs += Now.Subtract(StartMeteo).TotalMilliseconds

            Next

        Next
        Console.WriteLine("Meteo avg" & MeteoMs / MeteoCount)

        'For x = 0 To Dx - 1
        '    Dim S1 As Double = i * (V(1) - V(0)) / (Dx - 1) + V(0)
        '    Dim S2 As Double = i * (V(3) - V(2)) / (Dx - 1) + V(2)

        '    For j = 0 To Dy - 1

        '        If CInt((y + j) * _Viewer.ActualWidth + x + i) < _ImgData.Length Then
        '            Dim S As Double = (S2 - S1) / (Dy - 1) * j + S1
        '            If S > 70 Then
        '                S = 70
        '            End If
        '            'Dim SIndex As Integer = CInt(Math.Round(S * 10))
        '            'Points(BITMAP_SIZE * j + i) = V
        '            'SyncLock WindColors.WindColorGDIBrushes(SIndex)
        '            ' G.FillRectangle(WindColors.WindColorGDIBrushes(SIndex), New System.Drawing.RectangleF(CSng(i), CSng(j), 1.0, 1.0))
        '            'End SyncLock
        '            Dim C As Integer = WindColors.GetColor(S)
        '            Try
        '                _ImgData(CInt((y + j) * _Viewer.ActualWidth + x + i)) = C
        '            Catch
        '                'something when wrong, return
        '                Return
        '            End Try

        '        End If
        '        If _StopRender Then
        '            Return
        '        End If
        '    Next
        'Next


        Console.WriteLine("Meteo duration" & Now.Subtract(Start).TotalSeconds)
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
            GetImageData()
            ImageReady = True
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Image"))
        Finally
            'SyncLock _renderlock
            '_RenderThread = Nothing
            'End SyncLock

        End Try
    End Sub




End Class
