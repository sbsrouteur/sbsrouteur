Imports System.ComponentModel

Public MustInherit Class RoutePointValueBase
    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    MustOverride Overrides Function ToString() As String

    Protected Sub OnPropertyChanged(ByVal E As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, E)

    End Sub

End Class

Public Class RoutePointDoubleValue

    Inherits RoutePointValueBase

    Private _Value As Double

    Public Property Value() As Double
        Get
            Return _Value
        End Get
        Set(ByVal value As Double)
            _Value = value
            OnPropertyChanged(New PropertyChangedEventArgs("Value"))
        End Set
    End Property

    Public Sub New(ByVal Value As Double)
        _Value = Value
    End Sub

    Public Overrides Function ToString() As String
        Return Value.ToString("f1")
    End Function

End Class
