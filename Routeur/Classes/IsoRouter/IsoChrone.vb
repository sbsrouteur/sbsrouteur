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
Imports System.Collections.Concurrent

Public Class IsoChrone

    'Private _Data() As clsrouteinfopoints

    Private _PointSet As New ConcurrentBag(Of clsrouteinfopoints)

#If DBG_ISO_POINT_SET Then
    Private _RawPointSet As New LinkedList(Of clsrouteinfopoints)
#End If
    'Private Const NB_MAX_INDEX_ISO_LUT = 36
    Private _Drawn As Boolean = False
    Private _IsoChroneLock As New Object
    Private _SailManager As clsSailManager
    Private _StartPoint As Coords
    'Private _Iso_IndexLut(NB_MAX_INDEX_ISO_LUT) As Integer

    Private _AngleStep As Double

    Public Sub New(ByVal Manager As clsSailManager, StartPoint As Coords)
        _SailManager = Manager
        _StartPoint = StartPoint

        'For i As Integer = 0 To NB_MAX_INDEX_ISO_LUT
        '    _Iso_IndexLut(i) = -1
        'Next

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

    ReadOnly Property PointSet As ConcurrentBag(Of clsrouteinfopoints)
        Get
            Return _PointSet
        End Get

    End Property

    Function MaxIndex() As Integer
        Return _PointSet.Count
    End Function

    Sub AddPoint(P1 As clsrouteinfopoints)
        'SyncLock (_IsoChroneLock)
        _PointSet.Add(P1)
        'End SyncLock
    End Sub



    Sub CleanUp(ParentIso As IsoChrone, DoNotCleanUp As Boolean, AngleStep As Double)

        Dim IsoMaxCount As Integer = CInt(360 / AngleStep)
        Dim PrevPoint As clsrouteinfopoints = Nothing
        Dim NextPoint As clsrouteinfopoints = Nothing
        Dim Count As Integer = 0
        Dim Ignored As Integer = 0
        Dim WorkSet As New ConcurrentBag(Of clsrouteinfopoints)(PointSet)
        
        'Dim coordslist As New List(Of DotSpatial.Topology.Coordinate)
        Dim Angles(WorkSet.Count - 1) As Double
        Dim Dists(WorkSet.Count - 1) As Double
        Dim FilteredDist(WorkSet.Count - 1) As Double
        Dim TrimFactor As Double = 0.8
        Dim PrevAngle As Double = -10000
        If WorkSet.Count = 0 Then
            Dim i As Integer = 0
        End If
        WorkSet = New ConcurrentBag(Of clsrouteinfopoints)(From P In WorkSet Order By P.CapFromPos)
        Dim Start As DateTime = Now
        Dim StartCount As Integer = WorkSet.Count
        Dim EndCount As Integer
        Dim PointIndex As Integer = 0
        Dim LoopCount As Integer = 0


#If DBG_ISO_POINT_SET Then
        For Each P In WorkSet
            _RawPointSet.AddLast(P)
        Next
#End If
        'Dim Complete As Boolean = False
        'Dim Tmpworkset As New LinkedList(Of clsrouteinfopoints)

        'coordslist.Clear()
        'For Each Point In ParentIso.PointSet
        '    coordslist.Add(New Topology.Coordinate(Point.P.Lon, Point.P.Lat))
        'Next
        'Dim LR As New Topology.LinearRing(coordslist)
        'Dim Poly As New Topology.Polygon(LR)
        'For Each Point In WorkSet
        '    Dim P As New Topology.Coordinate(Point.P.Lon, Point.P.Lat)
        '    Dim Pt As New Topology.Point(P)
        '    If Not Poly.Intersects(Pt) Then
        '        Tmpworkset.AddLast(Point)
        '    End If

        'Next

        'WorkSet = Tmpworkset
        'Tmpworkset = Nothing

        'Dim RemoveList As New LinkedList(Of Topology.IPoint)
        ''While Not Complete

        'For i = 0 To LR.NumPoints - 1
        '    Dim P As Topology.IPoint = LR.GetPointN(i)

        '    Dim LR2 As Topology.LineString = CType(LR.Difference(P), Topology.LineString)

        '    If LR2.Intersects(P) Then
        '        LR = New Topology.LinearRing(LR2)
        '        RemoveList.AddLast(P)
        '    End If

        'Next
        ''End While

        If Not DoNotCleanUp Then


            Dim _NextPointSet As New LinkedList(Of clsrouteinfopoints)

            Dim PrevCount As Integer = 0
            Do
                PrevCount = _NextPointSet.Count
                _NextPointSet.Clear()
                Dim Index As Integer = 0
                Dim MaxDists(IsoMaxCount - 1) As Double

                For Each Point In ParentIso.PointSet
                    PointIndex = CInt(Point.CapFromPos * IsoMaxCount / 360) Mod IsoMaxCount
                    MaxDists(PointIndex) = Math.Max(Point.DistFromPos, MaxDists(PointIndex))
                Next
                For Each Point In WorkSet
                    If Not Double.IsNaN(Point.CapFromPos) Then

                        PointIndex = CInt(Point.CapFromPos * IsoMaxCount / 360) Mod IsoMaxCount
                        MaxDists(PointIndex) = Math.Max(Point.DistFromPos, MaxDists(PointIndex))
                    End If
                    Index += 1

                Next

                Console.WriteLine("CleanUp half loop in " & Now.Subtract(Start).TotalMilliseconds & " with " & LoopCount & " Loops")



                Index = 0

                For Each Point In WorkSet '(From Pt In WorkSet Order By Pt.CapFromPos)
                    'If Not Double.IsNaN(Point.CapFromPos) AndAlso (Point.CapFromPos <> PrevAngle AndAlso MaxDists(CInt(Point.CapFromPos * IsoMaxCount / 360) Mod IsoMaxCount) = Point.DistFromPos) Then
                    If Not Double.IsNaN(Point.CapFromPos) AndAlso (MaxDists(CInt(Point.CapFromPos * IsoMaxCount / 360) Mod IsoMaxCount) = Point.DistFromPos) Then
                        _NextPointSet.AddLast(Point)
                        'PrevAngle = Point.CapFromPos
                    End If
                    Index += 1
                Next
                WorkSet = New ConcurrentBag(Of clsrouteinfopoints)(_NextPointSet)
                ReDim Dists(WorkSet.Count - 1)
                ReDim FilteredDist(WorkSet.Count - 1)
                LoopCount += 1
            Loop Until _NextPointSet.Count <= IsoMaxCount Or _NextPointSet.Count = PrevCount
            _PointSet = New ConcurrentBag(Of clsrouteinfopoints)(_NextPointSet)

        Else
            _PointSet = WorkSet

        End If
        ''Create IndexLut for IsoChrone
        '_Iso_IndexLut(0) = 0
        'For Each Point In _PointSet

        '    Dim Index As Integer = CInt(Point.CapFromPos * NB_MAX_INDEX_ISO_LUT / 360)
        '    If _Iso_IndexLut(Index) = -1 Then
        '        _Iso_IndexLut(Index) = PointIndex

        '    End If
        '    PointIndex += 1
        'Next
        EndCount = _PointSet.Count
        Console.WriteLine("CleanUp from " & StartCount & " to " & EndCount & " in " & Now.Subtract(Start).TotalMilliseconds & " with " & LoopCount & " Loops")
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

        If Loxo < 0 Then
            Loxo = (Loxo + 3600) Mod 360
        End If

        Return IndexFromAngleDycho(PointSet, Loxo, 0, _PointSet.Count)

        'Dim StartIndex As Integer = _Iso_IndexLut(CInt(Loxo / 10)) - 1

        ''If StartIndex < 0 Then
        'StartIndex = 0
        ''End If

        'Dim PointIndex As Integer
        'Dim AngleError As Double = Double.MaxValue
        'Try

        '    For PointIndex = StartIndex To _PointSet.Count - 1
        '        Dim LocalError As Double = Math.Abs(Loxo - _PointSet(PointIndex).CapFromPos)
        '        If LocalError <= AngleError Then
        '            AngleError = LocalError
        '        Else
        '            Return PointIndex - 1
        '        End If
        '    Next

        '    Return _PointSet.Count - 1
        'Finally

        'End Try

    End Function

    Private Function IndexFromAngleDycho(PointSet As ConcurrentBag(Of clsrouteinfopoints), Loxo As Double, p2 As Integer, p3 As Integer) As Integer

        If _PointSet.Count = 0 Then
            Return 0
        ElseIf (p3 - p2) = 0 Then
            Return p3 Mod 360
        ElseIf (p3 - p2) = 1 Then
            Dim Err1 As Double = Math.Abs(PointSet(p2).CapFromPos - Loxo)
            Dim Err2 As Double = Math.Abs(PointSet(p2).CapFromPos - Loxo)

            If Err1 <= Err2 Then
                Return p2
            ElseIf p3 = 360 Then
                Return 0
            Else
                Return p3
            End If
        Else
            Dim Middle As Integer = CInt((p3 + p2) / 2)
            If Loxo > PointSet(Middle).CapFromPos Then
                Return IndexFromAngleDycho(PointSet, Loxo, Middle, p3)
            Else
                Return IndexFromAngleDycho(PointSet, Loxo, p2, Middle)
            End If
        End If
    End Function



End Class
