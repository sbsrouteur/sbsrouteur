Imports System.IO
Imports System.Drawing
Imports System.ComponentModel
Imports System.Net

Public Class TileServer
    Implements INotifyPropertyChanged
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public Event TileReady(ByVal ti As TileInfo)

    Public Const TILE_SIZE As Integer = 256

    Private _Renderer As _2D_Viewer

    Private _TileBuildList As New SortedList(Of String, String)
    Private _queryCount As Long = 0
    Private _HitCount As Long = 0
    '    Private _Busy As Boolean = False


    Private Sub BgCreateTile(ByVal state As Object)

        Dim TI As TileInfo = CType(state, TileInfo)
        SyncLock _TileBuildList

            If _TileBuildList.ContainsKey(TI.TilePath) Then
                Return
            Else
                _TileBuildList.Add(TI.TilePath, TI.TilePath)
                Busy = True
            End If
        End SyncLock

        Dim North As Double = TI.North
        Dim South As Double = TI.South
        Dim East As Double = TI.East
        Dim West As Double = TI.West

        Dim W As Double = East - West
        Dim H As Double = North - South
        Dim Resolution As Double = If(W < H, W, H) / TILE_SIZE
        Dim XOffset As Double
        Dim YOffset As Double
        Dim img As New Bitmap(TILE_SIZE, TILE_SIZE, Imaging.PixelFormat.Format32bppArgb)
        Dim FileError As Boolean = False

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
            GSHHS_Reader.ReadTile(YOffset, XOffset, _Renderer, North, South, East, West, img, MapLevel)

            If Not Directory.Exists(TI.BaseTilesPath) Then
                Directory.CreateDirectory(TI.BaseTilesPath)
            End If
            img.Save(TI.FileName, Imaging.ImageFormat.Png)
        Else

            Dim WC As New WebClient
            Try
                WC.DownloadFile("http://tile.openstreetmap.org/" & TI.Z & "/" & TI.OSM_TX & "/" & TI.OSM_TY & ".png", TI.FileName)
            Catch ex As Exception
                'MessageBox.Show(ex.Message, "Failed to get tile " & TI.FileName)
                FileError = True
            End Try

        End If


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

    End Sub

    Public Sub RequestTile(ByVal TI As TileInfo)

        _queryCount += 1
        If File.Exists(TI.FileName) AndAlso New FileInfo(TI.FileName).Length > 0 Then
            Dim Tile As New Bitmap(TI.FileName)
            _HitCount += 1
            RaiseEvent TileReady(TI)
        Else

#Const MONO_THREAD_TILES = 0

#If MONO_THREAD_TILES Then
            BgCreateTile (TI)
#Else
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgCreateTile, TI)
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

    Public ReadOnly Property TileServerStateIdle() As Boolean
        Get
            SyncLock _TileBuildList
                Return _TileBuildList.Count = 0
            End SyncLock

        End Get
    End Property


End Class
