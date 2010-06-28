Imports System.ComponentModel

Public MustInherit Class RoutePointViewBase

    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Enum EnumRouteMode As Integer

        Bearing = 1
        Angle = 2
        Ortho = 3
        VMG = 4
        BVMG = 5

    End Enum

    Private _RouteMode As EnumRouteMode
    Private _ActionDate As DateTime
    Private _Pending As Boolean


    Public Property ActionDate() As DateTime
        Get
            Return _ActionDate
        End Get
        Set(ByVal value As DateTime)
            _ActionDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ActionDate"))
        End Set
    End Property

    Public Property Pending() As Boolean
        Get
            Return _Pending
        End Get
        Set(ByVal value As Boolean)
            _Pending = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Pending"))
        End Set
    End Property

    Public Property RouteMode() As EnumRouteMode
        Get
            Return _RouteMode
        End Get
        Set(ByVal value As EnumRouteMode)
            _RouteMode = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("RouteMode"))

        End Set
    End Property

    Public MustOverride Property RouteValue() As RoutePointValueBase

    Protected Sub OnPropertyChanged(ByVal e As PropertyChangedEventArgs)

        RaiseEvent PropertyChanged(Me, e)

    End Sub

End Class


Public Class RouteBearingPointView
    Inherits RoutePointViewBase

    Private _Value As RoutePointDoubleValue

    Public Overrides Property RouteValue() As RoutePointValueBase
        Get
            Return _Value
        End Get

        Set(ByVal value As RoutePointValueBase)
            If Not TypeOf value Is RoutePointDoubleValue Then
                Throw New InvalidCastException("Can set value to double")
            End If

            _Value = CType(value, RoutePointDoubleValue)
            OnPropertyChanged(New PropertyChangedEventArgs("RouteValue"))
        End Set

    End Property

    Public Sub New(ByVal ActionDate As DateTime, ByVal Bearing As RoutePointDoubleValue)

        Me.ActionDate = ActionDate
        RouteValue = Bearing

    End Sub

End Class

Public Class RouteAnglePointView

    Inherits RoutePointViewBase

    Private _Value As RoutePointDoubleValue

    Public Overrides Property RouteValue() As RoutePointValueBase
        Get
            Return _Value
        End Get

        Set(ByVal value As RoutePointValueBase)
            If Not TypeOf value Is RoutePointDoubleValue Then
                Throw New InvalidCastException("Can set value to double")
            End If

            _Value = CType(value, RoutePointDoubleValue)
            OnPropertyChanged(New PropertyChangedEventArgs("RouteValue"))
        End Set

    End Property

    Public Sub New(ByVal ActionDate As DateTime, ByVal Bearing As RoutePointDoubleValue)

        Me.ActionDate = ActionDate
        RouteValue = Bearing

    End Sub
End Class