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
        For Each Point In (From P In _PointSet Order By P.CapFromPos)
            Discard = False
RestartLoop:
            For Each Point2 In _NextPointSet
                If Point2.CapFromPos < Point.CapFromPos Then
                    PrevPoint = Point2
                End If
                If Math.Abs(Point2.CapFromPos - Point.CapFromPos) < 1 Then
                    If Point2.DistFromPos > Point.DistFromPos Then
                        Discard = CheckPointIsBehind(Point2, Point)
                        If Discard Then
                            Exit For
                        End If
                    Else
                        Discard = CheckPointIsBehind(Point, Point2)
                        If Discard Then
                            _NextPointSet.Remove(Point2)
                            GoTo RestartLoop
                        End If
                    End If
                    
                End If
            Next

            If Not Discard Then
                If PrevPoint Is Nothing Then
                    _NextPointSet.AddLast(Point)
                Else
                    _NextPointSet.AddAfter(_NextPointSet.Find(PrevPoint), Point)

                End If
            End If

        Next
        Dim i As Integer = 0
    End Sub

    Private Function CheckPointIsBehind(StartPt As clsrouteinfopoints, EndPt As clsrouteinfopoints) As Boolean
        Dim Tc As New TravelCalculator With {.StartPoint = StartPt.P, .EndPoint = EndPt.P}
        Dim WD As Double = WindAngle(Tc.LoxoCourse_Deg, StartPt.WindDir)
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
