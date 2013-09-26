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
        For Each Point In (From P In _PointSet Order By P.CapFromPos)
            If NextPoint IsNot Nothing AndAlso Point.P.Lon <> NextPoint.P.Lon AndAlso Point.P.Lat <> NextPoint.P.Lat Then
                Continue For
            ElseIf NextPoint IsNot Nothing AndAlso Point.P.Lon = NextPoint.P.Lon AndAlso Point.P.Lat = NextPoint.P.Lat Then
                PrevPoint = Point
                Continue For
            End If

            If PrevPoint IsNot Nothing Then
                Dim tc As New TravelCalculator With {.StartPoint = PrevPoint.P, .EndPoint = Point.P}
                Dim TC2 As New TravelCalculator With {.StartPoint = Point.P}
                Dim MinDist As Double = Double.NaN
                Dim MinAngle As Double = Double.NaN
                Dim CurNextPoint As clsrouteinfopoints = Nothing
                Dim NewDist As Double = Double.NaN
                Dim NewAngle As Double = Double.NaN
                For Each Point2 In (From P In _PointSet Order By P.CapFromPos)
                    TC2.EndPoint = Point2.P
                    NewDist = TC2.SurfaceDistance
                    NewAngle = WindAngle(tc.LoxoCourse_Deg, TC2.LoxoCourse_Deg)
                    If NewDist = 0 Then
                        Continue For
                    End If
                    If Double.IsNaN(MinAngle) OrElse (MinAngle > NewAngle And MinDist > NewDist) Then
                        MinAngle = NewAngle
                        MinDist = TC2.SurfaceDistance
                        CurNextPoint = Point2
                    End If
                Next

                NextPoint = CurNextPoint
                _NextPointSet.AddLast(CurNextPoint)

            Else
                PrevPoint = Point
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
