Imports System.ComponentModel
Imports System.Collections.ObjectModel

Public Class RecordedRoute

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Key As Guid
    Private _RaceID As String
    Private _RaceName As String 
    Private _Route As RoutePointInfo


    Public Sub New()
        _Key = New Guid
    End Sub

    Public Property RaceID() As String
        Get
            Return _RaceID
        End Get
        Set(ByVal value As String)
            _RaceID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceID"))
        End Set
    End Property

    Public Property RaceName() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RaceName"))
        End Set
    End Property

    Public Property Route() As RoutePointInfo
        Get
            Return _Route
        End Get
        Set(ByVal value As RoutePointInfo)
            _Route = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Route"))
        End Set
    End Property

End Class
