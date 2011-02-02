Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class MeteoLayer

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _MeteoInfoList As New ObservableCollection(Of ZoneMeteoInfo)
    Private _Meteo As clsMeteoOrganizer
    Private _ViewPort As _2D_Viewer

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
        _MeteoInfoList.Clear()
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

                _MeteoInfoList.Add(New ZoneMeteoInfo(_Meteo, CurDate, New Coords(lat, lon), New Coords(lat - DLat, lon + DLon), X, Y, (x1 - X), (Y - Y1)))
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

End Class
