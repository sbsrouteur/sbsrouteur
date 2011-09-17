Imports System.Math

Public Class BspRect
    Public Const GRID_GRAIN_OVERSAMPLE As Integer = 1
    Private Const MAX_IGNORED_COUNT As Integer = 2000

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

    Shared Optimizer As New bspOptimizer

    Public Sub New()

    End Sub


    Public Sub New(ByVal P1 As Coords, ByVal P2 As Coords, ByVal Z As Integer)

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

    Public ReadOnly Property GetSegments(C As Coords, GridGrain As Double, db As DBWrapper) As List(Of MapSegment)
        Get
            If _Segments Is Nothing AndAlso Abs((P2.Lat - P1.Lat)) > MIN_POLYGON_SPLIT Then

                If _SubRects(0) Is Nothing Then
                    If Not Split(GridGrain) Then
                        Return Nothing
                    End If
                End If
                Dim Rect As BspRect = GetChildFromCoords(C, GridGrain)
                If Rect Is Nothing Then
                    Return Nothing
                Else
                    Return Rect.GetSegments(C, GridGrain, db)
                End If
            Else
                SyncLock _lockObj
                    If _Segments Is Nothing Then

                        Dim Seg As MapSegment
                        Dim PolygonIndex As Integer = 0
                        Dim Corner As New Coords
                        'Build the polygons list using the proper "trimmed " polygons
                        _Segments = New List(Of MapSegment)


                        For Each Seg In db.SegmentList(P1.N_Lon_Deg, P1.Lat_Deg, P2.N_Lon_Deg, P2.Lat_Deg)

                            _Segments.Add(Seg)


                        Next

                    End If
                    Return _Segments
                End SyncLock
            End If
        End Get
    End Property

    Public ReadOnly Property GetPolygons(ByVal C As Coords, ByVal Polygons As LinkedList(Of Polygon), ByVal gridgrain As Double) As LinkedList(Of Polygon)
        Get
            If _PolyGons Is Nothing AndAlso Abs((P2.Lat - P1.Lat)) > MIN_POLYGON_SPLIT Then

                If _SubRects(0) Is Nothing Then
                    If Not Split(gridgrain) Then
                        Return Nothing
                    End If
                End If
                Return GetChildFromCoords(C, gridgrain).GetPolygons(C, Polygons, gridgrain)

            Else
                If _PolyGons Is Nothing Then

                    Dim P As Polygon
                    Dim PolygonIndex As Integer = 0
                    Dim Corner As New Coords
                    'Build the polygons list using the proper "trimmed " polygons
                    _PolyGons = New LinkedList(Of Polygon)


                    For Each P In GSHHS_Reader.AllPolygons

                        If PolygonIndex < GSHHS_Reader.ExclusionCount Then
                            'Always add exclusions without clipping
                            _PolyGons.AddLast(P)
                            PolygonIndex += 1
                        Else
                            If P(0) IsNot Nothing Then

                                Dim PRet As Polygon = PolyClipper.ClipPolygon(P1, P2, P)
                                If Not PRet Is Nothing AndAlso PRet.Count > 2 Then
                                    _PolyGons.AddLast(PRet)
                                End If

                            End If

                        End If



                    Next

                End If
                Return _PolyGons
            End If
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


    '    Private Function OptimizeBSP(ByRef curvalue As inlandstate, ByRef i As Integer, ByVal retvalue As inlandstate, ByRef shouldReturn As Boolean) As inlandstate

    '        shouldReturn = False
    '        If _Inland = inlandstate.Unknown Then

    '            For i = 0 To 3

    '                curvalue = _SubRects(i).InLand
    '                If curvalue = inlandstate.Unknown Then
    '                    shouldReturn = True
    '                    Optimizer.AddToOptimizationList(Me)
    '                    Return retvalue
    '                ElseIf curvalue = inlandstate.Mixed Then
    '                    _Inland = inlandstate.Mixed
    '                    shouldReturn = True : Return retvalue
    '                ElseIf curvalue <> retvalue Then
    '                    _Inland = inlandstate.Mixed
    '                    shouldReturn = True : Return retvalue
    '                End If

    '            Next


    '            _Inland = retvalue
    '            If _PolyGons Is Nothing OrElse _PolyGons Is _NoPolyGons Then
    '                _PolyGons = New LinkedList(Of Polygon)
    '            End If

    '            For i = 0 To 3
    '                If Not _SubRects(i).Polygons Is Nothing Then
    '                    For Each p In _SubRects(i).Polygons
    '                        _PolyGons.AddLast(p)
    '                    Next
    '                End If
    '                _SubRects(i).P1 = Nothing
    '                _SubRects(i).P2 = Nothing
    '                _SubRects(i) = Nothing
    '            Next

    '#If BSP_STATS Then
    '            BspCount -= 4
    '            Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
    '#End If
    '            'CollectionCount += 1
    '            'If CollectionCount Mod 50000 = 0 Then
    '            'GC.Collect()
    '            'End If

    '        End If

    '        Return inlandstate.Unknown

    '    End Function

    Public Function DebugInfo(ByVal c As Coords) As String
        Dim RetString As String = ""
        Dim Child As BspRect = GetChildFromCoords(c, RouteurModel.GridGrain)

        If Child Is Nothing Then
            RetString = c.ToString & " in " & _Inland.ToString & "dx " & (_p1.Lon_Deg - _p2.Lon_Deg).ToString("f2") & " and " & (_p1.Lat_Deg - _p2.Lat_Deg).ToString("f2")
        Else
            RetString = Child.DebugInfo(c)
        End If

        Return RetString

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

    '    Public Property Inland(ByVal C As Coords, ByVal gridgrain As Double, ByVal Depth As Integer, Optional ByVal Merge As Boolean = True) As inlandstate
    '        Get
    '            Dim curvalue As inlandstate
    '            Dim i As Integer

    '            'If Abs(_MidLat / Math.PI * 180 - 31.175) <= gridgrain AndAlso Abs(_MidLon / Math.PI * 180 - 29.725) <= gridgrain Then
    '            '    Dim ibreak As Integer = 0
    '            'End If
    '            If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
    '#If BSP_STATS Then
    '                _HitTestDepth += Depth
    '                _HitTestCount += 1
    '                Stats.SetStatValue(Stats.StatID.BSPCellAvgDepth) = _HitTestDepth / _HitTestCount
    '#End If
    '                Return _Inland
    '            Else
    '                SyncLock GSHHS_Reader._Tree


    '                    'recheck just in case
    '                    If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
    '                        Return _Inland
    '                    End If
    '                    If _SubRects(0) Is Nothing Then
    '                        'RaiseEvent Log("Synclock bsp1 th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
    '                        If Not Split(gridgrain / GRID_GRAIN_OVERSAMPLE) Then

    '                            'Dim lNewCoords As Coords = New Coords((_p1.Lat_Deg + _p2.Lat_Deg) / 2, (_p1.Lon_Deg + _p2.Lon_Deg) / 2)
    '                            Dim lNewCoords As New Coords
    '                            lNewCoords.Lon = _MidLon
    '                            lNewCoords.Lat = _MidLat



    '                            Dim Polys = GSHHS_Reader.Polygons(lNewCoords)
    '                            If Polys Is Nothing OrElse Polys.Count = 0 Then
    '                                _Inland = inlandstate.InSea
    '                            ElseIf GSHHS_Reader.HitTest(lNewCoords, 0, Polys, True, True) Then
    '                                _Inland = inlandstate.InLand
    '                            Else
    '                                _Inland = inlandstate.InSea
    '                            End If
    '                            Return _Inland
    '                        ElseIf GridGrainDepth = -1 AndAlso Abs(P1.Lon_Deg - P2.Lon_Deg) < gridgrain Then
    '                            GridGrainDepth = Depth




    '                        End If

    '                    End If

    '                    'Dim x As Integer
    '                    Dim retvalue As inlandstate = GetChildFromCoords(C, gridgrain).InLand(C, gridgrain, Depth + 1)
    '                    'If Merge AndAlso GridGrainDepth <> -1 AndAlso Depth > GridGrainDepth Then
    '                    '    For i = 0 To 3
    '                    '        If SubRects(i) IsNot Nothing AndAlso SubRects(i).InLand = inlandstate.Unknown Then
    '                    '            Dim v = Inland(New Coords() With {.Lat = (SubRects(i).P1.Lat + SubRects(i).P2.Lat) / 2, _
    '                    '                                      .Lon = (SubRects(i).P1.Lon + SubRects(i).P1.Lon) / 2}, gridgrain, Depth + 1, False)
    '                    '        End If
    '                    '    Next
    '                    'End If

    '                    Dim lShouldReturn As Boolean
    '                    Dim lResult As inlandstate = OptimizeBSP(curvalue, i, retvalue, lShouldReturn)
    '                    If lShouldReturn Then
    '                        Return lResult
    '                    End If

    '                    Return retvalue
    '                End SyncLock

    '            End If



    '        End Get
    '        Set(ByVal value As inlandstate)
    '            _Inland = value
    '        End Set
    '    End Property


    'Public Sub InSert(ByVal C As Coords, ByVal State As inlandstate, ByVal GridGrain As Double)

    '    SyncLock GSHHS_Reader._Tree


    '        'recheck just in case
    '        If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
    '            'Force state to inland, if it was for see before
    '            If State <> _Inland AndAlso State = inlandstate.InLand Then
    '                _Inland = inlandstate.InLand
    '            End If
    '            Return
    '        End If
    '        If _SubRects(0) Is Nothing Then
    '            'RaiseEvent Log("Synclock bsp1 th" & System.Threading.Thread.CurrentThread.ManagedThreadId)
    '            If Not Split(GridGrain / GRID_GRAIN_OVERSAMPLE) Then

    '                _Inland = State
    '                Return
    '            End If

    '        End If


    '        GetChildFromCoords(C, GridGrain).InSert(C, State, GridGrain)

    '        Return
    '    End SyncLock
    '    Return

    'End Sub

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

    Private ReadOnly Property Polygons() As LinkedList(Of Polygon)
        Get
            Return _PolyGons
        End Get
    End Property


    Public Function Split(ByVal GridGrain As Double) As Boolean


        Dim lP1Lon As Double = _p1.Lon
        Dim lP2Lon As Double = _p2.Lon
        Dim DeltaX As Double = lP1Lon - _MidLon '(lP1Lon - lP2Lon) / 2
        Dim lP1Lat As Double = _p1.Lat
        Dim lP2Lat As Double = _p2.Lat
        Dim DeltaY As Double = lP1Lat - _MidLat '(lP1Lat - lP2Lat) / 2

        If Math.Abs(2 * DeltaX) < MIN_POLYGON_SPLIT / 180 * Math.PI Then

            Return False

        End If
        If _SubRects(0) Is Nothing Then
            SyncLock _lockObj

                If _SubRects(0) Is Nothing Then
                    For x As Integer = 0 To 1
                        For y As Integer = 0 To 1

                            Dim P1 As New Coords With {.Lat = lP1Lat - DeltaY * y, .Lon = lP1Lon - (1 - x) * DeltaX}
                            Dim P2 As New Coords With {.Lat = lP2Lat + DeltaY * (1 - y), .Lon = lP2Lon + x * DeltaX}

                            _SubRects(2 * x + y) = New BspRect(P1, P2, Z + 1)
                        Next
                    Next
                End If

            End SyncLock
        End If

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
