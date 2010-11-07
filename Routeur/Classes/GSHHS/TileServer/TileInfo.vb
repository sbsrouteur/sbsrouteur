Imports System.IO

Public Class TileInfo

    Private _BaseTilesPath As String = RouteurModel.BaseFileDir & "\tiles"

    Private _Z As Integer
    Private _Center As Coords

    Public Sub New(ByVal z As Integer, ByVal TileCenter As Coords)

        _Z = z
        _Center = TileCenter

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
            Return Z.ToString("000") & "_" & Center.Lat_Deg.ToString &"_"&Center.Lon_Deg 
        End Get

    End Property

    Public ReadOnly Property Z() As Integer
        Get
            Return _Z
        End Get
    End Property

End Class
