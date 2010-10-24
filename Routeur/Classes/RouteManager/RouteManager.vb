Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports System.IO

Public Class RouteManager

    Implements INotifyPropertyChanged

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private _Routes As New ObservableCollection(Of RecordedRoute)

    Public Sub AddNewRoute(ByVal RaceID As String, ByVal RaceName As String, ByVal Route As RoutePointInfo)

        Dim R As New RecordedRoute()

        R.RaceID = RaceID
        R.RaceName = RaceName
        R.Route = Route
        _Routes.Add(R)

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
            Dim o As New FileStream(RouteurModel.BaseFileDir & "Routes.xml", FileMode.CreateNew)
            XMLS.Serialize(o, Me)
        Catch ex As Exception
            MessageBox.Show("Failed to store routes " & ex.Message)
        End Try

    End Sub

End Class
