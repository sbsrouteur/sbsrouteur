﻿Imports System.ComponentModel

Public MustInherit Class RoutePointViewBase

    Inherits DataTemplateSelector

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
    Private _ID As Integer
    Private _UserID As Integer


    Public Property ActionDate() As DateTime
        Get
            Return _ActionDate
        End Get
        Set(ByVal value As DateTime)
            _ActionDate = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ActionDate"))
        End Set
    End Property

    Public Sub Delete()

        If ID = 0 Then
            Return
        End If

        'Create the delete query
        Dim Data As New Dictionary(Of String, Object)
        Data.Add("taskid", ID)
        Data.Add("idu", UserID)
        Data.Add("debug", False)


        WS_Wrapper.PostBoatSetup("pilototo_delete", GetStringFromJsonObject(Data))

    End Sub

    Public ReadOnly Property EnumRouteModes() As String()
        Get

            Return [Enum].GetNames(GetType(EnumRouteMode))

        End Get
    End Property

    Public Property ID() As Integer
        Get
            Return _ID
        End Get
        Set(ByVal value As Integer)
            _ID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ID"))
        End Set
    End Property
    Public Property IsPending() As Boolean
        Get
            Return _Pending
        End Get
        Set(ByVal value As Boolean)
            _Pending = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsPending"))
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

    Public Overrides Function SelectTemplate(ByVal item As Object, ByVal container As System.Windows.DependencyObject) As System.Windows.DataTemplate

        Dim element As FrameworkElement
        element = TryCast(container, FrameworkElement)

        If element IsNot Nothing Then
            If TypeOf item Is RouteBearingPointView OrElse TypeOf item Is RouteAnglePointView Then
                Return DirectCast(element.FindResource("DoubleValueTemplate"), DataTemplate)
                'ElseIf Typeof item Then

            End If
        End If

        Return MyBase.SelectTemplate(item, container)

    End Function
    Public Property UserID() As Integer
        Get
            Return _UserID
        End Get
        Set(ByVal value As Integer)
            _UserID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("UserID"))
        End Set
    End Property
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

    Public Sub New()
        RouteMode = EnumRouteMode.Bearing
    End Sub
    
    Public Sub New(ByVal UserID As Integer, ByVal ID As Integer, ByVal ActionDate As DateTime, ByVal Bearing As RoutePointDoubleValue)

        Me.ActionDate = ActionDate
        RouteValue = Bearing
        Me.RouteMode = EnumRouteMode.Bearing
        Me.ID = ID
        Me.userId = UserID
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

    Public Sub New()
        RouteMode = EnumRouteMode.Angle
    End Sub

    Public Sub New(ByVal UserID As Integer, ByVal PointID As Integer, ByVal ActionDate As DateTime, ByVal Bearing As RoutePointDoubleValue)

        Me.ActionDate = ActionDate
        Me.RouteMode = EnumRouteMode.Angle
        RouteValue = Bearing
        Me.ID = PointID
        Me.userid = UserID
    End Sub
End Class