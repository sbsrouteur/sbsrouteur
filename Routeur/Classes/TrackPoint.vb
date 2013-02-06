Imports System.ComponentModel

Public Class TrackPoint

    Implements INotifyPropertyChanged


    Private _P As Coords
    Private _Epoch As Long

    Public Event PropertyChanged(sender As Object, e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Property Epoch As Long
        Get
            Return _Epoch
        End Get
        Set(value As Long)
            _Epoch = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Epoch"))
        End Set
    End Property

    Public Property P As Coords
        Get
            Return _P
        End Get
        Set(value As Coords)
            _P = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("P"))
        End Set
    End Property


End Class
