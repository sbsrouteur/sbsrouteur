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

Imports System.Math
Imports System.Threading

Public Class BspRect
    Public Const GRID_GRAIN_OVERSAMPLE As Integer = 1
    Private Const MAX_IGNORED_COUNT As Integer = 2000
    Private Const MAX_TREE_Z As Integer = 10

    Public Enum inlandstate As Byte
        Unknown = 0
        InLand = 1
        InSea = 2
        Mixed = 3
    End Enum


    Private Shared MIN_POLYGON_SPLIT As Double = 0.01
    Private Shared _NoPolyGons As New LinkedList(Of Polygon)
#If BSP_STATS Then
    Public Shared BspCount As Long = 0
    Private Shared _HitTestCount As Long
    Private Shared _HitTestDepth As Long
#End If

    Private Shared GridGrainDepth As Integer = -1

    Shared Event Log(ByVal Msg As String)
    'Private Shared CollectionCount As Long

    Private _p1 As Coords
    Private _p2 As Coords
    Private _MidLat As Double
    Private _MidLon As Double
    Private _SubRects(3) As BspRect
    Private _Inland As inlandstate
    Private _PolyGons As LinkedList(Of Polygon)
    Private _Z As Integer
    Private _Segments As List(Of MapSegment)
    Private _lockObj As New Object

    Private Const SPIN_COUNT = 100
    Private _SpinLock(SPIN_COUNT - 1) As SpinLock
    Private _SpinCount As Integer = 0

    Private _SegmentsArray(,) As List(Of MapSegment)

    'Shared Optimizer As New bspOptimizer

    Public Sub New()

        ReDim _SegmentsArray(CInt(2 ^ MAX_TREE_Z), CInt(2 ^ MAX_TREE_Z))
        For i As Integer = 0 To SPIN_COUNT - 1
            _SpinLock(i) = New SpinLock
        Next
    End Sub


    Public Sub New(ByVal P1 As Coords, ByVal P2 As Coords, ByVal Z As Integer)

        ReDim _SegmentsArray(CInt(2 ^ MAX_TREE_Z), CInt(2 ^ MAX_TREE_Z))

        If P1.Lat < -Math.PI OrElse P1.Lat > Math.PI _
            OrElse P2.Lat < -Math.PI OrElse P2.Lat > Math.PI Then
            Dim i As Integer = 0
        End If
        _Z = Z
        _p1 = P1
        _p2 = P2
        For i As Integer = 0 To 3
            _SubRects(i) = Nothing
        Next
        _Inland = inlandstate.Unknown
        _MidLat = _p2.Lat + (_p1.Lat - _p2.Lat) / 2
        _MidLon = _p2.Lon + (_p1.Lon - _p2.Lon) / 2

        'If 16 * RouteurModel.GridGrain < MIN_POLYGON_SPLIT Then
        '    MIN_POLYGON_SPLIT = 16 * RouteurModel.GridGrain
        'Else
        '    MIN_POLYGON_SPLIT = RouteurModel.GridGrain
        'End If

    End Sub

    Public ReadOnly Property GetSegments(Center As Coords, db As DBWrapper) As List(Of MapSegment)
        Get
            Static HitCount As Long = 1
            Static CacheHitCount As Long = 0
            Static CumDuration As Long = 0
            Dim start As DateTime = Now

            If Double.IsNaN(Center.N_Lon_Deg) OrElse Double.IsNaN(Center.N_Lat_Deg) Then
                Return Nothing
            End If
            Try
                Dim X As Integer = CInt(Math.Floor((Center.N_Lon_Deg + 180) / (360 / 2 ^ MAX_TREE_Z)))
                Dim Y As Integer = CInt(Math.Floor((Center.N_Lat / PI * 180 + 90) / (180 / 2 ^ MAX_TREE_Z)))

                If _SegmentsArray(X, Y) IsNot Nothing Then
                    CacheHitCount += 1
                    Return _SegmentsArray(X, Y)
                Else
                    SpinLock(X, Y)
                    Try
                        If _SegmentsArray(X, Y) Is Nothing Then

                            Dim TmpSegments = New List(Of MapSegment)
                            Dim P1 As New Coords(Y * 180 / (2 ^ MAX_TREE_Z) - 90, X * 360 / (2 ^ MAX_TREE_Z) - 180)
                            Dim P2 As New Coords((Y + 1) * 180 / (2 ^ MAX_TREE_Z) - 90, (X + 1) * 360 / (2 ^ MAX_TREE_Z) - 180)

                            Dim Segs = db.SegmentList(P1.N_Lon_Deg, P1.Lat_Deg, P2.N_Lon_Deg, P2.Lat_Deg)
                            If Segs IsNot Nothing Then
                                TmpSegments.AddRange(Segs)
                                _SegmentsArray(X, Y) = TmpSegments
                            End If

                        End If
                    Finally
                        SpinLockExit(X, Y)
                    End Try
                End If
                Return _SegmentsArray(X, Y)
            Finally
                CumDuration += Now.Subtract(start).Ticks

                Routeur.Stats.SetStatValue(Stats.StatID.BSP_GetSegAvgMS) = CumDuration / HitCount / TimeSpan.TicksPerMillisecond
                Routeur.Stats.SetStatValue(Stats.StatID.BSP_GetSegHitRatio) = CacheHitCount / HitCount * 100
                HitCount += 1
            End Try

        End Get
    End Property

    Public ReadOnly Property GetSegments(C1 As Coords, C2 As Coords, db As DBWrapper) As List(Of MapSegment)
        Get

            Dim TmpSeg As New List(Of MapSegment)

            Dim CellList As List(Of Coords) = BuildBspCellLine(C1, C2)
            For Each Center As Coords In CellList
                Dim l = GetSegments(Center, db)
                If l IsNot Nothing Then
                    TmpSeg.AddRange(l)
                End If
            Next

            Return TmpSeg

        End Get
    End Property

    Public Property InLand() As inlandstate

        Get
            Return _Inland
        End Get

        Set(ByVal value As inlandstate)
            _Inland = value
        End Set

    End Property

    Public ReadOnly Property NodeCount() As Long
        Get
            If _SubRects(0) Is Nothing Then
                Return 1
            Else
                Dim N As Long
                For i = 0 To 3
                    N += _SubRects(i).NodeCount
                Next

                Return N
            End If
        End Get
    End Property

    Function BuildBspCellLine(C1 As Coords, C2 As Coords) As List(Of Coords)
        Dim RetList As New List(Of Coords)

        If C1 = C2 Then
            RetList.Add(C1)
            Return RetList
        End If

        Dim DxOffset As Double = (360) / (2 ^ MAX_TREE_Z)
        Dim DyOffset As Double = (180) / (2 ^ MAX_TREE_Z)

        If C2.Lon < C1.Lon Then
            DxOffset = -DxOffset
        End If

        If C2.Lat < C1.Lat Then
            DyOffset = -DyOffset
        End If

        'FIXME handle antemeridien
        If ((C2.Lat_Deg - C1.Lat_Deg) = 0 OrElse Abs(C2.N_Lon_Deg - C1.N_Lon_Deg) > 2 * Abs(C2.Lat_Deg - C1.Lat_Deg)) _
            AndAlso (C2.Lon_Deg - C1.Lon_Deg <> 0) Then
            Dim Dy As Double = (C2.Lat_Deg - C1.Lat_Deg) / (C2.Lon_Deg - C1.Lon_Deg) * DxOffset
            Dim CurY As Double = C1.Lat_Deg - Dy
            For x = C1.N_Lon_Deg - DxOffset To C2.N_Lon_Deg + DxOffset Step DxOffset
                RetList.Add(New Coords(CurY, x))
                RetList.Add(New Coords(CurY - DyOffset, x))
                RetList.Add(New Coords(CurY + DyOffset, x))
                CurY += Dy
            Next
        Else
            Dim Dx As Double = (C2.Lon_Deg - C1.Lon_Deg) / (C2.Lat_Deg - C1.Lat_Deg) * DyOffset
            Dim CurX As Double = C1.Lon_Deg - Dx
            For y = C1.Lat_Deg - DyOffset To C2.Lat_Deg + DyOffset Step DyOffset
                RetList.Add(New Coords(y, CurX))
                RetList.Add(New Coords(y, CurX - DxOffset))
                RetList.Add(New Coords(y, CurX + DxOffset))
                CurX += Dx
            Next
        End If
        Return RetList
    End Function

    Public Function DebugInfo(ByVal c As Coords) As String
        Dim RetString As String = ""
        Dim Child As BspRect = GetChildFromCoords(c)

        If Child Is Nothing Then
            RetString = c.ToString & " in " & _Inland.ToString & "dx " & (_p1.Lon_Deg - _p2.Lon_Deg).ToString("f2") & " and " & (_p1.Lat_Deg - _p2.Lat_Deg).ToString("f2")
        Else
            RetString = Child.DebugInfo(c)
        End If

        Return RetString

    End Function

    Private ReadOnly Property GetChildFromCoords(ByVal C As Coords) As BspRect
        Get

            Dim y As Integer
            Dim retvalue As BspRect

            If C.Lat > _MidLat Then
                y = 0
            Else
                y = 1
            End If

            If C.Lon > _MidLon Then
                'x = 1
                retvalue = _SubRects(2 + y)
            Else
                'x = 0
                retvalue = _SubRects(y)
            End If

            Return retvalue

        End Get

    End Property

    Public ReadOnly Property PolygonCount() As Integer
        Get
            If _PolyGons Is Nothing Then
                Return 0
            Else
                Return _PolyGons.Count
            End If
        End Get
    End Property

    Private ReadOnly Property Polygons() As LinkedList(Of Polygon)
        Get
            Return _PolyGons
        End Get
    End Property

    '    Public Sub Split()


    '        Dim lP1Lon As Double = _p1.Lon
    '        Dim lP2Lon As Double = _p2.Lon
    '        Dim DeltaX As Double = lP1Lon - _MidLon '(lP1Lon - lP2Lon) / 2
    '        Dim lP1Lat As Double = _p1.Lat
    '        Dim lP2Lat As Double = _p2.Lat
    '        Dim DeltaY As Double = lP1Lat - _MidLat '(lP1Lat - lP2Lat) / 2

    '        If _SubRects(3) Is Nothing Then
    '            SpinLock()

    '            If _SubRects(3) Is Nothing Then
    '                For x As Integer = 0 To 1
    '                    For y As Integer = 0 To 1

    '                        Dim P1 As New Coords With {.Lat = lP1Lat - DeltaY * y, .Lon = lP1Lon - (1 - x) * DeltaX}
    '                        Dim P2 As New Coords With {.Lat = lP2Lat + DeltaY * (1 - y), .Lon = lP2Lon + x * DeltaX}

    '                        _SubRects(2 * x + y) = New BspRect(P1, P2, Z + 1)
    '                    Next
    '                Next
    '            End If

    '            SpinLockExit()
    '        End If

    '#If BSP_STATS Then
    '        BspCount += 4
    '        Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
    '#End If

    '    End Sub

    '    Public Function Split(ByVal GridGrain As Double) As Boolean


    '        Dim lP1Lon As Double = _p1.Lon
    '        Dim lP2Lon As Double = _p2.Lon
    '        Dim DeltaX As Double = lP1Lon - _MidLon '(lP1Lon - lP2Lon) / 2
    '        Dim lP1Lat As Double = _p1.Lat
    '        Dim lP2Lat As Double = _p2.Lat
    '        Dim DeltaY As Double = lP1Lat - _MidLat '(lP1Lat - lP2Lat) / 2

    '        If Math.Abs(2 * DeltaX) < MIN_POLYGON_SPLIT / 180 * Math.PI Then

    '            Return False

    '        End If
    '        If _SubRects(0) Is Nothing Then
    '            SpinLock()

    '            If _SubRects(0) Is Nothing Then
    '                For x As Integer = 0 To 1
    '                    For y As Integer = 0 To 1

    '                        Dim P1 As New Coords With {.Lat = lP1Lat - DeltaY * y, .Lon = lP1Lon - (1 - x) * DeltaX}
    '                        Dim P2 As New Coords With {.Lat = lP2Lat + DeltaY * (1 - y), .Lon = lP2Lon + x * DeltaX}

    '                        _SubRects(2 * x + y) = New BspRect(P1, P2, Z + 1)
    '                    Next
    '                Next
    '            End If

    '            SpinLockExit()
    '        End If

    '#If BSP_STATS Then
    '        BspCount += 4
    '        Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
    '#End If

    '        Return True
    '    End Function

    Private Sub SpinLock(X As Integer, Y As Integer)

        Dim GotLock As Boolean = False
        Dim Index As Integer = (X + Y) Mod SPIN_COUNT
        'Static SpinCount As Long = 0
        'Static SpinDuration As Double = 0
        'Dim Start As DateTime = Now

        If _SpinLock(Index).IsHeldByCurrentThread Then
            _SpinCount += 1
            'Console.WriteLine("enter" & Thread.CurrentThread.ManagedThreadId & "/" & _SpinCount)
            Return
        End If
        Do
            _SpinLock(Index).Enter(GotLock)
            'System.Threading.Thread.Sleep(1)
        Loop Until GotLock
        _SpinCount += 1
        ''Console.WriteLine("enter" & Thread.CurrentThread.ManagedThreadId & "/" & _SpinCount)
        'SpinCount += 1
        'SpinDuration += Now.Subtract(Start).TotalMilliseconds
        'Console.WriteLine("enter" & Thread.CurrentThread.ManagedThreadId & "/" & SpinDuration / SpinCount)

    End Sub

    Private Sub SpinLockExit(X As Integer, Y As Integer)
        'Console.WriteLine("exit" & Thread.CurrentThread.ManagedThreadId & "/" & _SpinCount)
        Dim Index As Integer = (X + Y) Mod SPIN_COUNT
        _SpinCount -= 1
        'If _SpinCount = 0 Then
        _SpinLock(Index).Exit()
        'Else
        'Dim i As Int16 = 0
        'End If
    End Sub

    Public Property SubRects() As BspRect()
        Get
            Return _SubRects
        End Get
        Set(ByVal value As BspRect())
            _SubRects = value
        End Set
    End Property

    Public ReadOnly Property UnknownNodeCount() As Long
        Get
            If _SubRects(0) Is Nothing Then
                If _Inland = inlandstate.Unknown Then
                    Return 1
                Else
                    Return 0

                End If
            Else
                Dim N As Long
                For i = 0 To 3
                    N += _SubRects(i).UnknownNodeCount
                Next
                Return N
            End If
        End Get
    End Property

    Public Property Z() As Integer
        Get
            Return _Z
        End Get

        Set(ByVal value As Integer)
            _Z = value
        End Set
    End Property









End Class
