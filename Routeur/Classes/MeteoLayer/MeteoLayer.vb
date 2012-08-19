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
