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

Imports System.IO
Imports System.Data.SQLite
Imports System.Text
Imports System.Threading
Imports System.Security.Cryptography



Public Class DBWrapper

    Private Const OldDbName As String = "RouteurDB"
    Private Const DBName As String = "RouteurDB.db3"

    Private _DBPath As String
    Private Shared _Lock As New Object
    Private Shared _InitOK As Boolean = False

    Public Sub New()
        Dim BaseFile As String = System.IO.Path.Combine(RouteurModel.BaseFileDir, DBName)
        _DBPath = "Data Source = """ & BaseFile & """ ; Version = 3; ConnectionPooling=100"

        If Not _InitOK Then
            If Not File.Exists(BaseFile) Then
                If Not File.Exists(System.IO.Path.Combine(RouteurModel.BaseFileDir, OldDbName)) Then
                    CreateDB(BaseFile)
                ElseIf File.Exists(Path.Combine(RouteurModel.BaseFileDir, OldDbName)) Then
                    File.Move(Path.Combine(RouteurModel.BaseFileDir, OldDbName), BaseFile)
                End If
            End If
            CheckDBVersionAndUpdate()
        End If

    End Sub

    Public Sub New(MapLevel As Integer)
        Me.New()
        Me.MapLevel = MapLevel
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
            If Reader.HasRows Then
                _InitOK = True
            End If
            Return Reader.HasRows

        Finally
            If Reader IsNot Nothing Then
                Reader.Close()
            End If
            Cmd = Nothing
            Conn.Close()
        End Try

    End Function


    Public Sub AddSegment(MapLevel As Integer, P1 As Coords, p2 As Coords, Optional flush As Boolean = False)

        Static Conn As SQLiteConnection
        Static PolyCount As Long = 0
        Static Trans As SQLiteTransaction = Nothing

        If flush Then
            Trans.Commit()
            Conn.Close()
            _InitOK = True
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

        If Not _InitOK Then
            'Do not try loading coast lines until db is fully inited
            Return Nothing
        End If
        Dim RetList As New List(Of MapSegment)
        Dim Start As DateTime = Now
        Static CumDuration As Long = 0
        Static Count As Integer = 1
        Static HitCount As Long = 0
        Static HitCountNZ As Long = 0
        Static TotalHitCount As Long = 0
        Static HitCountError As Long = 0
        Try


            Dim RestartOnLockException As Boolean = False

RestartPoint:

            Using Con As New SQLiteConnection(_DBPath)
                Con.Open()

                Using Cmd As New SQLiteCommand(Con)
                    Dim Reader As SQLiteDataReader = Nothing

                    Try
                        Dim MinLon As String = Math.Min(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLon As String = Math.Max(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MinLat As String = Math.Min(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLat As String = Math.Max(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)

                        Cmd.CommandText = "Select IdSegment,lon1,lat1,lon2,lat2 from mapssegments inner join ( " &
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
                                Dim IdSeg As Long = CLng(Reader("IdSegment"))
                                RetList.Add(New MapSegment With {.Id = IdSeg, .Lon1 = seg_lon1, .Lon2 = seg_lon2, .Lat1 = seg_lat1, .Lat2 = seg_lat2})
                                TotalHitCount += 1

                            End While


                        End If
                        HitCount += 1
                        Return RetList
                    Catch ex As Exception
                        HitCountError += 1
                        RestartOnLockException = True
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

                If RestartOnLockException Then
                    RestartOnLockException = False
                    GoTo RestartPoint
                End If

                Return Nothing

            End Using
        Finally
            Dim EndTick As DateTime = Now
            Static lastupdate As DateTime
            CumDuration += Now.Subtract(Start).Ticks
            Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgQueryTimeMS) = CumDuration / Count / TimeSpan.TicksPerMillisecond
            Count += 1
            If Now.Subtract(lastupdate).TotalSeconds > 1 Then
                If HitCount <> 0 Then
                    Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgHitCount) = TotalHitCount / HitCount
                    Routeur.Stats.SetStatValue(Stats.StatID.RIndex_ExceptionRatio) = HitCountError / HitCount
                End If
                If HitCountNZ <> 0 Then
                    Routeur.Stats.SetStatValue(Stats.StatID.RIndex_AvgHitCountNonZero) = HitCount / HitCountNZ
                End If
                lastupdate = Now
            End If
        End Try


    End Function

    Public Property MapLevel As Integer = 1

    Public Function IntersectMapSegment(coords As Coords, coords1 As Coords, bspRect As BspRect) As Boolean

        Dim StartTick As DateTime = Now
        Static CumTime As Long = 0
        Static HitCount As Long = 0
        Try
            HitCount += 1
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
        Finally
            CumTime = CLng(CumTime + Now.Subtract(StartTick).TotalMilliseconds)
            Routeur.Stats.SetStatValue(Stats.StatID.DB_IntersectMapSegAvgMs) = CumTime / HitCount
        End Try



    End Function

    Shared VersionChecked As Boolean = False

    Private Sub CheckDBVersionAndUpdate()

        If VersionChecked Then
            Return
        End If
        SyncLock _Lock
            Dim CurVersion As Integer = GetCurDBVersion()
            Const DBVersion As Integer = 6

            If CurVersion = 0 Then
                'DBCreation
                CurVersion = 1
            End If

            If CurVersion < DBVersion Then
                Using conn As New SQLiteConnection(_DBPath)
                    conn.Open()
                    For i As Integer = CurVersion To DBVersion - 1

                        Using cmd As New SQLiteCommand(GetDBScript(i), conn)
                            Try
                                cmd.ExecuteNonQuery()

                            Catch ex As SQLiteException

                                If ex.ErrorCode = SQLiteErrorCode.Constraint Then
                                    'rollback the current transaction
                                    cmd.CommandText = "Rollback transaction"
                                    cmd.ExecuteNonQuery()

                                    'Duplicate DBDate, wait a bit and try again
                                    System.Threading.Thread.Sleep(1000)

                                    'Decrease i to re-run this DBVersion update script
                                    i -= 1
                                Else
                                    Throw ex
                                End If
                            End Try
                        End Using
                    Next
                End Using

            ElseIf CurVersion > DBVersion Then
                'Version is higher than exe
                MessageBox.Show("You DB version is more recent than current application can support. You Should Exit and update the application")
            Else
                'Version is OK nothing to do
            End If
            VersionChecked = True

        End SyncLock

    End Sub

    Private Function GetCurDBVersion() As Integer
        Using Conn As New SQLiteConnection(_DBPath)
            Conn.Open()
            Using cmd As New SQLiteCommand("Select Max(VersionNumber) from DBVersion", Conn)
                Dim RetObjet As Object = (cmd.ExecuteScalar)
                If Not IsDBNull(RetObjet) Then
                    Return CInt(RetObjet)
                Else
                    Return 0
                End If
            End Using
        End Using

    End Function

    Private Function GetDBScript(i As Integer) As String

        Return My.Resources.ResourceManager.GetString("UpdateV" & i & "ToV" & i + 1)

    End Function

    Function GetTrack(RaceID As Integer, BoatId As Integer) As LinkedList(Of Coords)
        Dim Retlist As New LinkedList(Of Coords)
        Dim LastEpoch As Long = 0
        Dim TC As New TravelCalculator
        Dim FirstPoint As Boolean = True
        Dim LastBearing As Double = -9999
        Dim SkipPoint As Boolean
        Dim EpochLimit As Long = CLng(Now.Subtract(New DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds - 48 * 3600)
        Using conn As New SQLiteConnection(_DBPath)
            conn.Open()
            Using cmd As New SQLiteCommand("select PointDate, Lon, Lat from TrackPoints inner join Tracks on TrackPoints.RefTrack = Tracks.id " &
                                            " where RaceID = " & RaceID & " and BoatNum=" & BoatId & " order by PointDate", conn)
                Dim Reader As SQLiteDataReader = cmd.ExecuteReader

                Dim PrevLat As Double = -99999999
                Dim PrevLon As Double = -9999999
                If Reader.HasRows Then
                    Dim Lat As Double
                    Dim Lon As Double
                    While Reader.Read
                        Lat = Reader.GetDouble(2)
                        Lon = Reader.GetDouble(1)
                        Dim Epoch As Long = Reader.GetInt64(0)
                        SkipPoint = Epoch < EpochLimit And Epoch - LastEpoch < 3600
                        If Not FirstPoint Then
                            SkipPoint = SkipPoint And Math.Abs(TC.LoxoCourse_Deg - LastBearing) < 10
                        Else
                            TC.StartPoint = New Coords(Lat, Lon)
                        End If
                        If Not SkipPoint Then
                            Retlist.AddLast(New Coords(Lon, Lat))
                            PrevLat = Lat
                            PrevLon = Lon
                            LastBearing = TC.LoxoCourse_Deg
                            If Not FirstPoint Then
                                TC.StartPoint = TC.EndPoint
                                TC.EndPoint = New Coords(Lat, Lon)
                            Else
                                FirstPoint = False
                            End If
                            LastEpoch = Epoch

                        End If
                    End While
                End If
            End Using
        End Using

        Return Retlist
    End Function

    Sub ImportTrack(RaceID As Integer, BoatNum As Integer)

        Throw New NotImplementedException

    End Sub

    Function GetLastTrackDate(RaceID As Integer, BoatNum As Integer) As Long
        Using conn As New SQLiteConnection(_DBPath)
            conn.Open()
            Using cmd As New SQLiteCommand(_DBPath, conn)
                cmd.CommandText = "select max(pointdate) from trackpoints " &
                                    "inner join tracks on reftrack = id where raceid=" & RaceID & " and BoatNum = " & BoatNum
                Dim Epoch As Object = cmd.ExecuteScalar
                Dim RetEpoch As Long
                If IsDBNull(Epoch) Then
                    RetEpoch = 0
                Else
                    RetEpoch = CLng(Epoch)
                End If

                Return RetEpoch
            End Using
        End Using
    End Function

    Sub AddTrackPoints(RaceID As Integer, BoatNum As Integer, PtList As List(Of TrackPoint))

        Dim Start As DateTime = Now
        If Not _InitOK Then
            'DB is not ready, try later
            Return
        End If

        Static PrevTrack As Integer = -1
        Static prevboat As Integer = -1
        Static CurRace As Integer = -1


        Using con As New SQLiteConnection(_DBPath)
            con.Open()
            Dim Trans As SQLiteTransaction
            Using cmd As New SQLiteCommand(con)

                If PrevTrack <> RaceID OrElse prevboat <> BoatNum Then
                    PrevTrack = RaceID
                    prevboat = BoatNum

                    cmd.CommandText = "select id from tracks where raceid=" & RaceID & " and BoatNum = " & BoatNum
                    Dim NumRace As Object = cmd.ExecuteScalar()

                    If IsDBNull(NumRace) OrElse NumRace Is Nothing Then
                        cmd.CommandText = "insert into tracks(raceid,boatnum) values(" & RaceID & "," & BoatNum & ");" & cmd.CommandText
                        CurRace = CInt(cmd.ExecuteScalar)

                    Else
                        CurRace = CInt(NumRace)
                    End If

                    cmd.CommandText = ""
                End If
                Dim PtIndex As Integer = 1
                Dim sb As New StringBuilder
                For Each Pt As TrackPoint In PtList
                    sb.Append(vbCrLf & "insert into Trackpoints(RefTrack,pointdate,lon,lat) values(" & CurRace & ",'" & Pt.Epoch &
                                        "' , " & Pt.P.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                         Pt.P.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ");")
                    If PtIndex Mod 1000 = 0 Then
                        cmd.CommandText = sb.ToString
                        Dim reqstart As DateTime = Now
                        Trans = con.BeginTransaction()
                        cmd.ExecuteNonQuery()
                        Trans.Commit()
                        Debug.WriteLine("ExecuteQuery 1000 inserts : " & Now.Subtract(reqstart).TotalMilliseconds & "ms")
                        sb.Clear()
                        PtIndex = 1
                    Else
                        PtIndex += 1
                    End If
                Next
                If sb.ToString <> "" Then
                    cmd.CommandText = sb.ToString
                    Dim reqstart As DateTime = Now
                    Trans = con.BeginTransaction()
                    cmd.ExecuteNonQuery()
                    Trans.Commit()
                    Debug.WriteLine("ExecuteQuery end inserts : " & Now.Subtract(reqstart).TotalMilliseconds & "ms")

                End If
            End Using
        End Using

        Debug.WriteLine("Track loaded " & Now.Subtract(Start).TotalMilliseconds & "ms pts: " & PtList.Count)
    End Sub

    Sub AddDBTile(Level As Integer, X As Integer, Y As Integer, TileData As Byte())

        Dim Hasher As MD5

        Hasher = MD5.Create
        Hasher.Initialize()

        Dim HashData = Hasher.ComputeHash(TileData)
        Dim HashString As String = ""

        For Each b As Byte In HashData
            HashString &= b.ToString("X2")
        Next

        Using con As New SQLiteConnection(_DBPath)
            con.Open()
            Dim Trans As SQLiteTransaction = Nothing
            Using cmd As New SQLiteCommand(con)

                Try
                    Trans = con.BeginTransaction

                    cmd.CommandText = "Select IdMapImage from MapImage where hash='" & HashString & "';"
                    Dim Id As Object = cmd.ExecuteScalar

                    If Id Is Nothing Then
                        'New Tile, add the image to the DB
                        cmd.CommandText = "insert into MapImage (Hash,Data) values ('" & HashString & "', @ImageData);"
                        cmd.Parameters.Add("@ImageData", System.Data.DbType.Binary).Value = TileData
                        cmd.ExecuteNonQuery()

                        cmd.Parameters.Clear()
                        cmd.CommandText = "Select IdMapImage from MapImage where hash='" & HashString & "';"
                        Id = cmd.ExecuteScalar
                    End If

                    'Insert the MapLut Record
                    cmd.CommandText = "insert into MapLut (Level,X,Y,RefBlob) values (" & Level & "," & X & "," & Y & "," & CInt(Id) & ");"
                    cmd.ExecuteNonQuery()
                    Trans.Commit()
                Catch ex As Exception
                    If Trans IsNot Nothing Then
                        Trans.Rollback()
                    End If
                End Try

            End Using
            con.Close()
        End Using



    End Sub

    Function ImageTileExists(TI As TileInfo) As Boolean

        If TI.TX < 0 OrElse TI.TY < 0 OrElse TI.TX > 2 ^ TI.Z OrElse TI.TY > 2 ^ TI.Z Then
            Dim i As Integer = 1
        End If

        Using con As New SQLiteConnection(_DBPath)
            con.Open()
            Using cmd As New SQLiteCommand(con)

                cmd.CommandText = "Select idMapLut from MapLut where level=" & TI.Z & " and X=" & TI.TX & " and Y=" & TI.TY & ";"
                Dim Id As Object = cmd.ExecuteScalar

                Return Id IsNot Nothing

            End Using
            con.Close()

        End Using
    End Function

    Function GetImageBuffer(Ti As TileInfo) As Byte()

        Using con As New SQLiteConnection(_DBPath)
            con.Open()
            Dim Reader As SQLiteDataReader = Nothing
            Dim NbRetries As Integer = 0

            While NbRetries < 2
                Using cmd As New SQLiteCommand(con)
                    Try
                        cmd.CommandText = "Select Data from MapImage join MapLut on RefBlob = IdMapImage where level=" & Ti.Z & " and X=" & Ti.TX & " and Y=" & Ti.TY & ";"
                        Reader = cmd.ExecuteReader

                        If Reader.HasRows Then
                            Reader.Read()
                            Return CType(Reader(0), Byte())
                        Else
                            Return Nothing
                        End If
                    Finally
                        If Reader IsNot Nothing Then
                            Reader.Close()
                        End If
                    End Try

                End Using
                NbRetries += 1
            End While
            con.Close()
            Return Nothing

        End Using

    End Function



End Class
