Imports System.IO
Imports System.Math


Public Class GSHHS_Reader

    Private Const GSHHS_FACTOR As Integer = 1000000

    Private Shared _PolyGons As New LinkedList(Of Polygon)
    'Private Shared _UseFullPolygon As New List(Of Coords())
    'Private Shared _usefullboxes As New List(Of Coords())
    Private Shared _P1 As New Coords(90, 179.999999)
    Private Shared _P2 As New Coords(-90, -179.99999)
    Public Shared WithEvents _Tree As BspRect
    Private Shared _LakePolyGons As New LinkedList(Of Polygon)
    Public Shared ExclusionCount As Integer

#If HIT_STATS Then
    Private Shared _HitDistTicks As Long
    Private Shared _HitDistCount As Long
    Private Shared _HitDistPolyCount As Long
    Private Shared _HitDistLoop As Long
    Private Shared _HitTestNoBspCount As Long
    Private Shared _HitTestNoBspTicks As Long
    Private Shared _HitTestBspCount As Long
    Private Shared _HitTestBspTicks As Long
    Private Shared _HitTestNoBspPoly As Long
#End If


    Private Shared _ExcludedID() As Integer = New Integer() {}

    Shared Event BspEvt(ByVal Count As Long)
    Shared Event Log(ByVal Msg As String)

    Public Shared ReadOnly Property AllPolygons() As LinkedList(Of Polygon)
        Get
            Return _PolyGons
        End Get
    End Property

    Public Shared ReadOnly Property Polygons(ByVal C As Coords) As LinkedList(Of Polygon)
        Get
            Return _Tree.GetPolygons(C, _PolyGons, RouteurModel.GridGrain)
        End Get
    End Property

    Public Shared Sub Read(ByVal State As Object)

        Dim SI As GSHHS_StartInfo = CType(State, GSHHS_StartInfo)
        Try
            Dim A As Polygon
            Dim landpoly As Boolean

            Dim PolyCount As Long = 0
            If Not File.Exists(SI.StartPath) Then
                Return
            End If

            _Tree = New BspRect(_P1, _P2)

#If NO_MAP = 0 Then
            Dim S As FileStream = New FileStream(SI.StartPath, FileMode.Open, FileAccess.Read)

            SI.ProgressWindows.Start(S.Length)
            Do
                A = ReadPoly(S, SI, landpoly)
                If Not A Is Nothing Then
                    If landpoly Then
                        _PolyGons.AddLast(A)
                    Else
                        _LakePolyGons.AddLast(A)
                    End If
                    PolyCount += A.Count
                End If
                SI.ProgressWindows.Progress(S.Position)
            Loop Until S.Position >= S.Length 'Or _UseFullPolygon.Count > 5 'A Is Nothing 'Or PolyGons.Count > 2

            S.Close()

            ExclusionCount = 0
            For Each excl In RouteurModel.Exclusions
                'ReDim A(CInt(excl.Count / 2) - 1)
                A = New Polygon
                For i = 0 To excl.Count - 1 Step 2
                    Dim c As New Coords(excl(i + 1), excl(i))
                    A.Add(c)
                Next
                _PolyGons.AddFirst(A)
                ExclusionCount += 1
                '_UseFullPolygon.Add(A)
                '_usefullboxes.Add(UpdateBox(A))

            Next

#End If

        Catch ex As Exception

            MessageBox.Show(ex.Message, "Error loading Map data")
        Finally
            SI.CompleteCallBack()
        End Try
    End Sub

    Private Shared Function ReadHeader(ByVal S As FileStream) As GSHHS_Header

        Dim H As New GSHHS_Header
        Dim HeaderBlock(39) As Byte

        If S.Length = S.Position Then
            Return Nothing
        End If

        'S.Read(HeaderBlock, 0, 40)
        With H
            .id = Readinteger(S)
            .n = Readinteger(S)
            .level = Readinteger(S)
            .west = Readinteger(S)
            .east = Readinteger(S)
            .south = Readinteger(S)
            .north = Readinteger(S)
            .area = Readinteger(S)

            .Greenwich = ((.level >> 16) And &HFF) <> 0
            .level = (.level And &HFF)

        End With

        Return H

    End Function

    Private Shared Function Readinteger(ByVal S As FileStream) As Integer

        Dim V(3) As Byte

        Dim i As Integer

        For i = 3 To 0 Step -1
            V(i) = CByte(S.ReadByte)
        Next

        Return BitConverter.ToInt32(V, 0)
    End Function

    Private Shared Function ReadPoly(ByVal S As FileStream, ByVal SI As GSHHS_StartInfo, ByRef LandPolygon As Boolean) As Polygon

        Dim H As GSHHS_Header = ReadHeader(S)
        Dim RetPoints As New Polygon
        Dim Lat As Double
        Dim lon As Double
        Dim i As Integer
        Dim ActivePoints As Integer
        'Dim TC As New TravelCalculator
        Dim InZone As Boolean
        Dim HasPointsInZone As Boolean = False
        Dim Fr_map As System.IO.StreamWriter = Nothing
        Dim lookupzone As LinkedList(Of Polygon) = SI.PolyGons

        Static maxeast As Integer = 270
        If H Is Nothing Then
            Return Nothing
        End If

        'ReDim RetPoints(CInt(H.n - 1))
        Dim IgnoredPoints As Long = 0
        ActivePoints = -1
        LandPolygon = H.level = 1 OrElse H.level = 3
        For i = 0 To CInt(H.n) - 1
            lon = CDbl(Readinteger(S)) / GSHHS_FACTOR
            Lat = CDbl(Readinteger(S)) / GSHHS_FACTOR


            If H.level = 1 OrElse (RouteurModel.LAKE_RACE AndAlso (H.level = 2 OrElse H.level = 3)) Then
                If lon > 180 Then
                    lon -= 360
                End If


                RetPoints(ActivePoints + 1) = New Coords(Lat, lon)

                InZone = HitTest(RetPoints(ActivePoints + 1), 0, lookupzone, False, True)
                'InZone = True
                'RetPoints(ActivePoints + 1).Lon = -RetPoints(ActivePoints + 1).Lon
                If i = 0 Then
                    ActivePoints += 1
                    HasPointsInZone = InZone
                ElseIf ActivePoints >= 0 Then
                    'TC.StartPoint = RetPoints(ActivePoints)
                    'TC.EndPoint = RetPoints(ActivePoints + 1)

                    HasPointsInZone = InZone Or HasPointsInZone

                    'If (InZone And TC.SurfaceDistance > 3) OrElse TC.SurfaceDistance > 25 Then
                    'If ((HasPointsInZone AndAlso TC.SurfaceDistance > RouteurModel.GridGrain / 2)) OrElse IgnoredPoints > 2000 Then
                    If InZone OrElse IgnoredPoints > 2000 Then 'HasPointsInZone  Then

                        ActivePoints += 1
                        'If InZone Then
                        '_Tree.InSert(RetPoints(ActivePoints), BspRect.inlandstate.InLand, RouteurModel.GridGrain)
                        'End If
                        IgnoredPoints = 0
                Else
                    IgnoredPoints += 1
                End If
                End If

            End If
            SI.ProgressWindows.Progress(S.Position)
        Next

        If maxeast = 270 Then
            maxeast = 180
        End If

        If ActivePoints = 0 Then
            Return Nothing
        End If


        If HasPointsInZone Then
            Dim IsExcluded As Boolean = False

            'For Each index In _ExcludedID
            '    If index = H.id Then
            '        IsExcluded = True
            '        Exit For
            '    End If
            'Next
            If Not IsExcluded AndAlso LandPolygon Then
                Dim box(1) As Coords
                Dim Mlon As Double = H.east / GSHHS_FACTOR
                Dim MLat As Double = H.north / GSHHS_FACTOR

                If Mlon > 180 Then
                    Mlon -= 360
                    'Else
                    '    Mlon = -Mlon
                    If H.id = 0 Then
                        Mlon = 180
                    End If
                End If
                'box = UpdateBox(RetPoints)
                box(0) = New Coords(MLat, Mlon)
                Mlon = H.west / GSHHS_FACTOR
                If Mlon > 180 Then
                    Mlon -= 360
                End If
                If H.id = 0 Then
                    Mlon = -180
                End If
                'Mlon = -Mlon
                MLat = H.south / GSHHS_FACTOR
                box(1) = New Coords(MLat, Mlon)

                'Console.WriteLine("AddPoly to list" & H.id)
                '_UseFullPolygon.Add(RetPoints)
                If RetPoints(0) Is Nothing Then
                    i = 0
                ElseIf _PolyGons.Count = 212 Then
                    i = CInt(Math.Sqrt(_PolyGons.Count))
                End If
                '_PolyGons.Add(RetPoints)
                '_usefullboxes.Add(box)
            End If
        End If

        Return RetPoints


    End Function

    Private Shared Function UpdateBox(ByVal RetPoints As Coords()) As Coords()

        Dim Box(1) As Coords
        Dim ActivePoints As Integer

        Dim i As Integer

        For i = 0 To 1
            Box(i) = New Coords
        Next

        Box(0).Lat_Deg = -90
        Box(0).Lon_Deg = 180
        Box(1).Lat_Deg = 90
        Box(1).Lon_Deg = -180



        For ActivePoints = 0 To RetPoints.Count - 1

            If RetPoints(ActivePoints).Lat > Box(0).Lat Then
                Box(0).Lat = RetPoints(ActivePoints).Lat

            End If

            If RetPoints(ActivePoints).Lat < Box(1).Lat Then
                Box(1).Lat = RetPoints(ActivePoints).Lat
            End If

            If RetPoints(ActivePoints).Lon > Box(1).Lon Then
                Box(1).Lon = RetPoints(ActivePoints).Lon
            End If

            If RetPoints(ActivePoints).Lon < Box(0).Lon Then
                Box(0).Lon = RetPoints(ActivePoints).Lon
            End If
        Next

        Return Box
    End Function

    Public Const MIN_DELTA As Double = 0.01 ' * 0.1 '10 * 10 / 60 / 60

    Public Shared Function HitDistance(ByVal PTest As Coords, ByVal LookupZone As List(Of Coords()), ByVal TimeIt As Boolean) As Double

#If HIT_STATS Then
        Dim StartTick As DateTime = Now
        Try
#End If

        Dim i As Integer
        Dim j As Integer
        Dim P As Coords = New Coords() With {.Lat = PTest.Lat, .Lon = PTest.Lon}
        Dim RetDistance As Double = Double.MaxValue
        Dim NbLoops As Integer

        If Polygons(PTest) Is Nothing OrElse Polygons(PTest).Count = 0 Then
            Return RetDistance
        End If

            'P.Lon = -P.Lon
            For Each Poly In LookupZone

                j = Poly.GetUpperBound(0)
                NbLoops = Poly.GetUpperBound(0)
                If NbLoops = 1 Then
                    NbLoops = 0
                End If
                For i = 0 To NbLoops

                    Dim dy1 As Double = Math.Abs(Poly(i).Lat_Deg - P.Lat_Deg)
                    Dim dy2 As Double = Math.Abs(Poly(j).Lat_Deg - P.Lat_Deg)
                    Dim dy3 As Double = Math.Abs(Poly(i).Lat_Deg - Poly(j).Lat_Deg)
                    Dim dx1 As Double = Math.Abs(Poly(i).Lon_Deg - P.Lon_Deg)
                    Dim dx2 As Double = Math.Abs(Poly(j).Lon_Deg - P.Lon_Deg)
                    Dim dx3 As Double = Math.Abs(Poly(i).Lon_Deg - Poly(j).Lon_Deg)
                    If dx1 <= dx3 OrElse Math.Abs(dy1) <= dy3 _
                        OrElse dx2 <= dx3 OrElse dy2 <= dy3 Then
                        Dim D As Double = PointToLineDistance(P, Poly(i), Poly(j))
                        If D < RetDistance Then
                            RetDistance = D
                        End If
                    End If

                    j = i
                Next

#If HIT_STATS Then
                If TimeIt Then
                    _HitDistLoop += NbLoops + 1
                End If
#End If

            Next

            P = Nothing

#If HIT_STATS Then
            If TimeIt Then
                _HitDistCount += 1
                _HitDistPolyCount += LookupZone.Count
            End If
#End If

            Return RetDistance

#If HIT_STATS Then
        Finally

            If TimeIt Then
                'If Now.Subtract(StartTick).TotalMilliseconds > 50 Then
                '    Dim i As DateTime = Now
                'End If
                _HitDistTicks += Now.Subtract(StartTick).Ticks
                Stats.SetStatValue(Stats.StatID.HitDistanceAvgMS) = _HitDistTicks / _HitDistCount / TimeSpan.TicksPerMillisecond
                Stats.SetStatValue(Stats.StatID.HitDistanceAvgLoops) = _HitDistLoop / _HitDistCount
                Stats.SetStatValue(Stats.StatID.HitDistanceAvgPolyCount) = _HitDistPolyCount / _HitDistCount
            End If

        End Try
#End If

    End Function

    Public Shared Function HitTest(ByVal P As Coords, ByVal HitDistance As Double, ByVal LookupZone As LinkedList(Of Polygon), ByVal UseBoxes As Boolean, Optional ByVal IgnoreBSP As Boolean = False) As Boolean

        'Static calls As Long = 0
        Dim StartTick As DateTime = Now

        If _Tree IsNot Nothing And Not IgnoreBSP Then
#If HIT_STATS Then
            Try
#End If

            Return _Tree.InLand(P, RouteurModel.GridGrain, 0) = BspRect.inlandstate.InLand

#If HIT_STATS Then
            Finally
                _HitTestBspCount += 1
                _HitTestBspTicks += Now.Subtract(StartTick).Ticks

                Stats.SetStatValue(Stats.StatID.HitTestBspAvgMS) = _HitTestBspTicks / _HitTestBspCount / TimeSpan.TicksPerMillisecond

            End Try
#End If

        End If


        Dim i As Integer
        Dim j As Integer
        Dim RetVal As Boolean = False

        If LookupZone.Count = 0 Then
            Return False
        End If

        Dim NbLoops As Long

#If HIT_STATS Then
        Try
#End If

        'Dim P As New Coords(PTest)
        UseBoxes = False
        Dim Poly As Polygon
        'Dim Box As Coords() = Nothing
        Dim x As Integer

        For x = 0 To LookupZone.Count - 1
            Poly = LookupZone(x)

            Dim MaxIndex As Integer = Poly.Count

            j = Poly.Count
            If j = 0 Then
                Continue For
            End If

            While Poly(j) Is Nothing AndAlso MaxIndex > 0
                j -= 1
            End While
            MaxIndex = j

            For i = 0 To MaxIndex


                If ((Poly(i).Lat > P.Lat) <> (Poly(j).Lat > P.Lat)) AndAlso (P.Lon < (Poly(j).Lon - Poly(i).Lon) * (P.Lat - Poly(i).Lat) / (Poly(j).Lat - Poly(i).Lat) + Poly(i).Lon) Then
                    RetVal = Not RetVal
                End If

                j = i
            Next
            NbLoops += i

            If RetVal Then
                Return RetVal
            End If


        Next

        If RetVal AndAlso RouteurModel.LAKE_RACE Then
            Return HitTest(P, HitDistance, _LakePolyGons, False, IgnoreBSP)
        End If

        Return RetVal

#If HIT_STATS Then

        Finally
            _HitTestNoBspCount += 1
            _HitTestNoBspTicks += Now.Subtract(starttick).ticks
            _HitTestNoBspPoly += NbLoops

            Stats.SetStatValue(Stats.StatID.HitTestNoBSPAvgLoops) = _HitTestNoBspPoly / _HitTestNoBspCount
            Stats.SetStatValue(Stats.StatID.HitTestNoBspAvgMS) = _HitTestNoBspTicks / _HitTestNoBspCount / TimeSpan.TicksPerMillisecond

        End Try
#End If

    End Function

    Private Shared Function PointsDistance(ByVal x1 As Double, ByVal y1 As Double, ByVal x2 As Double, ByVal y2 As Double) As Double

        'Dim dx As Double = x1 - x2
        'Dim dy As Double = y1 - y2
        'Return dx * dx + dy * dy
        Dim TC As New TravelCalculator
        TC.StartPoint = New Coords(y1, x1)
        TC.EndPoint = New Coords(y2, x2)
        Dim retval As Double = TC.SurfaceDistance
        TC.StartPoint = Nothing
        TC.EndPoint = Nothing
        Return retval

    End Function

    Private Shared Function IntersectGCFromPoint(ByVal Lat1_Rad As Double, ByVal Lon1_Rad As Double, ByVal Lat2_Rad As Double, ByVal Lon2_Rad As Double, ByVal Crs13_Rad As Double, ByVal Crs23_Rad As Double) As Double
        Dim dst12 As Double
        Dim Crs12 As Double
        Dim Crs21 As Double
        Dim Ang1 As Double
        Dim Ang2 As Double
        Dim Ang3 As Double
        Dim Dst13 As Double
        Dim Dlon As Double
        Dim Lat3 As Double
        Dim Lon3 As Double

        '        Now how to compute the latitude, lat3, and longitude, lon3 of an intersection formed by the crs13 true bearing from point 1 and the crs23 true bearing from point 2:
        dst12 = 2 * Asin(Sqrt((Sin((Lat1_Rad - Lat2_Rad) / 2)) ^ 2 + Cos(Lat1_Rad) * Cos(Lat2_Rad) * Sin((Lon1_Rad - Lon2_Rad) / 2) ^ 2))
        If Sin(Lon2_Rad - Lon1_Rad) < 0 Then
            Crs12 = Acos((Sin(Lat2_Rad) - Sin(Lat1_Rad) * Cos(dst12)) / (Sin(dst12) * Cos(Lat1_Rad)))
            Crs21 = 2.0 * PI - Acos((Sin(Lat1_Rad) - Sin(Lat2_Rad) * Cos(dst12)) / (Sin(dst12) * Cos(Lat2_Rad)))
        Else
            Crs12 = 2.0 * PI - Acos((Sin(Lat2_Rad) - Sin(Lat1_Rad) * Cos(dst12)) / (Sin(dst12) * Cos(Lat1_Rad)))
            Crs21 = Acos((Sin(Lat1_Rad) - Sin(Lat2_Rad) * Cos(dst12)) / (Sin(dst12) * Cos(Lat2_Rad)))
        End If


        Ang1 = ((Crs13_Rad - Crs12 + PI) Mod (2.0 * PI)) - PI
        Ang2 = ((Crs21 - Crs23_Rad + PI) Mod (2.0 * PI)) - PI

        If (Sin(Ang1) = 0 And Sin(Ang2) = 0) Then
            '"infinity of intersections"
            Return 0
            'Else
            '    'Return Double.MaxValue
            '    Lat3 = Lat1_Rad
            '    Lon3 = Lon1_Rad
        Else
            If Sin(Ang1) * Sin(Ang2) < 0 Then
                '    '"intersection ambiguous"
                Dim i As Integer = 0
            End If
            Ang1 = Abs(Ang1)
            Ang2 = Abs(Ang2)
            Ang3 = Acos(-Cos(Ang1) * Cos(Ang2) + Sin(Ang1) * Sin(Ang2) * Cos(dst12))
            Dst13 = Atan2(Sin(dst12) * Sin(Ang1) * Sin(Ang2), Cos(Ang2) + Cos(Ang1) * Cos(Ang3))
            Lat3 = Asin(Sin(Lat1_Rad) * Cos(Dst13) + Cos(Lat1_Rad) * Sin(Dst13) * Cos(Crs13_Rad))
            Dlon = Atan2(Sin(Crs13_Rad) * Sin(Dst13) * Cos(Lat1_Rad), Cos(Dst13) - Sin(Lat1_Rad) * Sin(Lat3))
            Lon3 = ((Lon1_Rad - Dlon + 7 * PI) Mod (2 * PI)) - PI
        End If

        Dim TCNav As New TravelCalculator
        TCNav.StartPoint = New Coords() With {.Lat = Lat2_Rad, .Lon = Lon2_Rad}

        TCNav.EndPoint = New Coords()
        TCNav.EndPoint.Lon = Lon3
        TCNav.EndPoint.Lat = Lat3
        Dim D As Double = TCNav.SurfaceDistance
        TCNav.StartPoint = Nothing

        TCNav.EndPoint = Nothing
        TCNav = Nothing
        Return D
    End Function

    
    Private Shared Function CrossProduct(ByVal e1 As Coords, ByVal e2 As Coords) As Coords

        Dim x As Double
        Dim y As Double
        Dim z As Double

        x = Sin(e1.Lat - e2.Lat) * Sin((-e1.Lon - e2.Lon) / 2) * Cos((-e1.Lon + e2.Lon) / 2) - Sin(e1.Lat + e2.Lat) * Cos((-e1.Lon + -e2.Lon) / 2) * Sin((-e1.Lon - -e2.Lon) / 2)
        y = Sin(e1.Lat - e2.Lat) * Cos((-e1.Lon + -e2.Lon) / 2) * Cos((-e1.Lon - -e2.Lon) / 2) + Sin(e1.Lat + e2.Lat) * Sin((-e1.Lon + -e2.Lon) / 2) * Sin((-e1.Lon - -e2.Lon) / 2)
        z = Cos(e1.Lat) * Cos(e2.Lat) * Sin(-e1.Lon - -e2.Lon)

        Return New Coords(x, y, z)
    End Function

    'Private Shared Function IntersectGCFromPoint(ByVal Lat1_Rad As Double, ByVal Lon1_Rad As Double, ByVal Lat2_Rad As Double, ByVal Lon2_Rad As Double, ByVal Crs13_Rad As Double, ByVal Crs23_Rad As Double) As Double

    '    Dim RetVal As Double = Double.MaxValue
    '    Dim TC As New TravelCalculator
    '    Dim PT1 As New Coords() With {.Lat = Lat1_Rad, .Lon = Lon1_Rad}
    '    Dim PT2 As Coords
    '    Dim PT3 As New Coords() With {.Lat = Lat2_Rad, .Lon = Lon2_Rad}
    '    Dim PT4 As Coords

    '    TC.StartPoint = Pt1
    '    PT2 = TC.ReachDistance(1, Crs13_Rad / PI * 180)

    '    TC.StartPoint = PT3
    '    PT4 = TC.ReachDistance(1, Crs23_Rad / PI * 180)

    '    TC.StartPoint = Nothing

    '    Dim PP1 As Coords = CrossProduct(Pt1, PT2)
    '    Dim PP2 As Coords = CrossProduct(PT3, PT4)
    '    'Dim m1 As Double = PP1.Module
    '    'Dim m2 As Double = PP2.Module
    '    'PP1.normalize()
    '    'PP2.normalize()

    '    Dim Res As Coords = CrossProduct(PP1, PP2)
    '    TC.StartPoint = PT2
    '    TC.EndPoint = Res
    '    RetVal = TC.SurfaceDistance
    '    Res.Lat = -Res.Lat
    '    Res.Lon = Res.Lon - PI
    '    If TC.SurfaceDistance < RetVal Then
    '        RetVal = TC.SurfaceDistance
    '    End If

    '    TC.StartPoint = Nothing
    '    TC.EndPoint = Nothing
    '    TC = Nothing

    '    Return RetVal
    'End Function

    'Private Shared Function IntersectGCFromPoint(ByVal P As Coords, ByVal P1 As Coords, ByVal P2 As Coords) As Double

    '    Dim A As Double = P.Mercator_X - P1.Mercator_X
    '    Dim B As Double = P.Mercator_Y - P1.Mercator_Y
    '    Dim C As Double = P.Mercator_X - P2.Mercator_X
    '    Dim D As Double = P.Mercator_Y - P2.Mercator_Y

    '    Return Abs(A * D - C * B) / Sqrt(C * C + D * D)

    'End Function

    'Private Shared Function PointToLineDistance(ByVal P As Coords, ByVal p1 As Coords, ByVal p2 As Coords) As Double

    '    Dim TCSegment As New TravelCalculator
    '    Dim TCNav As New TravelCalculator

    '    TCSegment.StartPoint = p1
    '    TCSegment.EndPoint = p2

    '    TCNav.StartPoint = p1
    '    TCNav.EndPoint = P

    '    Dim Cap1 As Double = TCSegment.TrueCourse
    '    Dim A1 As Double = VOR_Router.WindAngle(TCNav.TrueCap, TCSegment.TrueCap)
    '    If A1 < -90 OrElse A1 > 90 Then
    '        Dim D As Double = TCNav.SurfaceDistance
    '        TCNav.StartPoint = Nothing
    '        TCNav.EndPoint = Nothing
    '        TCSegment.EndPoint = Nothing
    '        TCSegment.StartPoint = Nothing
    '        Return D
    '    End If

    '    TCSegment.StartPoint = p2
    '    TCSegment.EndPoint = p1
    '    TCNav.StartPoint = p2
    '    TCNav.EndPoint = P
    '    Dim A2 As Double = VOR_Router.WindAngle(TCNav.TrueCap, TCSegment.TrueCap)
    '    If A2 < -90 OrElse A2 > 90 Then
    '        Dim D As Double = TCNav.SurfaceDistance
    '        TCNav.StartPoint = Nothing
    '        TCNav.EndPoint = Nothing
    '        TCSegment.EndPoint = Nothing
    '        TCSegment.StartPoint = Nothing
    '        Return D
    '    End If

    '    TCNav.StartPoint = Nothing
    '    TCNav.EndPoint = Nothing
    '    TCSegment.EndPoint = Nothing
    '    TCSegment.StartPoint = Nothing
    '    TCNav = Nothing
    '    TCSegment = Nothing

    '    Dim Cap2 As Double
    '    If A1 <= 0 Then
    '        Cap2 = (Cap1 + PI / 2) Mod (2 * PI)
    '    Else
    '        Cap2 = (Cap1 + 3 * PI / 2) Mod (2 * PI)
    '    End If
    '    Dim Lat1 As Double = p1.Lat
    '    Dim Lat2 As Double = P.Lat
    '    Dim Lon1 As Double = p1.Lon
    '    Dim Lon2 As Double = P.Lon
    '    Dim D1 As Double = IntersectGCFromPoint(Lat1, Lon1, Lat2, Lon2, Cap1, Cap2)
    '    'Dim D2 As Double = IntersectGCFromPoint(Lat1, Lon1, Lat2, Lon2, Cap1, Cap3)
    '    'Lat1 = p2.Lat
    '    'Lon2 = p2.Lon
    '    'Cap1 = (Cap1 + 2 * PI) Mod (2 * PI)
    '    'Dim D1 As Double = IntersectGCFromPoint(P, p1, p2)

    '    If D1 > 2000 Then
    '        Dim i As Integer = 0
    '    End If
    '    'Dim D3 As Double = IntersectGCFromPoint(Lat1, Lon1, Lat2, Lon2, Cap1, Cap2)
    '    'Dim D4 As Double = IntersectGCFromPoint(Lat1, Lon1, Lat2, Lon2, Cap1, Cap3)
    '    Return D1
    '    'Return Min(Min(D1, D2), Min(D3, D4))
    'End Function

    Private Shared Function PointToLineDistance(ByVal P As Coords, ByVal p1 As Coords, ByVal p2 As Coords) As Double
        ' px,py is the point to test.
        ' x1,y1,x2,y2 is the line to check distance.
        '
        ' Returns distance from the line, or if the intersecting point on the line nearest
        ' the point tested is outside the endpoints of the line, the distance to the
        ' nearest endpoint.
        '
        ' Returns 9999 on 0 denominator conditions.

        Dim tcSeg As New TravelCalculator With {.StartPoint = p1, .EndPoint = p2}
        Dim tcPoint As New TravelCalculator With {.StartPoint = p1, .EndPoint = P}
        Dim D As Double = 0
        Dim Angle As Double = Abs(GribManager.CheckAngleInterp(tcSeg.LoxoCourse_Deg - tcPoint.LoxoCourse_Deg))
        If Angle >= 90 Then
            D = tcPoint.SurfaceDistance
        Else
            If tcPoint.SurfaceDistance * Cos(Angle / 180 * PI) <= tcSeg.SurfaceDistance Then
                With tcSeg
                    .EndPoint = tcSeg.ReachDistance(tcPoint.SurfaceDistance * Cos(Angle / 180 * PI), tcSeg.LoxoCourse_Deg)
                    .StartPoint = P
                    D = .SurfaceDistance
                    .StartPoint = Nothing
                    .EndPoint = Nothing
                End With
            Else
                tcPoint.StartPoint = p2
                D = tcPoint.SurfaceDistance
            End If
        End If
        tcSeg = Nothing
        tcPoint.StartPoint = Nothing
        tcPoint.EndPoint = Nothing
        tcPoint = Nothing

        Return D

        'Dim LineMag As Double, u As Double
        'Dim ix As Double, iy As Double ' intersecting point
        'Dim x1 As Double = p1.Lon_Deg
        'Dim x2 As Double = p2.Lon_Deg
        'Dim y1 As Double = p1.Lat_Deg
        'Dim y2 As Double = p2.Lat_Deg
        'Dim px As Double = P.Lon_Deg
        'Dim py As Double = P.Lat_Deg

        'LineMag = Sqrt((x2 - x1) ^ 2 + (y2 - y1) ^ 2) ' PointsDistance(x1, y1, x2, y2)
        'If LineMag < 0.00000001 Then

        '    Return Double.MaxValue

        'End If

        'u = (((px - x1) * (x2 - x1)) + ((py - y1) * (y2 - y1)))
        'u = u / (LineMag)
        'If u < 0.00001 Or u > 1 Then
        '    '// closest point does not fall within the line segment, take the shorter distance
        '    '// to an endpoint
        '    ix = PointsDistance(px, py, x1, y1)
        '    iy = PointsDistance(px, py, x2, y2)
        '    If ix > iy Then
        '        Return iy
        '    Else
        '        Return ix
        '    End If
        '    'Return Double.MaxValue
        'Else
        '    ' Intersecting point is on the line, use the formula
        '    Dim tc As New TravelCalculator
        '    tc.StartPoint = New Coords(y1, x1)
        '    tc.EndPoint = New Coords(y2, x2)
        '    tc.EndPoint = tc.ReachDistance(u * LineMag, tc.Cap)
        '    tc.StartPoint = New Coords(py, px)
        '    Dim D As Double = tc.SurfaceDistance
        '    tc.StartPoint = Nothing
        '    tc.EndPoint = Nothing
        '    tc = Nothing
        '    Return D
        '    'ix = x1 + u * (x2 - x1)
        '    'iy = y1 + u * (y2 - y1)
        '    'Return PointsDistance(px, py, ix, iy)
        'End If
    End Function


    Public Shared Function PointToSegmentIntersect(ByVal P As Coords, ByVal p1 As Coords, ByVal p2 As Coords) As Coords
        ' px,py is the point to test.
        ' x1,y1,x2,y2 is the line to check distance.
        '
        ' Returns distance from the line, or if the intersecting point on the line nearest
        ' the point tested is outside the endpoints of the line, the distance to the
        ' nearest endpoint.
        '
        ' Returns 9999 on 0 denominator conditions.

        Dim tcSeg As New TravelCalculator With {.StartPoint = p1, .EndPoint = p2}
        Dim tcPoint As New TravelCalculator With {.StartPoint = p1, .EndPoint = P}
        Dim RetCoords As Coords
        Dim Angle As Double = Abs(GribManager.CheckAngleInterp(tcSeg.LoxoCourse_Deg - tcPoint.LoxoCourse_Deg))

        If Angle >= 90 Then
            'ClosestPoint Is p1
            RetCoords = New Coords(p1)
        Else
            If tcPoint.SurfaceDistance * Cos(Angle / 180 * PI) <= tcSeg.SurfaceDistance Then
                'Point is in segment
                With tcSeg
                    .EndPoint = tcSeg.ReachDistance(tcPoint.SurfaceDistance * Cos(Angle / 180 * PI), tcSeg.LoxoCourse_Deg)
                    RetCoords = New Coords(.EndPoint)
                    .StartPoint = Nothing
                    .EndPoint = Nothing
                End With
            Else
                'Closest point is P2
                RetCoords = New Coords(p2)
            End If
        End If
        tcSeg = Nothing
        tcPoint.StartPoint = Nothing
        tcPoint.EndPoint = Nothing
        tcPoint = Nothing

        Return RetCoords


    End Function

    Private Shared Sub _Tree_Log(ByVal Msg As String) Handles _Tree.Log
        RaiseEvent Log(Msg)
    End Sub
End Class

Public Class GSHHS_Header

    'int id;				/* Unique polygon id number, starting at 0 */
    'int n;				/* Number of points in this polygon */
    'int level;			/* 1 land, 2 lake, 3 island_in_lake, 4 pond_in_island_in_lake */
    'int west, east, south, north;	/* min/max extent in micro-degrees */
    'int area;			/* Area of polygon in 1/10 km^2 */
    'int version;			/* Polygon version, set to 3
    'short int greenwich;		/* Greenwich is 1 if Greenwich is crossed */
    'short int source;		/* 0 = CIA WDBII, 1 = WVS */

    Public id As Integer
    Public n As Integer
    Public level As Integer
    Public flag As Integer
    Public west As Integer
    Public east As Integer
    Public south As Integer
    Public north As Integer
    Public area As Integer

    Public Greenwich As Boolean

End Class

Public Class BoundingBoxInfo

    Public MinCoords As Coords
    Public MaxCoords As Coords

End Class






