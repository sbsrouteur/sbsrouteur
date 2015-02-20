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

Public Class VLMRaceInfo

    Public Enum RACE_TYPE
        Classic = 0
        Record = 1
    End Enum

    Private _idraces As String
    Private _RaceName As String
    Private _StartLon As Integer
    Private _StartLat As Integer
    Private _BoatType As String
    Private _StartDate As DateTime
    Private _race_waypoints As New List(Of VLM_RaceWaypoint)
    Private _vacfreq As Integer


    Public Property boattype() As String
        Get
            Return _BoatType
        End Get
        Set(ByVal value As String)
            _BoatType = value
        End Set
    End Property

    Public Property idraces() As String
        Get
            Return _idraces
        End Get
        Set(ByVal value As String)
            _idraces = value
        End Set
    End Property

    Public ReadOnly Property races_waypoints(ByVal index As Integer) As VLM_RaceWaypoint
        Get

            If index <= 0 Then
                Dim DestIndex As Integer = RouteurModel.CurWP - 1
                If DestIndex < 0 Then
                    DestIndex = 0
                ElseIf DestIndex >= _race_waypoints.Count Then
                    DestIndex = _race_waypoints.Count - 1
                End If
                Return _race_waypoints(DestIndex)
            ElseIf index <= _race_waypoints.Count Then
                Return _race_waypoints(index - 1)
            Else
                Return _race_waypoints(_race_waypoints.Count - 1)
            End If

        End Get

    End Property

    Public Property races_waypoints() As List(Of VLM_RaceWaypoint)
        Get
            Return _race_waypoints
        End Get
        Set(ByVal value As List(Of VLM_RaceWaypoint))
            _race_waypoints = value
        End Set
    End Property

    Public ReadOnly Property Route() As Coords()
        Get
            Dim R(_race_waypoints.Count - 1) As Coords
            Dim i As Integer
            Dim PrevPoint As Coords = Nothing
            For i = 0 To _race_waypoints.Count - 1
                If PrevPoint Is Nothing Then
                    R(i) = _race_waypoints(i).WPs(0)(0)

                Else
                    R(i) = GSHHS_Reader.PointToSegmentIntersect(PrevPoint, _race_waypoints(i).WPs(0)(0), _race_waypoints(i).WPs(0)(1))
                End If
                PrevPoint = R(i)
            Next

            Return R

        End Get
    End Property

    Public Property racename() As String
        Get
            Return _RaceName
        End Get
        Set(ByVal value As String)
            _RaceName = value
        End Set
    End Property

    Public Property racetype As RACE_TYPE

    Public Property deptime() As DateTime
        Get
            Return _StartDate
        End Get
        Set(ByVal value As DateTime)
            _StartDate = value
        End Set
    End Property

    Public ReadOnly Property Start() As Coords
        Get
            Return New Coords(startlat / 1000, startlong / 1000)
        End Get
    End Property

    Public Property startlat() As Integer
        Get
            Return _StartLat
        End Get
        Set(ByVal value As Integer)
            _StartLat = value
        End Set
    End Property

    Public Property startlong() As Integer
        Get
            Return _StartLon
        End Get
        Set(ByVal value As Integer)
            _StartLon = value
        End Set
    End Property

    Public Property vacfreq() As Integer
        Get
            Return _vacfreq
        End Get
        Set(ByVal value As Integer)
            _vacfreq = value
        End Set
    End Property

    ReadOnly Property RaceStarted As Boolean
        Get
            Return Now >= deptime
        End Get
    End Property

    Property NSZ As List(Of MapSegment)

End Class
