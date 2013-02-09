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
Imports System.Drawing
Imports System.ComponentModel
Imports System.Net

Public Class TileServer
    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Event TileReady(ByVal ti As TileInfo)
    Public Event TileProgress(ByVal pct As Double)

    Public Const TILE_SIZE As Integer = 256

    Private _Renderer As _2D_Viewer

    Private _VLM_TILE_SERVER As String = RouteurModel.Base_Game_Url
    
    Private _TileBuildList As New SortedList(Of String, TileInfo)
    Private _queryCount As Long = 0
    Private _HitCount As Long = 0
    '    Private _Busy As Boolean = False


    Private Sub BgCreateTile(ByVal state As Object)

        Dim TI As TileInfo = CType(state, TileInfo)
        SyncLock _TileBuildList

            If _TileBuildList.ContainsKey(TI.TilePath) Then
                Return
            Else
                _TileBuildList.Add(TI.TilePath, TI)
                Busy = True
            End If
        End SyncLock

        Dim North As Double = TI.North
        Dim South As Double = TI.South
        Dim East As Double = TI.East
        Dim West As Double = TI.West

        Dim W As Double = East - West
        Dim H As Double = North - South
        Dim img As New Bitmap(TILE_SIZE, TILE_SIZE, Imaging.PixelFormat.Format32bppArgb)
        Dim FileError As Boolean = False

#Const USE_OSM = 1

#If USE_OSM = 0 Then
        Dim MapLevel As String
        If True Then

            Select Case TI.Z
                Case 0 To 2
                    MapLevel = "c"
                Case 3 To 5
                    MapLevel = "l"

                Case 6 To 9
                    MapLevel = "i"
                Case 10 To 13

                    MapLevel = "h"

                Case Else
                    MapLevel = "f"
            End Select
            MapLevel = "..\gshhs\gshhs_" & MapLevel & ".b"
            If Not File.Exists(MapLevel) Then
                MapLevel = "i"
                MapLevel = "..\gshhs\gshhs_" & MapLevel & ".b"
            End If
            GSHHS_Reader.ReadTile(_Renderer, North, South, East, West, img, MapLevel)

            img.Save(TI.FileName, Imaging.ImageFormat.Png)
#Else
        If Not Directory.Exists(TI.BaseTilesPath) Then
            Directory.CreateDirectory(TI.BaseTilesPath)
        End If

        Dim WC As New WebClient
        Try
restart_download:
            FileError = False
            WC.DownloadFile(_VLM_TILE_SERVER & "/cache/gshhstiles/" & TI.Z & "/" & TI.TX & "/" & TI.TY & ".png", TI.FileName)
            
        Catch ex As Exception
            MessageBox.Show(ex.Message, "Failed to get tile " & TI.FileName)
            FileError = True
        End Try

#End If


        SyncLock _TileBuildList
            _TileBuildList.Remove(TI.TilePath)
            Busy = _TileBuildList.Count = 0
        End SyncLock
        If Not FileError Then
            RaiseEvent TileReady(TI)
        Else
            RaiseEvent TileReady(Nothing)
        End If

    End Sub

    Public Sub New(ByVal Render As _2D_Viewer)

        _Renderer = Render
        AddHandler GSHHS_Reader.TilesProgress, AddressOf TileProgressHandler

    End Sub

    Public Sub RequestTile(ByVal TI As TileInfo)

        _queryCount += 1
        If File.Exists(TI.FileName) AndAlso New FileInfo(TI.FileName).Length > 0 Then
            'Dim Tile As New Bitmap(TI.FileName)
            _HitCount += 1
            RaiseEvent TileReady(TI)
        Else

#Const LOCAL_GSHHS_MAP = 0

#If LOCAL_GSHHS_MAP = 0 Then
            BgCreateTile (TI)
#Else
            'System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgCreateTile, TI)
            SyncLock _TileBuildList

                If _TileBuildList.ContainsKey(TI.TilePath) Then
                    Return
                Else
                    _TileBuildList.Add(TI.TilePath, TI)
                    Busy = True
                End If
            End SyncLock
#End If

        End If
        Debug.WriteLine("tileserver Q;" & _queryCount & " " & _HitCount / _queryCount)
    End Sub

    Public Property Busy() As Boolean
        Get
            SyncLock _TileBuildList
                Return _TileBuildList.Count > 0
            End SyncLock
        End Get

        Set(ByVal value As Boolean)
            RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Busy"))
        End Set

    End Property

    Private Sub TileProgressHandler(ByVal pct As Double)
        RaiseEvent TileProgress(pct)
    End Sub

    Public ReadOnly Property TileServerStateIdle() As Boolean
        Get
            SyncLock _TileBuildList
                Return _TileBuildList.Count = 0
            End SyncLock

        End Get
    End Property

    Public Sub Render()


        If _TileBuildList.Count = 0 Then
            RaiseEvent TileProgress(100)
            Return
        End If
        Dim img() As Bitmap
        Dim i As Integer
        ReDim img(_TileBuildList.Count - 1)
        
        For i = 0 To img.Length - 1
            img(i) = New Bitmap(TILE_SIZE, TILE_SIZE, Imaging.PixelFormat.Format32bppArgb)
        Next
        
        Dim MapLevel As String
        Dim TI As TileInfo
        Select Case _TileBuildList.Values(0).Z
            Case 0 To 2
                MapLevel = "l"
            Case 3 To 5
                MapLevel = "i"

            Case 6 To 9
                MapLevel = "h"
            Case 10 To 13

                MapLevel = "f"

            Case Else
                MapLevel = "f"
        End Select
        MapLevel = "..\gshhs\gshhs_" & MapLevel & ".b"
        If Not File.Exists(MapLevel) Then
            MapLevel = "i"
            MapLevel = "..\gshhs\gshhs_" & MapLevel & ".b"
        End If

        Dim Start As DateTime = Now
        GSHHS_Reader.ReadTiles(_Renderer, _TileBuildList, img, MapLevel)
        'Console.WriteLine("Render level " & MapLevel & " in " & Now.Subtract(Start).ToString & " for " & img.Length & " tiles")
        For i = 0 To img.Length - 1
            TI = _TileBuildList.Values(i)
            If Not Directory.Exists(TI.BaseTilesPath) Then
                Directory.CreateDirectory(TI.BaseTilesPath)
            End If
            img(i).Save(TI.FileName, Imaging.ImageFormat.Png)
            RaiseEvent TileReady(TI)
        Next


        SyncLock _TileBuildList
            _TileBuildList.Clear()
            Busy = _TileBuildList.Count = 0
        End SyncLock

    End Sub

    Sub Clear()
        SyncLock _TileBuildList
            _TileBuildList.Clear()
            Busy = False
            RaiseEvent TileProgress(100)
        End SyncLock
    End Sub


End Class
