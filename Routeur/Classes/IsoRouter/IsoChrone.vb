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

Imports Routeur.VLM_Router
Imports System.Threading
Imports DotSpatial

Public Class IsoChrone

    'Private _Data() As clsrouteinfopoints
    Private _PointSet As New LinkedList(Of clsrouteinfopoints)
    Private _Drawn As Boolean = False
    Private _IsoChroneLock As New Object
    Private _SailManager As clsSailManager
    Private _StartPoint As Coords

    Private _AngleStep As Double

    Public Sub New(ByVal Manager As clsSailManager, StartPoint As Coords)
        _SailManager = Manager
        _StartPoint = StartPoint

    End Sub



    Public Property Drawn() As Boolean
        Get
            Return _Drawn
        End Get
        Set(ByVal value As Boolean)
            _Drawn = value
        End Set
    End Property

    ReadOnly Property PointSet As LinkedList(Of clsrouteinfopoints)
        Get
            Return _PointSet
        End Get

    End Property

    Function MaxIndex() As Integer
        Return _PointSet.Count
    End Function

    Sub AddPoint(P1 As clsrouteinfopoints)
        SyncLock (_IsoChroneLock)
            _PointSet.AddLast(P1)
        End SyncLock
    End Sub



    Sub CleanUp()

        Dim _NextPointSet As New LinkedList(Of clsrouteinfopoints)
        Dim PrevPoint As clsrouteinfopoints = Nothing
        Dim NextPoint As clsrouteinfopoints = Nothing
        Dim Count As Integer = 0
        Dim Ignored As Integer = 0
        Dim WorkSet As New LinkedList(Of clsrouteinfopoints)
        WorkSet = _PointSet
        Do
            Dim polygon As Topology.Polygon
            Dim coordslist As New List(Of DotSpatial.Topology.Coordinate)
            Ignored = 0
            Count = 1
            polygon = New Topology.Polygon(coordslist)
            coordslist.Add(New Topology.Coordinate(_StartPoint.Lat, _StartPoint.Lon))
            For Each Point In (From P In WorkSet Order By P.CapFromPos)

                If Count < 2 Then
                    _NextPointSet.AddLast(Point)
                    coordslist.Add(New Topology.Coordinate(Point.P.Lat, Point.P.Lon))
                    Count += 1
                Else

                    'If point is in hull, ignore it
                    If polygon.Contains(New Topology.Point(Point.P.Lat, Point.P.Lon)) Then
                        Ignored += 1
                    Else
                        _NextPointSet.AddLast(Point)
                        coordslist.Add(New Topology.Coordinate(Point.P.Lat, Point.P.Lon))
                        Count += 1
                    End If
                End If

            Next
            WorkSet.Clear()
            WorkSet = _NextPointSet
        Loop Until Ignored = 0
        _PointSet.Clear()
        _PointSet = _NextPointSet
        Return
    End Sub

    Private Function CheckPointIsBehind(StartPt As clsrouteinfopoints, EndPt As clsrouteinfopoints) As Boolean
        Dim Tc As New TravelCalculator With {.StartPoint = StartPt.P, .EndPoint = EndPt.P}
        Dim WD As Double = WindAngle(Tc.LoxoCourse_Deg, StartPt.CapFromPos)
        Dim MinAngle As Double
        Dim MaxAngle As Double
        _SailManager.GetCornerAngles(StartPt.WindStrength, MinAngle, MaxAngle)
        If WD > MaxAngle Or WD < MinAngle Then
            Return True
        Else
            Return False
        End If
    End Function



End Class
