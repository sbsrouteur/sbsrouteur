Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports System.IO
Imports System.Text
Imports System.Xml.Serialization

Public Class RouteManager

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Routes As New ObservableCollection(Of RecordedRoute)
    Private _FilterRaceID As String
    Private Shared _M As RouteurModel

    Public Sub AddNewRoute(ByVal RaceID As String, ByVal RaceName As String, ByVal Route As RoutePointInfo)

        Dim R As New RecordedRoute()

        R.RaceID = RaceID
        R.RaceName = RaceName
        If R.Route Is Nothing Then
            R.Route = New ObservableCollection(Of RoutePointView)
        End If
        Dim PrevPos As Coords = Nothing
        Dim TC As New TravelCalculator

        For Each P In Route.Route
            If Not PrevPos Is Nothing Then
                Dim NewPoint As New RoutePointView
                With NewPoint
                    TC.EndPoint = P.P
                    .ActionDate = P.T
                    .IsPending = False
                    .RouteValue = New RoutePointDoubleValue(TC.LoxoCourse_Deg)
                    .P = PrevPos
                    .RouteMode = RoutePointView.EnumRouteMode.Bearing
                End With
                R.Route.Add(NewPoint)
            End If
            PrevPos = P.P
            TC.StartPoint = PrevPos
        Next
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        R.RouteName = Route.RouteName
        'R.Route = Route
        R.Model = _M
        _Routes.Add(R)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Routes"))

    End Sub

    Public Sub Load()

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

        If RetValue IsNot Nothing Then
            Routes = RetValue.Routes

            For Each R In Routes
                R.Model = _M
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

    Public Sub New()

    End Sub

    Public Sub New(ByVal M As RouteurModel)
        _M = M
    End Sub

End Class
