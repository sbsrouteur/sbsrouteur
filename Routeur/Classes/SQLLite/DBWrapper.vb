Imports System.IO
Imports System.Data.SQLite



Public Class DBWrapper

    Private Const DBName As String = "RouteurDB"

    Private _DBPath As String

    Public Sub New()
        Dim BaseFile As String = System.IO.Path.Combine(RouteurModel.BaseFileDir, DBName)
        _DBPath = "Data Source = """ & BaseFile & """ ; Version = 3; ConnectionPooling=100"

        If Not File.Exists(BaseFile) Then
            CreateDB(BaseFile)
        End If

    End Sub

    Private Sub CreateDB(DBFileName As String)
        Try
            SQLiteConnection.CreateFile(DBFileName)

            Dim Conn As New SQLiteConnection(_DBPath)
            Conn.Open()
            Dim cmd As New SQLiteCommand(My.Resources.CreateDBScript, Conn)
            cmd.ExecuteNonQuery()
            cmd.CommandText = My.Resources.CreateRangeIndex
            cmd.ExecuteNonQuery()
            Conn.Close()
        Catch ex As Exception
            MessageBox.Show("Exception during database creation : " & ex.Message)
        End Try

    End Sub

    Public Function DataMapInited(MapLevel As Integer) As Boolean

        Dim Conn As New SQLiteConnection(_DBPath)
        Conn.Open()
        Dim Cmd As New SQLiteCommand("Select * from MapsSegments where maplevel = " & MapLevel, Conn)

        Dim Reader As SQLiteDataReader = Nothing
        Try
            Reader = Cmd.ExecuteReader
            Return Reader.HasRows

        Finally
            If Reader IsNot Nothing Then
                Reader.Close()
            End If
            Cmd = Nothing
            Conn.Close()
        End Try

    End Function

    'Public Sub AddPoly(MapLevel As Integer, A As Polygon, Optional Flush As Boolean = False)

    '    Static Conn As SQLiteConnection
    '    Static PolyCount As Long = 0
    '    Static Trans As SQLiteTransaction = Nothing

    '    If Flush Then
    '        Trans.Commit()
    '        Conn.Close()
    '    End If

    '    If A Is Nothing OrElse A.Length = 0 Then
    '        Return
    '    End If

    '    If Conn Is Nothing Then
    '        Conn = New SQLiteConnection(_DBPath)
    '        Conn.Open()
    '        Trans = Conn.BeginTransaction()
    '    End If


    '    Dim Cmd As New SQLiteCommand(Conn)
    '    Dim StartCoords As Coords = A(0)
    '    Try
    '        For i As Integer = 0 To A.Length - 1

    '            Dim CmdText As String = "insert into mapssegments (maplevel,lon1,lat1,lon2,lat2) values (" & MapLevel & "," &
    '                                                                A(i).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                A(i).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                A(i + 1).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                A(i + 1).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"
    '            Cmd.CommandText = CmdText
    '            Cmd.ExecuteNonQuery()

    '            PolyCount += 1
    '            If PolyCount Mod 3000 = 0 Then
    '                Trans.Commit()
    '                Trans = Conn.BeginTransaction
    '            End If

    '        Next
    '        Dim LastCmdText As String = "insert into mapssegments (maplevel,lon1,lat1,lon2,lat2) values (" & MapLevel & "," &
    '                                                                A(A.Length - 1).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                A(A.Length - 1).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                StartCoords.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
    '                                                                StartCoords.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"
    '        Cmd.CommandText = LastCmdText
    '        Cmd.ExecuteNonQuery()

    '    Finally
    '        'If Trans IsNot Nothing Then
    '        '    Trans.Commit()
    '        'End If
    '        'Conn.Close()
    '    End Try

    'End Sub

    Public Sub AddSegment(MapLevel As Integer, P1 As Coords, p2 As Coords, Optional flush As Boolean = False)

        Static Conn As SQLiteConnection
        Static PolyCount As Long = 0
        Static Trans As SQLiteTransaction = Nothing

        If flush Then
            Trans.Commit()
            Conn.Close()
            Return
        End If

        If Conn Is Nothing Then
            Conn = New SQLiteConnection(_DBPath)
            Conn.Open()
            Trans = Conn.BeginTransaction()
        End If


        Dim Cmd As New SQLiteCommand(Conn)
        Try

            Dim CmdText As String = "insert into mapssegments (maplevel,lon1,lat1,lon2,lat2) values (" & MapLevel & "," &
                                                                P1.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                P1.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                p2.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                p2.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"
            Cmd.CommandText = CmdText
            Cmd.ExecuteNonQuery()

            Cmd.CommandText = " select last_insert_rowid()"
            Dim NewID As Long = CLng(Cmd.ExecuteScalar())
            Dim MinX As Double = Math.Min(P1.Lon_Deg, p2.Lon_Deg)
            Dim MaxX As Double = Math.Max(P1.Lon_Deg, p2.Lon_Deg)
            Dim MinY As Double = Math.Min(P1.Lat_Deg, p2.Lat_Deg)
            Dim MaxY As Double = Math.Max(P1.Lat_Deg, p2.Lat_Deg)

            Cmd.CommandText = "insert into MapLevel_Idx" & MapLevel & " ( ID, MinX, MaxX, MinY, MaxY) values (" & NewID & "," &
                                                                MinX.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                MaxX.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                MinY.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                MaxY.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"

            Cmd.ExecuteNonQuery()
            PolyCount += 1
            If PolyCount Mod 3000 = 0 Then
                Trans.Commit()
                Trans = Conn.BeginTransaction
            End If



        Finally
            'If Trans IsNot Nothing Then
            '    Trans.Commit()
            'End If
            'Conn.Close()
        End Try

    End Sub


    Shared Function GetMapLevel(StartPath As String) As Integer

        If StartPath.Length <= 4 Then
            Return 1
        End If
        Select Case StartPath.Substring(StartPath.Length - 3, 1).ToLowerInvariant
            Case "c"
                Return 1
            Case "l"
                Return 2
            Case "i"
                Return 3
            Case "h"
                Return 4
            Case "f"
                Return 5
            Case Else
                Return 1
        End Select
    End Function

    Shared Function GetMapLevel(enummaplevel As RacePrefs.EnumMapLevels) As Integer
        Select Case enummaplevel
            Case RacePrefs.EnumMapLevels.crude
                Return 1
            Case RacePrefs.EnumMapLevels.low
                Return 2
            Case RacePrefs.EnumMapLevels.intermediate
                Return 3
            Case RacePrefs.EnumMapLevels.high
                Return 4
            Case RacePrefs.EnumMapLevels.full
                Return 5
            Case Else
                Return 1
        End Select
    End Function

    Public Function SegmentList(ByVal lon1 As Double, ByVal lat1 As Double, ByVal lon2 As Double, ByVal lat2 As Double, Optional sorted As Boolean = False) As List(Of MapSegment)

        Dim RetList As New List(Of MapSegment)
        Dim Start As DateTime = Now
        Static CumDuration As Long = 0
        Static Count As Integer = 1
        Static HitCount As Long = 0
        Static HitCountNZ As Long = 0
        Static TotalHitCount As Long = 0
        Try


            Dim M As New MapSegment(lon1, lat1, lon2, lat2)
            Using Con As New SQLiteConnection(_DBPath)
                Con.Open()

                Using Cmd As New SQLiteCommand(Con)
                    Dim Reader As SQLiteDataReader = Nothing

                    Try
                        Dim MinLon As String = Math.Min(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLon As String = Math.Max(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MinLat As String = Math.Min(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLat As String = Math.Max(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)

                        Cmd.CommandText = "Select * from mapssegments inner join ( " &
                                            " select id from MapLevel_Idx" & MapLevel & " where " &
                                            " (MaxX  >= " & MinLon & " and MinX <=" & MaxLon & ") and ( MaxY >=" & MinLat & " and MinY <=" & MaxLat & ") " &
                                            ") As T on IdSegment = id"

                        If sorted Then
                            Cmd.CommandText = Cmd.CommandText & " order by IdSegment asc"
                        End If
                        Reader = Cmd.ExecuteReader
                        If Reader.HasRows Then

                            HitCountNZ += 1
                            While Reader.Read
                                Dim seg_lon1 As Double = CDbl(Reader("lon1"))
                                Dim seg_lon2 As Double = CDbl(Reader("lon2"))
                                Dim seg_lat1 As Double = CDbl(Reader("lat1"))
                                Dim seg_lat2 As Double = CDbl(Reader("lat2"))

                                RetList.Add(New MapSegment With {.Lon1 = seg_lon1, .Lon2 = seg_lon2, .Lat1 = seg_lat1, .Lat2 = seg_lat2})
                                TotalHitCount += 1
                            End While

                        End If
                        HitCount += 1
                        Return RetList
                    Catch ex As Exception
                    Finally

                        If Reader IsNot Nothing Then
                            Reader.Close()
                        End If
                        If Cmd IsNot Nothing Then
                            Cmd.Dispose()

                        End If
                        Con.Close()
                    End Try
                End Using

                Return Nothing

            End Using
        Finally
            Dim EndTick As DateTime = Now

            CumDuration += Now.Subtract(Start).Ticks
            Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgQueryTimeMS) = CumDuration / Count / TimeSpan.TicksPerMillisecond
            Count += 1
            If HitCount <> 0 Then
                Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgHitCount) = TotalHitCount / HitCount
            End If
            If HitCountNZ <> 0 Then
                Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgHitCountNonZero) = TotalHitCount / HitCountNZ
            End If

        End Try


    End Function

    Public Property MapLevel As Integer = 1

    Public Function IntersectMapSegment(coords As Coords, coords1 As Coords, bspRect As BspRect) As Boolean


        Dim SegList As IList = bspRect.GetSegments(coords, coords1, Me)

        If coords.Lat = coords1.Lat AndAlso coords.Lon = coords1.Lon Then
            Return False
        End If

        'Debug.WriteLine(coords.ToString & ";" & coords1.ToString)
        If SegList IsNot Nothing Then
            For Each Seg As MapSegment In SegList
                If Seg IsNot Nothing AndAlso GSHHS_Utils.IntersectSegments(coords, coords1, New Coords(Seg.Lat1, Seg.Lon1), New Coords(Seg.Lat2, Seg.Lon2)) Then
                    Return True
                End If

                'Debug.WriteLine(";;" & Seg.Lat1 & ";" & Seg.Lon1 & ";" & Seg.Lat2 & ";" & Seg.Lon2)

            Next
        End If


        Return False


    End Function



End Class
