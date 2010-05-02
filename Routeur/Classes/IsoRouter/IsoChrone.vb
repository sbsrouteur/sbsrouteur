Imports Routeur.VOR_Router

Public Class IsoChrone

    Private _Data() As clsrouteinfopoints
    Private _Drawn As Boolean=False 


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
            Return _Data(IndexFromAngle(Angle))
        End Get
        Set(ByVal value As clsrouteinfopoints)
            _Data(IndexFromAngle(Angle)) = value
        End Set
    End Property

    Public Property Data(ByVal index As Integer) As clsrouteinfopoints
        Get
            Return _Data(index)
        End Get
        Set(ByVal value As clsrouteinfopoints)
            _Data(index) = value
        End Set
    End Property
    Public Property Drawn() As Boolean
        Get
            Return _Drawn
        End Get
        Set(ByVal value As Boolean)
            _Drawn = value
        End Set
    End Property

    Public ReadOnly Property IndexFromAngle(ByVal Angle As Double) As Integer
        Get
            Return CInt((Angle Mod 360) / _AngleStep)
        End Get
    End Property

    Public ReadOnly Property MaxIndex() As Integer
        Get
            Return CInt((360 - _AngleStep) / _AngleStep)
        End Get
    End Property

End Class
