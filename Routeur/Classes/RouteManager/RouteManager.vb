'This file is part of Routeur.
'Copyright (C) 2010-2013  sbsRouteur(at)free.fr

'Routeur is free software: you can redistribute it and/or modify
'it under the terms of the GNU General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.

'Routeur is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License for more details.

'You should have received a copy of the GNU General Public License
'along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Text
Imports System.Xml.Serialization
Imports System.Windows.Threading

Public Class RouteManager

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Routes As New ObservableCollection(Of RecordedRoute)
    Private _FilterRaceID As String
    Private Shared _M As RouteurModel
    Private _CurPos As Coords
    Private _Meteo As GribManager
    Private _Boat As String
    Private _Sails As clsSailManager
    Private _Dispatcher As Dispatcher

    Public Function AddNewRoute(ByVal RaceID As String, ByVal RaceName As String, ByVal RouteName As String) As RecordedRoute

        If Dispatcher IsNot Nothing AndAlso Dispatcher.Thread.ManagedThreadId <> System.Threading.Thread.CurrentThread.ManagedThreadId Then

            Dim dlg As New Func(Of String, String, String, RecordedRoute)(AddressOf AddNewRoute)

            Return CType(Dispatcher.Invoke(dlg, New Object() {RaceID, RaceName, RouteName}), RecordedRoute)

        End If

        Dim R As New RecordedRoute()

        R.RaceID = RaceID
        R.RaceName = RaceName
        If R.Route Is Nothing Then
            R.Route = New ObservableCollection(Of RoutePointView)
        End If
        R.RouteName = RouteName
        'R.Route = Route
        R.Model = _M

        _Routes.Add(R)


        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))
        Return R

    End Function


    Public Sub AddNewRoute(ByVal RaceID As String, ByVal RaceName As String, ByVal Route As RoutePointInfo)

        Dim R As RecordedRoute = AddNewRoute(RaceID, RaceName, Route.RouteName)

        If R.RouteName = VLM_Router.KEY_ROUTE_THIS_POINT Then
            R.RouteName = Now.Day & " " & Now.Hour & "h"
        End If
        Dim PrevP As RoutePointView = Nothing
        Dim TC As New TravelCalculator

        For Each P In Route.Route
            Dim NewPoint As New RoutePointView
            With NewPoint
                TC.EndPoint = P.P
                .ActionDate = P.T
                .ID = -1

                If PrevP IsNot Nothing Then
                    PrevP.RouteValue = New RoutePointDoubleValue(TC.LoxoCourse_Deg)
                End If
                .P = P.P
                .RouteMode = RoutePointView.EnumRouteMode.Bearing
                .IsPending = P.T > Now
            End With
            R.Route.Add(NewPoint)
            PrevP = NewPoint
            TC.StartPoint = P.P
        Next
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing

        Save()
    End Sub

    Public Property Boat() As String
        Get
            Return _Boat
        End Get
        Set(ByVal value As String)
            _Boat = value
        End Set
    End Property

    Public WriteOnly Property Curpos As Coords
        Set(ByVal value As Coords)
            _CurPos = value
        End Set
    End Property

    Public Sub DeleteRoute(ByVal R As RecordedRoute)

        If _Routes.Contains(R) AndAlso MessageBox.Show("Are you sure you want to delete the current route : " & R.RouteName, "Delete Recorded Route", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) = MessageBoxResult.Yes Then
            _Routes.Remove(R)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))
        End If

    End Sub

    Public Sub DuplicateRoute(ByVal R As RecordedRoute)

        Dim R2 As New RecordedRoute
        With R2
            .RouteName = R.RouteName & "(copied " & Now.ToString & ")"
            .RaceID = R.RaceID
            .Route = New ObservableCollection(Of RoutePointView)
            For Each Point In R.Route
                .Route.Add(New RoutePointView(Point))
            Next
            .Initialize()
        End With
        _Routes.Add(R2)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))
    End Sub

    Public Sub Load()

        Dim RetValue As RouteManager = Nothing
        Try
            Dim XMLS As New Xml.Serialization.XmlSerializer(GetType(RouteManager))
            Dim fname As String = RouteurModel.BaseFileDir & "\Routes.xml"
            If System.IO.File.Exists(fname) Then
                Dim o As New FileStream(fname, FileMode.Open)
                RetValue = CType(XMLS.Deserialize(o), RouteManager)
            End If
        Catch ex As Exception
            MessageBox.Show("Failed to recover routes " & ex.Message)
        End Try

        If RetValue IsNot Nothing Then
            Routes = RetValue.Routes

            For Each R In Routes
                R.Model = _M
                R.Initialize()
            Next
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))

        End If

    End Sub



    Private Function FilterCourseRoute(ByVal o As Object) As Boolean

        If Not TypeOf o Is RecordedRoute Then
            Return True
        Else
            Return CType(o, RecordedRoute).RaceID = _FilterRaceID
        End If

    End Function

    Public Property Dispatcher() As Dispatcher
        Get
            Return _Dispatcher
        End Get
        Set(ByVal value As Dispatcher)
            _Dispatcher = Value
        End Set
    End Property
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

    Public Sub RecomputeRoute(ByVal R As RecordedRoute)

        R.RecomputeRoute(_CurPos, _Meteo, Boat, _Sails)

    End Sub

    Public Sub Rescale()
        For Each R In Routes
            RaiseEvent PropertyChanged(R, New PropertyChangedEventArgs("Shape"))
        Next
    End Sub

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
            Using o As New FileStream(RouteurModel.BaseFileDir & "\Routes.xml", FileMode.Create)
                XMLS.Serialize(o, Me)
            End Using
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

    Public Sub New()

    End Sub

    Public Sub New(ByVal M As RouteurModel, ByVal Meteo As GribManager, ByVal Sails As clsSailManager)
        _M = M
        _Meteo = Meteo
        _Sails = Sails

    End Sub

End Class
