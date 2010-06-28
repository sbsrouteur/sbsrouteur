Public MustInherit Class RoutePointValueBase

    MustOverride Overrides Function ToString() As String

End Class

Public Class RoutePointDoubleValue

    Inherits RoutePointValueBase

    Private _Value As Double

    Public Property Value() As Double
        Get
            Return _Value
        End Get
        Set(ByVal value As Double)
            _Value = value
        End Set
    End Property

    Public Sub New(ByVal Value As Double)
        _Value = Value
    End Sub

    Public Overrides Function ToString() As String
        Return Value.ToString("f1")
    End Function

End Class
