Imports System.ComponentModel

Public Class ZoneMeteoInfo

    Implements INotifyPropertyChanged

    Public Enum ZoneMeteoInfoIndex As Integer
        Center = 0
        NorthWest = 1
        NorthEast = 2
        SouthWest = 3
        SouthEast = 4
    End Enum

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _WindColor(4) As Color
    Private _ZoneDate As DateTime
    Private _MeteoProvider As clsMeteoOrganizer
    Private _NWPoint As Coords
    Private _SEPoint As Coords
    Private _Left As Double
    Private _Top As Double
    Private _Width As Double
    Private _Height As Double
    Private _WindImage(3) As ImageSource
    Private _ImgAddr(3) As String
    Private _WindDir As Double
    Private _WindStr As Double
    Dim _VS(8) As Double


    Public Sub New(ByVal Meteo As clsMeteoOrganizer, ByVal Dte As DateTime, ByVal NWPoint As Coords, ByVal SEPoint As Coords, ByVal l As Double, ByVal t As Double, ByVal w As Double, ByVal h As Double)
        _ZoneDate = Dte
        _MeteoProvider = Meteo
        _NWPoint = NWPoint
        _SEPoint = SEPoint
        _Left = l
        _Top = t
        Width = w
        Height = h
        System.Threading.ThreadPool.QueueUserWorkItem(AddressOf UpdateVisual)
    End Sub


    Private Shared Function LoadImage(ByVal v00 As Double, ByVal v01 As Double, ByVal v10 As Double, ByVal V11 As Double) As ImageSource

        Dim ImgPath As String = IO.Path.Combine(RouteurModel.BaseFileDir, "WindTiles")
        Dim ImgName As String = "B" & CInt(v00).ToString("00") & CInt(v01).ToString("00") & CInt(v10).ToString("00") & CInt(V11).ToString("00") & ".jpg"
        Dim FullPath As String = IO.Path.Combine(ImgPath, ImgName)
        Const BITMAP_SIZE As Integer = 20

        If Not System.IO.Directory.Exists(ImgPath) Then
            System.IO.Directory.CreateDirectory(ImgPath)
        End If

        If IO.File.Exists(FullPath) Then
            Return New BitmapImage(New Uri(FullPath))
        Else

            Dim Img2 As New System.Drawing.Bitmap(BITMAP_SIZE, BITMAP_SIZE)
            Using G As System.Drawing.Graphics = System.Drawing.Graphics.FromImage(Img2)
                'Dim Points(BITMAP_SIZE * BITMAP_SIZE) As Int32

                For i = 0 To BITMAP_SIZE - 1
                    Dim S1 As Double = i * (v01 - v00) / (BITMAP_SIZE - 1) + v00
                    Dim S2 As Double = i * (V11 - v10) / (BITMAP_SIZE - 1) + v10
                    For j = 0 To BITMAP_SIZE - 1
                        Dim S As Double = (S2 - S1) / (BITMAP_SIZE - 1) * j + S1
                        Dim C As Color = WindColors.GetColor(CInt(S))
                        Dim V As Int32 = C.B << 24 + C.G << 16 + C.R << 8 + &HFF
                        'Points(BITMAP_SIZE * j + i) = V
                        SyncLock WindColors.WindColorGDIBrushes(CInt(S))
                            G.FillRectangle(WindColors.WindColorGDIBrushes(CInt(S)), New System.Drawing.RectangleF(CSng(i), CSng(j), 1.0, 1.0))
                        End SyncLock
                    Next
                Next

            End Using
            Dim BSaved As Boolean = False
            Try
                If Not System.IO.File.Exists(FullPath) Then
                    Img2.Save(FullPath)
                    BSaved = True
                Else
                    BSaved = True
                End If

            Catch
            End Try

            Return New BitmapImage(New Uri(FullPath))
        End If

    End Function

    Public Property Height() As Double
        Get
            Return _Height
        End Get
        Set(ByVal value As Double)
            If _Height <> value Then
                _Height = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Height"))
            End If
        End Set
    End Property

    Public ReadOnly Property Height2() As Double
        Get
            Return _Height
        End Get
    End Property

    Public Property Left() As Double
        Get
            Return _Left
        End Get
        Set(ByVal value As Double)
            _Left = value
        End Set
    End Property

    Public Property Left2() As Double
        Get
            Return _Left + Width
        End Get
        Set(ByVal value As Double)
            _Left = value
        End Set
    End Property


    Public ReadOnly Property NWPoint As Coords
        Get
            Return _NWPoint
        End Get
    End Property

    Public ReadOnly Property Right As Double
        Get
            Return Left + Width
        End Get
    End Property

    Public ReadOnly Property SEPoint As Coords
        Get
            Return _SEPoint
        End Get
    End Property


    Public Property Top() As Double
        Get
            Return _Top
        End Get
        Set(ByVal value As Double)
            _Top = Value
        End Set
    End Property

    Public Property Top2() As Double
        Get
            Return _Top + Height
        End Get
        Set(ByVal value As Double)
            _Top = value
        End Set
    End Property

    Public Property Width() As Double
        Get
            Return _Width
        End Get
        Set(ByVal value As Double)
            If _Width <> value Then
                _Width = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Width"))
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Width2"))
            End If
        End Set
    End Property

    Public ReadOnly Property Width2() As Double
        Get
            Return _Width / 2
        End Get
        
    End Property

    Public Property WindColor() As Color()
        Get
            Return _WindColor
        End Get
        Set(ByVal value As Color())
            _WindColor = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindColor"))
        End Set
    End Property

    Private Sub UpdateVisual(ByVal state As Object)

        Dim X1 As Double = _NWPoint.Lon_Deg
        Dim X2 As Double = _SEPoint.Lon_Deg
        Dim Y1 As Double = _NWPoint.Lat_Deg
        Dim Y2 As Double = _SEPoint.Lat_Deg
        Dim Dx As Double = (X2 - X1) / 2
        Dim Dy As Double = (Y2 - Y1) / 2


        Dim start As DateTime = Now

        System.Threading.Thread.Sleep(CInt(Rnd() * 100))
        For i = 0 To 2
            For j = 0 To 2
                Dim mi As MeteoInfo = _MeteoProvider.GetMeteoToDate(New Coords(Y1 + j * Dy, X1 + i * Dx), _ZoneDate, False, False)
                If mi Is Nothing Then
                    Return
                End If
                If i = 1 And j = 1 Then
                    WindDir = mi.Dir
                    WindStr = mi.Strength
                End If
                _VS(i + 3 * j) = mi.Strength
            Next
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindImage_0"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindImage_1"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindImage_2"))
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindImage_3"))
        Console.WriteLine("Visual updated in " & Now.Subtract(start).ToString)
    End Sub

    Public Property WindDir() As Double
        Get
            Return _WindDir
        End Get
        Set(ByVal value As Double)

            If _WindDir <> value Then
                _WindDir = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindDir"))
            End If
        End Set
    End Property

    Public ReadOnly Property WindImage_0 As ImageSource
        Get
            Dim i As Integer = 0
            Dim j As Integer = 0
            Return LoadImage(_VS(0), _VS(1), _VS(3), _VS(4))
        End Get
    End Property

    Public ReadOnly Property WindImage_1 As ImageSource
        Get
            Dim i As Integer = 1
            Dim j As Integer = 0
            Return LoadImage(_VS(1), _VS(2), _VS(4), _VS(5))
        End Get
    End Property

    Public ReadOnly Property WindImage_2 As ImageSource
        Get
            Dim i As Integer = 0
            Dim j As Integer = 1
            Return LoadImage(_VS(3), _VS(4), _VS(6), _VS(7))
        End Get
    End Property

    Public ReadOnly Property WindImage_3 As ImageSource
        Get
            Dim i As Integer = 1
            Dim j As Integer = 1
            Return LoadImage(_VS(4), _VS(5), _VS(7), _VS(8))
        End Get
    End Property

    Public Property WindStr() As Double
        Get
            Return _WindStr
        End Get
        Set(ByVal value As Double)
            If _WindStr <> value Then
                _WindStr = value
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("WindStr"))
            End If
        End Set
    End Property




End Class
