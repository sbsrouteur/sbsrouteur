Imports System.IO

Public Class TileInfo

    Private _BaseTilesPath As String = RouteurModel.BaseFileDir & "\tiles"

    Private _Z As Integer
    Private _Center As Coords

    Public Sub New(ByVal z As Integer, ByVal TileCenter As Coords)

        _Z = z
        _Center = TileCenter

    End Sub

    Public Sub New(ByVal z As Integer, ByVal Lon As Double, ByVal lat As Double)

        _Z = z

        Dim MapWidth As Double = 360 / (2 ^ z)
        Dim clon As Double = Math.Floor((Lon + 180) / MapWidth) * MapWidth + MapWidth / 2
        Dim clat As Double = Math.Floor((lat + 90) / MapWidth / 2) * MapWidth / 2 + MapWidth / 4
        _Center = New Coords(clat, clon)

    End Sub

    Public Function FileName() As String
        Return Path.Combine(BaseTilesPath, TilePath)
    End Function

    Public ReadOnly Property BaseTilesPath() As String
        Get
            Return _BaseTilesPath
        End Get
    End Property
    Public ReadOnly Property Center() As Coords
        Get
            Return _Center
        End Get
    End Property

    Public ReadOnly Property TilePath() As String

        Get
            Return Z.ToString("000") & "_" & Center.Lat_Deg.ToString & "_" & Center.Lon_Deg & ".jpg"
        End Get

    End Property

    Public ReadOnly Property Z() As Integer
        Get
            Return _Z
        End Get
    End Property

End Class
