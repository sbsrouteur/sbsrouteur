Public Class PolyClipper

    Public Enum ClipPlane As Integer
        North = 0
        East = 1
        South = 2
        West = 3
    End Enum

    Private Shared Function IsInSide(ByVal P1 As Coords, ByVal P2 As Coords, ByVal ClipPlane As ClipPlane, ByVal P As Coords) As Boolean

        Select Case ClipPlane

            Case ClipPlane.North
                Return P.Lat <= P1.Lat

            Case ClipPlane.South
                Return P.Lat >= P2.Lat
            Case ClipPlane.East
                Return P.Lon <= P1.Lon
            Case ClipPlane.West
                Return P.Lon >= P2.Lon
        End Select

        Return False

    End Function

    Private Shared Function Intersect(ByVal clip As ClipPlane, ByVal p1 As Coords, ByVal p2 As Coords, ByVal Start As Coords, ByVal EndP As Coords) As Coords

        Dim TargetLon As Double
        Dim TargetLat As Double

        Select Case clip

            Case ClipPlane.North
                TargetLat = p1.Lat + RouteurModel.GridGrain / 180 * Math.PI

            Case ClipPlane.South
                TargetLat = p2.Lat - RouteurModel.GridGrain / 180 * Math.PI

            Case ClipPlane.East
                TargetLon = p1.Lon + RouteurModel.GridGrain / 180 * Math.PI

            Case ClipPlane.West
                TargetLon = p2.Lon - RouteurModel.GridGrain / 180 * Math.PI

        End Select

        If clip = ClipPlane.North OrElse clip = ClipPlane.South Then
            TargetLon = Start.Lon + (EndP.Lon - Start.Lon) * (TargetLat - Start.Lat) / (EndP.Lat - Start.Lat)
        Else
            TargetLat = Start.Lat + (EndP.Lat - Start.Lat) * (TargetLon - Start.Lon) / (EndP.Lon - Start.Lon)
        End If

        Return New Coords() With {.Lat = TargetLat, .Lon = TargetLon}

    End Function


    Private Shared Function ClipPolygonPlane(ByVal Clip As ClipPlane, ByVal P1 As Coords, ByVal P2 As Coords, ByVal Polygon As Coords()) As Coords()



        Dim PrevIndex As Integer = Polygon.Count - 1
        Dim PrevInRect As Boolean = IsInSide(P1, P2, Clip, Polygon(PrevIndex))
        Dim Index As Integer
        Dim RetArray(Polygon.Count) As Coords
        Dim CurIndex As Integer = 0


        For Index = 0 To Polygon.Count - 1

            Dim PInside As Boolean = IsInSide(P1, P2, Clip, Polygon(Index))
            Dim AddPoint As Boolean = False

            If CurIndex >= RetArray.Length Then
                ReDim Preserve RetArray(CurIndex + 10)
            End If

            If PInside AndAlso PrevInRect Then
                AddPoint = True
            ElseIf PInside Xor PrevInRect Then
                Dim P As Coords = Intersect(Clip, P1, P2, Polygon(PrevIndex), Polygon(Index))
                RetArray(CurIndex) = P
                CurIndex += 1

                AddPoint = PInside

            End If

            If AddPoint Then
                If CurIndex >= RetArray.Length Then
                    ReDim Preserve RetArray(CurIndex + 10)
                End If
                RetArray(CurIndex) = Polygon(Index)
                CurIndex += 1
            End If

            PrevIndex = Index
            PrevInRect = PInside
        Next

        If CurIndex = 0 Then
            If RetArray(0) Is Nothing Then
                Return Nothing
            Else
                Return RetArray
            End If
        Else

            ReDim Preserve RetArray(CurIndex - 1)
            Return RetArray
        End If
    End Function

    Public Shared Function ClipPolygon(ByVal P1 As Coords, ByVal P2 As Coords, ByVal Polygon As Coords()) As Coords()

        Dim RetArray(Polygon.Count - 1) As Coords
        Dim clipvalue As ClipPlane

        Array.Copy(Polygon, RetArray, Polygon.Count)

        For clipvalue = ClipPlane.North To ClipPlane.West
            RetArray = ClipPolygonPlane(clipvalue, P1, P2, RetArray)

            If RetArray Is Nothing Then
                Return Nothing
            End If

        Next

        Return RetArray
    End Function

End Class
