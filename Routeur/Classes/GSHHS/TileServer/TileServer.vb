Imports System.IO
Imports System.Drawing

Public Class TileServer

    Public Event TileReady(ByVal tilename As String, ByVal Tile As Image)

    Public Const TILE_SIZE As Integer = 256

    Private _Renderer As _2D_Viewer

    Private _TileBuildList As New SortedList(Of String, String)

    Private Sub BgCreateTile(ByVal state As Object)

        Dim TI As TileInfo = CType(state, TileInfo)
        SyncLock _TileBuildList

            If _TileBuildList.ContainsKey(TI.TilePath) Then
                Return
            Else
                _TileBuildList.Add(TI.TilePath, TI.TilePath)
            End If
        End SyncLock

        Dim MapWidth As Double = 360 / 2 ^ (TI.Z + 1)
        Dim North As Double = TI.Center.Lat_Deg + MapWidth / 2
        Dim South As Double = TI.Center.Lat_Deg - MapWidth / 2
        Dim East As Double = TI.Center.Lon_Deg + MapWidth
        Dim West As Double = TI.Center.Lon_Deg - MapWidth

        Dim W As Double = East - West
        Dim H As Double = North - South
        Dim Resolution As Double = If(W < H, W, H) / TILE_SIZE
        Dim XOffset As Double
        Dim YOffset As Double
        Dim img As New Bitmap(TILE_SIZE, TILE_SIZE, Imaging.PixelFormat.Format32bppArgb)

        Dim MapLevel As String

        Select Case TI.Z
            Case Else
                MapLevel = "c"
        End Select
        MapLevel = "..\gshhs\gshhs_" & MapLevel & ".b"
        GSHHS_Reader.ReadTile(YOffset, XOffset, _Renderer, North, South, East, West, img, MapLevel)

        If Not Directory.Exists(TI.BaseTilesPath) Then
            Directory.CreateDirectory(TI.BaseTilesPath)
        End If
        img.Save(TI.FileName)

        SyncLock _TileBuildList
            _TileBuildList.Remove(TI.TilePath)
        End SyncLock
    End Sub

    Public Sub New(ByVal Render As _2D_Viewer)

        _renderer = Render

    End Sub

    Public Sub RequestTile(ByVal TI As TileInfo)


        If File.Exists(TI.FileName) Then
            Dim Tile As New Bitmap(TI.FileName)

            RaiseEvent TileReady(TI.TilePath, Tile)
        Else
            System.Threading.ThreadPool.QueueUserWorkItem(AddressOf BgCreateTile, TI)
        End If

    End Sub

    
End Class
