Imports System.Math

Public Class BspRect
    Public Const GRID_GRAIN_OVERSAMPLE As Integer = 8
    Private MIN_POLYGON_SPLIT As Double = 0.01
    Private Const MAX_IGNORED_COUNT As Integer = 2000

    Public Enum inlandstate As Byte
        Unknown = 0
        InLand = 1
        InSea = 2
        Mixed = 3
    End Enum

    Shared _NoPolyGons As New List(Of Coords())

    Private _p1 As Coords
    Private _p2 As Coords
    Private _MidLat As Double
    Private _MidLon As Double
    Private _SubRects(3) As BspRect
    Private _Inland As inlandstate
    Private Shared CollectionCount As Long
    Private _PolyGons As List(Of Coords())

    Public Shared BspCount As Long = 0
    Shared Event Log(ByVal Msg As String)
    Private Shared _HitTestCount As Long
    Private Shared _HitTestDepth As Long
    Private Shared GridGrainDepth As Integer = -1


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

        MIN_POLYGON_SPLIT = 16 * RouteurModel.GridGrain

    End Sub

    'Public Sub CreateBsp(ByVal GridGrain As Double, ByVal Polygons As Generic.List(Of Coords()))

    '    Dim Value As inlandstate

    '    If Not Split(GridGrain) Then
    '        If GSHHS_Reader.HitTest(New Coords((_p1.Lat_Deg + _p2.Lat_Deg) / 2, (_p1.Lon_Deg + _p2.Lon_Deg) / 2), 0, Polygons, False) Then
    '            _Inland = inlandstate.InLand
    '        Else
    '            _Inland = inlandstate.InSea
    '        End If
    '        Return
    '    End If
    '    For Each Rect In _SubRects
    '        Rect.CreateBsp(GridGrain, Polygons)
    '    Next

    '    Value = _SubRects(0).InLand
    '    If Value <> inlandstate.Mixed Then
    '        For i = 1 To 3

    '            If Value <> _SubRects(i).InLand Then
    '                _Inland = inlandstate.Mixed
    '                Return
    '            End If

    '        Next

    '        'All childs have the same state aggregate
    '        _Inland = Value
    '        For i = 0 To 3
    '            _SubRects(i) = Nothing
    '        Next
    '        BspCount -= 4

    '    End If

    'End Sub

    Public ReadOnly Property GetPolygons(ByVal C As Coords, ByVal Polygons As List(Of Coords()), ByVal gridgrain As Double) As List(Of Coords())
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
                    _PolyGons = New List(Of Coords())
                    
                    
                    For Each P In GSHHS_Reader.AllPolygons

                        If P(0) IsNot Nothing Then

                            Dim PRet() As Coords = PolyClipper.ClipPolygon(P1, P2, P)
                            If Not PRet Is Nothing Then
                                _PolyGons.Add(PRet)
                            End If

                        End If

                        'Dim CurIndex As Integer = 0
                        'Dim NewPoly() As Coords
                        'Dim InRect As Boolean = False
                        'Dim EntryIndex As Integer = 0
                        'Dim ExitIndex As Integer = -1
                        'Dim ExitSide As Integer = 0
                        'ReDim NewPoly(P.Count - 1)
                        'Dim Pnt As Coords
                        'Dim Points(1000) As System.Drawing.Point
                        'Dim EntrySide As Integer
                        'Dim Dy As Double = 0
                        'Dim Dx As Double = 0
                        'Dim EntryLat As Double
                        'Dim EntryLon As Double
                        'Dim PIndex As Integer = 0
                        'Dim ReversePolygon As Boolean = False

                        'For Each Pnt In P

                        'If Not Pnt Is Nothing Then
                        '    With Pnt
                        '        If (.Lon >= P2.Lon AndAlso .Lon <= P1.Lon AndAlso _
                        '            .Lat >= P2.Lat AndAlso .Lat <= P1.Lat) Then

                        '            If Not InRect Then
                        '                EntryLat = .Lat
                        '                EntryLon = .Lon
                        '                EntryIndex = PIndex
                        '                EntrySide = GetRectSide(.Lat, .Lon)
                        '            End If

                        '            If ExitIndex <> -1 AndAlso Not InRect Then
                        '                'Coming back into the rect
                        '                'Run Around the corner to the re-entry point
                        '                'ExitSide = GetRectSide(P(ExitIndex).Lat, P(ExitIndex).Lon)

                        '                'restart:
                        '                'If EntrySide <> ExitSide Then
                        '                Dx = 0
                        '                Dy = 0
                        '                If EntrySide = 0 OrElse EntrySide = 2 Then
                        '                    If EntrySide = 0 Then
                        '                        Dx = gridgrain '_p1.Lon - EntryLon
                        '                    Else
                        '                        Dx = -gridgrain ' _p2.Lon - EntryLon
                        '                    End If
                        '                End If

                        '                If EntrySide = 1 OrElse EntrySide = 3 Then
                        '                    If EntrySide = 1 Then
                        '                        Dy = -gridgrain '_p2.Lat - EntryLat
                        '                    Else
                        '                        Dy = gridgrain  '_p1.Lat - EntryLat
                        '                    End If
                        '                End If
                        '                'If EntrySide = ExitSide Then

                        '                '    If EntrySide = 0 OrElse EntrySide = 2 Then
                        '                '        If EntrySide = 0 AndAlso Abs(Dx) >= Abs(.Lon - EntryLon) Then
                        '                '            Dx = (.Lon - EntryLon) / 2
                        '                '        ElseIf EntrySide = 2 AndAlso Abs(Dx) > Abs(.Lon - EntryLon) Then
                        '                '            Dx = (.Lon - EntryLon) / 2
                        '                '        End If
                        '                '    End If

                        '                '    If EntrySide = 1 OrElse EntrySide = 3 Then
                        '                '        If EntrySide = 1 AndAlso Abs(Dy) > Abs(.Lat - EntryLat) Then
                        '                '            Dy = (.Lat - EntryLat) / 2
                        '                '        ElseIf EntrySide = 3 AndAlso Abs(Dy) > Abs(.Lat - EntryLat) Then
                        '                '            Dy = (.Lat - EntryLat) / 2
                        '                '        End If
                        '                '    End If

                        '                'End If

                        '                '                                        'If Abs(Dx) < RouteurModel.GridGrain / 2 AndAlso Abs(Dy) < RouteurModel.GridGrain / 2 Then
                        '                '                                        '    'To close to the corner, change side
                        '                '                                        '    EntrySide += 1
                        '                '                                        '    EntrySide = EntrySide Mod 4
                        '                '                                        '    EntryLat += Dy
                        '                '                                        '    EntryLon += Dx
                        '                '                                        '    GoTo restart
                        '                '                                        'End If

                        '                L.Clear()
                        '                L.Add(P)
                        '                If GSHHS_Reader.HitTest(New Coords() With {.Lat = EntryLat + Dy, .Lon = EntryLon + Dx}, _
                        '                                        0, L, True, True) Then
                        '                    ReversePolygon = False
                        '                Else
                        '                    ReversePolygon = True
                        '                End If
                        '                ' Inside, close the polygon
                        '                '    NewPoly(CurIndex) = New Coords()
                        '                '    NewPoly(CurIndex).Lon = .Lon
                        '                '    NewPoly(CurIndex).Lat = .Lat

                        '                '    Dim NewPoly2(CurIndex - 1) As Coords
                        '                '    System.Array.Copy(NewPoly, NewPoly2, CurIndex)
                        '                '    _PolyGons.Add(NewPoly2)
                        '                '    CurIndex = 0
                        '                '    ReDim NewPoly(P.Count - 1)
                        '                '    ExitIndex = -1
                        '                'Else
                        '                'Change Side, run around the corners
                        '                'Run Around the corner to the re-entry point
                        '                If Not ReversePolygon Then
                        '                    GoAroundCorners(EntrySide, ExitSide, NewPoly, CurIndex, False)
                        '                Else
                        '                    '************************
                        '                    'TODO Add a polygons here
                        '                    '************************
                        '                    Dim NewPoly2(CurIndex - 1) As Coords
                        '                    System.Array.Copy(NewPoly, NewPoly2, CurIndex)
                        '                    _PolyGons.Add(NewPoly2)
                        '                    CurIndex = 0
                        '                    ReDim NewPoly(P.Count - 1)
                        '                    GoAroundCorners(ExitSide, EntrySide, NewPoly, CurIndex, False)
                        '                End If

                        '                ExitIndex = -1
                        '                'End If
                        '                'Else
                        '                '    'Outside, just remember so that it will eventually get closed
                        '                '    ExitIndex = CurIndex - 1
                        '            End If

                        '            '                                        End If

                        '            '                                End With
                        '            'EntrySide = GetRectSide(.Lat, .Lon)
                        '            'If EntrySide <> ExitSide Then
                        '            '    GoAroundCorners(ExitSide, EntrySide, NewPoly, CurIndex)
                        '            'End If
                        '            'End If

                        '            If CurIndex > NewPoly.GetUpperBound(0) Then
                        '                ReDim Preserve NewPoly(CurIndex + 500)
                        '            End If
                        '            NewPoly(CurIndex) = New Coords()
                        '            NewPoly(CurIndex).Lon = .Lon
                        '            NewPoly(CurIndex).Lat = .Lat
                        '            InRect = True
                        '            CurIndex += 1

                        '        ElseIf InRect Then

                        '            'Getting out of the polygon
                        '            ExitIndex = PIndex
                        '            InRect = False

                        '            'ExitSide = GetRectSide(P(ExitIndex).Lat, P(ExitIndex).Lon)


                        '            'Dx = 0
                        '            'Dy = 0
                        '            'If EntrySide = 0 OrElse EntrySide = 2 Then
                        '            '    If EntrySide = 0 Then
                        '            '        Dx = gridgrain '_p1.Lon - EntryLon
                        '            '    Else
                        '            '        Dx = -gridgrain ' _p2.Lon - EntryLon
                        '            '    End If
                        '            'End If

                        '            'If EntrySide = 1 OrElse EntrySide = 3 Then
                        '            '    If EntrySide = 1 Then
                        '            '        Dy = -gridgrain '_p2.Lat - EntryLat
                        '            '    Else
                        '            '        Dy = gridgrain  '_p1.Lat - EntryLat
                        '            '    End If
                        '            'End If

                        '            'L.Clear()
                        '            'L.Add(P)
                        '            'If GSHHS_Reader.HitTest(New Coords() With {.Lat = EntryLat + Dy, .Lon = EntryLon + Dx}, _
                        '            '                                0, L, True, True) Then
                        '            '    ReversePolygon = False
                        '            'Else
                        '            '    ReversePolygon = True
                        '            'End If

                        '            'If Not ReversePolygon Then
                        '            '    'Close the polygons, otherwise will handle upon rentry
                        '            '    Dim NewPoly2(CurIndex - 1) As Coords
                        '            '    System.Array.Copy(NewPoly, NewPoly2, CurIndex)
                        '            '    _PolyGons.Add(NewPoly2)
                        '            '    CurIndex = 0
                        '            '    ExitIndex = -1
                        '            '    ReDim NewPoly(P.Count - 1)
                        '            '    GoAroundCorners(ExitSide, EntrySide, NewPoly, CurIndex, False)
                        '            'End If


                        '        End If

                        '    End With
                        'End If
                        ''PolygonsPath.AddPolygon(Points)
                        'PIndex += 1
                        'Next

                        'If CurIndex <> 0 AndAlso Not InRect And ExitIndex <> -1 Then
                        '    ExitSide = GetRectSide(P(ExitIndex).Lat, P(ExitIndex).Lon)

                        '    'restart:
                        '    Dx = 0
                        '    Dy = 0
                        '    If EntrySide = 0 OrElse EntrySide = 2 Then
                        '        If EntrySide = 0 Then
                        '            Dx = gridgrain '_p1.Lon - EntryLon
                        '        Else
                        '            Dx = -gridgrain ' _p2.Lon - EntryLon
                        '        End If
                        '    End If

                        '    If EntrySide = 1 OrElse EntrySide = 3 Then
                        '        If EntrySide = 1 Then
                        '            Dy = -gridgrain '_p2.Lat - EntryLat
                        '        Else
                        '            Dy = gridgrain  '_p1.Lat - EntryLat
                        '        End If
                        '    End If
                        '    'If EntrySide = ExitSide Then

                        '    '    If EntrySide = 0 OrElse EntrySide = 2 Then
                        '    '        If EntrySide = 0 AndAlso Abs(Dx) >= Abs(.Lon - EntryLon) Then
                        '    '            Dx = (.Lon - EntryLon) / 2
                        '    '        ElseIf EntrySide = 2 AndAlso Abs(Dx) > Abs(.Lon - EntryLon) Then
                        '    '            Dx = (.Lon - EntryLon) / 2
                        '    '        End If
                        '    '    End If

                        '    '    If EntrySide = 1 OrElse EntrySide = 3 Then
                        '    '        If EntrySide = 1 AndAlso Abs(Dy) > Abs(.Lat - EntryLat) Then
                        '    '            Dy = (.Lat - EntryLat) / 2
                        '    '        ElseIf EntrySide = 3 AndAlso Abs(Dy) > Abs(.Lat - EntryLat) Then
                        '    '            Dy = (.Lat - EntryLat) / 2
                        '    '        End If
                        '    '    End If

                        '    'End If

                        '    '                                        'If Abs(Dx) < RouteurModel.GridGrain / 2 AndAlso Abs(Dy) < RouteurModel.GridGrain / 2 Then
                        '    '                                        '    'To close to the corner, change side
                        '    '                                        '    EntrySide += 1
                        '    '                                        '    EntrySide = EntrySide Mod 4
                        '    '                                        '    EntryLat += Dy
                        '    '                                        '    EntryLon += Dx
                        '    '                                        '    GoTo restart
                        '    '                                        'End If

                        '    If EntrySide <> ExitSide Then
                        '        EntryLat = P(EntryIndex).Lat
                        '        EntryLon = P(EntryIndex).Lon

                        '        L.Add(P)
                        '        If GSHHS_Reader.HitTest(New Coords() With {.Lat = EntryLat + Dy, .Lon = EntryLon + Dx}, _
                        '                                0, L, True, True) Then
                        '            ReversePolygon = False
                        '        Else
                        '            ReversePolygon = True
                        '        End If
                        '        ' Inside, close the polygon
                        '        '    NewPoly(CurIndex) = New Coords()
                        '        '    NewPoly(CurIndex).Lon = .Lon
                        '        '    NewPoly(CurIndex).Lat = .Lat

                        '        '    Dim NewPoly2(CurIndex - 1) As Coords
                        '        '    System.Array.Copy(NewPoly, NewPoly2, CurIndex)
                        '        '    _PolyGons.Add(NewPoly2)
                        '        '    CurIndex = 0
                        '        '    ReDim NewPoly(P.Count - 1)
                        '        '    ExitIndex = -1
                        '        'Else
                        '        'Change Side, run around the corners
                        '        'Run Around the corner to the re-entry point
                        '        If Not ReversePolygon Then
                        '            GoAroundCorners(EntrySide, ExitSide, NewPoly, CurIndex, False)
                        '        Else
                        '            GoAroundCorners(ExitSide, EntrySide, NewPoly, CurIndex, False)
                        '        End If
                        '        'Dim NewPoly2(CurIndex - 1) As Coords
                        '        'System.Array.Copy(NewPoly, NewPoly2, CurIndex)
                        '        '_PolyGons.Add(NewPoly2)
                        '        'CurIndex = 0
                        '        'ReDim NewPoly(P.Count - 1)
                        '        ExitIndex = -1
                        '    End If


                        'End If

                        'If CurIndex <> 0 Then
                        '    ReDim Preserve NewPoly(CurIndex - 1)
                        '    _PolyGons.Add(NewPoly)
                        'End If

                    Next

                    'Dim Res As New System.Drawing.Region(RectPath)
                    'Res.Intersect(PolygonsPath)
                    'Dim Data = Res.GetRegionData()

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
            BspCount -= 4
            Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)

            CollectionCount += 1
            If CollectionCount Mod 10000 = 0 Then
                GC.Collect()
            End If



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

            If Abs(_MidLat / Math.PI * 180 - 31.175) <= gridgrain AndAlso Abs(_MidLon / Math.PI * 180 - 29.725) <= gridgrain Then
                Dim ibreak As Integer = 0
            End If
            If _SubRects(0) Is Nothing And _Inland <> inlandstate.Unknown Then
                _HitTestDepth += Depth
                _HitTestCount += 1
                Stats.SetStatValue(Stats.StatID.BSPCellAvgDepth) = _HitTestDepth / _HitTestCount
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


            ''Dim x As Integer
            'Dim y As Integer

            'If C.Lat > _MidLat Then
            '    y = 0
            'Else
            '    y = 1
            'End If

            'If C.Lon > _MidLon Then
            '    'x = 1
            '    _SubRects(2 + y).InSert(C, State, GridGrain)
            'Else
            '    'x = 0
            '    _SubRects(y).InSert(C, State, GridGrain)
            'End If
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

        If Math.Abs(DeltaX) < GridGrain / 180 * Math.PI Then

            Return False
        End If
        For x As Integer = 0 To 1
            For y As Integer = 0 To 1

                Dim P1 As New Coords With {.Lat = lP1Lat - DeltaY * y, .Lon = lP1Lon - (1 - x) * DeltaX}
                Dim P2 As New Coords With {.lat = lP2Lat + DeltaY * (1 - y), .lon = lP2Lon + x * DeltaX}

                _SubRects(2 * x + y) = New BspRect(P1, P2)
            Next
        Next
        BspCount += 4
        Stats.SetStatValue(Stats.StatID.BSPCellCount) = CDbl(BspCount)
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
