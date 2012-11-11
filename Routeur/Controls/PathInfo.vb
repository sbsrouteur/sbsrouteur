Imports System.Collections.ObjectModel

Public Class PathInfo
    Public Property Path As List(Of Coords)
    Public Property Routes As IList(Of ObservableCollection(Of VLM_Router.clsrouteinfopoints))
    Public Property EstimateRouteIndex As Integer
    Public Property Opponents As Dictionary(Of String, BoatInfo)
    Public Property ClearGrid As Boolean
    Public Property ClearBoats As Boolean
    Public Property IsoChrones As LinkedList(Of IsoChrone)
    Public Property WPs As List(Of VLM_RaceWaypoint)
    Public Property ManagedRoutes As IList(Of RecordedRoute)

    Property TrackColor As Color

End Class
