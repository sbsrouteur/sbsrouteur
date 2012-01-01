Imports Routeur.VLM_Router
Imports System.Threading

Public Class IsoChrone

    Private _Locks() As SpinLock
    Private _Data() As clsrouteinfopoints
    Private _Drawn As Boolean = False
    Private _IsoChroneLock As New Object


    Private _AngleStep As Double

    Public Sub New(ByVal AngleStep As Double)
        _AngleStep = AngleStep
        ReDim _Data(MaxIndex)
        ReDim _Locks(MaxIndex)
        Dim I As Integer
        For i = 0 To MaxIndex
            _Locks(i) = New SpinLock
        Next
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
            Return CInt(Math.Floor(((Angle + 360) Mod 360) / _AngleStep))
        End Get
    End Property

    Public ReadOnly Property AngleFromIndex(ByVal Index As Integer) As Double
        Get
            Return Index * _AngleStep
        End Get
    End Property

    Public ReadOnly Property Locks(index As Integer) As SpinLock
        Get
            Return _Locks(index)
        End Get
    End Property
    Public ReadOnly Property MaxIndex() As Integer
        Get
            Return CInt(Math.Floor((360 - _AngleStep) / _AngleStep))
        End Get
    End Property

End Class
