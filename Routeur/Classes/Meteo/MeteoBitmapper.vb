Imports System.ComponentModel
Imports System.Threading
Imports System.Windows.Threading

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

    Public Sub New(Meteo As clsMeteoOrganizer, Viewer As _2D_Viewer)
        _meteo = Meteo
        _Viewer = Viewer
    End Sub

    Public ReadOnly Property Image As WriteableBitmap
        Get

            'If _Img Is Nothing OrElse _Img.Width <> CInt(_Viewer.ActualWidth) OrElse _Img.Height <> CInt(_Viewer.ActualHeight) Then '
            '    If Thread.CurrentThread.ManagedThreadId = _Viewer.Dispatcher.Thread.ManagedThreadId Then
            '        _Img = New WriteableBitmap(CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight), 96, 96, PixelFormats.Bgr32, Nothing)
            '        StartImgRender()
            '    End If
            'Else
            '    'If _ImgData IsNot Nothing Then
            Dim ImgStride As Integer = CInt(_Viewer.ActualWidth * ((PixelFormats.Bgr32.BitsPerPixel) / 8))
            Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))

            '    '    'If _Img.IsFrozen Then
            '    '    _Img = New WriteableBitmap(CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight), 96, 96, PixelFormats.Bgr32, Nothing)
            '    '    'End If
            '    '    _Img.WritePixels(R, _ImgData, ImgStride, 0)
            '    '    _Img.Freeze()
            '    'Else
            '    '    StartImgRender()
            '    '    'End If
            'End If
            Debug.Assert(Thread.CurrentThread.ManagedThreadId = _Viewer.Dispatcher.Thread.ManagedThreadId)
            '_Img = New WriteableBitmap(CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight), 96, 96, PixelFormats.Bgr32, Nothing)
            '_Img.Lock()
            If _Img IsNot Nothing Then
                _Img.WritePixels(R, _ImgData, ImgStride, 0)
            End If
            '_Img.Unlock()

            ImageReady = False
            Return _Img
        End Get
    End Property

    Public Property ImageReady As Boolean = False

    Private Sub GetImageData()
        Dim R As New Int32Rect(0, 0, CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight))
        Dim Dx As Integer = CInt(_Viewer.ActualWidth / 10)
        Dim Dy As Integer = CInt(_Viewer.ActualHeight / 10)
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
                            Dim C As Color = WindColors.GetColor(S)
                            Try
                                _ImgData(CInt((y + j) * _Viewer.ActualWidth + x + i)) = (255 << 24) + (CInt(C.R) << 16) + (CInt(C.G) << 8) + C.B
                            Catch
                                'something when wrong, return
                                Return
                            End Try

                        End If

                    Next
                Next


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
        _Img = New WriteableBitmap(CInt(_Viewer.ActualWidth), CInt(_Viewer.ActualHeight), 96, 96, PixelFormats.Bgr32, Nothing)
        ReDim _ImgData(CInt(_Viewer.ActualWidth) * CInt(_Viewer.ActualHeight))
        '_Img.CopyPixels(R, _ImgData, 4 * CInt(_Viewer.ActualWidth), 0)
        _RenderThread = New Thread(AddressOf ImGRenderThread)
        _StopRender = False
        _RenderThread.Start()
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
