Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class MeteoLayer

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Const WIND_TILE_FOLDER As String = "WindTiles"

    Private _MeteoInfoList As New ObservableCollection(Of ZoneMeteoInfo)
    Private _Meteo As clsMeteoOrganizer
    Private _ViewPort As _2D_Viewer

    Private WithEvents _RefreshTimer As New Timers.Timer() With {.Interval = 0.5, .Enabled = False}

    Public Sub New(ByVal Meteo As clsMeteoOrganizer)
        _Meteo = Meteo

    End Sub

    Public Property MeteoInfoList() As ObservableCollection(Of ZoneMeteoInfo)
        Get
            Return _MeteoInfoList
        End Get
        Set(ByVal value As ObservableCollection(Of ZoneMeteoInfo))
            _MeteoInfoList = value
        End Set
    End Property

    Public Sub RefreshInfo(ByVal NWPoint As Coords, ByVal SEPoint As Coords, ByVal CurDate As Date)

        Dim i As Integer = 0
        Dim j As Integer = 0

        Static PrevNW As Coords = Nothing
        Static PrevSE As Coords = Nothing
        Static PrevDate As DateTime
        Dim NeedsRefresh As Boolean = False

        If Math.Abs(PrevDate.Subtract(CurDate).TotalHours) > 1 Then
            NeedsRefresh = True
            PrevDate = CurDate
        End If

        If CoordsComparer.Compare(PrevNW, NWPoint) <> 0 Then
            PrevNW = NWPoint
            NeedsRefresh = True
        End If

        If CoordsComparer.Compare(PrevSE, SEPoint) <> 0 Then
            PrevSE = SEPoint
            NeedsRefresh = True
        End If

        If Not NeedsRefresh Then
            Return
        End If

        For Each z In _MeteoInfoList
            z.Cancel = True
        Next

        'Console.Write("delete " & FList.Length & " in " & Now.Subtract(Start).ToString)
        _MeteoInfoList.Clear()
        System.Threading.ThreadPool.QueueUserWorkItem(AddressOf LasyFileManager)

        Dim DLon As Double = (SEPoint.Lon_Deg - NWPoint.Lon_Deg) / 10
        Dim DLat As Double = (NWPoint.Lat_Deg - SEPoint.Lat_Deg) / 10
        If ViewPort Is Nothing Then
            Return
        End If


        For lon = NWPoint.Lon_Deg To SEPoint.Lon_Deg Step DLon
            j = 0

            For lat = NWPoint.Lat_Deg To SEPoint.Lat_Deg Step -DLat

                Dim X As Double = ViewPort.LonToCanvas(lon)
                Dim x1 As Double = ViewPort.LonToCanvas(lon + DLon)
                Dim Y As Double = ViewPort.LatToCanvas(lat)
                Dim Y1 As Double = ViewPort.LatToCanvas(lat - DLat)
                Dim W As Double = (x1 - X)
                If DLon < 0 Then
                    W = -W
                End If

                _MeteoInfoList.Add(New ZoneMeteoInfo(_Meteo, CurDate, New Coords(lat, lon), New Coords(lat - DLat, lon + DLon), X, Y, W, (Y1 - Y)))
                j += 1
            Next
            i += 1
        Next

        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("MeteoInfoList"))

    End Sub
    Public Property ViewPort() As _2D_Viewer
        Get
            Return _ViewPort
        End Get
        Set(ByVal value As _2D_Viewer)
            _ViewPort = Value
        End Set
    End Property



    Private Sub LasyFileManager(ByVal state As Object)

        If Not System.IO.Directory.Exists(System.IO.Path.Combine(RouteurModel.BaseFileDir, WIND_TILE_FOLDER)) Then
            Return
        End If

        Dim Flist() As String = System.IO.Directory.GetFiles(System.IO.Path.Combine(RouteurModel.BaseFileDir, WIND_TILE_FOLDER))

        For Each File In Flist
            Try
                If Now.Subtract(System.IO.File.GetLastAccessTime(File)).TotalHours > 1 Then
                    System.IO.File.Delete(File)
                End If
            Catch ex As Exception
                'ignore, should ne deleted at next pass
            End Try
        Next

    End Sub

End Class
