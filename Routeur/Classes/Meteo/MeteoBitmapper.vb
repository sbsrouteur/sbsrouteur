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


                Debug.Assert(Thread.CurrentThread.ManagedThreadId = _Viewer.Dispatcher.Thread.ManagedThreadId)

                If _Img Is Nothing Then
                    _Img = New WriteableBitmap(CInt(W), CInt(H), 96, 96, PixelFormats.Pbgra32, Nothing)
                End If
                Using _Img.GetBitmapContext
                    _Img.WritePixels(R, _ImgData, ImgStride, 0)
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
        Dim Dir(99) As Double
        For x As Integer = 0 To CInt(_Viewer.ActualWidth - 1) Step Dx

            For y As Integer = 0 To CInt(_Viewer.ActualHeight - 1) Step Dy
                Dim V(3) As Double
                Dim MiFail As Boolean = False
                For i As Integer = 0 To 1
                    For j As Integer = 0 To 1
                        If _StopRender Then Return
                        Dim mi As MeteoInfo = _meteo.GetMeteoToDate(New Coords(_Viewer.CanvasToLat(y + j * Dy), _Viewer.CanvasToLon(x + i * Dx)), Me.Date, True)
                        If mi IsNot Nothing Then
                            V(i + 2 * j) = mi.Strength
                            Dim Index As Integer = CInt(10 * (y / Dy + j) + (x / Dx + i))
                            If Index < Dir.Count Then
                                Dir(Index) = (180 - mi.Dir) / 180 * PI
                            End If
                        Else
                            MiFail = True
                            Exit For
                        End If
                    Next
                Next
                If MiFail Then
                    Exit For
                End If

                For i = 0 To Dx - 1
                    Dim S1 As Double = i * (V(1) - V(0)) / (Dx - 1) + V(0)
                    Dim S2 As Double = i * (V(3) - V(2)) / (Dx - 1) + V(2)

                    For j = 0 To Dy - 1

                        If CInt((y + j) * _Viewer.ActualWidth + x + i) < _ImgData.Length Then
                            Dim S As Double = (S2 - S1) / (Dy - 1) * j + S1
                            If S > 70 Then
                                S = 70
                            End If
                            'Dim SIndex As Integer = CInt(Math.Round(S * 10))
                            'Points(BITMAP_SIZE * j + i) = V
                            'SyncLock WindColors.WindColorGDIBrushes(SIndex)
                            ' G.FillRectangle(WindColors.WindColorGDIBrushes(SIndex), New System.Drawing.RectangleF(CSng(i), CSng(j), 1.0, 1.0))
                            'End SyncLock
                            Dim C As Integer = WindColors.GetColor(S)
                            Try
                                _ImgData(CInt((y + j) * _Viewer.ActualWidth + x + i)) = C
                            Catch
                                'something when wrong, return
                                Return
                            End Try

                        End If

                    Next
                Next

            Next
        Next

        For i = 0 To 99
            Dim Row As Integer = Dy * CInt(Math.Floor(i / 10))
            Dim Col As Integer = Dx * (i Mod 10)
            For p = 0 To _Arrow.Count - 1 Step 2
                Dim d As Double = Sqrt(_Arrow(p) * _Arrow(p) + _Arrow(p + 1) * _Arrow(p + 1))
#Const DBG_ARROWS = False
#If DBG_ARROWS Then
                Dim a As Double = Atan2(_Arrow(p + 1), _Arrow(p)) + 3.6 * i / 180 * Math.PI
                
#Else
                Dim a As Double = Atan2(_Arrow(p + 1), _Arrow(p)) + Dir(i)
                
#End If
                Dim Px As Integer = CInt(d * Cos(a))
                Dim Py As Integer = CInt(d * Sin(a))
                Dim index As Integer = CInt((Row - Py) * _Viewer.ActualWidth + Col + Px)
                If index > 0 And index < _ImgData.Count Then
                    _ImgData(index) = &HFF000000
                End If
            Next

        Next

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
        Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))
        Try
            ReDim _ImgData(CInt(Math.Ceiling(_Viewer.ActualWidth)) * CInt(Math.Ceiling(_Viewer.ActualHeight)))
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
        Finally
            _RenderThread = Nothing
        End Try
    End Sub




End Class
