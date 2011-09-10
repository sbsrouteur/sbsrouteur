﻿Imports System.IO
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

        SQLiteConnection.CreateFile(DBFileName)

        Dim Conn As New SQLiteConnection(_DBPath)
        Conn.Open()
        Dim cmd As New SQLiteCommand(My.Resources.CreateDBScript, Conn)
        cmd.ExecuteNonQuery()
        Conn.Close()

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

    Public Sub AddPoly(MapLevel As Integer, A As Polygon)

        If A.Length = 0 Then
            Return
        End If
        Dim conn As New SQLiteConnection(_DBPath)
        conn.Open()
        Dim Cmd As New SQLiteCommand(conn)
        Dim StartCoords As Coords = A(0)
        Dim Trans As SQLiteTransaction = Nothing
        Try
            Trans = conn.BeginTransaction()
            For i As Integer = 0 To A.Length - 1

                Dim CmdText As String = "insert into mapssegments (maplevel,lon1,lat1,lon2,lat2) values (" & MapLevel & "," &
                                                                    A(i).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    A(i).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    A(i + 1).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    A(i + 1).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"
                Cmd.CommandText = CmdText
                Cmd.ExecuteNonQuery()

                If i Mod 1000 = 999 Then
                    Trans.Commit()
                    Trans = conn.BeginTransaction
                End If

            Next
            Dim LastCmdText As String = "insert into mapssegments (maplevel,lon1,lat1,lon2,lat2) values (" & MapLevel & "," &
                                                                    A(A.Length - 1).Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    A(A.Length - 1).Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    StartCoords.Lon_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & "," &
                                                                    StartCoords.Lat_Deg.ToString(System.Globalization.CultureInfo.InvariantCulture) & ")"
            Cmd.CommandText = LastCmdText
            Cmd.ExecuteNonQuery()

        Finally
            If Trans IsNot Nothing Then
                Trans.Commit()
            End If
            conn.Close()
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

    Public Function SegmentList(ByVal lon1 As Double, ByVal lat1 As Double, ByVal lon2 As Double, ByVal lat2 As Double) As List(Of MapSegment)

        Static count As Long = 0
        Static ticks As Long = 0
        Dim Start As DateTime = Now
        Dim RetList As New List(Of MapSegment)

        Try
            Using Con As New SQLiteConnection(_DBPath)
                Con.Open()

                Using Cmd As New SQLiteCommand(Con)
                    Dim Reader As SQLiteDataReader = Nothing

                    Try
                        Dim MinLon As String = Math.Min(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLon As String = Math.Max(lon1, lon2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MinLat As String = Math.Min(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)
                        Dim MaxLat As String = Math.Max(lat1, lat2).ToString(System.Globalization.CultureInfo.InvariantCulture)

                        Cmd.CommandText = "Select * from mapssegments where maplevel = " & MapLevel & " and (" &
                                            " (lon1 between " & MinLon & " and " & MaxLon & ") and ( lat1 between " & MinLat & " and " & MaxLat & ") )"

                        Reader = Cmd.ExecuteReader
                        If Reader.HasRows Then

                            While Reader.Read
                                Dim seg_lon1 As Double = CDbl(Reader("lon1"))
                                Dim seg_lon2 As Double = CDbl(Reader("lon2"))
                                Dim seg_lat1 As Double = CDbl(Reader("lat1"))
                                Dim seg_lat2 As Double = CDbl(Reader("lat2"))

                                RetList.Add(New MapSegment With {.Lon1 = seg_lon1, .Lon2 = seg_lon2, .Lat1 = seg_lat1, .Lat2 = seg_lat2})
                            End While

                        End If

                        Return RetList
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
            ticks += Now.Subtract(Start).Ticks
            count += 1

            If count Mod 1000 = 0 Then
                Console.WriteLine("Map intersect : " & New TimeSpan(CLng(ticks / count)).ToString)
            End If

        End Try

    End Function

    Public Property MapLevel As Integer = 1

    Public Function IntersectMapSegment(coords As Coords, coords1 As Coords, bspRect As BspRect) As Boolean

        For Each Seg As MapSegment In bspRect.GetSegments(coords, 0.01, Me)
            If Seg IsNot Nothing AndAlso GSHHS_Utils.IntersectSegments(coords, coords1, New Coords(Seg.Lat1, Seg.Lon1), New Coords(Seg.Lat2, Seg.Lon2)) Then
                Return True
            End If

        Next

        Return False
    End Function

End Class
