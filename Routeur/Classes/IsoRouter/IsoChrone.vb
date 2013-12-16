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

#If DBG_ISO_POINT_SET Then
    Private _RawPointSet As New LinkedList(Of clsrouteinfopoints)
#End If

    Private _Drawn As Boolean = False
    Private _IsoChroneLock As New Object
    Private _SailManager As clsSailManager
    Private _StartPoint As Coords
    Private _Iso_IndexLut(35) As Integer

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

#If DBG_ISO_POINT_SET Then
    ReadOnly Property RawPointSet As LinkedList(Of clsrouteinfopoints)
        Get
            Return _RawPointSet
        End Get
    End Property
#End If

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

        Dim PrevPoint As clsrouteinfopoints = Nothing
        Dim NextPoint As clsrouteinfopoints = Nothing
        Dim Count As Integer = 0
        Dim Ignored As Integer = 0
        Dim WorkSet As New LinkedList(Of clsrouteinfopoints)
        WorkSet = New LinkedList(Of clsrouteinfopoints)(_PointSet)

        Dim coordslist As New List(Of DotSpatial.Topology.Coordinate)
        Dim Angles(WorkSet.Count - 1) As Double
        Dim Dists(WorkSet.Count - 1) As Double
        Dim FilteredDist(WorkSet.Count - 1) As Double
        Dim TrimFactor As Double = 0.8
        WorkSet = New LinkedList(Of clsrouteinfopoints)(From P In WorkSet Order By P.CapFromPos)

#If DBG_ISO_POINT_SET Then
        For Each P In WorkSet
            _RawPointSet.AddLast(P)
        Next
#End If

            Dim _NextPointSet As New LinkedList(Of clsrouteinfopoints)
            Dim PrevCount As Integer = 0
            Do
                PrevCount = _NextPointSet.Count
                _NextPointSet.Clear()
                Dim Index As Integer = 0

                'Using sr As New IO.StreamWriter("c:\temp\dists.csv")
                For Each Point In WorkSet
                    Dists(Index) = Point.DistFromPos
                    Angles(Index) = Point.CapFromPos
                    Index += 1

                Next


                Dim factor As Double

                For Index = 0 To Dists.Length - 1
                    FilteredDist(Index) = 0
                    factor = 0
                    Dim PrevD As Double = 0
                    Dim NextD As Double = 0
                    Dim MaxD As Double = 0

                    For i As Integer = -2 To 2
                        FilteredDist(Index) += Dists((Index + i + Dists.Length) Mod Dists.Length) * (2 ^ (2 - Math.Abs(i)))
                        factor += (2 ^ (2 - Math.Abs(i)))
                    Next
                    FilteredDist(Index) /= factor
                    'sr.WriteLine(Angles(Index).ToString & ";" & Dists(Index).ToString & ";" & FilteredDist(Index).ToString)
                Next
                'sr.Close()
                'End Using

                Index = 0
                For Each Point In WorkSet
                    If (Math.Abs(Angles(Index) - Angles((Index + 1) Mod (Angles.Count - 1))) > 1) OrElse (FilteredDist(Index) <> 0 AndAlso Dists(Index) > FilteredDist(Index)) Then
                        _NextPointSet.AddLast(Point)
                    End If
                    Index += 1
                Next
                WorkSet = New LinkedList(Of clsrouteinfopoints)(_NextPointSet)
                ReDim Dists(WorkSet.Count - 1)
                ReDim FilteredDist(WorkSet.Count - 1)
            Loop Until _NextPointSet.Count <= 200 Or _NextPointSet.Count = PrevCount
            _PointSet = New LinkedList(Of clsrouteinfopoints)(_NextPointSet)

            'Create IndexLut for IsoChrone
            Dim CurIndex As Integer = 0
            Dim PointIndex As Integer = 0
            _Iso_IndexLut(0) = 0
            Dim Tc As New TravelCalculator With {.StartPoint = _StartPoint}
            For Each Point In _PointSet
                Tc.EndPoint = Point.P
                If Tc.LoxoCourse_Deg >= 10 * CurIndex Then
                    _Iso_IndexLut(CurIndex) = PointIndex
                    CurIndex += 1
                End If
                PointIndex += 1
            Next
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

    Function IndexFromAngle(Loxo As Double) As Integer

        Dim StartIndex As Integer = CInt(Loxo / 10)

        If CDbl(StartIndex) = Loxo / 10 Then
            Return StartIndex
        End If

        Dim PointIndex As Integer
        Dim AngleError As Double = Double.MaxValue
        Dim Tc As New TravelCalculator
        Try
            Tc.StartPoint = _StartPoint

            For PointIndex = StartIndex To _PointSet.Count - 1
                Tc.EndPoint = _PointSet(PointIndex).P
                If Math.Abs(Loxo - Tc.LoxoCourse_Deg) < AngleError Then
                    AngleError = Math.Abs(Loxo - Tc.LoxoCourse_Deg)
                Else
                    Return PointIndex
                End If
            Next

            Return 0
        Finally
            Tc.StartPoint = Nothing
            Tc.EndPoint = Nothing
        End Try

    End Function



End Class
