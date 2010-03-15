﻿Imports System.ComponentModel

Public Class clsPlayerInfo

    Implements INotifyPropertyChanged


    Private _AutoRouting As Boolean
    Private _Nick As String
    Private _Email As String
    Private _Password As String
    Private _NumBoat As Integer
    Private _ShowAutorouting As Boolean = False
    Private _RouteWayPoints As New List(Of List(Of Coords()))


    Private _Route() As Coords

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged


    Public Property AutoRouting() As Boolean
        Get
            Return _AutoRouting
        End Get
        Set(ByVal value As Boolean)
            _AutoRouting = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("AutoRouting"))
        End Set
    End Property

    Public Property Email() As String
        Get
            Return _Email
        End Get
        Set(ByVal value As String)
            _Email = value
        End Set
    End Property

    Public Property Nick() As String
        Get
            Return _Nick
        End Get
        Set(ByVal value As String)
            _Nick = value
            
        End Set
    End Property

    Public Property NumBoat() As Integer
        Get
            Return _NumBoat
        End Get
        Set(ByVal value As Integer)
            _NumBoat = value
        End Set
    End Property

    Public Property Password() As String
        Get
            Return _Password
        End Get
        Set(ByVal value As String)
            _Password = value
        End Set
    End Property

    Public Property Route() As Coords()
        Get
            Return _Route
        End Get
        Set(ByVal value As Coords())
            _Route = value
        End Set
    End Property

    Public Property RouteWayPoints() As List(Of List(Of Coords()))
        Get
            Return _RouteWayPoints
        End Get
        Set(ByVal value As List(Of List(Of Coords())))
            _RouteWayPoints = value
        End Set
    End Property


    Public Property ShowAutorouting() As Boolean
        Get
            Return _ShowAutorouting
        End Get
        Set(ByVal value As Boolean)
            _ShowAutorouting = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("ShowAutorouting"))
        End Set

    End Property

End Class
