Imports System.ComponentModel

Public Class RouterPrefs

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _TrackBackAngle As Double
    Private _ForwardAngle As Double
    Private _UseCustomWP As Boolean
    Private _CustWP As Coords

    Public Property CustWP() As Coords
        Get
            Return _CustWP
        End Get
        Set(ByVal value As Coords)
            _CustWP = value
        End Set
    End Property

    Public Property ForwardAngle() As Double
        Get
            Return _ForwardAngle
        End Get
        Set(ByVal value As Double)
            _ForwardAngle = value
        End Set
    End Property

    Public Property TrackBackAngle() As Double
        Get
            Return _TrackBackAngle
        End Get
        Set(ByVal value As Double)
            _TrackBackAngle = value
        End Set
    End Property

    Public Property UseCustomWP() As Boolean
        Get
            Return _UseCustomWP
        End Get
        Set(ByVal value As Boolean)
            _UseCustomWP = value
        End Set
    End Property

End Class
