﻿Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports System.IO

Public Class RouteManager

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Routes As New ObservableCollection(Of RecordedRoute)
    Private _FilterRaceID As String

    Public Sub AddNewRoute(ByVal RaceID As String, ByVal RaceName As String, ByVal Route As RoutePointInfo)

        Dim R As New RecordedRoute()

        R.RaceID = RaceID
        R.RaceName = RaceName
        R.Route = Route
        _Routes.Add(R)

    End Sub

    Public Shared Function Load() As RouteManager

        Dim RetValue As RouteManager = Nothing
        Dim XMLS As New Xml.Serialization.XmlSerializer(GetType(RouteManager))
        Try
            Dim fname As String = RouteurModel.BaseFileDir & "\Routes.xml"
            If System.IO.File.Exists(fname) Then
                Dim o As New FileStream(fname, FileMode.Open)
                RetValue = CType(XMLS.Deserialize(o), RouteManager)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to recover routes " & ex.Message)
        End Try

        If RetValue Is Nothing Then
            Return New RouteManager
        Else
            Return RetValue
        End If

    End Function

    Private Function FilterCourseRoute(ByVal o As Object) As Boolean

        If Not TypeOf o Is RecordedRoute Then
            Return True
        Else
            Return CType(o, RecordedRoute).RaceID = _FilterRaceID
        End If

    End Function

    Public Property FilterRaceID() As String
        Get
            Return _FilterRaceID
        End Get
        Set(ByVal value As String)
            _FilterRaceID = value
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("FilterRaceID"))
        End Set
    End Property

    Public ReadOnly Property RoutesInRace() As ICollectionView
        Get
            Dim lv As ICollectionView = CollectionViewSource.GetDefaultView(Routes)

            If FilterRaceID <> "" Then
                lv.Filter = AddressOf FilterCourseRoute
            Else
                lv.Filter = Nothing
            End If


            Return lv

        End Get

    End Property


    Public Property Routes() As ObservableCollection(Of RecordedRoute)
        Get
            Return _Routes
        End Get

        Set(ByVal value As ObservableCollection(Of RecordedRoute))
            _Routes = value

            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))
        End Set

    End Property

    Public Sub Save()

        Dim XMLS As New Xml.Serialization.XmlSerializer(Me.GetType)
        Try
            Dim o As New FileStream(RouteurModel.BaseFileDir & "\Routes.xml", FileMode.Create)
            XMLS.Serialize(o, Me)
        Catch ex As Exception
            MessageBox.Show("Failed to store routes " & ex.Message)
        End Try

    End Sub

    Public ReadOnly Property VisibleRoutes() As IList(Of RecordedRoute)
        Get

            Dim R = (From rt In _Routes Where rt.Visible).ToList
            Return R

        End Get
    End Property

End Class
