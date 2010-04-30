Imports Routeur.VOR_Router

Public Class IsoChrone

    Private _Data() As clsrouteinfopoints

    Private _AngleStep As Double

    Public Sub New(ByVal AngleStep As Double)
        _AngleStep = AngleStep
        ReDim _Data(CInt(360 / _AngleStep))

    End Sub

    Public Property Data() As clsrouteinfopoints()
        Get
            Return _Data
        End Get
        Set(ByVal value As clsrouteinfopoints())
            _Data = value
        End Set
    End Property

    Public Property Data(ByVal Angle As Double) As clsrouteinfopoints
        Get
            Return _Data(CInt((Angle Mod 360) / _AngleStep))
        End Get
        Set(ByVal value As clsrouteinfopoints)
            _Data(CInt((Angle Mod 360) / _AngleStep)) = value
        End Set
    End Property

    Public ReadOnly Property IndexFromAngle(ByVal Angle As Double) As Integer
        Get
            Return CInt((Angle Mod 360) / _AngleStep)
        End Get
    End Property

End Class
