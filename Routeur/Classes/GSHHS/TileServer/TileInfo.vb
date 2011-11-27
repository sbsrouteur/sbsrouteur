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

        Dim N_Lon As Double = ((W + E) / 2) Mod 180
        Dim N_Lat As Double = ((N + S) / 2) Mod 90
        Dim Lon As Double = ((W + E) / 2)
        Dim Lat As Double = ((N + S) / 2)
        Dim LonOffset As Double = Math.Floor(Lon / 180)
        _TX = CInt(Math.Floor((N_Lon + 180) / 360 * 2 ^ Z))
        _TY = CInt(Math.Floor((1 - Math.Log(Math.Tan(N_Lat * Math.PI / 180) + 1 / Math.Cos(N_Lat * Math.PI / 180)) / Math.PI) / 2 * 2 ^ Z))

        Debug.Assert(_TX >= 0 And _TX <= 2 ^ Z)
        Debug.Assert(_TY >= 0 And _TY <= 2 ^ Z)

        'Recompute tile coords
        Dim N1 As Double = PI - 2 * (_TY) * PI / (2 ^ Z)
        Dim S1 As Double = PI - 2 * (_TY + 1) * PI / (2 ^ Z)
        _North = Math.Atan(Math.Sinh(N1)) / PI * 180
        _South = Math.Atan(Math.Sinh(S1)) / PI * 180
        _East = (_TX + 1) * 360 / (2 ^ Z) - 180 + 180 * LonOffset
        _West = _TX * 360 / (2 ^ Z) - 180 + 180 * LonOffset
        '_Center = New Coords(N, W)

    End Sub
    Public Sub New(ByVal Z As Integer, ByVal Tx As Integer, ByVal TY As Integer)

        _Z = Z

        _TX = Tx

        _TY = TY
        'Recompute tile coords
        Dim N1 As Double = PI - 2 * (_TY) * PI / (2 ^ Z)
        Dim S1 As Double = PI - 2 * (_TY + 1) * PI / (2 ^ Z)
        _North = Math.Atan(Math.Sinh(N1)) / PI * 180
        _South = Math.Atan(Math.Sinh(S1)) / PI * 180
        _East = (_TX + 1) * 360 / (2 ^ Z) - 180
        _West = _TX * 360 / (2 ^ Z) - 180
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
            Return Z.ToString("000") & "_" & TX & "_" & TY & ".png"
        End Get

    End Property

    'Public ReadOnly Property OSM_TX() As Integer
    '    Get
    '        Return CInt(TX + 2 ^ (Z - 1))
    '    End Get
    'End Property

    'Public ReadOnly Property OSM_TY() As Integer
    '    Get
    '        Return CInt(2 ^ (Z - 1) - TY)
    '    End Get
    'End Property

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
