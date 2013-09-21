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

Public Class IsoChrone

    'Private _Data() As clsrouteinfopoints
    Private _PointSet As New LinkedList(Of clsrouteinfopoints)
    Private _Drawn As Boolean = False
    Private _IsoChroneLock As New Object
    Private _SailManager As clsSailManager


    Private _AngleStep As Double

    Public Sub New(ByVal Manager As clsSailManager)
        _SailManager = Manager
        
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
        Dim Discard As Boolean
        Dim PrevPoint As clsrouteinfopoints = Nothing
        Dim MinDist As Double = 1000000.0

        For Each Point In _PointSet
            If Point.DistFromPos < MinDist Then
                MinDist = Point.DistFromPos
            End If
        Next

        Dim MinAngle As Double = 1 / CLng(MinDist)
        If MinAngle < 1 Then
            MinAngle = 1
        End If

        Dim Poly As New List(Of DotSpatial.Topology.Coordinate)
        For Each Point In (From P In _PointSet Order By P.CapFromPos)
            Poly.Add(New DotSpatial.Topology.Coordinate(Point.P.Lon_Deg, Point.P.Lat_Deg))
        Next

        Dim Polygon As New DotSpatial.Topology.Polygon(Poly)
        Dim G As DotSpatial.Topology.IGeometry = Polygon.al
        Debug.Print(CStr(G.Coordinates.Count))

        For Each coord In G.Coordinates
            Dim P = (From pt In _PointSet Where pt.P.Lon_Deg = coord.X And pt.P.Lat_Deg = coord.Y).FirstOrDefault
            If P IsNot Nothing Then
                _NextPointSet.AddLast(P)
            End If
        Next

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
