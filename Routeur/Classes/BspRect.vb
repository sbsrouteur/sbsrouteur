﻿Imports System.Math

Public Class BspRect
    Public Const GRID_GRAIN_OVERSAMPLE As Integer = 8
    Private Const MAX_IGNORED_COUNT As Integer = 2000

    Public Enum inlandstate As Byte
        Unknown = 0
        InLand = 1
        InSea = 2
        Mixed = 3
    End Enum

    Private Shared MIN_POLYGON_SPLIT As Double = 0.01
    Private Shared _NoPolyGons As New LinkedList(Of Coords())
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
    Private _PolyGons As LinkedList(Of Coords())



    Public Sub New(ByVal P1 As Coords, ByVal P2 As Coords)

        If P1.Lat < -Math.PI OrElse P1.Lat > Math.PI _
            OrElse P2.Lat < -Math.PI OrElse P2.Lat > Math.PI Then
            Dim i As Integer = 0
        End If
        _p1 = P1
        _p2 = P2
        For i As Integer = 0 To 3
            _SubRects(i) = Nothing
        Next
        _Inland = inlandstate.Unknown
        _MidLat = _p2.Lat + (_p1.Lat - _p2.Lat) / 2
        _MidLon = _p2.Lon + (_p1.Lon - _p2.Lon) / 2

        If 16 * RouteurModel.GridGrain < MIN_POLYGON_SPLIT Then
            MIN_POLYGON_SPLIT = 16 * RouteurModel.GridGrain
        End If

    End Sub


    Public ReadOnly Property GetPolygons(ByVal C As Coords, ByVal Polygons As LinkedList(Of Coords()), ByVal gridgrain As Double) As LinkedList(Of Coords())
        Get
            If _PolyGons Is Nothing AndAlso Abs((P2.Lat - P1.Lat)) > MIN_POLYGON_SPLIT Then

                If SubRects(0) Is Nothing Then
                    Split(gridgrain)
                End If

                Return GetChildFromCoords(C, gridgrain).GetPolygons(C, Polygons, gridgrain)

            Else
                If _PolyGons Is Nothing Then

                    Dim P() As Coords
                    Dim Corner As New Coords
                    'Build the polygons list using the proper "trimmed " polygons
                    _PolyGons = New LinkedList(Of Coords())


                    For Each P In GSHHS_Reader.AllPolygons

                        If P(0) IsNot Nothing Then

                            Dim PRet() As Coords = PolyClipper.ClipPolygon(P1, P2, P)
                            If Not PRet Is Nothing Then
                                _PolyGons.AddLast(PRet)
                            End If

                        End If



                    Next

                End If
                Return _PolyGons
            End If
        End Get
    End Property

    Private ReadOnly Property GetRectSide(ByVal Lat As Double, ByVal Lon As Double) As Integer
        Get
            Dim RCosAlpha = (Lon - _MidLon) / 2
            Dim RSinAlpha = (Lat - _MidLat)
            Dim Side As Integer

            If RCosAlpha = 0 OrElse Abs(RSinAlpha / RCosAlpha) > 1 Then
                'Top/Bottom
                If RSinAlpha > 0 Then
                    Side = 0
                Else
                    Side = 2
                End If
            Else
                'Left Right
                If RCosAlpha > 0 Then
                    Side = 1
                Else
                    Side = 3
                End If
            End If

            Return Side
        End Get
    End Property

    Private Sub GoAroundCorners(ByVal EntrySide As Integer, ByVal exitside As Integer, ByVal Polygon As Coords(), ByRef CurIndex As Integer, ByVal TurnCCW As Boolean)

        Dim CurSide As Integer = EntrySide
        Dim Corner As New Coords

        Do
            If TurnCCW Then
                Select CurSide
                    Case 0
                        'Add NE Corner
                        Corner.Lat = _p1.Lat
                        Corner.Lon = _p2.Lon

                    Case 1
                        'Add SE Corner
                        Corner.Lat = _p1.Lat
                        Corner.Lon = _p1.Lon

                    Case 2
                        'Add SW Corner
                        Corner.Lat = _p2.Lat
                        Corner.Lon = _p1.Lon

                    Case 3
                        'Add NW Corner
                        Corner.Lat = _p2.Lat
                        Corner.Lon = _p2.Lon

                End Select

                CurSide += 3
            Else
                Select Case CurSide
                    Case 0
                        'Add NE Corner
                        Corner.Lat = _p1.Lat
                        Corner.Lon = _p1.Lon

                    Case 1
                        'Add SE Corner
                        Corner.Lat = _p2.Lat
                        Corner.Lon = _p1.Lon

                    Case 2
                        'Add SW Corner
                        Corner.Lat = _p2.Lat
                        Corner.Lon = _p2.Lon

                    Case 3
                        'Add NW Corner
                        Corner.Lat = _p1.Lat
                        Corner.Lon = _p2.Lon

                End Select

                CurSide += 1
            End If
            CurSide = CurSide Mod 4

            Polygon(CurIndex) = New Coords(Corner)
            CurIndex += 1

        Loop Until CurSide = exitside
    End Sub


    Public ReadOnly Property InLand() As inlandstate
        Get
            Return _Inland
        End Get
    End Property


    Private Function OptimizeBSP(ByRef curvalue As inlandstate, ByRef i As Integer, ByVal retvalue As inlandstate, ByRef shouldReturn As Boolean) As inlandstate

        shouldReturn = False
        If _Inland = inlandstate.Unknown Then

            For i = 0 To 3

                curvalue = _SubRects(i).InLand
                If _SubRects(i).PolygonCount <> 0 Then
                    Return retvalue
                End If
                If curvalue = inlandstate.Unknown Then
                    shouldReturn = True
                    Return retvalue
                ElseIf curvalue = inlandstate.Mixed Then
                    _Inland = inlandstate.Mixed
                    shouldReturn = True : Return retvalue
                ElseIf curvalue <> retvalue Then
                    _Inland = inlandstate.Mixed
                    shouldReturn = True : Return retvalue
                End If

            Next


            _Inland = retvalue
            _PolyGons = _NoPolyGons
            For i = 0 To 3
                _SubRects(i).P1 = Nothing
                _SubRects(i).P2 = Nothing
                _SubRects(i) = Nothing
            Next

#If BSP_STATS Then
            BspCount -= 4
            Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
#End If
            'CollectionCount += 1
            'If CollectionCount Mod 50000 = 0 Then
            'GC.Collect()
            'End If



        End If

        Return inlandstate.Unknown

    End Function

    Private ReadOnly Property GetChildFromCoords(ByVal C As Coords, ByVal gridgrain As Double) As BspRect
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

    Public Property Inland(ByVal C As Coords, ByVal gridgrain As Double, ByVal Depth As Integer, Optional ByVal Merge As Boolean = True) As inlandstate
        Get
            Dim curvalue As inlandstate
            Dim i As Integer

            'If Abs(_MidLat / Math.PI * 180 - 31.175) <= gridgrain AndAlso Abs(_MidLon / Math.PI * 180 - 29.725) <= gridgrain Then
            '    Dim ibreak As Integer = 0
            'End If
            If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
#If BSP_STATS Then
                _HitTestDepth += Depth
                _HitTestCount += 1
                Stats.SetStatValue(Stats.StatID.BSPCellAvgDepth) = _HitTestDepth / _HitTestCount
#End If
                Return _Inland
            Else
                SyncLock GSHHS_Reader._Tree


                    'recheck just in case
                    If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
                        Return _Inland
                    End If
                    If _SubRects(0) Is Nothing Then
                        'RaiseEvent Log("Synclock bsp1 th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
                        If Not Split(gridgrain / GRID_GRAIN_OVERSAMPLE) Then

                            'Dim lNewCoords As Coords = New Coords((_p1.Lat_Deg + _p2.Lat_Deg) / 2, (_p1.Lon_Deg + _p2.Lon_Deg) / 2)
                            Dim lNewCoords As New Coords
                            lNewCoords.Lon = _MidLon
                            lNewCoords.Lat = _MidLat



                            Dim Polys = GSHHS_Reader.Polygons(lNewCoords)
                            If GSHHS_Reader.HitTest(lNewCoords, 0, Polys, True, True) Then
                                _Inland = inlandstate.InLand
                            Else
                                _Inland = inlandstate.InSea
                            End If
                            Return _Inland
                        ElseIf GridGrainDepth = -1 AndAlso Abs(P1.Lon_Deg - P2.Lon_Deg) < gridgrain Then
                            GridGrainDepth = Depth




                        End If

                    End If

                    'Dim x As Integer
                    Dim retvalue As inlandstate = GetChildFromCoords(C, gridgrain).InLand(C, gridgrain, Depth + 1)
                    'If Merge AndAlso GridGrainDepth <> -1 AndAlso Depth > GridGrainDepth Then
                    '    For i = 0 To 3
                    '        If SubRects(i) IsNot Nothing AndAlso SubRects(i).InLand = inlandstate.Unknown Then
                    '            Dim v = Inland(New Coords() With {.Lat = (SubRects(i).P1.Lat + SubRects(i).P2.Lat) / 2, _
                    '                                      .Lon = (SubRects(i).P1.Lon + SubRects(i).P1.Lon) / 2}, gridgrain, Depth + 1, False)
                    '        End If
                    '    Next
                    'End If

                    Dim lShouldReturn As Boolean
                    Dim lResult As inlandstate = OptimizeBSP(curvalue, i, retvalue, lShouldReturn)
                    If lShouldReturn Then
                        Return lResult
                    End If

                    Return retvalue
                End SyncLock

            End If



        End Get
        Set(ByVal value As inlandstate)
            _Inland = value
        End Set
    End Property


    Public Sub InSert(ByVal C As Coords, ByVal State As inlandstate, ByVal GridGrain As Double)

        SyncLock GSHHS_Reader._Tree


            'recheck just in case
            If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
                'Force state to inland, if it was for see before
                If State <> _Inland AndAlso State = inlandstate.InLand Then
                    _Inland = inlandstate.InLand
                End If
                Return
            End If
            If _SubRects(0) Is Nothing Then
                'RaiseEvent Log("Synclock bsp1 th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
                If Not Split(GridGrain / GRID_GRAIN_OVERSAMPLE) Then

                    _Inland = State
                    Return
                End If

            End If


            GetChildFromCoords(C, GridGrain).InSert(C, State, GridGrain)

            Return
        End SyncLock
        Return

    End Sub

    Public Property P1() As Coords
        Get
            Return _p1
        End Get
        Set(ByVal value As Coords)
            _p1 = value
        End Set
    End Property

    Public Property P2() As Coords
        Get
            Return _p2
        End Get
        Set(ByVal value As Coords)
            _p2 = value
        End Set
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
    Public Function Split(ByVal GridGrain As Double) As Boolean


        Dim lP1Lon As Double = _p1.Lon
        Dim lP2Lon As Double = _p2.Lon
        Dim DeltaX As Double = lP1Lon - _MidLon '(lP1Lon - lP2Lon) / 2
        Dim lP1Lat As Double = _p1.Lat
        Dim lP2Lat As Double = _p2.Lat
        Dim DeltaY As Double = lP1Lat - _MidLat '(lP1Lat - lP2Lat) / 2

        If Math.Abs(DeltaX) < MIN_POLYGON_SPLIT / 180 * Math.PI Then

            Return False
        End If
        For x As Integer = 0 To 1
            For y As Integer = 0 To 1

                Dim P1 As New Coords With {.Lat = lP1Lat - DeltaY * y, .Lon = lP1Lon - (1 - x) * DeltaX}
                Dim P2 As New Coords With {.lat = lP2Lat + DeltaY * (1 - y), .lon = lP2Lon + x * DeltaX}

                _SubRects(2 * x + y) = New BspRect(P1, P2)
            Next
        Next

#If BSP_STATS Then
        BspCount += 4
        Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
#End If

        Return True
    End Function

    Public Property SubRects() As BspRect()
        Get
            Return _SubRects
        End Get
        Set(ByVal value As BspRect())
            _SubRects = value
        End Set
    End Property




End Class
