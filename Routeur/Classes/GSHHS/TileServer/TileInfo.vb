Imports System.IO
Imports System.Math

Public Class TileInfo

    Private Shared _BaseTilesPath As String = RouteurModel.BaseFileDir & "\tiles"
    Private Shared MAX_MERCATOR_LAT As Double = 85.2
    Private _Z As Integer
    'Private _Center As Coords
    Private _North As Double
    Private _South As Double
    Private _East As Double
    Private _West As Double
    Private _TX As Integer
    Private _TY As Integer
    'Public Sub New(ByVal z As Integer, ByVal TileCenter As Coords)

    '    _Z = z
    '    _Center = TileCenter

    'End Sub

    'Public Sub New(ByVal z As Integer, ByVal Lon As Double, ByVal lat As Double)

    '    _Z = z

    '    Dim MapWidth As Double = 360 / (2 ^ z)
    '    Dim clon As Double = Math.Floor((Lon + 180) / MapWidth) * MapWidth + MapWidth / 2
    '    Dim clat As Double = Math.Floor((lat + 90) / MapWidth / 2) * MapWidth / 2 + MapWidth / 4
    '    _Center = New Coords(clat, clon)

    'End Sub

    Public Sub New(ByVal Z As Integer, ByVal N As Double, ByVal S As Double, ByVal E As Double, ByVal W As Double)

        _Z = Z

        _TX = CInt(Math.Floor((W + E) / 2 * (2 ^ Z) / 360))
        Dim V As Double = (N + S) / 360 * PI
        V = Log(Tan(V) + 1 / Cos(V))
        _TY = CInt(Math.Floor(V * (2 ^ Z) / PI))
        'Recompute tile coords
        Dim N1 As Double = (_TY + 1) * PI / (2 ^ Z)
        Dim S1 As Double = (_TY) * PI / (2 ^ Z)
        _North = Math.Atan(Math.Sinh(N1)) / PI * 180
        _South = Math.Atan(Math.Sinh(S1)) / PI * 180
        _East = (_TX + 1) * 360 / (2 ^ Z)
        _West = _TX * 360 / (2 ^ Z)
        '_Center = New Coords(N, W)

    End Sub
    Public Sub New(ByVal Z As Integer, ByVal Tx As Integer, ByVal TY As Integer)

        _Z = Z

        _TX = Tx

        _TY = TY
        'Recompute tile coords
        Dim N1 As Double = (_TY + 1) * PI / (2 ^ Z)
        Dim S1 As Double = (_TY) * PI / (2 ^ Z)
        _North = Math.Atan(Math.Sinh(N1)) / PI * 180
        _South = Math.Atan(Math.Sinh(S1)) / PI * 180
        _East = (_TX + 1) * 360 / (2 ^ Z)
        _West = _TX * 360 / (2 ^ Z)
        '_Center = New Coords(N, W)

    End Sub

    Public Function FileName() As String
        Return Path.Combine(BaseTilesPath, TilePath)
    End Function

    Public ReadOnly Property BaseTilesPath() As String
        Get
            Return _BaseTilesPath
        End Get
    End Property
    'Public ReadOnly Property Center() As Coords
    '    Get
    '        Return _Center
    '    End Get
    'End Property

    Public ReadOnly Property East() As Double
        Get
            Return _East
        End Get
    End Property

    Public ReadOnly Property North() As Double
        Get
            Return _North
        End Get
    End Property

    Public ReadOnly Property South() As Double
        Get
            Return _South
        End Get
    End Property

    Public ReadOnly Property TilePath() As String

        Get
            Return Z.ToString("000") & "_" & TX & "_" & TY & ".jpg"
        End Get

    End Property

    Public ReadOnly Property TX() As Integer
        Get
            Return _TX
        End Get
    End Property

    Public ReadOnly Property TY() As Integer
        Get
            Return _TY
        End Get
    End Property

    Public ReadOnly Property West() As Double
        Get
            Return _West
        End Get
    End Property

    Public ReadOnly Property Z() As Integer
        Get
            Return _Z
        End Get
    End Property

End Class
