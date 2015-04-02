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

Imports System.Collections.ObjectModel

Public Class PathInfo
    Public Property Path As LinkedList(Of Coords)
    Public Property Routes As IList(Of ObservableCollection(Of VLM_Router.clsrouteinfopoints))
    Public Property PilototoPoints As IList(Of Coords)
    Public Property EstimateRouteIndex As Integer
    Public Property WPRouteIndex As Integer
    Public Property Opponents As Dictionary(Of String, BoatInfo)
    Public Property ClearGrid As Boolean
    Public Property ClearBoats As Boolean
    Public Property IsoChrones As LinkedList(Of IsoChrone)
    Public Property WPs As List(Of VLM_RaceWaypoint)
    Public Property NSZ As List(Of MapSegment)
    Public Property ManagedRoutes As IList(Of RecordedRoute)
    Public Property RoutingBorder As LinkedList(Of Coords)
    Public Property CoastHighligts As LinkedList(Of MapSegment)
    Public Property CurrentPos As Coords

#If DBG_ISO_POINT_SET Then
    Public Property DbgIsoNumber As Integer = 0
#End If

    Property TrackColor As Color


End Class
